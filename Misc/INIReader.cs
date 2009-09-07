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
using System.Text;
using System.IO;

namespace CodeTV
{
	class INIReader
	{
		private Dictionary<string, Dictionary<string, string>> sections = new Dictionary<string, Dictionary<string, string>>();

		public INIReader(StreamReader streamReader)
		{
			if (streamReader != null)
			{
				Dictionary<string, string> currentSection = null;

				string line;
				while ((line = streamReader.ReadLine()) != null)
				{
					// Remove comments
					int pos = line.IndexOf(';');
					if (pos != -1)
					{
						if (pos == 0)
							line = "";
						else
							line.Substring(0, pos);
					}

					// Get the section
					pos = line.IndexOf('[');
					if (pos != -1)
					{
						string section = line.Substring(pos + 1);
						pos = section.IndexOf(']');
						if (pos != -1)
							section = section.Substring(0, pos);

						if (!this.sections.ContainsKey(section))
							currentSection = this.sections[section] = new Dictionary<string, string>();

						continue;
					}

					// Get the entry
					pos = line.IndexOf('=');
					if (pos != -1 && currentSection != null)
					{
						string key = line.Substring(0, pos).Trim();
						string value = line.Substring(pos + 1).Trim();
						currentSection[key] = value;

						continue;
					}
				}
			}
		}

		public string GetEntry(string section, string entryKey)
		{
			return GetEntry(section, entryKey, null);
		}

		public string GetEntry(string section, string entryKey, string defaultValue)
		{
			try
			{
				return this.sections[section][entryKey];
			}
			catch(KeyNotFoundException) { }
			return defaultValue;
		}
	}
}
