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

using FirebirdSql.Data.FirebirdClient;

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
		IS_SYSTEM_GENERATOR smallint,
		GENERATOR_ID smallint,
		INITIAL_VALUE bigint,
		GENERATOR_INCREMENT int,
		NEXT_VALUE bigint)
AS
BEGIN
	FOR SELECT
		null, null, rdb$generator_name, rdb$system_flag, rdb$generator_id, rdb$initial_value, rdb$generator_increment 
	FROM rdb$generators");

		if (restrictions != null)
		{
			var index = 0;

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
				where.AppendFormat("rdb$generator_name = @p{0}", index++);
			}

			/* IS_SYSTEM_GENERATOR	*/
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

		sql.Append(" ORDER BY rdb$generator_name");

		sql.Append(
@" INTO :GENERATOR_CATALOG, :GENERATOR_SCHEMA, :GENERATOR_NAME, :IS_SYSTEM_GENERATOR, :GENERATOR_ID, :INITIAL_VALUE, :GENERATOR_INCREMENT
	DO BEGIN
		EXECUTE STATEMENT 'SELECT gen_id(' || GENERATOR_NAME || ', 0) FROM rdb$database' INTO :NEXT_VALUE;
        SUSPEND;
    END
END");


		return sql;
	}

	protected override void ProcessResult(DataTable schema)
	{
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			if (row["IS_SYSTEM_GENERATOR"] == DBNull.Value ||
				Convert.ToInt32(row["IS_SYSTEM_GENERATOR"], CultureInfo.InvariantCulture) == 0)
			{
				row["IS_SYSTEM_GENERATOR"] = false;
			}
			else
			{
				row["IS_SYSTEM_GENERATOR"] = true;
			}
		}

		schema.EndLoadData();
		schema.AcceptChanges();
	}

	#endregion
}
