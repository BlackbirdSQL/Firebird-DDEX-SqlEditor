﻿/*
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

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Globalization;
using System.Text;


namespace BlackbirdSql.Data.Model.Schema;

internal class DslIndexes : AbstractDslSchema
{
	internal DslIndexes() : base()
	{
	}

	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslIndexes.GetCommandText");

		// BlackbirdSql added ForeignKey

		StringBuilder sql = new();
		StringBuilder where = new();

		sql.Append(
			@"SELECT
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					idx.rdb$relation_name AS TABLE_NAME,
					idx.rdb$index_name AS INDEX_NAME,
					idx.rdb$foreign_key AS FOREIGN_KEY,
					(CASE WHEN idx.rdb$index_inactive <> 1 THEN false ELSE true END) AS IS_INACTIVE,
					idx.rdb$unique_flag AS UNIQUE_FLAG,
				    (SELECT COUNT(*) FROM rdb$relation_constraints rel
				    WHERE rel.rdb$constraint_type = 'PRIMARY KEY' AND rel.rdb$index_name = idx.rdb$index_name AND rel.rdb$relation_name = idx.rdb$relation_name) as PRIMARY_KEY,
					(SELECT COUNT(*) FROM rdb$relation_constraints rel
					WHERE rel.rdb$constraint_type = 'UNIQUE' AND rel.rdb$index_name = idx.rdb$index_name AND rel.rdb$relation_name = idx.rdb$relation_name) as UNIQUE_KEY,
					(CASE WHEN idx.rdb$system_flag <> 1 THEN
						 0
					ELSE
						 1
					END) AS IS_SYSTEM_FLAG,
					(CASE WHEN idx.rdb$index_type <> 1 THEN false ELSE true END) AS IS_DESCENDING,
					idx.rdb$description AS DESCRIPTION,
					(CASE WHEN idx.rdb$expression_source IS NULL AND idx.rdb$expression_blr IS NOT NULL THEN
						cast(idx.rdb$expression_blr as blob sub_type 1)
					ELSE
						idx.rdb$expression_source
					END) AS EXPRESSION,
					(CASE WHEN idx.rdb$expression_source IS NULL AND idx.rdb$expression_blr IS NULL THEN
						false
					ELSE
						true
					END) AS IS_COMPUTED
				FROM rdb$indices idx");

		if (restrictions != null)
		{
			int index = 0;

			/* TABLE_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* TABLE_SCHEMA	*/
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* TABLE_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat("idx.rdb$relation_name = @p{0}", index++);
			}

			/* INDEX_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.AppendFormat("idx.rdb$index_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY TABLE_NAME, INDEX_NAME");

		// Evs.Trace(GetType(), nameof(GetCommandText), "Sql: {0}.", sql);

		return sql;
	}

	protected override void ProcessResult(DataTable schema, string connectionString, string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslIndexes.ProcessResult");

		schema.BeginLoadData();
		schema.Columns.Add("IS_PRIMARY", typeof(bool));
		schema.Columns.Add("IS_UNIQUE", typeof(bool));
		schema.Columns.Add("IS_FOREIGNKEY", typeof(bool));

		foreach (DataRow row in schema.Rows)
		{
			row["IS_UNIQUE"] = !(row["UNIQUE_FLAG"] == DBNull.Value || Convert.ToInt32(row["UNIQUE_FLAG"], CultureInfo.InvariantCulture) == 0);

			row["IS_PRIMARY"] = !(row["PRIMARY_KEY"] == DBNull.Value || Convert.ToInt32(row["PRIMARY_KEY"], CultureInfo.InvariantCulture) == 0);

			row["IS_FOREIGNKEY"] = (row["FOREIGN_KEY"] != DBNull.Value);
		}

		schema.EndLoadData();
		schema.AcceptChanges();

		schema.Columns.Remove("PRIMARY_KEY");
		schema.Columns.Remove("FOREIGN_KEY");
		schema.Columns.Remove("UNIQUE_FLAG");
	}

	#endregion
}
