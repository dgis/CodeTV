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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

using Microsoft.Win32;

using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;

namespace CodeTV
{
	class GraphBuilderWDM : GraphBuilderTV, IWDM
	{
		protected IBaseFilter videoCaptureFilter;
		protected IBaseFilter audioCaptureFilter;

		protected IAMTVTuner tuner;
		protected IAMTVAudio amTVAudio;
		protected IBaseFilter crossbar;
		protected CrossbarHelper crossbarHelper;

		private static Dictionary<string, DsDevice> wdmVideoInputDevices;
		private static Dictionary<string, DsDevice> wdmAudioInputDevices;

		protected DsDevice videoCaptureDevice;
		protected DsDevice audioCaptureDevice;

		private ChannelAnalogic.CaptureFormat captureFormat = new ChannelAnalogic.CaptureFormat();

		public static Dictionary<string, DsDevice> VideoInputDevices
		{
			get
			{
				if (wdmVideoInputDevices == null)
				{
					wdmVideoInputDevices = new Dictionary<string, DsDevice>();

					DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
					foreach (DsDevice d in devices)
						if (d.Name != null)
							wdmVideoInputDevices.Add(d.Name, d);
				}
				return wdmVideoInputDevices;
			}
		}
		public static Dictionary<string, DsDevice> AudioInputDevices
		{
			get
			{
				if (wdmAudioInputDevices == null)
				{
					wdmAudioInputDevices = new Dictionary<string, DsDevice>();

					//DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.KSAudioDevice);
					DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
					foreach (DsDevice d in devices)
						if (d.Name != null)
							wdmAudioInputDevices.Add(d.Name, d);
				}
				return wdmAudioInputDevices;
			}
		}

		public DsDevice VideoCaptureDevice { get { return this.videoCaptureDevice; } set { this.videoCaptureDevice = value; } }
		public DsDevice AudioCaptureDevice { get { return this.audioCaptureDevice; } set { this.audioCaptureDevice = value; } }

		public IBaseFilter VideoCaptureFilter { get { return this.videoCaptureFilter; } }
		public IBaseFilter AudioCaptureFilter { get { return this.audioCaptureFilter; } }

		public IAMTVTuner Tuner { get { return this.tuner; } }
		public IAMTVAudio TVAudio { get { return this.amTVAudio; } }
		public IBaseFilter Crossbar { get { return this.crossbar; } }
		public CrossbarHelper WDMCrossbar { get { return this.crossbarHelper; } }

		public ChannelAnalogic.CaptureFormat FormatOfCapture { get { return this.captureFormat; } set { this.captureFormat = value; } }

		public GraphBuilderWDM(VideoControl renderingControl)
			: base(renderingControl)
		{
		}

		public override void BuildGraph()
		{
			useWPF = Settings.UseWPF;

			try
			{
				this.graphBuilder = (IFilterGraph2)new FilterGraph();
				rot = new DsROTEntry(this.graphBuilder);

				AddRenderers();
				if (!useWPF)
					ConfigureVMR9InWindowlessMode();
				AddAndConnectWDMBoardFilters();
				if (this.videoCaptureFilter == null)
					throw new ApplicationException("No video capture devices found!");

				GetControlInterface();

				this.hostingControl.CurrentGraphBuilder = this;

				OnGraphStarted();
			}
			catch (Exception ex)
			{
				Decompose();
				throw ex;
			}
		}

		private void GetControlInterface()
		{
			this.tuner = null;
			this.crossbar = null;

			object o;

			int hr = this.captureGraphBuilder.FindInterface(null, null, this.videoCaptureFilter, typeof(IAMTVTuner).GUID, out o);
			if (hr >= 0)
			{
				this.tuner = o as IAMTVTuner;
				o = null;

				hr = this.captureGraphBuilder.FindInterface(null, null, this.videoCaptureFilter, typeof(IAMCrossbar).GUID, out o);
				if (hr >= 0)
				{
					this.crossbar = o as IBaseFilter;
					o = null;
				}

				// Use the crossbar class to help us sort out all the possible video inputs
				// The class needs to be given the capture filters ANALOGVIDEO input pin
				IPin pinVideo = DsFindPin.ByCategory(this.videoCaptureFilter, PinCategory.AnalogVideoIn, 0);
				if (pinVideo != null)
				{
					try
					{
						this.crossbarHelper = new CrossbarHelper(pinVideo);
					}
					catch{}
					Marshal.ReleaseComObject(pinVideo);
				}

				hr = this.captureGraphBuilder.FindInterface(null, null, this.videoCaptureFilter, typeof(IAMTVAudio).GUID, out o);
				if (hr >= 0)
				{
					this.amTVAudio = o as IAMTVAudio;
					o = null;
				}
			}
		}

		private void SetCaptureResolution(ChannelAnalogic.CaptureFormat captureFormat) //Size captureResolution)
		{
			object o = null;
			int hr = this.captureGraphBuilder.FindInterface(null, //PinCategory.Preview, // Preview pin.
				MediaType.Video, //null,    // Any media type.
				this.videoCaptureFilter, // Pointer to the capture filter.
				typeof(IAMStreamConfig).GUID,
				out o);
			if (hr >= 0)
			{
				IAMStreamConfig amStreamConfig = o as IAMStreamConfig;

				AMMediaType mediaType;
				hr = amStreamConfig.GetFormat(out mediaType);
				if (hr >= 0)
				{
					if ((mediaType.majorType == MediaType.Video) &&
						(mediaType.formatType == FormatType.VideoInfo) &&
						(mediaType.formatSize >= Marshal.SizeOf(typeof(VideoInfoHeader))) &&
						(mediaType.formatPtr != IntPtr.Zero))
					{
						VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));

						Size resolution = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
						int framePerSecond = (int)(10000000.0 / videoInfoHeader.AvgTimePerFrame);
						string mediaSubType = (string)DeviceEnumerator.MediaSubTypeByGUID[mediaType.subType];

						if (captureFormat.Resolution == resolution &&
							captureFormat.FramePerSecond == framePerSecond &&
							captureFormat.MediaSubType == mediaSubType)
							return;
					}
					DsUtils.FreeAMMediaType(mediaType);
				}


				int iCount = 0, iSize = 0;
				hr = amStreamConfig.GetNumberOfCapabilities(out iCount, out iSize);

				// Check the size to make sure we pass in the correct structure.
				if (iSize == Marshal.SizeOf(typeof(VideoStreamConfigCaps)))
				{
					// Use the video capabilities structure.

					VideoStreamConfigCaps scc = new VideoStreamConfigCaps();
					GCHandle gchScc = GCHandle.Alloc(scc, GCHandleType.Pinned);
					IntPtr pScc = gchScc.AddrOfPinnedObject();

					for (int iFormat = 0; iFormat < iCount; iFormat++)
					{
						hr = amStreamConfig.GetStreamCaps(iFormat, out mediaType, pScc);
						if (hr >= 0)
						{
							if (mediaType != null &&
								mediaType.majorType == MediaType.Video &&
								mediaType.formatType == FormatType.VideoInfo &&
								mediaType.formatSize >= Marshal.SizeOf(typeof(VideoInfoHeader)) &&
								mediaType.formatPtr != IntPtr.Zero)
							{
								VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));

								Size resolution = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
								int framePerSecond = (int)(10000000.0 / videoInfoHeader.AvgTimePerFrame);
								string mediaSubType = (string)DeviceEnumerator.MediaSubTypeByGUID[mediaType.subType];

								if (captureFormat.Resolution == resolution &&
									captureFormat.FramePerSecond == framePerSecond &&
									captureFormat.MediaSubType == mediaSubType)
								{
									StopGraph();

									hr = amStreamConfig.SetFormat(mediaType);
									break;
								}
								DsUtils.FreeAMMediaType(mediaType);
							}
						}
					}
					gchScc.Free();
				}
			}
		}

		public override void SubmitTuneRequest(Channel channel)
		{
			if (channel is ChannelAnalogic)
			{
				ChannelAnalogic channelAnalogic = channel as ChannelAnalogic;

				int hr = 0;

				//SetCaptureResolution(this.captureFormat); // channelAnalogic.CaptureResolution);

				if (this.videoCaptureFilter != null)
				{
					IAMAnalogVideoDecoder analogVideoDecoder = this.videoCaptureFilter as IAMAnalogVideoDecoder;
					if (analogVideoDecoder != null)
					{
						AnalogVideoStandard videoStandard;
						hr = analogVideoDecoder.get_TVFormat(out videoStandard);
						if (videoStandard != channelAnalogic.VideoStandard)
							hr = analogVideoDecoder.put_TVFormat(channelAnalogic.VideoStandard);
					}
				}

				if (this.tuner != null)
				{
					int tuningSpace = 0;
					hr = this.tuner.get_TuningSpace(out tuningSpace);
					if (hr < 0 || channelAnalogic.TuningSpace != tuningSpace)
						this.tuner.put_TuningSpace(channelAnalogic.TuningSpace);

					int countryCode = 0;
					hr = this.tuner.get_CountryCode(out countryCode);
					if (hr < 0 || channelAnalogic.CountryCode != countryCode)
						this.tuner.put_CountryCode(channelAnalogic.CountryCode);

					int connectInput = 0;
					hr = this.tuner.get_ConnectInput(out connectInput);
					if (hr < 0 || channelAnalogic.ConnectInput != connectInput)
						this.tuner.put_ConnectInput(channelAnalogic.ConnectInput);

					TunerInputType inputType = TunerInputType.Antenna;
					hr = this.tuner.get_InputType(channelAnalogic.ConnectInput, out inputType);
					if (hr < 0 || channelAnalogic.InputType != inputType)
						this.tuner.put_InputType(channelAnalogic.ConnectInput, channelAnalogic.InputType);

					AMTunerModeType mode = AMTunerModeType.Default;
					hr = this.tuner.get_Mode(out mode);
					if (hr < 0 || channelAnalogic.Mode != mode)
						this.tuner.put_Mode(channelAnalogic.Mode);

					if (channelAnalogic.Channel > 0)
					{
						hr = this.tuner.put_Channel(channelAnalogic.Channel, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
						DsError.ThrowExceptionForHR(hr);
					}

					//int plChannel;
					//AMTunerSubChannel plVideoSubChannel;
					//AMTunerSubChannel plAudioSubChannel;
					//hr = this.tuner.get_Channel(out plChannel, out plVideoSubChannel, out plAudioSubChannel);

					//if (plChannel != channelAnalogic.Channel)
					//{
					//    DsError.ThrowExceptionForHR(-2147024809);
					//}
				}

				if (this.amTVAudio != null && channelAnalogic.AudioMode != TVAudioMode.None)
				{
					TVAudioMode audioMode = TVAudioMode.Stereo;
					hr = this.amTVAudio.get_TVAudioMode(out audioMode);
					if (hr < 0 || channelAnalogic.AudioMode != audioMode)
						this.amTVAudio.put_TVAudioMode(channelAnalogic.AudioMode);
				}


				if (WDMCrossbar != null)
				{
					try
					{
						if (channelAnalogic.AudioSource != WDMCrossbar.GetAudioInput())
							WDMCrossbar.SetAudioInput(channelAnalogic.AudioSource, false);
					}
					catch { }

					try
					{
						if (channelAnalogic.VideoSource != WDMCrossbar.GetVideoInput())
							WDMCrossbar.SetVideoInput(channelAnalogic.VideoSource, false);
					}
					catch { }
				}

				RunGraph();
			}

			if (useWPF)
				WpfUpdateVideoSize();
		}

		private void AddAndConnectWDMBoardFilters()
		{
			int hr = 0;
			DsDevice[] devices;
			Guid iid = typeof(IBaseFilter).GUID;

			this.captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
			captureGraphBuilder.SetFiltergraph(this.graphBuilder);

			try
			{
				if (this.VideoCaptureDevice != null)
				{
					IBaseFilter tmp;

					object o;
					this.VideoCaptureDevice.Mon.BindToObject(null, null, ref iid, out o);
					tmp = o as IBaseFilter;
					//MessageBox.Show(this.VideoInputDevice.DevicePath);
					//Add the Video input device to the graph
					hr = graphBuilder.AddFilter(tmp, this.VideoCaptureDevice.Name);

					this.videoCaptureFilter = tmp;
					SetCaptureResolution(this.captureFormat); // this.captureResolution);
					this.videoCaptureFilter = null;

					//Render any preview pin of the device
					//int hr1 = captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Video, tmp, null, this.videoRenderer);
					int hr1 = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, tmp, null, this.videoRenderer);

					if (hr >= 0 && hr1 >= 0)
					{
						// Got it !
						this.videoCaptureFilter = tmp;
					}
					else
					{
						// Try another...
						int hr2 = graphBuilder.RemoveFilter(tmp);
						Marshal.ReleaseComObject(tmp);
						if (hr >= 0 && hr1 < 0)
							DsError.ThrowExceptionForHR(hr1);
						DsError.ThrowExceptionForHR(hr);
						return;
					}
				}
				else
				{
					// Enumerate WDM Source filters category and found one that can connect to the network provider
					devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
					for (int i = 0; i < devices.Length; i++)
					{
						IBaseFilter tmp;

						object o;
						devices[i].Mon.BindToObject(null, null, ref iid, out o);
						tmp = o as IBaseFilter;

						//Add the Video input device to the graph
						hr = graphBuilder.AddFilter(tmp, devices[i].Name);

						//Render any preview pin of the device
						//int hr1 = captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Video, tmp, null, this.videoRenderer);
						int hr1 = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, tmp, null, this.videoRenderer);

						if (hr >= 0 && hr1 >= 0)
						{
							// Got it !
							this.videoCaptureFilter = tmp;
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
				// Assume we found a video filter...

				AddAndConnectNullRendererForWPF();
				//if (useWPF)
				//{
				//    IPin pinOutFromFilterOut = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Output, 0);
				//    if (pinOutFromFilterOut != null)
				//    {
				//        hr = this.graphBuilder.Render(pinOutFromFilterOut);
				//        Marshal.ReleaseComObject(pinOutFromFilterOut);
				//    }
				//}

				if (this.AudioCaptureDevice != null)
				{
					if (this.VideoCaptureDevice != null && this.videoCaptureFilter != null && this.AudioCaptureDevice.DevicePath == this.VideoCaptureDevice.DevicePath)
					{
						//Render any preview pin of the device
						int hr1 = captureGraphBuilder.RenderStream(null, MediaType.Audio, this.videoCaptureFilter, null, this.audioRenderer);
						DsError.ThrowExceptionForHR(hr);

						if (hr1 >= 0)
						{
							// Got it !
							this.audioCaptureFilter = null; // this.videoCaptureFilter;
						}
						else
						{
							// No audio?
						}
					}
					else
					{
						IBaseFilter tmp;

						object o;
						this.AudioCaptureDevice.Mon.BindToObject(null, null, ref iid, out o);
						tmp = o as IBaseFilter;

						//Add the audio input device to the graph
						hr = graphBuilder.AddFilter(tmp, this.AudioCaptureDevice.Name);
						DsError.ThrowExceptionForHR(hr);

						//Render any preview pin of the device
						int hr1 = captureGraphBuilder.RenderStream(null, MediaType.Audio, tmp, null, this.audioRenderer);
						DsError.ThrowExceptionForHR(hr);

						if (hr >= 0 && hr1 >= 0)
						{
							// Got it !
							this.audioCaptureFilter = tmp;
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
				}
				else
				{
					// Then enumerate WDM AudioInputDevice category
					//devices = DsDevice.GetDevicesOfCat(FilterCategory.AudioInputDevice);
					devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);

					for (int i = 0; i < devices.Length; i++)
					{
						IBaseFilter tmp;

						object o;
						devices[i].Mon.BindToObject(null, null, ref iid, out o);
						tmp = o as IBaseFilter;

						//Add the audio input device to the graph
						hr = graphBuilder.AddFilter(tmp, devices[i].Name);
						DsError.ThrowExceptionForHR(hr);

						//Render any preview pin of the device
						int hr1 = captureGraphBuilder.RenderStream(null, MediaType.Audio, tmp, null, this.audioRenderer);

						if (hr >= 0 && hr1 >= 0)
						{
							// Got it !
							this.audioCaptureFilter = tmp;

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
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(trace.TraceError, ex.ToString());
			}
			finally
			{
			}
		}

		protected override void Decompose()
		{
			if (this.graphBuilder != null)
			{
				int hr = 0;

				OnGraphEnded();

				// Decompose the graph
				try { hr = (this.graphBuilder as IMediaControl).StopWhenReady(); }
				catch { }
				try { hr = (this.graphBuilder as IMediaControl).Stop(); }
				catch { }
				RemoveHandlers();


				FilterGraphTools.RemoveAllFilters(this.graphBuilder);

				if (this.crossbarHelper != null) this.crossbarHelper.Dispose(); this.crossbarHelper = null;
				if (this.tuner != null) Marshal.ReleaseComObject(this.tuner); this.tuner = null;
				if (this.amTVAudio != null) Marshal.ReleaseComObject(this.amTVAudio); this.amTVAudio = null;
				if (this.crossbar != null) Marshal.ReleaseComObject(this.crossbar); this.crossbar = null;
				if (this.videoCaptureFilter != null) Marshal.ReleaseComObject(this.videoCaptureFilter); this.videoCaptureFilter = null;
				if (this.audioCaptureFilter != null) Marshal.ReleaseComObject(this.audioCaptureFilter); this.audioCaptureFilter = null;
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

		//protected override void ConfigureVMR9InWindowlessMode(int numberOfStream)
		//{
		//    int hr = 0;
		//    IVMRFilterConfig9 filterConfig = this.videoRenderer as IVMRFilterConfig9;

		//    // Configure VMR-9 in Windowsless mode
		//    hr = filterConfig.SetRenderingMode(VMR9Mode.Windowless);
		//    DsError.ThrowExceptionForHR(hr);

		//    // With 1 input stream
		//    hr = filterConfig.SetNumberOfStreams(numberOfStream);
		//    DsError.ThrowExceptionForHR(hr);

		//    IVMRWindowlessControl9 windowlessControl = this.videoRenderer as IVMRWindowlessControl9;

		//    // The main form is hosting the VMR-9
		//    hr = windowlessControl.SetVideoClippingWindow(this.hostingControl.Handle);
		//    DsError.ThrowExceptionForHR(hr);

		//    // Keep the aspect-ratio OK
		//    //hr = windowlessControl.SetAspectRatioMode(VMR9AspectRatioMode.LetterBox);
		//    hr = windowlessControl.SetAspectRatioMode(VMR9AspectRatioMode.None);
		//    DsError.ThrowExceptionForHR(hr);

		//    // Init the VMR-9 with default size values
		//    OnResizeMoveHandler(null, null);

		//    // Add Windows Messages handlers
		//    AddHandlers();
		//}

		public int FindCrossbarPin(IAMCrossbar pXBar, PhysicalConnectorType PhysicalType,
				PinDirection Dir, out int pIndex)
		{
			pIndex = -1;
			bool bInput = (Dir == PinDirection.Input ? true : false);

			// Find out how many pins the crossbar has.
			int cOut, cIn;
			int hr = pXBar.get_PinCounts(out cOut, out cIn);
			if (hr < 0) return hr;
			// Enumerate pins and look for a matching pin.
			int count = (bInput ? cIn : cOut);
			for (int i = 0; i < count; i++)
			{
				int iRelated = 0;
				PhysicalConnectorType ThisPhysicalType = 0;
				hr = pXBar.get_CrossbarPinInfo(bInput, i, out iRelated, out ThisPhysicalType);
				if (hr >= 0 && ThisPhysicalType == PhysicalType)
				{
					// Found a match, return the index.
					pIndex = i;
					return 0;
				}
			}
			// Did not find a matching pin.
			return -1;
		}

		public int ConnectAudio(IAMCrossbar pXBar, bool bActivate)
		{
			// Look for the Audio Decoder output pin.
			int i = 0;
			int hr = FindCrossbarPin(pXBar, PhysicalConnectorType.Audio_AudioDecoder,
				PinDirection.Output, out i);
			if (hr >= 0)
			{
				if (bActivate)  // Activate the audio. 
				{
					// Look for the Audio Tuner input pin.
					int j = 0;
					hr = FindCrossbarPin(pXBar, PhysicalConnectorType.Audio_Tuner, PinDirection.Input, out j);
					if (hr >= 0)
					{
						return pXBar.Route(i, j);
					}
				}
				else  // Mute the audio
				{
					return pXBar.Route(i, -1);
				}
			}
			return -1;
		}



		public override bool GetSignalStatistics(out bool locked, out bool present, out int strength, out int quality)
		{
			strength = quality = 0;
			locked = present = false;

			if (this.Tuner != null)
			{
				AMTunerSignalStrength signalStrength = AMTunerSignalStrength.NoSignal;
				this.Tuner.SignalPresent(out signalStrength);
				switch (signalStrength)
				{
					case AMTunerSignalStrength.HasNoSignalStrength:
						break;
					case AMTunerSignalStrength.NoSignal:
						break;
					case AMTunerSignalStrength.SignalPresent:
						locked = present = true;
						strength = quality = 100;
						return true;
				}
			}
			else
			{
				locked = present = true;
				strength = quality = 100;
				return true;
			}
			return false;
		}
	}
}
