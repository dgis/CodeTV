//    Copyright (C) 2006-2007  Regis COSNIER
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;
using CodeTV.PSI;

using WeifenLuo.WinFormsUI;
using WeifenLuo.WinFormsUI.Docking;
using System.Resources;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;

namespace CodeTV
{
	public partial class MainForm : Form
	{
		// Static stuffs
		public static TraceSwitch trace = new TraceSwitch("General", "General traces", "Info");
        private static Settings settings = Settings.LoadDefaultSettings(); // null;
		public static Settings Settings { get { return settings; } }
		private static MainForm mainForm = null;
		public static MainForm Form { get { return mainForm; } }
		private static string assemblyName = null;
		public static string AssemblyName { get { return assemblyName == null ? assemblyName = Assembly.GetExecutingAssembly().GetName().Name : assemblyName; } }
		private static string registryBaseKey = "SOFTWARE\\La compagnie du bonheur :)\\CodeTV";
		public static string RegistryBaseKey { get { return registryBaseKey; } }

		// GraphBuilder
		internal GraphBuilderBase currentGraphBuilder;
		public GraphBuilderBase GraphBuilder { get { return this.currentGraphBuilder; } }
		internal enum GraphBuilderType { BDA, BDATimeShifting, BDAMosaic }
		internal GraphBuilderType graphBuilderType = GraphBuilderType.BDA;

		// Channels
		internal ChannelFolder rootChannelFolder = new ChannelFolder("Root");
		internal Hashtable channelByChannelNumber = new Hashtable();
		internal ArrayList cloneChannelNumber = new ArrayList();
		internal int channelNumberMax = 0;

		// Video modes
		internal VideoMode currentVideoMode = VideoMode.Normal;
		internal VideoForm videoForm = new VideoForm();
		internal Rectangle currentTVModeBounds = new Rectangle();

		// EPG
		private TimeSpan epgUpdateTimeOut = TimeSpan.FromMilliseconds(30000.0);
		private DateTime lastAllServiceEPGUpdate = DateTime.MinValue;
		private DateTime lastAllProgramEPGUpdate = DateTime.MinValue;
		private DateTime lastAllScheduleEPGUpdate = DateTime.MinValue;

		// Panels
		private DeserializeDockContent deserializeDockContent;
		internal PanelChannel panelChannel;
		internal PanelChannelProperties panelChannelProperties;
		internal PanelEPG panelEPG;
		internal PanelInfos panelInfos;
		internal PanelMediaTuning panelMediaTuning;
		internal PanelSettings panelSettings;
		internal PanelTimeLine panelTimeLine;
		internal PanelVideo panelVideo;
		internal VideoControl videoControl;

		// Remote Control
		internal WinLIRC winLIRC = null;
		internal string channelNumberBuilder = "";
		public enum CommandName
		{
			Nop,
			ToggleMenu,
			Ok,
			Cancel,
			Left,
			Right,
			Up,
			Down,
			Key0,
			Key1,
			Key2,
			Key3,
			Key4,
			Key5,
			Key6,
			Key7,
			Key8,
			Key9,
			ChannelPreviousInFolder,
			ChannelNextInFolder,
			ChannelPrevious,
			ChannelNext,
			VolumePlus,
			VolumeMinus,
			MediaPlay,
			MediaPause,
			MediaStop,
			MediaRecord,
			MediaRewind,
			MediaFastForward,
			VideoReset,
			VideoZoomHalf,
			VideoZoomNormal,
			VideoZoomDouble,
			VideoZoomFreeMode,
			VideoZoomFromInside,
			VideoZoomFromOutside,
			VideoZoomStretchToWindow,
			VideoZoomIncrease,
			VideoZoomDecrease,
			VideoResetAspectRatio,
			VideoIncreaseAspectRatio,
			VideoDecreaseAspectRatio,
			VideoCenter,
			VideoMoveLeft,
			VideoMoveRight,
			VideoMoveUp,
			VideoMoveDown,
			VideoModeNormal,
			VideoModeTV,
			VideoModeFullscreen,
			SnapShot,
		}

		// Screen saver and power saving
		private const uint SPI_GETSCREENSAVEACTIVE = 0x0010;
		private const uint SPI_SETSCREENSAVEACTIVE = 0x0011;
		private const uint SPI_GETLOWPOWERTIMEOUT = 0x004F;
		private const uint SPI_GETPOWEROFFTIMEOUT = 0x0050;
		private const uint SPI_SETLOWPOWERTIMEOUT = 0x0051;
		private const uint SPI_SETPOWEROFFTIMEOUT = 0x0052;
		private const uint SPI_GETLOWPOWERACTIVE = 0x0053;
		private const uint SPI_GETPOWEROFFACTIVE = 0x0054;
		private const uint SPI_SETLOWPOWERACTIVE = 0x0055;
		private const uint SPI_SETPOWEROFFACTIVE = 0x0056;

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);
		[DllImport("User32.dll", CharSet = CharSet.Auto)]	
		private static extern bool SystemParametersInfo(uint uiAction, bool uiParam, IntPtr pvParam, uint fWinIni);

		private const uint ES_SYSTEM_REQUIRED = 0x00000001;
		private const uint ES_DISPLAY_REQUIRED = 0x00000002;
		private const uint ES_USER_PRESENT = 0x00000004;
		private const uint ES_CONTINUOUS = 0x80000000;

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]	
		private static extern uint SetThreadExecutionState(uint esFlags);

		private bool screenSaverWasEnabled = true;
		private uint powerSavingPreviousState;

		// Miscellaneous
		private bool firstLaunch = false;
		private FormWindowState beforeLaunchWindowState = FormWindowState.Normal;

		// Contructor
		public MainForm()
		{
			mainForm = this;

			InitializeComponent();


			// Disabled the screensaver
			SystemParametersInfo(SPI_GETSCREENSAVEACTIVE, 0, ref this.screenSaverWasEnabled, 0);
			if (this.screenSaverWasEnabled)
				SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, false, IntPtr.Zero, 0);

			// Disabled power saving
			this.powerSavingPreviousState = SetThreadExecutionState(ES_DISPLAY_REQUIRED | ES_CONTINUOUS);

			// Miscellaneous initialization
			ToolStripManager.LoadSettings(this, RegistryBaseKey);

			this.dockPanel.SuspendLayout();
			this.panelChannel = new PanelChannel();
			this.panelChannelProperties = new PanelChannelProperties();
			this.panelEPG = new PanelEPG();
			this.panelInfos = new PanelInfos();
			this.panelMediaTuning = new PanelMediaTuning();
			this.panelSettings = new PanelSettings();
			this.panelTimeLine = new PanelTimeLine();
			this.panelTimeLine.ShowHint = DockState.DockBottom;
			this.panelVideo = new PanelVideo();
			this.videoControl = new VideoControl();
			this.videoControl.BackColor = System.Drawing.Color.Black;
			this.videoControl.ContextMenuStrip = this.contextMenuStripPanelVideo;
			this.videoControl.Name = "videoControl";
			this.videoControl.Dock = DockStyle.Fill;
			this.videoControl.DoubleClick += new System.EventHandler(this.videoControl_DoubleClick);
			this.videoControl.Resize += new System.EventHandler(this.videoControl_Resize);
			this.videoControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.videoControl_KeyDown);
			this.videoControl.MouseWheel += new MouseEventHandler(videoControl_MouseWheel);
			this.panelVideo.Controls.Add(this.videoControl);

			this.deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
			try
			{
				this.dockPanel.LoadFromXml(FileUtils.WorkingDirectory + "\\" + AssemblyName + ".Settings.UI.xml", deserializeDockContent);
			}
			catch (FileNotFoundException)
			{
				this.dockPanel.DockBottomPortion = 58;
				this.dockPanel.DockTopPortion = 58;
				this.panelChannelProperties.Show(this.dockPanel, DockState.DockRightAutoHide);
				this.panelSettings.Show(this.dockPanel, DockState.DockRightAutoHide);
				this.panelChannel.Show(this.dockPanel, DockState.DockLeft);
				this.panelMediaTuning.Show(this.panelChannel.Pane, DockAlignment.Bottom, 0.3);
				this.panelTimeLine.Show(this.dockPanel, DockState.DockBottom);
			}
			this.panelVideo.Show(this.dockPanel);

			this.videoForm.FormClosing += new FormClosingEventHandler(videoForm_FormClosing);

			// Load the default channels
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(FileUtils.WorkingDirectory + "\\" + AssemblyName + ".Channels.xml", FileMode.Open);
				this.panelChannel.LoadChannels(fileStream);
			}
			catch
			{
				firstLaunch = true;
			}
			finally
			{
				if (fileStream != null)
					fileStream.Close();
			}

			// Settings
			this.panelMediaTuning.trackBarVolume.Value = MainForm.Settings.Volume;
			this.panelMediaTuning.trackBarBalance.Value = MainForm.Settings.Balance;

			this.editToolStripMenuItem.DropDown = this.contextMenuStripPanelVideo;

			this.aspecRatio169toolStripMenuItem.Checked = Settings.UseVideo169Mode;
			ChangeSetting("UseVideo169Mode");
			this.toolStripButtonTimeShifting.Checked = Settings.TimeShiftingActivated;
			ChangeSetting("TimeShiftingActivated");
			ChangeSetting("UseWPF");
			ChangeSetting("VideoBackgroundColor");
			ChangeSetting("UseWinLIRC");

			this.dockPanel.ResumeLayout();


			beforeLaunchWindowState = WindowState;
			switch (Settings.StartVideoMode)
			{
				case VideoMode.TV:
					ExecuteCommand(CommandName.VideoModeTV);
					//WindowState = FormWindowState.Minimized;
					break;
				case VideoMode.Fullscreen:
					ExecuteCommand(CommandName.VideoModeFullscreen);
					//WindowState = FormWindowState.Minimized;
					break;
			}
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			if (firstLaunch)
				ShowChannelWizard();

			switch (Settings.StartVideoMode)
			{
				case VideoMode.TV:
					Hide();
					//WindowState = beforeLaunchWindowState;
					break;
				case VideoMode.Fullscreen:
					Hide();
					//WindowState = beforeLaunchWindowState;
					break;
			}
			videoControl.Focus();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Settings.SaveDefaultSettings();

			ToolStripManager.SaveSettings(this, RegistryBaseKey);

			this.panelVideo.DockPanel = null;

			this.dockPanel.SaveAsXml(FileUtils.WorkingDirectory + "\\" + AssemblyName + ".Settings.UI.xml");

            FileStream fileStream = null;

            //try
            //{
            //    fileStream = new FileStream(Settings.SettingsFilePath, FileMode.Create, FileAccess.Write);
            //    Settings.Serialize(fileStream);
            //    this.dockPanel.SaveAsXml(fileStream, Encoding.UTF8, true);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
            //finally
            //{
            //    if (fileStream != null)
            //        fileStream.Close();
            //}
			
			try
			{
				fileStream = new FileStream(FileUtils.WorkingDirectory + "\\" + AssemblyName + ".Channels.xml", FileMode.Create, FileAccess.Write);
				this.panelChannel.SaveChannels(fileStream);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				if (fileStream != null)
					fileStream.Close();
			}


			ClearGraph();

			// Sets the previous screensaver state
			if (this.screenSaverWasEnabled)
				SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, true, IntPtr.Zero, 0);

			// Sets the previous power saving state
			SetThreadExecutionState(this.powerSavingPreviousState);
		}

		private IDockContent GetContentFromPersistString(string persistString)
		{
			if (persistString == typeof(PanelChannel).ToString())
				return this.panelChannel;
			else if (persistString == typeof(PanelChannelProperties).ToString())
				return this.panelChannelProperties;
			else if (persistString == typeof(PanelEPG).ToString())
				return this.panelEPG;
			else if (persistString == typeof(PanelInfos).ToString())
				return this.panelInfos;
			else if (persistString == typeof(PanelMediaTuning).ToString())
				return this.panelMediaTuning;
			else if (persistString == typeof(PanelSettings).ToString())
				return this.panelSettings;
			else if (persistString == typeof(PanelTimeLine).ToString())
				return this.panelTimeLine;
			else
				return null;
			//else
			//{
			//    string[] parsedStrings = persistString.Split(new char[] { ',' });
			//    if (parsedStrings.Length != 3)
			//        return null;

			//    if (parsedStrings[0] != typeof(DummyDoc).ToString())
			//        return null;

			//    DummyDoc dummyDoc = new DummyDoc();
			//    if (parsedStrings[1] != string.Empty)
			//        dummyDoc.FileName = parsedStrings[1];
			//    if (parsedStrings[2] != string.Empty)
			//        dummyDoc.Text = parsedStrings[2];

			//    return dummyDoc;
			//}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void propertyGridSettings_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			//ChangeSetting(e.ChangedItem.Label);
		}

		internal void ChangeSetting(string settingName, bool updatePropertyGrid)
		{
			if (updatePropertyGrid)
				this.panelSettings.propertyGridSettings.SelectedObject = Settings;
			ChangeSetting(settingName);
		}

		internal void ChangeSetting(string settingName)
		{
			switch (settingName)
			{
				case "VideoBackgroundColor":
					if (this.currentGraphBuilder != null)
						this.currentGraphBuilder.VideoRefresh();
					else
						this.videoControl.BackColor = Settings.VideoBackgroundColor;
					break;
				case "UseVideo169Mode":
					if (this.currentGraphBuilder != null)
					{
						this.currentGraphBuilder.UseVideo169Mode = Settings.UseVideo169Mode;
						this.currentGraphBuilder.VideoRefresh();
					}
					break;
				case "TimeShiftingActivated":
					this.toolStripButtonTimeShifting.Checked = Settings.TimeShiftingActivated;

					this.graphBuilderType = Settings.TimeShiftingActivated ? GraphBuilderType.BDATimeShifting : GraphBuilderType.BDA;

					if (this.currentGraphBuilder is IBDA)
					{
						ChannelDVB currentChannelDVB = (this.currentGraphBuilder as ITV).CurrentChannel as ChannelDVB;
						if (currentChannelDVB != null)
						{
							if (Settings.TimeShiftingActivated ^ this.currentGraphBuilder is GraphBuilderBDATimeShifting)
								TuneChannelGUI(currentChannelDVB, true);
						}
					}
					break;
				case "UseWPF":
					this.videoControl.UseWPF = Settings.UseWPF;
					if (this.currentGraphBuilder is ITV)
						TuneChannel((this.currentGraphBuilder as ITV).CurrentChannel, true);
					else
						ClearGraph();
					break;
				case "UseWinLIRC":
					if (Settings.UseWinLIRC)
					{
						this.winLIRC = new WinLIRC();
						this.winLIRC.SyncControl = this;
						this.winLIRC.HostName = Settings.WinLIRCHostName;
						this.winLIRC.Port = Settings.WinLIRCPort;
						this.winLIRC.CommandReceived += new WinLIRC.CommandReceivedEventHandler(winLIRC_CommandReceived);
						this.winLIRC.Start();
						//if (!this.winLIRC.Start())
						//    MessageBox.Show("Cannot connect to WinLIRC server: " + this.winLIRC.HostName + ":" + this.winLIRC.Port);
					}
					else if (this.winLIRC != null)
						this.winLIRC.Stop();
					break;
			}
		}

		public GraphBuilderTV TuneChannel(Channel channel, bool needRebuild, GraphBuilderBase currentGraph, VideoControl hostingControl)
		{
			if (needRebuild && currentGraph != null)
			{
				ClearGraph();
				//OnGraphStop();
				//currentGraph.Dispose();
				//currentGraph = null;
			}

			GraphBuilderTV currentGraphTV = currentGraph as GraphBuilderTV;

			if (channel is ChannelDVB)
			{
				ChannelDVB channelDVB = channel as ChannelDVB;

				if (needRebuild)
				{
					GraphBuilderBDA newGraph = null;
					switch(this.graphBuilderType)
					{
						case GraphBuilderType.BDA:
							newGraph = new GraphBuilderBDA(hostingControl);
							break;
						case GraphBuilderType.BDATimeShifting:
							newGraph = new GraphBuilderBDATimeShifting(hostingControl);
							break;
						case GraphBuilderType.BDAMosaic:
							newGraph = new GraphBuilderBDAMosaic(hostingControl);
							break;
					}
					newGraph.GraphStarted += new EventHandler(newGraph_GraphStarted);
					newGraph.GraphEnded += new EventHandler(newGraph_GraphEnded);
					newGraph.PossibleChanged += new EventHandler<GraphBuilderBase.PossibleEventArgs>(newGraph_PossibleChanged);
					newGraph.Settings = Settings;
					currentGraphTV = newGraph;

					newGraph.ReferenceClock = channelDVB.ReferenceClock;

					DsDevice device;
					if (!string.IsNullOrEmpty(channelDVB.AudioDecoderDevice))
					{
						if (GraphBuilderBDA.AudioDecoderDevices.TryGetValue(channelDVB.AudioDecoderDevice, out device))
							newGraph.AudioDecoderDevice = device;
						else
							throw new Exception(string.Format(Properties.Resources.AudioDeviceNotFound, channelDVB.AudioDecoderDevice));
					}
					if (channelDVB.VideoDecoderType == ChannelDVB.VideoType.MPEG2)
					{
						if (!string.IsNullOrEmpty(channelDVB.MPEG2DecoderDevice))
						{
							if (GraphBuilderBDA.MPEG2DecoderDevices.TryGetValue(channelDVB.MPEG2DecoderDevice, out device))
								newGraph.Mpeg2DecoderDevice = device;
							else
								throw new Exception(string.Format(Properties.Resources.MPEG2DecoderDeviceNotFound, channelDVB.MPEG2DecoderDevice));
						}
					}
					else
					{
						if (!string.IsNullOrEmpty(channelDVB.H264DecoderDevice))
						{
							if (GraphBuilderBDA.H264DecoderDevices.TryGetValue(channelDVB.H264DecoderDevice, out device))
								newGraph.H264DecoderDevice = device;
							else
								throw new Exception(string.Format(Properties.Resources.H264DecoderDeviceNotFound, channelDVB.H264DecoderDevice));
						}
					}
					if (!string.IsNullOrEmpty(channelDVB.AudioRendererDevice))
					{
						if (GraphBuilderBDA.AudioRendererDevices.TryGetValue(channelDVB.AudioRendererDevice, out device))
							newGraph.AudioRendererDevice = device;
						else
							throw new Exception(string.Format(Properties.Resources.AudioRendererDeviceNotFound, channelDVB.AudioRendererDevice));
					}
					if (!string.IsNullOrEmpty(channelDVB.TunerDevice))
					{
						if (GraphBuilderBDA.TunerDevices.TryGetValue(channelDVB.TunerDevice, out device))
							newGraph.TunerDevice = device;
						else
							throw new Exception(string.Format(Properties.Resources.TunerDeviceNotFound, channelDVB.TunerDevice));
					}
					if (!string.IsNullOrEmpty(channelDVB.CaptureDevice))
					{
						if (GraphBuilderBDA.CaptureDevices.TryGetValue(channelDVB.CaptureDevice, out device))
							newGraph.CaptureDevice = device;
						else
							throw new Exception(string.Format(Properties.Resources.CaptureDeviceNotFound, channelDVB.CaptureDevice));
					}

					IDVBTuningSpace tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
					if (channel is ChannelDVBT)
					{
						tuningSpace.put_UniqueName("DVBT TuningSpace");
						tuningSpace.put_FriendlyName("DVBT TuningSpace");
						tuningSpace.put_NetworkType(CLSID.DVBTNetworkProvider);
						tuningSpace.put_SystemType(DVBSystemType.Terrestrial);
					}
					else if (channel is ChannelDVBC)
					{
						tuningSpace.put_UniqueName("DVBC TuningSpace");
						tuningSpace.put_FriendlyName("DVBC TuningSpace");
						tuningSpace.put_NetworkType(CLSID.DVBCNetworkProvider);
						tuningSpace.put_SystemType(DVBSystemType.Cable);
					}
					else if (channel is ChannelDVBS)
					{
						tuningSpace.put_UniqueName("DVBS TuningSpace");
						tuningSpace.put_FriendlyName("DVBS TuningSpace");
						tuningSpace.put_NetworkType(CLSID.DVBSNetworkProvider);
						tuningSpace.put_SystemType(DVBSystemType.Satellite);
					}
					newGraph.TuningSpace = tuningSpace as ITuningSpace;
					newGraph.BuildGraph();
				}

				currentGraphTV.SubmitTuneRequest(channel);

				currentGraphTV.VideoZoomMode = channelDVB.VideoZoomMode;
				currentGraphTV.VideoKeepAspectRatio = channelDVB.VideoKeepAspectRatio;
				currentGraphTV.VideoOffset = channelDVB.VideoOffset;
				currentGraphTV.VideoZoom = channelDVB.VideoZoom;
				currentGraphTV.VideoAspectRatioFactor = channelDVB.VideoAspectRatioFactor;

				currentGraphTV.RunGraph();
				//currentGraph.VideoResizer();
				currentGraphTV.CurrentChannel = channel;
			}
			else if (channel is ChannelAnalogic)
			{
				ChannelAnalogic channelAnalogic = channel as ChannelAnalogic;

				if (needRebuild)
				{
					GraphBuilderWDM newGraph = new GraphBuilderWDM(hostingControl);
					newGraph.GraphStarted += new EventHandler(newGraph_GraphStarted);
					newGraph.GraphEnded += new EventHandler(newGraph_GraphEnded);
					newGraph.PossibleChanged += new EventHandler<GraphBuilderBase.PossibleEventArgs>(newGraph_PossibleChanged);
					newGraph.Settings = Settings;
					currentGraphTV = newGraph;

					DsDevice device;
					if (GraphBuilderWDM.AudioRendererDevices.TryGetValue(channelAnalogic.AudioRendererDevice, out device))
						newGraph.AudioRendererDevice = device;
					if (GraphBuilderWDM.VideoInputDevices.TryGetValue(channelAnalogic.VideoCaptureDeviceName, out device))
						newGraph.VideoCaptureDevice = device;
					if (GraphBuilderWDM.AudioInputDevices.TryGetValue(channelAnalogic.AudioCaptureDeviceName, out device))
						newGraph.AudioCaptureDevice = device;

					newGraph.FormatOfCapture = channelAnalogic.FormatOfCapture;

					newGraph.BuildGraph();
				}

				bool goodTuning = true;
				try
				{
					currentGraphTV.SubmitTuneRequest(channel);
				}
				catch (COMException)
				{
					goodTuning = false;
				}
				if (goodTuning)
				{
					currentGraphTV.VideoZoomMode = channelAnalogic.VideoZoomMode;
					currentGraphTV.VideoKeepAspectRatio = channelAnalogic.VideoKeepAspectRatio;
					currentGraphTV.VideoOffset = channelAnalogic.VideoOffset;
					currentGraphTV.VideoZoom = channelAnalogic.VideoZoom;
					currentGraphTV.VideoAspectRatioFactor = channelAnalogic.VideoAspectRatioFactor;

					currentGraphTV.RunGraph();
					//currentGraph.VideoResizer();
					currentGraphTV.CurrentChannel = channel;
				}
				else
					currentGraphTV.CurrentChannel = null;
			}
			return currentGraphTV;
		}

		internal void TuneChannel(Channel channel, bool needRebuild)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				this.currentGraphBuilder = TuneChannel(channel, needRebuild, this.currentGraphBuilder, this.videoControl);
				if (needRebuild)
					OnGraphInit();
				OnChannelChange();
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void TuneMosaicChannel(Channel channel)
		{
			//if (channel is ChannelDVBT)
			//{
			//    ChannelDVBT channelDVBT = channel as ChannelDVBT;


			//    ClearGraph();

			//    this.currentGraphBuilder = new GraphBuilderBDA2(this.videoControl);

			//    DsDevice device;
			//    if (this.bdaTunerDevices.TryGetValue(channelDVBT.TunerDevice, out device))
			//        this.currentGraphBuilder.TunerDevice = device;
			//    if (this.bdaTunerDevices.TryGetValue(channelDVBT.CaptureDevice, out device))
			//        this.currentGraphBuilder.CaptureDevice = device;

			//    IDVBTuningSpace tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
			//    tuningSpace.put_UniqueName("DVBT TuningSpace");
			//    tuningSpace.put_FriendlyName("DVBT TuningSpace");
			//    tuningSpace.put_NetworkType(CLSID.DVBTNetworkProvider);
			//    tuningSpace.put_SystemType(DVBSystemType.Terrestrial);
			//    this.objTuningSpace = tuningSpace as ITuningSpace;

			//    this.objTuningSpace.CreateTuneRequest(out objTuneRequest);
			//    IDVBTuneRequest dvbTuneRequest = (IDVBTuneRequest)objTuneRequest;

			//    IDVBTLocator dvbtLocator = (IDVBTLocator)new DVBTLocator();
			//    dvbtLocator.put_CarrierFrequency((int)channelDVBT.Frequency);
			//    dvbTuneRequest.put_Locator(dvbtLocator as ILocator);

			//    dvbTuneRequest.put_ONID(channelDVBT.ONID);
			//    dvbTuneRequest.put_TSID(channelDVBT.TSID);
			//    dvbTuneRequest.put_SID(channelDVBT.SID);


			//    this.currentGraphBuilder.BuildGraphWithNoRenderer(this.objTuningSpace);
			//    this.epg.RegisterEvent(this.currentGraphBuilder.TransportInformationFilter as IConnectionPointContainer);

			//    this.currentGraphBuilder.SubmitTuneRequest(this.objTuneRequest);
			//    this.currentGraphBuilder.RunGraph();

			//    IMpeg2Data mpeg2Data = this.currentGraphBuilder.SectionsAndTables as IMpeg2Data;
			//    PSISection[] psis = PSISection.GetPSITable((int)PIDS.PAT, (int)TABLE_IDS.PAT, mpeg2Data);
			//    for (int i = 0; i < psis.Length; i++ )
			//    {
			//        ArrayList pmts = new ArrayList();

			//        PSISection psi = psis[i];
			//        if (psi != null && psi is PSIPAT)
			//        {
			//            PSIPAT pat = (PSIPAT)psi;
			//            Trace.WriteLineIf(trace.TraceVerbose, "PSI Table " + i + "/" + psis.Length + "\r\n");
			//            Trace.WriteLineIf(trace.TraceVerbose, pat.ToString());

			//            foreach (PSIPAT.Data program in pat.ProgramIds)
			//            {
			//                if (!program.IsNetworkPID)
			//                {
			//                    PSISection[] psis2 = PSISection.GetPSITable(program.Pid, (int)TABLE_IDS.PMT, mpeg2Data);
			//                    for (int i2 = 0; i2 < psis2.Length; i2++)
			//                    {
			//                        PSISection psi2 = psis2[i2];
			//                        if (psi2 != null && psi2 is PSIPMT)
			//                        {
			//                            PSIPMT pmt = (PSIPMT)psi2;
			//                            Trace.WriteLineIf(trace.TraceVerbose, "PSI Table " + i2 + "/" + psis2.Length + "\r\n");
			//                            Trace.WriteLineIf(trace.TraceVerbose, pmt.ToString());

			//                            PSIPMT.Data videoStream = pmt.GetStreamByType(STREAM_TYPES.STREAMTYPE_13818_VIDEO);
			//                            if (videoStream != null)
			//                            {
			//                                if (channelDVBT.SID == pmt.TransportStreamId)
			//                                    pmts.Insert(0, pmt);
			//                                else
			//                                    pmts.Add(pmt);
			//                            }
			//                        }
			//                    }
			//                }
			//                //if (pmts.Count == 2) break;
			//            }
			//        }

			//        this.currentGraphBuilder.StopGraph();
			//        this.currentGraphBuilder.AddMosaicFilters(this.objTuningSpace, pmts);
			//        this.currentGraphBuilder.RunGraph();
			//    }

			//    this.currentChannel = channel;
			//    this.currentModeIsMosaic = true;

			//}
		}

		internal void ClearGraph()
		{
			if (this.currentGraphBuilder != null)
			{
				//this.currentModeIsMosaic = false;
				this.currentGraphBuilder.GraphStarted -= new EventHandler(newGraph_GraphStarted);
				this.currentGraphBuilder.GraphEnded -= new EventHandler(newGraph_GraphEnded);
				this.currentGraphBuilder.PossibleChanged -= new EventHandler<GraphBuilderBase.PossibleEventArgs>(newGraph_PossibleChanged);

				this.currentGraphBuilder.Dispose();
				OnGraphStop();
				this.currentGraphBuilder = null;
			}
		}

		private void timerSignalUpdate_Tick(object sender, EventArgs e)
		{
			OnTimerGUIUpdate();
		}



		private void UpdateChannelNumberRecursedTree(ChannelFolder channelFolder)
		{
			foreach (Channel channel in channelFolder.ChannelList)
			{
				if (channel is ChannelTV)
				{
					short channelNumber = (channel as ChannelTV).ChannelNumber;
					channelNumberMax = Math.Max(channelNumberMax, channelNumber);
					if (channelNumber >= 0)
					{
						if (this.channelByChannelNumber.ContainsKey(channelNumber))
							this.cloneChannelNumber.Add(channel);
						else
							this.channelByChannelNumber[channelNumber] = channel;
					}
				}
				else if (channel is ChannelFolder)
					UpdateChannelNumberRecursedTree(channel as ChannelFolder);
			}
		}

		internal void UpdateChannelNumber()
		{
			this.channelByChannelNumber = new Hashtable();
			this.cloneChannelNumber = new ArrayList();
			this.channelNumberMax = 0;

			UpdateChannelNumberRecursedTree(this.rootChannelFolder);
		}

		private string GetTimeString(TimeSpan timeSpan)
		{
			return timeSpan.Hours.ToString("D2") + ":" + timeSpan.Minutes.ToString("D2") + ":" + timeSpan.Seconds.ToString("D2") + "." + timeSpan.Milliseconds.ToString("D3");
			//return timeSpan.ToString("HH:mm:ss.fff");
		}

		private void newGraph_GraphStarted(object sender, EventArgs e)
		{
			//OnGraphInit();
		}

		private void newGraph_GraphEnded(object sender, EventArgs e)
		{
			//OnGraphStop();
		}

		void newGraph_PossibleChanged(object sender, GraphBuilderBase.PossibleEventArgs e)
		{
			switch (e.possible)
			{
				case "IsPossibleGraphRun":
					break;
				case "IsPossibleGraphPause":
					break;
				case "IsPossibleGraphStop":
					break;
				case "IsPossibleGraphRestart":
					break;
				case "IsPossibleGraphRelease":
					break;
				case "IsPossibleTimeShiftingPause":
					this.toolStripButtonPause.Enabled = e.isPossible;
					break;
				case "IsPossibleTimeShiftingResume":
					this.toolStripButtonPlay.Enabled = e.isPossible;
					break;
				case "IsPossibleRecorderStart":
					this.toolStripButtonRecord.Enabled = e.isPossible;
					break;
				case "IsPossibleRecorderStop":
					this.toolStripButtonStop.Enabled = e.isPossible;
					break;
				case "IsPossiblePlayerPlay":
					this.toolStripButtonPlay.Enabled = e.isPossible;
					break;
				case "IsPossiblePlayerPause":
					this.toolStripButtonPause.Enabled = e.isPossible;
					break;
				case "IsPossiblePlayerStop":
					this.toolStripButtonStop.Enabled = e.isPossible;
					break;
				case "IsPossibleSetSpeed":
					this.panelTimeLine.trackBarSpeed.Enabled = e.isPossible;
					break;
				case "IsPossibleSetPosition":
					this.panelTimeLine.trackBarExTimeLine.Enabled = e.isPossible;
					break;
			}
		}

		private void OnGraphInit()
		{
			if (this.currentGraphBuilder != null)
			{
				this.panelMediaTuning.trackBarVolume_ValueChanged(null, null);
				this.panelMediaTuning.trackBarBalance_ValueChanged(null, null);
				this.currentGraphBuilder.UseVideo169Mode = Settings.UseVideo169Mode;
				this.currentGraphBuilder.VideoRefresh();
			}

			if (this.currentGraphBuilder is IEPG)
				(this.currentGraphBuilder as IEPG).EPG.GuideDataEvent += new EPG.GuideDataEventHandler(epg_GuideDataEvent);

			if (this.currentGraphBuilder is ITV)
			{
				this.toolStripButtonPreviousChannelInFolder.Enabled = this.toolStripButtonNextChannelInFolder.Enabled = true;
				this.toolStripButtonSnapshot.Enabled = true;
			}

			this.toolStripStatusLabelTimeLinePosition.Text = "";
		}

		private void OnChannelChange()
		{
			this.toolStripStatusLabelVideoStatus.Text = Properties.Resources.Ready;

			string channelName = "";
			if (this.currentGraphBuilder is ITV && (this.currentGraphBuilder as ITV).CurrentChannel != null)
			{
				Channel channel = (this.currentGraphBuilder as ITV).CurrentChannel as Channel;
				channelName = channel.Name;
				if (channel is ChannelTV && (channel as ChannelTV).ChannelNumber >= 0)
					channelName = string.Format("[{0}] {1}", (channel as ChannelTV).ChannelNumber, channelName);
			}
			else
				channelName = Properties.Resources.NoChannel;
			this.toolStripStatusLabelChannelName.Text = channelName;

			this.toolStripStatusLabelEPG.Text = "  ";
			if (this.currentGraphBuilder is IEPG)
				UpdateEPG();

			this.toolStripProgressBarSignalStrength.Value = 0;
			this.toolStripProgressBarSignalStrength.ToolTipText = "";
			this.toolStripStatusLabelSignalStrength.Text = toolStripProgressBarSignalStrength.ToolTipText;

			this.toolStripProgressBarSignalQuality.Value = 0;
			this.toolStripProgressBarSignalQuality.ToolTipText = "";
			this.toolStripStatusLabelSignalQuality.Text = toolStripProgressBarSignalQuality.ToolTipText;

			SetVideoRefreshTimer();
		}

		public void SetVideoRefreshTimer()
		{
			if (this.currentGraphBuilder != null)
			{
				this.timerVideoRefresh.Stop();
				this.currentGraphBuilder.VideoRefresh();
				this.timerVideoRefresh.Tag = 3;
				this.timerVideoRefresh.Start();
			}
		}

		private void timerVideoRefreshAfterTuningChannel_Tick(object sender, EventArgs e)
		{
			this.timerVideoRefresh.Tag = (int)this.timerVideoRefresh.Tag - 1;
			if ((int)this.timerVideoRefresh.Tag == 0)
				this.timerVideoRefresh.Stop();

			if (this.currentGraphBuilder != null)
				this.currentGraphBuilder.VideoRefresh();
		}

		private void OnTimerGUIUpdate()
		{
			//Trace.WriteLine("OnTimerGUIUpdate");

			// To avoid the GetSignalStatistics CPU usage during a GUI drag
			//if (Utils.GetCapture() != IntPtr.Zero) return;

			if (this.currentGraphBuilder is ITV)
			{
				bool locked, present;
				int strength, quality;
				if ((this.currentGraphBuilder as ITV).GetSignalStatistics(out locked, out present, out strength, out quality))
				{
					if (quality < 0)
						quality = 0;
					else if (quality > 100)
						quality = 100;

					int strengthPercentage = strength;
					if (strength < 0)
					{
						//strengthPercentage = 0;
						strengthPercentage = 100 + strength;
						if(strengthPercentage < 0)
							strengthPercentage = 0;
					}
					else if (strength <= 100)
						strengthPercentage = strength;
					else if (strength < 40000)
					{
						if (quality > 80)
						{
							strengthPercentage = Math.Min(100, strength);
						}
						else
							strengthPercentage = (int)(5.0f * (float)strength / 2000.0f);
					}
					else
						strengthPercentage = 100;

					// strength = Signal to Noise Ratio (SNR) * 1000
					// (http://en.wikipedia.org/wiki/Signal-to-noise_ratio)
					// ***(http://www.ces.clemson.edu/linux/signal_quality.shtml)

					this.toolStripProgressBarSignalStrength.Value = (int)strengthPercentage;
					this.toolStripStatusLabelSignalStrength.Text = (present ? "*" : "") + this.toolStripProgressBarSignalStrength.Value.ToString() + "%";
					this.toolStripProgressBarSignalStrength.ToolTipText = this.toolStripStatusLabelSignalStrength.Text + " (" + strength + " dB)";

					this.toolStripProgressBarSignalQuality.Value = quality;
					this.toolStripProgressBarSignalQuality.ToolTipText = (locked ? "*" : "") + quality.ToString() + "%";
					this.toolStripStatusLabelSignalQuality.Text = this.toolStripProgressBarSignalQuality.ToolTipText;
				}
			}

			if (this.currentGraphBuilder is ITimeShifting)
			{
				ITimeShifting timeShifting = (this.currentGraphBuilder as ITimeShifting);
				TimeSpan start;
				TimeSpan stop;
				timeShifting.GetPositions(out start, out stop);
				TimeSpan position = timeShifting.GetPosition();

				int minimum = 0;
				int maximum = (int)((stop - start).TotalSeconds * 1000.0);
				int pos = (int)(((position > start ? position : start) - start).TotalSeconds * 1000.0);
				if (position < start)
					timeShifting.SetPosition(start);

				if (maximum > 0 && pos < maximum)
				{
					this.panelTimeLine.trackBarExTimeLine.Minimum = minimum;
					this.panelTimeLine.trackBarExTimeLine.Maximum = maximum;
					//this.panelTimeLine.trackBarExTimeLine.Value = pos;
					this.panelTimeLine.trackBarExTimeLine.SetValue(pos);
				}

				this.toolStripStatusLabelTimeLinePosition.Text = GetTimeString(position) + " (" + GetTimeString(start) + " - " + GetTimeString(stop) + ")";
			}
			else if (this.currentGraphBuilder is IPlayer)
			{
				IPlayer player = (this.currentGraphBuilder as IPlayer);
				TimeSpan duration = player.GetDuration();
				TimeSpan position = player.GetPosition();

				int minimum = 0;
				int maximum = (int)(duration.TotalSeconds * 1000.0);
				int pos = (int)(position.TotalSeconds * 1000.0);

				if (maximum > 0 && pos < maximum)
				{
					this.panelTimeLine.trackBarExTimeLine.Minimum = minimum;
					this.panelTimeLine.trackBarExTimeLine.Maximum = maximum;
					//this.panelTimeLine.trackBarExTimeLine.Value = pos;
					this.panelTimeLine.trackBarExTimeLine.SetValue(pos);
				}

				this.toolStripStatusLabelTimeLinePosition.Text = GetTimeString(position) + " / " + GetTimeString(duration);
			}
		}

		private void OnGraphStop()
		{
			this.toolStripStatusLabelVideoStatus.Text = Properties.Resources.Ready;
			this.toolStripStatusLabelChannelName.Text = Properties.Resources.NoChannel;

			this.toolStripStatusLabelEPG.Text = "  ";

			this.toolStripProgressBarSignalStrength.Value = 0;
			this.toolStripProgressBarSignalStrength.ToolTipText = "";
			this.toolStripStatusLabelSignalStrength.Text = toolStripProgressBarSignalStrength.ToolTipText;

			this.toolStripProgressBarSignalQuality.Value = 0;
			this.toolStripProgressBarSignalQuality.ToolTipText = "";
			this.toolStripStatusLabelSignalQuality.Text = toolStripProgressBarSignalQuality.ToolTipText;

			this.toolStripButtonSnapshot.Enabled = false;

			this.toolStripButtonPause.Enabled = false;
			this.toolStripButtonPlay.Enabled = false;
			this.toolStripButtonRecord.Enabled = false;
			this.toolStripButtonStop.Enabled = false;
			this.toolStripButtonPlay.Enabled = false;
			this.toolStripButtonPause.Enabled = false;
			this.toolStripButtonStop.Enabled = false;
			this.panelTimeLine.trackBarSpeed.Enabled = false;
			this.panelTimeLine.trackBarExTimeLine.Enabled = false;
			this.panelTimeLine.trackBarExTimeLine.Value = 0;


			//if (this.currentGraphBuilder is IPlayer)
			//{
			//    //this.panelPlayer.buttonPlayerPlay.Enabled = this.panelPlayer.buttonPlayerPause.Enabled = this.panelPlayer.buttonPlayerStop.Enabled = false;
			//    //this.panelPlayer.textBoxPlayer.Text = "";
			//}

			//if (this.currentGraphBuilder is IRecorder)
			//{
			//    this.toolStripButtonTimeRecorderRecord.Enabled = false;
			//}

			//if (this.currentGraphBuilder is ITimeShifting)
			//{
			//    //this.toolStripButtonTimeShifting.Enabled = false;
			//    this.toolStripButtonTimeShiftingPause.Enabled = false;
			//    this.toolStripButtonTimeShiftingRewind.Enabled = false;
			//    this.toolStripButtonTimeShiftingFastForward.Enabled = false;
			//    this.toolStripButtonTimeRecorderRecord.Enabled = false;
			//}

			//if (this.currentGraphBuilder is IEPG)
			//{
			//    (this.currentGraphBuilder as IEPG).EPG.GuideDataEvent -= new EPG.GuideDataEventHandler(epg_GuideDataEvent);
			//}
		}

		internal void TuneChannelGUI(Channel channel)
		{
			TuneChannelGUI(channel, false);
		}

		internal void TuneChannelGUI(Channel channel, bool forceRebuild)
		{
			this.videoControl.BackColor = Color.Transparent;
			try
			{
				try
				{
					if (channel.Tag is TreeNode)
						panelChannel.treeViewChannel.SelectedNode = channel.Tag as TreeNode;
				}
				catch
				{
				}
				//if (this.currentGraphBuilder != null && !(this.currentGraphBuilder is GraphBuilderTV))
				//{
				//    OnGraphStop();
				//    this.currentGraphBuilder.Dispose();
				//    this.currentGraphBuilder = null;
				//}
				bool needRebuild = forceRebuild || !(this.currentGraphBuilder is GraphBuilderTV) ||
					(this.currentGraphBuilder as GraphBuilderTV).NeedToRebuildTheGraph(channel);
				TuneChannel(channel, needRebuild);

				videoControl.Focus();
			}
			catch (Exception ex)
			{
				ClearGraph();
				Trace.WriteLineIf(trace.TraceError, ex.ToString());
				toolStripStatusLabelVideoStatus.Text = Properties.Resources.Error + " " + ex.Message;
				toolStripStatusLabelChannelName.Text = Properties.Resources.NoChannel;
				this.videoControl.BackColor = Settings.VideoBackgroundColor;
			}
		}

		private void ChangeVideoMode(VideoMode videoMode)
		{
			switch (this.currentVideoMode)
			{
				case VideoMode.Normal:
					this.panelVideo.Controls.Remove(this.videoControl);
					break;
				case VideoMode.TV:
					this.videoForm.Owner = null;
					this.currentTVModeBounds = this.videoForm.Bounds;
					this.videoForm.Controls.Remove(this.videoControl);
					break;
				case VideoMode.Fullscreen:
					Cursor.Show();
					this.videoForm.Controls.Remove(this.videoControl);
					break;
			}
			this.currentVideoMode = videoMode;
			switch (videoMode)
			{
				case VideoMode.Normal:
					this.videoForm.Hide();
					this.videoForm.FormVideoMode = videoMode;
					this.panelVideo.Controls.Add(this.videoControl);
					//Bounds = RestoreBounds;
					WindowState = FormWindowState.Normal;
					Show();
					break;
				case VideoMode.TV:
					Hide();
					this.videoForm.SuspendLayout();
					this.videoForm.FormVideoMode = videoMode;
					this.videoForm.TopMost = true;
					this.videoForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
					if (this.currentTVModeBounds.Height != 0)
						this.videoForm.Bounds = this.currentTVModeBounds;
					else
						this.videoForm.DesktopBounds = new Rectangle(new Point(this.DesktopLocation.X + (this.Size.Width - Form.DefaultSize.Width) / 2, this.DesktopLocation.Y + (this.Size.Height - Form.DefaultSize.Height) / 2), Form.DefaultSize);
					this.videoForm.Controls.Add(this.videoControl);
					this.videoForm.ResumeLayout(false);
					this.videoForm.UpdateSize();
					this.videoForm.Show(this);
					break;
				case VideoMode.Fullscreen:
					Hide();
					Cursor.Hide();
					this.videoForm.FormVideoMode = videoMode;
					this.videoForm.TopMost = false;
					this.videoForm.FormBorderStyle = FormBorderStyle.None;
					this.videoForm.DesktopBounds = Screen.FromControl(this).Bounds;
					this.videoForm.Controls.Add(this.videoControl);
					this.videoForm.Show();
					break;
			}
			this.videoControl.Focus();
		}

		private void videoControl_Resize(object sender, EventArgs e)
		{
			if (this.currentVideoMode == VideoMode.TV)
				this.currentTVModeBounds = this.videoForm.Bounds;
		}

		void videoForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			ExecuteCommand(CommandName.VideoModeNormal);
		}

		private void videoControl_DoubleClick(object sender, EventArgs e)
		{
			ExecuteCommand((CommandName)(CommandName.VideoModeNormal + ((int)(this.currentVideoMode + 1) % Enum.GetValues(typeof(VideoMode)).Length)));
		}

		private void toolStripButtonVideoModeTV_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoModeTV);
		}

		private void toolStripButtonVideoModeFullscreen_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoModeFullscreen);
		}

		private void UpdateEPG()
		{
			Trace.WriteLineIf(EPG.trace.TraceInfo, "UpdateEPG()");

			if (this.currentGraphBuilder is IEPG)
			{
				ChannelDVBT channel = (this.currentGraphBuilder as GraphBuilderBDA).CurrentChannel as ChannelDVBT;
				if (channel != null)
				{
					EPGProgram program = (this.currentGraphBuilder as IEPG).EPG.GetCurrentProgram(channel.ONID + ":" + channel.TSID + ":" + channel.SID);
					if (program != null)
					{
						string text = program.TimeStart.ToString("t") + " - " + program.TimeEnd.ToString("t") + " " + MakeEGPTextCorrect(program.Title);
						if (program.OneSentence != null && program.OneSentence.Length > 0)
							text += " (" + MakeEGPTextCorrect(program.OneSentence) + ")";
						this.toolStripStatusLabelEPG.Text = text;
					}
				}
				else
					this.toolStripStatusLabelEPG.Text = "  ";
			}
		}

		private string MakeEGPTextCorrect(string epgText)
		{
			if (string.IsNullOrEmpty(epgText)) return "";
			// Character seems to be encoded in ISO/IEC 6937
			// http://en.wikipedia.org/wiki/ISO/IEC_6937
			//return epgText.Replace("|", "");
			return Regex.Replace(epgText, @"[\0-\31]", " ", RegexOptions.Compiled | RegexOptions.Singleline);
		}

		private void epg_GuideDataEvent(object sender, EPG.GuideDataEventArgs fe)
		{
			Trace.WriteLineIf(EPG.trace.TraceInfo, "MainForm.epg_GuideDataEvent(" + fe.Type + ", " + fe.Identifier + ")");
			if (this.currentGraphBuilder is IBDA)
			{
				IGuideData guideData = (this.currentGraphBuilder as IBDA).TransportInformationFilter as IGuideData;
				switch (fe.Type)
				{
					case EPG.GuideDataEventType.GuideDataAcquired:
						(this.currentGraphBuilder as IEPG).EPG.UpdateAll(guideData);
						UpdateEPG();
						break;
					case EPG.GuideDataEventType.ServiceChanged:
						{
							DateTime now = DateTime.Now;
							if (fe.Identifier != null || (fe.Identifier == null && (DateTime.Now - this.lastAllServiceEPGUpdate > epgUpdateTimeOut)))
							{
								this.lastAllServiceEPGUpdate = now;
								(this.currentGraphBuilder as IEPG).EPG.UpdateService(fe.Identifier, (this.currentGraphBuilder as IBDA).TuningSpace, guideData);
								UpdateEPG();
							}
						}
						break;
					case EPG.GuideDataEventType.ProgramChanged:
						{
							DateTime now = DateTime.Now;
							if (fe.Identifier != null || (fe.Identifier == null && (DateTime.Now - this.lastAllProgramEPGUpdate > epgUpdateTimeOut)))
							{
								this.lastAllProgramEPGUpdate = now;
								(this.currentGraphBuilder as IEPG).EPG.UpdateProgram(fe.Identifier, guideData);
								UpdateEPG();
							}
						}
						break;
					case EPG.GuideDataEventType.ScheduleEntryChanged:
						{
							DateTime now = DateTime.Now;

							if (fe.Identifier != null || (fe.Identifier == null && (now - this.lastAllScheduleEPGUpdate > epgUpdateTimeOut)))
							{
								this.lastAllScheduleEPGUpdate = now;
								(this.currentGraphBuilder as IEPG).EPG.UpdateSchedule(fe.Identifier, guideData);
								UpdateEPG();
							}
						}
						break;
				}
			}
		}

		void filterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem filterToolStripMenuItem = sender as ToolStripMenuItem;
			IBaseFilter filter = FilterGraphTools.FindFilterByName(this.currentGraphBuilder.FilterGraph, filterToolStripMenuItem.Text);
			if(filter != null)
			{
				FilterGraphTools.ShowFilterPropertyPage(filter, this.Handle);
				Marshal.ReleaseComObject(filter);
			}
		}

		private void openVideoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = this.GetType().Module.FullyQualifiedName;
			//openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
			openFileDialog.Filter = Properties.Resources.FileDialogAllFiles;
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				this.Cursor = Cursors.WaitCursor;

				try
				{
					ClearGraph();

					GraphBuilderPlayer newGraph = new GraphBuilderPlayer(this.videoControl);
					newGraph.GraphStarted += new EventHandler(newGraph_GraphStarted);
					newGraph.GraphEnded += new EventHandler(newGraph_GraphEnded);
					newGraph.PossibleChanged += new EventHandler<GraphBuilderBase.PossibleEventArgs>(newGraph_PossibleChanged);
					newGraph.Settings = Settings;
					newGraph.FileName = openFileDialog.FileName;
					this.currentGraphBuilder = newGraph;
					this.currentGraphBuilder.BuildGraph();
					this.currentGraphBuilder.RunGraph();

					OnGraphInit();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
				finally
				{
					this.Cursor = Cursors.Default;
				}
			}
		}

		private void runToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.currentGraphBuilder != null)
				this.currentGraphBuilder.RunGraph();
		}

		private void pauseGraphToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.currentGraphBuilder != null)
				this.currentGraphBuilder.PauseGraph();
		}

		private void stopGraphToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.currentGraphBuilder != null)
				this.currentGraphBuilder.StopGraph();
		}

		private void restartToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.currentGraphBuilder != null)
			{
				this.currentGraphBuilder.StopGraph();
				this.currentGraphBuilder.RunGraph();
			}
		}

		private void releaseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ClearGraph();
			//if (this.currentGraphBuilder != null)
			//{
			//    OnGraphStop();
			//    this.currentGraphBuilder.Dispose();
			//    this.currentGraphBuilder = null;
			//}
		}

		private void channelsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
			if (menuItem != null)
			{
				ToolStripMenuItem toolStripMenuItem = menuItem;
				ChannelFolder channelFolder = this.rootChannelFolder;

				PopulateChannelsInDropDownMenu(toolStripMenuItem, channelFolder);
			}
		}

		private void PopulateChannelsInDropDownMenu(ToolStripMenuItem toolStripMenuItem, ChannelFolder channelFolder)
		{
			toolStripMenuItem.DropDownItems.Clear();

			foreach (Channel channel in channelFolder.ChannelList)
			{
				ToolStripMenuItem channelToolStripMenuItem = new ToolStripMenuItem(channel.Name);
				channelToolStripMenuItem.Tag = channel;
				if (channel is ChannelFolder)
				{
					channelToolStripMenuItem.Image = this.imageListLogoTV.Images["FolderClosed"];
					channelToolStripMenuItem.DropDownItems.Add("dummy");
					channelToolStripMenuItem.DropDownOpening += new EventHandler(channelToolStripMenuItem_DropDownOpening);
				}
				else
				{
					if (channel is ChannelTV)
					{
						channelToolStripMenuItem.Image = this.imageListLogoTV.Images[(channel as ChannelTV).Logo];
						if(channelToolStripMenuItem.Image == null)
							channelToolStripMenuItem.Image = this.imageListLogoTV.Images["LogoTVDefault"];
					}
					channelToolStripMenuItem.Click += new EventHandler(channelToolStripMenuItem_Click);
				}
				toolStripMenuItem.DropDownItems.Add(channelToolStripMenuItem);
			}
		}

		void channelToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
			ChannelFolder channelFolder = toolStripMenuItem.Tag as ChannelFolder;

			PopulateChannelsInDropDownMenu(toolStripMenuItem, channelFolder);
		}

		void channelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
			if (toolStripMenuItem.Tag is ChannelTV)
			{
				ChannelTV channel = toolStripMenuItem.Tag as ChannelTV;
				TuneChannelGUI((Channel)channel);
			}
		}

		private void graphToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			if (this.currentGraphBuilder != null)
			{
				switch (this.currentGraphBuilder.GetGraphState())
				{
					case FilterState.Running:
						this.runToolStripMenuItem.Enabled = false;
						this.pauseGraphToolStripMenuItem.Enabled = true;
						this.stopGraphToolStripMenuItem.Enabled = true;
						this.restartToolStripMenuItem.Enabled = true;
						break;
					case FilterState.Paused:
						this.runToolStripMenuItem.Enabled = true;
						this.pauseGraphToolStripMenuItem.Enabled = false;
						this.stopGraphToolStripMenuItem.Enabled = true;
						this.restartToolStripMenuItem.Enabled = true;
						break;
					case FilterState.Stopped:
						this.runToolStripMenuItem.Enabled = true;
						this.pauseGraphToolStripMenuItem.Enabled = false;
						this.stopGraphToolStripMenuItem.Enabled = false;
						this.restartToolStripMenuItem.Enabled = true;
						break;
				}
			}
			else
			{
				this.runToolStripMenuItem.Enabled = false;
				this.pauseGraphToolStripMenuItem.Enabled = false;
				this.stopGraphToolStripMenuItem.Enabled = false;
				this.restartToolStripMenuItem.Enabled = false;
			}
		}

		private void filtersToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			ToolStripMenuItem filtersMenuItem = sender as ToolStripMenuItem;
			if (filtersMenuItem != null)
			{
				filtersMenuItem.DropDownItems.Clear();

				if (this.currentGraphBuilder != null)
				{
					IEnumFilters enumFilters = null;
					int hr = this.currentGraphBuilder.FilterGraph.EnumFilters(out enumFilters);
					if (hr == 0)
					{
						IBaseFilter[] filters = new IBaseFilter[1];
						//22 int fetched;
						//22 while (enumFilters.Next(filters.Length, filters, out fetched) == 0)
						while (enumFilters.Next(filters.Length, filters, IntPtr.Zero) == 0)
						{
							FilterInfo filterInfo;

							hr = filters[0].QueryFilterInfo(out filterInfo);
							if (hr == 0)
							{
								if (filterInfo.pGraph != null)
									Marshal.ReleaseComObject(filterInfo.pGraph);

								if (filterInfo.achName != null)
								{
									ToolStripMenuItem filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem(filterInfo.achName);
									filterToolStripMenuItem.Enabled = FilterGraphTools.HasPropertyPages(filters[0]);
									filterToolStripMenuItem.Click += new EventHandler(filterToolStripMenuItem_Click);
									filtersMenuItem.DropDownItems.Add(filterToolStripMenuItem);
								}
							}

							Marshal.ReleaseComObject(filters[0]);
						}
						Marshal.ReleaseComObject(enumFilters);
					}
				}
			}
		}

		private void contextMenuStripPanelVideo_Opened(object sender, EventArgs e)
		{
			if(this.currentVideoMode == VideoMode.Fullscreen)
				Cursor.Show();
		}

		private void contextMenuStripPanelVideo_Closed(object sender, ToolStripDropDownClosedEventArgs e)
		{
			if (this.currentVideoMode == VideoMode.Fullscreen)
				Cursor.Hide();
		}

		private void contextMenuStripPanelVideo_Opening(object sender, CancelEventArgs e)
		{
			ToolStripManager.RevertMerge(toolStripMain, contextMenuStripPanelVideo);

			MenuVideoUpdate();
		}

		private void MenuVideoUpdate()
		{
			if (this.currentGraphBuilder != null)
			{
				this.graphToolStripMenuItem.Enabled = true;
				this.filtersToolStripMenuItem.Enabled = true;
				this.videoToolStripMenuItem.Enabled = true;
			}
			else
			{
				this.graphToolStripMenuItem.Enabled = false;
				this.filtersToolStripMenuItem.Enabled = false;
				this.videoToolStripMenuItem.Enabled = false;
			}
		}

		private void freeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoZoomFreeMode);
		}

		private void touchWindowFromInsideToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoZoomFromInside);
		}

		private void touchWindowFromOutsideToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoZoomFromOutside);
		}

		private void strechToWindowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoZoomStretchToWindow);
		}

		private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoZoomIncrease);
		}

		private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoZoomDecrease);
		}

		private void zoom50ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoZoomHalf);
		}

		private void zoom100ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoZoomNormal);
		}

		private void zoom200ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoZoomDouble);
		}

		private void moveLeftToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoMoveLeft);
		}

		private void moveRightToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoMoveRight);
		}

		private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoMoveUp);
		}

		private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoMoveDown);
		}

		private void moveCenterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoCenter);
		}

		private void increaseAspectRatioToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoIncreaseAspectRatio);
		}

		private void decreaseAspectRatioToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoDecreaseAspectRatio);
		}

		private void resetAspectRatioToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.VideoResetAspectRatio);
		}

		private void toolStripButtonPreviousChannelInFolder_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.ChannelPreviousInFolder);
		}

		private void toolStripButtonNextChannelInFolder_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.ChannelNextInFolder);
		}

		private void toolStripButtonPreviousChannel_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.ChannelPrevious);
		}

		private void toolStripButtonNextChannel_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.ChannelNext);
		}

		private void winLIRC_CommandReceived(object sender, WinLIRC.CommandReceivedEventArgs e)
		{
			string commandNameKey = e.RemoteCommandName + "-" + e.IrCommand;
			if (Settings.WinLIRCCommandMapping.ContainsKey(commandNameKey))
			{
				CommandName commandName = (CommandName)Settings.WinLIRCCommandMapping[commandNameKey];
				ExecuteCommand(commandName, e.Repeat);
			}
		}

		void videoControl_KeyDown(object sender, KeyEventArgs e)
		{
			//e.Handled = true;

			//timerRepeatKey.Start();

			if (!e.Control)
			{
				switch (e.KeyCode)
				{
					case Keys.M: ExecuteCommand(CommandName.ToggleMenu); break;
					case Keys.Return: ExecuteCommand(CommandName.Ok); break;
					case Keys.Escape: ExecuteCommand(CommandName.Cancel); break;
					case Keys.Left: ExecuteCommand(CommandName.Left); break;
					case Keys.Right: ExecuteCommand(CommandName.Right); break;
					case Keys.Up: ExecuteCommand(CommandName.Up); break;
					case Keys.Down: ExecuteCommand(CommandName.Down); break;
					case Keys.Subtract: ExecuteCommand(CommandName.ChannelPrevious); break;
					case Keys.Add: ExecuteCommand(CommandName.ChannelNext); break;
					case Keys.NumPad0: ExecuteCommand(CommandName.Key0); break;
					case Keys.NumPad1: ExecuteCommand(CommandName.Key1); break;
					case Keys.NumPad2: ExecuteCommand(CommandName.Key2); break;
					case Keys.NumPad3: ExecuteCommand(CommandName.Key3); break;
					case Keys.NumPad4: ExecuteCommand(CommandName.Key4); break;
					case Keys.NumPad5: ExecuteCommand(CommandName.Key5); break;
					case Keys.NumPad6: ExecuteCommand(CommandName.Key6); break;
					case Keys.NumPad7: ExecuteCommand(CommandName.Key7); break;
					case Keys.NumPad8: ExecuteCommand(CommandName.Key8); break;
					case Keys.NumPad9: ExecuteCommand(CommandName.Key9); break;
					case Keys.PageUp: ExecuteCommand(CommandName.VolumePlus); break;
					case Keys.PageDown: ExecuteCommand(CommandName.VolumeMinus); break;
					case Keys.Space: ExecuteCommand(CommandName.SnapShot); break;
				}
			}
			else if (e.Control)
			{	// Video zoom and aspect ratio
				switch (e.KeyCode)
				{
					case Keys.Enter: ExecuteCommand(CommandName.VideoReset); break;
					case Keys.Subtract: ExecuteCommand(CommandName.VideoZoomHalf); break;
					case Keys.NumPad0: ExecuteCommand(CommandName.VideoZoomNormal); break;
					case Keys.Add: ExecuteCommand(CommandName.VideoZoomDouble); break;
					case Keys.Divide: ExecuteCommand(CommandName.VideoZoomFromInside); break;
					case Keys.Multiply: ExecuteCommand(CommandName.VideoZoomFromOutside); break;
					case Keys.NumPad7: ExecuteCommand(CommandName.VideoZoomIncrease); break;
					case Keys.NumPad1: ExecuteCommand(CommandName.VideoZoomDecrease); break;
					case Keys.Decimal: ExecuteCommand(CommandName.VideoResetAspectRatio); break;
					case Keys.NumPad9: ExecuteCommand(CommandName.VideoIncreaseAspectRatio); break;
					case Keys.NumPad3: ExecuteCommand(CommandName.VideoDecreaseAspectRatio); break;

					case Keys.NumPad4: ExecuteCommand(CommandName.VideoMoveLeft); break;
					case Keys.NumPad6: ExecuteCommand(CommandName.VideoMoveRight); break;
					case Keys.NumPad8: ExecuteCommand(CommandName.VideoMoveUp); break;
					case Keys.NumPad2: ExecuteCommand(CommandName.VideoMoveDown); break;
					case Keys.NumPad5: ExecuteCommand(CommandName.VideoCenter); break;
				}
			}
		}

		void videoControl_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta > 0)
				ExecuteCommand(CommandName.VolumePlus);
			else if (e.Delta < 0)
				ExecuteCommand(CommandName.VolumeMinus);
		}

		public void ExecuteCommand(CommandName commandName)
		{
			ExecuteCommand(commandName, 0);
		}

		public void ExecuteCommand(CommandName commandName, int repeat)
		{
			if (repeat == 0)
			{
				switch (commandName)
				{
					case CommandName.Nop:
						break;
					case CommandName.ToggleMenu:
						break;
					case CommandName.Ok:
						break;
					case CommandName.Cancel:
						if (Settings.StartVideoMode == VideoMode.Fullscreen)
							ExecuteCommand(CommandName.VideoModeNormal);
						break;
					case CommandName.Left:
						break;
					case CommandName.Right:
						break;
					case CommandName.Up:
						break;
					case CommandName.Down:
						break;
					case CommandName.Key0: BuildChannelNumber(0); break;
					case CommandName.Key1: BuildChannelNumber(1); break;
					case CommandName.Key2: BuildChannelNumber(2); break;
					case CommandName.Key3: BuildChannelNumber(3); break;
					case CommandName.Key4: BuildChannelNumber(4); break;
					case CommandName.Key5: BuildChannelNumber(5); break;
					case CommandName.Key6: BuildChannelNumber(6); break;
					case CommandName.Key7: BuildChannelNumber(7); break;
					case CommandName.Key8: BuildChannelNumber(8); break;
					case CommandName.Key9: BuildChannelNumber(9); break;
					case CommandName.VolumePlus:
						this.panelMediaTuning.trackBarVolume.Value = Math.Min(this.panelMediaTuning.trackBarVolume.Value + 250, 0);
						break;
					case CommandName.VolumeMinus:
						this.panelMediaTuning.trackBarVolume.Value = Math.Max(this.panelMediaTuning.trackBarVolume.Value - 250, -10000);
						break;
					case CommandName.ChannelNextInFolder:
						{
							GraphBuilderTV currentGraph = this.currentGraphBuilder as GraphBuilderTV;
							if (currentGraph != null)
							{
								Channel channel = currentGraph.CurrentChannel;
								if (channel != null)
								{
									ChannelFolder parentChannel = channel.Parent;
									int pos = parentChannel.ChannelList.IndexOf(channel);
									if (pos >= 0 && pos < parentChannel.ChannelList.Count - 1)
									{
										Channel newChannel = parentChannel.ChannelList[pos + 1];
										if (newChannel is ChannelTV)
											TuneChannelGUI(newChannel);
									}
								}
							}
						}
						break;
					case CommandName.ChannelPreviousInFolder:
						{
							GraphBuilderTV currentGraph = this.currentGraphBuilder as GraphBuilderTV;
							if (currentGraph != null)
							{
								Channel channel = currentGraph.CurrentChannel;
								if (channel != null)
								{
									ChannelFolder parentChannel = channel.Parent;
									int pos = parentChannel.ChannelList.IndexOf(channel);
									if (pos > 0)
									{
										Channel newChannel = parentChannel.ChannelList[pos - 1];
										if (newChannel is ChannelTV)
											TuneChannelGUI(newChannel);
									}
								}
							}
						}
						break;
					case CommandName.ChannelNext:
						{
							short currentNumber = (short)-1;
							bool channelFound = false;
							GraphBuilderTV currentGraph = this.currentGraphBuilder as GraphBuilderTV;
							if (currentGraph != null)
							{
								ChannelTV channel = currentGraph.CurrentChannel as ChannelTV;
								if (channel != null && channel.ChannelNumber >= 0)
								{
									currentNumber = channel.ChannelNumber;
									channelFound = true;
								}
							}
							if (!channelFound)
							{
								if (panelChannel.treeViewChannel.SelectedNode != null)
								{
									ChannelTV channel = panelChannel.treeViewChannel.SelectedNode.Tag as ChannelTV;
									if (channel != null && channel.ChannelNumber >= 0)
									{
										currentNumber = channel.ChannelNumber;
										channelFound = true;
									}
								}
							}
							ChannelTV newChannel = null;
							while ((newChannel = this.channelByChannelNumber[++currentNumber] as ChannelTV) == null && currentNumber <= channelNumberMax) ;

							if (newChannel is ChannelTV)
								TuneChannelGUI(newChannel);
						}
						break;
					case CommandName.ChannelPrevious:
						{
							short currentNumber = (short)(channelNumberMax + 1);
							bool channelFound = false;
							GraphBuilderTV currentGraph = this.currentGraphBuilder as GraphBuilderTV;
							if (currentGraph != null)
							{
								ChannelTV channel = currentGraph.CurrentChannel as ChannelTV;
								if (channel != null && channel.ChannelNumber >= 0)
								{
									currentNumber = channel.ChannelNumber;
									channelFound = true;
								}
							}
							if (!channelFound)
							{
								if (panelChannel.treeViewChannel.SelectedNode != null)
								{
									ChannelTV channel = panelChannel.treeViewChannel.SelectedNode.Tag as ChannelTV;
									if (channel != null && channel.ChannelNumber >= 0)
									{
										currentNumber = channel.ChannelNumber;
										channelFound = true;
									}
								}
							}
							ChannelTV newChannel = null;
							while ((newChannel = this.channelByChannelNumber[--currentNumber] as ChannelTV) == null && currentNumber >= 0);

							if (newChannel is ChannelTV)
								TuneChannelGUI(newChannel);
						}
						break;
					case CommandName.MediaPlay:

						if (this.currentGraphBuilder is ITimeShifting)
							(this.currentGraphBuilder as ITimeShifting).Resume();
						else if (this.currentGraphBuilder is IPlayer)
							(this.currentGraphBuilder as IPlayer).Play();

						break;
					case CommandName.MediaPause:

						if (this.currentGraphBuilder is IBDA)
						{
							if (!(this.currentGraphBuilder is ITimeShifting) && !Settings.TimeShiftingActivated)
							{
								Settings.TimeShiftingActivated = true;
								//this.graphBuilderType = GraphBuilderType.BDATimeShifting;
								//this.toolStripButtonTimeShifting.Checked = true;

								//ChannelDVB currentChannelDVB = (this.currentGraphBuilder as ITV).CurrentChannel as ChannelDVB;
								//if (currentChannelDVB != null)
								//    TuneChannelGUI(currentChannelDVB, true);
							}

							if (this.currentGraphBuilder is ITimeShifting)
							{
								if ((this.currentGraphBuilder as ITimeShifting).Status == TimeShiftingStatus.Recording)
									(this.currentGraphBuilder as ITimeShifting).Pause();
								else if ((this.currentGraphBuilder as ITimeShifting).Status == TimeShiftingStatus.Paused)
									(this.currentGraphBuilder as ITimeShifting).Resume();
							}
						}
						else if (this.currentGraphBuilder is IPlayer)
							(this.currentGraphBuilder as IPlayer).Pause();

						break;
					case CommandName.MediaStop:

						if (this.currentGraphBuilder is IRecorder)
						{
							IRecorder recorder = this.currentGraphBuilder as IRecorder;
							if (recorder.Status == RecorderStatus.Recording)
							{
								recorder.Stop();
								break;
							}
						}
						else if (this.currentGraphBuilder is ITimeShifting && Settings.TimeShiftingActivated)
						{
							Settings.TimeShiftingActivated = false;
							//this.graphBuilderType = GraphBuilderType.BDA;
							//this.toolStripButtonTimeShifting.Checked = false;

							//ChannelDVB currentChannelDVB = (this.currentGraphBuilder as ITV).CurrentChannel as ChannelDVB;
							//if (currentChannelDVB != null)
							//    TuneChannelGUI(currentChannelDVB, true);
						}
						else //if (this.currentGraphBuilder is IPlayer)
							//(this.currentGraphBuilder as IPlayer).Stop();
							ClearGraph();

						break;
					case CommandName.MediaRecord:

						if (this.currentGraphBuilder is IBDA)
						{
							if (!(this.currentGraphBuilder is IRecorder) && !Settings.TimeShiftingActivated)
							{
								Settings.TimeShiftingActivated = true;
								//this.graphBuilderType = GraphBuilderType.BDATimeShifting;
								//this.toolStripButtonTimeShifting.Checked = true;

								//ChannelDVB currentChannelDVB = (this.currentGraphBuilder as ITV).CurrentChannel as ChannelDVB;
								//if (currentChannelDVB != null)
								//    TuneChannelGUI(currentChannelDVB, true);
							}

							if (this.currentGraphBuilder is IRecorder)
							{
								IRecorder recorder = this.currentGraphBuilder as IRecorder;
								if (recorder.Status == RecorderStatus.Stopped)
								{
									string filename = DateTime.Now.ToString(Properties.Resources.VideoRecorderTimeFormat);
									if (this.currentGraphBuilder is ITV)
									{
										ITV tv = this.currentGraphBuilder as ITV;
										if (tv.CurrentChannel != null)
											filename += " " + tv.CurrentChannel.Name;
									}

									if (!Directory.Exists(Settings.VideosFolder))
										Directory.CreateDirectory(Settings.VideosFolder);

									filename += ".dvr-ms";
									recorder.Start(Settings.VideosFolder + "\\" + filename);

									toolStripStatusLabelVideoStatus.Text = string.Format(Properties.Resources.RecordingInFile, filename);
								}
								else if (recorder.Status == RecorderStatus.Recording)
								{
									recorder.Stop();
								}
							}
						}

						break;
					case CommandName.MediaRewind:
						break;
					case CommandName.MediaFastForward:
						break;
					case CommandName.VideoReset:
					case CommandName.VideoZoomHalf:
					case CommandName.VideoZoomNormal:
					case CommandName.VideoZoomDouble:
					case CommandName.VideoZoomFreeMode:
					case CommandName.VideoZoomFromInside:
					case CommandName.VideoZoomFromOutside:
					case CommandName.VideoZoomStretchToWindow:
					case CommandName.VideoZoomIncrease:
					case CommandName.VideoZoomDecrease:
					case CommandName.VideoResetAspectRatio:
					case CommandName.VideoIncreaseAspectRatio:
					case CommandName.VideoDecreaseAspectRatio:
					case CommandName.VideoCenter:
					case CommandName.VideoMoveLeft:
					case CommandName.VideoMoveRight:
					case CommandName.VideoMoveUp:
					case CommandName.VideoMoveDown:
						if (this.currentGraphBuilder != null)
						{
							int x = 0, y = 0;
							int dx = 0, dy = 0;

							PointF videoOffset = this.currentGraphBuilder.VideoOffset;
							double videoZoom = this.currentGraphBuilder.VideoZoom;
							double videoAspectRatioFactor = this.currentGraphBuilder.VideoAspectRatioFactor;

							switch (commandName)
							{
								case CommandName.VideoReset:
									videoOffset = new PointF(0.5f, 0.5f);
									videoZoom = 1.0;
									videoAspectRatioFactor = 1.0;
									break;
								case CommandName.VideoZoomHalf:
									this.currentGraphBuilder.VideoZoomMode = VideoSizeMode.Free;
									videoZoom = 0.5;
									break;
								case CommandName.VideoZoomNormal:
									this.currentGraphBuilder.VideoZoomMode = VideoSizeMode.Free;
									videoZoom = 1.0;
									break;
								case CommandName.VideoZoomDouble:
									this.currentGraphBuilder.VideoZoomMode = VideoSizeMode.Free;
									videoZoom = 2.0;
									break;
								case CommandName.VideoZoomFreeMode:
									this.currentGraphBuilder.VideoZoomMode = VideoSizeMode.Free;
									break;
								case CommandName.VideoZoomFromInside:
									this.currentGraphBuilder.VideoZoomMode = VideoSizeMode.FromInside;
									videoOffset = new PointF(0.5f, 0.5f);
									videoZoom = 1.0;
									break;
								case CommandName.VideoZoomFromOutside:
									this.currentGraphBuilder.VideoZoomMode = VideoSizeMode.FromOutside;
									videoOffset = new PointF(0.5f, 0.5f);
									videoZoom = 1.0;
									break;
								case CommandName.VideoZoomStretchToWindow:
									this.currentGraphBuilder.VideoZoomMode = VideoSizeMode.StretchToWindow;
									break;
								case CommandName.VideoZoomIncrease: x = 1; break;
								case CommandName.VideoZoomDecrease: x = -1; break;
								case CommandName.VideoResetAspectRatio:
									videoAspectRatioFactor = 1.0;
									break;
								case CommandName.VideoIncreaseAspectRatio: y = 1; break;
								case CommandName.VideoDecreaseAspectRatio: y = -1; break;
								case CommandName.VideoCenter: videoOffset = new PointF(0.5f, 0.5f); break;
								case CommandName.VideoMoveLeft: dx = -1; break;
								case CommandName.VideoMoveRight: dx = 1; break;
								case CommandName.VideoMoveUp: dy = -1; break;
								case CommandName.VideoMoveDown: dy = 1; break;
								default: break;
							}

							if (x > 0 && videoZoom < 3f)
								videoZoom *= 1.02f;
							if (x < 0 && videoZoom > 0.2f)
								videoZoom /= 1.02f;
							if (y > 0 && videoAspectRatioFactor < 3f)
								videoAspectRatioFactor *= 1.02f;
							if (y < 0 && videoAspectRatioFactor > 0.2f)
								videoAspectRatioFactor /= 1.02f;

							if (dx < 0 && videoOffset.X > 0f)
								videoOffset.X = (float)Math.Max((double)videoOffset.X - 0.005 * videoZoom, 0.0);
							if (dx > 0 && videoOffset.X < 1f)
								videoOffset.X = (float)Math.Min((double)videoOffset.X + 0.005 * videoZoom, 1.0);
							if (dy < 0 && videoOffset.Y > 0f)
								videoOffset.Y = (float)Math.Max((double)videoOffset.Y - 0.005 * videoZoom, 0.0);
							if (dy > 0 && videoOffset.Y < 1f)
								videoOffset.Y = (float)Math.Min((double)videoOffset.Y + 0.005 * videoZoom, 1.0);

							this.currentGraphBuilder.VideoOffset = videoOffset;
							this.currentGraphBuilder.VideoZoom = videoZoom;
							this.currentGraphBuilder.VideoAspectRatioFactor = videoAspectRatioFactor;

							this.currentGraphBuilder.VideoRefresh();
						}
						break;
					case CommandName.VideoModeNormal:
						ChangeVideoMode(VideoMode.Normal);
						break;
					case CommandName.VideoModeTV:
						ChangeVideoMode(VideoMode.TV);
						break;
					case CommandName.VideoModeFullscreen:
						ChangeVideoMode(VideoMode.Fullscreen);
						break;
					case CommandName.SnapShot:
						IVMRWindowlessControl9 vmrWindowlessControl9 = null;
						if (this.currentGraphBuilder != null)
							vmrWindowlessControl9 = this.currentGraphBuilder.VideoRenderer as IVMRWindowlessControl9;
						if (vmrWindowlessControl9 != null)
						{
							IntPtr lpDib;
							int hr = vmrWindowlessControl9.GetCurrentImage(out lpDib);
							if (hr >= 0)
							{
								try
								{
									BitmapInfoHeader bih = new BitmapInfoHeader();
									Marshal.PtrToStructure(lpDib, bih);
									Bitmap bitmap = new Bitmap(bih.Width, bih.Height, PixelFormat.Format32bppRgb);
									Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
									BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

									int sourceBitsSize = bih.ImageSize;
									byte[] rgbValues = new byte[sourceBitsSize];
									int ptrIntBits = (int)lpDib + bih.Size;
									IntPtr ptrBits = (IntPtr)ptrIntBits;
									Marshal.Copy(ptrBits, rgbValues, 0, sourceBitsSize);
									Marshal.Copy(rgbValues, 0, bmpData.Scan0, sourceBitsSize);

									//unsafe
									//{
									//    int* sourceBits = (int*)lpDib.ToPointer();
									//    int* destinationBits = (int*)bmpData.Scan0.ToPointer();
									//    sourceBits += Marshal.SizeOf(typeof(BitmapInfoHeader)) / 4;
									//    for (int i = 0; i < bih.ImageSize; i += 4)
									//        *destinationBits++ = *sourceBits++;
									//}

									bitmap.UnlockBits(bmpData);

									// If the image is upsidedown
									bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

									if (!Directory.Exists(Settings.SnapshotsFolder))
										Directory.CreateDirectory(Settings.SnapshotsFolder);

									string path = Settings.SnapshotsFolder + '\\';
									string filename = DateTime.Now.ToString(Properties.Resources.SnapShotTimeFormat);
									if (this.currentGraphBuilder is ITV)
										filename += " " + MakeCorrectFilename((this.currentGraphBuilder as ITV).CurrentChannel.Name);
									filename += ".png";

									try
									{
										bitmap.Save(path + filename, ImageFormat.Png);
										toolStripStatusLabelVideoStatus.Text = string.Format(Properties.Resources.SnapshotSaved, filename);
									}
									catch(Exception)
									{
										MessageBox.Show(string.Format(Properties.Resources.SnapshotNotSaved, path + filename));
									}
								}
								finally
								{
									Marshal.FreeCoTaskMem(lpDib);
								}
							}
						}
						break;
				}
			}
		}

		internal string MakeCorrectFilename(string filename)
		{
			// Filename cannot contains:  < > : " / \ |
			return Regex.Replace(filename, @"[\0-\31|<|>|:|\""|/|\\|\|]", " ", RegexOptions.Compiled | RegexOptions.Singleline);
		}

		private void BuildChannelNumber(int channelDigit)
		{
			this.timerChannelNumberBuilder.Stop();
			//if (channelDigit == 0)

			this.channelNumberBuilder += channelDigit;
			this.timerChannelNumberBuilder.Start();
		}

		private void timerChannelNumberBuilder_Tick(object sender, EventArgs e)
		{
			this.timerChannelNumberBuilder.Stop();

			Trace.WriteLineIf(trace.TraceInfo, "timerChannelNumberBuilder_Tick channelNumberBuilder: " + this.channelNumberBuilder);
			short channelNumber = -1;
			try
			{
				channelNumber = short.Parse(this.channelNumberBuilder);
			}
			catch
			{
			}
			if (channelNumber != -1)
			{
				this.channelNumberBuilder = "";
				if (this.channelByChannelNumber.ContainsKey(channelNumber))
					TuneChannelGUI(this.channelByChannelNumber[channelNumber] as ChannelTV);
			}
		}

		private void aspecRatio169toolStripMenuItem_Click(object sender, EventArgs e)
		{
			Settings.UseVideo169Mode = aspecRatio169toolStripMenuItem.Checked;
			ChangeSetting("UseVideo169Mode", true);
		}

		private void toolStripButtonTimeShifting_Click(object sender, EventArgs e)
		{
			Settings.TimeShiftingActivated = toolStripButtonTimeShifting.Checked;
			ChangeSetting("TimeShiftingActivated", true);
		}

		private void toolStripButtonPlay_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.MediaPlay);
		}

		private void toolStripButtonStop_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.MediaStop);
		}

		private void toolStripButtonPause_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.MediaPause);
		}

		private void toolStripButtonRewind_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.MediaRewind);
		}

		private void toolStripButtonFastForward_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.MediaFastForward);
		}

		private void toolStripButtonRecord_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.MediaRecord);
		}

		private void videoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			switch (this.currentGraphBuilder.VideoZoomMode)
			{
				case VideoSizeMode.Free:
					this.freeToolStripMenuItem.Checked = true;
					this.touchWindowFromInsideToolStripMenuItem.Checked = false;
					this.touchWindowFromOutsideToolStripMenuItem.Checked = false;
					this.strechToWindowToolStripMenuItem.Checked = false;
					break;
				case VideoSizeMode.FromInside:
					this.freeToolStripMenuItem.Checked = false;
					this.touchWindowFromInsideToolStripMenuItem.Checked = true;
					this.touchWindowFromOutsideToolStripMenuItem.Checked = false;
					this.strechToWindowToolStripMenuItem.Checked = false;
					break;
				case VideoSizeMode.FromOutside:
					this.freeToolStripMenuItem.Checked = false;
					this.touchWindowFromInsideToolStripMenuItem.Checked = false;
					this.touchWindowFromOutsideToolStripMenuItem.Checked = true;
					this.strechToWindowToolStripMenuItem.Checked = false;
					break;
				case VideoSizeMode.StretchToWindow:
					this.freeToolStripMenuItem.Checked = false;
					this.touchWindowFromInsideToolStripMenuItem.Checked = false;
					this.touchWindowFromOutsideToolStripMenuItem.Checked = false;
					this.strechToWindowToolStripMenuItem.Checked = true;
					break;
			}
		}

		//int BlendApplicationImage(IntPtr hwndApp, IVMRWindowlessControl pWc, HBITMAP hbm)
		//{
		//int cx, cy, arWidth, arHeight;
		//hr = pWc.GetNativeVideoSize(out cx, out cy, out arWidth, out arHeight);
		//if (hr < 0)
		//    return hr;

		//HDC hdc = GetDC(hwndApp);
		//if (hdc == NULL)
		//{
		//    return E_FAIL;
		//}

		//HDC hdcBmp = CreateCompatibleDC(hdc);
		//ReleaseDC(hwndApp, hdc);

		//if (hdcBmp == NULL)
		//{
		//    return E_FAIL;
		//}

		//BITMAP bm;
		//if (0 == GetObject(hbm, sizeof(bm), &bm))
		//{
		//    DeleteDC(hdcBmp);
		//    return E_FAIL;
		//}

		//HBITMAP hbmOld = (HBITMAP)SelectObject(hdcBmp, hbm);
		//if (hbmOld == 0)
		//{
		//    DeleteDC(hdcBmp);
		//    return E_FAIL;
		//}

		//VMRALPHABITMAP bmpInfo;
		//ZeroMemory(&bmpInfo, sizeof(bmpInfo));
		//bmpInfo.dwFlags = VMRBITMAP_HDC;
		//bmpInfo.hdc = hdcBmp;

		//// Show the entire bitmap in the top-left corner of the video image.
		//SetRect(&bmpInfo.rSrc, 0, 0, bm.bmWidth, bm.bmHeight);
		//bmpInfo.rDest.left = 0.f;
		//bmpInfo.rDest.top = 0.f;
		//bmpInfo.rDest.right = (float)bm.bmWidth / (float)cx;
		//bmpInfo.rDest.bottom = (float)bm.bmHeight / (float)cy;

		//// Set the transparency value (1.0 is opaque, 0.0 is transparent).
		//bmpInfo.fAlpha = 0.2f;

		//IVMRMixerBitmap* pBmp;
		//hr = pWc->QueryInterface(IID_IVMRMixerBitmap, (LPVOID*)&pBmp);
		//if (SUCCEEDED(hr))
		//{
		//    pBmp->SetAlphaBitmap(&bmpInfo);
		//    pBmp->Release();
		//}

		//DeleteObject(SelectObject(hdcBmp, hbmOld));
		//DeleteDC(hdcBmp);
		//    return hr;
		//}

		private void toolStripButtonTest_Click(object sender, EventArgs e)
		{
			this.currentGraphBuilder.StartOSD();
			//GraphBuilderBDA graphBuilderBDA = this.currentGraphBuilder as GraphBuilderBDA;
			//IMediaFilter mf = graphBuilderBDA.FilterGraph as IMediaFilter;
			//IReferenceClock rc;
			//mf.GetSyncSource(out rc);

			//IReferenceClock arc = graphBuilderBDA.AudioRenderer as IReferenceClock;
			//IReferenceClock drc = graphBuilderBDA.Demultiplexer as IReferenceClock;
			//if (rc == arc)
			//    MessageBox.Show("AudioRenderer");
			//else if (rc == drc)
			//    MessageBox.Show("Demultiplexer");
			//else if (rc == null)
			//    MessageBox.Show("Null");
		}

		private void toolStripButtonTest2_Click(object sender, EventArgs e)
		{
			this.currentGraphBuilder.StopOSD();

			//GraphBuilderBDA graphBuilderBDA = this.currentGraphBuilder as GraphBuilderBDA;
			//IMediaFilter mf = graphBuilderBDA.FilterGraph as IMediaFilter;
			//IReferenceClock arc = graphBuilderBDA.AudioRenderer as IReferenceClock;
			//int hr = mf.SetSyncSource(arc);

			//IReferenceClock rc2;
			//mf.GetSyncSource(out rc2);

			////mf.GetSyncSource(
			////IReferenceClock clock = this.audioRenderer as IReferenceClock;
			////if(clock != null)
			////{
			////    // Set the graph clock.
			////    this.FilterGraph.SetDefaultSyncSource( 
			////    pGraph->QueryInterface(IID_IMediaFilter, (void**)&pMediaFilter);
			////    pMediaFilter->SetSyncSource(pClock);
			////    pClock->Release();
			////    pMediaFilter->Release();
			////}

			////this.FilterGraph.SetDefaultSyncSource();

			////IReferenceClock clock = this.audioRenderer as IReferenceClock;
			////if(clock != null)
			////{
			////    IMediaFilter mediaFilter = this.FilterGraph as IMediaFilter;
			////    mediaFilter.SetSyncSource(clock);
			////}

		}

		private void channelPanelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.panelChannel.Show(this.dockPanel);
		}

		private void channelPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.panelChannelProperties.Show(this.dockPanel);
		}

		private void channelInfosToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.panelInfos.Show(this.dockPanel);
		}

		private void mediaTuningToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.panelMediaTuning.Show(this.dockPanel);
		}

		private void timeLineToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.panelTimeLine.Show(this.dockPanel);
		}

		private void ePGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.panelEPG.Show(this.dockPanel);
		}

		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.panelSettings.Show(this.dockPanel);
		}

		private void channelWizardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowChannelWizard();
		}

		private void ShowChannelWizard()
		{
			WizardForm wizardForm = new WizardForm(this);
			wizardForm.ShowDialog(this);
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutForm dlg = new AboutForm();
			dlg.ShowDialog(this);
		}

		private void toolStripButtonSnapshot_Click(object sender, EventArgs e)
		{
			ExecuteCommand(CommandName.SnapShot);
		}
	}
}