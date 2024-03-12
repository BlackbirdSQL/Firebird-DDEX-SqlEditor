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
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Model;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.Common.Model;


public abstract class AbstractConnectionStrategy : IDisposable
{
	protected CsbAgent _Csa = null;
	protected long _Seed = -1;
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
				CsbAgent csa;

				if (Connection != null)
				{
					csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(Connection);
					return csa == null
						? CoreConstants.C_DefaultExDatasetId
						: (string.IsNullOrWhiteSpace(csa.DatasetId)
							? csa.Dataset : csa.DatasetId);
				}

				if (ConnectionInfo != null)
				{
					csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(ConnectionInfo);
					return csa == null
						? CoreConstants.C_DefaultExDatasetId
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
				if (_Connection.State != 0)
					_Connection.Close();

				_Connection.Dispose();
			}

			_Connection = value;

			if (_Connection != null)
			{
				CsbAgent csa = RctManager.CloneRegistered(_Connection);
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

		lock (_LockObject)
		{
			FbConnection conn = (FbConnection)connection;

			// Tracer.Trace(typeof(SqlConnectionStrategy), "ApplyConnectionOptions()", "starting");

			FbScript script = s.CommandBuilder(s.EditorExecutionSetRowCount.SqlCmd(), s.EditorExecutionSetBlobDisplay.SqlCmd(),
				s.EditorExecutionSetCount.SqlCmd(), s.EditorExecutionLockTimeout.SqlCmd(), s.EditorExecutionSetBail.SqlCmd(),
				s.EditorExecutionSuppressHeaders.SqlCmd(), s.EditorExecutionSetNoExec.SqlCmd(), s.EditorExecutionSetStats.SqlCmd(),
				s.EditorExecutionSetWarnings.SqlCmd());



			FbBatchExecution fbe = null;

			try
			{
				fbe = new FbBatchExecution(conn);
				script.UnknownStatement += OnUnknownStatement;

				// None of these isql commands are supported by the Firebird client so disabled until they are.
				// script.Parse();
				// fbe.AppendSqlStatements(script);
				// fbe.Execute();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex, script.ToString());
				StringBuilder sb = new StringBuilder(100);
				sb.AppendFormat(ControlsResources.UnableToApplyConnectionSettings, ex.Message);
				MessageCtl.ShowEx(sb.ToString(), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			finally
			{
				if (fbe != null)
				{
					// fbe.Dispose();
					fbe = null;
				}
			}
		}

	}

	private void OnUnknownStatement(object sender, UnknownStatementEventArgs e)
	{
		// e.Ignore = true;
		DataException ex = new($"Firebird unrecognised statement has been ignored: {e.Statement.Text}.");
		Diag.Dug(ex);
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

				ConnectionInfo = ci;
				SetDbConnection(connection);
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

					ConnectionInfo = ci;
					SetDbConnection(connection);
				}
			}

			return Connection;
		}
	}

	public virtual void ResetConnection()
	{
		lock (_LockObject)
		{
			ConnectionInfo = null;
			SetDbConnection(null);
		}
	}

	private void OnConnectionChanged(IDbConnection previousConnection)
	{
		lock (_LockObject)
		{
			ConnectionChangedEventArgs args = new ConnectionChangedEventArgs(previousConnection);
			ConnectionChanged?.Invoke(this, args);
		}
	}

	private void OnConnectionChangedPriority(IDbConnection previousConnection)
	{
		lock (_LockObject)
		{
			ConnectionChangedEventArgs args = new ConnectionChangedEventArgs(previousConnection);
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
				_Csa = (CsbAgent)csb;
				_Seed = RctManager.Seed;

				if (csb == null || _Csa.DatasetKey != selectedDatasetKey)
				{
					_Csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(selectedDatasetKey);
					_Seed = RctManager.Seed;
				}

				if (_Csa != null)
				{
					if (Connection == null)
						SetDbConnection(new FbConnection(_Csa.ConnectionString));


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

					ConnectionInfo.Parse(_Csa);
					DatabaseChanged?.Invoke(this, new EventArgs());
					if (isOpen)
						Connection.Open();
				}
			}
		}
		catch (FbException ex)
		{
			Diag.Dug(ex);
			MessageCtl.ShowEx(ex, string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrDatabaseNotAccessible, selectedDatasetKey), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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

		((CsbAgent)scsb).Pooling = false;
	}


}
