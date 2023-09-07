// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.INodeBuilder


using BlackbirdSql.Common.Controls.Graphing;

namespace BlackbirdSql.Common.Controls.Graphing.Interfaces;

public interface INodeBuilder
{
	ExecutionPlanGraph[] Execute(object dataSource);
}
