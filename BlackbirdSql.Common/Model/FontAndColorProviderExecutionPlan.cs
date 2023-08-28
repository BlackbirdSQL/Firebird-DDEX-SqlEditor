#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLEditor\Microsoft.VisualStudio.Data.Tools.SqlEditor.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;

using BlackbirdSql.Core;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Properties;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Drawing.Imaging;

namespace BlackbirdSql.Common.Model;


public sealed class FontAndColorProviderExecutionPlan : AbstractFontAndColorProvider
{
	private static FontAndColorProviderExecutionPlan instance = null;

	public static readonly string Text = "Text";



	public static AbstractFontAndColorProvider Instance
	{
		get
		{
			instance ??= new FontAndColorProviderExecutionPlan();

			return instance;
		}
	}

	protected override uint FontColorFlags => (uint)(__FONTCOLORFLAGS.FCF_MUSTRESTART | __FONTCOLORFLAGS.FCF_ONLYTTFONTS);

	private FontAndColorProviderExecutionPlan()
	{
		CategoryName = SharedResx.FontAndColorCategorySqlResultsExecutionPlan;
		Guid = VS.CLSID_FontAndColorsSqlResultsExecutionPlanCategory;
		FontDefault = new Font("Courier New", 10f);
		AllColorableItemInfo allColorableItemInfo = new AllColorableItemInfo
		{
			bNameValid = 1,
			bstrName = "Text",
			bLocalizedNameValid = 1,
			bstrLocalizedName = Text,
			bAutoForegroundValid = 1,
			crAutoForeground = 0u,
			bAutoBackgroundValid = 1,
			crAutoBackground = Native.COLORREF_WHITE,
			Info =
			{
				bForegroundValid = 1,
				crForeground = Native.COLORREF_AUTO,
				bBackgroundValid = 1,
				crBackground = Native.COLORREF_AUTO,
				bFontFlagsValid = 1,
				dwFontFlags = 0u
			},
			bMarkerVisualStyleValid = 1,
			dwMarkerVisualStyle = 0u,
			bLineStyleValid = 1,
			eLineStyle = LINESTYLE.LI_SOLID,
			bFlagsValid = 1,
			fFlags = (uint)__FCITEMFLAGS.FCIF_PLAINTEXT,
			bstrDescription = SharedResx.FontAndColorShowplanDescription
		};
		FontColorDefaults.Add(allColorableItemInfo);
	}
}
