﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Data.Model;
using BlackbirdSql.Data.Properties;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.TaskStatusCenter;

using static BlackbirdSql.Data.Model.AbstractLinkageParser;



namespace BlackbirdSql.Data.Ctl;


// =========================================================================================================
//										LinkageParserTaskHandler Class
//
/// <summary>
/// Handles TaskHandler and statusbar output for the LinkageParser.
/// </summary>
// =========================================================================================================
public class LinkageParserTaskHandler : IBsTaskHandlerClient
{

	// -----------------------------------------------------------------------------------------------------
	#region Constructors - LinkageParserTaskHandler
	// -----------------------------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Protected .ctor. LinkageParser's are uniquely distinct to a connection. Use the
	/// Instance() static to create or retrieve a parser for a connection or Site.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public LinkageParserTaskHandler(string connectionString)
	{

		// Evs.Trace(GetType(), "LinkageParserTaskHandle.LinkageParserTaskHandle");

		_ConnectionString = connectionString;
		// _DatasetKey = ApcManager.GetRegisterConnectionDatasetKey(root);
	}


	#endregion Constructors





	// =========================================================================================================
	#region Fields - LinkageParserTaskHandler
	// =========================================================================================================


	private string _DatasetKey = null;
	/// <summary>
	/// Handle to the ITaskHandler ProgressData.
	/// </summary>
	protected TaskProgressData _ProgressData = default;

	private readonly string _ConnectionString = null;

	/// <summary>
	/// Handle to the IDE ITaskHandler.
	/// </summary>
	private ITaskHandler _TaskHandler = null;

	protected string _TaskHandlerTaskName = "Parsing";



	#endregion Fields




	// =========================================================================================================
	#region Property accessors - LinkageParserTaskHandler
	// =========================================================================================================


	private string DatasetKey
	{
		get
		{
			if (_DatasetKey != null)
				return _DatasetKey;

			_DatasetKey = ApcManager.GetConnectionQualifiedName(_ConnectionString);

			return _DatasetKey;
		}
	}

	
	/// <summary>
	/// The name of the running task if the object is currently using the task handler.
	/// </summary>
	public string TaskHandlerTaskName => _TaskHandlerTaskName;

	private IVsTaskStatusCenterService StatusCenterService =>
		ApcManager.StatusCenterService;


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
		TaskHandlerOptions options = default;
		options.Title = ControlsResources.LinkageParser_TaskHandlerTitle.Fmt(DatasetKey);
		options.ActionsAfterCompletion = CompletionActions.None;

		_ProgressData = default;
		_ProgressData.CanBeCanceled = canBeCancelled;

		
		_TaskHandler = StatusCenterService.PreRegister(options, _ProgressData);

	}


	public void RegisterTask(Task<bool> payloadLauncher)
	{
		try
		{
			_TaskHandler.RegisterTask(payloadLauncher);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
		}
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
				Diag.TaskHandlerProgress(this, ControlsResources.LinkageParser_Started);
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
		if (asyncActive && (elapsed == LinkageParser.C_Elapsed_StageCompleted || progress == 100))
		{
			Thread.Sleep(10);
			Thread.Yield();
		}


		if (progress == 0)
		{
			// If we're pausing async and nothing has happened just exit.
			if (elapsed == LinkageParser.C_Elapsed_Disabling)
				return true;

			text = ControlsResources.LinkageParser_Updating;
		}
		else if (progress == 100)
		{
			completed = true;
			text = ControlsResources.LinkageParser_Completed.Fmt(progress, stage, totalElapsed);
		}
		else
		{
			if (asyncActive)
			{
				// If it's a user cancel request.
				if (!enabled)
				{
					completed = true;
					text = ControlsResources.LinkageParser_Cancelled.Fmt(progress, stage, elapsed);
				}
				else
				{
					if (elapsed == LinkageParser.C_Elapsed_Resuming)
						text = ControlsResources.LinkageParser_ResumingAsync;
					else if (elapsed == LinkageParser.C_Elapsed_Disabling)
						text = ControlsResources.LinkageParser_PausingAsync;
					else if (elapsed == LinkageParser.C_Elapsed_StageCompleted)
						text = ControlsResources.LinkageParser_PercentCompletedStage.Fmt(progress, stage);
					else
						text = ControlsResources.LinkageParser_PercentCompletedStageElapsed.Fmt(progress, stage, elapsed);

				}
			}
			else
			{
				if (elapsed == LinkageParser.C_Elapsed_Resuming)
					text = ControlsResources.LinkageParser_SwitchedToUiThread;
				else if (elapsed == LinkageParser.C_Elapsed_StageCompleted)
					text = ControlsResources.LinkageParser_PercentCompletedStage.Fmt(progress, stage);
				else
					text = ControlsResources.LinkageParser_PercentCompletedStageElapsed.Fmt(progress, stage, elapsed);
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
		string catalog = DatasetKey;

		bool clear = false;
		string text;

		if (stage == EnLinkStage.Start)
		{
			// async = isAsync ? " (async)" : "";
			text = ControlsResources.LinkageParser_UpdatingCatalog.Fmt(catalog);
		}
		else if (stage == EnLinkStage.Completed)
		{
			// async = isAsync ? " (async)" : "";
			text = ControlsResources.LinkageParser_CompletedCatalog.Fmt(catalog, totalElapsed);
		}
		else
		{
			if (asyncActive)
			{
				// If it's a user cancel request.
				if (!enabled)
					text = ControlsResources.LinkageParser_CancelledCatalog.Fmt(catalog);
				else
					text = ControlsResources.LinkageParser_ResumingCatalog.Fmt(catalog);
			}
			else
			{
				if (!enabled)
					text = ControlsResources.LinkageParser_CancelledCatalog.Fmt(catalog);
				else
					text = ControlsResources.LinkageParser_CatalogSwitchedToUiThread.Fmt(catalog);
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
