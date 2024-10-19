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


internal class DslRawTriggers : AbstractDslSchema
{
	public DslRawTriggers() : base()
	{
		// Evs.Trace(GetType(), "DslRawTriggers.DslRawTriggers");
	}



	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslRawTriggers.GetCommandText");

		StringBuilder sql = new ();

		string transientRestrictions = restrictions != null && !string.IsNullOrEmpty(restrictions[2])
			? $"WHERE trg.rdb$relation_name = '{restrictions[2]}'" : "";

		/*
		 * 
 		 * rdb$flags info
		 *	TRG_sql  0x1:			Show trigger in db structure
		 *	TRG_ignore_perm 0x2:	Ignore trigger permissions when executing

		 * 
		 * What this block does...
		 * 
		 * (Bypasses the -901 bug in FirebirdSql for parameterized EXECUTE BLOCKs by inserting any parameters here.)
		 * 
		 * rdb$flags info
		 *	TRG_sql  0x1:			Show trigger in db structure
		 *	TRG_ignore_perm 0x2:	Ignore trigger permissions when executing
		 * 
		 * Selects triggers
		 * then
		 * For each trigger
		 *		Flags it as an is-identity if rdb$trigger_type == 1
		 *		Selects it's dependencies 
		 *			(and also each dependencies index segment if it is on a 'PRIMARY KEY' index)
		 *		then
		 *			For each dependency (if not null)
		 *				adds dependency to the trigger dependency count
		 *				concatenates the field name to the trigger dependency fields string
		 *		then
		 *			if index rdb$constraint_type is null or dependency count is != 1
		 *				sets the is-identity flag to false
		 *	
		 *	To summate
		 *		If a trigger has rdb$trigger_type == 1 it is tentavely set as is-identity.
		 *		Then if it has only one (singular) dependency and that dependency is on a singular segment of a 
		 *		'PRIMARY KEY' index then it remains as is-identity if it was else it's is-identity is set
		 *		to false.
		*/


		sql.AppendFormat(@"SELECT
	-- :TRIGGER_NAME, :TABLE_NAME
	trg.rdb$trigger_name AS TRIGGER_NAME, trg.rdb$relation_name AS TABLE_NAME,
	trg.rdb$description AS DESCRIPTION,
	-- :IS_SYSTEM_FLAG
	(CASE WHEN trg.rdb$system_flag <> 1 THEN 0 ELSE 1 END) AS IS_SYSTEM_FLAG,
	-- :TRIGGER_TYPE
	trg.rdb$trigger_type AS TRIGGER_TYPE,
	-- :IS_INACTIVE
	(CASE WHEN trg.rdb$trigger_inactive <> 1 THEN false ELSE true END) AS IS_INACTIVE,
	-- :PRIORITY
	trg.rdb$trigger_sequence AS PRIORITY,
	-- :EXPRESSION for parser
	(CASE WHEN trg.rdb$trigger_source IS NULL AND trg.rdb$trigger_blr IS NOT NULL THEN

		REPLACE(REPLACE(REPLACE(REPLACE(
			REPLACE(REPLACE(REPLACE(
				REPLACE(REPLACE(TRIM(cast(trg.rdb$trigger_blr as blob sub_type 1)), ', ', ','), ',  ', ','),
			',   ', ','), ',    ', ','), ',     ', ','),
		',      ', ','), ',       ', ','), ',        ', ','), ',        ', ',')

	ELSE

		TRIM(trg.rdb$trigger_source)

	END) AS EXPRESSION,
	--Initial value of :IS_IDENTITY for parser
	(CASE WHEN trg.rdb$trigger_sequence = 1 AND trg.rdb$trigger_type = 1 THEN true ELSE false END) AS IS_IDENTITY
FROM rdb$triggers trg
{0}
ORDER BY trg.rdb$trigger_name", transientRestrictions);

		// Evs.Trace(sql.ToString());

		return sql;
	}


	#endregion
}
