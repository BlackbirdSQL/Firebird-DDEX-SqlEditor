// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Controls.ControlUtils
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.Utilities;
using Tracer = BlackbirdSql.Core.Diagnostics.Tracer;

namespace BlackbirdSql.Common.Controls;

public static class ControlUtils
{
	private static readonly SizeF DefaultScaleFactor = new SizeF(1f, 1f);

	public static double DisplayScalePercentX => DpiAwareness.SystemDpiXScale; // DpiHelper.DpiScalePercentX / 100;

	public static double DisplayScalePercentY => DpiAwareness.SystemDpiYScale; // DpiHelper.DpiScalePercentY / 100;


	public static int GetScaledImageSize(int imageSize = 16)
	{
		return Convert.ToInt32((float)imageSize * DisplayScalePercentX);
	}

	public static Icon GetScaledIcon(Icon originalIcon, int iconWidth = 16)
	{
		int scaledImageSize = GetScaledImageSize(iconWidth);
		return new Icon(originalIcon, scaledImageSize, scaledImageSize);
	}

	public static SizeF GetScaleFactor(Control control)
	{
		if (control == null)
		{
			return DefaultScaleFactor;
		}
		using Graphics graphics = control.CreateGraphics();
		return new SizeF(graphics.DpiX / 96f, graphics.DpiY / 96f);
	}

	public static Size GetScaledSize(Control control, Size size)
	{
		SizeF scaleFactor = GetScaleFactor(control);
		if (scaleFactor.Width != 1f || scaleFactor.Height != 1f)
		{
			size.Width = (int)Math.Ceiling((float)size.Width * scaleFactor.Width);
			size.Height = (int)Math.Ceiling((float)size.Height * scaleFactor.Height);
		}
		return size;
	}

	public static Size GetScaledSizeForSmallBitmap(Control control)
	{
		Size size = new Size(16, 16);
		return GetScaledSize(control, size);
	}

	public static bool IsGb18030Supported
	{
		get
		{
			try
			{
				Encoding.GetEncoding(54936);
				return true;
			}
			catch (Exception e)
			{
				Tracer.LogExCatch(typeof(ControlUtils), e);
				return false;
			}
		}
	}



	public static Bitmap ScaleImage(Bitmap image, Control control, bool keepSmooth = true)
	{
		if (control == null)
		{
			return image;
		}
		return ScaleImage(image, GetScaleFactor(control), keepSmooth);
	}

	public static Bitmap ScaleImage(Image image, SizeF scaleFactor, bool keepSmooth = true)
	{
		if (keepSmooth)
		{
			return ScaleImageInternal(image, scaleFactor, InterpolationMode.HighQualityBicubic);
		}
		return ScaleImageInternal(image, scaleFactor, InterpolationMode.NearestNeighbor);
	}

	public static Bitmap ScaleImageInternal(Image image, SizeF scaleFactor, InterpolationMode interpolationMode)
	{
		Bitmap bitmap = new Bitmap((int)((float)image.Width * scaleFactor.Width), (int)((float)image.Height * scaleFactor.Height));
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.InterpolationMode = interpolationMode;
			graphics.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
		}
		image.Dispose();
		return bitmap;
	}

	public static Image GenerateBlankImage(Control control)
	{
		Size size = new Size(16, 16);
		if (control != null)
		{
			size = GetScaledSize(control, size);
		}
		Image image = new Bitmap(size.Width, size.Height);
		using Graphics graphics = Graphics.FromImage(image);
		graphics.FillRectangle(Brushes.Transparent, 0, 0, size.Width, size.Height);
		return image;
	}
}
