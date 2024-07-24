// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.SqlConnectionStrategy

using System;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Controls.PropertiesWindow;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Model;



namespace BlackbirdSql.Shared.Model;


// =========================================================================================================
//
//									ConnectionStrategy Class
//
/// <summary>
/// Manages connections for a query.
/// </summary>
// =========================================================================================================
public class ConnectionStrategy : AbstractConnectionStrategy
{

	// ----------------------------------------------------
	#region Constructors / Destructors - ConnectionStrategy
	// ----------------------------------------------------

	/// <summary>
	/// Default .ctor.
	/// </summary>
	public ConnectionStrategy() : base()
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - ConnectionStrategy
	// =========================================================================================================


	private ConnectedPropertiesWindow _PropertiesWindowObject;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - ConnectionStrategy
	// =========================================================================================================



	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - ConnectionStrategy
	// =========================================================================================================


	/// <summary>
	/// Returns true if there were no exceptions else false.
	/// If transactions do not exist returns true.
	/// </summary>
	public override bool CommitTransactions()
	{
		try
		{
			if (!GetUpdateTransactionsStatus(false))
				return true;
		}
		catch
		{
			return false;
		}

		bool result = true;

		try
		{
			Transaction?.Commit();
		}
		catch
		{
			result = false;
		}


		DisposeTransaction(true);
		GetUpdateTransactionsStatus(true);

		return result;
	}



	public override IBsBatchExecutionHandler CreateBatchExecutionHandler()
	{
		return new SqlBatchExecutionHandler();
	}



	public override object GetPropertiesWindowDisplayObject()
	{
		// return null;

		if (ConnInfo == null)
		{
			return DisconnectedPropertiesWindow.Instance;
		}

		_PropertiesWindowObject ??= new ConnectedPropertiesWindow(this);

		return _PropertiesWindowObject;

	}



	public override Version GetServerVersion()
	{
		Version version = null;
		if (Connection != null && Connection.State == ConnectionState.Open)
		{
			try
			{
				version = NativeDb.GetServerVersion(Connection);
			}
			catch (InvalidOperationException)
			{
			}

			if (version.Equals(new Version()))
			{
				Diag.Dug(new DataException("GetServerVersion is returning version (0,0).  Something is wrong!!"));
			}
		}

		return version;
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



	/// <summary>
	/// Returns true if there were no exceptions else false.
	/// If transactions do not exist returns true.
	/// </summary>
	public override bool RollbackTransactions()
	{
		try
		{
			if (!GetUpdateTransactionsStatus(false))
				return true;
		}
		catch
		{
			return false;
		}

		bool result = true;

		try
		{
			Transaction?.Rollback();
		}
		catch
		{
			result = false;
		}


		DisposeTransaction(true);
		GetUpdateTransactionsStatus(true);

		return result;
	}



	public void SetDatasetKeyOnConnection(string selectedQualifiedName, Csb csb)
	{
		// Tracer.Trace(GetType(), "SetDatasetKeyOnConnection()", "selectedDatasetKey: {0}, ConnectionString: {1}.", selectedDatasetKey, csb.ConnectionString);

		lock (_LockObject)
		{
			try
			{
				if (csb == null || csb.AdornedQualifiedName != selectedQualifiedName || _ConnectionStamp != RctManager.Stamp)
				{
					_ConnectionStamp = RctManager.Stamp;
					csb = RctManager.ShutdownState ? null : RctManager.CloneRegistered(selectedQualifiedName, EnRctKeyType.AdornedQualifiedName);
				}

				if (csb == null)
					return;

				IBsConnectionInfo ci = (IBsConnectionInfo) new ConnectionInfoPropertyAgent();

				ci.Parse(csb);

				ConnInfo = ci;

				try
				{
					ci.CreateDataConnection();
				}
				finally
				{
					_DatabaseChangedEvent?.Invoke(this, new EventArgs());
				}

			}
			catch (DbException ex)
			{
				Diag.Expected(ex);
				MessageCtl.ShowEx(ex, Resources.ExDatabaseNotAccessibleEx.FmtRes(selectedQualifiedName), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
	}



	protected override void UpdateStateForCurrentConnection(ConnectionState currentState, ConnectionState previousState)
	{
		/*
		if (currentState == ConnectionState.Open)
		{
			QueryServerSideProperties();
		}

		if (currentState != ConnectionState.Open || previousState != 0 || !string.IsNullOrEmpty(SqlServerConnectionService.GetDatabaseName(ConnInfo)) || Connection == null)
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

			SqlServerConnectionService.SetDatabaseName(ConnInfo, text);
		}
		catch (Exception e)
		{
			Diag.Dug(e);
		}
		*/
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - ConnectionStrategy
	// =========================================================================================================



	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - ConnectionStrategy
	// =========================================================================================================


	private class SqlBatchExecutionHandler : IBsBatchExecutionHandler
	{
		public void Register(IDbConnection conn, IBsNativeDbStatementWrapper sqlStatement, QESQLBatch batch)
		{
			if (NativeDb.IsSupportedConnection(conn))
			{
				new NativeDbConnectionWrapperProxy(conn).InfoMessageEvent += batch.OnSqlInfoMessage;
				if (DbCommandWrapper.IsSupportedCommandType(sqlStatement))
				{
					new DbCommandWrapper(sqlStatement).StatementCompletedEvent += batch.OnSqlStatementCompleted;
				}
			}
		}

		public void UnRegister(IDbConnection conn, IBsNativeDbStatementWrapper sqlStatement, QESQLBatch batch)
		{
			if (NativeDb.IsSupportedConnection(conn))
			{
				new NativeDbConnectionWrapperProxy(conn).InfoMessageEvent -= batch.OnSqlInfoMessage;
				if (DbCommandWrapper.IsSupportedCommandType(sqlStatement))
				{
					new DbCommandWrapper(sqlStatement).StatementCompletedEvent -= batch.OnSqlStatementCompleted;
				}
				else
				{
					Diag.Dug(new DataException("Supported connection, but not supported command?"));
				}
			}
		}
	}


	#endregion Sub-Classes

}
