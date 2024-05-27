// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.IVariableResolver


using BlackbirdSql.Sys;

namespace BlackbirdSql.Common.Model.Interfaces;

public interface IBVariableResolver
{
	EnParserAction ResolveVariable(string varName, ref string varValue);

	EnParserAction DeleteVariable(string varName);

	EnParserAction ResolveVariableOwnership(string varName, string varValue, ref bool bTakeOwmership);
}
