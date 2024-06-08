// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.Live.LivePlanDataReaderNodeBuilder


using BlackbirdSql.Shared.Controls.Graphing.Enums;

namespace BlackbirdSql.Shared.Controls.Graphing;

internal class LivePlanDataReaderNodeBuilder : ActualPlanDataReaderNodeBuilder
{
	protected override EnExecutionPlanType ExecutionPlanType => EnExecutionPlanType.Live;
}
