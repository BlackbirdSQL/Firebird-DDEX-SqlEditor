// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEOLESQLExec
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Data.Ctl;
using BlackbirdSql.Sys;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.Data.Model;

[SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "User gets to choose")]


public class DbStatementWrapper : IBsNativeDbStatementWrapper
{
	private DbStatementWrapper()
	{
	}

	public DbStatementWrapper(IBsNativeDbBatchParser owner, object statement)
	{
		_Owner = owner;
		_Statement = (FbStatement)statement;
	}

	public void DisposeCommand()
	{
		_Command?.Dispose();
		_Command = null;
	}

	protected virtual void Dispose(bool isDisposing)
	{
		if (isDisposing)
			DisposeCommand();
	}

	public virtual void Dispose()
	{
		Dispose(true);
	}



	private readonly IBsNativeDbBatchParser _Owner;
	// TBC
	private readonly bool _CanCommit = false;
	private long _RowsSelected = 0;
	private bool _RenewConnection = false;
	private readonly FbStatement _Statement;
	private FbCommand _Command = null;
	private FbConnectionStringBuilder _Csb = null;

	private FbDataReader _QueryDataReader = null;


	private static readonly string[] _StandardParseTokens = [" ", "\r\n", "\n", "\r"];



	public DbCommand Command => _Command;


	public EnSqlStatementAction CurrentAction => _Owner.CurrentAction;

	public EnSqlExecutionType ExecutionType => _Owner.ExecutionType;

	public long ExecutionTimeout => _Owner.ExecutionTimeout;

	private bool IsAsync => _Owner.IsAsync;
	public bool IsSpecialAction => CurrentAction == EnSqlStatementAction.SpecialActions
		|| CurrentAction == EnSqlStatementAction.SpecialWithEstimatedPlan
		|| CurrentAction == EnSqlStatementAction.SpecialWithActualPlan;

	
	DataTable PlanTable => _Owner.PlanTable;

	private DbDataReader QueryDataReader => _QueryDataReader;

	public long RowsSelected => _RowsSelected;

	public long TotalRowsSelected => _Owner.TotalRowsSelected;

	public DbDataReader CurrentActionReader => CurrentAction == EnSqlStatementAction.ProcessQuery
		? QueryDataReader : _Owner.PlanReader;

	public IDbTransaction Transaction => _Owner.Transaction;

	public string Script => _Statement.Text;


	public EnSqlStatementType StatementType => (EnSqlStatementType)_Statement.StatementType;

	/// <summary>
	/// The event trigged after a SQL statement execution.
	/// </summary>
	public event EventHandler<StatementExecutionEventArgs> AfterExecutionEvent;



	public void Cancel()
	{
		_Owner.Cancel();
	}


	public int AsyncExecute(bool autoCommit)
	{

		if (CurrentAction != EnSqlStatementAction.ProcessQuery)
			return -1;


		if (ExecutionType == EnSqlExecutionType.PlanOnly)
		{
			GeneratePlan();
			return -1;
		}

		// Start up the payload launcher with tracking.
		// Fire and remember

		CancellationToken cancellationToken = _Owner.AsyncTokenSource.Token;

		if (!IsAsync)
			return ExecuteAsync(autoCommit, cancellationToken).AwaiterResult();


		return Task.Factory.StartNew(async delegate { return await ExecuteAsync(autoCommit, cancellationToken); },
			cancellationToken, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent,
			TaskScheduler.Default).AwaiterResult();

	}


	private async Task<int> ExecuteAsync(bool autoCommit, CancellationToken cancellationToken)
	{ 
		int rowsSelected = 0;
		SqlStatementType statementType = (SqlStatementType)StatementType;

		FbStatement statement = _Statement;
		FbCommand command;

		_RenewConnection = false;


		if (!(statementType == SqlStatementType.Connect ||
			statementType == SqlStatementType.CreateDatabase ||
			statementType == SqlStatementType.Disconnect ||
			statementType == SqlStatementType.DropDatabase ||
			statementType == SqlStatementType.SetAutoDDL ||
			statementType == SqlStatementType.SetDatabase ||
			statementType == SqlStatementType.SetNames ||
			statementType == SqlStatementType.SetSQLDialect))
		{
			command = ProvideCommand();
			command.CommandText = statement.Text;

			if (statementType != SqlStatementType.Commit && statementType != SqlStatementType.Rollback
				&& Transaction != null)
			{
				_Command.Transaction = (FbTransaction)Transaction;
			}
		}

		try
		{
			switch (statementType)
			{
				case SqlStatementType.AlterCharacterSet:
				case SqlStatementType.AlterDatabase:
				case SqlStatementType.AlterDomain:
				case SqlStatementType.AlterException:
				case SqlStatementType.AlterExternalFunction:
				case SqlStatementType.AlterFunction:
				case SqlStatementType.AlterIndex:
				case SqlStatementType.AlterPackage:
				case SqlStatementType.AlterProcedure:
				case SqlStatementType.AlterRole:
				case SqlStatementType.AlterSequence:
				case SqlStatementType.AlterTable:
				case SqlStatementType.AlterTrigger:
				case SqlStatementType.AlterView:
				case SqlStatementType.CommentOn:
				case SqlStatementType.CreateCollation:
				case SqlStatementType.CreateDomain:
				case SqlStatementType.CreateException:
				case SqlStatementType.CreateFunction:
				case SqlStatementType.CreateGenerator:
				case SqlStatementType.CreateIndex:
				case SqlStatementType.CreatePackage:
				case SqlStatementType.CreatePackageBody:
				case SqlStatementType.CreateProcedure:
				case SqlStatementType.CreateRole:
				case SqlStatementType.CreateSequence:
				case SqlStatementType.CreateShadow:
				case SqlStatementType.CreateTable:
				case SqlStatementType.CreateTrigger:
				case SqlStatementType.CreateView:
				case SqlStatementType.DeclareCursor:
				case SqlStatementType.DeclareExternalFunction:
				case SqlStatementType.DeclareFilter:
				case SqlStatementType.DeclareStatement:
				case SqlStatementType.DeclareTable:
				case SqlStatementType.Delete:
				case SqlStatementType.DropCollation:
				case SqlStatementType.DropDomain:
				case SqlStatementType.DropException:
				case SqlStatementType.DropExternalFunction:
				case SqlStatementType.DropFunction:
				case SqlStatementType.DropFilter:
				case SqlStatementType.DropGenerator:
				case SqlStatementType.DropIndex:
				case SqlStatementType.DropPackage:
				case SqlStatementType.DropPackageBody:
				case SqlStatementType.DropProcedure:
				case SqlStatementType.DropSequence:
				case SqlStatementType.DropRole:
				case SqlStatementType.DropShadow:
				case SqlStatementType.DropTable:
				case SqlStatementType.DropTrigger:
				case SqlStatementType.DropView:
				case SqlStatementType.EventInit:
				case SqlStatementType.EventWait:
				case SqlStatementType.Execute:
				case SqlStatementType.ExecuteImmediate:
				case SqlStatementType.ExecuteProcedure:
				case SqlStatementType.Grant:
				case SqlStatementType.Insert:
				case SqlStatementType.InsertCursor:
				case SqlStatementType.Merge:
				case SqlStatementType.Open:
				case SqlStatementType.Prepare:
				case SqlStatementType.Revoke:
				case SqlStatementType.RecreateFunction:
				case SqlStatementType.RecreatePackage:
				case SqlStatementType.RecreatePackageBody:
				case SqlStatementType.RecreateProcedure:
				case SqlStatementType.RecreateTable:
				case SqlStatementType.RecreateTrigger:
				case SqlStatementType.RecreateView:
				case SqlStatementType.SetGenerator:
				case SqlStatementType.Update:
				case SqlStatementType.Whenever:
					
					if (!IsAsync)
						_ = ExecuteCommandAsync(autoCommit, cancellationToken);
					else
						await ExecuteCommandAsync(autoCommit, cancellationToken);

					AfterStatementExecution(null, statement.Text, statementType, rowsSelected);
					break;

				case SqlStatementType.ExecuteBlock:
				case SqlStatementType.Select:

					ProvideCommand().CommandText = statement.Text;

					if (!IsAsync)
						_QueryDataReader = _Command.ExecuteReader(CommandBehavior.Default);
					else
						_QueryDataReader = await _Command.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);

					AfterStatementExecution(_QueryDataReader, statement.Text, statementType, -1);
					break;

				case SqlStatementType.Commit:

					CommitTransaction(true);
					AfterStatementExecution(null, statement.Text, statementType, -1);
					break;

				case SqlStatementType.Rollback:

					RollbackTransaction();
					AfterStatementExecution(null, statement.Text, statementType, -1);
					break;

				case SqlStatementType.CreateDatabase:

					CreateDatabase((string)Reflect.GetPropertyValue(statement, "CleanText"));
					AfterStatementExecution(null, statement.Text, statementType, -1);
					break;

				case SqlStatementType.DropDatabase:

					if (!_Owner.IsLocalConnection)
						throw new InvalidOperationException("Attempt to drop a database for a connection in use within the IDE. To drop a database you have to connect to that database in your script.");

					_Csb ??= new(_Owner.Connection.ConnectionString);
					FbConnection.DropDatabase(_Csb.ToString());
					_RenewConnection = true;
					AfterStatementExecution(null, statement.Text, statementType, -1);
					break;

				case SqlStatementType.Connect:

					ConnectToDatabase((string)Reflect.GetPropertyValue(statement, "CleanText"));
					_RenewConnection = false;

					AfterStatementExecution(null, statement.Text, statementType, -1);
					break;

				case SqlStatementType.Disconnect:

					_Owner.Connection.Close();
					FbConnection.ClearPool((FbConnection)_Owner.Connection);
					AfterStatementExecution(null, statement.Text, statementType, -1);
					break;

				case SqlStatementType.SetAutoDDL:

					SetAutoDdl((string)Reflect.GetPropertyValue(statement, "CleanText"), ref autoCommit);
					AfterStatementExecution(null, statement.Text, statementType, -1);
					break;

				case SqlStatementType.SetNames:

					SetNames((string)Reflect.GetPropertyValue(statement, "CleanText"));
					_RenewConnection = true;
					AfterStatementExecution(null, statement.Text, statementType, -1);
					break;

				case SqlStatementType.SetSQLDialect:

					SetSqlDialect((string)Reflect.GetPropertyValue(statement, "CleanText"));
					_RenewConnection = true;
					AfterStatementExecution(null, statement.Text, statementType, -1);
					break;

				case SqlStatementType.Fetch:
				case SqlStatementType.Describe:
					break;

				case SqlStatementType.SetDatabase:
				case SqlStatementType.SetStatistics:
				case SqlStatementType.SetTransaction:
				case SqlStatementType.ShowSQLDialect:
					throw new NotImplementedException();
			}
		}
		catch
		{
			RollbackTransaction();
			// CloseConnection();

			throw;

			/*
			string message = string.Format("An exception was thrown when executing command: {1}.{0}Batch execution aborted.{0}The returned message was: {2}.",
				Environment.NewLine,
				statement.Text,
				ex.Message);
			throw new SqlException(message);
			*/
		}
		finally
		{
			if (rowsSelected > 0)
			{
				_Owner.AddRowsSelected(rowsSelected);
				_RowsSelected = rowsSelected;
			}
		}

		if (ExecutionType == EnSqlExecutionType.QueryWithPlan && !cancellationToken.IsCancellationRequested)
			GeneratePlan();


		// DisposeCommand();
		if (!cancellationToken.IsCancellationRequested)
			CommitTransaction(false);
		// CloseConnection();

		return rowsSelected;
	}




	public bool AsyncNextResult()
	{

		if (!IsAsync)
			return CurrentActionReader.NextResult();

		// Start up the payload launcher with tracking.
		// Fire and remember

		CancellationToken cancellationToken = _Owner.AsyncTokenSource.Token;


		return Task.Factory.StartNew(async delegate { return await NextResultAsync(cancellationToken); },
			cancellationToken, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent,
			TaskScheduler.Default).AwaiterResult();

	}


	private async Task<bool> NextResultAsync(CancellationToken cancellationToken)
	{
		return await CurrentActionReader.NextResultAsync(cancellationToken);
	}


	public bool AsyncRead()
	{
		if (!IsAsync)
			return CurrentActionReader.Read();

		// Start up the payload launcher with tracking.
		// Fire and wait

		CancellationToken cancellationToken = _Owner.AsyncTokenSource.Token;

		return Task.Factory.StartNew(async delegate { return await ReadAsync(cancellationToken); },
			cancellationToken, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent,
			TaskScheduler.Default).AwaiterResult();

	}


	private async Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		return await CurrentActionReader.ReadAsync(cancellationToken);
	}




	public void GeneratePlan()
	{
		FbStatement statement = _Statement;
		FbCommand command = _Command;

		if (command == null)
		{
			if (ExecutionType != EnSqlExecutionType.PlanOnly)
				return;

			try
			{
				command = ProvideCommand();
				command.CommandText = statement.Text;
				command.Prepare();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return;
			}
		}

		string str = null;

		try
		{
			str = command.GetCommandExplainedPlan();
		}
		catch { }

		if (string.IsNullOrWhiteSpace(str))
			return;

		DataTable table = _Owner.PlanTable;

		DataRow row = table.NewRow();


		row[0] = str;
		table.Rows.Add(row);

	}



	private async Task<int> ExecuteCommandAsync(bool autoCommit, CancellationToken cancellationToken)
	{
		int rowsAffected;

		if (!IsAsync)
			rowsAffected = _Command.ExecuteNonQuery();
		else
			rowsAffected = await _Command.ExecuteNonQueryAsync(cancellationToken);

		if (!cancellationToken.IsCancellationRequested && autoCommit && (bool)Reflect.GetPropertyValue(_Command, "IsDDLCommand"))
		{
			CommitTransaction(false);
		}

		return rowsAffected;
	}

	private void CommitTransaction(bool force)
	{
		if (_CanCommit)
			_Owner.CommitTransaction();
	}

	private void RollbackTransaction()
	{
		if (_CanCommit)
			_Owner.RollbackTransaction();
	}

	private FbCommand ProvideCommand()
	{
		_Command ??= new FbCommand();
		_Command.Connection = (FbConnection)_Owner.Connection;
		return _Command;
	}


	private void AfterStatementExecution(DbDataReader dataReader, string commandText, SqlStatementType statementType, int rowsAffected)
	{
		AfterExecutionEvent?.Invoke(this, new(dataReader, commandText, (EnSqlStatementType)(int)statementType, rowsAffected));
	}


	private void CreateDatabase(string createDatabaseStatement)
	{
		// CREATE {DATABASE | SCHEMA} 'filespec'
		// [USER 'username' [PASSWORD 'password']]
		// [PAGE_SIZE [=] int]
		// [LENGTH [=] int [PAGE[S]]]
		// [DEFAULT CHARACTER SET charset]
		// [<secondary_file>];
		var pageSize = 0;
		var parser = new DbNativeStringParser(createDatabaseStatement)
		{
			Tokens = _StandardParseTokens
		};

		using (var enumerator = parser.Parse().GetEnumerator())
		{
			enumerator.MoveNext();
			if (enumerator.Current.Text.ToUpperInvariant() != "CREATE")
			{
				throw new ArgumentException("Malformed isql CREATE statement. Expected keyword CREATE but something else was found.");
			}
			enumerator.MoveNext(); // {DATABASE | SCHEMA}
			enumerator.MoveNext();
			_Csb ??= new(_Owner.Connection.ConnectionString)
			{
				Database = enumerator.Current.Text.Replace("'", string.Empty)
			};

			while (enumerator.MoveNext())
			{
				switch (enumerator.Current.Text.ToUpperInvariant())
				{
					case "USER":
						enumerator.MoveNext();
						_Csb.UserID = enumerator.Current.Text.Replace("'", string.Empty);
						break;

					case "PASSWORD":
						enumerator.MoveNext();
						_Csb.Password = enumerator.Current.Text.Replace("'", string.Empty);
						break;

					case "PAGE_SIZE":
						enumerator.MoveNext();
						if (enumerator.Current.Text == "=")
							enumerator.MoveNext();
						int.TryParse(enumerator.Current.Text, out pageSize);
						break;

					case "DEFAULT":
						enumerator.MoveNext();
						if (enumerator.Current.Text.ToUpperInvariant() != "CHARACTER")
							throw new ArgumentException("Expected the keyword CHARACTER but something else was found.");

						enumerator.MoveNext();
						if (enumerator.Current.Text.ToUpperInvariant() != "SET")
							throw new ArgumentException("Expected the keyword SET but something else was found.");

						enumerator.MoveNext();
						_Csb.Charset = enumerator.Current.Text;
						break;
				}
			}
		}

		_Csb ??= new(_Owner.Connection.ConnectionString);

		FbConnection.CreateDatabase(_Csb.ToString(), pageSize: pageSize);
		_RenewConnection = true;
		ProvideConnection();
	}

	private void ConnectToDatabase(string connectDbStatement)
	{
		// CONNECT 'filespec'
		// [USER 'username']
		// [PASSWORD 'password']
		// [CACHE int]
		// [ROLE 'rolename']
		var parser = new DbNativeStringParser(connectDbStatement)
		{
			Tokens = _StandardParseTokens
		};
		using (var enumerator = parser.Parse().GetEnumerator())
		{
			enumerator.MoveNext();
			if (enumerator.Current.Text.ToUpperInvariant() != "CONNECT")
			{
				throw new ArgumentException("Malformed isql CONNECT statement. Expected keyword CONNECT but something else was found.");
			}
			enumerator.MoveNext();

			_Csb ??= new(_Owner.Connection.ConnectionString);
			_Csb.Database = enumerator.Current.Text.Replace("'", string.Empty);

			while (enumerator.MoveNext())
			{
				switch (enumerator.Current.Text.ToUpperInvariant())
				{
					case "USER":
						enumerator.MoveNext();
						_Csb.UserID = enumerator.Current.Text.Replace("'", string.Empty);
						break;

					case "PASSWORD":
						enumerator.MoveNext();
						_Csb.Password = enumerator.Current.Text.Replace("'", string.Empty);
						break;

					case "CACHE":
						enumerator.MoveNext();
						break;

					case "ROLE":
						enumerator.MoveNext();
						_Csb.Role = enumerator.Current.Text.Replace("'", string.Empty);
						break;

					default:
						throw new ArgumentException("Unexpected token '" + enumerator.Current.Text + "' on isql CONNECT statement.");

				}
			}
		}
		_RenewConnection = true;
		ProvideConnection();
	}


	private void SetAutoDdl(string setAutoDdlStatement, ref bool autoCommit)
	{
		// SET AUTODDL [ON | OFF]
		var parser = new DbNativeStringParser(setAutoDdlStatement)
		{
			Tokens = _StandardParseTokens
		};

		using (var enumerator = parser.Parse().GetEnumerator())
		{
			enumerator.MoveNext();
			if (enumerator.Current.Text.ToUpperInvariant() != "SET")
			{
				throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
			}
			enumerator.MoveNext(); // AUTO
			if (enumerator.MoveNext())
			{
				var onOff = enumerator.Current.Text.ToUpperInvariant();
				if (onOff == "ON")
				{
					autoCommit = true;
				}
				else if (onOff == "OFF")
				{
					autoCommit = false;
				}
				else
				{
					throw new ArgumentException("Expected the ON or OFF but something else was found.");
				}
			}
			else
			{
				autoCommit = !autoCommit;
			}
		}
	}


	private void SetNames(string setNamesStatement)
	{
		// SET NAMES charset
		var parser = new DbNativeStringParser(setNamesStatement)
		{
			Tokens = _StandardParseTokens
		};

		using (var enumerator = parser.Parse().GetEnumerator())
		{
			enumerator.MoveNext();
			if (enumerator.Current.Text.ToUpperInvariant() != "SET")
			{
				throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
			}
			enumerator.MoveNext(); // NAMES
			enumerator.MoveNext();
			_Csb ??= new(_Owner.Connection.ConnectionString);
			_Csb.Charset = enumerator.Current.Text;
		}
	}

	/// <summary>
	/// Parses the isql statement SET SQL DIALECT and sets the dialect set to current connection string.
	/// </summary>
	/// <param name="setSqlDialectStatement">The set sql dialect statement.</param>
	private void SetSqlDialect(string setSqlDialectStatement)
	{
		// SET SQL DIALECT dialect
		var parser = new DbNativeStringParser(setSqlDialectStatement)
		{
			Tokens = _StandardParseTokens
		};

		using (var enumerator = parser.Parse().GetEnumerator())
		{
			enumerator.MoveNext();
			if (enumerator.Current.Text.ToUpperInvariant() != "SET")
			{
				throw new ArgumentException("Malformed isql SET statement. Expected keyword SET but something else was found.");
			}
			enumerator.MoveNext(); // SQL
			enumerator.MoveNext(); // DIALECT
			enumerator.MoveNext();
			int.TryParse(enumerator.Current.Text, out var dialect);
			_Csb.Dialect = dialect;
		}
	}



	private FbConnection ProvideConnection()
	{
		if (_RenewConnection)
		{
			return (FbConnection)_Owner.RenewConnection(_Csb.ToString());
		}

		if (_Owner.Connection.State == ConnectionState.Closed)
			_Owner.Connection.Open();

		return (FbConnection)_Owner.Connection;
	}


	private bool CloseConnection()
	{
		return _Owner.CloseConnection();
	}


	public void UpdateRowsSelected(long rowsSelected)
	{
		if (rowsSelected > 0)
		{
			_Owner.AddRowsSelected(rowsSelected);
			_RowsSelected = rowsSelected;
		}

	}



	public override string ToString()
	{
		return _Statement.Text;
	}
}

