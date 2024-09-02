// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.AuxiliaryDocData

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.Shared.Model;


// =========================================================================================================
//										AuxilliaryDocData Class
//
/// <summary>
/// This class handles all additional DocData information and events for an active document.
/// AuxilliaryDocData is linked to a document's <see cref="IVsTextLines"/> through DocData.
/// </summary>
// =========================================================================================================
public sealed class AuxilliaryDocData : IDisposable
{

	// ---------------------------------------------------
	#region Constructors / Destructors - AuxilliaryDocData
	// ---------------------------------------------------


	/// <summary>
	/// Default .ctor.
	/// </summary>
	public AuxilliaryDocData(IBsEditorPackage package, uint cookie, string documentMoniker, string inflightMoniker, object docData)
	{
		_ExtensionInstance = package;
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
					_QryMgr.ExecutionStartedEventAsync -= OnQueryExecutionStartedAsync;
					_QryMgr.ExecutionCompletedEventAsync -= OnQueryExecutionCompletedAsync;
					_QryMgr.NotifyConnectionStateEvent -= OnNotifyConnectionState;

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
	private readonly IBsEditorPackage _ExtensionInstance = null;
	private string _InflightMoniker;
	private bool _IntellisenseActive = false;
	private bool? _IntellisenseEnabled;
	private string _InternalDocumentMoniker;
	private QueryManager _QryMgr;
	// private IBsConnectionStrategyFactory _StrategyFactory;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AuxilliaryDocData
	// =========================================================================================================


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


	public string[] CommandDatabaseList
	{
		get { return _CommandDatabaseList; }
		set { _CommandDatabaseList = value; }
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


	public uint DocCookie
	{
		get
		{
			return _DocCookie;
		}
		set
		{
			_DocCookie = value;

			if (QryMgr != null)
				QryMgr.DocCookie = value;
		}
	}


	public object DocData => _DocData;

	public EnSqlExecutionType ExecutionType => QryMgr.LiveSettings.ExecutionType;

	public IBsEditorPackage ExtensionInstance => _ExtensionInstance;


	public bool HasActualPlan
	{
		get { return QryMgr.LiveSettings.WithActualPlan; }
		set { QryMgr.LiveSettings.WithActualPlan = value; }
	}


	public bool HasExecutionPlan => QryMgr.LiveSettings.HasExecutionPlan;

	public bool HasClone => _CloneCardinal > 0;


	public string InflightMoniker => _InflightMoniker;


	public bool IntellisenseActive
	{
		get
		{
			lock (_LockLocal)
				return _IntellisenseActive;
		}
		set
		{
			lock (_LockLocal)
			{
				bool activeOnly = PersistentSettings.EditorIntellisensePolicy == EnIntellisensePolicy.ActiveOnly;

				// If not active only and user data already set to value, exit.

				if (!activeOnly && _IntellisenseActive == value)
					return;


				// If active only alter the value if not current.

				if (value)
				{
					value = PersistentSettings.EditorIntellisensePolicy != EnIntellisensePolicy.ActiveOnly;

					if (!value)
					{
						IBsTabbedEditorPane currentTabbedEditor = ((IBsEditorPackage)ApcManager.PackageInstance).CurrentTabbedEditor;

						if (ReferenceEquals(this, currentTabbedEditor?.AuxDocData))
							value = true;
					}
				}

				// _IntellisenseActive tracks the user data setting so set only if not equal.

				if (_IntellisenseActive == value)
					return;

				_IntellisenseActive = value;

				IVsUserData vsUserData = VsUserData;

				if (vsUserData == null)
				{
					_IntellisenseActive = false;
					return;
				}

				Guid riidKey = VS.CLSID_PropIntelliSenseEnabled;

				___(vsUserData.SetData(ref riidKey, value));



				// If not active only, we're done.

				if (PersistentSettings.EditorIntellisensePolicy != EnIntellisensePolicy.ActiveOnly)
					return;

				((IBsLanguagePackage)ApcManager.PackageInstance).LanguageSvc?.RefreshIntellisense(value);
			}
		}
	}


	public bool? IntellisenseEnabled
	{
		get
		{
			lock (_LockLocal)
				return _IntellisenseEnabled;
		}
		set
		{
			lock (_LockLocal)
			{
				_IntellisenseEnabled = value;

				bool enabled = value != null && value.Value;

				if (enabled != _IntellisenseActive)
					IntellisenseActive = enabled;
			}
		}
	}


	public string InternalMoniker
	{
		get { return _InternalDocumentMoniker; }
		set { _InternalDocumentMoniker = value; }
	}


	public bool IsOnline => false;


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


	public object MetadataProviderProvider { get; }


	public EnEditorMode Mode => EnEditorMode.Standard;


	public QueryManager QryMgr
	{
		get { lock (_LockLocal) return _QryMgr; }
	}


	public EnSqlOutputMode SqlOutputMode
	{
		get
		{
			return LiveSettings.EditorResultsOutputMode;
		}
		set
		{
			// Tracer.Trace(GetType(), "set_SqlOutputMode", "value = {0}", value);
			if (QryMgr.IsLocked)
			{
				InvalidOperationException ex = new(Resources.ExSqlExecutionModeChangeFailed);
				Diag.ThrowException(ex);
			}

			if (LiveSettings.EditorResultsOutputMode != value)
			{
				LiveSettings.EditorResultsOutputMode = value;
				OutputModeChangedEvent?.Invoke(this, new(LiveSettings));
			}
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
				return vsUserData;
			}
		}
	}




	public event EventHandler<LiveSettingsEventArgs> OutputModeChangedEvent;
	public event EventHandler<LiveSettingsEventArgs> LiveSettingsChangedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AuxilliaryDocData
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	private static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	public int CloneAdd() => ++_CloneCardinal;
	public int CloneRemove() => --_CloneCardinal;



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


	public QueryManager CreateQueryManager(IBsCsb csb, uint docCookie)
	{
		try
		{
			ConnectionStrategy strategy = new(docCookie);

			_QryMgr = new QueryManager(strategy, docCookie);
			_QryMgr.StatusChangedEvent += OnQueryManagerStatusChanged;
			_QryMgr.ExecutionStartedEventAsync += OnQueryExecutionStartedAsync;
			_QryMgr.ExecutionCompletedEventAsync += OnQueryExecutionCompletedAsync;
			_QryMgr.NotifyConnectionStateEvent += OnNotifyConnectionState;

			strategy.Initialize(csb, OnNotifyConnectionState);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		return _QryMgr;
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

		if (!QryMgr.IsExecuting && !QryMgr.LiveTransactions)
			return true;


		bool inAutomation = UnsafeCmd.IsInAutomationFunction();
		DialogResult dialogResult = DialogResult.Yes;

		if (QryMgr.IsExecuting)
		{
			if (!inAutomation)
			{
				RdtManager.AsyeuShowWindowFrame(_DocCookie);
				msgResource ??= Resources.MsgAbortExecutionAndClose;
				dialogResult = MessageCtl.ShowEx(msgResource,
					Resources.MsgQueryAbort_IsExecutingCaption,
					MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			}


			if (dialogResult == DialogResult.No)
				return false;

			if (QryMgr.IsExecuting)
				QryMgr.Cancel(true);
			RollbackTransactions(false);
		}
		else if (QryMgr.HasTransactions)
		{
			/*
			if (isClone)
			{
				CloneRemove();
				return true;
			}
			*/

			dialogResult = DialogResult.No;

			if (!inAutomation)
			{
				RdtManager.AsyeuShowWindowFrame(_DocCookie);

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

					RollbackTransactions(false);
					break;
			}
		}


		return true;
	}



	public void RollbackTransactions(bool validate)
	{
		if (QryMgr == null || !QryMgr.IsConnected || QryMgr.IsLocked)
			return;

		QryMgr.RollbackTransactions(validate);
	}



	public static bool ShowAbortExecutionsCloseAllDialog(Dictionary<AuxilliaryDocData, string> docs)
	{
		DialogResult dialogResult = DialogResult.Yes;
		bool inAutomation = UnsafeCmd.IsInAutomationFunction();

		if (!inAutomation)
		{

			string names = "";

			List<string> sortList = [];
			List<string> savedList = [];

			foreach (KeyValuePair<AuxilliaryDocData, string> pair in docs)
			{
				string str;
				string datasetKey = pair.Key.QryMgr.Strategy.AdornedQualifiedTitle;

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
				sortList.Add("");

			foreach (string str in savedList)
				sortList.Add(str);

			foreach (string name in sortList)
				names += (names != "" ? "\n" : "") + Resources.MsgQueryAbort_NameIndent.FmtRes(name);

			dialogResult = MessageCtl.ShowEx(Resources.MsgQueryAbort_IsExecutingList.FmtRes(names),
				Resources.MsgQueryAbort_IsExecutingCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
		}

		if (dialogResult == DialogResult.No)
			return false;

		foreach (AuxilliaryDocData auxDocData in docs.Keys)
		{
			if (auxDocData.QryMgr.IsExecuting)
				auxDocData.QryMgr.Cancel(true);
			auxDocData.RollbackTransactions(false);
		}

		return true;

	}



	public static bool ShowDeactivateTtsCloseAllDialog(Dictionary<AuxilliaryDocData, string> docs)
	{
		DialogResult dialogResult = DialogResult.No;
		bool inAutomation = UnsafeCmd.IsInAutomationFunction();

		if (!inAutomation)
		{

			string names = "";

			List<string> sortList = [];
			List<string> savedList = [];

			foreach (KeyValuePair<AuxilliaryDocData, string> pair in docs)
			{
				string str;
				string datasetKey = pair.Key.QryMgr.Strategy.AdornedQualifiedTitle;

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
				sortList.Add("");

			foreach (string str in savedList)
				sortList.Add(str);


			foreach (string name in sortList)
				names += (names != "" ? "\n" : "") + Resources.MsgQueryAbort_NameIndent.FmtRes(name);

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
					auxDocData.RollbackTransactions(false);

				break;
		}

		return true;
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



	public void UpdateLiveSettingsState(IBsEditorTransientSettings liveSettings)
	{
		QryMgr.LiveSettingsApplied = false;
		LiveSettingsChangedEvent?.Invoke(this, new(liveSettings));
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AuxilliaryDocData
	// =========================================================================================================


	public bool OnNotifyConnectionState(object sender, NotifyConnectionStateEventArgs args)
	{
		bool isUnlocked = QryMgr?.NotifyConnectionState(args.State) ?? false;

		if (args.State > EnNotifyConnectionState.NotifyEnumEndMarker)
			return isUnlocked;


		string name;

		if (IsVirtualWindow)
		{
			try
			{
				name = Cmd.GetFileName(InflightMoniker);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex, $"Inflight moniker: {InflightMoniker}.");
				throw;
			}

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
			return isUnlocked;


		string msg;
		string ttsMsg = args.TtsDiscarded ? Resources.WarnQueryTtsDiscarded.FmtRes(indent) : "";


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
				return isUnlocked;
		}


		Diag.AsyuiOutputPaneWriteLine(msg, true);

		return isUnlocked;
	}



	private void OnQueryManagerStatusChanged(object sender, QueryStateChangedEventArgs args)
	{
		lock (_LockLocal)
		{
			if (!args.IsStateConnected)
				return;

			// Tracer.Trace(GetType(), "OnQueryManagerStatusChanged()", "IN");

			IVsUserData vsUserData = VsUserData;
			if (args.Value)
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
				vsUserData.SetData(ref riidKey, "");
			}

			return;
		}
	}



	private async Task<bool> OnQueryExecutionCompletedAsync(object sender, ExecutionCompletedEventArgs args)
	{
		// if (args.CancelToken.Cancelled())
		//	return;

		if (!args.SyncToken.Cancelled())
		{
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
		}

		_ConnectionUrlAtExecutionStart = null;

		return await Task.FromResult(true);
	}


	private async Task<bool> OnQueryExecutionStartedAsync(object sender, ExecutionStartedEventArgs args)
	{
		if (args.Connection != null)
			_ConnectionUrlAtExecutionStart = Csb.CreateConnectionUrl(args.Connection);
		else
			_ConnectionUrlAtExecutionStart = null;

		return await Task.FromResult(true);
	}


	#endregion Event Handling

}
