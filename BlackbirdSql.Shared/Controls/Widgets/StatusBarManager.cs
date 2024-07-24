// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.StatusBar.QEStatusBarManager

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Media;
using System.Timers;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;



namespace BlackbirdSql.Shared.Controls.Widgets;


// =========================================================================================================
//										StatusBarManager Class
//
/// <summary>
/// Manages the status bar output.
/// </summary>
// =========================================================================================================
public sealed class StatusBarManager : IDisposable
{

	// ----------------------------------------------------
	#region Constructors / Destructors - StatusBarManager
	// ----------------------------------------------------


	public StatusBarManager()
	{
		CurrentState = EnQeStatusBarKnownStates.Unknown;
		SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
	}



	static StatusBarManager()
	{
		// DefaultColor = SystemColors.Control;
		S_NumOfRowsPanel = new Bitmap(ControlsResources.ImgStatusBarConnect);
		S_OfflineBitmap = new Bitmap(ControlsResources.ImgStatusBarOffline);
		S_ExecSuccessBitmap = new Bitmap(ControlsResources.ImgStatusOK);
		S_ExecWithErrorBitmap = new Bitmap(ControlsResources.ImgStatusBarError);
		S_ExecCancelledBitmap = new Bitmap(ControlsResources.ImgStatusBarCancel);
		S_ExecutingBitmaps =
		[
			new Bitmap(ControlsResources.ImgProgressBar_00),
			new Bitmap(ControlsResources.ImgProgressBar_01),
			new Bitmap(ControlsResources.ImgProgressBar_02),
			new Bitmap(ControlsResources.ImgProgressBar_03),
			new Bitmap(ControlsResources.ImgProgressBar_04),
			new Bitmap(ControlsResources.ImgProgressBar_05),
			new Bitmap(ControlsResources.ImgProgressBar_06),
			new Bitmap(ControlsResources.ImgProgressBar_07),
			new Bitmap(ControlsResources.ImgProgressBar_08),
			new Bitmap(ControlsResources.ImgProgressBar_09),
			new Bitmap(ControlsResources.ImgProgressBar_10),
			new Bitmap(ControlsResources.ImgProgressBar_11),
			new Bitmap(ControlsResources.ImgProgressBar_12),
			new Bitmap(ControlsResources.ImgProgressBar_13),
			new Bitmap(ControlsResources.ImgProgressBar_14),
			new Bitmap(ControlsResources.ImgProgressBar_15),
			new Bitmap(ControlsResources.ImgProgressBar_16),
			new Bitmap(ControlsResources.ImgProgressBar_17),
			new Bitmap(ControlsResources.ImgProgressBar_18),
			new Bitmap(ControlsResources.ImgProgressBar_19),
			new Bitmap(ControlsResources.ImgProgressBar_20),
			new Bitmap(ControlsResources.ImgProgressBar_21)
		];
		S_ExecutingCancelBitmaps =
		[
			new Bitmap(ControlsResources.ImgCancelBar_00),
			new Bitmap(ControlsResources.ImgCancelBar_01),
			new Bitmap(ControlsResources.ImgCancelBar_02),
			new Bitmap(ControlsResources.ImgCancelBar_03),
			new Bitmap(ControlsResources.ImgCancelBar_04),
			new Bitmap(ControlsResources.ImgCancelBar_05),
			new Bitmap(ControlsResources.ImgCancelBar_06),
			new Bitmap(ControlsResources.ImgCancelBar_07),
			new Bitmap(ControlsResources.ImgCancelBar_08),
			new Bitmap(ControlsResources.ImgCancelBar_09),
			new Bitmap(ControlsResources.ImgCancelBar_10),
			new Bitmap(ControlsResources.ImgCancelBar_11),
			new Bitmap(ControlsResources.ImgCancelBar_12),
			new Bitmap(ControlsResources.ImgCancelBar_13),
			new Bitmap(ControlsResources.ImgCancelBar_14),
			new Bitmap(ControlsResources.ImgCancelBar_15),
			new Bitmap(ControlsResources.ImgCancelBar_16),
			new Bitmap(ControlsResources.ImgCancelBar_17),
			new Bitmap(ControlsResources.ImgCancelBar_18),
			new Bitmap(ControlsResources.ImgCancelBar_19),
			new Bitmap(ControlsResources.ImgCancelBar_20),
			new Bitmap(ControlsResources.ImgCancelBar_21)
		];
	}


	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}



	private void Dispose(bool disposing)
	{
		if (disposing && !_Disposed)
		{
			QueryManager qryMgr = QryMgr;
			if (qryMgr != null)
			{
				qryMgr.ExecutionStartedEvent -= OnQueryExecutionStarted;
				qryMgr.ExecutionCompletedEvent -= OnQueryExecutionCompleted;
				qryMgr.StatusChangedEvent -= OnStatusChanged;
			}

			((Component)(object)_CompletedTimePanel).Dispose();
			_CompletedTimePanel = null;
			((Component)(object)_DatabaseNamePanel).Dispose();
			_DatabaseNamePanel = null;
			((Component)(object)_ExecutionTimePanel).Dispose();
			_ExecutionTimePanel = null;
			((Component)(object)_GeneralPanel).Dispose();
			_GeneralPanel = null;
			((Component)(object)_NumOfRowsPanel).Dispose();
			_NumOfRowsPanel = null;
			((Component)(object)_ServerNamePanel).Dispose();
			_ServerNamePanel = null;
			((Component)(object)_UserNamePanel).Dispose();
			_UserNamePanel = null;
			if (_ElapsedExecutionTimer != null)
			{
				_ElapsedExecutionTimer.Dispose();
				_ElapsedExecutionTimer = null;
			}

			SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
		}

		_Disposed = true;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - StatusBarManager
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private bool _Disposed = false;
	private bool _RowCountValid;
	private readonly Queue<Action> _StatusBarUpdateActions = new Queue<Action>();
	private System.Timers.Timer _ElapsedExecutionTimer;

	private StatusStrip _StatusStrip;
	private AnimatedStatusStripItem _GeneralPanel = new AnimatedStatusStripItem(100);
	private ToolStripStatusLabelWithMaxLimit _ServerNamePanel = new ToolStripStatusLabelWithMaxLimit(30, 150);
	private ToolStripStatusLabelWithMaxLimit _UserNamePanel = new ToolStripStatusLabelWithMaxLimit(25, 150);
	private ToolStripStatusLabelWithMaxLimit _DatabaseNamePanel = new ToolStripStatusLabelWithMaxLimit(25, 150);
	private ToolStripStatusLabelWithMaxLimit _ExecutionTimePanel = new ToolStripStatusLabelWithMaxLimit(15, 70);
	private ToolStripStatusLabelWithMaxLimit _CompletedTimePanel = new ToolStripStatusLabelWithMaxLimit(25, 150);
	private ToolStripStatusLabelWithMaxLimit _NumOfRowsPanel = new ToolStripStatusLabelWithMaxLimit(15, 100);

	private static readonly Image S_NumOfRowsPanel;
	private static readonly Image S_OfflineBitmap;
	private static readonly Image S_ExecSuccessBitmap;
	private static readonly Image S_ExecWithErrorBitmap;
	private static readonly Image S_ExecCancelledBitmap;

	private static readonly Image[] S_ExecutingBitmaps;
	private static readonly Image[] S_ExecutingCancelBitmaps;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - StatusBarManager
	// =========================================================================================================


	public EnQeStatusBarKnownStates CurrentState { get; private set; }

	public StatusStrip StatusStrip => _StatusStrip;

	public AnimatedStatusStripItem GeneralPanel => _GeneralPanel;

	public ToolStripStatusLabelWithMaxLimit ServerNamePanel => _ServerNamePanel;

	public ToolStripStatusLabelWithMaxLimit UserNamePanel => _UserNamePanel;

	public ToolStripStatusLabelWithMaxLimit DatabaseNamePanel => _DatabaseNamePanel;

	public ToolStripStatusLabelWithMaxLimit ExecutionTimePanel => _ExecutionTimePanel;

	public ToolStripStatusLabelWithMaxLimit CompletedTimePanel => _CompletedTimePanel;

	public ToolStripStatusLabelWithMaxLimit NumOfRowsPanel => _NumOfRowsPanel;

	private DateTime ExecutionStartTime { get; set; }

	private IBSqlEditorWindowPane EditorWindowPane { get; set; }

	private QueryManager QryMgr
	{
		get
		{
			QueryManager result = null;
			AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(EditorWindowPane.DocData);
			if (auxDocData != null)
			{
				result = auxDocData.QryMgr;
			}

			return result;
		}
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - StatusBarManager
	// =========================================================================================================


	/*
	private static string FormatTimeSpanForStatus(TimeSpan ts)
	{
		return new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds, 0).ToString();
	}
	*/



	public void Initialize(StatusStrip statusStrip, bool rowCountValid, IBSqlEditorWindowPane editorWindowPane)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		EditorWindowPane = editorWindowPane;
		QryMgr.StatusChangedEvent += OnStatusChanged;
		QryMgr.ExecutionStartedEvent += OnQueryExecutionStarted;
		QryMgr.ExecutionCompletedEvent += OnQueryExecutionCompleted;
		// Tracer.Trace(GetType(), "StatusBarManager.Initialize", "_RowCountValid = {0}", rowCountValid);
		_StatusStrip = statusStrip;
		_StatusStrip.LayoutStyle = ToolStripLayoutStyle.Table;
		_RowCountValid = rowCountValid;
		_GeneralPanel.SetParent(_StatusStrip);
		((ToolStripStatusLabel)(object)_GeneralPanel).Spring = true;
		_StatusStrip.RenderMode = ToolStripRenderMode.Professional;
		_StatusStrip.Renderer = new EditorStatusStripRenderer(_StatusStrip);
		_ = QryMgr.Strategy.ConnInfo;
		IDbConnection connection = QryMgr.Strategy.Connection;

		if (connection != null && connection.State == ConnectionState.Open)
			TransitionIntoOnlineMode(useNewConnectionOpenedState: false);
		else
			TransitionIntoOfflineMode();
	}



	private void EnqueAndExecuteStatusBarUpdate(Action action)
	{
		lock (_LockLocal)
		{
			_StatusBarUpdateActions.Enqueue(action);
		}

		if (ThreadHelper.CheckAccess())
		{
			ProcessStatusBarUpdateActions();
			return;
		}

		Action method = ProcessStatusBarUpdateActions;

		_StatusStrip.BeginInvoke(method);
	}



	private void ProcessStatusBarUpdateActions()
	{
		if (_Disposed)
			return;


		while (true)
		{
			Action action = null;

			lock (_LockLocal)
			{
				if (_StatusBarUpdateActions.Count == 0)
					return;

				action = _StatusBarUpdateActions.Dequeue();
			}

			action?.Invoke();
		}
	}



	private void RebuildPanels()
	{
		_StatusStrip.Items.Clear();
		_StatusStrip.Items.Add((ToolStripItem)(object)_GeneralPanel);
		if (PersistentSettings.EditorStatusBarIncludeServerName)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_ServerNamePanel);
		}

		if (PersistentSettings.EditorStatusBarIncludeLoginName)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_UserNamePanel);
		}

		if (PersistentSettings.EditorStatusBarIncludeDatabaseName)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_DatabaseNamePanel);
		}

		if (PersistentSettings.EditorStatusBarExecutionTimeMethod == EnExecutionTimeMethod.Elapsed)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_ExecutionTimePanel);
		}

		if (PersistentSettings.EditorStatusBarExecutionTimeMethod == EnExecutionTimeMethod.End)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_CompletedTimePanel);
		}

		if (_RowCountValid && PersistentSettings.EditorStatusBarIncludeRowCount)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_NumOfRowsPanel);
		}
	}


	private void ResetDatasetId()
	{
		string datasetId = QryMgr.Strategy.DatasetId;
		((ToolStripItem)(object)_DatabaseNamePanel).Text = datasetId ?? string.Empty;
	}



	private void ResetPanelsForOnlineMode()
	{
		AbstractConnectionStrategy strategy = QryMgr.Strategy;
		string displayUserName = strategy.DisplayUserName;
		string datasetId = strategy.DatasetId;
		string text = strategy.DisplayServerName;
		Version serverVersion = strategy.GetServerVersion();

		if (serverVersion != null)
		{
			text = ControlsResources.StatusBar_ServerNameAndBuild.FmtRes(text,
				serverVersion.Major, serverVersion.Minor);
		}

		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			RebuildPanels();
		}

		((ToolStripItem)(object)_ServerNamePanel).Text = text;
		((ToolStripItem)(object)_UserNamePanel).Text = displayUserName;
		((ToolStripItem)(object)_DatabaseNamePanel).Text = datasetId ?? string.Empty;
		if (!QryMgr.QueryExecutionEndTime.HasValue || !QryMgr.QueryExecutionStartTime.HasValue)
		{
			((ToolStripItem)(object)_ExecutionTimePanel).Text = "00:00:00";
			((ToolStripItem)(object)_CompletedTimePanel).Text = "00:00:00";
		}
		else
		{
			((ToolStripItem)(object)_ExecutionTimePanel).Text = (QryMgr.QueryExecutionEndTime.Value - QryMgr.QueryExecutionStartTime.Value).FmtStatus();
			((ToolStripItem)(object)_CompletedTimePanel).Text = QryMgr.QueryExecutionEndTime.Value.ToString(CultureInfo.CurrentCulture);
		}

		SetRowsAffected(QryMgr.RowsAffected);
	}



	private void SetExecutionCompletedTime(DateTime endTime)
	{
		((ToolStripItem)(object)_CompletedTimePanel).Text = endTime.ToString(CultureInfo.CurrentCulture);
	}



	private string SetKnownState(EnQeStatusBarKnownStates newState)
	{
		// Tracer.Trace(GetType(), "StatusBarManager.SetKnownState", "newState = {0}", newState);

		if (CurrentState != newState)
		{
			if (QryMgr.IsExecuting)
			{
				if (newState == EnQeStatusBarKnownStates.Connected
					|| newState == EnQeStatusBarKnownStates.Connecting
					|| newState == EnQeStatusBarKnownStates.Disconnected
					|| newState == EnQeStatusBarKnownStates.Offline
					|| newState == EnQeStatusBarKnownStates.NewConnectionOpened)
				{
					return null;
				}
			}


			CurrentState = newState;
			_GeneralPanel.BeginInit();
			switch (newState)
			{
				case EnQeStatusBarKnownStates.Connecting:
					_GeneralPanel.SetOneImage(S_OfflineBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBar_Connecting;
					break;
				case EnQeStatusBarKnownStates.Connected:
					_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBar_Connected;
					break;
				case EnQeStatusBarKnownStates.Disconnected:
					_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBar_Disconnected;
					break;
				case EnQeStatusBarKnownStates.NewConnectionOpened:
					_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBar_NewConnectionOpened;
					break;
				case EnQeStatusBarKnownStates.Executing:
					_GeneralPanel.SetImages(S_ExecutingBitmaps, false);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBar_ExecutingQuery;
					_GeneralPanel.StartAnimate();
					break;
				case EnQeStatusBarKnownStates.CancellingExecution:
					_GeneralPanel.SetImages(S_ExecutingCancelBitmaps, false);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBar_Cancelling;
					_GeneralPanel.StartAnimate();
					break;
				case EnQeStatusBarKnownStates.ExecutionCancelled:
					_GeneralPanel.SetOneImage(S_ExecCancelledBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = QryMgr.HasTransactions
						? ControlsResources.StatusBar_QueryCancelledRollback : ControlsResources.StatusBar_QueryCancelled;
					ResetDatasetId();
					break;
				case EnQeStatusBarKnownStates.ExecutionFailed:
					_GeneralPanel.SetOneImage(S_ExecWithErrorBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = QryMgr.HasTransactions
						? ControlsResources.StatusBar_QueryCompletedWithErrorsRollback : ControlsResources.StatusBar_QueryCompletedWithErrors;
					ResetDatasetId();
					break;
				case EnQeStatusBarKnownStates.ExecutionOk:
					_GeneralPanel.SetOneImage(S_ExecSuccessBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text =
						ControlsResources.StatusBar_QueryCompletedSuccessfully.FmtRes(!QryMgr.QueryExecutionEndTime.HasValue ? string.Empty : QryMgr.QueryExecutionEndTime);
					ResetDatasetId();
					break;
				case EnQeStatusBarKnownStates.ExecutionTimedOut:
					_GeneralPanel.SetOneImage(S_ExecWithErrorBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBar_QueryTimedOut;
					ResetDatasetId();
					break;
				case EnQeStatusBarKnownStates.Offline:
					_GeneralPanel.SetOneImage(S_OfflineBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBar_Offline;
					break;
				case EnQeStatusBarKnownStates.Parsing:
					_GeneralPanel.SetImages(S_ExecutingBitmaps, false);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBar_ParsingQueryBatch;
					_GeneralPanel.StartAnimate();
					break;
				case EnQeStatusBarKnownStates.Unknown:
					_GeneralPanel.SetOneImage(null);
					((ToolStripItem)(object)_GeneralPanel).Text = string.Empty;
					break;
			}

			if (newState == EnQeStatusBarKnownStates.Executing || newState == EnQeStatusBarKnownStates.Offline)
			{
				DateTime now = DateTime.Now;
				SetExecutionCompletedTime(now);
				SetRowsAffected(0L);
			}

			if (newState == EnQeStatusBarKnownStates.ExecutionTimedOut || newState == EnQeStatusBarKnownStates.ExecutionOk
				|| newState == EnQeStatusBarKnownStates.ExecutionFailed || newState == EnQeStatusBarKnownStates.ExecutionCancelled)
			{
				if (QryMgr.QueryExecutionStartTime.HasValue && QryMgr.QueryExecutionEndTime.HasValue)
				{
					_ = QryMgr.QueryExecutionStartTime.Value;
					DateTime value = QryMgr.QueryExecutionEndTime.Value;
					SetExecutionCompletedTime(value);
				}

				SetRowsAffected(QryMgr.RowsAffected);
				AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(EditorWindowPane.DocData);
				if (auxDocData != null && auxDocData.LiveSettings.EditorResultsPlaySounds)
				{
					new SoundPlayer().Play();
				}
			}

			((ToolStripItem)(object)_GeneralPanel).ToolTipText = ((ToolStripItem)(object)_GeneralPanel).Text;
			_GeneralPanel.EndInit();
		}

		UpdateStatusBar();

		return ((ToolStripItem)(object)_GeneralPanel).Text;
	}



	private void SetRowsAffected(long rows)
	{
		string text = string.Format(CultureInfo.CurrentCulture, ControlsResources.StatusBar_NumberOfReturnedRows, rows);
		if (text != ((ToolStripItem)(object)_NumOfRowsPanel).Text)
		{
			((ToolStripItem)(object)_NumOfRowsPanel).Text = text;
		}
	}



	private void TransitionIntoOfflineMode()
	{
		// Tracer.Trace(GetType(), "StatusBarManager.TransitionIntoOfflineMode", "", null);
		AbstractConnectionStrategy strategy = QryMgr.Strategy;
		if (strategy != null && strategy.ConnInfo != null)
		{
			ResetPanelsForOnlineMode();
			SetKnownState(EnQeStatusBarKnownStates.Disconnected);
		}
		else if (CurrentState != EnQeStatusBarKnownStates.Offline)
		{
			_StatusStrip.Items.Clear();
			_StatusStrip.Items.Add((ToolStripItem)(object)_GeneralPanel);
			SetKnownState(EnQeStatusBarKnownStates.Offline);
		}
	}



	private void TransitionIntoOnlineMode(bool useNewConnectionOpenedState)
	{
		AbstractConnectionStrategy strategy = QryMgr.Strategy;

		if (strategy.ConnInfo != null)
		{
			ResetPanelsForOnlineMode();
			if (useNewConnectionOpenedState)
			{
				SetKnownState(EnQeStatusBarKnownStates.NewConnectionOpened);
			}
			else
			{
				SetKnownState(EnQeStatusBarKnownStates.Connected);
			}

			// userName = strategy.DisplayUserName;
		}
	}



	private void UpdateStatusBar()
	{
		_StatusStrip.BackColor = QryMgr.Strategy.StatusBarColor;
		if (_StatusStrip.BackColor == SystemColors.Control)
		{
			_ = _StatusStrip.BackColor = _StatusStrip.BackColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX.VSCOLOR_ENVIRONMENT_BACKGROUND);
		}

		if ((double)_StatusStrip.BackColor.GetBrightness() < 0.5)
		{
			_StatusStrip.ForeColor = Color.White;
		}
		else
		{
			_StatusStrip.ForeColor = Color.Black;
		}

		if (_StatusStrip.Dock == DockStyle.Top
			&& PersistentSettings.EditorContextStatusBarPosition != EnStatusBarPosition.Top
			|| _StatusStrip.Dock == DockStyle.Bottom
			&& PersistentSettings.EditorContextStatusBarPosition != EnStatusBarPosition.Bottom)
		{
			switch (PersistentSettings.EditorContextStatusBarPosition)
			{
				case EnStatusBarPosition.Bottom:
					_StatusStrip.Dock = DockStyle.Bottom;
					break;
				case EnStatusBarPosition.Top:
					_StatusStrip.Dock = DockStyle.Top;
					break;
			}
		}

		if (PersistentSettings.LayoutPropertyChanged)
		{
			RebuildPanels();
			PersistentSettings.LayoutPropertyChanged = false;
		}
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - StatusBarManager
	// =========================================================================================================


	private void OnExecutionTimerElapsed(object sender, ElapsedEventArgs args)
	{
		void a()
		{
			((ToolStripItem)(object)_ExecutionTimePanel).Text = (args.SignalTime - ExecutionStartTime).FmtStatus();
		}
		EnqueAndExecuteStatusBarUpdate(a);
	}



	private void OnQueryExecutionCompleted(object sender, QueryExecutionCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnQueryExecutionCompleted()", "args.ExecResult: {0}.", args.ExecutionResult);

		void updateAction()
		{
			if ((args.ExecutionResult & EnScriptExecutionResult.Cancel) == EnScriptExecutionResult.Cancel)
			{
				SetKnownState(EnQeStatusBarKnownStates.ExecutionCancelled);
			}
			else if ((args.ExecutionResult & EnScriptExecutionResult.Failure) == EnScriptExecutionResult.Failure)
			{
				SetKnownState(EnQeStatusBarKnownStates.ExecutionFailed);
			}
			else if ((args.ExecutionResult & EnScriptExecutionResult.Timeout) == EnScriptExecutionResult.Timeout)
			{
				SetKnownState(EnQeStatusBarKnownStates.ExecutionTimedOut);
			}
			else if ((args.ExecutionResult & EnScriptExecutionResult.Success) == EnScriptExecutionResult.Success)
			{
				SetKnownState(EnQeStatusBarKnownStates.ExecutionOk);
			}

			if (PersistentSettings.EditorStatusBarExecutionTimeMethod == EnExecutionTimeMethod.Elapsed)
			{
				if (_ElapsedExecutionTimer != null)
				{
					_ElapsedExecutionTimer.Enabled = false;
				}

				((ToolStripItem)(object)_ExecutionTimePanel).Text = (DateTime.Now - ExecutionStartTime).FmtStatus();
			}
		}

		EnqueAndExecuteStatusBarUpdate(updateAction);
	}



	private bool OnQueryExecutionStarted(object sender, QueryExecutionStartedEventArgs args)
	{
		void updateAction()
		{
			_StatusStrip.Show();
			if (PersistentSettings.EditorStatusBarExecutionTimeMethod == EnExecutionTimeMethod.Elapsed)
			{
				((ToolStripItem)(object)_ExecutionTimePanel).Text = (DateTime.Now - DateTime.Now).FmtStatus();
				ExecutionStartTime = DateTime.Now;
				if (_ElapsedExecutionTimer == null)
				{
					_ElapsedExecutionTimer = new System.Timers.Timer(1000.0);
					_ElapsedExecutionTimer.Elapsed += OnExecutionTimerElapsed;
				}

				_ElapsedExecutionTimer.Enabled = true;
			}
		}

		EnqueAndExecuteStatusBarUpdate(updateAction);

		return true;
	}



	private void OnStatusChanged(object sender, QueryStatusChangedEventArgs args)
	{
		bool isConnected = QryMgr.IsConnected;
		bool isExecuting = QryMgr.IsExecuting;
		bool isConnecting = QryMgr.IsConnecting;
		bool isCancelling = QryMgr.IsCancelling;


		void updateAction()
		{
			if (args.StatusFlag == EnQueryStatusFlags.Connected || args.StatusFlag == EnQueryStatusFlags.Connection)
			{
				// Tracer.Trace(GetType(), "OnStatusChanged()", "Change: {0}, IsConnected: {1}.", args.StatusFlag, isConnected);

				if (isConnected)
					TransitionIntoOnlineMode(args.NewConnection);
				else
					TransitionIntoOfflineMode();
			}

			if (args.StatusFlag == EnQueryStatusFlags.Executing && isExecuting)
			{
				SetKnownState(EnQeStatusBarKnownStates.Executing);
			}

			if (args.StatusFlag == EnQueryStatusFlags.Connecting)
			{
				if (isConnecting)
					SetKnownState(EnQeStatusBarKnownStates.Connecting);
				else if (isConnected)
					TransitionIntoOnlineMode(args.NewConnection);
				else
					TransitionIntoOfflineMode();
			}

			if (args.StatusFlag == EnQueryStatusFlags.Cancelling && isCancelling)
			{
				SetKnownState(EnQeStatusBarKnownStates.CancellingExecution);
			}

			if (args.StatusFlag == EnQueryStatusFlags.DatabaseChanged)
			{
				ResetDatasetId();
			}
		}


		EnqueAndExecuteStatusBarUpdate(updateAction);
	}



	private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		UpdateStatusBar();
	}


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - StatusBarManager
	// =========================================================================================================


	public enum EnQeStatusBarKnownStates
	{
		Unknown,
		Offline,
		Connecting,
		Connected,
		NewConnectionOpened,
		Executing,
		Parsing,
		ExecutionOk,
		ExecutionFailed,
		ExecutionCancelled,
		ExecutionTimedOut,
		CancellingExecution,
		Disconnected
	}


	#endregion Sub-Classes
}
