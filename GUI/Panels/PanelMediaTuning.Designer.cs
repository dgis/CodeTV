namespace CodeTV
{
	partial class PanelMediaTuning
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelMediaTuning));
			this.buttonBalanceDefault = new System.Windows.Forms.Button();
			this.labelBalanceLevel = new System.Windows.Forms.Label();
			this.trackBarVolume = new CodeTV.Controls.TrackBarEx();
			this.labelVolumeLevel = new System.Windows.Forms.Label();
			this.labelVolume = new System.Windows.Forms.Label();
			this.labelBalance = new System.Windows.Forms.Label();
			this.trackBarBalance = new CodeTV.Controls.TrackBarEx();
			((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarBalance)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonBalanceDefault
			// 
			resources.ApplyResources(this.buttonBalanceDefault, "buttonBalanceDefault");
			this.buttonBalanceDefault.Name = "buttonBalanceDefault";
			this.buttonBalanceDefault.UseVisualStyleBackColor = true;
			this.buttonBalanceDefault.Click += new System.EventHandler(this.buttonBalanceDefault_Click);
			// 
			// labelBalanceLevel
			// 
			resources.ApplyResources(this.labelBalanceLevel, "labelBalanceLevel");
			this.labelBalanceLevel.Name = "labelBalanceLevel";
			// 
			// trackBarVolume
			// 
			resources.ApplyResources(this.trackBarVolume, "trackBarVolume");
			this.trackBarVolume.BackColor = System.Drawing.SystemColors.Control;
			this.trackBarVolume.Maximum = 0;
			this.trackBarVolume.Minimum = -10000;
			this.trackBarVolume.Name = "trackBarVolume";
			this.trackBarVolume.TickFrequency = 1000;
			this.trackBarVolume.ValueChanged += new System.EventHandler(this.trackBarVolume_ValueChanged);
			// 
			// labelVolumeLevel
			// 
			resources.ApplyResources(this.labelVolumeLevel, "labelVolumeLevel");
			this.labelVolumeLevel.Name = "labelVolumeLevel";
			// 
			// labelVolume
			// 
			resources.ApplyResources(this.labelVolume, "labelVolume");
			this.labelVolume.Name = "labelVolume";
			// 
			// labelBalance
			// 
			resources.ApplyResources(this.labelBalance, "labelBalance");
			this.labelBalance.Name = "labelBalance";
			// 
			// trackBarBalance
			// 
			resources.ApplyResources(this.trackBarBalance, "trackBarBalance");
			this.trackBarBalance.BackColor = System.Drawing.SystemColors.Control;
			this.trackBarBalance.Maximum = 10000;
			this.trackBarBalance.Minimum = -10000;
			this.trackBarBalance.Name = "trackBarBalance";
			this.trackBarBalance.TickFrequency = 2000;
			this.trackBarBalance.ValueChanged += new System.EventHandler(this.trackBarBalance_ValueChanged);
			// 
			// PanelMediaTuning
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.buttonBalanceDefault);
			this.Controls.Add(this.labelBalanceLevel);
			this.Controls.Add(this.trackBarVolume);
			this.Controls.Add(this.labelVolumeLevel);
			this.Controls.Add(this.labelVolume);
			this.Controls.Add(this.labelBalance);
			this.Controls.Add(this.trackBarBalance);
			this.Name = "PanelMediaTuning";
			((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarBalance)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonBalanceDefault;
		private System.Windows.Forms.Label labelBalanceLevel;
		internal CodeTV.Controls.TrackBarEx trackBarVolume;
		private System.Windows.Forms.Label labelVolumeLevel;
		private System.Windows.Forms.Label labelVolume;
		private System.Windows.Forms.Label labelBalance;
		internal CodeTV.Controls.TrackBarEx trackBarBalance;
	}
}