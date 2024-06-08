// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.FunctionTypeItem


using BlackbirdSql.Shared.Controls.Graphing.Gram;


namespace BlackbirdSql.Shared.Controls.Graphing;

internal sealed class FunctionTypeItem
{
	internal enum EnItemType
	{
		Unknown,
		Udf,
		StoredProcedure
	}

	private readonly FunctionType function;

	private readonly EnItemType type;

	internal FunctionType Function => function;

	internal EnItemType Type => type;

	internal FunctionTypeItem(FunctionType function, EnItemType type)
	{
		this.function = function;
		this.type = type;
	}
}
