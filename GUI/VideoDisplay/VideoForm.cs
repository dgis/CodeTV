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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace CodeTV
{
	public partial class VideoForm : Form
	{
		VideoMode videoMode = VideoMode.Normal;

		public VideoMode FormVideoMode { get { return videoMode; } set { videoMode = value; } }

		public VideoForm()
		{
			InitializeComponent();
		}

		private void VideoForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
		}

		protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
		{
			//base.OnPaintBackground(pevent);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
		}

		private const int WM_MOUSEFIRST                   = 0x0200;
		private const int WM_MOUSEMOVE                    = 0x0200;
		private const int WM_LBUTTONDOWN                  = 0x0201;
		private const int WM_LBUTTONUP                    = 0x0202;
		private const int WM_LBUTTONDBLCLK                = 0x0203;
		private const int WM_RBUTTONDOWN                  = 0x0204;
		private const int WM_RBUTTONUP                    = 0x0205;
		private const int WM_RBUTTONDBLCLK                = 0x0206;
		private const int WM_MBUTTONDOWN                  = 0x0207;
		private const int WM_MBUTTONUP                    = 0x0208;
		private const int WM_MBUTTONDBLCLK                = 0x0209;
		private const int WM_MOUSEWHEEL                   = 0x020A;

		public override bool PreProcessMessage(ref Message msg)
		{
			if (videoMode == VideoMode.TV)
			{
				switch (msg.Msg)
				{
					case WM_LBUTTONDOWN:
						break;
					case WM_MOUSEMOVE:
						break;
					case WM_LBUTTONUP:
						break;
				}
			}
			return base.PreProcessMessage(ref msg);
		}

		protected override void WndProc(ref Message m)
		{
			if (videoMode == VideoMode.TV)
			{
				switch (m.Msg)
				{
					case WM_LBUTTONDOWN:
						break;
					case WM_MOUSEMOVE:
						break;
					case WM_LBUTTONUP:
						break;
				}
			}
			base.WndProc(ref m);
		}

		private void VideoForm_Resize(object sender, EventArgs e)
		{
			UpdateSize();
		}

		public void UpdateSize()
		{
			if (videoMode == VideoMode.TV)
			{
				Trace.WriteLineIf(MainForm.trace.TraceInfo, "VideoForm_Resize(...)");
				if (Controls.Count > 0 && Controls[0] is VideoControl)
				{
					VideoControl videoControl = Controls[0] as VideoControl;
					if (videoControl.CurrentGraphBuilder != null)
					{
						Size targetSize = videoControl.CurrentGraphBuilder.CurrentVideoTargetRectangle.Size;
						double ratioClient = (double)ClientSize.Height / (double)ClientSize.Width;
						double ratioTarget = (double)targetSize.Height / (double)targetSize.Width;
						Trace.WriteLineIf(MainForm.trace.TraceVerbose, string.Format(("\tClientSize {0}, TargetSize {1}"), ClientSize, targetSize));
						Trace.WriteLineIf(MainForm.trace.TraceVerbose, string.Format(("\tratioClient {0}, ratioTarget {1}"), ratioClient, ratioTarget));
						if (ratioClient != ratioTarget)
						{
							Trace.WriteLineIf(MainForm.trace.TraceVerbose, "\tratioClient != ratioTarget");

							//Size newClientSize = ClientSize;

							if (ratioClient > ratioTarget)
								Height += (int)((double)ClientSize.Width * ratioTarget) - ClientSize.Height;
							//newClientSize.Height = (int)((double)newClientSize.Width * ratioTarget);
							else
								Width += (int)((double)ClientSize.Height / ratioTarget) - ClientSize.Width;
							//newClientSize.Width = (int)((double)newClientSize.Height / ratioTarget);
							//if (ClientSize.Width < targetSize.Width)
							//    newClientSize.Width = (int)((double)newClientSize.Height / ratioTarget);
							//else
							//    newClientSize.Height = (int)((double)newClientSize.Width * ratioTarget);

							//ClientSize = newClientSize;
							videoControl.Invalidate();
						}
					}
				}
			}
		}
	}
}