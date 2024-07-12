// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.Strategy

using System;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.Shared.Model;


public abstract class AbstractConnectionStrategy : IDisposable
{

	public AbstractConnectionStrategy()
	{
	}

	protected Csb _Csa = null;
	protected long _Stamp = -1;
	protected long _ConnectionStamp = -1;
	protected string _LastDatasetKey = null;

	public delegate void ConnectionChangedEvent(object sender, ConnectionChangedEventArgs args);

	public delegate void DatabaseChangedEvent(object sender, EventArgs args);

	public class ConnectionChangedEventArgs(IDbConnection previousConnection) : EventArgs
	{
		public IDbConnection PreviousConnection { get; private set; } = previousConnection;
	}

	private static readonly Color DefaultColor = SystemColors.Control;

	private bool? _HasTransactions;
	private IDbConnection _Connection;
	private IDbTransaction _Transaction = null;
	private int _TransactionCardinal = 0;


	protected ConnectionPropertyAgent _ConnectionInfo;

	// A protected 'this' object lock
	protected readonly object _LockObject = new object();




	public string LastDatasetKey => _LastDatasetKey;


	public virtual ConnectionPropertyAgent ConnectionInfo
	{
		get
		{
			lock (_LockObject)
			{
				return _ConnectionInfo;
			}
		}
		protected set
		{
			lock (_LockObject)
			{
				if (_ConnectionInfo == value)
					return;

				_ConnectionInfo?.Dispose();
				_ConnectionInfo = value;
			}
		}
	}

	public IDbConnection Connection
	{
		get
		{
			lock (_LockObject)
			{
				if (_Connection == null || _ConnectionInfo == null || _ConnectionStamp == RctManager.Stamp)
					return _Connection;

				// We have to ensure the connection hasn't changed.

				// Get the connection string of the current connection adorned with the additional Csa properties
				// so that we don't get a negative equivalency because of missing stripped Csa properties in the
				// connection's connection string.
				string connectionString = RctManager.UpdateConnectionFromRegistration(_Connection);

				if (connectionString == null)
					return _Connection;

				Csb csaCurrent = new(connectionString, false);
				Csb csaRegistered = RctManager.CloneRegistered(_ConnectionInfo);

				// Compare the current connection with the registered connection.
				if (Csb.AreEquivalent(csaRegistered, csaCurrent, Csb.DescriberKeys))
				{ 
					// Nothing's changed.
					_ConnectionStamp = RctManager.Stamp;
					return _Connection;
				}

				// Tracer.Trace(GetType(), "get_Connection()", "Connections are not equivalent: \nCurrent: {0}\nRegistered: {1}",
				//	csaCurrent.ConnectionString, csaRegistered.ConnectionString);

				ConnectionPropertyAgent connectionInfo = new();
				connectionInfo.Parse(csaRegistered.ConnectionString);

				if (Csb.AreEquivalent(csaRegistered, csaCurrent, Csb.ConnectionKeys))
				{
					// The connection is the same but it's adornments have changed.
					lock (_LockObject)
					{
						_ConnectionStamp = RctManager.Stamp;
						ConnectionInfo = connectionInfo;
						return _Connection;
					}
				}

				// Tracer.Trace(GetType(), "get_Connection()", "Connections are not equivalent: \nCurrent: {0}\nRegistered: {1}",
				//	csaCurrent.ConnectionString, csaRegistered.ConnectionString);

				// If we're here it's a reset.

				IDbConnection connection = NativeDb.CreateDbConnection(csaRegistered.ConnectionString);

				if (_Connection.State == ConnectionState.Open)
					connection.Open();

				SetConnectionInfo(connectionInfo, connection);

				return _Connection;
			}
		}
	}

	public IDbTransaction Transaction
	{
		get
		{
			lock (_LockObject)
			{
				if (_Transaction != null)
				{
					bool transactionCompleted = true;

					try
					{
						transactionCompleted = _Connection == null || _Transaction.Completed();
					}
					catch (Exception ex)
					{
						Diag.Expected(ex);
					}


					if (transactionCompleted)
						DisposeTransaction(true);
				}

				return _Transaction;
			}
		}
		set
		{
			lock (_LockObject)
			{
				if (value == null)
					DisposeTransaction(false);

				_Transaction = value;
			}
		}
	}

	public virtual string DisplayServerName
	{
		get
		{
			ConnectionPropertyAgent connectionInfo = ConnectionInfo;
			if (connectionInfo != null && !string.IsNullOrEmpty(connectionInfo.DataSource))
			{
				return connectionInfo.ServerNameNoDot;
			}

			return string.Empty;
		}
	}

	public virtual string DatasetId
	{
		get
		{
			lock (_LockObject)
			{
				Csb csa;

				if (Connection != null)
				{
					csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(Connection);
					return csa == null
						? SysConstants.C_DefaultExDatasetId
						: (string.IsNullOrWhiteSpace(csa.DatasetId)
							? csa.Dataset : csa.DatasetId);
				}

				if (ConnectionInfo != null)
				{
					csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(ConnectionInfo);
					return csa == null
						? SysConstants.C_DefaultExDatasetId
						: (string.IsNullOrWhiteSpace(csa.DatasetId)
							? csa.Dataset : csa.DatasetId);
				}

				return string.Empty;
			}
		}
	}

	public virtual string DisplayUserName
	{
		get
		{
			string text = string.Empty;
			ConnectionPropertyAgent connectionInfo = ConnectionInfo;
			if (connectionInfo != null)
			{
				text = connectionInfo.UserID;
			}

			return text;
		}
	}


	public virtual Color StatusBarColor
	{
		get
		{
			ConnectionPropertyAgent connectionInfo = ConnectionInfo;
			Color result = PersistentSettings.EditorStatusBarBackgroundColor;
			if (connectionInfo != null && UseCustomColor(connectionInfo))
			{
				result = GetCustomColor(connectionInfo);
			}

			return result;
		}
	}

	public virtual bool IsExecutionPlanAndQueryStatsSupported => true;


	/// <summary>
	/// HasTransactions HandleQueryStatus() requests piggyback off of previous
	/// <see cref="GetUpdateTransactionsStatus()"/> calls.
	/// This reduces the number of IDbTransaction.HasTransactions() requests.
	/// </summary>
	public bool HasTransactions
	{
		get
		{
			_TransactionCardinal++;

			if (!_HasTransactions.HasValue
				|| (_TransactionCardinal % 20) == 0)
			{
				GetUpdateTransactionsStatus();
			}

			return _HasTransactions.Value;
		}
	}


	public bool TtsActive => Transaction != null;


	public event ConnectionChangedEvent ConnectionChanged;

	protected event ConnectionChangedEvent ConnectionChangedPriority;

	public event DatabaseChangedEvent DatabaseChanged;



	public void BeginTransaction(IsolationLevel isolationLevel)
	{
		if (Transaction != null)
			return;

		Transaction = Connection.BeginTransaction(isolationLevel);
	}

	public void CommitTransaction()
	{
		Transaction?.Commit();
	}

	public void RollbackTransaction()
	{
		Transaction?.Rollback();
	}


	public void DisposeTransaction(bool force)
	{
		if (_Transaction == null)
			return;

		bool hasTransactions = false;

		try
		{
			hasTransactions = _Transaction.HasTransactions();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);
		}



		try
		{
			if (!force && hasTransactions)
			{
				Diag.ThrowException(new DataException("Attempt to dispose of database Transaction object that has pending transactions."));
			}
		}
		catch { }

		try
		{
			if (hasTransactions)
				_Transaction.Rollback();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);
		}


		try
		{
			_Transaction.Dispose();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);
		}


		_Transaction = null;
	}


	protected void SetDbConnection(IDbConnection value)
	{
		lock (_LockObject)
		{
			IDbConnection connection = _Connection;

			if (connection == value)
				return;

			// If we're here and there are transactions nothing we can really do about it
			// because this could have been initiated from anywhere.
			DisposeTransaction(true);

			if (_Connection != null)
			{
				try
				{
					if (_Connection.State != ConnectionState.Closed)
						_Connection.Close();
				}
				catch (Exception ex)
				{
					Diag.Expected(ex);
				}


				try
				{
					_Connection.Dispose();
				}
				catch (Exception ex)
				{
					Diag.Expected(ex);
				}
			}


			_Connection = value;

			if (_Connection != null)
			{
				Csb csa = RctManager.CloneRegistered(_Connection);
				if (csa == null)
					return;

				_LastDatasetKey = csa.DatasetKey;
			}

			OnConnectionChangedPriority(connection);
			OnConnectionChanged(connection);
		}
	}

	public void SetConnectionInfo(ConnectionPropertyAgent ci)
	{
		CreateDbConnectionFromConnectionInfo(ci, false, out IDbConnection connection);
		SetConnectionInfo(ci, connection);
	}

	public void SetConnectionInfo(ConnectionPropertyAgent ci, IDbConnection connection)
	{
		if (ci != null && connection == null)
		{
			ArgumentNullException ex = new("connection");
			Diag.Dug(ex);
			throw ex;
		}

		lock (_LockObject)
		{
			_ConnectionStamp = RctManager.Stamp;
			ConnectionInfo = ci;
			SetDbConnection(connection);
		}
	}


	protected virtual bool AcquireConnectionInfo(bool tryOpenConnection, out ConnectionPropertyAgent ci, out IDbConnection connection)
	{
		ci = new ConnectionPropertyAgent();
		return CreateDbConnectionFromConnectionInfo(ci, tryOpenConnection, out connection);
	}

	protected virtual void ModifyConnectionInfo(bool tryOpenConnection, out ConnectionPropertyAgent ci, out IDbConnection connection)
	{
		AcquireConnectionInfo(tryOpenConnection, out ci, out connection);
	}

	protected abstract bool CreateDbConnectionFromConnectionInfo(ConnectionPropertyAgent ci, bool tryOpenConnection, out IDbConnection connection);


	public virtual int GetExecutionTimeout()
	{
		int result = 0;

		if (ConnectionInfo != null)
			return ConnectionInfo.CommandTimeout;

		return result;
	}


	public abstract bool CommitTransactions();

	public abstract bool RollbackTransactions();

	public IDbConnection EnsureConnection(bool tryOpenConnection)
	{
		lock (_LockObject)
		{
			bool result = true;

			if (Connection == null || (tryOpenConnection && Connection.State != ConnectionState.Open))
			{
				// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "EnsureConnection", "Connection is null or not open");
				result = AcquireConnectionInfo(tryOpenConnection, out ConnectionPropertyAgent ci, out IDbConnection connection);

				if (ci != null && connection == null)
					result = CreateDbConnectionFromConnectionInfo(ci, tryOpenConnection, out connection);

				SetConnectionInfo(ci, connection);
			}

			return result ? Connection : null;
		}
	}

	public IDbConnection ModifyConnection(bool tryOpenConnection)
	{
		lock (_LockObject)
		{
			if (tryOpenConnection)
			{
				ModifyConnectionInfo(tryOpenConnection, out var ci, out var connection);
				if (ci != null)
				{
					if (connection == null)
						CreateDbConnectionFromConnectionInfo(ci, false, out connection);

					SetConnectionInfo(ci, connection);
					return Connection;
				}
			}

			return null;
		}
	}

	public virtual void ResetConnection()
	{
		lock (_LockObject)
		{
			_ConnectionStamp = -1;
			ConnectionInfo = null;
			SetDbConnection(null);
		}
	}

	private void OnConnectionChanged(IDbConnection previousConnection)
	{
		lock (_LockObject)
		{
			ConnectionChangedEventArgs args = new (previousConnection);
			ConnectionChanged?.Invoke(this, args);
		}
	}

	private void OnConnectionChangedPriority(IDbConnection previousConnection)
	{
		lock (_LockObject)
		{
			ConnectionChangedEventArgs args = new (previousConnection);
			ConnectionChangedPriority?.Invoke(this, args);
		}
	}


	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		lock (_LockObject)
		{
			if (disposing && Connection != null)
			{
				DisposeTransaction(true);

				Connection.Close();
				Connection.Dispose();
				SetDbConnection(null);
			}
			_ConnectionInfo?.Dispose();
			_ConnectionInfo = null;
		}
	}

	public virtual void SetDatasetKeyOnConnection(string selectedDisplayName, DbConnectionStringBuilder csb)
	{
		try
		{
			lock (_LockObject)
			{
				_Csa = (Csb)csb;
				_Stamp = RctManager.Stamp;

				if (csb == null || _Csa.FullDisplayName != selectedDisplayName)
				{
					_Csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(selectedDisplayName, EnRctKeyType.DisplayName);
					_Stamp = RctManager.Stamp;
				}

				if (_Csa != null)
				{
					if (Connection == null)
						SetDbConnection(NativeDb.CreateDbConnection(_Csa.ConnectionString));


					bool isOpen = Connection.State == ConnectionState.Open;

					if (isOpen)
					{
						DisposeTransaction(false);
						Connection.Close();
					}

					Connection.ConnectionString = _Csa.ConnectionString;

					if (ConnectionInfo == null)
						ConnectionInfo = new();

					_ConnectionStamp = RctManager.Stamp;
					ConnectionInfo.Parse(_Csa);
					DatabaseChanged?.Invoke(this, new EventArgs());
					if (isOpen)
						Connection.Open();
				}
			}
		}
		catch (DbException ex)
		{
#if DEBUG
			Diag.Dug(ex);
#endif
			MessageCtl.ShowEx(ex, ControlsResources.ErrDatabaseNotAccessibleEx.FmtRes(selectedDisplayName), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public virtual object GetPropertiesWindowDisplayObject()
	{
		return null;
	}

	public abstract void ResetAndEnableConnectionStatistics();

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

	private static bool UseCustomColor(ConnectionPropertyAgent ci)
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

	private static Color GetCustomColor(ConnectionPropertyAgent ci)
	{
		Color result = DefaultColor;
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

	protected void CreateAndOpenConnectionWithCommonMessageLoop(ConnectionPropertyAgent ci, string connectingInfoMessage, string errorPrescription, out IDbConnection connection)
	{
		connection = null;
		IDbConnection testConnection = null;
		Exception exception = null;
		ManualResetEvent resetEvent = new ManualResetEvent(initialState: false);
		Action action = delegate
		{
			try
			{
				CreateDbConnectionFromConnectionInfo(ci, false, out testConnection);
				testConnection.Open();
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			finally
			{
				resetEvent.Set();
			}
		};
		CommonMessagePump obj = new CommonMessagePump
		{
			AllowCancel = true,
			EnableRealProgress = false,
			Timeout = TimeSpan.MaxValue,
			WaitTitle = ControlsResources.CommonMessageLoopConnecting
		};
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.CommonMessageLoopAttemptingToConnect, ci.DataSource));
		if (connectingInfoMessage != null)
		{
			stringBuilder.Append(Environment.NewLine + Environment.NewLine);
			stringBuilder.Append(connectingInfoMessage);
		}

		obj.WaitText = stringBuilder.ToString();
		action.BeginInvoke(null, null);
		if (obj.ModalWaitForHandles(resetEvent) != CommonMessagePumpExitCode.HandleSignaled)
		{
			((Action)delegate
			{
				int connectionTimeout = 15; // Ns2.SqlServerConnectionService.GetConnectionTimeout(ci);
				if (resetEvent.WaitOne(2 * connectionTimeout) && testConnection != null)
				{
					testConnection.Close();
					testConnection.Dispose();
				}
			}).BeginInvoke(null, null);
		}
		else
		{
			connection = testConnection;
		}

		if (exception == null)
		{
			return;
		}

#if DEBUG
		Diag.Dug(exception);
#endif


		if (!UnsafeCmd.IsInAutomationFunction())
		{
			string value = string.Format(CultureInfo.CurrentCulture, ControlsResources.CommonMessageLoopFailedToOpenConnection, ci.DataSource);
			string value2 = string.Format(CultureInfo.CurrentCulture, ControlsResources.CommonMessageLoopErrorMessage, exception.Message);
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append(value);
			stringBuilder2.Append(Environment.NewLine + Environment.NewLine);
			if (!string.IsNullOrEmpty(errorPrescription))
			{
				stringBuilder2.Append(errorPrescription);
				stringBuilder2.Append(Environment.NewLine + Environment.NewLine);
			}

			stringBuilder2.Append(value2);
			MessageCtl.ShowEx(stringBuilder2.ToString(), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public virtual string GetCustomQuerySuccessMessage()
	{
		return null;
	}

	public abstract Version GetServerVersion();

	public abstract string GetProductLevel();

	public abstract IBBatchExecutionHandler CreateBatchExecutionHandler();


	public bool GetUpdateTransactionsStatus()
	{
		bool hasTransactions = false;

		try
		{
			hasTransactions = Transaction != null && _Transaction.HasTransactions();
		}
		catch
		{
			DisposeTransaction(true);
		}

		_TransactionCardinal = 0;
		_HasTransactions = hasTransactions;

		return hasTransactions;
	}



	public static void PopulateConnectionStringBuilder(DbConnectionStringBuilder scsb, ConnectionPropertyAgent connectionInfo)
	{
		if (connectionInfo.Database != null)
		{
			connectionInfo.PopulateConnectionStringBuilder(scsb, false);
			// scsb.TrustServerCertificate = SqlServerConnectionService.GetTrustServerCertificate(connectionInfo);
			// scsb.Encrypt = SqlServerConnectionService.GetEncryptConnection(connectionInfo);


			// bool flag = false;

			/*
			if (SqlAuthenticationMethodUtils.IsAuthenticationSupported())
			{
				if (connectionInfo.AuthenticationType == 2)
				{
					SqlAuthenticationMethodUtils.SetAuthentication(scsb, SqlConnectionInfo.AuthenticationMethod.ActiveDirectoryPassword.ToString());
				}
				else if (connectionInfo.AuthenticationType == 3)
				{
					 SqlAuthenticationMethodUtils.SetAuthentication(scsb, SqlConnectionInfo.AuthenticationMethod.ActiveDirectoryIntegrated.ToString());
					flag = true;
				}
				else if (connectionInfo.AuthenticationType == 5)
				{
					SqlAuthenticationMethodUtils.SetAuthentication(scsb, SqlConnectionInfo.AuthenticationMethod.ActiveDirectoryInteractive.ToString());
				}
			}
			*/

			/*
			if (!flag)
			{
				if (SqlServerConnectionService.IsWindowsAuthentication(connectionInfo))
				{
					scsb.IntegratedSecurity = true;
				}
				else
				{
					Scsb.UserID = connectionInfo.UserID;
					if (connectionInfo.AuthenticationType != 5)
					{
						scsb.Password = connectionInfo.Password;
					}

					scsb.PersistSecurityInfo = connectionInfo.PersistPassword;
				}
			}
			*/

		}

		// ((Csb)scsb).Pooling = false;
	}


}
