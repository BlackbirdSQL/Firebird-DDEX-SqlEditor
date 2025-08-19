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

using System.Collections.Generic;
using System.Data;
using System.Text;
using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.Data.Model.Schema;


internal class DslRawTriggerDependencies : AbstractDslSchema
{
	public DslRawTriggerDependencies() : base()
	{
		// Evs.Trace(GetType(), "DslRawTriggerDependencies.DslRawTriggerDependencies");
	}



	protected override void InitializeParameters(IDbConnection connection)
	{
		InitializeColumnsList(connection, "RDB$GENERATORS");
	}





	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslRawTriggerDependencies.GetCommandText");


		StringBuilder sql = new ();

		string identityTypeColName = MajorVersionNumber >= 3 ? "fd_rfr.RDB$IDENTITY_TYPE" : "0";
		string generatorColName = MajorVersionNumber >= 3 ? "= fd_rfr.RDB$GENERATOR_NAME" : "IS NULL";

		string transientRestrictions = restrictions != null && !string.IsNullOrEmpty(restrictions[2])
			? $"WHERE trg.RDB$RELATION_NAME = '{restrictions[2]}'" : "";

		string initialValue = HasColumn("RDB$INITIAL_VALUE") ? "MAX(fd_gen.RDB$INITIAL_VALUE)" : "0";
		string increment = HasColumn("RDB$GENERATOR_INCREMENT") ? "MAX(fd_gen.RDB$GENERATOR_INCREMENT)" : "1";


		sql.Append($@"SELECT
	-- :TRIGGER_NAME
	trg.RDB$TRIGGER_NAME AS TRIGGER_NAME,
	fd_gen.RDB$GENERATOR_NAME AS SEQUENCE_GENERATOR,
	LIST(TRIM(fd.RDB$FIELD_NAME), ', ') AS DEPENDENCY_FIELDS,
	{initialValue} AS IDENTITY_SEED, 
	{increment} AS IDENTITY_INCREMENT,
	MAX({identityTypeColName}) AS IDENTITY_TYPE

FROM RDB$TRIGGERS trg

INNER JOIN RDB$DEPENDENCIES fd
	ON fd.RDB$DEPENDENT_NAME = trg.RDB$TRIGGER_NAME AND fd.RDB$DEPENDED_ON_NAME = trg.RDB$RELATION_NAME

INNER JOIN RDB$RELATION_CONSTRAINTS fd_con
	ON fd_con.RDB$RELATION_NAME = fd.RDB$DEPENDED_ON_NAME AND fd_con.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY'

LEFT OUTER JOIN RDB$INDEX_SEGMENTS fd_seg
	ON fd_con.RDB$INDEX_NAME IS NOT NULL AND fd_seg.RDB$INDEX_NAME = fd_con.RDB$INDEX_NAME AND fd_seg.RDB$FIELD_NAME = fd.RDB$FIELD_NAME

LEFT OUTER JOIN RDB$RELATION_FIELDS fd_rfr
	ON fd_seg.RDB$INDEX_NAME IS NOT NULL AND fd_rfr.RDB$RELATION_NAME = fd_con.RDB$RELATION_NAME AND fd_rfr.RDB$FIELD_NAME = fd_seg.RDB$FIELD_NAME

LEFT OUTER JOIN RDB$GENERATORS fd_gen
	--[= fd_rfr.RDB$GENERATOR_NAME | IS NULL]~1~
    ON fd_gen.RDB$GENERATOR_NAME {generatorColName}
{transientRestrictions}
GROUP BY TRIGGER_NAME, SEQUENCE_GENERATOR
ORDER BY trg.RDB$TRIGGER_NAME");

		// Evs.Trace(GetType(), nameof(GetCommandText), $"Sql: {sql}");

		return sql;
	}





	#endregion
}
