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
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Model.Enums;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;



namespace BlackbirdSql.Common.Controls.Widgets;


public sealed class QEStatusBarManager : IDisposable
{
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
		CancelingExecution,
		ConnectionReady
	}

	// private const uint LowTrace = 4u;

	// private const uint NormalTrace = 2u;

	// private const uint HeighTrace = 1u;

	// private static readonly Color DefaultColor;

	private StatusStrip _StatusStrip;

	private AnimatedStatusStripItem _GeneralPanel = new AnimatedStatusStripItem(150);

	private ToolStripStatusLabelWithMaxLimit _ServerNamePanel = new ToolStripStatusLabelWithMaxLimit(30, 150);

	private ToolStripStatusLabelWithMaxLimit _UserNamePanel = new ToolStripStatusLabelWithMaxLimit(25, 150);

	private ToolStripStatusLabelWithMaxLimit _DatabaseNamePanel = new ToolStripStatusLabelWithMaxLimit(25, 150);

	private ToolStripStatusLabelWithMaxLimit _ExecutionTimePanel = new ToolStripStatusLabelWithMaxLimit(15, 70);

	private ToolStripStatusLabelWithMaxLimit _CompletedTimePanel = new ToolStripStatusLabelWithMaxLimit(25, 150);

	private ToolStripStatusLabelWithMaxLimit _NumOfRowsPanel = new ToolStripStatusLabelWithMaxLimit(15, 100);

	// private const int C_NumOfPanelsInOfflineMode = 1;

	private bool disposed;

	private static readonly Image S_NumOfRowsPanel;

	private static readonly Image S_OfflineBitmap;

	private static readonly Image S_ExecSuccessBitmap;

	private static readonly Image S_ExecWithErrorBitmap;

	private static readonly Image S_ExecCanceledBitmap;

	private static readonly Image[] S_ExecutingBitmaps;

	private static readonly Image[] S_ExecutingCancelBitmaps;

	private bool _RowCountValid;

	// private string userName = string.Empty;

	private readonly Queue<Action> _StatusBarUpdateActions = new Queue<Action>();

	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private System.Timers.Timer _ElapsedExecutionTimer;

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
			AuxilliaryDocData auxDocData = ((IBEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(EditorWindowPane.DocData);
			if (auxDocData != null)
			{
				result = auxDocData.QryMgr;
			}

			return result;
		}
	}

	static QEStatusBarManager()
	{
		// DefaultColor = SystemColors.Control;
		S_NumOfRowsPanel = new Bitmap(ControlsResources.StatusBar_connect);
		S_OfflineBitmap = new Bitmap(ControlsResources.StatusBar_offline);
		S_ExecSuccessBitmap = new Bitmap(ControlsResources.StatusBar_success);
		S_ExecWithErrorBitmap = new Bitmap(ControlsResources.StatusBar_error);
		S_ExecCanceledBitmap = new Bitmap(ControlsResources.StatusBar_cancel);
		S_ExecutingBitmaps =
		[
			new Bitmap(ControlsResources.StatusBar_spin1),
			new Bitmap(ControlsResources.StatusBar_spin2),
			new Bitmap(ControlsResources.StatusBar_spin3),
			new Bitmap(ControlsResources.StatusBar_spin4),
			new Bitmap(ControlsResources.StatusBar_spin5),
			new Bitmap(ControlsResources.StatusBar_spin6),
			new Bitmap(ControlsResources.StatusBar_spin7),
			new Bitmap(ControlsResources.StatusBar_spin8),
			new Bitmap(ControlsResources.StatusBar_spin9),
			new Bitmap(ControlsResources.StatusBar_spin10),
			new Bitmap(ControlsResources.StatusBar_spin11),
			new Bitmap(ControlsResources.StatusBar_spin12),
			new Bitmap(ControlsResources.StatusBar_spin13),
			new Bitmap(ControlsResources.StatusBar_spin14)
		];
		S_ExecutingCancelBitmaps =
		[
			S_ExecutingBitmaps[13],
			S_ExecutingBitmaps[12],
			S_ExecutingBitmaps[11],
			S_ExecutingBitmaps[10],
			S_ExecutingBitmaps[9],
			S_ExecutingBitmaps[8],
			S_ExecutingBitmaps[7],
			S_ExecutingBitmaps[6],
			S_ExecutingBitmaps[5],
			S_ExecutingBitmaps[4],
			S_ExecutingBitmaps[3],
			S_ExecutingBitmaps[2],
			S_ExecutingBitmaps[1],
			S_ExecutingBitmaps[0]
		];
	}

	public QEStatusBarManager()
	{
		CurrentState = EnQeStatusBarKnownStates.Unknown;
		SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
	}

	private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		UpdateStatusBar();
	}

	public void Initialize(StatusStrip statusStrip, bool rowCountValid, IBSqlEditorWindowPane editorWindowPane)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		EditorWindowPane = editorWindowPane;
		QryMgr.StatusChangedEvent += HandleQueryManagerStatusChanged;
		QryMgr.ScriptExecutionStartedEvent += HandleScriptExecutionStartedEventHandler;
		QryMgr.ScriptExecutionCompletedEvent += HandleScriptExecutionCompletedEventHandler;
		// Tracer.Trace(GetType(), "QEStatusBarManager.Initialize", "_RowCountValid = {0}", rowCountValid);
		_StatusStrip = statusStrip;
		_StatusStrip.LayoutStyle = ToolStripLayoutStyle.Table;
		_RowCountValid = rowCountValid;
		_GeneralPanel.SetParent(_StatusStrip);
		((ToolStripStatusLabel)(object)_GeneralPanel).Spring = true;
		_StatusStrip.RenderMode = ToolStripRenderMode.Professional;
		_StatusStrip.Renderer = new EditorStatusStripRenderer(_StatusStrip);
		_ = QryMgr.ConnectionStrategy.ConnectionInfo;
		IDbConnection connection = QryMgr.ConnectionStrategy.Connection;
		if (connection != null && connection.State == ConnectionState.Open)
		{
			TransitionIntoOnlineMode(useNewConnectionOpenedState: false);
		}
		else
		{
			TransitionIntoOfflineMode();
		}
	}

	private void HandleQueryManagerStatusChanged(object sender, QueryManager.StatusChangedEventArgs args)
	{
		bool isConnected = QryMgr.IsConnected;
		bool isExecuting = QryMgr.IsExecuting;
		bool isConnecting = QryMgr.IsConnecting;
		bool isCancelling = QryMgr.IsCancelling;
		void a()
		{
			if (args.Change == QueryManager.EnStatusType.Connected || args.Change == QueryManager.EnStatusType.Connection)
			{
				if (isConnected)
				{
					TransitionIntoOnlineMode(useNewConnectionOpenedState: true);
				}
				else if (!isConnected)
				{
					TransitionIntoOfflineMode();
				}
			}

			if (args.Change == QueryManager.EnStatusType.Executing && isExecuting)
			{
				SetKnownState(EnQeStatusBarKnownStates.Executing);
			}

			if (args.Change == QueryManager.EnStatusType.Connecting)
			{
				if (isConnecting)
				{
					SetKnownState(EnQeStatusBarKnownStates.Connecting);
				}
				else if (isConnected)
				{
					TransitionIntoOnlineMode(useNewConnectionOpenedState: true);
				}
				else
				{
					TransitionIntoOfflineMode();
				}
			}

			if (args.Change == QueryManager.EnStatusType.Cancelling && isCancelling)
			{
				SetKnownState(EnQeStatusBarKnownStates.CancelingExecution);
			}

			if (args.Change == QueryManager.EnStatusType.DatabaseChanged)
			{
				ResetDatasetId();
			}
		}
		EnqueAndExecuteStatusBarUpdate(a);
	}

	private void EnqueAndExecuteStatusBarUpdate(Action act)
	{
		lock (_LockLocal)
		{
			_StatusBarUpdateActions.Enqueue(act);
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
		if (disposed)
		{
			return;
		}

		while (true)
		{
			Action action = null;
			lock (_LockLocal)
			{
				if (_StatusBarUpdateActions.Count == 0)
				{
					return;
				}

				action = _StatusBarUpdateActions.Dequeue();
			}

			action?.Invoke();
		}
	}

	private bool HandleScriptExecutionStartedEventHandler(object sender, QueryManager.ScriptExecutionStartedEventArgs args)
	{
		void act()
		{
			_StatusStrip.Show();
			if (PersistentSettings.EditorStatusBarExecutionTimeMethod == EnExecutionTimeMethod.Elapsed)
			{
				((ToolStripItem)(object)_ExecutionTimePanel).Text = (DateTime.Now - DateTime.Now).FmtSqlStatus();
				ExecutionStartTime = DateTime.Now;
				if (_ElapsedExecutionTimer == null)
				{
					_ElapsedExecutionTimer = new System.Timers.Timer(1000.0);
					_ElapsedExecutionTimer.Elapsed += OnExecutionTimerElapsed;
				}

				_ElapsedExecutionTimer.Enabled = true;
			}
		}
		EnqueAndExecuteStatusBarUpdate(act);
		return true;
	}

	private void HandleScriptExecutionCompletedEventHandler(object sender, ScriptExecutionCompletedEventArgs args)
	{
		void a()
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

				((ToolStripItem)(object)_ExecutionTimePanel).Text = (DateTime.Now - ExecutionStartTime).FmtSqlStatus();
			}
		}
		EnqueAndExecuteStatusBarUpdate(a);
	}

	private void OnExecutionTimerElapsed(object sender, ElapsedEventArgs args)
	{
		void a()
		{
			((ToolStripItem)(object)_ExecutionTimePanel).Text = (args.SignalTime - ExecutionStartTime).FmtSqlStatus();
		}
		EnqueAndExecuteStatusBarUpdate(a);
	}

	private void TransitionIntoOfflineMode()
	{
		// Tracer.Trace(GetType(), "QEStatusBarManager.TransitionIntoOfflineMode", "", null);
		AbstractConnectionStrategy connectionStrategy = QryMgr.ConnectionStrategy;
		if (connectionStrategy != null && connectionStrategy.ConnectionInfo != null)
		{
			ResetPanelsForOnlineMode();
			SetKnownState(EnQeStatusBarKnownStates.ConnectionReady);
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
		AbstractConnectionStrategy connectionStrategy = QryMgr.ConnectionStrategy;
		if (connectionStrategy.ConnectionInfo != null)
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

			// userName = connectionStrategy.DisplayUserName;
		}
	}

	private string SetKnownState(EnQeStatusBarKnownStates newState)
	{
		// Tracer.Trace(GetType(), "QEStatusBarManager.SetKnownState", "newState = {0}", newState);
		if (CurrentState != newState)
		{
			CurrentState = newState;
			_GeneralPanel.BeginInit();
			switch (newState)
			{
				case EnQeStatusBarKnownStates.Connecting:
					_GeneralPanel.SetOneImage(S_OfflineBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarConnecting;
					break;
				case EnQeStatusBarKnownStates.Connected:
					_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarConnected;
					break;
				case EnQeStatusBarKnownStates.ConnectionReady:
					_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarConnectionReady;
					break;
				case EnQeStatusBarKnownStates.NewConnectionOpened:
					_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarNewConnectionOpened;
					break;
				case EnQeStatusBarKnownStates.Executing:
					_GeneralPanel.SetImages(S_ExecutingBitmaps);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarExecutingQuery;
					_GeneralPanel.StartAnimate();
					break;
				case EnQeStatusBarKnownStates.CancelingExecution:
					_GeneralPanel.SetImages(S_ExecutingCancelBitmaps);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarCancellingQuery;
					_GeneralPanel.StartAnimate();
					break;
				case EnQeStatusBarKnownStates.ExecutionCancelled:
					_GeneralPanel.SetOneImage(S_ExecCanceledBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarQueryCanceled;
					ResetDatasetId();
					break;
				case EnQeStatusBarKnownStates.ExecutionFailed:
					_GeneralPanel.SetOneImage(S_ExecWithErrorBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarQueryCompletedWithErrors;
					ResetDatasetId();
					break;
				case EnQeStatusBarKnownStates.ExecutionOk:
					_GeneralPanel.SetOneImage(S_ExecSuccessBitmap);
					if (!QryMgr.QueryExecutionEndTime.HasValue)
					{
						((ToolStripItem)(object)_GeneralPanel).Text = string.Format(CultureInfo.CurrentCulture, ControlsResources.StatusBarQueryCompletedSuccessfully, string.Empty);
					}
					else
					{
						((ToolStripItem)(object)_GeneralPanel).Text = string.Format(CultureInfo.CurrentCulture, ControlsResources.StatusBarQueryCompletedSuccessfully, QryMgr.QueryExecutionEndTime);
					}

					ResetDatasetId();
					break;
				case EnQeStatusBarKnownStates.ExecutionTimedOut:
					_GeneralPanel.SetOneImage(S_ExecWithErrorBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarQueryTimedOut;
					ResetDatasetId();
					break;
				case EnQeStatusBarKnownStates.Offline:
					_GeneralPanel.SetOneImage(S_OfflineBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarDisconnected;
					break;
				case EnQeStatusBarKnownStates.Parsing:
					_GeneralPanel.SetImages(S_ExecutingBitmaps);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarParsingQueryBatch;
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

			if (newState == EnQeStatusBarKnownStates.ExecutionTimedOut || newState == EnQeStatusBarKnownStates.ExecutionOk || newState == EnQeStatusBarKnownStates.ExecutionFailed || newState == EnQeStatusBarKnownStates.ExecutionCancelled)
			{
				if (QryMgr.QueryExecutionStartTime.HasValue && QryMgr.QueryExecutionEndTime.HasValue)
				{
					_ = QryMgr.QueryExecutionStartTime.Value;
					DateTime value = QryMgr.QueryExecutionEndTime.Value;
					SetExecutionCompletedTime(value);
				}

				SetRowsAffected(QryMgr.RowsAffected);
				AuxilliaryDocData auxDocData = ((IBEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(EditorWindowPane.DocData);
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

	private void SetExecutionCompletedTime(DateTime endTime)
	{
		((ToolStripItem)(object)_CompletedTimePanel).Text = endTime.ToString(CultureInfo.CurrentCulture);
	}

	/*
	private static string FormatTimeSpanForStatus(TimeSpan ts)
	{
		return new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds, 0).ToString();
	}
	*/

	private void SetRowsAffected(long rows)
	{
		string text = string.Format(CultureInfo.CurrentCulture, ControlsResources.StatusBarNumberOfReturnedRows, rows);
		if (text != ((ToolStripItem)(object)_NumOfRowsPanel).Text)
		{
			((ToolStripItem)(object)_NumOfRowsPanel).Text = text;
		}
	}

	private void UpdateStatusBar()
	{
		_StatusStrip.BackColor = QryMgr.ConnectionStrategy.StatusBarColor;
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

	private void ResetPanelsForOnlineMode()
	{
		AbstractConnectionStrategy connectionStrategy = QryMgr.ConnectionStrategy;
		string displayUserName = connectionStrategy.DisplayUserName;
		string datasetId = connectionStrategy.DatasetId;
		string text = connectionStrategy.DisplayServerName;
		string productLevel = connectionStrategy.GetProductLevel();
		Version serverVersion = connectionStrategy.GetServerVersion();
		if (!string.IsNullOrEmpty(productLevel))
		{
			text = string.Format(CultureInfo.CurrentCulture, ControlsResources.ServerNameAndBuildAndProduct, text, serverVersion.Major, serverVersion.Minor, productLevel);
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
			((ToolStripItem)(object)_ExecutionTimePanel).Text = (QryMgr.QueryExecutionEndTime.Value - QryMgr.QueryExecutionStartTime.Value).FmtSqlStatus();
			((ToolStripItem)(object)_CompletedTimePanel).Text = QryMgr.QueryExecutionEndTime.Value.ToString(CultureInfo.CurrentCulture);
		}

		SetRowsAffected(QryMgr.RowsAffected);
	}

	private void ResetDatasetId()
	{
		string datasetId = QryMgr.ConnectionStrategy.DatasetId;
		((ToolStripItem)(object)_DatabaseNamePanel).Text = datasetId ?? string.Empty;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && !disposed)
		{
			QueryManager qryMgr = QryMgr;
			if (qryMgr != null)
			{
				qryMgr.ScriptExecutionStartedEvent -= HandleScriptExecutionStartedEventHandler;
				qryMgr.ScriptExecutionCompletedEvent -= HandleScriptExecutionCompletedEventHandler;
				qryMgr.StatusChangedEvent -= HandleQueryManagerStatusChanged;
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

			SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
		}

		disposed = true;
	}
}
