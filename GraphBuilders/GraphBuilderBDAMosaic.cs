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
using DirectShowLib.Utils;

namespace CodeTV
{
	public class GraphBuilderBDAMosaic : GraphBuilderBDA
    {
		public GraphBuilderBDAMosaic(VideoControl renderingControl)
			: base(renderingControl)
        {
        }

		public override void BuildGraph()
		{
			this.graphBuilder = (IFilterGraph2)new FilterGraph();
			rot = new DsROTEntry(this.graphBuilder);

			this.epg = new EPG(this.hostingControl);

			// Method names should be self explanatory
			AddNetworkProviderFilter(this.objTuningSpace);
			AddMPEG2DemuxFilter();

			//// Elecard MPEG2 Video Decoder {F50B3F13-19C4-11CF-AA9A-02608C9BABA2}
			//IBaseFilter objElecardMPEG2VideoDecoder = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("F50B3F13-19C4-11CF-AA9A-02608C9BABA2")));
			//if (objElecardMPEG2VideoDecoder != null)
			//    this.graphBuilder.AddFilter(objElecardMPEG2VideoDecoder, "Elecard MPEG2 Video Decoder");

			AddAndConnectBDABoardFilters();
			AddTransportStreamFiltersToGraph();
			AddRenderers();
			ConfigureVMR9InWindowlessMode();
			RenderMpeg2DemuxFilters();

			//BuildMosaicGraph(tuningSpace, ArrayList programs);

			this.epg.RegisterEvent(TransportInformationFilter as IConnectionPointContainer);

			this.hostingControl.CurrentGraphBuilder = this;

			OnGraphStarted();
		}

		private static /*unsafe*/ byte[] g_Mpeg2ProgramVideo = new byte[]
		{
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcSource.left              = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcSource.top               = 0x00000000
			0xD0, 0x02, 0x00, 0x00,                         //  .hdr.rcSource.right             = 0x000002d0
			0xE0, 0x01, 0x00, 0x00,                         //  .hdr.rcSource.bottom            = 0x000001e0
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcTarget.left              = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcTarget.top               = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcTarget.right             = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcTarget.bottom            = 0x00000000
			0x00, 0x09, 0x3D, 0x00,                         //  .hdr.dwBitRate                  = 0x003d0900
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.dwBitErrorRate             = 0x00000000
			0x63, 0x17, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame            = 0x0000000000051763
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.dwInterlaceFlags           = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.dwCopyProtectFlags         = 0x00000000
			0x04, 0x00, 0x00, 0x00,                         //  .hdr.dwPictAspectRatioX         = 0x00000004
			0x03, 0x00, 0x00, 0x00,                         //  .hdr.dwPictAspectRatioY         = 0x00000003
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.dwReserved1                = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.dwReserved2                = 0x00000000
			0x28, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biSize           = 0x00000028
			0xD0, 0x02, 0x00, 0x00,                         //  .hdr.bmiHeader.biWidth          = 0x000002d0
			0xE0, 0x01, 0x00, 0x00,                         //  .hdr.bmiHeader.biHeight         = 0x00000000
			0x00, 0x00,                                     //  .hdr.bmiHeader.biPlanes         = 0x0000
			0x00, 0x00,                                     //  .hdr.bmiHeader.biBitCount       = 0x0000
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biCompression    = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biSizeImage      = 0x00000000
			0xD0, 0x07, 0x00, 0x00,                         //  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
			0x27, 0xCF, 0x00, 0x00,                         //  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biClrUsed        = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biClrImportant   = 0x00000000
			0x98, 0xF4, 0x06, 0x00,                         //  .dwStartTimeCode                = 0x0006f498
			0x56, 0x00, 0x00, 0x00,                         //  .cbSequenceHeader               = 0x00000056
			0x02, 0x00, 0x00, 0x00,                         //  .dwProfile                      = 0x00000002
			0x02, 0x00, 0x00, 0x00,                         //  .dwLevel                        = 0x00000002
			0x00, 0x00, 0x00, 0x00,                         //  .Flags                          = 0x00000000
														//  .dwSequenceHeader [1]
			0x00, 0x00, 0x01, 0xB3, 0x2D, 0x01, 0xE0, 0x24,
			0x09, 0xC4, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
			0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
			0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
			0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
			0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
			0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
			0x1A, 0x1A, 0x1A, 0x1A, 0x19, 0x1B, 0x1B, 0x1B, 
			0x1B, 0x1B, 0x1C, 0x1C, 0x1C, 0x1C, 0x1E, 0x1E, 
			0x1E, 0x1F, 0x1F, 0x21, 0x00, 0x00, 0x01, 0xB5, 
			0x14, 0x82, 0x00, 0x01, 0x00, 0x00
		};

		public void AddMosaicFilters(ITuningSpace tuningSpace, ArrayList programs)
		{
			AddRenderers();

			//unsafe
			//{
				IntPtr formatPtr = Marshal.AllocHGlobal(g_Mpeg2ProgramVideo.Length);
				Marshal.Copy(g_Mpeg2ProgramVideo, 0, formatPtr, g_Mpeg2ProgramVideo.Length);

				IMpeg2Demultiplexer mpeg2Demultiplexer = this.mpeg2Demux as IMpeg2Demultiplexer;

				for(int p = 1; p < programs.Count; p++)
				{
					PSI.PSIPMT pmt = (PSI.PSIPMT)programs[p];
					PSI.PSIPMT.Data stream = (PSI.PSIPMT.Data)pmt.GetStreamByType(CodeTV.PSI.STREAM_TYPES.STREAMTYPE_13818_VIDEO);

					AMMediaType mediaType = new AMMediaType();
					mediaType.majorType = MediaType.Video;
					mediaType.subType = MediaSubType.Mpeg2Video;
					mediaType.fixedSizeSamples = false;
					mediaType.temporalCompression = false;
					mediaType.sampleSize = 0;
					mediaType.formatType = FormatType.Mpeg2Video;
					mediaType.unkPtr = IntPtr.Zero;

					mediaType.formatSize = g_Mpeg2ProgramVideo.Length;
					mediaType.formatPtr = formatPtr;

					//mediaType.formatType = FormatType.Mpeg2Video;
					//mediaType.formatSize = 0;
					//mediaType.formatPtr = IntPtr.Zero;

					string pinName = "video" + p;
					IPin outputPin;
					int hr = mpeg2Demultiplexer.CreateOutputPin(mediaType, pinName, out outputPin);
					if (outputPin != null)
					{
						IMPEG2PIDMap mpeg2PIDMap = outputPin as IMPEG2PIDMap;
						if (mpeg2PIDMap != null)
							hr = mpeg2PIDMap.MapPID(1, new int[] { stream.Pid }, MediaSampleContent.ElementaryStream);
						Marshal.ReleaseComObject(outputPin);
					}
				}
				Marshal.FreeHGlobal(formatPtr);
			//}

			ConfigureVMR9InWindowlessMode(programs.Count);
			ConnectAllOutputFilters();

			int numberColumn = 4;
			int numberRow = 4;
			float widthPadding = 0.01f;
			float heightPadding = 0.01f;

			float width = (1.0f / numberColumn) - 2.0f * widthPadding;
			float height = (1.0f / numberRow) - 2.0f * heightPadding;

			IVMRMixerControl9 vmrMixerControl9 = this.videoRenderer as IVMRMixerControl9;
			for (int p = 1; p < programs.Count; p++)
			{
				int column, row = Math.DivRem(p - 1, numberColumn, out column);
				NormalizedRect rect = new NormalizedRect();
				rect.left = (float)column / (float)numberColumn + widthPadding;
				rect.top = (float)row / (float)numberRow + heightPadding;
				rect.right = rect.left + width;
				rect.bottom = rect.top + height;
				vmrMixerControl9.SetOutputRect(p, ref rect);
			}
		}

		private void BuildMosaicGraph(ITuningSpace tuningSpace, ArrayList programs)
		{
			this.graphBuilder = (IFilterGraph2)new FilterGraph();
			rot = new DsROTEntry(this.graphBuilder);

			// Method names should be self explanatory
			AddNetworkProviderFilter(tuningSpace);
			AddMPEG2DemuxFilter();

			AddAndConnectBDABoardFilters();
			AddTransportStreamFiltersToGraph();
			AddRenderers();

			//unsafe
			//{
				IntPtr formatPtr = Marshal.AllocHGlobal(g_Mpeg2ProgramVideo.Length);
				Marshal.Copy(g_Mpeg2ProgramVideo, 0, formatPtr, g_Mpeg2ProgramVideo.Length);

				IMpeg2Demultiplexer mpeg2Demultiplexer = this.mpeg2Demux as IMpeg2Demultiplexer;

				for(int p = 1; p < programs.Count; p++)
				{
					PSI.PSIPMT pmt = (PSI.PSIPMT)programs[p];
					PSI.PSIPMT.Data stream = (PSI.PSIPMT.Data)pmt.GetStreamByType(CodeTV.PSI.STREAM_TYPES.STREAMTYPE_13818_VIDEO);

					AMMediaType mediaType = new AMMediaType();
					mediaType.majorType = MediaType.Video;
					mediaType.subType = MediaSubType.Mpeg2Video;
					mediaType.fixedSizeSamples = false;
					mediaType.temporalCompression = false;
					mediaType.sampleSize = 0;
					mediaType.formatType = FormatType.Mpeg2Video;
					mediaType.unkPtr = IntPtr.Zero;

					mediaType.formatSize = g_Mpeg2ProgramVideo.Length;
					mediaType.formatPtr = formatPtr;

					//mediaType.formatType = FormatType.Mpeg2Video;
					//mediaType.formatSize = 0;
					//mediaType.formatPtr = IntPtr.Zero;

					string pinName = "video" + p;
					IPin outputPin;
					int hr = mpeg2Demultiplexer.CreateOutputPin(mediaType, pinName, out outputPin);
					if (outputPin != null)
					{
						IMPEG2PIDMap mpeg2PIDMap = outputPin as IMPEG2PIDMap;
						if (mpeg2PIDMap != null)
							hr = mpeg2PIDMap.MapPID(1, new int[] { stream.Pid }, MediaSampleContent.ElementaryStream);
						Marshal.ReleaseComObject(outputPin);
					}
				}
				Marshal.FreeHGlobal(formatPtr);
			//}

			ConfigureVMR9InWindowlessMode(programs.Count);
			ConnectAllOutputFilters();

			int numberColumn = 4;
			int numberRow = 4;
			float widthPadding = 0.01f;
			float heightPadding = 0.01f;

			float width = (1.0f / numberColumn) - 2.0f * widthPadding;
			float height = (1.0f / numberRow) - 2.0f * heightPadding;

			IVMRMixerControl9 vmrMixerControl9 = this.videoRenderer as IVMRMixerControl9;
			for (int p = 1; p < programs.Count; p++)
			{
				int column, row = Math.DivRem(p - 1, numberColumn, out column);
				NormalizedRect rect = new NormalizedRect();
				rect.left = (float)column / (float)numberColumn + widthPadding;
				rect.top = (float)row / (float)numberRow + heightPadding;
				rect.right = rect.left + width;
				rect.bottom = rect.top + height;
				vmrMixerControl9.SetOutputRect(p, ref rect);
			}
		}
	}
}
