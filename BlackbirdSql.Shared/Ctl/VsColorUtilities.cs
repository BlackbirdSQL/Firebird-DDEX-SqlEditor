// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.VsColorUtilities

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;



namespace BlackbirdSql.Shared.Ctl;


internal static class VsColorUtilities
{
	private static System.Drawing.Pen _PanelBorderPen;

	private static IVsUIShell2 _UiShell;

	private static IVsFontAndColorUtilities _FontAndColorUtilities;

	private static SolidColorBrush _SsoxFillSelectedBrush;

	private static SolidColorBrush _SsoxActiveCaptionTextBrush;


	internal static System.Drawing.Pen PanelBorderPen
	{
		get
		{
			if (System.Windows.Forms.Application.RenderWithVisualStyles)
			{
				return _PanelBorderPen ??= new System.Drawing.Pen(VisualStyleInformation.TextControlBorder);
			}
			return SystemPens.ControlDark;
		}
	}

	internal static System.Drawing.Color PanelBorderColor
	{
		get
		{
			if (System.Windows.Forms.Application.RenderWithVisualStyles)
			{
				return VisualStyleInformation.TextControlBorder;
			}
			return System.Drawing.SystemColors.ControlDark;
		}
	}

	private static IVsUIShell2 UiShell
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			return _UiShell ??= Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell2;

		}
	}

	private static IVsFontAndColorUtilities FontAndColorUtilities
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			return _FontAndColorUtilities ??= Package.GetGlobalService(typeof(SVsFontAndColorStorage)) as IVsFontAndColorUtilities;
		}
	}

	static VsColorUtilities()
	{
		SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
	}

	private static void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		_PanelBorderPen = null;
		_SsoxFillSelectedBrush = null;
		_SsoxActiveCaptionTextBrush = null;
	}

	internal static System.Drawing.Color GetWatermarkBackgroundColor()
	{
		return GetShellColor(__VSSYSCOLOREX.VSCOLOR_DESIGNER_TRAY);
	}

	internal static System.Drawing.Color GetWatermarkForegroundColor()
	{
		return GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOWTEXT);
	}

	internal static System.Drawing.Color GetGridBackgroundHighlightColor()
	{
		return GetShellColor(__VSSYSCOLOREX.VSCOLOR_HELP_SEARCH_FRAME_BACKGROUND);
	}

	internal static System.Drawing.Color GetGhostedTextColor()
	{
		return GetShellColor(__VSSYSCOLOREX.VSCOLOR_CONTROL_EDIT_HINTTEXT);
	}

	internal static void GetToolTipColors(out System.Drawing.Color background, out System.Drawing.Color border, out System.Drawing.Color text)
	{
		background = GetShellColor(__VSSYSCOLOREX.VSCOLOR_SCREENTIP_BACKGROUND);
		border = GetShellColor(__VSSYSCOLOREX.VSCOLOR_SCREENTIP_BORDER);
		text = GetShellColor(__VSSYSCOLOREX.VSCOLOR_SCREENTIP_TEXT);
	}

	internal static SolidColorBrush GetSqlServerObjectExplorerFillSelectedBrush()
	{
		if (_SsoxFillSelectedBrush == null)
		{
			if (SystemInformation.HighContrast)
			{
				_SsoxFillSelectedBrush = System.Windows.SystemColors.ActiveCaptionBrush;
			}
			else
			{
				_SsoxFillSelectedBrush = GetShellMediaBrush(-299);
			}
		}
		return _SsoxFillSelectedBrush;
	}

	internal static SolidColorBrush GetSqlServerObjectExplorerActiveCaptionTextBrush()
	{
		if (_SsoxActiveCaptionTextBrush == null)
		{
			if (SystemInformation.HighContrast)
			{
				_SsoxActiveCaptionTextBrush = System.Windows.SystemColors.ActiveCaptionTextBrush;
			}
			else
			{
				_SsoxActiveCaptionTextBrush = GetShellMediaBrush(-38);
			}
		}
		return _SsoxActiveCaptionTextBrush;
	}

	internal static System.Drawing.Color GetShellColor(COLORINDEX colorIndex)
	{
		System.Drawing.Color empty = System.Drawing.Color.Empty;
		IVsFontAndColorUtilities fontAndColorUtilities = FontAndColorUtilities;

		if (fontAndColorUtilities == null)
			return empty;

		Diag.ThrowIfNotOnUIThread();

		fontAndColorUtilities.GetRGBOfIndex(colorIndex, out uint pcrResult);

		return ColorTranslator.FromWin32((int)pcrResult);
	}

	internal static System.Drawing.Color GetShellColor(__VSSYSCOLOREX color)
	{
		return GetShellColor((int)color);
	}

	internal static System.Drawing.Color GetShellColor(__VSSYSCOLOREX2 color)
	{
		return GetShellColor((int)color);
	}
	internal static System.Drawing.Color GetShellColor(__VSSYSCOLOREX3 color)
	{
		return GetShellColor((int)color);
	}

	private static System.Drawing.Color GetShellColor(int color)
	{
		System.Drawing.Color empty = System.Drawing.Color.Empty;
		IVsUIShell2 uiShell = UiShell;

		if (uiShell == null)
			return empty;

		Diag.ThrowIfNotOnUIThread();

		___(uiShell.GetVSSysColorEx(color, out uint pdwRGBval));

		return ColorTranslator.FromWin32((int)pdwRGBval);
	}

	internal static void AssignStandardColors(LinkLabel linkLabel)
	{
		linkLabel.ActiveLinkColor = GetShellColor(__VSSYSCOLOREX.VSCOLOR_CONTROL_LINK_TEXT_HOVER);
		linkLabel.LinkColor = GetShellColor(__VSSYSCOLOREX.VSCOLOR_CONTROL_LINK_TEXT);
		linkLabel.VisitedLinkColor = GetShellColor(__VSSYSCOLOREX.VSCOLOR_CONTROL_LINK_TEXT_PRESSED);
	}

	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	private static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);

	private static System.Windows.Media.Color GetMediaColor(int colorIndex)
	{
		if (UiShell == null)
			throw new InvalidOperationException();

		Diag.ThrowIfNotOnUIThread();

		___(UiShell.GetVSSysColorEx(colorIndex, out uint pdwRGBval));
		System.Drawing.Color color = ColorTranslator.FromWin32((int)pdwRGBval);

		return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	private static SolidColorBrush GetShellMediaBrush(int colorIndex)
	{
		return new SolidColorBrush(GetMediaColor(colorIndex));
	}
}
