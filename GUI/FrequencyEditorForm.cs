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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CodeTV
{
	public partial class FrequencyEditorForm : Form
	{
		private ChannelTV channel;
		private Dictionary<string, Dictionary<string, List<string>>> currentFrequencies;
		private Dictionary<string, List<string>> currentCountries;

		public FrequencyEditorForm(ChannelTV channel)
		{
			InitializeComponent();
			
			this.channel = channel;
		}

		private void FrequencyForm_Load(object sender, EventArgs e)
		{
			if (this.channel is ChannelDVBT)
				this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.DVBT);
			else if (this.channel is ChannelDVBC)
				this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.DVBC);
			else if (this.channel is ChannelDVBS)
				this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.DVBS);

			if (this.currentFrequencies != null)
			{
				this.comboBoxCountry.Items.Clear();
				if (currentFrequencies != null)
					foreach (string country in this.currentFrequencies.Keys)
						this.comboBoxCountry.Items.Add(country);
				if(this.comboBoxCountry.Items.Count > 0)
					this.comboBoxCountry.SelectedIndex = 0;
			}
		}

		private void comboBoxCountry_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.currentFrequencies != null)
			{
				this.comboBoxRegion.Items.Clear();
				if (this.currentFrequencies.TryGetValue((string)this.comboBoxCountry.SelectedItem, out this.currentCountries))
				{
					foreach (string region in this.currentCountries.Keys)
						this.comboBoxRegion.Items.Add(region);
				}
				this.comboBoxRegion.SelectedIndex = 0;
			}
		}

		private void comboBoxRegions_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.currentCountries != null)
			{
				this.listBoxFrequencies.Items.Clear();

				List<string> region;
				if (this.currentCountries.TryGetValue((string)this.comboBoxRegion.SelectedItem, out region))
					foreach (string r in region)
						this.listBoxFrequencies.Items.Add(r);
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			if(this.listBoxFrequencies.SelectedItem != null)
			{
				if (this.channel != null)
				{
					string frequency = this.listBoxFrequencies.SelectedItem as string;
					TransponderReader.PopulateChannelWithTransponderSettings(ref this.channel, frequency);
				}
			}
		}
	}
}