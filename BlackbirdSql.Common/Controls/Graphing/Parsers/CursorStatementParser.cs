// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.CursorStatementParser


namespace BlackbirdSql.Common.Controls.Graphing.Parsers;

internal class CursorStatementParser : StatementParser
{
	private static CursorStatementParser cursorStatementParser;

	public new static CursorStatementParser Instance
	{
		get
		{
			cursorStatementParser ??= new CursorStatementParser();
			return cursorStatementParser;
		}
	}

	protected override Operation GetNodeOperation(Node node)
	{
		object obj = node["CursorActualType"];
		obj ??= node["StatementType"];
		if (obj == null)
		{
			return Operation.Unknown;
		}
		return OperationTable.GetCursorType(obj.ToString());
	}

	private CursorStatementParser()
	{
	}
}
