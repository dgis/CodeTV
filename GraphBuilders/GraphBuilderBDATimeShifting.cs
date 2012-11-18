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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

using Microsoft.Win32;

using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.SBE;
using DirectShowLib.Utils;
using System.IO;

namespace CodeTV
{
	public class GraphBuilderBDATimeShifting : GraphBuilderBDA, ITimeShifting, IRecorder
    {
		protected IFilterGraph2 graphBuilder2;
		protected DsROTEntry rot2;

		protected IBaseFilter streamBufferSink;
		protected IBaseFilter streamBufferSource;
		protected IBaseFilter mpeg2VideoStreamAnalyzer;

		public override IFilterGraph2 FilterGraph { get { return this.graphBuilder2; } }
		public IFilterGraph2 FilterGraphSink { get { return this.graphBuilder; } }

		private IStreamBufferRecordControl currentRecorderControl = null;

		protected bool isPossibleTimeShiftingPause = false;
		protected bool isPossibleTimeShiftingResume = false;
		protected bool isPossibleRecorderStart = false;
		protected bool isPossibleRecorderStop = false;

		public bool IsPossibleTimeShiftingPause { get { return this.isPossibleTimeShiftingPause; } protected set { if (this.isPossibleTimeShiftingPause != value) { this.isPossibleTimeShiftingPause = value; OnPossibleChanged("IsPossibleTimeShiftingPause", value); } } }
		public bool IsPossibleTimeShiftingResume { get { return this.isPossibleTimeShiftingResume; } protected set { if (this.isPossibleTimeShiftingResume != value) { this.isPossibleTimeShiftingResume = value; OnPossibleChanged("IsPossibleTimeShiftingResume", value); } } }
		public bool IsPossibleRecorderStart { get { return this.isPossibleRecorderStart; } protected set { if (this.isPossibleRecorderStart != value) { this.isPossibleRecorderStart = value; OnPossibleChanged("IsPossibleRecorderStart", value); } } }
		public bool IsPossibleRecorderStop { get { return this.isPossibleRecorderStop; } protected set { if (this.isPossibleRecorderStop != value) { this.isPossibleRecorderStop = value; OnPossibleChanged("IsPossibleRecorderStop", value); } } }


		public GraphBuilderBDATimeShifting(VideoControl renderingControl)
			: base(renderingControl)
        {
        }

		public override void BuildGraph()
		{
			useWPF = Settings.UseWPF;

			try
			{
				BuildSinkGraph(this.objTuningSpace);
				BuildSourceGraph();
			}
			catch (Exception ex)
			{
				Decompose();
				throw ex;
			}

			OnGraphStarted();
		}

		private void BuildSinkGraph(ITuningSpace tuningSpace)
		{
			this.graphBuilder = (IFilterGraph2)new FilterGraph();
			this.rot = new DsROTEntry(this.graphBuilder);

			this.epg = new EPG(this.hostingControl);

			this.cookiesSink = this.hostingControl.SubscribeEvents(this as VideoControl.IVideoEventHandler, this.graphBuilder as IMediaEventEx);

			AddNetworkProviderFilter(this.objTuningSpace);
			AddMPEG2DemuxFilter();
			AddMpeg2VideoStreamAnalyzerFilter();
			CreateMPEG2DemuxPins();

			AddAndConnectBDABoardFilters();
			if (this.tuner == null && this.capture == null)
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

			ConfigureTimeShiftingRegistry();

			AddStreamBufferSinkFilter();
			ConnectStreamBufferSinkFilter();

			this.epg.RegisterEvent(TransportInformationFilter as IConnectionPointContainer);
		}


		//IntPtr HKEY_CLASSES_ROOT = (IntPtr)(UIntPtr)0x80000000;
		//IntPtr HKEY_CURRENT_USER = (IntPtr)(UIntPtr)0x80000001;
		//IntPtr HKEY_LOCAL_MACHINE = (IntPtr)(UIntPtr)0x80000002;
		//IntPtr HKEY_USERS = (IntPtr)(UIntPtr)0x80000003;

		[DllImport("Advapi32.dll")]
		public static extern int RegCreateKey(UIntPtr hKey, string lpSubKey, IntPtr phkResult);

		[DllImport("Advapi32.dll")]
		public static extern int RegCloseKey(IntPtr hKey);

		IntPtr streamBufferConfigHKey = IntPtr.Zero;

		protected void ConfigureTimeShiftingRegistry()
		{
            //http://msdn.microsoft.com/en-us/library/windows/desktop/dd694948%28v=vs.85%29.aspx

			UIntPtr HKEY_CURRENT_USER = (UIntPtr)0x80000001;
			//unchecked
			//{
			//    HKEY_CURRENT_USER = (UIntPtr)0x80000001;
			//}

			// Create the StreamBufferConfig object.
			StreamBufferConfig streamBufferConfig = new StreamBufferConfig();
			IStreamBufferConfigure streamBufferConfigure = streamBufferConfig as IStreamBufferConfigure;

			// Create a new registry key to hold our settings.
			streamBufferConfigHKey = IntPtr.Zero;
			IntPtr p = Marshal.AllocCoTaskMem(4);
			Marshal.WriteIntPtr(p, IntPtr.Zero);
			try
			{
				int lRes = RegCreateKey(HKEY_CURRENT_USER, MainForm.RegistryBaseKey + "\\SBE", p);
				streamBufferConfigHKey = Marshal.ReadIntPtr(p);
			}
			finally
			{
				Marshal.FreeCoTaskMem(p);
			}

			// Set the registry key.
			IStreamBufferInitialize streamBufferInitialize = streamBufferConfigure as IStreamBufferInitialize;
			int hr = streamBufferInitialize.SetHKEY(streamBufferConfigHKey);
            DsError.ThrowExceptionForHR(hr);

            //http://msdn.microsoft.com/en-us/library/windows/desktop/dd694977%28v=vs.85%29.aspx
            //For Windows Vista or later the IStreamBufferSink::LockProfile method requires administrator privileges,
            // unless you first call IStreamBufferConfigure3::SetNamespace with the value NULL.
            IStreamBufferConfigure3 streamBufferConfigure3 = streamBufferConfig as IStreamBufferConfigure3;
            if (streamBufferConfigure3 != null)
            {
                hr = streamBufferConfigure3.SetNamespace(null);
                DsError.ThrowExceptionForHR(hr);
            }

			// Set the TimeShifting configuration
			//hr = streamBufferConfigure.SetDirectory("C:\\MyDirectory");
            string directory = Settings.VideosFolder;
            string directoryPath = FileUtils.GetAbsolutePath(directory as string);
            hr = streamBufferConfigure.SetDirectory(directoryPath);
            DsError.ThrowExceptionForHR(hr);


			hr = streamBufferConfigure.SetBackingFileDuration(600); // Min 15 seconds
            //TODO not working anymore!!
            hr = streamBufferConfigure.SetBackingFileCount(
                Math.Min(100, Math.Max(4, Settings.TimeShiftingBufferLengthMin / 10)),  // 4-100
                Math.Min(102, Math.Max(6, Settings.TimeShiftingBufferLengthMax / 10))); // 6-102
		}



		private void BuildSourceGraph()
		{
			this.graphBuilder2 = (IFilterGraph2)new FilterGraph();
			this.rot2 = new DsROTEntry(this.graphBuilder2);

			this.cookiesSource = this.hostingControl.SubscribeEvents(this as VideoControl.IVideoEventHandler, this.graphBuilder2 as IMediaEventEx);

			AddStreamBufferSourceFilter();
			AddRenderers(this.graphBuilder2);
			ConfigureVMR9InWindowlessMode();
			AddBDAVideoDecoderFilters(this.graphBuilder2);
			ConnectStreamBufferSourceFilter();

			this.hostingControl.CurrentGraphBuilder = this;
		}

		public override void OnGraphStarted()
		{
			base.OnGraphStarted();

			IsPossibleTimeShiftingPause = false;
			IsPossibleTimeShiftingResume = false;
			IsPossibleRecorderStart = false;
			IsPossibleRecorderStop = false;

			IsPossibleSetSpeed = true;
			IsPossibleSetPosition = true;
		}

		public override void OnGraphEnded()
		{
			IsPossibleTimeShiftingPause = false;
			IsPossibleTimeShiftingResume = false;
			IsPossibleRecorderStart = false;
			IsPossibleRecorderStop = false;

			IsPossibleSetSpeed = false;
			IsPossibleSetPosition = false;

			base.OnGraphEnded();
		}


		public override void SubmitTuneRequest(Channel channel)
		{
			//StopGraph();
			//ConnectStreamBufferSinkToSource();

			//IStreamBufferMediaSeeking mediaSeeking = this.streamBufferSource as IStreamBufferMediaSeeking;
			//if (mediaSeeking != null)
			//{
			//    long currentposition = (long)(position.TotalMilliseconds * 10000.0);
			//    mediaSeeking.SetPositions(new DsLong(currentposition), AMSeekingSeekingFlags.AbsolutePositioning,
			//        null, AMSeekingSeekingFlags.NoPositioning);
			//}


			TimeSpan start, stop;
			(this as ITimeShifting).GetPositions(out start, out stop);
			(this as ITimeShifting).SetPosition(stop);

			base.SubmitTuneRequest(channel);

			IsPossibleTimeShiftingResume = ((this as ITimeShifting).Status == TimeShiftingStatus.Paused);
			IsPossibleTimeShiftingPause = !IsPossibleTimeShiftingResume;
			IsPossibleRecorderStart = ((this as IRecorder).Status == RecorderStatus.Stopped);
			IsPossibleRecorderStop = !IsPossibleRecorderStart;
		}

		private int cookiesSink = 0;
		private int cookiesSource = 0;

		public override void OnVideoEvent(int cookies)
		{
			if (this.graphBuilder == null || this.graphBuilder2 == null)
				return;
			try
			{
				if (cookies == cookiesSink)
				{
					IMediaEventEx mediaEvent = this.graphBuilder as IMediaEventEx;
					EventCode eventCode;
					IntPtr param1, param2;
					while (mediaEvent.GetEvent(out eventCode, out param1, out param2, 0) >= 0)
					{
						Trace.WriteLineIf(trace.TraceVerbose, "OnVideoEvent(Sink) -> " + eventCode.ToString());

						if (eventCode == EventCode.VMRRenderDeviceSet)
							VideoRefresh();

						//switch (eventCode) 
						//{ 
						//    // Call application-defined functions for each 
						//    // type of event that you want to handle.
						//}
						int hr = mediaEvent.FreeEventParams(eventCode, param1, param2);
					}
				}
				else if (cookies == cookiesSource)
				{
					IMediaEventEx mediaEvent = this.graphBuilder2 as IMediaEventEx;
					EventCode eventCode;
					IntPtr param1, param2;
					while (mediaEvent.GetEvent(out eventCode, out param1, out param2, 0) >= 0)
					{
						Trace.WriteLineIf(trace.TraceVerbose, "OnVideoEvent(Source) -> " + eventCode.ToString());

						if (eventCode == EventCode.VMRRenderDeviceSet)
							VideoRefresh();

						//switch (eventCode) 
						//{ 
						//    // Call application-defined functions for each 
						//    // type of event that you want to handle.
						//}
						int hr = mediaEvent.FreeEventParams(eventCode, param1, param2);
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(trace.TraceError, ex.ToString());
			}
		}

		public override FilterState GetGraphState()
		{
			IMediaControl mediaControl = this.graphBuilder2 as IMediaControl;
			FilterState pfs;
			mediaControl.GetState(0, out pfs);
			return pfs;
		}

		public override void RunGraph()
		{
			IMediaControl mediaControl2 = this.graphBuilder2 as IMediaControl;
			FilterState pfs2;
			mediaControl2.GetState(0, out pfs2);
			if (pfs2 != FilterState.Running)
			{
				int hr = mediaControl2.Run();
				DsError.ThrowExceptionForHR(hr);
			}

			IMediaControl mediaControl = this.graphBuilder as IMediaControl;
			FilterState pfs;
			mediaControl.GetState(0, out pfs);
			if (pfs != FilterState.Running)
			{
				int hr = 0;
				try
				{
					hr = mediaControl.Run();
				}
				catch (Exception ex)
				{
					Trace.WriteLineIf(trace.TraceError, ex.ToString());
				}
				DsError.ThrowExceptionForHR(hr);
			}
		}

		public override void PauseGraph()
		{
			IMediaControl mediaControl = this.graphBuilder as IMediaControl;
			FilterState pfs;
			mediaControl.GetState(0, out pfs);
			if (pfs == FilterState.Running)
			{
				int hr = mediaControl.Pause();
				DsError.ThrowExceptionForHR(hr);
			}

			IMediaControl mediaControl2 = this.graphBuilder2 as IMediaControl;
			FilterState pfs2;
			mediaControl2.GetState(0, out pfs2);
			if (pfs2 == FilterState.Running)
			{
				int hr = mediaControl2.Pause();
				DsError.ThrowExceptionForHR(hr);
			}
		}

		public override void StopGraph()
		{
			IMediaControl mediaControl = this.graphBuilder as IMediaControl;
			FilterState pfs;
			mediaControl.GetState(0, out pfs);
			if (pfs == FilterState.Running || pfs == FilterState.Paused)
			{
				int hr = mediaControl.Stop();
				DsError.ThrowExceptionForHR(hr);
			}

			IMediaControl mediaControl2 = this.graphBuilder2 as IMediaControl;
			FilterState pfs2;
			mediaControl2.GetState(0, out pfs2);
			if (pfs2 == FilterState.Running || pfs2 == FilterState.Paused)
			{
				int hr = mediaControl2.Stop();
				DsError.ThrowExceptionForHR(hr);
			}
		}

		private void AddMpeg2VideoStreamAnalyzerFilter()
		{
			if (this.H264DecoderDevice == null)
			//if (this.H264DecoderDevice != null || !isH264ElecardSpecialMode)
			{
				this.mpeg2VideoStreamAnalyzer = (IBaseFilter)new Mpeg2VideoStreamAnalyzer();

				int hr = this.graphBuilder.AddFilter(this.mpeg2VideoStreamAnalyzer, "MPEG-2 Video Analyzer");
				DsError.ThrowExceptionForHR(hr);
			}
		}

		private void AddStreamBufferSinkFilter()
		{
            try
            {
                this.streamBufferSink = (IBaseFilter)new SBE2Sink();
            }
            catch { }
			if (this.streamBufferSink == null) // In case SBE2Sink is not supported, fallback to the former filter.
                this.streamBufferSink = (IBaseFilter)new StreamBufferSink();

			int hr = this.graphBuilder.AddFilter(this.streamBufferSink, "Stream Buffer Sink");
			DsError.ThrowExceptionForHR(hr);

			IStreamBufferInitialize streamBufferInitialize = this.streamBufferSink as IStreamBufferInitialize;
			hr = streamBufferInitialize.SetHKEY(streamBufferConfigHKey);
            DsError.ThrowExceptionForHR(hr);
		}

		private void AddStreamBufferSourceFilter()
		{
			this.streamBufferSource = (IBaseFilter)new StreamBufferSource();

			int hr = this.graphBuilder2.AddFilter(this.streamBufferSource, "Stream Buffer Source");
			DsError.ThrowExceptionForHR(hr);


			IStreamBufferInitialize streamBufferInitialize = this.streamBufferSource as IStreamBufferInitialize;
			hr = streamBufferInitialize.SetHKEY(streamBufferConfigHKey);


			ConnectStreamBufferSinkToSource();
		}

		private void ConnectStreamBufferSinkToSource()
		{
			if (!Directory.Exists(Settings.VideosFolder))
				Directory.CreateDirectory(Settings.VideosFolder);

			string currentSBEProfile = Settings.VideosFolder + "\\CurrentRecording.sbe-stub";
            string currentSBEProfileFilename = FileUtils.GetAbsolutePath(currentSBEProfile as string);

			IStreamBufferSink sink = this.streamBufferSink as IStreamBufferSink;
            int hr = sink.LockProfile(currentSBEProfile); //currentSBEProfileFilename); //currentSBEProfile);
            DsError.ThrowExceptionForHR(hr);

			IStreamBufferSource source = this.streamBufferSource as IStreamBufferSource;
            hr = source.SetStreamSink(sink); // This line does not seem to be compatible with SBE2Sink.
            //So, I commented out the generation of exception   DsError.ThrowExceptionForHR(hr);
            hr = (source as IFileSourceFilter).Load(currentSBEProfile, null); //currentSBEProfileFilename, null); //currentSBEProfile, null);
            DsError.ThrowExceptionForHR(hr);
        }

		private void ConnectStreamBufferSinkFilter()
		{
			IPin audioDemuxOut = null, audioDvrIn = null;
			try
			{
				audioDemuxOut = DsFindPin.ByDirection(this.mpeg2Demux, PinDirection.Output, 0);
				audioDvrIn = DsFindPin.ByDirection(this.streamBufferSink, PinDirection.Input, 0);
				FilterGraphTools.ConnectFilters(this.graphBuilder, audioDemuxOut, audioDvrIn, false);
			}
			finally
			{
				if (audioDemuxOut != null) Marshal.ReleaseComObject(audioDemuxOut);
				if (audioDvrIn != null) Marshal.ReleaseComObject(audioDvrIn);
			}

            //if (false) //Not working anymore on Windows 7!! this.mpeg2VideoStreamAnalyzer != null)
            //{
            //    IPin videoDemuxOut = null, videoVSAIn = null;
            //    try
            //    {
            //        videoDemuxOut = DsFindPin.ByName(this.mpeg2Demux, this.H264DecoderDevice == null ? "MPG2" : "H264");
            //        videoVSAIn = DsFindPin.ByDirection(this.mpeg2VideoStreamAnalyzer, PinDirection.Input, 0);
            //        FilterGraphTools.ConnectFilters(this.graphBuilder, videoDemuxOut, videoVSAIn, false);
            //    }
            //    finally
            //    {
            //        if (videoDemuxOut != null) Marshal.ReleaseComObject(videoDemuxOut);
            //        if (videoVSAIn != null) Marshal.ReleaseComObject(videoVSAIn);
            //    }

            //    IPin videoVSAOut = null, videoDvrIn = null;
            //    try
            //    {
            //        videoVSAOut = DsFindPin.ByDirection(this.mpeg2VideoStreamAnalyzer, PinDirection.Output, 0);
            //        videoDvrIn = DsFindPin.ByDirection(this.streamBufferSink, PinDirection.Input, 1);
            //        FilterGraphTools.ConnectFilters(this.graphBuilder, videoVSAOut, videoDvrIn, false);
            //    }
            //    finally
            //    {
            //        if (videoVSAOut != null) Marshal.ReleaseComObject(videoVSAOut);
            //        if (videoDvrIn != null) Marshal.ReleaseComObject(videoDvrIn);
            //    }
            //}
            //else
            //{
            IPin videoDemuxOut = null, videoDvrIn = null;
            try
            {
                videoDemuxOut = DsFindPin.ByName(this.mpeg2Demux, this.H264DecoderDevice == null ? "MPG2" : "H264");
                videoDvrIn = DsFindPin.ByDirection(this.streamBufferSink, PinDirection.Input, 1);
                FilterGraphTools.ConnectFilters(this.graphBuilder, videoDemuxOut, videoDvrIn, false);
            }
            finally
            {
                if (videoDemuxOut != null) Marshal.ReleaseComObject(videoDemuxOut);
                if (videoDvrIn != null) Marshal.ReleaseComObject(videoDvrIn);
            }
            //}
		}

		private void ConnectStreamBufferSourceFilter()
		{
			IBaseFilter videoDecoder = this.H264DecoderDevice == null ? this.videoMpeg2DecoderFilter : this.videoH264DecoderFilter;

            //this.streamBufferSource as IStreamBufferConfigure

			IPin videoDvrOut = null, videoDecoderIn = null;
			try
			{
                videoDvrOut = DsFindPin.ByDirection(this.streamBufferSource, PinDirection.Output, 1);
                //TODO Create output pin for the StreamBufferSource
				videoDecoderIn = DsFindPin.ByDirection(videoDecoder, PinDirection.Input, 0);

				//AMMediaType mediaH264 = new AMMediaType();
				//mediaH264.majorType = MediaType.Video;
				//mediaH264.subType = new Guid(0x8d2d71cb, 0x243f, 0x45e3, 0xb2, 0xd8, 0x5f, 0xd7, 0x96, 0x7e, 0xc0, 0x9b);
				//mediaH264.sampleSize = 0;
				//mediaH264.temporalCompression = false;
				//mediaH264.fixedSizeSamples = false;
				//mediaH264.unkPtr = IntPtr.Zero;
				//mediaH264.formatType = FormatType.Mpeg2Video;

				//MPEG2VideoInfo videoH264PinFormat = GetVideoH264PinFormat();
				//mediaH264.formatSize = Marshal.SizeOf(videoH264PinFormat);
				//mediaH264.formatPtr = Marshal.AllocHGlobal(mediaH264.formatSize);
				//Marshal.StructureToPtr(videoH264PinFormat, mediaH264.formatPtr, false);

				////int hr = videoDvrOut.Connect(videoDecoderIn, mediaH264);
				//int hr = videoDecoderIn.Connect(videoDvrOut, mediaH264);
				//Marshal.FreeHGlobal(mediaH264.formatPtr);
				//DsError.ThrowExceptionForHR(hr);

                //if (this.H264DecoderDevice != null)
                //{
                //    AMMediaType mediaH264 = new AMMediaType();
                //    mediaH264.majorType = MediaType.Video;
                //    //mediaH264.subType = new Guid(0x8d2d71cb, 0x243f, 0x45e3, 0xb2, 0xd8, 0x5f, 0xd7, 0x96, 0x7e, 0xc0, 0x9b);
                //    mediaH264.subType = new Guid(0x34363248, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
                //    mediaH264.sampleSize = 0;
                //    mediaH264.temporalCompression = true; // false;
                //    mediaH264.fixedSizeSamples = true; // false;
                //    mediaH264.unkPtr = IntPtr.Zero;
                //    mediaH264.formatType = FormatType.Mpeg2Video;

                //    MPEG2VideoInfo videoH264PinFormat = GetVideoH264PinFormat();
                //    mediaH264.formatSize = Marshal.SizeOf(videoH264PinFormat);
                //    mediaH264.formatPtr = Marshal.AllocHGlobal(mediaH264.formatSize);
                //    Marshal.StructureToPtr(videoH264PinFormat, mediaH264.formatPtr, false);

                //    //IPin pinDemuxerVideoH264;
                //    //int hr = mpeg2Demultiplexer.CreateOutputPin(mediaH264, "H264", out pinDemuxerVideoH264);
                //    //if (pinDemuxerVideoH264 != null)
                //    //Marshal.ReleaseComObject(pinDemuxerVideoH264);
                //    int hr = this.graphBuilder2.ConnectDirect(videoDvrOut, videoDecoderIn, mediaH264);
                //    //hr = this.graphBuilder2.Connect(videoDvrOut, videoDecoderIn);
                //    DsError.ThrowExceptionForHR(hr);

                //    Marshal.FreeHGlobal(mediaH264.formatPtr);
                //}
                //else
    				FilterGraphTools.ConnectFilters(this.graphBuilder2, videoDvrOut, videoDecoderIn, false);
				//int hr = this.graphBuilder2.Render(videoDvrOut);
				// H264 video stream cannot connect to H264 decoder (Cyberlink)!!!
			}
			finally
			{
				if (videoDvrOut != null) Marshal.ReleaseComObject(videoDvrOut);
				if (videoDecoderIn != null) Marshal.ReleaseComObject(videoDecoderIn);
			}

			IPin videoDecoderOut = null, videoVMRIn = null;
			try
			{
				videoDecoderOut = DsFindPin.ByDirection(videoDecoder, PinDirection.Output, 0);
				videoVMRIn = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Input, 0);
				FilterGraphTools.ConnectFilters(this.graphBuilder2, videoDecoderOut, videoVMRIn, false);
			}
			finally
			{
				if (videoDecoderOut != null) Marshal.ReleaseComObject(videoDecoderOut);
				if (videoVMRIn != null) Marshal.ReleaseComObject(videoVMRIn);
			}

			IPin audioDvrOut = null, audioRendererIn = null;
			try
			{
				audioDvrOut = DsFindPin.ByDirection(this.streamBufferSource, PinDirection.Output, 0);
				audioRendererIn = DsFindPin.ByDirection(this.audioRenderer, PinDirection.Input, 0);
				FilterGraphTools.ConnectFilters(this.graphBuilder2, audioDvrOut, audioRendererIn, true);
			}
			finally
			{
				if (audioDvrOut != null) Marshal.ReleaseComObject(audioDvrOut);
				if (audioRendererIn != null) Marshal.ReleaseComObject(audioRendererIn);
			}
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

				try { hr = (this.graphBuilder2 as IMediaControl).StopWhenReady(); }
				catch { }
				try { hr = (this.graphBuilder2 as IMediaControl).Stop(); }
				catch { }
				RemoveHandlers();


				FilterGraphTools.RemoveAllFilters(this.graphBuilder);
				FilterGraphTools.RemoveAllFilters(this.graphBuilder2);


				if (this.networkProvider != null) Marshal.ReleaseComObject(this.networkProvider); this.networkProvider = null;
				if (this.mpeg2Demux != null) Marshal.ReleaseComObject(this.mpeg2Demux); this.mpeg2Demux = null;
				if (this.mpeg2VideoStreamAnalyzer != null) Marshal.ReleaseComObject(this.mpeg2VideoStreamAnalyzer); this.mpeg2VideoStreamAnalyzer = null;
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
				if (this.streamBufferSink != null) Marshal.ReleaseComObject(this.streamBufferSink); this.streamBufferSink = null;
				if (this.streamBufferSource != null) Marshal.ReleaseComObject(this.streamBufferSource); this.streamBufferSource = null;

				if (streamBufferConfigHKey != IntPtr.Zero)
				{
					RegCloseKey(streamBufferConfigHKey);
					streamBufferConfigHKey = IntPtr.Zero;
				}

				try { rot.Dispose(); }
				catch { }
				try { Marshal.ReleaseComObject(this.graphBuilder); this.graphBuilder = null; }
				catch { }

				try { rot2.Dispose(); }
				catch { }
				try { Marshal.ReleaseComObject(this.graphBuilder2); this.graphBuilder2 = null; }
				catch { }
			}
        }

		#region ITimeShifting Members

		TimeShiftingStatus ITimeShifting.Status
		{
			get
			{
				IMediaControl mediaControl2 = this.graphBuilder2 as IMediaControl;
				if (mediaControl2 == null)
					return TimeShiftingStatus.Recording;

				FilterState pfs2;
				mediaControl2.GetState(0, out pfs2);
				switch(pfs2)
				{
					case FilterState.Paused:
						return TimeShiftingStatus.Paused;
					case FilterState.Running:
						return TimeShiftingStatus.Recording;
					default:
						return TimeShiftingStatus.Recording;
				}
			}
		}

		void ITimeShifting.Pause()
		{
			IMediaControl mediaControl2 = this.graphBuilder2 as IMediaControl;
			FilterState pfs2;
			mediaControl2.GetState(0, out pfs2);
			if (pfs2 == FilterState.Running)
			{
				int hr = mediaControl2.Pause();
				DsError.ThrowExceptionForHR(hr);

				IsPossibleTimeShiftingPause = false;
				IsPossibleTimeShiftingResume = true;
			}
		}

		void ITimeShifting.Resume()
		{
			IMediaControl mediaControl2 = this.graphBuilder2 as IMediaControl;
			FilterState pfs2;
			mediaControl2.GetState(0, out pfs2);
			if (pfs2 == FilterState.Paused)
			{
				int hr = mediaControl2.Run();
				DsError.ThrowExceptionForHR(hr);

				IsPossibleTimeShiftingPause = true;
				IsPossibleTimeShiftingResume = false;
			}
		}

		void ITimeShifting.GetPositions(out TimeSpan start, out TimeSpan stop)
		{
			IStreamBufferMediaSeeking mediaSeeking = this.streamBufferSource as IStreamBufferMediaSeeking;
			if (mediaSeeking != null)
			{
				long startPosition = 0; // Reference time (100-nanosecond units). 100 * 10e-9 = 10e-7 = 10000000
				long stopPosition = 0; // Reference time (100-nanosecond units). 100 * 10e-9 = 10e-7 = 10000000
				mediaSeeking.GetAvailable(out startPosition, out stopPosition);

				start = TimeSpan.FromMilliseconds(startPosition / 10000);
				stop = TimeSpan.FromMilliseconds(stopPosition / 10000);
			}
			else
				start = stop = TimeSpan.Zero;
		}

		TimeSpan ITimeShifting.GetPosition()
		{
			IStreamBufferMediaSeeking mediaSeeking = this.streamBufferSource as IStreamBufferMediaSeeking;
			if (mediaSeeking != null)
			{
				long currentposition = 0; // Reference time (100-nanosecond units). 100 * 10e-9 = 10e-7 = 10000000
				mediaSeeking.GetCurrentPosition(out currentposition);

				return TimeSpan.FromMilliseconds(currentposition / 10000);
			}
			return TimeSpan.Zero;
		}


		void ITimeShifting.SetPosition(TimeSpan position)
		{
			IStreamBufferMediaSeeking mediaSeeking = this.streamBufferSource as IStreamBufferMediaSeeking;
			if (mediaSeeking != null)
			{
				long currentposition = (long)(position.TotalMilliseconds * 10000.0);
				mediaSeeking.SetPositions(new DsLong(currentposition), AMSeekingSeekingFlags.AbsolutePositioning,
					null, AMSeekingSeekingFlags.NoPositioning);
			}
		}

		double ITimeShifting.GetRate()
		{
			double rate = 1.0;
			IStreamBufferMediaSeeking mediaSeeking = this.streamBufferSource as IStreamBufferMediaSeeking;
			if (mediaSeeking != null)
			{
				int hr = mediaSeeking.GetRate(out rate);
			}
			return rate;
		}

		void ITimeShifting.SetRate(double rate)
		{
			if (rate >= 0 && rate < 0.1)
				rate = 0.1;
			else if (rate < 0 && rate > -0.1)
				rate = -0.1;

			int hr = 0;

			IStreamBufferMediaSeeking2 mediaSeeking2 = this.streamBufferSource as IStreamBufferMediaSeeking2;
			if (mediaSeeking2 != null)
			{
				//mediaSeeking2.SetRateEx(rate, 25);
				hr = mediaSeeking2.SetRateEx(rate, 25);
				if (hr == 0) return;
			}

			IStreamBufferMediaSeeking mediaSeeking1 = this.streamBufferSource as IStreamBufferMediaSeeking;
			if (mediaSeeking1 != null)
			{
				hr = mediaSeeking1.SetRate(rate);
				if (hr == 0) return;
			}

			IMediaSeeking mediaSeeking0 = this.graphBuilder2 as IMediaSeeking;
			if (mediaSeeking0 != null)
			{
				hr = mediaSeeking0.SetRate(rate);
				if (hr == 0) return;
			}
		}

		#endregion

		#region IRecorder Members

		RecorderStatus IRecorder.Status
		{
			get
			{
				bool started = false, stopped = true;
				if (this.currentRecorderControl != null)
				{
					int hLastResult = 0;
					this.currentRecorderControl.GetRecordingStatus(out hLastResult, out started, out stopped);
				}
				if (started)
					return RecorderStatus.Recording;
				else
					return RecorderStatus.Stopped;
			}
		}

		void IRecorder.Start(string filename)
		{
			IStreamBufferSink sink = this.streamBufferSink as IStreamBufferSink;

			object pRecUnk;
			//int hr = sink.CreateRecorder(filename, RecordingType.Reference, out pRecUnk);
			int hr = sink.CreateRecorder(filename, RecordingType.Content, out pRecUnk);
			if (hr >= 0)
			{
				long rtStart = 0;
				//IStreamBufferMediaSeeking mediaSeeking = this.streamBufferSource as IStreamBufferMediaSeeking;
				//if (mediaSeeking != null)
				//{
				//    // Reference time (100-nanosecond units). 100 * 10e-9 = 10e-7 = 10000000
				//    mediaSeeking.GetCurrentPosition(out rtStart);
				//}

				// Start the recording.
				this.currentRecorderControl = pRecUnk as IStreamBufferRecordControl;
				//long rtStart = -10000000; // Start 1 seconds ago.
				hr = this.currentRecorderControl.Start(ref rtStart);
				DsError.ThrowExceptionForHR(hr);

				IsPossibleRecorderStart = false;
				IsPossibleRecorderStop = true;
			}
		}

		void IRecorder.Stop()
		{
			if (this.currentRecorderControl != null)
			{
				int hr = this.currentRecorderControl.Stop(0); // Stop the recording and close the file.
				Marshal.ReleaseComObject(this.currentRecorderControl);
				this.currentRecorderControl = null;
				DsError.ThrowExceptionForHR(hr);

				IsPossibleRecorderStart = true;
				IsPossibleRecorderStop = false;
			}
		}

		void IRecorder.Play(string filename)
		{
			int hr;
			IMediaControl mediaControl2 = this.graphBuilder2 as IMediaControl;
			FilterState pfs2;
			mediaControl2.GetState(0, out pfs2);
			if (pfs2 == FilterState.Running || pfs2 == FilterState.Paused)
			{
				hr = mediaControl2.Stop();
				DsError.ThrowExceptionForHR(hr);
			}

			//IStreamBufferSink sink = this.streamBufferSink as IStreamBufferSink;
			IStreamBufferSource source = this.streamBufferSource as IStreamBufferSource;
			hr = source.SetStreamSink(null);
			hr = (source as IFileSourceFilter).Load(filename, null);

			mediaControl2.GetState(0, out pfs2);
			if (pfs2 != FilterState.Running)
			{
				hr = mediaControl2.Run();
				DsError.ThrowExceptionForHR(hr);
			}
		}

		TimeSpan IRecorder.GetDuration()
		{
			return TimeSpan.Zero;
		}

		#endregion
	}
}
