// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Data.Properties;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Data.Model;

// =========================================================================================================
//										AbstractLinkageParser Class
//
/// <summary>
/// Handles Trigger / Generator linkage building tasks of the LinkageParser class.
/// </summary>
// =========================================================================================================
public abstract class AbstractLinkageParser : AbstruseLinkageParser
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractLinkageParser
	// ---------------------------------------------------------------------------------



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected default .ctor for creating a Transient instance with restrictions.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected AbstractLinkageParser(string connectionString, string[] restrictions) : base()
	{
		// Tracer.Trace(typeof(AbstractLinkageParser), $"AbstractLinkageParser(FbConnection, AbstractLinkageParser)");

		if (restrictions == null || restrictions.Length < 3 || string.IsNullOrWhiteSpace(restrictions[2]))
		{
			Diag.ThrowException(new ArgumentNullException(nameof(restrictions), "Value is null or index 2 is null."));
		}


		_ConnectionString = connectionString;
		_ConnectionUrl = ApcManager.CreateConnectionUrl(_ConnectionString);
		_TransientRestrictions = (string[])restrictions.Clone();

		lock(_LockGlobal)
			_TransientInstance = this;

		CreateLinkTables();
	}

	/// <summary>
	/// Protected default .ctor for creating an unregistered clone.
	/// Callers must make a call to EnsureLoaded() for rhs beforehand.
	/// </summary>
	protected AbstractLinkageParser(AbstractLinkageParser rhs) : base()
	{
		// Tracer.Trace(typeof(AbstractLinkageParser), $"AbstractLinkageParser(FbConnection, AbstractLinkageParser)");

		// Callers must EnsureLoaded().

		_IsIntransient = rhs._IsIntransient;
		_ConnectionString = rhs._ConnectionString;
		_ConnectionUrl = rhs._ConnectionUrl;
		_Sequences = rhs._Sequences.Copy();
		_Triggers = rhs._Triggers.Copy();

		_LinkStage = EnLinkStage.Completed;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected default .ctor for creating an instance clone for a db connection.
	/// </summary>
	/// <param name="connection"></param>
	// ---------------------------------------------------------------------------------
	protected AbstractLinkageParser(IVsDataExplorerConnection root, AbstractLinkageParser rhs) : base()
	{
		// Tracer.Trace(typeof(AbstractLinkageParser), $"AbstractLinkageParser(FbConnection, AbstractLinkageParser)");

		bool rhsValid = rhs != null && rhs.Loaded;

		_Instances ??= [];
		_Instances.Add(root, this);


		_InstanceRoot = root;


		if (!rhsValid)
		{
			_ConnectionString = root.DecryptedConnectionString();
			_ConnectionUrl = ApcManager.CreateConnectionUrl(_ConnectionString);

			CreateLinkTables();
		}
		else
		{
			_IsIntransient = rhs._IsIntransient;
			_ConnectionString = rhs._ConnectionString;
			_ConnectionUrl = rhs._ConnectionUrl;
			_Sequences = rhs._Sequences.Copy();
			_Triggers = rhs._Triggers.Copy();

			_LinkStage = EnLinkStage.Completed;
		}
	}



	protected abstract object Clone();


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
	protected static AbstractLinkageParser GetInstanceImpl(IVsDataExplorerConnection root)
	{
		// Tracer.Trace(typeof(AbstractLinkageParser), "GetInstance(FbConnection)");

		if (root == null)
		{
			ArgumentNullException ex = new ArgumentNullException(Resources.ExceptionAttemptToAddNullConnection);
			Diag.Dug(ex);
			throw ex;
		}

		// Tracer.Trace(typeof(AbstractLinkageParser), "GetInstance(FbConnection)");

		if (_Instances == null || !_Instances.TryGetValue(root, out IBsNativeDbLinkageParser parser))
		{
			parser = null;
		}

		return (AbstractLinkageParser)parser;
	}


	~AbstractLinkageParser()
	{
		Dispose(true);
	}


	public override void Dispose()
	{
		Dispose(true);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disposes of a parser.
	/// </summary>
	/// <param name="disposing">
	/// If disposing is set to true, then all parsers with weak equivalency will
	/// be tagged as intransient, meaning their trigger linkage databases cannot
	/// be copied to another parser with weak equivalency. 
	/// </param>
	/// <returns>True of the parser was found and disposed else false.</returns>
	// -------------------------------------------------------------------------
	protected override bool Dispose(bool disposing)
	{
		// Tracer.Trace(typeof(AbstractLinkageParser), "Dispose(bool)", "isValidTransient: {0}.", isValidTransient);

		if (_InstanceRoot == null || _Instances == null)
			return false;

		ApcManager.InvalidateRctManager();

		Disable();

		if (disposing)
			InvalidateEquivalentParsers(_ConnectionString);

		// Tracer.Trace(typeof(AbstractLinkageParser), "Dispose(bool)", "DISPOSING OF PARSER. isValidTransient: {0}.", isValidTransient);

		_Instances.Remove(_InstanceRoot);

		if (_Instances.Count == 0)
			_Instances = null;

		_InstanceRoot = null;
		_ConnectionString = null;

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disposes of a parser given an IVsDataConnection site.
	/// </summary>
	/// <param name="site">
	/// The IVsDataConnection explorer connection object
	/// </param>
	/// <param name="disposing">
	/// If disposing is set to true, then all parsers with weak equivalency will
	/// be tagged as intransient, meaning their trigger linkage databases cannot
	/// be copied to another parser with weak equivalency. 
	/// </param>
	/// <returns>True of the parser was found and disposed else false.</returns>
	// -------------------------------------------------------------------------
	protected static bool DisposeInstanceImpl(IVsDataExplorerConnection root, bool disposing)
	{
		// Tracer.Trace(typeof(AbstractLinkageParser), "DisposeInstance(FbConnection)");

		lock (_LockGlobal)
		{
			if (_Instances == null || !_Instances.TryGetValue(root, out IBsNativeDbLinkageParser obj))
				return false;


			if (obj is not AbstractLinkageParser parser)
				return false;

			// Tracer.Trace(typeof(AbstractLinkageParser), "DisposeInstance(FbConnection)", "isValidTransient: {0}.", isValidTransient);
			// isValidTransient &= parser.Loaded;


			parser.Dispose(disposing);
		}

		return true;
	}


	#endregion Constructors / Destructors




	// -----------------------------------------------------------------------------------------------------
	#region Internal types - AbstractLinkageParser
	// -----------------------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enum of the linkage build stages. After each stage cancellation tokens are
	/// checked.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public enum EnLinkStage
	{
		Start = 0,
		GeneratorsLoaded = 1,
		TriggerDependenciesLoaded = 2,
		TriggersLoaded = 3,
		SequencesPopulated = 4,
		LinkageCompleted = 5,
		Completed = 6
	}


	#endregion Internal types




	// =========================================================================================================
	#region Fields - AbstractLinkageParser
	// =========================================================================================================


	private static IBsNativeProviderSchemaFactory _SchemaFactory = null;

	/// <summary>
	/// The working db connection associated with this LinkageParser
	/// </summary>
	protected string _ConnectionString = null;

	protected static IBsNativeDbLinkageParser _TransientInstance = null;
	protected string[] _TransientRestrictions = null;

	/// <summary>
	/// The SE db connection associated with this LinkageParser. This object may be
	/// incorrectly created by the ide from a user project invariant assembly if an
	/// edmx is open on ide startup, so it's primary purpose is to serve as an index
	/// to the instance table and supply a connection string.
	/// Also, because of the possible assembly mismatch, subscribing to it's dispose
	/// event has to done through Reflection.
	/// </summary>
	protected IVsDataExplorerConnection _InstanceRoot = null;


	protected string _ConnectionUrl = null;


	/// <summary>
	/// Parser status inidicator that is set to false if the user cancels async
	/// operations in the IDE task handler.
	/// </summary>
	protected bool _Enabled = true;

	protected bool _Disabling = false;


	/// <summary>
	/// Identfies if a LinkageParser as intransient. Intransient parsers
	/// cannot be used as weak equivalent parsers for cloning because at
	/// some point one of the equivalent parsers was disposed and therefore
	/// all equivalent parsers were invalidated.
	/// We do this because a disposed parser was likely due to a refresh
	/// and we do not want the new refreshed parser copying an equivalent.
	/// </summary>
	private bool _IsIntransient = false;

	/// <summary>
	/// The total elapsed time in milliseconds that the parser was actively
	/// building the linkage tables. 
	/// </summary>
	protected long _TotalElapsed = 0;

	/// <summary>
	/// Per connection LinkageParser instances xref.
	/// _Instances must be accessed within _LockGlobal code logic.
	/// </summary>
	private static Dictionary<IVsDataExplorerConnection, IBsNativeDbLinkageParser> _Instances = null;


	/// <summary>
	/// The tracked linkage build process stage.
	/// </summary>
	/// <remarks>
	/// The async linkage process can be interrupted by either a user TaskHandler
	/// cancellation or when the UI thread requires the trigger or sequence tables,
	/// in which case the UI thread takes over the linkage process.
	/// </remarks>
	protected EnLinkStage _LinkStage = EnLinkStage.Start;

	/// <summary>
	/// The intermediate full SELECT of the system generator table.
	/// </summary>
	private DataTable _RawGenerators = null;

	/// <summary>
	/// The intermediate full SELECT of the system trigger table.
	/// </summary>
	private DataTable _RawTriggers = null;

	/// <summary>
	/// The intermediate full SELECT of the system trigger table dependencies.
	/// </summary>
	/// <remarks>
	/// Dependent on network speed, splitting the trigger and trigger dependencies
	/// SELECT statements is up to +- 80% faster than a combined statement.
	/// </remarks>
	private DataTable _RawTriggerDependencies = null;


	/// <summary>
	/// The final populated generator table with trigger linkage.
	/// </summary>
	protected DataTable _Sequences = null;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Stopwatch instance used to report total time and times taken to complete
	/// individual tasks to the IDE task handler and status bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected Stopwatch _Stopwatch;


	/// <summary>
	/// The final populated trigger table with generator linkage.
	/// </summary>
	protected DataTable _Triggers = null;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractLinkageParser
	// =========================================================================================================


	public override string ConnectionString => _ConnectionString;

	public bool IsTransient => _TransientRestrictions != null;


	/// <summary>
	/// Getter indicating whether or not linkage has completed. !Loaded differs
	/// from Incomplete in that !Incomplete may be because the linker has
	/// been disabled.
	/// </summary>
	public override bool Loaded => _LinkStage >= EnLinkStage.Completed;

	/// <summary>
	/// Sets the start and end of an external db request.
	/// </summary>
	private bool Requesting
	{
		set
		{
			lock (_LockObject)
			{
				if (value)
				{
					Stopwatch.Reset();
					Stopwatch.Start();
				}
				else
				{
					Stopwatch.Stop();
					_TotalElapsed += Stopwatch.ElapsedMilliseconds;
				}
			}
		}
	}


	/// <summary>
	/// Gets an instance of DslProviderSchemaFactory. Async operations must always be
	/// passed an instance of DslProviderSchemaFactory and set _SchemaFactory because
	/// GetService() will deadlock behind any sync operation that may occur while an
	/// async task is busy launching.
	/// </summary>
	/// <remarks>
	/// Deadlocks are still happening on GetService() calls on a ConectionNode refresh
	/// where LinkageParser dependent nodes are in an expanded state, so we are
	/// going to keep a permanent instance of IBProviderSchemaFactory, because tracing
	/// of threads is inconsistent in debug vs normal runs.
	/// </remarks>
	private static IBsNativeProviderSchemaFactory SchemaFactory => _SchemaFactory ??= ProviderSchemaFactoryService.EnsureInstance();



	/// <summary>
	/// Getter indicating whether or not the parser has fetched the generators.
	/// </summary>
	protected bool SequencesPopulated => _LinkStage >= EnLinkStage.SequencesPopulated;


	/// <summary>
	/// Timer for VS taskhandler .
	/// </summary>
	protected Stopwatch Stopwatch => _Stopwatch ??= new();


	#endregion Property accessors





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
	protected abstract bool AsyncExecute(int delay, int multiplier);



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
		lock (_LockObject)
		{
			Stopwatch.Reset();
			Stopwatch.Start();
		}

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
		_LinkStage = EnLinkStage.SequencesPopulated;

		lock (_LockObject)
		{
			Stopwatch.Stop();
			_TotalElapsed += Stopwatch.ElapsedMilliseconds;
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Populates the internal trigger table (with linkage) that will be used in future
	/// schema requests for triggers.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void BuildTriggerTable()
	{
		lock (_LockObject)
		{
			Stopwatch.Reset();
			Stopwatch.Start();
		}


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
					Diag.Dug(ex, Resources.ExceptionTriggerErrorAtColumnIndex.FmtRes(i));
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
					Diag.Dug(ex, Resources.ExceptionTriggerGeneratorErrorAtColumnIndex.FmtRes(i, j));
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
					key = [genId];
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

		_LinkStage = EnLinkStage.LinkageCompleted;
		_RawTriggers = null;
		_RawTriggerDependencies = null;

		lock (_LockObject)
		{
			Stopwatch.Stop();
			_TotalElapsed += Stopwatch.ElapsedMilliseconds;
		}

		base.BuildTriggerTable();

	}



	private void CreateLinkTables()
	{
		_Sequences = new();

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

		_Sequences.PrimaryKey = [_Sequences.Columns["SEQUENCE_GENERATOR"]];

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

		_Triggers.PrimaryKey = [_Triggers.Columns["TRIGGER_NAME"]];

		_Triggers.AcceptChanges();
	}



	protected abstract bool Disable();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an external full SELECT of the database system generator table.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected async Task<DataTable> GetRawGeneratorSchemaAsync(IDbConnection connection, CancellationToken cancelToken)
	{
		Requesting = true;

		try
		{
			_RawGenerators = await SchemaFactory.GetSchemaAsync(connection, "RawGenerators", null, cancelToken);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			Requesting = false;
		}


		if (_RawGenerators != null)
			_LinkStage = EnLinkStage.GeneratorsLoaded;

		return _RawGenerators;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an external full SELECT of the database trigger dependencies.
	/// Splitting the dependency fetch from <see cref="GetRawTriggerSchema"/> improves
	/// the overall fetch time by +-80%.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected async Task<DataTable> GetRawTriggerDependenciesSchemaAsync(IDbConnection connection, CancellationToken cancelToken)
	{
		Requesting = true;

		try
		{
			string[] restrictions = null;

			if (_TransientRestrictions != null)
			{
				restrictions = new string[4];
				restrictions[2] = _TransientRestrictions[2];
			}

			_RawTriggerDependencies = await SchemaFactory.GetSchemaAsync(connection, "RawTriggerDependencies", restrictions, cancelToken);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			Requesting = false;
		}


		if (_RawTriggerDependencies != null)
			_LinkStage = EnLinkStage.TriggerDependenciesLoaded;

		return _RawTriggerDependencies;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an external full SELECT of the database system trigger table.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected async Task<DataTable> GetRawTriggerSchemaAsync(IDbConnection connection, CancellationToken cancelToken)
	{
		Requesting = true;

		try
		{
			string[] restrictions = null;

			if (_TransientRestrictions != null)
			{
				restrictions = new string[4];
				restrictions[2] = _TransientRestrictions[2];
			}

			_RawTriggers = await SchemaFactory.GetSchemaAsync(connection, "RawTriggers", restrictions, cancelToken);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			Requesting = false;
		}

		if (_RawTriggers != null)
			_LinkStage = EnLinkStage.TriggersLoaded;


		return _RawTriggers;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates a row of the internal trigger table with identity linkage information.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void UpdateTriggerData(DataRow trig, string genId, bool isIdentity, int dependencyCount)
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



	protected static AbstractLinkageParser FindEquivalentParser(IVsDataExplorerConnection root, bool mustBeLoaded = false)
	{
		// if (_TransientParser != null)
		//	Tracer.Trace(typeof(AbstractLinkageParser), "FindEquivalentParser()", "Clearing unused _TransientParser");

		return FindEquivalentParser(root.DecryptedConnectionString(), mustBeLoaded);
	}



	protected static AbstractLinkageParser FindEquivalentParser(string connectionString, bool mustBeLoaded = false)
	{
		// Tracer.Trace(typeof(AbstractLinkageParser), "FindEquivalentParser()");

		if (_Instances == null)
		{
			// Tracer.Trace(typeof(AbstractLinkageParser), "FindEquivalentParser()", "Not found. _Instances is null.");
			return null;
		}

		// Tracer.Trace(typeof(AbstractLinkageParser), "FindEquivalentParser()", "Searching instances. Count: {0}.", _Instances.Count);


		foreach (KeyValuePair< IVsDataExplorerConnection, IBsNativeDbLinkageParser> pair in _Instances)
		{
			AbstractLinkageParser parser = (AbstractLinkageParser)pair.Value;

			if (!parser._Enabled || parser._IsIntransient)
				continue;

			if (ApcManager.IsWeakConnectionEquivalency(connectionString, parser.ConnectionString))
			{
				if (!mustBeLoaded || parser.Loaded)
					return parser;
			}
		}

		// Tracer.Trace(typeof(AbstractLinkageParser), "FindEquivalentParser()", "No equivalent found in instances. Count: {0}", _Instances.Count);

		return null;
	}



	protected static AbstractLinkageParser FindInstanceOrTransient(string connectionString)
	{
		// Tracer.Trace(typeof(AbstractLinkageParser), "FindParser()", "connectionString: {0}.", connectionString);

		string connectionUrl = null;

		if (_Instances == null)
			return null;

		connectionUrl ??= ApcManager.CreateConnectionUrl(connectionString);

		// Tracer.Trace(typeof(AbstractLinkageParser), "FindParser()", "Searching instances. connectionUrl: {0}.", connectionUrl);


		foreach (KeyValuePair< IVsDataExplorerConnection, IBsNativeDbLinkageParser> pair in _Instances)
		{
			AbstractLinkageParser parser = (AbstractLinkageParser)pair.Value;

			if (!parser._Enabled || parser._IsIntransient)
				continue;

			// Tracer.Trace(typeof(AbstractLinkageParser), "FindParser()", "Comparing instance. parser._ConnectionUrl: {0}.", parser._ConnectionUrl);

			if (connectionUrl.Equals(parser._ConnectionUrl))
				return parser;
		}

		// Tracer.Trace(typeof(AbstractLinkageParser), "FindParser()", "No parser found in instances. Count: {0}", _Instances.Count);

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds a trigger in the internal linked trigger table and returns it's row else
	/// return null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override DataRow FindTrigger(object name)
	{
		EnsureLoaded();

		return _Triggers.Rows.Find(name);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs a generator table GetSchema() request using the internal linked sequence
	/// table.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override DataTable GetSequenceSchema(string[] restrictions)
	{
		EnsureLoaded();

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
	public override DataTable GetTriggerSchema(string[] restrictions, int systemFlag, int identityFlag)
	{
		EnsureLoaded();

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



	// If a parser has been refreshed all other parsers with weak equivalency must be invalidated
	// as potential transients so that they cannot be used.
	protected static void InvalidateEquivalentParsers(string connectionString)
	{
		// Tracer.Trace(typeof(AbstractLinkageParser), "FindEquivalentParser()");

		lock (_LockGlobal)
		{
			if (_TransientInstance != null && ApcManager.IsWeakConnectionEquivalency(connectionString, _TransientInstance.ConnectionString))
				_TransientInstance = null;

			if (_Instances == null)
			{
				// Tracer.Trace(typeof(AbstractLinkageParser), "InvalidateEquivalentParsers()", "Not found. _Instances is null.");
				return;
			}
		}


		// Tracer.Trace(typeof(AbstractLinkageParser), "InvalidateEquivalentParsers()", "Searching instances. Count: {0}.", _Instances.Count);

		int i = -1;

		foreach (KeyValuePair< IVsDataExplorerConnection, IBsNativeDbLinkageParser> pair in _Instances)
		{
			i++;
			AbstractLinkageParser parser = (AbstractLinkageParser)pair.Value;

			if (!parser._Enabled)
				continue;

			if (ApcManager.IsWeakConnectionEquivalency(connectionString, parser.ConnectionString))
				parser._IsIntransient = true;
		}

		return;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Locates and returns the internal linked trigger table row for an identity
	/// column else returns null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override DataRow LocateIdentityTrigger(object objTable, object objField)
	{
		EnsureLoaded();

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
	#region Event handlers - AbstractLinkageParser
	// =========================================================================================================


	#endregion Event handlers


}
