// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Model;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.Core;

// =========================================================================================================
//											ExtensionMembers Class
//
/// <summary>
/// Central class for package external class extension methods. 
/// </summary>
// =========================================================================================================
static class ExtensionMembers
{

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the ExplorerConnection ConnectionKey of an IVsDataConnectionProperties Site
	/// else null if not found.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FindConnectionKey(this IVsDataConnectionProperties value)
	{
		return ApcManager.ExplorerConnectionManager.FindConnectionKey(value.ToString(), false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds a DataExplorerConnectionManager Connection's  ConnectionKey given a ConnectionString
	/// else null if not found.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FindConnectionKey(this IVsDataExplorerConnectionManager value, string connectionString, bool encrypted)
	{
		(_, IVsDataExplorerConnection explorerConnection) = value.SearchExplorerConnectionEntry(connectionString, encrypted);

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
	public static string FindConnectionKey(this IDbConnection value)
	{
		return ApcManager.ExplorerConnectionManager.FindConnectionKey(value.ConnectionString, false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ExplorerConnection ConnectionKey of an IVsDataConnectionProperties Site
	/// else throws an exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetConnectionKey(this IVsDataConnectionProperties value)
	{
		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection explorerConnection) = manager.SearchExplorerConnectionEntry(value.ToString(), false);

		if (explorerConnection == null)
		{
			COMException ex = new($"Failed to find ExplorerConnection for IVsDataConnectionProperties ConnectionString: {value.ToString()}.");
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
	public static string GetConnectionKey(this IVsDataExplorerConnection value, bool deepSearch = true)
	{
		if (value.ConnectionNode == null)
		{
			ArgumentException exa = new($"Failed to retrieve ConnectionKey. Connection Node for IVsDataExplorerConnection {value.DisplayName} is null");
			Diag.Dug(exa);
			throw exa;
		}

		string retval;
		IVsDataObject @object = (IVsDataObject)Reflect.GetFieldValueBase(value.ConnectionNode, "_object");

		COMException ex;

		if (!deepSearch && @object == null)
		{
			ex = new($"Failed to get ConnectionKey for ExplorerConnection {value.DisplayName}. ConnectionNode._object returned null");
			Diag.Dug(ex);
			throw ex;
		}

		if (@object != null)
		{
			object leaf = Reflect.GetFieldValue(@object, "_leaf");

			if (!deepSearch && leaf == null)
			{
				ex = new($"Failed to get ConnectionKey for ExplorerConnection {value.DisplayName}. ConnectionNode.Object._leaf returned null");
				Diag.Dug(ex);
				throw ex;
			}

			if (leaf != null)
			{
				retval = (string)Reflect.GetPropertyValue(leaf, "Id", BindingFlags.Instance | BindingFlags.Public);

				if (!deepSearch && retval == null)
				{
					ex = new($"Failed to get ConnectionKey for ExplorerConnection {value.DisplayName}. ConnectionNode.Object._leaf.Id returned null");
					Diag.Dug(ex);
					throw ex;
				}

				return retval;
			}
		}

		// deepSearch == true.

		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(retval, _) = manager.SearchExplorerConnectionEntry(value.EncryptedConnectionString, true);

		if (retval == null)
		{
			ex = new($"Failed to get ConnectionKey for ExplorerConnection {value.DisplayName}. No entry exists in the ExplorerConnectionManager.");
			Diag.Dug(ex);
			throw ex;
		}

		// Tracer.Trace(typeof(IVsDataExplorerConnection), "GetConnectionKey()", "Retrieved ConnectionKey: {0}.", retval);

		return retval;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ConnectionKey of node else throws an exception.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetConnectionKey(this IVsDataExplorerNode value)
	{
		return value.ExplorerConnection.GetConnectionKey();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Searches an ExplorerConnection entry from the ConnectionUrl of a provided
	/// ConnectionString.
	/// </summary>
	/// <returns>
	/// The tuple (label, explorerConnection)
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static (string, IVsDataExplorerConnection) SearchExplorerConnectionEntry(
		this IVsDataExplorerConnectionManager value, string connectionString, bool encrypted)
	{
		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;
		string unencryptedConnectionString = encrypted ? DataProtection.DecryptString(connectionString) : connectionString;

		Guid clsidProvider = new(SystemData.ProviderGuid);
		string connectionUrl = Csb.CreateConnectionUrl(unencryptedConnectionString);

		Csb csa;

		foreach (KeyValuePair<string, IVsDataExplorerConnection> pair in manager.Connections)
		{
			if (!(clsidProvider == pair.Value.Provider))
				continue;

			csa = new(pair.Value.DecryptedConnectionString(), false);

			if (csa.SafeDatasetMoniker == connectionUrl)
			{
				return (pair.Key, pair.Value);
			}
		}

		return (null, null);
	}


}
