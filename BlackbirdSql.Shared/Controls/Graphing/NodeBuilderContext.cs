// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.NodeBuilderContext


using BlackbirdSql.Shared.Controls.Graphing.Enums;

namespace BlackbirdSql.Shared.Controls.Graphing;

public class NodeBuilderContext
{
	private readonly ExecutionPlanGraph _Graph;

	private readonly EnExecutionPlanType _ExecutionPlanType;

	private readonly object _Context;

	public ExecutionPlanGraph Graph => _Graph;

	public EnExecutionPlanType ExecutionPlanType => _ExecutionPlanType;

	public object Context => _Context;

	public NodeBuilderContext(ExecutionPlanGraph graph, EnExecutionPlanType type, object context)
	{
		_Graph = graph;
		_ExecutionPlanType = type;
		_Context = context;
	}
}
