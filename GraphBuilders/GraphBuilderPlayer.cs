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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

using Microsoft.Win32;

using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;

using CodeTV.PSI;
using CodeTV.TsFileSink;

namespace CodeTV
{
	public class GraphBuilderPlayer : GraphBuilderBase, IPlayer
    {
		private string fileName = "";

		protected IBaseFilter filterSource;
		protected IBaseFilter mpeg2Demux;

		public IBaseFilter FilterSource { get { return this.filterSource; } }

		public string FileName { get { return this.fileName; } set { this.fileName = value; } }


		protected bool isPossiblePlayerPlay = false;
		protected bool isPossiblePlayerPause = false;
		protected bool isPossiblePlayerStop = false;

		public bool IsPossiblePlayerPlay { get { return this.isPossiblePlayerPlay; } protected set { if (this.isPossiblePlayerPlay != value) { this.isPossiblePlayerPlay = value; OnPossibleChanged("IsPossiblePlayerPlay", value); } } }
		public bool IsPossiblePlayerPause { get { return this.isPossiblePlayerPause; } protected set { if (this.isPossiblePlayerPause != value) { this.isPossiblePlayerPause = value; OnPossibleChanged("IsPossiblePlayerPause", value); } } }
		public bool IsPossiblePlayerStop { get { return this.isPossiblePlayerStop; } protected set { if (this.isPossiblePlayerStop != value) { this.isPossiblePlayerStop = value; OnPossibleChanged("IsPossiblePlayerStop", value); } } }


		public GraphBuilderPlayer(VideoControl renderingControl)
			: base(renderingControl)
        {
        }

		//public override void BuildGraphWithNoRenderer()
		//{
		//    this.graphBuilder = (IFilterGraph2)new FilterGraph();
		//    rot = new DsROTEntry(this.graphBuilder);

		//    OnGraphStarted();
		//}

		//private IPin pinDemuxerVideoMPEG4;

		protected static byte[] Mpeg2ProgramVideo =
				{
					0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
					0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
					0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
					0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

					//0x051736=333667-> 10000000/333667 = 29.97fps
					//0x061A80=400000-> 10000000/400000 = 25fps
					0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
					0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
					0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
					0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
					0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
					0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
					0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
					0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
					0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
					0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
					0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
					0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
					0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
					0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
					0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
					//0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
					0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
					0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
					0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000

					//  .dwSequenceHeader [1]
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
		};

		[ComImport, Guid("4F8BF30C-3BEB-43A3-8BF2-10096FD28CF2")]
		protected class TsFileSource { }

		public override void BuildGraph()
		{
			try
			{
				useWPF = Settings.UseWPF;

				int hr = 0;

				this.graphBuilder = (IFilterGraph2)new FilterGraph();
				rot = new DsROTEntry(this.graphBuilder);

				string extension = Path.GetExtension(this.fileName).ToLower();

				if (extension == ".ts")
				{
					TsFileSource fileSource = new TsFileSource();
					this.filterSource = fileSource as IBaseFilter;
					if (this.filterSource != null)
					{
						this.graphBuilder.AddFilter(this.filterSource, "TsFileSource");

						IFileSourceFilter interFaceFile = (IFileSourceFilter)fileSource;
						interFaceFile.Load(this.fileName, null);

						ITSFileSource tsFileSource = fileSource as ITSFileSource;
						ushort audioPid = 0;
						tsFileSource.GetAudioPid(ref audioPid);

						ushort videoPid = 0;
						tsFileSource.GetVideoPid(ref videoPid);

						byte[] videoPidTypeByteBuffer = new byte[16];
						tsFileSource.GetVideoPidType(videoPidTypeByteBuffer);

						int posCharZero = 0;
						for (; posCharZero < videoPidTypeByteBuffer.Length; posCharZero++) if (videoPidTypeByteBuffer[posCharZero] == 0) break;

						char[] videoPidTypeCharBuffer = new char[posCharZero];
						Array.Copy(videoPidTypeByteBuffer, 0, videoPidTypeCharBuffer, 0, posCharZero);
						string videoPidType = new string(videoPidTypeCharBuffer);
						// "MPEG 2", "H.264"


						AddMPEG2DemuxFilter();

						//IMpeg2Demultiplexer mpeg2Demultiplexer = this.mpeg2Demux as IMpeg2Demultiplexer;

						////Log.WriteFile(Log.LogType.Log, false, "DVBGraphBDA: create mpg4 video pin");
						//AMMediaType mediaMPG4 = new AMMediaType();
						//mediaMPG4.majorType = MediaType.Video;
						//mediaMPG4.subType = new Guid(0x8d2d71cb, 0x243f, 0x45e3, 0xb2, 0xd8, 0x5f, 0xd7, 0x96, 0x7e, 0xc0, 0x9b);
						//mediaMPG4.sampleSize = 0;
						//mediaMPG4.temporalCompression = false;
						//mediaMPG4.fixedSizeSamples = false;
						//mediaMPG4.unkPtr = IntPtr.Zero;
						//mediaMPG4.formatType = FormatType.Mpeg2Video;
						//mediaMPG4.formatSize = Mpeg2ProgramVideo.GetLength(0);
						//mediaMPG4.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaMPG4.formatSize);
						//System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo, 0, mediaMPG4.formatPtr, mediaMPG4.formatSize);

						//int hr = mpeg2Demultiplexer.CreateOutputPin(mediaMPG4, "MPG4", out this.pinDemuxerVideoMPEG4);
						//if (this.pinDemuxerVideoMPEG4 != null)
						//{
						//    IMPEG2PIDMap mpeg2PIDMap = this.pinDemuxerVideoMPEG4 as IMPEG2PIDMap;
						//    if (mpeg2PIDMap != null)
						//        hr = mpeg2PIDMap.MapPID(1, new int[] { 0x00a2 }, MediaSampleContent.ElementaryStream);
						//    Marshal.ReleaseComObject(this.pinDemuxerVideoMPEG4);
						//}
						////if (hr < 0 || this.pinDemuxerVideoMPEG4 == null)
						////{

						////    _lastError = String.Format("failed to add mpg4 video pin");
						////    Log.WriteFile(Log.LogType.Log, true, "DVBGraphBDA:FAILED to create MPG4 pin:0x{0:X}", hr);
						////}


						//DsDevice[] tunDevices = DeviceEnumerator.GetH264Devices();
						//if (tunDevices.Length > 0)
						//{
						//    IBaseFilter elecardMPEG4VideoDecoder;
						//    hr = this.graphBuilder.AddSourceFilterForMoniker(tunDevices[0].Mon, null, tunDevices[0].Name, out elecardMPEG4VideoDecoder);
						//    DsError.ThrowExceptionForHR(hr);
						//}

						AddRenderers();
						if (!useWPF)
							ConfigureVMR9InWindowlessMode(2);

						//IVMRMixerControl9 vmrMixerControl9 = this.videoRenderer as IVMRMixerControl9;
						//vmrMixerControl9.SetZOrder(0, 1);








						//// Connect the MPEG-2 Demux output pin for the "BDA MPEG2 Transport Information Filter"
						//IPin pinOut = DsFindPin.ByDirection(this.filterSource, PinDirection.Output, 0);
						//if (pinOut != null)
						//{
						//    hr = this.graphBuilder.Render(pinOut);
						//    //DsError.ThrowExceptionForHR(hr);
						//    // In fact the last pin don't render since i havn't added the BDA MPE Filter...
						//    Marshal.ReleaseComObject(pinOut);
						//}

						//ConnectFilters();
						//IPin pinOut = DsFindPin.ByDirection(this.mpeg2Demux, PinDirection.Output, 0);
						//if (pinOut != null)
						//{
						//    hr = this.graphBuilder.Render(pinOut);
						//    //DsError.ThrowExceptionForHR(hr);
						//    // In fact the last pin don't render since i havn't added the BDA MPE Filter...
						//    Marshal.ReleaseComObject(pinOut);
						//}

						//pinOut = DsFindPin.ByDirection(this.mpeg2Demux, PinDirection.Output, 1);
						//if (pinOut != null)
						//{
						//    hr = this.graphBuilder.Render(pinOut);
						//    //DsError.ThrowExceptionForHR(hr);
						//    // In fact the last pin don't render since i havn't added the BDA MPE Filter...
						//    Marshal.ReleaseComObject(pinOut);
						//}

						IPin pinOut = DsFindPin.ByDirection(this.filterSource, PinDirection.Output, 0);
						if (pinOut != null)
						{
							hr = this.graphBuilder.Render(pinOut);
							//DsError.ThrowExceptionForHR(hr);
							// In fact the last pin don't render since i havn't added the BDA MPE Filter...
							Marshal.ReleaseComObject(pinOut);
						}

						AddAndConnectNullRendererForWPF();

						this.hostingControl.CurrentGraphBuilder = this;

						OnGraphStarted();

						return;
					}
				}



				AddRenderers();
				if (!useWPF)
					ConfigureVMR9InWindowlessMode();

				this.graphBuilder.RenderFile(this.fileName, null);

				//AddAndConnectNullRendererForWPF();
				if (useWPF)
				{
					// In order to keep the audio/video synchro, we need the NullRenderer
					IBaseFilter nullRenderer = new NullRenderer() as IBaseFilter;
					hr = graphBuilder.AddFilter(nullRenderer, "NullRenderer");
					ThrowExceptionForHR("Adding the NullRenderer: ", hr);

					IPin pinOutFromFilterOut = DsFindPin.ByDirection(this.videoRenderer, PinDirection.Output, 0);
					if (pinOutFromFilterOut != null)
					{
						UnRender(pinOutFromFilterOut);
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

					WpfUpdateVideoSize(); //WPF
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

		public override void OnGraphStarted()
		{
			base.OnGraphStarted();

			IsPossiblePlayerPlay = false;
			IsPossiblePlayerPause = true;
			IsPossiblePlayerStop = true;

			IsPossibleSetSpeed = true;
			IsPossibleSetPosition = true;
		}

		public override void OnGraphEnded()
		{
			IsPossiblePlayerPlay = false;
			IsPossiblePlayerPause = false;
			IsPossiblePlayerStop = false;

			IsPossibleSetSpeed = false;
			IsPossibleSetPosition = false;

			base.OnGraphEnded();
		}

		private void UnRender(IPin pinOutOrigin)
		{
			int hr;
			//hr = pinOutOrigin.Disconnect();
			IPin pinOutEnd = null;
			hr = pinOutOrigin.ConnectedTo(out pinOutEnd);
			if (pinOutEnd != null)
			{
				try
				{
					PinInfo pInfo = new PinInfo();
					hr = pinOutEnd.QueryPinInfo(out pInfo);
					if (hr >= 0)
					{
						if (pInfo.filter != null)
						{
							try
							{
								IEnumPins ppEnum;
								hr = pInfo.filter.EnumPins(out ppEnum);
								if (hr >= 0)
								{
									try
									{
										// Walk the pins looking for a match
										IPin[] pPins = new IPin[1];
										//22 int lFetched;
										//22 while ((ppEnum.Next(1, pPins, out lFetched) >= 0) && (lFetched == 1))
										while (ppEnum.Next(1, pPins, IntPtr.Zero) >= 0)
										{
											try
											{
												// Read the direction
												PinDirection ppindir;
												hr = pPins[0].QueryDirection(out ppindir);
												if (hr >= 0)
												{
													// Is it the right direction?
													if (ppindir == PinDirection.Output)
													{
														if (pPins[0] != null)
														{
															UnRender(pPins[0]);
														}
													}
												}
											}
											finally
											{
												Marshal.ReleaseComObject(pPins[0]);
											}
										}
									}
									finally
									{
										Marshal.ReleaseComObject(ppEnum);
									}
								}

								hr = graphBuilder.RemoveFilter(pInfo.filter);
							}
							finally
							{
								Marshal.ReleaseComObject(pInfo.filter);
							}
						}
					}
				}
				finally
				{
					Marshal.ReleaseComObject(pinOutEnd);
				}
			}
		}

        protected void AddMPEG2DemuxFilter()
        {
            this.mpeg2Demux = (IBaseFilter)new MPEG2Demultiplexer();

            int hr = this.graphBuilder.AddFilter(this.mpeg2Demux, "MPEG2 Demultiplexer");
            DsError.ThrowExceptionForHR(hr);
        }

		//protected void AddBDADecoderFilters()
		//{
		//    try
		//    {
		//        if (this.DecoderDevice != null)
		//        {
		//            IBaseFilter tmp;
		//            int hr = graphBuilder.AddSourceFilterForMoniker(this.DecoderDevice.Mon, null, this.DecoderDevice.Name, out tmp);
		//            DsError.ThrowExceptionForHR(hr);
		//        }
		//    }
		//    finally
		//    {
		//    }
		//}

		protected void ConnectFilters()
		{
			int hr = 0;
			IPin pinOut;

			// Connect the 5 MPEG-2 Demux output pins
			for (int i = 0; i < 5; i++)
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

        protected override void Decompose()
        {
			if (this.graphBuilder != null)
			{
				int hr = 0;

				OnGraphEnded();

				// Decompose the graph
				if (GetGraphState() != FilterState.Stopped)
				{
					try { hr = (this.graphBuilder as IMediaControl).StopWhenReady(); }
					catch { }
					try { hr = (this.graphBuilder as IMediaControl).Stop(); }
					catch { }
				}
				RemoveHandlers();


				FilterGraphTools.RemoveAllFilters(this.graphBuilder);


				if (this.filterSource != null) Marshal.ReleaseComObject(this.filterSource); this.filterSource = null;
				if (this.mpeg2Demux != null) Marshal.ReleaseComObject(this.mpeg2Demux); this.mpeg2Demux = null;

				if (this.audioRenderer != null) Marshal.ReleaseComObject(this.audioRenderer); this.audioRenderer = null;
				if (this.videoRenderer != null) Marshal.ReleaseComObject(this.videoRenderer); this.videoRenderer = null;

				try { rot.Dispose(); }
				catch { }
				try { Marshal.ReleaseComObject(this.graphBuilder); this.graphBuilder = null; }
				catch { }
			}
        }

		#region IPlayer Members

		PlayerStatus IPlayer.Status
		{
			get
			{
				IMediaControl mediaControl = this.graphBuilder as IMediaControl;
				if (mediaControl == null)
					return PlayerStatus.Stopped;

				FilterState pfs;
				mediaControl.GetState(0, out pfs);
				switch (pfs)
				{
					case FilterState.Paused:
						return PlayerStatus.Paused;
					case FilterState.Running:
						return PlayerStatus.Playing;
					case FilterState.Stopped:
						return PlayerStatus.Stopped;
					default:
						return PlayerStatus.Stopped;
				}
			}
		}

		string IPlayer.FileName { get { return this.fileName; } }

		void IPlayer.Play()
		{
			RunGraph();

			IsPossiblePlayerPlay = IsPossibleGraphRun;
			IsPossiblePlayerPause = IsPossibleGraphPause;
			IsPossiblePlayerStop = IsPossibleGraphStop;
		}

		void IPlayer.Pause()
		{
			PauseGraph();

			IsPossiblePlayerPlay = IsPossibleGraphRun;
			IsPossiblePlayerPause = IsPossibleGraphPause;
			IsPossiblePlayerStop = IsPossibleGraphStop;
		}

		void IPlayer.Stop()
		{
			StopGraph();

			IsPossiblePlayerPlay = IsPossibleGraphRun;
			IsPossiblePlayerPause = IsPossibleGraphPause;
			IsPossiblePlayerStop = IsPossibleGraphStop;
		}

		TimeSpan IPlayer.GetPosition()
		{
			double position;
			(FilterGraph as IMediaPosition).get_CurrentPosition(out position);
			return TimeSpan.FromSeconds(position);
		}

		void IPlayer.SetPosition(TimeSpan position)
		{
			//DsLong startPosition = 0;
			//Guid pFormat;
			//(FilterGraph as IMediaSeeking).GetTimeFormat(out pFormat); -> TIME_FORMAT_MEDIA_TIME
			//(FilterGraph as IMediaSeeking).SetPositions(startPosition, AMSeekingSeekingFlags.AbsolutePositioning, DsLong.FromInt64(0), AMSeekingSeekingFlags.NoPositioning);
			(FilterGraph as IMediaPosition).put_CurrentPosition(position.TotalSeconds);
		}

		TimeSpan IPlayer.GetDuration()
		{
			double duration;
			(FilterGraph as IMediaPosition).get_Duration(out duration);
			return TimeSpan.FromSeconds(duration);
		}

		double IPlayer.GetRate()
		{
			double rate = 1.0;
			IMediaSeeking mediaSeeking = this.graphBuilder as IMediaSeeking;
			if (mediaSeeking != null)
			{
				int hr = mediaSeeking.GetRate(out rate);
			}
			return rate;
		}

		void IPlayer.SetRate(double rate)
		{
			if (rate >= 0 && rate < 0.1)
				rate = 0.1;
			else if (rate < 0 && rate > -0.1)
				rate = -0.1;

			int hr = 0;

			IMediaSeeking mediaSeeking = this.graphBuilder as IMediaSeeking;
			if (mediaSeeking != null)
			{
				hr = mediaSeeking.SetRate(rate);
				if (hr == 0) return;
			}
		}

		#endregion
	}
}
