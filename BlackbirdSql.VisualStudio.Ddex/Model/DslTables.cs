/*
 *	This is an override of the FirebirdClient Schema
 *	We're maintaining the same structure so that it's easy to overload any GetSchema's that may need it.
 *	We still use the original Firebird metadata manifest pulled from the Firebird assembly
 *	
 *	  The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Text;

using BlackbirdSql.Core;

namespace BlackbirdSql.VisualStudio.Ddex.Model;


internal class DslTables : DslSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		// BlackbirdSql added index, foreign key and trigger counts
		sql.Append(
			@"SELECT
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					rfr.rdb$relation_name AS TABLE_NAME,
					null AS TABLE_TYPE,
					(CASE WHEN rfr.rdb$system_flag <> 1 THEN
                        0
					ELSE
                        1
					END) IS_SYSTEM_FLAG,
					rfr.rdb$owner_name AS OWNER_NAME,
					rfr.rdb$description AS DESCRIPTION,
					rfr.rdb$view_source AS VIEW_SOURCE,
					(SELECT COUNT(*) FROM rdb$triggers trg WHERE trg.rdb$relation_name = rfr.rdb$relation_name) AS TRIGGER_COUNT,
					(SELECT COUNT(*) FROM rdb$indices idx WHERE idx.rdb$relation_name = rfr.rdb$relation_name) AS INDEX_COUNT,
					(SELECT COUNT(*) FROM rdb$relation_constraints con
						INNER JOIN rdb$ref_constraints ref ON ref.rdb$constraint_name = con.rdb$constraint_name
						INNER JOIN rdb$indices tempidx ON tempidx.rdb$index_name = con.rdb$index_name
						INNER JOIN rdb$indices refidx ON refidx.rdb$index_name = tempidx.rdb$foreign_key
						WHERE con.rdb$relation_name = rfr.rdb$relation_name) AS FOREIGNKEY_COUNT
				FROM rdb$relations rfr
				WHERE rfr.rdb$view_source IS NULL");

		if (restrictions != null)
		{
			var index = 0;

			/* TABLE_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* TABLE_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* TABLE_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.Append(" AND ");

				where.AppendFormat("rdb$relation_name = @p{0}", index++);
			}

			/* TABLE_TYPE */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				where.Append(" AND ");

				switch (restrictions[3].ToString())
				{
					case "SYSTEM TABLE":
						where.Append("rdb$system_flag = 1");
						break;

					case "TABLE":
					default:
						where.Append("rdb$system_flag = 0");
						break;
				}
			}
		}

		if (where.Length > 0)
		{
			sql.Append(where.ToString());
		}

		sql.Append(" ORDER BY IS_SYSTEM_FLAG, OWNER_NAME, TABLE_NAME");

		// Diag.Trace(sql.ToString());
		return sql;
	}

	protected override void ProcessResult(DataTable schema)
	{
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			row["TABLE_TYPE"] = "TABLE";
			if (Convert.ToInt32(row["IS_SYSTEM_FLAG"]) == 1)
			{
				row["TABLE_TYPE"] = "SYSTEM_TABLE";
			}
			if (row["VIEW_SOURCE"] != DBNull.Value &&
				row["VIEW_SOURCE"].ToString().Length > 0)
			{
				row["TABLE_TYPE"] = "VIEW";
			}
		}

		schema.EndLoadData();
		schema.AcceptChanges();

		schema.Columns.Remove("VIEW_SOURCE");
	}

	#endregion
}
