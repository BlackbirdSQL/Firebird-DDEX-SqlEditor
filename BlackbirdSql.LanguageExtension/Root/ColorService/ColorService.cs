#region Assembly Microsoft.Cosmos.ClientTools.IDECommon, Version=2.6.5000.0, Culture=neutral, PublicKeyToken=f300afd708cefcd3
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\ADL Tools\2.6.5000.0\Microsoft.Cosmos.ClientTools.IDECommon.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.LanguageExtension.Interfaces;
// using Microsoft.Msagl.Drawing;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

// using Microsoft.Cosmos.ClientTools.ClientCommon.Controls;
// using Ns = Microsoft.Cosmos.ClientTools.ClientCommon.Controls;


// Microsoft.Cosmos.ClientTools.ClientCommon.Controls
namespace BlackbirdSql.LanguageExtension.ColorService
{
	/// <summary>
	/// Plagiarized off of static Microsoft.Cosmos.ClientTools.ClientCommon.Controls.ColorService
	/// </summary>
	public class ColorService : AbstractColorService
	{
		// public static readonly Guid Category = new Guid(ServiceData.ColorServiceCategoryGuid);

		private static object _DataSkewGridBackGroundColorKey;

		private static object _DataSkewGridBackGroundBrushKey;

		private static object _DataSkewGridBackGroundTextColorKey;

		private static object _DataSkewGridBackGroundTextBrushKey;

		private static object _DataSkewInfoHeaderColorKey;

		private static object _DataSkewInfoHeaderBrushKey;

		private static object _DataSkewInnerBackgroundColorKey;

		private static object _DataSkewInnerBackgroundBrushKey;

		private static object _ExtractScriptBuilderHeaderColorKey;

		private static object _ExtractScriptBuilderHeaderBrushKey;

		private static object _ExtractScriptBuilderHeaderTextColorKey;

		private static object _ExtractScriptBuilderHeaderTextBrushKey;

		private static object _ExtractScriptBuilderHeaderMouseOnColorKey;

		private static object _ExtractScriptBuilderHeaderMouseOnBrushKey;

		private static object _HyperlinkForegroundColorKey;

		private static object _HyperlinkForegroundBrushKey;

		private static object _HyperlinkForegroundTextColorKey;

		private static object _HyperlinkForegroundTextBrushKey;

		private static object _InfoBarBorderColorKey;

		private static object _InfoBarBorderBrushKey;

		private static object _InfoBarForegroundTextColorKey;

		private static object _InfoBarForegroundTextBrushKey;

		private static object _ScriptBuiltInFunctionTextColorKey;

		private static object _ScriptBuiltInFunctionTextBrushKey;

		private static object _ScriptClassNameTextColorKey;

		private static object _ScriptClassNameTextBrushKey;

		private static object _ScriptCommentTextColorKey;

		private static object _ScriptCommentTextBrushKey;

		private static object _ScriptErrorTextColorKey;

		private static object _ScriptErrorTextBrushKey;

		private static object _ScriptHighlightWordColorKey;

		private static object _ScriptHighlightWordBrushKey;

		private static object _ScriptIdentifierTextColorKey;

		private static object _ScriptIdentifierTextBrushKey;

		private static object _ScriptKeyWordTextColorKey;

		private static object _ScriptKeyWordTextBrushKey;

		private static object _ScriptNumberTextColorKey;

		private static object _ScriptNumberTextBrushKey;

		private static object _ScriptParameterTextColorKey;

		private static object _ScriptParameterTextBrushKey;

		private static object _ScriptPreprocessTextColorKey;

		private static object _ScriptPreprocessTextBrushKey;

		private static object _ScriptStringTextColorKey;

		private static object _ScriptStringTextBrushKey;

		private static object _ScriptTextTextColorKey;

		private static object _ScriptTextTextBrushKey;

		private static object _StageInfoTabItemHeaderColorKey;

		private static object _StageInfoTabItemHeaderBrushKey;

		private static object _StageInfoTabItemSelectedHeaderColorKey;

		private static object _StageInfoTabItemSelectedHeaderBrushKey;

		private static object _StreamPreviewCompositeDataTextColorKey;

		private static object _StreamPreviewCompositeDataTextBrushKey;

		private static object _ThemeIDColorKeyColorKey;

		private static object _ThemeIDColorKeyBrushKey;

		private static object _UdoBarColorKey;

		private static object _UdoBarBrushKey;

		private static object _UdoBarTextColorKey;

		private static object _UdoBarTextBrushKey;

		private static object _ValidationBoxBackgroundColorKey;

		private static object _ValidationBoxBackgroundBrushKey;

		private static object _ValidationErrorForegroundTextColorKey;

		private static object _ValidationErrorForegroundTextBrushKey;

		private static object _ValidationOKForegroundTextColorKey;

		private static object _ValidationOKForegroundTextBrushKey;


		// =========================================================================================================
		#region Constructors / Destructors - ColorService
		// =========================================================================================================


		// ---------------------------------------------------------------------------------
		/// <summary>
		/// Private singleton .ctor
		/// </summary>
		// ---------------------------------------------------------------------------------
		protected ColorService(IVsUIShell5 uiShell) : base(uiShell)
		{
		}



		// ---------------------------------------------------------------------------------
		/// <summary>
		/// Gets or creates the singleton ColorService instance
		/// </summary>
		// ---------------------------------------------------------------------------------
		public static IBColorService GetInstance(IVsUIShell5 uiShell)
		{
			return _Instance ??= new ColorService(uiShell);
		}


		#endregion Constructors / Destructors



		public static bool IsDarkTheme
		{
			get
			{
				System.Windows.Media.Color wpfColor = ThemeIDColorKeyColorKey.GetWpfColor();
				if (wpfColor.R == byte.MaxValue && wpfColor.G == byte.MaxValue)
				{
					return wpfColor.B == byte.MaxValue;
				}

				return false;
			}
		}



		public static object DataSkewGridBackGroundColorKey => _DataSkewGridBackGroundColorKey ??= ThemeResourceKey(Category, "DataSkewGridBackGround", ThemeResourceKeyType.BackgroundColor);

		public static object DataSkewGridBackGroundBrushKey => _DataSkewGridBackGroundBrushKey ??= ThemeResourceKey(Category, "DataSkewGridBackGround", ThemeResourceKeyType.BackgroundBrush);

		public static object DataSkewGridBackGroundTextColorKey => _DataSkewGridBackGroundTextColorKey ??= ThemeResourceKey(Category, "DataSkewGridBackGround", ThemeResourceKeyType.ForegroundColor);

		public static object DataSkewGridBackGroundTextBrushKey => _DataSkewGridBackGroundTextBrushKey ??= ThemeResourceKey(Category, "DataSkewGridBackGround", ThemeResourceKeyType.ForegroundBrush);

		public static object DataSkewInfoHeaderColorKey => _DataSkewInfoHeaderColorKey ??= ThemeResourceKey(Category, "DataSkewInfoHeader", ThemeResourceKeyType.BackgroundColor);

		public static object DataSkewInfoHeaderBrushKey => _DataSkewInfoHeaderBrushKey ??= ThemeResourceKey(Category, "DataSkewInfoHeader", ThemeResourceKeyType.BackgroundBrush);

		public static object DataSkewInnerBackgroundColorKey => _DataSkewInnerBackgroundColorKey ??= ThemeResourceKey(Category, "DataSkewInnerBackground", ThemeResourceKeyType.BackgroundColor);

		public static object DataSkewInnerBackgroundBrushKey => _DataSkewInnerBackgroundBrushKey ??= ThemeResourceKey(Category, "DataSkewInnerBackground", ThemeResourceKeyType.BackgroundBrush);

		public static object ExtractScriptBuilderHeaderColorKey => _ExtractScriptBuilderHeaderColorKey ??= ThemeResourceKey(Category, "ExtractScriptBuilderHeader", ThemeResourceKeyType.BackgroundColor);

		public static object ExtractScriptBuilderHeaderBrushKey => _ExtractScriptBuilderHeaderBrushKey ??= ThemeResourceKey(Category, "ExtractScriptBuilderHeader", ThemeResourceKeyType.BackgroundBrush);

		public static object ExtractScriptBuilderHeaderTextColorKey => _ExtractScriptBuilderHeaderTextColorKey ??= ThemeResourceKey(Category, "ExtractScriptBuilderHeader", ThemeResourceKeyType.ForegroundColor);

		public static object ExtractScriptBuilderHeaderTextBrushKey => _ExtractScriptBuilderHeaderTextBrushKey ??= ThemeResourceKey(Category, "ExtractScriptBuilderHeader", ThemeResourceKeyType.ForegroundBrush);

		public static object ExtractScriptBuilderHeaderMouseOnColorKey => _ExtractScriptBuilderHeaderMouseOnColorKey ??= ThemeResourceKey(Category, "ExtractScriptBuilderHeaderMouseOn", ThemeResourceKeyType.BackgroundColor);

		public static object ExtractScriptBuilderHeaderMouseOnBrushKey => _ExtractScriptBuilderHeaderMouseOnBrushKey ??= ThemeResourceKey(Category, "ExtractScriptBuilderHeaderMouseOn", ThemeResourceKeyType.BackgroundBrush);

		public static object HyperlinkForegroundColorKey => _HyperlinkForegroundColorKey ??= ThemeResourceKey(Category, "HyperlinkForeground", ThemeResourceKeyType.BackgroundColor);

		public static object HyperlinkForegroundBrushKey => _HyperlinkForegroundBrushKey ??= ThemeResourceKey(Category, "HyperlinkForeground", ThemeResourceKeyType.BackgroundBrush);

		public static object HyperlinkForegroundTextColorKey => _HyperlinkForegroundTextColorKey ??= ThemeResourceKey(Category, "HyperlinkForeground", ThemeResourceKeyType.ForegroundColor);

		public static object HyperlinkForegroundTextBrushKey => _HyperlinkForegroundTextBrushKey ??= ThemeResourceKey(Category, "HyperlinkForeground", ThemeResourceKeyType.ForegroundBrush);

		public static object InfoBarBackgroundColorKey => InfoBarColors.InfoBarBackgroundColorKey;

		public static object InfoBarBackgroundBrushKey => InfoBarColors.InfoBarBackgroundBrushKey;

		public static object InfoBarBorderColorKey => _InfoBarBorderColorKey ??= ThemeResourceKey(Category, "InfoBarBorder", ThemeResourceKeyType.BackgroundColor);

		public static object InfoBarBorderBrushKey => _InfoBarBorderBrushKey ??= ThemeResourceKey(Category, "InfoBarBorder", ThemeResourceKeyType.BackgroundBrush);

		public static object InfoBarForegroundTextColorKey => _InfoBarForegroundTextColorKey ??= ThemeResourceKey(Category, "InfoBarForeground", ThemeResourceKeyType.ForegroundColor);

		public static object InfoBarForegroundTextBrushKey => _InfoBarForegroundTextBrushKey ??= ThemeResourceKey(Category, "InfoBarForeground", ThemeResourceKeyType.ForegroundBrush);


		public static object ScriptBuiltInFunctionTextColorKey => _ScriptBuiltInFunctionTextColorKey ??= ThemeResourceKey(Category, "ScriptBuiltInFunction", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptBuiltInFunctionTextBrushKey => _ScriptBuiltInFunctionTextBrushKey ??= ThemeResourceKey(Category, "ScriptBuiltInFunction", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptClassNameTextColorKey => _ScriptClassNameTextColorKey ??= ThemeResourceKey(Category, "ScriptClassName", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptClassNameTextBrushKey => _ScriptClassNameTextBrushKey ??= ThemeResourceKey(Category, "ScriptClassName", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptCommentTextColorKey => _ScriptCommentTextColorKey ??= ThemeResourceKey(Category, "ScriptComment", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptCommentTextBrushKey => _ScriptCommentTextBrushKey ??= ThemeResourceKey(Category, "ScriptComment", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptErrorTextColorKey => _ScriptErrorTextColorKey ??= ThemeResourceKey(Category, "ScriptError", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptErrorTextBrushKey => _ScriptErrorTextBrushKey ??= ThemeResourceKey(Category, "ScriptError", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptHighlightWordColorKey => _ScriptHighlightWordColorKey ??= ThemeResourceKey(Category, "ScriptHighlightWord", ThemeResourceKeyType.BackgroundColor);

		public static object ScriptHighlightWordBrushKey => _ScriptHighlightWordBrushKey ??= ThemeResourceKey(Category, "ScriptHighlightWord", ThemeResourceKeyType.BackgroundBrush);

		public static object ScriptIdentifierTextColorKey => _ScriptIdentifierTextColorKey ??= ThemeResourceKey(Category, "ScriptIdentifier", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptIdentifierTextBrushKey => _ScriptIdentifierTextBrushKey ??= ThemeResourceKey(Category, "ScriptIdentifier", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptKeyWordTextColorKey => _ScriptKeyWordTextColorKey ??= ThemeResourceKey(Category, "ScriptKeyWord", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptKeyWordTextBrushKey => _ScriptKeyWordTextBrushKey ??= ThemeResourceKey(Category, "ScriptKeyWord", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptNumberTextColorKey => _ScriptNumberTextColorKey ??= ThemeResourceKey(Category, "ScriptNumber", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptNumberTextBrushKey => _ScriptNumberTextBrushKey ??= ThemeResourceKey(Category, "ScriptNumber", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptParameterTextColorKey => _ScriptParameterTextColorKey ??= ThemeResourceKey(Category, "ScriptParameter", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptParameterTextBrushKey => _ScriptParameterTextBrushKey ??= ThemeResourceKey(Category, "ScriptParameter", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptPreprocessTextColorKey => _ScriptPreprocessTextColorKey ??= ThemeResourceKey(Category, "ScriptPreprocess", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptPreprocessTextBrushKey => _ScriptPreprocessTextBrushKey ??= ThemeResourceKey(Category, "ScriptPreprocess", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptStringTextColorKey => _ScriptStringTextColorKey ??= ThemeResourceKey(Category, "ScriptString", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptStringTextBrushKey => _ScriptStringTextBrushKey ??= ThemeResourceKey(Category, "ScriptString", ThemeResourceKeyType.ForegroundBrush);

		public static object ScriptTextTextColorKey => _ScriptTextTextColorKey ??= ThemeResourceKey(Category, "ScriptText", ThemeResourceKeyType.ForegroundColor);

		public static object ScriptTextTextBrushKey => _ScriptTextTextBrushKey ??= ThemeResourceKey(Category, "ScriptText", ThemeResourceKeyType.ForegroundBrush);

		public static object StageInfoTabItemHeaderColorKey => _StageInfoTabItemHeaderColorKey ??= ThemeResourceKey(Category, "StageInfoTabItemHeader", ThemeResourceKeyType.BackgroundColor);

		public static object StageInfoTabItemHeaderBrushKey => _StageInfoTabItemHeaderBrushKey ??= ThemeResourceKey(Category, "StageInfoTabItemHeader", ThemeResourceKeyType.BackgroundBrush);

		public static object StageInfoTabItemSelectedHeaderColorKey => _StageInfoTabItemSelectedHeaderColorKey ??= ThemeResourceKey(Category, "StageInfoTabItemSelectedHeader", ThemeResourceKeyType.BackgroundColor);

		public static object StageInfoTabItemSelectedHeaderBrushKey => _StageInfoTabItemSelectedHeaderBrushKey ??= ThemeResourceKey(Category, "StageInfoTabItemSelectedHeader", ThemeResourceKeyType.BackgroundBrush);

		public static object StreamPreviewCompositeDataTextColorKey => _StreamPreviewCompositeDataTextColorKey ??= ThemeResourceKey(Category, "StreamPreviewCompositeData", ThemeResourceKeyType.ForegroundColor);

		public static object StreamPreviewCompositeDataTextBrushKey => _StreamPreviewCompositeDataTextBrushKey ??= ThemeResourceKey(Category, "StreamPreviewCompositeData", ThemeResourceKeyType.ForegroundBrush);

		public static object ThemeIDColorKeyColorKey => _ThemeIDColorKeyColorKey ??= ThemeResourceKey(Category, "ThemeIDColorKey", ThemeResourceKeyType.BackgroundColor);

		public static object ThemeIDColorKeyBrushKey => _ThemeIDColorKeyBrushKey ??= ThemeResourceKey(Category, "ThemeIDColorKey", ThemeResourceKeyType.BackgroundBrush);

		public static object UdoBarColorKey => _UdoBarColorKey ??= ThemeResourceKey(Category, "UdoBar", ThemeResourceKeyType.BackgroundColor);

		public static object UdoBarBrushKey => _UdoBarBrushKey ??= ThemeResourceKey(Category, "UdoBar", ThemeResourceKeyType.BackgroundBrush);

		public static object UdoBarTextColorKey => _UdoBarTextColorKey ??= ThemeResourceKey(Category, "UdoBar", ThemeResourceKeyType.ForegroundColor);

		public static object UdoBarTextBrushKey => _UdoBarTextBrushKey ??= ThemeResourceKey(Category, "UdoBar", ThemeResourceKeyType.ForegroundBrush);


		public static object ValidationBoxBackgroundColorKey => _ValidationBoxBackgroundColorKey ??= ThemeResourceKey(Category, "ValidationBoxBackground", ThemeResourceKeyType.BackgroundColor);

		public static object ValidationBoxBackgroundBrushKey => _ValidationBoxBackgroundBrushKey ??= ThemeResourceKey(Category, "ValidationBoxBackground", ThemeResourceKeyType.BackgroundBrush);

		public static object ValidationErrorForegroundTextColorKey => _ValidationErrorForegroundTextColorKey ??= ThemeResourceKey(Category, "ValidationErrorForeground", ThemeResourceKeyType.ForegroundColor);

		public static object ValidationErrorForegroundTextBrushKey => _ValidationErrorForegroundTextBrushKey ??= ThemeResourceKey(Category, "ValidationErrorForeground", ThemeResourceKeyType.ForegroundBrush);

		public static object ValidationOKForegroundTextColorKey => _ValidationOKForegroundTextColorKey ??= ThemeResourceKey(Category, "ValidationOKForeground", ThemeResourceKeyType.ForegroundColor);

		public static object ValidationOKForegroundTextBrushKey => _ValidationOKForegroundTextBrushKey ??= ThemeResourceKey(Category, "ValidationOKForeground", ThemeResourceKeyType.ForegroundBrush);


		private static object ThemeResourceKey(Guid Category, string key, ThemeResourceKeyType themeResourceKeyType)
		{
			if (Instance == null)
			{
				return new object();
			}

			return Instance.GetKey(Category, key, (int)themeResourceKeyType);
		}
	}
}
