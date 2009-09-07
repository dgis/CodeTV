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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeTV.Controls
{
	class TrackBarEx : TrackBar
	{
		protected	bool dragging = false;

		protected const int WM_ERASEBKGND = 0x0014;
		protected const int WM_HSCROLL = 0x0114;
		protected const int WM_USER = 0x0400;

		protected const int TBM_GETTHUMBRECT = WM_USER + 25;
		protected const int TBM_GETCHANNELRECT = WM_USER + 26;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		protected static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		protected static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

		protected const uint TB_LINEUP = 0;
		protected const uint TB_LINEDOWN = 1;
		protected const uint TB_PAGEUP = 2;
		protected const uint TB_PAGEDOWN = 3;
		protected const uint TB_THUMBPOSITION = 4;
		protected const uint TB_THUMBTRACK = 5;
		protected const uint TB_TOP = 6;
		protected const uint TB_BOTTOM = 7;
		protected const uint TB_ENDTRACK = 8;

		public TrackBarEx()
			: base()
		{

		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if(!dragging)
			{
				dragging = true;
				//Capture = true;
				Select(); // SetFocus();

				SetKnob(e.Location);
				PostMessageToParent(TB_THUMBPOSITION);
			}
			else
				base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if(dragging)
			{
				dragging = false;
				//Capture = false;
				SetKnob(e.Location);
				PostMessageToParent(TB_THUMBPOSITION);
		//		RedrawWindow();
			}
			else
				base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if(dragging)
			{
				SetKnob(e.Location);
				PostMessageToParent(TB_THUMBTRACK);
		//		PostMessageToParent(TB_THUMBPOSITION);
		//		RedrawWindow();
			}
			else
				base.OnMouseMove(e);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECTL
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		protected Rectangle GetChannelRect()
		{
			RECTL rectl = new RECTL();
			IntPtr lParam = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RECTL)));
			Marshal.StructureToPtr(rectl, lParam, false);
			SendMessage(Handle, TBM_GETCHANNELRECT, 0, lParam.ToInt32());
			rectl = (RECTL)Marshal.PtrToStructure(lParam, typeof(RECTL));
			Rectangle result = new Rectangle(rectl.left, rectl.top, rectl.right - rectl.left, rectl.bottom - rectl.top);
			Marshal.FreeHGlobal(lParam);
			return result;
		}

		protected Rectangle GetThumbRect()
		{
			RECTL rectl = new RECTL();
			IntPtr lParam = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RECTL)));
			Marshal.StructureToPtr(rectl, lParam, false);
			SendMessage(Handle, TBM_GETTHUMBRECT, 0, lParam.ToInt32());
			rectl = (RECTL)Marshal.PtrToStructure(lParam, typeof(RECTL));
			Rectangle result = new Rectangle(rectl.left, rectl.top, rectl.right - rectl.left, rectl.bottom - rectl.top);
			Marshal.FreeHGlobal(lParam);
			return result;
		}

		protected void SetKnob(Point point)
		{
			Rectangle rectChannel, rectThumb;

			rectChannel = GetChannelRect();
			rectThumb = GetThumbRect();

			int nRangeMin = Minimum;
			int nRangeMax = Maximum;
			int nThumbCorrection = rectThumb.Width / 2;
			int nPosMin = rectChannel.Left + nThumbCorrection;
			int nPosMax = rectChannel.Right - nThumbCorrection;
			long llNewPos = (long)(nRangeMax - nRangeMin) * (long)(point.X - nPosMin);
			if(llNewPos < 0)
				//Value = 0;
				Value = Minimum;
			else if(nPosMax != nPosMin)
			{
				llNewPos /= nPosMax - nPosMin;
				int nNewPos = (int)llNewPos + nRangeMin;
				if(nNewPos > nRangeMax)
					nNewPos = nRangeMax;
				Value = nNewPos;
			}
		}

		protected void PostMessageToParent(uint nSBCode)
		{
			PostMessage(Parent.Handle, WM_HSCROLL, (int)(uint)(((ushort)Value << 16) | (ushort)nSBCode), (int)Handle);
		}

		protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			if (m.Msg == WM_ERASEBKGND)
				return;

			base.WndProc(ref m);
		}

		protected void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);

		}
	}
}
