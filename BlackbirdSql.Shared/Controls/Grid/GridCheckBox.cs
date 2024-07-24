// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridCheckBox

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Properties;
using Microsoft.Win32;



namespace BlackbirdSql.Shared.Controls.Grid;


public sealed class GridCheckBox
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
			try
			{
				m_textFont = value ?? throw new ArgumentNullException();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
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

			if (value is not SolidBrush solidBrush)
			{
				ArgumentException ex = new(ControlsResources.ExOnlySolidBrush, "value");
				Diag.Dug(ex);
				throw ex;
			}

			m_textBrush = solidBrush;
		}
	}

	static GridCheckBox()
	{
		ExtraHorizSpace = SystemInformation.Border3DSize.Width;
		s_imageTextGap = 4;
		SystemEvents.UserPreferenceChanged += OnUserPrefChanged;
	}

	private static void OnUserPrefChanged(object sender, UserPreferenceChangedEventArgs pref)
	{
		SDisabledButtonTextBrush = null;
	}

	public GridCheckBox()
	{
		m_cacheGdiPlusFormat.LineAlignment = StringAlignment.Center;
		m_cacheGdiPlusFormat.HotkeyPrefix = HotkeyPrefix.None;
		m_cacheGdiPlusFormat.Trimming = StringTrimming.EllipsisCharacter;
		m_cacheFormat &= ~TextFormatFlags.SingleLine;
	}

	public static Rectangle CalculateInitialContentsRect(Graphics g, Rectangle r, string text, Size size, HorizontalAlignment contentsAlignment, Font textFont, bool bRtl, ref StringFormat sFormat, out int nStringWidth)
	{
		TextFormatFlags sFormat2 = ConvertStringFormatIntoTextFormat(sFormat, adjustStringAlign: false);
		return CalculateInitialContentsRect(g, r, text, size, contentsAlignment, textFont, bRtl, ref sFormat2, ref sFormat, out nStringWidth);
	}

	public static Rectangle CalculateInitialContentsRect(Graphics g, Rectangle r, string text, Size size, HorizontalAlignment contentsAlignment, Font textFont, bool bRtl, ref TextFormatFlags sFormat, out int nStringWidth)
	{
		StringFormat gdiPlusFormat = null;
		return CalculateInitialContentsRect(g, r, text, size, contentsAlignment, textFont, bRtl, ref sFormat, ref gdiPlusFormat, out nStringWidth);
	}

	public static void DrawCheckbox(Graphics g, Rectangle rect, HorizontalAlignment alignment, bool isRTL, EnGridCheckBoxState state, bool bEnabled)
	{
		Point glyphLocation = AlignCheckBox(rect, g, alignment, isRTL);
		if (bEnabled)
		{
			switch (state)
			{
				case EnGridCheckBoxState.Checked:
					CheckBoxRenderer.DrawCheckBox(g, glyphLocation, CheckBoxState.CheckedNormal);
					break;
				case EnGridCheckBoxState.Unchecked:
					CheckBoxRenderer.DrawCheckBox(g, glyphLocation, CheckBoxState.UncheckedNormal);
					break;
				case EnGridCheckBoxState.Disabled:
					CheckBoxRenderer.DrawCheckBox(g, glyphLocation, CheckBoxState.UncheckedDisabled);
					break;
				case EnGridCheckBoxState.Indeterminate:
					CheckBoxRenderer.DrawCheckBox(g, glyphLocation, CheckBoxState.MixedNormal);
					break;
				case EnGridCheckBoxState.None:
					break;
			}
		}
		else
		{
			switch (state)
			{
				case EnGridCheckBoxState.Checked:
					CheckBoxRenderer.DrawCheckBox(g, glyphLocation, CheckBoxState.CheckedDisabled);
					break;
				case EnGridCheckBoxState.Unchecked:
					CheckBoxRenderer.DrawCheckBox(g, glyphLocation, CheckBoxState.UncheckedDisabled);
					break;
				case EnGridCheckBoxState.Disabled:
					CheckBoxRenderer.DrawCheckBox(g, glyphLocation, CheckBoxState.UncheckedDisabled);
					break;
				case EnGridCheckBoxState.Indeterminate:
					CheckBoxRenderer.DrawCheckBox(g, glyphLocation, CheckBoxState.MixedDisabled);
					break;
				case EnGridCheckBoxState.None:
					break;
			}
		}
	}

	public void Paint(Graphics g, Rectangle r, ButtonState state, string text, EnGridCheckBoxState checkState, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, bool useGdiPlus, bool isHeader)
	{
		if (useGdiPlus)
		{
			Paint(g, r, state, text, checkState, contentsAlignment, tbLayout, bEnabled, m_textFont, m_textBrush, ConvertStringFormatIntoTextFormat(m_cacheGdiPlusFormat, adjustStringAlign: true), m_RTL, m_cacheGdiPlusFormat, isHeader);
		}
		else
		{
			Paint(g, r, state, text, checkState, contentsAlignment, tbLayout, bEnabled, m_textFont, m_textBrush, m_cacheFormat, m_RTL, null, isHeader);
		}
	}

	private static void PaintButtonFrame(Graphics g, Rectangle r, ButtonState state, bool isHeader)
	{
		if (Application.RenderWithVisualStyles)
		{
			Rectangle bounds = r;
			Rectangle rect = r;
			VisualStyleElement visualStyleElement;
			if (isHeader)
			{
				visualStyleElement = DrawManager.GetHeader(state);
				rect.Width++;
				bounds.Width++;
			}
			else
			{
				visualStyleElement = DrawManager.GetButton(state);
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

	private static Rectangle CalculateInitialContentsRect(Graphics g, Rectangle r, string text, Size size, HorizontalAlignment contentsAlignment, Font textFont, bool bRtl, ref TextFormatFlags sFormat, ref StringFormat gdiPlusFormat, out int nStringWidth)
	{
		nStringWidth = 0;
		Size size2 = new Size(0, 0);
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

			size2.Width = (int)Math.Ceiling(sizeF.Width);
			size2.Height = (int)Math.Ceiling(sizeF.Height);
			nStringWidth = size2.Width;
		}

		size2.Width += size.Width;
		size2.Height = Math.Max(size2.Height, size.Height);
		if (text != null && text != "")
		{
			size2.Width += s_imageTextGap;
		}

		int num = (r.Height - size2.Height) / 2;
		Rectangle result;
		if (contentsAlignment == HorizontalAlignment.Left && !bRtl || contentsAlignment == HorizontalAlignment.Right && bRtl)
		{
			result = new Rectangle(r.X + ExtraHorizSpace, r.Y + num, size2.Width, size2.Height);
			GridConstants.AdjustFormatFlagsForAlignment(ref sFormat, HorizontalAlignment.Left);
			if (gdiPlusFormat != null)
			{
				gdiPlusFormat.Alignment = StringAlignment.Near;
			}
		}
		else if (contentsAlignment == HorizontalAlignment.Right && !bRtl || contentsAlignment == HorizontalAlignment.Left && bRtl)
		{
			result = new Rectangle(r.X + r.Width - ExtraHorizSpace - size2.Width, r.Y + num, size2.Width, size2.Height);
			GridConstants.AdjustFormatFlagsForAlignment(ref sFormat, HorizontalAlignment.Right);
			if (gdiPlusFormat != null)
			{
				gdiPlusFormat.Alignment = StringAlignment.Far;
			}
		}
		else
		{
			int num2 = (int)Math.Ceiling(Math.Max((r.Width - (float)size2.Width) / 2f, 0f));
			result = new Rectangle(r.X + num2, r.Y + num, size2.Width, size2.Height);
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

	private static EnGridButtonArea PaintButtonOrHitTest(Graphics g, Rectangle r, ButtonState state, string text, EnGridCheckBoxState checkState, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl, bool bEnabled, Point ptToHitTest, bool bPaint, StringFormat gdiPlusFormat)
	{
		Size glyphSize = CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.UncheckedNormal);
		Rectangle rect = CalculateInitialContentsRect(g, r, text, glyphSize, contentsAlignment,
			textFont, bRtl, ref sFormat, ref gdiPlusFormat, out int nStringWidth);

		if (rect.Width <= 0)
			return EnGridButtonArea.Background;

		if (text != null && text != "")
		{
			int num = Math.Max((rect.Height - glyphSize.Height) / 2, 0);
			if (tbLayout == EnTextBitmapLayout.TextRightOfBitmap)
			{
				Rectangle rect2 = new Rectangle(rect.X, rect.Y + num, Math.Min(glyphSize.Width, rect.Width), glyphSize.Height);
				rect2.Height = Math.Min(rect2.Height, rect.Height);
				if (bPaint)
				{
					DrawCheckbox(g, rect2, contentsAlignment, bRtl, checkState, bEnabled);
				}
				else if (rect2.Contains(ptToHitTest))
				{
					return EnGridButtonArea.Image;
				}

				Rectangle rectangle = new Rectangle(rect2.Right + s_imageTextGap, rect.Y, rect.Width - rect2.Width - s_imageTextGap, rect.Height);
				if (rectangle.X < rect.Right)
				{
					rectangle.Width = Math.Min(rectangle.Width, rect.Right - rectangle.X);
					if (bPaint)
					{
						if (gdiPlusFormat != null)
						{
							g.DrawString(text, textFont, textBrush, rectangle, gdiPlusFormat);
						}
						else
						{
							TextRenderer.DrawText(g, text, textFont, rectangle, textBrush.Color, sFormat);
						}
					}
					else if (rectangle.Contains(ptToHitTest))
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
				Rectangle rectangle = new Rectangle(rect.X, rect.Y, Math.Min(nStringWidth, rect.Width), rect.Height);
				if (bPaint)
				{
					if (gdiPlusFormat != null)
					{
						g.DrawString(text, textFont, textBrush, rectangle, gdiPlusFormat);
					}
					else
					{
						TextRenderer.DrawText(g, text, textFont, rectangle, textBrush.Color, sFormat);
					}
				}
				else if (rectangle.Contains(ptToHitTest))
				{
					return EnGridButtonArea.Text;
				}

				Rectangle rect2 = new Rectangle(rectangle.Right + s_imageTextGap, rect.Y + num, rect.Width - rectangle.Width - s_imageTextGap, glyphSize.Height);
				if (rect2.X < rect.Right)
				{
					rect2.Width = Math.Min(rect2.Width, rect.Right - rect2.X);
					rect2.Height = Math.Min(rect2.Height, rect.Height);
					if (bPaint)
					{
						DrawCheckbox(g, rect2, contentsAlignment, bRtl, checkState, bEnabled);
					}
					else if (rect2.Contains(ptToHitTest))
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
				if (rect.Contains(ptToHitTest))
				{
					return EnGridButtonArea.Image;
				}

				return EnGridButtonArea.Background;
			}

			DrawCheckbox(g, rect, contentsAlignment, bRtl, checkState, bEnabled);
		}

		return EnGridButtonArea.Nothing;
	}

	private static void Paint(Graphics g, Rectangle r, ButtonState state, string text, EnGridCheckBoxState checkState, HorizontalAlignment contentsAlignment, EnTextBitmapLayout tbLayout, bool bEnabled, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl, StringFormat gdiPlusFormat, bool isHeader)
	{
		if (!r.IsEmpty)
		{
			SolidBrush textBrush2 = bEnabled ? textBrush : SDisabledButtonTextBrush;
			PaintButtonFrame(g, r, state, isHeader);
			PaintButtonOrHitTest(g, r, state, text, checkState, contentsAlignment, tbLayout, textFont, textBrush2, sFormat, bRtl, bEnabled, Point.Empty, bPaint: true, gdiPlusFormat);
		}
	}

	private static Point AlignCheckBox(Rectangle rect, Graphics g, HorizontalAlignment alignment, bool isRTL)
	{
		Size glyphSize = CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.UncheckedNormal);
		Point location = rect.Location;
		if (glyphSize.Width < rect.Width)
		{
			if (alignment == HorizontalAlignment.Center)
			{
				location.X = rect.X + (rect.Width - glyphSize.Width) / 2;
			}
			else if (alignment == HorizontalAlignment.Left && !isRTL || alignment == HorizontalAlignment.Right && isRTL)
			{
				location.X = rect.X;
			}
			else
			{
				location.X = rect.Right - glyphSize.Width;
			}
		}

		if (glyphSize.Height < rect.Height)
		{
			location.Y = rect.Y + (rect.Height - glyphSize.Height) / 2 + 1;
		}

		return location;
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
