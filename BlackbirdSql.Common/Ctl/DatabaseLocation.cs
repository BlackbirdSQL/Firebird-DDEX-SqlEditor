// Microsoft.SqlServer.Dac.Extensions, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Schema.Tasks.Sql.DesignServices.SqlDatabaseLocation

using System;
using System.Collections.Generic;
using System.Data.Common;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;
using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Common.Ctl;

public readonly struct DatabaseLocation
{
	private class DatabaseLocationComparer : IEqualityComparer<DatabaseLocation>
	{
		public bool Equals(DatabaseLocation x, DatabaseLocation y)
		{
			if (x.IsEmpty || y.IsEmpty)
				return x.IsEmpty == y.IsEmpty;

			return StringComparer.OrdinalIgnoreCase.Compare(x.DataSource, y.DataSource) == 0
				&& StringComparer.OrdinalIgnoreCase.Compare(x.Database, y.Database) == 0
				&& x.TargetType == y.TargetType;
		}

		public int GetHashCode(DatabaseLocation obj)
		{
			return obj._HashCode;
		}
	}

	private readonly string _DataSource;
	private readonly string _Database;
	private readonly EnModelTargetType _TargetType;

	private readonly int _HashCode;

	public bool IsEmpty => string.IsNullOrWhiteSpace(_DataSource);
	public string Database => _Database;
	public string DataSource => _DataSource;
	public EnModelTargetType TargetType => _TargetType;



	public DatabaseLocation(DbConnectionStringBuilder csb, EnModelTargetType targetType)
	{
		if (csb == null)
		{
			ArgumentNullException ex = new("connectionString");
			Diag.Dug(ex);
			throw ex;
		}

		CsbAgent csa = csb as CsbAgent;

		if (string.IsNullOrWhiteSpace(csa.DataSource))
		{
			ArgumentNullException ex = new("dataSource");
			Diag.Dug(ex);
			throw ex;
		}
		if (string.IsNullOrEmpty(csa.Database))
		{
			ArgumentNullException ex = new("Database");
			Diag.Dug(ex);
			throw ex;
		}
		_DataSource = csa.DataSource;
		_Database = csa.Database;
		_TargetType = targetType;

		string text = _DataSource + "//" + _Database + "//" + _TargetType.ToString();

		_HashCode = text.ToLowerInvariant().GetHashCode();
	}

	public DatabaseLocation(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		if (node == null)
		{
			ArgumentNullException ex = new("node");
			Diag.Dug(ex);
			throw ex;
		}

		CsbAgent csa = new(node);

		if (string.IsNullOrWhiteSpace(csa.DataSource))
		{
			ArgumentNullException ex = new("DataSource");
			Diag.Dug(ex);
			throw ex;
		}
		if (string.IsNullOrEmpty(csa.Database))
		{
			ArgumentNullException ex = new("Database");
			Diag.Dug(ex);
			throw ex;
		}
		_DataSource = csa.DataSource;
		_Database = csa.Database;
		_TargetType = targetType;

		string text = _DataSource + "//" + _Database + "//" + _TargetType.ToString();

		_HashCode = text.ToLowerInvariant().GetHashCode();
	}

	public static IEqualityComparer<DatabaseLocation> CreateComparer()
	{
		return new DatabaseLocationComparer();
	}

	public bool Matches(DbConnectionStringBuilder csb)
	{
		if (csb is CsbAgent csa
			&& string.Compare(csa.DataSource, DataSource, StringComparison.Ordinal) == 0
			&& string.Compare(csa.Database, Database, StringComparison.Ordinal) == 0)
		{
			return true;
		}
		return false;
	}
}
