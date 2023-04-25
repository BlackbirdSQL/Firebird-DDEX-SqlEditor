using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Data.Services;

using FirebirdSql.Data.FirebirdClient;

using C5;
using BlackbirdDsl;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using System.Reflection;
using Microsoft.VisualStudio.LanguageServer.Client;

namespace BlackbirdSql.Common.Extensions;

internal class ExpressionParser
{
	protected static Dictionary<FbConnection, ExpressionParser> _Instances = null;

	protected FbConnection _Connection = null;
	protected DataTable _Sequences = null;
	protected DataTable _Triggers = null;

	protected DataTable _RawGenerators = null;
	protected DataTable _RawTriggerGenerators = null;
	protected DataTable _RawTriggers = null;


	protected Parser _DslParser = null;

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
	// A sync call has taken over. Async is locked out ormust abort at the first opportunity.
	protected bool _AsyncLocked = false;

	protected System.Diagnostics.Stopwatch _Stopwatch = null;





	public static ExpressionParser Instance(FbConnection connection)
	{
		ExpressionParser value;

		if (_Instances == null)
		{
			_Instances = new();
			value = new(connection)
			{
				_Connection = connection
			};

			_Instances.Add(connection, value);
			connection.StateChange += value.ConnectiontionStateChanged;
			connection.Disposed += Connection_Disposed;
		}
		{
			if (!_Instances.TryGetValue(connection, out value))
			{
				value = new(connection);
				_Instances.Add(connection, value);
				connection.StateChange += value.ConnectiontionStateChanged;
				connection.Disposed += Connection_Disposed;
			}
		}

		return value;
	}

	private static void Connection_Disposed(object sender, EventArgs e)
	{
		throw new NotImplementedException();
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
			return (!_AsyncLocked && !_AsyncActive && ClearToLoad && ConnectionActive);
		}
	}

	public DataTable Sequences
	{
		get
		{
			if (_Sequences == null)
			{
				_Sequences = new();
				_Sequences.ExtendedProperties["ExpressionParser"] = this;
			}

			return _Sequences;
		}
	}

	public DataTable Triggers
	{
		get
		{
			if (_Triggers == null)
			{
				_Triggers = new();
				_Triggers.ExtendedProperties["ExpressionParser"] = this;
			}
			return _Triggers;
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


	public void AsyncLoad()
	{
		if (!ClearToLoadAsync)
			return;

		var thread = Task.Run(() => { _ = LoadAsync(); });
	}

	protected async Task LoadAsync()
	{ 
		_AsyncActive = true;

		DataTable schema;
		// Task<DataTable> task;

		if (_Connection != null && !_RawGeneratorsLoaded)
		{
			Requesting = true;
			schema = await GetRawGeneratorSchemaAsync().ConfigureAwait(false);
			Requesting = false;

			if (schema == null)
				return;

			if (_AsyncLocked)
			{
				_AsyncActive = false;
				return;
			}

			_RawGenerators = schema;
			_RawGeneratorsLoaded = true;
		}


		if (_Connection != null && !_RawTriggerGeneratorsLoaded)
		{
			Requesting = true;
			schema = await GetRawTriggerGeneratorSchemaAsync().ConfigureAwait(false);
			Requesting = false;

			if (schema == null)
				return;

			if (_AsyncLocked)
			{
				_AsyncActive = false;
				return;
			}

			_RawTriggerGenerators = schema;
			_RawTriggerGeneratorsLoaded = true;
		}

		if (!_RawTriggersLoaded)
		{
			Requesting = true;
			// schema = await DslSchemaFactory.GetSchemaAsync(_Connection, "Triggers", null).ConfigureAwait(false);
			schema = await GetRawTriggerSchemaAsync().ConfigureAwait(false);
			Requesting = false;

			if (schema == null)
				return;

			if (_AsyncLocked)
			{
				_AsyncActive = false;
				return;
			}

			_RawTriggers = schema;
			_RawTriggersLoaded = true;
		}


		if (!SequencesLoaded)
		{
			if (_AsyncLocked)
			{
				_AsyncActive = false;
				return;
			}

			BuildSequenceTable();

			if (_AsyncLocked)
			{
				_AsyncActive = false;
				return;
			}
		}

		if (!_Loaded)
		{
			if (_AsyncLocked)
			{
				_AsyncActive = false;
				return;
			}
			BuildTriggerTable();
		}
		_AsyncActive = false;
		return;
	}


	public void Load()
	{
		if (!ClearToLoad)
			return;

		_SyncActive = true;
		_AsyncLocked = true;

		if (!_RawGeneratorsLoaded)
		{
			Requesting = true;
			_RawGenerators = DslSchemaFactory.GetSchema(_Connection, "Generators", null);
			Requesting = false;

			_RawGeneratorsLoaded = true;
		}


		if (!_RawTriggerGeneratorsLoaded)
		{
			Requesting = true;
			_RawTriggerGenerators = DslSchemaFactory.GetSchema(_Connection, "TriggerGenerators", null);
			Requesting = false;

			_RawTriggerGeneratorsLoaded = true;
		}

		if (!_RawTriggersLoaded)
		{
			Requesting = true;
			_RawTriggers = DslSchemaFactory.GetSchema(_Connection, "Triggers", null);
			Requesting = false;

			_RawTriggersLoaded = true;
		}


		if (!SequencesLoaded)
		{
			BuildSequenceTable();
		}

		if (!_Loaded)
		{
			BuildTriggerTable();
		}

		_SyncActive = false;
	}



	public void BuildSequenceTable()
	{
		Stopwatch.Reset();
		Stopwatch.Start();

		Sequences.Columns.Add("GENERATOR_CATALOG", typeof(string));
		Sequences.Columns.Add("GENERATOR_SCHEMA", typeof(string));
		Sequences.Columns.Add("SEQUENCE_GENERATOR", typeof(string));
		Sequences.Columns.Add("IS_SYSTEM_FLAG", typeof(int));
		Sequences.Columns.Add("GENERATOR_ID", typeof(short));
		Sequences.Columns.Add("GENERATOR_IDENTITY", typeof(int));
		Sequences.Columns.Add("IDENTITY_SEED", typeof(long));
		Sequences.Columns.Add("IDENTITY_INCREMENT", typeof(int));
		Sequences.Columns.Add("IDENTITY_CURRENT", typeof(long)); 
		Sequences.Columns.Add("TRIGGER_NAME", typeof(string));

		Sequences.PrimaryKey = new DataColumn[] { Sequences.Columns["SEQUENCE_GENERATOR"] };

		Sequences.AcceptChanges();



		Sequences.BeginLoadData();

		DataRow seq;

		foreach (DataRow row in _RawGenerators.Rows)
		{
			seq = Sequences.NewRow();

			seq.ItemArray = row.ItemArray;
			seq["TRIGGER_NAME"] = DBNull.Value;

			Sequences.Rows.Add(seq);
		}

		Sequences.EndLoadData();
		Sequences.AcceptChanges();

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

		Triggers.Columns.Add("TABLE_CATALOG", typeof(string));
		Triggers.Columns.Add("TABLE_SCHEMA", typeof(string));
		Triggers.Columns.Add("TABLE_NAME", typeof(string));
		Triggers.Columns.Add("TRIGGER_NAME", typeof(string));
		Triggers.Columns.Add("DESCRIPTION", typeof(string));
		Triggers.Columns.Add("IS_SYSTEM_FLAG", typeof(int));
		Triggers.Columns.Add("TRIGGER_TYPE", typeof(long));
		Triggers.Columns.Add("IS_INACTIVE", typeof(bool));
		Triggers.Columns.Add("PRIORITY", typeof(short));
		Triggers.Columns.Add("EXPRESSION", typeof(string));
		Triggers.Columns.Add("IS_IDENTITY", typeof(bool));
		Triggers.Columns.Add("SEQUENCE_GENERATOR", typeof(string));
		Triggers.Columns.Add("DEPENDENCY_FIELDS", typeof(string));
		Triggers.Columns.Add("IDENTITY_SEED", typeof(long));
		Triggers.Columns.Add("IDENTITY_INCREMENT", typeof(int));
		Triggers.Columns.Add("IDENTITY_TYPE", typeof(short));
		Triggers.Columns.Add("DEPENDENCY_FIELD", typeof(string));
		Triggers.Columns.Add("DEPENDENCY_COUNT", typeof(int));
		Triggers.Columns.Add("IDENTITY_CURRENT", typeof(long));

		Triggers.PrimaryKey = new DataColumn[] { Triggers.Columns["TRIGGER_NAME"] };

		Triggers.AcceptChanges();


		Triggers.BeginLoadData();

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

			trig = Triggers.NewRow();

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
					seq = Sequences.Rows.Find(key);

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


			Triggers.Rows.Add(trig);

		}


		Triggers.EndLoadData();
		Triggers.AcceptChanges();

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


	protected DataTable GetRawGeneratorSchema()
	{
		DslRawGenerators schema = new(this);

		return schema.GetRawSchema(_Connection, "Generators");
	}

	protected async Task<DataTable> GetRawGeneratorSchemaAsync()
	{
		DslRawGenerators schema = new(this);

		return await schema.GetRawSchemaAsync(_Connection, "Generators").ConfigureAwait(false);
	}


	protected DataTable GetRawTriggerSchema()
	{
		DslRawTriggers schema = new(this);

		return schema.GetRawSchema(_Connection, "Triggers");
	}

	protected async Task<DataTable> GetRawTriggerSchemaAsync()
	{
		DslRawTriggers schema = new(this);

		return await schema.GetRawSchemaAsync(_Connection, "Triggers").ConfigureAwait(false);
	}


	protected DataTable GetRawTriggerGeneratorSchema()
	{
		DslRawTriggerGenerators schema = new(this);

		return schema.GetRawSchema(_Connection, "TriggerGenerators");
	}

	protected async Task<DataTable> GetRawTriggerGeneratorSchemaAsync()
	{
		DslRawTriggerGenerators schema = new(this);

		return await schema.GetRawSchemaAsync(_Connection, "TriggerGenerators").ConfigureAwait(false);
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
			Triggers.DefaultView.RowFilter = where.ToString();
			return Triggers.DefaultView.ToTable();
		}

		Triggers.DefaultView.RowFilter = null;

		return Triggers;

	}

	public async Task<DataTable> GetTriggerSchemaAsync(string[] restrictions, int systemFlag, int identityFlag)
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
			Sequences.DefaultView.RowFilter = where.ToString();
			return Sequences.DefaultView.ToTable();
		}

		Sequences.DefaultView.RowFilter = null;

		return Sequences;

	}


	public async Task<DataTable> GetSequenceSchemaAsync(string[] restrictions)
	{
		await Task.CompletedTask.ConfigureAwait(false);
		return GetSequenceSchema(restrictions);
	}

	void ConnectiontionStateChanged(object sender, StateChangeEventArgs e)
	{
		if ((e.CurrentState & (ConnectionState.Closed | ConnectionState.Broken)) != 0)
		{
			Reset();
		}
		else if ((e.OriginalState & (ConnectionState.Closed | ConnectionState.Broken)) != 0
			&& (e.CurrentState & (ConnectionState.Closed | ConnectionState.Broken)) == 0)
		{
			_Connection = (FbConnection)sender;
			if (ClearToLoadAsync)
				AsyncLoad();
		}
	}

	void ConnectionDisposed(object sender, EventArgs e)
	{
		_Instances.Remove(_Connection);
		_Connection = null;
	}

	void Reset()
	{
		_Sequences = null;
		_Triggers = null;

		_RawGenerators = null;
		_RawTriggerGenerators = null;
		_RawTriggers = null;

		_RawGeneratorsLoaded = false;
		_RawTriggerGeneratorsLoaded = false;
		_RawTriggersLoaded = false;

		_SequencesLoaded = false;
		_Loaded = false;
		_SyncActive = false;
		_Requesting = false;

		_AsyncActive = false;
		_AsyncLocked = false;

		_Stopwatch.Reset();
	}
}
