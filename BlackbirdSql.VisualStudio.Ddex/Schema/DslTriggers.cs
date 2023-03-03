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
using System.Reflection;


namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslTriggers : DslSchema
{
	/// <summary>
	/// If set to a value of 0 or 1 adds the conditional for system_flag.
	/// </summary>
	protected int _systemFlag = -1;

	/// <summary>
	/// If set to a value of 0 or 1 filters the result set on the final
	/// calculated value of IS_IDENTITY
	/// </summary>
	protected int _identityFlag = -1;

	public DslTriggers() : base()
	{
	}

	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		string identityType = "null";
		string generatorSelector = "IS NULL";
		string unknownSequence = Properties.Resources.UnknownSequence;

		if (MajorVersionNumber >= 3)
		{
			identityType = "fd_rfr.rdb$identity_type";
			generatorSelector = "= fd_rfr.rdb$generator_name";
		}

		/*
		 * What this block does...
		 * 
		 * (Bypasses the -901 bug in FirebirdSql for parameterized EXECUTE BLOCKs by inserting any parameters here.)
		 * 
		 * Selects triggers
		 * then
		 * For each trigger
		 *		Flags it as an is-identity if rdb$flags == 1 AND rdb$trigger_type == 1
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
		 *		If a trigger has rdb$flags == 1 AND rdb$trigger_type == 1 it is tentavely set as is-identity.
		 *		Then if it has only one (singular) dependency and that dependency is on a singular segment of a 
		 *		'PRIMARY KEY' index then it remains as is-identity if it was else it's is-identity is set
		 *		to false.
		*/
		sql.Append(@"EXECUTE BLOCK
	RETURNS (
		TABLE_CATALOG varchar(50), TABLE_SCHEMA varchar(50), TABLE_NAME varchar(100), TRIGGER_NAME varchar(100),
		IS_SYSTEM_FLAG int, TRIGGER_TYPE bigint, IS_INACTIVE boolean, DESC smallint,
		PRIORITY smallint, EXPRESSION blob sub_type 1, DESCRIPTION blob sub_type 1,
		DEPENDENCY_FIELDS blob sub_type 1, DEPENDENCY_COUNT int, IS_IDENTITY boolean, SEQUENCE_GENERATOR varchar(50),
		IDENTITY_SEED bigint, IDENTITY_INCREMENT int, IDENTITY_CURRENT bigint)
AS
DECLARE DEPENDENCY_FIELD varchar(50);
DECLARE CONSTRAINT_TYPE varchar(20);
DECLARE SEGMENT_FIELD varchar(50);
DECLARE IDENTITY_TYPE smallint;
-- :IDENTITY_FLAG is Firefox's interpretation of an identity field
-- We care about it if and only if :SEQUENCE_GENERATOR is not null
DECLARE IDENTITY_FLAG int;
BEGIN
	FOR
		SELECT
            -- :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :TRIGGER_NAME
			null, null, trg.rdb$relation_name, trg.rdb$trigger_name,
			-- :IS_SYSTEM_FLAG
			(CASE WHEN trg.rdb$system_flag <> 1 THEN 0 ELSE 1 END),
			-- :TRIGGER_TYPE
			trg.rdb$trigger_type,
			-- :IS_INACTIVE
			(CASE WHEN trg.rdb$trigger_inactive <> 1 THEN false ELSE true END),
			-- :DESC
			trg.rdb$trigger_sequence,
			-- :EXPRESSION
			(CASE WHEN trg.rdb$trigger_source IS NULL AND trg.rdb$trigger_blr IS NOT NULL THEN
				cast(trg.rdb$trigger_blr as blob sub_type 1)
			ELSE
				trg.rdb$trigger_source
			END),
			-- :DESCRIPTION
			trg.rdb$description,
			-- Initial value of :IS_IDENTITY
			(CASE WHEN trg.rdb$flags = 1 AND trg.rdb$trigger_sequence = 1 AND trg.rdb$trigger_type = 1 THEN true ELSE false END)
		FROM rdb$triggers trg");

		if (_systemFlag == 1)
		{
			where.Append("trg.rdb$system_flag = 1");
		}
		else if (_systemFlag == 0)
		{
			
			where.Append("trg.rdb$system_flag <> 1");
		}

		if (restrictions != null)
		{
			// var index = 0;

			/* TABLE_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* TABLE_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* TABLE_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				// Cannot pass params to execute block
				where.AppendFormat("trg.rdb$relation_name = '{0}'", restrictions[2]);
				// where.AppendFormat("trg.rdb$relation_name = @p{0}", index++);
			}

			/* TRIGGER_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				// Cannot pass params to execute block
				where.AppendFormat("trg.rdb$trigger_name = '{0}'", restrictions[3]);
				// where.AppendFormat("trg.rdb$trigger_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(@"
WHERE {0}", where.ToString());
		}

		sql.AppendFormat(@"
ORDER BY trg.rdb$trigger_name
		INTO :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :TRIGGER_NAME, :IS_SYSTEM_FLAG, :TRIGGER_TYPE, :IS_INACTIVE,
            :PRIORITY, :EXPRESSION, :DESCRIPTION, :IS_IDENTITY
	-- Loop through each trigger
	DO BEGIN
		-- We want to get a comma-separated list of dependency fields her and the count.
		-- Further, if it still qualifies as IS_IDENTITY see if we can extract the generator info

		:DEPENDENCY_FIELDS = ''; :DEPENDENCY_COUNT = 0; :SEQUENCE_GENERATOR = null; :IDENTITY_SEED = 0;
		:IDENTITY_INCREMENT = 0; :IDENTITY_CURRENT = 0; :IDENTITY_TYPE = null;

		FOR
			SELECT
				-- :DEPENDENCY_FIELD, :CONSTRAINT_TYPE, :SEGMENT_FIELD
				fd.rdb$field_name, fd_con.rdb$constraint_type, fd_seg.rdb$field_name,
                -- :IDENTITY_TYPE [fd_rfr.rdb$identity_type|null]~0~
				{0},
				-- :SEQUENCE_GENERATOR, :IDENTITY_SEED, :IDENTITY_INCREMENT
				fd_gen.rdb$generator_name, fd_gen.rdb$initial_value, fd_gen.rdb$generator_increment
			FROM rdb$dependencies fd
			LEFT OUTER JOIN rdb$relation_constraints fd_con
				ON fd_con.rdb$relation_name = fd.rdb$depended_on_name AND fd_con.rdb$constraint_type = 'PRIMARY KEY'
			LEFT OUTER JOIN rdb$index_segments fd_seg
				ON fd_con.rdb$index_name IS NOT NULL AND fd_seg.rdb$index_name = fd_con.rdb$index_name AND fd_seg.rdb$field_name = fd.rdb$field_name
			LEFT OUTER JOIN rdb$relation_fields fd_rfr
				ON fd_seg.rdb$index_name IS NOT NULL AND fd_rfr.rdb$relation_name = fd_con.rdb$relation_name AND fd_rfr.rdb$field_name = fd_seg.rdb$field_name
			LEFT OUTER JOIN rdb$generators fd_gen
                -- [= fd_rfr.rdb$generator_name|IS NULL]~1~
				ON fd_gen.rdb$generator_name {1} 
			WHERE fd.rdb$field_name IS NOT NULL AND fd.rdb$dependent_name = :TRIGGER_NAME AND fd.rdb$depended_on_name = :TABLE_NAME
			INTO :DEPENDENCY_FIELD, :CONSTRAINT_TYPE, :SEGMENT_FIELD, :IDENTITY_TYPE, :SEQUENCE_GENERATOR, :IDENTITY_SEED, :IDENTITY_INCREMENT
		DO BEGIN
			-- If there are no segments cannot be an identity trigger
			IF (:SEGMENT_FIELD IS NULL) THEN
				:IS_IDENTITY = false;
			:DEPENDENCY_COUNT = :DEPENDENCY_COUNT + 1;
			IF (DEPENDENCY_FIELDS <> '') THEN
				:DEPENDENCY_FIELDS = :DEPENDENCY_FIELDS || ', ';
			:DEPENDENCY_FIELDS = :DEPENDENCY_FIELDS || TRIM(DEPENDENCY_FIELD);

			IF (:DEPENDENCY_COUNT = 1 AND :SEQUENCE_GENERATOR IS NOT NULL) THEN
			BEGIN
				EXECUTE STATEMENT 'SELECT gen_id(' || SEQUENCE_GENERATOR || ', 0) FROM rdb$database' INTO :IDENTITY_CURRENT;
				:IDENTITY_CURRENT = :IDENTITY_CURRENT - :IDENTITY_INCREMENT;
				IF (:IDENTITY_CURRENT < :IDENTITY_SEED) THEN
				BEGIN
					:IDENTITY_CURRENT = :IDENTITY_SEED;
				END
			END
		END
        -- If the trigger dependency count is not 1 it can't be an identity field.
		IF (:DEPENDENCY_COUNT <> 1) THEN
			:IS_IDENTITY = false;

        IF (:IS_IDENTITY AND :SEQUENCE_GENERATOR IS NOT NULL) THEN
        BEGIN
            -- There is a generator so :IDENTITY_FLAG determines if is-identity still holds true
            IF (:IDENTITY_FLAG IS NULL OR :IDENTITY_TYPE = 0) THEN
                :IS_IDENTITY = false;
        END
        IF (:IS_IDENTITY AND :SEQUENCE_GENERATOR IS NULL) THEN
		BEGIN
            -- [Unidentifiable]~2~
            :SEQUENCE_GENERATOR = '{2}';
			:IDENTITY_SEED = -1;
			:IDENTITY_INCREMENT = -1;
			:IDENTITY_CURRENT = -1;
        END

		IF (NOT :IS_IDENTITY) THEN
		BEGIN
			:SEQUENCE_GENERATOR = NULL;
			:IDENTITY_SEED = 0;
			:IDENTITY_INCREMENT = 0;
			:IDENTITY_CURRENT = 0;
		END", identityType, generatorSelector, unknownSequence);
		 
		if (_identityFlag != -1)
		{
			sql.AppendFormat(@"
		IF(:IS_IDENTITY = {0}) THEN
		BEGIN
			SUSPEND;
		END", _identityFlag > 0 ? "true" : "false");
		}
		else
		{
			sql.Append(@"
		SUSPEND;");
		}
		sql.Append(@"
	END
END");

		// Diag.Trace(sql.ToString());

		return sql;
	}


	#endregion
}
