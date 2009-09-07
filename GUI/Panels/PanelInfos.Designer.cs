namespace CodeTV
{
	partial class PanelInfos
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelInfos));
			this.textBoxChannleInfo = new System.Windows.Forms.TextBox();
			this.buttonGetTransponderInfos = new System.Windows.Forms.Button();
			this.buttonGetChannelInfos = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textBoxChannleInfo
			// 
			resources.ApplyResources(this.textBoxChannleInfo, "textBoxChannleInfo");
			this.textBoxChannleInfo.Name = "textBoxChannleInfo";
			// 
			// buttonGetTransponderInfos
			// 
			resources.ApplyResources(this.buttonGetTransponderInfos, "buttonGetTransponderInfos");
			this.buttonGetTransponderInfos.Name = "buttonGetTransponderInfos";
			this.buttonGetTransponderInfos.UseVisualStyleBackColor = true;
			this.buttonGetTransponderInfos.Click += new System.EventHandler(this.buttonGetTransponderInfos_Click);
			// 
			// buttonGetChannelInfos
			// 
			resources.ApplyResources(this.buttonGetChannelInfos, "buttonGetChannelInfos");
			this.buttonGetChannelInfos.Name = "buttonGetChannelInfos";
			this.buttonGetChannelInfos.UseVisualStyleBackColor = true;
			this.buttonGetChannelInfos.Click += new System.EventHandler(this.buttonGetChannelInfos_Click);
			// 
			// PanelInfos
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.textBoxChannleInfo);
			this.Controls.Add(this.buttonGetTransponderInfos);
			this.Controls.Add(this.buttonGetChannelInfos);
			this.Name = "PanelInfos";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxChannleInfo;
		private System.Windows.Forms.Button buttonGetTransponderInfos;
		private System.Windows.Forms.Button buttonGetChannelInfos;
	}
}