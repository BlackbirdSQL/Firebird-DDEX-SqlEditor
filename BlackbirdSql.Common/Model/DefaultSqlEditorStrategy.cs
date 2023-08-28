#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model.Interfaces;

using FirebirdSql.Data.FirebirdClient;
using System.Data.Common;

namespace BlackbirdSql.Common.Model;


public sealed class DefaultSqlEditorStrategy : ISqlEditorStrategy, IDisposable
{
	private readonly UIConnectionInfo _DefaultUiConnectionInfo;

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

	public bool IsDw { get; set; }

	public IMetadataProviderProvider MetadataProviderProvider => null;

	public EnEditorMode Mode => EnEditorMode.Standard;

	public ISqlEditorExtendedCommandHandler ExtendedCommandHandler => null;

	public DefaultSqlEditorStrategy()
	{
	}

	public DefaultSqlEditorStrategy(UIConnectionInfo defaultUiConnectionInfo)
		: this(defaultUiConnectionInfo, isOnline: false)
	{
	}

	public DefaultSqlEditorStrategy(UIConnectionInfo defaultUiConnectionInfo, bool isOnline)
	{
		if (defaultUiConnectionInfo == null)
		{
			ArgumentNullException ex = new("defaultUiConnectionInfo");
			Diag.Dug(ex);
			throw ex;
		}

		_DefaultUiConnectionInfo = defaultUiConnectionInfo;
		_isOnline = isOnline;
	}

	public DefaultSqlEditorStrategy(DbConnectionStringBuilder csb, bool isOnline = false)
	{
		if (csb == null)
		{
			ArgumentNullException ex = new("csb");
			Diag.Dug(ex);
			throw ex;
		}

		_DefaultUiConnectionInfo = new UIConnectionInfo();
		_DefaultUiConnectionInfo.Parse(csb.ConnectionString);
		_isOnline = isOnline;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public ISqlEditorErrorTaskFactory GetErrorTaskFactory()
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
		if (_DefaultUiConnectionInfo != null)
		{
			sqlConnectionStrategy.SetConnectionInfo(_DefaultUiConnectionInfo);
		}

		return sqlConnectionStrategy;
	}
}
