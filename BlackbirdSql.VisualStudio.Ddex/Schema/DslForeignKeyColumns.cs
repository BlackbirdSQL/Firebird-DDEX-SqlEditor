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


internal class DslForeignKeyColumns : DslColumns
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.AppendFormat(
			@"SELECT
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					co.rdb$relation_name AS TABLE_NAME,
					coidxseg.rdb$field_name AS COLUMN_NAME,
					dep.rdb$dependent_name AS TRIGGER_NAME,
				    null AS COLUMN_DATA_TYPE,
				    fld.rdb$field_sub_type AS COLUMN_SUB_TYPE,
					CAST(fld.rdb$field_length AS integer) AS COLUMN_SIZE,
					CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
					CAST(fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
					CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
					CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
					coidxseg.rdb$field_position as ORDINAL_POSITION,
					null AS DOMAIN_CATALOG,
					null AS DOMAIN_SCHEMA,
					rfr.rdb$field_source AS DOMAIN_NAME,
					null AS SYSTEM_DATA_TYPE,
					rfr.rdb$default_source AS COLUMN_DEFAULT,
					(CASE WHEN fld.rdb$computed_source IS NULL AND fld.rdb$computed_blr IS NOT NULL THEN
						 cast(fld.rdb$computed_blr as blob sub_type 1)
					ELSE
						 fld.rdb$computed_source
					END) AS EXPRESSION,
					(CASE WHEN fld.rdb$computed_source IS NULL AND fld.rdb$computed_blr IS NULL THEN
						 FALSE
					ELSE
						 TRUE
					END) AS IS_COMPUTED,
					fld.rdb$dimensions AS COLUMN_ARRAY,
					coalesce(fld.rdb$null_flag, rfr.rdb$null_flag) AS COLUMN_NULLABLE,
				    0 AS IS_READONLY,
					fld.rdb$field_type AS FIELD_TYPE,
					null AS CHARACTER_SET_CATALOG,
					null AS CHARACTER_SET_SCHEMA,
					cs.rdb$character_set_name AS CHARACTER_SET_NAME,
					null AS COLLATION_CATALOG,
					null AS COLLATION_SCHEMA,
					coll.rdb$collation_name AS COLLATION_NAME,
					rfr.rdb$description AS DESCRIPTION,
					{0} AS IDENTITY_TYPE,
					(CASE WHEN seg.rdb$field_name IS NULL THEN
						FALSE
					ELSE
						TRUE
					END) AS IN_PRIMARYKEY,
					(CASE WHEN dep.rdb$dependent_name IS NOT NULL AND trg.rdb$trigger_name IS NOT NULL AND trg.rdb$trigger_sequence = 1 AND trg.rdb$flags = 1 and trg.rdb$trigger_type = 1 THEN
						true
					ELSE
						false
					END) AS IS_AUTOINCREMENT,
					(SELECT COUNT(*)
                        FROM rdb$dependencies fd
                        WHERE fd.rdb$field_name IS NOT NULL AND fd.rdb$dependent_name = trg.rdb$trigger_name AND fd.rdb$depended_on_name = trg.rdb$relation_name
						GROUP BY fd.rdb$dependent_name, fd.rdb$depended_on_name)
                    AS TRIGGER_DEPENDENCYCOUNT,
					'ForeignKey' AS PARENT_TYPE,
					null AS CONSTRAINT_CATALOG,
					null AS CONSTRAINT_SCHEMA,
					co.rdb$constraint_name AS CONSTRAINT_NAME,
					null as REFERENCED_TABLE_CATALOG,
					null as REFERENCED_TABLE_SCHEMA,
					refidx.rdb$relation_name as REFERENCED_TABLE_NAME,
					refidxseg.rdb$field_name AS REFERENCED_COLUMN_NAME,
					co.rdb$index_name AS INDEX_NAME
				FROM rdb$relation_constraints co
				INNER JOIN rdb$ref_constraints ref 
                    ON co.rdb$constraint_type = 'FOREIGN KEY' AND co.rdb$constraint_name = ref.rdb$constraint_name
				INNER JOIN rdb$indices tempidx 
                    ON co.rdb$index_name = tempidx.rdb$index_name
				INNER JOIN rdb$index_segments coidxseg 
                    ON co.rdb$index_name = coidxseg.rdb$index_name
				INNER JOIN rdb$indices refidx 
                    ON refidx.rdb$index_name = tempidx.rdb$foreign_key
				INNER JOIN rdb$index_segments refidxseg 
                    ON refidxseg.rdb$index_name = refidx.rdb$index_name AND refidxseg.rdb$field_position = coidxseg.rdb$field_position
				INNER JOIN rdb$relation_fields rfr
                    ON rfr.rdb$relation_name = co.rdb$relation_name AND rfr.rdb$field_name = coidxseg.rdb$field_name
				INNER JOIN rdb$fields fld
					ON rfr.rdb$field_source = fld.rdb$field_name
				LEFT JOIN rdb$character_sets cs
					ON cs.rdb$character_set_id = fld.rdb$character_set_id
				LEFT JOIN rdb$collations coll
					ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)
				LEFT JOIN rdb$relation_constraints con
					ON con.rdb$relation_name = rfr.rdb$relation_name AND con.rdb$constraint_type = 'PRIMARY KEY'
				LEFT JOIN rdb$index_segments seg 
					ON seg.rdb$index_name = con.rdb$index_name AND seg.rdb$field_name = rfr.rdb$field_name
                LEFT JOIN rdb$triggers trg
                    ON trg.rdb$relation_name = con.rdb$relation_name AND trg.rdb$trigger_sequence = 1 AND trg.rdb$flags = 1 and trg.rdb$trigger_type = 1
                        AND seg.rdb$index_name = con.rdb$index_name AND seg.rdb$field_name = rfr.rdb$field_name
				LEFT JOIN rdb$dependencies dep
					ON dep.rdb$field_name IS NOT NULL AND dep.rdb$depended_on_name = trg.rdb$relation_name AND dep.rdb$dependent_name = trg.rdb$trigger_name AND dep.rdb$field_name = seg.rdb$field_name",
			MajorVersionNumber >= 3 ? "rfr.rdb$identity_type" : "null");

		if (restrictions != null)
		{
			var index = 0;

			/* TABLE_CATALOG	*/
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
				where.AppendFormat("co.rdb$relation_name = @p{0}", index++);
			}

			/* CONSTRAINT_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				where.AppendFormat("co.rdb$constraint_name = @p{0}", index++);
			}

			/* COLUMN_NAME */
			if (restrictions.Length >= 5 && restrictions[4] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				where.AppendFormat("coidxseg.rdb$field_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY CONSTRAINT_NAME, ORDINAL_POSITION");

		return sql;
	}

	#endregion
}
