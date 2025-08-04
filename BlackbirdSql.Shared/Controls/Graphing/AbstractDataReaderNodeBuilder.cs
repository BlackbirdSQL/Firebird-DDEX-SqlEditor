// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.DataReaderNodeBuilder
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using BlackbirdSql.Shared.Controls.Graphing.Enums;
using BlackbirdSql.Shared.Controls.Graphing.Interfaces;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Controls.Graphing;

internal abstract class AbstractDataReaderNodeBuilder : INodeBuilder
{
	private static readonly Regex operatorReplaceExpression = new Regex("[ \\-]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	private static readonly Regex argumentObjectExpression = new Regex("OBJECT:\\((?<Object>[^\\)]*)\\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	protected abstract int NodeIdIndex { get; }

	protected abstract int ParentIndex { get; }

	protected abstract EnExecutionPlanType ExecutionPlanType { get; }

	public AbstractDataReaderNodeBuilder()
	{
	}

	internal ExecutionPlanGraph[] Execute(object dataSource)
	{
		if (dataSource is not IDataReader dataReader)
		{
			throw new ArgumentException(ControlsResources.ExUnknownExecutionPlanSource);
		}
		List<ExecutionPlanGraph> list = new List<ExecutionPlanGraph>();
		Dictionary<int, Node> dictionary = null;
		NodeBuilderContext nodeBuilderContext = null;
		object[] values = null;
		string[] propertyNames = GetPropertyNames();
		while (dataReader.Read())
		{
			ReadValues(dataReader, ref values);
			int num = (int)values[NodeIdIndex];
			int num2 = (int)values[ParentIndex];
			Node node = null;
			if (num2 == 0)
			{
				if (nodeBuilderContext != null)
				{
					list.Add(nodeBuilderContext.Graph);
				}
				nodeBuilderContext = new NodeBuilderContext(new ExecutionPlanGraph(), ExecutionPlanType, this);
				dictionary = new Dictionary<int, Node>();
			}
			else
			{
				node = dictionary[num2];
			}
			Node node2 = CreateNode(num, nodeBuilderContext);
			ParseProperties(node2, propertyNames, values);
			SetNodeSpecialProperties(node2);
			node?.Children.Add(node2);
			if (!dictionary.ContainsKey(num))
			{
				dictionary.Add(num, node2);
			}
		}
		if (nodeBuilderContext != null)
		{
			list.Add(nodeBuilderContext.Graph);
		}
		return list.ToArray();
	}

	protected abstract string[] GetPropertyNames();

	private void ReadValues(IDataReader reader, ref object[] values)
	{
		if (values == null || reader.FieldCount != values.Length)
		{
			values = new object[reader.FieldCount];
		}
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = reader.GetValue(i);
		}
	}

	private void ParseProperties(Node node, string[] names, object[] values)
	{
		int num = Math.Min(names.Length, values.Length);
		for (int i = 0; i < num; i++)
		{
			if (names[i] != null && values[i] is not DBNull && values[i] != null)
			{
				node[names[i]] = values[i];
			}
		}
	}

	private static void SetNodeSpecialProperties(Node node)
	{
		node.SubtreeCost = GetNodeSubtreeCost(node);
		string text = (string)node["StatementType"];
		Operation operation;
		if (string.Compare(text, "PLAN_ROW", StringComparison.OrdinalIgnoreCase) != 0)
		{
			operation = OperationTable.GetStatement(text);
			node["LogicalOp"] = operation.DisplayName;
			node["PhysicalOp"] = operation.DisplayName;
			node["Argument"] = node["StatementText"];
		}
		else
		{
			PropertyDescriptor propertyDescriptor = node.Properties["StatementText"];
			if (propertyDescriptor != null)
			{
				node.Properties.Remove(propertyDescriptor);
			}
			if (node["Argument"] is string input)
			{
				Match match = argumentObjectExpression.Match(input);
				if (match != Match.Empty)
				{
					node["Object"] = match.Groups["Object"].Value;
				}
			}
			if (node["PhysicalOp"] is not string text2 || node["LogicalOp"] is not string text3)
			{
				throw new FormatException(ControlsResources.ExUnknownExecutionPlanSource);
			}
			text2 = operatorReplaceExpression.Replace(text2, "");
			text3 = operatorReplaceExpression.Replace(text3, "");
			Operation physicalOperation = OperationTable.GetPhysicalOperation(text2);
			Operation logicalOperation = OperationTable.GetLogicalOperation(text3);
			operation = ((logicalOperation != null && logicalOperation.Image != null && logicalOperation.Description != null) ? logicalOperation : physicalOperation);
			node["LogicalOp"] = logicalOperation.DisplayName;
			node["PhysicalOp"] = physicalOperation.DisplayName;
			if (node["EstimateRebinds"] != null && node["EstimateRewinds"] != null)
			{
				double num = (double)node["EstimateRebinds"];
				double num2 = (double)node["EstimateRewinds"];
				node["EstimateExecutions"] = num + num2 + 1.0;
			}
			double num3 = ((node["EstimateRows"] == null) ? 0.0 : Convert.ToDouble(node["EstimateRows"]));
			double num4 = ((node["EstimateExecutions"] == null) ? 0.0 : Convert.ToDouble(node["EstimateExecutions"]));
			if (node["ActualExecutions"] != null)
			{
				Convert.ToDouble(node["ActualExecutions"]);
			}
			node["EstimateRowsAllExecs"] = num3 * num4;
		}
		node.Operation = operation;
	}

	protected virtual Node CreateNode(int nodeId, NodeBuilderContext context)
	{
		return new Node(nodeId, context);
	}

	private static double GetNodeSubtreeCost(Node node)
	{
		object obj = node["TotalSubtreeCost"];
		if (obj == null)
		{
			return 0.0;
		}
		return Convert.ToDouble(obj, CultureInfo.CurrentCulture);
	}
}
