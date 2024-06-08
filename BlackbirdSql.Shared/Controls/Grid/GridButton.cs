#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Core;
using Microsoft.Win32;
using BlackbirdSql.Shared.Enums;




// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Controls.Grid
{
	public sealed class GridButton
	{
		private Font m_textFont = Control.DefaultFont;

		private SolidBrush m_textBrush = new SolidBrush(SystemColors.ControlText);

		private bool m_RTL;

		private TextFormatFlags m_cacheFormat = GridConstants.DefaultTextFormatFlags;

		private readonly StringFormat m_cacheGdiPlusFormat = new StringFormat(StringFormatFlags.NoWrap);

		public static readonly int ExtraHorizSpace;

		[ThreadStatic]
		private static SolidBrush _SDisabledButtonTextBrush;

		private static readonly int s_imageTextGap;

		private static SolidBrush SDisabledButtonTextBrush
		{
			get
			{
				return _SDisabledButtonTextBrush ??= new SolidBrush(SystemColors.GrayText);
			}
			set
			{
				_SDisabledButtonTextBrush?.Dispose();

				_SDisabledButtonTextBrush = value;
			}
		}

		public static int ButtonAdditionalHeight => SystemInformation.Border3DSize.Width * 2 + 2;

		public bool RTL
		{
			get
			{
				return m_RTL;
			}
			set
			{
				if (m_RTL != value)
				{
					m_RTL = value;
					if (value)
					{
						m_cacheFormat |= TextFormatFlags.RightToLeft;
						m_cacheGdiPlusFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
					}
					else
					{
						m_cacheFormat &= ~TextFormatFlags.RightToLeft;
						m_cacheGdiPlusFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
					}
				}
			}
		}

		public Font TextFont
		{
			get
			{
				return m_textFont;
			}
			set
			{
				if (value == null)
				{
					ArgumentNullException ex = new();
					Diag.Dug(ex);
					throw ex;
				}
				m_textFont = value;
			}
		}

		public Brush TextBrush
		{
			get
			{
				return m_textBrush;
			}
			set
			{
				if (value == null)
				{
					ArgumentNullException ex = new();
					Diag.Dug(ex);
					throw ex;
				}

				try
				{
					SolidBrush solidBrush = value as SolidBrush ?? throw new ArgumentException(ControlsResources.OnlySolidBrush, "value");
					m_textBrush = solidBrush;
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw ex;
				}
			}
		}

		static GridButton()
		{
			ExtraHorizSpace = SystemInformation.Border3DSize.Width;
			s_imageTextGap = 4;
			SystemEvents.UserPreferenceChanged += OnUserPrefChanged;
		}

		private static void OnUserPrefChanged(object sender, UserPreferenceChangedEventArgs pref)
		{
			SDisabledButtonTextBrush = null;
		}

		public GridButton()
		{
			m_cacheGdiPlusFormat.LineAlignment = StringAlignment.Center;
			m_cacheGdiPlusFormat.HotkeyPrefix = HotkeyPrefix.None;
			m_cacheGdiPlusFormat.Trimming = StringTrimming.EllipsisCharacter;
			m_cacheFormat &= ~TextFormatFlags.SingleLine;
		}

		public static Rectangle CalculateInitialContentsRect(Graphics g, Rectangle r, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, Font textFont, bool bRtl, ref StringFormat sFormat, out int nStringWidth)
		{
			TextFormatFlags sFormat2 = ConvertStringFormatIntoTextFormat(sFormat, adjustStringAlign: false);
			return CalculateInitialContentsRect(g, r, text, bmp, contentsAlignment, textFont, bRtl, ref sFormat2, ref sFormat, out nStringWidth);
		}

		public static Rectangle CalculateInitialContentsRect(Graphics g, Rectangle r, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, Font textFont, bool bRtl, ref TextFormatFlags sFormat, out int nStringWidth)
		{
			StringFormat gdiPlusFormat = null;
			return CalculateInitialContentsRect(g, r, text, bmp, contentsAlignment, textFont, bRtl, ref sFormat, ref gdiPlusFormat, out nStringWidth);
		}

		public static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, Font textFont, Brush textBrush, StringFormat sFormat, bool bRtl)
		{
			Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, textFont, (SolidBrush)textBrush, ConvertStringFormatIntoTextFormat(sFormat, adjustStringAlign: true), bRtl, sFormat, EnGridButtonType.Normal);
		}

		public static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, Font textFont, Brush textBrush, StringFormat sFormat, bool bRtl, EnGridButtonType buttonType)
		{
			Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, textFont, (SolidBrush)textBrush, ConvertStringFormatIntoTextFormat(sFormat, adjustStringAlign: true), bRtl, sFormat, buttonType);
		}

		public static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl)
		{
			Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, textFont, textBrush, sFormat, bRtl, null, EnGridButtonType.Normal);
		}

		public static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl, EnGridButtonType buttonType)
		{
			Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, textFont, textBrush, sFormat, bRtl, null, buttonType);
		}

		public static EnGridButtonArea HitTest(Graphics g, Point point, Rectangle buttonRect, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, Font textFont, Brush textBrush, StringFormat sFormat, bool bRtl)
		{
			return HitTest(g, point, buttonRect, text, bmp, contentsAlignment, tbLayout, textFont, (SolidBrush)textBrush, ConvertStringFormatIntoTextFormat(sFormat, adjustStringAlign: true), bRtl);
		}

		public static EnGridButtonArea HitTest(Graphics g, Point point, Rectangle buttonRect, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl)
		{
			return PaintButtonOrHitTest(g, buttonRect, ButtonState.Normal, text, bmp, contentsAlignment, tbLayout, textFont, null, sFormat, bRtl, bEnabled: true, point, bPaint: false, null);
		}

		public void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled)
		{
			Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, m_textFont, m_textBrush, m_cacheFormat, m_RTL);
		}

		public void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, EnGridButtonType buttonType)
		{
			Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, m_textFont, m_textBrush, m_cacheFormat, m_RTL, buttonType);
		}

		public void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, bool useGdiPlus)
		{
			Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, useGdiPlus, EnGridButtonType.Normal);
		}

		public void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, bool useGdiPlus, EnGridButtonType buttonType)
		{
			if (useGdiPlus)
			{
				Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, m_textFont, m_textBrush, m_cacheGdiPlusFormat, m_RTL, buttonType);
			}
			else
			{
				Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, m_textFont, m_textBrush, m_cacheFormat, m_RTL, buttonType);
			}
		}

		public EnGridButtonArea HitTest(Graphics g, Point p, Rectangle r, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout)
		{
			return HitTest(g, p, r, text, bmp, contentsAlignment, tbLayout, m_textFont, m_textBrush, m_cacheFormat, m_RTL);
		}

		private static void PaintButtonFrame(Graphics g, Rectangle r, ButtonState state, EnGridButtonType buttonType)
		{
			if (Application.RenderWithVisualStyles)
			{
				VisualStyleElement visualStyleElement = null;
				Rectangle bounds = r;
				Rectangle rect = r;
				switch (buttonType)
				{
					case EnGridButtonType.Header:
						visualStyleElement = DrawManager.GetHeader(state);
						rect.Width++;
						bounds.Width++;
						break;
					case EnGridButtonType.Normal:
						visualStyleElement = DrawManager.GetButton(state);
						break;
					case EnGridButtonType.LineNumber:
						visualStyleElement = DrawManager.GetLineIndexButton(state);
						_ = bounds.Width;
						bounds.Width = 0;
						break;
				}

				if (visualStyleElement != null && VisualStyleRenderer.IsElementDefined(visualStyleElement))
				{
					Region clip = g.Clip;
					using (Region clip2 = new Region(rect))
					{
						g.Clip = clip2;
						new VisualStyleRenderer(visualStyleElement).DrawBackground(g, bounds);
						g.Clip = clip;
					}

					return;
				}
			}

			ControlPaint.DrawButton(g, r, state);
		}

		private static Rectangle CalculateInitialContentsRect(Graphics g, Rectangle r, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, Font textFont, bool bRtl, ref TextFormatFlags sFormat, ref StringFormat gdiPlusFormat, out int nStringWidth)
		{
			nStringWidth = 0;
			Size size = new Size(0, 0);
			if (text != null && text.Length > 0)
			{
				SizeF sizeF;
				if (gdiPlusFormat != null)
				{
					sizeF = g.MeasureString(text, textFont);
				}
				else
				{
					Size proposedSize = new Size(int.MaxValue, int.MaxValue);
					sizeF = TextRenderer.MeasureText(g, text, textFont, proposedSize, sFormat);
				}

				size.Width = (int)Math.Ceiling(sizeF.Width);
				size.Height = (int)Math.Ceiling(sizeF.Height);
				nStringWidth = size.Width;
			}

			if (bmp != null)
			{
				size.Width += bmp.Width;
				size.Height = Math.Max(size.Height, bmp.Height);
				if (text != null && text != "")
				{
					size.Width += s_imageTextGap;
				}
			}

			int num = (r.Height - size.Height) / 2;
			Rectangle result;
			if (contentsAlignment == HorizontalAlignment.Left && !bRtl || contentsAlignment == HorizontalAlignment.Right && bRtl)
			{
				result = new Rectangle(r.X + ExtraHorizSpace, r.Y + num, size.Width, size.Height);
				GridConstants.AdjustFormatFlagsForAlignment(ref sFormat, HorizontalAlignment.Left);
				if (gdiPlusFormat != null)
				{
					gdiPlusFormat.Alignment = StringAlignment.Near;
				}
			}
			else if (contentsAlignment == HorizontalAlignment.Right && !bRtl || contentsAlignment == HorizontalAlignment.Left && bRtl)
			{
				result = new Rectangle(r.X + r.Width - ExtraHorizSpace - size.Width, r.Y + num, size.Width, size.Height);
				GridConstants.AdjustFormatFlagsForAlignment(ref sFormat, HorizontalAlignment.Right);
				if (gdiPlusFormat != null)
				{
					gdiPlusFormat.Alignment = StringAlignment.Far;
				}
			}
			else
			{
				int num2 = (int)Math.Ceiling(Math.Max((r.Width - (float)size.Width) / 2f, 0f));
				result = new Rectangle(r.X + num2, r.Y + num, size.Width, size.Height);
				GridConstants.AdjustFormatFlagsForAlignment(ref sFormat, HorizontalAlignment.Center);
				if (gdiPlusFormat != null)
				{
					gdiPlusFormat.Alignment = StringAlignment.Center;
				}
			}

			result.X = Math.Max(result.X, r.X + ExtraHorizSpace);
			result.Width = Math.Min(result.Width, r.Width - 2 * ExtraHorizSpace);
			result.Y = Math.Max(result.Y, r.Y);
			result.Height = Math.Min(result.Height, r.Height);
			return result;
		}

		private static EnGridButtonArea PaintButtonOrHitTest(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl, bool bEnabled, Point ptToHitTest, bool bPaint, StringFormat gdiPlusFormat)
		{
			Rectangle rectangle = CalculateInitialContentsRect(g, r, text, bmp, contentsAlignment, textFont, bRtl, ref sFormat, ref gdiPlusFormat, out int nStringWidth);
			if (rectangle.Width <= 0)
			{
				return EnGridButtonArea.Background;
			}

			if (text != null && text != "")
			{
				if (bmp != null)
				{
					int num = Math.Max((rectangle.Height - bmp.Height) / 2, 0);
					if (tbLayout == EnTextBitmapLayout.TextRightOfBitmap)
					{
						Rectangle rect = new Rectangle(rectangle.X, rectangle.Y + num, Math.Min(bmp.Width, rectangle.Width), bmp.Height);
						rect.Height = Math.Min(rect.Height, rectangle.Height);
						if (bPaint)
						{
							DrawBitmap(g, bmp, rect, bEnabled);
						}
						else if (rect.Contains(ptToHitTest))
						{
							return EnGridButtonArea.Image;
						}

						Rectangle rectangle2 = new Rectangle(rect.Right + s_imageTextGap, rectangle.Y, rectangle.Width - rect.Width - s_imageTextGap, rectangle.Height);
						if (rectangle2.X < rectangle.Right)
						{
							rectangle2.Width = Math.Min(rectangle2.Width, rectangle.Right - rectangle2.X);
							if (bPaint)
							{
								if (gdiPlusFormat != null)
								{
									g.DrawString(text, textFont, textBrush, rectangle2, gdiPlusFormat);
								}
								else
								{
									TextRenderer.DrawText(g, text, textFont, rectangle2, textBrush.Color, sFormat);
								}
							}
							else if (rectangle2.Contains(ptToHitTest))
							{
								return EnGridButtonArea.Text;
							}
						}

						if (!bPaint)
						{
							return EnGridButtonArea.Background;
						}
					}
					else
					{
						Rectangle rectangle2 = new Rectangle(rectangle.X, rectangle.Y, Math.Min(nStringWidth, rectangle.Width), rectangle.Height);
						if (bPaint)
						{
							if (gdiPlusFormat != null)
							{
								g.DrawString(text, textFont, textBrush, rectangle2, gdiPlusFormat);
							}
							else
							{
								TextRenderer.DrawText(g, text, textFont, rectangle2, textBrush.Color, sFormat);
							}
						}
						else if (rectangle2.Contains(ptToHitTest))
						{
							return EnGridButtonArea.Text;
						}

						Rectangle rect = new Rectangle(rectangle2.Right + s_imageTextGap, rectangle.Y + num, rectangle.Width - rectangle2.Width - s_imageTextGap, bmp.Height);
						if (rect.X < rectangle.Right)
						{
							rect.Width = Math.Min(rect.Width, rectangle.Right - rect.X);
							rect.Height = Math.Min(rect.Height, rectangle.Height);
							if (bPaint)
							{
								DrawBitmap(g, bmp, rect, bEnabled);
							}
							else if (rect.Contains(ptToHitTest))
							{
								return EnGridButtonArea.Image;
							}
						}

						if (!bPaint)
						{
							return EnGridButtonArea.Background;
						}
					}
				}
				else
				{
					if (!bPaint)
					{
						if (rectangle.Contains(ptToHitTest))
						{
							return EnGridButtonArea.Text;
						}

						return EnGridButtonArea.Background;
					}

					if (gdiPlusFormat != null)
					{
						g.DrawString(text, textFont, textBrush, rectangle, gdiPlusFormat);
					}
					else
					{
						TextRenderer.DrawText(g, text, textFont, rectangle, textBrush.Color, sFormat);
					}
				}
			}
			else if (bmp != null)
			{
				if (!bPaint)
				{
					if (rectangle.Contains(ptToHitTest))
					{
						return EnGridButtonArea.Image;
					}

					return EnGridButtonArea.Background;
				}

				DrawBitmap(g, bmp, rectangle, bEnabled);
			}
			else if (!bPaint)
			{
				return EnGridButtonArea.Background;
			}

			return EnGridButtonArea.Nothing;
		}

		private static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl, StringFormat gdiPlusFormat, EnGridButtonType buttonType)
		{
			if (!r.IsEmpty)
			{
				SolidBrush textBrush2 = bEnabled ? textBrush : SDisabledButtonTextBrush;
				PaintButtonFrame(g, r, state, buttonType);
				PaintButtonOrHitTest(g, r, state, text, bmp, contentsAlignment, tbLayout, textFont, textBrush2, sFormat, bRtl, bEnabled, Point.Empty, bPaint: true, gdiPlusFormat);
			}
		}

		private static void DrawBitmap(Graphics g, Bitmap bmp, Rectangle rect, bool bEnabled)
		{
			if (bmp != null)
			{
				if (bEnabled)
				{
					g.DrawImage(bmp, rect);
				}
				else
				{
					ControlPaint.DrawImageDisabled(g, bmp, rect.X, rect.Y, SystemColors.Control);
				}
			}
		}

		private static TextFormatFlags ConvertStringFormatIntoTextFormat(StringFormat sf, bool adjustStringAlign)
		{
			TextFormatFlags inputFlags = GridConstants.DefaultTextFormatFlags;
			if (sf != null)
			{
				inputFlags = (sf.FormatFlags & StringFormatFlags.DirectionRightToLeft) != StringFormatFlags.DirectionRightToLeft ? inputFlags & ~TextFormatFlags.RightToLeft : inputFlags | TextFormatFlags.RightToLeft;
				if (adjustStringAlign)
				{
					HorizontalAlignment ha = HorizontalAlignment.Left;
					if (sf.Alignment == StringAlignment.Center)
					{
						ha = HorizontalAlignment.Center;
					}
					else if (sf.Alignment == StringAlignment.Far)
					{
						ha = HorizontalAlignment.Right;
					}

					GridConstants.AdjustFormatFlagsForAlignment(ref inputFlags, ha);
				}
			}

			return inputFlags;
		}
	}
}
