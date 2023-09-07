// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.FunctionTypeItem


using BlackbirdSql.Common.Controls.Graphing.Gram;


namespace BlackbirdSql.Common.Controls.Graphing;

internal sealed class FunctionTypeItem
{
	internal enum ItemType
	{
		Unknown,
		Udf,
		StoredProcedure
	}

	private readonly FunctionType function;

	private readonly ItemType type;

	internal FunctionType Function => function;

	internal ItemType Type => type;

	internal FunctionTypeItem(FunctionType function, ItemType type)
	{
		this.function = function;
		this.type = type;
	}
}
