namespace CodeTV
{
    partial class MainForm
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

				ClearGraph();
				//if (this.currentGraphBuilder != null)
				//{
				//    this.currentGraphBuilder.Dispose();
				//    this.currentGraphBuilder = null;
				//}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.toolStripContainerMain = new System.Windows.Forms.ToolStripContainer();
			this.statusStripMain = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabelVideoStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelChannelName = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelEPG = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBarSignalStrength = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripStatusLabelSignalStrength = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBarSignalQuality = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripStatusLabelSignalQuality = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelTimeLinePosition = new System.Windows.Forms.ToolStripStatusLabel();
			this.panelVideoPlace = new System.Windows.Forms.Panel();
			this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
			this.toolStripMain = new System.Windows.Forms.ToolStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.channelWizardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripPanelVideo = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.channelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.titi1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.videoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.freeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.touchWindowFromInsideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.touchWindowFromOutsideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.strechToWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.zoomInToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoomOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom50ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom100ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom200ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
			this.moveLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.moveRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.moveCenterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.aspectRatioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.decreaseAspectRatioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.resetAspectRatioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aspecRatio169toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.graphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pauseGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stopGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.releaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.filtersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toto1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.channelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.channelInfosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mediaTuningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.timeLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.channelPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonNextChannel = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonPreviousChannel = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonPreviousChannelInFolder = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonNextChannelInFolder = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonVideoModeTV = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonVideoModeFullscreen = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonSnapshot = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonTimeShifting = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonPlay = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonPause = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRewind = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonFastForward = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRecord = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonTest = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonTest2 = new System.Windows.Forms.ToolStripButton();
			this.imageListLogoTV = new System.Windows.Forms.ImageList(this.components);
			this.timerSignalUpdate = new System.Windows.Forms.Timer(this.components);
			this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
			this.timerChannelNumberBuilder = new System.Windows.Forms.Timer(this.components);
			this.timerVideoRefresh = new System.Windows.Forms.Timer(this.components);
			this.toolStripContainerMain.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainerMain.ContentPanel.SuspendLayout();
			this.toolStripContainerMain.TopToolStripPanel.SuspendLayout();
			this.toolStripContainerMain.SuspendLayout();
			this.statusStripMain.SuspendLayout();
			this.panelVideoPlace.SuspendLayout();
			this.toolStripMain.SuspendLayout();
			this.contextMenuStripPanelVideo.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainerMain
			// 
			// 
			// toolStripContainerMain.BottomToolStripPanel
			// 
			this.toolStripContainerMain.BottomToolStripPanel.Controls.Add(this.statusStripMain);
			// 
			// toolStripContainerMain.ContentPanel
			// 
			this.toolStripContainerMain.ContentPanel.BackColor = System.Drawing.Color.Transparent;
			this.toolStripContainerMain.ContentPanel.Controls.Add(this.panelVideoPlace);
			resources.ApplyResources(this.toolStripContainerMain.ContentPanel, "toolStripContainerMain.ContentPanel");
			resources.ApplyResources(this.toolStripContainerMain, "toolStripContainerMain");
			this.toolStripContainerMain.Name = "toolStripContainerMain";
			// 
			// toolStripContainerMain.TopToolStripPanel
			// 
			this.toolStripContainerMain.TopToolStripPanel.Controls.Add(this.toolStripMain);
			// 
			// statusStripMain
			// 
			resources.ApplyResources(this.statusStripMain, "statusStripMain");
			this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelVideoStatus,
            this.toolStripStatusLabelChannelName,
            this.toolStripStatusLabelEPG,
            this.toolStripProgressBarSignalStrength,
            this.toolStripStatusLabelSignalStrength,
            this.toolStripProgressBarSignalQuality,
            this.toolStripStatusLabelSignalQuality,
            this.toolStripStatusLabelTimeLinePosition});
			this.statusStripMain.Name = "statusStripMain";
			this.statusStripMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			// 
			// toolStripStatusLabelVideoStatus
			// 
			this.toolStripStatusLabelVideoStatus.Name = "toolStripStatusLabelVideoStatus";
			resources.ApplyResources(this.toolStripStatusLabelVideoStatus, "toolStripStatusLabelVideoStatus");
			// 
			// toolStripStatusLabelChannelName
			// 
			this.toolStripStatusLabelChannelName.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
						| System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
						| System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.toolStripStatusLabelChannelName.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
			this.toolStripStatusLabelChannelName.Margin = new System.Windows.Forms.Padding(5, 3, 5, 2);
			this.toolStripStatusLabelChannelName.Name = "toolStripStatusLabelChannelName";
			resources.ApplyResources(this.toolStripStatusLabelChannelName, "toolStripStatusLabelChannelName");
			// 
			// toolStripStatusLabelEPG
			// 
			this.toolStripStatusLabelEPG.Margin = new System.Windows.Forms.Padding(5, 3, 5, 2);
			this.toolStripStatusLabelEPG.Name = "toolStripStatusLabelEPG";
			this.toolStripStatusLabelEPG.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
			resources.ApplyResources(this.toolStripStatusLabelEPG, "toolStripStatusLabelEPG");
			this.toolStripStatusLabelEPG.Spring = true;
			// 
			// toolStripProgressBarSignalStrength
			// 
			this.toolStripProgressBarSignalStrength.Name = "toolStripProgressBarSignalStrength";
			resources.ApplyResources(this.toolStripProgressBarSignalStrength, "toolStripProgressBarSignalStrength");
			// 
			// toolStripStatusLabelSignalStrength
			// 
			this.toolStripStatusLabelSignalStrength.Name = "toolStripStatusLabelSignalStrength";
			resources.ApplyResources(this.toolStripStatusLabelSignalStrength, "toolStripStatusLabelSignalStrength");
			// 
			// toolStripProgressBarSignalQuality
			// 
			this.toolStripProgressBarSignalQuality.Name = "toolStripProgressBarSignalQuality";
			resources.ApplyResources(this.toolStripProgressBarSignalQuality, "toolStripProgressBarSignalQuality");
			// 
			// toolStripStatusLabelSignalQuality
			// 
			this.toolStripStatusLabelSignalQuality.Name = "toolStripStatusLabelSignalQuality";
			resources.ApplyResources(this.toolStripStatusLabelSignalQuality, "toolStripStatusLabelSignalQuality");
			// 
			// toolStripStatusLabelTimeLinePosition
			// 
			this.toolStripStatusLabelTimeLinePosition.Name = "toolStripStatusLabelTimeLinePosition";
			resources.ApplyResources(this.toolStripStatusLabelTimeLinePosition, "toolStripStatusLabelTimeLinePosition");
			// 
			// panelVideoPlace
			// 
			this.panelVideoPlace.Controls.Add(this.dockPanel);
			resources.ApplyResources(this.panelVideoPlace, "panelVideoPlace");
			this.panelVideoPlace.Name = "panelVideoPlace";
			// 
			// dockPanel
			// 
			this.dockPanel.ActiveAutoHideContent = null;
			resources.ApplyResources(this.dockPanel, "dockPanel");
			this.dockPanel.DockBottomPortion = 200;
			this.dockPanel.DockLeftPortion = 200;
			this.dockPanel.DockRightPortion = 200;
			this.dockPanel.DockTopPortion = 200;
			this.dockPanel.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingSdi;
			this.dockPanel.Name = "dockPanel";
			// 
			// toolStripMain
			// 
			this.toolStripMain.AllowItemReorder = true;
			resources.ApplyResources(this.toolStripMain, "toolStripMain");
			this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.toolStripMenuItemAbout,
            this.toolStripSeparator5,
            this.toolStripButtonNextChannel,
            this.toolStripButtonPreviousChannel,
            this.toolStripButtonPreviousChannelInFolder,
            this.toolStripButtonNextChannelInFolder,
            this.toolStripSeparator1,
            this.toolStripButtonVideoModeTV,
            this.toolStripButtonVideoModeFullscreen,
            this.toolStripSeparator4,
            this.toolStripButtonSnapshot,
            this.toolStripButtonTimeShifting,
            this.toolStripButtonPlay,
            this.toolStripButtonPause,
            this.toolStripButtonRewind,
            this.toolStripButtonFastForward,
            this.toolStripButtonStop,
            this.toolStripButtonRecord,
            this.toolStripSeparator3,
            this.toolStripButtonHelp,
            this.toolStripSeparator2,
            this.toolStripButtonTest,
            this.toolStripButtonTest2});
			this.toolStripMain.Name = "toolStripMain";
			this.toolStripMain.Stretch = true;
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openVideoToolStripMenuItem,
            this.channelWizardToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
			// 
			// openVideoToolStripMenuItem
			// 
			this.openVideoToolStripMenuItem.Image = global::CodeTV.Properties.Resources.Open;
			this.openVideoToolStripMenuItem.Name = "openVideoToolStripMenuItem";
			resources.ApplyResources(this.openVideoToolStripMenuItem, "openVideoToolStripMenuItem");
			this.openVideoToolStripMenuItem.Click += new System.EventHandler(this.openVideoToolStripMenuItem_Click);
			// 
			// channelWizardToolStripMenuItem
			// 
			this.channelWizardToolStripMenuItem.Image = global::CodeTV.Properties.Resources.wand;
			this.channelWizardToolStripMenuItem.Name = "channelWizardToolStripMenuItem";
			resources.ApplyResources(this.channelWizardToolStripMenuItem, "channelWizardToolStripMenuItem");
			this.channelWizardToolStripMenuItem.Click += new System.EventHandler(this.channelWizardToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
			// 
			// contextMenuStripPanelVideo
			// 
			this.contextMenuStripPanelVideo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.channelsToolStripMenuItem,
            this.videoToolStripMenuItem,
            this.graphToolStripMenuItem,
            this.filtersToolStripMenuItem});
			this.contextMenuStripPanelVideo.Name = "contextMenuStripPanelVideo";
			resources.ApplyResources(this.contextMenuStripPanelVideo, "contextMenuStripPanelVideo");
			this.contextMenuStripPanelVideo.Opened += new System.EventHandler(this.contextMenuStripPanelVideo_Opened);
			this.contextMenuStripPanelVideo.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuStripPanelVideo_Closed);
			this.contextMenuStripPanelVideo.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripPanelVideo_Opening);
			// 
			// channelsToolStripMenuItem
			// 
			this.channelsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.titi1ToolStripMenuItem});
			this.channelsToolStripMenuItem.Name = "channelsToolStripMenuItem";
			resources.ApplyResources(this.channelsToolStripMenuItem, "channelsToolStripMenuItem");
			this.channelsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.channelsToolStripMenuItem_DropDownOpening);
			// 
			// titi1ToolStripMenuItem
			// 
			this.titi1ToolStripMenuItem.Name = "titi1ToolStripMenuItem";
			resources.ApplyResources(this.titi1ToolStripMenuItem, "titi1ToolStripMenuItem");
			// 
			// videoToolStripMenuItem
			// 
			this.videoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.freeToolStripMenuItem,
            this.touchWindowFromInsideToolStripMenuItem,
            this.touchWindowFromOutsideToolStripMenuItem,
            this.strechToWindowToolStripMenuItem,
            this.toolStripMenuItem4,
            this.zoomInToolStripMenuItem,
            this.zoomOutToolStripMenuItem,
            this.zoom50ToolStripMenuItem,
            this.zoom100ToolStripMenuItem,
            this.zoom200ToolStripMenuItem,
            this.toolStripMenuItem6,
            this.moveLeftToolStripMenuItem,
            this.moveRightToolStripMenuItem,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.moveCenterToolStripMenuItem,
            this.toolStripMenuItem5,
            this.aspectRatioToolStripMenuItem,
            this.decreaseAspectRatioToolStripMenuItem,
            this.resetAspectRatioToolStripMenuItem,
            this.aspecRatio169toolStripMenuItem});
			this.videoToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
			this.videoToolStripMenuItem.Name = "videoToolStripMenuItem";
			resources.ApplyResources(this.videoToolStripMenuItem, "videoToolStripMenuItem");
			this.videoToolStripMenuItem.Click += new System.EventHandler(this.videoToolStripMenuItem_Click);
			// 
			// freeToolStripMenuItem
			// 
			this.freeToolStripMenuItem.CheckOnClick = true;
			this.freeToolStripMenuItem.Image = global::CodeTV.Properties.Resources.shading;
			this.freeToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.freeToolStripMenuItem.MergeIndex = 1;
			this.freeToolStripMenuItem.Name = "freeToolStripMenuItem";
			resources.ApplyResources(this.freeToolStripMenuItem, "freeToolStripMenuItem");
			this.freeToolStripMenuItem.Click += new System.EventHandler(this.freeToolStripMenuItem_Click);
			// 
			// touchWindowFromInsideToolStripMenuItem
			// 
			this.touchWindowFromInsideToolStripMenuItem.Checked = true;
			this.touchWindowFromInsideToolStripMenuItem.CheckOnClick = true;
			this.touchWindowFromInsideToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.touchWindowFromInsideToolStripMenuItem.Image = global::CodeTV.Properties.Resources.arrow_in;
			this.touchWindowFromInsideToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.touchWindowFromInsideToolStripMenuItem.MergeIndex = 1;
			this.touchWindowFromInsideToolStripMenuItem.Name = "touchWindowFromInsideToolStripMenuItem";
			resources.ApplyResources(this.touchWindowFromInsideToolStripMenuItem, "touchWindowFromInsideToolStripMenuItem");
			this.touchWindowFromInsideToolStripMenuItem.Click += new System.EventHandler(this.touchWindowFromInsideToolStripMenuItem_Click);
			// 
			// touchWindowFromOutsideToolStripMenuItem
			// 
			this.touchWindowFromOutsideToolStripMenuItem.CheckOnClick = true;
			this.touchWindowFromOutsideToolStripMenuItem.Image = global::CodeTV.Properties.Resources.arrow_out;
			this.touchWindowFromOutsideToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.touchWindowFromOutsideToolStripMenuItem.MergeIndex = 1;
			this.touchWindowFromOutsideToolStripMenuItem.Name = "touchWindowFromOutsideToolStripMenuItem";
			resources.ApplyResources(this.touchWindowFromOutsideToolStripMenuItem, "touchWindowFromOutsideToolStripMenuItem");
			this.touchWindowFromOutsideToolStripMenuItem.Click += new System.EventHandler(this.touchWindowFromOutsideToolStripMenuItem_Click);
			// 
			// strechToWindowToolStripMenuItem
			// 
			this.strechToWindowToolStripMenuItem.CheckOnClick = true;
			this.strechToWindowToolStripMenuItem.Image = global::CodeTV.Properties.Resources.shape_handles;
			this.strechToWindowToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.strechToWindowToolStripMenuItem.MergeIndex = 1;
			this.strechToWindowToolStripMenuItem.Name = "strechToWindowToolStripMenuItem";
			resources.ApplyResources(this.strechToWindowToolStripMenuItem, "strechToWindowToolStripMenuItem");
			this.strechToWindowToolStripMenuItem.Click += new System.EventHandler(this.strechToWindowToolStripMenuItem_Click);
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.toolStripMenuItem4.MergeIndex = 1;
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
			// 
			// zoomInToolStripMenuItem
			// 
			this.zoomInToolStripMenuItem.Image = global::CodeTV.Properties.Resources.zoom_in;
			this.zoomInToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.zoomInToolStripMenuItem.MergeIndex = 1;
			this.zoomInToolStripMenuItem.Name = "zoomInToolStripMenuItem";
			resources.ApplyResources(this.zoomInToolStripMenuItem, "zoomInToolStripMenuItem");
			this.zoomInToolStripMenuItem.Click += new System.EventHandler(this.zoomInToolStripMenuItem_Click);
			// 
			// zoomOutToolStripMenuItem
			// 
			this.zoomOutToolStripMenuItem.Image = global::CodeTV.Properties.Resources.zoom_out;
			this.zoomOutToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.zoomOutToolStripMenuItem.MergeIndex = 1;
			this.zoomOutToolStripMenuItem.Name = "zoomOutToolStripMenuItem";
			resources.ApplyResources(this.zoomOutToolStripMenuItem, "zoomOutToolStripMenuItem");
			this.zoomOutToolStripMenuItem.Click += new System.EventHandler(this.zoomOutToolStripMenuItem_Click);
			// 
			// zoom50ToolStripMenuItem
			// 
			this.zoom50ToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.zoom50ToolStripMenuItem.MergeIndex = 1;
			this.zoom50ToolStripMenuItem.Name = "zoom50ToolStripMenuItem";
			resources.ApplyResources(this.zoom50ToolStripMenuItem, "zoom50ToolStripMenuItem");
			this.zoom50ToolStripMenuItem.Click += new System.EventHandler(this.zoom50ToolStripMenuItem_Click);
			// 
			// zoom100ToolStripMenuItem
			// 
			this.zoom100ToolStripMenuItem.Image = global::CodeTV.Properties.Resources.zoom;
			this.zoom100ToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.zoom100ToolStripMenuItem.MergeIndex = 1;
			this.zoom100ToolStripMenuItem.Name = "zoom100ToolStripMenuItem";
			resources.ApplyResources(this.zoom100ToolStripMenuItem, "zoom100ToolStripMenuItem");
			this.zoom100ToolStripMenuItem.Click += new System.EventHandler(this.zoom100ToolStripMenuItem_Click);
			// 
			// zoom200ToolStripMenuItem
			// 
			this.zoom200ToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.zoom200ToolStripMenuItem.MergeIndex = 1;
			this.zoom200ToolStripMenuItem.Name = "zoom200ToolStripMenuItem";
			resources.ApplyResources(this.zoom200ToolStripMenuItem, "zoom200ToolStripMenuItem");
			this.zoom200ToolStripMenuItem.Click += new System.EventHandler(this.zoom200ToolStripMenuItem_Click);
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.toolStripMenuItem6.MergeIndex = 1;
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			resources.ApplyResources(this.toolStripMenuItem6, "toolStripMenuItem6");
			// 
			// moveLeftToolStripMenuItem
			// 
			this.moveLeftToolStripMenuItem.Image = global::CodeTV.Properties.Resources.arrow_left;
			this.moveLeftToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.moveLeftToolStripMenuItem.MergeIndex = 1;
			this.moveLeftToolStripMenuItem.Name = "moveLeftToolStripMenuItem";
			resources.ApplyResources(this.moveLeftToolStripMenuItem, "moveLeftToolStripMenuItem");
			this.moveLeftToolStripMenuItem.Click += new System.EventHandler(this.moveLeftToolStripMenuItem_Click);
			// 
			// moveRightToolStripMenuItem
			// 
			this.moveRightToolStripMenuItem.Image = global::CodeTV.Properties.Resources.arrow_right;
			this.moveRightToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.moveRightToolStripMenuItem.MergeIndex = 1;
			this.moveRightToolStripMenuItem.Name = "moveRightToolStripMenuItem";
			resources.ApplyResources(this.moveRightToolStripMenuItem, "moveRightToolStripMenuItem");
			this.moveRightToolStripMenuItem.Click += new System.EventHandler(this.moveRightToolStripMenuItem_Click);
			// 
			// moveUpToolStripMenuItem
			// 
			this.moveUpToolStripMenuItem.Image = global::CodeTV.Properties.Resources.arrow_up;
			this.moveUpToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.moveUpToolStripMenuItem.MergeIndex = 1;
			this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
			resources.ApplyResources(this.moveUpToolStripMenuItem, "moveUpToolStripMenuItem");
			this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.moveUpToolStripMenuItem_Click);
			// 
			// moveDownToolStripMenuItem
			// 
			this.moveDownToolStripMenuItem.Image = global::CodeTV.Properties.Resources.arrow_down;
			this.moveDownToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.moveDownToolStripMenuItem.MergeIndex = 1;
			this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
			resources.ApplyResources(this.moveDownToolStripMenuItem, "moveDownToolStripMenuItem");
			this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.moveDownToolStripMenuItem_Click);
			// 
			// moveCenterToolStripMenuItem
			// 
			this.moveCenterToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.moveCenterToolStripMenuItem.MergeIndex = 1;
			this.moveCenterToolStripMenuItem.Name = "moveCenterToolStripMenuItem";
			resources.ApplyResources(this.moveCenterToolStripMenuItem, "moveCenterToolStripMenuItem");
			this.moveCenterToolStripMenuItem.Click += new System.EventHandler(this.moveCenterToolStripMenuItem_Click);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.toolStripMenuItem5.MergeIndex = 1;
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
			// 
			// aspectRatioToolStripMenuItem
			// 
			this.aspectRatioToolStripMenuItem.Image = global::CodeTV.Properties.Resources.shape_square_add;
			this.aspectRatioToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.aspectRatioToolStripMenuItem.MergeIndex = 1;
			this.aspectRatioToolStripMenuItem.Name = "aspectRatioToolStripMenuItem";
			resources.ApplyResources(this.aspectRatioToolStripMenuItem, "aspectRatioToolStripMenuItem");
			this.aspectRatioToolStripMenuItem.Click += new System.EventHandler(this.increaseAspectRatioToolStripMenuItem_Click);
			// 
			// decreaseAspectRatioToolStripMenuItem
			// 
			this.decreaseAspectRatioToolStripMenuItem.Image = global::CodeTV.Properties.Resources.shape_square_delete;
			this.decreaseAspectRatioToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.decreaseAspectRatioToolStripMenuItem.MergeIndex = 1;
			this.decreaseAspectRatioToolStripMenuItem.Name = "decreaseAspectRatioToolStripMenuItem";
			resources.ApplyResources(this.decreaseAspectRatioToolStripMenuItem, "decreaseAspectRatioToolStripMenuItem");
			this.decreaseAspectRatioToolStripMenuItem.Click += new System.EventHandler(this.decreaseAspectRatioToolStripMenuItem_Click);
			// 
			// resetAspectRatioToolStripMenuItem
			// 
			this.resetAspectRatioToolStripMenuItem.Image = global::CodeTV.Properties.Resources.shape_square;
			this.resetAspectRatioToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.resetAspectRatioToolStripMenuItem.MergeIndex = 1;
			this.resetAspectRatioToolStripMenuItem.Name = "resetAspectRatioToolStripMenuItem";
			resources.ApplyResources(this.resetAspectRatioToolStripMenuItem, "resetAspectRatioToolStripMenuItem");
			this.resetAspectRatioToolStripMenuItem.Click += new System.EventHandler(this.resetAspectRatioToolStripMenuItem_Click);
			// 
			// aspecRatio169toolStripMenuItem
			// 
			this.aspecRatio169toolStripMenuItem.CheckOnClick = true;
			this.aspecRatio169toolStripMenuItem.Image = global::CodeTV.Properties.Resources.image;
			this.aspecRatio169toolStripMenuItem.Name = "aspecRatio169toolStripMenuItem";
			resources.ApplyResources(this.aspecRatio169toolStripMenuItem, "aspecRatio169toolStripMenuItem");
			this.aspecRatio169toolStripMenuItem.Click += new System.EventHandler(this.aspecRatio169toolStripMenuItem_Click);
			// 
			// graphToolStripMenuItem
			// 
			this.graphToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runToolStripMenuItem,
            this.pauseGraphToolStripMenuItem,
            this.stopGraphToolStripMenuItem,
            this.restartToolStripMenuItem,
            this.releaseToolStripMenuItem});
			this.graphToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
			this.graphToolStripMenuItem.Name = "graphToolStripMenuItem";
			resources.ApplyResources(this.graphToolStripMenuItem, "graphToolStripMenuItem");
			this.graphToolStripMenuItem.DropDownOpening += new System.EventHandler(this.graphToolStripMenuItem_DropDownOpening);
			// 
			// runToolStripMenuItem
			// 
			this.runToolStripMenuItem.Image = global::CodeTV.Properties.Resources.control_play;
			this.runToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.runToolStripMenuItem.MergeIndex = 1;
			this.runToolStripMenuItem.Name = "runToolStripMenuItem";
			resources.ApplyResources(this.runToolStripMenuItem, "runToolStripMenuItem");
			this.runToolStripMenuItem.Click += new System.EventHandler(this.runToolStripMenuItem_Click);
			// 
			// pauseGraphToolStripMenuItem
			// 
			this.pauseGraphToolStripMenuItem.Image = global::CodeTV.Properties.Resources.control_pause;
			this.pauseGraphToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.pauseGraphToolStripMenuItem.MergeIndex = 1;
			this.pauseGraphToolStripMenuItem.Name = "pauseGraphToolStripMenuItem";
			resources.ApplyResources(this.pauseGraphToolStripMenuItem, "pauseGraphToolStripMenuItem");
			this.pauseGraphToolStripMenuItem.Click += new System.EventHandler(this.pauseGraphToolStripMenuItem_Click);
			// 
			// stopGraphToolStripMenuItem
			// 
			this.stopGraphToolStripMenuItem.Image = global::CodeTV.Properties.Resources.control_stop;
			this.stopGraphToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.stopGraphToolStripMenuItem.MergeIndex = 1;
			this.stopGraphToolStripMenuItem.Name = "stopGraphToolStripMenuItem";
			resources.ApplyResources(this.stopGraphToolStripMenuItem, "stopGraphToolStripMenuItem");
			this.stopGraphToolStripMenuItem.Click += new System.EventHandler(this.stopGraphToolStripMenuItem_Click);
			// 
			// restartToolStripMenuItem
			// 
			this.restartToolStripMenuItem.Image = global::CodeTV.Properties.Resources.control_repeat;
			this.restartToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.restartToolStripMenuItem.MergeIndex = 1;
			this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
			resources.ApplyResources(this.restartToolStripMenuItem, "restartToolStripMenuItem");
			this.restartToolStripMenuItem.Click += new System.EventHandler(this.restartToolStripMenuItem_Click);
			// 
			// releaseToolStripMenuItem
			// 
			this.releaseToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
			this.releaseToolStripMenuItem.MergeIndex = 1;
			this.releaseToolStripMenuItem.Name = "releaseToolStripMenuItem";
			resources.ApplyResources(this.releaseToolStripMenuItem, "releaseToolStripMenuItem");
			this.releaseToolStripMenuItem.Click += new System.EventHandler(this.releaseToolStripMenuItem_Click);
			// 
			// filtersToolStripMenuItem
			// 
			this.filtersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toto1ToolStripMenuItem});
			this.filtersToolStripMenuItem.Name = "filtersToolStripMenuItem";
			resources.ApplyResources(this.filtersToolStripMenuItem, "filtersToolStripMenuItem");
			this.filtersToolStripMenuItem.DropDownOpening += new System.EventHandler(this.filtersToolStripMenuItem_DropDownOpening);
			// 
			// toto1ToolStripMenuItem
			// 
			this.toto1ToolStripMenuItem.Name = "toto1ToolStripMenuItem";
			resources.ApplyResources(this.toto1ToolStripMenuItem, "toto1ToolStripMenuItem");
			// 
			// windowToolStripMenuItem
			// 
			this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.channelToolStripMenuItem,
            this.channelInfosToolStripMenuItem,
            this.mediaTuningToolStripMenuItem,
            this.timeLineToolStripMenuItem,
            this.toolStripMenuItem2,
            this.channelPropertiesToolStripMenuItem,
            this.settingsToolStripMenuItem});
			this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
			resources.ApplyResources(this.windowToolStripMenuItem, "windowToolStripMenuItem");
			// 
			// channelToolStripMenuItem
			// 
			this.channelToolStripMenuItem.Image = global::CodeTV.Properties.Resources.application_side_tree;
			this.channelToolStripMenuItem.Name = "channelToolStripMenuItem";
			resources.ApplyResources(this.channelToolStripMenuItem, "channelToolStripMenuItem");
			this.channelToolStripMenuItem.Click += new System.EventHandler(this.channelPanelToolStripMenuItem_Click);
			// 
			// channelInfosToolStripMenuItem
			// 
			this.channelInfosToolStripMenuItem.Image = global::CodeTV.Properties.Resources.book_open;
			this.channelInfosToolStripMenuItem.Name = "channelInfosToolStripMenuItem";
			resources.ApplyResources(this.channelInfosToolStripMenuItem, "channelInfosToolStripMenuItem");
			this.channelInfosToolStripMenuItem.Click += new System.EventHandler(this.channelInfosToolStripMenuItem_Click);
			// 
			// mediaTuningToolStripMenuItem
			// 
			this.mediaTuningToolStripMenuItem.Image = global::CodeTV.Properties.Resources.sound_none;
			this.mediaTuningToolStripMenuItem.Name = "mediaTuningToolStripMenuItem";
			resources.ApplyResources(this.mediaTuningToolStripMenuItem, "mediaTuningToolStripMenuItem");
			this.mediaTuningToolStripMenuItem.Click += new System.EventHandler(this.mediaTuningToolStripMenuItem_Click);
			// 
			// timeLineToolStripMenuItem
			// 
			this.timeLineToolStripMenuItem.Image = global::CodeTV.Properties.Resources.time;
			this.timeLineToolStripMenuItem.Name = "timeLineToolStripMenuItem";
			resources.ApplyResources(this.timeLineToolStripMenuItem, "timeLineToolStripMenuItem");
			this.timeLineToolStripMenuItem.Click += new System.EventHandler(this.timeLineToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
			// 
			// channelPropertiesToolStripMenuItem
			// 
			this.channelPropertiesToolStripMenuItem.Image = global::CodeTV.Properties.Resources.database_table;
			this.channelPropertiesToolStripMenuItem.Name = "channelPropertiesToolStripMenuItem";
			resources.ApplyResources(this.channelPropertiesToolStripMenuItem, "channelPropertiesToolStripMenuItem");
			this.channelPropertiesToolStripMenuItem.Click += new System.EventHandler(this.channelPropertiesToolStripMenuItem_Click);
			// 
			// settingsToolStripMenuItem
			// 
			this.settingsToolStripMenuItem.Image = global::CodeTV.Properties.Resources.Properties;
			this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
			this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
			// 
			// toolStripMenuItemAbout
			// 
			this.toolStripMenuItemAbout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem8});
			this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
			resources.ApplyResources(this.toolStripMenuItemAbout, "toolStripMenuItemAbout");
			// 
			// toolStripMenuItem8
			// 
			this.toolStripMenuItem8.Image = global::CodeTV.Properties.Resources.Help;
			this.toolStripMenuItem8.Name = "toolStripMenuItem8";
			resources.ApplyResources(this.toolStripMenuItem8, "toolStripMenuItem8");
			this.toolStripMenuItem8.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
			// 
			// toolStripButtonNextChannel
			// 
			this.toolStripButtonNextChannel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonNextChannel.Image = global::CodeTV.Properties.Resources.add;
			resources.ApplyResources(this.toolStripButtonNextChannel, "toolStripButtonNextChannel");
			this.toolStripButtonNextChannel.Name = "toolStripButtonNextChannel";
			this.toolStripButtonNextChannel.Click += new System.EventHandler(this.toolStripButtonNextChannel_Click);
			// 
			// toolStripButtonPreviousChannel
			// 
			this.toolStripButtonPreviousChannel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonPreviousChannel.Image = global::CodeTV.Properties.Resources.delete;
			resources.ApplyResources(this.toolStripButtonPreviousChannel, "toolStripButtonPreviousChannel");
			this.toolStripButtonPreviousChannel.Name = "toolStripButtonPreviousChannel";
			this.toolStripButtonPreviousChannel.Click += new System.EventHandler(this.toolStripButtonPreviousChannel_Click);
			// 
			// toolStripButtonPreviousChannelInFolder
			// 
			this.toolStripButtonPreviousChannelInFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonPreviousChannelInFolder, "toolStripButtonPreviousChannelInFolder");
			this.toolStripButtonPreviousChannelInFolder.Image = global::CodeTV.Properties.Resources.arrow_up;
			this.toolStripButtonPreviousChannelInFolder.Name = "toolStripButtonPreviousChannelInFolder";
			this.toolStripButtonPreviousChannelInFolder.Click += new System.EventHandler(this.toolStripButtonPreviousChannelInFolder_Click);
			// 
			// toolStripButtonNextChannelInFolder
			// 
			this.toolStripButtonNextChannelInFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonNextChannelInFolder, "toolStripButtonNextChannelInFolder");
			this.toolStripButtonNextChannelInFolder.Image = global::CodeTV.Properties.Resources.arrow_down;
			this.toolStripButtonNextChannelInFolder.Name = "toolStripButtonNextChannelInFolder";
			this.toolStripButtonNextChannelInFolder.Click += new System.EventHandler(this.toolStripButtonNextChannelInFolder_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// toolStripButtonVideoModeTV
			// 
			this.toolStripButtonVideoModeTV.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonVideoModeTV.Image = global::CodeTV.Properties.Resources.television;
			resources.ApplyResources(this.toolStripButtonVideoModeTV, "toolStripButtonVideoModeTV");
			this.toolStripButtonVideoModeTV.Name = "toolStripButtonVideoModeTV";
			this.toolStripButtonVideoModeTV.Click += new System.EventHandler(this.toolStripButtonVideoModeTV_Click);
			// 
			// toolStripButtonVideoModeFullscreen
			// 
			this.toolStripButtonVideoModeFullscreen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonVideoModeFullscreen.Image = global::CodeTV.Properties.Resources.film;
			resources.ApplyResources(this.toolStripButtonVideoModeFullscreen, "toolStripButtonVideoModeFullscreen");
			this.toolStripButtonVideoModeFullscreen.Name = "toolStripButtonVideoModeFullscreen";
			this.toolStripButtonVideoModeFullscreen.Click += new System.EventHandler(this.toolStripButtonVideoModeFullscreen_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
			// 
			// toolStripButtonSnapshot
			// 
			this.toolStripButtonSnapshot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonSnapshot, "toolStripButtonSnapshot");
			this.toolStripButtonSnapshot.Image = global::CodeTV.Properties.Resources.camera;
			this.toolStripButtonSnapshot.Name = "toolStripButtonSnapshot";
			this.toolStripButtonSnapshot.Click += new System.EventHandler(this.toolStripButtonSnapshot_Click);
			// 
			// toolStripButtonTimeShifting
			// 
			this.toolStripButtonTimeShifting.CheckOnClick = true;
			this.toolStripButtonTimeShifting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonTimeShifting.Image = global::CodeTV.Properties.Resources.time;
			resources.ApplyResources(this.toolStripButtonTimeShifting, "toolStripButtonTimeShifting");
			this.toolStripButtonTimeShifting.Name = "toolStripButtonTimeShifting";
			this.toolStripButtonTimeShifting.Click += new System.EventHandler(this.toolStripButtonTimeShifting_Click);
			// 
			// toolStripButtonPlay
			// 
			this.toolStripButtonPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonPlay, "toolStripButtonPlay");
			this.toolStripButtonPlay.Image = global::CodeTV.Properties.Resources.control_play_blue;
			this.toolStripButtonPlay.Name = "toolStripButtonPlay";
			this.toolStripButtonPlay.Click += new System.EventHandler(this.toolStripButtonPlay_Click);
			// 
			// toolStripButtonPause
			// 
			this.toolStripButtonPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonPause, "toolStripButtonPause");
			this.toolStripButtonPause.Image = global::CodeTV.Properties.Resources.control_pause_blue;
			this.toolStripButtonPause.Name = "toolStripButtonPause";
			this.toolStripButtonPause.Click += new System.EventHandler(this.toolStripButtonPause_Click);
			// 
			// toolStripButtonRewind
			// 
			this.toolStripButtonRewind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonRewind, "toolStripButtonRewind");
			this.toolStripButtonRewind.Image = global::CodeTV.Properties.Resources.control_rewind_blue;
			this.toolStripButtonRewind.Name = "toolStripButtonRewind";
			this.toolStripButtonRewind.Click += new System.EventHandler(this.toolStripButtonRewind_Click);
			// 
			// toolStripButtonFastForward
			// 
			this.toolStripButtonFastForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonFastForward, "toolStripButtonFastForward");
			this.toolStripButtonFastForward.Image = global::CodeTV.Properties.Resources.control_fastforward_blue;
			this.toolStripButtonFastForward.Name = "toolStripButtonFastForward";
			this.toolStripButtonFastForward.Click += new System.EventHandler(this.toolStripButtonFastForward_Click);
			// 
			// toolStripButtonStop
			// 
			this.toolStripButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonStop, "toolStripButtonStop");
			this.toolStripButtonStop.Image = global::CodeTV.Properties.Resources.control_stop_blue;
			this.toolStripButtonStop.Name = "toolStripButtonStop";
			this.toolStripButtonStop.Click += new System.EventHandler(this.toolStripButtonStop_Click);
			// 
			// toolStripButtonRecord
			// 
			this.toolStripButtonRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonRecord, "toolStripButtonRecord");
			this.toolStripButtonRecord.Name = "toolStripButtonRecord";
			this.toolStripButtonRecord.Click += new System.EventHandler(this.toolStripButtonRecord_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
			// 
			// toolStripButtonHelp
			// 
			this.toolStripButtonHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonHelp.Image = global::CodeTV.Properties.Resources.Help;
			resources.ApplyResources(this.toolStripButtonHelp, "toolStripButtonHelp");
			this.toolStripButtonHelp.Name = "toolStripButtonHelp";
			this.toolStripButtonHelp.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			// 
			// toolStripButtonTest
			// 
			this.toolStripButtonTest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonTest, "toolStripButtonTest");
			this.toolStripButtonTest.Name = "toolStripButtonTest";
			this.toolStripButtonTest.Click += new System.EventHandler(this.toolStripButtonTest_Click);
			// 
			// toolStripButtonTest2
			// 
			this.toolStripButtonTest2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.toolStripButtonTest2, "toolStripButtonTest2");
			this.toolStripButtonTest2.Name = "toolStripButtonTest2";
			this.toolStripButtonTest2.Click += new System.EventHandler(this.toolStripButtonTest2_Click);
			// 
			// imageListLogoTV
			// 
			this.imageListLogoTV.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListLogoTV.ImageStream")));
			this.imageListLogoTV.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListLogoTV.Images.SetKeyName(0, "FolderClosed");
			this.imageListLogoTV.Images.SetKeyName(1, "FolderOpened");
			this.imageListLogoTV.Images.SetKeyName(2, "LogoTVDefault");
			// 
			// timerSignalUpdate
			// 
			this.timerSignalUpdate.Enabled = true;
			this.timerSignalUpdate.Interval = 500;
			this.timerSignalUpdate.Tick += new System.EventHandler(this.timerSignalUpdate_Tick);
			// 
			// BottomToolStripPanel
			// 
			resources.ApplyResources(this.BottomToolStripPanel, "BottomToolStripPanel");
			this.BottomToolStripPanel.Name = "BottomToolStripPanel";
			this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			// 
			// TopToolStripPanel
			// 
			resources.ApplyResources(this.TopToolStripPanel, "TopToolStripPanel");
			this.TopToolStripPanel.Name = "TopToolStripPanel";
			this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			// 
			// ContentPanel
			// 
			resources.ApplyResources(this.ContentPanel, "ContentPanel");
			// 
			// timerChannelNumberBuilder
			// 
			this.timerChannelNumberBuilder.Interval = 700;
			this.timerChannelNumberBuilder.Tick += new System.EventHandler(this.timerChannelNumberBuilder_Tick);
			// 
			// timerVideoRefreshAfterTuningChannel
			// 
			this.timerVideoRefresh.Interval = 1500;
			this.timerVideoRefresh.Tick += new System.EventHandler(this.timerVideoRefreshAfterTuningChannel_Tick);
			// 
			// MainForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(219)))), ((int)(((byte)(249)))));
			this.Controls.Add(this.toolStripContainerMain);
			this.Name = "MainForm";
			this.Shown += new System.EventHandler(this.MainForm_Shown);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.toolStripContainerMain.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainerMain.BottomToolStripPanel.PerformLayout();
			this.toolStripContainerMain.ContentPanel.ResumeLayout(false);
			this.toolStripContainerMain.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainerMain.TopToolStripPanel.PerformLayout();
			this.toolStripContainerMain.ResumeLayout(false);
			this.toolStripContainerMain.PerformLayout();
			this.statusStripMain.ResumeLayout(false);
			this.statusStripMain.PerformLayout();
			this.panelVideoPlace.ResumeLayout(false);
			this.toolStripMain.ResumeLayout(false);
			this.toolStripMain.PerformLayout();
			this.contextMenuStripPanelVideo.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainerMain;
        private System.Windows.Forms.StatusStrip statusStripMain;
		private System.Windows.Forms.ToolStrip toolStripMain;
		internal System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelVideoStatus;
		private System.Windows.Forms.Timer timerSignalUpdate;
        internal System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelChannelName;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBarSignalStrength;
        private System.Windows.Forms.ToolStripButton toolStripButtonVideoModeTV;
        private System.Windows.Forms.ToolStripButton toolStripButtonVideoModeFullscreen;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSignalStrength;
        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
		private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
		private System.Windows.Forms.ToolStripContentPanel ContentPanel;
		private System.Windows.Forms.ToolStripButton toolStripButtonTest;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEPG;
		private System.Windows.Forms.ToolStripButton toolStripButtonTest2;
		internal System.Windows.Forms.ImageList imageListLogoTV;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBarSignalQuality;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSignalQuality;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.Panel panelVideoPlace;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripPanelVideo;
		private System.Windows.Forms.ToolStripMenuItem filtersToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toto1ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem graphToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pauseGraphToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stopGraphToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem channelsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem titi1ToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolStripButtonNextChannelInFolder;
		private System.Windows.Forms.ToolStripButton toolStripButtonPreviousChannelInFolder;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.Timer timerChannelNumberBuilder;
		private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem releaseToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem videoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem freeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem touchWindowFromInsideToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem touchWindowFromOutsideToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem strechToWindowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aspectRatioToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoomInToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem zoomOutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom50ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom100ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom200ToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
		private System.Windows.Forms.ToolStripMenuItem moveLeftToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveRightToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveCenterToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem decreaseAspectRatioToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem resetAspectRatioToolStripMenuItem;
		internal WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripButton toolStripButtonTimeShifting;
		private System.Windows.Forms.ToolStripButton toolStripButtonPause;
		private System.Windows.Forms.ToolStripButton toolStripButtonRewind;
		private System.Windows.Forms.ToolStripButton toolStripButtonFastForward;
		private System.Windows.Forms.ToolStripButton toolStripButtonRecord;
		private System.Windows.Forms.ToolStripButton toolStripButtonHelp;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton toolStripButtonSnapshot;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openVideoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem channelWizardToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem channelToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem channelInfosToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mediaTuningToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem channelPropertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAbout;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
		private System.Windows.Forms.ToolStripMenuItem aspecRatio169toolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolStripButtonPreviousChannel;
		private System.Windows.Forms.ToolStripButton toolStripButtonNextChannel;
		private System.Windows.Forms.ToolStripMenuItem timeLineToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolStripButtonPlay;
		private System.Windows.Forms.ToolStripButton toolStripButtonStop;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTimeLinePosition;
		private System.Windows.Forms.Timer timerVideoRefresh;


    }
}

