// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.NodeBuilderFactory
using System;
using System.Data;
using BlackbirdSql.Common.Controls.Graphing.Enums;
using BlackbirdSql.Common.Controls.Graphing.Interfaces;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Controls.Graphing;

public static class NodeBuilderFactory
{
	public static INodeBuilder Create(object dataSource, EnExecutionPlanType type)
	{
		if (dataSource is string || dataSource is byte[] || dataSource is ExecutionPlanXML)
		{
			return new XmlPlanNodeBuilder(type);
		}
		if (dataSource is IDataReader)
		{
			switch (type)
			{
			case EnExecutionPlanType.Actual:
				return new ActualPlanDataReaderNodeBuilder();
			case EnExecutionPlanType.Estimated:
				return new EstimatedPlanDataReaderNodeBuilder();
			case EnExecutionPlanType.Live:
				return new LivePlanDataReaderNodeBuilder();
			}
		}
		throw new ArgumentException(ControlsResources.UnknownExecutionPlanSource);
	}
}
