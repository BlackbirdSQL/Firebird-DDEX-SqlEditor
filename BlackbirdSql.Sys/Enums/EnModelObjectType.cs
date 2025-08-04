using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Sys.Enums;

internal enum EnModelObjectType
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
	Role,
	User,
	View,
	ViewColumn,
	Trigger,
	TriggerColumn,
	StoredProcedure,
	StoredProcedureParameter,
	Function,
	FunctionParameter,
	NewQuery,
	NewDesignerQuery
}



internal static class EnModelObjectTypeExtensions
{
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source node object can be altered in an
	/// IDE editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool CanAlter(this IVsDataExplorerNode @this)
	{
		if (@this != null && @this.Object != null)
		{
			if (@this.Object.Type.Name.EndsWith("Trigger")
				|| @this.ModelObjectTypeIn(EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}
		}

		return false;
	}



	internal static bool CanCopy(this IVsDataExplorerNode @this)
	{
		if (@this != null)
		{
			if (!@this.ModelObjectTypeIn(EnModelObjectType.Table,
				EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return false;
			}

			return true;
		}

		return false;
	}



	internal static bool CanExecute(this IVsDataExplorerNode @this)
	{
		if (@this != null)
		{
			if (!@this.ModelObjectTypeIn(EnModelObjectType.StoredProcedure,
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
	internal static bool CanOpen(this IVsDataExplorerNode @this)
	{
		if (@this != null && @this.Object != null)
		{
			IVsDataObject @object = @this.Object;

			if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
				|| @this.ModelObjectTypeIn(EnModelObjectType.Index, EnModelObjectType.ForeignKey))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
				{
					return true;
				}
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| @this.ModelObjectTypeIn(EnModelObjectType.Table, EnModelObjectType.View,
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
	internal static IVsDataExplorerNode DesignerNode(this IVsDataExplorerNode @this)
	{
		IVsDataExplorerNode result = null;
		IVsDataExplorerNode node = @this;

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
	internal static bool HasScript(this IVsDataExplorerNode @this)
	{
		if (@this != null && @this.Object != null)
		{
			IVsDataObject @object = @this.Object;

			if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
				|| @this.ModelObjectTypeIn(EnModelObjectType.Index, EnModelObjectType.ForeignKey))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
				{
					return true;
				}
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| @this.ModelObjectTypeIn(EnModelObjectType.Table, EnModelObjectType.View,
					EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}

		}

		return false;
	}


	internal static EnModelObjectType ModelObjectType(this IVsDataExplorerNode @this)
	{
		if (@this == null || @this.Object == null)
			return EnModelObjectType.Unknown;

		// Evs.Trace(typeof(EnModelObjectType), nameof(ModelObjectType), $"Node type for node '{@this.Name}' is {@this.Object.Type.Name.ToUpperInvariant()}.");

		return @this.Object.Type.Name.ToUpperInvariant() switch
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
			"USER" => EnModelObjectType.User,
			"ROLE" => EnModelObjectType.Role,
			"STOREDPROCEDUREPARAMETER" => EnModelObjectType.StoredProcedureParameter,
			"STOREDPROCEDURECOLUMN" => EnModelObjectType.StoredProcedureParameter,
			"FUNCTIONPARAMETER" => EnModelObjectType.FunctionParameter,
			"FUNCTIONCOLUMN" => EnModelObjectType.FunctionParameter,
			"NEWQUERY" => EnModelObjectType.NewQuery,
			"NEWSQLQUERY" => EnModelObjectType.NewQuery,
			"NEWDESIGNERQUERY" => EnModelObjectType.NewDesignerQuery,
			_ => EnModelObjectType.Unknown
		};
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if typeName exists in the values array else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool ModelObjectTypeIn(this IVsDataExplorerNode @this, params EnModelObjectType[] types)
	{
		EnModelObjectType objecttype = @this.ModelObjectType();

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
	internal static EnModelObjectType NodeBaseType(this IVsDataExplorerNode @this)
	{
		if (@this == null)
			return EnModelObjectType.Unknown;

		if (@this.Object == null)
			return EnModelObjectType.Unknown;

		if (@this.ModelObjectTypeIn(EnModelObjectType.Table, EnModelObjectType.Index,
			EnModelObjectType.ForeignKey, EnModelObjectType.View,
			EnModelObjectType.StoredProcedure, EnModelObjectType.Function,
			EnModelObjectType.Database, EnModelObjectType.User, EnModelObjectType.Role))
		{
			return @this.ModelObjectType();
		}
		else if (@this.Object.Type.Name.EndsWith("Column"))
		{
			return EnModelObjectType.Column;
		}
		else if (@this.Object.Type.Name.StartsWith("StoredProcedure"))
		{
			return EnModelObjectType.StoredProcedureParameter;
		}
		else if (@this.Object.Type.Name.StartsWith("Function"))
		{
			return EnModelObjectType.FunctionParameter;
		}
		else if (@this.Object.Type.Name.EndsWith("Trigger"))
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
	internal static IVsDataExplorerNode ScriptNode(this IVsDataExplorerNode @this)
	{
		IVsDataExplorerNode result = null;
		IVsDataExplorerNode node = @this;

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
