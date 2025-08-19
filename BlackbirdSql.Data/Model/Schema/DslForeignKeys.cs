/*
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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

namespace BlackbirdSql.Data.Model.Schema;

internal class DslForeignKeys : AbstractDslSchema
{
	public DslForeignKeys() : base()
	{
	}


	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslForeignKeys.DslForeignKeys");

		StringBuilder sql = new();
		StringBuilder where = new();

		sql.Append(
			@"SELECT
	null AS CONSTRAINT_CATALOG,
	null AS CONSTRAINT_SCHEMA,
	r.RDB$CONSTRAINT_NAME AS CONSTRAINT_NAME,
	null AS TABLE_CATALOG,
	null AS TABLE_SCHEMA,
	r.RDB$RELATION_NAME AS TABLE_NAME,
	r.RDB$INDEX_NAME as INDEX_NAME,
	null as REFERENCED_TABLE_CATALOG,
	null as REFERENCED_TABLE_SCHEMA,
	refidx.RDB$RELATION_NAME as REFERENCED_TABLE_NAME,
	refidx.RDB$INDEX_NAME as REFERENCED_INDEX_NAME,
	r.RDB$DEFERRABLE AS IS_DEFERRABLE,
	r.RDB$INITIALLY_DEFERRED AS INITIALLY_DEFERRED,
	ref.RDB$MATCH_OPTION AS MATCH_OPTION,
	CASE (ref.RDB$UPDATE_RULE)
        WHEN 'CASCADE' THEN 1
        WHEN 'SET NULL' THEN 2
        WHEN 'SET_DEFAULT' THEN 3
        ELSE 0
    END AS UPDATE_ACTION,
	CASE (ref.RDB$DELETE_RULE)
        WHEN 'CASCADE' THEN 1
        WHEN 'SET NULL' THEN 2
        WHEN 'SET_DEFAULT' THEN 3
        ELSE 0
    END AS DELETE_ACTION,
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
FROM RDB$RELATION_CONSTRAINTS r
INNER JOIN RDB$REF_CONSTRAINTS ref
	ON ref.RDB$CONSTRAINT_NAME = r.RDB$CONSTRAINT_NAME
INNER JOIN RDB$INDICES idx
	ON idx.RDB$INDEX_NAME = r.RDB$INDEX_NAME
INNER JOIN RDB$INDICES refidx
	ON refidx.RDB$INDEX_NAME = idx.RDB$FOREIGN_KEY");

		where.Append("r.RDB$CONSTRAINT_TYPE = 'FOREIGN KEY'");

		if (restrictions != null)
		{
			int index = 0;

			/* CONSTRAINT_CATALOG	*/
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* CONSTRAINT_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* TABLE_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat(" AND r.RDB$RELATION_NAME = @p{0}", index++);
			}

			/* CONSTRAINT_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				where.AppendFormat(" AND rel.RDB$CONSTRAINT_NAME = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(@"
WHERE {0} ", where.ToString());
		}

		sql.Append(@"
ORDER BY TABLE_NAME, CONSTRAINT_NAME");

		return sql;
	}


	protected override void ProcessResult(DataTable schema, string connectionString, string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslForeignKeys.ProcessResult");

		schema.Columns.Add("IS_PRIMARY", typeof(bool));
		schema.Columns.Add("IS_UNIQUE", typeof(bool));
		schema.Columns.Add("IS_COMPUTED", typeof(bool));

		schema.AcceptChanges();
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			row["IS_COMPUTED"] = Convert.ToBoolean(row["IS_COMPUTED_FLAG"]);

			row["IS_UNIQUE"] = !(row["UNIQUE_FLAG"] == DBNull.Value || Convert.ToInt32(row["UNIQUE_FLAG"], CultureInfo.InvariantCulture) == 0);
			row["IS_PRIMARY"] = !(row["PRIMARY_KEY"] == DBNull.Value || Convert.ToInt32(row["PRIMARY_KEY"], CultureInfo.InvariantCulture) == 0);
		}

		schema.EndLoadData();

		schema.Columns.Remove("PRIMARY_KEY");
		schema.Columns.Remove("UNIQUE_FLAG");
		schema.Columns.Remove("IS_COMPUTED_FLAG");

		schema.AcceptChanges();
	}

	#endregion
}
