// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.XmlPlanParser
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Controls.Graphing.Parsers;

internal abstract class AbstractXmlPlanParser : AbstractObjectParser
{
	public static void Parse(object item, object parentItem, Node parentNode, NodeBuilderContext context)
	{
		AbstractXmlPlanParser parser = XmlPlanParserFactory.GetParser(item.GetType());
		if (parser != null)
		{
			Node node;
			if (parser.ShouldParseItem(item))
			{
				node = parser.GetCurrentNode(item, parentItem, parentNode, context);
				if (node != null)
				{
					if (context != null && context.Graph != null && !context.Graph.NodeStmtMap.ContainsKey(node))
					{
						context.Graph.NodeStmtMap.Add(node, item);
					}
					parser.ParseProperties(item, node.Properties, context);
				}
			}
			else
			{
				node = parentNode;
			}
			foreach (object child in parser.GetChildren(item))
			{
				Parse(child, item, node, context);
			}
			if (node != parentNode)
			{
				parser.SetNodeSpecialProperties(node);
				parentNode?.Children.Add(node);
			}
			return;
		}
		throw new InvalidOperationException(ControlsResources.UnexpectedRunType);
	}

	public abstract Node GetCurrentNode(object item, object parentItem, Node parentNode, NodeBuilderContext context);

	public virtual IEnumerable GetChildren(object parsedItem)
	{
		return EnumerateChildren(parsedItem);
	}

	public virtual IEnumerable<FunctionTypeItem> ExtractFunctions(object parsedItem)
	{
		yield break;
	}

	protected virtual bool ShouldParseItem(object parsedItem)
	{
		return true;
	}

	protected virtual void SetNodeSpecialProperties(Node node)
	{
		node.Operation ??= GetNodeOperation(node);
		node.SubtreeCost = GetNodeSubtreeCost(node);
		if (node["EstimateRebinds"] != null && node["EstimateRewinds"] != null)
		{
			double num = (double)node["EstimateRebinds"];
			double num2 = (double)node["EstimateRewinds"];
			node["EstimateExecutions"] = num + num2 + 1.0;
		}
		double num3 = node["EstimateRows"] == null ? 0.0 : Convert.ToDouble(node["EstimateRows"]);
		double num4 = node["EstimateExecutions"] == null ? 0.0 : Convert.ToDouble(node["EstimateExecutions"]);
		if (node["ActualExecutions"] != null)
		{
			_ = ((RunTimeCounters)node["ActualExecutions"]).TotalCounters;
		}
		node["EstimateRowsAllExecs"] = num3 * num4;
	}

	protected override bool ShouldSkipProperty(PropertyDescriptor property)
	{
		Type type = property.PropertyType;
		if (type.IsArray)
		{
			type = type.GetElementType();
		}
		return XmlPlanParserFactory.GetParser(type) != null;
	}

	protected virtual Operation GetNodeOperation(Node node)
	{
		throw new InvalidOperationException();
	}

	protected virtual double GetNodeSubtreeCost(Node node)
	{
		throw new InvalidOperationException();
	}

	public static IEnumerable EnumerateChildren(object parsedItem)
	{
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(parsedItem))
		{
			if (Type.GetTypeCode(property.PropertyType) != TypeCode.Object)
			{
				continue;
			}
			object value = property.GetValue(parsedItem);
			if (value == null)
			{
				continue;
			}
			if (value is IEnumerable numerator)
			{
				foreach (object item in numerator)
				{
					if (XmlPlanParserFactory.GetParser(item.GetType()) != null)
					{
						yield return item;
					}
				}
			}
			else if (XmlPlanParserFactory.GetParser(value.GetType()) != null)
			{
				yield return value;
			}
		}
	}

	public static Node NewNode(NodeBuilderContext context)
	{
		return new Node((context.Context as XmlPlanNodeBuilder).GetCurrentNodeId(), context);
	}
}
