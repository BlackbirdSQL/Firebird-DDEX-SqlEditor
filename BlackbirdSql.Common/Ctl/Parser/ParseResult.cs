// Microsoft.SqlServer.Management.SqlParser, Version=16.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlParser.Parser.ParseResult


namespace BlackbirdSql.Common.Ctl.Parser;

/// <summary>
/// Interim Placeholder
/// </summary>
public class ParseResult
{
	// private readonly SqlScript sqlScript;

	// public int BatchCount => sqlScript.Batches.Count;

	// public IEnumerable<Error> Errors => sqlScript.Errors;

	// public SqlScript Script => sqlScript;

	internal ParseResult(/* SqlScript sqlScript */)
	{
		// this.sqlScript = sqlScript;
	}

	public int GetTokenNumber(int line, int col)
	{
		return 0; // sqlScript.GetTokenNumber(line, col);
	}
}
