
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Model.Enums;
using Microsoft.VisualStudio.Shell;

using static BlackbirdSql.Core.Ctl.CoreConstants;


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
[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread")]
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
	/// Generates a unique DatasetKey or DatasetId from the proposedConnectionName
	/// or proposedDatasetId. At most one may be specified, the other null. If both are
	/// null the readonly Dataset property will be used.
	/// </summary>
	/// <param name="proposedConnectionName">
	/// The proposed ConnectionName property if specified in the csa, else null.
	/// </param>
	/// <param name="proposedDatasetId">
	/// The proposed DatasetId property if specified in the csa, else null.
	/// </param>
	/// <param name="dataSource">
	/// The DataSource (server name) property in the csa.
	/// </param>
	/// <param name="dataset">
	/// The readonly Dataset property of the csa.
	/// </param>
	/// <param name="connectionUrl">
	/// The readonly SafeDatasetMoniker property of the csa.
	/// </param>
	/// <returns>
	/// A tuple of a boolean indicating whether or not a new connection would be created
	/// for Item1 and the final unique datasetKey for Item2.
	/// For Item3 and Item4 the proposed ConnectionName or unique proposed DatasetId
	/// if either overrides the provided values else null or string.Empty if the
	/// proposed value should be removed.
	/// At most only one of 3 and 4 will contain a value. If either contains a value the user
	/// should be warned that the proposed value will be overridden with a unique value.
	/// Item5 returns the connection to be modified if the site properties refer to
	/// another unique connection (changedTargetDatasetKey).
	/// </returns>
	// ---------------------------------------------------------------------------------
	public override (bool, string, string, string, string) GenerateUniqueDatasetKey(string proposedConnectionName,
		string proposedDatasetId, string dataSource, string dataset, string connectionUrl, string originalConnectionUrl)
	{
		// These are the 5 values in the tuple to be returned.
		bool rNewConnection = true;
		string rUniqueDatasetKey = null;
		string rUniqueConnectionName = null;
		string rUniqueDatasetId = null;
		string rChangedTarget = null;


		int connectionIndex = -1;

		if (proposedConnectionName == string.Empty)
			proposedConnectionName = null;

		if (proposedDatasetId == string.Empty)
			proposedDatasetId = null;


		// Get the index if the connection already exists.
		if (TryGetHybridInternalRowValue(connectionUrl, out DataRow row))
		{
			rNewConnection = false;
			connectionIndex = Convert.ToInt32(row["Id"]);

			if (originalConnectionUrl != null && originalConnectionUrl != connectionUrl)
				rChangedTarget = (string)(row[CoreConstants.C_KeyExDatasetKey]);
		}

		bool proposedDatasetIdIsDerived = false;
		bool proposedConnectionNameIsDerived = false;


		// For brevity, if there's a proposed DatasetId and it's the same as the dataSet,
		// it's not needed.
		if (proposedDatasetId != null && proposedDatasetId == dataset)
		{
			proposedDatasetIdIsDerived = true;
			proposedDatasetId = null;
		}


		// The derived datasetId will form the basis of the datasetId part of an auto-generated key.
		string derivedDatasetId = proposedDatasetId ?? dataset;

		// If there's a proposed ConnectionName and it's the same as the derived ConnectionName,
		// it's also not needed.
		if (proposedConnectionName != null && proposedConnectionName == CsbAgent.C_DatasetKeyFmt.FmtRes(dataSource, derivedDatasetId))
		{
			proposedConnectionNameIsDerived = true;
			proposedConnectionName = null;
		}


		// The derived prefix is the above stripped of any unique suffix. We're going to brute force
		// this instead of a regex.
		string derivedDatasetIdPrefix = derivedDatasetId;

		string suffix;
		// pos has to be past first char and before last char
		int pos = derivedDatasetId.IndexOf('_');

		if (pos > 0 && pos < derivedDatasetId.Length - 1)
		{
			for (int i = 1; i < 1000; i++)
			{
				suffix = "_" + i;

				if (derivedDatasetId.EndsWith(suffix))
				{
					derivedDatasetIdPrefix = derivedDatasetId.TrimSuffix(suffix);
					break;
				}
			}

		}


		// It's always preferable to propose a datasetId.
		// If there's a proposed DatasetKey (ConnectionName), it takes precedence, so ensure uniqueness.

		if (proposedConnectionName != null)
		{
			// The proposed prefix is the above stripped of any unique suffix. We're going to brute force
			// this instead of a regex.
			string proposedConnectionNamePrefix = proposedConnectionName;

			// pos has to be past first char and before last char
			pos = proposedConnectionName.IndexOf('_');

			if (pos > 0 && pos < proposedConnectionName.Length - 1)
			{
				for (int i = 1; i < 1000; i++)
				{
					suffix = "_" + i;

					if (proposedConnectionName.EndsWith(suffix))
					{
						proposedConnectionNamePrefix = proposedConnectionName.TrimSuffix(suffix);
						break;
					}
				}

			}


			// Establish a unique DatasetKey using i as the suffix.
			// This loop will execute at least once.
			for (int i = 0; i <= Count; i++)
			{
				rUniqueConnectionName = i == 0
					? proposedConnectionName
					: (proposedConnectionNamePrefix + $"_{i + 1}");

				if (!TryGetEntry(rUniqueConnectionName, out int index))
					break;

				if (connectionIndex > -1 && connectionIndex == index)
					break;
			}
			rUniqueDatasetKey = rUniqueConnectionName;
		}
		else
		{
			// Establish a unique DatasetId using i as the suffix.
			// This loop will execute at least once.

			for (int i = 0; i <= Count; i++)
			{
				rUniqueDatasetId = i == 0
					? derivedDatasetId
					: (derivedDatasetIdPrefix + $"_{i + 1}");

				rUniqueDatasetKey = CsbAgent.C_DatasetKeyFmt.FmtRes(dataSource, rUniqueDatasetId);

				if (!TryGetEntry(rUniqueDatasetKey, out int index))
					break;

				if (connectionIndex > -1 && connectionIndex == index)
					break;
			}
		}

		if (proposedConnectionName != null && proposedConnectionName == rUniqueConnectionName)
			rUniqueConnectionName = null;

		if (proposedDatasetId != null && proposedDatasetId == rUniqueDatasetId)
			rUniqueDatasetId = null;

		if (rUniqueConnectionName == null && proposedConnectionNameIsDerived)
			rUniqueConnectionName = string.Empty;
		if (rUniqueDatasetId == null && proposedDatasetIdIsDerived)
			rUniqueDatasetId = string.Empty;


		// Tracer.Trace(GetType(), "GenerateUniqueDatasetKey()", "DataSource: {0}, Dataset: {1}, proposedConnectionName: {2},
		//	proposedDatasetId: {3}, newConnection: {4}, uniqueDatasetKey: {5}, uniqueConnectionName: {6}, uniqueDatasetId: {7}",
		//	dataSource ?? "Null", dataset ?? "Null", proposedConnectionName ?? "Null", proposedDatasetId ?? "Null",
		//	newConnection, uniqueDatasetKey ?? "Null", uniqueConnectionName ?? "Null", uniqueDatasetId ?? "Null");

		return (rNewConnection, rUniqueDatasetKey, rUniqueConnectionName, rUniqueDatasetId, rChangedTarget);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Attempts to update a registered connection if it exists else returns null.
	/// </summary>
	/// <param name="connectionString">The ConnectionString for locating the configured
	/// connection.</param>
	/// <param name="source">The source requesting the update.</param>
	/// <returns>
	/// A tuple (bool, CsbAgent) where: Item1 indicates whether or not the returned csa
	/// created from the provided connection string needed to be modified and Item2
	/// contains the modified or unchanged CsbAgent, or null if no connection exists.
	/// </returns>
	/// <remarks>
	/// UpdateRegisteredConnection will always return an updated csa, if it was updated,
	/// or the original created from the provided connection string. If Item1 of the
	/// returned tuple is true, callers must ensure they update their internal
	/// connection objects with the new property values.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public override CsbAgent UpdateRegisteredConnection(string connectionString, EnConnectionSource source, bool forceOwnership)
	{
		if (_InternalConnectionsTable == null)
			return null;

		if (connectionString == null)
		{
			ArgumentNullException ex = new(nameof(connectionString));
			Diag.Dug(ex);
			throw ex;
		}

		CsbAgent csa = new(connectionString);
		// CsbAgent csaOriginal = (CsbAgent)csa.Clone();

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
		EnConnectionSource destinationSource = (EnConnectionSource)(int)row[C_KeyExConnectionSource];

		bool canTakeOwnerShip = forceOwnership || VerifyUpdateRights(source, destinationSource);

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
				csa.DatasetKey = CsbAgent.C_DatasetKeyFmt.FmtRes(csa.DataSource, csa.DatasetId);
			}
			else
			{
				csa.DatasetKey = CsbAgent.C_DatasetKeyFmt.FmtRes(csa.DataSource, csa.Dataset);
			}
		}

		int rowId = Convert.ToInt32(row["Id"]);
		string rowDatasetKey = (string)row["DatasetKey"];

		object rowObject = row["ConnectionName"];
		string rowConnectionName = rowObject != null && rowObject != DBNull.Value ? (string)rowObject : null;
		rowObject = row["DatasetId"];
		string rowDatasetId = rowObject != null && rowObject != DBNull.Value ? (string)rowObject : null;

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

		if (!csa.ContainsKey(C_KeyExConnectionSource) || csa.ConnectionSource != (canTakeOwnerShip ? source : destinationSource))
			csa.ConnectionSource = (canTakeOwnerShip ? source : destinationSource);

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


			// bool updated = !CsbAgent.AreEquivalent(csa, csaOriginal, CsbAgent.DescriberKeys);

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
		if (owner == EnConnectionSource.Unknown || updater <= owner)
			return true;

		return false;
	}


	#endregion Methods

}
