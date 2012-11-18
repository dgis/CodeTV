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

using System.Threading;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;
using MediaFoundation.EVR;
using MediaFoundation;
//using MediaFoundation.Misc;

namespace CodeTV
{
	public class GraphBuilderBase : IDisposable, VideoControl.IVideoEventHandler, ISampleGrabberCB
    {
		public static TraceSwitch trace = new TraceSwitch("GraphBuilder", "GraphBuilder traces", "Info");

		protected Settings settings;

		protected VideoControl hostingControl;
        protected bool useWPF = true;
        protected bool useEVR = false;
		//protected DirectDraw directDraw = new DirectDraw();

		protected static Dictionary<string, DsDevice> audioRendererDevices;
		protected DsDevice audioRendererDevice;

		protected IFilterGraph2 graphBuilder;
		protected DsROTEntry rot;

		protected IBaseFilter audioRenderer;
		protected IBaseFilter videoRenderer;

        //For EVR
        protected CodeTV.IMFVideoDisplayControl evrVideoDisplayControl;


		protected Size currentVideoSourceSize;
		protected Rectangle currentVideoTargetRectangle;
		protected VideoSizeMode videoZoomMode = VideoSizeMode.FromInside;
		protected bool videoKeepAspectRatio = true;
		protected PointF videoOffset = new PointF(0.5f, 0.5f);
		protected double videoZoom = 1.0;
		protected double videoAspectRatioFactor = 1.0;
		protected bool useVideo169Mode = false;

		public Settings Settings { get { return this.settings; } set { this.settings = value; } }

		public static Dictionary<string, DsDevice> AudioRendererDevices
		{
			get
			{
				if (audioRendererDevices == null)
				{
					audioRendererDevices = new Dictionary<string, DsDevice>();

					DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AudioRendererCategory);
					foreach (DsDevice d in devices)
						audioRendererDevices.Add(d.Name, d);
				}
				return audioRendererDevices;
			}
		}

		public DsDevice AudioRendererDevice { get { return this.audioRendererDevice; } set { this.audioRendererDevice = value; } }

        public virtual IFilterGraph2 FilterGraph { get { return this.graphBuilder; } }
		public IBaseFilter AudioRenderer { get { return this.audioRenderer; } }
		public IBaseFilter VideoRenderer { get { return this.videoRenderer; } }

		public Size CurrentVideoSourceSize { get { return this.currentVideoSourceSize; } set { this.currentVideoSourceSize = value; } }
		public Rectangle CurrentVideoTargetRectangle { get { return this.currentVideoTargetRectangle; } set { this.currentVideoTargetRectangle = value; } }
		public VideoSizeMode VideoZoomMode { get { return this.videoZoomMode; } set { this.videoZoomMode = value; } }
		public bool VideoKeepAspectRatio { get { return this.videoKeepAspectRatio; } set { this.videoKeepAspectRatio = value; } }
		public PointF VideoOffset { get { return this.videoOffset; } set { this.videoOffset = value; } }
		public double VideoZoom { get { return this.videoZoom; } set { this.videoZoom = value; } }
		public double VideoAspectRatioFactor { get { return this.videoAspectRatioFactor; } set { this.videoAspectRatioFactor = value; } }
		public bool UseVideo169Mode { get { return this.useVideo169Mode; } set { this.useVideo169Mode = value; } }

		public event EventHandler GraphStarted;
		public event EventHandler GraphEnded;
		public event EventHandler<PossibleEventArgs> PossibleChanged;

		public class PossibleEventArgs: EventArgs
		{
			public PossibleEventArgs(string possible, bool isPossible) { this.possible = possible; this.isPossible = isPossible; }
			public string possible;
			public bool isPossible;
		}

		protected void OnPossibleChanged(string possible, bool isPossible)
		{
			if (PossibleChanged != null)
				PossibleChanged(this, new PossibleEventArgs(possible, isPossible));
		}

		protected bool isPossibleGraphRun = false;
		protected bool isPossibleGraphPause = false;
		protected bool isPossibleGraphStop = false;
		protected bool isPossibleGraphRestart = false;
		protected bool isPossibleGraphRelease = false;

		protected bool isPossibleSetSpeed = false;
		protected bool isPossibleSetPosition = false;

		public bool IsPossibleGraphRun { get { return this.isPossibleGraphRun; } protected set { if (this.isPossibleGraphRun != value) { this.isPossibleGraphRun = value; OnPossibleChanged("IsPossibleGraphRun", value); } } }
		public bool IsPossibleGraphPause { get { return this.isPossibleGraphPause; } protected set { if (this.isPossibleGraphPause != value) { this.isPossibleGraphPause = value; OnPossibleChanged("IsPossibleGraphPause", value); } } }
		public bool IsPossibleGraphStop { get { return this.isPossibleGraphStop; } protected set { if (this.isPossibleGraphStop != value) { this.isPossibleGraphStop = value; OnPossibleChanged("IsPossibleGraphStop", value); } } }
		public bool IsPossibleGraphRestart { get { return this.isPossibleGraphRestart; } protected set { if (this.isPossibleGraphRestart != value) { this.isPossibleGraphRestart = value; OnPossibleChanged("IsPossibleGraphRestart", value); } } }
		public bool IsPossibleGraphRelease { get { return this.isPossibleGraphRelease; } protected set { if (this.isPossibleGraphRelease != value) { this.isPossibleGraphRelease = value; OnPossibleChanged("IsPossibleGraphRelease", value); } } }

		public bool IsPossibleSetSpeed { get { return this.isPossibleSetSpeed; } protected set { if (this.isPossibleSetSpeed != value) { this.isPossibleSetSpeed = value; OnPossibleChanged("IsPossibleSetSpeed", value); } } }
		public bool IsPossibleSetPosition { get { return this.isPossibleSetPosition; } protected set { if (this.isPossibleSetPosition != value) { this.isPossibleSetPosition = value; OnPossibleChanged("IsPossibleSetPosition", value); } } }


		public GraphBuilderBase(VideoControl renderingControl)
        {
            this.hostingControl = renderingControl;

			//directDraw.Init();
        }

		public virtual void BuildGraph()
		{
			useWPF = Settings.UseWPF;

			this.graphBuilder = (IFilterGraph2)new FilterGraph();
			rot = new DsROTEntry(this.graphBuilder);

			AddRenderers();
			if(!useWPF)
				ConfigureVMR9InWindowlessMode();

			this.hostingControl.CurrentGraphBuilder = this;

			OnGraphStarted();
		}

		public virtual void OnGraphStarted()
		{
			if (GraphStarted != null)
				GraphStarted(this, new EventArgs());

			IsPossibleGraphRun = true;
			IsPossibleGraphPause = false;
			IsPossibleGraphStop = true;
			IsPossibleGraphRestart = false;
			IsPossibleGraphRelease = true;
		}

		public virtual void OnGraphEnded()
		{
			IsPossibleGraphRun = false;
			IsPossibleGraphPause = false;
			IsPossibleGraphStop = false;
			IsPossibleGraphRestart = false;
			IsPossibleGraphRelease = false;

			if (GraphEnded != null)
				GraphEnded(this, new EventArgs());
		}

		public virtual FilterState GetGraphState()
		{
			IMediaControl mediaControl = this.graphBuilder as IMediaControl;
			FilterState pfs;
			mediaControl.GetState(0, out pfs);
			return pfs;
		}

		public virtual void RunGraph()
		{
			IMediaControl mediaControl = this.graphBuilder as IMediaControl;
			FilterState pfs;
			mediaControl.GetState(0, out pfs);
			if (pfs != FilterState.Running)
			{
				int hr = mediaControl.Run();
				ThrowExceptionForHR("Running the graph: ", hr);

				IsPossibleGraphRun = false;
				IsPossibleGraphPause = true;
				IsPossibleGraphStop = true;
			}
		}

		public virtual void PauseGraph()
		{
			IMediaControl mediaControl = this.graphBuilder as IMediaControl;
			FilterState pfs;
			mediaControl.GetState(0, out pfs);
			if (pfs == FilterState.Running)
			{
				int hr = mediaControl.Pause();
				ThrowExceptionForHR("Pausing the graph: ", hr);

				IsPossibleGraphRun = true;
				IsPossibleGraphPause = false;
				IsPossibleGraphStop = true;
			}
		}

		public virtual void StopGraph()
		{
			IMediaControl mediaControl = this.graphBuilder as IMediaControl;
			FilterState pfs;
			mediaControl.GetState(0, out pfs);
			if (pfs == FilterState.Running || pfs == FilterState.Paused)
			{
				int hr = mediaControl.Stop();
				ThrowExceptionForHR("Stopping the graph: ", hr);

				IsPossibleGraphRun = true;
				IsPossibleGraphPause = false;
				IsPossibleGraphStop = false;
			}
		}

		public virtual void SaveGraph(string filepath)
        {
            FilterGraphTools.SaveGraphFile(this.graphBuilder, filepath);
        }

		public virtual void OnVideoEvent(int cookies)
		{
			if (this.graphBuilder == null)
				return;

			IMediaEventEx mediaEvent = this.graphBuilder as IMediaEventEx;
			EventCode eventCode;
			IntPtr param1, param2;
			while (mediaEvent.GetEvent(out eventCode, out param1, out param2, 0) >= 0)
			{
				Trace.WriteLineIf(trace.TraceVerbose, "OnVideoEvent() -> " + eventCode.ToString());

                if (eventCode == EventCode.VMRRenderDeviceSet || eventCode == EventCode.VideoSizeChanged || eventCode == EventCode.Paused)
					VideoRefresh();
				//if (eventCode == EventCode.Paused || eventCode == EventCode.NeedRestart)
				//{
				//    RunGraph();
				//}
				//switch (eventCode) 
				//{ 
				//    // Call application-defined functions for each 
				//    // type of event that you want to handle.
				//}
				int hr = mediaEvent.FreeEventParams(eventCode, param1, param2);
			}
		}

        #region Membres de IDisposable

		public virtual void Dispose()
        {
            Decompose();
        }

        #endregion


		protected virtual void AddRenderers()
		{
			AddRenderers(this.graphBuilder);
		}

		protected virtual void AddRenderers(IFilterGraph2 graphBuilder)
		{
			int hr = 0;
			Guid iid = typeof(IBaseFilter).GUID;

			if (this.AudioRendererDevice != null)
			{
				try
				{
					this.audioRenderer = null;

					IBaseFilter tmp;

					object o;
					this.AudioRendererDevice.Mon.BindToObject(null, null, ref iid, out o);
					tmp = o as IBaseFilter;

					//Add the Video input device to the graph
					hr = graphBuilder.AddFilter(tmp, this.AudioRendererDevice.Name);
					if (hr >= 0)
					{
						// Got it !
						this.audioRenderer = tmp;
					}
					else
					{
						// Try another...
						int hr1 = graphBuilder.RemoveFilter(tmp);
						Marshal.ReleaseComObject(tmp);
						//DsError.ThrowExceptionForHR(hr);
					}
				}
				catch { }
			}

			if (this.audioRenderer == null)
			{
				// Add default audio renderer
				this.audioRenderer = (IBaseFilter)new DSoundRender();
				hr = graphBuilder.AddFilter(this.audioRenderer, "DirectSound Renderer");
				ThrowExceptionForHR("Adding the DirectSound Renderer: ", hr);
			}

			// To see something

            if (useWPF)
            {
                // Get the SampleGrabber interface
                ISampleGrabber sampleGrabber = new SampleGrabber() as ISampleGrabber;
                this.videoRenderer = sampleGrabber as IBaseFilter;

                // Set the media type to Video
                AMMediaType media = new AMMediaType();
                media.majorType = MediaType.Video;
                //media.subType = MediaSubType.YUY2; // RGB24;
                //media.formatType = FormatType.Null;
                //media.sampleSize = 1;
                //media.temporalCompression = false;
                //media.fixedSizeSamples = false;
                //media.unkPtr = IntPtr.Zero;
                //media.formatType = FormatType.None;
                //media.formatSize = 0;
                //media.formatPtr = IntPtr.Zero;
                media.subType = MediaSubType.RGB32; // RGB24;
                media.formatType = FormatType.VideoInfo;
                hr = sampleGrabber.SetMediaType(media);
                ThrowExceptionForHR("Setting the MediaType on the SampleGrabber: ", hr);
                DsUtils.FreeAMMediaType(media);

                // Configure the samplegrabber
                hr = sampleGrabber.SetCallback(this, 1);
                DsError.ThrowExceptionForHR(hr);


                // Add the frame grabber to the graph
                hr = graphBuilder.AddFilter(this.videoRenderer, "SampleGrabber");
                ThrowExceptionForHR("Adding the SampleGrabber: ", hr);

                //hr = ConnectFilters(this.videoRenderer, nullRenderer);
            }
            else
            {
                try
                {
                    this.videoRenderer = (IBaseFilter)new EnhancedVideoRenderer();
                    hr = graphBuilder.AddFilter(this.videoRenderer, "Enhanced Video Renderer");
                    ThrowExceptionForHR("Adding the EVR: ", hr);
                    useEVR = true;
                }
                catch (Exception) { }
                if (!useEVR)
                {
                    this.videoRenderer = (IBaseFilter)new VideoMixingRenderer9();
                    hr = graphBuilder.AddFilter(this.videoRenderer, "Video Mixing Renderer 9");
                    ThrowExceptionForHR("Adding the VMR9: ", hr);
                }
            }
            //IReferenceClock clock = this.audioRenderer as IReferenceClock;
			//if(clock != null)
			//{
			//    // Set the graph clock.
			//    this.FilterGraph.SetDefaultSyncSource( 
			//    pGraph->QueryInterface(IID_IMediaFilter, (void**)&pMediaFilter);
			//    pMediaFilter->SetSyncSource(pClock);
			//    pClock->Release();
			//    pMediaFilter->Release();
			//}
		}

		protected void AddAndConnectNullRendererForWPF()
		{
			if (useWPF)
			{
				// In order to keep the audio/video synchro, we need the NullRenderer
				IBaseFilter nullRenderer = new NullRenderer() as IBaseFilter;
				int hr = graphBuilder.AddFilter(nullRenderer, "NullRenderer");
				ThrowExceptionForHR("Adding the NullRenderer: ", hr);

				IPin pinOutFromFilterOut = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Output, 0);
				if (pinOutFromFilterOut != null)
				{
					try
					{
						IPin pinInFromFilterOut = DsFindPin.ByDirection(nullRenderer, PinDirection.Input, 0);
						if (pinInFromFilterOut != null)
						{
							try
							{
								hr = this.graphBuilder.Connect(pinOutFromFilterOut, pinInFromFilterOut);
							}
							finally
							{
								Marshal.ReleaseComObject(pinInFromFilterOut);
							}
						}
					}
					finally
					{
						Marshal.ReleaseComObject(pinOutFromFilterOut);
					}
				}

				//IPin pinOutFromFilterOut = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Output, 0);
				//if (pinOutFromFilterOut != null)
				//{
				//    hr = this.graphBuilder.Render(pinOutFromFilterOut);
				//    Marshal.ReleaseComObject(pinOutFromFilterOut);
				//}
			}
		}

		protected void ThrowExceptionForHR(string errorText, int hr)
		{
            // If a severe error has occurred
            if (hr < 0)
            {

				try
				{
					string s = DsError.GetErrorText(hr);

					// If a string is returned, build a com error from it
					if (s != null)
					{
						throw new COMException(s, hr);
					}
					else
					{
						// No string, just use standard com error
						Marshal.ThrowExceptionForHR(hr);
					}
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("{0}{1}", errorText, ex.Message), ex);
				}
			}
		}

		protected int ConnectFilters(IBaseFilter filterIn, IBaseFilter filterOut)
		{
			int hr = -1;
			IPin pinOutFromFilterIn = DsFindPin.ByDirection(filterIn, PinDirection.Output, 0);
			if (pinOutFromFilterIn != null)
			{
				IPin pinInFromFilterOut = DsFindPin.ByDirection(filterOut, PinDirection.Input, 0);
				if (pinInFromFilterOut != null)
				{
					hr = this.graphBuilder.Connect(pinOutFromFilterIn, pinInFromFilterOut);
					Marshal.ReleaseComObject(pinInFromFilterOut);
				}

				Marshal.ReleaseComObject(pinOutFromFilterIn);
			}

			return hr;
		}

		//private IntPtr m_handle = IntPtr.Zero;
		//private int m_videoWidth;
		//private int m_videoHeight;
		//private int m_stride;

		protected void WpfUpdateVideoSize()
		{
			if (this.videoRenderer is ISampleGrabber)
			{
				try
				{
					ISampleGrabber sampGrabber = this.videoRenderer as ISampleGrabber;

					// Get the media type from the SampleGrabber
					AMMediaType media = new AMMediaType();
					int hr = sampGrabber.GetConnectedMediaType(media);
					DsError.ThrowExceptionForHR(hr);

					if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
					{
						throw new NotSupportedException("Unknown Grabber Media Format");
					}


					// Grab the size info
					VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
					int videoWidth = videoInfoHeader.BmiHeader.Width;
					int videoHeight = videoInfoHeader.BmiHeader.Height;
					int stride = videoWidth * (videoInfoHeader.BmiHeader.BitCount / 8);

					DsUtils.FreeAMMediaType(media);
					media = null;


					int bytePerPixel = 4;
					// These are 'dummy' pixels only used to create our bitmap
					// further editing after bitmap creation of these pixels does nothing,
					// that is what this hack is for
					byte[] frameBuffer = new byte[videoWidth * videoHeight * bytePerPixel];
					//Create a new bitmap source
					BitmapSource bitmapSource = BitmapSource.Create(videoWidth,
															  videoHeight,
															  96,
															  96,
															  PixelFormats.Bgr32,
															  null,
															  frameBuffer,
															  videoWidth * bytePerPixel);

					this.hostingControl.WPFImage.Source = bitmapSource;
					lock (this.hostingControl.WPFImage)
					{
						this.wpfBitmapBuffer = new WPFUtil.BitmapBuffer(bitmapSource);
					}
				}
				catch (Exception)
				{
				}
			}
		}

		protected void WPFStop()
		{
			if (this.hostingControl != null && this.hostingControl.WPFImage != null)
			{
				lock (this.hostingControl.WPFImage)
				{
					this.wpfBitmapBuffer = null;
				}
			}
		}

		WPFUtil.BitmapBuffer wpfBitmapBuffer;
		[DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
		private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

		/// <summary> sample callback, NOT USED. </summary>
		int ISampleGrabberCB.SampleCB(double sampleTime, IMediaSample mediaSample)
		{
			return 0;
		}

		/// <summary> buffer callback, COULD BE FROM FOREIGN THREAD. </summary>
		unsafe int ISampleGrabberCB.BufferCB(double sampleTime, IntPtr buffer, int bufferLength)
		{
			System.Windows.Controls.Image image = this.hostingControl.WPFImage;
			if (image != null)
			{
				image.Dispatcher.BeginInvoke(DispatcherPriority.Render, new ThreadStart(delegate
				{
					//lock (image)
					{
						if (wpfBitmapBuffer != null)
						{
							// TODO: synchronisation between each frame here and the monitor VBL
							//directDraw.WaitForVerticalBlank();
							CopyMemory(wpfBitmapBuffer.BufferPointer, buffer, bufferLength);
							image.InvalidateVisual();
						}
					}
				}));
			}

			return 0;
		}

		protected virtual void ConfigureVMR9InWindowlessMode()
		{
			ConfigureVMR9InWindowlessMode(1);
		}

		protected virtual void ConfigureVMR9InWindowlessMode(int numberOfStream)
        {
            int hr;

            if (useEVR)
            {
                object o;
                IMFGetService pGetService = null;
                pGetService = (IMFGetService)this.videoRenderer;

                hr = pGetService.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(CodeTV.IMFVideoDisplayControl).GUID, out o);

                try
                {
                    evrVideoDisplayControl = (CodeTV.IMFVideoDisplayControl)o;
                }
                catch
                {
                    Marshal.ReleaseComObject(o);
                    throw;
                }

                try
                {
                    // Set the number of streams.
                    hr = evrVideoDisplayControl.SetVideoWindow(this.hostingControl.Handle);
                    if (numberOfStream > 1)
                    {
                        IEVRFilterConfig evrFilterConfig;
                        evrFilterConfig = (IEVRFilterConfig)this.videoRenderer;
                        hr = evrFilterConfig.SetNumberOfStreams(numberOfStream);
                    }

                    // Keep the aspect-ratio OK
                    hr = evrVideoDisplayControl.SetAspectRatioMode(MFVideoAspectRatioMode.None); // VMR9AspectRatioMode.None);
                    ThrowExceptionForHR("Setting the EVR AspectRatioMode: ", hr);

                    //http://msdn.microsoft.com/en-us/library/windows/desktop/ms701834(v=vs.85).aspx
                    //MFVideoRenderPrefs videoRenderPrefs;
                    //hr = evrVideoDisplayControl.GetRenderingPrefs(out videoRenderPrefs);
                    ////videoRenderPrefs = MFVideoRenderPrefs.DoNotRenderBorder | MFVideoRenderPrefs.DoNotRepaintOnStop;
                    ////videoRenderPrefs = MFVideoRenderPrefs.AllowBatching;
                    //videoRenderPrefs = MFVideoRenderPrefs.ForceBatching;
                    ////videoRenderPrefs = MFVideoRenderPrefs.ForceBatching | MFVideoRenderPrefs.AllowBatching;
                    //hr = evrVideoDisplayControl.SetRenderingPrefs(videoRenderPrefs);

                    //MFVideoAspectRatioMode pdwAspectRatioMode;
                    //hr = evrVideoDisplayControl.GetAspectRatioMode(out pdwAspectRatioMode);
                    //pdwAspectRatioMode = MFVideoAspectRatioMode.None;
                    //hr = evrVideoDisplayControl.SetAspectRatioMode(pdwAspectRatioMode);

                    //int color = 0;
                    //hr = evrVideoDisplayControl.GetBorderColor(out color); // VMR9AspectRatioMode.None);
                    //hr = evrVideoDisplayControl.SetBorderColor(0xff0FF0); // VMR9AspectRatioMode.None);
                    //ThrowExceptionForHR("Setting the EVR AspectRatioMode: ", hr);


                    //EVR Clipping Window bug!!!!!!!!!!!!!!!!!!!http://social.msdn.microsoft.com/Forums/en/windowsdirectshowdevelopment/thread/579b6a6b-bdba-4d3b-a0b6-4de72114232b
                }
                finally
                {
                    //Marshal.ReleaseComObject(pDisplay);
                }


                //hr = pGetService.GetService(MFServices.MR_VIDEO_RENDER_SERVICE, typeof(IMFVideoMixerControl).GUID, out o);
                //try
                //{
                //    //IMFVideoMixerControl videoMixerControl = (IMFVideoMixerControl)o;
                //    //videoMixerControl.SetStreamOutputRect(
                //}
                //catch
                //{
                //    Marshal.ReleaseComObject(o);
                //    throw;
                //}

                //hr = pGetService.GetService(MFServices.MR_VIDEO_ACCELERATION_SERVICE, typeof(IDirectXVideoMemoryConfiguration).GUID, out o);
                //try
                //{
                //    //IDirectXVideoMemoryConfiguration videoMixerControl = (IDirectXVideoMemoryConfiguration)o;

                //    //videoMixerControl.SetStreamOutputRect(

                //}
                //catch
                //{
                //    Marshal.ReleaseComObject(o);
                //    throw;
                //}
            }
            else
            {
                IVMRFilterConfig9 filterConfig = this.videoRenderer as IVMRFilterConfig9;
                if (filterConfig != null)
                {
                    // Configure VMR-9 in Windowsless mode
                    hr = filterConfig.SetRenderingMode(VMR9Mode.Windowless);
                    //hr = filterConfig.SetRenderingMode(VMR9Mode.Windowed);
                    ThrowExceptionForHR("Setting the VMR9 RenderingMode: ", hr);

                    // With 1 input stream
                    hr = filterConfig.SetNumberOfStreams(numberOfStream);
                    ThrowExceptionForHR("Setting the VMR9 NumberOfStreams: ", hr);
                }

                IVMRWindowlessControl9 windowlessControl = this.videoRenderer as IVMRWindowlessControl9;
                if (windowlessControl != null)
                {
                    // The main form is hosting the VMR-9
                    hr = windowlessControl.SetVideoClippingWindow(this.hostingControl.Handle);
                    ThrowExceptionForHR("Setting the VMR9 VideoClippingWindow: ", hr);

                    // Keep the aspect-ratio OK
                    //hr = windowlessControl.SetAspectRatioMode(VMR9AspectRatioMode.LetterBox);
                    hr = windowlessControl.SetAspectRatioMode(VMR9AspectRatioMode.None);
                    ThrowExceptionForHR("Setting the VMR9 AspectRatioMode: ", hr);
                }

                //IVMRMixerControl9 vmrMixerControl9 = this.videoRenderer as IVMRMixerControl9;
                //if (vmrMixerControl9 != null)
                //{
                //    VMR9MixerPrefs dwMixerPrefs;
                //    hr = vmrMixerControl9.GetMixingPrefs(out dwMixerPrefs);
                //    //dwMixerPrefs = DirectShowLib.VMR9MixerPrefs.NoDecimation | DirectShowLib.VMR9MixerPrefs.ARAdjustXorY | DirectShowLib.VMR9MixerPrefs.GaussianQuadFiltering | DirectShowLib.VMR9MixerPrefs.RenderTargetRGB;
                //    //dwMixerPrefs = DirectShowLib.VMR9MixerPrefs.NoDecimation | DirectShowLib.VMR9MixerPrefs.ARAdjustXorY | DirectShowLib.VMR9MixerPrefs.GaussianQuadFiltering | DirectShowLib.VMR9MixerPrefs.RenderTargetRGB;

                //    dwMixerPrefs &= ~DirectShowLib.VMR9MixerPrefs.DecimateMask;
                //    dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.NoDecimation; //Default
                //    //dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.DecimateOutput;
                //    dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.ARAdjustXorY; //Default
                //    //dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.NonSquareMixing;

                //    dwMixerPrefs &= ~DirectShowLib.VMR9MixerPrefs.FilteringMask;
                //    dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.BiLinearFiltering; //Default
                //    //dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.PointFiltering;
                //    //dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.AnisotropicFiltering;
                //    //dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.PyramidalQuadFiltering;
                //    //dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.GaussianQuadFiltering;

                //    dwMixerPrefs &= ~DirectShowLib.VMR9MixerPrefs.RenderTargetMask;
                //    //dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.RenderTargetRGB; //Default
                //    dwMixerPrefs |= DirectShowLib.VMR9MixerPrefs.RenderTargetYUV;

                //    hr = vmrMixerControl9.SetMixingPrefs(dwMixerPrefs);
                //}

            }

            // Init the VMR-9 with default size values
            OnResizeMoveHandler(null, null);

            // Add Windows Messages handlers
            AddHandlers();
		}


		//private Color colorKey = Color.Violet; // The color use as ColorKey for GDI operations
		private System.Drawing.Color colorKey = System.Drawing.Color.Violet; // The color use as ColorKey for GDI operations
		private Bitmap colorKeyBitmap; // A RGB bitmap used for GDI operations.
		//private Bitmap alphaBitmap; // A ARGB bitmap used for Direct3D operations

		//// Managed Direct3D magic number to retrieve unmanaged Direct3D interfaces
		//private const int DxMagicNumber = -759872593;
		//private Device device = null; // A Managed Direct3D device
		//private PresentParameters presentParams;
		//private Surface surface = null; // A Direct3D suface filled with alphaBitmap
		//private IntPtr unmanagedSurface; // A pointer on the unmanaged surface

		//private void InitializeDirect3D()
		//{
		//    // Basic Presentation Parameters...
		//    PresentParameters presentParams = new PresentParameters();
		//    presentParams.Windowed = true;
		//    presentParams.SwapEffect = SwapEffect.Discard;

		//    // Assume a hardware Direct3D device is available
		//    // Add MultiThreaded to be safe. Each DirectShow filter runs in a separate thread...
		//    device = new Device(
		//        0,
		//        DeviceType.Hardware,
		//        this,
		//        CreateFlags.SoftwareVertexProcessing | CreateFlags.MultiThreaded,
		//        presentParams
		//        );

		//    // Create a surface from our alpha bitmap
		//    surface = new Surface(device, alphaBitmap, Pool.SystemMemory);
		//    // Get the unmanaged pointer
		//    unmanagedSurface = surface.GetObjectByValue(DxMagicNumber);
		//}

		//protected override void Dispose(bool disposing)
		//{

		//    // Dispose Managed Direct3D objects
		//    surface.Dispose();
		//    device.Dispose();

		//    base.Dispose(disposing);
		//}

        //private NormalizedRect GetDestRectangle()
        //{
        //    int hr = 0;
        //    int width, height, arW, arH;
        //    NormalizedRect rect = new NormalizedRect();

        //    if (useEVR)
        //    {
        //        Size videoSize = new Size(), arVideoSize = new Size();
        //        hr = evrVideoDisplayControl.GetNativeVideoSize(out videoSize, out arVideoSize);
        //        //hr = evrVideoDisplayControl.GetIdealVideoSize(out videoSize, out arVideoSize);
        //        width = videoSize.Width;
        //        height = videoSize.Height;
        //        arW = arVideoSize.Width;
        //        arH = arVideoSize.Height;
        //    }
        //    else
        //        hr = (this.videoRenderer as IVMRWindowlessControl9).GetNativeVideoSize(out width, out height, out arW, out arH);
        //    DsError.ThrowExceptionForHR(hr);

        //    // Position the bitmap in the middle of the video stream.
        //    if (width >= height)
        //    {
        //        rect.top = 0.0f;
        //        rect.left = (1.0f - ((float)height / (float)width)) / 2;
        //        rect.bottom = 1.0f;
        //        rect.right = rect.left + (float)height / (float)width;
        //    }
        //    else
        //    {
        //        rect.top = (1.0f - ((float)width / (float)height)) / 2;
        //        rect.left = 0.0f;
        //        rect.right = rect.top + (float)width / (float)height;
        //        rect.bottom = 1.0f;
        //    }

        //    return rect;
        //}

		public void StartOSD()
		{
            //// Get the colorkeyed bitmap without antialiasing
            //colorKeyBitmap = BitmapGenerator.GenerateColorKeyBitmap(colorKey, false);
            ////// Get the bitmap with alpha transparency
            ////alphaBitmap = BitmapGenerator.GenerateAlphaBitmap();


            //IVMRMixerBitmap9 mixerBitmap = this.videoRenderer as IVMRMixerBitmap9;

            ////if (usingGDI)
            ////{
            //    // Old school GDI stuff...
            //    Graphics g = Graphics.FromImage(colorKeyBitmap);
            //    IntPtr hdc = g.GetHdc();
            //    IntPtr memDC = NativeMethodes.CreateCompatibleDC(hdc);
            //    IntPtr hBitmap = colorKeyBitmap.GetHbitmap();
            //    NativeMethodes.SelectObject(memDC, hBitmap);

            //    // Set Alpha Bitmap Parameters for using a GDI DC
            //    VMR9AlphaBitmap alphaBmp = new VMR9AlphaBitmap();
            //    alphaBmp.dwFlags = VMR9AlphaBitmapFlags.hDC | VMR9AlphaBitmapFlags.SrcColorKey | VMR9AlphaBitmapFlags.FilterMode;
            //    alphaBmp.hdc = memDC;
            //    alphaBmp.rSrc = new DsRect(0, 0, colorKeyBitmap.Size.Width, colorKeyBitmap.Size.Height);
            //    alphaBmp.rDest = GetDestRectangle();
            //    alphaBmp.clrSrcKey = ColorTranslator.ToWin32(colorKey);
            //    alphaBmp.dwFilterMode = VMRMixerPrefs.PointFiltering;
            //    alphaBmp.fAlpha = 0.75f;

            //    // Set Alpha Bitmap Parameters
            //    int hr = mixerBitmap.SetAlphaBitmap(ref alphaBmp);
            //    DsError.ThrowExceptionForHR(hr);

            //    // Release GDI handles
            //    NativeMethodes.DeleteObject(hBitmap);
            //    NativeMethodes.DeleteDC(memDC);
            //    g.ReleaseHdc(hdc);
            //    g.Dispose();
            ////}
            ////else // Using a Direct3D surface
            ////{
            ////    // Set Alpha Bitmap Parameters for using a Direct3D surface
            ////    VMR9AlphaBitmap alphaBmp = new VMR9AlphaBitmap();
            ////    alphaBmp.dwFlags = VMR9AlphaBitmapFlags.EntireDDS;
            ////    alphaBmp.pDDS = unmanagedSurface;
            ////    alphaBmp.rDest = GetDestRectangle();
            ////    alphaBmp.fAlpha = 1.0f;
            ////    // Note : Alpha values from the bitmap are cumulative with the fAlpha parameter.
            ////    // Example : texel alpha = 128 (50%) & fAlpha = 0.5f (50%) = effective alpha : 64 (25%)

            ////    // Set Alpha Bitmap Parameters
            ////    int hr = mixerBitmap.SetAlphaBitmap(ref alphaBmp);
            ////    DsError.ThrowExceptionForHR(hr);
            ////}
		}

		public void StopOSD()
		{
            //IVMRMixerBitmap9 mixerBitmap = this.videoRenderer as IVMRMixerBitmap9;

            //// Get current Alpha Bitmap Parameters
            //VMR9AlphaBitmap alphaBmp;
            //int hr = mixerBitmap.GetAlphaBitmapParameters(out alphaBmp);
            //DsError.ThrowExceptionForHR(hr);

            //// Disable them
            //alphaBmp.dwFlags = VMR9AlphaBitmapFlags.Disable;

            //// Update the Alpha Bitmap Parameters
            //hr = mixerBitmap.UpdateAlphaBitmapParameters(ref alphaBmp);
            //DsError.ThrowExceptionForHR(hr);
		}

		protected virtual void ConnectAllOutputFiltersFrom(IBaseFilter fromFilter, IFilterGraph2 graph)
		{
			// Get the pin enumerator
			IEnumPins ppEnum;
			int hr = fromFilter.EnumPins(out ppEnum);
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
							hr = graph.Render(pPins[0]);
							//DsError.ThrowExceptionForHR(hr);
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

		/// <summary>
		/// Enumerates all filters of the selected category and returns the IBaseFilter for the 
		/// filter described in friendlyname
		/// </summary>
		/// <param name="category">Category of the filter</param>
		/// <param name="friendlyname">Friendly name of the filter</param>
		/// <returns>IBaseFilter for the device</returns>
		protected virtual IBaseFilter CreateFilter(Guid category, string friendlyname)
		{
			object source = null;
			Guid iid = typeof(IBaseFilter).GUID;
			foreach (DsDevice device in DsDevice.GetDevicesOfCat(category))
			{
				if (device.Name.CompareTo(friendlyname) == 0)
				{
					device.Mon.BindToObject(null, null, ref iid, out source);
					break;
				}
			}

			return (IBaseFilter)source;
		}

		protected virtual void AddHandlers()
        {
            // Add Windows Messages handlers
			this.hostingControl.PaintBackground += new VideoControl.PaintBackgroundEventHandler(OnPaintBackground);
			this.hostingControl.Paint += new PaintEventHandler(OnPaintHandler); // for WM_PAINT
			this.hostingControl.Resize += new EventHandler(OnResizeMoveHandler); // for WM_SIZE
            //this.hostingControl.Move += new EventHandler(OnResizeMoveHandler); // for WM_MOVE
            SystemEvents.DisplaySettingsChanged += new EventHandler(OnDisplayChangedHandler); // for WM_DISPLAYCHANGE
        }

		protected virtual void RemoveHandlers()
        {
            // Remove Windows Messages handlers
			this.hostingControl.PaintBackground -= new VideoControl.PaintBackgroundEventHandler(OnPaintBackground);
			this.hostingControl.Paint -= new PaintEventHandler(OnPaintHandler); // for WM_PAINT
            this.hostingControl.Resize -= new EventHandler(OnResizeMoveHandler); // for WM_SIZE
            //this.hostingControl.Move -= new EventHandler(OnResizeMoveHandler); // for WM_MOVE
            this.hostingControl.UseBlackBands = false;
            SystemEvents.DisplaySettingsChanged -= new EventHandler(OnDisplayChangedHandler); // for WM_DISPLAYCHANGE
        }

		protected virtual void Decompose()
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

                if (this.evrVideoDisplayControl != null) Marshal.ReleaseComObject(this.evrVideoDisplayControl); this.evrVideoDisplayControl = null;

				if (this.audioRenderer != null) Marshal.ReleaseComObject(this.audioRenderer); this.audioRenderer = null;
				if (this.videoRenderer != null) Marshal.ReleaseComObject(this.videoRenderer); this.videoRenderer = null;

				try { rot.Dispose(); }
				catch { }
				try { Marshal.ReleaseComObject(this.graphBuilder); this.graphBuilder = null; }
				catch { }
			}
        }

		//protected virtual Rectangle GetInnerRectangle(Rectangle outerRectangle, double innerRatio)
		//{
		//    Rectangle innerRectangle = outerRectangle;
		//    double outerRatio = (double)outerRectangle.Width / (double)outerRectangle.Height;
		//    if (outerRatio >= innerRatio)
		//    {
		//        innerRectangle.Width = (int)((double)outerRectangle.Height * innerRatio);
		//        innerRectangle.X = (int)((outerRectangle.Width - innerRectangle.Width) / 2.0);
		//    }
		//    else
		//    {
		//        innerRectangle.Height = (int)((double)outerRectangle.Width / innerRatio);
		//        innerRectangle.Y = (int)((outerRectangle.Height - innerRectangle.Height) / 2.0);
		//    }
		//    return innerRectangle;
		//}

        protected Rectangle[] GetBlackBands()
		{
			Rectangle outerRectangle = this.hostingControl.ClientRectangle;
			DsRect innerDsRect = new DsRect();
            int hr;
            if (useEVR)
            {
                MFVideoNormalizedRect pnrcSource = new MFVideoNormalizedRect();
                MediaFoundation.Misc.MFRect prcDest = new MediaFoundation.Misc.MFRect();
                hr = evrVideoDisplayControl.GetVideoPosition(pnrcSource, prcDest);
                innerDsRect = DsRect.FromRectangle((Rectangle)prcDest);
            }
            else
            {
                IVMRWindowlessControl9 vmrWindowlessControl9 = this.videoRenderer as IVMRWindowlessControl9;
                hr = vmrWindowlessControl9.GetVideoPosition(null, innerDsRect);
            }
			Rectangle innerRectangle = innerDsRect.ToRectangle();

			//Trace.WriteLineIf(trace.TraceVerbose, string.Format(("\tvideoRenderer.GetVideoPosition({0})"), innerRectangle.ToString()));
			//Trace.WriteLineIf(trace.TraceVerbose, string.Format(("\thostingControl.ClientRectangle({0})"), outerRectangle.ToString()));

			List<Rectangle> alRectangles = new List<Rectangle>();

			if (innerRectangle.Top > outerRectangle.Top)
				alRectangles.Add(new Rectangle(outerRectangle.Left, outerRectangle.Top, outerRectangle.Width - 1, innerRectangle.Top - 1));

			if (innerRectangle.Bottom < outerRectangle.Bottom)
				alRectangles.Add(new Rectangle(outerRectangle.Left, innerRectangle.Bottom, outerRectangle.Width - 1, outerRectangle.Height - (innerRectangle.Bottom + 1)));

			if (innerRectangle.Left > outerRectangle.Left)
			{
				Rectangle rectangleLeft = new Rectangle(outerRectangle.Left, innerRectangle.Top, innerRectangle.Left - 1, innerRectangle.Height - 1);
				rectangleLeft.Intersect(outerRectangle);
				alRectangles.Add(rectangleLeft);
			}

			if (innerRectangle.Right < outerRectangle.Right)
			{
				Rectangle rectangleLeft = new Rectangle(innerRectangle.Right, innerRectangle.Top, outerRectangle.Width - (innerRectangle.Right + 1), innerRectangle.Height - 1);
				rectangleLeft.Intersect(outerRectangle);
				alRectangles.Add(rectangleLeft);
			}
            return alRectangles.ToArray();
        }

		protected virtual void PaintBlackBands(Graphics g)
		{
			if (this.videoRenderer != null)
			{
				Trace.WriteLineIf(trace.TraceInfo, "PaintBlackBands()");

                Rectangle[] alRectangles = GetBlackBands();
                if (alRectangles.Length > 0)
				{
					g.FillRectangles(new SolidBrush(Settings.VideoBackgroundColor), alRectangles);
					g.DrawRectangles(new System.Drawing.Pen(Settings.VideoBackgroundColor), alRectangles);
				}
			}
		}

		public virtual void VideoRefresh()
		{
			if (this.videoRenderer != null)
			{
				//Trace.WriteLineIf(trace.TraceInfo, "VideoRefresh()");

                VideoResizer(this.videoZoomMode, this.videoKeepAspectRatio, this.videoOffset, this.videoZoom, this.videoAspectRatioFactor);

				if (!useWPF)
				{
					try
					{
                        int hr;
                        if (useEVR)
                        {
                            hr = this.evrVideoDisplayControl.RepaintVideo();
                            this.hostingControl.ModifyBlackBands(GetBlackBands(), Settings.VideoBackgroundColor);
                        }
                        else
                        {
                            Graphics g = this.hostingControl.CreateGraphics();
                            IntPtr hdc = g.GetHdc();
                            hr = (this.videoRenderer as IVMRWindowlessControl9).RepaintVideo(this.hostingControl.Handle, hdc);
                            g.ReleaseHdc(hdc);

                            PaintBlackBands(g);
                            g.Dispose();
                        }
					}
					catch { }
				}
			}
		}

		// Idea from Gabest (http://www.gabest.org) and modify by me
		public virtual void VideoResizer(VideoSizeMode videoZoomMode, bool keepAspectRatio, PointF offset, double zoom, double aspectRatioFactor)
		{
			Trace.WriteLineIf(trace.TraceInfo, "VideoResizer(...)");
			int hr = 0;
			
			Rectangle windowRect = this.hostingControl.ClientRectangle;
			currentVideoTargetRectangle = windowRect;
			currentVideoSourceSize = new Size();

            FilterState filterState = GetGraphState();
            if (filterState == FilterState.Paused || filterState == FilterState.Running)
            {
                if (videoZoomMode != VideoSizeMode.StretchToWindow)
			    {
					int arX, arY;
					int arX2 = 0, arY2 = 0;

                    if (useEVR)
                    {
                        Size videoSize = new Size(), arVideoSize = new Size();
                        hr = evrVideoDisplayControl.GetNativeVideoSize(out videoSize, out arVideoSize);
                        //IMFVideoDisplayControlEx evrVideoDisplayControlPlus = evrVideoDisplayControl as IMFVideoDisplayControlEx;
                        //hr = evrVideoDisplayControlPlus.GetNativeVideoSize(out videoSize, out arVideoSize);
                        //hr = evrVideoDisplayControlPlus.GetIdealVideoSize(videoSize, arVideoSize);
                        arX = videoSize.Width;
                        arY = videoSize.Height;
                        arX2 = arVideoSize.Width;
                        arY2 = arVideoSize.Height;
                        Trace.WriteLineIf(trace.TraceVerbose, string.Format(("\tvideoRenderer.GetNativeVideoSize({0}, {1})"), videoSize.ToString(), arVideoSize.ToString()));
                    }
                    else
    					hr = (this.videoRenderer as IVMRWindowlessControl9).GetNativeVideoSize(out arX, out arY, out arX2, out arY2);
					if (hr >= 0 && arY > 0)
					{
						//DsError.ThrowExceptionForHR(hr);
						//Trace.WriteLineIf(trace.TraceVerbose, string.Format("\tGetNativeVideoSize(width: {0}, height: {1}, arX {2}, arY: {3}", arX, arY, arX2, arY2));

						if (arX2 > 0 && arY2 > 0)
						{
							arX = arX2;
							arY = arY2;
						}

						currentVideoSourceSize.Width = arX;
						currentVideoSourceSize.Height = arY;

						Size windowSize = windowRect.Size;

						double newAspectRation = aspectRatioFactor * (double)arX / (double)arY * (this.useVideo169Mode ? 3.0 / 4.0 : 1.0);
						int height = windowSize.Height;
						int width = (int)((double)height * newAspectRation);

						if (videoZoomMode == VideoSizeMode.FromInside || videoZoomMode == VideoSizeMode.FromOutside)
						{
							if (videoZoomMode == VideoSizeMode.FromInside && width > windowSize.Width
							|| videoZoomMode == VideoSizeMode.FromOutside && width < windowSize.Width)
							{
								width = windowSize.Width;
								height = (int)((double)width / newAspectRation);
							}
						}

						Size size = new Size((int)(zoom * width), (int)(zoom * height));

						Point pos = new Point(
							(int)(offset.X * (windowRect.Width * 3 - size.Width) - windowRect.Width),
							(int)(offset.Y * (windowRect.Height * 3 - size.Height) - windowRect.Height));

						//Point pos = new Point(
						//    (int)(offset.X * (windowRect.Width - size.Width)),
						//    (int)(offset.Y * (windowRect.Height - size.Height)));

						currentVideoTargetRectangle = new Rectangle(pos, size);
					}
				}
                if (useEVR)
                {
                    //hr = evrVideoDisplayControl.SetVideoWindow(this.hostingControl.Handle);
                    MFVideoNormalizedRect pnrcSource = new MFVideoNormalizedRect(0.0f, 0.0f, 1.0f, 1.0f);
                    hr = this.evrVideoDisplayControl.SetVideoPosition(pnrcSource, (MediaFoundation.Misc.MFRect)currentVideoTargetRectangle);
                    this.hostingControl.ModifyBlackBands(GetBlackBands(), Settings.VideoBackgroundColor);
                }
                else
                    hr = (this.videoRenderer as IVMRWindowlessControl9).SetVideoPosition(null, DsRect.FromRectangle(currentVideoTargetRectangle));
                //Trace.WriteLineIf(trace.TraceVerbose, string.Format(("\tPos {0:F2} {1:F2}, Zoom {2:F2}, ARF {4:F2}, AR {4:F2}"), offset.X, offset.Y, zoom, aspectRatioFactor, (float)videoTargetRect.Width / videoTargetRect.Height));
                Trace.WriteLineIf(trace.TraceVerbose, string.Format(("\tvideoRenderer.SetVideoPosition({0})"), currentVideoTargetRectangle.ToString()));
            }
		}

		protected virtual void OnResizeMoveHandler(object sender, EventArgs e)
		{
			if (this.videoRenderer != null)
			{
				//Trace.WriteLineIf(trace.TraceInfo, "OnResizeMoveHandler()");

				VideoResizer(this.videoZoomMode, this.videoKeepAspectRatio, this.videoOffset, this.videoZoom, this.videoAspectRatioFactor);

                if (!useEVR) {
                    Graphics g = this.hostingControl.CreateGraphics();
                    PaintBlackBands(g);
                    g.Dispose();
                }
			}
		}

		protected virtual bool OnPaintBackground(object sender, PaintEventArgs e)
		{
			// The OnPaintBackground is call a lot often than OnPaintHandler!!!!
			if (this.videoRenderer != null)
			{
                //PaintBlackBands(e.Graphics);
				//Trace.WriteLineIf(trace.TraceInfo, "OnPaintBackground()");
				return false;
			}
			return true;
		}

		void OnPaintHandler(object sender, PaintEventArgs e)
		{
			if (this.videoRenderer != null)
			{
				Trace.WriteLineIf(trace.TraceInfo, "OnPaintHandler()");

				try
				{
                    int hr;
                    if (useEVR)
                    {
                        //PaintBlackBands(e.Graphics);
                        hr = this.evrVideoDisplayControl.RepaintVideo();
                        //PaintBlackBands(e.Graphics);
                    }
                    else
                    {
                        IntPtr hdc = e.Graphics.GetHdc();
                        hr = (this.videoRenderer as IVMRWindowlessControl9).RepaintVideo(this.hostingControl.Handle, hdc);
                        e.Graphics.ReleaseHdc(hdc);
                        PaintBlackBands(e.Graphics);
                    }
				}
				catch { }
			}
		}

		protected virtual void OnDisplayChangedHandler(object sender, EventArgs e)
		{
			if (this.videoRenderer != null)
			{
				//Trace.WriteLineIf(trace.TraceInfo, "OnDisplayChangedHandler()");
                int hr;
                if (!useEVR)
                    hr = (this.videoRenderer as IVMRWindowlessControl9).DisplayModeChanged();
			}
		}




        public static readonly Guid bobDxvaGuid = new Guid(0x335aa36e, 0x7884, 0x43a4, 0x9c, 0x91, 0x7f, 0x87, 0xfa, 0xf3, 0xe3, 0x7e);

		// From MediaPortal
		protected void Vmr9SetDeinterlaceMode(int mode)
		{
			//0=None
			//1=Bob
			//2=Weave
			//3=Best
			//Log("vmr9:SetDeinterlace() SetDeinterlaceMode(%d)",mode);
			IVMRDeinterlaceControl9 pDeint = (this.videoRenderer as IVMRDeinterlaceControl9);
            if (pDeint != null)
            {
                //VMR9VideoDesc VideoDesc;
                //uint dwNumModes = 0;
                Guid deintMode;
                int hr;
                if (mode == 0)
                {
                    //off
                    hr = pDeint.SetDeinterlaceMode(-1, Guid.Empty);
                    //if (!SUCCEEDED(hr)) Log("vmr9:SetDeinterlace() failed hr:0x%x",hr);
                    hr = pDeint.GetDeinterlaceMode(0, out deintMode);
                    //if (!SUCCEEDED(hr)) Log("vmr9:GetDeinterlaceMode() failed hr:0x%x",hr);
                    //Log("vmr9:SetDeinterlace() deinterlace mode OFF: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
                    //		deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);

                    return;
                }
                if (mode == 1)
                {
                    //BOB

                    hr = pDeint.SetDeinterlaceMode(-1, bobDxvaGuid);
                    //if (!SUCCEEDED(hr)) Log("vmr9:SetDeinterlace() failed hr:0x%x",hr);
                    hr = pDeint.GetDeinterlaceMode(0, out deintMode);
                    //if (!SUCCEEDED(hr)) Log("vmr9:GetDeinterlaceMode() failed hr:0x%x",hr);
                    //Log("vmr9:SetDeinterlace() deinterlace mode BOB: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
                    //        deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);

                    return;
                }
                if (mode == 2)
                {
                    //WEAVE
                    hr = pDeint.SetDeinterlaceMode(-1, Guid.Empty);
                    //if (!SUCCEEDED(hr)) Log("vmr9:SetDeinterlace() failed hr:0x%x",hr);
                    hr = pDeint.GetDeinterlaceMode(0, out deintMode);
                    //if (!SUCCEEDED(hr)) Log("vmr9:GetDeinterlaceMode() failed hr:0x%x",hr);
                    //Log("vmr9:SetDeinterlace() deinterlace mode WEAVE: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
                    //        deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);

                    return;
                }

                //    AM_MEDIA_TYPE pmt;
                //    ULONG fetched;
                //    IPin* pins[10];
                //    CComPtr<IEnumPins> pinEnum;
                //    hr=m_pVMR9Filter->EnumPins(&pinEnum);
                //    pinEnum->Reset();
                //    pinEnum->Next(1,&pins[0],&fetched);
                //    hr=pins[0]->ConnectionMediaType(&pmt);
                //    pins[0]->Release();

                //    VIDEOINFOHEADER2* vidInfo2 =(VIDEOINFOHEADER2*)pmt.pbFormat;
                //    if (vidInfo2==NULL)
                //    {
                //        Log("vmr9:SetDeinterlace() VMR9 not connected");
                //        return ;
                //    }
                //    if ((pmt.formattype != FORMAT_VideoInfo2) || (pmt.cbFormat< sizeof(VIDEOINFOHEADER2)))
                //    {
                //        Log("vmr9:SetDeinterlace() not using VIDEOINFOHEADER2");

                //        return ;
                //    }

                //    Log("vmr9:SetDeinterlace() resolution:%dx%d planes:%d bitcount:%d fmt:%d %c%c%c%c",
                //        vidInfo2->bmiHeader.biWidth,vidInfo2->bmiHeader.biHeight,
                //        vidInfo2->bmiHeader.biPlanes,
                //        vidInfo2->bmiHeader.biBitCount,
                //        vidInfo2->bmiHeader.biCompression,
                //        (char)(vidInfo2->bmiHeader.biCompression&0xff),
                //        (char)((vidInfo2->bmiHeader.biCompression>>8)&0xff),
                //        (char)((vidInfo2->bmiHeader.biCompression>>16)&0xff),
                //        (char)((vidInfo2->bmiHeader.biCompression>>24)&0xff)
                //        );
                //    char major[128];
                //    char subtype[128];
                //    strcpy(major,"unknown");
                //    sprintf(subtype,"unknown (0x%x-0x%x-0x%x-0x%x)",pmt.subtype.Data1,pmt.subtype.Data2,pmt.subtype.Data3,pmt.subtype.Data4);
                //    if (pmt.majortype==MEDIATYPE_AnalogVideo)
                //        strcpy(major,"Analog video");
                //    if (pmt.majortype==MEDIATYPE_Video)
                //        strcpy(major,"video");
                //    if (pmt.majortype==MEDIATYPE_Stream)
                //        strcpy(major,"stream");

                //    if (pmt.subtype==MEDIASUBTYPE_MPEG2_VIDEO)
                //        strcpy(subtype,"mpeg2 video");
                //    if (pmt.subtype==MEDIASUBTYPE_MPEG1System)
                //        strcpy(subtype,"mpeg1 system");
                //    if (pmt.subtype==MEDIASUBTYPE_MPEG1VideoCD)
                //        strcpy(subtype,"mpeg1 videocd");

                //    if (pmt.subtype==MEDIASUBTYPE_MPEG1Packet)
                //        strcpy(subtype,"mpeg1 packet");
                //    if (pmt.subtype==MEDIASUBTYPE_MPEG1Payload )
                //        strcpy(subtype,"mpeg1 payload");
                ////	if (pmt.subtype==MEDIASUBTYPE_ATSC_SI)
                ////		strcpy(subtype,"ATSC SI");
                ////	if (pmt.subtype==MEDIASUBTYPE_DVB_SI)
                ////		strcpy(subtype,"DVB SI");
                ////	if (pmt.subtype==MEDIASUBTYPE_MPEG2DATA)
                ////		strcpy(subtype,"MPEG2 Data");
                //    if (pmt.subtype==MEDIASUBTYPE_MPEG2_TRANSPORT)
                //        strcpy(subtype,"MPEG2 Transport");
                //    if (pmt.subtype==MEDIASUBTYPE_MPEG2_PROGRAM)
                //        strcpy(subtype,"MPEG2 Program");

                //    if (pmt.subtype==MEDIASUBTYPE_CLPL)
                //        strcpy(subtype,"MEDIASUBTYPE_CLPL");
                //    if (pmt.subtype==MEDIASUBTYPE_YUYV)
                //        strcpy(subtype,"MEDIASUBTYPE_YUYV");
                //    if (pmt.subtype==MEDIASUBTYPE_IYUV)
                //        strcpy(subtype,"MEDIASUBTYPE_IYUV");
                //    if (pmt.subtype==MEDIASUBTYPE_YVU9)
                //        strcpy(subtype,"MEDIASUBTYPE_YVU9");
                //    if (pmt.subtype==MEDIASUBTYPE_Y411)
                //        strcpy(subtype,"MEDIASUBTYPE_Y411");
                //    if (pmt.subtype==MEDIASUBTYPE_Y41P)
                //        strcpy(subtype,"MEDIASUBTYPE_Y41P");
                //    if (pmt.subtype==MEDIASUBTYPE_YUY2)
                //        strcpy(subtype,"MEDIASUBTYPE_YUY2");
                //    if (pmt.subtype==MEDIASUBTYPE_YVYU)
                //        strcpy(subtype,"MEDIASUBTYPE_YVYU");
                //    if (pmt.subtype==MEDIASUBTYPE_UYVY)
                //        strcpy(subtype,"MEDIASUBTYPE_UYVY");
                //    if (pmt.subtype==MEDIASUBTYPE_Y211)
                //        strcpy(subtype,"MEDIASUBTYPE_Y211");
                //    if (pmt.subtype==MEDIASUBTYPE_RGB565)
                //        strcpy(subtype,"MEDIASUBTYPE_RGB565");
                //    if (pmt.subtype==MEDIASUBTYPE_RGB32)
                //        strcpy(subtype,"MEDIASUBTYPE_RGB32");
                //    if (pmt.subtype==MEDIASUBTYPE_ARGB32)
                //        strcpy(subtype,"MEDIASUBTYPE_ARGB32");
                //    if (pmt.subtype==MEDIASUBTYPE_RGB555)
                //        strcpy(subtype,"MEDIASUBTYPE_RGB555");
                //    if (pmt.subtype==MEDIASUBTYPE_RGB24)
                //        strcpy(subtype,"MEDIASUBTYPE_RGB24");
                //    if (pmt.subtype==MEDIASUBTYPE_AYUV)
                //        strcpy(subtype,"MEDIASUBTYPE_AYUV");
                //    if (pmt.subtype==MEDIASUBTYPE_YV12)
                //        strcpy(subtype,"MEDIASUBTYPE_YV12");
                ////	if (pmt.subtype==MEDIASUBTYPE_NV12)
                ////		strcpy(subtype,"MEDIASUBTYPE_NV12");
                //    Log("vmr9:SetDeinterlace() major:%s subtype:%s", major,subtype);
                //    VideoDesc.dwSize = sizeof(VMR9VideoDesc);
                //    VideoDesc.dwFourCC=vidInfo2->bmiHeader.biCompression;
                //    VideoDesc.dwSampleWidth=vidInfo2->bmiHeader.biWidth;
                //    VideoDesc.dwSampleHeight=vidInfo2->bmiHeader.biHeight;
                //    VideoDesc.SampleFormat=ConvertInterlaceFlags(vidInfo2->dwInterlaceFlags);
                //    VideoDesc.InputSampleFreq.dwDenominator=(DWORD)vidInfo2->AvgTimePerFrame;
                //    VideoDesc.InputSampleFreq.dwNumerator=10000000;
                //    VideoDesc.OutputFrameFreq.dwDenominator=(DWORD)vidInfo2->AvgTimePerFrame;
                //    VideoDesc.OutputFrameFreq.dwNumerator=VideoDesc.InputSampleFreq.dwNumerator;
                //    if (VideoDesc.SampleFormat != VMR9_SampleProgressiveFrame)
                //    {
                //        VideoDesc.OutputFrameFreq.dwNumerator=2*VideoDesc.InputSampleFreq.dwNumerator;
                //    }

                //    // Fill in the VideoDesc structure (not shown).
                //    hr = pDeint->GetNumberOfDeinterlaceModes(&VideoDesc, &dwNumModes, NULL);
                //    if (SUCCEEDED(hr) && (dwNumModes != 0))
                //    {
                //        // Allocate an array for the GUIDs that identify the modes.
                //        GUID *pModes = new GUID[dwNumModes];
                //        if (pModes)
                //        {
                //            Log("vmr9:SetDeinterlace() found %d deinterlacing modes", dwNumModes);
                //            // Fill the array.
                //            hr = pDeint->GetNumberOfDeinterlaceModes(&VideoDesc, &dwNumModes, pModes);
                //            if (SUCCEEDED(hr))
                //            {
                //                // Loop through each item and get the capabilities.
                //                for (int i = 0; i < dwNumModes; i++)
                //                {
                //                    hr=pDeint->SetDeinterlaceMode(0xFFFFFFFF,&pModes[0]);
                //                    if (SUCCEEDED(hr))
                //                    {
                //                        Log("vmr9:SetDeinterlace() set deinterlace mode:%d",i);



                //                        pDeint->GetDeinterlaceMode(0,&deintMode);
                //                        if (deintMode.Data1==pModes[0].Data1 &&
                //                            deintMode.Data2==pModes[0].Data2 &&
                //                            deintMode.Data3==pModes[0].Data3 &&
                //                            deintMode.Data4==pModes[0].Data4)
                //                        {
                //                            Log("vmr9:SetDeinterlace() succeeded");
                //                        }
                //                        else
                //                            Log("vmr9:SetDeinterlace() deinterlace mode set to: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
                //                                    deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);
                //                        break;
                //                    }
                //                    else
                //                        Log("vmr9:SetDeinterlace() deinterlace mode:%d failed 0x:%x",i,hr);

                //                }
                //            }
                //            delete [] pModes;
                //        }
                //    }
            }
		}
	}

    //[Guid("A490B1E4-AB84-4D31-A1B2-181E03B1077A")]
    //[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //[System.Security.SuppressUnmanagedCodeSecurity]
    //public interface IMFVideoDisplayControlEx
    //{
    //    int GetAspectRatioMode(out MFVideoAspectRatioMode pdwAspectRatioMode);
    //    int GetBorderColor(out int pClr);
    //    int GetCurrentImage(BitmapInfoHeader pBih, out IntPtr pDib, out int pcbDib, out long pTimeStamp);
    //    int GetFullscreen(out bool pfFullscreen);
    //    int GetIdealVideoSize(Size pszMin, Size pszMax);
    //    //int GetNativeVideoSize(Size pszVideo, Size pszARVideo);
    //    int GetNativeVideoSize(out Size pszVideo, out Size pszARVideo);
    //    int GetRenderingPrefs(out MFVideoRenderPrefs pdwRenderFlags);
    //    int GetVideoPosition(MFVideoNormalizedRect pnrcSource, MediaFoundation.Misc.MFRect prcDest);
    //    int GetVideoWindow(out IntPtr phwndVideo);
    //    int RepaintVideo();
    //    int SetAspectRatioMode(MFVideoAspectRatioMode dwAspectRatioMode);
    //    int SetBorderColor(int Clr);
    //    int SetFullscreen(bool fFullscreen);
    //    int SetRenderingPrefs(MFVideoRenderPrefs dwRenderFlags);
    //    int SetVideoPosition(MFVideoNormalizedRect pnrcSource, MediaFoundation.Misc.MFRect prcDest);
    //    int SetVideoWindow(IntPtr hwndVideo);
    //}

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("A490B1E4-AB84-4D31-A1B2-181E03B1077A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoDisplayControl
    {
        //[PreserveSig]
        //int GetNativeVideoSize(
        //    [Out] Size pszVideo,
        //    [Out] Size pszARVideo
        //    );
        [PreserveSig]
        int GetNativeVideoSize(
            out Size pszVideo,
            out Size pszARVideo
            );

        [PreserveSig]
        int GetIdealVideoSize(
            [Out] Size pszMin,
            [Out] Size pszMax
            );

        [PreserveSig]
        int SetVideoPosition(
            [In] MFVideoNormalizedRect pnrcSource,
            [In] MediaFoundation.Misc.MFRect prcDest
            );

        [PreserveSig]
        int GetVideoPosition(
            [Out] MFVideoNormalizedRect pnrcSource,
            [Out] MediaFoundation.Misc.MFRect prcDest
            );

        [PreserveSig]
        int SetAspectRatioMode(
            [In] MFVideoAspectRatioMode dwAspectRatioMode
            );

        [PreserveSig]
        int GetAspectRatioMode(
            out MFVideoAspectRatioMode pdwAspectRatioMode
            );

        [PreserveSig]
        int SetVideoWindow(
            [In] IntPtr hwndVideo
            );

        [PreserveSig]
        int GetVideoWindow(
            out IntPtr phwndVideo
            );

        [PreserveSig]
        int RepaintVideo();

        [PreserveSig]
        int GetCurrentImage(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(BMMarshaler))] BitmapInfoHeader pBih,
            out IntPtr pDib,
            out int pcbDib,
            out long pTimeStamp
            );

        [PreserveSig]
        int SetBorderColor(
            [In] int Clr
            );

        [PreserveSig]
        int GetBorderColor(
            out int pClr
            );

        [PreserveSig]
        int SetRenderingPrefs(
            [In] MFVideoRenderPrefs dwRenderFlags
            );

        [PreserveSig]
        int GetRenderingPrefs(
            out MFVideoRenderPrefs pdwRenderFlags
            );

        [PreserveSig]
        int SetFullscreen(
            [In, MarshalAs(UnmanagedType.Bool)] bool fFullscreen
            );

        [PreserveSig]
        int GetFullscreen(
            [MarshalAs(UnmanagedType.Bool)] out bool pfFullscreen
            );
    }

    // Class to handle BITMAPINFO
    internal class BMMarshaler : ICustomMarshaler
    {
        protected MediaFoundation.Misc.BitmapInfoHeader m_bmi;

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            m_bmi = managedObj as MediaFoundation.Misc.BitmapInfoHeader;

            IntPtr ip = m_bmi.GetPtr();

            return ip;
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            MediaFoundation.Misc.BitmapInfoHeader bmi = MediaFoundation.Misc.BitmapInfoHeader.PtrToBMI(pNativeData);

            // If we this call is In+Out, the return value is ignored.  If
            // this is out, then m_bmi will be null.
            if (m_bmi != null)
            {
                m_bmi.CopyFrom(bmi);
                bmi = null;
            }

            return bmi;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            m_bmi = null;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        // The number of bytes to marshal out - never called
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new BMMarshaler();
        }
    }
}
