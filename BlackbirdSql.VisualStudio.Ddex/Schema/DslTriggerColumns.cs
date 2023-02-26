//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Text;
using BlackbirdSql.Common;

namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslTriggerColumns : DslColumns
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		/*
		 * What this does...
		 * 
		 * Selects non-null dependency fields on a left inner join with a trigger
		 * Selects fields on a left inner join on dependency then
		 * 
		 * performs selects as per DslColumns() having completed the 'triggers'
		 *	part.
		 *	
		*/

		sql.AppendFormat(
			@"SELECT
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					dep.rdb$depended_on_name AS TABLE_NAME,
					dep.rdb$field_name AS COLUMN_NAME,
					dep.rdb$dependent_name AS TRIGGER_NAME,
				    null AS COLUMN_DATA_TYPE,
				    fld.rdb$field_sub_type AS COLUMN_SUB_TYPE,
					CAST(fld.rdb$field_length AS integer) AS COLUMN_SIZE,
					CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
					CAST(fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
					CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
					CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
					rfr.rdb$field_position AS ORDINAL_POSITION,
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
					(CASE WHEN trg.rdb$trigger_sequence = 1 AND trg.rdb$flags = 1 and trg.rdb$trigger_type = 1 THEN
						true
					ELSE
						false
					END) AS IS_AUTOINCREMENT,
					(SELECT COUNT(*)
                        FROM rdb$dependencies fd
                        WHERE fd.rdb$field_name IS NOT NULL AND fd.rdb$dependent_name = dep.rdb$dependent_name AND fd.rdb$depended_on_name = dep.rdb$depended_on_name
						GROUP BY fd.rdb$dependent_name, fd.rdb$depended_on_name)
                    AS TRIGGER_DEPENDENCYCOUNT,
					'Trigger' AS PARENT_TYPE
				FROM rdb$dependencies dep
                INNER JOIN rdb$triggers trg
                    ON trg.rdb$trigger_name = dep.rdb$dependent_name AND trg.rdb$relation_name = dep.rdb$depended_on_name
				INNER JOIN rdb$relation_fields rfr
					ON rfr.rdb$relation_name = dep.rdb$depended_on_name AND rfr.rdb$field_name = dep.rdb$field_name
				INNER JOIN rdb$fields fld
					ON rfr.rdb$field_source = fld.rdb$field_name
				LEFT OUTER JOIN rdb$character_sets cs
					ON cs.rdb$character_set_id = fld.rdb$character_set_id
				LEFT OUTER JOIN rdb$collations coll
					ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)
				LEFT OUTER JOIN rdb$relation_constraints con
					ON dep.rdb$depended_on_name IS NOT NULL AND con.rdb$relation_name = dep.rdb$depended_on_name AND con.rdb$constraint_type = 'PRIMARY KEY'
				LEFT OUTER JOIN rdb$index_segments seg 
					ON con.rdb$index_name IS NOT NULL AND seg.rdb$index_name = con.rdb$index_name AND seg.rdb$field_name = dep.rdb$field_name",
				MajorVersionNumber >= 3 ? "rfr.rdb$identity_type" : "null");

		where.Append(" WHERE dep.rdb$field_name IS NOT NULL");

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

			/* TRIGGER_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat(" AND dep.rdb$dependent_name = @p{0}", index++);
			}


			/* COLUMN_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				where.AppendFormat(" AND dep.rdb$field_name = @p{0}", index++);
			}
		}

		sql.Append(where.ToString());

		sql.Append(" ORDER BY TRIGGER_NAME, ORDINAL_POSITION");


		return sql;
	}


	#endregion
}
