using BlackbirdSql.Core.Ctl.Diagnostics;
using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Core.Model.Enums;

public enum EnModelObjectType
{
	Unknown = 0,
	Column,
	Database,
	Domain,
	Table,
	Index,
	IndexColumn,
	ForeignKeyColumn,
	ForeignKey,
	View,
	ViewColumn,
	Trigger,
	TriggerColumn,
	StoredProcedure,
	StoredProcedureParameter,
	Function,
	FunctionParameter,
	NewSqlQuery,
	NewDesignerQuery
}

public static class EnModelObjectTypeExtensions
{
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source node object can be altered in an
	/// IDE editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool CanAlter(this IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			if (node.Object.Type.Name.EndsWith("Trigger")
				|| node.ModelObjectTypeIn(EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}
		}

		return false;
	}



	public static bool CanCopy(this IVsDataExplorerNode node)
	{
		if (node != null)
		{
			if (!node.ModelObjectTypeIn(EnModelObjectType.Table,
				EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return false;
			}

			return true;
		}

		return false;
	}



	public static bool CanExecute(this IVsDataExplorerNode node)
	{
		if (node != null)
		{
			if (!node.ModelObjectTypeIn(EnModelObjectType.StoredProcedure,
				EnModelObjectType.Function))
			{
				return false;
			}

			return true;
		}

		return false;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source that can be opened in an IDE
	/// editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool CanOpen(this IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
				|| node.ModelObjectTypeIn(EnModelObjectType.Index, EnModelObjectType.ForeignKey))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
				{
					return true;
				}
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| node.ModelObjectTypeIn(EnModelObjectType.Table, EnModelObjectType.View,
					EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}

		}

		return false;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the designer node (type Table or View) this node belongs to else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IVsDataExplorerNode DesignerNode(this IVsDataExplorerNode currNode)
	{
		IVsDataExplorerNode result = null;
		IVsDataExplorerNode node = currNode;

		while (node != null && node != node.ExplorerConnection.ConnectionNode)
		{
			if (node.ModelObjectTypeIn(EnModelObjectType.Table, EnModelObjectType.View))
			{
				result = node;
				break;
			}
			node = node.Parent;
		}

		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source that can be opened in an IDE
	/// editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool HasScript(this IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
				|| node.ModelObjectTypeIn(EnModelObjectType.Index, EnModelObjectType.ForeignKey))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
				{
					return true;
				}
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| node.ModelObjectTypeIn(EnModelObjectType.Table, EnModelObjectType.View,
					EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}

		}

		return false;
	}


	public static EnModelObjectType ModelObjectType(this IVsDataExplorerNode node)
	{
		if (node.Object == null)
			return EnModelObjectType.Unknown;

		// Tracer.Trace(typeof(EnModelObjectTypeExtensions), "ModelObjectType", "Node type for node '{0}' is {1}.", node.Name, node.Object.Type.Name.ToUpperInvariant());

		return node.Object.Type.Name.ToUpperInvariant() switch
		{
			"DATABASE" => EnModelObjectType.Database,
			"TABLE" => EnModelObjectType.Table,
			"STOREDPROCEDURE" => EnModelObjectType.StoredProcedure,
			"FUNCTION" => EnModelObjectType.Function,
			"TRIGGER" => EnModelObjectType.Trigger,
			"IDENTITYTRIGGER" => EnModelObjectType.Trigger,
			"STANDARDTRIGGER" => EnModelObjectType.Trigger,
			"SYSTEMTRIGGER" => EnModelObjectType.Trigger,
			"INDEX" => EnModelObjectType.Index,
			"FOREIGNKEY" => EnModelObjectType.ForeignKey,
			"VIEW" => EnModelObjectType.View,
			"COLUMN" => EnModelObjectType.Column,
			"INDEXCOLUMN" => EnModelObjectType.IndexColumn,
			"FOREIGNKEYCOLUMN" => EnModelObjectType.ForeignKeyColumn,
			"TRIGGERCOLUMN" => EnModelObjectType.TriggerColumn,
			"VIEWCOLUMN" => EnModelObjectType.ViewColumn,
			"STOREDPROCEDUREPARAMETER" => EnModelObjectType.StoredProcedureParameter,
			"STOREDPROCEDURECOLUMN" => EnModelObjectType.StoredProcedureParameter,
			"FUNCTIONPARAMETER" => EnModelObjectType.FunctionParameter,
			"FUNCTIONRETURNVALUE" => EnModelObjectType.FunctionParameter,
			"NEWQUERY" => EnModelObjectType.NewSqlQuery,
			"NEWSQLQUERY" => EnModelObjectType.NewSqlQuery,
			"NEWDESIGNERQUERY" => EnModelObjectType.NewDesignerQuery,
			_ => EnModelObjectType.Unknown
		};
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if typeName exists in the values array else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ModelObjectTypeIn(this IVsDataExplorerNode node, params EnModelObjectType[] types)
	{
		EnModelObjectType objecttype = node.ModelObjectType();

		foreach (EnModelObjectType type in types)
		{
			if (type == objecttype)
				return true;
		}

		return false;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a node's type as reflected in the IVsObjectSupport xml given the node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static EnModelObjectType NodeBaseType(this IVsDataExplorerNode node)
	{
		if (node.Object == null)
			return EnModelObjectType.Unknown;

		if (node.ModelObjectTypeIn(EnModelObjectType.Table, EnModelObjectType.Index,
			EnModelObjectType.ForeignKey, EnModelObjectType.View,
			EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
		{
			return node.ModelObjectType();
		}
		else if (node.Object.Type.Name.EndsWith("Column"))
		{
			return EnModelObjectType.Column;
		}
		else if (node.Object.Type.Name.EndsWith("Parameter")
			|| node.Object.Type.Name.EndsWith("ReturnValue"))
		{
			return EnModelObjectType.FunctionParameter;
		}
		else if (node.Object.Type.Name.EndsWith("Trigger"))
		{
			return EnModelObjectType.Trigger;
		}

		return EnModelObjectType.Unknown;
	}






	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the HasScript node this node belongs to else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IVsDataExplorerNode ScriptNode(this IVsDataExplorerNode currNode)
	{
		IVsDataExplorerNode result = null;
		IVsDataExplorerNode node = currNode;

		while (node != null && node != node.ExplorerConnection.ConnectionNode)
		{
			if (node.HasScript())
			{
				result = node;
				break;
			}
			node = node.Parent;
		}

		return result;
	}


}
