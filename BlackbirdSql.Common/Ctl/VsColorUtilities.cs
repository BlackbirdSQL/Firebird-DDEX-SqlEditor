// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.VsColorUtilities
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using BlackbirdSql.Common;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;




public static class VsColorUtilities
{
	private static System.Drawing.Pen _panelBorderPen;

	private static IVsUIShell2 _uiShell;

	private static IVsFontAndColorUtilities _vsFontAndColorUtilities;

	private static SolidColorBrush _ssoxFillSelectedBrush;

	private static SolidColorBrush _ssoxActiveCaptionTextBrush;

	public static System.Drawing.Pen PanelBorderPen
	{
		get
		{
			if (System.Windows.Forms.Application.RenderWithVisualStyles)
			{
				return _panelBorderPen ??= new System.Drawing.Pen(VisualStyleInformation.TextControlBorder);
			}
			return SystemPens.ControlDark;
		}
	}

	public static System.Drawing.Color PanelBorderColor
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
			_uiShell ??= Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell2;
			return _uiShell;
		}
	}

	private static IVsFontAndColorUtilities FontAndColorUtilities
	{
		get
		{
			_vsFontAndColorUtilities ??= Package.GetGlobalService(typeof(SVsFontAndColorStorage)) as IVsFontAndColorUtilities;
			return _vsFontAndColorUtilities;
		}
	}

	static VsColorUtilities()
	{
		SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
	}

	private static void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		_panelBorderPen = null;
		_ssoxFillSelectedBrush = null;
		_ssoxActiveCaptionTextBrush = null;
	}

	public static System.Drawing.Color GetWatermarkBackgroundColor()
	{
		return GetShellColor(-47);
	}

	public static System.Drawing.Color GetWatermarkForegroundColor()
	{
		return GetShellColor(-219);
	}

	public static System.Drawing.Color GetGridBackgroundHighlightColor()
	{
		return GetShellColor(-77);
	}

	public static System.Drawing.Color GetGhostedTextColor()
	{
		return GetShellColor(-26);
	}

	public static void GetToolTipColors(out System.Drawing.Color background, out System.Drawing.Color border, out System.Drawing.Color text)
	{
		background = GetShellColor(-127);
		border = GetShellColor(-126);
		text = GetShellColor(-128);
	}

	public static SolidColorBrush GetSqlServerObjectExplorerFillSelectedBrush()
	{
		if (_ssoxFillSelectedBrush == null)
		{
			if (SystemInformation.HighContrast)
			{
				_ssoxFillSelectedBrush = System.Windows.SystemColors.ActiveCaptionBrush;
			}
			else
			{
				_ssoxFillSelectedBrush = GetShellMediaBrush(-299);
			}
		}
		return _ssoxFillSelectedBrush;
	}

	public static SolidColorBrush GetSqlServerObjectExplorerActiveCaptionTextBrush()
	{
		if (_ssoxActiveCaptionTextBrush == null)
		{
			if (SystemInformation.HighContrast)
			{
				_ssoxActiveCaptionTextBrush = System.Windows.SystemColors.ActiveCaptionTextBrush;
			}
			else
			{
				_ssoxActiveCaptionTextBrush = GetShellMediaBrush(-38);
			}
		}
		return _ssoxActiveCaptionTextBrush;
	}

	public static System.Drawing.Color GetShellColor(COLORINDEX colorIndex)
	{
		System.Drawing.Color empty = System.Drawing.Color.Empty;
		IVsFontAndColorUtilities fontAndColorUtilities = FontAndColorUtilities;
		if (fontAndColorUtilities == null)
		{
			return empty;
		}
		fontAndColorUtilities.GetRGBOfIndex(colorIndex, out var pcrResult);
		return ColorTranslator.FromWin32((int)pcrResult);
	}

	public static System.Drawing.Color GetShellColor(int color)
	{
		System.Drawing.Color empty = System.Drawing.Color.Empty;
		IVsUIShell2 uiShell = UiShell;
		if (uiShell == null)
		{
			return empty;
		}
		Native.ThrowOnFailure(uiShell.GetVSSysColorEx(color, out var pdwRGBval));
		return ColorTranslator.FromWin32((int)pdwRGBval);
	}

	public static void AssignStandardColors(LinkLabel linkLabel)
	{
		linkLabel.ActiveLinkColor = GetShellColor(-30);
		linkLabel.LinkColor = GetShellColor(-29);
		linkLabel.VisitedLinkColor = GetShellColor(-31);
	}

	private static System.Windows.Media.Color GetMediaColor(int colorIndex)
	{
		if (UiShell == null)
		{
			throw new InvalidOperationException();
		}
		ErrorHandler.ThrowOnFailure(UiShell.GetVSSysColorEx(colorIndex, out var pdwRGBval));
		System.Drawing.Color color = ColorTranslator.FromWin32((int)pdwRGBval);
		return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	private static SolidColorBrush GetShellMediaBrush(int colorIndex)
	{
		return new SolidColorBrush(GetMediaColor(colorIndex));
	}
}
