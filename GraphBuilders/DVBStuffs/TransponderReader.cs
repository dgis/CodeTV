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
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace CodeTV
{
	class TransponderReader
	{
		private static Dictionary<string, Dictionary<string, List<string>>> dvbtChannels;
		private static Dictionary<string, Dictionary<string, List<string>>> dvbcChannels;
		private static Dictionary<string, Dictionary<string, List<string>>> dvbsChannels;

		public static Dictionary<string, Dictionary<string, List<string>>> GetFrequencies(TunerType type)
		{
			switch (type)
			{
				case TunerType.DVBT:
					if (dvbtChannels == null)
						Read("\\Transponders\\TerFiles\\", "TERTYPE", out dvbtChannels);
					return dvbtChannels;
				case TunerType.DVBC:
					if (dvbcChannels == null)
						Read("\\Transponders\\CabFiles\\", "CABTYPE", out dvbcChannels);
					return dvbcChannels;
				case TunerType.DVBS:
					if (dvbsChannels == null)
						Read("\\Transponders\\SatFiles\\", "SATTYPE", out dvbsChannels);
					return dvbsChannels;
				case TunerType.Analogic:
					return null;
			}
			return null;
		}

		private static void Read(string path, string section, out Dictionary<string, Dictionary<string, List<string>>> channels)
		{
			channels = new Dictionary<string, Dictionary<string, List<string>>>();
			try
			{
				string[] files = Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + path, "*.ini");
				foreach (string file in files)
				{
					if (!File.Exists(file)) continue;

					using (StreamReader streamReader = File.OpenText(file))
					{
						ReadFile(streamReader, section, channels);
					}
				}
			}
			catch (Exception)// ex)
			{
				//MessageBox.Show(string.Format("Cannot find transponder (frequency) files:\r\n\t{0}", ex.Message));
			}

			Assembly assembly = Assembly.GetExecutingAssembly();
			AssemblyName an = assembly.GetName();
			string cabFiles = an.Name + path.Replace('\\', '.');
			string[] fileResources = assembly.GetManifestResourceNames();
			foreach (string fileResource in fileResources)
			{
				if (fileResource.StartsWith(cabFiles, StringComparison.InvariantCultureIgnoreCase))
				{
					using (StreamReader streamReader = new StreamReader(assembly.GetManifestResourceStream(fileResource)))
						ReadFile(streamReader, section, channels);
				}
			}
		}

		private static void ReadFile(StreamReader streamReader, string section, Dictionary<string, Dictionary<string, List<string>>> channels)
		{
			INIReader iniReader = new INIReader(streamReader);
			string country = iniReader.GetEntry(section, "1");
			if (country != null)
			{
				Dictionary<string, List<string>> regions;
				if (!channels.TryGetValue(country, out regions))
					channels[country] = regions = new Dictionary<string, List<string>>();

				string region = iniReader.GetEntry(section, "2");
				if (region != null)
				{
					List<string> frequencies;
					if (!regions.TryGetValue(region, out frequencies))
						regions[region] = frequencies = new List<string>();

					string numberOfFrequencies = iniReader.GetEntry("DVB", "0");
					if (numberOfFrequencies != null)
					{
						int count = int.Parse(numberOfFrequencies);
						for (int i = 1; i <= count; i++)
							frequencies.Add(iniReader.GetEntry("DVB", i.ToString()));
					}
				}
			}
		}

		public static void PopulateChannelWithTransponderSettings(ref ChannelTV channel, string frequency)
		{
			string[] fields = frequency.Split(new char[] { ',' });
			if (channel is ChannelDVBT)
			{
				//Channelname,Frequency(KHz),Inversion(0,1),0,Bandwidth (7MHz=1, 8MHz=2)
				//C05,177.500,0,0,1
				ChannelDVBT channelDVBT = channel as ChannelDVBT;
				channelDVBT.Frequency = int.Parse(fields[1].Trim().Replace(".", ""));
				channelDVBT.Bandwidth = (fields[4].Trim() == "1" ? 7 : 8);

			}
			else if (channel is ChannelDVBC)
			{
				//Frequency(KHz), Symbolrate(Ks/s), QAM
				//50500,6900,0
				//57500,6900,64
				ChannelDVBC channelDVBC = channel as ChannelDVBC;
				channelDVBC.Frequency = int.Parse(fields[0].Trim().Replace(".", ""));
				channelDVBC.SymbolRate = int.Parse(fields[1].Trim());
				int modulation = int.Parse(fields[2].Trim());
				switch (modulation)
				{
					case 0: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.ModNotSet; break;
					case 16: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod16Qam; break;
					case 32: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod32Qam; break;
					case 64: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod64Qam; break;
					case 80: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod80Qam; break;
					case 96: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod96Qam; break;
					case 112: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod112Qam; break;
					case 128: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod128Qam; break;
					case 160: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod160Qam; break;
					case 192: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod192Qam; break;
					case 224: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod224Qam; break;
					case 256: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod256Qam; break;
					case 320: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod320Qam; break;
					case 384: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod384Qam; break;
					case 448: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod448Qam; break;
					case 512: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod512Qam; break;
					case 640: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod640Qam; break;
					case 768: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod768Qam; break;
					case 896: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod896Qam; break;
					case 1024: channelDVBC.Modulation = DirectShowLib.BDA.ModulationType.Mod1024Qam; break;
				}
			}
			else if (channel is ChannelDVBS)
			{
				//Frequecny,polarisation,symbol rate,FEC
				//11016,H, 6284,78
				ChannelDVBS channelDVBS = channel as ChannelDVBS;

				channelDVBS.Frequency = int.Parse(fields[0].Trim().Replace(".", ""));

				string polarisation = fields[1].Trim();
				if (polarisation == "H")
					channelDVBS.SignalPolarisation = DirectShowLib.BDA.Polarisation.LinearH;
				else if (polarisation == "V")
					channelDVBS.SignalPolarisation = DirectShowLib.BDA.Polarisation.LinearV;
				else if (polarisation == "L")
					channelDVBS.SignalPolarisation = DirectShowLib.BDA.Polarisation.CircularL;
				else if (polarisation == "R")
					channelDVBS.SignalPolarisation = DirectShowLib.BDA.Polarisation.CircularR;

				channelDVBS.SymbolRate = int.Parse(fields[2].Trim());
			}
		}
	}
}
