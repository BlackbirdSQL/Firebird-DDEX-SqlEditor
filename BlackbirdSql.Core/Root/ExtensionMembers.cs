// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Model;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql;

// =========================================================================================================
//											ExtensionMembers Class
//
/// <summary>
/// Central class for package external class extension methods. 
/// </summary>
// =========================================================================================================
public static partial class ExtensionMembers
{

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the derived QualifiedName from the DecryptedConnectionString() of an
	/// <see cref="IVsDataExplorerConnection"/>. Use this in place of the
	/// ExplorerConnection's DisplayName if you do not want ViewSupport accidentally
	/// initialized.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string SafeName(this IVsDataExplorerConnection @this)
	{
		string retval = Csb.GetServerExplorerName(@this.DecryptedConnectionString());

		if (string.IsNullOrWhiteSpace(retval))
			retval = @this.ConnectionNode.Name;

		return retval;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the ExplorerConnection ConnectionKey of an IVsDataConnectionProperties Site
	/// else null if not found.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FindConnectionKey(this IVsDataConnectionProperties @this)
	{
		return ApcManager.ExplorerConnectionManager.FindConnectionKey(@this.ToString(), false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds a DataExplorerConnectionManager Connection's  ConnectionKey given a ConnectionString
	/// else null if not found.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FindConnectionKey(this IVsDataExplorerConnectionManager @this, string connectionString, bool encrypted)
	{
		(_, IVsDataExplorerConnection explorerConnection) = @this.SearchExplorerConnectionEntry(connectionString, encrypted);

		if (explorerConnection == null)
			return null;

		return explorerConnection.GetConnectionKey();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the ExplorerConnection ConnectionKey of an IDbConnection
	/// else null if not found.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FindConnectionKey(this IDbConnection @this)
	{
		return ApcManager.ExplorerConnectionManager.FindConnectionKey(@this.ConnectionString, false);
	}

	public static IVsDataExplorerConnection ExplorerConnection(this IVsDataConnection @this, bool canBeWeakEquivalent = false)
	{
		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection explorerConnection) = manager.SearchExplorerConnectionEntry(@this.EncryptedConnectionString, true, canBeWeakEquivalent);

		return explorerConnection;
	}


	public static IVsDataExplorerConnection ExplorerConnection(this IVsDataConnectionProperties @this, bool canBeWeakEquivalent = false)
	{
		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection explorerConnection) = manager.SearchExplorerConnectionEntry(@this.ToString(), false, canBeWeakEquivalent);

		return explorerConnection;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ExplorerConnection ConnectionKey of an IVsDataConnectionProperties Site
	/// else throws an exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetConnectionKey(this IVsDataConnectionProperties @this)
	{
		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection explorerConnection) = manager.SearchExplorerConnectionEntry(@this.ToString(), false);

		if (explorerConnection == null)
		{
			COMException ex = new($"Failed to find ExplorerConnection for IVsDataConnectionProperties ConnectionString: {@this.ToString()}.");
			Diag.Dug(ex);
			throw ex;
		}

		return explorerConnection.GetConnectionKey();
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ConnectionKey  of an IVsDataExplorerConnection. 
	/// else throws a COMException if not found.
	/// Throws an ArgumentException if the XonnectionNode returns null.
	/// </summary>
	/// <param name="deepSearch">
	/// If true (the default) and the ConnectionKey cannot be found within the connection,
	/// will use the connection's ExplorerConnectionManager entry key as the ConnectionKey.
	/// </param>
	/// <exception cref="COMException" />
	// ---------------------------------------------------------------------------------
	public static string GetConnectionKey(this IVsDataExplorerConnection @this, bool deepSearch = true)
	{
		if (@this.ConnectionNode == null)
		{
			ArgumentException exa = new($"Failed to retrieve ConnectionKey. Connection Node for IVsDataExplorerConnection {@this.SafeName()} is null");
			Diag.Dug(exa);
			throw exa;
		}

		string retval;
		IVsDataObject @object = (IVsDataObject)Reflect.GetFieldValueBase(@this.ConnectionNode, "_object");

		COMException ex;

		if (!deepSearch && @object == null)
		{
			ex = new($"Failed to get ConnectionKey for ExplorerConnection {@this.SafeName()}. ConnectionNode._object returned null");
			Diag.Dug(ex);
			throw ex;
		}

		if (@object != null)
		{
			object leaf = Reflect.GetFieldValue(@object, "_leaf");

			if (!deepSearch && leaf == null)
			{
				ex = new($"Failed to get ConnectionKey for ExplorerConnection {@this.SafeName()}. ConnectionNode.Object._leaf returned null");
				Diag.Dug(ex);
				throw ex;
			}

			if (leaf != null)
			{
				retval = (string)Reflect.GetPropertyValue(leaf, "Id", BindingFlags.Instance | BindingFlags.Public);

				if (!deepSearch && retval == null)
				{
					ex = new($"Failed to get ConnectionKey for ExplorerConnection {@this.SafeName()}. ConnectionNode.Object._leaf.Id returned null");
					Diag.Dug(ex);
					throw ex;
				}

				return retval;
			}
		}

		// deepSearch == true.

		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(retval, _) = manager.SearchExplorerConnectionEntry(@this.EncryptedConnectionString, true);

		if (retval == null)
		{
			ex = new($"Failed to get ConnectionKey for ExplorerConnection {@this.SafeName()}. No entry exists in the ExplorerConnectionManager.");
			Diag.Dug(ex);
			throw ex;
		}

		// Evs.Debug(typeof(IVsDataExplorerConnection), "GetConnectionKey()", $"Retrieved ConnectionKey: {retval}.");

		return retval;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ConnectionKey of node else throws an exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetConnectionKey(this IVsDataExplorerNode @this)
	{
		return @this.ExplorerConnection.GetConnectionKey();
	}

}
