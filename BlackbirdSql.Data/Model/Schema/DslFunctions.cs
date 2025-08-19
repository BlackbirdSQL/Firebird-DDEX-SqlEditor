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

using System.Collections.Generic;
using System.Data;
using System.Text;
using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.Data.Model.Schema;


internal class DslFunctions : AbstractDslSchema
{
	public DslFunctions() : base()
	{
	}

	protected override void InitializeParameters(IDbConnection connection)
	{
		InitializeColumnsList(connection, "RDB$FUNCTIONS");
	}


	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslFunctions.GetCommandText");

		StringBuilder sql = new();
		StringBuilder where = new();


		string functionSource = HasColumn("RDB$FUNCTION_SOURCE") && HasColumn("RDB$FUNCTION_BLR")
			? @"					(CASE WHEN RDB$FUNCTION_SOURCE IS NULL AND RDB$FUNCTION_BLR IS NOT NULL THEN
						 cast(RDB$FUNCTION_BLR as blob sub_type 1)
					ELSE
						 RDB$FUNCTION_SOURCE
					END)"
			: "''";
		string orderBy = HasColumn("RDB$PACKAGE_NAME") ? "PACKAGE_NAME, FUNCTION_NAME" : "FUNCTION_NAME";
		string packageName = HasColumn("RDB$PACKAGE_NAME") ? "RDB$PACKAGE_NAME" : "(CASE WHEN RDB$SYSTEM_FLAG <> 1 THEN 'USER' ELSE 'SYSTEM' END)";

		sql.Append($@"SELECT
					null AS FUNCTION_CATALOG,
					null AS FUNCTION_SCHEMA,
					RDB$FUNCTION_NAME AS FUNCTION_NAME,
					(CASE WHEN RDB$SYSTEM_FLAG <> 1 THEN 0 ELSE 1 END) AS IS_SYSTEM_FLAG,
					RDB$FUNCTION_TYPE AS FUNCTION_TYPE,
					RDB$QUERY_NAME AS QUERY_NAME,
					RDB$MODULE_NAME AS FUNCTION_MODULE_NAME,
					RDB$ENTRYPOINT AS FUNCTION_ENTRY_POINT,
					RDB$RETURN_ARGUMENT AS RETURN_ARGUMENT,
					RDB$DESCRIPTION AS DESCRIPTION,
					{functionSource} AS SOURCE,
					{packageName} AS PACKAGE_NAME
				FROM RDB$FUNCTIONS");

		if (restrictions != null)
		{
			int index = 0;

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
				where.Append($"RDB$FUNCTION_NAME = @p{index++}");
			}

			/* IS_SYSTEM_FLAG */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.Append($"RDB$SYSTEM_FLAG = @p{index++}");
			}
		}

		if (where.Length > 0)
		{
			sql.Append($" WHERE {where} ");
		}

		sql.Append($" ORDER BY {orderBy}");

		// Evs.Trace(GetType(), nameof(GetCommandText), $"Sql: {sql}");

		return sql;
	}


	#endregion
}
