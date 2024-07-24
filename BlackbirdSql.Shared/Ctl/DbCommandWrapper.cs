// Microsoft.Data.Tools.Schema.Sql, Version=162.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Schema.Common.SqlClient.DbCommandWrapper

using System;
using System.Data.Common;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Ctl;


public sealed class DbCommandWrapper
{
	private readonly object _Command;

	public event BatchStatementCompletedEventHandler StatementCompletedEvent;

	public DbCommandWrapper(object command)
	{
		try
		{
			Cmd.CheckForNullReference(command, "command");

			if (!IsSupportedCommandType(command))
			{
				InvalidOperationException ex = new(Resources.ExInvalidCommandType);
				throw ex;
			}
		}
		catch (Exception ex) 
		{
			Diag.Dug(ex);
			throw ex;
		}

		_Command = command;
	}


	
	public DbCommand GetAsSqlCommand()
	{
		return (DbCommand)_Command;
	}


	public void Dummy()
	{
		StatementCompletedEvent?.Invoke(this, new(0, 0, 0, false));
	}

	public static bool IsSupportedCommandType(object command)
	{
		if (command == null)
		{
			ArgumentNullException ex = new("Db Command is null");
			Diag.Dug(ex);
			throw ex;
		}

		if(!NativeDb.IsSupportedCommandType(command))
		{
			NotSupportedException ex = new("Invalid command type: " + command.GetType().FullName);
			Diag.Dug(ex);
		}

		return true;
	}


}
