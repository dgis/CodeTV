using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace CodeTV
{
	public partial class AboutForm : Form
	{
		public AboutForm()
		{
			InitializeComponent();

			using (StreamReader streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(MainForm.AssemblyName + ".ReadMe.txt")))
				this.richTextBox.Text = streamReader.ReadToEnd();


			this.richTextBox.LinkClicked += new LinkClickedEventHandler(richTextBox_LinkClicked);
		}

		void richTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(e.LinkText);
		}
	}
}