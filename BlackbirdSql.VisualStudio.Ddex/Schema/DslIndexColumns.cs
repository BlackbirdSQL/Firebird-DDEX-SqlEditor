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

using System.Text;


namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslIndexColumns : DslSchema
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
					idx.rdb$index_name AS CONSTRAINT_NAME,
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					idx.rdb$relation_name AS TABLE_NAME,
					idx.rdb$expression_source AS EXPRESSION,
					(CASE WHEN seg.rdb$field_name IS NOT NULL OR idx.rdb$expression_source IS NULL THEN
						seg.rdb$field_name
					ELSE
						idx.rdb$index_name
					END) AS COLUMN_NAME,
					seg.rdb$field_position AS ORDINAL_POSITION,
					idx.rdb$index_name AS INDEX_NAME,
					(CASE WHEN idx.rdb$expression_source IS NULL THEN
						FALSE
					ELSE
						TRUE
					END) AS COMPUTED
				FROM rdb$indices idx
					LEFT JOIN rdb$index_segments seg ON idx.rdb$index_name = seg.rdb$index_name");

		if (restrictions != null)
		{
			var index = 0;

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

			/* COLUMN_NAME */
			if (restrictions.Length >= 5 && restrictions[4] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.AppendFormat("seg.rdb$field_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY TABLE_NAME, INDEX_NAME, ORDINAL_POSITION");

		return sql;
	}

	#endregion
}
