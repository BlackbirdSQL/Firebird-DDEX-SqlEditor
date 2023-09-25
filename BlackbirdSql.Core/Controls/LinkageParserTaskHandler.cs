// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Properties;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TaskStatusCenter;

using static BlackbirdSql.Core.Model.AbstractLinkageParser;


namespace BlackbirdSql.Core.Controls;

// =========================================================================================================
//										LinkageParserTaskHandler Class
//
/// <summary>
/// Handles TaskHandler and statusbar output for the LinkageParser.
/// </summary>
// =========================================================================================================
internal class LinkageParserTaskHandler : IBTaskHandlerClient
{

	// =========================================================================================================
	#region Variables - LinkageParserTaskHandler
	// =========================================================================================================


	private readonly string _Catalog;

	/// <summary>
	/// Handle to the ITaskHandler ProgressData.
	/// </summary>
	protected TaskProgressData _ProgressData = default;


	/// <summary>
	/// Handle to the IDE ITaskHandler.
	/// </summary>
	protected ITaskHandler _TaskHandler = null;

	protected string _TaskHandlerTaskName = "Parsing";


	#endregion Variables




	// =========================================================================================================
	#region Property accessors - LinkageParserTaskHandler
	// =========================================================================================================


	/// <summary>
	/// The name of the running task if the object is currently using the task handler.
	/// </summary>
	public string TaskHandlerTaskName => _TaskHandlerTaskName;


	public CancellationToken UserCancellation
	{
		get
		{
			if (_TaskHandler == null)
				return default;
			return _TaskHandler.UserCancellation;
		}
	}


	#endregion Property accessors




	// -----------------------------------------------------------------------------------------------------
	#region Constructors - LinkageParserTaskHandler
	// -----------------------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected .ctor. LinkageParser's are uniquely distinct to a connection. Use the
	/// Instance() static to create or retrieve a parser for a connection or Site.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public LinkageParserTaskHandler(DbConnection connection)
	{
		// Tracer.Trace(GetType(), "LinkageParserTaskHandle.LinkageParserTaskHandle");
		_Catalog = $"{connection.DataSource} ({Path.GetFileNameWithoutExtension(connection.Database)})";
	}


	#endregion Constructors





	// =========================================================================================================
	#region Methods - LinkageParserTaskHandler
	// =========================================================================================================


	public ITaskHandler GetTaskHandler()
	{
		return _TaskHandler;
	}


	public TaskProgressData GetProgressData()
	{
		return _ProgressData;
	}



	public void PreRegister(bool canBeCancelled)
	{
		IVsTaskStatusCenterService tsc = ServiceProvider.GetGlobalServiceAsync<SVsTaskStatusCenterService,
			IVsTaskStatusCenterService>(swallowExceptions: false).Result;

		TaskHandlerOptions options = default;
		options.Title = Resources.LinkageParserTaskHandlerTitle.Res(_Catalog);
		options.ActionsAfterCompletion = CompletionActions.None;

		_ProgressData = default;
		_ProgressData.CanBeCanceled = canBeCancelled;

		_TaskHandler = tsc.PreRegister(options, _ProgressData);

	}


	public void RegisterTask(Task<bool> asyncPayloadLauncher)
	{
		_TaskHandler.RegisterTask(asyncPayloadLauncher);
	}


	public virtual bool Progress(string text)
	{
		return TaskHandlerProgress(text);
	}


	// ---------------------------------------------------------------------------------
		/// <summary>
		/// Moves back onto the UI thread and updates the IDE task handler progress bar
		/// with project update information.
		/// </summary>
		// ---------------------------------------------------------------------------------
	public virtual bool TaskHandlerProgress(string text)
	{
		if (_ProgressData.PercentComplete == null)
		{
			_ProgressData.PercentComplete = 0;
			if (_TaskHandler != null)
				Diag.TaskHandlerProgress(this, Resources.LinkageParserStarted);
		}

		Diag.TaskHandlerProgress(this, text);

		return true;
	}



	public virtual bool Progress(int progress, int elapsed, long totalElapsed, bool enabled, bool asyncActive)
	{
		return Progress(null, progress, elapsed, totalElapsed, enabled, asyncActive);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	/// <param name="progress">The % completion of TaskHandlerTaskName.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	public virtual bool TaskHandlerProgress(int progress, int elapsed)
	{
		return false;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the IDE task handler progress bar and possibly the output pane.
	/// </summary>
	/// <param name="stage">The descriptive name of the completed stage</param>
	/// <param name="progress">The % completion of the linkage build.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	public bool Progress(string stage, int progress, long elapsed, long totalElapsed, bool enabled, bool asyncActive)
	{
		bool completed = false;
		string text;

		// These calls occur immediately after a previous call to TaskHandlerProgress()
		// so we need to ensure output is contiguous.
		if (asyncActive && (elapsed == -3 || progress == 100))
			Thread.Sleep(10);


		if (progress == 0)
		{
			// If we're pausing async and nothing has happened just exit.
			if (elapsed == -2)
				return true;

			text = Resources.LinkageParserUpdating;
		}
		else if (progress == 100)
		{
			completed = true;
			text = Resources.LinkageParserCompleted.Res(progress, stage, totalElapsed);
		}
		else
		{
			if (asyncActive)
			{
				// If it's a user cancel request.
				if (!enabled)
				{
					completed = true;
					text = Resources.LinkageParserCancelled.Res(progress, stage, elapsed);
				}
				else
				{
					if (elapsed == -1)
						text = Resources.LinkageParserResumingAsync;
					else if (elapsed == -2)
						text = Resources.LinkageParserPausingAsync;
					else if (elapsed == -3)
						text = Resources.LinkageParserPercentCompletedStage.Res(progress, stage);
					else
						text = Resources.LinkageParserPercentCompletedStageElapsed.Res(progress, stage, elapsed);

				}
			}
			else
			{
				if (elapsed == -1)
					text = Resources.LinkageParserSwitchedToUiThread;
				else if (elapsed == -3)
					text = Resources.LinkageParserPercentCompletedStage.Res(progress, stage);
				else
					text = Resources.LinkageParserPercentCompletedStageElapsed.Res(progress, stage, elapsed);
			}

		}

		_ProgressData.PercentComplete = progress;

		return Diag.TaskHandlerProgress(this, text, completed);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the status bar.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool Status(EnLinkStage stage, long totalElapsed, bool enabled, bool asyncActive)
	{
		// string async;
		string catalog = _Catalog;

		bool clear = false;
		string text;

		if (stage == EnLinkStage.Start)
		{
			// async = isAsync ? " (async)" : "";
			text = Resources.LinkageParserUpdatingCatalog.Res(catalog);
		}
		else if (stage == EnLinkStage.Completed)
		{
			// async = isAsync ? " (async)" : "";
			text = Resources.LinkageParserCompletedCatalog.Res(catalog, totalElapsed);
		}
		else
		{
			if (asyncActive)
			{
				// If it's a user cancel request.
				if (!enabled)
					text = Resources.LinkageParserCancelledCatalog.Res(catalog);
				else
					text = Resources.LinkageParserResumingCatalog.Res(catalog);
			}
			else
			{
				if (!enabled)
					text = Resources.LinkageParserCancelledCatalog.Res(catalog);
				else
					text = Resources.LinkageParserCatalogSwitchedToUiThread.Res(catalog);
			}
		}

		if (stage == EnLinkStage.Completed || (asyncActive && !enabled))
		{
			clear = true;
		}

		return UpdateStatusBar(text, clear);
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the VisualStudio status bar.
	/// </summary>
	/// <param name="message">Thye status bar display text.</param>
	/// <param name="complete">Tags this message as a completion message.</param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public virtual bool UpdateStatusBar(string message, bool complete = false)
	{
		return Diag.UpdateStatusBar(message, complete);
	}


	public static int GetPercentageComplete(EnLinkStage stage, bool starting = false)
	{
		switch (stage)
		{
			case EnLinkStage.Start:
				return 0;
			case EnLinkStage.GeneratorsLoaded:
				return starting ? 4 : 13;
			case EnLinkStage.TriggerDependenciesLoaded:
				return starting ? 15 : 43;
			case EnLinkStage.TriggersLoaded:
				return starting ? 48 : 88;
			case EnLinkStage.SequencesPopulated:
				return starting ? 89 : 94;
			case EnLinkStage.LinkageCompleted:
				return starting ? 96 : 99;
			default:
				return 100;
		}
	}


	#endregion Taskhandler and Status Bar


}
