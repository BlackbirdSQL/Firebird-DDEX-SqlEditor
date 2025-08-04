// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.LineScanner

using Babel;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using Microsoft.VisualStudio.Package;



namespace BlackbirdSql.LanguageExtension.Ctl;


// =========================================================================================================
//
//										LsbLineScanner Class
//
/// <summary>
/// Language service IScanner line scanner implementation.
/// </summary>
// =========================================================================================================
internal class LsbLineScanner : IScanner
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbLineScanner
	// ---------------------------------------------------------------------------------


	public LsbLineScanner()
	{
		_BabelScanner = new LineScanner
		{
			BatchSeparator = PersistentSettings.EditorExecutionBatchSeparator
		};
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbLineScanner
	// =========================================================================================================


	private readonly LineScanner _BabelScanner;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbLineScanner
	// =========================================================================================================


	internal string BatchSeparator
	{
		set
		{
			_BabelScanner.BatchSeparator = value;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbLineScanner
	// =========================================================================================================


	public bool ScanTokenAndProvideInfoAboutIt(Microsoft.VisualStudio.Package.TokenInfo tokenInfo, ref int state)
	{
		Babel.TokenInfo tokenInfo2 = new Babel.TokenInfo();
		if (_BabelScanner.ScanTokenAndProvideInfoAboutIt(tokenInfo2, ref state))
		{
			tokenInfo.StartIndex = tokenInfo2.StartIndex;
			tokenInfo.EndIndex = tokenInfo2.EndIndex;
			tokenInfo.Color = GetVsTokenColorForBableType(tokenInfo2.Type);
			tokenInfo.Type = GetVSTokenTypeForBabelType(tokenInfo2.Type);
			tokenInfo.Trigger = (Microsoft.VisualStudio.Package.TokenTriggers)tokenInfo2.Trigger;
			tokenInfo.Token = tokenInfo2.Token;
			return true;
		}
		return false;
	}

	private TokenColor GetVsTokenColorForBableType(Babel.TokenType tokenType)
	{
		if (tokenType == Babel.TokenType.Delimiter)
		{
			return (TokenColor)10;
		}
		return (TokenColor)tokenType;
	}

	private Microsoft.VisualStudio.Package.TokenType GetVSTokenTypeForBabelType(Babel.TokenType tokenType)
	{
		return tokenType switch
		{
			Babel.TokenType.Comment => Microsoft.VisualStudio.Package.TokenType.Comment, 
			Babel.TokenType.Delimiter => Microsoft.VisualStudio.Package.TokenType.Delimiter, 
			Babel.TokenType.Error => Microsoft.VisualStudio.Package.TokenType.Unknown, 
			Babel.TokenType.Identifier => Microsoft.VisualStudio.Package.TokenType.Identifier, 
			Babel.TokenType.Keyword => Microsoft.VisualStudio.Package.TokenType.Keyword, 
			Babel.TokenType.Number => Microsoft.VisualStudio.Package.TokenType.Literal, 
			Babel.TokenType.SqlCmdCommand => Microsoft.VisualStudio.Package.TokenType.Text, 
			Babel.TokenType.SqlOperator => Microsoft.VisualStudio.Package.TokenType.Operator, 
			Babel.TokenType.SqlStoredProcedure => Microsoft.VisualStudio.Package.TokenType.Identifier, 
			Babel.TokenType.SqlString => Microsoft.VisualStudio.Package.TokenType.String, 
			Babel.TokenType.SqlSystemFunction => Microsoft.VisualStudio.Package.TokenType.Identifier, 
			Babel.TokenType.SqlSystemTable => Microsoft.VisualStudio.Package.TokenType.Identifier, 
			Babel.TokenType.Text => Microsoft.VisualStudio.Package.TokenType.Text, 
			_ => Microsoft.VisualStudio.Package.TokenType.Text, 
		};
	}

	public void SetSource(string source, int offset)
	{
		_BabelScanner.SetSource(source, offset);
	}


	#endregion Methods
}
