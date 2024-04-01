#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data.Common;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Model;



namespace BlackbirdSql.Common.Model;

public sealed class DefaultSqlEditorStrategy : IBSqlEditorStrategy, IDisposable
{
	private readonly ConnectionPropertyAgent _DefaultConnectionInfo;

	private readonly bool _isOnline;

	public string DatabaseName
	{
		get
		{
			NotImplementedException ex = new("Not implemented in " + GetType().FullName);
			Diag.Dug(ex);
			throw ex;
		}
	}

	public bool IsOnline => _isOnline;

	public bool IsDw => false;

	public object MetadataProviderProvider => null;

	public EnEditorMode Mode => EnEditorMode.Standard;

	public IBSqlEditorExtendedCommandHandler ExtendedCommandHandler => null;

	public DefaultSqlEditorStrategy()
	{
	}

	public DefaultSqlEditorStrategy(ConnectionPropertyAgent defaultConnectionInfo)
		: this(defaultConnectionInfo, isOnline: false)
	{
	}

	public DefaultSqlEditorStrategy(ConnectionPropertyAgent defaultConnectionInfo, bool isOnline)
	{
		if (defaultConnectionInfo == null)
		{
			ArgumentNullException ex = new("defaultUiConnectionInfo");
			Diag.Dug(ex);
			throw ex;
		}

		_DefaultConnectionInfo = defaultConnectionInfo;
		_isOnline = isOnline;
	}

	public DefaultSqlEditorStrategy(DbConnectionStringBuilder csb, bool isOnline = false)
	{
		/*
		if (csb == null)
		{
			ArgumentNullException ex = new("csb");
			Diag.Dug(ex);
			throw ex;
		}
		*/

		if (csb != null)
		{
			_DefaultConnectionInfo = new ConnectionPropertyAgent();
			_DefaultConnectionInfo.Parse(csb.ConnectionString);
		}
		_isOnline = isOnline;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public IBSqlEditorErrorTaskFactory GetErrorTaskFactory()
	{
		return null;
	}

	public string ResolveSqlCmdVariable(string variableName)
	{
		return null;
	}

	public SqlConnectionStrategy CreateConnectionStrategy()
	{
		SqlConnectionStrategy sqlConnectionStrategy = new SqlConnectionStrategy();
		if (_DefaultConnectionInfo != null)
		{
			sqlConnectionStrategy.SetConnectionInfo(_DefaultConnectionInfo);
		}

		return sqlConnectionStrategy;
	}
}
