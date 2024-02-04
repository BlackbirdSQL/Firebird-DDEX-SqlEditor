
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
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
	/// Generates a unique DatasetKey (ConnectionName) or DatasetId (DatabaseName)
	/// from the proposedConnectionName or proposedDatasetId, usually supplied by a
	/// connection dialog's underlying site or csa, or from a connection rename.
	/// At most one should be specified. If both are specified there will be a
	/// redundancy check of the connection name otherwise the connection name takes
	/// precedence.
	/// If both are null the caller's data will be considered corrupted and rebuilt
	/// either from the stored connection if it exists else from the DataSource and
	/// readonly Dataset arguments.
	/// </summary>
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
	/// The readonly SafeDatasetMoniker property of the underlying csa.
	/// </param>
	/// <param name="originalConnectionUrl">
	/// If a stored connection is being modified, the connectionUrl of the stored
	/// connection, else null. If originalConnectionUrl matches connectionUrl they will
	/// be considered equal and it will be ignored.</param>
	/// <param name="oStoredConnectionSource">
	/// Out | The ConnectionSource of connectionUrl if the connection exists in the rct
	/// else EnConnectionSource.Unknown.
	/// </param>
	/// <param name="oChangedTargetDatasetKey">
	/// Out | If a connection is being modified (originalConnectionUrl is not null) and
	/// connectionUrl points to an existing connection, then the target has changed and
	/// changedTargetDatasetKey refers to the changed target's DatasetKey, else null.
	/// </param>
	/// <param name="oUniqueDatasetKey">Out | The final unique DatasetKey.</param>
	/// <param name="oUniqueConnectionName">
	/// Out | The unique proposed ConnectionName. If null is returned then whatever was
	/// provided in proposedConnectionName is correct remains as is. If string.Empty is
	/// returned then whatever was provided in proposedConnectionName is redundant and
	/// should be removed. If a value is returned then proposedConnectionName was
	/// ambiguous and uniqueConnectionName must be used in it's place.
	/// uniqueConnectionName and uniqueDatasetId are mutually exclusive.
	/// </param>
	/// <param name="oUniqueDatasetId">
	/// Out | The unique proposed DatsetId. If null is returned then whatever was
	/// provided in proposedDatasetId is correct and remains as is. If string.Empty is
	/// returned then whatever was provided in proposedDatasetId is redundant and should
	/// be removed. If a value is returned then proposedDatasetId was ambiguous and
	/// uniqueDatasetId must be used in it's place. uniqueConnectionName and
	/// uniqueDatasetId are mutually exclusive.
	/// </param>
	/// <returns>
	/// A boolean indicating whether or not the provided arguments would cause a new
	/// connection to be registered in the rct. This only applies to registration in
	/// the rct and does not determine whether or not a new connection would be created
	/// in the SE. The caller must determine that.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public override bool GenerateUniqueDatasetKey(string proposedConnectionName,
		string proposedDatasetId, string dataSource, string dataset, string connectionUrl,
		string originalConnectionUrl, out EnConnectionSource oStoredConnectionSource,
		out string oChangedTargetDatasetKey, out string oUniqueDatasetKey,
		out string oUniqueConnectionName, out string oUniqueDatasetId)
	{
		// These are the 5 values in the tuple to be returned.
		bool rNewRctConnection = true;
		oStoredConnectionSource = EnConnectionSource.Unknown;
		oUniqueDatasetKey = null;
		oUniqueConnectionName = null;
		oUniqueDatasetId = null;
		oChangedTargetDatasetKey = null;


		int connectionIndex = -1;

		if (proposedConnectionName == string.Empty)
			proposedConnectionName = null;

		if (proposedDatasetId == string.Empty)
			proposedDatasetId = null;

		if (originalConnectionUrl != null && originalConnectionUrl == connectionUrl)
			originalConnectionUrl = null;

		// Get the index if the connection already exists.
		if (TryGetHybridInternalRowValue(connectionUrl, out DataRow row))
		{
			rNewRctConnection = false;
			connectionIndex = Convert.ToInt32(row["Id"]);

			oStoredConnectionSource = (EnConnectionSource)(int)row[CoreConstants.C_KeyExConnectionSource];

			if (originalConnectionUrl != null)
				oChangedTargetDatasetKey = (string)row[CoreConstants.C_KeyExDatasetKey];
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
		if (proposedConnectionName != null &&
			(proposedConnectionName == SystemData.DatasetKeyFmt.FmtRes(dataSource, derivedDatasetId)
			|| proposedConnectionName == SystemData.DatasetKeyAlternateFmt.FmtRes(dataSource, derivedDatasetId)))
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
		int index;

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
			for (int i = -1; i <= Count; i++)
			{
				// Try the original proposed first.
				if (i == -1)
				{
					if (proposedConnectionName == proposedConnectionNamePrefix)
						continue;

					oUniqueConnectionName = proposedConnectionName;
				}
				else
				{
					oUniqueConnectionName = (i == 0)
					? proposedConnectionNamePrefix
					: (proposedConnectionNamePrefix + $"_{i + 1}");
				}

				if (!TryGetEntry(oUniqueConnectionName, out index))
					break;

				if (connectionIndex > -1 && connectionIndex == index)
					break;
			}
			oUniqueDatasetKey = oUniqueConnectionName;
		}
		else
		{
			// Establish a unique DatasetId using i as the suffix.
			// This loop will execute at least once.

			for (int i = -1; i <= Count; i++)
			{
				// Try the original first.
				if (i == -1)
				{
					if (derivedDatasetId == derivedDatasetIdPrefix)
						continue;

					oUniqueDatasetId = derivedDatasetId;
				}
				else
				{
					oUniqueDatasetId = (i == 0)
						? derivedDatasetIdPrefix
						: (derivedDatasetIdPrefix + $"_{i + 1}");
				}

				oUniqueDatasetKey = SystemData.DatasetKeyFmt.FmtRes(dataSource, oUniqueDatasetId);

				if (!TryGetEntry(oUniqueDatasetKey, out index))
					break;

				if (connectionIndex > -1 && connectionIndex == index)
					break;
			}
		}

		if (proposedConnectionName != null && proposedConnectionName == oUniqueConnectionName)
			oUniqueConnectionName = null;

		if (proposedDatasetId != null && proposedDatasetId == oUniqueDatasetId)
			oUniqueDatasetId = null;

		if (oUniqueConnectionName == null && proposedConnectionNameIsDerived)
			oUniqueConnectionName = string.Empty;
		if (oUniqueDatasetId == null && proposedDatasetIdIsDerived)
			oUniqueDatasetId = string.Empty;

		
		// Tracer.Trace(GetType(), "GenerateUniqueDatasetKey()", "DataSource: {0}, Dataset: {1},
		//		proposedConnectionName: {2}, proposedDatasetId: {3},  rNewConnection: {4}, rUniqueDatasetKey: {5},
		//		rUniqueConnectionName: {6}, rUniqueDatasetId: {7}", dataSource ?? "Null", dataset ?? "Null", 
		//		proposedConnectionName ?? "Null", proposedDatasetId ?? "Null", rNewConnection,
		//		rUniqueDatasetKey ?? "Null", rUniqueConnectionName ?? "Null", rUniqueDatasetId ?? "Null");

		return rNewRctConnection;
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
		EnConnectionSource rowConnectionSource = (EnConnectionSource)(int)row[C_KeyExConnectionSource];

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
				csa.DatasetKey = SystemData.DatasetKeyFmt.FmtRes(csa.DataSource, csa.DatasetId);
			}
			else
			{
				csa.DatasetKey = SystemData.DatasetKeyFmt.FmtRes(csa.DataSource, csa.Dataset);
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
