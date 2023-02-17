/*
 *	This is an override of the FirebirdClient Schema
 *	We're maintaining the same structure so that it's easy to overload any GetSchema's that may need it.
 *	We still use the original Firebird metadata manifest pulled from the Firebird assembly
 *	
 *    The contents of this file are subject to the Initial
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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Globalization;
using System.Text;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Common;


namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslTriggers : DslSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.Append(
			@"SELECT
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					rfr.rdb$relation_name AS TABLE_NAME,
					rfr.rdb$trigger_name AS TRIGGER_NAME,
					rfr.rdb$system_flag AS IS_SYSTEM_TRIGGER,
					rfr.rdb$trigger_type AS TRIGGER_TYPE,
					rfr.rdb$trigger_inactive AS IS_INACTIVE,
					rfr.rdb$trigger_sequence AS SEQUENCENO,
					(CASE WHEN rfr.rdb$trigger_source IS NULL AND rfr.rdb$trigger_blr IS NOT NULL THEN
                        cast(rfr.rdb$trigger_blr as blob sub_type 1)
					ELSE
                        rfr.rdb$trigger_source
					END) AS EXPRESSION,
					(rfr.rdb$description) AS DESCRIPTION,
					(CASE WHEN rfr.rdb$flags = 1 and rfr.rdb$system_flag = 0 and rfr.rdb$trigger_type = 1 THEN
						TRUE
					ELSE
						FALSE
					END) AS IS_AUTOINCREMENT,
					LIST(TRIM(dep.rdb$field_name), ', ') AS DEPENDENT_FIELDS
				FROM rdb$triggers rfr
				LEFT OUTER JOIN rdb$dependencies dep
					ON dep.rdb$depended_on_name = rfr.rdb$relation_name AND dep.rdb$dependent_name = rfr.rdb$trigger_name");

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
				where.AppendFormat("rdb$relation_name = @p{0}", index++);
			}

			/* TRIGGER_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.AppendFormat("rdb$trigger_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" GROUP BY TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TRIGGER_NAME, IS_SYSTEM_TRIGGER, TRIGGER_TYPE, IS_INACTIVE, SEQUENCENO, EXPRESSION, DESCRIPTION, IS_AUTOINCREMENT");
		sql.Append(" ORDER BY TABLE_NAME, TRIGGER_NAME");

		return sql;
	}

	protected override void ProcessResult(DataTable schema)
	{
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			if (row["IS_SYSTEM_TRIGGER"] == DBNull.Value ||
				Convert.ToInt32(row["IS_SYSTEM_TRIGGER"], CultureInfo.InvariantCulture) == 0)
			{
				row["IS_SYSTEM_TRIGGER"] = false;
			}
			else
			{
				row["IS_SYSTEM_TRIGGER"] = true;
			}
		}	

		schema.EndLoadData();
		schema.AcceptChanges();

	}

	#endregion
}
