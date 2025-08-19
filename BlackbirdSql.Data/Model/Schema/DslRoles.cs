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
using System.Text;
using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.Data.Model.Schema;


internal class DslRoles : AbstractDslSchema
{
	public DslRoles() : base()
	{
	}

	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		StringBuilder sql = new();
		StringBuilder where = new();

		sql.Append(
			@"SELECT
					RDB$ROLE_NAME AS ROLE_NAME,
					RDB$OWNER_NAME AS OWNER_NAME
				FROM RDB$ROLES");

		if (restrictions != null)
		{
			int index = 0;

			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
				where.Append($"RDB$ROLE_NAME = @p{index++}");
			}
		}

		if (where.Length > 0)
		{
			sql.Append($" WHERE {where} ");
		}

		sql.Append(" ORDER BY ROLE_NAME");

		return sql;
	}

	#endregion
}
