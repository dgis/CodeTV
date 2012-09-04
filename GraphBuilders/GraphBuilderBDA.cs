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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

using Microsoft.Win32;

using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;

using CodeTV.PSI;

namespace CodeTV
{
	public class GraphBuilderBDA : GraphBuilderTV, IBDA, IEPG
	{
		protected IBaseFilter networkProvider;
		protected IBaseFilter mpeg2Demux;
		protected IBaseFilter tuner;
		protected IBaseFilter capture;
		protected IBaseFilter bdaTIF;
		protected IBaseFilter bdaSecTab;
		protected ITuningSpace objTuningSpace;
		protected ITuneRequest objTuneRequest;
		protected IBaseFilter audioDecoderFilter = null;
		protected IBaseFilter videoMpeg2DecoderFilter = null;
		protected IBaseFilter videoH264DecoderFilter = null;

		protected ChannelDVB.Clock referenceClock = ChannelDVB.Clock.AudioRenderer;

		private static Dictionary<string, DsDevice> audioDecoderDevices;
		protected DsDevice audioDecoderDevice;
		private static Dictionary<string, DsDevice> mpeg2DecoderDevices;
		protected DsDevice mpeg2DecoderDevice;
		private static Dictionary<string, DsDevice> h264DecoderDevices;
		protected DsDevice h264DecoderDevice;
		private static Dictionary<string, DsDevice> bdaTunerDevices;
		protected DsDevice tunerDevice;
		private static Dictionary<string, DsDevice> bdaCaptureDevices;
		protected DsDevice captureDevice;

		protected EPG epg;

		public static Dictionary<string, DsDevice> AudioDecoderDevices
		{
			get
			{
				if (audioDecoderDevices == null)
				{
					audioDecoderDevices = new Dictionary<string, DsDevice>();

					DsDevice[] devices = DeviceEnumerator.GetDevicesWithThisInPin(MediaType.Audio, MediaSubType.Mpeg2Audio);
					foreach (DsDevice d in devices)
						if (d.Name != null)
							audioDecoderDevices.Add(d.Name, d);
				}
				return audioDecoderDevices;
			}
		}

		public static Dictionary<string, DsDevice> MPEG2DecoderDevices
		{
			get
			{
				if (mpeg2DecoderDevices == null)
				{
					mpeg2DecoderDevices = new Dictionary<string, DsDevice>();

					DsDevice[] devices = DeviceEnumerator.GetMPEG2VideoDevices();
					foreach (DsDevice d in devices)
						if (d.Name != null)
							mpeg2DecoderDevices.Add(d.Name, d);
				}
				return mpeg2DecoderDevices;
			}
		}

		public static Dictionary<string, DsDevice> H264DecoderDevices
		{
			get
			{
				if (h264DecoderDevices == null)
				{
					h264DecoderDevices = new Dictionary<string, DsDevice>();

					DsDevice[] devices = DeviceEnumerator.GetH264Devices();
					foreach (DsDevice d in devices)
						h264DecoderDevices.Add(d.Name, d);
				}
				return h264DecoderDevices;
			}
		}

		public static Dictionary<string, DsDevice> TunerDevices
		{
			get
			{
				if(bdaTunerDevices == null)
				{
					bdaTunerDevices = new Dictionary<string, DsDevice>();

					// Then enumerate BDA Receiver Components category to found a filter connecting 
					// to the tuner and the MPEG2 Demux
					DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
					foreach (DsDevice d in devices)
						bdaTunerDevices.Add(d.Name, d);
				}
				return bdaTunerDevices;
			}
		}

		public static Dictionary<string, DsDevice> CaptureDevices
		{
			get
			{
				if (bdaCaptureDevices == null)
				{
					bdaCaptureDevices = new Dictionary<string, DsDevice>();

					// Enumerate BDA Source filters category and found one that can connect to the network provider
					DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
					foreach (DsDevice d in devices)
						bdaCaptureDevices.Add(d.Name, d);
				}
				return bdaCaptureDevices;
			}
		}

		public ChannelDVB.Clock ReferenceClock { get { return this.referenceClock; } set { this.referenceClock = value; } }

		public DsDevice AudioDecoderDevice { get { return this.audioDecoderDevice; } set { this.audioDecoderDevice = value; } }
		public DsDevice Mpeg2DecoderDevice { get { return this.mpeg2DecoderDevice; } set { this.mpeg2DecoderDevice = value; } }
		public DsDevice H264DecoderDevice { get { return this.h264DecoderDevice; } set { this.h264DecoderDevice = value; } }
		public DsDevice TunerDevice { get { return this.tunerDevice; } set { this.tunerDevice = value; } }
		public DsDevice CaptureDevice { get { return this.captureDevice; } set { this.captureDevice = value; } }

		public IBaseFilter NetworkProvider { get { return this.networkProvider; } }
		public IBaseFilter TunerFilter { get { return this.tuner; } }
		public IBaseFilter CaptureFilter { get { return this.capture; } }
		public IBaseFilter Demultiplexer { get { return this.mpeg2Demux; } }
		public IBaseFilter SectionsAndTables { get { return this.bdaSecTab; } }
		public IBaseFilter TransportInformationFilter { get { return this.bdaTIF; } }

		public ITuningSpace TuningSpace { get { return this.objTuningSpace; } set { this.objTuningSpace = value; } }
		public ITuneRequest TuneRequest { get { return this.objTuneRequest; } set { this.objTuneRequest = value; } }

		public EPG EPG { get { return this.epg; } }

		private int cookies = 0;
		protected bool isH264ElecardSpecialMode = false;

		public GraphBuilderBDA(VideoControl renderingControl)
			: base(renderingControl)
		{
		}

		public override void BuildGraph()
		{
			try
			{
				useWPF = Settings.UseWPF;

				int hr;

				this.graphBuilder = (IFilterGraph2)new FilterGraph();
				rot = new DsROTEntry(this.graphBuilder);

				this.epg = new EPG(this.hostingControl);

				this.cookies = this.hostingControl.SubscribeEvents(this as VideoControl.IVideoEventHandler, this.graphBuilder as IMediaEventEx);

				// Method names should be self explanatory
				AddNetworkProviderFilter(this.objTuningSpace);
				AddMPEG2DemuxFilter();

				CreateMPEG2DemuxPins();

				//AddAndConnectTransportStreamFiltersToGraph();

				AddAudioDecoderFilters(this.graphBuilder);
				AddBDAVideoDecoderFilters(this.graphBuilder);

				AddAndConnectBDABoardFilters();
				if (this.tuner == null) // && this.capture == null)
					throw new ApplicationException("No BDA devices found!");

				if (this.H264DecoderDevice != null || !isH264ElecardSpecialMode)
				{
					//+++ This order is to avoid a bug from the DirectShow 
					AddAndConnectSectionsAndTablesFilterToGraph();

					IMpeg2Data mpeg2Data = this.bdaSecTab as IMpeg2Data;
					ISectionList ppSectionList;
					int hr2 = mpeg2Data.GetTable(0, 0, null, 0, out ppSectionList);

					AddAndConnectTIFToGraph();
					//---
				}

				AddRenderers();
				if (!useWPF)
					ConfigureVMR9InWindowlessMode();
				//RenderMpeg2DemuxFilters();
				ConnectAudioAndVideoFilters();

				this.epg.RegisterEvent(TransportInformationFilter as IConnectionPointContainer);

				Vmr9SetDeinterlaceMode(1);

				IMediaFilter mf = this.graphBuilder as IMediaFilter;
				switch (this.referenceClock)
				{
					case ChannelDVB.Clock.Default:
						hr = this.graphBuilder.SetDefaultSyncSource();
						break;
					case ChannelDVB.Clock.MPEGDemultiplexer:
						hr = mf.SetSyncSource(this.mpeg2Demux as IReferenceClock);
						break;
					case ChannelDVB.Clock.AudioRenderer:
						hr = mf.SetSyncSource(this.audioRenderer as IReferenceClock);
						break;
				}

				this.hostingControl.CurrentGraphBuilder = this;

				OnGraphStarted();
			}
			catch (Exception ex)
			{
				Decompose();
				throw ex;
			}
		}

		private int currentMpeg2VideoPid = -1;
		private int currentH264VideoPid = -1;
		private int currentAudioPid = -1;

		public override void SubmitTuneRequest(Channel channel)
		{
			if (channel is ChannelDVB)
			{
				ChannelDVB channelDVB = channel as ChannelDVB;

				int hr = 0;

				IMpeg2Demultiplexer mpeg2Demultiplexer = this.mpeg2Demux as IMpeg2Demultiplexer;

				IPin pinDemuxerTIF;
				hr = this.mpeg2Demux.FindPin("TIF", out pinDemuxerTIF);
				if (pinDemuxerTIF != null)
				{
					IMPEG2PIDMap mpeg2PIDMap = pinDemuxerTIF as IMPEG2PIDMap;
					if (mpeg2PIDMap != null)
						hr = mpeg2PIDMap.MapPID(6, new int[] { 0x00, 0x10, 0x11, 0x12, 0x13, 0x14 }, MediaSampleContent.Mpeg2PSI);
					Marshal.ReleaseComObject(pinDemuxerTIF);
				}

				if (channelDVB.VideoPid != -1)
				{
					if (channelDVB.VideoDecoderType == ChannelDVB.VideoType.H264)
					{
						IPin pinDemuxerVideoMPEG4;
						hr = this.mpeg2Demux.FindPin("H264", out pinDemuxerVideoMPEG4);
						if (pinDemuxerVideoMPEG4 != null)
						{
							IMPEG2PIDMap mpeg2PIDMap = pinDemuxerVideoMPEG4 as IMPEG2PIDMap;
							if (mpeg2PIDMap != null)
							{
								if (this.currentH264VideoPid >= 0)
									hr = mpeg2PIDMap.UnmapPID(1, new int[] { this.currentH264VideoPid });

								hr = mpeg2PIDMap.MapPID(1, new int[] { channelDVB.VideoPid }, MediaSampleContent.ElementaryStream);
								this.currentH264VideoPid = channelDVB.VideoPid;
							}
							Marshal.ReleaseComObject(pinDemuxerVideoMPEG4);
						}

						//IVMRMixerControl9 vmrMixerControl9 = this.videoRenderer as IVMRMixerControl9;
						//vmrMixerControl9.SetZOrder(0, 1);
					}
					else
					{
						IPin pinDemuxerVideoMPEG2;
						hr = this.mpeg2Demux.FindPin("MPG2", out pinDemuxerVideoMPEG2);
						if (pinDemuxerVideoMPEG2 != null)
						{
							IMPEG2PIDMap mpeg2PIDMap = pinDemuxerVideoMPEG2 as IMPEG2PIDMap;
							if (mpeg2PIDMap != null)
							{
								//IEnumPIDMap enumPIDMap;
								//hr = mpeg2PIDMap.EnumPIDMap(out enumPIDMap);
								//PIDMap[] pidMap = new PIDMap[1];
								//int numReceive = 0;
								//ArrayList al = new ArrayList();
								//while (enumPIDMap.Next(1, pidMap, out numReceive) >= 0)
								//{
								//    al.Add(pidMap[0].ulPID);
								//}

								if (this.currentMpeg2VideoPid >= 0)
									hr = mpeg2PIDMap.UnmapPID(1, new int[] { this.currentMpeg2VideoPid });

								hr = mpeg2PIDMap.MapPID(1, new int[] { channelDVB.VideoPid }, MediaSampleContent.ElementaryStream);
								this.currentMpeg2VideoPid = channelDVB.VideoPid;
							}
							Marshal.ReleaseComObject(pinDemuxerVideoMPEG2);
						}
					}
				}

				if (channelDVB.AudioPid != -1)
				{
					IPin pinDemuxerAudio;
					hr = this.mpeg2Demux.FindPin("Audio", out pinDemuxerAudio);
					if (pinDemuxerAudio != null)
					{
						IMPEG2PIDMap mpeg2PIDMap = pinDemuxerAudio as IMPEG2PIDMap;
						if (mpeg2PIDMap != null)
						{
							if (this.currentAudioPid >= 0)
								hr = mpeg2PIDMap.UnmapPID(1, new int[] { this.currentAudioPid });

							hr = mpeg2PIDMap.MapPID(1, new int[] { channelDVB.AudioPid }, MediaSampleContent.ElementaryStream);
							this.currentAudioPid = channelDVB.AudioPid;
						}
						Marshal.ReleaseComObject(pinDemuxerAudio);
					}
				}

				IPin pinDemuxerSectionsAndTables;
				hr = this.mpeg2Demux.FindPin("PSI", out pinDemuxerSectionsAndTables);
				if (pinDemuxerSectionsAndTables != null)
				{
					IMPEG2PIDMap mpeg2PIDMap = pinDemuxerSectionsAndTables as IMPEG2PIDMap;
					if (mpeg2PIDMap != null)
						hr = mpeg2PIDMap.MapPID(2, new int[] { (int)PIDS.PAT, (int)PIDS.SDT }, MediaSampleContent.Mpeg2PSI);
					Marshal.ReleaseComObject(pinDemuxerSectionsAndTables);
				}

				ITuner tuner = NetworkProvider as ITuner;
				hr = tuner.get_TuningSpace(out this.objTuningSpace);

				this.objTuningSpace.CreateTuneRequest(out this.objTuneRequest);
				IDVBTuneRequest dvbTuneRequest = (IDVBTuneRequest)this.objTuneRequest;
				dvbTuneRequest.put_ONID(channelDVB.ONID);
				dvbTuneRequest.put_TSID(channelDVB.TSID);
				dvbTuneRequest.put_SID(channelDVB.SID);

				ILocator locator = null;
				if (channel is ChannelDVBT)
				{
					ChannelDVBT channelDVBT = channel as ChannelDVBT;

					IDVBTLocator dvbLocator = (IDVBTLocator)new DVBTLocator();

					dvbLocator.put_Bandwidth(channelDVBT.Bandwidth);
					dvbLocator.put_Guard(channelDVBT.Guard);
					dvbLocator.put_HAlpha(channelDVBT.HAlpha);
					dvbLocator.put_LPInnerFEC(channelDVBT.LPInnerFEC);
					dvbLocator.put_LPInnerFECRate(channelDVBT.LPInnerFECRate);
					dvbLocator.put_Mode(channelDVBT.Mode);
					dvbLocator.put_OtherFrequencyInUse(channelDVBT.OtherFrequencyInUse);

					locator = dvbLocator as ILocator;
				}
				else if (channel is ChannelDVBC)
				{
					ChannelDVBC channelDVBC = channel as ChannelDVBC;

					IDVBCLocator dvbLocator = (IDVBCLocator)new DVBCLocator();
					locator = dvbLocator as ILocator;
				}
				else if (channel is ChannelDVBS)
				{
					ChannelDVBS channelDVBS = channel as ChannelDVBS;

					IDVBSLocator dvbLocator = (IDVBSLocator)new DVBSLocator();
					dvbLocator.put_CarrierFrequency((int)channelDVBS.Frequency);

					dvbLocator.put_Azimuth(channelDVBS.Azimuth);
					dvbLocator.put_Elevation(channelDVBS.Elevation);
					dvbLocator.put_OrbitalPosition(channelDVBS.OrbitalPosition);
					dvbLocator.put_SignalPolarisation(channelDVBS.SignalPolarisation);
					dvbLocator.put_WestPosition(channelDVBS.WestPosition);

					locator = dvbLocator as ILocator;
				}
				
				locator.put_CarrierFrequency((int)channelDVB.Frequency);

				locator.put_InnerFEC(channelDVB.InnerFEC);
				locator.put_InnerFECRate(channelDVB.InnerFECRate);
				locator.put_Modulation(channelDVB.Modulation);
				locator.put_OuterFEC(channelDVB.OuterFEC);
				locator.put_OuterFECRate(channelDVB.OuterFECRate);
				locator.put_SymbolRate(channelDVB.SymbolRate);

				dvbTuneRequest.put_Locator(locator);

				hr = (this.networkProvider as ITuner).put_TuneRequest(this.objTuneRequest);
				DsError.ThrowExceptionForHR(hr);
			}

			if(useWPF)
				WpfUpdateVideoSize(); //WPF
		}

		protected void AddNetworkProviderFilter(ITuningSpace tuningSpace)
		{
			int hr = 0;
			Guid genProviderClsId = new Guid("{B2F3A67C-29DA-4C78-8831-091ED509A475}");
			Guid networkProviderClsId;

			// First test if the Generic Network Provider is available (only on MCE 2005 + Update Rollup 2)
			//if (FilterGraphTools.IsThisComObjectInstalled(genProviderClsId))
			//{
			//    this.networkProvider = FilterGraphTools.AddFilterFromClsid(this.graphBuilder, genProviderClsId, "Generic Network Provider");

			//    hr = (this.networkProvider as ITuner).put_TuningSpace(tuningSpace);
			//    return;
			//}

			// Get the network type of the requested Tuning Space
			hr = tuningSpace.get__NetworkType(out networkProviderClsId);

			// Get the network type of the requested Tuning Space
			if (networkProviderClsId == typeof(DVBTNetworkProvider).GUID)
			{
				this.networkProvider = FilterGraphTools.AddFilterFromClsid(this.graphBuilder, networkProviderClsId, "DVBT Network Provider");
			}
			else if (networkProviderClsId == typeof(DVBSNetworkProvider).GUID)
			{
				this.networkProvider = FilterGraphTools.AddFilterFromClsid(this.graphBuilder, networkProviderClsId, "DVBS Network Provider");
			}
			else if (networkProviderClsId == typeof(ATSCNetworkProvider).GUID)
			{
				this.networkProvider = FilterGraphTools.AddFilterFromClsid(this.graphBuilder, networkProviderClsId, "ATSC Network Provider");
			}
			else if (networkProviderClsId == typeof(DVBCNetworkProvider).GUID)
			{
				this.networkProvider = FilterGraphTools.AddFilterFromClsid(this.graphBuilder, networkProviderClsId, "DVBC Network Provider");
			}
			else
				// Tuning Space can also describe Analog TV but this application don't support them
				throw new ArgumentException("This application doesn't support this Tuning Space");

			hr = (this.networkProvider as ITuner).put_TuningSpace(tuningSpace);
		}

		/// <summary>
		/// CLSID_ElecardMPEGDemultiplexer
		/// </summary>
		//[ComImport, Guid("136DCBF5-3874-4B70-AE3E-15997D6334F7")]
		[ComImport, Guid("668EE184-FD2D-4C72-8E79-439A35B438DE")]
		public class ElecardMPEGDemultiplexer
		{
		}


		protected void AddMPEG2DemuxFilter()
		{
			if (this.H264DecoderDevice != null && isH264ElecardSpecialMode)
				this.mpeg2Demux = (IBaseFilter)new ElecardMPEGDemultiplexer();
			else
				this.mpeg2Demux = (IBaseFilter)new MPEG2Demultiplexer();

			int hr = this.graphBuilder.AddFilter(this.mpeg2Demux, "MPEG2 Demultiplexer");
			DsError.ThrowExceptionForHR(hr);


			//IMpeg2Demultiplexer mpeg2Demultiplexer = this.mpeg2Demux as IMpeg2Demultiplexer;

			////Log.WriteFile(Log.LogType.Log, false, "DVBGraphBDA: create mpg4 video pin");
			//AMMediaType mediaMPG4 = new AMMediaType();
			//mediaMPG4.majorType = MediaType.;
			//mediaMPG4.subType = MediaSubType.; //new Guid(0x8d2d71cb, 0x243f, 0x45e3, 0xb2, 0xd8, 0x5f, 0xd7, 0x96, 0x7e, 0xc0, 0x9b);
			//mediaMPG4.sampleSize = 0;
			//mediaMPG4.temporalCompression = false;
			//mediaMPG4.fixedSizeSamples = false;
			//mediaMPG4.unkPtr = IntPtr.Zero;
			//mediaMPG4.formatType = FormatType.Mpeg2Video;
			//mediaMPG4.formatSize = Mpeg2ProgramVideo.GetLength(0);

			////mediaMPG4.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaMPG4.formatSize);
			////System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo, 0, mediaMPG4.formatPtr, mediaMPG4.formatSize);

			//IntPtr formatPtr = Marshal.AllocHGlobal(Mpeg2ProgramVideo.Length);
			//Marshal.Copy(Mpeg2ProgramVideo, 0, formatPtr, Mpeg2ProgramVideo.Length);
			//mediaMPG4.formatPtr = formatPtr;

			//IPin pinDemuxerVideoMPEG4;
			//int hr = mpeg2Demultiplexer.CreateOutputPin(mediaMPG4, "H264", out pinDemuxerVideoMPEG4);
			//if (pinDemuxerVideoMPEG4 != null)
			//    Marshal.ReleaseComObject(pinDemuxerVideoMPEG4);

			//Marshal.FreeHGlobal(formatPtr);

		}

		protected virtual void CreateMPEG2DemuxPins()
		{
			IMpeg2Demultiplexer mpeg2Demultiplexer = this.mpeg2Demux as IMpeg2Demultiplexer;

			{
				//Pin 1 connected to the "BDA MPEG2 Transport Information Filter"
				//    Major Type	MEDIATYPE_MPEG2_SECTIONS {455F176C-4B06-47CE-9AEF-8CAEF73DF7B5}
				//    Sub Type		MEDIASUBTYPE_DVB_SI {E9DD31A3-221D-4ADB-8532-9AF309C1A408}
				//    Format		None

				//    MPEG2 PSI Sections
				//    Pids: 0x00 0x10 0x11 0x12 0x13 0x14 0x6e 0xd2 0x0136 0x019a 0x01fe 0x0262 0x03f2

				AMMediaType mediaTIF = new AMMediaType();
				mediaTIF.majorType = MediaType.Mpeg2Sections;
				mediaTIF.subType = MediaSubType.DvbSI;
				mediaTIF.fixedSizeSamples = false;
				mediaTIF.temporalCompression = false;
				mediaTIF.sampleSize = 1;
				mediaTIF.unkPtr = IntPtr.Zero;
				mediaTIF.formatType = FormatType.None;
				mediaTIF.formatSize = 0;
				mediaTIF.formatPtr = IntPtr.Zero;

				IPin pinDemuxerTIF;
				int hr = mpeg2Demultiplexer.CreateOutputPin(mediaTIF, "TIF", out pinDemuxerTIF);
				if (pinDemuxerTIF != null)
					Marshal.ReleaseComObject(pinDemuxerTIF);
			}

			if (this.H264DecoderDevice != null)
			{
				AMMediaType mediaH264 = new AMMediaType();
				mediaH264.majorType = MediaType.Video;
				//mediaH264.subType = new Guid(0x8d2d71cb, 0x243f, 0x45e3, 0xb2, 0xd8, 0x5f, 0xd7, 0x96, 0x7e, 0xc0, 0x9b);
				mediaH264.subType = new Guid(0x34363248, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
				mediaH264.sampleSize = 0;
				mediaH264.temporalCompression = true; // false;
				mediaH264.fixedSizeSamples = false;
				mediaH264.unkPtr = IntPtr.Zero;
				mediaH264.formatType = FormatType.Mpeg2Video;

				MPEG2VideoInfo videoH264PinFormat = GetVideoH264PinFormat();
				mediaH264.formatSize = Marshal.SizeOf(videoH264PinFormat);
				mediaH264.formatPtr = Marshal.AllocHGlobal(mediaH264.formatSize);
				Marshal.StructureToPtr(videoH264PinFormat, mediaH264.formatPtr, false);

				IPin pinDemuxerVideoH264;
				int hr = mpeg2Demultiplexer.CreateOutputPin(mediaH264, "H264", out pinDemuxerVideoH264);
				if (pinDemuxerVideoH264 != null)
					Marshal.ReleaseComObject(pinDemuxerVideoH264);

				Marshal.FreeHGlobal(mediaH264.formatPtr);
			}
			else
			{
				AMMediaType mediaMPG2 = new AMMediaType();
				mediaMPG2.majorType = MediaType.Video;
				mediaMPG2.subType = MediaSubType.Mpeg2Video;
				mediaMPG2.fixedSizeSamples = false;
				mediaMPG2.temporalCompression = false; // true???
				mediaMPG2.sampleSize = 0;
				mediaMPG2.formatType = FormatType.Mpeg2Video;
				mediaMPG2.unkPtr = IntPtr.Zero;

				MPEG2VideoInfo videoMPEG2PinFormat = GetVideoMPEG2PinFormat();
				mediaMPG2.formatSize = Marshal.SizeOf(videoMPEG2PinFormat);
				mediaMPG2.formatPtr = Marshal.AllocHGlobal(mediaMPG2.formatSize);
				Marshal.StructureToPtr(videoMPEG2PinFormat, mediaMPG2.formatPtr, false);

				IPin pinDemuxerVideoMPEG2;
				int hr = mpeg2Demultiplexer.CreateOutputPin(mediaMPG2, "MPG2", out pinDemuxerVideoMPEG2);
				if (pinDemuxerVideoMPEG2 != null)
					Marshal.ReleaseComObject(pinDemuxerVideoMPEG2);

				Marshal.FreeHGlobal(mediaMPG2.formatPtr);
			}

			{
				AMMediaType mediaAudio = new AMMediaType();
				mediaAudio.majorType = MediaType.Audio;
				mediaAudio.subType = MediaSubType.Mpeg2Audio;
				mediaAudio.sampleSize = 0;
				mediaAudio.temporalCompression = false;
				mediaAudio.fixedSizeSamples = true;
				mediaAudio.unkPtr = IntPtr.Zero;
				mediaAudio.formatType = FormatType.WaveEx;

				MPEG1WaveFormat audioPinFormat = GetAudioPinFormat();
				mediaAudio.formatSize = Marshal.SizeOf(audioPinFormat);
				mediaAudio.formatPtr = Marshal.AllocHGlobal(mediaAudio.formatSize);
				Marshal.StructureToPtr(audioPinFormat, mediaAudio.formatPtr, false);

				IPin pinDemuxerAudio;
				int hr = mpeg2Demultiplexer.CreateOutputPin(mediaAudio, "Audio", out pinDemuxerAudio);
				if (pinDemuxerAudio != null)
					Marshal.ReleaseComObject(pinDemuxerAudio);

				Marshal.FreeHGlobal(mediaAudio.formatPtr);
			}

			{
				//Pin 5 connected to "MPEG-2 Sections and Tables" (Allows to grab custom PSI tables)
				//    Major Type	MEDIATYPE_MPEG2_SECTIONS {455F176C-4B06-47CE-9AEF-8CAEF73DF7B5}
				//    Sub Type		MEDIASUBTYPE_MPEG2DATA {C892E55B-252D-42B5-A316-D997E7A5D995}
				//    Format		None

				AMMediaType mediaSectionsAndTables = new AMMediaType();
				mediaSectionsAndTables.majorType = MediaType.Mpeg2Sections;
				mediaSectionsAndTables.subType = MediaSubType.Mpeg2Data;
				mediaSectionsAndTables.sampleSize = 0; // 1;
				mediaSectionsAndTables.temporalCompression = false;
				mediaSectionsAndTables.fixedSizeSamples = true;
				mediaSectionsAndTables.unkPtr = IntPtr.Zero;
				mediaSectionsAndTables.formatType = FormatType.None;
				mediaSectionsAndTables.formatSize = 0;
				mediaSectionsAndTables.formatPtr = IntPtr.Zero;

				IPin pinDemuxerSectionsAndTables;
				int hr = mpeg2Demultiplexer.CreateOutputPin(mediaSectionsAndTables, "PSI", out pinDemuxerSectionsAndTables);
				if (pinDemuxerSectionsAndTables != null)
					Marshal.ReleaseComObject(pinDemuxerSectionsAndTables);
			}
		}

		protected void AddAudioDecoderFilters(IFilterGraph2 graphBuilder)
		{
			int hr = 0;
			if (this.AudioDecoderDevice != null)
				hr = graphBuilder.AddSourceFilterForMoniker(this.AudioDecoderDevice.Mon, null, this.AudioDecoderDevice.Name, out this.audioDecoderFilter);
			DsError.ThrowExceptionForHR(hr);
		}

		protected void AddBDAVideoDecoderFilters(IFilterGraph2 graphBuilder)
		{
			int hr = 0;
			if (this.H264DecoderDevice != null)
				hr = graphBuilder.AddSourceFilterForMoniker(this.H264DecoderDevice.Mon, null, this.H264DecoderDevice.Name, out this.videoH264DecoderFilter);
			else if (this.Mpeg2DecoderDevice != null)
				hr = graphBuilder.AddSourceFilterForMoniker(this.Mpeg2DecoderDevice.Mon, null, this.Mpeg2DecoderDevice.Name, out this.videoMpeg2DecoderFilter);
			DsError.ThrowExceptionForHR(hr);
		}

		protected void AddAndConnectBDABoardFilters()
		{
			int hr = 0;
			DsDevice[] devices;

			this.captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
			captureGraphBuilder.SetFiltergraph(this.graphBuilder);

			try
			{
				if (this.TunerDevice != null)
				{
					IBaseFilter tmp;

					hr = graphBuilder.AddSourceFilterForMoniker(this.TunerDevice.Mon, null, this.TunerDevice.Name, out tmp);
					DsError.ThrowExceptionForHR(hr);

					hr = captureGraphBuilder.RenderStream(null, null, this.networkProvider, null, tmp);
					if (hr == 0)
					{
						// Got it !
						this.tuner = tmp;
					}
					else
					{
						// Try another...
						int hr2 = graphBuilder.RemoveFilter(tmp);
						Marshal.ReleaseComObject(tmp);
						DsError.ThrowExceptionForHR(hr);
						return;
					}
				}
				else
				{
					// Enumerate BDA Source filters category and found one that can connect to the network provider
					devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
					for (int i = 0; i < devices.Length; i++)
					{
						IBaseFilter tmp;

						hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
						DsError.ThrowExceptionForHR(hr);

						hr = captureGraphBuilder.RenderStream(null, null, this.networkProvider, null, tmp);
						if (hr == 0)
						{
							// Got it !
							this.tuner = tmp;
							break;
						}
						else
						{
							// Try another...
							hr = graphBuilder.RemoveFilter(tmp);
							Marshal.ReleaseComObject(tmp);
						}
					}
				}
				// Assume we found a tuner filter...

				if (this.CaptureDevice != null)
				{
					IBaseFilter tmp;

					hr = graphBuilder.AddSourceFilterForMoniker(this.CaptureDevice.Mon, null, this.CaptureDevice.Name, out tmp);
					DsError.ThrowExceptionForHR(hr);

					hr = captureGraphBuilder.RenderStream(null, null, this.tuner, null, tmp);
					if (hr == 0)
					{
						// Got it !
						this.capture = tmp;

						// Connect it to the MPEG-2 Demux
						hr = captureGraphBuilder.RenderStream(null, null, this.capture, null, this.mpeg2Demux);
						if (hr < 0)
							// BDA also support 3 filter scheme (never show it in real life).
							throw new ApplicationException("This application only support the 2 filters BDA scheme");
					}
					else
					{
						// Try another...
						int hr2 = graphBuilder.RemoveFilter(tmp);
						Marshal.ReleaseComObject(tmp);
						DsError.ThrowExceptionForHR(hr);
						return;
					}
				}
				else
				{
					this.capture = null;

					// Connect it to the MPEG-2 Demux
					hr = captureGraphBuilder.RenderStream(null, null, this.tuner, null, this.mpeg2Demux);
					if (hr < 0)
						// BDA also support 3 filter scheme (never show it in real life).
						throw new ApplicationException("This application only support the 2 filters BDA scheme");


					//// Then enumerate BDA Receiver Components category to found a filter connecting 
					//// to the tuner and the MPEG2 Demux
					//devices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);

					//for (int i = 0; i < devices.Length; i++)
					//{
					//    IBaseFilter tmp;

					//    hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
					//    DsError.ThrowExceptionForHR(hr);

					//    hr = captureGraphBuilder.RenderStream(null, null, this.tuner, null, tmp);
					//    if (hr == 0)
					//    {
					//        // Got it !
					//        this.capture = tmp;

					//        // Connect it to the MPEG-2 Demux
					//        hr = captureGraphBuilder.RenderStream(null, null, this.capture, null, this.mpeg2Demux);
					//        if (hr < 0)
					//            // BDA also support 3 filter scheme (never show it in real life).
					//            throw new ApplicationException("This application only support the 2 filters BDA scheme");

					//        break;
					//    }
					//    else
					//    {
					//        // Try another...
					//        hr = graphBuilder.RemoveFilter(tmp);
					//        Marshal.ReleaseComObject(tmp);
					//    }
					//}
				}
			}
			finally
			{
			}
		}

		protected void AddTransportStreamFiltersToGraph()
		{
			int hr = 0;
			DsDevice[] devices;

			// Add two filters needed in a BDA graph
			devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
			for (int i = 0; i < devices.Length; i++)
			{
				if (devices[i].Name.Equals("BDA MPEG2 Transport Information Filter"))
				{
					hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out this.bdaTIF);
					DsError.ThrowExceptionForHR(hr);
					continue;
				}

				if (devices[i].Name.Equals("MPEG-2 Sections and Tables"))
				{
					hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out this.bdaSecTab);
					DsError.ThrowExceptionForHR(hr);
					continue;
				}
			}
		}

		protected void AddAndConnectTIFToGraph()
		{
			int hr = 0;
			IPin pinOut;
			DsDevice[] devices;

			// Add two filters needed in a BDA graph
			devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
			for (int i = 0; i < devices.Length; i++)
			{
				if (devices[i].Name.Equals("BDA MPEG2 Transport Information Filter"))
				{
					hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out this.bdaTIF);
					DsError.ThrowExceptionForHR(hr);

					// Connect the MPEG-2 Demux output pin for the "BDA MPEG2 Transport Information Filter"
					hr = this.mpeg2Demux.FindPin("TIF", out pinOut);
					if (pinOut != null)
					{
						IPin pinIn = DsFindPin.ByDirection(this.bdaTIF, PinDirection.Input, 0);
						if (pinIn != null)
						{
							hr = this.graphBuilder.Connect(pinOut, pinIn);
							Marshal.ReleaseComObject(pinIn);
						}

						// In fact the last pin don't render since i havn't added the BDA MPE Filter...
						Marshal.ReleaseComObject(pinOut);
					}

					continue;
				}
			}
		}

		protected void AddAndConnectSectionsAndTablesFilterToGraph()
		{
			int hr = 0;
			IPin pinOut;
			DsDevice[] devices;

			// Add two filters needed in a BDA graph
			devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
			for (int i = 0; i < devices.Length; i++)
			{
				if (devices[i].Name.Equals("MPEG-2 Sections and Tables"))
				{
					hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out this.bdaSecTab);
					DsError.ThrowExceptionForHR(hr);

					// Connect the MPEG-2 Demux output pin for the "MPEG-2 Sections and Tables" filter
					hr = this.mpeg2Demux.FindPin("PSI", out pinOut);
					if (pinOut != null)
					{
						IPin pinIn = DsFindPin.ByDirection(this.bdaSecTab, PinDirection.Input, 0);
						if (pinIn != null)
						{
							hr = this.graphBuilder.Connect(pinOut, pinIn);
							Marshal.ReleaseComObject(pinIn);
						}

						//DsError.ThrowExceptionForHR(hr);
						// In fact the last pin don't render since i havn't added the BDA MPE Filter...
						Marshal.ReleaseComObject(pinOut);
					}

					continue;
				}
			}
		}

		protected void ConnectAllOutputFilters()
		{
			// Get the pin enumerator
			IEnumPins ppEnum;
			int hr = this.mpeg2Demux.EnumPins(out ppEnum);
			DsError.ThrowExceptionForHR(hr);

			try
			{
				// Walk the pins looking for a match
				IPin[] pPins = new IPin[1];
				//22 int lFetched;
				//22 while ((ppEnum.Next(1, pPins, out lFetched) >= 0) && (lFetched == 1))
				while (ppEnum.Next(1, pPins, IntPtr.Zero) >= 0)
				{
					// Read the direction
					PinDirection ppindir;
					hr = pPins[0].QueryDirection(out ppindir);
					DsError.ThrowExceptionForHR(hr);

					// Is it the right direction?
					if (ppindir == PinDirection.Output)
					{
						if (pPins[0] != null)
						{
							hr = this.graphBuilder.Render(pPins[0]);
							//DsError.ThrowExceptionForHR(hr);
							// In fact the last pin don't render since i havn't added the BDA MPE Filter...
						}
					}
					Marshal.ReleaseComObject(pPins[0]);
				}
			}
			finally
			{
				Marshal.ReleaseComObject(ppEnum);
			}
		}

		//protected void ConnectAudioAndVideoFilters()
		//{
		//    int hr = 0;
		//    IPin pinOut;

		//    hr = this.mpeg2Demux.FindPin("H264", out pinOut);
		//    if (pinOut != null)
		//    {
		//        IPin pinInFromFilterOut = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Input, 0);
		//        if (pinInFromFilterOut != null)
		//        {
		//            hr = this.graphBuilder.Connect(pinOut, pinInFromFilterOut);
		//            Marshal.ReleaseComObject(pinInFromFilterOut);
		//        }

		//        //hr = this.graphBuilder.Render(pinOut);
		//        ////DsError.ThrowExceptionForHR(hr);
		//        // In fact the last pin don't render since i havn't added the BDA MPE Filter...
		//        Marshal.ReleaseComObject(pinOut);
		//    }

		//    hr = this.mpeg2Demux.FindPin("MPG2", out pinOut);
		//    if (pinOut != null)
		//    {
		//        //hr = this.graphBuilder.Render(pinOut);
		//        IPin pinInFromFilterOut = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Input, 0);
		//        if (pinInFromFilterOut != null)
		//        {
		//            hr = this.graphBuilder.Connect(pinOut, pinInFromFilterOut);
		//            Marshal.ReleaseComObject(pinInFromFilterOut);
		//        }
		//        ////DsError.ThrowExceptionForHR(hr);
		//        // In fact the last pin don't render since i havn't added the BDA MPE Filter...
		//        Marshal.ReleaseComObject(pinOut);
		//    }

		//    if (useWPF)
		//    {
		//        IPin pinOutFromFilterOut = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Output, 0);
		//        if (pinOutFromFilterOut != null)
		//        {
		//            hr = this.graphBuilder.Render(pinOutFromFilterOut);
		//            Marshal.ReleaseComObject(pinOutFromFilterOut);
		//        }
		//    }

		//    hr = this.mpeg2Demux.FindPin("Audio", out pinOut);
		//    if (pinOut != null)
		//    {
		//        hr = this.graphBuilder.Render(pinOut);
		//        //DsError.ThrowExceptionForHR(hr);
		//        // In fact the last pin don't render since i havn't added the BDA MPE Filter...
		//        Marshal.ReleaseComObject(pinOut);
		//    }
		//}

		protected void ConnectAudioAndVideoFilters()
		{
			int hr = 0;
			IPin pinOut;

			hr = this.mpeg2Demux.FindPin("H264", out pinOut);
			if (pinOut != null)
			{
				try
				{
					if (this.videoH264DecoderFilter == null)
					{
						IPin pinInFromFilterOut = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Input, 0);
						if (pinInFromFilterOut != null)
						{
							try
							{
								hr = this.graphBuilder.Connect(pinOut, pinInFromFilterOut);
							}
							finally
							{
								Marshal.ReleaseComObject(pinInFromFilterOut);
							}
						}
					}
					else
					{
						IPin videoDecoderIn = null;
						try
						{
							videoDecoderIn = DsFindPin.ByDirection(this.videoH264DecoderFilter, PinDirection.Input, 0);

							FilterGraphTools.ConnectFilters(this.graphBuilder, pinOut, videoDecoderIn, false);
						}
						finally
						{
							if (videoDecoderIn != null) Marshal.ReleaseComObject(videoDecoderIn);
						}

						IPin videoDecoderOut = null, videoVMRIn = null;
						try
						{
							videoDecoderOut = DsFindPin.ByDirection(this.videoH264DecoderFilter, PinDirection.Output, 0);
							videoVMRIn = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Input, 0);
							FilterGraphTools.ConnectFilters(this.graphBuilder, videoDecoderOut, videoVMRIn, false);
						}
						finally
						{
							if (videoDecoderOut != null) Marshal.ReleaseComObject(videoDecoderOut);
							if (videoVMRIn != null) Marshal.ReleaseComObject(videoVMRIn);
						}
					}
				}
				finally
				{
					Marshal.ReleaseComObject(pinOut);
				}
			}

			hr = this.mpeg2Demux.FindPin("MPG2", out pinOut);
			if (pinOut != null)
			{
				try
				{
					if (this.videoMpeg2DecoderFilter == null)
					{
						IPin pinInFromFilterOut = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Input, 0);
						if (pinInFromFilterOut != null)
						{
							try
							{
								hr = this.graphBuilder.Connect(pinOut, pinInFromFilterOut);
							}
							finally
							{
								Marshal.ReleaseComObject(pinInFromFilterOut);
							}
						}
					}
					else
					{

						IPin videoDecoderIn = null;
						try
						{
							videoDecoderIn = DsFindPin.ByDirection(this.videoMpeg2DecoderFilter, PinDirection.Input, 0);

							FilterGraphTools.ConnectFilters(this.graphBuilder, pinOut, videoDecoderIn, false);
						}
						finally
						{
							if (videoDecoderIn != null) Marshal.ReleaseComObject(videoDecoderIn);
						}

						IPin videoDecoderOut = null, videoVMRIn = null;
						try
						{
							videoDecoderOut = DsFindPin.ByDirection(this.videoMpeg2DecoderFilter, PinDirection.Output, 0);
							videoVMRIn = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Input, 0);
							//FilterGraphTools.ConnectFilters(this.graphBuilder, videoDecoderOut, videoVMRIn, false);
							hr = graphBuilder.ConnectDirect(videoDecoderOut, videoVMRIn, null);
							ThrowExceptionForHR(string.Format("Connecting the video decoder ({0}) to the video renderer ({1}): ", this.Mpeg2DecoderDevice != null ? this.Mpeg2DecoderDevice.Name : "", this.useWPF ? "SampleGraber" : "VMR9"), hr);
						}
						finally
						{
							if (videoDecoderOut != null) Marshal.ReleaseComObject(videoDecoderOut);
							if (videoVMRIn != null) Marshal.ReleaseComObject(videoVMRIn);
						}
					}
				}
				finally
				{
					Marshal.ReleaseComObject(pinOut);
				}
			}

			AddAndConnectNullRendererForWPF();

			hr = this.mpeg2Demux.FindPin("Audio", out pinOut);
			if (pinOut != null)
			{
				hr = this.graphBuilder.Render(pinOut);
				//DsError.ThrowExceptionForHR(hr);
				Marshal.ReleaseComObject(pinOut);
			}
		}

		protected void ConnectTransportStreamFilters()
		{
			int hr = 0;
			IPin pinOut;

			// Connect the MPEG-2 Demux output pin for the "BDA MPEG2 Transport Information Filter"
			//pinOut = DsFindPin.ByDirection(this.mpeg2Demux, PinDirection.Output, 0);
			hr = this.mpeg2Demux.FindPin("1", out pinOut);
			if (pinOut != null)
			{
				hr = this.graphBuilder.Render(pinOut);
				//DsError.ThrowExceptionForHR(hr);
				// In fact the last pin don't render since i havn't added the BDA MPE Filter...
				Marshal.ReleaseComObject(pinOut);
			}

			// Connect the MPEG-2 Demux output pin for the "MPEG-2 Sections and Tables" filter
			//pinOut = DsFindPin.ByDirection(this.mpeg2Demux, PinDirection.Output, 4);
			hr = this.mpeg2Demux.FindPin("5", out pinOut);
			if (pinOut != null)
			{
				hr = this.graphBuilder.Render(pinOut);
				//DsError.ThrowExceptionForHR(hr);
				// In fact the last pin don't render since i havn't added the BDA MPE Filter...
				Marshal.ReleaseComObject(pinOut);
			}
		}

		protected void RenderMpeg2DemuxFilters()
		{
			int hr = 0;
			IPin pinOut;

			// Connect the 5 MPEG-2 Demux output pins
			for (int i = 0; i < 6; i++)
			{
				pinOut = DsFindPin.ByDirection(this.mpeg2Demux, PinDirection.Output, i);

				if (pinOut != null)
				{
					hr = this.graphBuilder.Render(pinOut);
					//DsError.ThrowExceptionForHR(hr);
					// In fact the last pin don't render since i havn't added the BDA MPE Filter...
					Marshal.ReleaseComObject(pinOut);
				}
			}
		}

		protected static MPEG2VideoInfo videoMPEG2PinFormat;
		protected static MPEG2VideoInfo videoH264PinFormat;
		protected static MPEG1WaveFormat audioPinFormat;

		protected static MPEG2VideoInfo GetVideoMPEG2PinFormat()
		{
			if (videoMPEG2PinFormat == null)
			{
				MPEG2VideoInfo videoPinFormat = new MPEG2VideoInfo();
				videoPinFormat.hdr = new VideoInfoHeader2();
				videoPinFormat.hdr.SrcRect = new DsRect();
				videoPinFormat.hdr.SrcRect.left = 0;		//0x00, 0x00, 0x00, 0x00,  //00  .hdr.rcSource.left              = 0x00000000
				videoPinFormat.hdr.SrcRect.top = 0;			//0x00, 0x00, 0x00, 0x00,  //04  .hdr.rcSource.top               = 0x00000000
				videoPinFormat.hdr.SrcRect.right = 0;		//0xD0, 0x02, 0x00, 0x00,  //08  .hdr.rcSource.right             = 0x000002d0 //720
				videoPinFormat.hdr.SrcRect.bottom = 0;		//0x40, 0x02, 0x00, 0x00,  //0c  .hdr.rcSource.bottom            = 0x00000240 //576
				videoPinFormat.hdr.TargetRect = new DsRect();
				videoPinFormat.hdr.TargetRect.left = 0;		//0x00, 0x00, 0x00, 0x00,  //10  .hdr.rcTarget.left              = 0x00000000
				videoPinFormat.hdr.TargetRect.top = 0;		//0x00, 0x00, 0x00, 0x00,  //14  .hdr.rcTarget.top               = 0x00000000
				videoPinFormat.hdr.TargetRect.right = 0;	//0xD0, 0x02, 0x00, 0x00,  //18  .hdr.rcTarget.right             = 0x000002d0 //720
				videoPinFormat.hdr.TargetRect.bottom = 0;	//0x40, 0x02, 0x00, 0x00,  //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
				videoPinFormat.hdr.BitRate = 0x003d0900;	//0x00, 0x09, 0x3D, 0x00,  //20  .hdr.dwBitRate                  = 0x003d0900
				videoPinFormat.hdr.BitErrorRate = 0;		//0x00, 0x00, 0x00, 0x00,  //24  .hdr.dwBitErrorRate             = 0x00000000

				////0x051736=333667-> 10000000/333667 = 29.97fps
				////0x061A80=400000-> 10000000/400000 = 25fps
				videoPinFormat.hdr.AvgTimePerFrame = 400000;				//0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps

				videoPinFormat.hdr.InterlaceFlags = AMInterlace.None;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				////videoPinFormat.hdr.InterlaceFlags = AMInterlace.IsInterlaced | AMInterlace.Field1First; // | AMINTERLACE_DisplayModeBobOnly;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				//videoPinFormat.hdr.InterlaceFlags = AMInterlace.IsInterlaced | AMInterlace.OneFieldPerSample | AMInterlace.DisplayModeBobOnly; // | AMINTERLACE_DisplayModeBobOnly;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				//videoPinFormat.hdr.InterlaceFlags = AMInterlace.IsInterlaced | AMInterlace.DisplayModeBobOnly; // | AMINTERLACE_DisplayModeBobOnly;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				//videoPinFormat.hdr.InterlaceFlags = AMInterlace.IsInterlaced | AMInterlace.FieldPatBothRegular | AMInterlace.DisplayModeWeaveOnly; // | AMINTERLACE_DisplayModeBobOnly;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				//videoPinFormat.hdr.InterlaceFlags = AMInterlace.IsInterlaced | AMInterlace.DisplayModeBobOrWeave; // | AMINTERLACE_DisplayModeBobOnly;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				//videoPinFormat.hdr.InterlaceFlags |= AMInterlace.Field1First;

				videoPinFormat.hdr.CopyProtectFlags = AMCopyProtect.None;	//0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
				videoPinFormat.hdr.PictAspectRatioX = 4;					//0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
				videoPinFormat.hdr.PictAspectRatioY = 3;					//0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
				videoPinFormat.hdr.ControlFlags = AMControl.None;			//0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
				videoPinFormat.hdr.Reserved2 = 0;							//0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
				videoPinFormat.hdr.BmiHeader = new BitmapInfoHeader();
				videoPinFormat.hdr.BmiHeader.Size = 0x00000028;				//0x28, 0x00, 0x00, 0x00,  //44  .hdr.bmiHeader.biSize           = 0x00000028
				videoPinFormat.hdr.BmiHeader.Width = 720;					//0xD0, 0x02, 0x00, 0x00,  //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
				videoPinFormat.hdr.BmiHeader.Height = 576;					//0x40, 0x02, 0x00, 0x00,  //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
				videoPinFormat.hdr.BmiHeader.Planes = 0;					//0x00, 0x00,              //50  .hdr.bmiHeader.biPlanes         = 0x0000
				videoPinFormat.hdr.BmiHeader.BitCount = 0;					//0x00, 0x00,              //54  .hdr.bmiHeader.biBitCount       = 0x0000
				videoPinFormat.hdr.BmiHeader.Compression = 0;				//0x00, 0x00, 0x00, 0x00,  //58  .hdr.bmiHeader.biCompression    = 0x00000000
				videoPinFormat.hdr.BmiHeader.ImageSize = 0;					//0x00, 0x00, 0x00, 0x00,  //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
				videoPinFormat.hdr.BmiHeader.XPelsPerMeter = 0x000007d0;	//0xD0, 0x07, 0x00, 0x00,  //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
				videoPinFormat.hdr.BmiHeader.YPelsPerMeter = 0x0000cf27;	//0x27, 0xCF, 0x00, 0x00,  //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
				videoPinFormat.hdr.BmiHeader.ClrUsed = 0;					//0x00, 0x00, 0x00, 0x00,  //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
				videoPinFormat.hdr.BmiHeader.ClrImportant = 0;				//0x00, 0x00, 0x00, 0x00,  //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
				videoPinFormat.StartTimeCode = 0x0006f498;		//0x98, 0xF4, 0x06, 0x00,    //70  .dwStartTimeCode                = 0x0006f498
				videoPinFormat.SequenceHeader = 0;				//0x00, 0x00, 0x00, 0x00,    //74  .cbSequenceHeader               = 0x00000000
				videoPinFormat.Profile = AM_MPEG2Profile.Main;	//0x02, 0x00, 0x00, 0x00,    //78  .dwProfile                      = 0x00000002
				videoPinFormat.Level = AM_MPEG2Level.Main;		//0x02, 0x00, 0x00, 0x00,    //7c  .dwLevel                        = 0x00000002
				videoPinFormat.Flags = (AMMPEG2)0;				//0x00, 0x00, 0x00, 0x00,    //80  .Flags    

				videoMPEG2PinFormat = videoPinFormat;
			}
			return videoMPEG2PinFormat;
		}

		protected static MPEG2VideoInfo GetVideoH264PinFormat()
		{
			if (videoH264PinFormat == null)
			{
				MPEG2VideoInfo videoPinFormat = new MPEG2VideoInfo();
				videoPinFormat.hdr = new VideoInfoHeader2();
				videoPinFormat.hdr.SrcRect = new DsRect();
				videoPinFormat.hdr.SrcRect.left = 0;		//0x00, 0x00, 0x00, 0x00,  //00  .hdr.rcSource.left              = 0x00000000
				videoPinFormat.hdr.SrcRect.top = 0;			//0x00, 0x00, 0x00, 0x00,  //04  .hdr.rcSource.top               = 0x00000000
				videoPinFormat.hdr.SrcRect.right = 0;		//0xD0, 0x02, 0x00, 0x00,  //08  .hdr.rcSource.right             = 0x000002d0 //720
				videoPinFormat.hdr.SrcRect.bottom = 0;		//0x40, 0x02, 0x00, 0x00,  //0c  .hdr.rcSource.bottom            = 0x00000240 //576
				videoPinFormat.hdr.TargetRect = new DsRect();
				videoPinFormat.hdr.TargetRect.left = 0;		//0x00, 0x00, 0x00, 0x00,  //10  .hdr.rcTarget.left              = 0x00000000
				videoPinFormat.hdr.TargetRect.top = 0;		//0x00, 0x00, 0x00, 0x00,  //14  .hdr.rcTarget.top               = 0x00000000
				videoPinFormat.hdr.TargetRect.right = 0;	//0xD0, 0x02, 0x00, 0x00,  //18  .hdr.rcTarget.right             = 0x000002d0 //720
				videoPinFormat.hdr.TargetRect.bottom = 0;	//0x40, 0x02, 0x00, 0x00,  //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
				videoPinFormat.hdr.BitRate = 0x003d0900;	//0x00, 0x09, 0x3D, 0x00,  //20  .hdr.dwBitRate                  = 0x003d0900
				videoPinFormat.hdr.BitErrorRate = 0;		//0x00, 0x00, 0x00, 0x00,  //24  .hdr.dwBitErrorRate             = 0x00000000

				////0x051736=333667-> 10000000/333667 = 29.97fps
				////0x061A80=400000-> 10000000/400000 = 25fps
				videoPinFormat.hdr.AvgTimePerFrame = 400000;				//0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
				videoPinFormat.hdr.InterlaceFlags = AMInterlace.None;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				//videoPinFormat.hdr.InterlaceFlags = AMInterlace.IsInterlaced | AMInterlace.OneFieldPerSample | AMInterlace.DisplayModeBobOnly;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				//videoPinFormat.hdr.InterlaceFlags = AMInterlace.IsInterlaced | AMInterlace.DisplayModeBobOnly;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				//videoPinFormat.hdr.InterlaceFlags = AMInterlace.IsInterlaced | AMInterlace.FieldPatBothRegular | AMInterlace.DisplayModeWeaveOnly;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				//videoPinFormat.hdr.InterlaceFlags = AMInterlace.IsInterlaced | AMInterlace.DisplayModeBobOrWeave;		//0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
				videoPinFormat.hdr.CopyProtectFlags = AMCopyProtect.None;	//0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
				videoPinFormat.hdr.PictAspectRatioX = 4;					//0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
				videoPinFormat.hdr.PictAspectRatioY = 3;					//0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
				videoPinFormat.hdr.ControlFlags = AMControl.None;			//0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
				videoPinFormat.hdr.Reserved2 = 0;							//0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
				videoPinFormat.hdr.BmiHeader = new BitmapInfoHeader();
				videoPinFormat.hdr.BmiHeader.Size = 0x00000028;				//0x28, 0x00, 0x00, 0x00,  //44  .hdr.bmiHeader.biSize           = 0x00000028
				videoPinFormat.hdr.BmiHeader.Width = 720;					//0xD0, 0x02, 0x00, 0x00,  //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
				videoPinFormat.hdr.BmiHeader.Height = 576;					//0x40, 0x02, 0x00, 0x00,  //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
				videoPinFormat.hdr.BmiHeader.Planes = 0; // 1 ?					//0x00, 0x00,              //50  .hdr.bmiHeader.biPlanes         = 0x0000
				videoPinFormat.hdr.BmiHeader.BitCount = 0;					//0x00, 0x00,              //54  .hdr.bmiHeader.biBitCount       = 0x0000
				videoPinFormat.hdr.BmiHeader.Compression = 0;				//0x00, 0x00, 0x00, 0x00,  //58  .hdr.bmiHeader.biCompression    = 0x00000000
				videoPinFormat.hdr.BmiHeader.ImageSize = 0;					//0x00, 0x00, 0x00, 0x00,  //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
				videoPinFormat.hdr.BmiHeader.XPelsPerMeter = 0x000007d0;	//0xD0, 0x07, 0x00, 0x00,  //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
				videoPinFormat.hdr.BmiHeader.YPelsPerMeter = 0x0000cf27;	//0x27, 0xCF, 0x00, 0x00,  //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
				videoPinFormat.hdr.BmiHeader.ClrUsed = 0;					//0x00, 0x00, 0x00, 0x00,  //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
				videoPinFormat.hdr.BmiHeader.ClrImportant = 0;				//0x00, 0x00, 0x00, 0x00,  //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
				videoPinFormat.StartTimeCode = 0x0006f498;		//0x98, 0xF4, 0x06, 0x00,    //70  .dwStartTimeCode                = 0x0006f498
				videoPinFormat.SequenceHeader = 0;				//0x00, 0x00, 0x00, 0x00,    //74  .cbSequenceHeader               = 0x00000000
				videoPinFormat.Profile = AM_MPEG2Profile.Main;	//0x02, 0x00, 0x00, 0x00,    //78  .dwProfile                      = 0x00000002
				videoPinFormat.Level = AM_MPEG2Level.Main;		//0x02, 0x00, 0x00, 0x00,    //7c  .dwLevel                        = 0x00000002
				videoPinFormat.Flags = (AMMPEG2)0;				//0x00, 0x00, 0x00, 0x00,    //80  .Flags    

				videoH264PinFormat = videoPinFormat;
			}
			return videoH264PinFormat;
		}

		protected static MPEG1WaveFormat GetAudioPinFormat()
		{
			if (audioPinFormat == null)
			{
				audioPinFormat = new MPEG1WaveFormat();
				audioPinFormat.wFormatTag = 0x0050; // WAVE_FORMAT_MPEG
				audioPinFormat.nChannels = 2;
				audioPinFormat.nSamplesPerSec = 48000;
				audioPinFormat.nAvgBytesPerSec = 32000;
				audioPinFormat.nBlockAlign = 768;
				audioPinFormat.wBitsPerSample = 16;
				audioPinFormat.cbSize = 22; // extra size

				audioPinFormat.HeadLayer = 2;
				audioPinFormat.HeadBitrate = 0x00177000;
				audioPinFormat.HeadMode = 1;
				audioPinFormat.HeadModeExt = 1;
				audioPinFormat.HeadEmphasis = 1;
				audioPinFormat.HeadFlags = 0x1c;
				audioPinFormat.PTSLow = 0;
				audioPinFormat.PTSHigh = 0;
			}

			return audioPinFormat;
		}

		protected override void Decompose()
		{
			if (this.graphBuilder != null)
			{
				int hr = 0;

				OnGraphEnded();

				this.epg.UnRegisterEvent();

				// Decompose the graph
				try { hr = (this.graphBuilder as IMediaControl).StopWhenReady(); }
				catch { }
				try { hr = (this.graphBuilder as IMediaControl).Stop(); }
				catch { }
				RemoveHandlers();


				FilterGraphTools.RemoveAllFilters(this.graphBuilder);


				if (this.networkProvider != null) Marshal.ReleaseComObject(this.networkProvider); this.networkProvider = null;
				if (this.mpeg2Demux != null) Marshal.ReleaseComObject(this.mpeg2Demux); this.mpeg2Demux = null;
				if (this.tuner != null) Marshal.ReleaseComObject(this.tuner); this.tuner = null;
				if (this.capture != null) Marshal.ReleaseComObject(this.capture); this.capture = null;
				if (this.bdaTIF != null) Marshal.ReleaseComObject(this.bdaTIF); this.bdaTIF = null;
				if (this.bdaSecTab != null) Marshal.ReleaseComObject(this.bdaSecTab); this.bdaSecTab = null;
				if (this.audioDecoderFilter != null) Marshal.ReleaseComObject(this.audioDecoderFilter); this.audioDecoderFilter = null;
				if (this.videoH264DecoderFilter != null) Marshal.ReleaseComObject(this.videoH264DecoderFilter); this.videoH264DecoderFilter = null;
				if (this.videoMpeg2DecoderFilter != null) Marshal.ReleaseComObject(this.videoMpeg2DecoderFilter); this.videoMpeg2DecoderFilter = null;
				if (this.audioRenderer != null) Marshal.ReleaseComObject(this.audioRenderer); this.audioRenderer = null;
				if (this.videoRenderer != null) Marshal.ReleaseComObject(this.videoRenderer); this.videoRenderer = null;
				if (this.captureGraphBuilder != null) Marshal.ReleaseComObject(this.captureGraphBuilder); this.captureGraphBuilder = null;

				if (useWPF)
					WPFStop();

				try { rot.Dispose(); }
				catch { }
				try { Marshal.ReleaseComObject(this.graphBuilder); this.graphBuilder = null; }
				catch { }
			}
		}

		public override bool GetSignalStatistics(out bool locked, out bool present, out int strength, out int quality)
		{
			int longVal = strength = quality = 0;
			bool byteVal = locked = present = false;

			//Get IID_IBDA_Topology
			IBDA_Topology bdaNetTop = this.tuner as IBDA_Topology;
			if (bdaNetTop == null)
			{
				return false;
			}

			int nodeTypes;
			int[] nodeType = new int[32];
			int hr = bdaNetTop.GetNodeTypes(out nodeTypes, 32, nodeType);
			DsError.ThrowExceptionForHR(hr);

			for (int i = 0; i < nodeTypes; i++)
			{
				object iNode;
				hr = bdaNetTop.GetControlNode(0, 1, nodeType[i], out iNode);
				if (hr == 0)
				{
					IBDA_SignalStatistics pSigStats = iNode as IBDA_SignalStatistics;
					if (pSigStats != null)
					{
						longVal = 0;
						if (pSigStats.get_SignalStrength(out longVal) == 0)
							strength = longVal;

						longVal = 0;
						if (pSigStats.get_SignalQuality(out longVal) == 0)
							quality = longVal;

						byteVal = false;
						if (pSigStats.get_SignalLocked(out byteVal) == 0)
							locked = byteVal;

						byteVal = false;
						if (pSigStats.get_SignalPresent(out byteVal) == 0)
							present = byteVal;
					}
					Marshal.ReleaseComObject(iNode);
					return true;
				}
			}
			return false;
		}

		public string GetTablesInfos(Channel channel, bool allTransponderInfo)
		{
			string result = "";
			if (channel != null && channel is ChannelDVB)
			{
				ChannelDVB channelDVB = channel as ChannelDVB;

				IMpeg2Data mpeg2Data = this.bdaSecTab as IMpeg2Data;
				// Hervé Stalin : Utile ?
				//Hashtable serviceNameByServiceId = new Hashtable(); 
				PSISection[] psiSdts = PSISection.GetPSITable((int)PIDS.SDT, (int)TABLE_IDS.SDT_ACTUAL, mpeg2Data);
				for (int i = 0; i < psiSdts.Length; i++)
				{
					PSISection psiSdt = psiSdts[i];
					if (psiSdt != null && psiSdt is PSISDT)
					{
						PSISDT sdt = (PSISDT)psiSdt;
						if (allTransponderInfo)
						{
							result += "PSI Table " + i + "/" + psiSdts.Length + "\r\n";
							result += sdt.ToString();
						}
						// Hervé Stalin : Utile ?
						//foreach (PSISDT.Data service in sdt.Services)
						//{
						//    serviceNameByServiceId[service.ServiceId] = service.GetServiceName();
						//}
					}
				}

				//Hervé Stalin : Code pode pour créér un hashtable de lcn
				//Hashtable logicalChannelNumberByServiceId = new Hashtable();
				PSISection[] psiNits = PSISection.GetPSITable((int)PIDS.NIT, (int)TABLE_IDS.NIT_ACTUAL, mpeg2Data);
				for (int i = 0; i < psiNits.Length; i++)
				{
					PSISection psinit = psiNits[i];
					if (psinit != null && psinit is PSINIT)
					{
						PSINIT nit = (PSINIT)psinit;
						result += "PSI Table " + i + "/" + psiNits.Length + "\r\n";
						result += nit.ToString();

						//foreach (PSINIT.Data data in nit.Streams)
						//{
						//    foreach (PSIDescriptor psiDescriptorData in data.Descriptors)
						//    {
						//        if (psiDescriptorData.DescriptorTag == DESCRIPTOR_TAGS.DESCR_LOGICAL_CHANNEL)
						//        {
						//            PSIDescriptorLogicalChannel psiDescriptorLogicalChannel = (PSIDescriptorLogicalChannel)psiDescriptorData;
						//            foreach (PSIDescriptorLogicalChannel.ChannelNumber f in psiDescriptorLogicalChannel.LogicalChannelNumbers)
						//            {
						//                logicalChannelNumberByServiceId[f.ServiceID] = f.LogicalChannelNumber;
						//            }

						//        }
						//    }
						//}

					}
				}


				PSISection[] psiPats = PSISection.GetPSITable((int)PIDS.PAT, (int)TABLE_IDS.PAT, mpeg2Data);
				for (int i = 0; i < psiPats.Length; i++)
				{
					PSISection psiPat = psiPats[i];
					if (psiPat != null && psiPat is PSIPAT)
					{
						PSIPAT pat = (PSIPAT)psiPat;
						if (allTransponderInfo)
						{
							result += "PSI Table " + i + "/" + psiPats.Length + "\r\n";
							result += pat.ToString();
						}

						foreach (PSIPAT.Data program in pat.ProgramIds)
						{
							if (allTransponderInfo || program.ProgramNumber == channelDVB.SID)
							{
								if (!program.IsNetworkPID)
								{
									PSISection[] psiPmts = PSISection.GetPSITable(program.Pid, (int)TABLE_IDS.PMT, mpeg2Data);
									for (int i2 = 0; i2 < psiPmts.Length; i2++)
									{
										PSISection psiPmt = psiPmts[i2];
										if (psiPmt != null && psiPmt is PSIPMT)
										{
											PSIPMT pmt = (PSIPMT)psiPmt;
											result += "PSI Table " + i2 + "/" + psiPmts.Length + "\r\n";
											result += pmt.ToString();
										}
										if (!allTransponderInfo) return result;
									}
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}
