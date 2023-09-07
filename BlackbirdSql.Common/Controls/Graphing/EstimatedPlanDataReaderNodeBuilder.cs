// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.EstimatedPlanDataReaderNodeBuilder


using BlackbirdSql.Common.Controls.Graphing.Enums;

namespace BlackbirdSql.Common.Controls.Graphing;

internal sealed class EstimatedPlanDataReaderNodeBuilder : AbstractDataReaderNodeBuilder
{
	private static readonly string[] propertyNames = new string[18]
	{
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
		NodeBuilderConstants.EstimateExecutions
	};

	protected override EnExecutionPlanType ExecutionPlanType => EnExecutionPlanType.Estimated;

	protected override int NodeIdIndex => 2;

	protected override int ParentIndex => 3;

	protected override string[] GetPropertyNames()
	{
		return propertyNames;
	}
}
