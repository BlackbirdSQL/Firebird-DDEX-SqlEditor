#region Assembly Microsoft.Data.Tools.Schema.Sql, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


using System;
using System.Data;

using BlackbirdSql.Core;
using BlackbirdSql.Common.Properties;

using FirebirdSql.Data.FirebirdClient;

// using Microsoft.Data.SqlClient;


// using Microsoft.Data.Tools.Schema.Common.SqlClient;
// using Ns = Microsoft.Data.Tools.Schema.Common.SqlClient;



namespace BlackbirdSql.Common.Ctl;

public sealed class DbCommandWrapper
{
	private readonly IDbCommand _Command;

	public event StatementCompletedEventHandler StatementCompleted
	{
		add
		{
			// NotImplementedException ex = new();
			// Diag.Dug(ex);
			/* GetAsSqlCommand().StatementCompleted += value;*/
		}
		remove
		{
			// NotImplementedException ex = new();
			// Diag.Dug(ex);
			/* GetAsSqlCommand().StatementCompleted -= value; */
		}
	}

	public DbCommandWrapper(IDbCommand command)
	{
		try
		{
			Cmd.CheckForNullReference(command, "command");
			if (command is not FbCommand)
			{
				InvalidOperationException ex = new(SharedResx.InvalidCommandType);
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


	
	public FbCommand GetAsSqlCommand()
	{

		return (FbCommand)_Command;
	}
	

	public static bool IsSupportedCommand(IDbCommand command)
	{
		if (command == null)
		{
			ArgumentNullException ex = new("Db Command is null");
			Diag.Dug(ex);
			throw ex;
		}

		bool result = command is FbCommand;

		if (!result)
		{
			NotSupportedException ex = new("Invalid comman type: " + command.GetType().FullName);
			Diag.Dug(ex);
		}

		return result;
	}


}
