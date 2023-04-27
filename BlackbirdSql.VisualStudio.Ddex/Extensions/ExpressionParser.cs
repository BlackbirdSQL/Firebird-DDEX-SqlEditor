using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FirebirdSql.Data.FirebirdClient;

using C5;
using BlackbirdDsl;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using Microsoft.VisualStudio.LanguageServer.Client;
using System.Collections;

namespace BlackbirdSql.Common.Extensions;

internal class ExpressionParser
{
	protected static Dictionary<FbConnection, ExpressionParser> _Instances = null;

	protected Parser _DslParser = null;

	protected FbConnection _Connection = null;
	protected DataTable _Sequences = null;
	protected DataTable _Triggers = null;

	protected DataTable _RawGenerators = null;
	protected DataTable _RawTriggerGenerators = null;
	protected DataTable _RawTriggers = null;


	protected Task<bool> _ExternalAsyncTask;


	protected bool _RawGeneratorsLoaded = false;
	protected bool _RawTriggerGeneratorsLoaded = false;
	protected bool _RawTriggersLoaded = false;


	protected bool _SequencesLoaded = false;
	// Triggers and Trigger generators are loaded
	protected bool _Loaded = false;
	// An async call is active
	protected bool _SyncActive = false;
	// Sync is making a call to GetSchema. GetSchema must allow it through.
	protected bool _Requesting = false;

	// An async call is active
	protected bool _AsyncActive = false;
	// A sync call has taken over. Async is locked out or abort at the first opportunity.
	protected CancellationTokenSource _AsyncLockedTokenSource;
	CancellationToken _AsyncLockedToken;

	protected System.Diagnostics.Stopwatch _Stopwatch = null;





	public static ExpressionParser Instance(FbConnection connection)
	{
		ExpressionParser parser;

		if (connection == null)
		{
			ArgumentNullException ex = new ArgumentNullException("Attempt to add a null connection");
			Diag.Dug(ex);
			throw ex;
		}

		if (_Instances == null)
		{
			Diag.Trace("Instances null. Adding new parser");
			_Instances = new();
			parser = new(connection);
			_Instances.Add(connection, parser);

		}
		else
		{
			if (!_Instances.TryGetValue(connection, out parser))
			{
				Diag.Trace("Parser instances not found. Adding new parser");
				parser = new(connection);
				_Instances.Add(connection, parser);
			}
			else
			{
				Diag.Trace("Parser instances found. Using existing parser");
			}
		}

		return parser;
	}



	public bool SequencesLoaded { get { return _SequencesLoaded; } }


	public bool ConnectionActive
	{
		get
		{
			return (_Connection != null && (_Connection.State & (ConnectionState.Closed | ConnectionState.Broken)) == 0);
		}
	}

	public bool Loaded { get { return _Loaded; } }

	public bool Requesting
	{
		get
		{
			return _Requesting;
		}
		set
		{
			if (value)
			{
				Stopwatch.Reset();
				Stopwatch.Start();
			}
			_Requesting = value;
		}
	}


	public bool ClearToLoad
	{
		get { return (!_SyncActive && !Loaded); }
	}

	public bool ClearToLoadAsync
	{
		get
		{
			return false; // (!_AsyncLockedToken.IsCancellationRequested && !_AsyncActive && ClearToLoad);
		}
	}



	public System.Diagnostics.Stopwatch Stopwatch
	{
		get
		{
			_Stopwatch ??= new();

			return _Stopwatch;
		}
	}


	public Parser DslParser
	{
		get
		{
			_DslParser ??= new Parser();
			return _DslParser;
		}

	}


	protected ExpressionParser(FbConnection connection)
	{
		_Connection = connection;

		_Connection.StateChange += ConnectionStateChanged;
		_Connection.Disposed += ConnectionDisposed;

		_AsyncLockedTokenSource = new();
		_AsyncLockedToken = _AsyncLockedTokenSource.Token;

		_ExternalAsyncTask = null; 


		_Sequences = new ();

		_Sequences.Columns.Add("GENERATOR_CATALOG", typeof(string));
		_Sequences.Columns.Add("GENERATOR_SCHEMA", typeof(string));
		_Sequences.Columns.Add("SEQUENCE_GENERATOR", typeof(string));
		_Sequences.Columns.Add("IS_SYSTEM_FLAG", typeof(int));
		_Sequences.Columns.Add("GENERATOR_ID", typeof(short));
		_Sequences.Columns.Add("GENERATOR_IDENTITY", typeof(int));
		_Sequences.Columns.Add("IDENTITY_SEED", typeof(long));
		_Sequences.Columns.Add("IDENTITY_INCREMENT", typeof(int));
		_Sequences.Columns.Add("IDENTITY_CURRENT", typeof(long));
		_Sequences.Columns.Add("TRIGGER_NAME", typeof(string));

		_Sequences.PrimaryKey = new DataColumn[] { _Sequences.Columns["SEQUENCE_GENERATOR"] };

		_Sequences.AcceptChanges();


		_Triggers = new();

		_Triggers.Columns.Add("TABLE_CATALOG", typeof(string));
		_Triggers.Columns.Add("TABLE_SCHEMA", typeof(string));
		_Triggers.Columns.Add("TABLE_NAME", typeof(string));
		_Triggers.Columns.Add("TRIGGER_NAME", typeof(string));
		_Triggers.Columns.Add("DESCRIPTION", typeof(string));
		_Triggers.Columns.Add("IS_SYSTEM_FLAG", typeof(int));
		_Triggers.Columns.Add("TRIGGER_TYPE", typeof(long));
		_Triggers.Columns.Add("IS_INACTIVE", typeof(bool));
		_Triggers.Columns.Add("PRIORITY", typeof(short));
		_Triggers.Columns.Add("EXPRESSION", typeof(string));
		_Triggers.Columns.Add("IS_IDENTITY", typeof(bool));
		_Triggers.Columns.Add("SEQUENCE_GENERATOR", typeof(string));
		_Triggers.Columns.Add("DEPENDENCY_FIELDS", typeof(string));
		_Triggers.Columns.Add("IDENTITY_SEED", typeof(long));
		_Triggers.Columns.Add("IDENTITY_INCREMENT", typeof(int));
		_Triggers.Columns.Add("IDENTITY_TYPE", typeof(short));
		_Triggers.Columns.Add("DEPENDENCY_FIELD", typeof(string));
		_Triggers.Columns.Add("DEPENDENCY_COUNT", typeof(int));
		_Triggers.Columns.Add("IDENTITY_CURRENT", typeof(long));

		_Triggers.PrimaryKey = new DataColumn[] { _Triggers.Columns["TRIGGER_NAME"] };

		_Triggers.AcceptChanges();
	}

	~ExpressionParser()
	{
		_AsyncLockedTokenSource.Cancel();
		_AsyncLockedTokenSource?.Dispose();
	}

	public (string, int) ParseGeneratorInfo(string sql)
	{
		return ParseGeneratorInfo(sql, null, null, null);
	}


	public (string, int) ParseGeneratorInfo(string sql, string trigger)
	{
		return ParseGeneratorInfo(sql, trigger, null, null);
	}


	public (string, int) ParseGeneratorInfo(string sql, string trigger, string table)
	{
		return ParseGeneratorInfo(sql, trigger, table, null);
	}

	public (string, int) ParseGeneratorInfo(string sql, string trigger, string table, string column)
	{
		int stage;
		int increment = -1;
		string generator = null;
		string value;
		string arguments = null;

		string[][] _Stages =
		{
			new string[] { "CREATE", "TRIGGER", "_TRIGGER_", "FOR", "_TABLE_",
				"AS", "BEGIN", "_COLUMN_", "=", "GEN_ID", "_PARAM_" },
			new string[] { "CREATE", "TRIGGER", "_TRIGGER_", "FOR", "_TABLE_",
				"AS", "BEGIN", "_COLUMN_", "=", "NEXT", "VALUE", "FOR", "_PARAM_" }
		};


		_Stages[0][2] = _Stages[1][2] = trigger?.ToUpper();
		_Stages[0][4] = _Stages[1][4] = table?.ToUpper();
		_Stages[0][7] = _Stages[1][7] = (column != null ? ("NEW." + column.ToUpper()) : null);


		StringCell tokens = DslParser.Execute(sql.ToUpper());

		// { "CREATE", "TRIGGER", "_TRIGGER_", "FOR", "_TABLE_", "AS", "BEGIN", "_COLUMN_", "=", "GEN_ID", "_PARAM_" }

		int version;

		for (version = 0; version < _Stages.Length && arguments == null; version++)
		{
			stage = 0;

			foreach (StringCell token in tokens.Enumerator)
			{
				value = token.ToString().Trim();

				if (String.IsNullOrEmpty(value))
					continue;

				if (stage == _Stages[version].Length - 1)
				{
					arguments = value;
					break;
				}

				if (stage < 6)
				{
					for (int i = stage; i < 6; i++)
					{
						if (value == _Stages[version][i])
						{
							for (stage = i + 1; stage < 5 && _Stages[version][stage] == null; stage++) ;
							break;
						}
					}
					continue;
				}

				if (value == _Stages[version][stage])
				{
					for (stage++; stage < _Stages[version].Length && _Stages[version][stage] == null; stage++) ;
					if (stage == _Stages[version].Length) // Should never  happen
						break;
				}
			}
		}

		if (arguments == null)
			return (null, -1);

		if (version < _Stages.Length)
		{
			char[] chrs = { '(', ')', ' ' };

			string[] parameters = arguments.Trim(chrs).Split(',');

			if (parameters.Length > 0)
				generator = parameters[0].Trim();

			if (parameters.Length > 1)
				increment = Convert.ToInt32(parameters[1].Trim());
		}
		else
		{
			generator = arguments.Trim();
			increment = 1;
		}

		return (generator, increment);
	}





	public void Load()
	{
		if (!ClearToLoad)
			return;

		if (_Connection == null)
		{
			ObjectDisposedException ex = new("Connection is null");
			Diag.Dug(ex);
			throw ex;
		}

		if (!ConnectionActive)
		{
			DataException ex = new("Connection closed");
			Diag.Dug(ex);
			throw ex;
		}


		_SyncActive = true;

		Diag.Trace("Entering Load");

		if (_ExternalAsyncTask != null)
		{
			Diag.Trace("Waiting for external task");
			_AsyncLockedTokenSource.Cancel();
			_ExternalAsyncTask.Wait();
			Diag.Trace("Waiting over for external task");
		}
		else
		{
			Diag.Trace("Commencing load. No external async task.");
		}


		if (!ConnectionActive)
			return;

		if (!_RawGeneratorsLoaded)
		{
			Diag.Trace("Getting RawGeneratorSchema");
			if (GetRawGeneratorSchema() != null)
				_RawGeneratorsLoaded = true;
		}

		if (!ConnectionActive)
			return;


		if (!_RawTriggerGeneratorsLoaded)
		{
			Diag.Trace("Getting RawTriggerGeneratorSchema");
			if (GetRawTriggerGeneratorSchema() != null)
				_RawTriggerGeneratorsLoaded = true;
		}

		if (!ConnectionActive)
			return;

		if (!_RawTriggersLoaded)
		{
			Diag.Trace("Getting RawTriggerSchema");
			if (GetRawTriggerSchema() != null)
				_RawTriggersLoaded = true;
		}

		if (!ConnectionActive)
			return;

		if (!SequencesLoaded)
		{
			BuildSequenceTable();
		}

		if (!ConnectionActive)
			return;

		if (!_Loaded)
		{
			BuildTriggerTable();
		}

		_SyncActive = false;
	}


	public void AsyncLoad()
	{
		if (!ClearToLoadAsync)
			return;

		_AsyncActive = true;

		Diag.Trace("Initiating LoadAsync");

		var thread = Task.Run(() => { _ = LoadAsync(_AsyncLockedToken); });
	}


	// Must only be called by AsyncLoad to maintain syncronization integrity with sync calls.
	protected Task<bool> LoadAsync(CancellationToken asyncLockedToken)
	{
		_AsyncActive = true;

		Diag.Trace("Calling PerformLoadAsync");
		_ExternalAsyncTask = PerformLoadAsync(asyncLockedToken);

		_ExternalAsyncTask.Wait();

		_AsyncActive = false;

		Diag.Trace("Wait over for PerformLoadAsync");

		return _ExternalAsyncTask;

	}


	// Must only be called by AwaitLoadAsync to maintain syncronization integrity with sync calls.
	protected async Task<bool> PerformLoadAsync(CancellationToken asyncLockedToken)
	{
		DataTable schema;

		if (!_RawGeneratorsLoaded)
		{
			if (asyncLockedToken.IsCancellationRequested)
				return false;

			Diag.Trace("External start GetRawGeneratorSchemaAsync");
			schema = await GetRawGeneratorSchemaAsync(); //.ConfigureAwait(false);


			if (schema == null)
			{
				Diag.Trace("GetRawGeneratorSchemaAsync failed");
				return false;
			}

			_RawGeneratorsLoaded = true;
			Diag.Trace("External wait over GetRawGeneratorSchemaAsync");
		}


		if (!_RawTriggerGeneratorsLoaded)
		{
			if (asyncLockedToken.IsCancellationRequested)
				return false;

			Diag.Trace("External start GetRawTriggerGeneratorSchemaAsync");

			schema = await GetRawTriggerGeneratorSchemaAsync(); //.ConfigureAwait(false);


			if (schema == null)
			{
				Diag.Trace("GetRawTriggerGeneratorSchemaAsync failed");
				return false;
			}

			_RawTriggerGeneratorsLoaded = true;
			Diag.Trace("External wait over GetRawTriggerGeneratorSchemaAsync");
		}


		if (!_RawTriggersLoaded)
		{
			if (asyncLockedToken.IsCancellationRequested)
				return false;
			Diag.Trace("External start GetRawTriggerSchemaAsync");
			schema = await GetRawTriggerSchemaAsync(); // .ConfigureAwait(false);

			if (schema == null)
			{
				Diag.Trace("GetRawTriggerSchemaAsync failed");
				return false;
			}

			_RawTriggersLoaded = true;
			Diag.Trace("External wait over GetRawTriggerSchemaAsync");
		}


		if (!SequencesLoaded)
			BuildSequenceTable();

		if (!_Loaded)
			BuildTriggerTable();

		return true;
	}


	protected DataTable GetRawGeneratorSchema()
	{
		DslRawGenerators schema = new(this);

		Requesting = true;
		_RawGenerators = schema.GetRawSchema(_Connection, "Generators");
		Diag.Trace("Out Generators GetRawSchema");
		Requesting = false;

		return _RawGenerators;
	}

	protected async Task<DataTable> GetRawGeneratorSchemaAsync()
	{
		DslRawGenerators schema = new(this);

		Requesting = true;
		_RawGenerators = await schema.GetRawSchemaAsync(_Connection, "Generators").ConfigureAwait(false);
		Diag.Trace("Out Generators GetRawSchemaAsync");
		Requesting = false;

		return _RawGenerators;
	}


	protected DataTable GetRawTriggerSchema()
	{
		DslRawTriggers schema = new(this);

		Requesting = true;
		_RawTriggers = schema.GetRawSchema(_Connection, "Triggers");
		Diag.Trace("Out Triggers GetRawSchema");
		Requesting = false;

		return _RawTriggers;
	}

	protected async Task<DataTable> GetRawTriggerSchemaAsync()
	{
		DslRawTriggers schema = new(this);

		Requesting = true;
		_RawTriggers = await schema.GetRawSchemaAsync(_Connection, "Triggers").ConfigureAwait(false);
		Diag.Trace("Out Triggers GetRawSchemaAsync");
		Requesting = false;

		return _RawTriggers;
	}


	protected DataTable GetRawTriggerGeneratorSchema()
	{
		DslRawTriggerGenerators schema = new(this);

		Requesting = true;
		_RawTriggerGenerators = schema.GetRawSchema(_Connection, "TriggerGenerators");
		Diag.Trace("Out TriggerGenerators GetRawSchema");
		Requesting = false;

		return _RawTriggerGenerators;
	}

	protected async Task<DataTable> GetRawTriggerGeneratorSchemaAsync()
	{
		DslRawTriggerGenerators schema = new(this);

		Requesting = true;
		_RawTriggerGenerators = await schema.GetRawSchemaAsync(_Connection, "TriggerGenerators").ConfigureAwait(false);
		Diag.Trace("Out TriggerGenerators GetRawSchemaAsync");
		Requesting = false;

		return _RawTriggerGenerators;
	}





	public void BuildSequenceTable()
	{
		Stopwatch.Reset();
		Stopwatch.Start();


		_Sequences.BeginLoadData();

		DataRow seq;

		foreach (DataRow row in _RawGenerators.Rows)
		{
			seq = _Sequences.NewRow();

			seq.ItemArray = row.ItemArray;
			seq["TRIGGER_NAME"] = DBNull.Value;

			_Sequences.Rows.Add(seq);
		}

		_Sequences.EndLoadData();
		_Sequences.AcceptChanges();

		_RawGenerators = null;
		_SequencesLoaded = true;

		Stopwatch.Stop();
		Diag.Trace("Sequences loaded time :" + Stopwatch.ElapsedMilliseconds);
		Stopwatch.Reset();
		Stopwatch.Start();
	}

	public void NotifyGeneratorsFetched()
	{
		Stopwatch.Stop();
		Diag.Trace("Generators fetch time :" + Stopwatch.ElapsedMilliseconds);
	}

	public void NotifyTriggerGeneratorsFetched()
	{
		Stopwatch.Stop();
		Diag.Trace("TriggerGenerators fetch time :" + Stopwatch.ElapsedMilliseconds);
	}

	public void NotifyTriggersFetched()
	{
		Stopwatch.Stop();
		Diag.Trace("Triggers fetch time :" + Stopwatch.ElapsedMilliseconds);
	}


	public void BuildTriggerTable()
	{
		Stopwatch.Reset();
		Stopwatch.Start();


		_Triggers.BeginLoadData();

		bool isIdentity;
		int increment;
		int dependencyCount;
		int result;
		int i, j;
		string genId;
		string trigKey, trigGenKey;

		DataRow seq;
		DataRow trig;
		DataRow trigGenRow;
		object[] key;


		System.Collections.IEnumerator enumerator = _RawTriggerGenerators.Rows.GetEnumerator();


		foreach (DataRow row in _RawTriggers.Rows)
		{
			if (!enumerator.MoveNext())
				break;

			trigGenRow = enumerator.Current as DataRow;

			trigKey = row["TRIGGER_NAME"].ToString();

			while ((result = trigKey.CompareTo((trigGenKey = trigGenRow["TRIGGER_NAME"].ToString()))) > 0)
			{
				if (!enumerator.MoveNext())
					break;
				trigGenRow = enumerator.Current as DataRow;
			}

			if (result < 0)
				continue;

			trig = _Triggers.NewRow();

			for (i = 0; i < _RawTriggers.Columns.Count; i++)
			{
				try
				{
					trig[i] = row[i];
				}
				catch (Exception ex)
				{
					Diag.Dug(ex, String.Format("Trig Error at i:{0}.", i));
				}
			}

			for (j = 1; j < _RawTriggerGenerators.Columns.Count; j++, i++)
			{
				try
				{
					trig[i] = trigGenRow[j];
				}
				catch (Exception ex)
				{
					Diag.Dug(ex, String.Format("TrigGen Error at i:{0}, j:{1}.", i, j));
				}
			}

			isIdentity = Convert.ToBoolean(trig["IS_IDENTITY"]);

			if (trig["DEPENDENCY_FIELDS"] != DBNull.Value)
				dependencyCount = trig["DEPENDENCY_FIELDS"].ToString().Split(',').Length;
			else
				dependencyCount = 0;

			if (dependencyCount == 1)
				trig["DEPENDENCY_FIELD"] = trig["DEPENDENCY_FIELDS"];
			else
				isIdentity = false;

			genId = null;

			if (isIdentity)
			{
				(genId, increment) = ParseGeneratorInfo(trig["EXPRESSION"].ToString(), trig["TRIGGER_NAME"].ToString(),
					trig["TABLE_NAME"].ToString(), trig["DEPENDENCY_FIELDS"].ToString());

				if (genId != null)
				{
					key = new object[] { genId };
					seq = _Sequences.Rows.Find(key);

					if (seq != null)
					{
						if (trig["SEQUENCE_GENERATOR"] == DBNull.Value)
						{
							trig["SEQUENCE_GENERATOR"] = genId;
							trig["IDENTITY_INCREMENT"] = increment;
							trig["IDENTITY_SEED"] = seq["IDENTITY_SEED"];
						}
						else if (isIdentity)
						{
							// There is a generator so: IDENTITY_TYPE determines if is -identity still holds true
							if (trig["IDENTITY_TYPE"] == DBNull.Value || Convert.ToInt16(trig["IDENTITY_TYPE"]) == 0)
							{
								isIdentity = false;
							}
						}
						trig["IDENTITY_CURRENT"] = seq["IDENTITY_CURRENT"];

						if (seq["TRIGGER_NAME"] == DBNull.Value)
							seq["TRIGGER_NAME"] = trig["TRIGGER_NAME"];
					}
				}

			}

			UpdateTriggerData(trig, genId, isIdentity, dependencyCount);


			_Triggers.Rows.Add(trig);

		}


		_Triggers.EndLoadData();
		_Triggers.AcceptChanges();

		_Loaded = true;
		_RawTriggers = null;
		_RawTriggerGenerators = null;

		Stopwatch.Stop();
		Diag.Trace("Triggers evaluation time :" + Stopwatch.ElapsedMilliseconds);

		_Stopwatch = null;
	}

	protected void UpdateTriggerData(DataRow trig, string genId, bool isIdentity, int dependencyCount)
	{

		trig["DEPENDENCY_COUNT"] = dependencyCount;


		if (isIdentity && genId == null)
			isIdentity = false;

		if (!isIdentity)
		{
			trig["IDENTITY_SEED"] = 0;
			trig["IDENTITY_INCREMENT"] = 0;
			trig["IDENTITY_CURRENT"] = 0;
		}

		trig["IS_IDENTITY"] = isIdentity;
	}

	public DataTable GetTriggerSchema(string[] restrictions, int systemFlag, int identityFlag)
	{
		if (!Loaded)
			Load();

		var where = new StringBuilder();

		if (systemFlag == 1)
		{
			where.Append("IS_SYSTEM_FLAG = 1");
		}
		else if (systemFlag == 0)
		{
			where.Append("IS_SYSTEM_FLAG <> 1");
		}

		if (identityFlag == 0)
		{
			if (where.Length > 0)
				where.Append(" AND ");
			where.Append("IS_IDENTITY = FALSE");
		}
		else if (identityFlag == 1)
		{
			if (where.Length > 0)
				where.Append(" AND ");
			where.Append("IS_IDENTITY = TRUE");
		}


		if (restrictions != null)
		{
			// var index = 0;

			/* TABLE_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* TABLE_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* TABLE_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				// Cannot pass params to execute block
				where.AppendFormat("TABLE_NAME = '{0}'", restrictions[2]);
				// where.AppendFormat("trg.rdb$relation_name = @p{0}", index++);
			}

			/* TRIGGER_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				// Cannot pass params to execute block
				where.AppendFormat("TRIGGER_NAME = '{0}'", restrictions[3]);
				// where.AppendFormat("trg.rdb$trigger_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			_Triggers.DefaultView.RowFilter = where.ToString();
			return _Triggers.DefaultView.ToTable();
		}

		_Triggers.DefaultView.RowFilter = null;

		return _Triggers;

	}

	public async Task<DataTable> GetTriggerSchemaAsync(string[] restrictions, int systemFlag, int identityFlag, CancellationToken asyncLockedToken)
	{
		await Task.CompletedTask.ConfigureAwait(false);
		return GetTriggerSchema(restrictions, systemFlag, identityFlag);
	}

	public DataTable GetSequenceSchema(string[] restrictions)
	{
		if (!Loaded)
			Load();

		var where = new StringBuilder();

		if (restrictions != null)
		{
			// var index = 0;

			/* GENERATOR_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* GENERATOR_SCHEMA	*/
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* SEQUENCE_GENERATOR */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				// Cannot pass params to execute block
				// where.AppendFormat("rdb$generator_name = @p{0}", index++);
				where.AppendFormat("SEQUENCE_GENERATOR = '{0}'", restrictions[2].ToString());
			}

			/* IS_SYSTEM_GENERATOR	*/
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				// Cannot pass params to execute block
				// where.AppendFormat("rdb$system_flag = @p{0}", index++);
				where.AppendFormat("IS_SYSTEM_FLAG = '{0}'", restrictions[3].ToString());
			}
		}


		if (where.Length > 0)
		{
			_Sequences.DefaultView.RowFilter = where.ToString();
			return _Sequences.DefaultView.ToTable();
		}

		_Sequences.DefaultView.RowFilter = null;

		return _Sequences;

	}


	public async Task<DataTable> GetSequenceSchemaAsync(string[] restrictions, CancellationToken asyncLockedToken)
	{
		await Task.CompletedTask.ConfigureAwait(false);
		return GetSequenceSchema(restrictions);
	}

	public DataRow FindTrigger(object name)
	{
		return _Triggers.Rows.Find(name);
	}

	void ConnectionStateChanged(object sender, StateChangeEventArgs e)
	{
		if ((e.CurrentState & (ConnectionState.Closed | ConnectionState.Broken)) != 0)
		{
			Diag.Trace("Connection closing. Cancelling async load");
			_AsyncLockedTokenSource.Cancel();
		}
		else if ((e.OriginalState & (ConnectionState.Closed | ConnectionState.Broken)) != 0
			&& (e.CurrentState & (ConnectionState.Closed | ConnectionState.Broken)) == 0)
		{
			Diag.Trace("Connection reopening. Calling async load");

			_Connection = (FbConnection)sender;
			if (ClearToLoadAsync)
				AsyncLoad();
		}
	}



	protected static void ConnectionDisposed(object sender, EventArgs e)
	{
		if (sender is not FbConnection connection)
		{
			Diag.Trace("Connection disposed but sender is not an FbConnection");
			return;
		}


		if (_Instances.TryGetValue(connection, out ExpressionParser parser))
		{
			Diag.Trace("Connection disposed");
			_Instances.Remove(connection);
			parser._Connection = null;

		}
	}
}
