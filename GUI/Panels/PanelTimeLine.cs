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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;
using System.Globalization;

namespace CodeTV
{
	public partial class PanelTimeLine : PanelDockBase
	{
		public PanelTimeLine()
		{
			InitializeComponent();

			MainForm = MainForm.Form;
		}

		private void trackBarExTimeLine_Scroll(object sender, EventArgs e)
		{
			if (MainForm.GraphBuilder is ITimeShifting)
			{
				ITimeShifting timeShifting = MainForm.GraphBuilder as ITimeShifting;
				TimeSpan start;
				TimeSpan stop;
				timeShifting.GetPositions(out start, out stop);

				double currentPosition = (double)this.trackBarExTimeLine.Value / 1000.0;

				TimeSpan position = TimeSpan.FromSeconds(currentPosition) + start;
				timeShifting.SetPosition(position);
			
				MainForm.SetVideoRefreshTimer();
			}
			else if (MainForm.GraphBuilder is IPlayer)
			{
				IPlayer player = MainForm.GraphBuilder as IPlayer;
				double currentPosition = (double)this.trackBarExTimeLine.Value / 1000.0;

				TimeSpan position = TimeSpan.FromSeconds(currentPosition);
				player.SetPosition(position);

				MainForm.SetVideoRefreshTimer();
			}
		}

		private void trackBarSpeed_ValueChanged(object sender, EventArgs e)
		{
			if (MainForm.GraphBuilder is ITimeShifting)
			{
				ITimeShifting timeShifting = MainForm.GraphBuilder as ITimeShifting;
				timeShifting.SetRate((double)this.trackBarSpeed.Value / 100.0);
				double speed = timeShifting.GetRate();
				this.labelSpeed2.Text = string.Format(new CultureInfo(""), "{0:F1}X", speed);

				MainForm.SetVideoRefreshTimer();
			}
			else if (MainForm.GraphBuilder is IPlayer)
			{
				IPlayer player = MainForm.GraphBuilder as IPlayer;
				player.SetRate((double)this.trackBarSpeed.Value / 100.0);
				double speed = player.GetRate();
				this.labelSpeed2.Text = string.Format(new CultureInfo(""), "{0:F1}X", speed);

				MainForm.SetVideoRefreshTimer();
			}
			else
				this.labelSpeed2.Text = string.Format(new CultureInfo(""), "{0:F1}X", (double)this.trackBarSpeed.Value / 100.0);
		}
	}
}