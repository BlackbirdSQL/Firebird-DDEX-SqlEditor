// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.Node
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.ComponentModel;
using BlackbirdSql.Shared.Properties;
using Microsoft.AnalysisServices.Graphing;


namespace BlackbirdSql.Shared.Controls.Graphing;

public class Node : Microsoft.AnalysisServices.Graphing.Node
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
	public enum EnCloneAccessScopeType
	{
		Primary,
		Secondary,
		Both,
		Either,
		ExactMatch,
		Local
	}

	public enum EnSTATUS
	{
		PENDING,
		RUNNING,
		FINISH
	}

	public class ChildrenCollection : NodeCollection<BlackbirdSql.Shared.Controls.Graphing.Node>
	{
		public override bool IsReadOnly => false;

		public ChildrenCollection(BlackbirdSql.Shared.Controls.Graphing.Node node)
			: base(node)
		{
		}

		public override void Add(BlackbirdSql.Shared.Controls.Graphing.Node child)
		{
			BlackbirdSql.Shared.Controls.Graphing.Edge iedge = new BlackbirdSql.Shared.Controls.Graphing.Edge(base.Owner, child);
			((INodeModify)base.Owner).StoreRelatedEdge((IEdge)iedge);
			((INodeModify)child).StoreRelatedEdge((IEdge)iedge);
			((IGraphModify)base.Owner.Graph).AddEdge(iedge);
		}
	}

	private double cost;

	private bool costCalculated;

	private double subtreeCost;

	private Operation operation;

	private readonly PropertyDescriptorCollection properties;

	private readonly ChildrenCollection children;

	private readonly string objectProperty = NodeBuilderConstants.Object;

	private readonly string _PredicateProperty = NodeBuilderConstants.LogicalOp;

	private readonly List<string> SeekOrScanPhysicalOpList = new List<string> { "IndexSeek", "TableScan", "IndexScan", "ColumnstoreIndexScan" };

	public double Cost
	{
		get
		{
			if (!costCalculated)
			{
				cost = SubtreeCost;
				foreach (BlackbirdSql.Shared.Controls.Graphing.Node child in Children)
				{
					cost -= child.SubtreeCost;
				}
				cost = Math.Max(cost, 0.0);
				costCalculated = true;
			}
			return cost;
		}
	}

	public double RelativeCost
	{
		get
		{
			double num = Root.SubtreeCost;
			if (!(num > 0.0))
			{
				return 0.0;
			}
			return Cost / num;
		}
	}

	public double SubtreeCost
	{
		get
		{
			if (subtreeCost == 0.0)
			{
				foreach (BlackbirdSql.Shared.Controls.Graphing.Node child in Children)
				{
					subtreeCost += child.SubtreeCost;
				}
			}
			return subtreeCost;
		}
		set
		{
			subtreeCost = value;
		}
	}

	public Operation Operation
	{
		get
		{
			return operation;
		}
		set
		{
			operation = value;
		}
	}

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

	public new ChildrenCollection Children => children;

	public BlackbirdSql.Shared.Controls.Graphing.Node Parent
	{
		get
		{
			INodeEnumerator parents = base.Parents;
			if (parents.MoveNext())
			{
				return parents.Current as BlackbirdSql.Shared.Controls.Graphing.Node;
			}
			return null;
		}
	}

	public BlackbirdSql.Shared.Controls.Graphing.Node Root => Graph.Nodes[0] as BlackbirdSql.Shared.Controls.Graphing.Node;

	public string LogicalOpUnlocName { get; set; }

	public string PhysicalOpUnlocName { get; set; }

	private IGraph Graph => _igraph;

	public Node(int id, NodeBuilderContext context)
		: base(context.Graph)
	{
		base.ID = id;
		properties = new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		children = new ChildrenCollection(this);
		LogicalOpUnlocName = null;
		PhysicalOpUnlocName = null;
	}

	public bool IsComputeScalarType()
	{
		if (this[NodeBuilderConstants.PhysicalOp] != null)
		{
			return ((string)this[NodeBuilderConstants.PhysicalOp]).StartsWith("ComputeScalar", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public bool IsSeekOrScanType()
	{
		if (this[NodeBuilderConstants.PhysicalOp] != null)
		{
			return SeekOrScanPhysicalOpList.Contains(PhysicalOpUnlocName);
		}
		return false;
	}

	public bool IsFinished()
	{
		EnSTATUS? sTATUS = this[NodeBuilderConstants.Status] as EnSTATUS?;
		if (sTATUS.HasValue)
		{
			return sTATUS.Value == EnSTATUS.FINISH;
		}
		return false;
	}

	public bool IsRunning()
	{
		EnSTATUS? sTATUS = this[NodeBuilderConstants.Status] as EnSTATUS?;
		if (sTATUS.HasValue)
		{
			return sTATUS.Value == EnSTATUS.RUNNING;
		}
		return false;
	}

	public bool IsLogicallyEquivalentTo(BlackbirdSql.Shared.Controls.Graphing.Node nodeToCompare, bool ignoreDatabaseName)
	{
		if (this == nodeToCompare)
		{
			return true;
		}
		if (this[_PredicateProperty] != nodeToCompare[_PredicateProperty] && (!IsSeekOrScanType() || !nodeToCompare.IsSeekOrScanType()))
		{
			return false;
		}
		if ((this[objectProperty] != null && nodeToCompare[objectProperty] == null) || (nodeToCompare[objectProperty] != null && this[objectProperty] == null))
		{
			return false;
		}
		if (this[objectProperty] != null && nodeToCompare[objectProperty] != null)
		{
			ExpandableObjectWrapper expandableObjectWrapper = (ExpandableObjectWrapper)this[objectProperty];
			ExpandableObjectWrapper expandableObjectWrapper2 = (ExpandableObjectWrapper)nodeToCompare[objectProperty];
			if (ignoreDatabaseName)
			{
				if (!CompareObjectPropertyValue((PropertyValue)expandableObjectWrapper.Properties[ControlsResources.ObjectServer], (PropertyValue)expandableObjectWrapper2.Properties[ControlsResources.ObjectServer]))
				{
					return false;
				}
				if (!CompareObjectPropertyValue((PropertyValue)expandableObjectWrapper.Properties[ControlsResources.ObjectSchema], (PropertyValue)expandableObjectWrapper2.Properties[ControlsResources.ObjectSchema]))
				{
					return false;
				}
				if (!CompareObjectPropertyValue((PropertyValue)expandableObjectWrapper.Properties[ControlsResources.ObjectTable], (PropertyValue)expandableObjectWrapper2.Properties[ControlsResources.ObjectTable]))
				{
					return false;
				}
				if (!CompareObjectPropertyValue((PropertyValue)expandableObjectWrapper.Properties[ControlsResources.ObjectAlias], (PropertyValue)expandableObjectWrapper2.Properties[ControlsResources.ObjectAlias]))
				{
					return false;
				}
				PropertyValue propertyValue = (PropertyValue)expandableObjectWrapper.Properties["CloneAccessScopeSpecified"];
				PropertyValue propertyValue2 = (PropertyValue)expandableObjectWrapper2.Properties["CloneAccessScopeSpecified"];
				if ((propertyValue == null && propertyValue2 != null) || (propertyValue != null && propertyValue2 == null))
				{
					return false;
				}
				if (propertyValue != null && propertyValue2 != null)
				{
					if ((bool)propertyValue.Value != (bool)propertyValue2.Value)
					{
						return false;
					}
					if ((bool)propertyValue.Value)
					{
						PropertyValue propertyValue3 = (PropertyValue)expandableObjectWrapper.Properties["CloneAccessScope"];
						PropertyValue propertyValue4 = (PropertyValue)expandableObjectWrapper2.Properties["CloneAccessScope"];
						if ((propertyValue3 == null && propertyValue4 != null) || (propertyValue3 != null && propertyValue4 == null))
						{
							return false;
						}
						if (propertyValue3 != null && propertyValue4 != null && (EnCloneAccessScopeType)propertyValue3.Value != (EnCloneAccessScopeType)propertyValue4.Value)
						{
							return false;
						}
					}
				}
			}
			else if (expandableObjectWrapper.DisplayName != expandableObjectWrapper2.DisplayName)
			{
				return false;
			}
		}
		return true;
	}

	private bool CompareObjectPropertyValue(PropertyValue p1, PropertyValue p2)
	{
		if ((p1 == null && p2 != null) || (p1 != null && p2 == null))
		{
			return false;
		}
		if (p1 != null && p2 != null)
		{
			string strA = p1.Value as string;
			string strB = p2.Value as string;
			if (string.Compare(strA, strB, StringComparison.Ordinal) != 0)
			{
				return false;
			}
		}
		return true;
	}
}
