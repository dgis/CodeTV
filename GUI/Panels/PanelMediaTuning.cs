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

namespace CodeTV
{
	public partial class PanelMediaTuning : PanelDockBase
	{
		public PanelMediaTuning()
		{
			InitializeComponent();

			this.MainForm = MainForm.Form;
		}

		internal void trackBarVolume_ValueChanged(object sender, EventArgs e)
		{
			MainForm.Settings.Volume = this.trackBarVolume.Value;
			if (this.MainForm.GraphBuilder != null)
			{
				IBasicAudio basicAudio = this.MainForm.GraphBuilder.FilterGraph as IBasicAudio;
				if (basicAudio != null)
				{
					basicAudio.put_Volume(this.trackBarVolume.Value);
					int volume = 0;
					basicAudio.get_Volume(out volume);
					this.labelVolumeLevel.Text = volume.ToString();
					return;
				}
			}
			this.labelVolumeLevel.Text = this.trackBarVolume.Value.ToString();
		}

		internal void trackBarBalance_ValueChanged(object sender, EventArgs e)
		{
			MainForm.Settings.Balance = this.trackBarBalance.Value;
			if (this.MainForm.GraphBuilder != null)
			{
				IBasicAudio basicAudio = this.MainForm.GraphBuilder.FilterGraph as IBasicAudio;
				if (basicAudio != null)
				{
					basicAudio.put_Balance(this.trackBarBalance.Value);
					int balance = 0;
					basicAudio.get_Balance(out balance);
					this.labelBalanceLevel.Text = balance.ToString();
					return;
				}
			}
			this.labelBalanceLevel.Text = this.trackBarBalance.Value.ToString();
		}

		private void buttonBalanceDefault_Click(object sender, EventArgs e)
		{
			this.trackBarBalance.Value = 0;
		}
	}
}