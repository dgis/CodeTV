namespace CodeTV
{
	partial class FrequencyEditorForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrequencyEditorForm));
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.comboBoxCountry = new System.Windows.Forms.ComboBox();
			this.comboBoxRegion = new System.Windows.Forms.ComboBox();
			this.listBoxFrequencies = new System.Windows.Forms.ListBox();
			this.labelCountry = new System.Windows.Forms.Label();
			this.labelRegion = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.buttonOK, "buttonOK");
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.buttonCancel, "buttonCancel");
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// comboBoxCountry
			// 
			this.comboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCountry.FormattingEnabled = true;
			resources.ApplyResources(this.comboBoxCountry, "comboBoxCountry");
			this.comboBoxCountry.Name = "comboBoxCountry";
			this.comboBoxCountry.Sorted = true;
			this.comboBoxCountry.SelectedIndexChanged += new System.EventHandler(this.comboBoxCountry_SelectedIndexChanged);
			// 
			// comboBoxRegion
			// 
			this.comboBoxRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxRegion.FormattingEnabled = true;
			resources.ApplyResources(this.comboBoxRegion, "comboBoxRegion");
			this.comboBoxRegion.Name = "comboBoxRegion";
			this.comboBoxRegion.Sorted = true;
			this.comboBoxRegion.SelectedIndexChanged += new System.EventHandler(this.comboBoxRegions_SelectedIndexChanged);
			// 
			// listBoxFrequencies
			// 
			this.listBoxFrequencies.FormattingEnabled = true;
			resources.ApplyResources(this.listBoxFrequencies, "listBoxFrequencies");
			this.listBoxFrequencies.Name = "listBoxFrequencies";
			// 
			// labelCountry
			// 
			resources.ApplyResources(this.labelCountry, "labelCountry");
			this.labelCountry.Name = "labelCountry";
			// 
			// labelRegion
			// 
			resources.ApplyResources(this.labelRegion, "labelRegion");
			this.labelRegion.Name = "labelRegion";
			// 
			// FrequencyEditorForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.labelRegion);
			this.Controls.Add(this.labelCountry);
			this.Controls.Add(this.listBoxFrequencies);
			this.Controls.Add(this.comboBoxRegion);
			this.Controls.Add(this.comboBoxCountry);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Name = "FrequencyEditorForm";
			this.Load += new System.EventHandler(this.FrequencyForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.ComboBox comboBoxCountry;
		private System.Windows.Forms.ComboBox comboBoxRegion;
		private System.Windows.Forms.ListBox listBoxFrequencies;
		private System.Windows.Forms.Label labelCountry;
		private System.Windows.Forms.Label labelRegion;
	}
}