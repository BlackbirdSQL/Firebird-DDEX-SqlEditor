using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Threading;

using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Properties;

using NativeMethods = BlackbirdSql.Common.Providers.NativeMethods;
using AdoDotNetProvider = BlackbirdSql.Common.Providers.AdoDotNetProvider;




namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TInternalClassConnectionSupport Class
//
/// <summary>
/// Implementation of the <see cref="IVsDataConnectionSupport"/> interface.
/// </summary>
// =========================================================================================================
internal abstract class AbstractConnectionSupport : DataConnectionSupport, IVsDataSiteableObject<IVsDataProvider>
{

	// ---------------------------------------------------------------------------------
	#region Variables classes - AbstractConnectionSupport
	// ---------------------------------------------------------------------------------


	private bool _InAsyncMode;

	private DbCommand _CurrentCommand;

	private DbTransaction _CurrentTransaction;

	private IList<DbCommand> _PreparedCommands;

	private int _DefaultCommandTimeout;

	private DbCommand _BaseCommand;

	private bool _AttachedConnection;

	private DbConnection _Connection;

	private IVsDataProvider _Provider;


	#endregion Variables





	// =====================================================================================================
	#region Property Accessors - AbstractConnectionSupport
	// =====================================================================================================


	public override string ConnectionString
	{
		get 
		{
			return _Connection.ConnectionString;
		}
		set
		{
			if (_Connection.State != 0)
				Close();

			try
			{
				_Connection.ConnectionString = value;
			}
			catch (Exception ex)
			{
				ArgumentException exa = new(ex.Message, "value", ex);
				Diag.Dug(exa);
				throw exa;
			}
		}
	}


	public override int ConnectionTimeout
	{
		get { return _Connection.ConnectionTimeout; }
		set { throw new NotSupportedException(); }
	}


	public override DataConnectionState State => _Connection.State switch
	{
		ConnectionState.Closed => DataConnectionState.Closed,
		ConnectionState.Open => DataConnectionState.Open,
		ConnectionState.Broken => DataConnectionState.Broken,
		_ => DataConnectionState.Closed,
	};


	public override object ProviderObject => _Connection;


	IVsDataProvider IVsDataSiteableObject<IVsDataProvider>.Site
	{
		get
		{
			return _Provider;
		}
		set
		{
			Diag.Trace();
			if (value != null)
			{
				string text = null;
				if (value.Technology == NativeMethods.GUID_AdoDotNetTechnology)
				{
					text = value.GetProperty("InvariantName") as string;
				}

				if (text == null)
				{
					AdoDotNetProvider.CreateObject<DbConnection>(text);
				}
			}

			_Provider = value;
		}
	}

	protected DbTransaction CurrentTransaction
	{
		get
		{
			return _CurrentTransaction;
		}
		set
		{
			_CurrentTransaction = value;
		}
	}

	protected bool InAsyncMode
	{
		get { return _InAsyncMode; }
		set { _InAsyncMode = value; }
	}


	protected DbCommand CurrentCommand
	{
		get { return _CurrentCommand; }
		set { _CurrentCommand = value; }
	}




	protected DbConnection Connection => _Connection;


	#endregion Property Accessors



	public override void Initialize(object providerObj)
	{
		Diag.Trace();
		if (_Provider == null)
		{
			InvalidOperationException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		if (providerObj == null)
		{
			_Connection = AdoDotNetProvider.CreateObject<DbConnection>(_Provider.GetProperty("InvariantName") as string);
		}
		else
		{
			if (providerObj is not DbConnection dbConnection)
			{
				DataException ex = new(string.Format(null, Resources.AdoDotNetConnectionSupport_ProviderObjectNotRecognized,
					_Provider.GetProperty("InvariantName") as string));
				Diag.Dug(ex);
				throw ex;
			}

			_AttachedConnection = true;
			_Connection = dbConnection;
		}

		_BaseCommand = _Connection.CreateCommand();
		_DefaultCommandTimeout = _BaseCommand.CommandTimeout;
		_PreparedCommands = new List<DbCommand>();
		_CurrentCommand = null;
		_CurrentTransaction = null;
		_Connection.StateChange += HandleStateChange;
	}


	public override bool Open(bool doPromptCheck)
	{
		_Connection.Open();
		return true;
	}

	public override void Close()
	{
		if (_CurrentTransaction != null)
		{
			_CurrentTransaction.Rollback();
			_CurrentTransaction = null;
		}

		if (_CurrentCommand != null)
		{
			_CurrentCommand.Cancel();
			_CurrentCommand = null;
		}

		foreach (DbCommand preparedCommand in _PreparedCommands)
		{
			preparedCommand.Dispose();
		}

		_PreparedCommands.Clear();
		_Connection.Close();
	}

	protected virtual IVsDataParameter CreateParameterCore()
	{
		if (_Provider == null)
		{
			InvalidOperationException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		return new AdoDotNetParameter(_Provider.GetProperty("InvariantName") as string);
	}

	protected virtual IVsDataParameter[] DeriveParametersCore(string command, DataCommandType commandType, int commandTimeout)
	{
		if (command == null)
		{
			ArgumentNullException ex = new("command");
			Diag.Dug(ex);
			throw ex;
		}

		if (commandTimeout < -1)
		{
			ArgumentOutOfRangeException ex = new("commandTimeout");
			Diag.Dug(ex);
			throw ex;
		}

		try
		{
			_CurrentCommand = GetCommand(command, commandType, null, commandTimeout);
			DeriveParametersOn(_CurrentCommand);
			IVsDataParameter[] array = new IVsDataParameter[_CurrentCommand.Parameters.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = CreateParameterFrom(_CurrentCommand.Parameters[i]);
			}

			return array;
		}
		catch (ThreadAbortException)
		{
			_CurrentCommand?.Cancel();

			throw;
		}
		finally
		{
			_CurrentCommand?.Parameters.Clear();

			_CurrentCommand = null;
		}
	}

	protected virtual string PrepareCore(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
	{
		if (command == null)
		{
			ArgumentNullException ex = new("command");
			Diag.Dug(ex);
			throw ex;
		}

		if (commandType == DataCommandType.Prepared)
		{
			ArgumentException ex = new(Resources.AdoDotNetConnectionSupport_CannotPreparePreparedCommand);
			Diag.Dug(ex);
			throw ex;
		}

		if (commandTimeout < -1)
		{
			ArgumentOutOfRangeException ex = new("commandTimeout");
			Diag.Dug(ex);
			throw ex;
		}

		DbCommand dbCommand = _Connection.CreateCommand();
		dbCommand.CommandText = command;
		dbCommand.CommandType = GetCommandType(commandType);
		SetParameters(dbCommand, parameters);
		if (commandTimeout == -1)
		{
			dbCommand.CommandTimeout = _DefaultCommandTimeout;
		}
		else
		{
			dbCommand.CommandTimeout = commandTimeout;
		}

		dbCommand.Transaction = _CurrentTransaction;
		dbCommand.Prepare();
		_PreparedCommands.Add(dbCommand);
		return (_PreparedCommands.Count - 1).ToString(CultureInfo.InvariantCulture);
	}



	protected virtual IVsDataReader DeriveSchemaCore(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
	{
		if (command == null)
		{
			ArgumentNullException ex = new("command");
			Diag.Dug(ex);
			throw ex;
		}

		if (commandTimeout < -1)
		{
			ArgumentOutOfRangeException ex = new("commandTimeout");
			Diag.Dug(ex);
			throw ex;
		}

		try
		{
			_CurrentCommand = GetCommand(command, commandType, parameters, commandTimeout);
			DbDataReader reader = _CurrentCommand.ExecuteReader(CommandBehavior.SchemaOnly);
			return new AdoDotNetSchemaReader(reader, _CurrentCommand);
		}
		catch (ThreadAbortException)
		{
			_CurrentCommand?.Cancel();

			_CurrentCommand = null;
			throw;
		}
		catch
		{
			_CurrentCommand = null;
			throw;
		}
		finally
		{
			if (!InAsyncMode)
			{
				_CurrentCommand = null;
			}
		}
	}

	protected virtual IVsDataReader ExecuteCore(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
	{
		if (command == null)
		{
			ArgumentNullException ex = new("command");
			Diag.Dug(ex);
			throw ex;
		}

		if (commandTimeout < -1)
		{
			ArgumentOutOfRangeException ex = new("commandTimeout");
			Diag.Dug(ex);
			throw ex;
		}

		LinkageParser parser = null;

		if (!InAsyncMode)
		{
			parser = LinkageParser.Instance((FbConnection)_Connection);
			parser.SyncEnter();
		}

		try
		{
			_CurrentCommand = GetCommand(command, commandType, parameters, commandTimeout);
			DbDataReader reader = _CurrentCommand.ExecuteReader();
			return new AdoDotNetReader(reader, _CurrentCommand);
		}
		catch (ThreadAbortException)
		{
			_CurrentCommand?.Cancel();

			_CurrentCommand = null;
			throw;
		}
		catch
		{
			_CurrentCommand = null;
			throw;
		}
		finally
		{
			if (!InAsyncMode)
			{
				_CurrentCommand = null;
			}
			parser?.SyncExit();
		}
	}

	protected virtual int ExecuteWithoutResultsCore(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
	{
		if (command == null)
		{
			ArgumentNullException ex = new("command");
			Diag.Dug(ex);
			throw ex;
		}

		if (commandTimeout < -1)
		{
			ArgumentOutOfRangeException ex = new("commandTimeout");
			Diag.Dug(ex);
			throw ex;
		}

		LinkageParser parser = null;

		if (!InAsyncMode)
		{
			parser = LinkageParser.Instance((FbConnection)_Connection);
			parser.SyncEnter();
		}

		try
		{
			_CurrentCommand = GetCommand(command, commandType, parameters, commandTimeout);
			return _CurrentCommand.ExecuteNonQuery();
		}
		catch (ThreadAbortException)
		{
			_CurrentCommand?.Cancel();

			throw;
		}
		finally
		{
			_CurrentCommand = null;
			parser?.SyncExit();
		}
	}

	protected virtual DbCommand GetCommand(string command, DataCommandType commandType, IVsDataParameter[] parameters, int commandTimeout)
	{
		if (command == null)
		{
			ArgumentNullException ex = new("command");
			Diag.Dug(ex);
			throw ex;
		}

		if (commandTimeout < -1)
		{
			ArgumentOutOfRangeException ex = new("commandTimeout");
			Diag.Dug(ex);
			throw ex;
		}

		DbCommand dbCommand;
		if (commandType != DataCommandType.Prepared)
		{
			dbCommand = _BaseCommand;
			dbCommand.CommandText = command;
			dbCommand.CommandType = GetCommandType(commandType);
		}
		else
		{
			if (!int.TryParse(command, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
				|| result < 0 || result >= _PreparedCommands.Count)
			{
				FormatException ex = new(Resources.AdoDotNetConnectionSupport_InvalidPreparedCommand);
				Diag.Dug(ex);
				throw ex;
			}

			dbCommand = _PreparedCommands[result];
		}

		SetParameters(dbCommand, parameters);
		if (commandTimeout == -1)
		{
			dbCommand.CommandTimeout = _DefaultCommandTimeout;
		}
		else
		{
			dbCommand.CommandTimeout = commandTimeout;
		}

		dbCommand.Transaction = _CurrentTransaction;
		return dbCommand;
	}

	protected static CommandType GetCommandType(DataCommandType commandType)
	{
		switch (commandType)
		{
			case DataCommandType.Text:
				return CommandType.Text;
			case DataCommandType.Table:
				return CommandType.TableDirect;
			case DataCommandType.Procedure:
				return CommandType.StoredProcedure;
			default:
				NotSupportedException ex = new();
				Diag.Dug(ex);
				throw ex;
		};
	}

	protected static void SetParameters(DbCommand command, IVsDataParameter[] parameters)
	{
		Diag.Trace();
		if (command == null)
		{
			ArgumentNullException ex = new("command");
			Diag.Dug(ex);
			throw ex;
		}

		command.Parameters.Clear();
		if (parameters == null)
		{
			return;
		}

		for (int i = 0; i < parameters.Length; i++)
		{
			DataParameter dataParameter = (DataParameter)parameters[i];
			if (dataParameter is not AdoDotNetParameter adoDotNetParameter)
			{
				NotSupportedException ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			DbParameter dbParameter = adoDotNetParameter.Parameter;
			command.Parameters.Add(dbParameter);
		}
	}

	protected virtual void DeriveParametersOn(DbCommand command)
	{
		NotSupportedException ex = new();
		Diag.Dug(ex);
		throw ex;
	}

	protected virtual IVsDataParameter CreateParameterFrom(DbParameter parameter)
	{
		return new AdoDotNetParameter(parameter);
	}

	protected virtual void BeginTransactionCore()
	{
		if (_CurrentTransaction != null)
		{
			DataException ex = new(Resources.AdoDotNetConnectionSupport_AlreadyInTransaction);
			Diag.Dug(ex);
			throw ex;
		}

		_CurrentTransaction = _Connection.BeginTransaction();
	}

	protected virtual void CommitTransactionCore()
	{
		if (_CurrentTransaction == null)
		{
			DataException ex = new(Resources.AdoDotNetConnectionSupport_NotInTransaction);
			Diag.Dug(ex);
			throw ex;
		}

		_CurrentTransaction.Commit();
		_CurrentTransaction = null;
	}


	protected virtual void RollbackTransactionCore()
	{
		if (_CurrentTransaction != null)
		{
			try
			{
				_CurrentTransaction.Rollback();
			}
			finally
			{
				_CurrentTransaction = null;
			}
		}
	}



	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_Connection.StateChange -= HandleStateChange;
			_CurrentTransaction?.Dispose();

			if (_PreparedCommands != null)
			{
				foreach (DbCommand preparedCommand in _PreparedCommands)
				{
					preparedCommand.Dispose();
				}
			}

			_BaseCommand?.Dispose();

			if (!_AttachedConnection && _Connection != null)
			{
				_Connection.Dispose();
			}
		}

		base.Dispose(disposing);
	}

	private void HandleStateChange(object sender, StateChangeEventArgs e)
	{
		DataConnectionState oldState = e.OriginalState switch
		{
			ConnectionState.Closed => DataConnectionState.Closed,
			ConnectionState.Open => DataConnectionState.Open,
			ConnectionState.Broken => DataConnectionState.Broken,
			_ => DataConnectionState.Closed,
		};
		switch (e.CurrentState)
		{
			case ConnectionState.Closed:
				OnStateChanged(new DataConnectionStateChangedEventArgs(oldState, DataConnectionState.Closed));
				break;
			case ConnectionState.Open:
				OnStateChanged(new DataConnectionStateChangedEventArgs(oldState, DataConnectionState.Open));
				break;
			case ConnectionState.Broken:
				OnStateChanged(new DataConnectionStateChangedEventArgs(oldState, DataConnectionState.Broken));
				break;
		}
	}
}
