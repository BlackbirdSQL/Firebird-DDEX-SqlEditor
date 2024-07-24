// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.SqlEditorStrategy

using System;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;



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



	protected virtual void Dispose(bool disposing)
	{
		lock (_LockObject)
			ConnInfo = null;
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - AbstractConnectionStrategy
	// =========================================================================================================


	// A protected 'this' object lock
	protected readonly object _LockObject = new object();

	private int _EventCardinal = 0;

	protected IBsConnectionInfo _ConnInfo;
	private bool? _HasTransactions;
	protected long _ConnectionStamp = -1;
	private int _TransactionCardinal = 0;

	private static readonly Color _SDefaultColor = SystemColors.Control;

	protected ConnectionChangedDelegate _ConnectionChangedEvent;
	protected DatabaseChangedDelegate _DatabaseChangedEvent;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractConnectionStrategy
	// =========================================================================================================


	public IDbConnection Connection => _ConnInfo?.DataConnection;

	public long ConnectionStamp => _ConnectionStamp;

	public IBsConnectionInfo ConnInfo
	{
		get
		{
			lock (_LockObject)
				return _ConnInfo;
		}
		set
		{
			lock (_LockObject)
			{
				_ConnectionStamp = RctManager.Stamp;

				if (ReferenceEquals(_ConnInfo, value))
					return;


				IBsConnectionInfo previousConnInfo = _ConnInfo;

				if (previousConnInfo != null)
					previousConnInfo.ConnectionChangedEvent -= OnPropertyAgentConnectionChanged;

				_ConnInfo = value;

				ConnectionChangedEventArgs args = new(_ConnInfo?.DataConnection, previousConnInfo?.DataConnection);

				OnPropertyAgentConnectionChanged(this, args);

				if (_ConnInfo != null)
					_ConnInfo.ConnectionChangedEvent += OnPropertyAgentConnectionChanged;

				EventEnter(true, true);

				try
				{
					previousConnInfo?.Dispose();
				}
				finally
				{
					EventExit();
				}
			}
		}
	}

	public virtual string DatasetId
	{
		get
		{
			lock (_LockObject)
			{
				if (ConnInfo != null)
				{
					Csb csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(ConnInfo);

					return csa == null
						? SysConstants.C_DefaultExDatasetId
						: (string.IsNullOrWhiteSpace(csa.DatasetId)
							? csa.Dataset : csa.DatasetId);
				}

				return string.Empty;
			}
		}
	}


	public virtual string DisplayServerName
	{
		get
		{
			IBsConnectionInfo connectionInfo = ConnInfo;
			if (connectionInfo != null && !string.IsNullOrEmpty(connectionInfo.DataSource))
			{
				return connectionInfo.DataSource;
			}

			return string.Empty;
		}
	}


	public virtual string DisplayUserName
	{
		get
		{
			string value = string.Empty;
			IBsConnectionInfo connectionInfo = ConnInfo;

			if (connectionInfo != null)
				value = connectionInfo.UserID;

			return value;
		}
	}


	/// <summary>
	/// Returns the last known HasTransactions status.
	/// </summary>
	public bool HadTransactions
	{
		get
		{
			if (Connection == null)
			{
				_HasTransactions = false;
				_TransactionCardinal = 0;
				return false;
			}

			if (!_HasTransactions.HasValue)
				return HasTransactions;

			return _HasTransactions.Value;
		}
	}


	/// <summary>
	/// HasTransactions OnQueryStatus() requests piggyback off of previous
	/// <see cref="GetUpdateTransactionsStatus()"/> calls.
	/// This reduces the number of IDbTransaction.HasTransactions() requests.
	/// </summary>
	public bool HasTransactions
	{
		get
		{
			if (Connection == null)
			{
				_HasTransactions = false;
				_TransactionCardinal = 0;
				return false;
			}

			_TransactionCardinal++;

			if (!_HasTransactions.HasValue
				|| (_TransactionCardinal % LibraryData.C_ConnectionValidationModulus) == 0)
			{
				GetUpdateTransactionsStatus(true);
			}

			return _HasTransactions.Value;
		}
	}


	public string CurrentDatasetKey => _ConnInfo?.DatasetKey;


	public IDbConnection LiveConnection
	{
		get
		{
			lock (_LockObject)
			{
				if (Connection == null || _ConnectionStamp == RctManager.Stamp)
				{
					_ConnectionStamp = RctManager.Stamp;
					return Connection;
				}

				IDbConnection connection = _ConnInfo.LiveConnection;

				if (connection != null)
				{
					_ConnectionStamp = RctManager.Stamp;
					return connection;
				}

				// If we're here it's a reset.

				Csb csaRegistered = RctManager.CloneRegistered(_ConnInfo);

				ConnInfo = (IBsConnectionInfo)new ConnectionInfoPropertyAgent();

				_ConnInfo.Parse(csaRegistered.ConnectionString);
				_ConnInfo.CreateDataConnection();

				return Connection;
			}
		}
	}


	public object LockObject => _LockObject;

	public virtual Color StatusBarColor
	{
		get
		{
			IBsConnectionInfo connectionInfo = ConnInfo;
			Color result = PersistentSettings.EditorStatusBarBackgroundColor;
			if (connectionInfo != null && UseCustomColor(connectionInfo))
			{
				result = GetCustomColor(connectionInfo);
			}

			return result;
		}
	}


	public IDbTransaction Transaction => _ConnInfo?.DataTransaction;


	public bool TtsActive => Transaction != null;




	public event ConnectionChangedDelegate ConnectionChangedEvent
	{
		add { _ConnectionChangedEvent += value; }
		remove { _ConnectionChangedEvent -= value; }
	}


	public event DatabaseChangedDelegate DatabaseChangedEvent
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
	/// <param name="ci">
	/// Outputs the new ConnInfo or null if ConnInfo is unchanged or the request was
	/// cancelled.
	/// </param>
	/// <returns>True is processing may continue else false.</returns>
	// ---------------------------------------------------------------------------------
	private bool AcquireValidConnectionInfo(out IBsConnectionInfo ci)
	{
		bool isComplete = ConnInfo != null && ConnInfo.IsComplete;

		if (isComplete)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "AcquireConnectionInfo", "ConnInfo is not null");
			ci = null;
			return true;
		}

		bool isCompletePublic = ConnInfo != null && ConnInfo.IsCompletePublic;

		if (isCompletePublic)
		{
			ci = PromptForCompleteConnection();
		}
		else
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "AcquireConnectionInfo", "ConnInfo is null. Prompting");

			ci = PromptForConnection();
		}


		if (ci == null)
			return false;


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the IsComplete ConnectionInfo object for a connection. If the object is not
	/// complete but publicly complete calls the password prompt dialog. If the object
	/// does not exist or is not publicly complete, calls the connection dialog.
	/// </summary>
	/// <param name="ci">
	/// Outputs the new ConnInfo or null if ConnInfo is unchanged or the request was
	/// cancelled.
	/// </param>
	/// <returns>True is processing may continue else false.</returns>
	// ---------------------------------------------------------------------------------
	private async Task<IBsConnectionInfo> AcquireValidConnectionInfoAsync()
	{
		bool isComplete = ConnInfo != null && ConnInfo.IsComplete;

		if (isComplete)
			return null;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


		bool isCompletePublic = ConnInfo != null && ConnInfo.IsCompletePublic;
		IBsConnectionInfo ci;

		if (isCompletePublic)
			ci = PromptForCompleteConnection();
		else
			ci = PromptForConnection();

		return ci;
	}



	public void BeginTransaction(IsolationLevel isolationLevel) => _ConnInfo.BeginTransaction(isolationLevel);


	public bool CloseConnection() => _ConnInfo == null || _ConnInfo.CloseConnection();


	public abstract bool CommitTransactions();



	public abstract IBsBatchExecutionHandler CreateBatchExecutionHandler();


	public void DisposeTransaction(bool force) => _ConnInfo?.DisposeTransaction(force);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the verified open connection else null if the user cancelled.
	/// If the connection could not be created/opened/verified, disposes of the
	/// connection then throws an exception.
	/// </summary>
	/// <returns>
	/// Returns a verified open connection else null.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public IDbConnection EnsureVerifiedOpenConnection()
	{
		if (ConnInfo != null && ConnInfo.IsComplete)
		{
			try
			{
				lock (_LockObject)
				{
					if (Connection == null)
						ConnInfo.CreateDataConnection();
					ConnInfo.OpenOrVerifyConnection();
				}
			}
			catch (Exception ex)
			{
				Diag.Expected(ex);
				throw;
			}

			return Connection;
		}


		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "EnsureConnection", "Connection is null or not open");

		if (!AcquireValidConnectionInfo(out IBsConnectionInfo ci))
			return null;


		if (ci != null)
		{
			// We have a new ConnInfo.

			lock (_LockObject)
			{
				ConnInfo = ci;

				_ConnInfo.CreateDataConnection();
				_ConnInfo.OpenOrVerifyConnection();
			}

		}

		return Connection;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the verified open connection else null if the user cancelled.
	/// If the connection could not be created/opened/verified, throws an exception.
	/// </summary>
	/// <returns>
	/// Returns a verified open connection.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public async Task<IDbConnection> EnsureVerifiedOpenConnectionAsync()
	{
		if (ConnInfo != null && ConnInfo.IsComplete)
		{
			try
			{
				lock (_LockObject)
				{
					if (Connection == null)
						ConnInfo.CreateDataConnection();
				}

				_ = await ConnInfo.OpenOrVerifyConnectionAsync();
			}
			catch (Exception ex)
			{
				Diag.Expected(ex);
				throw;
			}

			return Connection;
		}


		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "EnsureConnection", "Connection is null or not open");

		IBsConnectionInfo ci = await AcquireValidConnectionInfoAsync();

		if (ThreadHelper.CheckAccess())
		{
			Diag.Dug(new ApplicationException("Task has moved onto main thread"));
		}


		if (ci == null)
			return null;


		// We have a new ConnInfo.

		lock (_LockObject)
		{
			ConnInfo = ci;

			_ConnInfo.CreateDataConnection();
		}

		_ = await _ConnInfo.OpenOrVerifyConnectionAsync();

		return Connection;
	}



	private static Color GetCustomColor(IBsConnectionInfo ci)
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

			stringBuilder.Append(DatasetId);
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

		if (ConnInfo != null)
			return ConnInfo.CommandTimeout;

		return result;
	}



	public virtual object GetPropertiesWindowDisplayObject()
	{
		return null;
	}



	public abstract Version GetServerVersion();



	public bool GetUpdateTransactionsStatus(bool suppressExceptions)
	{
		bool hasTransactions = false;

		_TransactionCardinal = 0;

		try
		{
			hasTransactions = _ConnInfo != null && _ConnInfo.HasTransactions;
		}
		catch
		{
			if (!suppressExceptions)
			{
				_HasTransactions = false;
				throw;
			}
		}

		_HasTransactions = hasTransactions;

		return hasTransactions;
	}



	public bool ModifyConnection()
	{
		lock (_LockObject)
		{
			IBsConnectionInfo ci = PromptForConnection();

			if (ci == null)
				return false;

			ConnInfo = ci;

			_ConnInfo.CreateDataConnection();
			_ConnInfo.OpenOrVerifyConnection();

			return true;
		}
	}



	private IBsConnectionInfo PromptForCompleteConnection()
	{
		Diag.ThrowIfNotOnUIThread();

		// Tracer.Trace(typeof(AbstractConnectionStrategy), "PromptForCompleteConnection()");

		if (_ConnInfo == null)
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

					if (Csb.IsComplete(connectionString))
					{
						// Tracer.Trace(typeof(AbstractConnectionStrategy), "PromptForCompleteConnection()", "ConnectionString result: {0}.", connectionString);
						connectionString = RctManager.AdornConnectionStringFromRegistration(connectionString);

						RctManager.UpdateRegisteredConnection(connectionString, EnConnectionSource.Session, true);

						IBsConnectionInfo ci = (IBsConnectionInfo)new ConnectionInfoPropertyAgent();

						ci.Parse(connectionString);

						return ci;
					}
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}

		return null;
	}



	private IBsConnectionInfo PromptForConnection()
	{
		Diag.ThrowIfNotOnUIThread();

		// Tracer.Trace(typeof(AbstractConnectionStrategy), "PromptForConnection()");

		IBsDataConnectionDlgHandler connectionDialogHandler = ApcManager.EnsureService<IBsDataConnectionDlgHandler>();

		using (connectionDialogHandler)
		{
			try
			{
				connectionDialogHandler.AddSources(new Guid(VS.AdoDotNetTechnologyGuid));

				if (ConnInfo != null)
				{
					if (RctManager.ShutdownState)
						return null;

					string connectionString = RctManager.AdornConnectionStringFromRegistration(ConnInfo.ConnectionString);

					connectionDialogHandler.Title = "Modify SqlEditor Connection";
					if (!string.IsNullOrEmpty(connectionString))
						connectionDialogHandler.EncryptedConnectionString = DataProtection.EncryptString(connectionString);
				}
				else
				{
					connectionDialogHandler.Title = "Configure SqlEditor Connection";
				}

				if (connectionDialogHandler.ShowDialog())
				{
					string connectionString = DataProtection.DecryptString(connectionDialogHandler.EncryptedConnectionString);

					if (RctManager.ShutdownState)
						return null;

					IBsConnectionInfo ci = (IBsConnectionInfo)new ConnectionInfoPropertyAgent();
					ci.Parse(connectionString);

					return ci;
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}

		return null;
	}


	public abstract void ResetAndEnableConnectionStatistics();



	public virtual void ResetConnection()
	{
		lock (_LockObject)
		{
			_ConnectionStamp = -1;
			ConnInfo = null;
		}
	}



	public abstract bool RollbackTransactions();



	protected abstract void UpdateStateForCurrentConnection(ConnectionState currentState, ConnectionState previousState);


	private static bool UseCustomColor(IBsConnectionInfo ci)
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
	public bool EventEnter(bool increment = true, bool force = false)
	{
		lock (_LockObject)
		{
			if (_EventCardinal != 0 && !force)
				return false;

			if (increment)
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
		UpdateStateForCurrentConnection(args.CurrentState, args.OriginalState);
	}


	private void OnPropertyAgentConnectionChanged(object sender, ConnectionChangedEventArgs args)
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
					UpdateStateForCurrentConnection(currentConnection.State, ConnectionState.Closed);
				}

				_ConnectionChangedEvent?.Invoke(this, args);
			}
		}
		finally
		{
			EventExit();
		}
	}



	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - AbstractConnectionStrategy
	// =========================================================================================================


	public delegate void DatabaseChangedDelegate(object sender, EventArgs args);


	#endregion Sub-Classes

}
