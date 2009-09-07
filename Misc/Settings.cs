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
using System.Configuration;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace CodeTV
{
	public class Settings
	{
		private static XmlSerializer tvntSettingsXmlSerializer = new XmlSerializer(typeof(Settings));

		private int volume = 0;
		private int balance = 0;
		private Color videoBackgroundColor = Color.Black;
		private bool useVideo169Mode = false;
		private VideoMode startVideoMode = VideoMode.Normal;
		private bool timeShiftingActivated = false;
		private bool useWPF = false;

		private bool useWinLIRC = false;
		private string winLIRCHostName = "localhost";
		private ushort winLIRCPort = 8765;
		private Hashtable winLIRCcommandMapping = new Hashtable();
		private HashtableSerializationProxy winLIRCcommandMappingProxy = null;

		private string snapshotsFolder = "Snapshots";
		private string videosFolder = "Recorder";

		private int timeShiftingBufferLengthMin = 180; // 3
		private int timeShiftingBufferLengthMax = 180; // 3

		[BrowsableAttribute(false)]
		[Category("Rendering"), Description("The master volume.")]
		public int Volume { get { return this.volume; } set { this.volume = value; OnPropertyChanged("Volume"); } }

		[BrowsableAttribute(false)]
		[Category("Rendering"), Description("The balance.")]
		public int Balance { get { return this.balance; } set { this.balance = value; OnPropertyChanged("Balance"); } }

		[XmlIgnoreAttribute]
		[Category("Rendering"), DisplayName("Video Background Color"), Description("The background color around the video.")]
		public Color VideoBackgroundColor { get { return this.videoBackgroundColor; } set { this.videoBackgroundColor = value; OnPropertyChanged("VideoBackgroundColor"); } }

		[BrowsableAttribute(false)]
		[XmlElement("VideoBackgroundColor")]
		public string VideoBackgroundColorString { get { return ColorTranslator.ToHtml(this.videoBackgroundColor); } set { this.videoBackgroundColor = ColorTranslator.FromHtml(value); OnPropertyChanged("VideoBackgroundColorString"); } }

		[Category("Rendering"), DisplayName("Use Video 16:9 Mode"), Description("Use the video 16:9 mode to modify all the video aspect ration for 16:9 TV.")]
		public bool UseVideo169Mode { get { return this.useVideo169Mode; } set { this.useVideo169Mode = value; OnPropertyChanged("UseVideo169Mode"); } }

		[Category("Rendering"), DisplayName("Starting Video Mode"), Description("The video mode (Normal, TV or Fullscreen).")]
		public VideoMode StartVideoMode { get { return this.startVideoMode; } set { this.startVideoMode = value; OnPropertyChanged("StartVideoMode"); } }

		[Category("Rendering"), DisplayName("Use WPF Renderer (Experimental)"), Description("Sets to true to use the WPF rendering.")]
		public bool UseWPF { get { return this.useWPF; } set { this.useWPF = value; OnPropertyChanged("UseWPF"); } }

		[Category("Remote Command"), DisplayName("Use WinLIRC Remote Control"), Description("Use the WinLIRC remote command.")]
		public bool UseWinLIRC { get { return this.useWinLIRC; } set { this.useWinLIRC = value; OnPropertyChanged("UseWinLIRC"); } }

		[Category("Remote Command"), DisplayName("WinLIRC Server Host Name"), Description("The WinLIRC hostname (Default is localhost).")]
		public string WinLIRCHostName { get { return this.winLIRCHostName; } set { this.winLIRCHostName = value; OnPropertyChanged("WinLIRCHostName"); } }

		[Category("Remote Command"), DisplayName("WinLIRC Server Port"), Description("The WinLIRC port (Default is 8765).")]
		public ushort WinLIRCPort { get { return this.winLIRCPort; } set { this.winLIRCPort = value; OnPropertyChanged("WinLIRCPort"); } }

		[XmlIgnoreAttribute]
		[Editor(typeof(WinLIRCCommandNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
		[Category("Remote Command"), DisplayName("WinLIRC Command Mapping"), Description("The WinLIRC command mapping.")]
		public Hashtable WinLIRCCommandMapping { get { return this.winLIRCcommandMapping; } set { this.winLIRCcommandMapping = value; this.winLIRCcommandMappingProxy = new HashtableSerializationProxy(this.winLIRCcommandMapping); OnPropertyChanged("WinLIRCCommandMapping"); } }

		[BrowsableAttribute(false)]
		public HashtableSerializationProxy WinLIRCCommandMappingProxy { get { return this.winLIRCcommandMappingProxy; } set { this.winLIRCcommandMappingProxy = value; this.winLIRCcommandMapping = this.winLIRCcommandMappingProxy.EmbeddedHashTable; } }


		[Category("Folders"), DisplayName("Snapshots Folder"), Description("The folder where the snapshots are saved.")]
		public string SnapshotsFolder { get { return this.snapshotsFolder; } set { this.snapshotsFolder = value; OnPropertyChanged("SnapshotsFolder"); } }

		[Category("Folders"), DisplayName("Videos Folder"), Description("The folder where the videos are saved.")]
		public string VideosFolder { get { return this.videosFolder; } set { this.videosFolder = value; OnPropertyChanged("VideosFolder"); } }


		[Category("Time Shifting"), DisplayName("Time Shifting Minumum Length"), Description("The minimum time in minute the buffer can be (multiple of 10 minutes). The default is 180 minutes. Whaterver you put, the minimun will be 40 minutes and the maximum 1000 minutes.")]
		public int TimeShiftingBufferLengthMin { get { return this.timeShiftingBufferLengthMin; } set { this.timeShiftingBufferLengthMin = value; OnPropertyChanged("TimeShiftingBufferLengthMin"); } }

		[Category("Time Shifting"), DisplayName("Time Shifting Maximum Length"), Description("The maximum time in minute the buffer can be (multiple of 10 minutes). The default is 180 minutes. Whaterver you put, the minimun will be 60 minutes and the maximum 1020 minutes.")]
		public int TimeShiftingBufferLengthMax { get { return this.timeShiftingBufferLengthMax; } set { this.timeShiftingBufferLengthMax = value; OnPropertyChanged("TimeShiftingBufferLengthMax"); } }

		[Category("Time Shifting"), DisplayName("Time Shifting Activated"), Description("If true, the time shifting is activated at the start of the application.")]
		public bool TimeShiftingActivated { get { return this.timeShiftingActivated; } set { this.timeShiftingActivated = value; OnPropertyChanged("TimeShiftingActivated"); } }


		//[Category("TV"), Description("Last tune channel")]
		//public string LastChannel { get { return this.lastChannel; } set { this.lastChannel = value; } }

		//[XmlIgnoreAttribute]
		//[BrowsableAttribute(false)]
		//public ChannelFolder Parent { get { return this.parent; } set { this.parent = value; } }




		public Settings()
		{
			this.winLIRCcommandMappingProxy = new HashtableSerializationProxy(this.winLIRCcommandMapping);
		}

		public static string SettingsFilePath { get { return FileUtils.WorkingDirectory + "\\" + MainForm.AssemblyName + ".Settings.xml"; } }

		public static Settings LoadDefaultSettings()
		{
			return Load(SettingsFilePath);
		}

		public void SaveDefaultSettings()
		{
			Save(SettingsFilePath);
		}

		public static Settings Load(string fileName)
		{
			Settings tvntSettings = null;
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(fileName, FileMode.Open);
				tvntSettings = Settings.Deserialize(fileStream);
			}
			catch (FileNotFoundException)
			{
				tvntSettings = new Settings();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				if (fileStream != null)
					fileStream.Close();
			}
			return tvntSettings;
		}

		public void Save(string fileName)
		{
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
				Serialize(fileStream);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				if (fileStream != null)
					fileStream.Close();
			}
		}

		public static Settings Deserialize(Stream stream)
		{
			return (Settings)tvntSettingsXmlSerializer.Deserialize(stream);
		}

		public void Serialize(Stream stream)
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, Encoding.UTF8);
			xmlTextWriter.Formatting = Formatting.Indented;
			tvntSettingsXmlSerializer.Serialize(xmlTextWriter, this);
			//xmlTextWriter.Close();
		}

		public Settings MakeCopy()
		{
			MemoryStream memoryStream = new MemoryStream();
			Serialize(memoryStream);
			memoryStream.Seek(0, SeekOrigin.Begin);
			Settings tvntSettingsCopy = Settings.Deserialize(memoryStream);
			memoryStream.Close();
			return (Settings)tvntSettingsCopy;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public class CommandMapping
		{
			private MainForm.CommandName commandName;
			private string remoteCommandName;

			public MainForm.CommandName CommandName
			{
				get { return commandName; }
				set { commandName = value; }
			}

			public string RemoteCommandName
			{
				get { return remoteCommandName; }
				set { remoteCommandName = value; }
			}
		}

		internal class WinLIRCCommandNameEditor : UITypeEditor
		{
			public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
			{
				if (context != null && context.Instance != null && provider != null)
				{
					Settings settings = context.Instance as Settings;
					WinLIRCMappingEditorForm dlg = new WinLIRCMappingEditorForm(settings);
					if(dlg.ShowDialog() == DialogResult.OK)
						return dlg.WinLIRCCommandMapping;
					else
						return settings.WinLIRCCommandMapping;
				}

				return null;
			}

			public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
			{
				if (context != null && context.Instance != null)
					return UITypeEditorEditStyle.Modal;
				return base.GetEditStyle(context);
			}
		}
	}
}
