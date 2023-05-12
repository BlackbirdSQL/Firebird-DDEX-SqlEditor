// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TaskStatusCenter;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.VisualStudio.Ddex.Schema;
using BlackbirdSql.VisualStudio.Ddex.Configuration;
using Microsoft.VisualStudio.RpcContracts;
using Extensibility;

namespace BlackbirdSql.Common.Extensions;



// =========================================================================================================
//										AbstractLinkageParser Class
//
/// <summary>
/// Handles Trigger / Generator linkage building tasks of the LinkageParser class.
/// </summary>
// =========================================================================================================
internal abstract class AbstractLinkageParser : AbstruseLinkageParser, ITaskHandlerClient
{


	// -----------------------------------------------------------------------------------------------------
	#region Internal types - AbstractLinkageParser
	// -----------------------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enum of the linkage build stages. After each stage cancellation tokens are
	/// checked.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected enum EnumLinkStage
	{
		Start = 0,
		GeneratorsLoaded = 1,
		TriggerDependenciesLoaded = 2,
		TriggersLoaded = 3,
		SequencesPopulated = 4,
		Completed = 5
	}


	#endregion Internal types





	// =========================================================================================================
	#region Variables - AbstractLinkageParser
	// =========================================================================================================


	/// <summary>
	/// True when async operations are active else false.
	/// </summary>
	protected bool _AsyncActive = false;

	/// <summary>
	/// Handle to the async task if it exists.
	/// </summary>
	protected Task<bool> _AsyncTask;

	/// <summary>
	/// Cancellation token for async operations. 
	/// </summary>
	protected CancellationToken _AsyncToken;

	/// <summary>
	/// Cancellation token source for async operations. 
	/// </summary>
	protected CancellationTokenSource _AsyncTokenSource = null;

	/// <summary>
	/// The total elapsed time in milliseconds that the parser was actively
	/// building the linkage tables. 
	/// </summary>
	protected long _Elapsed = 0;

	/// <summary>
	/// Parser status inidicator that is set to false if the user cancels async
	/// operations in the IDE task handler.
	/// </summary>
	protected bool _Enabled = true;

	/// <summary>
	/// Per connection LinkageParser instances xref.
	/// </summary>
	protected static Dictionary<FbConnection, object> _Instances = null;


	/// <summary>
	/// The tracked linkage build process stage.
	/// </summary>
	/// <remarks>
	/// The async linkage process can be interrupted by either a user TaskHandler
	/// cancellation or when the UI thread requires the trigger or sequence tables,
	/// in which case the UI thread takes over the linkage process.
	/// </remarks>
	protected EnumLinkStage _LinkStage = EnumLinkStage.Start;

	/// <summary>
	/// Handle to the ITaskHandler ProgressData.
	/// </summary>
	protected TaskProgressData _ProgressData = default;

	/// <summary>
	/// The intermediate full SELECT of the system generator table.
	/// </summary>
	protected DataTable _RawGenerators = null;

	/// <summary>
	/// The intermediate full SELECT of the system trigger table.
	/// </summary>
	protected DataTable _RawTriggers = null;

	/// <summary>
	/// The intermediate full SELECT of the system trigger table dependencies.
	/// </summary>
	/// <remarks>
	/// Dependent on network speed, splitting the trigger and trigger dependencies
	/// SELECT statements is up to +- 80% faster than a combined statement.
	/// </remarks>
	protected DataTable _RawTriggerDependencies = null;

	/// <summary>
	/// Set to true when the parser is making an external database request and then back
	/// to false once the request is completed.
	/// </summary>
	protected bool _Requesting = false;

	/// <summary>
	/// The final populated generator table with trigger linkage.
	/// </summary>
	protected DataTable _Sequences = null;

	/// <summary>
	/// Increment of UI thread calls. Once _SyncActive is back down to zero, async
	/// operations will continue provided 'Enabled' is still true.
	/// </summary>
	protected int _SyncActive = 0;

	/// <summary>
	/// Handle to the IDE ITaskHandler.
	/// </summary>
	protected ITaskHandler _TaskHandler = null;

	/// <summary>
	/// The final populated trigger table with generator linkage.
	/// </summary>
	protected DataTable _Triggers = null;


	#endregion Variables





	// =========================================================================================================
	#region Property accessors - AbstractLinkageParser
	// =========================================================================================================


	/// <summary>
	/// Getter indicating wether or not the UI thread can and should resume linkage operations.
	/// </summary>
	public bool ClearToLoad
	{
		get { return (_SyncActive == 0 && !Loaded); }
	}


	/// <summary>
	/// Getter inidicating whether or not async linkage can and should begin or resume operations.
	/// </summary>
	public bool ClearToLoadAsync
	{
		get
		{
			return (_Enabled && !_AsyncActive && !Loaded && _SyncActive == 0
				&& !_AsyncToken.IsCancellationRequested && ConnectionActive );
		}
	}


	/// <summary>
	/// Getter inidicating whther or not the parser's db connection is active and open.
	/// </summary>
	public bool ConnectionActive
	{
		get
		{
			return (_Connection != null
				&& (_Connection.State & ConnectionState.Open) != 0
				&& (_Connection.State & (ConnectionState.Closed | ConnectionState.Broken)) == 0);
		}
	}


	/// <summary>
	/// Getter to the parser status indicator. Returns false if the user has cancelled async
	/// operations through the IDE task handler.
	/// </summary>
	public bool Enabled { get { return _Enabled; } }


	/// <summary>
	/// Getter indicating whether or not linkage has completed.
	/// </summary>
	public bool Loaded { get { return _LinkStage >= EnumLinkStage.Completed; } }


	/// <summary>
	/// Sets the start and end of an external db request.
	/// </summary>
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


	/// <summary>
	/// Getter indicating whether or not the parser has fetched the generators.
	/// </summary>
	public bool SequencesPopulated { get { return _LinkStage >= EnumLinkStage.SequencesPopulated; } }


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - AbstractLinkageParser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected default .ctor for creating an instance for a db connection.
	/// </summary>
	/// <param name="connection"></param>
	// ---------------------------------------------------------------------------------
	protected AbstractLinkageParser(FbConnection connection) : base(connection)
	{
		// Diag.Trace("new connection");

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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns an instance of the LinkageParser for a connection or creates one if it
	/// doesn't exist.
	/// </summary>
	/// <param name="connection">
	/// The db connection uniquely and distinctly associated with the parser
	/// instance.
	/// </param>
	/// <returns>The distinctly unique parser associated with the db connection.</returns>
	// ---------------------------------------------------------------------------------
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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Destructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	~AbstractLinkageParser()
	{
		_AsyncTokenSource.Cancel();
		_AsyncTokenSource?.Dispose();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Abstract method declarations - AbstractLinkageParser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Begins or resumes asynchronous linkage build operations.
	/// </summary>
	/// <param name="delay">
	/// The delay in milliseconds before beginning operations if an async build was
	/// initiated through the creation of a new data connection in the SE. A delay is
	/// required to allow the SE time to render the initial root node.
	/// </param>
	/// <param name="multiplier">
	/// A multiplier to split the delay into smaller timeslices and allow checking of
	/// cancellation tokens during the total delay time of 'delay * multiplier'.
	/// </param>
	/// <returns>True if the linkage was successfully completed, else false.</returns>
	public abstract bool AsyncExecute(int delay = 0, int multiplier = 1);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches the UI thread build of the linkage tables if the UI requires them. If
	/// an async build is in progress, waits for the active operation to complete and
	/// then switches over to a UI thread build for the remaining tasks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract bool Execute();


	#endregion Abstract method declarations





	// =========================================================================================================
	#region Internal Methods - AbstractLinkageParser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Populates the internal generator table (with linkage) that will be used in
	/// future schema requests for the generators.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void BuildSequenceTable()
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
		_LinkStage = EnumLinkStage.SequencesPopulated;

		Stopwatch.Stop();
		_Elapsed += Stopwatch.ElapsedMilliseconds;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Populates the internal trigger table (with linkage) that will be used in future
	/// schema requests for triggers.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void BuildTriggerTable()
	{
		Stopwatch.Reset();
		Stopwatch.Start();


		_Triggers.BeginLoadData();
		_Sequences.BeginLoadData();

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


		System.Collections.IEnumerator enumerator = _RawTriggerDependencies.Rows.GetEnumerator();

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
						trig[i + 2] = DBNull.Value;
					else
						trig[i + 2] = row[i];
				}
				catch (Exception ex)
				{
					Diag.Dug(ex, String.Format("Trig Error at i:{0}.", i));
				}
			}

			for (j = 1, i += 2; j < _RawTriggerDependencies.Columns.Count; j++, i++)
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
				(genId, increment, seed) = ParseTriggerDSL(trig["EXPRESSION"].ToString(), trig["TRIGGER_NAME"].ToString(),
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

		_Sequences.EndLoadData();
		_Sequences.AcceptChanges();

		_LinkStage = EnumLinkStage.Completed;
		_RawTriggers = null;
		_RawTriggerDependencies = null;
		_DslParser = null;

		Stopwatch.Stop();
		_Elapsed += Stopwatch.ElapsedMilliseconds;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an external full SELECT of the database system generator table.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected DataTable GetRawGeneratorSchema()
	{
		DslRawGenerators schema = new();

		Requesting = true;

		try
		{
			_RawGenerators = schema.GetRawSchema(_Connection, "Generators");
		}
		catch
		{
			try
			{
				// Try reset
				_Connection.Close();
				_Connection.Open();

				if (!ConnectionActive)
				{
					Microsoft.VisualStudio.Data.DataProviderException ex = new("Connection closed");
					throw ex;
				}

				_RawGenerators = schema.GetRawSchema(_Connection, "Generators");
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}
		finally
		{
			Requesting = false;
		}


		if (_RawGenerators != null)
			_LinkStage = EnumLinkStage.GeneratorsLoaded;

		return _RawGenerators;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an external full SELECT of the database trigger dependencies.
	/// Splitting the dependency fetch from <see cref="GetRawTriggerSchema"/> improves
	/// the overall fetch time by +-80%.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected DataTable GetRawTriggerDependenciesSchema()
	{
		DslRawTriggerDependencies schema = new();


		Requesting = true;

		try
		{
			_RawTriggerDependencies = schema.GetRawSchema(_Connection, "TriggerDependencies");
		}
		catch
		{
			try
			{
				// Try reset
				_Connection.Close();
				_Connection.Open();

				if (!ConnectionActive)
				{
					Microsoft.VisualStudio.Data.DataProviderException ex = new("Connection closed");
					throw ex;
				}

				_RawTriggerDependencies = schema.GetRawSchema(_Connection, "TriggerDependencies");
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}
		finally
		{
			Requesting = false;
		}


		if (_RawTriggerDependencies != null)
			_LinkStage = EnumLinkStage.TriggerDependenciesLoaded;
		
		return _RawTriggerDependencies;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an external full SELECT of the database system trigger table.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected DataTable GetRawTriggerSchema()
	{
		DslRawTriggers schema = new();

		Requesting = true;

		try
		{
			_RawTriggers = schema.GetRawSchema(_Connection, "Triggers");
		}
		catch
		{
			try
			{
				// Try reset
				_Connection.Close();
				_Connection.Open();

				if (!ConnectionActive)
				{
					Microsoft.VisualStudio.Data.DataProviderException ex = new("Connection closed");
					throw ex;
				}

				_RawTriggers = schema.GetRawSchema(_Connection, "Triggers");
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}
		finally
		{
			Requesting = false;
		}

		if (_RawTriggers != null)
			_LinkStage = EnumLinkStage.TriggersLoaded;


		return _RawTriggers;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates a row of the internal trigger table with identity linkage information.
	/// </summary>
	// ---------------------------------------------------------------------------------
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


	#endregion Internal Methods





	// =========================================================================================================
	#region Public Methods - AbstractLinkageParser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds a trigger in the internal linked trigger table and returns it's row else
	/// return null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public DataRow FindTrigger(object name)
	{
		return _Triggers.Rows.Find(name);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a generator table GetSchema() request using the internal linked sequence
	/// table.
	/// </summary>
	// ---------------------------------------------------------------------------------
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a trigger table GetSchema() request using the internal linked trigger
	/// table.
	/// </summary>
	// ---------------------------------------------------------------------------------
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Locates and returns the internal linked trigger table row for an identity
	/// column else returns null.
	/// </summary>
	// ---------------------------------------------------------------------------------
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


	#endregion Public Methods





	// =========================================================================================================
	#region Utility Methods - AbstractLinkageParser
	// =========================================================================================================



	public ITaskHandler GetTaskHandler()
	{
		return _TaskHandler;
	}


	public TaskProgressData GetProgressData()
	{
		return _ProgressData;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the IDE task handler progress bar and possibly the output pane.
	/// </summary>
	/// <param name="stage">The descriptive name of the completed stage</param>
	/// <param name="progress">The % completion of the linkage build.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	protected bool TaskHandlerProgress(string stage, int progress, long elapsed)
	{
		bool completed = false;
		string text;

		if (progress == 0)
		{
			text = "Updating sequence linkage.";
		}
		else if (progress == 100)
		{
			completed = true;
			text = $"{progress}% completed. {stage} took {elapsed}ms.\nLinkage completed in {_Elapsed}ms.";
		}
		else
		{
			if (_AsyncActive)
			{
				// If it's a user cancel request.
				if (!_Enabled)
				{
					completed = true;
					text = $">  Cancelled. {progress}% completed. {stage} took {elapsed}ms.";
				}
				else
				{
					if (elapsed == -1)
						text = ">  Resuming async sequence linkage.";
					else
						text = $"{progress}% completed. {stage} took {elapsed}ms.";

				}
			}
			else
			{
				if (elapsed == -1)
					text = ">  Switched sequence linkage to UI thread.";
				else
					text = $"{progress}% completed. {stage} took {elapsed}ms.";
			}

		}

		return Diag.TaskHandlerProgress(this, text, progress, completed);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the status bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool UpdateStatusBar(EnumLinkStage stage, bool isAsync)
	{
		// string async;
		string catalog = $"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)})";

		bool clear = false;
		string text;

		if (stage == EnumLinkStage.Start)
		{
			// async = isAsync ? " (async)" : "";
			text = $"Updating {catalog} sequence linkage.";
		}
		else if (stage == EnumLinkStage.Completed)
		{
			// async = isAsync ? " (async)" : "";
			text = $"Completed {catalog} sequence linkage in {_Elapsed}ms.";
		}
		else
		{
			if (isAsync)
			{
				// If it's a user cancel request.
				if (!_Enabled)
					text = $">  Cancelled {catalog} sequence linkage.";
				else
					text = $"Resuming {catalog} sequence linkage.";
			}
			else
			{
				if (!_Enabled)
					text = $"Resuming {catalog} sequence linkage.";
				else
					text = $"Switched {catalog} sequence linkage to UI thread.";
			}
		}

		if (stage == EnumLinkStage.Completed || (isAsync && !_Enabled))
		{
			clear = true;
		}

		return Diag.UpdateStatusBar(text, clear);
	}



	protected int GetPercentageComplete(EnumLinkStage stage)
	{
		switch (stage)
		{
			case EnumLinkStage.Start:
				return 0;
			case EnumLinkStage.GeneratorsLoaded:
				return 13;
			case EnumLinkStage.TriggerDependenciesLoaded:
				return 43;
			case EnumLinkStage.TriggersLoaded:
				return 88;
			case EnumLinkStage.SequencesPopulated:
				return 94;
			default:
				return 100;
		}
	}

	#endregion Utility methods





	// =========================================================================================================
	#region Event handlers - AbstractLinkageParser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for a LinkageParser's db connection state change.
	/// </summary>
	// ---------------------------------------------------------------------------------
	void ConnectionStateChanged(object sender, StateChangeEventArgs e)
	{
		if (_AsyncTokenSource != null && (e.CurrentState & (ConnectionState.Closed | ConnectionState.Broken)) != 0)
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Event handler for handling the Disposed event of a connection, removing the xref
	/// in the <see cref="_Instances"/> dictionary.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void ConnectionDisposed(object sender, EventArgs e)
	{
		if (_Connection == null)
			return;

		// Diag.Trace();

		_Instances.Remove(_Connection);
		_Connection = null;
	}


	#endregion Event handlers


}
