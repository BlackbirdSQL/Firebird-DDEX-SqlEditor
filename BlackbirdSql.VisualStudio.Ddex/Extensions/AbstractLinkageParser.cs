// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TaskStatusCenter;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.VisualStudio.Ddex.Schema;

namespace BlackbirdSql.Common.Extensions;

/// <summary>
/// Handles Trigger / Generator linkage building tasks.
/// </summary>
internal abstract class AbstractLinkageParser : AbstruseLinkageParser
{
	protected enum EnumLinkStage
	{
		Start = 0,
		GeneratorsLoaded = 1,
		TriggerGeneratorsLoaded = 2,
		TriggersLoaded = 3,
		SequencesLoaded = 4,
		Completed = 5,
		Clear = 6
	}

	protected bool _Enabled = true;
	protected long _Elapsed = 0;

	protected static Dictionary<FbConnection, object> _Instances = null;


	protected DataTable _Sequences = null;
	protected DataTable _Triggers = null;

	protected DataTable _RawGenerators = null;
	protected DataTable _RawTriggerGenerators = null;
	protected DataTable _RawTriggers = null;


	protected EnumLinkStage _LinkStage = EnumLinkStage.Start;

	// An async call is active
	protected int _SyncActive = 0;
	// Sync is making a call to GetSchema. GetSchema must allow it through.
	protected bool _Requesting = false;

	protected TaskProgressData _ProgressData = default;
	protected ITaskHandler _TaskHandler = null;


	// An async call is active
	protected bool _AsyncActive = false;
	// A sync call has taken over. Async is locked out or abort at the first opportunity.
	protected CancellationTokenSource _AsyncTokenSource = null;
	protected CancellationToken _AsyncToken;


	protected Task<bool> _AsyncTask;


	public bool Enabled { get { return _Enabled; } }

	public bool SequencesLoaded { get { return _LinkStage >= EnumLinkStage.SequencesLoaded; } }


	public bool ConnectionActive
	{
		get
		{
			return (_Connection != null && (_Connection.State & (ConnectionState.Closed | ConnectionState.Broken)) == 0);
		}
	}

	public bool Loaded { get { return _LinkStage >= EnumLinkStage.Completed; } }

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
			else
			{
				Stopwatch.Stop();
				_Elapsed += Stopwatch.ElapsedMilliseconds;
			}
			_Requesting = value;
		}
	}


	public bool ClearToLoad
	{
		get { return (_SyncActive == 0 && !Loaded); }
	}

	public bool ClearToLoadAsync
	{
		get
		{
			return (_Enabled && !_AsyncActive && !_AsyncToken.IsCancellationRequested && ClearToLoad);
		}
	}




	protected AbstractLinkageParser(FbConnection connection) : base(connection)
	{
		_Instances.Add(connection, this);

		_Connection.StateChange += ConnectionStateChanged;
		_Connection.Disposed += ConnectionDisposed;


		_AsyncTask = null; 


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
		_Sequences.Columns.Add("DEPENDENCY_TRIGGER", typeof(string));
		_Sequences.Columns.Add("DEPENDENCY_TABLE", typeof(string));
		_Sequences.Columns.Add("DEPENDENCY_FIELD", typeof(string));

		_Sequences.PrimaryKey = new DataColumn[] { _Sequences.Columns["SEQUENCE_GENERATOR"] };

		_Sequences.AcceptChanges();


		_Triggers = new();

		_Triggers.Columns.Add("TABLE_CATALOG", typeof(string));
		_Triggers.Columns.Add("TABLE_SCHEMA", typeof(string));
		_Triggers.Columns.Add("TRIGGER_NAME", typeof(string));
		_Triggers.Columns.Add("TABLE_NAME", typeof(string));
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




	protected static AbstractLinkageParser Instance(FbConnection connection)
	{
		AbstractLinkageParser parser = null;

		if (connection == null)
		{
			ArgumentNullException ex = new ArgumentNullException("Attempt to add a null connection");
			Diag.Dug(ex);
			throw ex;
		}

		if (_Instances == null)
		{
			_Instances = new();
		}
		else
		{
			if (_Instances.TryGetValue(connection, out object parserObject))
			{
				parser = (AbstractLinkageParser)parserObject;
			}
		}

		return parser;
	}


	~AbstractLinkageParser()
	{
		_AsyncTokenSource.Cancel();
		_AsyncTokenSource?.Dispose();
	}


	public abstract bool SyncEnter();

	public abstract void SyncExit();

	public abstract bool Execute();


	public abstract bool PopulateLinkageTables(CancellationToken asyncToken = default, CancellationToken userToken = default);


	public abstract bool AsyncExecute(int delay = 0, int multiplier = 1);


	protected abstract bool AsyncExit();


	protected abstract bool TaskHandlerProgress(string stage, int progress, long elapsed);

	protected abstract Task<bool> TaskHandlerProgressAsync(string stage, int progress, long elapsed);

	protected abstract bool UpdateStatusBar(EnumLinkStage stage, bool isAsync);

	protected abstract Task<bool> UpdateStatusBarAsync(EnumLinkStage stage, bool isAsync);


	protected DataTable GetRawGeneratorSchema()
	{
		DslRawGenerators schema = new();

		try
		{
			Requesting = true;
			_RawGenerators = schema.GetRawSchema(_Connection, "Generators");
			Requesting = false;
		}
		catch (Exception ex) 
		{
			Diag.Dug(ex);
			throw;
		}

		TaskHandlerProgress("SELECT Generators", 13, Stopwatch.ElapsedMilliseconds);

		return _RawGenerators;
	}


protected DataTable GetRawTriggerSchema()
	{
		DslRawTriggers schema = new();

		try
		{ 
			Requesting = true;
			_RawTriggers = schema.GetRawSchema(_Connection, "Triggers");
			Requesting = false;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		TaskHandlerProgress("SELECT Triggers", 88, Stopwatch.ElapsedMilliseconds);

		return _RawTriggers;
	}


	protected DataTable GetRawTriggerGeneratorSchema()
	{
		DslRawTriggerGenerators schema = new();

		try
		{ 
			Requesting = true;
			_RawTriggerGenerators = schema.GetRawSchema(_Connection, "TriggerGenerators");
			Requesting = false;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		TaskHandlerProgress("SELECT TriggerGenerators", 43, Stopwatch.ElapsedMilliseconds);

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
			seq["DEPENDENCY_TRIGGER"] = DBNull.Value;
			seq["DEPENDENCY_TABLE"] = DBNull.Value;
			seq["DEPENDENCY_FIELD"] = DBNull.Value;

			_Sequences.Rows.Add(seq);
		}

		_Sequences.EndLoadData();
		_Sequences.AcceptChanges();

		_RawGenerators = null;
		_LinkStage = EnumLinkStage.SequencesLoaded;

		Stopwatch.Stop();
		_Elapsed += Stopwatch.ElapsedMilliseconds;

		TaskHandlerProgress("Build Sequence Table", 94, Stopwatch.ElapsedMilliseconds);
	}

	

	public void BuildTriggerTable()
	{
		Stopwatch.Reset();
		Stopwatch.Start();


		_Triggers.BeginLoadData();

		bool isIdentity;
		int increment;
		int seed;
		int dependencyCount;
		int result = 1;
		int i, j;
		string genId;
		string trigKey;
		string trigGenKey = null;

		DataRow seq;
		DataRow trig;
		DataRow trigGenRow = null;
		object[] key;


		System.Collections.IEnumerator enumerator = _RawTriggerGenerators.Rows.GetEnumerator();

		if (enumerator.MoveNext())
		{
			trigGenRow = enumerator.Current as DataRow;
			trigGenKey = trigGenRow["TRIGGER_NAME"].ToString();
		}

		foreach (DataRow row in _RawTriggers.Rows)
		{
			trigKey = row["TRIGGER_NAME"].ToString();

			// We do a staggered enumeration of the two raw trigger tables to avoid doing a find on each trigger.
			while (trigGenRow != null && (result = string.Compare(trigKey, trigGenKey, StringComparison.OrdinalIgnoreCase)) > 0)
			{
				if (enumerator.MoveNext())
				{
					trigGenRow = enumerator.Current as DataRow;
					trigGenKey = trigGenRow["TRIGGER_NAME"].ToString();
				}
				else
				{
					trigGenRow = null;
					result = 1;
					break;
				}
			}

			// if (result < 0)
			//	continue;

			trig = _Triggers.NewRow();

			for (i = -2; i < _RawTriggers.Columns.Count; i++)
			{
				try
				{
					if (i < 0)
						trig[i+2] = DBNull.Value;
					else
						trig[i+2] = row[i];
				}
				catch (Exception ex)
				{
					Diag.Dug(ex, String.Format("Trig Error at i:{0}.", i));
				}
			}

			for (j = 1, i += 2; j < _RawTriggerGenerators.Columns.Count; j++, i++)
			{
				if (result != 0)
				{
					trig[i] = DBNull.Value;
				 	continue;
				}

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
				(genId, increment, seed) = ParseGeneratorInfo(trig["EXPRESSION"].ToString(), trig["TRIGGER_NAME"].ToString(),
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
						trig["IDENTITY_SEED"] = seq["IDENTITY_SEED"];

						if (seq["DEPENDENCY_TRIGGER"] == DBNull.Value || seq["DEPENDENCY_TRIGGER"].ToString() == "")
							seq["DEPENDENCY_TRIGGER"] = trig["TRIGGER_NAME"];
						else
							seq["DEPENDENCY_TRIGGER"] = seq["DEPENDENCY_TRIGGER"].ToString() + ", " + trig["TRIGGER_NAME"].ToString();

						seq["DEPENDENCY_TABLE"] = trig["TABLE_NAME"];
						seq["DEPENDENCY_FIELD"] = trig["DEPENDENCY_FIELD"];
					}
				}

			}

			UpdateTriggerData(trig, genId, isIdentity, dependencyCount);


			_Triggers.Rows.Add(trig);

		}


		_Triggers.EndLoadData();
		_Triggers.AcceptChanges();

		_LinkStage = EnumLinkStage.Completed;
		_RawTriggers = null;
		_RawTriggerGenerators = null;

		Stopwatch.Stop();
		_Elapsed += Stopwatch.ElapsedMilliseconds;

		TaskHandlerProgress("Total processing time was", 100, _Elapsed);

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
			Execute();

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

	public DataTable GetSequenceSchema(string[] restrictions)
	{
		if (!Loaded)
			Execute();

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

	public DataRow LocateIdentityTrigger(object objTable, object objField)
	{
		string table = objTable.ToString();
		string field = objField.ToString();

		foreach (DataRow row in _Triggers.Rows)
		{
			if (row["TABLE_NAME"].ToString() == table && row["DEPENDENCY_FIELD"].ToString() == field && Convert.ToBoolean(row["IS_IDENTITY"]) == true)
				return row;
		}

		return null;
	}

	void ConnectionStateChanged(object sender, StateChangeEventArgs e)
	{
		if ((e.CurrentState & (ConnectionState.Closed | ConnectionState.Broken)) != 0)
		{
			_AsyncTokenSource.Cancel();
		}
		else if ((e.OriginalState & (ConnectionState.Closed | ConnectionState.Broken)) != 0
			&& (e.CurrentState & (ConnectionState.Closed | ConnectionState.Broken)) == 0)
		{
			_Connection = (FbConnection)sender;
			if (ClearToLoadAsync)
				AsyncExecute();
		}
	}



	protected static void ConnectionDisposed(object sender, EventArgs e)
	{
		if (sender is not FbConnection connection)
		{
			return;
		}


		if (_Instances.TryGetValue(connection, out object parser))
		{
			_Instances.Remove(connection);
			((AbstractLinkageParser)parser)._Connection = null;

		}
	}
}
