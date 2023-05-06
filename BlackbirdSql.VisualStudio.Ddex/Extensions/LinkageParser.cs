// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.TaskStatusCenter;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.TextManager.Interop;

namespace BlackbirdSql.Common.Extensions;



// Deadlock warning message suppression
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]


// =========================================================================================================
//										LinkageParser Class
//
/// <summary>
/// Builds the Trigger / Generator linkage tables.
/// </summary>
/// <remarks>
/// The class is split into 3 separate classes. 1. AbstruseLinkageParser handles the parsing,
/// 2. AbstractLinkageParser handles the actual data table building, and 3. LinkageParser
/// manages the sync/async build tasks.
/// The building task is executed async as <see cref="TaskCreationOptions.LongRunning"/> and
/// suspended whenever any UI main thread database tasks are requested for a particular connection,
/// and then resumed once the request is completed.
/// If a UI request requires the completed Trigger / Generator linkage tables the UI thread takes over
/// from the async thread to complete the build.
/// </remarks>
// =========================================================================================================
internal class LinkageParser : AbstractLinkageParser
{


	// =========================================================================================================
	#region Constructors - LinkageParser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected .ctor. LinkageParser's are uniquely distinct to a connection. Use the Instance()
	/// static to create or retrieve a parser for a connection.
	/// </summary>
	/// <param name="connection"></param>
	// ---------------------------------------------------------------------------------
	protected LinkageParser(FbConnection connection) : base(connection)
	{
		// Diag.Trace("Creating new connection parser");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets or creates the parser instance of a connection derived from the Site
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
	/// Increments the sync call counter and suspends any async tasks. This should be
	/// called on every occasion that a sync db call is made in the package.
	/// </summary>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public override bool SyncEnter()
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
	/// a sync db call is exited in the package.
	/// When the counter reaches zero, outstanding async tasks are resumed.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void SyncExit()
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches the sync build of the linkage tables. If an async build is in progress,
	/// waits for the active operation to complete and then switches over to a sync
	/// build for the remaining tasks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool Execute()
	{
		if (!ClearToLoad || !ConnectionActive)
			return false;

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
	/// Initiates or resumes an async build of the linkage tables.
	/// </summary>
	/// <param name="delay">Milliseconds async task must sleep before proceding</param>
	/// <param name="multiplier">Number of times to execute delay.</param>
	/// <remarks>
	/// Keep delay short and use 'multiplier' to extend the delay so that any
	/// cancellation can be honoured sooner.
	/// </remarks>
	// ---------------------------------------------------------------------------------
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

			if (!_Enabled || asyncToken.IsCancellationRequested)
				return false;
		}

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
	// ------7---------------------------------------------------------------------------
	public override bool PopulateLinkageTables(CancellationToken asyncToken = default,
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

		if (_LinkStage < EnumLinkStage.GeneratorsLoaded)
		{
			if (GetRawGeneratorSchema() != null)
				_LinkStage = EnumLinkStage.GeneratorsLoaded;
		}


		if (_AsyncActive && userToken.IsCancellationRequested)
			_Enabled = false;
		if (!_Enabled || asyncToken.IsCancellationRequested)
			return false;

		if (!ConnectionActive)
			return false;


		if (_LinkStage < EnumLinkStage.TriggerGeneratorsLoaded)
		{
			if (GetRawTriggerGeneratorSchema() != null)
				_LinkStage = EnumLinkStage.TriggerGeneratorsLoaded;
		}


		if (_AsyncActive && userToken.IsCancellationRequested)
			_Enabled = false;
		if (!_Enabled || asyncToken.IsCancellationRequested)
			return false;

		if (!ConnectionActive)
			return false;

		if (_LinkStage < EnumLinkStage.TriggersLoaded)
		{
			if (GetRawTriggerSchema() != null)
				_LinkStage = EnumLinkStage.TriggersLoaded;
		}

		if (_AsyncActive && userToken.IsCancellationRequested)
			_Enabled = false;
		if (!_Enabled || asyncToken.IsCancellationRequested)
			return false;


		if (!SequencesLoaded)
			BuildSequenceTable();


		if (_AsyncActive && userToken.IsCancellationRequested)
			_Enabled = false;
		if (!_Enabled || asyncToken.IsCancellationRequested)
			return false;

		if (_LinkStage < EnumLinkStage.Completed)
			BuildTriggerTable();


		UpdateStatusBar(_LinkStage, _AsyncActive);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Tags the parser status as out of an asynchronous state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override bool AsyncExit()
	{
		// User requested this cancellation so UpdateStatusBar will be placed on UI thread
		// queue, but as fire and forget so we're okay here.
		if (!_Enabled)
			UpdateStatusBar(_LinkStage, _AsyncActive);

		_AsyncActive = false;

		_TaskHandler = null;
		_ProgressData = default;

		return true;
	}



	/// <summary>
	/// Launches UpdateStatusBarAsync from a thread in the thread pool so that it
	/// can switch to the UI thread and be clear to update the IDE status bar.
	/// </summary>
	/// <param name="stage"></param>
	/// <param name="isAsync"></param>
	/// <returns></returns>
	protected override bool UpdateStatusBar(EnumLinkStage stage, bool isAsync)
	{
		_ = Task.Factory.StartNew(() => UpdateStatusBarAsync(stage, isAsync).Result, default,
				TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent,
				TaskScheduler.Default);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE status bar.
	/// </summary>
	/// <remarks>
	/// The bar is only updated at the start of an Execute and end of an Execute. 
	/// </remarks>
	// ---------------------------------------------------------------------------------
	// [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD102:Implement internal logic asynchronously", Justification = "<Pending>")]
	protected override async Task<bool> UpdateStatusBarAsync(EnumLinkStage stage, bool isAsync)
	{
		// Switch to main thread
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		try
		{
			string async;
			string catalog = $"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)})";

			IVsStatusbar statusBar = await ServiceProvider.GetGlobalServiceAsync<SVsStatusbar, IVsStatusbar>(swallowExceptions: false);

			// statusBar.FreezeOutput(0);

			if (stage == EnumLinkStage.Clear)
			{
				statusBar.Clear();
			}
			else if (stage == EnumLinkStage.Start)
			{
				async = isAsync ? " (async)" : "";

				statusBar.SetText($"Updating {catalog} sequence linkage.");
			}
			else if (stage == EnumLinkStage.Completed)
			{
				async = isAsync ? " (async)" : "";

				statusBar.SetText($"Completed {catalog} sequence linkage in {_Elapsed}ms.");
			}
			else
			{
				if (isAsync)
				{
					// If it's a user cancel request.
					if (!_Enabled)
						statusBar.SetText($"Cancelled {catalog} sequence linkage.");
					else
						statusBar.SetText($"Resuming {catalog} sequence linkage.");
				}
				else
				{
					if (!_Enabled)
						statusBar.SetText($"Resuming {catalog} sequence linkage.");
					else
						statusBar.SetText($"Switched {catalog} sequence linkage to UI thread.");
				}
			}

			if (stage == EnumLinkStage.Completed || (isAsync && !_Enabled))
			{
				_ = Task.Run(async delegate
				{
					stage = EnumLinkStage.Clear;
					await Task.Delay(4000);
					_ = UpdateStatusBarAsync(stage, isAsync);
				});
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		return true;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Launches TaskHandlerProgressAsync from a thread in the thread pool so that it it
	/// can switch to the UI thread and be clear to update the IDE task handler progress
	/// bar.
	/// </summary>
	/// <param name="stage">The descriptive name of the completed stage</param>
	/// <param name="progress">The % completion of the linkage build.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	protected override bool TaskHandlerProgress(string stage, int progress, long elapsed)
	{
		_ = Task.Factory.StartNew(() => TaskHandlerProgressAsync(stage, progress, elapsed).Result,
				default, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent,
				TaskScheduler.Default);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	/// <param name="stage">The descriptive name of the completed stage</param>
	/// <param name="progress">The % completion of the linkage build.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	protected async override Task<bool> TaskHandlerProgressAsync(string stage, int progress, long elapsed)
	{
		if (_TaskHandler == null)
			return false;

		// Switch to main thread
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		// Check again since joining UI thread.
		if (_TaskHandler == null)
			return false;

		_ProgressData.PercentComplete = progress;

		if (progress == 100)
		{
			_ProgressData.ProgressText = $"Completed. {stage} {elapsed}ms.";
		}
		else if (_AsyncToken.IsCancellationRequested || !_Enabled)
		{
			_ProgressData.ProgressText = $"Cancelled. {progress}% completed. {stage} took {elapsed}ms.";
		}
		else
		{
			_ProgressData.ProgressText = $"{progress}% completed. {stage} took {elapsed}ms.";
		}


		_TaskHandler.Progress.Report(_ProgressData);

		return true;
	}


	#endregion Methods

}
