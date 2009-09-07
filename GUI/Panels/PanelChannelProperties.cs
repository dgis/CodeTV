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
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.Utils;
using CodeTV.PSI;

namespace CodeTV
{
	public partial class PanelChannelProperties : PanelDockBase
	{

		public PanelChannelProperties()
		{
			InitializeComponent();

			this.MainForm = MainForm.Form;
		}

		private void PanelChannelProperties_Load(object sender, EventArgs e)
		{
			this.comboBoxTunerType.Items.AddRange(new TunerTypeEx[] {
				new TunerTypeEx(TunerType.DVBT),
				new TunerTypeEx(TunerType.DVBC),
				new TunerTypeEx(TunerType.DVBS),
				new TunerTypeEx(TunerType.Analogic)
			});
			this.comboBoxTunerType.SelectedIndex = 0;

            this.propertyGridChannel.SelectedObjectsChanged += new EventHandler(propertyGridChannel_SelectedObjectsChanged);
		}

        private void comboBoxTunerType_SelectedIndexChanged(object sender, EventArgs e)
        {
        //    TunerTypeEx tunerTypeEx = this.comboBoxTunerType.SelectedItem as TunerTypeEx;
        //    switch (tunerTypeEx.Type)
        //    {
        //        case TunerType.DVBT:
        //            this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.DVBT);
        //            break;
        //        case TunerType.DVBC:
        //            this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.DVBC);
        //            break;
        //        case TunerType.DVBS:
        //            this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.DVBS);
        //            break;
        //        case TunerType.Analogic:
        //            this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.Analogic);
        //            break;
        }

        //    if (this.currentFrequencies != null)
        //    {
        //        MainForm.panelScanner.comboBoxScanCountry.Items.Clear();
        //        if (this.currentFrequencies != null)
        //            foreach (string country in this.currentFrequencies.Keys)
        //                MainForm.panelScanner.comboBoxScanCountry.Items.Add(country);
        //        MainForm.panelScanner.comboBoxScanCountry.SelectedIndex = 0;
        //    }
        //}

		private void buttonNewChannel_Click(object sender, EventArgs e)
		{
			this.propertyGridChannel.SelectedObject = (this.comboBoxTunerType.SelectedItem as TunerTypeEx).GetNewChannel();
			this.MainForm.panelChannel.currentPropertyChannels = null;
			this.buttonPropertyApply.Enabled = false;
		}

		private void buttonAddChannelToFavorite_Click(object sender, EventArgs e)
		{
            MainForm.panelChannel.Show(MainForm.dockPanel);
            MainForm.panelChannel.Select();

			if (this.propertyGridChannel.SelectedObjects != null)
			{
				ArrayList al = new ArrayList();
				foreach (Channel channel in this.propertyGridChannel.SelectedObjects)
					al.Add(channel.MakeCopy());

				MainForm.panelChannel.AddChannelToFavorite((Channel[])al.ToArray(typeof(Channel)));
				//AddChannelToFavorite(new Channel[] { (this.propertyGridChannel.SelectedObject as Channel).MakeCopy() });
			}
		}

		private void buttonChannelUpdatePid_Click(object sender, EventArgs e)
		{
			if (this.propertyGridChannel.SelectedObject is ChannelDVB)
			{
				ChannelDVB currentChannelTV = this.propertyGridChannel.SelectedObject as ChannelDVB;
				ChannelDVB.VideoType videoType = currentChannelTV.VideoDecoderType;
				if (videoType != ChannelDVB.VideoType.MPEG2)
					currentChannelTV.VideoDecoderType = ChannelDVB.VideoType.MPEG2;
				MainForm.TuneChannelGUI(currentChannelTV);
				if (MainForm.GraphBuilder is GraphBuilderBDA)
				{
					IMpeg2Data mpeg2Data = (MainForm.GraphBuilder as GraphBuilderBDA).SectionsAndTables as IMpeg2Data;
					if (mpeg2Data == null)
					{
						MessageBox.Show(Properties.Resources.CannotUpdatePidChannel);
						return;
					}

					PSISection[] psis = PSISection.GetPSITable((int)PIDS.PAT, (int)TABLE_IDS.PAT, mpeg2Data);
					for (int i = 0; i < psis.Length; i++)
					{
						PSISection psi = psis[i];
						if (psi != null && psi is PSIPAT)
						{
							PSIPAT pat = (PSIPAT)psi;
							Trace.WriteLineIf(MainForm.trace.TraceVerbose, "PSI Table " + i + "/" + psis.Length + "\r\n");
							Trace.WriteLineIf(MainForm.trace.TraceVerbose, pat.ToString());

							foreach (PSIPAT.Data program in pat.ProgramIds)
							{
								if (program.ProgramNumber == currentChannelTV.SID)
								{
									WizardForm.UpdateDVBChannelPids(mpeg2Data, program.Pid, currentChannelTV);
									this.propertyGridChannel.SelectedObject = currentChannelTV;
									if (videoType != ChannelDVB.VideoType.MPEG2)
										currentChannelTV.VideoDecoderType = videoType;
									MainForm.TuneChannelGUI(currentChannelTV);
									return;
								}
							}
						}
					}
				}
			}
		}

		internal void buttonChannelTest_Click(object sender, EventArgs e)
		{
			MainForm.TuneChannelGUI((this.propertyGridChannel.SelectedObject as Channel).MakeCopy());
		}

		private void MakeCopyOfSelectedChannel()
		{
			// Make a new copy in the propertyGridChannel in order to be eventually reapply
			Hashtable newPropertyChannels = new Hashtable();
			foreach (TreeNode tn in this.MainForm.panelChannel.currentPropertyChannels.Values)
				newPropertyChannels[(tn.Tag as Channel).MakeCopy()] = tn;
			this.MainForm.panelChannel.currentPropertyChannels = newPropertyChannels;

			this.propertyGridChannel.SelectedObjects = (new ArrayList(this.MainForm.panelChannel.currentPropertyChannels.Keys)).ToArray();
		}

		private void buttonPropertyApply_Click(object sender, EventArgs e)
		{
			foreach (Channel newChannel in this.MainForm.panelChannel.currentPropertyChannels.Keys)
			{
				TreeNode treeNode = this.MainForm.panelChannel.currentPropertyChannels[newChannel] as TreeNode;
				Channel oldChannel = treeNode.Tag as Channel;
				newChannel.Tag = treeNode;
				newChannel.Parent = oldChannel.Parent;
				int index = newChannel.Parent.ChannelList.IndexOf(oldChannel);
				newChannel.Parent.ChannelList.RemoveAt(index);
				newChannel.Parent.ChannelList.Insert(index, newChannel);

				treeNode.Text = newChannel.Name;
				treeNode.Tag = newChannel;
				MainForm.panelChannel.AdjustTVLogo(newChannel);
			}

			MainForm.panelChannelProperties.MakeCopyOfSelectedChannel();

            MainForm.panelChannel.Show(MainForm.dockPanel);
            MainForm.panelChannel.Select();

			MainForm.UpdateChannelNumber();
		}

		private void resetChannelPropertyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.propertyGridChannel.SelectedGridItem != null)
				this.propertyGridChannel.ResetSelectedProperty();
		}

		void propertyGridChannel_SelectedObjectsChanged(object sender, EventArgs e)
		{
			int selectedChannelCount = this.propertyGridChannel.SelectedObjects.Length;
			if (selectedChannelCount == 1)
			{
				this.buttonAddChannelToFavorite.Enabled = true;
				this.buttonChannelTest.Enabled = true;
			}
			else if (selectedChannelCount > 1)
			{
				this.buttonAddChannelToFavorite.Enabled = true;
				this.buttonChannelTest.Enabled = false;
			}
			else
			{
				this.buttonPropertyApply.Enabled = false;
				this.buttonAddChannelToFavorite.Enabled = false;
				this.buttonChannelTest.Enabled = false;
			}
		}
	}
}