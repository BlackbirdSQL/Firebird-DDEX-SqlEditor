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
			if (x.IsEmpty || y.IsEmpty)
				return x.IsEmpty == y.IsEmpty;

			if (StringComparer.OrdinalIgnoreCase.Compare(x.DataSource, y.DataSource) == 0
				&& StringComparer.OrdinalIgnoreCase.Compare(x.Database, y.Database) == 0)
			{
				return x.Alternate == y.Alternate;
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
	private readonly bool _Alternate;

	private readonly int _HashCode;

	public bool IsEmpty => string.IsNullOrWhiteSpace(_DataSource);
	public string Database => _Database;
	public string DataSource => _DataSource;
	public bool Alternate => _Alternate;



	public DatabaseLocation(DbConnectionStringBuilder csb, bool alternate)
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
		_Alternate = alternate;

		string text = _DataSource + "//" + _Database + "//" + (_Alternate ? "Alter" : "");

		_HashCode = text.ToLowerInvariant().GetHashCode();
	}

	public DatabaseLocation(IVsDataExplorerNode node, bool alternate)
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
		_Alternate = alternate;

		string text = _DataSource + "//" + _Database + "//" + (_Alternate ? "Alter" : "");

		_HashCode = text.ToLowerInvariant().GetHashCode();
	}

	public static IEqualityComparer<DatabaseLocation> CreateComparer()
	{
		return new DatabaseLocationComparer();
	}

	public bool Matches(DbConnectionStringBuilder csb)
	{
		if (csb is FbConnectionStringBuilder fbcsb
			&& string.Compare(fbcsb.DataSource, DataSource, StringComparison.Ordinal) == 0
			&& string.Compare(fbcsb.Database, Database, StringComparison.Ordinal) == 0)
		{
			return true;
		}
		return false;
	}
}
