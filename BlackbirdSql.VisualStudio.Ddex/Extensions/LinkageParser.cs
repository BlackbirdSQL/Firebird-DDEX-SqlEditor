// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TaskStatusCenter;

using FirebirdSql.Data.FirebirdClient;

namespace BlackbirdSql.Common.Extensions;




// =========================================================================================================
//										LinkageParser Class
//
/// <summary>
/// Builds the Trigger / Generator linkage tables.
/// </summary>
/// <remarks>
/// The class is split into 3 separate classes:
/// 1. AbstruseLinkageParser - handles the parsing,
/// 2. AbstractLinkageParser - handles the actual data table building, and
/// 3. LinkageParser - manages the sync/async build tasks.
/// The building task is executed async as <see cref="TaskCreationOptions.LongRunning"/> and
/// suspended whenever any UI main thread database tasks are requested for a particular connection,
/// and then resumed once the request is completed.
/// If a UI request requires the completed Trigger / Generator linkage tables the UI thread takes over
/// from the async thread to complete the build.
/// </remarks>
// =========================================================================================================
internal class LinkageParser : AbstractLinkageParser
{


	// -----------------------------------------------------------------------------------------------------
	#region Constructors - LinkageParser
	// -----------------------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected .ctor. LinkageParser's are uniquely distinct to a connection. Use the
	/// Instance() static to create or retrieve a parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected LinkageParser(FbConnection connection) : base(connection)
	{
		// Diag.Trace("Creating new connection parser");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates the parser instance of a connection derived from the Site
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static LinkageParser Instance(IVsDataConnection site)
	{
		if (site.GetService(typeof(IVsDataConnectionSupport)) is not IVsDataConnectionSupport vsDataConnectionSupport)
			return null;
		FbConnection connection = vsDataConnectionSupport.ProviderObject as FbConnection;

		return Instance(connection);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves or creates a distinct unique parser for a connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new LinkageParser Instance(FbConnection connection)
	{
		LinkageParser parser = (LinkageParser)AbstractLinkageParser.Instance(connection);

		parser ??= new(connection);

		return parser;
	}


	#endregion Constructors





	// =========================================================================================================
	#region Methods - LinkageParser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs delay in small timeslices to intermittently check if the user has not
	/// cancelled an async operation during the overall delay period of delay *
	/// multiplier.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool AsyncDelay(int delay, int multiplier, CancellationToken asyncToken,
		CancellationToken userToken)
	{
		if (delay == 0 || multiplier == 0)
			return true;

		for (int i = 0; i < multiplier; i++)
		{
			Thread.Sleep(delay);

			if (userToken.IsCancellationRequested)
				_Enabled = false;

			if (!_Enabled || asyncToken.IsCancellationRequested || !ConnectionActive)
				return false;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Initiates or resumes an async build of the linkage tables.
	/// </summary>
	/// <param name="delay">Milliseconds async task must sleep before proceding</param>
	/// <param name="multiplier">Number of times to execute delay.</param>
	/// <remarks>
	/// Keep delay short and use 'multiplier' to extend the delay so that any
	/// cancellation can be honoured sooner.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
	public override bool AsyncExecute(int delay = 0, int multiplier = 1)
	{
		if (!ClearToLoadAsync)
			return false;

		_AsyncActive = true;

		UpdateStatusBar(_LinkStage, _AsyncActive);

		IVsTaskStatusCenterService tsc = ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService,
			IVsTaskStatusCenterService>(swallowExceptions: false).Result;

		TaskHandlerOptions options = default;
		options.Title = $"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)}) sequence linkage";
		options.ActionsAfterCompletion = CompletionActions.None;

		_ProgressData = default;
		_ProgressData.CanBeCanceled = true;

		_TaskHandler = tsc.PreRegister(options, _ProgressData);

		_AsyncTokenSource?.Dispose();
		_AsyncTokenSource = new();
		_AsyncToken = _AsyncTokenSource.Token;

		_AsyncTask = Task.Factory.StartNew(() =>
		{
			try
			{
				if (AsyncDelay(delay, multiplier, _AsyncToken, _TaskHandler.UserCancellation))
					PopulateLinkageTables(_AsyncToken, _TaskHandler.UserCancellation);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			return AsyncExit();
		},
			default, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);


		_TaskHandler.RegisterTask(_AsyncTask);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Tags the parser status as out of an asynchronous state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool AsyncExit()
	{
		// User requested this cancellation so UpdateStatusBar will be placed on UI thread
		// queue, but as fire and forget so we're okay here.
		if (!_Enabled)
			UpdateStatusBar(_LinkStage, _AsyncActive);

		_AsyncActive = false;

		// _TaskHandler = null;
		// _ProgressData = default;

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches the UI thread build of the linkage tables if the UI requires them. If
	/// an async build is in progress, waits for the active operation to complete and
	/// then switches over to a UI thread build for the remaining tasks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
	public override bool Execute()
	{
		if (!ClearToLoad)
			return false;


		if (_Connection == null)
		{
			Microsoft.VisualStudio.Data.DataProviderException ex = new("Connection disposed");
			Diag.Dug(ex);
			throw ex;
		}

		if (!ConnectionActive)
		{
			// Try reset
			try
			{
				_Connection.Close();
				_Connection.Open();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			if (!ConnectionActive)
			{
				Microsoft.VisualStudio.Data.DataProviderException ex = new("Connection closed");
				Diag.Dug(ex);
				throw ex;
			}
		}

		SyncEnter();

		UpdateStatusBar(_LinkStage, _AsyncActive);

		IVsTaskStatusCenterService tsc = ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService, IVsTaskStatusCenterService>(swallowExceptions: false).Result;

		TaskHandlerOptions options = default;
		options.Title = $"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)}) sequence linkage";
		options.ActionsAfterCompletion = CompletionActions.None;

		_ProgressData = default;
		_ProgressData.CanBeCanceled = false;

		_TaskHandler = tsc.PreRegister(options, _ProgressData);


		Task<bool> task = Task.Factory.StartNew(() =>
		{
			try
			{
				return PopulateLinkageTables();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		},
			default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

		_TaskHandler.RegisterTask(task);

		task.Wait();

		SyncExit();


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs the actual linkage build process.
	/// </summary>
	/// <param name="asyncLockedToken">
	/// Passing a cancellation token indicates that the call has been made asynchronously.
	/// </param>
	/// <returns>
	/// True if the method ran it's course else false.
	/// </returns>
	// ----------------------------------------------------------------------------------
	protected override bool PopulateLinkageTables(CancellationToken asyncToken = default,
		CancellationToken userToken = default)
	{

		if (_Connection == null)
		{
			ObjectDisposedException ex = new("Connection is null");
			Diag.Dug(ex);
			throw ex;
		}

		if (!ConnectionActive)
			return false;

		TaskHandlerProgress(null, GetPercentageComplete(_LinkStage),
			_LinkStage == EnumLinkStage.Start ? 0 : -1);

		if (_LinkStage < EnumLinkStage.GeneratorsLoaded)
		{
			GetRawGeneratorSchema();

			if (_AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress("SELECT Generators", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}


		if (_AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (!ConnectionActive)
			return false;


		if (_LinkStage < EnumLinkStage.TriggerDependenciesLoaded)
		{
			GetRawTriggerDependenciesSchema();

			if (_AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress("SELECT TriggerDependencies", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		if (_AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (!ConnectionActive)
			return false;

		if (_LinkStage < EnumLinkStage.TriggersLoaded)
		{
			GetRawTriggerSchema();

			if (_AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress("SELECT Triggers", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}

		if (_AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;


		if (!SequencesPopulated)
		{
			BuildSequenceTable();

			if (_AsyncActive && userToken.IsCancellationRequested)
				_Enabled = false;

			TaskHandlerProgress("Populating Sequence Table", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);
		}


		if (_AsyncActive && (!_Enabled || asyncToken.IsCancellationRequested))
			return false;

		if (_LinkStage < EnumLinkStage.Completed)
			BuildTriggerTable();

		TaskHandlerProgress($"Parsing and linking of {_Triggers.Rows.Count} Triggers", GetPercentageComplete(_LinkStage), Stopwatch.ElapsedMilliseconds);

		_Stopwatch = null;

		UpdateStatusBar(_LinkStage, _AsyncActive);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the sync call counter and suspends any async tasks. This should be
	/// called on every occasion that a UI thread db call is made in the package.
	/// </summary>
	// ---------------------------------------------------------------------------------
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
	public bool SyncEnter()
	{
		_SyncActive++;

		if (_AsyncTask == null || _AsyncTask.IsCompleted)
		{
			return true;
		}

		_AsyncTokenSource.Cancel();
		_AsyncTask.Wait();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the sync call counter. This should be called on every occasion that
	/// a UI thread db call is exited in the package.
	/// When the counter reaches zero, outstanding async tasks are resumed.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void SyncExit()
	{
		_SyncActive--;
		if (_SyncActive < 0)
		{
			InvalidOperationException ex = new("Attempt to exit a synchronous state when the object was not in a synchronous state");
			Diag.Dug(ex);
			throw ex;
		}

		if (_SyncActive > 0)
			return;

		_TaskHandler = null;
		_ProgressData = default;

		if (_LinkStage < EnumLinkStage.Completed && _Enabled)
		{
			if (_AsyncToken.IsCancellationRequested)
			{
				_AsyncTokenSource.Dispose();
				_AsyncTokenSource = new();
				_AsyncToken = _AsyncTokenSource.Token;
			}
			AsyncExecute();
		}
	}


	#endregion Methods


}
