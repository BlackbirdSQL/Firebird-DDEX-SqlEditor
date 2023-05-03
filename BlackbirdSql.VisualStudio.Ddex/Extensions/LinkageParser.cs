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



namespace BlackbirdSql.Common.Extensions;



// Deadlock warning message suppression
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]


// =========================================================================================================
//										LinkageParser Class
//
/// <summary>
/// Builds the Trigger / Generator linkage tables.
/// The class is split into 3 separate classes. 1. AbstruseLinkageParser handles the parsing,
/// 2. AbstractLinkageParser handles the actual data table building, and 3. LinkageParser
/// manages the sync/async build tasks.
/// The building task is executed async as <see cref="TaskCreationOptions.LongRunning"/> and
/// suspended whenever any UI main thread database tasks are requested for a particular connection,
/// and then resumed once the request is completed.
/// If a UI request requires the completed Trigger / Generator linkage tables the UI thread takes over
/// from the async thread to complete the build.
/// </summary>
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
	/// Ensures the async queue is cleared out before passing control back to any sync
	/// request.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool ClearAsyncQueue()
	{
		if (_ExternalAsyncTask == null || _ExternalAsyncTask.IsCompleted)
		{
			return true;
		}

		_AsyncLockedTokenSource.Cancel();
		_ExternalAsyncTask.Wait();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the sync call counter and suspends any async tasks. This should be
	/// called on every occasion that a sync db call is made in the package.
	/// </summary>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public override bool EnterSync()
	{
		_SyncActive++;
		return ClearAsyncQueue();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the sync call counter. This should be called on every occasion that
	/// a sync db call is exited in the package.
	/// When the counter reaches zero, outstanding async tasks are resumed.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void ExitSync()
	{
		_SyncActive--;
		if (_SyncActive < 0)
			_SyncActive = 0;

		_TaskHandler = null;
		_ProgressData = default;

		if (_SyncActive == 0 && _LinkStage != EnumLinkStage.Completed && !_SyncLockedToken.IsCancellationRequested)
		{
			if (_AsyncLockedToken.IsCancellationRequested)
			{
				_AsyncLockedTokenSource.Dispose();
				_AsyncLockedTokenSource = new();
				_AsyncLockedToken = _AsyncLockedTokenSource.Token;
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

		if (!EnterSync())
		{
			ExitSync();
			return false;
		}

		UpdateStatusBar(_LinkStage, _AsyncActive);

		IVsTaskStatusCenterService tsc = ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService, IVsTaskStatusCenterService>(swallowExceptions: false).Result;

		var options = default(TaskHandlerOptions);
		options.Title = $"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)}) trigger sequence linkage";
		options.ActionsAfterCompletion = CompletionActions.None;

		_ProgressData = default;
		_ProgressData.CanBeCanceled = false;

		_TaskHandler = tsc.PreRegister(options, _ProgressData);


		Task<bool> task = Task.Factory.StartNew(() => PerformExecute(),
			default, TaskCreationOptions.PreferFairness, TaskScheduler.Current);

		_TaskHandler.RegisterTask(task);

		task.Wait();

		ExitSync();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Initiates or resumes an async build of the linkage tables.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool AsyncExecute(int delay = 0)
	{
		if (!ClearToLoadAsync)
			return false;

		_AsyncActive = true;

		UpdateStatusBar(_LinkStage, _AsyncActive);

		IVsTaskStatusCenterService tsc = ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService, IVsTaskStatusCenterService>(swallowExceptions: false).Result;

		var options = default(TaskHandlerOptions);
		options.Title = $"{_Connection.DataSource} ({Path.GetFileNameWithoutExtension(_Connection.Database)}) async trigger sequence linkage";
		options.ActionsAfterCompletion = CompletionActions.None;
		_ProgressData = default;
		_ProgressData.CanBeCanceled = true;


		_TaskHandler = tsc.PreRegister(options, _ProgressData);

		_ExternalAsyncTask = Task.Factory.StartNew(() => { PerformExecute(_AsyncLockedTokenSource, _SyncLockedTokenSource, delay); return AsyncExited(); },
			default, TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);


		_TaskHandler.RegisterTask(_ExternalAsyncTask);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs the actual linkage build process.
	/// </summary>
	/// <param name="asyncLockedToken">
	/// Passing a cancellation token indicates that the call has been made asynchronously.
	/// </param>
	// ---------------------------------------------------------------------------------
	public override bool PerformExecute(CancellationTokenSource asyncTokenSource = default,
		CancellationTokenSource syncTokenSource = default, int delay = 0)
	{
		CancellationToken asyncToken = asyncTokenSource == default ? default : asyncTokenSource.Token;
		CancellationToken syncToken = syncTokenSource == default ? default : syncTokenSource.Token;

		if (delay != 0)
			Thread.Sleep(delay);

		if (_AsyncActive && _TaskHandler.UserCancellation.IsCancellationRequested)
			syncTokenSource.Cancel();
		if (asyncToken.IsCancellationRequested || syncToken.IsCancellationRequested)
			return false;

		if (_Connection == null)
		{
			ObjectDisposedException ex = new("Connection is null");
			Diag.Dug(ex);
			throw ex;
		}

		if (!ConnectionActive)
			return false;

		if (_LinkStage < EnumLinkStage.TriggerGeneratorsLoaded)
		{
			if (GetRawTriggerGeneratorSchema() != null)
				_LinkStage = EnumLinkStage.TriggerGeneratorsLoaded;
		}


		if (_AsyncActive && _TaskHandler.UserCancellation.IsCancellationRequested)
			syncTokenSource.Cancel();
		if (asyncToken.IsCancellationRequested || syncToken.IsCancellationRequested)
			return false;

		if (!ConnectionActive)
			return false;


		if (_LinkStage < EnumLinkStage.GeneratorsLoaded)
		{
			if (GetRawGeneratorSchema() != null)
				_LinkStage = EnumLinkStage.GeneratorsLoaded;
		}


		if (_AsyncActive && _TaskHandler.UserCancellation.IsCancellationRequested)
			syncTokenSource.Cancel();
		if (asyncToken.IsCancellationRequested || syncToken.IsCancellationRequested)
			return false;

		if (!ConnectionActive)
			return false;

		if (_LinkStage < EnumLinkStage.TriggersLoaded)
		{
			if (GetRawTriggerSchema() != null)
				_LinkStage = EnumLinkStage.TriggersLoaded;
		}

		if (_AsyncActive && _TaskHandler.UserCancellation.IsCancellationRequested)
			syncTokenSource.Cancel();
		if (asyncToken.IsCancellationRequested || syncToken.IsCancellationRequested)
			return false;


		if (!SequencesLoaded)
			BuildSequenceTable();


		if (_AsyncActive && _TaskHandler.UserCancellation.IsCancellationRequested)
			syncTokenSource.Cancel();
		if (asyncToken.IsCancellationRequested || syncToken.IsCancellationRequested)
			return false;

		if (_LinkStage != EnumLinkStage.Completed)
			BuildTriggerTable();


		UpdateStatusBar(_LinkStage, _AsyncActive);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Tags the parser status as out of an asynchronous state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override bool AsyncExited()
	{
		// User requested this cancellation so UpdateStatusBar will be placed on UI thread
		// queue, but as fire and forget so we're okay here.
		if (_SyncLockedToken.IsCancellationRequested)
			UpdateStatusBar(_LinkStage, _AsyncActive);

		_AsyncActive = false;

		_TaskHandler = null;
		_ProgressData = default;

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the IDE status bar
	/// </summary>
	/// <remarks>
	/// The bar is only updated at the start of an Execute and end of an Execute. 
	/// </remarks>
	// ---------------------------------------------------------------------------------
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD102:Implement internal logic asynchronously", Justification = "<Pending>")]
	protected bool UpdateStatusBar(EnumLinkStage stage, bool isAsync)
	{
		ThreadHelper.JoinableTaskFactory.Run(async delegate
		{
			// Switch to main thread
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			try
			{
				string async;
				string catalog = $"{_Connection.DataSource}({Path.GetFileNameWithoutExtension(_Connection.Database)})";

				IVsStatusbar statusBar = await ServiceProvider.GetGlobalServiceAsync<SVsStatusbar, IVsStatusbar>(swallowExceptions: false);

				// statusBar.FreezeOutput(0);

				if (_ClearStatusBar)
				{
					statusBar.Clear();
				}
				else if (stage == EnumLinkStage.Start)
				{
					async = isAsync ? "(async)" : "";

					statusBar.SetText($"Updating{async} {catalog} trigger sequence linkage.");
				}
				else if (stage == EnumLinkStage.Completed)
				{
					async = isAsync ? "(async)" : "";

					statusBar.SetText($"Completed{async} {catalog} trigger sequence linkage.");
				}
				else 
				{
					if (isAsync)
					{
						// If it's a user cancel request.
						if (_SyncLockedToken.IsCancellationRequested)
							statusBar.SetText($"Cancelled(async) {catalog} trigger sequence linkage.");
						else
							statusBar.SetText($"Resuming(async) {catalog} trigger sequence linkage.");
					}
					else
					{
						if (_SyncLockedToken.IsCancellationRequested)
							statusBar.SetText($"Resuming {catalog} trigger sequence linkage.");
						else
							statusBar.SetText($"Switched(async) {catalog} trigger sequence linkage to UI thread.");
					}
				}

				if (stage == EnumLinkStage.Completed && !_ClearStatusBar)
				{
					_ = Task.Run(async delegate
					{
						_ClearStatusBar = true;
						await Task.Delay(4000);
						UpdateStatusBar(stage, isAsync);
					});
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		});

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Update the ISE task handler progress bar.
	/// </summary>
	/// <param name="stage">The descriptive name of the completed stage</param>
	/// <param name="progress">The % completion of the linkage build.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	protected override void TaskHandlerProgress(string stage, int progress, TimeSpan elapsed)
	{
		if (_TaskHandler == null)
			return;

		_ProgressData.PercentComplete = progress;

		if (progress == 100)
		{
			_ProgressData.ProgressText = $"Completed. {stage} took {elapsed.Milliseconds}ms.";
		}
		else if (_AsyncLockedToken.IsCancellationRequested || _SyncLockedToken.IsCancellationRequested)
		{
			_ProgressData.ProgressText = $"Cancelled. {progress}% completed. {stage} took {elapsed.Milliseconds}ms.";
		}
		else
		{
			_ProgressData.ProgressText = $"{progress}% completed. {stage} took {elapsed.Milliseconds}ms.";
		}

		
		_TaskHandler.Progress.Report(_ProgressData);
	}


	#endregion Methods

}
