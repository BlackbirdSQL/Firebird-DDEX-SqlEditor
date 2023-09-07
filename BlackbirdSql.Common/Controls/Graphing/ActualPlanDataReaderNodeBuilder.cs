// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ActualPlanDataReaderNodeBuilder


using BlackbirdSql.Common.Controls.Graphing.Enums;

namespace BlackbirdSql.Common.Controls.Graphing;

internal class ActualPlanDataReaderNodeBuilder : AbstractDataReaderNodeBuilder
{
	private static readonly string[] propertyNames = new string[20]
	{
		NodeBuilderConstants.ActualRows,
		NodeBuilderConstants.ActualExecutions,
		NodeBuilderConstants.StatementText,
		null,
		NodeBuilderConstants.NodeId,
		null,
		NodeBuilderConstants.PhysicalOp,
		NodeBuilderConstants.LogicalOp,
		NodeBuilderConstants.Argument,
		NodeBuilderConstants.DefinedValues,
		NodeBuilderConstants.EstimateRows,
		NodeBuilderConstants.EstimateIO,
		NodeBuilderConstants.EstimateCPU,
		NodeBuilderConstants.AvgRowSize,
		NodeBuilderConstants.TotalSubtreeCost,
		NodeBuilderConstants.OutputList,
		NodeBuilderConstants.Warnings,
		NodeBuilderConstants.StatementType,
		NodeBuilderConstants.Parallel,
		null
	};

	protected override EnExecutionPlanType ExecutionPlanType => EnExecutionPlanType.Actual;

	protected override int NodeIdIndex => 4;

	protected override int ParentIndex => 5;

	protected override string[] GetPropertyNames()
	{
		return propertyNames;
	}
}
