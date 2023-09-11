#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Media;
using System.Timers;
using System.Windows.Forms;

using BlackbirdSql.Common.Config;
using BlackbirdSql.Common.Controls.ResultsPane;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Model;

using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;


using Tracer = BlackbirdSql.Core.Diagnostics.Tracer;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Controls;


public sealed class QEStatusBarManager : IDisposable
{
	public enum QEStatusBarKnownStates
	{
		Unknown,
		Offline,
		Connecting,
		Connected,
		NewConnectionOpened,
		Executing,
		Debugging,
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

	private readonly object _LockObject = new object();

	private System.Timers.Timer _ElapsedExecutionTimer;

	public QEStatusBarKnownStates CurrentState { get; private set; }

	public StatusStrip StatusStrip => _StatusStrip;

	public AnimatedStatusStripItem GeneralPanel => _GeneralPanel;

	public ToolStripStatusLabelWithMaxLimit ServerNamePanel => _ServerNamePanel;

	public ToolStripStatusLabelWithMaxLimit UserNamePanel => _UserNamePanel;

	public ToolStripStatusLabelWithMaxLimit DatabaseNamePanel => _DatabaseNamePanel;

	public ToolStripStatusLabelWithMaxLimit ExecutionTimePanel => _ExecutionTimePanel;

	public ToolStripStatusLabelWithMaxLimit CompletedTimePanel => _CompletedTimePanel;

	public ToolStripStatusLabelWithMaxLimit NumOfRowsPanel => _NumOfRowsPanel;

	private DateTime ExecutionStartTime { get; set; }

	private ISqlEditorWindowPane EditorWindowPane { get; set; }

	private QueryManager QryMgr
	{
		get
		{
			QueryManager result = null;
			AuxiliaryDocData auxDocData = ((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(EditorWindowPane.DocData);
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
		S_ExecutingBitmaps = new Image[14]
		{
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
		};
		S_ExecutingCancelBitmaps = new Image[14]
		{
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
		};
	}

	public QEStatusBarManager()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		CurrentState = QEStatusBarKnownStates.Unknown;
		SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
	}

	private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		UpdateStatusBar();
	}

	public void Initialize(StatusStrip statusStrip, bool rowCountValid, ISqlEditorWindowPane editorWindowPane)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		EditorWindowPane = editorWindowPane;
		QryMgr.StatusChangedEvent += HandleQueryManagerStatusChanged;
		QryMgr.ScriptExecutionStartedEvent += HandleScriptExecutionStartedEventHandler;
		QryMgr.ScriptExecutionCompletedEvent += HandleScriptExecutionCompletedEventHandler;
		Tracer.Trace(GetType(), "QEStatusBarManager.Initialize", "_RowCountValid = {0}", rowCountValid);
		this._StatusStrip = statusStrip;
		this._StatusStrip.LayoutStyle = ToolStripLayoutStyle.Table;
		this._RowCountValid = rowCountValid;
		_GeneralPanel.SetParent(this._StatusStrip);
		((ToolStripStatusLabel)(object)_GeneralPanel).Spring = true;
		this._StatusStrip.RenderMode = ToolStripRenderMode.Professional;
		this._StatusStrip.Renderer = new EditorStatusStripRenderer(this._StatusStrip);
		_ = QryMgr.ConnectionStrategy.UiConnectionInfo;
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
		bool isDebugging = QryMgr.IsDebugging;
		bool isExecuting = QryMgr.IsExecuting;
		bool isConnecting = QryMgr.IsConnecting;
		bool isCancelling = QryMgr.IsCancelling;
		void a()
		{
			if (args.Change == QueryManager.StatusType.Connected || args.Change == QueryManager.StatusType.Connection)
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

			if (args.Change == QueryManager.StatusType.Debugging)
			{
				if (isDebugging)
				{
					SetKnownState(QEStatusBarKnownStates.Debugging);
				}
				else
				{
					SetKnownState(QEStatusBarKnownStates.ExecutionOk);
				}
			}

			if (args.Change == QueryManager.StatusType.Executing && isExecuting)
			{
				SetKnownState(QEStatusBarKnownStates.Executing);
			}

			if (args.Change == QueryManager.StatusType.Connecting)
			{
				if (isConnecting)
				{
					SetKnownState(QEStatusBarKnownStates.Connecting);
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

			if (args.Change == QueryManager.StatusType.Cancelling && isCancelling)
			{
				SetKnownState(QEStatusBarKnownStates.CancelingExecution);
			}

			if (args.Change == QueryManager.StatusType.DatabaseChanged)
			{
				ResetDatabaseName();
			}
		}
		EnqueAndExecuteStatusBarUpdate(a);
	}

	private void EnqueAndExecuteStatusBarUpdate(Action a)
	{
		lock (_LockObject)
		{
			_StatusBarUpdateActions.Enqueue(a);
		}

		if (DisplaySQLResultsControl.IsRunningOnUIThread())
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
			lock (_LockObject)
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
		void a()
		{
			_StatusStrip.Show();
			if (UserSettings.Instance.Current.StatusBar.ShowTimeOption == EnDisplayTimeOptions.Elapsed)
			{
				((ToolStripItem)(object)_ExecutionTimePanel).Text = (DateTime.Now - DateTime.Now).FormatForStatus();
				ExecutionStartTime = DateTime.Now;
				if (_ElapsedExecutionTimer == null)
				{
					_ElapsedExecutionTimer = new System.Timers.Timer(1000.0);
					_ElapsedExecutionTimer.Elapsed += OnExecutionTimerElapsed;
				}

				_ElapsedExecutionTimer.Enabled = true;
			}
		}
		EnqueAndExecuteStatusBarUpdate(a);
		return true;
	}

	private void HandleScriptExecutionCompletedEventHandler(object sender, ScriptExecutionCompletedEventArgs args)
	{
		void a()
		{
			if ((args.ExecutionResult & EnScriptExecutionResult.Cancel) == EnScriptExecutionResult.Cancel)
			{
				SetKnownState(QEStatusBarKnownStates.ExecutionCancelled);
			}
			else if ((args.ExecutionResult & EnScriptExecutionResult.Failure) == EnScriptExecutionResult.Failure)
			{
				SetKnownState(QEStatusBarKnownStates.ExecutionFailed);
			}
			else if ((args.ExecutionResult & EnScriptExecutionResult.Timeout) == EnScriptExecutionResult.Timeout)
			{
				SetKnownState(QEStatusBarKnownStates.ExecutionTimedOut);
			}
			else if ((args.ExecutionResult & EnScriptExecutionResult.Success) == EnScriptExecutionResult.Success)
			{
				SetKnownState(QEStatusBarKnownStates.ExecutionOk);
			}

			if (UserSettings.Instance.Current.StatusBar.ShowTimeOption == EnDisplayTimeOptions.Elapsed)
			{
				if (_ElapsedExecutionTimer != null)
				{
					_ElapsedExecutionTimer.Enabled = false;
				}

				((ToolStripItem)(object)_ExecutionTimePanel).Text = (DateTime.Now - ExecutionStartTime).FormatForStatus();
			}
		}
		EnqueAndExecuteStatusBarUpdate(a);
	}

	private void OnExecutionTimerElapsed(object sender, ElapsedEventArgs args)
	{
		void a()
		{
			((ToolStripItem)(object)_ExecutionTimePanel).Text = (args.SignalTime - ExecutionStartTime).FormatForStatus();
		}
		EnqueAndExecuteStatusBarUpdate(a);
	}

	private void TransitionIntoOfflineMode()
	{
		Tracer.Trace(GetType(), "QEStatusBarManager.TransitionIntoOfflineMode", "", null);
		ConnectionStrategy connectionStrategy = QryMgr.ConnectionStrategy;
		if (connectionStrategy != null && connectionStrategy.UiConnectionInfo != (UIConnectionInfo)null)
		{
			ResetPanelsForOnlineMode();
			SetKnownState(QEStatusBarKnownStates.ConnectionReady);
		}
		else if (CurrentState != QEStatusBarKnownStates.Offline)
		{
			_StatusStrip.Items.Clear();
			_StatusStrip.Items.Add((ToolStripItem)(object)_GeneralPanel);
			SetKnownState(QEStatusBarKnownStates.Offline);
		}
	}

	private void TransitionIntoOnlineMode(bool useNewConnectionOpenedState)
	{
		ConnectionStrategy connectionStrategy = QryMgr.ConnectionStrategy;
		if (connectionStrategy.UiConnectionInfo != null)
		{
			ResetPanelsForOnlineMode();
			if (useNewConnectionOpenedState)
			{
				SetKnownState(QEStatusBarKnownStates.NewConnectionOpened);
			}
			else
			{
				SetKnownState(QEStatusBarKnownStates.Connected);
			}

			// userName = connectionStrategy.DisplayUserName;
		}
	}

	private string SetKnownState(QEStatusBarKnownStates newState)
	{
		Tracer.Trace(GetType(), "QEStatusBarManager.SetKnownState", "newState = {0}", newState);
		if (CurrentState != newState)
		{
			CurrentState = newState;
			_GeneralPanel.BeginInit();
			switch (newState)
			{
				case QEStatusBarKnownStates.Connecting:
					_GeneralPanel.SetOneImage(S_OfflineBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarConnecting;
					break;
				case QEStatusBarKnownStates.Connected:
					_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarConnected;
					break;
				case QEStatusBarKnownStates.ConnectionReady:
					_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarConnectionReady;
					break;
				case QEStatusBarKnownStates.NewConnectionOpened:
					_GeneralPanel.SetOneImage(S_NumOfRowsPanel);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarNewConnectionOpened;
					break;
				case QEStatusBarKnownStates.Executing:
					_GeneralPanel.SetImages(S_ExecutingBitmaps);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarExecutingQuery;
					_GeneralPanel.StartAnimate();
					break;
				case QEStatusBarKnownStates.Debugging:
					_GeneralPanel.SetOneImage(null);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarDebuggingQuery;
					break;
				case QEStatusBarKnownStates.CancelingExecution:
					_GeneralPanel.SetImages(S_ExecutingCancelBitmaps);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarCancellingQuery;
					_GeneralPanel.StartAnimate();
					break;
				case QEStatusBarKnownStates.ExecutionCancelled:
					_GeneralPanel.SetOneImage(S_ExecCanceledBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarQueryCanceled;
					ResetDatabaseName();
					break;
				case QEStatusBarKnownStates.ExecutionFailed:
					_GeneralPanel.SetOneImage(S_ExecWithErrorBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarQueryCompletedWithErrors;
					ResetDatabaseName();
					break;
				case QEStatusBarKnownStates.ExecutionOk:
					_GeneralPanel.SetOneImage(S_ExecSuccessBitmap);
					if (!QryMgr.QueryExecutionEndTime.HasValue)
					{
						((ToolStripItem)(object)_GeneralPanel).Text = string.Format(CultureInfo.CurrentCulture, ControlsResources.StatusBarQueryCompletedSuccessfully, string.Empty);
					}
					else
					{
						((ToolStripItem)(object)_GeneralPanel).Text = string.Format(CultureInfo.CurrentCulture, ControlsResources.StatusBarQueryCompletedSuccessfully, QryMgr.QueryExecutionEndTime);
					}

					ResetDatabaseName();
					break;
				case QEStatusBarKnownStates.ExecutionTimedOut:
					_GeneralPanel.SetOneImage(S_ExecWithErrorBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarQueryTimedOut;
					ResetDatabaseName();
					break;
				case QEStatusBarKnownStates.Offline:
					_GeneralPanel.SetOneImage(S_OfflineBitmap);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarDisconnected;
					break;
				case QEStatusBarKnownStates.Parsing:
					_GeneralPanel.SetImages(S_ExecutingBitmaps);
					((ToolStripItem)(object)_GeneralPanel).Text = ControlsResources.StatusBarParsingQueryBatch;
					_GeneralPanel.StartAnimate();
					break;
				case QEStatusBarKnownStates.Unknown:
					_GeneralPanel.SetOneImage(null);
					((ToolStripItem)(object)_GeneralPanel).Text = string.Empty;
					break;
			}

			if (newState == QEStatusBarKnownStates.Executing || newState == QEStatusBarKnownStates.Debugging || newState == QEStatusBarKnownStates.Offline)
			{
				DateTime now = DateTime.Now;
				SetExecutionCompletedTime(now);
				SetRowsAffected(0L);
			}

			if (newState == QEStatusBarKnownStates.ExecutionTimedOut || newState == QEStatusBarKnownStates.ExecutionOk || newState == QEStatusBarKnownStates.ExecutionFailed || newState == QEStatusBarKnownStates.ExecutionCancelled)
			{
				if (QryMgr.QueryExecutionStartTime.HasValue && QryMgr.QueryExecutionEndTime.HasValue)
				{
					_ = QryMgr.QueryExecutionStartTime.Value;
					DateTime value = QryMgr.QueryExecutionEndTime.Value;
					SetExecutionCompletedTime(value);
				}

				SetRowsAffected(QryMgr.RowsAffected);
				AuxiliaryDocData auxDocData = ((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(EditorWindowPane.DocData);
				if (auxDocData != null && auxDocData.ResultsSettings.ProvideFeedbackWithSounds)
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
		if (_StatusStrip.BackColor == EditorTabAndStatusBarSettings.Defaults.StatusBarColor)
		{
			_ = _StatusStrip.BackColor = _StatusStrip.BackColor = VsColorUtilities.GetShellColor(-53);
		}

		if ((double)_StatusStrip.BackColor.GetBrightness() < 0.5)
		{
			_StatusStrip.ForeColor = Color.White;
		}
		else
		{
			_StatusStrip.ForeColor = Color.Black;
		}

		if (_StatusStrip.Dock == DockStyle.Top && UserSettings.Instance.Current.EditorContext.StatusBarPosition != 0 || _StatusStrip.Dock == DockStyle.Bottom && UserSettings.Instance.Current.EditorContext.StatusBarPosition != EnStatusBarPosition.Bottom)
		{
			switch (UserSettings.Instance.Current.EditorContext.StatusBarPosition)
			{
				case EnStatusBarPosition.Bottom:
					_StatusStrip.Dock = DockStyle.Bottom;
					break;
				case EnStatusBarPosition.Top:
					_StatusStrip.Dock = DockStyle.Top;
					break;
			}
		}

		if (UserSettings.Instance.Current.StatusBar.LayoutPropertyChanged)
		{
			RebuildPanels();
			UserSettings.Instance.Current.StatusBar.LayoutPropertyChanged = false;
		}
	}

	private void RebuildPanels()
	{
		_StatusStrip.Items.Clear();
		_StatusStrip.Items.Add((ToolStripItem)(object)_GeneralPanel);
		if (UserSettings.Instance.Current.StatusBar.StatusBarIncludeServerName)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_ServerNamePanel);
		}

		if (UserSettings.Instance.Current.StatusBar.StatusBarIncludeLoginName)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_UserNamePanel);
		}

		if (UserSettings.Instance.Current.StatusBar.StatusBarIncludeDatabaseName)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_DatabaseNamePanel);
		}

		if (UserSettings.Instance.Current.StatusBar.ShowTimeOption == EnDisplayTimeOptions.Elapsed)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_ExecutionTimePanel);
		}

		if (UserSettings.Instance.Current.StatusBar.ShowTimeOption == EnDisplayTimeOptions.End)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_CompletedTimePanel);
		}

		if (_RowCountValid && UserSettings.Instance.Current.StatusBar.StatusBarIncludeRowCount)
		{
			_StatusStrip.Items.Add(new ToolStripSeparator());
			_StatusStrip.Items.Add((ToolStripItem)(object)_NumOfRowsPanel);
		}
	}

	private void ResetPanelsForOnlineMode()
	{
		ConnectionStrategy connectionStrategy = QryMgr.ConnectionStrategy;
		string displayUserName = connectionStrategy.DisplayUserName;
		string displayDatabaseName = connectionStrategy.DisplayDatabaseName;
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
		((ToolStripItem)(object)_DatabaseNamePanel).Text = displayDatabaseName ?? string.Empty;
		if (!QryMgr.QueryExecutionEndTime.HasValue || !QryMgr.QueryExecutionStartTime.HasValue)
		{
			((ToolStripItem)(object)_ExecutionTimePanel).Text = "00:00:00";
			((ToolStripItem)(object)_CompletedTimePanel).Text = "00:00:00";
		}
		else
		{
			((ToolStripItem)(object)_ExecutionTimePanel).Text = (QryMgr.QueryExecutionEndTime.Value - QryMgr.QueryExecutionStartTime.Value).FormatForStatus();
			((ToolStripItem)(object)_CompletedTimePanel).Text = QryMgr.QueryExecutionEndTime.Value.ToString(CultureInfo.CurrentCulture);
		}

		SetRowsAffected(QryMgr.RowsAffected);
	}

	private void ResetDatabaseName()
	{
		string displayDatabaseName = QryMgr.ConnectionStrategy.DisplayDatabaseName;
		((ToolStripItem)(object)_DatabaseNamePanel).Text = displayDatabaseName ?? string.Empty;
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
