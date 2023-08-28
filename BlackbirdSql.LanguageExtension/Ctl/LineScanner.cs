#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


// using Babel;
using Microsoft.VisualStudio.Package;




// using Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;


// namespace Microsoft.VisualStudio.Data.Tools.SqlLanguageServices
namespace BlackbirdSql.LanguageExtension
{
	internal class LineScanner : IScanner
	{
		private readonly Babel.LineScanner _BabelScanner;

		public bool IsSqlCmdModeEnabled
		{
			set
			{
				_BabelScanner.IsSqlCmdModeEnabled = value;
			}
		}

		public string BatchSeparator
		{
			set
			{
				_BabelScanner.BatchSeparator = value;
			}
		}

		public LineScanner()
		{
			_BabelScanner = new Babel.LineScanner();
		}

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
	}
}
