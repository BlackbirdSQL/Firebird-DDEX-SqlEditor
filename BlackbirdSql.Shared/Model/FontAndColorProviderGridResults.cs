﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLEditor\Microsoft.VisualStudio.Data.Tools.SqlEditor.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Shared.Model;

public sealed class FontAndColorProviderGridResults : AbstractFontAndColorProvider
{
	private static FontAndColorProviderGridResults instance = null;

	public static readonly string GridCell = "Grid Cell";

	public static readonly string SelectedCell = "Selected Cell";

	public static readonly string SelectedCellInactive = "Inactive Selected Cell";

	public static readonly string NullValueCell = "Null Value Cell";

	public static readonly string HeaderRow = "Header Row";


	public static readonly COLORINDEX VsSysColorIndexWindowBkColor = COLORINDEX.CI_SYSPLAINTEXT_BK;

	public static readonly COLORINDEX VsSysColorIndexWindowTextColor = COLORINDEX.CI_SYSPLAINTEXT_FG;

	public static readonly __VSSYSCOLOREX3 VsSysColorIndexSelected = __VSSYSCOLOREX3.VSCOLOR_HIGHLIGHT;

	public static readonly __VSSYSCOLOREX3 VsSysColorIndexSelectedInactive = __VSSYSCOLOREX3.VSCOLOR_INACTIVECAPTION;

	public static readonly __VSSYSCOLOREX3 VsSysColorIndexNullCell = __VSSYSCOLOREX3.VSCOLOR_INFOBACKGROUND;

	public static readonly __VSSYSCOLOREX3 VsSysColorIndexHeaderRow = __VSSYSCOLOREX3.VSCOLOR_INACTIVECAPTION;

	public static AbstractFontAndColorProvider Instance
	{
		get
		{
			instance ??= new FontAndColorProviderGridResults();

			return instance;
		}
	}

	protected override uint FontColorFlags => (uint)__FONTCOLORFLAGS.FCF_ONLYTTFONTS;

	private FontAndColorProviderGridResults()
	{
		Diag.ThrowIfNotOnUIThread();

		CategoryName = ControlsResources.FontAndColorCategoryResultsGrid;
		Guid = VS.CLSID_FontAndColorsSqlResultsGridCategory;
		FontDefault = new Font(Control.DefaultFont.Name, Control.DefaultFont.SizeInPoints);
		FontAndColorUtilities.EncodeAutomaticColor(out uint pcrResult);
		FontAndColorUtilities.EncodeIndexedColor(VsSysColorIndexWindowTextColor, out uint pcrResult2);
		FontAndColorUtilities.EncodeIndexedColor(VsSysColorIndexWindowBkColor, out uint pcrResult3);
		FontAndColorUtilities.EncodeVSColor((int)VsSysColorIndexSelected, out uint pcrResult4);
		FontAndColorUtilities.EncodeVSColor((int)VsSysColorIndexSelectedInactive, out uint pcrResult5);
		FontAndColorUtilities.EncodeVSColor((int)VsSysColorIndexNullCell, out uint pcrResult6);
		FontAndColorUtilities.EncodeVSColor((int)VsSysColorIndexHeaderRow, out uint pcrResult7);
		AllColorableItemInfo allColorableItemInfo = new AllColorableItemInfo
		{
			bNameValid = 1,
			bstrName = GridCell,
			bLocalizedNameValid = 1,
			bstrLocalizedName = ControlsResources.FontAndColorItemNameGridCell,
			bAutoForegroundValid = 1,
			crAutoForeground = pcrResult2,
			bAutoBackgroundValid = 1,
			crAutoBackground = pcrResult3,
			Info =
			{
				bForegroundValid = 1,
				crForeground = pcrResult,
				bBackgroundValid = 1,
				crBackground = pcrResult,
				bFontFlagsValid = 1,
				dwFontFlags = 0u
			},
			bMarkerVisualStyleValid = 1,
			dwMarkerVisualStyle = 0u,
			bLineStyleValid = 1,
			eLineStyle = LINESTYLE.LI_SOLID,
			bFlagsValid = 1,
			fFlags = (uint)(__FCITEMFLAGS.FCIF_PLAINTEXT | __FCITEMFLAGS.FCIF_ALLOWCUSTOMCOLORS
				| __FCITEMFLAGS.FCIF_ALLOWBGCHANGE | __FCITEMFLAGS.FCIF_ALLOWFGCHANGE),
			bstrDescription = ControlsResources.FontAndColorGridCellDescription
		};
		FontColorDefaults.Add(allColorableItemInfo);
		allColorableItemInfo = CreateItemInfo(SelectedCell, ControlsResources.FontAndColorItemNameGridSelectedCell, pcrResult4, pcrResult, ControlsResources.FontAndColorGridSelectedCellDescription);
		FontColorDefaults.Add(allColorableItemInfo);
		allColorableItemInfo = CreateItemInfo(SelectedCellInactive, ControlsResources.FontAndColorItemNameGridInactiveSelectedCell, pcrResult5, pcrResult, ControlsResources.FontAndColorGridInactiveSelectedCellDescription);
		FontColorDefaults.Add(allColorableItemInfo);
		allColorableItemInfo = CreateItemInfo(NullValueCell, ControlsResources.FontAndColorItemGridNullValueCellName, pcrResult6, pcrResult, ControlsResources.FontAndGridItemGridNullValueCellDescription);
		FontColorDefaults.Add(allColorableItemInfo);
		allColorableItemInfo = CreateItemInfo(HeaderRow, ControlsResources.FontAndColorItemGridHeaderRowCellName, pcrResult7, pcrResult, ControlsResources.FontAndColorItemGridHeaderRowCellDescription);
		FontColorDefaults.Add(allColorableItemInfo);
	}

	private AllColorableItemInfo CreateItemInfo(string itemName, string locName, uint bkColor, uint autoColor, string description)
	{
		AllColorableItemInfo result = default;
		result.bNameValid = 1;
		result.bstrName = itemName;
		result.bLocalizedNameValid = 1;
		result.bstrLocalizedName = locName;
		result.bAutoBackgroundValid = 1;
		result.crAutoBackground = bkColor;
		result.Info.bBackgroundValid = 1;
		result.Info.bForegroundValid = 1;
		result.Info.crBackground = autoColor;
		result.Info.crForeground = autoColor;
		result.Info.bFontFlagsValid = 1;
		result.Info.dwFontFlags = 0u;
		result.bFlagsValid = 1;
		result.fFlags = 20u;
		result.bstrDescription = description;
		return result;
	}
}
