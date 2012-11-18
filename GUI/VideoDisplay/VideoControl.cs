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
using System.Windows.Forms.Integration;

using DirectShowLib;

namespace CodeTV
{
	public partial class VideoControl : UserControl
	{
		//private OSDControl osdControl;
		//private OSDForm osdForm;
		
        private bool useWPF = true;
		private VideoWPF wpfVideo = null;
		private ElementHost wpfElementhost = null;

        private bool useBlackBands = false;
        private List<Control> blackBands = null;

		public VideoControl()
		{
			InitializeComponent();

			//this.DoubleClick += new System.EventHandler(this.videoControl_DoubleClick);
			//this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.videoControl_KeyDown);
			//this.MouseWheel += new MouseEventHandler(videoControl_MouseWheel);
			//this.Resize += new System.EventHandler(this.videoControl_Resize);

			//this.osdForm = new OSDForm();
			//this.osdForm.Location = new System.Drawing.Point(10, 10);
			//this.osdForm.Size = new System.Drawing.Size(200, 300);
			////this.osdForm.TopMost = false;
			//this.osdForm.FormBorderStyle = FormBorderStyle.SizableToolWindow; // FormBorderStyle.None;
			//this.osdForm.DesktopBounds = this.Bounds;
			//this.osdForm.Show();

			//this.osdControl = new OSDControl();
			//this.osdControl.Location = new System.Drawing.Point(10, 10);
			//this.osdControl.Size = new System.Drawing.Size(200, 300);
			////this.osdControl.Dock = DockStyle.Fill;
			//this.osdControl.BackColor = Color.Transparent;
			//Controls.Add(this.osdControl);

			this.Load += new EventHandler(VideoControl_Load);
		}

		private void VideoControl_Load(object sender, EventArgs e)
		{
			if (this.useWPF)
				AddWPFControl();
		}

		private void AddWPFControl()
		{
			if (this.wpfVideo == null)
				this.wpfVideo = new VideoWPF();

			if (this.wpfElementhost == null)
			{
				this.wpfElementhost = new ElementHostEx();
				this.wpfElementhost.Dock = DockStyle.Fill;
				this.wpfElementhost.Child = this.wpfVideo;

				//ElementHost.EnableModelessKeyboardInterop(this.wpfVideo);
				this.wpfElementhost.DoubleClick += new EventHandler(wpfElementhost_DoubleClick);
				wpfVideo.KeyDown += new System.Windows.Input.KeyEventHandler(wpfVideo_KeyDown);
				wpfVideo.MouseDown += new System.Windows.Input.MouseButtonEventHandler(wpfVideo_MouseDown);
			}

			this.Controls.Add(this.wpfElementhost);
		}

		void wpfVideo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			int toto = 0;
			toto += 564;
			//throw new Exception("The method or operation is not implemented.");
		}

		void wpfVideo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			int toto = 0;
			toto += 564;
			//KeyEventArgs keyEventArgs = new KeyEventArgs(e.Key
			//OnKeyDown(keyEventArgs);
		}

		void wpfElementhost_DoubleClick(object sender, EventArgs e)
		{
			int toto = 0;
			toto += 564;
		}

		private const int WM_APP = 0x8000;
		private int currentWindowsMessage = WM_APP + 1;

		private Hashtable subscribersByWindowsMessage = new Hashtable();

		public int SubscribeEvents(IVideoEventHandler videoEventHandler, IMediaEventEx mediaEvent)
		{
			mediaEvent.SetNotifyWindow(Handle, currentWindowsMessage, IntPtr.Zero);
			subscribersByWindowsMessage[currentWindowsMessage] = videoEventHandler;
			return currentWindowsMessage++;
		}

		private GraphBuilderBase currentGraphBuilder = null;
		public GraphBuilderBase CurrentGraphBuilder { get { return currentGraphBuilder; } set { currentGraphBuilder = value; } }


		//protected override void OnResize(EventArgs e)
		//{
		//    base.OnResize(e);

		//    if (this.osdForm != null)
		//        this.osdForm.DesktopBounds = this.Bounds;
		//}

		//protected override void OnPaint(System.Windows.Forms.PaintEventArgs pevent)
		//{
		//    pevent.Graphics.FillRectangle(new SolidBrush(Color.Red), new Rectangle(20, 20, 200, 200));
		//    //g.DrawRectangles(new Pen(Settings.VideoBackgroundColor), (Rectangle[])alRectangles.ToArray(typeof(Rectangle)));
		//}

		public delegate bool PaintBackgroundEventHandler(object sender, System.Windows.Forms.PaintEventArgs pevent);
		public event PaintBackgroundEventHandler PaintBackground;

		protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
		{
			if (!this.useWPF)
			{
				if (PaintBackground != null)
				{
					if (PaintBackground(this, pevent))
						base.OnPaintBackground(pevent);
				}
				else
					base.OnPaintBackground(pevent);
			}
		}

		protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			if (m.Msg > WM_APP && m.Msg < currentWindowsMessage)
			{
				IVideoEventHandler videoEventHandler = (IVideoEventHandler)subscribersByWindowsMessage[m.Msg];
				if (videoEventHandler != null)
				{
					videoEventHandler.OnVideoEvent(m.Msg);
					return;
				}
			}
			base.WndProc(ref m);
		}

		public interface IVideoEventHandler
		{
			void OnVideoEvent(int cookies);
		}


		public bool UseWPF
		{
			get { return this.useWPF; }
			set
			{
				if (this.useWPF != value)
				{
					if (this.useWPF)
					{
						this.Controls.Remove(this.wpfElementhost);
					}
					else
					{
						AddWPFControl();
					}
					this.useWPF = value;
					this.Invalidate();
				}
			}
		}

        public bool UseBlackBands
		{
			get { return this.useBlackBands; }
			set
			{
				if (this.useBlackBands != value)
				{
					if (this.useBlackBands)
                        RemoveSpecialBorder();
					else
                        AddSpecialBorder();
					this.useBlackBands = value;
					this.Invalidate();
				}
			}
		}

        public void ModifyBlackBands(Rectangle[] borders, Color videoBackgroundColor)
        {
            if (!this.UseBlackBands)
                this.UseBlackBands = true;

            if (borders.Length >= 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    Control control = this.blackBands[i];
                    if (control != null)
                    {
                        if (!control.Visible) control.Visible = true;
                        if (control.BackColor != videoBackgroundColor)
                            control.BackColor = videoBackgroundColor;
                        if (control.Location.X != borders[i].X || control.Location.Y != borders[i].Y)
                            control.Location = new System.Drawing.Point(borders[i].X, borders[i].Y);
                        if (control.Size.Width + 1 != borders[i].Width + 1 || control.Size.Height + 1 != borders[i].Height + 1)
                            control.Size = new System.Drawing.Size(borders[i].Width + 1, borders[i].Height + 1);
                    }
                }
            }
        }

        private void AddSpecialBorder()
		{
			if (this.blackBands == null)
            {
				this.blackBands = new List<Control>();

                for (int i = 0; i < 2; i++)
                {
                    Control control = new Control();
                    control.BackColor = System.Drawing.Color.Black;
                    control.Location = new System.Drawing.Point(0, 0);
                    control.Size = new System.Drawing.Size(1, 1);
                    control.TabStop = false;
                    control.Visible = false;
                    this.Controls.Add(control);
                    this.blackBands.Add(control);
                }
            }
		}

        private void RemoveSpecialBorder()
        {
            if (this.blackBands != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    Control control = this.blackBands[0];
                    this.blackBands.RemoveAt(0);
                    this.Controls.Remove(control);
                }
            }
        }

		public System.Windows.Controls.Image WPFImage { get { return this.wpfVideo.WPFImage; } }
	}

	public class ElementHostEx : ElementHost
	{
		public ElementHostEx()
			: base()
		{
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			base.OnPaintBackground(pevent);
		}
	}
}
