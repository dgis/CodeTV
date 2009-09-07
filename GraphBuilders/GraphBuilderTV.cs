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
	public class GraphBuilderTV : GraphBuilderBase, ITV
    {
		protected Channel currentChannel;

		protected ICaptureGraphBuilder2 captureGraphBuilder;

		public Channel CurrentChannel { get { return this.currentChannel; } set { this.currentChannel = value; } }

		public GraphBuilderTV(VideoControl renderingControl)
			: base(renderingControl)
        {
        }

		//public override void BuildGraphWithNoRenderer()
		//{
		//    this.graphBuilder = (IFilterGraph2)new FilterGraph();
		//    rot = new DsROTEntry(this.graphBuilder);

		//    OnGraphStarted();
		//}

		public override void BuildGraph()
		{
			this.graphBuilder = (IFilterGraph2)new FilterGraph();
			rot = new DsROTEntry(this.graphBuilder);

			AddRenderers();
			ConfigureVMR9InWindowlessMode();

			this.hostingControl.CurrentGraphBuilder = this;

			OnGraphStarted();
		}

		public virtual bool NeedToRebuildTheGraph(Channel newChannel)
		{
			if (!(newChannel is ChannelTV) || !(this.currentChannel is ChannelTV))
				return true;
			
			ChannelTV channelNew = newChannel as ChannelTV;
			ChannelTV channelCurrent = this.currentChannel as ChannelTV;
			return channelCurrent.NeedToRebuildTheGraph(channelNew);
		}

		public virtual void SubmitTuneRequest(Channel channel)
        {
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

				if (this.audioRenderer != null) Marshal.ReleaseComObject(this.audioRenderer); this.audioRenderer = null;
				if (this.videoRenderer != null) Marshal.ReleaseComObject(this.videoRenderer); this.videoRenderer = null;
				if (this.captureGraphBuilder != null) Marshal.ReleaseComObject(this.captureGraphBuilder); this.captureGraphBuilder = null;

				try { rot.Dispose(); }
				catch { }
				try { Marshal.ReleaseComObject(this.graphBuilder); this.graphBuilder = null; }
				catch { }
			}
        }
		public virtual bool GetSignalStatistics(out bool locked, out bool present, out int strength, out int quality)
		{
			strength = quality = 0;
			locked = present = false;
			return false;
		}
	}
}
