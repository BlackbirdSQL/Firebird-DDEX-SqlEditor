// Microsoft.SqlServer.Dac.Extensions, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Schema.Tasks.Sql.DesignServices.SqlDatabaseLocation

using System;
using System.Collections.Generic;
using System.Data.Common;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Model;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Services;
// using Microsoft.Data.SqlClient;



namespace BlackbirdSql.Common.Ctl;


public readonly struct DatabaseLocation
{
	private class DatabaseLocationComparer : IEqualityComparer<DatabaseLocation>
	{
		public bool Equals(DatabaseLocation x, DatabaseLocation y)
		{
			if (x.Empty || y.Empty)
				return x.Empty == y.Empty;

			if (StringComparer.OrdinalIgnoreCase.Compare(x.Database, y.Database) == 0 && StringComparer.OrdinalIgnoreCase.Compare(x.DataSource, y.DataSource) == 0)
			{
				return StringComparer.OrdinalIgnoreCase.Compare(x.UserName, y.UserName) == 0;
			}
			return false;
		}

		public int GetHashCode(DatabaseLocation obj)
		{
			return obj._HashCode;
		}
	}

	private readonly string _DataSource;


	private readonly string _Database;

	// private readonly bool _integratedAuth;

	private readonly string _UserName;

	private readonly int _HashCode;

	// private readonly SqlAuthenticationMethodUtils.AuthenticationMethod _authentication;

	public bool Empty => string.IsNullOrWhiteSpace(_DataSource);

	public string Database => _Database;

	public string DataSource => _DataSource;

	public string UserName => _UserName;

	// public bool IntegratedSecurity => _integratedAuth;

	// public SqlAuthenticationMethodUtils.AuthenticationMethod Authentication => _authentication;

	public DatabaseLocation(DbConnectionStringBuilder csb)
	{
		if (csb == null)
		{
			ArgumentNullException ex = new("connectionString");
			Diag.Dug(ex);
			throw ex;
		}

		FbConnectionStringBuilder fbcsb = csb as FbConnectionStringBuilder;

		if (string.IsNullOrWhiteSpace(fbcsb.DataSource))
		{
			ArgumentNullException ex = new("dataSource");
			Diag.Dug(ex);
			throw ex;
		}
		if (string.IsNullOrEmpty(fbcsb.Database))
		{
			ArgumentNullException ex = new("database");
			Diag.Dug(ex);
			throw ex;
		}
		_DataSource = fbcsb.DataSource;
		_Database = fbcsb.Database;
		_UserName = fbcsb.UserID;

		string text = _Database + _DataSource + _UserName;

		_HashCode = text.GetHashCode();
	}

	public DatabaseLocation(IVsDataExplorerNode node)
	{
		if (node == null)
		{
			ArgumentNullException ex = new("node");
			Diag.Dug(ex);
			throw ex;
		}

		MonikerAgent moniker = new(node);

		if (string.IsNullOrWhiteSpace(moniker.Server))
		{
			ArgumentNullException ex = new("Server");
			Diag.Dug(ex);
			throw ex;
		}
		if (string.IsNullOrEmpty(moniker.Database))
		{
			ArgumentNullException ex = new("Database");
			Diag.Dug(ex);
			throw ex;
		}
		_DataSource = moniker.Server;
		_Database = moniker.Database;
		_UserName = moniker.User;

		string text = _Database + _DataSource + _UserName;

		_HashCode = text.GetHashCode();
	}

	public static IEqualityComparer<DatabaseLocation> CreateComparer()
	{
		return new DatabaseLocationComparer();
	}

	public bool Matches(DbConnectionStringBuilder csb)
	{
		if (csb is FbConnectionStringBuilder fbcsb
			&& string.Compare(fbcsb.DataSource, DataSource, StringComparison.Ordinal) == 0
			&& string.Compare(fbcsb.UserID, UserName, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return true;
		}
		return false;
	}
}
