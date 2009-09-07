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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace CodeTV
{
	/// <summary>
	/// Summary description for WinLIRC.
	/// </summary>
	public class WinLIRC
	{
		private string hostName = "127.0.0.1";
		private int port = 8765; // WinLirc Default port
		private Control syncControl;
		private TcpClient tcpClient;
		private NetworkStream networkStream;
		private Thread winLircTread;

		public WinLIRC()
		{
		}

		public string HostName { get { return this.hostName; } set { this.hostName = value; } }
		public int Port { get { return this.port; } set { this.port = value; } }
		public Control SyncControl { get { return this.syncControl; } set { this.syncControl = value; } }

		public class CommandReceivedEventArgs : EventArgs
		{
			private string rawIrCommand;
			private string code;
			private int repeat;
			private string irCommand;
			private string remoteCommandName;

			public CommandReceivedEventArgs(string rawIrCommand, string code, int repeat, string irCommand, string remoteCommandName)
			{
				this.rawIrCommand = rawIrCommand;
				this.code = code;
				this.repeat = repeat;
				this.irCommand = irCommand;
				this.remoteCommandName = remoteCommandName;
			}

			public string RawIrCommand { get { return this.rawIrCommand; } }
			public string Code { get { return this.code; } }
			public int Repeat { get { return this.repeat; } }
			public string IrCommand { get { return this.irCommand; } }
			public string RemoteCommandName { get { return this.remoteCommandName; } }
		}

		public delegate void CommandReceivedEventHandler(object sender, CommandReceivedEventArgs e);
		public event CommandReceivedEventHandler CommandReceived;

		private delegate void CommandReceivedDelegate(string irCommand);
		protected void OnCommandReceived(string rawIrCommand)
		{
			if (CommandReceived != null)
			{
				string code = "", irCommand = "", remoteCommandName = "";
				int repeat = 0;
				string[] parameters = rawIrCommand.Split(new char[] { ' ' });
				if (parameters.Length > 0) code = parameters[0];
				if (parameters.Length > 1) try { repeat = int.Parse(parameters[1]); } catch { }
				if (parameters.Length > 2)
				{
					irCommand = parameters[2];
					if (parameters.Length > 3)
					{
						remoteCommandName = parameters[3];
						if (remoteCommandName[remoteCommandName.Length - 1] == '\n')
							remoteCommandName = remoteCommandName.Substring(0, remoteCommandName.Length - 1);
					}

					CommandReceived(this, new CommandReceivedEventArgs(rawIrCommand, code, repeat, irCommand, remoteCommandName));
				}
			}
		}

		public bool Start()
		{
			Stop();

			try
			{
				this.tcpClient = new TcpClient(this.hostName, this.port);
				//this.tcpClient = new TcpClient();
				//this.tcpClient.Connect(this.hostName, this.port);
				this.networkStream = tcpClient.GetStream();

				this.winLircTread = new Thread(new ThreadStart(this.WinLIRCThread));
				this.winLircTread.Name = "WinLIRC";
				this.winLircTread.IsBackground = true;
				this.winLircTread.Start();

				return true;
			}
			catch (ArgumentNullException e)
			{
				Trace.WriteLineIf(MainForm.trace.TraceError, string.Format("ArgumentNullException: {0}", e));
			}
			catch (SocketException e)
			{
				Trace.WriteLineIf(MainForm.trace.TraceError, string.Format("SocketException: {0}", e));
			}
			return false;
		}

		public void Stop()
		{
			Stop(true);
		}

		public void Stop(bool waitClose)
		{
			// Close everything.
			if (this.networkStream != null)
			{
				this.networkStream.Close();
				this.networkStream = null;
			}
			if (this.tcpClient != null)
			{
				this.tcpClient.Close();
				this.tcpClient = null;
			}

			if (this.winLircTread != null)
			{
				try
				{
					// Violently kill the thread
					this.winLircTread.Abort();

					if (waitClose)
					{
						while (this.winLircTread.IsAlive)
						{
							Thread.Sleep(400);
						}
					}
				}
				catch (Exception e)
				{
					Trace.WriteLineIf(MainForm.trace.TraceError, string.Format("SocketException: {0}", e));
				}
				this.winLircTread = null;
			}
		}

		private void WinLIRCThread()
		{
			try
			{
				byte[] data = new byte[256];

				while (true)
				{
					// Read the first batch of the TcpServer response bytes.
					Int32 bytes = this.networkStream.Read(data, 0, data.Length);
					string irCommand = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

					if (this.syncControl != null && this.syncControl.InvokeRequired)
						this.syncControl.BeginInvoke(new CommandReceivedDelegate(this.OnCommandReceived), new object[] { irCommand });
					else
						OnCommandReceived(irCommand);
				}
			}
			catch (ArgumentNullException e)
			{
				Trace.WriteLineIf(MainForm.trace.TraceError, string.Format("ArgumentNullException: {0}", e));
			}
			catch (SocketException e)
			{
				Trace.WriteLineIf(MainForm.trace.TraceError, string.Format("SocketException: {0}", e));
			}
			catch (ThreadAbortException e)
			{
				Trace.WriteLineIf(MainForm.trace.TraceError, string.Format("Thread Aborted: {0}", e));
			}
		}
	}
}
