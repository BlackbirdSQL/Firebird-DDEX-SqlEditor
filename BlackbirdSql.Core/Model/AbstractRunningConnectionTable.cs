
using System;
using System.Data;
using System.Reflection;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Shell;

using static BlackbirdSql.Sys.SysConstants;



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
public abstract class AbstractRunningConnectionTable : AbstruseRunningConnectionTable
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractRunningConnectionTable
	// ---------------------------------------------------------------------------------


	protected AbstractRunningConnectionTable() : base()
	{
	}


	public override void Dispose()
	{
		base.Dispose();

	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields and Constants - AbstractRunningConnectionTable
	// =========================================================================================================


	#endregion Fields and Constants




	// =========================================================================================================
	#region Property accessors - AbstractRunningConnectionTable
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if either the sync or async tasks are in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool ShutdownState
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
	/// Generates a unique DatasetKey (ConnectionName) or DatasetId (DatabaseName)
	/// from the proposedConnectionName or proposedDatasetId, usually supplied by a
	/// connection dialog's underlying site or csa, or from a connection rename.
	/// At most one should be specified. If both are specified there will be a
	/// redundancy check of the connection name otherwise the connection name takes
	/// precedence.
	/// If both are null the proposed derivedDatasetId will be derived from the dataSource.
	/// </summary>
	/// <param name="connectionSource">
	/// The ConnectionSource making the request.
	/// </param>
	/// <param name="proposedConnectionName">
	/// The proposed DatasetKey (ConnectionName) property else null.
	/// </param>
	/// <param name="proposedDatasetId">
	/// The proposed DatasetId (DatabaseName) property else null.
	/// </param>
	/// <param name="dataSource">
	/// The DataSource (server name) property to be used in constructing the DatasetKey.
	/// </param>
	/// <param name="dataset">
	/// The readonly Dataset property to be used in constructing a DatasetId if the
	/// proposed DatasetId is null.
	/// </param>
	/// <param name="connectionUrl">
	/// The readonly SafeDatasetMoniker property of the underlying csa of the caller.
	/// </param>
	/// <param name="storedConnectionUrl">
	/// If a stored connection is being modified, the connectionUrl of the stored
	/// connection, else null. If connectionUrl matches connectionUrl they will
	/// be considered equal and it will be ignored.</param>
	/// <param name="outStoredConnectionSource">
	/// Out | The ConnectionSource of connectionUrl if the connection exists in the rct
	/// else EnConnectionSource.None.
	/// </param>
	/// <param name="outChangedTargetDatasetKey">
	/// Out | If a connection is being modified (storedConnectionUrl is not null) and
	/// connectionUrl points to an existing connection, then the target has changed and
	/// outChangedTargetDatasetKey refers to the changed target's DatasetKey, else null.
	/// </param>
	/// <param name="outUniqueDatasetKey">
	/// Out | The final unique DatasetKey.
	/// </param>
	/// <param name="outUniqueConnectionName">
	/// Out | The unique resulting proposed ConnectionName. If null is returned then whatever was
	/// provided in proposedConnectionName is correct and remains as is. If string.Empty is
	/// returned then whatever was provided in proposedConnectionName is good but changes the
	/// existing name. If a value is returned then proposedConnectionName was
	/// ambiguous and outUniqueConnectionName must be used in it's place.
	/// outUniqueConnectionName and outUniqueDatasetId are mutually exclusive.
	/// </param>
	/// <param name="outUniqueDatasetId">
	/// Out | The unique resulting proposed DatsetId. If null is returned then whatever was
	/// provided in proposedDatasetId is correct and remains as is. If string.Empty is
	/// returned then whatever was provided in proposedDatasetId is good but changes the
	/// existing name. If a value is returned then proposedDatasetId was ambiguous and
	/// outUniqueDatasetId must be used in it's place. outUniqueConnectionName and
	/// outUniqueDatasetId are mutually exclusive.
	/// </param>
	/// <returns>
	/// A boolean indicating whether or not the provided arguments would cause a new
	/// connection to be registered in the rct. This only applies to registration in
	/// the rct and does not determine whether or not a new connection would be created
	/// in the SE. The caller must determine that.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public override bool GenerateUniqueDatasetKey(EnConnectionSource connectionSource,
		string proposedConnectionName, string proposedDatasetId, string dataSource,
		string dataset, string connectionUrl, string storedConnectionUrl,
		out EnConnectionSource outStoredConnectionSource,
		out string outChangedTargetDatasetKey, out string outUniqueDatasetKey,
		out string outUniqueConnectionName, out string outUniqueDatasetId)
	{
		bool rNewRctConnection = true;

		// These are the 5 values in the tuple to be returned except .
		outStoredConnectionSource = EnConnectionSource.None;
		outUniqueConnectionName = null;
		outUniqueDatasetId = null;
		outChangedTargetDatasetKey = null;
		// and outUniqueDatasetKey

		int connectionIndex = -1;

		if (string.IsNullOrWhiteSpace(proposedConnectionName))
			proposedConnectionName = null;

		if (string.IsNullOrWhiteSpace(proposedDatasetId))
			proposedDatasetId = null;

		 if (storedConnectionUrl != null && storedConnectionUrl == connectionUrl)
			storedConnectionUrl = null;

		string existingConnectionName = null;
		string existingDatasetId = null;

		// Get the index if the connection already exists.
		if (TryGetHybridInternalRowValue(connectionUrl, out DataRow row))
		{
			rNewRctConnection = false;
			connectionIndex = Convert.ToInt32(row["Id"]);

			outStoredConnectionSource = (EnConnectionSource)(int)row[C_KeyExConnectionSource];

			existingConnectionName = Cmd.IsNullValueOrEmpty(row[C_KeyExConnectionName]) ? null : row[C_KeyExConnectionName].ToString();
			existingDatasetId = Cmd.IsNullValueOrEmpty(row[C_KeyExDatasetId]) ? null : row[C_KeyExDatasetId].ToString();

			// Notify caller that the proposed settings apply to a different connection.
			if (storedConnectionUrl != null)
				outChangedTargetDatasetKey = (string)row[C_KeyExDatasetKey];
		}


		// It's always preferable to propose a datasetId.
		// If there's a proposed DatasetKey (ConnectionName), it takes precedence, so ensure uniqueness.

		if (proposedConnectionName != null)
		{
			outUniqueConnectionName = GetUniqueConnectionName(proposedConnectionName, connectionIndex);

			outUniqueDatasetKey = outUniqueConnectionName;

			// The proposedConnectionName is good.
			if (proposedConnectionName == outUniqueConnectionName)
			{
				// Does it change the existing?
				if (existingConnectionName != null && existingConnectionName != proposedConnectionName)
					outUniqueConnectionName = string.Empty;
				else
					outUniqueConnectionName = null;
			}
		}
		else
		{
			// The derived datasetId will form the basis of the datasetId part of an auto-generated key.
			string derivedDatasetId = proposedDatasetId ?? dataset;

			(outUniqueDatasetKey, outUniqueDatasetId) = GetUniqueDatasetId(dataSource, derivedDatasetId, connectionIndex);

			// The proposedDatasetId is good.
			if (proposedDatasetId != null && proposedDatasetId == outUniqueDatasetId)
			{
				// Does it change the existing?
				if (existingDatasetId != null && existingDatasetId != proposedDatasetId)
					outUniqueDatasetId = string.Empty;
				else
					outUniqueDatasetId = null;
			}
		}


		// Tracer.Trace(GetType(), "GenerateUniqueDatasetKey()", "DataSource: {0}, Dataset: {1},
		//		proposedConnectionName: {2}, proposedDatasetId: {3},  rNewConnection: {4}, rUniqueDatasetKey: {5},
		//		rUniqueConnectionName: {6}, rUniqueDatasetId: {7}", dataSource ?? "Null", dataset ?? "Null", 
		//		proposedConnectionName ?? "Null", proposedDatasetId ?? "Null", rNewConnection,
		//		rUniqueDatasetKey ?? "Null", rUniqueConnectionName ?? "Null", rUniqueDatasetId ?? "Null");

		return rNewRctConnection;
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

		string proposedConnectionNamePrefix = GetUniqueIdentifierPrefix(proposedConnectionName);

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
	/// Gets a unique datasetId given the datasource/server of the connection, a
	/// proposedDatasetId and the row index of the stored connection the name is
	/// being applied to. If no stored connection exists index is -1.
	/// </summary>
	/// <returns>
	/// A tuple of the resulting unique datasetKey and the unique datasetId used to
	/// create the unique datasetKey
	/// </returns>
	private (string, string) GetUniqueDatasetId(string dataSource, string proposedDatasetId, int connectionIndex)
	{
		// Get the proposed prefix is the proposedDatasetId stripped of any unique suffix.
		// We're going to brute force this instead of a regex.

		string proposedDatasetIdPrefix = GetUniqueIdentifierPrefix(proposedDatasetId);

		// Establish a unique DatasetId using i as the suffix.
		// This loop will execute at least once.

		string uniqueDatasetId = null;
		string uniqueDatasetKey = null;

		for (int i = -1; i <= Count; i++)
		{
			// Try the original first.
			if (i == -1)
			{
				if (proposedDatasetId == proposedDatasetIdPrefix)
					continue;

				uniqueDatasetId = proposedDatasetId;
			}
			else
			{
				uniqueDatasetId = (i == 0)
					? proposedDatasetIdPrefix
					: (proposedDatasetIdPrefix + $"_{i + 1}");
			}

			uniqueDatasetKey = DatasetKeyFormat.FmtRes(dataSource, uniqueDatasetId);

			if (!TryGetEntry(uniqueDatasetKey, out int index))
				break;

			if (connectionIndex > -1 && connectionIndex == index)
				break;
		}

		return (uniqueDatasetKey, uniqueDatasetId);

	}


	/// <summary>
	/// Gets the prefix of an identifier stripped of any unique suffix in the for '_999'.
	/// We're going to brute force this instead of a regex.
	/// </summary>
	private string GetUniqueIdentifierPrefix(string identifier)
	{
		string prefix = identifier;
		string suffix;
		// pos has to be past first char and before last char
		int pos = identifier.IndexOf('_');

		if (pos > 0 && pos < identifier.Length - 1)
		{
			for (int i = 1; i < 1000; i++)
			{
				suffix = "_" + i;

				if (identifier.EndsWith(suffix))
				{
					prefix = identifier.TrimSuffix(suffix);
					break;
				}
			}

		}

		return prefix;
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
	/// UpdateRegisteredConnection will always return an updated csa, if it was updated,
	/// or the original created from the provided connection string. If Item1 of the
	/// returned tuple is true, callers must ensure they update their internal
	/// connection objects with the new property values.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public override Csb UpdateRegisteredConnection(string connectionString, EnConnectionSource source, bool forceOwnership)
	{
		if (_InternalConnectionsTable == null)
			return null;

		if (connectionString == null)
		{
			ArgumentNullException ex = new(nameof(connectionString));
			Diag.Dug(ex);
			throw ex;
		}

		Csb csa = new(connectionString);
		// Csb csaOriginal = (Csb)csa.Clone();

		string connectionUrl = csa.SafeDatasetMoniker;

		// Tracer.Trace(GetType(), "UpdateRegisteredConnection()", "Update connection string: {0}", connectionString);


		// Nothing to update. New connection.
		if (!TryGetHybridInternalRowValue(connectionUrl, out DataRow row))
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

		if (source <= EnConnectionSource.None)
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
			else if (csa.ContainsKey(C_KeyExDatasetId) && !string.IsNullOrWhiteSpace(csa.DatasetId))
			{
				csa.DatasetKey = DatasetKeyFormat.FmtRes(csa.DataSource, csa.DatasetId);
			}
			else
			{
				csa.DatasetKey = DatasetKeyFormat.FmtRes(csa.DataSource, csa.Dataset);
			}
		}

		int rowId = Convert.ToInt32(row["Id"]);
		string rowDatasetKey = (string)row["DatasetKey"];

		object rowObject = row["ConnectionName"];
		string rowConnectionName = rowObject != DBNull.Value ? rowObject?.ToString() : null;
		rowObject = row["DatasetId"];
		string rowDatasetId = rowObject != DBNull.Value ? rowObject?.ToString() : null;

		// string str = null;

		/*
		str = $"Original Data row for DatasetKey: {csa.DatasetKey}, ConnectionKey: {csa.ConnectionKey}: ";

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

			if (TryGetHybridInternalRowValue(csa.DatasetKey, out DataRow tmpRow))
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

					if (rowDatasetId == null)
						csa.Remove(C_KeyExDatasetId);
					else
						csa.DatasetId = rowDatasetId;

					// Cleanup ConnectionName and DatasetId
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

		// Tracer.Trace(GetType(), "UpdateRegisteredConnection()", "Updateable - Updating row.");

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


			bool rowUpdated = UpdateDataRowFromCsa(row, csa);

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
				Invalidate();


			// bool updated = !Csb.AreEquivalent(csa, csaOriginal, Csb.DescriberKeys);

			return csa;

		}
		finally
		{
			EndLoadData();

			// str += $"\nOriginal csa: {csaOriginal.ConnectionString}.\nNew csa: {csa.ConnectionString}.";

			// Tracer.Trace(GetType(), "UpdateRegisteredConnection()", "\n _LoadDataCardinal: {0}, _databases==null: {1}\n{2}", _LoadDataCardinal, _Databases == null, str);
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
	public static bool VerifyUpdateRights(EnConnectionSource updater,
		EnConnectionSource owner)
	{
		if (owner <= EnConnectionSource.None || updater <= owner)
			return true;

		return false;
	}


	#endregion Methods

}
