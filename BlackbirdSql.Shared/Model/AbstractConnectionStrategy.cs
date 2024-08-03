// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.SqlEditorStrategy

using System;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
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



	protected virtual bool Dispose(bool disposing)
	{
		if (!disposing || _Disposed)
			return false;

		_Disposed = true;

		lock (_LockObject)
			LiveMdlCsb = null;

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
	private int _TransactionCardinal = 0;

	private static readonly Color _SDefaultColor = SystemColors.Control;

	protected ConnectionChangedDelegate _ConnectionChangedEvent;
	protected DatabaseChangedDelegate _DatabaseChangedEvent;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractConnectionStrategy
	// =========================================================================================================


	public virtual string AdornedDisplayName =>
		MdlCsb?.AdornedDisplayName ?? string.Empty;

	public string AdornedQualifiedName =>
		MdlCsb?.AdornedQualifiedName ?? string.Empty;

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
				if (_MdlCsb == null || !RaiseNotifyConnectionState(EnNotifyConnectionState.RequestIsUnlocked, false) || HasTransactions)
					return _MdlCsb;

				if (RctManager.Loaded && _MdlCsb.IsInvalidated)
				{
					Csb csb = RctManager.CloneRegistered(_MdlCsb);
					LiveMdlCsb = new ModelCsb(csb);
				}
				return _MdlCsb;
			}
		}
		set
		{
			lock (_LockObject)
			{
				_RctStamp = RctManager.Stamp;

				if (ReferenceEquals(_MdlCsb, value))
					return;


				IBsModelCsb previousMdlCsb = _MdlCsb;

				if (previousMdlCsb != null)
					previousMdlCsb.ConnectionChangedEvent -= OnPropertyAgentConnectionChanged;

				_MdlCsb = value;

				ConnectionChangedEventArgs args = new(_MdlCsb?.DataConnection, previousMdlCsb?.DataConnection);

				OnPropertyAgentConnectionChanged(this, args);

				if (_MdlCsb != null)
					_MdlCsb.ConnectionChangedEvent += OnPropertyAgentConnectionChanged;

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
	/// <see cref="GetUpdatedTransactionsStatus()"/> calls.
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
				GetUpdatedTransactionsStatus(true);
			}

			return _HasTransactions.Value;
		}
	}


	public string CurrentDatasetKey => _MdlCsb?.DatasetKey;


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

				LiveMdlCsb = new ModelCsb(csaRegistered);
				_MdlCsb.CreateDataConnection();

				return Connection;
			}
		}
	}


	public Version ServerVersion => _MdlCsb?.ServerVersion ?? new ();


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
	/// Outputs the new MdlCsb or null if MdlCsb is unchanged or the request was
	/// cancelled.
	/// </param>
	/// <returns>True is processing may continue else false.</returns>
	// ---------------------------------------------------------------------------------
	private bool AcquireValidConnectionInfo(out IBsModelCsb mdlCsb)
	{
		bool isComplete = MdlCsb?.IsComplete ?? false;

		if (isComplete)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "AcquireConnectionInfo", "MdlCsb is not null");
			mdlCsb = null;
			return true;
		}

		bool isCompletePublic = MdlCsb != null && MdlCsb.IsCompletePublic;

		if (isCompletePublic)
		{
			mdlCsb = PromptForCompleteConnection();
		}
		else
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "AcquireConnectionInfo", "MdlCsb is null. Prompting");

			mdlCsb = PromptForConnection();
		}


		if (mdlCsb == null)
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
	/// Outputs the new MdlCsb or null if MdlCsb is unchanged or the request was
	/// cancelled.
	/// </param>
	/// <returns>True is processing may continue else false.</returns>
	// ---------------------------------------------------------------------------------
	private async Task<IBsModelCsb> AcquireValidConnectionInfoAsync()
	{
		bool isComplete = MdlCsb != null && MdlCsb.IsComplete;

		if (isComplete)
			return null;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


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



	public void DisposeTransaction(bool force) => _MdlCsb?.DisposeTransaction(force);



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
		if (MdlCsb != null && MdlCsb.IsComplete)
		{
			try
			{
				lock (_LockObject)
				{
					if (Connection == null)
						MdlCsb.CreateDataConnection();
					MdlCsb.OpenOrVerifyConnection();
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

		if (!AcquireValidConnectionInfo(out IBsModelCsb mdlCsb))
			return null;


		if (mdlCsb != null)
		{
			// We have a new MdlCsb.

			lock (_LockObject)
			{
				LiveMdlCsb = mdlCsb;

				_MdlCsb.CreateDataConnection();
				_MdlCsb.OpenOrVerifyConnection();
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
		if (MdlCsb != null && MdlCsb.IsComplete)
		{
			try
			{
				lock (_LockObject)
				{
					if (Connection == null)
						MdlCsb.CreateDataConnection();
				}

				_ = await MdlCsb.OpenOrVerifyConnectionAsync();
			}
			catch (Exception ex)
			{
				Diag.Expected(ex);
				throw;
			}

			return Connection;
		}


		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "EnsureConnection", "Connection is null or not open");

		IBsModelCsb mdlCsb = await AcquireValidConnectionInfoAsync();

		if (ThreadHelper.CheckAccess())
		{
			Diag.Dug(new ApplicationException("Task has moved onto main thread"));
		}


		if (mdlCsb == null)
			return null;


		// We have a new MdlCsb.

		lock (_LockObject)
		{
			LiveMdlCsb = mdlCsb;

			_MdlCsb.CreateDataConnection();
		}

		_ = await _MdlCsb.OpenOrVerifyConnectionAsync();

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





	public bool GetUpdatedTransactionsStatus(bool suppressExceptions)
	{
		bool hasTransactions = false;

		_TransactionCardinal = 0;

		try
		{
			hasTransactions = _MdlCsb != null && _MdlCsb.HasTransactions;
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
			IBsModelCsb mdlCsb = PromptForConnection();

			if (mdlCsb == null)
				return false;

			LiveMdlCsb = mdlCsb;

			_MdlCsb.CreateDataConnection();
			_MdlCsb.OpenOrVerifyConnection();

			return true;
		}
	}



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
				Diag.Dug(ex);
				throw;
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
				Diag.Dug(ex);
				throw;
			}
		}

		return null;
	}



	public virtual void ResetConnection()
	{
		lock (_LockObject)
		{
			_RctStamp = long.MinValue;
			LiveMdlCsb = null;
		}
	}



	public abstract bool RollbackTransactions(bool validate);



	protected abstract void UpdateStateForCurrentConnection(ConnectionState currentState, ConnectionState previousState);


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



	protected abstract void RaiseInvalidateToolbar();


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - AbstractConnectionStrategy
	// =========================================================================================================


	public delegate void DatabaseChangedDelegate(object sender, EventArgs args);


	protected abstract bool RaiseNotifyConnectionState(EnNotifyConnectionState state, bool ttsDiscarded);

	#endregion Sub-Classes

}
