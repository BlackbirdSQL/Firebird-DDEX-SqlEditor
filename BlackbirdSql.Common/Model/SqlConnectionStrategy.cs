#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Controls.PropertiesWindow;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;

namespace BlackbirdSql.Common.Model;


public class SqlConnectionStrategy : ConnectionStrategy
{
	private class SqlBatchExecutionHandler : IBatchExecutionHandler
	{
		public void Register(IDbConnection conn, IDbCommand command, QESQLBatch batch)
		{
			if (DbConnectionWrapper.IsSupportedConnection(conn))
			{
				new DbConnectionWrapper(conn).InfoMessageEvent += batch.OnSqlInfoMessage;
				if (DbCommandWrapper.IsSupportedCommand(command))
				{
					new DbCommandWrapper(command).StatementCompletedEvent += batch.OnSqlStatementCompleted;
				}
			}
		}

		public void UnRegister(IDbConnection conn, IDbCommand command, QESQLBatch batch)
		{
			if (DbConnectionWrapper.IsSupportedConnection(conn))
			{
				new DbConnectionWrapper(conn).InfoMessageEvent -= batch.OnSqlInfoMessage;
				if (DbCommandWrapper.IsSupportedCommand(command))
				{
					new DbCommandWrapper(command).StatementCompletedEvent -= batch.OnSqlStatementCompleted;
				}
				else
				{
					SqlTracer.AssertTraceEvent(condition: false, TraceEventType.Error, EnSqlTraceId.CoreServices, "Supported connection, but not supported command?");
				}
			}
		}
	}

	private bool? _IsCloudConnection;

	private bool? _IsDwConnection;

	private int? _Spid;

	private string _TracingId;

	private string _ProductLevel;

	public static readonly FbConnectionStringBuilder BuilderWithDefaultApplicationName = new("server=localhost;");

	public const string C_ApplicationName = LibraryData.ApplicationName;

	private ConnectedPropertiesWindow _propertiesWindowObject;

	// private static readonly string Master = "master";

	private static readonly string PdwExplainPattern = "(^|\\s|;)explain(\\s)+(select|insert|update|delete|create|drop|with)";

	private static readonly Regex PdwExplainRegex = new Regex(PdwExplainPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);



	public virtual bool IsCloudConnection
	{
		get
		{
			lock (_LockObject)
			{
				bool result = true;
				if (base.Connection != null /* && base.Connection.State == ConnectionState.Open */)
				{
					if (!_IsCloudConnection.HasValue)
					{
						// FbConnection sqlConnection = (FbConnection)Connection;
						try
						{
							FbConnectionStringBuilder csb = new(Connection.ConnectionString);
							_IsCloudConnection = result = csb.ServerType == FbServerType.Default;
						}
						catch (Exception e)
						{
							Tracer.LogExCatch(typeof(SqlConnectionStrategy), e);
						}
						/*
						finally
						{

							if (sqlConnection != null)
							{
								sqlConnection.Close();
								sqlConnection.Dispose();
							}
						}
						*/
					}

					// result = _IsCloudConnection.HasValue && _IsCloudConnection.Value;
				}
				else
				{
					SqlTracer.AssertTraceEvent(!_IsCloudConnection.HasValue, TraceEventType.Error, EnSqlTraceId.VSShell, "connection is not open, but _IsCloudConnection has a value.  This value should be cleared when we disconnect");
				}

				return result;
			}
		}
	}


	public virtual bool IsDwConnection
	{
		get
		{
			if (!_IsDwConnection.HasValue)
				_IsDwConnection = false;
			return (bool)_IsDwConnection;
			/*
			lock (_LockObject)
			{
				bool result = false;
				if (base.Connection != null && base.Connection.State == ConnectionState.Open)
				{
					if (!_IsDwConnection.HasValue)
					{
						FbConnection sqlConnection = null;
						try
						{
							sqlConnection = ReliableConnectionHelper.CloneAndOpenConnection(base.Connection);
							_IsDwConnection = SqlVersionUtils.IsSqlDwAzure(base.Connection);
						}
						catch (Exception e)
						{
							Tracer.LogExCatch(typeof(SqlConnectionStrategy), e);
						}
						finally
						{
							if (sqlConnection != null)
							{
								sqlConnection.Close();
								sqlConnection.Dispose();
							}
						}
					}

					result = _IsDwConnection.HasValue && _IsDwConnection.Value;
				}
				else
				{
					SqlTracer.AssertTraceEvent(!_IsDwConnection.HasValue, TraceEventType.Error, EnSqlTraceId.VSShell, "connection is not open, but _IsDwConnection has a value.  This value should be cleared when we disconnect");
				}

				return result;
			}
			*/
		}
	}

	public string TracingID
	{
		get
		{
			lock (_LockObject)
			{
				if (Connection == null || Connection.State != ConnectionState.Open)
				{
					return string.Empty;
				}

				return _TracingId ?? string.Empty;
			}
		}
	}

	public string Spid
	{
		get
		{
			lock (_LockObject)
			{
				if (Connection == null || Connection.State != ConnectionState.Open)
				{
					return string.Empty;
				}

				return _Spid.HasValue ? _Spid.ToString() : string.Empty;
			}
		}
	}

	public override string DisplayUserName
	{
		get
		{
			string text = base.DisplayUserName;
			if (!string.IsNullOrEmpty(Spid))
			{
				text = string.Format(CultureInfo.CurrentCulture, ControlsResources.DisplayUserNameAndSPID, text, Spid);
			}

			return text;
		}
	}

	public SqlConnectionStrategy()
	{
		ConnectionChangedPriority += HandleConnectionChanged;
	}


	protected override IDbConnection CreateDbConnectionFromConnectionInfo(UIConnectionInfo uici, bool openConnection)
	{
		if (uici != null)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "CreateDbConnectionFromConnectionInfo", "UIConnectionInfo is not null");
			if (openConnection)
			{
				CreateAndOpenDbConnectionFromConnectionInfo(uici, out var connection);
				return connection;
			}

			FbConnectionStringBuilder csb = new();
			PopulateConnectionStringBuilder(csb, uici);
			return new FbConnection(csb.ToString());
		}

		Tracer.Trace(GetType(), Tracer.Level.Verbose, "CreateDbConnectionFromConnectionInfo", "ERROR UIConnectionInfois null.");

		return null;
	}

	protected virtual void CreateAndOpenDbConnectionFromConnectionInfo(UIConnectionInfo uici, out IDbConnection connection)
	{
		Tracer.Trace(GetType(), Tracer.Level.Verbose, "CreateAndOpenDbConnectionFromConnectionInfo", "Enter");

		FbConnectionStringBuilder csb = new FbConnectionStringBuilder();
		PopulateConnectionStringBuilder(csb, uici);

		connection = new FbConnection(csb.ToString());

		connection.Open();

		/*
		if (ReliableConnectionHelper.OpenConnection(sqlConnectionStringBuilder, useRetry: true) is ReliableSqlConnection reliableSqlConnection && !string.IsNullOrEmpty(reliableSqlConnection.Database) && reliableSqlConnection.State == ConnectionState.Open)
		{
			connection = reliableSqlConnection.GetUnderlyingConnection();
		}
		*/
	}

	private void HandleConnectionChanged(object sender, ConnectionChangedEventArgs args)
	{
		IDbConnection previousConnection = args.PreviousConnection;
		if (previousConnection != null)
		{
			if (previousConnection is DbConnection dbConnection)
			{
				dbConnection.StateChange -= HandleConnectionStateChanged;
			}
		}

		lock (_LockObject)
		{
			_Spid = null;
			_ProductLevel = null;
			_TracingId = null;
			// _IsCloudConnection = null;
			// _IsDwConnection = null;
			if (Connection != null)
			{
				(Connection as DbConnection).StateChange += HandleConnectionStateChanged;
				UpdateStateForCurrentConnection(Connection.State, ConnectionState.Closed);
			}
		}
	}

	private void HandleConnectionStateChanged(object sender, StateChangeEventArgs args)
	{
		UpdateStateForCurrentConnection(args.CurrentState, args.OriginalState);
	}

	private void UpdateStateForCurrentConnection(ConnectionState currentState, ConnectionState previousState)
	{
		/*
		if (currentState == ConnectionState.Broken || currentState == ConnectionState.Closed)
		{
			lock (_LockObject)
			{
				_Spid = null;
				_TracingId = null;
				_ProductLevel = null;
				_IsCloudConnection = null;
				_IsDwConnection = null;
			}
		}

		if (currentState == ConnectionState.Open)
		{
			QueryServerSideProperties();
		}

		if (currentState != ConnectionState.Open || previousState != 0 || !string.IsNullOrEmpty(SqlServerConnectionService.GetDatabaseName(ConnectionInfo)) || Connection == null)
		{
			return;
		}

		try
		{
			FbConnection asSqlConnection = ReliableConnectionHelper.GetAsSqlConnection(Connection);
			string text = null;
			try
			{
				text = GetDefaultDatabaseForLogin(asSqlConnection, useExistingDbOnError: false);
			}
			catch (Exception ex)
			{
				if (ex.menamy != "EnumeratorException")
					throw ex;
			}

			if (string.IsNullOrEmpty(text))
			{
				text = Master;
			}

			SqlServerConnectionService.SetDatabaseName(ConnectionInfo, text);
		}
		catch (Exception e)
		{
			Tracer.LogExCatch(GetType(), e);
		}
		*/
	}



	protected override void AcquireConnectionInfo(bool tryOpenConnection, out UIConnectionInfo uici, out IDbConnection connection)
	{
		if (UiConnectionInfo != null)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "AcquireConnectionInfo", "UiConnectionInfo is not null");
			uici = UiConnectionInfo;
			connection = CreateDbConnectionFromConnectionInfo(uici, tryOpenConnection);
		}
		else
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "AcquireConnectionInfo", "UiConnectionInfo is null. Prompting");
			uici = PromptForConnectionForEditor(out connection);
		}
	}

	protected override void ChangeConnectionInfo(bool tryOpenConnection, out UIConnectionInfo uici, out IDbConnection connection)
	{
		uici = PromptForConnectionForEditor(out connection);
	}

	public override void ApplyConnectionOptions(IDbConnection conn, IBQueryExecutionSettings s)
	{
		if (!IsDwConnection)
		{
			base.ApplyConnectionOptions(conn, s);
		}
		else
		{
			DwApplyConnectionOptions(conn, s);
		}
	}


	private void DwApplyConnectionOptions(IDbConnection conn, IBQueryExecutionSettings s)
	{
		return;

		/* Not used.

		try
		{
			lock (_LockObject)
			{
				Tracer.Trace(typeof(SqlConnectionStrategy), "ApplyConnectionOptions()", "starting");
				StringBuilder stringBuilder = new StringBuilder(512);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0} {1} {2}", s.SetRowCountString, s.SetTextSizeString, s.SetImplicitTransactionString);
				string text = stringBuilder.ToString();
				Tracer.Trace(typeof(SqlConnectionStrategy), "ApplyConnectionOptions()", ": Executing script: \"{0}\"", text);
				using IDbCommand dbCommand = conn.CreateCommand();
				dbCommand.CommandText = text;
				dbCommand.ExecuteNonQuery();
			}
		}
		catch (Exception ex)
		{
			Tracer.LogExCatch(typeof(SqlConnectionStrategy), ex);
			StringBuilder stringBuilder2 = new StringBuilder(100);
			stringBuilder2.AppendFormat(ControlsResources.UnableToApplyConnectionSettings, ex.Message);
			Cmd.ShowMessageBoxEx(string.Empty, stringBuilder2.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		*/
	}


	private UIConnectionInfo PromptForConnectionForEditor(out IDbConnection connection)
	{
		return PromptForConnection(out connection, ValidateConnectionForEditor);
	}

	public static UIConnectionInfo PromptForConnection(out IDbConnection connection)
	{
		return PromptForConnection(out connection, ValidateConnection);
	}

	private static UIConnectionInfo PromptForConnection(out IDbConnection connection, VerifyConnectionDelegate validateConnectionDelegate)
	{
		ThreadHelper.ThrowIfNotOnUIThread();


		using (ConnectionDialogWrapper connectionDialogWrapper = new ConnectionDialogWrapper())
		{
			UIConnectionInfo uIConnectionInfo = new UIConnectionInfo();
			connectionDialogWrapper.ConnectionVerifier = validateConnectionDelegate;
			Native.ThrowOnFailure((Package.GetGlobalService(typeof(IVsUIShell)) as IVsUIShell).GetDialogOwnerHwnd(out var phwnd));
			if (connectionDialogWrapper.ShowDialogValidateConnection(phwnd, uIConnectionInfo, out connection) == true)
			{
				return uIConnectionInfo;
			}
		}

		return null;
	}

	protected virtual IDbConnection ValidateConnectionForEditor(UIConnectionInfo ci /*, IServerConnectionProvider server*/)
	{
		IDbConnection dbConnection = ValidateConnection(ci /*, server*/);

		/*
		if (ci.ServerType == SqlServerConnectionProvider.ServerType && dbConnection != null && SqlVersionUtils.IsSqlDwAps(dbConnection))
		{
			ConnectionDialogException ex = new(ControlsResources.ErrQEPdwNotSupported);
			Diag.Dug(ex);
			throw ex;
		}
		*/

		return dbConnection;
	}

	public static IDbConnection ValidateConnection(UIConnectionInfo ci /* , IServerConnectionProvider server */)
	{
		ci.DataSource = ci.DataSource.Trim();
		if (string.IsNullOrWhiteSpace(ci.ApplicationName) || string.Equals(BuilderWithDefaultApplicationName.ApplicationName, ci.ApplicationName, StringComparison.Ordinal))
		{
			ci.ApplicationName = C_ApplicationName;
		}

		FbConnectionStringBuilder csb = new();

		PopulateConnectionStringBuilder(csb, ci);

		IDbConnection dbConnection = new FbConnection(csb.ConnectionString);

		dbConnection.Open();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "Select 1";
		dbCommand.Connection = dbConnection;
		dbCommand.ExecuteNonQuery();

		return dbConnection;
	}

	public override List<string> GetAvailableDatabases()
	{
		List<string> list = new List<string>();

		DataTable databases = XmlParser.Databases;

		foreach (DataRow row in databases.Rows)
		{
			if ((string)row["Name"] == "")
				continue;

			list.Add((string)row["DatasetKey"]);
		}

		return list;

		/*
		IDbCommand dbCommand = null;
		IDataReader dataReader = null;
		IDbConnection dbConnection = null;
		Exception ex = null;
		lock (_LockObject)
		{
			try
			{
				if (Connection != null && Connection.State == ConnectionState.Open)
				{
					if (IsCloudConnection && !string.Equals("master", Connection.Database, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(Connection.Database);
						return list;
					}

					dbConnection = ReliableConnectionHelper.CloneAndOpenConnection(Connection);
					dbCommand = dbConnection.CreateCommand();
					Version version = new Version(ReliableConnectionHelper.ReadServerVersion(Connection));
					if (version.Major == 8)
					{
						dbCommand.CommandText = "SELECT dtb.name AS [Name], dtb.status AS [Status] FROM master.dbo.sysdatabases dtb";
					}
					else
					{
						dbCommand.CommandText = "SELECT dtb.name AS [Name], dtb.state AS [State] FROM master.sys.databases dtb";
					}

					Dictionary<string, int> dictionary = new Dictionary<string, int>(1024);
					dataReader = dbCommand.ExecuteReader();
					bool flag = dataReader.Read();
					while (flag)
					{
						dataReader.GetName(0);
						string @string = dataReader.GetString(0);
						bool flag2 = true;
						if (version.Major == 8)
						{
							flag2 = (dataReader.GetInt32(1) & 0x3E0) == 0;
						}
						else
						{
							int num = Convert.ToInt32(dataReader.GetValue(1), CultureInfo.InvariantCulture);
							if (num == 1 || num == 2 || num == 3 || num == 4 || num == 6)
							{
								flag2 = false;
							}
						}

						if (flag2)
						{
							if (dictionary.ContainsKey(@string))
							{
								dictionary[@string]++;
							}
							else
							{
								dictionary[@string] = 1;
							}
						}

						flag = dataReader.Read();
					}

					if (dataReader != null)
					{
						dataReader.Dispose();
						dataReader = null;
					}

					foreach (string key in dictionary.Keys)
					{
						if (dictionary[key] == 1)
						{
							list.Add(key);
						}
					}

					list.Sort();
				}
			}
			catch (FbException ex2)
			{
				Tracer.LogExCatch(GetType(), ex2);
				ex = ex2;
			}
			catch (Exception ex3)
			{
				Tracer.LogExCatch(GetType(), ex3);
				ex = ex3;
			}
			finally
			{
				if (dataReader != null)
				{
					dataReader.Dispose();
					dataReader = null;
				}

				if (dbCommand != null)
				{
					dbCommand.Dispose();
					dbCommand = null;
				}

				if (dbConnection != null)
				{
					if (dbConnection.State != 0)
					{
						dbConnection.Close();
					}

					dbConnection.Dispose();
				}
			}

			if (ex != null)
			{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
				Cmd.ShowExceptionInDialog(ControlsResources.SqlEditorNoAvailableDatabase, ex);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
				return list;
			}

			return list;
		}
		*/
	}

	public override void ResetAndEnableConnectionStatistics()
	{
		lock (_LockObject)
		{
			try
			{
				// FbCommand cmd = new("ALTER SESSION RESET", (FbConnection)Connection);
				// cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			// FbConnection asSqlConnection = ReliableConnectionHelper.GetAsSqlConnection(Connection);
			// asSqlConnection.ResetStatistics();
			// asSqlConnection.StatisticsEnabled = true;
		}
	}

	public override void SetDatasetKeyOnConnection(string selectedDatasetKey, DbConnectionStringBuilder csb)
	{
		lock (_LockObject)
		{
			if (Connection != null /* && Connection.State == ConnectionState.Open */ && IsCloudConnection)
			{
				try
				{
					_Csb = csb;

					_Csb ??= XmlParser.GetCsbFromDatabases(selectedDatasetKey);
					if (_Csb != null)
					{
						if (Connection.State == ConnectionState.Open)
							Connection.Close();
						Connection.ConnectionString = _Csb.ConnectionString;
						// UiConnectionInfo.ConnectionStringBuilder = _Csb;
						UiConnectionInfo.Parse(_Csb);
						SetConnectionInfo(UiConnectionInfo);
						Connection.Open();
					}
				}
				catch (FbException e)
				{
					Tracer.LogExCatch(typeof(ConnectionStrategy), e);
					Cmd.ShowMessageBoxEx(null, string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrDatabaseNotAccessible, selectedDatasetKey), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else
			{
				base.SetDatasetKeyOnConnection(selectedDatasetKey, csb);
			}
		}
	}

	public override object GetPropertiesWindowDisplayObject()
	{
		// return null;

		if (UiConnectionInfo == null)
		{
			return DisconnectedPropertiesWindow.Instance;
		}

		_propertiesWindowObject ??= new ConnectedPropertiesWindow(this);

		return _propertiesWindowObject;

	}

	protected virtual void QueryServerSideProperties()
	{
		lock (_LockObject)
		{
			if (Connection == null || Connection.State != ConnectionState.Open)
			{
				return;
			}

			StringBuilder stringBuilder = new StringBuilder(256);
			stringBuilder.AppendLine("select @@spid;");
			stringBuilder.AppendLine("select SERVERPROPERTY('ProductLevel');");
			if (IsCloudConnection && !IsDwConnection)
			{
				stringBuilder.AppendLine("SELECT CONVERT(NVARCHAR(36), CONTEXT_INFO());");
			}

			IDbCommand dbCommand = Connection.CreateCommand();
			dbCommand.CommandText = stringBuilder.ToString();
			IDataReader dataReader = null;
			try
			{
				dataReader = dbCommand.ExecuteReader();
				if (dataReader.Read())
				{
					_Spid = int.Parse(dataReader.GetValue(0).ToString(), CultureInfo.InvariantCulture);
					dataReader.NextResult();
				}

				if (dataReader.Read())
				{
					_ProductLevel = dataReader.GetValue(0).ToString();
					dataReader.NextResult();
				}

				if (IsCloudConnection && !IsDwConnection && dataReader.Read())
				{
					_TracingId = dataReader.GetValue(0).ToString();
				}
			}
			catch (Exception exception)
			{
				SqlTracer.TraceException((TraceEventType)869, EnSqlTraceId.VSShell, exception, "QueryServerSideProperties");
				_ = Connection.State;
				_ = 1;
			}
			finally
			{
				if (dataReader != null)
				{
					dataReader.Close();
					dataReader = null;
				}

				if (dbCommand != null)
				{
					dbCommand.Dispose();
					dbCommand = null;
				}
			}
		}
	}

	/*
	public static string GetDefaultDatabaseForLogin(string connectionString, bool useExistingDbOnError)
	{
		using FbConnection conn = ConnectionHelperUtils.OpenSqlConnection(new FbConnectionStringBuilder(connectionString));
		return GetDefaultDatabaseForLogin(conn, useExistingDbOnError);
	}
	*/


	public static string GetDefaultDatabaseForLogin(FbConnection conn, bool useExistingDbOnError)
	{
		return conn.Database;
		/*
		ServerConnection serverConnection = new ServerConnection(conn);
		string result = string.Empty;
		Enumerator enumerator = new Enumerator();
		Request request = new Request
		{
			Urn = "Server/Login[@Name='" + Urn.EscapeString(serverConnection.TrueLogin) + "']",
			Fields = new string[1]
		};
		request.Fields[0] = "DefaultDatabase";
		try
		{
			DataSet dataSet = enumerator.Process(conn, request);
			if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
			{
				result = Convert.ToString(dataSet.Tables[0].Rows[0]["DefaultDatabase"], CultureInfo.InvariantCulture);
			}

			return result;
		}
		catch (EnumeratorException ex)
		{
			if (useExistingDbOnError)
			{
				SqlTracer.TraceEvent(TraceEventType.Information, EnSqlTraceId.TSqlEditorAndLanguageServices, "An error occurred when querying the default database name. Current database name or master will be used instead of throwing. The error is '{0}'", ex.ToString());
				string database = conn.Database;
				return !string.IsNullOrEmpty(database) ? database : Master;
			}

			throw;
		}
		*/
	}

	public override Version GetServerVersion()
	{
		Version version = new Version();
		if (Connection != null && Connection.State == ConnectionState.Open)
		{
			try
			{
				version = FbServerProperties.ParseServerVersion(((FbConnection)Connection).ServerVersion);
			}
			catch (InvalidOperationException)
			{
			}

			SqlTracer.AssertTraceEvent(!version.Equals(new Version()), TraceEventType.Error, EnSqlTraceId.CoreServices, "GetServerVersion is returning version (0,0).  Something is wrong!!");
		}

		return version;
	}

	public override string GetProductLevel()
	{
		lock (_LockObject)
		{
			return _ProductLevel ?? string.Empty;
		}
	}

	public override bool IsTransactionOpen()
	{
		FbConnection connection = (FbConnection)Connection;
		bool flag = false;
		if (connection != null && connection.State == ConnectionState.Open)
		{
			try
			{
				FbDatabaseInfo databaseInfo = new(connection);

				flag = databaseInfo.GetActiveTransactions().Count > 0;
			}
			catch (Exception ex)
			{
				Tracer.Trace(GetType(), "AreTransactionsOpen", ex.ToString());
				return flag;
			}
		}

		return flag;
	}

	public override void CommitOpenTransactions()
	{
		/*
		FbConnection connection = (FbConnection)Connection;
		if (connection == null || connection.State != ConnectionState.Open)
		{
			return;
		}

		IDbCommand dbCommand = null;
		try
		{
			FbDatabaseInfo databaseInfo = new(connection);

			List<long> transactions = databaseInfo.GetActiveTransactions();

			foreach (long transaction in transactions)
			{
				databaseInfo.
			}

			dbCommand = connection.CreateCommand();
			dbCommand.CommandType = CommandType.Text;
			dbCommand.CommandText = "while (@@trancount > 0) begin commit transaction; end";
			dbCommand.ExecuteNonQuery();
		}
		finally
		{
			dbCommand?.Dispose();
		}
		*/
	}

	public override IBatchExecutionHandler CreateBatchExecutionHandler()
	{
		return new SqlBatchExecutionHandler();
	}


	public virtual bool ShouldShowColumnWithXmlHyperLinkEnabled(QEResultSet resultSet, int columnIndex, string script)
	{
		if (!IsDwConnection)
		{
			return false;
		}

		return DwShouldShowColumnWithXmlHyperLinkEnabled(resultSet, columnIndex, script);
	}

	private bool DwShouldShowColumnWithXmlHyperLinkEnabled(QEResultSet resultSet, int columnIndex, string script)
	{
		StringCollection columnNames = resultSet.ColumnNames;
		if (columnNames != null && columnNames.Count == 1 && string.Compare(columnNames[0], "explain", StringComparison.Ordinal) == 0 && resultSet.GetSchemaRow(columnIndex)["DataType"].ToString() == "System.String")
		{
			return PdwExplainRegex.IsMatch(script);
		}

		return false;
	}

}
