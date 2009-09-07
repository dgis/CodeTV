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
	class TrackBarSpeed : TrackBarEx
	{
		protected int defaultValue = 0;

		public int DefaultValue { get { return defaultValue; } set { defaultValue = value; } }

		public TrackBarSpeed()
			: base()
		{

		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (dragging)
			{
				dragging = false;
				Capture = false;
			}
			else
				base.OnMouseUp(e);

			Value = defaultValue;
			PostMessageToParent(TB_THUMBPOSITION);
		}
	}
}
