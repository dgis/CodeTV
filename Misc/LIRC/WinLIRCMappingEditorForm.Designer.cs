namespace CodeTV
{
	partial class WinLIRCMappingEditorForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinLIRCMappingEditorForm));
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.comboBoxApplicationCommand = new System.Windows.Forms.ComboBox();
			this.listViewCommandMapping = new System.Windows.Forms.ListView();
			this.columnHeaderWinLIRCCommand = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderApplicationCommand = new System.Windows.Forms.ColumnHeader();
			this.textBoxWinLIRCCommand = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonAddMapping = new System.Windows.Forms.Button();
			this.buttonRemoveMapping = new System.Windows.Forms.Button();
			this.checkBoxLearn = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(112, 395);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(193, 395);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 0;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// comboBoxApplicationCommand
			// 
			this.comboBoxApplicationCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxApplicationCommand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxApplicationCommand.FormattingEnabled = true;
			this.comboBoxApplicationCommand.Location = new System.Drawing.Point(192, 35);
			this.comboBoxApplicationCommand.Name = "comboBoxApplicationCommand";
			this.comboBoxApplicationCommand.Size = new System.Drawing.Size(177, 21);
			this.comboBoxApplicationCommand.TabIndex = 1;
			// 
			// listViewCommandMapping
			// 
			this.listViewCommandMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listViewCommandMapping.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderWinLIRCCommand,
            this.columnHeaderApplicationCommand});
			this.listViewCommandMapping.FullRowSelect = true;
			this.listViewCommandMapping.GridLines = true;
			this.listViewCommandMapping.Location = new System.Drawing.Point(12, 90);
			this.listViewCommandMapping.Name = "listViewCommandMapping";
			this.listViewCommandMapping.Size = new System.Drawing.Size(357, 299);
			this.listViewCommandMapping.TabIndex = 2;
			this.listViewCommandMapping.UseCompatibleStateImageBehavior = false;
			this.listViewCommandMapping.View = System.Windows.Forms.View.Details;
			this.listViewCommandMapping.SelectedIndexChanged += new System.EventHandler(this.listViewCommandMapping_SelectedIndexChanged);
			// 
			// columnHeaderWinLIRCCommand
			// 
			this.columnHeaderWinLIRCCommand.Text = "WinLIRC Command";
			this.columnHeaderWinLIRCCommand.Width = 172;
			// 
			// columnHeaderApplicationCommand
			// 
			this.columnHeaderApplicationCommand.Text = "ApplicationCommand";
			this.columnHeaderApplicationCommand.Width = 152;
			// 
			// textBoxWinLIRCCommand
			// 
			this.textBoxWinLIRCCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxWinLIRCCommand.Location = new System.Drawing.Point(12, 35);
			this.textBoxWinLIRCCommand.Name = "textBoxWinLIRCCommand";
			this.textBoxWinLIRCCommand.Size = new System.Drawing.Size(174, 20);
			this.textBoxWinLIRCCommand.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "WinLIRC Command";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(190, 19);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(109, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Application Command";
			// 
			// buttonAddMapping
			// 
			this.buttonAddMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAddMapping.Location = new System.Drawing.Point(213, 62);
			this.buttonAddMapping.Name = "buttonAddMapping";
			this.buttonAddMapping.Size = new System.Drawing.Size(75, 23);
			this.buttonAddMapping.TabIndex = 6;
			this.buttonAddMapping.Text = "Add";
			this.buttonAddMapping.UseVisualStyleBackColor = true;
			this.buttonAddMapping.Click += new System.EventHandler(this.buttonAddMapping_Click);
			// 
			// buttonRemoveMapping
			// 
			this.buttonRemoveMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRemoveMapping.Location = new System.Drawing.Point(294, 62);
			this.buttonRemoveMapping.Name = "buttonRemoveMapping";
			this.buttonRemoveMapping.Size = new System.Drawing.Size(75, 23);
			this.buttonRemoveMapping.TabIndex = 7;
			this.buttonRemoveMapping.Text = "Remove";
			this.buttonRemoveMapping.UseVisualStyleBackColor = true;
			this.buttonRemoveMapping.Click += new System.EventHandler(this.buttonRemoveMapping_Click);
			// 
			// checkBoxLearn
			// 
			this.checkBoxLearn.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxLearn.AutoSize = true;
			this.checkBoxLearn.Location = new System.Drawing.Point(13, 62);
			this.checkBoxLearn.Name = "checkBoxLearn";
			this.checkBoxLearn.Size = new System.Drawing.Size(44, 23);
			this.checkBoxLearn.TabIndex = 8;
			this.checkBoxLearn.Text = "Learn";
			this.checkBoxLearn.UseVisualStyleBackColor = true;
			this.checkBoxLearn.CheckedChanged += new System.EventHandler(this.checkBoxLearn_CheckedChanged);
			// 
			// WinLIRCMappingEditorForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(381, 430);
			this.Controls.Add(this.checkBoxLearn);
			this.Controls.Add(this.buttonRemoveMapping);
			this.Controls.Add(this.buttonAddMapping);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxWinLIRCCommand);
			this.Controls.Add(this.listViewCommandMapping);
			this.Controls.Add(this.comboBoxApplicationCommand);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "WinLIRCMappingEditorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "WinLIRC Mapping Edito";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.ComboBox comboBoxApplicationCommand;
		private System.Windows.Forms.ListView listViewCommandMapping;
		private System.Windows.Forms.ColumnHeader columnHeaderWinLIRCCommand;
		private System.Windows.Forms.ColumnHeader columnHeaderApplicationCommand;
		private System.Windows.Forms.TextBox textBoxWinLIRCCommand;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonAddMapping;
		private System.Windows.Forms.Button buttonRemoveMapping;
		private System.Windows.Forms.CheckBox checkBoxLearn;
	}
}