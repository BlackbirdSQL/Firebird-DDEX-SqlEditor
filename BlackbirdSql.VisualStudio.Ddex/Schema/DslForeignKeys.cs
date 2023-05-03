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

using System.Text;

namespace BlackbirdSql.VisualStudio.Ddex.Schema;

internal class DslForeignKeys : DslSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.Append(
			@"SELECT
	null AS CONSTRAINT_CATALOG,
	null AS CONSTRAINT_SCHEMA,
	r.rdb$constraint_name AS CONSTRAINT_NAME,
	null AS TABLE_CATALOG,
	null AS TABLE_SCHEMA,
	r.rdb$relation_name AS TABLE_NAME,
	r.rdb$index_name as INDEX_NAME,
	null as REFERENCED_TABLE_CATALOG,
	null as REFERENCED_TABLE_SCHEMA,
	refidx.rdb$relation_name as REFERENCED_TABLE_NAME,
	refidx.rdb$index_name as REFERENCED_INDEX_NAME,
	r.rdb$deferrable AS IS_DEFERRABLE,
	r.rdb$initially_deferred AS INITIALLY_DEFERRED,
	ref.rdb$match_option AS MATCH_OPTION,
	CASE (ref.rdb$update_rule)
        WHEN 'CASCADE' THEN 1
        WHEN 'SET NULL' THEN 2
        WHEN 'SET_DEFAULT' THEN 3
        ELSE 0
    END AS UPDATE_ACTION,
	CASE (ref.rdb$delete_rule)
        WHEN 'CASCADE' THEN 1
        WHEN 'SET NULL' THEN 2
        WHEN 'SET_DEFAULT' THEN 3
        ELSE 0
    END AS DELETE_ACTION
FROM rdb$relation_constraints r
INNER JOIN rdb$ref_constraints ref
	ON ref.rdb$constraint_name = r.rdb$constraint_name
INNER JOIN rdb$indices idx
	ON idx.rdb$index_name = r.rdb$index_name
INNER JOIN rdb$indices refidx
	ON refidx.rdb$index_name = idx.rdb$foreign_key");

		where.Append("r.rdb$constraint_type = 'FOREIGN KEY'");

		if (restrictions != null)
		{
			var index = 0;

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
				where.AppendFormat(" AND r.rdb$relation_name = @p{0}", index++);
			}

			/* CONSTRAINT_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				where.AppendFormat(" AND rel.rdb$constraint_name = @p{0}", index++);
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

	#endregion
}
