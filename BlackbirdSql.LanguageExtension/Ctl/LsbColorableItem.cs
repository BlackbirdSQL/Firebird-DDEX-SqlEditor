// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.ColorableItem

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.LanguageExtension.Ctl;


// =========================================================================================================
//
//										LsbColorableItem Class
//
/// <summary>
/// Language service IVsColorableItem implementation.
/// </summary>
// =========================================================================================================
public class LsbColorableItem : IVsColorableItem, IVsMergeableUIItem
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbColorableItem
	// ---------------------------------------------------------------------------------


	public LsbColorableItem(string displayName, string canonicalName, COLORINDEX foreground, COLORINDEX background, bool bold, bool strikethrough)
	{
		_DisplayName = displayName;
		_CanonicalName = canonicalName;
		_BackgroundColor = background;
		_ForegroundColor = foreground;

		if (bold)
			_FontFlags |= FONTFLAGS.FF_BOLD;
		if (strikethrough)
			_FontFlags |= FONTFLAGS.FF_STRIKETHROUGH;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbColorableItem
	// =========================================================================================================


	private readonly COLORINDEX _BackgroundColor;
	private readonly string _CanonicalName;
	private readonly string _DisplayName;
	private readonly FONTFLAGS _FontFlags;
	private readonly COLORINDEX _ForegroundColor;


	#endregion Fields





	// =========================================================================================================
	#region Methods - LsbColorableItem
	// =========================================================================================================


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
		piForeground[0] = _ForegroundColor;
		if (piBackground == null)
		{
			throw new ArgumentNullException("piBackground");
		}
		if (piBackground.Length == 0)
		{
			throw new ArgumentOutOfRangeException("piBackground");
		}
		piBackground[0] = _BackgroundColor;

		return VSConstants.S_OK;
	}

	public int GetDefaultFontFlags(out uint pdwFontFlags)
	{
		pdwFontFlags = (uint)_FontFlags;

		return VSConstants.S_OK;
	}

	public int GetDisplayName(out string pbstrName)
	{
		pbstrName = _DisplayName;

		return VSConstants.S_OK;
	}

	public int GetCanonicalName(out string pbstrNonLocalizeName)
	{
		pbstrNonLocalizeName = _CanonicalName;
		return VSConstants.S_OK;
	}

	public int GetDescription(out string pbstrDesc)
	{
		pbstrDesc = "";
		return VSConstants.S_OK;
	}

	public int GetMergingPriority(out int piMergingPriority)
	{
		piMergingPriority = 4096;
		return VSConstants.S_OK;
	}


	#endregion Methods

}
