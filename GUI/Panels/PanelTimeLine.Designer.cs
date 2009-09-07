namespace CodeTV
{
	partial class PanelTimeLine
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelTimeLine));
			this.labelSpeed2 = new System.Windows.Forms.Label();
			this.labelSpeed = new System.Windows.Forms.Label();
			this.trackBarSpeed = new CodeTV.Controls.TrackBarSpeed();
			this.trackBarExTimeLine = new CodeTV.Controls.TrackBarTimeLine();
			((System.ComponentModel.ISupportInitialize)(this.trackBarSpeed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarExTimeLine)).BeginInit();
			this.SuspendLayout();
			// 
			// labelSpeed2
			// 
			resources.ApplyResources(this.labelSpeed2, "labelSpeed2");
			this.labelSpeed2.Name = "labelSpeed2";
			// 
			// labelSpeed
			// 
			resources.ApplyResources(this.labelSpeed, "labelSpeed");
			this.labelSpeed.Name = "labelSpeed";
			// 
			// trackBarSpeed
			// 
			resources.ApplyResources(this.trackBarSpeed, "trackBarSpeed");
			this.trackBarSpeed.DefaultValue = 100;
			this.trackBarSpeed.Maximum = 3200;
			this.trackBarSpeed.Minimum = -3200;
			this.trackBarSpeed.Name = "trackBarSpeed";
			this.trackBarSpeed.TickFrequency = 100;
			this.trackBarSpeed.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackBarSpeed.Value = 1;
			this.trackBarSpeed.ValueChanged += new System.EventHandler(this.trackBarSpeed_ValueChanged);
			// 
			// trackBarExTimeLine
			// 
			resources.ApplyResources(this.trackBarExTimeLine, "trackBarExTimeLine");
			this.trackBarExTimeLine.Name = "trackBarExTimeLine";
			this.trackBarExTimeLine.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackBarExTimeLine.ValueChanged += new System.EventHandler(this.trackBarExTimeLine_Scroll);
			this.trackBarExTimeLine.Scroll += new System.EventHandler(this.trackBarExTimeLine_Scroll);
			// 
			// PanelTimeLine
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.labelSpeed);
			this.Controls.Add(this.trackBarExTimeLine);
			this.Controls.Add(this.labelSpeed2);
			this.Controls.Add(this.trackBarSpeed);
			this.Name = "PanelTimeLine";
			((System.ComponentModel.ISupportInitialize)(this.trackBarSpeed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarExTimeLine)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelSpeed;
		internal CodeTV.Controls.TrackBarTimeLine trackBarExTimeLine;
		internal System.Windows.Forms.Label labelSpeed2;
		internal CodeTV.Controls.TrackBarSpeed trackBarSpeed;
	}
}