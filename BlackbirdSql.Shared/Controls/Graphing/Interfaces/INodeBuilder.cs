// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.INodeBuilder


using BlackbirdSql.Shared.Controls.Graphing;

namespace BlackbirdSql.Shared.Controls.Graphing.Interfaces;

internal interface INodeBuilder
{
	ExecutionPlanGraph[] Execute(object dataSource);
}
