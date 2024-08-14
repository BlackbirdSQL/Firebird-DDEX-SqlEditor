// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.SqlEditorStrategy

using System;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;



namespace BlackbirdSql.Shared.Model;


// =========================================================================================================
//
//									AbstractConnectionStrategy Class
//
/// <summary>
/// Abstract ConnectionStrategy class. Manages connections for a query.
/// </summary>
// =========================================================================================================
public abstract class AbstractConnectionStrategy : IDisposable
{

	// ------------------------------------------------------------
	#region Constructors / Destructors - AbstractConnectionStrategy
	// ------------------------------------------------------------

	/// <summary>
	/// Default .ctor.
	/// </summary>
	public AbstractConnectionStrategy()
	{
	}


	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}



	protected virtual bool Dispose(bool disposing)
	{
		if (!disposing || _Disposed)
			return false;

		_Disposed = true;

		lock (_LockObject)
			SetDatabaseCsb(null, false);

		return true;
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - AbstractConnectionStrategy
	// =========================================================================================================


	// A protected 'this' object lock
	protected readonly object _LockObject = new object();

	private int _EventCardinal = 0;

	private IBsModelCsb _MdlCsb;
	private bool _Disposed = false;
	private bool? _HasTransactions;
	protected long _RctStamp = long.MinValue;

	private static readonly Color _SDefaultColor = SystemColors.Control;

	protected ConnectionChangedDelegate _ConnectionChangedEvent;
	protected StateChangeEventHandler _ConnectionStateChangedEvent;
	protected DatabaseChangeEventHandler _DatabaseChangedEvent;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractConnectionStrategy
	// =========================================================================================================


	public virtual string AdornedDisplayName =>
		MdlCsb?.AdornedDisplayName ?? string.Empty;

	public string AdornedQualifiedName =>
		MdlCsb?.AdornedQualifiedName ?? string.Empty;

	public string AdornedQualifiedTitle =>
		MdlCsb?.AdornedQualifiedTitle ?? string.Empty;

	public string AdornedTitle =>
		MdlCsb?.AdornedTitle ?? string.Empty;

	public DbConnection Connection => _MdlCsb?.DataConnection;

	public long ConnectionId => _MdlCsb?.ConnectionId ?? long.MinValue;

	public long RctStamp => _RctStamp;

	public virtual string LiveAdornedDisplayName =>
		LiveMdlCsb?.AdornedDisplayName ?? string.Empty;

	public virtual string LiveAdornedTitle =>
		LiveMdlCsb?.AdornedTitle ?? string.Empty;

	public IBsModelCsb LiveMdlCsb
	{
		get
		{
			lock (_LockObject)
			{
				if (_MdlCsb == null || !RaiseNotifyConnectionState(EnNotifyConnectionState.RequestIsUnlocked, false) || LiveTransactions)
					return _MdlCsb;

				if (RctManager.Loaded && _MdlCsb.IsInvalidated)
				{
					bool hasConnection = _MdlCsb.DataConnection != null;
					Csb csb = RctManager.CloneRegistered(_MdlCsb);

					SetDatabaseCsb(new ModelCsb(csb), hasConnection);
				}
				return _MdlCsb;
			}
		}
	}

	public IBsModelCsb MdlCsb => _MdlCsb;

	public virtual string DatasetDisplayName =>
		MdlCsb?.DisplayName ?? string.Empty;


	public virtual string DisplayServerName =>
		MdlCsb?.DataSource ?? string.Empty;


	public virtual string DisplayUserName =>
		MdlCsb?.UserID ?? string.Empty;


	/// <summary>
	/// Returns the last known HasTransactions status.
	/// </summary>
	public bool HasTransactions
	{
		get
		{
			if (Connection == null || Transaction == null)
			{
				_HasTransactions = false;
				return false;
			}

			if (!_HasTransactions.HasValue)
				return LiveTransactions;

			return _HasTransactions.Value;
		}
	}


	/// <summary>
	/// HasTransactions OnQueryStatus() requests piggyback off of previous
	/// <see cref="GetUpdatedTtsStatus()"/> calls.
	/// This reduces the number of IDbTransaction.HasTransactions() requests.
	/// </summary>
	public bool LiveTransactions
	{
		get
		{
			if (Connection == null)
			{
				_HasTransactions = false;
				return false;
			}


			bool hasTransactions;

			try
			{
				hasTransactions = _MdlCsb != null && _MdlCsb.HasTransactions;
			}
			catch
			{
				hasTransactions = false;
			}

			_HasTransactions = hasTransactions;

			return _HasTransactions.Value;
		}
		set
		{
			_HasTransactions = value;
		}
	}

	public bool PeekTransactions => _MdlCsb?.PeekTransactions ?? false;


	public string CurrentDatasetKey => _MdlCsb?.DatasetKey;



	/// <summary>
	/// Gets the current DataConnectiom. If it's null attempts to create it
	/// using the most recent ConnectionCsb info.
	/// </summary>
	public IDbConnection LiveConnection
	{
		get
		{
			lock (_LockObject)
			{
				if (Connection == null || _RctStamp == RctManager.Stamp)
				{
					_RctStamp = RctManager.Stamp;
					return Connection;
				}

				IDbConnection connection = _MdlCsb.LiveConnection;

				if (connection != null)
				{
					_RctStamp = RctManager.Stamp;
					return connection;
				}

				if (!RctManager.Loaded)
					return connection;

				// If we're here it's a reset.


				Csb csaRegistered = RctManager.CloneRegistered(_MdlCsb);
				ModelCsb mdlCsb = new(csaRegistered);

				SetDatabaseCsb(mdlCsb, true);

				return Connection;
			}
		}
	}


	public Version ServerVersion => _MdlCsb?.ServerVersion;


	public virtual Color StatusBarColor
	{
		get
		{
			IBsModelCsb modelCsb = MdlCsb;
			Color result = PersistentSettings.EditorStatusBarBackgroundColor;
			if (modelCsb != null && UseCustomColor(modelCsb))
			{
				result = GetCustomColor(modelCsb);
			}

			return result;
		}
	}


	public IDbTransaction Transaction => _MdlCsb?.DataTransaction;


	public bool TtsActive => Transaction != null;




	public event ConnectionChangedDelegate ConnectionChangedEvent
	{
		add { _ConnectionChangedEvent += value; }
		remove { _ConnectionChangedEvent -= value; }
	}


	public event StateChangeEventHandler ConnectionStateChangedEvent
	{
		add { _ConnectionStateChangedEvent += value; }
		remove { _ConnectionStateChangedEvent -= value; }
	}


	public event DatabaseChangeEventHandler DatabaseChangedEvent
	{
		add { _DatabaseChangedEvent += value; }
		remove { _DatabaseChangedEvent -= value; }
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AbstractConnectionStrategy
	// =========================================================================================================





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the IsComplete ConnectionInfo object for a connection. If the object is not
	/// complete but publicly complete calls the password prompt dialog. If the object
	/// does not exist or is not publicly complete, calls the connection dialog.
	/// </summary>
	/// <returns>Returns a valid ModelCsb else null if the user cancelled.</returns>
	// ---------------------------------------------------------------------------------
	private async Task<IBsModelCsb> AcquireValidConnectionInfoAsync(CancellationToken cancelToken)
	{
		bool isComplete = MdlCsb != null && MdlCsb.IsComplete;

		if (isComplete)
			return MdlCsb;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		if (cancelToken.IsCancellationRequested)
			return null;


		bool isCompletePublic = MdlCsb?.IsCompletePublic ?? false;
		IBsModelCsb mdlCsb;

		if (isCompletePublic)
			mdlCsb = PromptForCompleteConnection();
		else
			mdlCsb = PromptForConnection();

		return mdlCsb;
	}



	public void BeginTransaction(IsolationLevel isolationLevel) => _MdlCsb.BeginTransaction(isolationLevel);


	public bool CloseConnection() => _MdlCsb == null || _MdlCsb.CloseConnection();


	public abstract bool CommitTransactions(bool validate);



	public void DisposeTransaction() => _MdlCsb?.DisposeTransaction();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enusres a valid connecion. Returns the verified open connection else null if
	/// the user was prompted and cancelled.
	/// If an IsComplete connection could not be created/opened/verified, throws an
	/// exception.
	/// </summary>
	/// <returns>
	/// Returns a verified open connection.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public async Task<IDbConnection> EnsureVerifiedOpenConnectionAsync(CancellationToken cancelToken)
	{
		bool isOpen;
		bool hasTransactions;


		if (MdlCsb != null && MdlCsb.IsComplete)
		{
			try
			{
				lock (_LockObject)
				{
					if (Connection == null)
						MdlCsb.CreateDataConnection();
				}

				(isOpen, hasTransactions) = await MdlCsb.OpenOrVerifyConnectionAsync(cancelToken);
			}
			catch (Exception ex)
			{
				_HasTransactions = false;
				Diag.Expected(ex);
				throw;
			}

			if (!cancelToken.IsCancellationRequested)
				_HasTransactions = hasTransactions;

			return Connection;
		}


		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "EnsureConnection", "Connection is null or not open");

		_HasTransactions = false;

		IBsModelCsb mdlCsb = await AcquireValidConnectionInfoAsync(cancelToken);


		if (mdlCsb == null)
			return null;


		// We have a new MdlCsb.

		lock (_LockObject)
		{
			SetDatabaseCsb(mdlCsb, true);
		}

		try
		{
			(isOpen, hasTransactions) = await _MdlCsb.OpenOrVerifyConnectionAsync(cancelToken);
		}
		catch (Exception ex)
		{
			_HasTransactions = false;
			Diag.Debug(ex);
			throw;
		}

		if (cancelToken.IsCancellationRequested)
			return Connection;

		_HasTransactions = hasTransactions;

		RaiseInvalidateToolbar();

		return Connection;
	}



	private static Color GetCustomColor(IBsModelCsb ci)
	{
		Color result = _SDefaultColor;
		/*
		if (ci != null)
		{
			object obj = ci.AdvancedOptions["CUSTOM_CONNECTION_COLOR"];
			if (obj != null)
			{
				if (obj is string text)
				{
					if (int.TryParse(text, out int result2))
					{
						result = Color.FromArgb(result2);
					}
				}
			}
		}
		*/
		return result;
	}






	public virtual string GetCustomQuerySuccessMessage()
	{
		return null;
	}



	public virtual string GetEditorCaption(bool ignoreSettings)
	{
		StringBuilder stringBuilder = new StringBuilder(string.Empty, 80);
		if (PersistentSettings.EditorStatusTabTextIncludeServerName || ignoreSettings)
		{
			stringBuilder.Append(DisplayServerName);
		}

		if (PersistentSettings.EditorStatusTabTextIncludeDatabaseName || ignoreSettings)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Append(".");
			}

			stringBuilder.Append(DatasetDisplayName);
		}

		if ((PersistentSettings.EditorStatusTabTextIncludeLoginName || ignoreSettings) && !string.IsNullOrEmpty(DisplayUserName))
		{
			if (stringBuilder.Length != 0)
			{
				if (ignoreSettings)
				{
					stringBuilder.Append(Environment.NewLine);
				}
				else
				{
					stringBuilder.Append(" ");
				}
			}

			stringBuilder.AppendFormat("({0})", DisplayUserName);
		}

		return stringBuilder.ToString();
	}



	public virtual int GetExecutionTimeout()
	{
		int result = 0;

		if (MdlCsb != null)
			return MdlCsb.CommandTimeout;

		return result;
	}



	public virtual object GetPropertiesWindowDisplayObject()
	{
		return null;
	}



	public bool ModifyConnection()
	{
		lock (_LockObject)
		{
			IBsModelCsb mdlCsb = PromptForConnection();

			if (mdlCsb == null)
				return false;

			SetDatabaseCsb(mdlCsb, true);

			_MdlCsb.OpenOrVerifyConnection();

			return true;
		}
	}



	/// <summary>
	/// Prompts for user connection info.
	/// </summary>
	/// <returns>Returns the new complete ModelCsb else null if the user cancelled.</returns>
	private IBsModelCsb PromptForCompleteConnection()
	{
		Diag.ThrowIfNotOnUIThread();

		// Tracer.Trace(typeof(AbstractConnectionStrategy), "PromptForCompleteConnection()");

		if (_MdlCsb == null)
			return null;


		IBsDataConnectionPromptDialogHandler connectionDialogHandler = ApcManager.EnsureService<IBsDataConnectionPromptDialogHandler>();

		using (connectionDialogHandler)
		{
			try
			{

				connectionDialogHandler.PublicConnectionString = LiveConnection.ConnectionString;

				if (connectionDialogHandler.ShowDialog() && !RctManager.ShutdownState)
				{
					string connectionString = connectionDialogHandler.CompleteConnectionString;

					if (Csb.GetIsComplete(connectionString))
					{
						// Tracer.Trace(typeof(AbstractConnectionStrategy), "PromptForCompleteConnection()", "ConnectionString result: {0}.", connectionString);
						connectionString = RctManager.AdornConnectionStringFromRegistration(connectionString);

						RctManager.UpdateRegisteredConnection(connectionString, EnConnectionSource.Session, true);

						Csb csb = RctManager.CloneRegistered(connectionString, EnRctKeyType.ConnectionString);

						return new ModelCsb(csb);

					}
				}
			}
			catch (Exception ex)
			{
				Diag.Expected(ex);
			}
		}

		return null;
	}



	private IBsModelCsb PromptForConnection()
	{
		Diag.ThrowIfNotOnUIThread();

		// Tracer.Trace(typeof(AbstractConnectionStrategy), "PromptForConnection()");

		IBsDataConnectionDlgHandler connectionDialogHandler = ApcManager.EnsureService<IBsDataConnectionDlgHandler>();

		using (connectionDialogHandler)
		{
			try
			{
				connectionDialogHandler.AddSources(new Guid(VS.AdoDotNetTechnologyGuid));

				if (MdlCsb != null)
				{
					if (RctManager.ShutdownState)
						return null;

					string connectionString = RctManager.AdornConnectionStringFromRegistration(MdlCsb.ConnectionString);

					connectionDialogHandler.Title = ControlsResources.Strategy_ConnectionPromptModify;
					if (!string.IsNullOrEmpty(connectionString))
						connectionDialogHandler.EncryptedConnectionString = DataProtection.EncryptString(connectionString);
				}
				else
				{
					connectionDialogHandler.Title = ControlsResources.Strategy_ConnectionPromptConfigure;
				}

				if (connectionDialogHandler.ShowDialog())
				{
					string connectionString = DataProtection.DecryptString(connectionDialogHandler.EncryptedConnectionString);

					if (RctManager.ShutdownState)
						return null;

					Csb csb = RctManager.CloneRegistered(connectionString, EnRctKeyType.ConnectionString);
					return new ModelCsb(csb);
				}
			}
			catch (Exception ex)
			{
				Diag.Expected(ex);
			}
		}

		return null;
	}



	public virtual void ResetConnection()
	{
		lock (_LockObject)
		{
			_RctStamp = long.MinValue;
			SetDatabaseCsb(null, false);
		}
	}



	public abstract bool RollbackTransactions(bool validate);



	protected void SetDatabaseCsb(IBsModelCsb mdlCsb, bool createConnection)
	{
		lock (_LockObject)
		{
			_RctStamp = RctManager.Stamp;

			if (ReferenceEquals(_MdlCsb, mdlCsb))
				return;

			ConnectionChangedEventArgs args;
			IBsModelCsb previousMdlCsb = _MdlCsb;

			/*
			if (previousMdlCsb != null)
			{
				previousMdlCsb.ConnectionChangedEvent -= OnConnectionChanged;

				if (previousMdlCsb.DataConnection != null)
				{
					args = new(previousMdlCsb?.DataConnection, null);
					OnConnectionChanged(this, args);
				}
			}
			*/

			// Sanity check.
			if (mdlCsb == null)
				createConnection = false;

			if (createConnection && mdlCsb.DataConnection == null)
				mdlCsb.CreateDataConnection();

			_MdlCsb = mdlCsb;

			_DatabaseChangedEvent?.Invoke(this, new(mdlCsb, previousMdlCsb));

			if (_MdlCsb != null)
			{
				if (_MdlCsb.DataConnection != null)
				{
					args = new(_MdlCsb.DataConnection, null);

					OnConnectionChanged(this, args);
				}

				_MdlCsb.ConnectionChangedEvent += OnConnectionChanged;
			}


			EventEnter(false, true);

			try
			{
				previousMdlCsb?.Dispose();
			}
			finally
			{
				EventExit();
			}
		}
	}



	private static bool UseCustomColor(IBsModelCsb ci)
	{
		return false;
		/*
		bool result = false;
		if (ci != null)
		{
			object obj = ci.AdvancedOptions["USE_CUSTOM_CONNECTION_COLOR"];
			if (obj != null)
			{
				if (obj is string text && bool.TryParse(text, out var result2))
				{
					result = result2;
				}
			}
		}

		return result;
		*/
	}



	public async Task<bool> VerifyOpenConnectionAsync(CancellationToken cancelToken)
	{
		bool isOpen;
		bool liveTransactions;

		try
		{
			// If we HadTransactions validate them.
			(isOpen, liveTransactions) = await MdlCsb.OpenOrVerifyConnectionAsync(cancelToken);
		}
		catch (Exception ex)
		{
			liveTransactions = false;
			Diag.Expected(ex);
		}

		if (!cancelToken.IsCancellationRequested)
			LiveTransactions = liveTransactions;

		return true;
	}

	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstractConnectionStrategy
	// =========================================================================================================




	// -------------------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventCardinal"/> counter when execution enters an event
	/// handler to prevent recursion.
	/// </summary>
	/// <returns>
	/// Returns false if an event has already been entered else true if it is safe to enter.
	/// </returns>
	// -------------------------------------------------------------------------------------------
	public bool EventEnter(bool test = false, bool force = false)
	{
		lock (_LockObject)
		{
			if (_EventCardinal != 0 && !force)
				return false;

			if (!test)
				_EventCardinal++;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventCardinal"/> counter that was previously incremented
	/// by <see cref="EventEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------------
	public void EventExit()
	{
		lock (_LockObject)
		{
			if (_EventCardinal == 0)
				Diag.Dug(new InvalidOperationException(Resources.ExEventsAlreadyEnabled));
			else
				_EventCardinal--;
		}
	}



	private void OnConnectionStateChanged(object sender, StateChangeEventArgs args)
	{
		_ConnectionStateChangedEvent?.Invoke(sender, args);
	}


	private void OnConnectionChanged(object sender, ConnectionChangedEventArgs args)
	{
		// Tracer.Trace(GetType(), "OnPropertyAgentConnectionChanged()", "Current: {0}, Previous: {1}.", args.CurrentConnection != null, args.PreviousConnection != null);

		if (!EventEnter())
			return;

		try
		{
			lock (_LockObject)
			{
				if (args.PreviousConnection is DbConnection previousConnection)
				{
					previousConnection.StateChange -= OnConnectionStateChanged;
				}

				if (args.CurrentConnection is DbConnection currentConnection)
				{
					currentConnection.StateChange += OnConnectionStateChanged;
					StateChangeEventArgs sargs = new(currentConnection.State, ConnectionState.Closed);
					_ConnectionStateChangedEvent?.Invoke(sender, sargs);
				}

				_ConnectionChangedEvent?.Invoke(this, args);
			}
		}
		finally
		{
			EventExit();
		}
	}



	protected abstract void RaiseInvalidateToolbar();


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - AbstractConnectionStrategy
	// =========================================================================================================


	protected abstract bool RaiseNotifyConnectionState(EnNotifyConnectionState state, bool ttsDiscarded);


	#endregion Sub-Classes

}
