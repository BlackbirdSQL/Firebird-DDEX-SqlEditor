// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdDsl;
using C5;
using FirebirdSql.Data.FirebirdClient;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using BlackbirdSql.Core;
using Diag = BlackbirdSql.Core.Diag;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

namespace BlackbirdSql.VisualStudio.Ddex.Extensions;



// =========================================================================================================
//										AbstruseLinkageParser Class
//
/// <summary>
/// Handles Trigger / Generator linkage parsing specific tasks utilizing BlackbirdDsl.<see cref="Parser"/>.
/// </summary>
// =========================================================================================================
internal abstract class AbstruseLinkageParser
{


	// -----------------------------------------------------------------------------------------------------
	#region Variables - AbstruseLinkageParser
	// -----------------------------------------------------------------------------------------------------



	protected static object _LockObject = new object();
	protected object _LocalObject = new();


	// Threading / multi process control variables.
	// A quick double tap on an unopened SE connection causes a hang. It was assumed this was due to multiple thread
	// processes queued onto the ui thread. iow different processes entering the ddex at different entry points all
	// coming from the same SE Site.
	// To prevent this from causing unpredictable state changes and/or deadlocks within the ddex, we queue them back
	// onto a single queue when necessary and drip feed the ddex with those processes.
	// Essentially this allows us to support multiple thread processes across multiple SE nodes because we decide
	// which tasks can be async and which will be sync.
	// So this worked for a single async task which we create and then the first sync task from the double tap, but
	// the 3rd task from the 2nd tap doesn't ever seem to reach us. In fact the SE blocks input to it's tool window
	// while, for example, a node is loading & expanding; but a double tap in this scenario seems to cause the 2nd
	// input to incorrectly slip through and hang ServerExplorer.
	// Examining the VSServerExplorer package code, there is no support for async operations, even though
	// IVsDataViewSupport can be configured for it, so we know from that, that the 2nd tap should never have been
	// allow through as 2 seperate inputs.
	// Also, it seems the Fb client async methods are not actually async, so... duno...

	// The parser id / index
	public static int _InstanceSeed = 1000;
	public int _InstanceId = 0;

	// The async process thread id / index
	public static int _AsyncProcessSeed = 9000;
	public int _AsyncProcessId = 0;


	protected bool _SeDisabled = false;
	protected static bool S_SeDisabled = false;
	protected static IntPtr S_SeToolHWnd = IntPtr.Zero;

	/// <summary>
	/// The async process state, 0 = No aync process, 1 = Queued on a task, 2 = Active.
	/// </summary>
	protected int _AsyncCardinal = 0;

	/// <summary>
	/// The number of sync tasks. Active + Queued.
	/// </summary>
	protected int _SyncCardinal = 0;

	/// <summary>
	/// The async task variables if it exists.
	/// </summary>
	protected Task<bool> _AsyncTask;
	protected CancellationToken _AsyncToken;
	protected CancellationTokenSource _AsyncTokenSource = null;



	// This is the token the async process signals when it exits. The first
	// process in the sync queue waits on this.
	protected CancellationToken _SyncWaitAsyncToken;
	protected CancellationTokenSource _SyncWaitAsyncTokenSource = null;






	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Instance of the BlackbirdDsl.<see cref="Parser"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected Parser _DslParser = null;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The db connection asociated with this LinkageParser
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected FbConnection _Connection = null;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Stopwatch instance used to report total time and times taken to complete
	/// individual tasks to the IDE task handler and status bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected System.Diagnostics.Stopwatch _Stopwatch;


	#endregion Variables





	// =========================================================================================================
	#region Property accessors - AbstruseLinkageParser
	// =========================================================================================================


	/// <summary>
	/// Getter indicating wether or not the UI thread can and should resume linkage operations.
	/// </summary>
	public System.Diagnostics.Stopwatch Stopwatch
	{
		get
		{
			_Stopwatch ??= new();

			return _Stopwatch;
		}
	}


	/// <summary>
	/// Getter to retrieve or create an instance of the BlackbirdDsl.<see cref="Parser"/>.
	/// </summary>
	public Parser DslParser
	{
		get
		{
			_DslParser ??= new Parser(DslOptions.TOKENIZE_ONLY);
			return _DslParser;
		}

	}


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - AbstruseLinkageParser
	// =========================================================================================================


	/// <summary>
	/// Protected default .ctor for creating an instance for a db connection.
	/// </summary>
	/// <param name="connection"></param>
	protected AbstruseLinkageParser(FbConnection connection)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		// Tracer.Trace(typeof(AbstruseLinkageParser), $"StaticId:[{"0000"}] AbstruseLinkageParser(FbConnection)");
		_Connection = connection;

	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - AbstruseLinkageParser
	// =========================================================================================================


	protected void SetSeToolWindow()
	{
		/*
		ThreadHelper.ThrowIfNotOnUIThread();

		if (S_SeToolHWnd == IntPtr.Zero)
		{
			try
			{
				IVsUIShell uiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;


				uint grfFTW = 0u; ;
				Guid rguidPersistenceSlot = VSConstants.StandardToolWindows.ServerExplorer;

				Native.ThrowOnFailure(uiShell.FindToolWindow(grfFTW, rguidPersistenceSlot, out IVsWindowFrame toolWindow));
				Native.ThrowOnFailure(toolWindow.GetProperty((int)__VSFPROPID9.VSFPROPID_ContainingHwnd, out object pvar));

				S_SeToolHWnd = (IntPtr)pvar;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}
		*/
	}



	protected void EnableSeWindow(bool enable)
	{
		/*
		ThreadHelper.ThrowIfNotOnUIThread();

		if (enable == S_SeDisabled && S_SeDisabled == _SeDisabled && S_SeToolHWnd != IntPtr.Zero)
		{
			lock (_LockObject)
			{
				S_SeDisabled = _SeDisabled = !enable;
				try
				{
					Native.EnableWindow(S_SeToolHWnd, enable);
				}
				catch (Exception ex)
				{
					S_SeDisabled = _SeDisabled = enable;
					Diag.Dug(ex);
				}
			}
		}
		*/
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a trigger DSL statement.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public (string, int, int) ParseTriggerDSL(string sql)
	{
		return ParseTriggerDSL(sql, null, null, null);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a trigger DSL statement given a trigger name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public (string, int, int) ParseTriggerDSL(string sql, string trigger)
	{
		return ParseTriggerDSL(sql, trigger, null, null);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a trigger DSL statement givern a trigger and table name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public (string, int, int) ParseTriggerDSL(string sql, string trigger, string table)
	{
		return ParseTriggerDSL(sql, trigger, table, null);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a trigger DSL statement givern a trigger, table name and column name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public (string, int, int) ParseTriggerDSL(string sql, string trigger, string table, string column)
	{
		int increment = -1;
		int seed = -1;
		string generator = null;


		int sequence = -1;
		int stage = 0;
		int i;
		string token, sequenceToken;

		string[][] _Sequences =
		{
			new string[] { "CREATE", "TRIGGER", "_TRIGGER_", "FOR", "_TABLE_",
				"AS", "BEGIN", "_COLUMN_", "=", "GEN_ID", "_GENPARAM_" },
			new string[] { "CREATE", "TRIGGER", "_TRIGGER_", "FOR", "_TABLE_",
				"AS", "BEGIN", "_COLUMN_", "=", "NEXT", "VALUE", "FOR", "_GENERATOR_" },
			// new string[] { "SET", "GENERATOR", "_GENERATOR_", "TO", "_SEED_" },
			// new string[] { "ALTER", "SEQUENCE", "_GENERATOR_", "RESTART", "WITH", "_SEED_" }
		};

		bool[] _Completed = { false, false /*, false, false */ };

		bool[][] _Sequencing =
		{
			new bool[] { false, true, true, true, true,
				false, false, false, true, true, true },
			new bool[] { false, true, true, true, true,
				false, false, false, true, true, true, true, true },
			// new bool[] { false, true, true, true, true },
			// new bool[] { false, true, true, true, true, true }
		};




		StringCell tokens = DslParser.Execute(sql.ToUpper());


		foreach (StringCell tokenCell in tokens.Enumerator)
		{
			token = tokenCell.ToString().Trim();

			if (String.IsNullOrEmpty(token))
				continue;

			if (sequence == -1)
			{
				stage = 0;
				for (i = 0; i < _Sequences.Length; i++)
				{
					if (_Completed[i])
						continue;

					if (token == _Sequences[i][stage])
					{
						sequence = i;
						stage++;
						break;
					}
					// Special case
					if (i < 2 && token == _Sequences[i][5])
					{
						sequence = i;
						stage = 6;
						break;
					}
				}
				continue;
			}

			sequenceToken = _Sequences[sequence][stage];

			switch (sequenceToken)
			{
				case "GEN_ID":
					// Single special case. We're not going to do nested sequences just for one.
					if (token != sequenceToken)
					{
						sequence++;
						sequenceToken = _Sequences[sequence][stage];
					}
					break;
				case "_TRIGGER_":
					if (trigger != null)
						sequenceToken = trigger;
					else
						sequenceToken = token;
					break;
				case "_TABLE_":
					if (table != null)
						sequenceToken = table;
					else
						sequenceToken = token;
					break;
				case "_COLUMN_":
					if (column != null)
						sequenceToken = "NEW." + column;
					else if (token.StartsWith("NEW."))
						sequenceToken = token;
					break;
				case "_GENPARAM_":
					if (token.StartsWith("(") && token.EndsWith(")"))
					{
						(generator, increment) = GetGenIdParams(token);
						if (generator != null)
							sequenceToken = token;
					}
					break;
				case "_GENERATOR_":
					if (generator == null)
					{
						generator = token;
						increment = 1;
					}
					sequenceToken = generator;
					break;
				case "_SEED_":
					seed = Convert.ToInt32(token);
					sequenceToken = token;
					break;
			}

			if (token != sequenceToken)
			{
				if (_Sequencing[sequence][stage])
					sequence = -1;
				continue;
			}

			stage++;

			if (stage == _Sequences[sequence].Length)
			{
				if (sequence < 2)
					_Completed[0] = _Completed[1] = true;
				else
					_Completed[2] = _Completed[3] = true;
				sequence = -1;
			}

			for (i = 0; i < _Completed.Length; i++)
			{
				if (!_Completed[i])
					break;
			}

			if (i == _Completed.Length)
				break;
		}

		if (seed == -1 && generator != null)
			seed = 0;

		return (generator, increment, seed);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Splits the legacy GEN_ID function arguments into generator name and increment. 
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected (string, int) GetGenIdParams(string param)
	{
		int increment = -1;
		string generator = null;

		char[] chrs = { '(', ')', ' ' };

		string[] parameters = param.Trim(chrs).Split(',');

		if (parameters.Length > 0)
			generator = parameters[0].Trim();

		if (parameters.Length > 1)
			increment = Convert.ToInt32(parameters[1].Trim());
		else if (generator != null)
			increment = 1;

		return (generator, increment);
	}


	#endregion Methods

}
