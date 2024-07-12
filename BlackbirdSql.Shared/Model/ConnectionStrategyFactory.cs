#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data.Common;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Model;


public sealed class ConnectionStrategyFactory : IBConnectionStrategy, IDisposable
{

	public ConnectionStrategyFactory(DbConnectionStringBuilder csb, bool isOnline = false)
	{
		if (csb != null)
		{
			_DefaultConnectionInfo = new ConnectionPropertyAgent();
			_DefaultConnectionInfo.Parse(csb.ConnectionString);
		}

		_IsOnline = isOnline;
	}


	/*
	public DefaultConnectionStrategy()
	{
	}



	public DefaultConnectionStrategy(ConnectionPropertyAgent defaultConnectionInfo)
		: this(defaultConnectionInfo, isOnline: false)
	{
	}



	public DefaultConnectionStrategy(ConnectionPropertyAgent defaultConnectionInfo, bool isOnline)
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
	*/



	public void Dispose()
	{
		_DefaultConnectionInfo?.Dispose();
		GC.SuppressFinalize(this);
	}





	private readonly ConnectionPropertyAgent _DefaultConnectionInfo;

	private readonly bool _IsOnline;




	public string DatabaseName
	{
		get
		{
			NotImplementedException ex = new("Not implemented in " + GetType().FullName);
			Diag.Dug(ex);
			throw ex;
		}
	}

	public bool IsOnline => _IsOnline;

	public bool IsDw => false;

	public object MetadataProviderProvider => null;

	public EnEditorMode Mode => EnEditorMode.Standard;

	public IBExtendedCommandHandler ExtendedCommandHandler => null;




	public IBErrorTaskFactory GetErrorTaskFactory()
	{
		return null;
	}

	public ConnectionStrategy CreateConnectionStrategy()
	{
		ConnectionStrategy strategy = new ConnectionStrategy();
		if (_DefaultConnectionInfo != null)
		{
			strategy.SetConnectionInfo(_DefaultConnectionInfo);
		}

		return strategy;
	}
}
