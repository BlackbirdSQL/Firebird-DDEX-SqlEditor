
using System;
using System.Data;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Enums;
using static BlackbirdSql.CoreConstants;
using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//									AbstractRunningConnectionTable Class
//
/// <summary>
/// Holds a database of volatile and non-volatile configured connections for a session.
/// Access to this class set should be through the <see cref="RctManager"/> only.
/// The Rct classes do not manage access availibility and deadlock prevention. That is the responsibility
/// of the <see cref="RctManager"/>.
/// </summary>
/// <remarks>
/// This abstract class deals specifically with the managing of preconfigured and volatile data.
/// The abstruse class deals specifically with the initial loading of preconfigured connections.
/// The final class <see cref="RunningConnectionTable"/> deals specifically with IDictionary and DataTable
/// accessor handling of registered connection configurations.
/// Connections are distinct by their equivalency connection properties as defined in the BlackbirdSql user
/// options. No further distinction takes place.
/// The SE and SqlEditor(Session) are now fully synchronized. If SqlEditor creates a new connection,
/// it will be added to the SE unless 'Add New Connections To Server Explorer' is unchecked.
/// If the DatasetKey for a connection is changed, the old name becomes a synonym for the duration of the
/// session.
/// Once a synonym is added it cannot be used by another unique connection within the same
/// Solution/Application session.
/// Deleting an SE connection will convert it to a volatile session connection
/// in the Rct.
/// </remarks>
// =========================================================================================================
internal abstract class AbstractRunningConnectionTable : AbstruseRunningConnectionTable
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractRunningConnectionTable
	// ---------------------------------------------------------------------------------


	protected AbstractRunningConnectionTable() : base()
	{
	}



	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields and Constants - AbstractRunningConnectionTable
	// =========================================================================================================


	private int _EventCardinal = 0;


	#endregion Fields and Constants





	// =========================================================================================================
	#region Property accessors - AbstractRunningConnectionTable
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if either the sync or async tasks are in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool InternalShutdownState
	{
		get
		{
			return _Instance == null
				|| _SyncPayloadLauncherLaunchState == EnLauncherPayloadLaunchState.Shutdown
				|| _AsyncPayloadLauncherLaunchState == EnLauncherPayloadLaunchState.Shutdown;
		}
		set
		{
			if (value)
			{
				_SyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;
				_AsyncPayloadLauncherLaunchState = EnLauncherPayloadLaunchState.Shutdown;
			}
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractRunningConnectionTable
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Generates a unique DatasetKey (ConnectionName) or DatasetName (DatabaseName)
	/// from the proposedConnectionName or proposedDatasetName, usually supplied by a
	/// connection dialog's underlying site or csa, or from a connection rename.
	/// At most one should be specified. If both are specified there will be a
	/// redundancy check of the connection name otherwise the connection name takes
	/// precedence.
	/// If both are null the proposed derivedDatasetName will be derived from the
	/// dataSource.
	/// </summary>
	/// <param name="connectionSource">The ConnectionSource making the request.</param>
	/// <param name="proposedConnectionName">
	/// The proposed DatasetKey (ConnectionName) property else null.
	/// </param>
	/// <param name="proposedDatasetName">
	/// The proposed DatasetName (DatabaseName) property else null.
	/// </param>
	/// <param name="dataSource">
	/// The DataSource (server name) property to be used in constructing the DatasetKey.
	/// </param>
	/// <param name="dataset">
	/// The readonly Dataset property to be used in constructing a DatasetName if the
	/// proposed DatasetName is null.
	/// </param>
	/// <param name="connectionUrl">
	/// The readonly SafeDatasetMoniker property of the underlying csa of the caller.
	/// </param>
	/// <param name="storedConnectionUrl">
	/// If a stored connection is being modified, the connectionUrl of the stored
	/// connection, else null.
	/// If storedConnectionUrl matches connectionUrl they will be considered equal and
	/// it will be ignored.
	/// </param>
	/// <param name="createServerExplorerConnection">
	/// Indicates wether or not the connection will be added as an SE Connection internally
	/// if it does not exist.
	/// </param>
	/// <param name="outStoredConnectionSource">
	/// Out | The ConnectionSource of connectionUrl if the connection exists in the rct
	/// else EnConnectionSource.None.
	/// </param>
	/// <param name="outChangedTargetDatasetKey">
	/// Out | If a connection is being modified (storedConnectionUrl is not null) and
	/// connectionUrl points to an existing connection, then the target has changed and
	/// outChangedTargetDatasetKey refers to the changed target's DatasetKey, else null.
	/// </param>
	/// <param name="outUniqueDatasetKey">Out | The final unique DatasetKey.</param>
	/// <param name="outUniqueConnectionName">
	/// Out | The unique resulting proposed ConnectionName. If null is returned then
	/// whatever was provided in proposedConnectionName is correct and remains as is. If
	/// "" is returned then whatever was provided in proposedConnectionName is good but
	/// changes the existing name. If a value is returned then proposedConnectionName
	/// was ambiguous and outUniqueConnectionName must be used in it's place.
	/// outUniqueConnectionName and outUniqueDatasetName are mutually exclusive.
	/// </param>
	/// <param name="outUniqueDatasetName">
	/// Out | The unique resulting proposed DatsetId. If null is returned then whatever
	/// was provided in proposedDatasetName is correct and remains as is. If "" is
	/// returned then whatever was provided in proposedDatasetName is good but changes the
	/// existing name. If a value is returned then proposedDatasetName was ambiguous and
	/// outUniqueDatasetName must be used in it's place.
	/// outUniqueConnectionName and outUniqueDatasetName are mutually exclusive.
	/// </param>
	/// <returns>
	/// A boolean indicating whether or not an Rct connection exists for the provided
	/// arguments.
	/// </returns>
	// ---------------------------------------------------------------------------------
	protected override bool GenerateUniqueDatasetKey(EnConnectionSource connectionSource,
		ref string proposedConnectionName, ref string proposedDatasetName, string dataSource,
		string dataset, string connectionUrl, string storedConnectionUrl,
		ref bool createServerExplorerConnection, out EnConnectionSource outStoredConnectionSource,
		out string outChangedTargetDatasetKey, out string outUniqueDatasetKey,
		out string outUniqueConnectionName, out string outUniqueDatasetName)
	{
		bool retRctExists = false;

		// These are the 5 values in the tuple to be returned except .
		outStoredConnectionSource = EnConnectionSource.Unknown;
		outUniqueConnectionName = null;
		outUniqueDatasetName = null;
		outChangedTargetDatasetKey = null;
		// and outUniqueDatasetKey

		int connectionIndex = -1;

		if (string.IsNullOrWhiteSpace(proposedConnectionName))
			proposedConnectionName = null;

		if (string.IsNullOrWhiteSpace(proposedDatasetName))
			proposedDatasetName = null;


		if (storedConnectionUrl != null && storedConnectionUrl == connectionUrl)
			storedConnectionUrl = null;

		string storedDatasetKey = null;
		string storedConnectionName = null;
		string storedDatasetName = null;

		// Get the index if the connection already exists.
		if (InternalTryGetHybridRowValue(connectionUrl, EnRctKeyType.ConnectionUrl, out DataRow row))
		{
			retRctExists = true;

			connectionIndex = Convert.ToInt32(row["Id"]);

			outStoredConnectionSource = (EnConnectionSource)(int)row[C_KeyExConnectionSource];

			storedDatasetKey = Cmd.IsNullValueOrEmpty(row[C_KeyExDatasetKey]) ? null : row[C_KeyExDatasetKey].ToString();
			storedConnectionName = Cmd.IsNullValueOrEmpty(row[C_KeyExConnectionName]) ? null : row[C_KeyExConnectionName].ToString();
			storedDatasetName = Cmd.IsNullValueOrEmpty(row[C_KeyExDatasetName]) ? null : row[C_KeyExDatasetName].ToString();

			// Notify caller that the proposed settings apply to a different connection.
			if (storedConnectionUrl != null)
				outChangedTargetDatasetKey = (string)row[C_KeyExDatasetKey];

			// If the connection is already ServerExplorer, do not create.
			createServerExplorerConnection &= outStoredConnectionSource != EnConnectionSource.ServerExplorer;
		}

		// If the connection source is EntityDataModel and the existing is not ServerExplorer
		// we have to add it internally.
		createServerExplorerConnection |= connectionSource == EnConnectionSource.EntityDataModel || connectionSource == EnConnectionSource.DataSource
			&& outStoredConnectionSource != EnConnectionSource.ServerExplorer;



		// It's always preferable to propose a datasetName.
		// If there's a proposed DatasetKey (ConnectionName), it takes precedence, so ensure uniqueness.
		string derivedDatasetName;

		// First up see if we can convert a proposedConnectionName to it's datasetName equivalent.
		// We can't do this when connections loading and their configurations are fixed.
		if (proposedConnectionName != null && !InternalLoading)
		{
			derivedDatasetName = GetDerivedDatasetNameFromConnectionName(dataSource, proposedConnectionName);

			if (!string.IsNullOrEmpty(derivedDatasetName))
			{
				proposedDatasetName = derivedDatasetName;
				proposedConnectionName = null;

				storedDatasetName ??= GetDerivedDatasetNameFromConnectionName(dataSource, storedConnectionName);
				storedConnectionName = null;
			}
		}

		if (proposedConnectionName != null)
		{
			if (retRctExists && storedConnectionName != null && storedConnectionName == proposedConnectionName)
			{
				// If the connection exists and the proposed connection name == the existing stored name
				// then we don't need to generate a unique name.
				outUniqueConnectionName = storedConnectionName;
			}
			else
			{
				outUniqueConnectionName = GetUniqueConnectionName(proposedConnectionName, connectionIndex);
			}

			outUniqueDatasetKey = outUniqueConnectionName;

			// The proposedConnectionName is good.
			if (proposedConnectionName == outUniqueConnectionName)
			{
				// Does it change the existing?
				if (storedConnectionName != null && storedConnectionName != proposedConnectionName)
					outUniqueConnectionName = "";
				else
					outUniqueConnectionName = null;
			}
		}
		else
		{
			if (retRctExists && storedDatasetName != null && storedDatasetName == proposedDatasetName)
			{
				// If the connection exists and the proposed datasetName == the existing stored datasetName
				// then we don't need to generate a unique id.
				outUniqueDatasetKey = storedDatasetKey;
				outUniqueDatasetName = storedDatasetName;
			}
			else
			{

				// The derived datasetName will form the basis of the datasetName part of an auto-generated key.
				derivedDatasetName = proposedDatasetName ?? dataset;

				(outUniqueDatasetKey, outUniqueDatasetName) = GetUniqueDatasetName(dataSource, derivedDatasetName, connectionIndex);
			}

			// The proposedDatasetName is good.
			if (proposedDatasetName != null && proposedDatasetName == outUniqueDatasetName)
			{
				// Does it change the existing?

				storedDatasetName ??= GetDerivedDatasetNameFromConnectionName(dataSource, storedConnectionName);

				if (storedDatasetName != null && storedDatasetName != proposedDatasetName)
					outUniqueDatasetName = "";
				else
					outUniqueDatasetName = null;
			}
		}


		// Evs.Debug(GetType(), "GenerateUniqueDatasetKey()",
		//	$"DataSource: {dataSource ?? "Null"}, Dataset: {dataset ?? "Null"}, " +
		//	$"proposedConnectionName: {proposedConnectionName ?? "Null"}, " +
		//	$"proposedDatasetName: {proposedDatasetName ?? "Null"},  " +
		//	$"rNewRctConnection: {rNewRctConnection}, outUniqueDatasetKey: {outUniqueDatasetKey}, " +
		//	$"outUniqueConnectionName: {outUniqueConnectionName}, outUniqueDatasetName: {outUniqueDatasetName}.");

		return retRctExists;
	}


	/// <summary>
	/// Attempts to extract a connectionName's equivalent datasetName. If it doesn't exist
	/// and connectionName is not null then returns "" to indicate there is an
	/// existing but it's value is imaginary, otherwise returns null.
	/// </summary>
	private string GetDerivedDatasetNameFromConnectionName(string dataSource, string connectionName)
	{
		if (connectionName == null)
			return null;

		string[] split = S_DatasetKeyFormat.Fmt(dataSource, "\n").Split('\n');

		if ((split[0] == "" || connectionName.StartsWith(split[0], StringComparison.InvariantCulture))
			&& (split[1] == "" || connectionName.EndsWith(split[1], StringComparison.InvariantCulture))
			&& connectionName.Length > split[0].Length + split[1].Length)
		{
			int start = split[0].Length;
			int len = connectionName.Length - split[0].Length - split[1].Length;

			return connectionName.Substring(start, len);
		}

		return "";
	}


	/// <summary>
	/// Gets a unique connectionName given a proposedConnectionName and the row index
	/// of the stored connection the name is being applied to. If no stored connection
	/// exists index is -1.
	/// </summary>
	/// <returns>The unique connectionName</returns>
	private string GetUniqueConnectionName(string proposedConnectionName, int connectionIndex)
	{
		// Get the proposed prefix is the proposedConnectionName stripped of any unique suffix.
		// We're going to brute force this instead of a regex.

		string proposedConnectionNamePrefix = Cmd.GetUniqueIdentifierPrefix(proposedConnectionName);

		// Establish a unique DatasetKey using i as the suffix.
		// This loop will execute at least once.

		string uniqueConnectionName = null;

		for (int i = -1; i <= Count; i++)
		{
			// Try the original proposed first.
			if (i == -1)
			{
				if (proposedConnectionName == proposedConnectionNamePrefix)
					continue;

				uniqueConnectionName = proposedConnectionName;
			}
			else
			{
				uniqueConnectionName = (i == 0)
					? proposedConnectionNamePrefix
					: (proposedConnectionNamePrefix + $"_{i + 1}");
			}

			if (!TryGetEntry(uniqueConnectionName, out int index))
				break;

			if (connectionIndex > -1 && connectionIndex == index)
				break;
		}

		return uniqueConnectionName;


	}



	/// <summary>
	/// Gets a unique datasetName given the datasource/server of the connection, a
	/// proposedDatasetName and the row index of the stored connection the name is
	/// being applied to. If no stored connection exists index is -1.
	/// </summary>
	/// <returns>
	/// A tuple of the resulting unique datasetKey and the unique datasetName used to
	/// create the unique datasetKey
	/// </returns>
	private (string, string) GetUniqueDatasetName(string dataSource, string proposedDatasetName, int connectionIndex)
	{
		// Get the proposed prefix is the proposedDatasetName stripped of any unique suffix.
		// We're going to brute force this instead of a regex.

		string proposedDatasetNamePrefix = Cmd.GetUniqueIdentifierPrefix(proposedDatasetName);

		// Evs.Debug(GetType(), "GetUniqueDatasetName()",
		//	$"dataSource: {dataSource}, proposedDatasetName: {proposedDatasetName}, " +
		//	$"connectionIndex: {connectionIndex}, proposedDatasetNamePrefix: {proposedDatasetNamePrefix}.");

		// Establish a unique DatasetName using i as the suffix.
		// This loop will execute at least once.

		string uniqueDatasetName = null;
		string uniqueDatasetKey = null;

		for (int i = -1; i <= Count; i++)
		{
			// Try the original first.
			if (i == -1)
			{
				if (proposedDatasetName == proposedDatasetNamePrefix)
					continue;

				uniqueDatasetName = proposedDatasetName;
			}
			else
			{
				uniqueDatasetName = (i == 0)
					? proposedDatasetNamePrefix
					: (proposedDatasetNamePrefix + $"_{i + 1}");
			}

			uniqueDatasetKey = S_DatasetKeyFormat.Fmt(dataSource, uniqueDatasetName);

			if (!TryGetEntry(uniqueDatasetKey, out int index))
				break;

			if (connectionIndex > -1 && connectionIndex == index)
				break;
		}

		return (uniqueDatasetKey, uniqueDatasetName);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Attempts to update a registered connection if it exists else returns null.
	/// </summary>
	/// <param name="connectionString">The ConnectionString for locating the configured
	/// connection.</param>
	/// <param name="source">The source requesting the update.</param>
	/// <returns>
	/// A tuple (bool, Csb) where: Item1 indicates whether or not the returned csa
	/// created from the provided connection string needed to be modified and Item2
	/// contains the modified or unchanged Csb, or null if no connection exists.
	/// </returns>
	/// <remarks>
	/// InternalUpdateRegisteredConnection will always return an updated csa, if it was updated,
	/// or the original created from the provided connection string. If Item1 of the
	/// returned tuple is true, callers must ensure they update their internal
	/// connection objects with the new property values.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected Csb InternalUpdateRegisteredConnection(string connectionString, EnConnectionSource source, bool forceOwnership)
	{
		lock (_LockObject)
		{
			if (_InternalConnectionsTable == null)
				return null;
		}

		if (connectionString == null)
		{
			ArgumentNullException ex = new(nameof(connectionString));
			Diag.Ex(ex);
			throw ex;
		}

		Csb csa = new(connectionString);
		// Csb csaOriginal = (Csb)csa.Clone();

		string connectionUrl = csa.Moniker;

		// Evs.Debug(GetType(), "InternalUpdateRegisteredConnection()", $"Update connection string: {connectionString}.");


		// Nothing to update. New connection.
		if (!InternalTryGetHybridRowValue(connectionUrl, EnRctKeyType.ConnectionUrl, out DataRow row))
			return null;


		/*
		 * So let's establish some rules. Remember, if we're here it's a unique connection whose
		 * equivalency properties cannot change, even by the SE, otherwise it would be a new 
		 * connection.
		 * 
		 * Rules:
		 * Connections are distinct by their equivalency connection properties as defined in
		 * the BlackbirdSql user options. No further distinction takes place.
		 * The SE and SqlEditor(Session) are now fully synchronized. If SqlEditor creates a 
		 * new connection it will be added to the SE unless 'Add New Connections To Server 
		 * Explorer' is unchecked.
		 * If the DatasetKey for a connection is changed, the old name becomes a synonym for 
		 * the duration of the session.
		 * Once a synonym is added it cannot be used by another unique connection within the 
		 * same Solution/Application session.
		 */


		// We have a connection.

		// Firstly establish if we may update the stored connection.
		EnConnectionSource rowConnectionSource = (EnConnectionSource)(int)row[C_KeyExConnectionSource];

		if (source <= EnConnectionSource.Unknown)
			source = EnConnectionSource.Session;

		bool canTakeOwnerShip = forceOwnership || VerifyUpdateRights(source, rowConnectionSource);

		// Sanity check.
		csa.ValidateKeys(true, true);

		// Another sanity check.
		// Do we have a datasetkey, if not or it doesn't match the derived key, update it with
		// the connectionName, and if not update it with the datsetid, and if not with dataset.
		if (!csa.ContainsKey(C_KeyExDatasetKey) || string.IsNullOrWhiteSpace(csa.DatasetKey))
		{
			if (csa.ContainsKey(C_KeyExConnectionName) && !string.IsNullOrWhiteSpace(csa.ConnectionName))
			{
				csa.DatasetKey = csa.ConnectionName;
			}
			else
			{
				csa.DatasetKey = S_DatasetKeyFormat.Fmt(csa.ServerName, csa.DisplayDatasetName);
			}
		}

		int rowId = Convert.ToInt32(row["Id"]);
		string rowDatasetKey = (string)row["DatasetKey"];

		object rowObject = row["ConnectionName"];
		string rowConnectionName = rowObject != DBNull.Value ? rowObject?.ToString() : null;
		rowObject = row["DatasetName"];
		string rowDatasetName = rowObject != DBNull.Value ? rowObject?.ToString() : null;

		/*
		string str = $"Original Data row for DatasetKey: {csa.DatasetKey}, ConnectionKey: {csa.ConnectionKey}: ";

		string colName;

		foreach (DataColumn col in _InternalConnectionsTable.Columns)
		{
			colName = col.ColumnName;
			str += colName + ":" + (row[colName] == null ? "null" : (row[colName] == DBNull.Value ? "DBNull" : row[colName].ToString())) + ", ";
		}
		*/

		// Has the datasetkey changed.
		if (csa.DatasetKey != rowDatasetKey)
		{
			// There's been a name change request.
			// Check if the name's been registered. If it has, is it ours?

			bool itsOurDatasetKey = false;

			if (InternalTryGetHybridRowValue(csa.DatasetKey, EnRctKeyType.DatasetKey, out DataRow tmpRow))
			{
				// Is taken but perhaps it's ours.
				if (connectionUrl.Equals((string)tmpRow[C_KeyExConnectionUrl]))
				{
					itsOurDatasetKey = true;
				}
				else
				{
					// It's not ours so reset the keys to their stored originals.
					csa.DatasetKey = rowDatasetKey;

					if (rowConnectionName == null)
						csa.Remove(C_KeyExConnectionName);
					else
						csa.ConnectionName = rowConnectionName;

					if (rowDatasetName == null)
						csa.Remove(C_KeyExDatasetName);
					else
						csa.DatasetName = rowDatasetName;

					// Cleanup ConnectionName and DatasetName
					csa.ValidateKeys(true, false);
				}
			}
			else
			{
				itsOurDatasetKey = true;
			}

			if (itsOurDatasetKey)
			{
				// It's ours so add the new key as a synonym if it doesn't exist
				Insert(csa.DatasetKey, rowId, add: false);
				// Clean up the csa.
				csa.ValidateKeys(true, false);

			}
		}

		// At this point we may or may not have updated the row's keys. Now go through the remainder
		// of the values.

		// Update the source. If canTakeOwnerShip the updating source can take ownership.

		// Evs.Debug(GetType(), "InternalUpdateRegisteredConnection()", "Updateable - Updating row.");

		if (!csa.ContainsKey(C_KeyExConnectionSource) || csa.ConnectionSource != (canTakeOwnerShip ? source : rowConnectionSource))
			csa.ConnectionSource = (canTakeOwnerShip ? source : rowConnectionSource);

		// If it's owned by the SE it must have a ConnectionKey.
		if (csa.ConnectionSource == EnConnectionSource.ServerExplorer)
		{
			if (string.IsNullOrWhiteSpace(csa.ConnectionKey))
				csa.ConnectionKey = csa.DatasetKey;
		}
		else
		{
			csa.Remove(C_KeyExConnectionKey);
		}

		// Update the DataRow.

		BeginLoadData(false);


		try
		{

			// Update the stored connection row. This will also replace the old key as the
			// primary with the new DatasetKey.
			// The old key will still exist as a synonym which means that for any solution
			// session superceded keys cannot be used by another unique connection.


			bool rowUpdated = false;

			try
			{
				rowUpdated = UpdateDataRowFromCsa(row, csa);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				return null;
			}

			connectionString = csa.ConnectionString;

			if (!connectionString.Equals(row[C_KeyExConnectionString]))
			{
				BeginLoadData(true);
				rowUpdated = true;
				row.BeginEdit();
				row[C_KeyExConnectionString] = connectionString;
				row.EndEdit();
				row.AcceptChanges();
				EndLoadData();
			}

			if (rowUpdated)
				InternalInvalidate();


			// bool updated = !Csb.AreEquivalent(csa, csaOriginal, Csb.DescriberKeys);

			return csa;

		}
		finally
		{
			EndLoadData();

			// str += $"\nOriginal csa: {csaOriginal.ConnectionString}.\nNew csa: {csa.ConnectionString}.";

			// Evs.Debug(GetType(), "InternalUpdateRegisteredConnection()",
			//	$"\n _LoadDataCardinal: {_LoadDataCardinal}, _InternalDatabases==null: {_InternalDatabases == null}\n{str}");
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Verifies wether or not a source has update rights over a peristent and/or
	/// volatile stored connection, given the source and the owning source of the
	/// connecton.
	/// <summary>
	/// ServerExplorer always has rights to update a connection provided there are no
	/// equivalency conflicts. An Applicaton has full rights except over SE connections.
	/// SqlEditor(session) has rights over transient connections and ExternalUtility
	/// (FlameRobin) connections, but because it's connections are volatile the
	/// connections are discarded when a solution closes.
	/// Deleting an SE connection will convert it to a volatile session connection
	/// in the Rct.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static bool VerifyUpdateRights(EnConnectionSource updater,
		EnConnectionSource owner)
	{
		if (owner <= EnConnectionSource.Unknown || updater <= owner)
			return true;

		if ((owner == EnConnectionSource.EntityDataModel && updater == EnConnectionSource.DataSource)
			|| (owner == EnConnectionSource.DataSource && updater == EnConnectionSource.EntityDataModel))
		{
			return true;
		}

		return false;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstruseRunningConnectionTable
	// =========================================================================================================


	// -------------------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventCardinal"/> counter when execution enters an event
	/// handler to prevent recursion.
	/// </summary>
	/// <returns>
	/// Returns false if an event has already been entered else true if it is safe to enter.
	/// </returns>
	// -------------------------------------------------------------------------------------------
	protected bool EventEnter(bool test = false, bool force = false)
	{
		lock (_LockObject)
		{
			if (_EventCardinal != 0 && !force)
				return false;

			if (!test)
				_EventCardinal++;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventCardinal"/> counter that was previously incremented
	/// by <see cref="EventEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------------
	protected void EventExit()
	{
		lock (_LockObject)
		{
			if (_EventCardinal == 0)
				Diag.Ex(new InvalidOperationException(Resources.ExceptionEventsAlreadyEnabled));
			else
				_EventCardinal--;
		}
	}


	#endregion Event Handling





	// =========================================================================================================
	#region IVsHierarchyEvents Event Handling - AbstruseRunningConnectionTable
	// =========================================================================================================

	// Currently only used for debugging


	#endregion IVsHierarchyEvents Event Handling


}
