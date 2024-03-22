
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using BlackbirdSql.Core.Ctl.Extensions;

using CoreConstants = BlackbirdSql.Core.Ctl.CoreConstants;


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
public class RunningConnectionTable : AbstractRunningConnectionTable
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractRunningConnectionTable
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Singleton .ctor contructor.
	/// </summary>
	private RunningConnectionTable() : base()
	{
	}



	/// <summary>
	/// Creates the singleton instance of the RunningConnectionTable for this session.
	/// Instantiation must always occur here and not by the Instance accessor to avoid
	/// confusion.
	/// </summary>
	public static RunningConnectionTable CreateInstance() => new RunningConnectionTable();


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
	public new DataRow this[string hybridKey]
	{
		get
		{
			if (hybridKey == null)
				return null;

			TryGetHybridRowValue(hybridKey, out DataRow value);

			return value;
		}
		set
		{
			AppendSingleConnectionRow(value);
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The table of all registered databases of the current data provider located in
	/// Server Explorer, FlameRobin and the Solution's Project settings, and any
	/// volatile unique connections defined in SqlEditor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public DataTable Databases
	{
		get
		{
			if (_Databases == null)
			{
				if (_Instance == null)
					return null;

				RctManager.EnsureLoaded();

				if (_LoadDataCardinal > 0)
				{
					AccessViolationException ex = new($"Attempt to access Rct internal table while loading. LoadDataCardinal: {_LoadDataCardinal}.");
					Diag.Dug(ex);
					throw ex;
				}

				_InternalConnectionsTable.AcceptChanges();

				_InternalConnectionsTable.DefaultView.ApplyDefaultSort = false;
				_InternalConnectionsTable.DefaultView.Sort = "Orderer,DataSource,DisplayName ASC";
				_InternalConnectionsTable.AcceptChanges();

				_Databases = _InternalConnectionsTable.DefaultView.ToTable(false);
				_Databases.PrimaryKey = [_Databases.Columns["Id"]];
				_Databases.AcceptChanges();
				_DataSources = null;
			}

			return _Databases;

		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The sorted and filtered view of the Databases table containing only
	/// DataSources/Servers.
	/// </summary>
	/// <returns>
	/// The populated <see cref="DataTable"/> that can be used together with
	/// <see cref="Databases"/> in a 1-n scenario. <see cref="ErmBindingSource"/> for
	/// an example.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public DataTable DataSources
	{
		get
		{
			if (_DataSources == null)
			{
				_DataSources = Databases.DefaultView.ToTable(true, "Orderer",
					"DataSource", "DataSourceLc", "Port");
				_DataSources.AcceptChanges();
			}
			return _DataSources;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a strings enumerable of the Rct's registered DatasetKeys.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public IEnumerable<string> RegisteredDatasets
	{
		get
		{
			object datasetKey;

			return Databases.Select()
					.Where(x => (datasetKey = x[CoreConstants.C_KeyExDatasetKey]) != DBNull.Value
						&& !string.IsNullOrWhiteSpace((string)datasetKey))
					.OrderBy(x => (string)x[CoreConstants.C_KeyExDatasetKey])
					.Select(x => (string)x[CoreConstants.C_KeyExDatasetKey]);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The sequential seed of the last attempt to modify the Rct.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static long Seed => _Seed;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - RunningConnectionTable
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds a synonym DatasetKey to the synonyms dictionary.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void AddSynonym(string synonym, string key)
	{
		if (_Instance == null)
			return;

		DataRow[] rows = _InternalConnectionsTable.Select().Where(x => key.Equals(x[CoreConstants.C_KeyExDatasetKey])).ToArray();

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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the registered connection data row given the ConnectionUrl,
	/// ConnectionString, DatasetKey or DatsetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool TryGetHybridRowValue(string hybridKey, out DataRow value)
	{
		if (hybridKey == null)
		{
			value = null;
			return false;
		}


		// Tracer.Trace(GetType(), "TryGetHybridRowValue()", "hybridKey: {0}", hybridKey);

		bool isConnectionUrl = hybridKey.StartsWith(_Scheme);

		if (!isConnectionUrl &&
			(hybridKey.StartsWith("data source=", StringComparison.InvariantCultureIgnoreCase)
			|| hybridKey.ToLowerInvariant().Contains(";data source=")))
		{
			CsbAgent csa = new(hybridKey, false);
			hybridKey = csa.SafeDatasetMoniker;
			isConnectionUrl = true;
		}

		if (isConnectionUrl)
		{
			DataRow[] rows = Databases.Select().Where(x => hybridKey.Equals(x[CoreConstants.C_KeyExConnectionUrl])).ToArray();

			value = rows.Length > 0 ? rows[0] : null;

			// if (value == null)
			//	Tracer.Trace(GetType(), "TryGetHybridRowValue()", "FAILED Final hybridKey: {0}", hybridKey);
		}
		else
		{
			if (TryGetEntry(hybridKey, out int id))
			{
				value = Databases.Rows.Find(id);
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
