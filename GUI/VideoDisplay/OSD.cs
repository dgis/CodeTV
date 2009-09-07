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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace CodeTV
{
	class OSD
	{
	}

	public sealed class BitmapGenerator
	{
		public static Bitmap GenerateColorKeyBitmap(Color colorKey, bool useAntiAlias)
		{
			// Some drawing tools needed later
			Pen blackBorder = new Pen(Color.Black, 2.0f);
			Brush green = new SolidBrush(Color.Green);
			Font font = new Font("Tahoma", 16);
			Brush textColorKeyed = new SolidBrush(colorKey);
			Brush textColor = new SolidBrush(Color.White);

			float scale = 4.0f;

			// Create a 256x256 RGB bitmap
			//Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
			//Bitmap bmp = new Bitmap(512, 512, PixelFormat.Format24bppRgb);
			Bitmap bmp = new Bitmap(1024, 1024, PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(bmp);

			// configure antialiased drawing or not
			if (useAntiAlias)
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			else
			{
				g.SmoothingMode = SmoothingMode.None;
				g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
			}

			// Clear the bitmap with the color key
			g.Clear(colorKey);

			// Draw a green circle with black border in the middle
			g.FillEllipse(green, (int)(scale * 50), (int)(scale * 50), (int)(scale * 155), (int)(scale * 155));
			g.DrawEllipse(blackBorder, (int)(scale * 50), (int)(scale * 50), (int)(scale * 155), (int)(scale * 155));

			// Draw some text in the cicle with the color key color
			g.DrawString("Some text...", font, textColorKeyed, (int)(scale * 70), (int)(scale * 120));

			// Draw some text in white elsewhere in the bitmap
			g.DrawString("Other text...", font, textColor, (int)(scale * 10), (int)(scale * 10));
			g.DrawString("Text again...", font, textColor, (int)(scale * 120), (int)(scale * 220));

			// Release GDI+ objects
			blackBorder.Dispose();
			green.Dispose();
			font.Dispose();
			textColor.Dispose();
			g.Dispose();

			// return the bitmap
			return bmp;
		}

		public static Bitmap GenerateAlphaBitmap()
		{
			// Alpha values
			int alpha25 = (int)(255 * 0.25f); // 25% opacity
			int alpha50 = (int)(255 * 0.50f); // 50% opacity
			int alpha75 = (int)(255 * 0.75f); // 75% opacity
			int alpha100 = 255;                // 100% opacity

			// Some drawing tools needed later
			Pen blackBorder = new Pen(Color.Black, 2.0f);
			Brush violet25 = new SolidBrush(Color.FromArgb(alpha25, Color.Violet));
			Brush red50 = new SolidBrush(Color.FromArgb(alpha50, Color.Red));
			Brush green75 = new SolidBrush(Color.FromArgb(alpha75, Color.Green));
			Brush blue100 = new SolidBrush(Color.FromArgb(alpha100, Color.Blue));
			Font font = new Font("Tahoma", 16);
			Brush text100 = new SolidBrush(Color.FromArgb(255, Color.White));

			// Create a 256x256 ARGB bitmap
			Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
			Graphics g = Graphics.FromImage(bmp);

			// Do antialiased drawings
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;

			// Clear the bitmap with complete transparency
			g.Clear(Color.Transparent);

			// Draw serveral cicles with various Alpha values
			g.FillEllipse(violet25, 9, 9, 110, 110);
			g.DrawEllipse(blackBorder, 9, 9, 110, 110);
			g.DrawString("25%", font, text100, 40, 50);

			g.FillEllipse(red50, 137, 9, 110, 110);
			g.DrawEllipse(blackBorder, 137, 9, 110, 110);
			g.DrawString("50%", font, text100, 170, 50);

			g.FillEllipse(green75, 9, 137, 110, 110);
			g.DrawEllipse(blackBorder, 9, 137, 110, 110);
			g.DrawString("75%", font, text100, 40, 180);

			g.FillEllipse(blue100, 137, 137, 110, 110);
			g.DrawEllipse(blackBorder, 137, 137, 110, 110);
			g.DrawString("100%", font, text100, 170, 180);

			// Release GDI+ objects
			blackBorder.Dispose();
			violet25.Dispose();
			red50.Dispose();
			green75.Dispose();
			blue100.Dispose();
			font.Dispose();
			text100.Dispose();
			g.Dispose();

			// return the bitmap
			return bmp;
		}
	}
}
