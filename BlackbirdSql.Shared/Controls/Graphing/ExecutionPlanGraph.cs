// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ShowPlanGraph
using System.Collections.Generic;
using BlackbirdSql.Shared.Controls.Graphing.Enums;
using Microsoft.AnalysisServices.Graphing;


namespace BlackbirdSql.Shared.Controls.Graphing;

internal class ExecutionPlanGraph : Graph
{
	private readonly Dictionary<BlackbirdSql.Shared.Controls.Graphing.Node, object> nodeStmtMap = new Dictionary<BlackbirdSql.Shared.Controls.Graphing.Node, object>();

	internal Dictionary<BlackbirdSql.Shared.Controls.Graphing.Node, object> NodeStmtMap => nodeStmtMap;

	internal string Statement => (RootNode["StatementText"] as string) ?? (RootNode["ProcName"] as string) ?? "";

	internal int StatementId => PullIntFromRoot("StatementId");

	internal int StatementCompId => PullIntFromRoot("StatementCompId");

	internal string QueryPlanHash => RootNode["QueryPlanHash"] as string;

	internal BlackbirdSql.Shared.Controls.Graphing.Node RootNode => base.NodeCollection[0] as BlackbirdSql.Shared.Controls.Graphing.Node;

	internal static ExecutionPlanGraph[] ParseShowPlanXML(object showPlan, EnExecutionPlanType type = EnExecutionPlanType.Unknown)
	{
		return NodeBuilderFactory.Create(showPlan, type).Execute(showPlan);
	}

	private int PullIntFromRoot(string name)
	{
		string text = RootNode[name].ToString();
		if (text != null && int.TryParse(text, out int result))
		{
			return result;
		}
		return -1;
	}
}
