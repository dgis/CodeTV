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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CodeTV.Controls
{
	class MultiTreeView : TreeView
	{
		private TreeNode pivotTreeNode = null;

		private bool isRubberbandStarted = false;
		private Point rubberbandStart;
		private bool isAlreadyBanding = false;
		private Rectangle lastRubberbandRectangle;

		//private bool labelEdit = false;
		//private bool labelEditFirstClick = true;

		private ArrayList selectedNodes = new ArrayList();
		private bool useRubberBand = false;

		public ArrayList SelectedNodes { get { return this.selectedNodes; } }
		public bool UseRubberBand { get { return this.useRubberBand; } set { this.useRubberBand = value; } }

		public MultiTreeView()
			: base()
		{
			DrawMode = TreeViewDrawMode.OwnerDrawText;
			DrawNode += new DrawTreeNodeEventHandler(MultiTreeView_DrawNode);
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			//this.labelEdit = LabelEdit;
		}

		//private Font nodeFont2 = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Regular);

		void MultiTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			if (e.Bounds.Height == 0)
				return;

			// Retrieve the node font. If the node font has not been set,
			// use the TreeView font.
			Font nodeFont = e.Node.NodeFont;
			if (nodeFont == null) nodeFont = Font;

			Rectangle bounds = e.Node.Bounds;
			bounds.Width = (int)e.Graphics.MeasureString(e.Node.Text, nodeFont).Width + 5; //2;
			Rectangle textBounds = bounds;
			textBounds.Y += 1;
			textBounds.Height -= 1;

			Color foreColor, backColor;
			if (e.Node.Checked)
			{
				foreColor = BackColor;
				backColor = SystemColors.Highlight;
			}
			else
			{
				foreColor = ForeColor;
				backColor = BackColor;
			}

			// Draw the background and node text for a selected node.
			e.Graphics.FillRectangle(new SolidBrush(backColor), bounds);

			// Draw the node text.
			//e.Graphics.DrawString(e.Node.Text, nodeFont, BackColor, textBounds);
			e.Graphics.DrawString(e.Node.Text, nodeFont, new SolidBrush(foreColor), textBounds.X, textBounds.Y);


			// If the node has focus, draw the focus rectangle large, making
			// it large enough to include the text of the node tag, if present.
			if ((e.State & TreeNodeStates.Focused) != 0)
			{
				using (Pen focusPen = new Pen(Color.Black))
				{
					focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
					Rectangle focusBounds = bounds;
					focusBounds.Size = new Size(focusBounds.Width - 1, focusBounds.Height - 1);
					e.Graphics.DrawRectangle(focusPen, focusBounds);
				}
			}
		}

		public bool IsHierarchySelected(TreeNode treeNode)
		{
			while (treeNode != null)
				if (treeNode.Checked)
					return true;
				else
					treeNode = treeNode.Parent;
			return false;
		}

		public bool IsSelected(TreeNode treeNode)
		{
			return treeNode.Checked;
			//foreach (TreeNode tn in this.selectedNodes)
			//    if (tn == treeNode)
			//        return true;
			//return false;
		}

		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			TreeViewHitTestInfo tvhti = HitTest(e.Location);
			if (e.Button == MouseButtons.Left)
			{
				if ((tvhti.Location & TreeViewHitTestLocations.Label) == 0)
				{
					if (this.useRubberBand)
					{
						this.isRubberbandStarted = true;
						this.rubberbandStart = e.Location;
					}
					else
					{
						BeginUpdate();
						if ((ModifierKeys & Keys.Control) == 0)
						{
							ArrayList al = new ArrayList(this.selectedNodes);
							foreach (TreeNode tn in al)
								tn.Checked = false;
							this.selectedNodes.Clear();
						}
						EndUpdate();
					}
				}
				else
				{
					bool isSelected = IsSelected(tvhti.Node);
					//if (this.labelEdit)
					//{
					//    if (labelEditFirstClick)
					//    {
					//        labelEditFirstClick = false;
					//        LabelEdit = false;
					//    }
					//    else if (isSelected && (ModifierKeys & (Keys.Control | Keys.Shift)) == 0)
					//    {
					//        LabelEdit = true;
					//        labelEditFirstClick = true;
					//        tvhti.Node.BeginEdit();
					//    }
					//    else
					//    {
					//        labelEditFirstClick = false;
					//    }
					//}
					if (!isSelected)
					{
						if (SelectedNode == tvhti.Node)
							SelectedNode = null;
						SelectedNode = tvhti.Node;
					}
				}
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			if (this.isRubberbandStarted && e.Button == MouseButtons.Left)
			{
				this.isRubberbandStarted = false;
				if (this.isAlreadyBanding)
				{
					Invalidate(lastRubberbandRectangle, false);
					Update();
					this.isAlreadyBanding = false;
				}
			}

			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			if (this.isRubberbandStarted)
			{
				Rectangle rubberbandRectangle = new Rectangle(rubberbandStart.X, rubberbandStart.Y, e.X - rubberbandStart.X, e.Y - rubberbandStart.Y);
				if (rubberbandRectangle.Width < 0)
				{
					rubberbandRectangle.X = rubberbandRectangle.Right;
					rubberbandRectangle.Width = -rubberbandRectangle.Width;
				}
				if (rubberbandRectangle.Height < 0)
				{
					rubberbandRectangle.Y = rubberbandRectangle.Bottom;
					rubberbandRectangle.Height = -rubberbandRectangle.Height;
				}

				BeginUpdate();
				if ((ModifierKeys & Keys.Control) == 0)
				{
					ArrayList al = new ArrayList(this.selectedNodes);
					foreach (TreeNode tn in al)
						tn.Checked = false;
					this.selectedNodes.Clear();
				}
				TreeNode treeNode = TopNode;
				while (treeNode != null)
				{
					if (rubberbandRectangle.IntersectsWith(treeNode.Bounds))
						treeNode.Checked = true;
					treeNode = treeNode.NextVisibleNode;
				}
				EndUpdate();

				if (this.isAlreadyBanding)
				{
					Invalidate(lastRubberbandRectangle, false);
					Update();
				}

				//BeginUpdate();

				//ControlPaint.DrawReversibleFrame(RectangleToScreen(rubberbandRectangle), this.BackColor, FrameStyle.Dashed);

				Graphics g = CreateGraphics();
				//Rectangle rubberbandRectangleInside = rubberbandRectangle;
				//rubberbandRectangleInside.Inflate(-4, -4);
				//ControlPaint.DrawSelectionFrame(g, true, rubberbandRectangle, rubberbandRectangleInside, this.BackColor);

				Rectangle rubberbandRectangleInside = rubberbandRectangle;
				rubberbandRectangleInside.Inflate(-1, -1);
				Color selectionForeColor = SystemColors.Highlight;
				g.DrawRectangle(new Pen(new SolidBrush(selectionForeColor)), rubberbandRectangleInside);

				//Color selectionBackColor = Color.FromArgb(128, selectionForeColor);
				//g.FillRectangle(new SolidBrush(selectionBackColor), rubberbandRectangleInside);

				this.isAlreadyBanding = true;
				this.lastRubberbandRectangle = rubberbandRectangle;

				//EndUpdate();
			}

			base.OnMouseMove(e);
		}

		protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
			BeginUpdate();

			if ((ModifierKeys & Keys.Control) != 0)
			{
				e.Node.BackColor = (!e.Node.Checked ? Color.LightBlue : BackColor);
				e.Node.Checked ^= true;
			}
			else
			{
				ArrayList al = new ArrayList(this.selectedNodes);
				foreach (TreeNode treeNode in al)
					treeNode.Checked = false;
				this.selectedNodes.Clear();
				e.Node.Checked = true;
			}
			if ((ModifierKeys & Keys.Shift) != 0)
			{
				if (pivotTreeNode == null)
					pivotTreeNode = SelectedNode;
				if (pivotTreeNode != null)
				{
					if (e.Node.Bounds.Top >= pivotTreeNode.Bounds.Top)
					{
						TreeNode treeNode = pivotTreeNode;
						while (treeNode != null && treeNode != e.Node)
						{
							treeNode.Checked = true;
							treeNode = treeNode.NextVisibleNode;
						}
					}
					else
					{
						TreeNode treeNode = pivotTreeNode;
						while (treeNode != null && treeNode != e.Node)
						{
							treeNode.Checked = true;
							treeNode = treeNode.PrevVisibleNode;
						}
					}
				}
			}
			else
				pivotTreeNode = e.Node;
			EndUpdate();

			base.OnBeforeSelect(e);
		}

		protected override void OnAfterCheck(TreeViewEventArgs e)
		{
			if (e.Node.Checked)
			{
				e.Node.BackColor = SystemColors.MenuHighlight;
				e.Node.ForeColor = BackColor;
			}
			else
			{
				e.Node.BackColor = BackColor;
				e.Node.ForeColor = ForeColor;
			}

			if (e.Node.Checked)
			{
				// The list must be ordered
				if (this.selectedNodes.Count != 0)
				{
					bool inserted = false;
					int currentNodeY = e.Node.Bounds.Y;
					int selectedCount = this.selectedNodes.Count;
					for (int i = selectedCount - 1; i >= 0 ; i--)
					{
						if (currentNodeY > (this.selectedNodes[i] as TreeNode).Bounds.Y)
						{
							this.selectedNodes.Insert(i + 1, e.Node);
							inserted = true;
							break;
						}
					}
					if (!inserted)
						this.selectedNodes.Insert(0, e.Node);
				}
				else
					this.selectedNodes.Add(e.Node);
			}
			else
				this.selectedNodes.Remove(e.Node);

			base.OnAfterCheck(e);
		}

		private const int WM_ERASEBKGND = 0x0014;

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_ERASEBKGND)
				return;

			base.WndProc(ref m);
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.ResumeLayout(false);

		}
	}
}
