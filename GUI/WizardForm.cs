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
using System.Diagnostics;
using System.Collections;
using CodeTV.PSI;
using DirectShowLib;
using System.Drawing.Drawing2D;

namespace CodeTV
{
	public partial class WizardForm : Form
	{
		MainForm mainForm = null;
		public MainForm MainForm { get { return mainForm; } set { mainForm = value; } }

		bool startOnNumbering = false;
		public bool StartOnNumbering { get { return startOnNumbering; } set { startOnNumbering = value; } }

		// wizardPageScanner
		private bool continueScanning = true;
		internal Dictionary<string, Dictionary<string, List<string>>> currentFrequencies;
		internal Dictionary<string, List<string>> currentCountries;

		// wizardPageChannelNumber

		private int currentChannelIndentation = 0;
		private Hashtable mapChannelsIndentation = new Hashtable();
		private int maxChannelNumber = 0;


		public WizardForm(MainForm mainForm)
		{
			this.mainForm = mainForm;

			InitializeComponent();


			// wizardPageChannelParameters

			this.comboBoxTunerType.Items.AddRange(new TunerTypeEx[] {
				new TunerTypeEx(TunerType.DVBT),
				new TunerTypeEx(TunerType.DVBC),
				new TunerTypeEx(TunerType.DVBS),
				new TunerTypeEx(TunerType.Analogic)
			});
			this.comboBoxTunerType.SelectedIndex = 0;

			// wizardPageScanner


			// wizardPageChannelNumber

			this.listViewChannelNumbering.SmallImageList = MainForm.imageListLogoTV;
		}

		private void wizardTunning_BeforeSwitchPages(object sender, CristiPotlog.Controls.Wizard.BeforeSwitchPagesEventArgs e)
		{
			CristiPotlog.Controls.WizardPage oldPage = wizardTunning.Pages[e.OldIndex];
			CristiPotlog.Controls.WizardPage newPage = wizardTunning.Pages[e.NewIndex];

			if (oldPage == wizardPageWelcome && e.OldIndex < e.NewIndex)
			{
				if (startOnNumbering)
				{
					e.NewIndex = 4;
				}
			}
			else if (oldPage == wizardPageChannelParameters && e.OldIndex < e.NewIndex)
			{
				if (this.radioButtonAddThisChannel.Checked)
					e.NewIndex++;
			}
			else if (oldPage == wizardPageChannelDestination)
			{
				if (e.OldIndex < e.NewIndex)
					CopyChannelToDestinationFolder();
				else if (e.OldIndex > e.NewIndex)
					if (this.radioButtonAddThisChannel.Checked)
						e.NewIndex--;
			}
			else if (oldPage == wizardPageChannelNumbering && e.OldIndex < e.NewIndex)
			{
				UpdateChannelNumbering();
			}
		}

		private void wizardTunning_AfterSwitchPages(object sender, CristiPotlog.Controls.Wizard.AfterSwitchPagesEventArgs e)
		{
			CristiPotlog.Controls.WizardPage oldPage = wizardTunning.Pages[e.OldIndex];
			CristiPotlog.Controls.WizardPage newPage = wizardTunning.Pages[e.NewIndex];

			if (newPage == wizardPageWelcome && e.OldIndex < e.NewIndex)
			{
			}
			else if (newPage == wizardPageChannelParameters)
			{
				wizardTunning.NextEnabled = ((radioButtonAddThisChannel.Checked || radioButtonScanFrequency.Checked) &&
					propertyGridChannel.SelectedObject != null);
			}
			else if (newPage == wizardPageScanner && e.OldIndex < e.NewIndex)
			{
				InitializeWizardPageScanner();

				wizardTunning.NextEnabled = (listViewScanResult.SelectedItems.Count > 0);
			}
			else if (newPage == wizardPageChannelDestination && e.OldIndex < e.NewIndex)
			{
				wizardTunning.NextEnabled = (textBoxFolderDestinationName.Tag is Channel);
			}
			else if (newPage == wizardPageChannelNumbering && e.OldIndex < e.NewIndex)
			{
				if (startOnNumbering)
					wizardTunning.BackEnabled = false;

				InitChannelNumbering();
			}
		}




		// wizardPageChannelParameters

		private void buttonGetChannel_Click(object sender, EventArgs e)
		{
			contextMenuStripChannel.Show(buttonGetChannel, new Point(0, buttonGetChannel.Height));
		}

		private void contextMenuStripChannel_Opening(object sender, CancelEventArgs e)
		{
			PopulateChannelsInDropDownMenu(this.contextMenuStripChannel.Items, MainForm.rootChannelFolder);
		}

		private void PopulateChannelsInDropDownMenu(ToolStripItemCollection toolStripItemCollection, ChannelFolder channelFolder)
		{
			toolStripItemCollection.Clear();

			foreach (Channel channel in channelFolder.ChannelList)
			{
				ToolStripMenuItem channelToolStripMenuItem = new ToolStripMenuItem(channel.Name);
				channelToolStripMenuItem.Tag = channel;
				if (channel is ChannelFolder)
				{
					channelToolStripMenuItem.DropDownItems.Add("dummy");
					channelToolStripMenuItem.DropDownOpening += new EventHandler(channelToolStripMenuItem_DropDownOpening);
				}
				else
				{
					if (channel is ChannelTV)
					{
						channelToolStripMenuItem.Image = MainForm.imageListLogoTV.Images[(channel as ChannelTV).Logo];
						if(channelToolStripMenuItem.Image == null)
							channelToolStripMenuItem.Image = MainForm.imageListLogoTV.Images["LogoTVDefault"];
					}
					channelToolStripMenuItem.Click += new EventHandler(channelToolStripMenuItem_Click);
				}
				toolStripItemCollection.Add(channelToolStripMenuItem);
			}
		}

		void channelToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			ToolStripDropDownItem toolStripMenuItem = sender as ToolStripDropDownItem;
			ChannelFolder channelFolder = toolStripMenuItem.Tag as ChannelFolder;

			PopulateChannelsInDropDownMenu(toolStripMenuItem.DropDownItems, channelFolder);
		}

		void channelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
			if (toolStripMenuItem.Tag is ChannelTV)
			{
				ChannelTV channel = toolStripMenuItem.Tag as ChannelTV;
				this.propertyGridChannel.SelectedObject = channel;
			}
		}

		private void buttonNewChannel_Click(object sender, EventArgs e)
		{
			this.propertyGridChannel.SelectedObject = (this.comboBoxTunerType.SelectedItem as TunerTypeEx).GetNewChannel();
		}

		private void propertyGridChannel_SelectedObjectsChanged(object sender, EventArgs e)
		{
			if (wizardTunning.SelectedPage == wizardPageChannelParameters)
			{
				wizardTunning.NextEnabled = ((radioButtonAddThisChannel.Checked || radioButtonScanFrequency.Checked) &&
					propertyGridChannel.SelectedObject != null);
			}
		}




		// wizardPageScanner

		private void InitializeWizardPageScanner()
		{
			ChannelTV channelTV = this.propertyGridChannel.SelectedObject as ChannelTV;
			if (channelTV != null)
			{
				switch (channelTV.TunerType)
				{
					case TunerType.DVBT:
						this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.DVBT);
						break;
					case TunerType.DVBC:
						this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.DVBC);
						break;
					case TunerType.DVBS:
						this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.DVBS);
						break;
					case TunerType.Analogic:
						this.currentFrequencies = TransponderReader.GetFrequencies(TunerType.Analogic);
						break;
				}

				if (this.currentFrequencies != null)
				{
					this.comboBoxScanCountry.Items.Clear();
					if (this.currentFrequencies != null)
						foreach (string country in this.currentFrequencies.Keys)
							this.comboBoxScanCountry.Items.Add(country);
					if (this.comboBoxScanCountry.Items.Count > 0)
						this.comboBoxScanCountry.SelectedIndex = 0;

					this.tabControlScanner.Enabled = true;
				}
				else
				{
					this.comboBoxScanCountry.Items.Clear();
					this.comboBoxScanRegion.Items.Clear();
					this.tabControlScanner.Enabled = false;
				}
			}
		}

		private void comboBoxScanCountry_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.currentFrequencies != null)
			{
				this.comboBoxScanRegion.Items.Clear();
				if (this.currentFrequencies.TryGetValue((string)this.comboBoxScanCountry.SelectedItem, out this.currentCountries))
				{
					foreach (string region in this.currentCountries.Keys)
						this.comboBoxScanRegion.Items.Add(region);
				}
				this.comboBoxScanRegion.SelectedIndex = 0;
			}
		}

		private void buttonScanChannels_Click(object sender, EventArgs e)
		{
			if (this.propertyGridChannel.SelectedObject != null && this.propertyGridChannel.SelectedObject is ChannelTV)
			{
				MainForm.ClearGraph();

				this.continueScanning = true;
				this.buttonScanChannels.Enabled = false;
				this.buttonScanStop.Enabled = true;

				ChannelTV templateChannelTV = this.propertyGridChannel.SelectedObject as ChannelTV;
				ChannelTV currentChannelTV;
				bool needRebuild = true;
				if (templateChannelTV is ChannelDVB)
				{
					if (this.tabControlScanner.SelectedTab == this.tabPageScanPredefined)
					{
						List<string> region;
						if (this.currentCountries.TryGetValue((string)this.comboBoxScanRegion.SelectedItem, out region))
						{
							foreach (string currentFrequency in region)
							{
								if (!continueScanning) break;

								currentChannelTV = templateChannelTV.MakeCopy() as ChannelTV;
								TransponderReader.PopulateChannelWithTransponderSettings(ref currentChannelTV, currentFrequency);
								needRebuild = ScanProbeFrequency(currentChannelTV, needRebuild);
							}
						}
					}
					else if (this.tabControlScanner.SelectedTab == this.tabPageScanManual)
					{
						int startFrequency = int.Parse(this.textBoxScanStartFrequency.Text);
						int stopFrequency = int.Parse(this.textBoxScanStopFrequency.Text);
						int bandwidth = int.Parse(this.textBoxScanBandwidth.Text);
						int currentFrequency = startFrequency;
						while (currentFrequency <= stopFrequency && continueScanning)
						{
							currentChannelTV = templateChannelTV.MakeCopy() as ChannelTV;

							(currentChannelTV as ChannelDVB).Frequency = currentFrequency;
							currentFrequency += bandwidth;


							needRebuild = ScanProbeFrequency(currentChannelTV, needRebuild);
						}
					}
				}
				else if (templateChannelTV is ChannelAnalogic)
				{
					int startChannelNumber = 0; // int.Parse(this.textBoxScanStartFrequency.Text);
					int stopChannelNumber = 100; // int.Parse(this.textBoxScanStopFrequency.Text);
					int currentChannelNumber = startChannelNumber;
					while (currentChannelNumber <= stopChannelNumber && continueScanning)
					{
						currentChannelTV = templateChannelTV.MakeCopy() as ChannelTV;

						(currentChannelTV as ChannelAnalogic).Channel = currentChannelNumber;
						currentChannelNumber++;

						needRebuild = ScanProbeFrequency(currentChannelTV, needRebuild);
					}
				}
				this.buttonScanChannels.Enabled = true;
				this.buttonScanStop.Enabled = false;
			}
			else
				MessageBox.Show(Properties.Resources.WizardYouMustCreateChannelTemplate);
		}

		private bool ScanProbeFrequency(ChannelTV currentChannelTV, bool needRebuild)
		{
			this.propertyGridChannel.SelectedObject = currentChannelTV;
			Application.DoEvents();

			MainForm.videoControl.BackColor = Color.Transparent;
			try
			{
				MainForm.TuneChannel(currentChannelTV, needRebuild);
				needRebuild = false;
				textBoxScanStatus.Text = Properties.Resources.Scanning;
				if (currentChannelTV is ChannelDVB)
					textBoxScanFrequency.Text = string.Format(Properties.Resources.ScanningFrequency, (currentChannelTV as ChannelDVB).Frequency);
				else if (currentChannelTV is ChannelAnalogic)
					textBoxScanFrequency.Text = string.Format(Properties.Resources.ScanningChannel, (currentChannelTV as ChannelAnalogic).Channel);
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(MainForm.trace.TraceError, ex.ToString());
				textBoxScanStatus.Text = Properties.Resources.Error + " " + ex.Message;
				textBoxScanFrequency.Text = Properties.Resources.ScanningError;
				MainForm.videoControl.BackColor = MainForm.Settings.VideoBackgroundColor;
			}

			if (currentChannelTV is ChannelDVB)
			{
				if (MainForm.GraphBuilder is IBDA)
				{
					IBDA graphBuilderBDA = MainForm.GraphBuilder as IBDA;
					bool locked, present;
					int strength, quality;
					if ((graphBuilderBDA as ITV).GetSignalStatistics(out locked, out present, out strength, out quality))
					{
						if (locked && present)
						{
							IMpeg2Data mpeg2Data = graphBuilderBDA.SectionsAndTables as IMpeg2Data;

							short originalNetworkId = -1;
							Hashtable serviceNameByServiceId = new Hashtable();
							PSISection[] psiSdts = PSISection.GetPSITable((int)PIDS.SDT, (int)TABLE_IDS.SDT_ACTUAL, mpeg2Data);
							for (int i = 0; i < psiSdts.Length; i++)
							{
								PSISection psiSdt = psiSdts[i];
								if (psiSdt != null && psiSdt is PSISDT)
								{
									PSISDT sdt = (PSISDT)psiSdt;
									Trace.WriteLineIf(MainForm.trace.TraceVerbose, "PSI Table " + i + "/" + psiSdts.Length + "\r\n");
									Trace.WriteLineIf(MainForm.trace.TraceVerbose, sdt.ToString());

									originalNetworkId = (short)sdt.OriginalNetworkId;
									foreach (PSISDT.Data service in sdt.Services)
									{
										serviceNameByServiceId[service.ServiceId] = service.GetServiceName();
									}
								}
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

									ChannelDVB newTemplateChannelDVB = (MainForm.GraphBuilder as ITV).CurrentChannel.MakeCopy() as ChannelDVB;
									newTemplateChannelDVB.ONID = originalNetworkId;
									newTemplateChannelDVB.TSID = pat.TransportStreamId;

									foreach (PSIPAT.Data program in pat.ProgramIds)
									{
										if (!program.IsNetworkPID)
										{
											ChannelDVB newChannelDVB = newTemplateChannelDVB.MakeCopy() as ChannelDVB;
											newChannelDVB.SID = program.ProgramNumber;
											newChannelDVB.Name = (string)serviceNameByServiceId[program.ProgramNumber];
											if (newChannelDVB.Name == null)
												newChannelDVB.Name = Properties.Resources.NoName;

											UpdateDVBChannelPids(mpeg2Data, program.Pid, newChannelDVB);

											ListViewItem lvi = new ListViewItem(newChannelDVB.Name, "LogoTVDefault");
											lvi.SubItems.Add(newChannelDVB.Frequency.ToString());
											lvi.Tag = newChannelDVB;
											this.listViewScanResult.Items.Add(lvi);
											this.buttonScanClear.Enabled = true;

											//PSISection[] psis2 = PSISection.GetPSITable(program.Pid, (int)TABLE_IDS.PMT, mpeg2Data);
											//for (int i2 = 0; i2 < psis2.Length; i2++)
											//{
											//    PSISection psi2 = psis2[i2];
											//    if (psi2 != null && psi2 is PSIPMT)
											//    {
											//        PSIPMT pmt = (PSIPMT)psi2;
											//        Trace.WriteLineIf(trace.TraceVerbose, "PSI Table " + i2 + "/" + psis2.Length + "\r\n");
											//        Trace.WriteLineIf(trace.TraceVerbose, pmt.ToString());
											//    }
											//}
										}
									}
									Application.DoEvents();
								}
							}
						}
					}
				}
			}
			else if (currentChannelTV is ChannelAnalogic)
			{
				GraphBuilderWDM graphBuilderWDM = MainForm.GraphBuilder as GraphBuilderWDM;
				if (graphBuilderWDM.CurrentChannel != null)
				{
					bool locked, present;
					int strength, quality;
					if (graphBuilderWDM.GetSignalStatistics(out locked, out present, out strength, out quality))
					{
						if (locked && present)
						{
							ChannelAnalogic newChannel = graphBuilderWDM.CurrentChannel.MakeCopy() as ChannelAnalogic;
							newChannel.Name = Properties.Resources.NewChannelName + " " + newChannel.Channel.ToString();

							ListViewItem lvi = new ListViewItem(newChannel.Name, "LogoTVDefault");
							lvi.SubItems.Add(newChannel.Channel.ToString());
							lvi.Tag = newChannel;
							this.listViewScanResult.Items.Add(lvi);
							this.buttonScanClear.Enabled = true;
						}
					}
				}
			}
			return needRebuild;
		}

		internal static void UpdateDVBChannelPids(IMpeg2Data mpeg2Data, ushort pmtPid, ChannelDVB channelDVB)
		{
			Hashtable hashtableEcmPids = new Hashtable();

			channelDVB.PmtPid = pmtPid;

			PSISection[] psiPmts = PSISection.GetPSITable(pmtPid, (int)TABLE_IDS.PMT, mpeg2Data);
			for (int i2 = 0; i2 < psiPmts.Length; i2++)
			{
				PSISection psiPmt = psiPmts[i2];
				if (psiPmt != null && psiPmt is PSIPMT)
				{
					PSIPMT pmt = (PSIPMT)psiPmt;

					channelDVB.PcrPid = pmt.PcrPid;
					if (pmt.Descriptors != null)
					{
						foreach (PSIDescriptor descriptor in pmt.Descriptors)
						{
							switch (descriptor.DescriptorTag)
							{
								case DESCRIPTOR_TAGS.DESCR_CA:
									hashtableEcmPids[(descriptor as PSIDescriptorCA).CaPid] = (descriptor as PSIDescriptorCA).CaSystemId;
									break;
							}
						}
					}

					channelDVB.AudioPids = new int[0];
					channelDVB.EcmPids = new int[0];

					foreach (PSIPMT.Data data in pmt.Streams)
					{
						switch ((int)data.StreamType)
						{
							case (int)STREAM_TYPES.STREAMTYPE_11172_VIDEO:
							case (int)STREAM_TYPES.STREAMTYPE_13818_VIDEO:
								channelDVB.VideoDecoderType = ChannelDVB.VideoType.MPEG2;
								channelDVB.VideoPid = data.Pid;
								if (data.Descriptors != null)
								{
									foreach (PSIDescriptor descriptor in data.Descriptors)
										if (descriptor.DescriptorTag == DESCRIPTOR_TAGS.DESCR_CA)
											hashtableEcmPids[(descriptor as PSIDescriptorCA).CaPid] = (descriptor as PSIDescriptorCA).CaSystemId;
								}
								break;
							case 27: //H264
								channelDVB.VideoDecoderType = ChannelDVB.VideoType.H264;
								channelDVB.VideoPid = data.Pid;
								if (data.Descriptors != null)
								{
									foreach (PSIDescriptor descriptor in data.Descriptors)
										if (descriptor.DescriptorTag == DESCRIPTOR_TAGS.DESCR_CA)
											hashtableEcmPids[(descriptor as PSIDescriptorCA).CaPid] = (descriptor as PSIDescriptorCA).CaSystemId;
								}
								break;
							case (int)STREAM_TYPES.STREAMTYPE_11172_AUDIO:
							case (int)STREAM_TYPES.STREAMTYPE_13818_AUDIO:
								int[] audioPids = new int[channelDVB.AudioPids.Length + 1];
								Array.Copy(channelDVB.AudioPids, audioPids, channelDVB.AudioPids.Length);
								audioPids[channelDVB.AudioPids.Length] = data.Pid;
								channelDVB.AudioPids = audioPids;
								if (audioPids.Length == 1) // If it is the first one
									channelDVB.AudioPid = data.Pid;

								foreach (PSIDescriptor descriptor in data.Descriptors)
									if (descriptor.DescriptorTag == DESCRIPTOR_TAGS.DESCR_CA)
										hashtableEcmPids[(descriptor as PSIDescriptorCA).CaPid] = (descriptor as PSIDescriptorCA).CaSystemId;
								break;
							case (int)STREAM_TYPES.STREAMTYPE_13818_PES_PRIVATE:
								if (data.Descriptors != null)
								{
									foreach (PSIDescriptor descriptor in data.Descriptors)
									{
										if (descriptor.DescriptorTag == DESCRIPTOR_TAGS.DESCR_TELETEXT)
										{
											channelDVB.TeletextPid = data.Pid;
											break;
										}
									}
								}
								break;
						}
					}
				}
			}

			channelDVB.EcmPids = new int[hashtableEcmPids.Count];
			hashtableEcmPids.Keys.CopyTo(channelDVB.EcmPids, 0);
			if (channelDVB.EcmPids.Length > 0)
				channelDVB.EcmPid = channelDVB.EcmPids[0];
		}

		private void buttonScanStop_Click(object sender, EventArgs e)
		{
			this.continueScanning = false;
			this.buttonScanChannels.Enabled = true;
			this.buttonScanStop.Enabled = false;
		}

		private void buttonScanClear_Click(object sender, EventArgs e)
		{
			this.listViewScanResult.Items.Clear();
			this.buttonScanClear.Enabled = false;
		}

		private void buttonSelectAll_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem lvi in listViewScanResult.Items)
				lvi.Selected = true;
		}

		private void listViewScanResult_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (wizardTunning.SelectedPage == wizardPageScanner)
			{
				wizardTunning.NextEnabled = (listViewScanResult.SelectedItems.Count > 0);
			}
		}
		
		// wizardPageChannelDestination

		private void buttonFolderDestination_Click(object sender, EventArgs e)
		{
			contextMenuStripChannelFolder.Show(buttonFolderDestination, new Point(0, buttonFolderDestination.Height));
		}

		private void contextMenuStripChannelFolder_Opening(object sender, CancelEventArgs e)
		{
			PopulateChannelsFolderInDropDownMenu(this.contextMenuStripChannelFolder.Items, MainForm.rootChannelFolder);
		}

		private void PopulateChannelsFolderInDropDownMenu(ToolStripItemCollection toolStripItemCollection, ChannelFolder channelFolder)
		{
			toolStripItemCollection.Clear();
			ToolStripMenuItem channelToolStripMenuItem0 = new ToolStripMenuItem(Properties.Resources.ChooseFolder);
			channelToolStripMenuItem0.Tag = channelFolder;
			channelToolStripMenuItem0.Click += new EventHandler(channelFolderToolStripMenuItem_Click);
			toolStripItemCollection.Add(channelToolStripMenuItem0);

			foreach (Channel channel in channelFolder.ChannelList)
			{
				if (channel is ChannelFolder)
				{
					ToolStripMenuItem channelToolStripMenuItem = new ToolStripMenuItem(channel.Name);
					channelToolStripMenuItem.Tag = channel;
					channelToolStripMenuItem.DropDownItems.Clear();
					channelToolStripMenuItem.DropDownOpening += new EventHandler(channelFolderToolStripMenuItem_DropDownOpening);

					ToolStripMenuItem channelToolStripMenuItem1 = new ToolStripMenuItem("Dummy");
					channelToolStripMenuItem.DropDownItems.Add(channelToolStripMenuItem1);

					toolStripItemCollection.Add(channelToolStripMenuItem);
				}
			}
		}

		void channelFolderToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			ToolStripDropDownItem toolStripMenuItem = sender as ToolStripDropDownItem;
			ChannelFolder channelFolder = toolStripMenuItem.Tag as ChannelFolder;

			PopulateChannelsFolderInDropDownMenu(toolStripMenuItem.DropDownItems, channelFolder);
		}

		void channelFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
			if (toolStripMenuItem.Tag is ChannelFolder)
			{
				ChannelFolder channel = toolStripMenuItem.Tag as ChannelFolder;
				textBoxFolderDestinationName.Tag = channel; // Must be call before changing the "Text" property!!
				textBoxFolderDestinationName.Text = channel.Name;
			}
		}

		private void textBoxFolderDestinationName_TextChanged(object sender, EventArgs e)
		{
			if (wizardTunning.SelectedPage == wizardPageChannelDestination)
			{
				wizardTunning.NextEnabled = (textBoxFolderDestinationName.Tag is Channel);
			}
		}

		private void CopyChannelToDestinationFolder()
		{
			if (radioButtonAddThisChannel.Checked)
			{
				ChannelFolder channelFolder = textBoxFolderDestinationName.Tag as ChannelFolder;
				if (channelFolder != null)
				{
					ChannelTV channelTV = this.propertyGridChannel.SelectedObject as ChannelTV;
					if (channelTV != null)
						MainForm.panelChannel.AddChannelToFavorite(channelFolder, new Channel[] { channelTV });
				}
			}
			else if (radioButtonScanFrequency.Checked)
			{
				ChannelFolder channelFolder = textBoxFolderDestinationName.Tag as ChannelFolder;
				if (channelFolder != null)
				{
					ArrayList al = new ArrayList();
					foreach (ListViewItem lvi in this.listViewScanResult.SelectedItems)
						al.Add((lvi.Tag as Channel).MakeCopy());
					if (al.Count > 0)
						MainForm.panelChannel.AddChannelToFavorite(channelFolder, (Channel[])al.ToArray(typeof(Channel)));
				}
			}
			MainForm.UpdateChannelNumber();
		}


		// wizardPageChannelNumber

		private void InitChannelNumberingRecursedTree(ChannelFolder channelFolder)
		{
			foreach (Channel channel in channelFolder.ChannelList)
			{
				ListViewItem lvi = new ListViewItem(channel.Name);
				lvi.IndentCount = currentChannelIndentation;
				lvi.Tag = channel;
				mapChannelsIndentation[lvi] = currentChannelIndentation;
				if (channel is ChannelTV)
				{
					ChannelTV channelTV = channel as ChannelTV;
					lvi.SubItems.Add(channelTV.ChannelNumber.ToString());
					maxChannelNumber = Math.Max(maxChannelNumber, channelTV.ChannelNumber);
					lvi.ImageKey = (MainForm.imageListLogoTV.Images.ContainsKey(channelTV.Logo) ? channelTV.Logo : "LogoTVDefault");

					this.listViewChannelNumbering.Items.Add(lvi);
				}
				else if (channel is ChannelFolder)
				{
					this.listViewChannelNumbering.Items.Add(lvi);
					lvi.ImageKey = "FolderClosed";

					currentChannelIndentation++;
					InitChannelNumberingRecursedTree(channel as ChannelFolder);
					currentChannelIndentation--;
				}
			}
		}

		private void InitChannelNumbering()
		{
			this.listViewChannelNumbering.SuspendLayout();
			this.listViewChannelNumbering.Items.Clear();
			currentChannelIndentation = 0;
			maxChannelNumber = 0;
			mapChannelsIndentation.Clear();
			InitChannelNumberingRecursedTree(MainForm.rootChannelFolder);
			this.listViewChannelNumbering.ResumeLayout();
			numericUpDownNumberingClickStart.Value = maxChannelNumber + 1;
		}

		private void listViewChannelNumbering_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true;
		}

		private void listViewChannelNumbering_DoubleClick(object sender, EventArgs e)
		{
			if (listViewChannelNumbering.SelectedItems.Count > 0)
			{
				ListViewItem lvi = listViewChannelNumbering.SelectedItems[0];
				if (lvi.Tag is ChannelTV)
				{
					ChannelReNumberingForm dlg = new ChannelReNumberingForm();
					try
					{
						dlg.numericUpDownNumber.Value = decimal.Parse(lvi.SubItems[1].Text);
					}
					catch
					{
						dlg.numericUpDownNumber.Value = -1;
					}
					if (dlg.ShowDialog(this) == DialogResult.OK)
					{
						lvi.SubItems[1].Text = dlg.numericUpDownNumber.Value.ToString();
					}
				}
			}
		}

		private void listViewChannelNumbering_Click(object sender, EventArgs e)
		{
			if (listViewChannelNumbering.SelectedItems.Count > 0 && checkBoxClickNumbering.Checked)
			{
				ListViewItem lvi = listViewChannelNumbering.SelectedItems[0];
				if(lvi.Tag is ChannelTV)
				{
					lvi.SubItems[1].Text = numericUpDownNumberingClickStart.Value.ToString();
					numericUpDownNumberingClickStart.Value = ++numericUpDownNumberingClickStart.Value;
				}
			}
		}

		private void UpdateChannelNumbering()
		{
			foreach (ListViewItem lvi in listViewChannelNumbering.Items)
			{
				if (lvi.Tag is ChannelTV)
				{
					ChannelTV channelTV = lvi.Tag as ChannelTV;
					channelTV.ChannelNumber = short.Parse(lvi.SubItems[1].Text);
				}
			}
			MainForm.UpdateChannelNumber();
		}
	}

	public class ListViewEx : ListView
	{
		public ListViewEx() : base()
		{
			DoubleBuffered = true;
		}
	}
}