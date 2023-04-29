using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

using FirebirdSql.Data.FirebirdClient;

using C5;
using BlackbirdDsl;
using BlackbirdSql.VisualStudio.Ddex.Schema;

namespace BlackbirdSql.Common.Extensions;



// Deadlock warning message suppression
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]



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
internal class LinkageParser : AbstractLinkageParser
{


	/// <summary>
	/// Protected .ctor. LinkageParser's are uniquely distinct to a connection. Use the Instance()
	/// static to create or retrieve a parser for a connection.
	/// </summary>
	/// <param name="connection"></param>
	protected LinkageParser(FbConnection connection) : base(connection)
	{
	}


	/// <summary>
	/// Retrieves or creates a distinct unique parser for a connection.
	/// </summary>
	/// <param name="connection"></param>
	/// <returns></returns>
	public static new LinkageParser Instance(FbConnection connection)
	{
		LinkageParser parser = (LinkageParser)AbstractLinkageParser.Instance(connection);

		parser ??= new(connection);

		return parser;
	}


	/// <summary>
	/// Ensures the async queue is cleared out before passing control back to any sync request.
	/// </summary>
	/// <returns></returns>
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


	/// <summary>
	/// Increments the sync call counter and suspends any async tasks. This should be called on every occasion that
	/// a sync db call is made in the package.
	/// </summary>
	/// <returns></returns>
	public override bool EnterSync()
	{
		_SyncActive++;
		return ClearAsyncQueue();
	}

	/// <summary>
	/// Decrements the sync call counter. This should be called on every occasion that
	/// a sync db call is exited in the package.
	/// When the counter reaches zero, outstanding async tasks are resumed.
	/// </summary>
	public override void ExitSync()
	{
		_SyncActive--;
		if (_SyncActive < 0)
			_SyncActive = 0;

		if (_SyncActive == 0 && !Loaded)
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

	/// <summary>
	/// Begins or resumes the Trigger / Generator linkage build.
	/// </summary>
	/// <param name="asyncLockedToken">
	/// Passing a cancellation token indicates that the call has been made asynchronously.
	/// </param>
	/// <returns></returns>
	public override bool Execute(CancellationToken asyncLockedToken = default)
	{
		if (!ClearToLoad)
			return false;

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

		if (asyncLockedToken == default && !EnterSync())
		{
			ExitSync();
			return false;
		}

		if (!ConnectionActive)
		{
			if (asyncLockedToken == default)
				ExitSync();
			return false;
		}

		if (!_RawGeneratorsLoaded)
		{
			if (GetRawGeneratorSchema() != null)
				_RawGeneratorsLoaded = true;
		}

		if (asyncLockedToken.IsCancellationRequested)
			return false;

		if (!ConnectionActive)
		{
			if (asyncLockedToken == default)
				ExitSync();
			return false;
		}


		if (!_RawTriggerGeneratorsLoaded)
		{
			if (GetRawTriggerGeneratorSchema() != null)
				_RawTriggerGeneratorsLoaded = true;
		}

		if (asyncLockedToken.IsCancellationRequested)
			return false;

		if (!ConnectionActive)
		{
			if (asyncLockedToken == default)
				ExitSync();
			return false;
		}

		if (!_RawTriggersLoaded)
		{
			if (GetRawTriggerSchema() != null)
				_RawTriggersLoaded = true;
		}

		if (asyncLockedToken.IsCancellationRequested)
			return false;

		if (!ConnectionActive)
		{
			if (asyncLockedToken == default)
				ExitSync();
			return false;
		}


		if (!SequencesLoaded)
		{
			BuildSequenceTable();
		}

		if (asyncLockedToken.IsCancellationRequested)
			return false;

		if (!ConnectionActive)
		{
			if (asyncLockedToken == default)
				ExitSync();
			return false;
		}

		if (!_Loaded)
		{
			BuildTriggerTable();
		}

		if (asyncLockedToken == default)
			ExitSync();

		return true;
	}


	/// <summary>
	/// Initiates an async build of the linkage tables.
	/// </summary>
	public override bool AsyncExecute()
	{
		if (!ClearToLoadAsync)
			return false;

		_AsyncActive = true;

		_ExternalAsyncTask = Task.Factory.StartNew<bool>(() => Execute(_AsyncLockedToken), _SyncLockedToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

		_ = _ExternalAsyncTask.AppendAction(() => { AsyncExited(); });

		return true;
	}


	/// <summary>
	/// Tags the parser status as out of an asynchronous state.
	/// </summary>
	protected override void AsyncExited()
	{
		_AsyncActive = false;

	}

	





}
