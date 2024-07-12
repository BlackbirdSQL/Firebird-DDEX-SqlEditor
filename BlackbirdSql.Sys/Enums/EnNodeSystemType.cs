// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.Sys.Enums;


// ---------------------------------------------------------------------------------
/// <summary>
/// Enumerator for flagging the current SE node as 'IsSystemObject' or not
/// </summary>
// ---------------------------------------------------------------------------------
public enum EnNodeSystemType
{
	Undefined = 0,
	Global = 1,
	User = 2,
	System = 3
};


public static class EnNodeSystemTypeExtensions
{

	/// <summary>
	/// Determines the IsSystemObject type of a node.
	/// </summary>
	public static EnNodeSystemType NodeSystemType(this IVsDataExplorerNode node)
	{
		if (node == null)
			return EnNodeSystemType.Undefined;
		if (node == node.ExplorerConnection.ConnectionNode)
			return EnNodeSystemType.Global;

		IVsDataObject @object;

		if (node.Object == null)
		{
			if (node.Name.StartsWith("User"))
				return EnNodeSystemType.User;
			if (node.Name.StartsWith("System"))
				return EnNodeSystemType.System;

			if (node.Parent.Object != null)
				@object = node.Parent.Object;
			else
				return EnNodeSystemType.Global;
		}
		else
		{
			@object = node.Object;
		}


		if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
			|| @object.Type.Name.EndsWith("ReturnValue"))
		{
			if (node.Parent != null && node.Parent.Object != null)
				@object = node.Parent.Object;
			else if (node.Parent.Parent != null && node.Parent.Parent.Object != null)
				@object = node.Parent.Parent.Object;
			else
				return EnNodeSystemType.Global;

		}

		if (@object.Properties.ContainsKey("IS_SYSTEM_FLAG"))
		{
			if ((int)@object.Properties["IS_SYSTEM_FLAG"] != 0)
				return EnNodeSystemType.System;
			return EnNodeSystemType.User;
		}
		else if (@object.Properties.ContainsKey("IS_SYSTEM_VIEW"))
		{
			if ((short)@object.Properties["IS_SYSTEM_VIEW"] != 0)
				return EnNodeSystemType.System;
			return EnNodeSystemType.User;
		}

		// Tracer.Trace(typeof(EnNodeSystemTypeExtensions), "NodeSystemType()", "Node {0} has no system type flag.", node.Name);

		return EnNodeSystemType.Global;

	}
}

