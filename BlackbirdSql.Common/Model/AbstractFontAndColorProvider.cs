#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLEditor\Microsoft.VisualStudio.Data.Tools.SqlEditor.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Common.Model;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

public abstract class AbstractFontAndColorProvider : IVsFontAndColorDefaults, IVsFontAndColorEvents
{
	private readonly ArrayList fontColorDefaults;

	private string categoryName;

	private Font font;

	private Font fontDefault;

	private Guid guid;

	private static IVsFontAndColorUtilities _FontAndColorUtilities;

	protected ArrayList FontColorDefaults => fontColorDefaults;

	protected string CategoryName
	{
		get
		{
			return categoryName;
		}
		set
		{
			categoryName = value;
		}
	}

	protected Font Font => font;

	protected Font FontDefault
	{
		get
		{
			return fontDefault;
		}
		set
		{
			fontDefault = value;
		}
	}

	protected Guid Guid
	{
		get
		{
			return guid;
		}
		set
		{
			guid = value;
		}
	}

	protected IVsFontAndColorUtilities FontAndColorUtilities
	{
		get
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			return _FontAndColorUtilities ??=
				Microsoft.VisualStudio.Shell.Package.GetGlobalService(
					typeof(SVsFontAndColorStorage)) as IVsFontAndColorUtilities;
		}
	}

	protected virtual uint FontColorFlags => 0u;

	public event FontChangedEventHandler FontChangedEvent;

	public event ColorChangedEventHandler ColorChangedEvent;

	protected AbstractFontAndColorProvider()
	{
		fontColorDefaults = new ArrayList();
	}

	int IVsFontAndColorDefaults.GetFlags(out uint flags)
	{
		flags = FontColorFlags;
		return VSConstants.S_OK;
	}

	int IVsFontAndColorDefaults.GetPriority(out ushort priority)
	{
		priority = 256;
		return VSConstants.S_OK;
	}

	int IVsFontAndColorDefaults.GetCategoryName(out string name)
	{
		name = categoryName;
		return VSConstants.S_OK;
	}

	int IVsFontAndColorDefaults.GetBaseCategory(out Guid guid)
	{
		guid = Guid.Empty;
		return VSConstants.E_NOTIMPL;
	}

	int IVsFontAndColorDefaults.GetFont(FontInfo[] fontInfo)
	{
		fontInfo[0].bFaceNameValid = 1;
		fontInfo[0].bstrFaceName = fontDefault.Name;
		fontInfo[0].bPointSizeValid = 1;
		fontInfo[0].wPointSize = (ushort)fontDefault.SizeInPoints;
		fontInfo[0].bCharSetValid = 1;
		fontInfo[0].iCharSet = fontDefault.GdiCharSet;
		return VSConstants.S_OK;
	}

	int IVsFontAndColorDefaults.GetItemCount(out int itemCount)
	{
		itemCount = fontColorDefaults.Count;
		return VSConstants.S_OK;
	}

	int IVsFontAndColorDefaults.GetItem(int item, AllColorableItemInfo[] info)
	{
		info[0] = (AllColorableItemInfo)fontColorDefaults[item];
		return VSConstants.S_OK;
	}

	int IVsFontAndColorDefaults.GetItemByName(string itemName, AllColorableItemInfo[] info)
	{
		foreach (AllColorableItemInfo fontColorDefault in fontColorDefaults)
		{
			if (fontColorDefault.bstrName == itemName)
			{
				info[0] = fontColorDefault;
				return VSConstants.S_OK;
			}
		}

		return VSConstants.E_INVALIDARG;
	}

	int IVsFontAndColorEvents.OnApply()
	{
		OnApply();
		return VSConstants.S_OK;
	}

	public virtual void OnApply()
	{
	}

	int IVsFontAndColorEvents.OnFontChanged(ref Guid guid, FontInfo[] fontInfo, LOGFONTW[] logFont, IntPtr hFont)
	{
		if (guid == this.guid)
		{
			OnFontChanged(ref guid, fontInfo, logFont, hFont);
		}

		return VSConstants.S_OK;
	}

	public virtual void OnFontChanged(ref Guid guid, FontInfo[] fontInfo, LOGFONTW[] logFont, IntPtr hFont)
	{
		if (fontInfo[0].bFaceNameValid != 0 && fontInfo[0].bPointSizeValid != 0)
		{
			font = new Font(fontInfo[0].bstrFaceName, fontInfo[0].wPointSize);
			FontChangedEventHandler fontChanged = FontChangedEvent;
			if (font != null)
			{
				fontChanged?.Invoke(this, new FontChangedEventArgs(font));
			}
		}
	}

	int IVsFontAndColorEvents.OnItemChanged(ref Guid guid, string itemName, int itemID, ColorableItemInfo[] itemInfo, uint literalForeground, uint literalBackground)
	{
		if (guid == this.guid)
		{
			OnItemChanged(ref guid, itemName, itemID, itemInfo, literalForeground, literalBackground);
		}

		return VSConstants.S_OK;
	}

	public virtual void OnItemChanged(ref Guid guid, string itemName, int itemID, ColorableItemInfo[] itemInfo, uint literalForeground, uint literalBackground)
	{
		Color? bkColor = null;
		Color? fgColor = null;
		if (itemInfo[0].crForeground != Native.COLORREF_AUTO)
		{
			fgColor = ColorTranslator.FromWin32((int)itemInfo[0].crForeground);
		}

		if (itemInfo[0].crBackground != Native.COLORREF_AUTO)
		{
			bkColor = ColorTranslator.FromWin32((int)itemInfo[0].crBackground);
		}

		RaiseEvent(this, new ColorChangedEventArgs(itemName, bkColor, fgColor));
	}

	int IVsFontAndColorEvents.OnReset(ref Guid guid)
	{
		return VSConstants.S_OK;
	}

	int IVsFontAndColorEvents.OnResetToBaseCategory(ref Guid guid)
	{
		return VSConstants.S_OK;
	}

	public static bool GetFontAndColorSettingsForCategory(Guid categoryGuid, string itemName, IVsFontAndColorStorage vsFontAndColorStorage, out Font categoryFont, out Color? foreColor, out Color? bkColor, bool readFont)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		categoryFont = null;
		foreColor = null;
		bkColor = null;
		try
		{
			bool flag = vsFontAndColorStorage.OpenCategory(ref categoryGuid, (uint)__FCSTORAGEFLAGS.FCSF_READONLY) != 0;
			LOGFONTW[] pLOGFONT = new LOGFONTW[1];
			FontInfo[] array = new FontInfo[1];
			ColorableItemInfo[] array2 = new ColorableItemInfo[1];
			try
			{
				if (!flag)
				{
					if (readFont)
					{
						Native.ThrowOnFailure(vsFontAndColorStorage.GetFont(pLOGFONT, array));
					}

					Native.ThrowOnFailure(vsFontAndColorStorage.GetItem(itemName, array2));
				}
			}
			catch (Exception)
			{
				flag = true;
			}

			if (flag)
			{
				Native.ThrowOnFailure(vsFontAndColorStorage.CloseCategory());
				Native.ThrowOnFailure(vsFontAndColorStorage.OpenCategory(ref categoryGuid, (uint)(__FCSTORAGEFLAGS.FCSF_READONLY | __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS)));
				if (readFont)
				{
					Native.ThrowOnFailure(vsFontAndColorStorage.GetFont(pLOGFONT, array));
				}

				Native.ThrowOnFailure(vsFontAndColorStorage.GetItem(itemName, array2));
			}

			if (readFont && array[0].bFaceNameValid != 0 && array[0].bPointSizeValid != 0)
			{
				categoryFont = new Font(array[0].bstrFaceName, array[0].wPointSize);
			}

			if (array2[0].bBackgroundValid != 0 && array2[0].crBackground != Native.COLORREF_AUTO)
			{
				bkColor = Native.ColorFromRGB(array2[0].crBackground);
			}

			if (array2[0].bForegroundValid != 0 && array2[0].crForeground != Native.COLORREF_AUTO)
			{
				foreColor = Native.ColorFromRGB(array2[0].crForeground);
			}
		}
		catch
		{
		}
		finally
		{
			Native.ThrowOnFailure(vsFontAndColorStorage.CloseCategory());
		}

		return true;
	}

	protected void RaiseEvent(object sender, ColorChangedEventArgs args)
	{
		ColorChangedEvent?.Invoke(this, args);
	}

	protected uint DecodeSystemColor(uint systemColorReference)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		Native.ThrowOnFailure(FontAndColorUtilities.GetEncodedSysColor(systemColorReference, out int piSysColor));

		return Native.GetSysColor(piSysColor);
	}
}
