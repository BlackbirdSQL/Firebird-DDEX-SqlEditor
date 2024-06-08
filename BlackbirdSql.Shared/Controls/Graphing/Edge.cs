// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.Edge
using System;
using System.ComponentModel;
using System.Globalization;
using BlackbirdSql.Shared.Controls.Graphing.ComponentModel;
using Microsoft.AnalysisServices.Graphing;


namespace BlackbirdSql.Shared.Controls.Graphing;

public class Edge : Microsoft.AnalysisServices.Graphing.Edge
{
	private PropertyDescriptorCollection properties;

	public PropertyDescriptorCollection Properties => properties;

	public object this[string propertyName]
	{
		get
		{
			if (properties[propertyName] is not PropertyValue propertyValue)
			{
				return null;
			}
			return propertyValue.Value;
		}
		set
		{
			if (properties[propertyName] is PropertyValue propertyValue)
			{
				propertyValue.Value = value;
			}
			else
			{
				properties.Add(PropertyFactory.CreateProperty(propertyName, value));
			}
		}
	}

	public double RowSize
	{
		get
		{
			object obj = this["AvgRowSize"];
			if (obj == null)
			{
				return 0.0;
			}
			return Convert.ToDouble(obj, CultureInfo.CurrentCulture);
		}
	}

	public double RowCount
	{
		get
		{
			if (this["ActualRowsRead"] == null && this["ActualRows"] == null)
			{
				return EstimatedRowCount;
			}
			double num = 0.0;
			double num2 = 0.0;
			if (this["ActualRowsRead"] != null)
			{
				num = Convert.ToDouble(this["ActualRowsRead"].ToString(), CultureInfo.CurrentCulture);
			}
			if (this["ActualRows"] != null)
			{
				num2 = Convert.ToDouble(this["ActualRows"].ToString(), CultureInfo.CurrentCulture);
			}
			if (!(num > num2))
			{
				return num2;
			}
			return num;
		}
	}

	public double EstimatedRowCount
	{
		get
		{
			object obj = this["EstimateRows"];
			obj ??= this["StatementEstRows"];
			if (obj == null)
			{
				return 0.0;
			}
			return Convert.ToDouble(obj, CultureInfo.CurrentCulture);
		}
	}

	public double EstimatedDataSize
	{
		get
		{
			object obj = this["EstimatedDataSize"];
			if (obj == null)
			{
				return 0.0;
			}
			return Convert.ToDouble(obj, CultureInfo.CurrentCulture);
		}
		private set
		{
			this["EstimatedDataSize"] = value;
		}
	}

	public Edge(INode fromNode, INode toNode)
		: base(fromNode, toNode)
	{
		Initialize(toNode as BlackbirdSql.Shared.Controls.Graphing.Node);
	}

	private void Initialize(BlackbirdSql.Shared.Controls.Graphing.Node node)
	{
		properties = new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		string[] array = new string[6] { "ActualRows", "ActualRowsRead", "AvgRowSize", "EstimateRows", "EstimateRowsAllExecs", "StatementEstRows" };
		foreach (string propertyName in array)
		{
			object obj = node[propertyName];
			if (obj != null)
			{
				this[propertyName] = obj;
			}
		}
		EstimatedDataSize = RowSize * EstimatedRowCount;
	}
}
