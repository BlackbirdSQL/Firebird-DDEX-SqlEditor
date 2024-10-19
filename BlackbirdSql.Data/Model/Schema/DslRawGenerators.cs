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

namespace BlackbirdSql.Data.Model.Schema;


internal class DslRawGenerators : AbstractDslSchema
{

	public DslRawGenerators() : base()
	{
		// Evs.Trace(GetType(), "DslRawGenerators.DslRawGenerators");
	}


	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslRawGenerators.GetCommandText");

		var sql = new StringBuilder();

		sql.Append(
@"EXECUTE BLOCK
	RETURNS (
		GENERATOR_CATALOG varchar(50),
		GENERATOR_SCHEMA varchar(50),
		SEQUENCE_GENERATOR varchar(100),
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
	FROM rdb$generators
	ORDER BY rdb$generator_name
	INTO :GENERATOR_CATALOG, :GENERATOR_SCHEMA, :SEQUENCE_GENERATOR, :IS_SYSTEM_FLAG, :GENERATOR_ID, :IDENTITY_SEED, :IDENTITY_INCREMENT
	DO BEGIN
		EXECUTE STATEMENT 'SELECT gen_id(' || SEQUENCE_GENERATOR || ', 0) FROM rdb$database' INTO :IDENTITY_CURRENT;
		:IDENTITY_CURRENT = :IDENTITY_CURRENT - :IDENTITY_INCREMENT;
		IF (:IDENTITY_CURRENT < :IDENTITY_SEED - 1) THEN
		BEGIN
			:IDENTITY_CURRENT = :IDENTITY_SEED - 1;
		END
        SUSPEND;
    END
END");


		return sql;
	}



	#endregion
}
