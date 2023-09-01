#region Assembly Microsoft.Cosmos.ScopeStudio.VsExtension.LanguageService.SqlIP, Version=2.6.5000.0, Culture=neutral, PublicKeyToken=f300afd708cefcd3
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Collections.Generic;
using System.Drawing;

// using Microsoft.Cosmos.ClientTools.ClientCommon.Controls;

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using BlackbirdSql.LanguageExtension.ColorService.Enums;



// namespace Microsoft.Cosmos.ScopeStudio.VsExtension.LanguageService;
namespace BlackbirdSql.LanguageExtension.ColorService;


public static class ColorConfiguration
{
	public struct TokenDefinition
	{
		public TokenType TokenType;

		public ScopeStudioTokenColor ScopeStudioTokenColor;

		public TokenTriggers TokenTriggers;

		public TokenDefinition(TokenType type, ScopeStudioTokenColor color, TokenTriggers triggers)
		{
			TokenType = type;
			ScopeStudioTokenColor = color;
			TokenTriggers = triggers;
		}
	}

	private static readonly Dictionary<int, IVsColorableItem> _colorableItems;

	private static TokenDefinition _defaultDefinition;

	private static readonly Dictionary<int, TokenDefinition> _definitions;

	private static readonly IVsFontAndColorUtilities _fontColorUtils;

	private static readonly IVsFontAndColorStorage _fontColorStorage;

	private static CommentInfo scopeInfo;

	public const string C_Name = "FB-SQL";

	public static IVsFontAndColorUtilities FontAndColorUtilities => _fontColorUtils;

	public static IVsFontAndColorStorage FontAndColorStorage => _fontColorStorage;

	public static IDictionary<int, IVsColorableItem> ColorableItems => _colorableItems;

	public static CommentInfo MyCommentInfo => scopeInfo;

	public static void SetColor(int tokenColorIndex, string name, COLORINDEX foreground, COLORINDEX background)
	{
		SetColor(tokenColorIndex, name, foreground, background, Color.Empty, Color.Empty, bold: false, strikethrough: false);
	}

	public static void SetColor(int tokenColorIndex, string name, COLORINDEX foreground, COLORINDEX background, bool bold, bool strikethrough)
	{
		SetColor(tokenColorIndex, name, foreground, background, Color.Empty, Color.Empty, bold, strikethrough);
	}

	public static void SetColor(int tokenColorIndex, string name, COLORINDEX foreground, COLORINDEX background, Color hiForeColor, Color hiBackColor, bool bold, bool strikethrough)
	{
		_colorableItems[tokenColorIndex] = new DslColorableItem(name, name, foreground, background, hiForeColor, hiBackColor, bold, strikethrough);
	}

	public static Color GetColorOfIndex(COLORINDEX idx)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		uint pcrResult = 0xFFFFFFu;
		if (_fontColorUtils != null && _fontColorUtils.GetRGBOfIndex(idx, out pcrResult) != 0)
		{
			pcrResult = 0xFFFFFFu;
		}

		return Color.FromArgb((int)pcrResult | -16777216);
	}

	public static void AddTokenDefinition(int tokenIndex, TokenType type, ScopeStudioTokenColor color, TokenTriggers trigger)
	{
		_definitions[tokenIndex] = new TokenDefinition(type, color, trigger);
	}

	public static TokenDefinition GetDefinition(int token)
	{
		if (!_definitions.TryGetValue(token, out var value))
		{
			return _defaultDefinition;
		}

		return value;
	}

	private static void AddTokenDefinitions(IEnumerable<int> tokenValues, TokenType type, ScopeStudioTokenColor color, TokenTriggers trigger)
	{
		foreach (int tokenValue in tokenValues)
		{
			AddTokenDefinition(tokenValue, type, color, trigger);
		}
	}

	internal static void ReInitSyntaxColor()
	{
		SetColor(6, "FB-SQL - Text", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptTextTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(7, "FB-SQL - Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptKeyWordTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(8, "FB-SQL - Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptCommentTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(9, "FB-SQL - Identifier", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptIdentifierTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(10, "FB-SQL - String", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptStringTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(11, "FB-SQL - Number", COLORINDEX.CI_GREEN, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptNumberTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(12, "FB-SQL - ClassName", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptClassNameTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(13, "FB-SQL - Error", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptErrorTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(14, "FB-SQL - BuiltInFunction", COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptBuiltInFunctionTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(15, "FB-SQL - Parameter", COLORINDEX.CI_BROWN, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptParameterTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
		SetColor(16, "FB-SQL - Preprocess", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK, ColorService.ScriptPreprocessTextColorKey.GetGDIColor(), Color.Empty, bold: false, strikethrough: false);
	}

	static ColorConfiguration()
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		_colorableItems = new Dictionary<int, IVsColorableItem>();
		_defaultDefinition = new TokenDefinition(TokenType.Text, ScopeStudioTokenColor.Text, TokenTriggers.None);
		_definitions = new Dictionary<int, TokenDefinition>();
		_fontColorUtils = Package.GetGlobalService(typeof(SVsFontAndColorStorage)) as IVsFontAndColorUtilities;
		_fontColorStorage = Package.GetGlobalService(typeof(SVsFontAndColorStorage)) as IVsFontAndColorStorage;
		scopeInfo.BlockEnd = "*/";
		scopeInfo.BlockStart = "/*";
		scopeInfo.LineStart = "//";
		scopeInfo.UseLineComments = true;
		SetColor(0, "Text", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK, bold: false, strikethrough: false);
		SetColor(1, "Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK, bold: false, strikethrough: false);
		SetColor(2, "Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK, bold: false, strikethrough: false);
		SetColor(3, "Identifier", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK, bold: false, strikethrough: false);
		SetColor(4, "String", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK, bold: false, strikethrough: false);
		SetColor(5, "Number", COLORINDEX.CI_GREEN, COLORINDEX.CI_USERTEXT_BK, bold: false, strikethrough: false);
		ReInitSyntaxColor();
		AddTokenDefinition(5, TokenType.Literal, ScopeStudioTokenColor.Text, TokenTriggers.None);
		AddTokenDefinition(2, TokenType.Keyword, ScopeStudioTokenColor.Keyword, TokenTriggers.None);
		AddTokenDefinition(6, TokenType.Comment, ScopeStudioTokenColor.Comment, TokenTriggers.None);
		AddTokenDefinition(9, TokenType.Identifier, ScopeStudioTokenColor.Identifier, TokenTriggers.MemberSelect);
		AddTokenDefinition(4, TokenType.String, ScopeStudioTokenColor.String, TokenTriggers.None);
		AddTokenDefinition(18, TokenType.Text, ScopeStudioTokenColor.Number, TokenTriggers.None);
		AddTokenDefinition(17, TokenType.Identifier, ScopeStudioTokenColor.ClassName, TokenTriggers.None);
		AddTokenDefinition(1, TokenType.Text, ScopeStudioTokenColor.Error, TokenTriggers.None);
		AddTokenDefinition(3, TokenType.Keyword, ScopeStudioTokenColor.BuiltInFunction, TokenTriggers.None);
		AddTokenDefinition(7, TokenType.Text, ScopeStudioTokenColor.Parameter, TokenTriggers.None);
		AddTokenDefinition(8, TokenType.WhiteSpace, ScopeStudioTokenColor.Text, TokenTriggers.MemberSelect);
		AddTokenDefinition(10, TokenType.Delimiter, ScopeStudioTokenColor.Text, TokenTriggers.ParameterStart);
		AddTokenDefinition(12, TokenType.Delimiter, ScopeStudioTokenColor.Text, TokenTriggers.MemberSelect | TokenTriggers.ParameterNext);
		AddTokenDefinition(11, TokenType.Delimiter, ScopeStudioTokenColor.Text, TokenTriggers.ParameterEnd);
		AddTokenDefinitions(new List<int> { 14, 13, 15, 16 }, TokenType.Delimiter, ScopeStudioTokenColor.Text, TokenTriggers.MemberSelect);
	}


	private static byte SatMul(byte v, double s)
	{
		if (s < 0.0)
		{
			s = 0.0;
		}

		double num = v * s;
		if (!(num > 255.0))
		{
			return (byte)num;
		}

		return byte.MaxValue;
	}


	public static System.Windows.Media.Color GetWpfColor(this object resourceKey)
	{
		if (AbstractColorService.Instance == null)
		{
			return System.Windows.Media.Colors.Transparent;
		}

		return AbstractColorService.Instance.GetColor(resourceKey);
	}

	public static System.Windows.Media.Color GetWpfColor(this object resourceKey, double scale)
	{
		if (AbstractColorService.Instance == null)
		{
			return System.Windows.Media.Colors.Transparent;
		}

		System.Windows.Media.Color color = AbstractColorService.Instance.GetColor(resourceKey);
		if (scale == 1.0)
		{
			return color;
		}

		return System.Windows.Media.Color.FromArgb(color.A, SatMul(color.R, scale), SatMul(color.G, scale), SatMul(color.B, scale));
	}

	public static Color GetGDIColor(this object resourceKey)
	{
		if (AbstractColorService.Instance == null)
		{
			return Color.Transparent;
		}

		return AbstractColorService.Instance.GetGDIColor(resourceKey);
	}

	public static Color GetGDIColor(this object resourceKey, double scale)
	{
		if (AbstractColorService.Instance == null)
		{
			return Color.Transparent;
		}

		Color gDIColor = AbstractColorService.Instance.GetGDIColor(resourceKey);
		if (scale == 1.0)
		{
			return gDIColor;
		}

		return Color.FromArgb(gDIColor.A, SatMul(gDIColor.R, scale), SatMul(gDIColor.G, scale), SatMul(gDIColor.B, scale));
	}

	/*
	public static Microsoft.Msagl.Drawing.Color GetMsaglColor(this object resourceKey)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		System.Windows.Media.Color wpfColor = resourceKey.GetWpfColor();
		return new Microsoft.Msagl.Drawing.Color(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);
	}
	*/
}
