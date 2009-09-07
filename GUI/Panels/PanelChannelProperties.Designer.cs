namespace CodeTV
{
	partial class PanelChannelProperties
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelChannelProperties));
			this.buttonChannelUpdatePid = new System.Windows.Forms.Button();
			this.buttonPropertyApply = new System.Windows.Forms.Button();
			this.buttonNewChannel = new System.Windows.Forms.Button();
			this.buttonAddChannelToFavorite = new System.Windows.Forms.Button();
			this.labelChannelType = new System.Windows.Forms.Label();
			this.buttonChannelTest = new System.Windows.Forms.Button();
			this.comboBoxTunerType = new System.Windows.Forms.ComboBox();
			this.propertyGridChannel = new System.Windows.Forms.PropertyGrid();
			this.contextMenuStripPropertyGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.resetChannelPropertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripPropertyGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonChannelUpdatePid
			// 
			resources.ApplyResources(this.buttonChannelUpdatePid, "buttonChannelUpdatePid");
			this.buttonChannelUpdatePid.Name = "buttonChannelUpdatePid";
			this.buttonChannelUpdatePid.UseVisualStyleBackColor = true;
			this.buttonChannelUpdatePid.Click += new System.EventHandler(this.buttonChannelUpdatePid_Click);
			// 
			// buttonPropertyApply
			// 
			resources.ApplyResources(this.buttonPropertyApply, "buttonPropertyApply");
			this.buttonPropertyApply.Name = "buttonPropertyApply";
			this.buttonPropertyApply.UseVisualStyleBackColor = true;
			this.buttonPropertyApply.Click += new System.EventHandler(this.buttonPropertyApply_Click);
			// 
			// buttonNewChannel
			// 
			resources.ApplyResources(this.buttonNewChannel, "buttonNewChannel");
			this.buttonNewChannel.Name = "buttonNewChannel";
			this.buttonNewChannel.UseVisualStyleBackColor = true;
			this.buttonNewChannel.Click += new System.EventHandler(this.buttonNewChannel_Click);
			// 
			// buttonAddChannelToFavorite
			// 
			resources.ApplyResources(this.buttonAddChannelToFavorite, "buttonAddChannelToFavorite");
			this.buttonAddChannelToFavorite.Name = "buttonAddChannelToFavorite";
			this.buttonAddChannelToFavorite.UseVisualStyleBackColor = true;
			this.buttonAddChannelToFavorite.Click += new System.EventHandler(this.buttonAddChannelToFavorite_Click);
			// 
			// labelChannelType
			// 
			resources.ApplyResources(this.labelChannelType, "labelChannelType");
			this.labelChannelType.Name = "labelChannelType";
			// 
			// buttonChannelTest
			// 
			resources.ApplyResources(this.buttonChannelTest, "buttonChannelTest");
			this.buttonChannelTest.Name = "buttonChannelTest";
			this.buttonChannelTest.UseVisualStyleBackColor = true;
			this.buttonChannelTest.Click += new System.EventHandler(this.buttonChannelTest_Click);
			// 
			// comboBoxTunerType
			// 
			resources.ApplyResources(this.comboBoxTunerType, "comboBoxTunerType");
			this.comboBoxTunerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTunerType.FormattingEnabled = true;
			this.comboBoxTunerType.Name = "comboBoxTunerType";
			this.comboBoxTunerType.SelectedIndexChanged += new System.EventHandler(this.comboBoxTunerType_SelectedIndexChanged);
			// 
			// propertyGridChannel
			// 
			resources.ApplyResources(this.propertyGridChannel, "propertyGridChannel");
			this.propertyGridChannel.ContextMenuStrip = this.contextMenuStripPropertyGrid;
			this.propertyGridChannel.Name = "propertyGridChannel";
			this.propertyGridChannel.ToolbarVisible = false;
			// 
			// contextMenuStripPropertyGrid
			// 
			this.contextMenuStripPropertyGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetChannelPropertyToolStripMenuItem});
			this.contextMenuStripPropertyGrid.Name = "contextMenuStripSnapShots";
			resources.ApplyResources(this.contextMenuStripPropertyGrid, "contextMenuStripPropertyGrid");
			// 
			// resetChannelPropertyToolStripMenuItem
			// 
			this.resetChannelPropertyToolStripMenuItem.Name = "resetChannelPropertyToolStripMenuItem";
			resources.ApplyResources(this.resetChannelPropertyToolStripMenuItem, "resetChannelPropertyToolStripMenuItem");
			this.resetChannelPropertyToolStripMenuItem.Click += new System.EventHandler(this.resetChannelPropertyToolStripMenuItem_Click);
			// 
			// PanelChannelProperties
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.buttonChannelUpdatePid);
			this.Controls.Add(this.buttonPropertyApply);
			this.Controls.Add(this.buttonAddChannelToFavorite);
			this.Controls.Add(this.buttonChannelTest);
			this.Controls.Add(this.buttonNewChannel);
			this.Controls.Add(this.labelChannelType);
			this.Controls.Add(this.propertyGridChannel);
			this.Controls.Add(this.comboBoxTunerType);
			this.Name = "PanelChannelProperties";
			this.Load += new System.EventHandler(this.PanelChannelProperties_Load);
			this.contextMenuStripPropertyGrid.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonChannelUpdatePid;
		internal System.Windows.Forms.Button buttonPropertyApply;
		private System.Windows.Forms.Button buttonNewChannel;
		private System.Windows.Forms.Button buttonAddChannelToFavorite;
		private System.Windows.Forms.Label labelChannelType;
		private System.Windows.Forms.Button buttonChannelTest;
		internal System.Windows.Forms.ComboBox comboBoxTunerType;
		internal System.Windows.Forms.PropertyGrid propertyGridChannel;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripPropertyGrid;
		private System.Windows.Forms.ToolStripMenuItem resetChannelPropertyToolStripMenuItem;
	}
}