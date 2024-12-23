﻿
using System;
using System.Data;
using System.Text;



namespace BlackbirdSql.Data.Model.Schema;


internal class DslUsers : AbstractDslSchema
{
	internal DslUsers() : base()
	{
	}

	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslFunctions.GetCommandText");

		StringBuilder sql = new();
		StringBuilder where = new($"TRIM(SEC$PLUGIN) = '{Authentication}'");

		sql.AppendFormat(
@"SELECT
	(CASE WHEN TRIM(SEC$PLUGIN) = 'Srp' THEN 'Srp' ELSE 'Legacy' END) AS PROTOCOL,
	SEC$USER_NAME AS USER_LOGIN,
	(CASE WHEN SEC$FIRST_NAME IS NULL THEN SEC$USER_NAME ELSE SEC$FIRST_NAME END) AS FIRST_NAME,
	SEC$MIDDLE_NAME AS MIDDLE_NAME,
	SEC$LAST_NAME AS LAST_NAME,
	(CASE WHEN SEC$ACTIVE IS NULL THEN true ELSE SEC$ACTIVE END) AS IS_ACTIVE,
	SEC$ADMIN AS IS_ADMIN,
	SEC$DESCRIPTION AS DESCRIPTION
FROM SEC$USERS");

		if (restrictions != null)
		{
			int index = 0;

			// User name.
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
				where.Append(" AND ");
				where.AppendFormat("SEC$USER_NAME = @p{0}", index++);
			}

		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY PROTOCOL DESC, USER_NAME");

		return sql;
	}



	protected override void ProcessResult(DataTable schema, string connectionString, string[] restrictions)
	{
		// Evs.Trace(GetType(), nameof(ProcessResult));

		schema.Columns.Add("USER_NAME", typeof(string));

		schema.AcceptChanges();
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			string name = row["FIRST_NAME"].ToString();

			if (row["MIDDLE_NAME"] != DBNull.Value)
				name += " " + row["MIDDLE_NAME"].ToString();
			if (row["LAST_NAME"] != DBNull.Value)
				name += " " + row["LAST_NAME"].ToString();

			row["USER_NAME"] = name;
		}


		schema.EndLoadData();

		// Remove not more needed columns
		schema.Columns.Remove("FIRST_NAME");
		schema.Columns.Remove("MIDDLE_NAME");
		schema.Columns.Remove("LAST_NAME");

		schema.AcceptChanges();

		// Evs.Trace("Rows returned: " + schema.Rows.Count);

	}


	#endregion
}
