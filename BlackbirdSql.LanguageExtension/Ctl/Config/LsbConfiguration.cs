// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.Configuration

using System.Collections.Generic;
using BlackbirdSql.LanguageExtension.Properties;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.LanguageExtension.Ctl.Config;


// =========================================================================================================
//
//										LsbConfiguration Class
//
/// <summary>
/// Language service config class.
/// </summary>
// =========================================================================================================
internal static class LsbConfiguration
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbConfiguration
	// ---------------------------------------------------------------------------------


	static LsbConfiguration()
	{
		_S_ColorableItems = [];
		_S_DefaultDefinition = new TokenDefinitionI(TokenType.Text, TokenColor.Text, TokenTriggers.None);
		_S_Definitions = [];

		_S_CommentInfo.BlockEnd = "*/";
		_S_CommentInfo.BlockStart = "/*";
		_S_CommentInfo.LineStart = "--";
		_S_CommentInfo.UseLineComments = true;

		CreateColor(Resources.Keyword, "Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.Comment, "Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.Identifier, "Identifier", COLORINDEX.CI_USERTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.String, "String", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.Number, "Number", COLORINDEX.CI_USERTEXT_FG, COLORINDEX.CI_USERTEXT_BK);

		TokenColor tokenColor = CreateColor(Resources.Text, "Text", COLORINDEX.CI_USERTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
		S_TextColorableItem = S_ColorableItems[(int)(tokenColor - 1)];

		CreateColor(Resources.SQLStoredProcedure, "SQL Stored Procedure", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLSystemTable, "SQL System Table", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLSystemFunction, "SQL System Function", COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLOperator, "SQL Operator", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLString, "SQL String", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK);
		CreateColor(Resources.SQLCMDCommand, "SQLCMD Command", COLORINDEX.CI_FIRSTFIXEDCOLOR, COLORINDEX.CI_LIGHTGRAY);
		CreateColor(Resources.Error, "Error", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, bold: false, strikethrough: true);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - LsbConfiguration
	// =========================================================================================================


	internal const string C_Name = PackageData.C_LanguageLongName;
	internal const string C_Extension = PackageData.C_Extension;


	#endregion Constants





	// =========================================================================================================
	#region Fields - LsbConfiguration
	// =========================================================================================================


	private static readonly List<IVsColorableItem> _S_ColorableItems;
	private static TokenDefinitionI _S_DefaultDefinition;
	private static readonly Dictionary<int, TokenDefinitionI> _S_Definitions;
	private static CommentInfo _S_CommentInfo;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbConfiguration
	// =========================================================================================================


	internal static IList<IVsColorableItem> S_ColorableItems => _S_ColorableItems;

	internal static CommentInfo S_CommentInfo => _S_CommentInfo;

	internal static IVsColorableItem S_TextColorableItem { get; private set; }


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbConfiguration
	// =========================================================================================================


	internal static TokenColor CreateColor(string name, string canonicalName, COLORINDEX foreground, COLORINDEX background, bool bold = false, bool strikethrough = false)
	{
		_S_ColorableItems.Add(new LsbColorableItem(name, canonicalName, foreground, background, bold, strikethrough));
		return (TokenColor)_S_ColorableItems.Count;
	}

	internal static void ColorToken(int token, TokenType type, TokenColor color, TokenTriggers trigger)
	{
		_S_Definitions[token] = new TokenDefinitionI(type, color, trigger);
	}

	internal static TokenDefinitionI GetDefinition(int token)
	{
		if (!_S_Definitions.TryGetValue(token, out TokenDefinitionI value))
			return _S_DefaultDefinition;

		return value;
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - LsbConfiguration
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Struct TokenDefinitionI.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal struct TokenDefinitionI
	{
		public TokenDefinitionI(TokenType type, TokenColor color, TokenTriggers triggers)
		{
			TokenType = type;
			TokenColor = color;
			TokenTriggers = triggers;
		}



		internal TokenType TokenType;
		internal TokenColor TokenColor;
		internal TokenTriggers TokenTriggers;
	}


	#endregion Nested types


}
