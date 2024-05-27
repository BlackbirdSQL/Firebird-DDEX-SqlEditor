#region Assembly Microsoft.Data.Tools.Schema.Sql, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\Microsoft.Data.Tools.Schema.Sql.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Data.Properties;
using BlackbirdSql.Sys;
using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.Data.Model;


public sealed class DbConnectionWrapper : SBsNativeDbConnectionWrapper, IBsNativeDbConnectionWrapper
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

	private event EventHandler<DbInfoMessageEventArgs> _InfoMessageEvent;

	public event EventHandler<DbInfoMessageEventArgs> InfoMessageEvent
	{
		add
		{
			if (_InternalInfoMessageEvent == null)
			{
				_InternalInfoMessageEvent = OnInternalInfoMessage;
				GetAsSqlConnection().InfoMessage += OnInternalInfoMessage;
			}
			_InfoMessageEvent += value;
		}
		remove
		{
			_InfoMessageEvent -= value;
		}
	}


	private event EventHandler<FbInfoMessageEventArgs> _InternalInfoMessageEvent = null;

	private void OnInternalInfoMessage(object sender, FbInfoMessageEventArgs e)
	{
		// Tracer.Trace(GetType(), "OnInternalInfoMessage()", "etype: {0}.", e.GetType().Name);

		if (_InfoMessageEvent == null)
			return;

		DbInfoMessageEventArgs internalContainerEvent = new(e);
		_InfoMessageEvent.Invoke(sender, internalContainerEvent);
	}

	public DbConnectionWrapper(IDbConnection connection, Action<FbConnection> sqlConnectionCreatedObserver = null)
	{
		if (connection is not FbConnection)
		{
			InvalidOperationException ex = new(Resources.InvalidConnectionType);
			throw ex;
		}

		_Connection = connection;
		_SqlConnectionCreatedObserver = sqlConnectionCreatedObserver;
	}


	private FbConnection GetAsSqlConnection()
	{
		return (FbConnection)_Connection;
	}

	public DbConnection CloneAndOpenConnection()
	{
		FbConnection sqlConnection = ((ICloneable)GetAsSqlConnection()).Clone() as FbConnection;
		_SqlConnectionCreatedObserver?.Invoke(sqlConnection);

		sqlConnection.Open();
		return sqlConnection;
	}
}
