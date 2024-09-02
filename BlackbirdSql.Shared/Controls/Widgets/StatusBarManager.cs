// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.StatusBar.QEStatusBarManager

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Media;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Config;
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
				qryMgr.ExecutionStartedEventAsync -= OnExecutionStartedAsync;
				qryMgr.ExecutionCompletedEventAsync -= OnExecutionCompletedAsync;
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


	public void Initialize(StatusStrip statusStrip, bool rowCountValid, IBsTabbedEditorPane editorWindowPane)
	{
		EditorWindowPane = editorWindowPane;
		QryMgr.StatusChangedEvent += OnStatusChanged;
		QryMgr.ExecutionStartedEventAsync += OnExecutionStartedAsync;
		QryMgr.ExecutionCompletedEventAsync += OnExecutionCompletedAsync;

		_StatusStrip = statusStrip;
		_StatusStrip.LayoutStyle = ToolStripLayoutStyle.Table;
		_RowCountValid = rowCountValid;

		_GeneralPanel.SetParent(_StatusStrip);

		((ToolStripStatusLabel)(object)_GeneralPanel).Spring = true;

		_StatusStrip.RenderMode = ToolStripRenderMode.Professional;
		_StatusStrip.Renderer = new EditorStatusStripRenderer(_StatusStrip);

		// Invert.
		bool isOnline = QryMgr.IsOnline;
		_ModeState = isOnline ? EnState.Offline : EnState.Disconnected;

		QueryStateChangedEventArgs args = new(QryMgr.StateFlags, EnQueryState.Online, isOnline,
			EnQueryState.None, EnQueryState.None);

		OnStatusChanged(this, args);

		if (QryMgr.IsConnected)
		{
			args = new(QryMgr.StateFlags, EnQueryState.Connected, true, EnQueryState.Online, EnQueryState.None);

			OnStatusChanged(this, args);
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - StatusBarManager
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private bool _Disposed = false;
	private bool _RowCountValid;
	private EnState _CurrentState = EnState.Unknown;
	private EnState _ModeState = EnState.Offline;
	private readonly Queue<Action> _UpdateActionsQueue = new Queue<Action>();
	private System.Timers.Timer _ElapsedExecutionTimer;

	private StatusStrip _StatusStrip;
	private AnimatedStatusStripItem _GeneralPanel = new AnimatedStatusStripItem(100);
	private StatusBarStatusLabel _ServerNamePanel = new(30, 150);
	private StatusBarStatusLabel _UserNamePanel = new(25, 150);
	private StatusBarStatusLabel _DatabaseNamePanel = new(25, 150);
	private StatusBarStatusLabel _ExecutionTimePanel = new(15, 70);
	private StatusBarStatusLabel _CompletedTimePanel = new(25, 150);
	private StatusBarStatusLabel _NumOfRowsPanel = new(15, 100);

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


	public EnState CurrentState => _CurrentState;

	public StatusStrip StatusStrip => _StatusStrip;


	public AnimatedStatusStripItem GeneralPanel => _GeneralPanel;

	public StatusBarStatusLabel ServerNamePanel => _ServerNamePanel;

	public StatusBarStatusLabel UserNamePanel => _UserNamePanel;

	public StatusBarStatusLabel DatabaseNamePanel => _DatabaseNamePanel;

	public StatusBarStatusLabel ExecutionTimePanel => _ExecutionTimePanel;

	public StatusBarStatusLabel CompletedTimePanel => _CompletedTimePanel;

	public StatusBarStatusLabel NumOfRowsPanel => _NumOfRowsPanel;

	private DateTime ExecutionStartTime { get; set; }

	private IBsTabbedEditorPane EditorWindowPane { get; set; }

	private QueryManager QryMgr => EditorWindowPane.AuxDocData?.QryMgr;


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



	private void EnqueUpdateAction(Action action)
	{
		lock (_LockLocal)
		{
			_UpdateActionsQueue.Enqueue(action);
		}

		if (ThreadHelper.CheckAccess())
		{
			ProcessUpdateActions();
			return;
		}

		Action method = ProcessUpdateActions;

		_StatusStrip.BeginInvoke(method);
	}



	private void ProcessUpdateActions()
	{
		if (_Disposed)
			return;


		while (true)
		{
			Action action = null;

			lock (_LockLocal)
			{
				if (_UpdateActionsQueue.Count == 0)
					return;

				action = _UpdateActionsQueue.Dequeue();
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
		string datasetId = QryMgr.Strategy.LiveAdornedTitle;
		((ToolStripItem)(object)_DatabaseNamePanel).Text = datasetId ?? "";
	}



	private void ResetPanelsForOnlineMode()
	{
		AbstractConnectionStrategy strategy = QryMgr.Strategy;
		string displayUserName = strategy.DisplayUserName;
		string datasetId = strategy.LiveAdornedTitle;
		string text = strategy.DisplayServerName;
		Version serverVersion = strategy.ServerVersion;

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
		((ToolStripItem)(object)_DatabaseNamePanel).Text = datasetId ?? "";
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



	private string SetBarState(EnState newState)
	{
		// Tracer.Trace(GetType(), "SetBarState()", "StatusBarCurrentState: {0}, StatusBarNewState: {1}", CurrentState, newState);

		ToolStripItem toolStripItem = (ToolStripItem)(object)_GeneralPanel;

		if (CurrentState == newState)
		{
			UpdateStatusBar();

			return toolStripItem.Text;
		}


		if (newState > EnState.ModeMarker)
		{
			_ModeState = newState == EnState.NewConnectionOpened
				? EnState.Connected : newState;
		}

		_CurrentState = newState;
		_GeneralPanel.BeginInit();


		switch (newState)
		{
			case EnState.Connecting:
				_GeneralPanel.SetOneImage(S_OfflineBitmap);
				toolStripItem.Text = ControlsResources.StatusBar_Connecting;
				break;
			case EnState.Connected:
				_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
				toolStripItem.Text = ControlsResources.StatusBar_Connected;
				break;
			case EnState.Disconnected:
				_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
				toolStripItem.Text = ControlsResources.StatusBar_Disconnected;
				break;
			case EnState.NewConnectionOpened:
				_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
				toolStripItem.Text = ControlsResources.StatusBar_NewConnectionOpened;
				break;
			case EnState.Executing:
				_GeneralPanel.SetImages(S_ExecutingBitmaps, false);
				toolStripItem.Text = ControlsResources.StatusBar_ExecutingQuery;
				_GeneralPanel.StartAnimate();
				break;
			case EnState.Cancelling:
				_GeneralPanel.SetImages(S_ExecutingCancelBitmaps, false);
				toolStripItem.Text = ControlsResources.StatusBar_Cancelling;
				_GeneralPanel.StartAnimate();
				break;
			case EnState.CancellingPrompt:
				_GeneralPanel.SetImages(S_ExecutingCancelBitmaps, false);
				toolStripItem.Text = ControlsResources.StatusBar_CancellingAndPrompt;
				_GeneralPanel.StartAnimate();
				break;
			case EnState.ConnectFailedPrompt:
				_GeneralPanel.SetImages(S_ExecutingCancelBitmaps, false);
				toolStripItem.Text = ControlsResources.StatusBar_ConnectFailedAndPrompt;
				_GeneralPanel.StartAnimate();
				break;
			case EnState.ExecutionFailedPrompt:
				_GeneralPanel.SetImages(S_ExecutingCancelBitmaps, false);
				toolStripItem.Text = ControlsResources.StatusBar_ExecutionFailedAndPrompt;
				_GeneralPanel.StartAnimate();
				break;
			case EnState.ExecutionCancelled:
				_GeneralPanel.SetOneImage(S_ExecCancelledBitmap);
				toolStripItem.Text = QryMgr.HadTransactions && !QryMgr.PeekTransactions
					? ControlsResources.StatusBar_QueryCancelledTtsLost
					: (QryMgr.PeekTransactions
						? ControlsResources.StatusBar_QueryCancelledRollback
						: ControlsResources.StatusBar_QueryCancelled);
				ResetDatasetId();
				break;
			case EnState.ExecutionFailed:
				_GeneralPanel.SetOneImage(S_ExecWithErrorBitmap);
				toolStripItem.Text = QryMgr.HadTransactions && !QryMgr.PeekTransactions
					? ControlsResources.StatusBar_QueryCompletedWithErrorsTtsLost
					: (QryMgr.PeekTransactions
						? ControlsResources.StatusBar_QueryCompletedWithErrorsRollback
						: ControlsResources.StatusBar_QueryCompletedWithErrors);
				ResetDatasetId();
				break;
			case EnState.ExecutionOk:
				_GeneralPanel.SetOneImage(S_ExecSuccessBitmap);
				toolStripItem.Text =
					ControlsResources.StatusBar_QueryCompletedSuccessfully.FmtRes(!QryMgr.QueryExecutionEndTime.HasValue ? "" : QryMgr.QueryExecutionEndTime);
				ResetDatasetId();
				break;
			case EnState.ExecutionTimedOut:
				_GeneralPanel.SetOneImage(S_ExecWithErrorBitmap);
				toolStripItem.Text = ControlsResources.StatusBar_QueryTimedOut;
				ResetDatasetId();
				break;
			case EnState.Offline:
				_GeneralPanel.SetOneImage(S_OfflineBitmap);
				toolStripItem.Text = ControlsResources.StatusBar_Offline;
				break;
			case EnState.Parsing:
				_GeneralPanel.SetImages(S_ExecutingBitmaps, false);
				toolStripItem.Text = ControlsResources.StatusBar_ParsingQueryBatch;
				_GeneralPanel.StartAnimate();
				break;
			case EnState.Unknown:
				_GeneralPanel.SetOneImage(null);
				toolStripItem.Text = "";
				break;
		}

		if (newState == EnState.Executing || newState == EnState.Offline)
		{
			DateTime now = DateTime.Now;
			SetExecutionCompletedTime(now);
			SetRowsAffected(0L);
		}

		if (newState == EnState.ExecutionTimedOut || newState == EnState.ExecutionOk
			|| newState == EnState.ExecutionFailed || newState == EnState.ExecutionCancelled)
		{
			if (QryMgr.QueryExecutionStartTime.HasValue && QryMgr.QueryExecutionEndTime.HasValue)
			{
				_ = QryMgr.QueryExecutionStartTime.Value;
				DateTime value = QryMgr.QueryExecutionEndTime.Value;
				SetExecutionCompletedTime(value);
			}

			SetRowsAffected(QryMgr.RowsAffected);

			AuxilliaryDocData auxDocData = EditorWindowPane.AuxDocData;

			if (auxDocData != null && auxDocData.LiveSettings.EditorResultsPlaySounds)
			{
				new SoundPlayer().Play();
			}
		}

		toolStripItem.ToolTipText = toolStripItem.Text;
		_GeneralPanel.EndInit();

		UpdateStatusBar();

		return toolStripItem.Text;
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

		_ModeState = EnState.Offline;

		if (CurrentState != EnState.Offline)
		{
			_StatusStrip.Items.Clear();
			_StatusStrip.Items.Add((ToolStripItem)(object)_GeneralPanel);
		}
	}



	private void TransitionIntoOnlineMode()
	{
		if (_ModeState == EnState.Offline)
			_ModeState = EnState.Disconnected;

		ResetPanelsForOnlineMode();
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


	private void OnExecutionTimerElapsed(object sender, ElapsedEventArgs a)
	{
		void update()
		{
			((ToolStripItem)(object)_ExecutionTimePanel).Text = (a.SignalTime - ExecutionStartTime).FmtStatus();
		}

		EnqueUpdateAction(update);
	}



	private async Task<bool> OnExecutionCompletedAsync(object sender, ExecutionCompletedEventArgs a)
	{
		// Tracer.Trace(GetType(), "OnQueryExecutionCompletedAsync()", "a.ExecResult: {0}.", a.ExecutionResult);

		if (a.SyncToken.Cancelled())
			return true;

		// await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		void updateAction()
		{
			if ((a.ExecutionResult & EnScriptExecutionResult.Cancel) == EnScriptExecutionResult.Cancel)
			{
				SetBarState(EnState.ExecutionCancelled);
			}
			else if ((a.ExecutionResult & EnScriptExecutionResult.Failure) == EnScriptExecutionResult.Failure)
			{
				SetBarState(EnState.ExecutionFailed);
			}
			else if ((a.ExecutionResult & EnScriptExecutionResult.Timeout) == EnScriptExecutionResult.Timeout)
			{
				SetBarState(EnState.ExecutionTimedOut);
			}
			else if ((a.ExecutionResult & EnScriptExecutionResult.Success) == EnScriptExecutionResult.Success)
			{
				SetBarState(EnState.ExecutionOk);
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

		EnqueUpdateAction(updateAction);

		return await Task.FromResult(true);
	}



	private async Task<bool> OnExecutionStartedAsync(object sender, ExecutionStartedEventArgs a)
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

		EnqueUpdateAction(updateAction);

		return await Task.FromResult(true);
	}



	private void OnStatusChanged(object sender, QueryStateChangedEventArgs args)
	{
		EnqueUpdateAction(updateAction);

		void updateAction()
		{
			QueryStateChangedEventArgs a = args.Clone();

			bool isOnline = a.Online;

			if (a.IsStateOnline)
				isOnline = a.Value;

			if (isOnline)
			{
				if (_ModeState == EnState.Offline || (a.IsStateConnected && a.Value && a.DatabaseChanged))
					TransitionIntoOnlineMode();
			}
			else if (!isOnline && _ModeState != EnState.Offline)
			{
				TransitionIntoOfflineMode();
			}

			while (true)
			{
				EnState result = EnState.Unknown;


				// --------------------------------------------
				// Enable State.
				//---------------------------------------------

				if (a.Value)
				{
					if (a.IsStateConnecting)
					{
						result = EnState.Connecting;
						SetBarState(result);
						return;
					}

					if (a.IsStateExecuting)
					{
						result = EnState.Executing;
						SetBarState(result);
						return;
					}


					if (a.IsStatePrompting)
					{
						if (a.PrevState == EnQueryState.Connecting)
							result = EnState.ConnectFailedPrompt;
						else if (a.PrevState == EnQueryState.Cancelling)
							result = EnState.CancellingPrompt;
						else // if (a.PrevState == EnQueryState.Executing)
							result = EnState.ExecutionFailedPrompt;

						SetBarState(result);
						return;
					}


					if (a.IsStateConnected)
					{
						if (a.Executing)
							result = EnState.Executing;
						else if (a.DatabaseChanged)
							result = EnState.NewConnectionOpened;
						else
							result = EnState.Connected;

						SetBarState(result);
						ResetDatasetId();

						return;
					}

					if (a.IsStateOnline)
					{
						result = EnState.Disconnected;

						SetBarState(result);
						ResetDatasetId();

						return;
					}

					return;
				}



				// --------------------------------------------
				// Disable values follow.
				// --------------------------------------------

				if (a.IsStateExecuting || a.IsStateCancelling
					|| (a.IsStateConnected && a.Executing))
				{
					return;
				}


				if (a.IsStateOnline)
				{
					result = EnState.Offline;
					SetBarState(result);
					ResetDatasetId();

					return;
				}

				if (!a.CanProcreate)
				{
					SetBarState(_ModeState);
					return;
				}


				a = a.PopClone();

			}
		}

	}



	private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		UpdateStatusBar();
	}


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - StatusBarManager
	// =========================================================================================================


	public enum EnState
	{
		Unknown,
		Connecting,
		Executing,
		Parsing,
		Cancelling,
		CancellingPrompt,
		ConnectFailedPrompt,
		ExecutionFailedPrompt,
		ExecutionOk,
		ExecutionFailed,
		ExecutionCancelled,
		ExecutionTimedOut,
		ModeMarker,
		Offline,
		Connected,
		NewConnectionOpened,
		Disconnected
	}


	#endregion Sub-Classes
}
