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

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Data.Model.Schema;

internal class DslIndexes : AbstractDslSchema
{
	public DslIndexes() : base()
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
					idx.RDB$RELATION_NAME AS TABLE_NAME,
					idx.RDB$INDEX_NAME AS INDEX_NAME,
					idx.RDB$FOREIGN_KEY AS FOREIGN_KEY,
					(CASE WHEN idx.RDB$INDEX_INACTIVE <> 1 THEN 0 ELSE 1 END) AS IS_INACTIVE,
					idx.RDB$UNIQUE_FLAG AS UNIQUE_FLAG,
				    (SELECT COUNT(*) FROM RDB$RELATION_CONSTRAINTS rel
				    WHERE rel.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY' AND rel.RDB$INDEX_NAME = idx.RDB$INDEX_NAME AND rel.RDB$RELATION_NAME = idx.RDB$RELATION_NAME) as PRIMARY_KEY,
					(SELECT COUNT(*) FROM RDB$RELATION_CONSTRAINTS rel
					WHERE rel.RDB$CONSTRAINT_TYPE = 'UNIQUE' AND rel.RDB$INDEX_NAME = idx.RDB$INDEX_NAME AND rel.RDB$RELATION_NAME = idx.RDB$RELATION_NAME) as UNIQUE_KEY,
					(CASE WHEN idx.RDB$SYSTEM_FLAG <> 1 THEN
						 0
					ELSE
						 1
					END) AS IS_SYSTEM_FLAG,
					(CASE WHEN idx.RDB$INDEX_TYPE <> 1 THEN 0 ELSE 1 END) AS IS_DESCENDING_FLAG,
					idx.RDB$DESCRIPTION AS DESCRIPTION,
					(CASE WHEN idx.RDB$EXPRESSION_SOURCE IS NULL AND idx.RDB$EXPRESSION_BLR IS NOT NULL THEN
						cast(idx.RDB$EXPRESSION_BLR as blob sub_type 1)
					ELSE
						idx.RDB$EXPRESSION_SOURCE
					END) AS EXPRESSION,
					(CASE WHEN idx.RDB$EXPRESSION_SOURCE IS NULL AND idx.RDB$EXPRESSION_BLR IS NULL THEN
						0
					ELSE
						1
					END) AS IS_COMPUTED_FLAG
				FROM RDB$INDICES idx");

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
				where.Append($"idx.RDB$RELATION_NAME = @p{index++}");
			}

			/* INDEX_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.Append($"idx.RDB$INDEX_NAME = @p{index++}");
			}
		}

		if (where.Length > 0)
		{
			sql.Append($" WHERE {where} ");
		}

		sql.Append(" ORDER BY TABLE_NAME, INDEX_NAME");

		// Evs.Trace(GetType(), nameof(GetCommandText), "Sql: {0}.", sql);

		return sql;
	}

	protected override void ProcessResult(DataTable schema, string connectionString, string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslIndexes.ProcessResult");

		schema.Columns.Add("IS_PRIMARY", typeof(bool));
		schema.Columns.Add("IS_UNIQUE", typeof(bool));
		schema.Columns.Add("IS_FOREIGNKEY", typeof(bool));

		schema.Columns.Add("IS_COMPUTED", typeof(bool));
		schema.Columns.Add("IS_DESCENDING", typeof(bool));

		schema.AcceptChanges();
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			row["IS_COMPUTED"] = Convert.ToBoolean(row["IS_COMPUTED_FLAG"]);
			row["IS_DESCENDING"] = Convert.ToBoolean(row["IS_DESCENDING_FLAG"]);

			row["IS_UNIQUE"] = !(row["UNIQUE_FLAG"] == DBNull.Value || Convert.ToInt32(row["UNIQUE_FLAG"], CultureInfo.InvariantCulture) == 0);

			row["IS_PRIMARY"] = !(row["PRIMARY_KEY"] == DBNull.Value || Convert.ToInt32(row["PRIMARY_KEY"], CultureInfo.InvariantCulture) == 0);

			row["IS_FOREIGNKEY"] = (row["FOREIGN_KEY"] != DBNull.Value);
		}

		schema.EndLoadData();

		schema.Columns.Remove("PRIMARY_KEY");
		schema.Columns.Remove("FOREIGN_KEY");
		schema.Columns.Remove("UNIQUE_FLAG");

		schema.Columns.Remove("IS_COMPUTED_FLAG");
		schema.Columns.Remove("IS_DESCENDING_FLAG");

		schema.AcceptChanges();
	}

	#endregion
}
