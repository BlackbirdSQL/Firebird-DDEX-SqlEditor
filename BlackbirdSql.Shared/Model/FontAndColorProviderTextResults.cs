#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLEditor\Microsoft.VisualStudio.Data.Tools.SqlEditor.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Shared.Model;

public sealed class FontAndColorProviderTextResults : AbstractFontAndColorProvider
{
	private static FontAndColorProviderTextResults instance = null;


	public static readonly string PlainText = "Plain Text";

	private IEditorFormatMap _editorFormatMap;

	private IClassificationFormatMap _classificationFormatMap;

	public IEditorFormatMap EditorFormatMap
	{
		get
		{
			if (_editorFormatMap == null)
			{
				if (Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel)) is IComponentModel componentModel)
				{
					IEditorFormatMapService service = componentModel.GetService<IEditorFormatMapService>();
					Guid categoryGuid = VS.CLSID_FontAndColorsSqlResultsTextCategory;
					string text = categoryGuid.ToString();
					categoryGuid = VS.CLSID_FontAndColorsSqlResultsTextCategory;
					_editorFormatMap = service.GetEditorFormatMap(text + ":" + categoryGuid.ToString());
				}
			}

			return _editorFormatMap;
		}
	}

	public IClassificationFormatMap ClassificationFormatMap
	{
		get
		{
			if (_classificationFormatMap == null)
			{
				if (Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel)) is IComponentModel componentModel)
				{
					IClassificationFormatMapService service = componentModel.GetService<IClassificationFormatMapService>();
					Guid categoryGuid = VS.CLSID_FontAndColorsSqlResultsTextCategory;
					string text = categoryGuid.ToString();
					categoryGuid = VS.CLSID_FontAndColorsSqlResultsTextCategory;
					_classificationFormatMap = service.GetClassificationFormatMap(text + ":" + categoryGuid.ToString());
				}
			}

			return _classificationFormatMap;
		}
	}

	public static AbstractFontAndColorProvider Instance
	{
		get
		{
			instance ??= new FontAndColorProviderTextResults();

			return instance;
		}
	}

	private FontAndColorProviderTextResults()
	{
		Diag.ThrowIfNotOnUIThread();

		CategoryName = ControlsResources.FontAndColorCategoryResultsText;
		Guid = VS.CLSID_FontAndColorsSqlResultsTextCategory;
		FontDefault = new Font("Courier New", Control.DefaultFont.SizeInPoints);
		FontAndColorUtilities.EncodeAutomaticColor(out uint pcrResult);
		FontAndColorUtilities.EncodeIndexedColor(COLORINDEX.CI_SYSPLAINTEXT_FG, out uint pcrResult2);
		FontAndColorUtilities.EncodeIndexedColor(COLORINDEX.CI_SYSPLAINTEXT_BK, out uint pcrResult3);
		AllColorableItemInfo allColorableItemInfo = new AllColorableItemInfo
		{
			bNameValid = 1,
			bstrName = PlainText,
			bLocalizedNameValid = 1,
			bstrLocalizedName = ControlsResources.FontAndColorItemNameResultsPlainText,
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
			bstrDescription = ControlsResources.FontAndColorTextResultsDescription
		};
		FontColorDefaults.Add(allColorableItemInfo);
	}

	public override void OnFontChanged(ref Guid guid, FontInfo[] pInfo, LOGFONTW[] logFont, IntPtr hFont)
	{
		if (ClassificationFormatMap != null)
		{
			if (!ClassificationFormatMap.IsInBatchUpdate)
			{
				ClassificationFormatMap.BeginBatchUpdate();
			}

			TextFormattingRunProperties textFormattingRunProperties = ClassificationFormatMap.DefaultTextProperties;
			if (pInfo[0].bFaceNameValid == 1)
			{
				Typeface typefaceFromFont = GetTypefaceFromFont(pInfo[0].bstrFaceName);
				textFormattingRunProperties = textFormattingRunProperties.SetTypeface(typefaceFromFont);
			}

			if (pInfo[0].bPointSizeValid == 1)
			{
				double fontRenderingEmSize = pInfo[0].wPointSize * 96.0 / 72.0;
				textFormattingRunProperties = textFormattingRunProperties.SetFontRenderingEmSize(fontRenderingEmSize);
			}

			ClassificationFormatMap.DefaultTextProperties = textFormattingRunProperties;
		}
	}

	public static Typeface GetTypefaceFromFont(string typefaceName)
	{
		System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(typefaceName);
		System.Windows.FontStyle normal = FontStyles.Normal;
		FontStretch normal2 = FontStretches.Normal;
		FontWeight normal3 = FontWeights.Normal;
		return new Typeface(fontFamily, normal, normal3, normal2, GetFallbackFontFamily());
	}

	public static System.Windows.Media.FontFamily GetFallbackFontFamily()
	{
		return new System.Windows.Media.FontFamily("Global Monospace, Global User Interface");
	}

	public override void OnItemChanged(ref Guid guid, string itemName, int itemID, ColorableItemInfo[] itemInfo, uint literalForeground, uint literalBackground)
	{
		Diag.ThrowIfNotOnUIThread();

		if (!itemName.Equals(PlainText, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		___(FontAndColorUtilities.GetColorType(itemInfo[0].crBackground, out int pctType));
		___(FontAndColorUtilities.GetColorType(itemInfo[0].crForeground, out int pctType2));
		if ((long)pctType == 3)
		{
			itemInfo[0].crBackground = DecodeSystemColor(itemInfo[0].crBackground);
		}
		else if ((long)pctType == 5)
		{
			FontAndColorUtilities.GetRGBOfIndex(COLORINDEX.CI_SYSPLAINTEXT_BK, out itemInfo[0].crBackground);
		}

		if ((long)pctType2 == 3)
		{
			itemInfo[0].crForeground = DecodeSystemColor(itemInfo[0].crForeground);
		}
		else if ((long)pctType2 == 5)
		{
			FontAndColorUtilities.GetRGBOfIndex(COLORINDEX.CI_SYSPLAINTEXT_FG, out itemInfo[0].crForeground);
		}

		if (EditorFormatMap != null && itemInfo[0].bBackgroundValid == 1)
		{
			ResourceDictionary properties = EditorFormatMap.GetProperties("TextView Background");
			System.Drawing.Color color = ColorTranslator.FromWin32((int)itemInfo[0].crBackground);
			System.Windows.Media.Color color2 = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
			SolidColorBrush solidColorBrush = new SolidColorBrush(color2);
			solidColorBrush.Freeze();
			properties["BackgroundColor"] = color2;
			properties["Background"] = solidColorBrush;
			if (!EditorFormatMap.IsInBatchUpdate)
			{
				EditorFormatMap.BeginBatchUpdate();
			}

			EditorFormatMap.SetProperties("TextView Background", properties);
		}

		if (itemInfo[0].bForegroundValid == 1 && ClassificationFormatMap != null)
		{
			TextFormattingRunProperties defaultTextProperties = ClassificationFormatMap.DefaultTextProperties;
			System.Drawing.Color color3 = ColorTranslator.FromWin32((int)itemInfo[0].crForeground);
			System.Windows.Media.Color foreground = System.Windows.Media.Color.FromArgb(color3.A, color3.R, color3.G, color3.B);
			defaultTextProperties = defaultTextProperties.SetForeground(foreground);
			if (!ClassificationFormatMap.IsInBatchUpdate)
			{
				ClassificationFormatMap.BeginBatchUpdate();
			}

			ClassificationFormatMap.DefaultTextProperties = defaultTextProperties;
		}
	}

	public override void OnApply()
	{
		if (EditorFormatMap != null && EditorFormatMap.IsInBatchUpdate)
		{
			EditorFormatMap.EndBatchUpdate();
		}

		if (ClassificationFormatMap != null && ClassificationFormatMap.IsInBatchUpdate)
		{
			ClassificationFormatMap.EndBatchUpdate();
		}
	}
}
