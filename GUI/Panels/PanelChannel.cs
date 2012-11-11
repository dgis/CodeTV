//    Copyright (C) 2006-2007  Regis COSNIER
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace CodeTV
{
	public partial class PanelChannel : PanelDockBase
	{
		// Drag'n drop
		private TreeNode treeNodeUnderMouseToDrag;
		private TreeNode treeNodeUnderMouseToDrop;
		private Rectangle dragBoxFromMouseDown;

		public PanelChannel()
		{
			InitializeComponent();

			this.MainForm = MainForm.Form;
 
            this.treeViewChannel.Tag = MainForm.rootChannelFolder;
            this.treeViewChannel.ImageList = MainForm.imageListLogoTV;
        }

		private void treeViewChannel_DoubleClick(object sender, EventArgs e)
		{
			if (this.treeViewChannel.SelectedNode != null && this.treeViewChannel.SelectedNode.Tag is ChannelTV)
			{
				ChannelTV channel = this.treeViewChannel.SelectedNode.Tag as ChannelTV;
				MainForm.TuneChannelGUI((Channel)channel);
			}
		}

		private void treeViewChannel_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				TreeViewHitTestInfo tvhti = this.treeViewChannel.HitTest(e.Location.X, e.Location.Y);
				if (tvhti.Node == null)
					this.treeViewChannel.SelectedNode = null;
				//else if(MainForm.panelChannelProperties.Visible)
				//    ShowProperties();
			}
		}

		private void treeViewChannel_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (MainForm.panelChannelProperties.Visible)
				ShowProperties();
		}

		private void treeViewChannel_AfterCollapse(object sender, TreeViewEventArgs e)
		{
			(e.Node.Tag as ChannelFolder).Expanded = false;
			e.Node.ImageKey = e.Node.SelectedImageKey = "FolderClosed";
		}

		private void treeViewChannel_AfterExpand(object sender, TreeViewEventArgs e)
		{
			(e.Node.Tag as ChannelFolder).Expanded = true;
			e.Node.ImageKey = e.Node.SelectedImageKey = "FolderOpened";
		}

		private void treeViewChannel_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (e.Label != null)
			{
				if (e.Label.Length > 0)
				{
					Channel selectedChannel = (Channel)this.treeViewChannel.SelectedNode.Tag;
					selectedChannel.Name = e.Label;
					e.Node.EndEdit(false);
				}
				else
				{
					/* Cancel the label edit action, inform the user, and 
					   place the node in edit mode again. */
					e.CancelEdit = true;
					MessageBox.Show(Properties.Resources.CannotRenameChannel);
					e.Node.BeginEdit();
				}
			}
			this.treeViewChannel.LabelEdit = false;
		}

		private void treeViewChannel_DragDrop(object sender, DragEventArgs e)
		{
			// Ensure that the list item index is contained in the data.
			if (//this.treeNodeUnderMouseToDrop != null &&
				//!this.treeViewChannel.IsSelected(this.treeNodeUnderMouseToDrop) &&
				e.Data.GetDataPresent(typeof(ArrayList)))
			{
				// Perform drag-and-drop, depending upon the effect.
				if (e.Effect == DragDropEffects.Copy || e.Effect == DragDropEffects.Move)
				{
					ArrayList alDraggedChannels = (ArrayList)e.Data.GetData(typeof(ArrayList));

					ArrayList draggedChannelsList = new ArrayList();
					Hashtable draggedChannelsMap = new Hashtable();
					if (e.Effect == DragDropEffects.Copy)
					{
						foreach (TreeNode tn in alDraggedChannels)
						{
							Channel channel = (tn.Tag as Channel).MakeCopy();
							draggedChannelsMap[channel] = tn;
							draggedChannelsList.Add(channel);
						}
					}
					else
					{
						foreach (TreeNode tn in alDraggedChannels)
						{
							Channel channel = tn.Tag as Channel;
							draggedChannelsMap[channel] = tn;
							draggedChannelsList.Add(channel);
						}
					}

					// Compute the drop container and the drop index 
					ChannelFolder parentChannel;
					TreeNodeCollection parentTreeNodeCollection;
					int afterChannelListIndex = -1;
					int afterTreeNodeIndex = -1;
					if (this.treeNodeUnderMouseToDrop == null)
					{
						parentChannel = this.MainForm.rootChannelFolder;
						parentTreeNodeCollection = this.treeViewChannel.Nodes;
						afterChannelListIndex = parentChannel.ChannelList.Count;
						afterTreeNodeIndex = parentTreeNodeCollection.Count;
						//afterChannelListIndex = 0;
						//afterTreeNodeIndex = 0;
					}
					else if (this.treeNodeUnderMouseToDrop.Tag is ChannelFolder)
					{
						parentChannel = this.treeNodeUnderMouseToDrop.Tag as ChannelFolder;
						parentTreeNodeCollection = this.treeNodeUnderMouseToDrop.Nodes;
						afterChannelListIndex = 0;
						afterTreeNodeIndex = 0;
					}
					else
					{
						if (this.treeNodeUnderMouseToDrop.Parent != null)
						{
							parentChannel = this.treeNodeUnderMouseToDrop.Parent.Tag as ChannelFolder;
							parentTreeNodeCollection = this.treeNodeUnderMouseToDrop.Parent.Nodes;
						}
						else
						{
							parentChannel = this.MainForm.rootChannelFolder;
							parentTreeNodeCollection = this.treeViewChannel.Nodes;
						}
						afterChannelListIndex = parentChannel.ChannelList.IndexOf(this.treeNodeUnderMouseToDrop.Tag as Channel);
						afterTreeNodeIndex = this.treeNodeUnderMouseToDrop.Index;
					}

					// Remove the old TreeNode
					if (e.Effect == DragDropEffects.Move)
					{
						foreach (TreeNode tn in draggedChannelsMap.Values)
						{
							// Modify the drop index if some moved elements are before
							// the drop target in the same level
							if (this.treeNodeUnderMouseToDrop == null ||
								(tn.Parent == this.treeNodeUnderMouseToDrop.Parent &&
								tn.Index < this.treeNodeUnderMouseToDrop.Index))
							{
								afterChannelListIndex--;
								afterTreeNodeIndex--;
							}

							// Remove the elements
							if (tn.Parent != null)
							{
								(tn.Parent.Tag as ChannelFolder).ChannelList.Remove(tn.Tag as Channel);
								tn.Parent.Nodes.Remove(tn);
							}
							else
							{
								(this.treeViewChannel.Tag as ChannelFolder).ChannelList.Remove(tn.Tag as Channel);
								this.treeViewChannel.Nodes.Remove(tn);
							}
						}
					}

					if (afterChannelListIndex < 0) afterChannelListIndex = 0;
					if (afterTreeNodeIndex < 0) afterTreeNodeIndex = 0;

					int indexOffset = 0;
					foreach (Channel channel in draggedChannelsList)
					{
						// First level: we add to the existing folder
						if (afterChannelListIndex == -1)
							parentChannel.Add(channel);
						else
						{
							channel.Parent = parentChannel;
							parentChannel.ChannelList.Insert(afterChannelListIndex + indexOffset, channel);
						}

						TreeNode treeNode = MakeTreeNodeFromChannel(channel);
						if (afterTreeNodeIndex == -1)
							parentTreeNodeCollection.Add(treeNode);
						else
							parentTreeNodeCollection.Insert(afterTreeNodeIndex + indexOffset, treeNode);

						// Second level: we adjust the parent
						if (channel is ChannelFolder)
						{
							ChannelFolder channelFolder = channel as ChannelFolder;
							RecursedFillTree(channelFolder, treeNode.Nodes);

							if (channelFolder.Expanded)
								treeNode.Expand();
							else
								treeNode.Collapse();
						}
						indexOffset++;
					}
					(alDraggedChannels[0] as TreeNode).EnsureVisible();
				}
			}
		}

		private void treeViewChannel_DragOver(object sender, DragEventArgs e)
		{
			// Determine whether string data exists in the drop data. If not, then
			// the drop effect reflects that the drop cannot occur.
			if (!e.Data.GetDataPresent(typeof(ArrayList)))
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			// Set the effect based upon the KeyState.
			if ((e.KeyState & 4) == 4 &&
			  (e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
			{
				// SHIFT KeyState for move.
				e.Effect = DragDropEffects.Move;
			}
			else if ((e.KeyState & 8) == 8 &&
			  (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
			{
				// CTL KeyState for copy.
				e.Effect = DragDropEffects.Copy;
			}
			else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
			{
				// By default, the drop action should be move, if allowed.
				e.Effect = DragDropEffects.Move;
			}
			else
				e.Effect = DragDropEffects.None;

			// Get the item the mouse is below. 

			// The mouse locations are relative to the screen, so they must be 
			// converted to client coordinates.
			this.treeNodeUnderMouseToDrop = this.treeViewChannel.HitTest(this.treeViewChannel.PointToClient(new Point(e.X, e.Y))).Node;

			if (this.treeViewChannel.IsHierarchySelected(this.treeNodeUnderMouseToDrop))
				e.Effect = DragDropEffects.None;
		}

		private void treeViewChannel_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			if (e.EscapePressed)
			{
				// Reset the drag rectangle when the mouse button is raised.
				this.dragBoxFromMouseDown = Rectangle.Empty;

				e.Action = DragAction.Cancel;
			}
		}

		private void treeViewChannel_MouseDown(object sender, MouseEventArgs e)
		{
			bool startDrag = false;
			foreach (TreeNode tn in this.treeViewChannel.SelectedNodes)
			{
				if (tn.Bounds.Contains(e.X, e.Y))
					startDrag = true;
			}
			this.treeNodeUnderMouseToDrag = this.treeViewChannel.HitTest(e.X, e.Y).Node;
			if (this.treeNodeUnderMouseToDrag != null && startDrag)
			{
				// Remember the point where the mouse down occurred. The DragSize indicates
				// the size that the mouse can move before a drag event should be started.                
				Size dragSize = SystemInformation.DragSize;
				dragSize.Height *= this.treeViewChannel.SelectedNodes.Count;

				// Create a rectangle using the DragSize, with the mouse position being
				// at the center of the rectangle.
				this.dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
															   e.Y - (dragSize.Height / 2)), dragSize);
			}
			else
				// Reset the rectangle if the mouse is not over an item in the ListBox.
				this.dragBoxFromMouseDown = Rectangle.Empty;
		}

		private void treeViewChannel_MouseMove(object sender, MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
			{
				// If the mouse moves outside the rectangle, start the drag.
				if (this.dragBoxFromMouseDown != Rectangle.Empty && !this.dragBoxFromMouseDown.Contains(e.X, e.Y))
				{
					// Child element must be remove if any
					foreach (TreeNode tnParent in new ArrayList(this.treeViewChannel.SelectedNodes))
					{
						if (tnParent.GetNodeCount(false) > 0)
						{
							foreach (TreeNode tnProbableChild in new ArrayList(this.treeViewChannel.SelectedNodes))
							{
								if (tnProbableChild != tnParent && tnProbableChild.FullPath.IndexOf(tnParent.FullPath) == 0)
									tnProbableChild.Checked = false;
							}
						}
					}

					// Proceed with the drag-and-drop, passing in the list item.
					this.treeViewChannel.DoDragDrop(new ArrayList(this.treeViewChannel.SelectedNodes),
						DragDropEffects.None | DragDropEffects.Copy | DragDropEffects.Move);
				}
			}
		}

		private void treeViewChannel_MouseUp(object sender, MouseEventArgs e)
		{
			// Reset the drag rectangle when the mouse button is raised.
			this.dragBoxFromMouseDown = Rectangle.Empty;
		}

		private void switchOnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.treeViewChannel.SelectedNode != null && this.treeViewChannel.SelectedNode.Tag is ChannelTV)
			{
				ChannelTV channel = this.treeViewChannel.SelectedNode.Tag as ChannelTV;
				MainForm.TuneChannelGUI((Channel)channel);
			}
		}

		private void rebuildTheGraphAndTuneToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.treeViewChannel.SelectedNode != null && this.treeViewChannel.SelectedNode.Tag is ChannelTV)
			{
				ChannelTV channel = this.treeViewChannel.SelectedNode.Tag as ChannelTV;
				MainForm.TuneChannelGUI((Channel)channel, true);
			}
		}

		private void tuneAllChannelInMosaicToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.treeViewChannel.SelectedNode != null && this.treeViewChannel.SelectedNode.Tag is ChannelDVB)
			{
				ChannelDVB channel = this.treeViewChannel.SelectedNode.Tag as ChannelDVB;
				MainForm.TuneChannelGUI((Channel)channel);
			}
		}



        //private Image ResizeImage(Image image, int newWidth, int newHeight, bool highQuality)
        //{
        //    System.Drawing.Image result = new Bitmap(newWidth, newHeight);
        //    System.Drawing.Graphics graphic = Graphics.FromImage(result);

        //    // Set the quality options
        //    if (highQuality)
        //    {
        //        graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //        graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        //        graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        //        graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        //    }

        //    // Constraint the sizes
        //    double originalRatio = (double)image.Width / image.Height;
        //    double standardRatio = (double)newWidth / newHeight;
        //    int offsetX = 0, offsetY = 0;
        //    if (originalRatio > standardRatio)
        //    {
        //        newHeight = (int)(image.Height * newWidth / image.Width);
        //        offsetY = (newWidth - newHeight) >> 1;
        //    }
        //    else
        //    {
        //        newWidth = (int)(image.Width * newHeight / image.Height);
        //        offsetX = (newHeight - newWidth) >> 1;
        //    }

        //    // Draw the image with the resized dimensions
        //    graphic.DrawImage(image, offsetX, offsetY, newWidth, newHeight);

        //    return result;
        //}

		internal void AdjustTVLogo(Channel channel)
		{
			if (channel.Tag != null)
			{
				TreeNode treeNode = channel.Tag as TreeNode;

				if (channel is ChannelFolder)
				{
					treeNode.ImageKey = treeNode.SelectedImageKey = "FolderClosed";
					return;
				}
				else if (channel is ChannelTV)
				{
					string logo = (channel as ChannelTV).Logo;
					if (logo != null && logo.Length > 0)
					{
                        if (MainForm.imageListLogoTV.Images.ContainsKey(logo))
                        {
                            treeNode.ImageKey = treeNode.SelectedImageKey = logo;
                            return;
                        }
                        else
                        {
                            try
                            {
                                string path = FileUtils.GetAbsolutePath(logo);
                                if (File.Exists(path))
                                {
                                    Bitmap bitmap = new Bitmap(path);
                                    //if (!Bitmap.IsAlphaPixelFormat(bitmap.PixelFormat))
                                    //    bitmap.MakeTransparent(Color.White);
                                    //Image thumbnail = ResizeImage(bitmap, 16, 16, false);
                                    Image thumbnail = Utils.ResizeImage(bitmap, 16, 16, false);
                                    MainForm.imageListLogoTV.Images.Add(logo, thumbnail);
                                    treeNode.ImageKey = treeNode.SelectedImageKey = logo;
                                    thumbnail.Dispose();
                                    bitmap.Dispose();
                                    return;
                                }
                            }
                            catch (ArgumentException) { }
                        }
					}
				}

				treeNode.ImageKey = treeNode.SelectedImageKey = "LogoTVDefault";
			}
		}

		internal TreeNode MakeTreeNodeFromChannel(Channel channel)
		{
			TreeNode treeNode = new TreeNode(channel.Name);
			treeNode.Tag = channel;
			channel.Tag = treeNode;
			if (channel is ChannelFolder)
			{
				ChannelFolder channelFolder = channel as ChannelFolder;
				if (channelFolder.Expanded)
					treeNode.Expand();
				else
					treeNode.Collapse();
			}
			AdjustTVLogo(channel);
			return treeNode;
		}

		internal void RecursedFillTree(ChannelFolder parentChannel, TreeNodeCollection parentTreeNodeCollection)
		{
			foreach (Channel channel in parentChannel.ChannelList)
			{
				channel.Parent = parentChannel;

				TreeNode treeNode = MakeTreeNodeFromChannel(channel);

				if (channel is ChannelFolder)
					RecursedFillTree(channel as ChannelFolder, treeNode.Nodes);

				parentTreeNodeCollection.Add(treeNode);
			}
		}

		internal void InsertChannels(Stream fileStream, ChannelFolder parentChannel, TreeNodeCollection parentTreeNodeCollection)
		{
			ChannelFolder channels = (ChannelFolder)Channel.Deserialize(fileStream);
			if (parentChannel == null)
				parentChannel = MainForm.rootChannelFolder;
			if (parentTreeNodeCollection == null)
				parentTreeNodeCollection = this.treeViewChannel.Nodes;

			// First level: we add to the existing folder
			// Second level: we adjust the parent
			foreach (Channel channel in channels.ChannelList)
			{
				parentChannel.Add(channel);

				TreeNode treeNode = MakeTreeNodeFromChannel(channel);

				if (channel is ChannelFolder)
					RecursedFillTree(channel as ChannelFolder, treeNode.Nodes);

				parentTreeNodeCollection.Add(treeNode);
			}

			MainForm.UpdateChannelNumber();
		}

		internal void LoadChannels(Stream fileStream)
		{
			this.treeViewChannel.Nodes.Clear();
			MainForm.rootChannelFolder.ChannelList.Clear();

			InsertChannels(fileStream, null, null);
		}

		internal void SaveChannels(Stream fileStream)
		{
			MainForm.rootChannelFolder.Serialize(fileStream);
		}



		private void loadReplaceAllChannelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = this.GetType().Module.FullyQualifiedName;
			openFileDialog.Filter = Properties.Resources.ChannelXMLFilter;
			openFileDialog.FilterIndex = 2;
			openFileDialog.RestoreDirectory = true;
			openFileDialog.FileName = MainForm.AssemblyName + ".Channels.xml";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				Stream fileStream = null;
				try
				{
					if ((fileStream = openFileDialog.OpenFile()) != null)
						LoadChannels(fileStream);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
				finally
				{
					if (fileStream != null)
						fileStream.Close();
				}
			}
		}

		private void insertChannelsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = this.GetType().Module.FullyQualifiedName;
			openFileDialog.Filter = Properties.Resources.ChannelXMLFilter;
			openFileDialog.FilterIndex = 2;
			openFileDialog.RestoreDirectory = true;
			openFileDialog.FileName = MainForm.AssemblyName + ".Channels.xml";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				Stream fileStream = null;
				try
				{
					if ((fileStream = openFileDialog.OpenFile()) != null)
					{
						ChannelFolder parentChannel = MainForm.rootChannelFolder;
						TreeNodeCollection parentTreeNodeCollection = this.treeViewChannel.Nodes;
						if (this.treeViewChannel.SelectedNode != null)
						{
							if (this.treeViewChannel.SelectedNode.Tag is ChannelFolder)
							{
								parentChannel = this.treeViewChannel.SelectedNode.Tag as ChannelFolder;
								parentTreeNodeCollection = this.treeViewChannel.SelectedNode.Nodes;
							}
							else
							{
								if (this.treeViewChannel.SelectedNode.Parent != null)
								{
									parentChannel = this.treeViewChannel.SelectedNode.Parent.Tag as ChannelFolder;
									parentTreeNodeCollection = this.treeViewChannel.SelectedNode.Parent.Nodes;
								}
							}
						}
						InsertChannels(fileStream, parentChannel, parentTreeNodeCollection);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
				finally
				{
					if (fileStream != null)
						fileStream.Close();
				}
			}
		}

		private void saveAllChannelsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			//saveFileDialog.InitialDirectory = "c:\\" ;
			saveFileDialog.Filter = Properties.Resources.ChannelXMLFilter;
			saveFileDialog.FilterIndex = 2;
			saveFileDialog.RestoreDirectory = true;
			saveFileDialog.FileName = MainForm.AssemblyName + ".Channels.xml";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				Stream fileStream = null;
				try
				{
					if ((fileStream = saveFileDialog.OpenFile()) != null)
						SaveChannels(fileStream);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
				finally
				{
					if (fileStream != null)
						fileStream.Close();
				}
			}
		}

		private void addFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ChannelFolder channelFolder = new ChannelFolder(Properties.Resources.NewChannelFolder);
			TreeNode treeNode = MakeTreeNodeFromChannel(channelFolder);
			treeNode.Expand();

			if (this.treeViewChannel.SelectedNode == null || this.treeViewChannel.SelectedNode.Parent == null)
			{
				ChannelFolder parentChannelFolder = (ChannelFolder)this.treeViewChannel.Tag;
				parentChannelFolder.Add(channelFolder);
				this.treeViewChannel.Nodes.Add(treeNode);
			}
			else if (this.treeViewChannel.SelectedNode.Tag is ChannelFolder)
			{
				ChannelFolder parentChannelFolder = (ChannelFolder)this.treeViewChannel.SelectedNode.Tag;
				parentChannelFolder.Add(channelFolder);
				this.treeViewChannel.SelectedNode.Nodes.Add(treeNode);
				this.treeViewChannel.SelectedNode.Expand();
			}
			else
			{
				ChannelFolder parentChannelFolder = (ChannelFolder)this.treeViewChannel.SelectedNode.Parent.Tag;
				TreeNode parentTreeNode = this.treeViewChannel.SelectedNode.Parent;
				int indexChannel = parentChannelFolder.ChannelList.IndexOf(this.treeViewChannel.SelectedNode.Tag as Channel);
				int indexTreeNode = parentTreeNode.Nodes.IndexOf(this.treeViewChannel.SelectedNode);

				channelFolder.Parent = parentChannelFolder;
				parentChannelFolder.ChannelList.Insert(indexChannel, channelFolder);
				this.treeViewChannel.SelectedNode.Parent.Nodes.Insert(indexTreeNode, treeNode);
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.treeViewChannel.SelectedNode != null)
			{
				Hashtable selectedChannels = new Hashtable();
				foreach (TreeNode tn in this.treeViewChannel.SelectedNodes)
				{
					TreeNode t = tn;
					TreeNode tnParent = tn.Parent;
					while (this.treeViewChannel.SelectedNodes.Contains(tnParent))
					{
						t = tnParent;
						tnParent = t.Parent;
					}
					if (t != null)
						selectedChannels[t.Tag] = t;
				}

				foreach (TreeNode tn in selectedChannels.Values)
				{
					Channel selectedChannel = (Channel)tn.Tag;
					(selectedChannel.Parent as ChannelFolder).ChannelList.Remove(selectedChannel);
					if (tn.Parent == null)
						this.treeViewChannel.Nodes.Remove(tn);
					else
						tn.Parent.Nodes.Remove(tn);
				}

				MainForm.UpdateChannelNumber();
				//Channel selectedChannel = (Channel)this.treeViewChannel.SelectedNode.Tag;
				//(selectedChannel.Parent as ChannelFolder).ChannelList.Remove(selectedChannel);
				//if (this.treeViewChannel.SelectedNode.Parent == null)
				//    this.treeViewChannel.Nodes.Remove(this.treeViewChannel.SelectedNode);
				//else
				//    this.treeViewChannel.SelectedNode.Parent.Nodes.Remove(this.treeViewChannel.SelectedNode);
			}
		}

		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.treeViewChannel.SelectedNode != null)
			{
				this.treeViewChannel.LabelEdit = true;
				this.treeViewChannel.SelectedNode.BeginEdit();
			}
		}

		private void propertyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowProperties();
			MainForm.panelChannelProperties.Show(MainForm.dockPanel);
		}

		internal Hashtable currentPropertyChannels;

		public void ShowProperties()
		{
			this.currentPropertyChannels = new Hashtable();
			foreach (TreeNode tn in this.treeViewChannel.SelectedNodes)
				this.currentPropertyChannels[(tn.Tag as Channel).MakeCopy()] = tn;

			MainForm.panelChannelProperties.propertyGridChannel.SelectedObjects = (new ArrayList(this.currentPropertyChannels.Keys)).ToArray();

			if (this.treeViewChannel.SelectedNodes.Count == 1 && (this.treeViewChannel.SelectedNodes[0] as TreeNode).Tag is ChannelTV)
				MainForm.panelChannelProperties.comboBoxTunerType.SelectedIndex = MainForm.panelChannelProperties.comboBoxTunerType.FindStringExact(new TunerTypeEx(((this.treeViewChannel.SelectedNodes[0] as TreeNode).Tag as ChannelTV).TunerType).ToString());

			MainForm.panelChannelProperties.buttonPropertyApply.Enabled = true;
		}

		private void contextMenuStripChannels_Opening(object sender, CancelEventArgs e)
		{
			Point mousePosition = ContextMenuStrip.MousePosition;
			mousePosition = this.treeViewChannel.PointToClient(mousePosition);

			bool clickOnSelection = false;

			bool atLeastOneChannelSelected = (this.treeViewChannel.SelectedNodes.Count > 0);
			bool onlyOneChannelSelected = (this.treeViewChannel.SelectedNodes.Count == 1);
			bool onlyOneChannelTVSelected = (onlyOneChannelSelected && (this.treeViewChannel.SelectedNodes[0] as TreeNode).Tag is ChannelTV);
			bool showPropertyPossible = true;

			if (!onlyOneChannelSelected)
			{
				foreach (TreeNode tn in this.treeViewChannel.SelectedNodes)
				{
					Channel channel = tn.Tag as Channel;
					if (channel is ChannelFolder)
						showPropertyPossible = false;
					if (tn.Bounds.Contains(mousePosition))
						clickOnSelection = true;
				}
			}

			if (!clickOnSelection)
			{
				TreeViewHitTestInfo tvhti = this.treeViewChannel.HitTest(mousePosition);
				if ((tvhti.Location & TreeViewHitTestLocations.Label) != 0 || (tvhti.Location & TreeViewHitTestLocations.Image) != 0)
				{
					this.treeViewChannel.SelectedNode = tvhti.Node;

					Channel channel = tvhti.Node.Tag as Channel;
					if (channel is ChannelFolder)
						showPropertyPossible = false;

					atLeastOneChannelSelected = true;
					onlyOneChannelSelected = true;
					onlyOneChannelTVSelected = channel is ChannelTV;
				}
			}

			this.propertyToolStripMenuItem.Enabled = showPropertyPossible;
			this.switchOnToolStripMenuItem.Enabled = onlyOneChannelTVSelected;
			this.tuneAllChannelInMosaicToolStripMenuItem.Enabled = onlyOneChannelTVSelected;
			this.renameToolStripMenuItem.Enabled = onlyOneChannelSelected;
			this.deleteToolStripMenuItem.Enabled = atLeastOneChannelSelected;
		}

		private bool GetFavoriteContainer(out ChannelFolder parentChannelFolder, out TreeNodeCollection parentTreeNodeCollection)
		{
			if (this.treeViewChannel.SelectedNode == null || (this.treeViewChannel.SelectedNode.Parent == null && !(this.treeViewChannel.SelectedNode.Tag is ChannelFolder)))
			{
				parentChannelFolder = (ChannelFolder)this.treeViewChannel.Tag;
				parentTreeNodeCollection = this.treeViewChannel.Nodes;
				return true;
			}
			else if (this.treeViewChannel.SelectedNode.Tag is ChannelTV)
			{
				TreeNode parentTreeNode = this.treeViewChannel.SelectedNode.Parent;
				parentChannelFolder = (ChannelFolder)parentTreeNode.Tag;
				parentTreeNodeCollection = parentTreeNode.Nodes;
				return true;
			}
			else if (this.treeViewChannel.SelectedNode.Tag is ChannelFolder)
			{
				parentChannelFolder = (ChannelFolder)this.treeViewChannel.SelectedNode.Tag;
				parentTreeNodeCollection = treeViewChannel.SelectedNode.Nodes;
				return true;
			}
			parentChannelFolder = null;
			parentTreeNodeCollection = null;
			return false;
		}

		internal void AddChannelToFavorite(Channel[] channels)
		{
			ChannelFolder parentChannelFolder;
			TreeNodeCollection parentTreeNodeCollection;

			if (GetFavoriteContainer(out parentChannelFolder, out parentTreeNodeCollection))
			{
				foreach (Channel channel in channels)
				{
					parentChannelFolder.Add(channel);
					parentTreeNodeCollection.Add(MakeTreeNodeFromChannel(channel));
				}
			}
		}

		internal void AddChannelToFavorite(ChannelFolder channelFolder, Channel[] channels)
		{
			TreeNode parentTreeNode = channelFolder.Tag as TreeNode;
			TreeNodeCollection parentTreeNodeCollection = (parentTreeNode == null ? this.treeViewChannel.Nodes : parentTreeNode.Nodes);
			foreach (Channel channel in channels)
			{
				channelFolder.Add(channel);
				parentTreeNodeCollection.Add(MakeTreeNodeFromChannel(channel));
			}
		}

		private void numberiToolStripMenuItem_Click(object sender, EventArgs e)
		{
			WizardForm wizardForm = new WizardForm(MainForm);
			wizardForm.StartOnNumbering = true;
			wizardForm.ShowDialog(MainForm);
		}
	}
}