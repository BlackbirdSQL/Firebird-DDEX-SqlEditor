
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//										RunningConnectionTable Class
//
/// <summary>
/// Holds a database of volatile and non-volatile configured connections for a session.
/// Access to this class set should be through the <see cref="RctManager"/> only.
/// The Rct classes do not manage access availibility and deadlock prevention. That is the responsibility
/// of the <see cref="RctManager"/>.
/// </summary>
/// <remarks>
/// The abstruse class deals specifically with the initial loading of preconfigured connections.
/// The abstract class <see cref="AbstractRunningConnectionTable"/> deals specifically with the managing of
/// preconfigured and volatile data.
/// This final class deals specifically with IDictionary and DataTable accessor handling of registered
/// connection configurations.
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
public abstract class RunningConnectionTable : AbstractRunningConnectionTable
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractRunningConnectionTable
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Singleton .ctor contructor.
	/// </summary>
	protected RunningConnectionTable() : base()
	{
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields and Constants - RunningConnectionTable
	// =========================================================================================================


	#endregion Fields and Constants




	// =========================================================================================================
	#region Property accessors - RunningConnectionTable
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the registered connection row given the DatasetKey, unique ConnectionUrl
	/// or ConnectionString..
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected new DataRow this[string hybridKey]
	{
		get
		{
			if (hybridKey == null)
				return null;

			EnRctKeyType keyType = hybridKey.StartsWith(_Scheme) ? EnRctKeyType.ConnectionUrl : EnRctKeyType.DatasetKey;

			if (keyType != EnRctKeyType.ConnectionUrl)
			{
				if (hybridKey.StartsWith("data source=", StringComparison.InvariantCultureIgnoreCase)
					|| hybridKey.ToLowerInvariant().Contains(";data source="))
				{
					Csb csa = new(hybridKey, false);
					hybridKey = csa.Moniker;
					keyType = EnRctKeyType.ConnectionUrl;
				}
			}

			TryGetHybridRowValue(hybridKey, keyType, out DataRow value);

			return value;
		}
		set
		{
			AppendSingleConnectionRow(value);
		}

	}


	protected bool AsyncPending => _AsyncPayloadLauncherLaunchState == EnLauncherPayloadLaunchState.Pending;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The table of all registered databases of the current data provider located in
	/// Server Explorer, FlameRobin and the Solution's Project settings, and any
	/// volatile unique connections defined in SqlEditor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected DataTable InternalDatabases
	{
		get
		{
			lock (_LockObject)
			{

				if (_InternalDatabases != null)
					return _InternalDatabases;

				if (_Instance == null)
					return null;
			}

			InternalResolveDeadlocksAndEnsureLoaded(false);

			if (_LoadDataCardinal > 0)
			{
				AccessViolationException ex = new($"Attempt to access Rct internal table while loading. LoadDataCardinal: {_LoadDataCardinal}.");
				Diag.Dug(ex);
				throw ex;
			}

			lock (_LockObject)
			{

				_InternalConnectionsTable.AcceptChanges();

				_InternalConnectionsTable.DefaultView.ApplyDefaultSort = false;
				_InternalConnectionsTable.DefaultView.Sort = "Orderer,DataSource,AdornedQualifiedTitle ASC";
				_InternalConnectionsTable.AcceptChanges();

				_InternalDatabases = _InternalConnectionsTable.DefaultView.ToTable(false);
				_InternalDatabases.PrimaryKey = [_InternalDatabases.Columns["Id"]];
				_InternalDatabases.AcceptChanges();
				_InternalServers = null;

				return _InternalDatabases;
			}

		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The sorted and filtered view of the InternalDatabases table containing only
	/// DataSources/Servers.
	/// </summary>
	/// <returns>
	/// The populated <see cref="DataTable"/> that can be used together with
	/// <see cref="InternalDatabases"/> in a 1-n scenario. <see cref="ErmBindingSource"/> for
	/// an example.
	/// </returns>
	// ---------------------------------------------------------------------------------
	protected DataTable InternalServers
	{
		get
		{
			if (_InternalServers == null)
			{
				_InternalServers = InternalDatabases.DefaultView.ToTable(true, "Orderer",
					"DataSource", "DataSourceLc", "Port");
				_InternalServers.AcceptChanges();
			}
			return _InternalServers;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// This internal table storing all registered connections and datasources/servers.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected DataTable InternalConnectionsTable
	{
		get { lock (_LockObject) return _InternalConnectionsTable; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a strings enumerable of the Rct's registered DatasetKeys'
	/// AdornedQualifiedName's.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected IEnumerable<string> InternalAdornedQualifiedNames
	{
		get
		{
			object adornedQualifiedName;

			return InternalDatabases.Select()
					.Where(x => (adornedQualifiedName = x[CoreConstants.C_KeyExAdornedQualifiedName]) != DBNull.Value
						&& !string.IsNullOrWhiteSpace((string)adornedQualifiedName))
					.OrderBy(x => (string)x[CoreConstants.C_KeyExAdornedQualifiedName])
					.Select(x => (string)x[CoreConstants.C_KeyExAdornedQualifiedName]);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a strings enumerable of the Rct's registered DatasetKeys'
	/// AdornedQualifiedTitle's.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected IEnumerable<string> InternalAdornedQualifiedTitles
	{
		get
		{
			object adornedQualifiedTitle;

			return InternalDatabases.Select()
					.Where(x => (adornedQualifiedTitle = x[CoreConstants.C_KeyExAdornedQualifiedTitle]) != DBNull.Value
						&& !string.IsNullOrWhiteSpace((string)adornedQualifiedTitle))
					.OrderBy(x => (string)x[CoreConstants.C_KeyExAdornedQualifiedTitle])
					.Select(x => (string)x[CoreConstants.C_KeyExAdornedQualifiedTitle]);
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - RunningConnectionTable
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a synonym DatasetKey to the synonyms dictionary.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void AddSynonym(string synonym, string key)
	{
		if (_Instance == null)
			return;

		DataRow[] rows = null;

		lock (_LockObject)
			rows = _InternalConnectionsTable.Select().Where(x => key.Equals(x[CoreConstants.C_KeyExDatasetKey])).ToArray();

		if (rows.Length == 0)
		{
			ArgumentException ex = new($"DatasetKey '{key}' not found for synonym '{synonym}.");
			Diag.Dug(ex);
			throw ex;
		}

		try
		{
			Add(synonym, Convert.ToInt32(rows[0]["Id"]));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Failed to add Synonym {synonym} for DatsetKey: {key}.");
			throw ex;
		}
	}



	protected abstract bool InternalResolveDeadlocksAndEnsureLoaded(bool asynchronous);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the registered connection data row given the ConnectionUrl,
	/// ConnectionString, DatasetKey or DatsetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool TryGetHybridRowValue(string hybridKey, EnRctKeyType keyType, out DataRow value)
	{
		if (hybridKey == null)
		{
			value = null;
			return false;
		}


		// Tracer.Trace(GetType(), "TryGetHybridRowValue()", "hybridKey: {0}", hybridKey);


		if (keyType == EnRctKeyType.ConnectionString)
		{
			Csb csa = new(hybridKey, false);
			hybridKey = csa.Moniker;
			keyType = EnRctKeyType.ConnectionUrl;
		}


		if (keyType == EnRctKeyType.ConnectionUrl)
		{
			DataRow[] rows = InternalDatabases.Select().Where(x => hybridKey.Equals(x[CoreConstants.C_KeyExConnectionUrl])).ToArray();

			value = rows.Length > 0 ? rows[0] : null;

			// if (value == null)
			//	Tracer.Trace(GetType(), "TryGetHybridRowValue()", "FAILED Final hybridKey: {0}", hybridKey);
		}
		else if (keyType == EnRctKeyType.AdornedQualifiedName)
		{
			DataRow[] rows = InternalDatabases.Select().Where(x => hybridKey.Equals(x[CoreConstants.C_KeyExAdornedQualifiedName])).ToArray();

			value = rows.Length > 0 ? rows[0] : null;

			// if (value == null)
			//	Tracer.Trace(GetType(), "TryGetHybridRowValue()", "FAILED Final hybridKey: {0}", hybridKey);
		}
		else if (keyType == EnRctKeyType.AdornedQualifiedTitle)
		{
			DataRow[] rows = InternalDatabases.Select().Where(x => hybridKey.Equals(x[CoreConstants.C_KeyExAdornedQualifiedTitle])).ToArray();

			value = rows.Length > 0 ? rows[0] : null;

			// if (value == null)
			//	Tracer.Trace(GetType(), "TryGetHybridRowValue()", "FAILED Final hybridKey: {0}", hybridKey);
		}
		else
		{
			if (TryGetEntry(hybridKey, out int id))
			{
				value = InternalDatabases.Rows.Find(id);
			}
			else
			{
				value = null;
			}
		}

		return value != null;

	}


	#endregion Methods


}
