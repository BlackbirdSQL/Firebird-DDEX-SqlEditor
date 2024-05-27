#region Assembly Microsoft.Data.Tools.Schema.Sql, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\Microsoft.Data.Tools.Schema.Sql.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Data.Common;



namespace BlackbirdSql.Sys;


public sealed class NativeDbConnectionWrapperProxy : IBsNativeDbConnectionWrapper
{
	private readonly IBsNativeDbConnectionWrapper _NativeObject = null;

	public IBsNativeDbConnectionWrapper NativeObject => _NativeObject;


	public string DataSource => _NativeObject.DataSource;

	public string ServerVersion => _NativeObject.ServerVersion;

	public event EventHandler<DbInfoMessageEventArgs> InfoMessageEvent
	{
		add { _NativeObject.InfoMessageEvent += value; }
		remove { _NativeObject.InfoMessageEvent -= value; }
	}


	public NativeDbConnectionWrapperProxy(IDbConnection connection, Action<DbConnection> sqlConnectionCreatedObserver = null)
	{
		_NativeObject = DbNative.CreateDbConnectionWrapper(connection, sqlConnectionCreatedObserver);
	}


	public DbConnection CloneAndOpenConnection() => _NativeObject.CloneAndOpenConnection();
}
