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
using System.Windows.Forms;

namespace CodeTV
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        //[MTAThread]
        static void Main()
        {
#if !DEBUG

			// Raise the priority of this application to get a fluent video display
			System.Diagnostics.Process self = System.Diagnostics.Process.GetCurrentProcess();
			//self.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
			self.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
#endif
			//Application.CurrentCulture = new System.Globalization.CultureInfo("en-US");

			Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}