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
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace CodeTV
{
	public interface IPlugin
	{
		string Name { get; }

		void Start();
		void Stop();

		void OnMainAppStarted(MainForm form);
		void OnMainAppStopped();

		void OnChannelChanged(Channel channel, bool graphNeedRebuild);
		void OnGraphBuilt(GraphBuilderBase graph);
		void OnGraphDecomposed(GraphBuilderBase graph);
	}

	public class PluginManager
	{
		private string pluginDirectory = "";
		private ArrayList plugins = new ArrayList();

		public ArrayList Plugins { get { return this.plugins; } }

		public void LoadPlugins(string folder)
		{
			this.pluginDirectory = folder;
			string[] files = Directory.GetFiles(this.pluginDirectory, "Plugin*.dll");

			foreach (string file in files)
			{
				try
				{
					Assembly assembly = Assembly.LoadFile(file);
					foreach (Type type in assembly.GetTypes())
					{
						if (!type.IsClass || type.IsNotPublic)
							continue;

						if (((IList)type.GetInterfaces()).Contains(typeof(IPlugin)))
							this.plugins.Add((IPlugin)Activator.CreateInstance(type));
					}
				}
				catch //(Exception ex)
				{
				}
			}
		}

		public void Start()
		{
			foreach (IPlugin plugin in this.plugins)
			{
				try
				{
					plugin.Start();
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format(Properties.Resources.PluginException, plugin.Name, ex.ToString()));
				}
			}
		}

		public void Stop()
		{
			foreach (IPlugin plugin in this.plugins)
			{
				try
				{
					plugin.Stop();
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format(Properties.Resources.PluginException, plugin.Name, ex.ToString()));
				}
			}
		}

		public void OnMainAppStarted(MainForm form)
		{
			foreach (IPlugin plugin in this.plugins)
			{
				try
				{
					plugin.OnMainAppStarted(form);
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format(Properties.Resources.PluginException, plugin.Name, ex.ToString()));
				}
			}
		}

		public void OnMainAppStopped()
		{
			foreach (IPlugin plugin in this.plugins)
			{
				try
				{
					plugin.OnMainAppStopped();
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format(Properties.Resources.PluginException, plugin.Name, ex.ToString()));
				}
			}
		}


		public void OnChannelChanged(Channel channel, bool graphNeedRebuild)
		{
			foreach (IPlugin plugin in this.plugins)
			{
				try
				{
					plugin.OnChannelChanged(channel, graphNeedRebuild);
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format(Properties.Resources.PluginException, plugin.Name, ex.ToString()));
				}
			}
		}

		public void OnGraphBuilt(GraphBuilderBase graph)
		{
			foreach (IPlugin plugin in this.plugins)
			{
				try
				{
					plugin.OnGraphBuilt(graph);
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format(Properties.Resources.PluginException, plugin.Name, ex.ToString()));
				}
			}
		}

		public void OnGraphDecomposed(GraphBuilderBase graph)
		{
			foreach (IPlugin plugin in this.plugins)
			{
				try
				{
					plugin.OnGraphDecomposed(graph);
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format(Properties.Resources.PluginException, plugin.Name, ex.ToString()));
				}
			}
		}
	}
}
