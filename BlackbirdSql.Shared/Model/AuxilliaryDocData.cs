// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.AuxiliaryDocData

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Model;


// =========================================================================================================
//										AuxilliaryDocData Class
//
/// <summary>
/// This class handles all additional DocData information and events for an active document.
/// AuxilliaryDocData is linked to a document's <see cref="IVsTextLines"/> through DocData.
/// </summary>
// =========================================================================================================
public sealed class AuxilliaryDocData
{

	// ---------------------------------------------------
	#region Constructors / Destructors - AuxilliaryDocData
	// ---------------------------------------------------


	/// <summary>
	/// Default .ctor.
	/// </summary>
	public AuxilliaryDocData(uint cookie, string documentMoniker, string inflightMoniker, object docData)
	{
		_DocCookie = cookie;
		_InternalDocumentMoniker = documentMoniker;
		_InflightMoniker = inflightMoniker;
		_DocData = docData;
	}



	public void Dispose()
	{
		Dispose(disposing: true);
	}



	private void Dispose(bool disposing)
	{
		lock (_LockLocal)
		{
			if (disposing)
			{
				// _StrategyFactory?.Dispose();

				if (_QryMgr != null)
				{
					_QryMgr.StatusChangedEvent -= OnQueryManagerStatusChanged;
					_QryMgr.ExecutionStartedEvent -= OnQueryExecutionStarted;
					_QryMgr.ExecutionCompletedEvent -= OnQueryExecutionCompleted;
					_QryMgr.NotifyConnectionStateEvent -= OnNotifyConnectionState;
					_QryMgr.InvalidateToolbarEvent -= OnInvalidateToolbar;
					_QryMgr.ShowWindowFrameEvent -= OnShowWindowFrame;

					NotifyConnectionStateEvent -= _QryMgr.OnNotifyConnectionState;

					_QryMgr.Dispose();
					_QryMgr = null;
				}
			}
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AuxilliaryDocData
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	/// <summary>
	/// Records the last moniker created so that we can do a fast equivalency comparison
	/// on the connection and use this static for the DatasetKey if they are equivalent.
	/// This avoids repeatedly creating a new Moniker and going through the
	/// registration process each time.
	/// </summary>
	private string[] _CommandDatabaseList = null;
	private long _CommandRctStamp = -1;
	private string _CommandSelectedName = null;

	private string _ConnectionUrlAtExecutionStart;
	private readonly object _DocData;
	private uint _DocCookie;
	private int _CloneCardinal = 0;
	private string _InflightMoniker;
	private bool? _IntellisenseEnabled;
	private string _InternalDocumentMoniker;
	private QueryManager _QryMgr;
	// private IBsConnectionStrategyFactory _StrategyFactory;

	private NotifyConnectionStateEventHandler _NotifyConnectionStateEvent = null;

	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AuxilliaryDocData
	// =========================================================================================================


	public object MetadataProviderProvider { get; }

	/*
	public IBsConnectionStrategyFactory StrategyFactory
	{
		get
		{
			lock (_LockLocal)
				return _StrategyFactory;
		}
		set
		{
			if (value == null)
			{
				ArgumentNullException ex = new("value");
				Diag.Dug(ex);
				throw ex;
			}

			lock (_LockLocal)
			{
				_StrategyFactory?.Dispose();
				_StrategyFactory = value;

				if (_QryMgr != null)
				{
					_QryMgr.Strategy = _StrategyFactory.CreateConnectionStrategy();
					_QryMgr.Strategy.InitializeKeepAlive(_QryMgr.OnInvalidateToolbar, _QryMgr.OnQueryIsLocked);
				}
			}
		}
	}
	*/

	public string[] CommandDatabaseList
	{
		get { return _CommandDatabaseList; }
		set { _CommandDatabaseList = value; }
	}


	public uint DocCookie
	{
		get { return _DocCookie; }
		set { _DocCookie = value; }
	}


	public object DocData => _DocData;


	public bool ClientStatisticsEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				return QryMgr.LiveSettings.WithClientStats;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				QryMgr.LiveSettings.WithClientStats = value;
			}
		}
	}


	public EnSqlExecutionType ExecutionType
	{
		get { return QryMgr.LiveSettings.ExecutionType; }
	}


	public string InflightMoniker => _InflightMoniker;


	public bool HasActualPlan
	{
		get { return QryMgr.LiveSettings.WithActualPlan; }
		set { QryMgr.LiveSettings.WithActualPlan = value; }
	}


	public bool HasExecutionPlan => QryMgr.LiveSettings.HasExecutionPlan;


	public bool HasClone
	{
		get { return _CloneCardinal > 0; }
	}


	public bool? IntellisenseEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				if (!_IntellisenseEnabled.HasValue)
					_IntellisenseEnabled = PersistentSettings.EditorEnableIntellisense;

				return _IntellisenseEnabled;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				IVsUserData vsUserData = VsUserData;
				if (vsUserData != null)
				{
					Guid riidKey = VS.CLSID_PropIntelliSenseEnabled;
					___(vsUserData.SetData(ref riidKey, value));
					_IntellisenseEnabled = value;
				}
			}
		}
	}


	public bool IsOnline => false;


	public EnEditorMode Mode => EnEditorMode.Standard;


	/// <summary>
	/// True if underlying document is virtual else false. (Was IsQueryWindow)
	/// </summary>
	public bool IsVirtualWindow
	{
		get
		{
			return _InflightMoniker != null;
		}
		set
		{
			if (value)
				Diag.ThrowException(new ArgumentException(Resources.ExArgumentOnlyFalse.FmtRes(nameof(IsVirtualWindow))));

			if (_InflightMoniker == null)
				return;

			RdtManager.InflightMonikerCsbTable.Remove(_InflightMoniker);

			_InflightMoniker = null;
		}
	}


	public IBsEditorTransientSettings LiveSettings => QryMgr.LiveSettings;

	public string InternalMoniker
	{
		get { return _InternalDocumentMoniker; }
		set { _InternalDocumentMoniker = value; }
	}


	public QueryManager QryMgr
	{
		get { lock (_LockLocal) return _QryMgr; }
	}


	public QueryManager CreateQueryManager(Csb csb, EnEditorCreationFlags creationFlags)
	{
		ConnectionStrategy strategy = new();

		if ((creationFlags & EnEditorCreationFlags.CreateConnection) > 0)
		{
			try
			{
				ModelCsb mdlCsb = new ModelCsb(csb);
				mdlCsb.CreateDataConnection();
				strategy.LiveMdlCsb = mdlCsb;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}

		_QryMgr = new QueryManager(strategy);
		_QryMgr.StatusChangedEvent += OnQueryManagerStatusChanged;
		_QryMgr.ExecutionStartedEvent += OnQueryExecutionStarted;
		_QryMgr.ExecutionCompletedEvent += OnQueryExecutionCompleted;
		_QryMgr.NotifyConnectionStateEvent += OnNotifyConnectionState;
		_QryMgr.InvalidateToolbarEvent += OnInvalidateToolbar;
		_QryMgr.ShowWindowFrameEvent += OnShowWindowFrame;

		NotifyConnectionStateEvent += QryMgr.OnNotifyConnectionState;

		strategy.InitializeKeepAlive(OnInvalidateToolbar, OnNotifyConnectionState);

		return _QryMgr;
	}

	public EnSqlOutputMode SqlOutputMode
	{
		get
		{
			return LiveSettings.EditorResultsOutputMode;
		}
		set
		{
			// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SqlOutputMode", "value = {0}", value);
			if (QryMgr.IsLocked)
			{
				InvalidOperationException ex = new(Resources.ExSqlExecutionModeChangeFailed);
				Diag.ThrowException(ex);
			}

			if (LiveSettings.EditorResultsOutputMode != value)
			{
				LiveSettings.EditorResultsOutputMode = value;
				SqlExecutionModeChangedEvent?.Invoke(this, new(value));
			}
		}
	}


	public long CommandRctStamp
	{
		get
		{
			return _CommandRctStamp;
		}
		set
		{
			if (_CommandRctStamp == value)
				return;

			_CommandDatabaseList = null;
			_CommandRctStamp = value;
		}
	}

	public string CommandSelectedName
	{
		get
		{
			return _CommandSelectedName;
		}
		set
		{
			if (_CommandSelectedName == value)
				return;

			_CommandDatabaseList = null;
			_CommandSelectedName = value;
		}
	}


	public bool SuppressSavePrompt => IsVirtualWindow || !PersistentSettings.EditorPromptSave;


	public bool TtsEnabled
	{
		get
		{
			lock (_LockLocal)
				return QryMgr.LiveSettings.TtsEnabled;
		}
		set
		{
			lock (_LockLocal)
				QryMgr.LiveSettings.TtsEnabled = value;
		}
	}


	public IVsUserData VsUserData
	{
		get
		{
			lock (_LockLocal)
			{
				IVsUserData vsUserData = DocData as IVsUserData;
				// Tracer.Trace(GetType(), "AuxilliaryDocData.GetIVsUserData", "value of IVsUserData returned is {0}", vsUserData);
				return vsUserData;
			}
		}
	}




	public event EventHandler<SqlExecutionModeChangedEventArgs> SqlExecutionModeChangedEvent;
	public event EventHandler<LiveSettingsChangedEventArgs> LiveSettingsChangedEvent;

	public event NotifyConnectionStateEventHandler NotifyConnectionStateEvent
	{
		add { _NotifyConnectionStateEvent += value; }
		remove { _NotifyConnectionStateEvent -= value; }
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AuxilliaryDocData
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	private static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	public int AddClone() => ++_CloneCardinal;


	public void CommitTransactions(bool validate)
	{
		if (QryMgr == null || !QryMgr.IsConnected || QryMgr.IsLocked)
			return;

		bool result = QryMgr.CommitTransactions(validate);

		if (!result)
			return;

		if (!validate || DocData is not IVsPersistDocData2 persistData)
			return;

		// Only clear dirty flag on queries not on disk.

		if (DocCookie == 0)
			return;

		string moniker = RdtManager.GetDocumentMoniker(_DocCookie);

		if (moniker == null || !moniker.StartsWith(Path.GetTempPath(), StringComparison.InvariantCultureIgnoreCase))
			return;

		persistData.SetDocDataDirty(0);
	}


	public IBsErrorTaskFactory GetErrorTaskFactory() => null;



	/// <summary>
	/// For performance. Gets the AuxilliaryDocData directly from DocData.
	/// </summary>
	public static AuxilliaryDocData GetUserDataAuxilliaryDocData(object docData)
	{
		if (docData == null)
			return null;

		if (docData is not IVsUserData vsUserData)
			return null;

		AuxilliaryDocData value = null;
		Guid clsid = new(LibraryData.C_AuxilliaryDocDataGuid);

		try
		{
			vsUserData.GetData(ref clsid, out object objData);

			if (objData != null)
				value = objData as AuxilliaryDocData;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, "GetData failed on DocData for LibraryData.AuxilliaryDocDataGuid.");
		}

		return value;
	}



	public bool RequestDeactivateQuery(string msgResource = null)
	{
		// Tracer.Trace(GetType(), "RequestDeactivateQuery()");

		if (QryMgr == null)
			return true;

		bool inAutomation = UnsafeCmd.IsInAutomationFunction();
		DialogResult dialogResult = DialogResult.Yes;

		if (QryMgr.IsExecuting)
		{
			if (!inAutomation)
			{
				ShowWindowFrame();
				msgResource ??= Resources.MsgAbortExecutionAndClose;
				dialogResult = MessageCtl.ShowEx(msgResource,
					Resources.MsgQueryAbort_IsExecutingCaption,
					MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			}


			if (dialogResult == DialogResult.No)
				return true;

			if (QryMgr.IsExecuting)
				QryMgr.Cancel(true);
			RollbackTransactions();
		}
		else if (QryMgr.HasTransactions)
		{
			if (HasClone)
			{
				_CloneCardinal--;
				return true;
			}

			dialogResult = DialogResult.No;

			if (!inAutomation)
			{
				ShowWindowFrame();
				msgResource ??= Resources.MsgQueryAbort_UncommittedTransactionsClose;
				dialogResult = MessageCtl.ShowEx(msgResource,
					Resources.MsgQueryAbort_UncommittedTransactionsCaption,
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
			}

			switch (dialogResult)
			{
				case DialogResult.Cancel:
					return false;

				case DialogResult.Yes:

					CommitTransactions(false);
					break;

				case DialogResult.No:

					RollbackTransactions();
					break;
			}
		}

		return true;
	}


	public static bool ShowAbortExecutionsCloseAllDialog(Dictionary<AuxilliaryDocData, string> docs)
	{
		DialogResult dialogResult = DialogResult.Yes;
		bool inAutomation = UnsafeCmd.IsInAutomationFunction();

		if (!inAutomation)
		{

			string names = string.Empty;

			List<string> sortList = [];
			List<string> savedList = [];

			foreach (KeyValuePair<AuxilliaryDocData, string> pair in docs)
			{
				string str;
				string datasetKey = pair.Key.QryMgr.Strategy.AdornedQualifiedName;

				if (pair.Key.IsVirtualWindow)
				{
					str = Resources.MsgQueryAbort_QualifiedName.FmtRes(pair.Value, datasetKey);
					sortList.Add(str);
				}
				else
				{
					str = Cmd.GetShortenedMonikerPath(pair.Value, 72 - datasetKey.Length);
					str = Resources.MsgQueryAbort_QualifiedName.FmtRes(str, datasetKey);
					savedList.Add(str);
				}
			}

			sortList.Sort(StringComparer.InvariantCultureIgnoreCase);
			savedList.Sort(StringComparer.InvariantCultureIgnoreCase);

			if (sortList.Count > 0 && savedList.Count > 0)
				sortList.Add(string.Empty);

			foreach (string str in savedList)
				sortList.Add(str);

			foreach (string name in sortList)
				names += (names != string.Empty ? "\n" : "") + Resources.MsgQueryAbort_NameIndent.FmtRes(name);

			dialogResult = MessageCtl.ShowEx(Resources.MsgQueryAbort_IsExecutingList.FmtRes(names),
				Resources.MsgQueryAbort_IsExecutingCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
		}

		if (dialogResult == DialogResult.No)
			return false;

		foreach (AuxilliaryDocData auxDocData in docs.Keys)
		{
			if (auxDocData.QryMgr.IsExecuting)
				auxDocData.QryMgr.Cancel(true);
			auxDocData.RollbackTransactions();
		}

		return true;

	}



	public static bool ShowDeactivateTtsCloseAllDialog(Dictionary<AuxilliaryDocData, string> docs)
	{
		DialogResult dialogResult = DialogResult.No;
		bool inAutomation = UnsafeCmd.IsInAutomationFunction();

		if (!inAutomation)
		{

			string names = string.Empty;

			List<string> sortList = [];
			List<string> savedList = [];

			foreach (KeyValuePair<AuxilliaryDocData, string> pair in docs)
			{
				string str;
				string datasetKey = pair.Key.QryMgr.Strategy.AdornedQualifiedName;

				if (pair.Key.IsVirtualWindow)
				{
					str = Resources.MsgQueryAbort_QualifiedName.FmtRes(pair.Value, datasetKey);
					sortList.Add(str);
				}
				else
				{
					str = Cmd.GetShortenedMonikerPath(pair.Value, 72 - datasetKey.Length);
					str = Resources.MsgQueryAbort_QualifiedName.FmtRes(str, datasetKey);
					savedList.Add(str);
				}
			}

			sortList.Sort(StringComparer.InvariantCultureIgnoreCase);
			savedList.Sort(StringComparer.InvariantCultureIgnoreCase);

			if (sortList.Count > 0 && savedList.Count > 0)
				sortList.Add(string.Empty);

			foreach (string str in savedList)
				sortList.Add(str);


			foreach (string name in sortList)
				names += (names != string.Empty ? "\n" : "") + Resources.MsgQueryAbort_NameIndent.FmtRes(name);

			dialogResult = MessageCtl.ShowEx(Resources.MsgQueryAbort_UncommittedTransactionsList.FmtRes(names),
				Resources.MsgQueryAbort_UncommittedTransactionsCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
		}

		switch (dialogResult)
		{
			case DialogResult.Cancel:
				return false;

			case DialogResult.Yes:

				foreach (AuxilliaryDocData auxDocData in docs.Keys)
					auxDocData.CommitTransactions(false);

				break;

			case DialogResult.No:

				foreach (AuxilliaryDocData auxDocData in docs.Keys)
					auxDocData.RollbackTransactions();

				break;
		}

		return true;
	}




	public void RollbackTransactions()
	{
		if (QryMgr == null || !QryMgr.IsConnected || QryMgr.IsLocked
			|| !QryMgr.GetUpdatedTransactionsStatus(true))
		{
			return;
		}

		QryMgr.RollbackTransactions(false);
	}



	/// <summary>
	/// For performance. Sets a reference to the AuxilliaryDocData in DocData.
	/// </summary>
	public static void SetUserDataAuxilliaryDocData(object docData, AuxilliaryDocData value)
	{
		if (docData == null)
			return;

		if (docData is not IVsUserData vsUserData)
		{
			Type type = docData.GetType();

			ArgumentException ex = new ArgumentException($"DocData is not IVsUserData: {type.FullName}. SetData failed on DocData for LibraryData.AuxilliaryDocDataGuid.",
				nameof(docData));

			Diag.Dug(ex);

			return;
		}

		Guid clsid = new(LibraryData.C_AuxilliaryDocDataGuid);

		try
		{
			vsUserData.SetData(ref clsid, value);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, "SetData failed on DocData for LibraryData.AuxilliaryDocDataGuid.");
		}
	}



	public void ShowWindowFrame()
	{
		if (DocCookie == 0)
			return;

		if (!ThreadHelper.CheckAccess())
		{
			// Fire and wait.

			bool result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				RdtManager.ShowFrame(_DocCookie);

				return true;
			});

		}
		else
		{
			RdtManager.ShowFrame(_DocCookie);
		}
	}



	public void UpdateLiveSettingsState(IBsEditorTransientSettings liveSettings)
	{
		QryMgr.LiveSettingsApplied = false;
		LiveSettingsChangedEvent?.Invoke(this, new(liveSettings));
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AuxilliaryDocData
	// =========================================================================================================


	public void OnInvalidateToolbar(object sender, EventArgs args)
	{
		if (DocCookie != 0)
			RdtManager.InvalidateToolbar(_DocCookie);
	}



	public bool OnNotifyConnectionState(object sender, NotifyConnectionStateEventArgs args)
	{
		if (args.State > EnNotifyConnectionState.NotifyEnumEndMarker)
		{
			return _NotifyConnectionStateEvent?.Invoke(sender, args) ?? true;
		}


		string name;

		if (IsVirtualWindow)
		{
			name = Path.GetFileName(InflightMoniker);
			name = Resources.QueryGlyphFormat.FmtRes(SystemData.C_SessionTitleGlyph, name);
		}
		else
		{
			name = RdtManager.GetDocumentMoniker(DocCookie);
			name = Cmd.GetShortenedMonikerPath(name, 80);
		}

		string prefix;
		string indent = new string(' ', 4);
		string datasetKey = QryMgr?.Strategy?.MdlCsb?.DatasetKey;

		if (datasetKey == null)
			return true;


		string msg;
		string ttsMsg = args.TtsDiscarded ? Resources.WarnQueryTtsDiscarded.FmtRes(indent) : string.Empty;


		switch (args.State)
		{
			case EnNotifyConnectionState.NotifyAutoClosed:
				prefix = Resources.InfoQueryPrefix.FmtRes(indent);
				msg = Resources.InfoQueryConnectionAutoClosed.FmtRes(prefix, indent,
					datasetKey, QryMgr.Strategy.MdlCsb.ConnectionLifeTime, name);
				break;
			case EnNotifyConnectionState.NotifyBroken:
				prefix = Resources.WarnQueryPrefix.FmtRes(indent);
				msg = Resources.WarnQueryConnectionBroken.FmtRes(prefix, indent,
					datasetKey, ttsMsg, name);
				break;
			case EnNotifyConnectionState.NotifyDead:
				prefix = Resources.WarnQueryPrefix.FmtRes(indent);
				msg = Resources.WarnQueryConnectionDead.FmtRes(prefix, indent,
					datasetKey, ttsMsg, name);
				break;
			case EnNotifyConnectionState.NotifyReset:
				prefix = Resources.WarnQueryPrefix.FmtRes(indent);
				msg = Resources.WarnQueryConnectionReset.FmtRes(prefix, indent,
					datasetKey, ttsMsg, name);
				break;
			default:
				return true;
		}


		Diag.AsyncOutputPaneWriteLine(msg, true);

		return true;
	}



	private void OnQueryManagerStatusChanged(object sender, QueryStatusChangedEventArgs args)
	{
		lock (_LockLocal)
		{
			if (args.StatusFlag != EnQueryStatusFlags.Connected)
				return;


			IVsUserData vsUserData = VsUserData;
			if (QryMgr.IsConnected)
			{
				IDbConnection connection = QryMgr.Strategy.Connection;

				if (connection.State != ConnectionState.Open)
					return;

				if (connection is DbConnection dbConnection)
				{
					string serverVersion = dbConnection.ServerVersion;
					Guid riidKey = VS.CLSID_PropSqlVersion;
					vsUserData.SetData(ref riidKey, serverVersion);
				}
			}
			else
			{
				Guid riidKey = VS.CLSID_PropSqlVersion;
				vsUserData.SetData(ref riidKey, string.Empty);
			}

			return;
		}
	}



	private void OnQueryExecutionCompleted(object sender, QueryExecutionCompletedEventArgs args)
	{
		if (args.SyncCancel)
			return;

		string connectionUrl = null;
		IDbConnection connection = QryMgr.Strategy.Connection;

		if (connection != null)
			connectionUrl = Csb.CreateConnectionUrl(connection);

		EnNullEquality nullEquality = Cmd.NullEquality(_ConnectionUrlAtExecutionStart, connectionUrl);

		if (nullEquality == EnNullEquality.NotNulls)
		{
			nullEquality = connectionUrl.Equals(_ConnectionUrlAtExecutionStart, StringComparison.Ordinal)
				? EnNullEquality.Equal : EnNullEquality.UnEqual;
		}

		if (nullEquality == EnNullEquality.UnEqual)
		{
			IVsUserData vsUserData = VsUserData;
			if (vsUserData != null)
			{
				Guid riidKey = VS.CLSID_PropDatabaseChanged;
				___(vsUserData.SetData(ref riidKey, connectionUrl));
			}
		}

		_ConnectionUrlAtExecutionStart = null;
	}


	private bool OnQueryExecutionStarted(object sender, QueryExecutionStartedEventArgs args)
	{
		if (args.Connection != null)
			_ConnectionUrlAtExecutionStart = Csb.CreateConnectionUrl(args.Connection);
		else
			_ConnectionUrlAtExecutionStart = null;

		return true;
	}



	public void OnShowWindowFrame(object sender, EventArgs args) => ShowWindowFrame();


	#endregion Event Handling




	// =========================================================================================================
	#region Sub-Classes - AuxilliaryDocData
	// =========================================================================================================


	public class LiveSettingsChangedEventArgs(IBsEditorTransientSettings liveSettings) : EventArgs
	{
		public IBsEditorTransientSettings LiveSettings { get; private set; } = liveSettings;
	}



	public class SqlExecutionModeChangedEventArgs(EnSqlOutputMode executionMode) : EventArgs
	{
		public EnSqlOutputMode SqlOutputMode { get; private set; } = executionMode;
	}


	#endregion Sub-Classes
}
