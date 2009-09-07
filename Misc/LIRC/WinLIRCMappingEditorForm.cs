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
	public partial class WinLIRCMappingEditorForm : Form
	{
		private Settings settings = null;
		private WinLIRC winLIRC = null;
		private Hashtable winLIRCcommandMapping = null;

		public WinLIRCMappingEditorForm(Settings settings)
		{
			InitializeComponent();

			this.settings = settings;

			this.winLIRCcommandMapping = new Hashtable(this.settings.WinLIRCCommandMapping);

			this.winLIRC = new WinLIRC();
			this.winLIRC.SyncControl = this;
			this.winLIRC.HostName = this.settings.WinLIRCHostName;
			this.winLIRC.Port = this.settings.WinLIRCPort;
			this.winLIRC.CommandReceived += new WinLIRC.CommandReceivedEventHandler(winLIRC_CommandReceived);

			foreach (string winLIRCCommand in this.winLIRCcommandMapping.Keys)
			{
				ListViewItem lvi = new ListViewItem(winLIRCCommand);
				lvi.SubItems.Add(((MainForm.CommandName)this.winLIRCcommandMapping[winLIRCCommand]).ToString());
				this.listViewCommandMapping.Items.Add(lvi);
			}

			this.comboBoxApplicationCommand.Items.AddRange(Enum.GetNames(typeof(MainForm.CommandName)));
			this.comboBoxApplicationCommand.SelectedIndex = 0;
		}

		public Hashtable WinLIRCCommandMapping { get { return this.winLIRCcommandMapping; } }

		private void winLIRC_CommandReceived(object sender, WinLIRC.CommandReceivedEventArgs e)
		{
			this.textBoxWinLIRCCommand.Text = e.RemoteCommandName + "-" + e.IrCommand;
		}

		private void checkBoxLearn_CheckedChanged(object sender, EventArgs e)
		{
			if (this.checkBoxLearn.Checked)
			{
				if (!this.winLIRC.Start())
				{
					MessageBox.Show(string.Format("Cannot connect to WinLIRC server: {0}:{1}", this.winLIRC.HostName, this.winLIRC.Port));
					this.checkBoxLearn.Checked = false;
				}
			}
			else
				this.winLIRC.Start();
		}

		private void buttonAddMapping_Click(object sender, EventArgs e)
		{
			MainForm.CommandName commandName = (MainForm.CommandName)Enum.Parse(typeof(MainForm.CommandName), this.comboBoxApplicationCommand.SelectedItem.ToString());
			string winLIRCCommand = this.textBoxWinLIRCCommand.Text;
			if (this.winLIRCcommandMapping.ContainsKey(winLIRCCommand))
			{
				foreach (ListViewItem lvi in this.listViewCommandMapping.Items)
				{
					if (lvi.Text == winLIRCCommand)
					{
						lvi.SubItems[1].Text = commandName.ToString();
						break;
					}
				}
			}
			else
			{
				ListViewItem lvi = new ListViewItem(winLIRCCommand);
				lvi.SubItems.Add(commandName.ToString());
				this.listViewCommandMapping.Items.Add(lvi);
			}
			this.winLIRCcommandMapping[winLIRCCommand] = commandName;
		}

		private void buttonRemoveMapping_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem lvi in new ArrayList(this.listViewCommandMapping.SelectedItems))
			{
				this.winLIRCcommandMapping.Remove(lvi.Text);
				this.listViewCommandMapping.Items.Remove(lvi);
			}
		}

		private void listViewCommandMapping_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.listViewCommandMapping.SelectedItems.Count == 1)
			{
				ListViewItem lvi = this.listViewCommandMapping.SelectedItems[0];
				this.textBoxWinLIRCCommand.Text = lvi.Text;
				this.comboBoxApplicationCommand.SelectedItem = lvi.SubItems[1].Text;
			}
		}
	}
}
