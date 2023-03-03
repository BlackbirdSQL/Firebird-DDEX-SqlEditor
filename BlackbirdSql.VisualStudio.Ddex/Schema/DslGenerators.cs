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

using System;
using System.Data;
using System.Globalization;
using System.Text;

using BlackbirdSql.Common;


namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslGenerators : DslSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.Append(
@"EXECUTE BLOCK
	RETURNS (
		GENERATOR_CATALOG varchar(50),
		GENERATOR_SCHEMA varchar(50),
		GENERATOR_NAME varchar(100),
		IS_SYSTEM_FLAG int,
		GENERATOR_ID smallint,
		GENERATOR_IDENTITY int,
		IDENTITY_SEED bigint,
		IDENTITY_INCREMENT int,
		IDENTITY_CURRENT bigint)
AS
BEGIN
	FOR SELECT
		null, null, rdb$generator_name, (CASE WHEN rdb$system_flag <> 1 THEN 0 ELSE 1 END),
		rdb$generator_id, rdb$initial_value, rdb$generator_increment 
	FROM rdb$generators");

		if (restrictions != null)
		{
			// var index = 0;

			/* GENERATOR_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* GENERATOR_SCHEMA	*/
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* GENERATOR_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				// Cannot pass params to execute block
				// where.AppendFormat("rdb$generator_name = @p{0}", index++);
				where.AppendFormat("rdb$generator_name = '{0}'", restrictions[2].ToString());
			}

			/* IS_SYSTEM_GENERATOR	*/
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				// Cannot pass params to execute block
				// where.AppendFormat("rdb$system_flag = @p{0}", index++);
				where.AppendFormat("rdb$system_flag = '{0}'", restrictions[3].ToString());
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(@"
	WHERE {0} ", where.ToString());
		}

		sql.Append(@"
	ORDER BY rdb$generator_name");

		sql.Append(@"
	INTO :GENERATOR_CATALOG, :GENERATOR_SCHEMA, :GENERATOR_NAME, :IS_SYSTEM_FLAG, :GENERATOR_ID, :IDENTITY_SEED, :IDENTITY_INCREMENT
	DO BEGIN
		EXECUTE STATEMENT 'SELECT gen_id(' || GENERATOR_NAME || ', 0) FROM rdb$database' INTO :IDENTITY_CURRENT;
		:IDENTITY_CURRENT = :IDENTITY_CURRENT - :IDENTITY_INCREMENT;
		IF (:IDENTITY_CURRENT < :IDENTITY_SEED) THEN
		BEGIN
			:IDENTITY_CURRENT = :IDENTITY_SEED;
		END
        SUSPEND;
    END
END");


		return sql;
	}


	#endregion
}
