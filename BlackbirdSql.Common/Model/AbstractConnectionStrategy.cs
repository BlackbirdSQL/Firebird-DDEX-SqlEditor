// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.ConnectionStrategy

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls;
using BlackbirdSql.Core.Model;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.Common.Model;


public abstract class AbstractConnectionStrategy : IDisposable
{
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

	private IDbConnection _Connection;
	private IDbTransaction _Transaction = null;


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

				Csb csaCurrent = new(connectionString, false);
				Csb csaRegistered = RctManager.CloneRegistered(_ConnectionInfo);

				// Compare the current connection with the registered connection.
				if (Csb.AreEquivalent(csaRegistered, csaCurrent, Csb.DescriberKeys))
				{
					_ConnectionStamp = RctManager.Stamp;
					return _Connection;
				}

				// Tracer.Trace(GetType(), "get_Connection()", "Connections are not equivalent: \nCurrent: {0}\nRegistered: {1}",
				//	csaCurrent.ConnectionString, csaRegistered.ConnectionString);

				ConnectionPropertyAgent connectionInfo = new();
				connectionInfo.Parse(csaRegistered.ConnectionString);
				IDbConnection connection = DbNative.CreateDbConnection(csaRegistered.ConnectionString);

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
					bool transactionCompleted = _Connection == null || _Transaction.Completed();

					if (transactionCompleted)
					{
						_Transaction.Dispose();
						_Transaction = null;
					}
				}

				return _Transaction;
			}
		}
		set
		{
			lock (_LockObject)
			{
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

	public abstract bool TtsActive { get; }

	public abstract bool HasTransactions { get; }

	public event ConnectionChangedEvent ConnectionChanged;

	protected event ConnectionChangedEvent ConnectionChangedPriority;

	public event DatabaseChangedEvent DatabaseChanged;

	public AbstractConnectionStrategy()
	{
	}


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


	public void DisposeTransaction()
	{
		_Transaction?.Dispose();
		_Transaction = null;
	}


	protected void SetDbConnection(IDbConnection value)
	{
		lock (_LockObject)
		{
			IDbConnection connection = _Connection;

			if (connection == value)
				return;

			_Transaction?.Dispose();
			_Transaction = null;

			if (_Connection != null)
			{
				if (_Connection.State != ConnectionState.Closed)
					_Connection.Close();
				DisposeTransaction();
				_Connection.Dispose();
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
		IDbConnection connection = CreateDbConnectionFromConnectionInfo(ci, tryOpenConnection: false);
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


	protected virtual void AcquireConnectionInfo(bool tryOpenConnection, out ConnectionPropertyAgent ci, out IDbConnection connection)
	{
		ci = new ConnectionPropertyAgent();
		connection = CreateDbConnectionFromConnectionInfo(ci, tryOpenConnection);
	}

	protected virtual void ChangeConnectionInfo(bool tryOpenConnection, out ConnectionPropertyAgent ci, out IDbConnection connection)
	{
		AcquireConnectionInfo(tryOpenConnection, out ci, out connection);
	}

	protected abstract IDbConnection CreateDbConnectionFromConnectionInfo(ConnectionPropertyAgent ci, bool tryOpenConnection);

	public virtual void ApplyConnectionOptions(IDbConnection connection, IBEditorTransientSettings s)
	{

		/*

		lock (_LockObject)
		{

			// Tracer.Trace(typeof(SqlConnectionStrategy), "ApplyConnectionOptions()", "starting");

			List<string> statements = [s.EditorExecutionSetRowCount.SqlCmd(), s.EditorExecutionSetBlobDisplay.SqlCmd(),
				s.EditorExecutionSetCount.SqlCmd(), s.EditorExecutionLockTimeout.SqlCmd(), s.EditorExecutionSetBail.SqlCmd(),
				s.EditorExecutionSuppressHeaders.SqlCmd(), s.EditorExecutionSetNoExec.SqlCmd(), s.EditorExecutionSetStats.SqlCmd(),
				s.EditorExecutionSetWarnings.SqlCmd()];



			object fbe = null;

			try
			{
				fbe = DbNative.CreateBatchExecutionObject(connection, statements);

				// DbNative.ExecuteBatch(connection, fbe, true);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				MessageCtl.ShowEx(ex, "Failed to execute batch", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		*/

	}

	public virtual int GetExecutionTimeout()
	{
		int result = 0;

		if (ConnectionInfo != null)
			return ConnectionInfo.CommandTimeout;

		return result;
	}


	public abstract bool CommitTransactions();

	public abstract void RollbackTransactions();

	public IDbConnection EnsureConnection(bool tryOpenConnection)
	{
		lock (_LockObject)
		{
			if (Connection == null || (tryOpenConnection && Connection.State != ConnectionState.Open))
			{
				// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "EnsureConnection", "Connection is null or not open");
				AcquireConnectionInfo(tryOpenConnection, out var ci, out var connection);

				if (ci != null && connection == null)
					connection = CreateDbConnectionFromConnectionInfo(ci, tryOpenConnection);

				SetConnectionInfo(ci, connection);
			}

			return Connection;
		}
	}

	public IDbConnection ChangeConnection(bool tryOpenConnection)
	{
		lock (_LockObject)
		{
			if (tryOpenConnection)
			{
				ChangeConnectionInfo(tryOpenConnection, out var ci, out var connection);
				if (ci != null)
				{
					connection ??= CreateDbConnectionFromConnectionInfo(ci, tryOpenConnection: false);

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

	public abstract List<string> GetAvailableDatabases();

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
				if (_Transaction != null)
				{
					_Transaction.Dispose();
					_Transaction = null;
				}
				Connection.Close();
				Connection.Dispose();
				SetDbConnection(null);
			}
		}
	}

	public virtual void SetDatasetKeyOnConnection(string selectedDatasetKey, DbConnectionStringBuilder csb)
	{
		try
		{
			lock (_LockObject)
			{
				_Csa = (Csb)csb;
				_Stamp = RctManager.Stamp;

				if (csb == null || _Csa.DatasetKey != selectedDatasetKey)
				{
					_Csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(selectedDatasetKey);
					_Stamp = RctManager.Stamp;
				}

				if (_Csa != null)
				{
					if (Connection == null)
						SetDbConnection(DbNative.CreateDbConnection(_Csa.ConnectionString));


					bool isOpen = Connection.State == ConnectionState.Open;
					if (isOpen)
					{
						_Transaction?.Dispose();
						_Transaction = null;
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
			Diag.Dug(ex);
			MessageCtl.ShowEx(ex, ControlsResources.ErrDatabaseNotAccessible.FmtRes(selectedDatasetKey), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
				testConnection = CreateDbConnectionFromConnectionInfo(ci, false);
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

		Diag.Dug(exception);

		if (!Cmd.IsInAutomationFunction())
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

		((Csb)scsb).Pooling = false;
	}


}
