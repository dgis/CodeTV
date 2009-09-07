namespace CodeTV
{
	partial class PanelSettings
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelSettings));
			this.propertyGridSettings = new System.Windows.Forms.PropertyGrid();
			this.contextMenuStripPropertyGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.resetChannelPropertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripPropertyGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// propertyGridSettings
			// 
			this.propertyGridSettings.ContextMenuStrip = this.contextMenuStripPropertyGrid;
			resources.ApplyResources(this.propertyGridSettings, "propertyGridSettings");
			this.propertyGridSettings.Name = "propertyGridSettings";
			this.propertyGridSettings.ToolbarVisible = false;
			this.propertyGridSettings.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridSettings_PropertyValueChanged);
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
			// PanelSettings
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.propertyGridSettings);
			this.Name = "PanelSettings";
			this.Load += new System.EventHandler(this.PanelSettings_Load);
			this.contextMenuStripPropertyGrid.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.PropertyGrid propertyGridSettings;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripPropertyGrid;
		private System.Windows.Forms.ToolStripMenuItem resetChannelPropertyToolStripMenuItem;
	}
}