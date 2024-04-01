// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.Configuration
using System.Collections.Generic;
using BlackbirdSql.Core;
using BlackbirdSql.LanguageExtension.Properties;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.LanguageExtension.Ctl.Config;


internal static class LsbConfiguration
{
	public struct TokenDefinition
	{
		public TokenType TokenType;

		public TokenColor TokenColor;

		public TokenTriggers TokenTriggers;

		public TokenDefinition(TokenType type, TokenColor color, TokenTriggers triggers)
		{
			TokenType = type;
			TokenColor = color;
			TokenTriggers = triggers;
		}
	}

	private static readonly List<IVsColorableItem> colorableItems;

	private static TokenDefinition defaultDefinition;

	private static readonly Dictionary<int, TokenDefinition> definitions;

	public const string Name = PackageData.LanguageLongName;

	public const string Extension = SystemData.Extension;

	private static CommentInfo myCommentInfo;

	public static IList<IVsColorableItem> ColorableItems => colorableItems;

	public static CommentInfo MyCommentInfo => myCommentInfo;

	public static IVsColorableItem TextColorableItem { get; private set; }

	public static TokenColor CreateColor(string name, string canonicalName, COLORINDEX foreground, COLORINDEX background, bool bold = false, bool strikethrough = false)
	{
		colorableItems.Add(new LsbColorableItem(name, canonicalName, foreground, background, bold, strikethrough));
		return (TokenColor)colorableItems.Count;
	}

	public static void ColorToken(int token, TokenType type, TokenColor color, TokenTriggers trigger)
	{
		definitions[token] = new TokenDefinition(type, color, trigger);
	}

	public static TokenDefinition GetDefinition(int token)
	{
		if (!definitions.TryGetValue(token, out var value))
		{
			return defaultDefinition;
		}
		return value;
	}

	static LsbConfiguration()
	{
		colorableItems = [];
		defaultDefinition = new TokenDefinition(TokenType.Text, TokenColor.Text, TokenTriggers.None);
		definitions = [];
		myCommentInfo.BlockEnd = "*/";
		myCommentInfo.BlockStart = "/*";
		myCommentInfo.LineStart = "--";
		myCommentInfo.UseLineComments = true;
		CreateColor(Resources.Keyword, "Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.Comment, "Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.Identifier, "Identifier", COLORINDEX.CI_USERTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.String, "String", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.Number, "Number", COLORINDEX.CI_USERTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
		TokenColor tokenColor = CreateColor(Resources.Text, "Text", COLORINDEX.CI_USERTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
		TextColorableItem = ColorableItems[(int)(tokenColor - 1)];
		CreateColor(Resources.SQLStoredProcedure, "SQL Stored Procedure", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLSystemTable, "SQL System Table", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLSystemFunction, "SQL System Function", COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLOperator, "SQL Operator", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLString, "SQL String", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLCMDCommand, "SQLCMD Command", COLORINDEX.CI_FIRSTFIXEDCOLOR, COLORINDEX.CI_LIGHTGRAY);
		CreateColor(Resources.Error, "Error", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, bold: false, strikethrough: true);
	}
}
