#region Assembly Microsoft.Data.Tools.Schema.Sql, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\Microsoft.Data.Tools.Schema.Sql.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Properties;

// using Microsoft.Data.SqlClient;

using FirebirdSql.Data.FirebirdClient;

// using Microsoft.Data.Tools.Schema.Common.SqlClient;
// using Ns = Microsoft.Data.Tools.Schema.Common.SqlClient;


namespace BlackbirdSql.Common.Ctl;


public sealed class DbConnectionWrapper
{
	private readonly IDbConnection _Connection;

	// private readonly bool _isReliableConnection;

	private readonly Action<FbConnection> _SqlConnectionCreatedObserver;

	public string DataSource
	{
		get
		{
			return ((FbConnection)_Connection).DataSource;
		}
	}

	public string ServerVersion
	{
		get
		{
			return ((FbConnection)_Connection).ServerVersion;
		}
	}

	public event EventHandler<FbInfoMessageEventArgs> InfoMessageEvent
	{
		add
		{
			GetAsSqlConnection().InfoMessage += value;
		}
		remove
		{
			GetAsSqlConnection().InfoMessage -= value;
		}
	}

	public DbConnectionWrapper(IDbConnection connection, Action<FbConnection> sqlConnectionCreatedObserver = null)
	{
		Cmd.CheckForNullReference(connection, "connection");
		if (connection is not FbConnection)
		{
			InvalidOperationException ex = new(ControlsResources.InvalidConnectionType);
			Diag.Dug(ex);
			throw ex;
		}

		_Connection = connection;
		_SqlConnectionCreatedObserver = sqlConnectionCreatedObserver;
	}

	public static bool IsSupportedConnection(IDbConnection connection)
	{
		return connection is FbConnection;
	}

	public FbConnection GetAsSqlConnection()
	{
		return (FbConnection)_Connection;
	}

	public FbConnection CloneAndOpenConnection()
	{
		FbConnection sqlConnection = ((ICloneable)GetAsSqlConnection()).Clone() as FbConnection;
		_SqlConnectionCreatedObserver?.Invoke(sqlConnection);

		sqlConnection.Open();
		return sqlConnection;
	}
}
