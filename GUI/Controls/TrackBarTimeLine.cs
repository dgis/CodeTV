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
	class TrackBarTimeLine : TrackBarEx
	{
		private const int TBS_AUTOTICKS = 0x0001;
		private const int TBS_VERT = 0x0002;
		private const int TBS_HORZ = 0x0000;
		private const int TBS_TOP = 0x0004;
		private const int TBS_BOTTOM = 0x0000;
		private const int TBS_LEFT = 0x0004;
		private const int TBS_RIGHT = 0x0000;
		private const int TBS_BOTH = 0x0008;
		private const int TBS_NOTICKS = 0x0010;
		private const int TBS_ENABLESELRANGE = 0x0020;
		private const int TBS_FIXEDLENGTH = 0x0040;
		private const int TBS_NOTHUMB = 0x0080;
		private const int TBS_TOOLTIPS = 0x0100;
		private const int TBS_REVERSED = 0x0200;	// Accessibility hint: the smaller number (usually the min value) means "high" and the larger number (usually the max value) means "low"
		private const int TBS_DOWNISLEFT = 0x0400;


		public TrackBarTimeLine()
			: base()
		{
			
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Style |= TBS_HORZ | TBS_TOP | TBS_ENABLESELRANGE | TBS_NOTICKS;
				return createParams;
			}
		}

		bool noValueChangedEvent = false;
		public void SetValue(int newValue)
		{
			noValueChangedEvent = true;
			Value = newValue;
			noValueChangedEvent = false;
		}

		protected override void OnValueChanged(EventArgs e)
		{
			if(!noValueChangedEvent)
				base.OnValueChanged(e);
		}
	}
}
