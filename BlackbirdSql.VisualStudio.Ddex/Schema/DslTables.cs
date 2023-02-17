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
using System.Globalization;
using System.Text;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Common;
using System.Windows.Media;
using BlackbirdSql.Common.Extensions.Commands;
using System.Reflection;

namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslTables : DslSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		// var where = new StringBuilder();

		// BlackbirdSql remove "SYSTEM TABLE" restriction. It's working against the ddex structures and
		// is making things difficult. Also we don't need VIEW_SOURCE here
		sql.Append(
			@"SELECT
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					rdb$relation_name AS TABLE_NAME,
					null AS TABLE_TYPE,
					(CASE WHEN rdb$system_flag IS NULL THEN
                        0
					ELSE
                        rdb$system_flag
					END) IS_SYSTEM_TABLE,
					rdb$owner_name AS OWNER_NAME,
					rdb$description AS DESCRIPTION,
					rdb$view_source AS VIEW_SOURCE
				FROM rdb$relations
				WHERE rdb$view_source IS NULL");

		// Intercept request from new query
		DataToolsCommands.CommandLastObjectType = DataToolsCommands.CommandObjectType;
		/*
		if (DataToolsCommands.CommandObjectType != DataToolsCommands.DataObjectType.None
			&& (restrictions == null || restrictions.Length < 3 || (restrictions.Length > 2 && restrictions[2] == null)))
		{
			// Diag.Trace("FILTERING TABLE LIST TO SYSTEM_FLAG: " + DataToolsCommands.CommandObjectType);
			if (DataToolsCommands.CommandObjectType == DataToolsCommands.DataObjectType.User)
				sql.Append(" AND rdb$system_flag = 0");
			else
				sql.Append(" AND rdb$system_flag = 1");

			DataToolsCommands.CommandObjectType = DataToolsCommands.DataObjectType.None;
		}
		*/

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
				sql.AppendFormat(" AND rdb$relation_name = @p{0}", index++);
			}

			/* TABLE_TYPE 
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				switch (restrictions[3].ToString())
				{
					case "VIEW":
						where.Append("rdb$view_source IS NOT NULL");
						break;

					case "SYSTEM TABLE":
						where.Append("rdb$view_source IS NULL and rdb$system_flag = 1");
						break;

					case "TABLE":
					default:
						where.Append("rdb$view_source IS NULL and rdb$system_flag = 0");
						break;
				}
			}
			*/
		}

		/* if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}
		*/

		sql.Append(" ORDER BY IS_SYSTEM_TABLE, OWNER_NAME, TABLE_NAME");

		// Diag.Trace(sql.ToString());

		return sql;
	}

	protected override void ProcessResult(DataTable schema)
	{
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			// Setting to true/false is pointless as the underlying column is already an int16 so we'll handle in sql.
			// You cannot summarily change the column type through assignment.
			if (Convert.ToInt32(row["IS_SYSTEM_TABLE"], CultureInfo.InvariantCulture) == 0)
			{
				// row["IS_SYSTEM_TABLE"] = false;
				row["TABLE_TYPE"] = "TABLE";
			}
			else
			{
				// row["IS_SYSTEM_TABLE"] = true;
				row["TABLE_TYPE"] = "SYSTEM_TABLE";
			}
			/* if (row["VIEW_SOURCE"] != null &&
				row["VIEW_SOURCE"].ToString().Length > 0)
			{
				row["TABLE_TYPE"] = "VIEW";
			} */
		}

		schema.EndLoadData();
		schema.AcceptChanges();

		// schema.Columns.Remove("VIEW_SOURCE");
	}

	#endregion
}
