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
	/// calculated value of IS_AUTOINCREMENT
	/// </summary>
	protected int _autoIncrement = -1;

	public DslTriggers() : base()
	{
	}

	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		var sql = new StringBuilder();
		var where = new StringBuilder();

		/*
		 * What this block does...
		 * 
		 * (Bypasses the -901 bug in FirebirdSql for parameterized EXECUTE BLOCKs by inserting any parameters here.)
		 * 
		 * Selects triggers
		 * then
		 * For each trigger
		 *		Flags it as an auto increment if rdb$flags == 1 AND rdb$trigger_type == 1
		 *		Selects it's dependencies 
		 *			(and also each dependencies index segment if it is on a 'PRIMARY KEY' index)
		 *		then
		 *			For each dependency (if not null)
		 *				adds dependency to the trigger dependency count
		 *				concatenates the field name to the trigger dependency fields string
		 *		then
		 *			if index rdb$constraint_type is null or dependency count is != 1
		 *				sets the auto increment flag to 0
		 *	
		 *	To summate
		 *		If a trigger has rdb$flags == 1 AND rdb$trigger_type == 1 it is tentavely set as auto-increment.
		 *		Then if it has only one (singular) dependency and that dependency is on a singular segment of a 
		 *		'PRIMARY KEY' index then it remains as auto-increment if it was else it's auto-increment is set
		 *		to false.
		*/
		sql.Append(@"EXECUTE BLOCK
	RETURNS (
		TABLE_CATALOG varchar(50), TABLE_SCHEMA varchar(50), TABLE_NAME varchar(100), TRIGGER_NAME varchar(100),
		IS_SYSTEM_TRIGGER smallint, TRIGGER_TYPE bigint, IS_INACTIVE smallint, SEQUENCENO smallint,
		EXPRESSION blob sub_type 1, DESCRIPTION blob sub_type 1, IS_AUTOINCREMENT smallint,
		DEPENDENCY_FIELDS blob sub_type 1, TRIGGER_DEPENDENCYCOUNT int)
AS
DECLARE DEPENDENCY_FIELD varchar(50);
DECLARE CONSTRAINT_TYPE varchar(20);
DECLARE SEGMENT_FIELD varchar(50);
BEGIN
	FOR
		SELECT
			null, null, trg.rdb$relation_name, trg.rdb$trigger_name,
			(CASE WHEN trg.rdb$system_flag = 1 THEN 1 ELSE 0 END),
			trg.rdb$trigger_type, trg.rdb$trigger_inactive, trg.rdb$trigger_sequence,
			(CASE WHEN trg.rdb$trigger_source IS NULL AND trg.rdb$trigger_blr IS NOT NULL THEN
				cast(trg.rdb$trigger_blr as blob sub_type 1)
			ELSE
				trg.rdb$trigger_source
			END),
			trg.rdb$description,
			(CASE WHEN trg.rdb$flags = 1 AND trg.rdb$trigger_type = 1 THEN 1 ELSE 0 END),
			'', 0
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

				where.AppendFormat("trg.rdb$relation_name = '{0}'", restrictions[2]);
				// where.AppendFormat("trg.rdb$relation_name = @p{0}", index++);
			}

			/* TRIGGER_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				where.AppendFormat("trg.rdb$trigger_name = '{0}'", restrictions[3]);
				// where.AppendFormat("trg.rdb$trigger_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(@"
		WHERE {0}", where.ToString());
		}

		sql.Append(@"
		ORDER BY trg.rdb$trigger_name
		INTO :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :TRIGGER_NAME, :IS_SYSTEM_TRIGGER, :TRIGGER_TYPE, :IS_INACTIVE,
			:SEQUENCENO, :EXPRESSION, :DESCRIPTION, :IS_AUTOINCREMENT, :DEPENDENCY_FIELDS, :TRIGGER_DEPENDENCYCOUNT
	DO BEGIN
		FOR
			SELECT
				fd.rdb$field_name, con.rdb$constraint_type, seg.rdb$field_name
			FROM rdb$dependencies fd
			LEFT OUTER JOIN rdb$relation_constraints con
				ON con.rdb$relation_name = fd.rdb$depended_on_name AND con.rdb$constraint_type = 'PRIMARY KEY'
			LEFT OUTER JOIN rdb$index_segments seg
				ON con.rdb$index_name IS NOT NULL AND seg.rdb$index_name = con.rdb$index_name AND seg.rdb$field_name = fd.rdb$field_name
			WHERE fd.rdb$field_name IS NOT NULL AND fd.rdb$dependent_name = :TRIGGER_NAME AND fd.rdb$depended_on_name = :TABLE_NAME
			INTO :DEPENDENCY_FIELD, :CONSTRAINT_TYPE, :SEGMENT_FIELD
		DO BEGIN
			IF (:SEGMENT_FIELD IS NULL) THEN
			BEGIN
				:IS_AUTOINCREMENT = 0;
			END
			:TRIGGER_DEPENDENCYCOUNT = :TRIGGER_DEPENDENCYCOUNT + 1;
			IF (DEPENDENCY_FIELDS <> '') THEN
			BEGIN
				:DEPENDENCY_FIELDS = :DEPENDENCY_FIELDS || ', ';
			END
			:DEPENDENCY_FIELDS = :DEPENDENCY_FIELDS || TRIM(DEPENDENCY_FIELD);
		END
		IF (:TRIGGER_DEPENDENCYCOUNT <> 1) THEN
		BEGIN
			:IS_AUTOINCREMENT = 0;
		END");
		if (_autoIncrement != -1)
		{
			sql.AppendFormat(@"
		IF(:IS_AUTOINCREMENT = {0}) THEN
		BEGIN
			SUSPEND;
		END", _autoIncrement);
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

	protected override void ProcessResult(DataTable schema)
	{
		/*
		schema.BeginLoadData();

		int len;

		foreach (DataRow row in schema.Rows)
		{
			if (row["IS_SYSTEM_TRIGGER"] == DBNull.Value ||
				Convert.ToInt32(row["IS_SYSTEM_TRIGGER"], CultureInfo.InvariantCulture) == 0)
			{
				row["IS_SYSTEM_TRIGGER"] = 0;
			}

			if (row["DEPENDENCY_FIELDS"] != DBNull.Value)
			{
				len = (row["DEPENDENCY_FIELDS"].ToString()).Split(',').Length;
				row["TRIGGER_DEPENDENCYCOUNT"] = len;
			}
			else
			{
				len = 0;
			}

			if (len != 1)
				row["IS_AUTOINCREMENT"] = false;
		}

		schema.EndLoadData();
		schema.AcceptChanges();

		 */

	}

	#endregion
}
