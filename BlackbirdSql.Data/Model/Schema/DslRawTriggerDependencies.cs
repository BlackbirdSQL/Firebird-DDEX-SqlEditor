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


internal class DslRawTriggerDependencies : AbstractDslSchema
{
	public DslRawTriggerDependencies() : base()
	{
		// Tracer.Trace(GetType(), "DslRawTriggerDependencies.DslRawTriggerDependencies");
	}

	

	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Tracer.Trace(GetType(), "DslRawTriggerDependencies.GetCommandText");

		StringBuilder sql = new ();

		string identityType = "0";
		string generatorSelector = "IS NULL";

		string transientRestrictions = restrictions != null && !string.IsNullOrEmpty(restrictions[2])
			? $"WHERE trg.rdb$relation_name = '{restrictions[2]}'" : "";

		if (MajorVersionNumber >= 3)
		{
			identityType = "fd_rfr.rdb$identity_type";
			generatorSelector = "= fd_rfr.rdb$generator_name";
		}


		sql.AppendFormat(@"SELECT
	-- :TRIGGER_NAME
	trg.rdb$trigger_name AS TRIGGER_NAME,
	fd_gen.rdb$generator_name AS SEQUENCE_GENERATOR,
	LIST(TRIM(fd.rdb$field_name), ', ') AS DEPENDENCY_FIELDS,
	MAX(fd_gen.rdb$initial_value) AS IDENTITY_SEED, 
	MAX(fd_gen.rdb$generator_increment) AS IDENTITY_INCREMENT,
	MAX({0}) AS IDENTITY_TYPE

FROM rdb$triggers trg

INNER JOIN rdb$dependencies fd
	ON fd.rdb$dependent_name = trg.rdb$trigger_name AND fd.rdb$depended_on_name = trg.rdb$relation_name

INNER JOIN rdb$relation_constraints fd_con
	ON fd_con.rdb$relation_name = fd.rdb$depended_on_name AND fd_con.rdb$constraint_type = 'PRIMARY KEY'

LEFT OUTER JOIN rdb$index_segments fd_seg
	ON fd_con.rdb$index_name IS NOT NULL AND fd_seg.rdb$index_name = fd_con.rdb$index_name AND fd_seg.rdb$field_name = fd.rdb$field_name

LEFT OUTER JOIN rdb$relation_fields fd_rfr
	ON fd_seg.rdb$index_name IS NOT NULL AND fd_rfr.rdb$relation_name = fd_con.rdb$relation_name AND fd_rfr.rdb$field_name = fd_seg.rdb$field_name

LEFT OUTER JOIN rdb$generators fd_gen
	--[= fd_rfr.rdb$generator_name | IS NULL]~1~
    ON fd_gen.rdb$generator_name {1}
{2}
GROUP BY TRIGGER_NAME, SEQUENCE_GENERATOR
ORDER BY trg.rdb$trigger_name", identityType, generatorSelector, transientRestrictions);

		// Tracer.Trace(sql.ToString());

		return sql;
	}





	#endregion
}
