namespace CodeTV
{
	partial class PanelChannel
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelChannel));
			this.treeViewChannel = new CodeTV.Controls.MultiTreeView();
			this.contextMenuStripChannels = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.switchOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.rebuildTheGraphAndTuneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tuneAllChannelInMosaicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.loadReplaceAllChannleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.insertChannelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAllChannelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.addFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.numberiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.propertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripChannels.SuspendLayout();
			this.SuspendLayout();
			// 
			// treeViewChannel
			// 
			this.treeViewChannel.AllowDrop = true;
			this.treeViewChannel.ContextMenuStrip = this.contextMenuStripChannels;
			resources.ApplyResources(this.treeViewChannel, "treeViewChannel");
			this.treeViewChannel.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.treeViewChannel.HideSelection = false;
			this.treeViewChannel.Name = "treeViewChannel";
			this.treeViewChannel.UseRubberBand = false;
			this.treeViewChannel.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.treeViewChannel_QueryContinueDrag);
			this.treeViewChannel.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeViewChannel_AfterCollapse);
			this.treeViewChannel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeViewChannel_MouseClick);
			this.treeViewChannel.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewChannel_AfterLabelEdit);
			this.treeViewChannel.DoubleClick += new System.EventHandler(this.treeViewChannel_DoubleClick);
			this.treeViewChannel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeViewChannel_MouseUp);
			this.treeViewChannel.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewChannel_DragDrop);
			this.treeViewChannel.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewChannel_AfterSelect);
			this.treeViewChannel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.treeViewChannel_MouseMove);
			this.treeViewChannel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeViewChannel_MouseDown);
			this.treeViewChannel.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeViewChannel_AfterExpand);
			this.treeViewChannel.DragOver += new System.Windows.Forms.DragEventHandler(this.treeViewChannel_DragOver);
			// 
			// contextMenuStripChannels
			// 
			this.contextMenuStripChannels.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.switchOnToolStripMenuItem,
            this.rebuildTheGraphAndTuneToolStripMenuItem,
            this.tuneAllChannelInMosaicToolStripMenuItem,
            this.toolStripMenuItem2,
            this.loadReplaceAllChannleToolStripMenuItem,
            this.insertChannelsToolStripMenuItem,
            this.saveAllChannelsToolStripMenuItem,
            this.toolStripMenuItem3,
            this.addFolderToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.numberiToolStripMenuItem,
            this.propertyToolStripMenuItem});
			this.contextMenuStripChannels.Name = "contextMenuStripChannels";
			resources.ApplyResources(this.contextMenuStripChannels, "contextMenuStripChannels");
			this.contextMenuStripChannels.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripChannels_Opening);
			// 
			// switchOnToolStripMenuItem
			// 
			resources.ApplyResources(this.switchOnToolStripMenuItem, "switchOnToolStripMenuItem");
			this.switchOnToolStripMenuItem.Name = "switchOnToolStripMenuItem";
			this.switchOnToolStripMenuItem.Click += new System.EventHandler(this.switchOnToolStripMenuItem_Click);
			// 
			// rebuildTheGraphAndTuneToolStripMenuItem
			// 
			this.rebuildTheGraphAndTuneToolStripMenuItem.Name = "rebuildTheGraphAndTuneToolStripMenuItem";
			resources.ApplyResources(this.rebuildTheGraphAndTuneToolStripMenuItem, "rebuildTheGraphAndTuneToolStripMenuItem");
			this.rebuildTheGraphAndTuneToolStripMenuItem.Click += new System.EventHandler(this.rebuildTheGraphAndTuneToolStripMenuItem_Click);
			// 
			// tuneAllChannelInMosaicToolStripMenuItem
			// 
			this.tuneAllChannelInMosaicToolStripMenuItem.Name = "tuneAllChannelInMosaicToolStripMenuItem";
			resources.ApplyResources(this.tuneAllChannelInMosaicToolStripMenuItem, "tuneAllChannelInMosaicToolStripMenuItem");
			this.tuneAllChannelInMosaicToolStripMenuItem.Click += new System.EventHandler(this.tuneAllChannelInMosaicToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
			// 
			// loadReplaceAllChannleToolStripMenuItem
			// 
			this.loadReplaceAllChannleToolStripMenuItem.Name = "loadReplaceAllChannleToolStripMenuItem";
			resources.ApplyResources(this.loadReplaceAllChannleToolStripMenuItem, "loadReplaceAllChannleToolStripMenuItem");
			this.loadReplaceAllChannleToolStripMenuItem.Click += new System.EventHandler(this.loadReplaceAllChannelToolStripMenuItem_Click);
			// 
			// insertChannelsToolStripMenuItem
			// 
			this.insertChannelsToolStripMenuItem.Name = "insertChannelsToolStripMenuItem";
			resources.ApplyResources(this.insertChannelsToolStripMenuItem, "insertChannelsToolStripMenuItem");
			this.insertChannelsToolStripMenuItem.Click += new System.EventHandler(this.insertChannelsToolStripMenuItem_Click);
			// 
			// saveAllChannelsToolStripMenuItem
			// 
			this.saveAllChannelsToolStripMenuItem.Name = "saveAllChannelsToolStripMenuItem";
			resources.ApplyResources(this.saveAllChannelsToolStripMenuItem, "saveAllChannelsToolStripMenuItem");
			this.saveAllChannelsToolStripMenuItem.Click += new System.EventHandler(this.saveAllChannelsToolStripMenuItem_Click);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
			// 
			// addFolderToolStripMenuItem
			// 
			this.addFolderToolStripMenuItem.Name = "addFolderToolStripMenuItem";
			resources.ApplyResources(this.addFolderToolStripMenuItem, "addFolderToolStripMenuItem");
			this.addFolderToolStripMenuItem.Click += new System.EventHandler(this.addFolderToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// renameToolStripMenuItem
			// 
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			resources.ApplyResources(this.renameToolStripMenuItem, "renameToolStripMenuItem");
			this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
			// 
			// numberiToolStripMenuItem
			// 
			this.numberiToolStripMenuItem.Name = "numberiToolStripMenuItem";
			resources.ApplyResources(this.numberiToolStripMenuItem, "numberiToolStripMenuItem");
			this.numberiToolStripMenuItem.Click += new System.EventHandler(this.numberiToolStripMenuItem_Click);
			// 
			// propertyToolStripMenuItem
			// 
			this.propertyToolStripMenuItem.Name = "propertyToolStripMenuItem";
			resources.ApplyResources(this.propertyToolStripMenuItem, "propertyToolStripMenuItem");
			this.propertyToolStripMenuItem.Click += new System.EventHandler(this.propertyToolStripMenuItem_Click);
			// 
			// PanelChannel
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.treeViewChannel);
			this.Name = "PanelChannel";
			this.contextMenuStripChannels.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal CodeTV.Controls.MultiTreeView treeViewChannel;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripChannels;
		private System.Windows.Forms.ToolStripMenuItem switchOnToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rebuildTheGraphAndTuneToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem tuneAllChannelInMosaicToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem loadReplaceAllChannleToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem insertChannelsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAllChannelsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem addFolderToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem propertyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem numberiToolStripMenuItem;
	}
}