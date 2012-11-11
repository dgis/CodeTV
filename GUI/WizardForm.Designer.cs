namespace CodeTV
{
	partial class WizardForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardForm));
            this.contextMenuStripChannel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.dummyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripChannelFolder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.wizardTunning = new CristiPotlog.Controls.Wizard();
            this.wizardPageChannelNumbering = new CristiPotlog.Controls.WizardPage();
            this.numericUpDownNumberingClickStart = new System.Windows.Forms.NumericUpDown();
            this.checkBoxClickNumbering = new System.Windows.Forms.CheckBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.listViewChannelNumbering = new CodeTV.ListViewEx();
            this.columnHeaderChannel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderChannelNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.wizardPageChannelDestination = new CristiPotlog.Controls.WizardPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBoxFolderDestinationName = new System.Windows.Forms.TextBox();
            this.buttonFolderDestination = new System.Windows.Forms.Button();
            this.wizardPageScanner = new CristiPotlog.Controls.WizardPage();
            this.textBoxScanFrequency = new System.Windows.Forms.TextBox();
            this.textBoxScanStatus = new System.Windows.Forms.TextBox();
            this.labelFrequencyUpdate = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.buttonAddAllToFavorite = new System.Windows.Forms.Button();
            this.buttonScanClear = new System.Windows.Forms.Button();
            this.listViewScanResult = new System.Windows.Forms.ListView();
            this.columnHeaderScanResultName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderScanResultFrequency = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonScanStop = new System.Windows.Forms.Button();
            this.tabControlScanner = new System.Windows.Forms.TabControl();
            this.tabPageScanPredefined = new System.Windows.Forms.TabPage();
            this.labelScanRegion = new System.Windows.Forms.Label();
            this.labelScanCountry = new System.Windows.Forms.Label();
            this.comboBoxScanRegion = new System.Windows.Forms.ComboBox();
            this.comboBoxScanCountry = new System.Windows.Forms.ComboBox();
            this.tabPageScanManual = new System.Windows.Forms.TabPage();
            this.labelScanStopFrequencyKHz = new System.Windows.Forms.Label();
            this.labelScanStopFrequency = new System.Windows.Forms.Label();
            this.textBoxScanStopFrequency = new System.Windows.Forms.TextBox();
            this.labelScanBandwidthKHz = new System.Windows.Forms.Label();
            this.labelScanStartFrequencyKHz = new System.Windows.Forms.Label();
            this.labelScanBandwidth = new System.Windows.Forms.Label();
            this.labelScanStartFrequency = new System.Windows.Forms.Label();
            this.textBoxScanBandwidth = new System.Windows.Forms.TextBox();
            this.textBoxScanStartFrequency = new System.Windows.Forms.TextBox();
            this.buttonScanChannels = new System.Windows.Forms.Button();
            this.wizardPageChannelParameters = new CristiPotlog.Controls.WizardPage();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButtonScanFrequency = new System.Windows.Forms.RadioButton();
            this.radioButtonAddThisChannel = new System.Windows.Forms.RadioButton();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonGetChannel = new System.Windows.Forms.Button();
            this.buttonNewChannel = new System.Windows.Forms.Button();
            this.labelChannelType = new System.Windows.Forms.Label();
            this.comboBoxTunerType = new System.Windows.Forms.ComboBox();
            this.propertyGridChannel = new System.Windows.Forms.PropertyGrid();
            this.wizardPageWelcome = new CristiPotlog.Controls.WizardPage();
            this.wizardPageEnd = new CristiPotlog.Controls.WizardPage();
            this.columnHeaderScanResultNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripChannel.SuspendLayout();
            this.contextMenuStripChannelFolder.SuspendLayout();
            this.wizardTunning.SuspendLayout();
            this.wizardPageChannelNumbering.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberingClickStart)).BeginInit();
            this.wizardPageChannelDestination.SuspendLayout();
            this.wizardPageScanner.SuspendLayout();
            this.tabControlScanner.SuspendLayout();
            this.tabPageScanPredefined.SuspendLayout();
            this.tabPageScanManual.SuspendLayout();
            this.wizardPageChannelParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStripChannel
            // 
            this.contextMenuStripChannel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dummyToolStripMenuItem});
            this.contextMenuStripChannel.Name = "contextMenuStripChannel";
            resources.ApplyResources(this.contextMenuStripChannel, "contextMenuStripChannel");
            this.contextMenuStripChannel.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripChannel_Opening);
            // 
            // dummyToolStripMenuItem
            // 
            this.dummyToolStripMenuItem.Name = "dummyToolStripMenuItem";
            resources.ApplyResources(this.dummyToolStripMenuItem, "dummyToolStripMenuItem");
            // 
            // contextMenuStripChannelFolder
            // 
            this.contextMenuStripChannelFolder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.contextMenuStripChannelFolder.Name = "contextMenuStripChannelFolder";
            resources.ApplyResources(this.contextMenuStripChannelFolder, "contextMenuStripChannelFolder");
            this.contextMenuStripChannelFolder.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripChannelFolder_Opening);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // wizardTunning
            // 
            this.wizardTunning.Controls.Add(this.wizardPageScanner);
            this.wizardTunning.Controls.Add(this.wizardPageChannelParameters);
            this.wizardTunning.Controls.Add(this.wizardPageChannelDestination);
            this.wizardTunning.Controls.Add(this.wizardPageChannelNumbering);
            this.wizardTunning.Controls.Add(this.wizardPageWelcome);
            this.wizardTunning.Controls.Add(this.wizardPageEnd);
            this.wizardTunning.HeaderImage = global::CodeTV.Properties.Resources.CodeTV;
            resources.ApplyResources(this.wizardTunning, "wizardTunning");
            this.wizardTunning.Name = "wizardTunning";
            this.wizardTunning.Pages.AddRange(new CristiPotlog.Controls.WizardPage[] {
            this.wizardPageWelcome,
            this.wizardPageChannelParameters,
            this.wizardPageScanner,
            this.wizardPageChannelDestination,
            this.wizardPageChannelNumbering,
            this.wizardPageEnd});
            this.wizardTunning.WelcomeImage = global::CodeTV.Properties.Resources.CodeTV2;
            this.wizardTunning.BeforeSwitchPages += new CristiPotlog.Controls.Wizard.BeforeSwitchPagesEventHandler(this.wizardTunning_BeforeSwitchPages);
            this.wizardTunning.AfterSwitchPages += new CristiPotlog.Controls.Wizard.AfterSwitchPagesEventHandler(this.wizardTunning_AfterSwitchPages);
            // 
            // wizardPageChannelNumbering
            // 
            this.wizardPageChannelNumbering.Controls.Add(this.numericUpDownNumberingClickStart);
            this.wizardPageChannelNumbering.Controls.Add(this.checkBoxClickNumbering);
            this.wizardPageChannelNumbering.Controls.Add(this.textBox7);
            this.wizardPageChannelNumbering.Controls.Add(this.listViewChannelNumbering);
            this.wizardPageChannelNumbering.Description = "Associate the channel with a number";
            resources.ApplyResources(this.wizardPageChannelNumbering, "wizardPageChannelNumbering");
            this.wizardPageChannelNumbering.Name = "wizardPageChannelNumbering";
            this.wizardPageChannelNumbering.Title = "Channel Numbering";
            // 
            // numericUpDownNumberingClickStart
            // 
            resources.ApplyResources(this.numericUpDownNumberingClickStart, "numericUpDownNumberingClickStart");
            this.numericUpDownNumberingClickStart.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.numericUpDownNumberingClickStart.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDownNumberingClickStart.Name = "numericUpDownNumberingClickStart";
            // 
            // checkBoxClickNumbering
            // 
            resources.ApplyResources(this.checkBoxClickNumbering, "checkBoxClickNumbering");
            this.checkBoxClickNumbering.Name = "checkBoxClickNumbering";
            this.checkBoxClickNumbering.UseVisualStyleBackColor = true;
            // 
            // textBox7
            // 
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox7.CausesValidation = false;
            resources.ApplyResources(this.textBox7, "textBox7");
            this.textBox7.Name = "textBox7";
            this.textBox7.ReadOnly = true;
            this.textBox7.TabStop = false;
            // 
            // listViewChannelNumbering
            // 
            resources.ApplyResources(this.listViewChannelNumbering, "listViewChannelNumbering");
            this.listViewChannelNumbering.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderChannel,
            this.columnHeaderChannelNumber});
            this.listViewChannelNumbering.FullRowSelect = true;
            this.listViewChannelNumbering.GridLines = true;
            this.listViewChannelNumbering.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewChannelNumbering.MultiSelect = false;
            this.listViewChannelNumbering.Name = "listViewChannelNumbering";
            this.listViewChannelNumbering.UseCompatibleStateImageBehavior = false;
            this.listViewChannelNumbering.View = System.Windows.Forms.View.Details;
            this.listViewChannelNumbering.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewChannelNumbering_DrawColumnHeader);
            this.listViewChannelNumbering.Click += new System.EventHandler(this.listViewChannelNumbering_Click);
            this.listViewChannelNumbering.DoubleClick += new System.EventHandler(this.listViewChannelNumbering_DoubleClick);
            // 
            // columnHeaderChannel
            // 
            resources.ApplyResources(this.columnHeaderChannel, "columnHeaderChannel");
            // 
            // columnHeaderChannelNumber
            // 
            resources.ApplyResources(this.columnHeaderChannelNumber, "columnHeaderChannelNumber");
            // 
            // wizardPageChannelDestination
            // 
            this.wizardPageChannelDestination.Controls.Add(this.textBox1);
            this.wizardPageChannelDestination.Controls.Add(this.textBoxFolderDestinationName);
            this.wizardPageChannelDestination.Controls.Add(this.buttonFolderDestination);
            this.wizardPageChannelDestination.Description = "Selection of a destination for the newly created channel";
            resources.ApplyResources(this.wizardPageChannelDestination, "wizardPageChannelDestination");
            this.wizardPageChannelDestination.Name = "wizardPageChannelDestination";
            this.wizardPageChannelDestination.Title = "Channel Destination";
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.CausesValidation = false;
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.TabStop = false;
            // 
            // textBoxFolderDestinationName
            // 
            resources.ApplyResources(this.textBoxFolderDestinationName, "textBoxFolderDestinationName");
            this.textBoxFolderDestinationName.Name = "textBoxFolderDestinationName";
            this.textBoxFolderDestinationName.ReadOnly = true;
            this.textBoxFolderDestinationName.TextChanged += new System.EventHandler(this.textBoxFolderDestinationName_TextChanged);
            // 
            // buttonFolderDestination
            // 
            resources.ApplyResources(this.buttonFolderDestination, "buttonFolderDestination");
            this.buttonFolderDestination.Name = "buttonFolderDestination";
            this.buttonFolderDestination.UseVisualStyleBackColor = true;
            this.buttonFolderDestination.Click += new System.EventHandler(this.buttonFolderDestination_Click);
            // 
            // wizardPageScanner
            // 
            this.wizardPageScanner.Controls.Add(this.textBoxScanFrequency);
            this.wizardPageScanner.Controls.Add(this.textBoxScanStatus);
            this.wizardPageScanner.Controls.Add(this.labelFrequencyUpdate);
            this.wizardPageScanner.Controls.Add(this.textBox5);
            this.wizardPageScanner.Controls.Add(this.textBox6);
            this.wizardPageScanner.Controls.Add(this.buttonAddAllToFavorite);
            this.wizardPageScanner.Controls.Add(this.buttonScanClear);
            this.wizardPageScanner.Controls.Add(this.listViewScanResult);
            this.wizardPageScanner.Controls.Add(this.buttonScanStop);
            this.wizardPageScanner.Controls.Add(this.tabControlScanner);
            this.wizardPageScanner.Controls.Add(this.buttonScanChannels);
            this.wizardPageScanner.Description = "Scan available channel among a frequency range or list";
            resources.ApplyResources(this.wizardPageScanner, "wizardPageScanner");
            this.wizardPageScanner.Name = "wizardPageScanner";
            this.wizardPageScanner.Title = "Scanner";
            // 
            // textBoxScanFrequency
            // 
            this.textBoxScanFrequency.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxScanFrequency.CausesValidation = false;
            resources.ApplyResources(this.textBoxScanFrequency, "textBoxScanFrequency");
            this.textBoxScanFrequency.Name = "textBoxScanFrequency";
            this.textBoxScanFrequency.ReadOnly = true;
            this.textBoxScanFrequency.TabStop = false;
            // 
            // textBoxScanStatus
            // 
            this.textBoxScanStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxScanStatus.CausesValidation = false;
            resources.ApplyResources(this.textBoxScanStatus, "textBoxScanStatus");
            this.textBoxScanStatus.Name = "textBoxScanStatus";
            this.textBoxScanStatus.ReadOnly = true;
            this.textBoxScanStatus.TabStop = false;
            // 
            // labelFrequencyUpdate
            // 
            resources.ApplyResources(this.labelFrequencyUpdate, "labelFrequencyUpdate");
            this.labelFrequencyUpdate.Name = "labelFrequencyUpdate";
            // 
            // textBox5
            // 
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox5.CausesValidation = false;
            resources.ApplyResources(this.textBox5, "textBox5");
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.TabStop = false;
            // 
            // textBox6
            // 
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox6.CausesValidation = false;
            resources.ApplyResources(this.textBox6, "textBox6");
            this.textBox6.Name = "textBox6";
            this.textBox6.ReadOnly = true;
            this.textBox6.TabStop = false;
            // 
            // buttonAddAllToFavorite
            // 
            resources.ApplyResources(this.buttonAddAllToFavorite, "buttonAddAllToFavorite");
            this.buttonAddAllToFavorite.Name = "buttonAddAllToFavorite";
            this.buttonAddAllToFavorite.UseVisualStyleBackColor = true;
            this.buttonAddAllToFavorite.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonScanClear
            // 
            resources.ApplyResources(this.buttonScanClear, "buttonScanClear");
            this.buttonScanClear.Name = "buttonScanClear";
            this.buttonScanClear.UseVisualStyleBackColor = true;
            this.buttonScanClear.Click += new System.EventHandler(this.buttonScanClear_Click);
            // 
            // listViewScanResult
            // 
            resources.ApplyResources(this.listViewScanResult, "listViewScanResult");
            this.listViewScanResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderScanResultName,
            this.columnHeaderScanResultFrequency,
            this.columnHeaderScanResultNumber});
            this.listViewScanResult.FullRowSelect = true;
            this.listViewScanResult.GridLines = true;
            this.listViewScanResult.HideSelection = false;
            this.listViewScanResult.Name = "listViewScanResult";
            this.listViewScanResult.UseCompatibleStateImageBehavior = false;
            this.listViewScanResult.View = System.Windows.Forms.View.Details;
            this.listViewScanResult.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewScanResult_ItemSelectionChanged);
            // 
            // columnHeaderScanResultName
            // 
            resources.ApplyResources(this.columnHeaderScanResultName, "columnHeaderScanResultName");
            // 
            // columnHeaderScanResultFrequency
            // 
            resources.ApplyResources(this.columnHeaderScanResultFrequency, "columnHeaderScanResultFrequency");
            // 
            // buttonScanStop
            // 
            resources.ApplyResources(this.buttonScanStop, "buttonScanStop");
            this.buttonScanStop.Name = "buttonScanStop";
            this.buttonScanStop.UseVisualStyleBackColor = true;
            this.buttonScanStop.Click += new System.EventHandler(this.buttonScanStop_Click);
            // 
            // tabControlScanner
            // 
            this.tabControlScanner.Controls.Add(this.tabPageScanPredefined);
            this.tabControlScanner.Controls.Add(this.tabPageScanManual);
            resources.ApplyResources(this.tabControlScanner, "tabControlScanner");
            this.tabControlScanner.Name = "tabControlScanner";
            this.tabControlScanner.SelectedIndex = 0;
            // 
            // tabPageScanPredefined
            // 
            this.tabPageScanPredefined.Controls.Add(this.labelScanRegion);
            this.tabPageScanPredefined.Controls.Add(this.labelScanCountry);
            this.tabPageScanPredefined.Controls.Add(this.comboBoxScanRegion);
            this.tabPageScanPredefined.Controls.Add(this.comboBoxScanCountry);
            resources.ApplyResources(this.tabPageScanPredefined, "tabPageScanPredefined");
            this.tabPageScanPredefined.Name = "tabPageScanPredefined";
            this.tabPageScanPredefined.UseVisualStyleBackColor = true;
            // 
            // labelScanRegion
            // 
            resources.ApplyResources(this.labelScanRegion, "labelScanRegion");
            this.labelScanRegion.Name = "labelScanRegion";
            // 
            // labelScanCountry
            // 
            resources.ApplyResources(this.labelScanCountry, "labelScanCountry");
            this.labelScanCountry.Name = "labelScanCountry";
            // 
            // comboBoxScanRegion
            // 
            this.comboBoxScanRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxScanRegion.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxScanRegion, "comboBoxScanRegion");
            this.comboBoxScanRegion.Name = "comboBoxScanRegion";
            this.comboBoxScanRegion.Sorted = true;
            // 
            // comboBoxScanCountry
            // 
            this.comboBoxScanCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxScanCountry.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxScanCountry, "comboBoxScanCountry");
            this.comboBoxScanCountry.Name = "comboBoxScanCountry";
            this.comboBoxScanCountry.Sorted = true;
            this.comboBoxScanCountry.SelectedIndexChanged += new System.EventHandler(this.comboBoxScanCountry_SelectedIndexChanged);
            // 
            // tabPageScanManual
            // 
            this.tabPageScanManual.Controls.Add(this.labelScanStopFrequencyKHz);
            this.tabPageScanManual.Controls.Add(this.labelScanStopFrequency);
            this.tabPageScanManual.Controls.Add(this.textBoxScanStopFrequency);
            this.tabPageScanManual.Controls.Add(this.labelScanBandwidthKHz);
            this.tabPageScanManual.Controls.Add(this.labelScanStartFrequencyKHz);
            this.tabPageScanManual.Controls.Add(this.labelScanBandwidth);
            this.tabPageScanManual.Controls.Add(this.labelScanStartFrequency);
            this.tabPageScanManual.Controls.Add(this.textBoxScanBandwidth);
            this.tabPageScanManual.Controls.Add(this.textBoxScanStartFrequency);
            resources.ApplyResources(this.tabPageScanManual, "tabPageScanManual");
            this.tabPageScanManual.Name = "tabPageScanManual";
            this.tabPageScanManual.UseVisualStyleBackColor = true;
            // 
            // labelScanStopFrequencyKHz
            // 
            resources.ApplyResources(this.labelScanStopFrequencyKHz, "labelScanStopFrequencyKHz");
            this.labelScanStopFrequencyKHz.Name = "labelScanStopFrequencyKHz";
            // 
            // labelScanStopFrequency
            // 
            resources.ApplyResources(this.labelScanStopFrequency, "labelScanStopFrequency");
            this.labelScanStopFrequency.Name = "labelScanStopFrequency";
            // 
            // textBoxScanStopFrequency
            // 
            resources.ApplyResources(this.textBoxScanStopFrequency, "textBoxScanStopFrequency");
            this.textBoxScanStopFrequency.Name = "textBoxScanStopFrequency";
            // 
            // labelScanBandwidthKHz
            // 
            resources.ApplyResources(this.labelScanBandwidthKHz, "labelScanBandwidthKHz");
            this.labelScanBandwidthKHz.Name = "labelScanBandwidthKHz";
            // 
            // labelScanStartFrequencyKHz
            // 
            resources.ApplyResources(this.labelScanStartFrequencyKHz, "labelScanStartFrequencyKHz");
            this.labelScanStartFrequencyKHz.Name = "labelScanStartFrequencyKHz";
            // 
            // labelScanBandwidth
            // 
            resources.ApplyResources(this.labelScanBandwidth, "labelScanBandwidth");
            this.labelScanBandwidth.Name = "labelScanBandwidth";
            // 
            // labelScanStartFrequency
            // 
            resources.ApplyResources(this.labelScanStartFrequency, "labelScanStartFrequency");
            this.labelScanStartFrequency.Name = "labelScanStartFrequency";
            // 
            // textBoxScanBandwidth
            // 
            resources.ApplyResources(this.textBoxScanBandwidth, "textBoxScanBandwidth");
            this.textBoxScanBandwidth.Name = "textBoxScanBandwidth";
            // 
            // textBoxScanStartFrequency
            // 
            resources.ApplyResources(this.textBoxScanStartFrequency, "textBoxScanStartFrequency");
            this.textBoxScanStartFrequency.Name = "textBoxScanStartFrequency";
            // 
            // buttonScanChannels
            // 
            resources.ApplyResources(this.buttonScanChannels, "buttonScanChannels");
            this.buttonScanChannels.Name = "buttonScanChannels";
            this.buttonScanChannels.UseVisualStyleBackColor = true;
            this.buttonScanChannels.Click += new System.EventHandler(this.buttonScanChannels_Click);
            // 
            // wizardPageChannelParameters
            // 
            this.wizardPageChannelParameters.Controls.Add(this.textBox4);
            this.wizardPageChannelParameters.Controls.Add(this.textBox3);
            this.wizardPageChannelParameters.Controls.Add(this.label2);
            this.wizardPageChannelParameters.Controls.Add(this.radioButtonScanFrequency);
            this.wizardPageChannelParameters.Controls.Add(this.radioButtonAddThisChannel);
            this.wizardPageChannelParameters.Controls.Add(this.textBox2);
            this.wizardPageChannelParameters.Controls.Add(this.label1);
            this.wizardPageChannelParameters.Controls.Add(this.buttonGetChannel);
            this.wizardPageChannelParameters.Controls.Add(this.buttonNewChannel);
            this.wizardPageChannelParameters.Controls.Add(this.labelChannelType);
            this.wizardPageChannelParameters.Controls.Add(this.comboBoxTunerType);
            this.wizardPageChannelParameters.Controls.Add(this.propertyGridChannel);
            this.wizardPageChannelParameters.Description = "Choose the common channels parameters (devices, renderers, etc...)";
            resources.ApplyResources(this.wizardPageChannelParameters, "wizardPageChannelParameters");
            this.wizardPageChannelParameters.Name = "wizardPageChannelParameters";
            this.wizardPageChannelParameters.Title = "Channels Parameters";
            // 
            // textBox4
            // 
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.CausesValidation = false;
            resources.ApplyResources(this.textBox4, "textBox4");
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.TabStop = false;
            // 
            // textBox3
            // 
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.CausesValidation = false;
            resources.ApplyResources(this.textBox3, "textBox3");
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.TabStop = false;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // radioButtonScanFrequency
            // 
            resources.ApplyResources(this.radioButtonScanFrequency, "radioButtonScanFrequency");
            this.radioButtonScanFrequency.Name = "radioButtonScanFrequency";
            this.radioButtonScanFrequency.TabStop = true;
            this.radioButtonScanFrequency.UseVisualStyleBackColor = true;
            this.radioButtonScanFrequency.CheckedChanged += new System.EventHandler(this.propertyGridChannel_SelectedObjectsChanged);
            // 
            // radioButtonAddThisChannel
            // 
            resources.ApplyResources(this.radioButtonAddThisChannel, "radioButtonAddThisChannel");
            this.radioButtonAddThisChannel.Name = "radioButtonAddThisChannel";
            this.radioButtonAddThisChannel.TabStop = true;
            this.radioButtonAddThisChannel.UseVisualStyleBackColor = true;
            this.radioButtonAddThisChannel.CheckedChanged += new System.EventHandler(this.propertyGridChannel_SelectedObjectsChanged);
            // 
            // textBox2
            // 
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.CausesValidation = false;
            resources.ApplyResources(this.textBox2, "textBox2");
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // buttonGetChannel
            // 
            resources.ApplyResources(this.buttonGetChannel, "buttonGetChannel");
            this.buttonGetChannel.Name = "buttonGetChannel";
            this.buttonGetChannel.UseVisualStyleBackColor = true;
            this.buttonGetChannel.Click += new System.EventHandler(this.buttonGetChannel_Click);
            // 
            // buttonNewChannel
            // 
            resources.ApplyResources(this.buttonNewChannel, "buttonNewChannel");
            this.buttonNewChannel.Name = "buttonNewChannel";
            this.buttonNewChannel.UseVisualStyleBackColor = true;
            this.buttonNewChannel.Click += new System.EventHandler(this.buttonNewChannel_Click);
            // 
            // labelChannelType
            // 
            resources.ApplyResources(this.labelChannelType, "labelChannelType");
            this.labelChannelType.Name = "labelChannelType";
            // 
            // comboBoxTunerType
            // 
            this.comboBoxTunerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTunerType.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxTunerType, "comboBoxTunerType");
            this.comboBoxTunerType.Name = "comboBoxTunerType";
            // 
            // propertyGridChannel
            // 
            resources.ApplyResources(this.propertyGridChannel, "propertyGridChannel");
            this.propertyGridChannel.Name = "propertyGridChannel";
            this.propertyGridChannel.ToolbarVisible = false;
            this.propertyGridChannel.SelectedObjectsChanged += new System.EventHandler(this.propertyGridChannel_SelectedObjectsChanged);
            // 
            // wizardPageWelcome
            // 
            this.wizardPageWelcome.Description = "This wizard will help you to create the channel";
            resources.ApplyResources(this.wizardPageWelcome, "wizardPageWelcome");
            this.wizardPageWelcome.Name = "wizardPageWelcome";
            this.wizardPageWelcome.Style = CristiPotlog.Controls.WizardPageStyle.Welcome;
            this.wizardPageWelcome.Title = "Channel Tunning Wizard";
            // 
            // wizardPageEnd
            // 
            this.wizardPageEnd.Description = "Your channels are now configured!";
            resources.ApplyResources(this.wizardPageEnd, "wizardPageEnd");
            this.wizardPageEnd.Name = "wizardPageEnd";
            this.wizardPageEnd.Style = CristiPotlog.Controls.WizardPageStyle.Finish;
            this.wizardPageEnd.Title = "The end";
            // 
            // columnHeaderScanResultNumber
            // 
            resources.ApplyResources(this.columnHeaderScanResultNumber, "columnHeaderScanResultNumber");
            // 
            // WizardForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.wizardTunning);
            this.Name = "WizardForm";
            this.contextMenuStripChannel.ResumeLayout(false);
            this.contextMenuStripChannelFolder.ResumeLayout(false);
            this.wizardTunning.ResumeLayout(false);
            this.wizardPageChannelNumbering.ResumeLayout(false);
            this.wizardPageChannelNumbering.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberingClickStart)).EndInit();
            this.wizardPageChannelDestination.ResumeLayout(false);
            this.wizardPageChannelDestination.PerformLayout();
            this.wizardPageScanner.ResumeLayout(false);
            this.wizardPageScanner.PerformLayout();
            this.tabControlScanner.ResumeLayout(false);
            this.tabPageScanPredefined.ResumeLayout(false);
            this.tabPageScanPredefined.PerformLayout();
            this.tabPageScanManual.ResumeLayout(false);
            this.tabPageScanManual.PerformLayout();
            this.wizardPageChannelParameters.ResumeLayout(false);
            this.wizardPageChannelParameters.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private CristiPotlog.Controls.Wizard wizardTunning;
		private CristiPotlog.Controls.WizardPage wizardPageWelcome;
		private CristiPotlog.Controls.WizardPage wizardPageChannelParameters;
		private CristiPotlog.Controls.WizardPage wizardPageScanner;
		private CristiPotlog.Controls.WizardPage wizardPageEnd;
		private System.Windows.Forms.Button buttonNewChannel;
		private System.Windows.Forms.Label labelChannelType;
		internal System.Windows.Forms.ComboBox comboBoxTunerType;
		internal System.Windows.Forms.PropertyGrid propertyGridChannel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonAddAllToFavorite;
		private System.Windows.Forms.Button buttonScanClear;
		private System.Windows.Forms.ListView listViewScanResult;
		private System.Windows.Forms.ColumnHeader columnHeaderScanResultName;
		private System.Windows.Forms.ColumnHeader columnHeaderScanResultFrequency;
		private System.Windows.Forms.Button buttonScanStop;
		private System.Windows.Forms.TabControl tabControlScanner;
		private System.Windows.Forms.TabPage tabPageScanPredefined;
		private System.Windows.Forms.Label labelScanRegion;
		private System.Windows.Forms.Label labelScanCountry;
		private System.Windows.Forms.ComboBox comboBoxScanRegion;
		internal System.Windows.Forms.ComboBox comboBoxScanCountry;
		private System.Windows.Forms.TabPage tabPageScanManual;
		private System.Windows.Forms.Label labelScanStopFrequencyKHz;
		private System.Windows.Forms.Label labelScanStopFrequency;
		private System.Windows.Forms.TextBox textBoxScanStopFrequency;
		private System.Windows.Forms.Label labelScanBandwidthKHz;
		private System.Windows.Forms.Label labelScanStartFrequencyKHz;
		private System.Windows.Forms.Label labelScanBandwidth;
		private System.Windows.Forms.Label labelScanStartFrequency;
		private System.Windows.Forms.TextBox textBoxScanBandwidth;
		private System.Windows.Forms.TextBox textBoxScanStartFrequency;
		private System.Windows.Forms.Button buttonScanChannels;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.RadioButton radioButtonScanFrequency;
		private System.Windows.Forms.RadioButton radioButtonAddThisChannel;
		private System.Windows.Forms.Button buttonGetChannel;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripChannel;
		private System.Windows.Forms.ToolStripMenuItem dummyToolStripMenuItem;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox3;
		private CristiPotlog.Controls.WizardPage wizardPageChannelDestination;
		private System.Windows.Forms.Button buttonFolderDestination;
		private System.Windows.Forms.TextBox textBoxFolderDestinationName;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripChannelFolder;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.TextBox textBox4;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox5;
		private System.Windows.Forms.TextBox textBox6;
		private CristiPotlog.Controls.WizardPage wizardPageChannelNumbering;
		private System.Windows.Forms.Label labelFrequencyUpdate;
		private System.Windows.Forms.TextBox textBoxScanFrequency;
		private System.Windows.Forms.TextBox textBoxScanStatus;
		private System.Windows.Forms.TextBox textBox7;
		private System.Windows.Forms.ColumnHeader columnHeaderChannel;
		private System.Windows.Forms.ColumnHeader columnHeaderChannelNumber;
		private ListViewEx listViewChannelNumbering;
		private System.Windows.Forms.CheckBox checkBoxClickNumbering;
		private System.Windows.Forms.NumericUpDown numericUpDownNumberingClickStart;
        private System.Windows.Forms.ColumnHeader columnHeaderScanResultNumber;
	}
}