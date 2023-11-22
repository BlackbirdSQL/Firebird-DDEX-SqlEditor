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
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.VisualStudio.Ddex.Model;

internal class DslFunctions : AbstractDslSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Tracer.Trace(GetType(), "DslFunctions.GetCommandText");

		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.AppendFormat(
			@"SELECT
					null AS FUNCTION_CATALOG,
					null AS FUNCTION_SCHEMA,
					rdb$function_name AS FUNCTION_NAME,
					(CASE WHEN rdb$system_flag <> 1 THEN 0 ELSE 1 END) AS IS_SYSTEM_FLAG,
					rdb$function_type AS FUNCTION_TYPE,
					rdb$query_name AS QUERY_NAME,
					rdb$module_name AS FUNCTION_MODULE_NAME,
					rdb$entrypoint AS FUNCTION_ENTRY_POINT,
					rdb$return_argument AS RETURN_ARGUMENT,
					rdb$description AS DESCRIPTION,
					(CASE WHEN rdb$function_source IS NULL AND rdb$function_blr IS NOT NULL THEN
						 cast(rdb$function_blr as blob sub_type 1)
					ELSE
						 rdb$function_source
					END) AS SOURCE,
					{0} AS PACKAGE_NAME
				FROM rdb$functions",
			MajorVersionNumber >= 3 ? "rdb$package_name" : "(CASE WHEN rdb$system_flag <> 1 THEN 'USER' ELSE 'SYSTEM' END)");

		if (restrictions != null)
		{
			var index = 0;

			/* FUNCTION_CATALOG	*/
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* FUNCTION_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* FUNCTION_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat("rdb$function_name = @p{0}", index++);
			}

			/* IS_SYSTEM_FLAG */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.AppendFormat("rdb$system_flag = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY PACKAGE_NAME, FUNCTION_NAME");

		return sql;
	}


	#endregion
}
