// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.ColorableItem
using System;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.LanguageExtension.Ctl;


public class LsbColorableItem : IVsColorableItem, IVsMergeableUIItem
{
	private readonly string _displayName;

	private readonly string _canonicalName;

	private readonly COLORINDEX _backgroundColor;

	private readonly COLORINDEX _foregroundColor;

	private readonly uint _fontFlags;

	public LsbColorableItem(string displayName, string canonicalName, COLORINDEX foreground, COLORINDEX background, bool bold, bool strikethrough)
	{
		_displayName = displayName;
		_canonicalName = canonicalName;
		_backgroundColor = background;
		_foregroundColor = foreground;
		if (bold)
		{
			_fontFlags |= 1u;
		}
		if (strikethrough)
		{
			_fontFlags |= 2u;
		}
	}

	public int GetDefaultColors(COLORINDEX[] piForeground, COLORINDEX[] piBackground)
	{
		if (piForeground == null)
		{
			throw new ArgumentNullException("piForeground");
		}
		if (piForeground.Length == 0)
		{
			throw new ArgumentOutOfRangeException("piForeground");
		}
		piForeground[0] = _foregroundColor;
		if (piBackground == null)
		{
			throw new ArgumentNullException("piBackground");
		}
		if (piBackground.Length == 0)
		{
			throw new ArgumentOutOfRangeException("piBackground");
		}
		piBackground[0] = _backgroundColor;
		return 0;
	}

	public int GetDefaultFontFlags(out uint pdwFontFlags)
	{
		pdwFontFlags = _fontFlags;
		return 0;
	}

	public int GetDisplayName(out string pbstrName)
	{
		pbstrName = _displayName;
		return 0;
	}

	public int GetCanonicalName(out string pbstrNonLocalizeName)
	{
		pbstrNonLocalizeName = _canonicalName;
		return 0;
	}

	public int GetDescription(out string pbstrDesc)
	{
		pbstrDesc = string.Empty;
		return 0;
	}

	public int GetMergingPriority(out int piMergingPriority)
	{
		piMergingPriority = 4096;
		return 0;
	}
}
