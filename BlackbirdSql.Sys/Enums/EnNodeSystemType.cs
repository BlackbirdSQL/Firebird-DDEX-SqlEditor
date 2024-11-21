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
	public static EnNodeSystemType NodeSystemType(this IVsDataExplorerNode @this)
	{
		if (@this == null)
			return EnNodeSystemType.Undefined;
		if (@this == @this.ExplorerConnection.ConnectionNode)
			return EnNodeSystemType.Global;

		IVsDataObject @object;

		if (@this.Object == null)
		{
			if (@this.Name.StartsWith("User"))
				return EnNodeSystemType.User;
			if (@this.Name.StartsWith("System"))
				return EnNodeSystemType.System;

			if (@this.Parent.Object != null)
				@object = @this.Parent.Object;
			else
				return EnNodeSystemType.Global;
		}
		else
		{
			@object = @this.Object;
		}


		if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter"))
		{
			if (@this.Parent != null && @this.Parent.Object != null)
				@object = @this.Parent.Object;
			else if (@this.Parent.Parent != null && @this.Parent.Parent.Object != null)
				@object = @this.Parent.Parent.Object;
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

		// Evs.Trace(typeof(EnNodeSystemTypeExtensions), nameof(NodeSystemType), "Node {0} has no system type flag.", node.Name);

		return EnNodeSystemType.Global;

	}
}

