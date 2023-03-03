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

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Globalization;
using System.Text;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Common;
using System.Collections.Generic;
using System.CodeDom;

namespace BlackbirdSql.VisualStudio.Ddex.Schema;


/// <summary>
/// The base class for all column based types.
/// Refer to <see cref="DslForeignKeyColumns"/> to see an example of a more complex
/// derived type.
/// </summary>
internal class DslColumns : DslSchema
{
	/// <summary>
	/// The parent DslSchema this column collection belongs to.
	/// (Not it's <see cref="DslObjectTypes"/> type.)
	/// </summary>
	protected string _ParentType = "Table";

	/// <summary>
	/// The <see cref="DslObjectTypes"/> type of this column.
	/// </summary>
	protected string _ObjectType = "TableColumn";

	/// <summary>
	/// The column to be used in the where clause parent restriction.
	/// </summary>
	/// <remarks>
	/// If the parent column does not match r.rdb$relation_name then...
	///		1. If the number of restrictions for this column type is the same as for
	///			TableColumn it will replace the r.rdb$relation_name restriction.
	///		2. If the number of restrictions for this column type is the equal to
	///			the TableColumn restrictions + 1 it will be added as an additional parent
	///			restriction.
	/// </remarks>
	protected string _ParentRestriction = "r.rdb$relation_name";

	/// <summary>
	/// The column to be used for ORDINAL_POSITION
	/// </summary>
	protected string _OrdinalPosition = "r.rdb$field_position";

	/// <summary>
	/// The column alias to be used for the owner or parent order
	/// </summary>
	protected string _OrderingField = "r.rdb$relation_name";

	/// <summary>
	/// The FROM expression clause that returns the rdb$relation_fields
	/// row set using the alias 'r'
	/// </summary>
	protected string _FromClause = "rdb$relation_fields r";

	/// <summary>
	/// The list of required columns where the source may change.
	/// You may change the source in your derived class but the keys must remain static.
	/// null values are permitted.
	/// </summary>
	/// <remarks>
	/// Example
	///  _RequiredColumns["TRIGGER_NAME"] = "dep.rdb$dependent_name";
	/// </remarks>
	protected readonly IDictionary<string, object> _RequiredColumns = new Dictionary<string, object>()
	{
		{ "TRIGGER_NAME", "r_dep.rdb$dependent_name" }
	};

	/// <summary>
	/// List of additional columns to be added to the SELECT columns in the form
	/// 'alias, columnType' where columnType = {column, type}
	/// null values are permitted for 'column'.
	/// </summary>
	protected readonly Dictionary<string, ColumnType> _AdditionalColumns = new Dictionary<string, ColumnType>();

	/// <summary>
	/// Any additional conditions to be inserted into the WHERE clause.
	/// </summary>
	protected string _ConditionClause = "";




	public DslColumns() : base()
	{
	}

	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{

		// Diag.Trace();
		var sql = new StringBuilder();
		var where = new StringBuilder();

		string identityType = "null";
		string generatorSelector = " IS NULL";
		string unknownSequence = Properties.Resources.UnknownSequence;

		if (MajorVersionNumber >= 3)
		{
			identityType = "r.rdb$identity_type";
			generatorSelector = "= r.rdb$generator_name";
		}

		string returnClause = "";

		foreach (KeyValuePair<string, ColumnType> clause in _AdditionalColumns)
		{
			returnClause += @",
		" + clause.Key + " " + clause.Value.Type;
		}

		string columnsClause = "";
		string intoClause = "";

		foreach (KeyValuePair<string, object> clause in _RequiredColumns)
		{
			columnsClause += @",
			" + (clause.Value == null ? "null" : clause.Value.ToString());
			intoClause += @",
			:" + clause.Key;
		}

		foreach(KeyValuePair<string, ColumnType> clause in _AdditionalColumns)
		{
			columnsClause += @",
			" + (clause.Value.Column == null ? "null" : clause.Value.Column.ToString());
			intoClause += @",
			:" + clause.Key;
		}


		int defaultRestrictionsLen = DslObjectTypes.GetIdentifierLength("TableColumn");
		int derivedRestrictionsLen = DslObjectTypes.GetIdentifierLength(_ObjectType);

		if (restrictions != null)
		{
			// int index = 0;

			/* TABLE_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* TABLE_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* Owner / Parent */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				// Cannot pass params to execute block
				if (derivedRestrictionsLen > defaultRestrictionsLen)
					// where.AppendFormat("r.rdb$relation_name = @p{0}", index++);
					where.AppendFormat("r.rdb$relation_name = '{0}'", restrictions[2].ToString());
				else
					// where.AppendFormat("{0} = @p{1}", _ParentRestriction, index++);
					where.AppendFormat("{0} = '{1}'", _ParentRestriction, restrictions[2].ToString());
			}

			/* CONSTRAINT_NAME */
			if (derivedRestrictionsLen > defaultRestrictionsLen)
			{
				if (restrictions.Length >= 4 && restrictions[3] != null)
				{
					if (where.Length > 0)
						where.Append(" AND ");

					// Cannot pass params to execute block
					// where.AppendFormat("{0} = @p{1}", _ParentRestriction, index++);
					where.AppendFormat("{0} = '{1}'", _ParentRestriction, restrictions[3].ToString());
				}
			}


			/* COLUMN_NAME */
			if (restrictions.Length >= derivedRestrictionsLen && restrictions[derivedRestrictionsLen - 1] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				// Cannot pass params to execute block
				// where.AppendFormat("r.rdb$field_name = @p{0}", index++);
				where.AppendFormat("r.rdb$field_name = '{0}'", restrictions[derivedRestrictionsLen - 1].ToString());
			}
		}

		if (_ConditionClause != "")
		{
			if (where.Length > 0)
				where.Append(" AND ");

			where.Append(_ConditionClause);
		}

		if (where.Length > 0)
		{
			where = new(@"
		WHERE " + where.ToString());
		}

		// BlackbirdSql added IN_PRIMARYKEY
		// BlackbirdSql added TRIGGER_NAME
		// BlackbirdSql added SEQUENCE_NAME
		// BlackbirdSql added IS_IDENTITY
		/*
		 * What this does in addition to FbColumns()
		 * 
		 * Firstly gets the FROM clause result set for rdb$relation_fields using alias 'r'.
		 * Joins in this section of the from clause should all be INNER JOINs.
		 * 
		 * Then gets the following (all on a LEFT OUTER JOIN)...
		 * 
		 * For the Primary Key...
		 * Selects the constraint that is 'PRIMARY KEY' then
		 * Selects the segment that matches the field as IN_PRIMARYKEY then
		 * In ProcessResult() checks if it is the only IN_PRIMARYKEY and
		 *	sets IS_UNIQUE to false if it is not.
		 *	
		 *	For Identity columns...
		 *	Selects the Trigger that implies an Auto incremement on the 'PRIMARY KEY' 
		 *		constraint/segment for the current field then
		 *	Selects the dependent on that trigger matching the current field
		 * In ProcessResult() checks if it is the only IN_PRIMARYKEY and
		 *	sets IS_UNIQUE and IS_IDENTITY to false if it is not.
		 *	
		 *	All this establishes if the field is a singular primary key and only
		 *	then establishes if it's an identity column.
		 *	
		 *	We don't care for cases where an IN_PRIMARYKEY is in a multi-part
		 *	index and also a multi-part identity column, which is possible but violates
		 *	acceptable relation db design.
		 *	So you can have multiple primary key fields but not multiple 
		 *	identity fields
		 *	
		 * Legend:
		 * 
		 * {0} returnClause
		 * {1} _FromClause - default: 'rdb$relation_fields r' (alias must always be r for rdb$relation_fields)
		 * {2} WHERE clause prefixed with "cflfWHERE " and preceded by _ConditionClause.
		 * {3} Ordinal position field - default: 'r.rdb$field_position'
		 * {4} identityType
		 * {5} _ParentType
		 * {6} columnsClause
		 * {7} generatorSelector
		 * {8} _OrderingField
		 * {9} intoClause
		 * {10} unknownSequence
		*/

		sql.AppendFormat(
					@"EXECUTE BLOCK
	RETURNS (
        TABLE_CATALOG varchar(10), TABLE_SCHEMA varchar(10), TABLE_NAME varchar(50), COLUMN_NAME varchar(50),
		COLUMN_SUB_TYPE smallint,
        COLUMN_SIZE integer, NUMERIC_PRECISION integer, NUMERIC_SCALE integer, CHARACTER_MAX_LENGTH integer, CHARACTER_OCTET_LENGTH integer,
        ORDINAL_POSITION smallint, DOMAIN_CATALOG varchar(10), DOMAIN_SCHEMA varchar(1), DOMAIN_NAME varchar(50), COLUMN_DEFAULT blob sub_type 1,
        EXPRESSION blob sub_type 1, IS_COMPUTED boolean, IS_ARRAY boolean, IS_NULLABLE boolean, READONLY_FLAG smallint, FIELD_TYPE smallint,
        CHARACTER_SET_CATALOG varchar(10), CHARACTER_SET_SCHEMA varchar(10), CHARACTER_SET_NAME varchar(50), 
        COLLATION_CATALOG varchar(10), COLLATION_SCHEMA varchar(10), COLLATION_NAME varchar(50),
        DESCRIPTION varchar(50), IN_PRIMARYKEY boolean, IS_UNIQUE boolean, IS_IDENTITY boolean, SEQUENCE_GENERATOR varchar(50),
        -- [returnClause]~0~ directly after TRIGGER_NAME varchar(50)
		IDENTITY_SEED bigint, IDENTITY_INCREMENT int, IDENTITY_CURRENT bigint, PARENT_TYPE varchar(15), TRIGGER_NAME varchar(50){0})
AS
DECLARE PRIMARY_DEPENDENCYCOUNT int;
DECLARE TRIGGER_DEPENDENCYCOUNT int;
DECLARE IDENTITY_TYPE smallint;
DECLARE SEGMENT_FIELD varchar(50);
BEGIN
    :TRIGGER_DEPENDENCYCOUNT = 0;

	SELECT COUNT(*)
	-- [rdb$relation_fields r]~1~
	FROM {1}
	INNER JOIN rdb$relation_constraints r_con
		ON r_con.rdb$relation_name = r.rdb$relation_name AND r_con.rdb$constraint_type = 'PRIMARY KEY'
	INNER JOIN rdb$index_segments r_seg 
		ON r_seg.rdb$index_name = r_con.rdb$index_name AND r_seg.rdb$field_name = r.rdb$field_name
	INNER JOIN rdb$triggers r_trg
		ON r_seg.rdb$field_name IS NOT NULL AND r_trg.rdb$relation_name = r_con.rdb$relation_name
			AND r_trg.rdb$trigger_sequence = 1 AND r_trg.rdb$flags = 1 AND r_trg.rdb$trigger_type = 1
	INNER JOIN rdb$dependencies r_dep
		ON r_dep.rdb$dependent_name = r_trg.rdb$trigger_name
			AND r_dep.rdb$depended_on_name = r_trg.rdb$relation_name AND r_dep.rdb$field_name = r.rdb$field_name{2}
	-- [crlfWHERE r.rdb$relation_name = '...']~2~ directly after ' = r.rdb$field_name'

	INTO :PRIMARY_DEPENDENCYCOUNT;

	IF (:PRIMARY_DEPENDENCYCOUNT IS NULL) THEN
		:PRIMARY_DEPENDENCYCOUNT = 0;

	FOR
		SELECT
			-- :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :COLUMN_NAME, :COLUMN_SUB_TYPE
			null, null, r.rdb$relation_name, r.rdb$field_name, r_fld.rdb$field_sub_type,
			-- :COLUMN_SIZE
			CAST(r_fld.rdb$field_length AS integer),
			-- :NUMERIC_PRECISION
			CAST(r_fld.rdb$field_precision AS integer),
			-- :NUMERIC_SCALE
			CAST(r_fld.rdb$field_scale AS integer),
			-- CHARACTER_MAX_LENGTH
			CAST(r_fld.rdb$character_length AS integer),
			-- :CHARACTER_OCTET_LENGTH
			CAST(r_fld.rdb$field_length AS integer),
			-- ORDINAL_POSITION - [r.rdb$field_position]~3~
			{3},
			-- :DOMAIN_CATALOG, :DOMAIN_SCHEMA, :DOMAIN_NAME
			null, null, r.rdb$field_source,
			-- :COLUMN_DEFAULT
			r.rdb$default_source,
			-- :EXPRESSION
			(CASE WHEN r_fld.rdb$computed_source IS NULL AND r_fld.rdb$computed_blr IS NOT NULL THEN
					cast(r_fld.rdb$computed_blr as blob sub_type 1)
			ELSE
					r_fld.rdb$computed_source
			END),
			-- :IS_COMPUTED
			(CASE WHEN r_fld.rdb$computed_source IS NULL AND r_fld.rdb$computed_blr IS NULL THEN
					false
			ELSE
					true
			END),
			-- :IS_ARRAY
			(CASE WHEN r_fld.rdb$dimensions IS NULL THEN false ELSE true END),
			-- :IS_NULLABLE
			(CASE WHEN coalesce(r_fld.rdb$null_flag, r.rdb$null_flag) IS NULL THEN true ELSE false END),
			-- :READONLY_FLAG
			0,
			-- :FIELD_TYPE
			r_fld.rdb$field_type,
			-- :CHARACTER_SET_CATALOG, :CHARACTER_SET_SCHEMA, :CHARACTER_SET_NAME
			null, null, r_cs.rdb$character_set_name,
			-- :COLLATION_CATALOG, :COLLATION_SCHEMA, :COLLATION_NAME
			null, null, r_coll.rdb$collation_name,
			-- :DESCRIPTION
			r.rdb$description,
			-- :IN_PRIMARYKEY
			(CASE WHEN r_seg.rdb$field_name IS NULL THEN false ELSE true END),
			-- :IS_IDENTITY
			(CASE WHEN r_dep.rdb$dependent_name IS NOT NULL AND r_trg.rdb$trigger_name IS NOT NULL AND r_trg.rdb$trigger_sequence = 1 AND r_trg.rdb$flags = 1 and r_trg.rdb$trigger_type = 1 THEN
				true
			ELSE
				false
			END),
			-- :SEGMENT_FIELD
			r_seg.rdb$field_name,
			-- :IDENTITY_TYPE - [r.rdb$identity_type|null]~4~
			{4},
			-- :SEQUENCE_GENERATOR, :IDENTITY_SEED, :IDENTITY_INCREMENT
			r_gen.rdb$generator_name, r_gen.rdb$initial_value, r_gen.rdb$generator_increment,
			-- :PARENT_TYPE - [Table|'ParentType']~5~
			-- [, r_dep.rdb$dependent_name]~6~ (for additional columns)
			'{5}'{6}
		-- [rdb$relation_fields r]~1~
		FROM {1}
		INNER JOIN rdb$fields r_fld
			ON r_fld.rdb$field_name = r.rdb$field_source
		LEFT OUTER JOIN rdb$character_sets r_cs
			ON r_cs.rdb$character_set_id = r_fld.rdb$character_set_id
		LEFT OUTER JOIN rdb$collations r_coll
			ON (r_coll.rdb$collation_id = r_fld.rdb$collation_id AND r_coll.rdb$character_set_id = r_fld.rdb$character_set_id)
        LEFT OUTER JOIN rdb$relation_constraints r_con
			ON r_con.rdb$relation_name = r.rdb$relation_name AND r_con.rdb$constraint_type = 'PRIMARY KEY'
        LEFT OUTER JOIN rdb$index_segments r_seg 
            ON r_seg.rdb$index_name = r_con.rdb$index_name AND r_seg.rdb$field_name = r.rdb$field_name
        LEFT OUTER JOIN rdb$triggers r_trg
            ON r_trg.rdb$relation_name = r_con.rdb$relation_name AND r_trg.rdb$trigger_sequence = 1 AND r_trg.rdb$flags = 1 AND r_trg.rdb$trigger_type = 1
        LEFT OUTER JOIN rdb$dependencies r_dep
            ON r_dep.rdb$depended_on_name = r_trg.rdb$relation_name AND r_dep.rdb$field_name = r_seg.rdb$field_name
                AND r_dep.rdb$dependent_name = r_trg.rdb$trigger_name
        LEFT OUTER JOIN rdb$generators r_gen
			-- [= r.rdb$generator_name|IS NULL]~7~
		-- [crlfWHERE r.rdb$relation_name = '...']~2~
            ON r_gen.rdb$generator_name{7}{2}
		-- [r.rdb$relation_name]~8~ [r.rdb$field_position]~3~
		ORDER BY {8}, {3}

        INTO :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :COLUMN_NAME, :COLUMN_SUB_TYPE,
        :COLUMN_SIZE, :NUMERIC_PRECISION, :NUMERIC_SCALE, :CHARACTER_MAX_LENGTH, :CHARACTER_OCTET_LENGTH,
        :ORDINAL_POSITION, :DOMAIN_CATALOG, :DOMAIN_SCHEMA, :DOMAIN_NAME, :COLUMN_DEFAULT,
        :EXPRESSION, :IS_COMPUTED, :IS_ARRAY, :IS_NULLABLE, :READONLY_FLAG, :FIELD_TYPE,
        :CHARACTER_SET_CATALOG, :CHARACTER_SET_SCHEMA, :CHARACTER_SET_NAME, :COLLATION_CATALOG, :COLLATION_SCHEMA,
        :COLLATION_NAME, :DESCRIPTION,
		:IN_PRIMARYKEY, :IS_IDENTITY, :SEGMENT_FIELD,
		-- [,crlfTRIGGER_NAME]~9~ directly after :PARENT_TYPE
		:IDENTITY_TYPE, :SEQUENCE_GENERATOR, :IDENTITY_SEED, :IDENTITY_INCREMENT, :PARENT_TYPE{9}
	DO BEGIN
		IF (:SEGMENT_FIELD IS NULL OR :PRIMARY_DEPENDENCYCOUNT <> 1) THEN
			:IS_IDENTITY = false;

		IF (:SEQUENCE_GENERATOR IS NOT NULL) THEN
		BEGIN
			EXECUTE STATEMENT 'SELECT gen_id(' || SEQUENCE_GENERATOR || ', 0) FROM rdb$database' INTO :IDENTITY_CURRENT;
			:IDENTITY_CURRENT = :IDENTITY_CURRENT - :IDENTITY_INCREMENT;
			IF (:IDENTITY_CURRENT < :IDENTITY_SEED) THEN
			BEGIN
				:IDENTITY_CURRENT = :IDENTITY_SEED;
			END

			IF (:IS_IDENTITY) THEN
			BEGIN
				-- There is a generator so :IDENTITY_TYPE determines if is-identity still holds true
				IF (:IDENTITY_TYPE IS NULL OR :IDENTITY_TYPE = 0) THEN
					:IS_IDENTITY = false;
			END
		END
        if (:IS_IDENTITY AND :SEQUENCE_GENERATOR IS NULL) THEN
		BEGIN
            -- [Unidentifiable]~10~
            :SEQUENCE_GENERATOR = '{10}';
			:IDENTITY_SEED = -1;
			:IDENTITY_INCREMENT = -1;
			:IDENTITY_CURRENT = -1;
        END
		IF (NOT :IS_IDENTITY  AND :SEQUENCE_GENERATOR IS NULL) THEN
		BEGIN
			:SEQUENCE_GENERATOR = NULL;
			:IDENTITY_SEED = 0;
			:IDENTITY_INCREMENT = 0;
			:IDENTITY_CURRENT = 0;
		END

		IF (:IN_PRIMARYKEY AND :PRIMARY_DEPENDENCYCOUNT = 1) THEN
			:IS_UNIQUE = true;
		ELSE
			:IS_UNIQUE = false;
		

-- End section 2 

-- Begin section 3 - Column row complete

        SUSPEND;            

-- End section 3

-- Finalize

	END
END",
					returnClause, _FromClause, where.ToString(), _OrdinalPosition, identityType, _ParentType,
					columnsClause, generatorSelector, _OrderingField, intoClause, unknownSequence);


		// Diag.Trace(sql.ToString());

		return sql;
	}



	protected override void ProcessResult(DataTable schema)
	{
		schema.BeginLoadData();

		schema.Columns.Add("COLUMN_DATA_TYPE", typeof(string));


		foreach (DataRow row in schema.Rows)
		{
			var blrType = Convert.ToInt32(row["FIELD_TYPE"], CultureInfo.InvariantCulture);

			var subType = 0;
			if (row["COLUMN_SUB_TYPE"] != DBNull.Value)
			{
				subType = Convert.ToInt32(row["COLUMN_SUB_TYPE"], CultureInfo.InvariantCulture);
			}

			var scale = 0;
			if (row["NUMERIC_SCALE"] != DBNull.Value)
			{
				scale = Convert.ToInt32(row["NUMERIC_SCALE"], CultureInfo.InvariantCulture);
			}

			var dbType = (FbDbType)DslTypeHelper.GetDbDataTypeFromBlrType(blrType, subType, scale);
			row["COLUMN_DATA_TYPE"] = DslTypeHelper.GetDataTypeName((DslDbDataType)dbType).ToLowerInvariant();

			if (dbType == FbDbType.Binary || dbType == FbDbType.Text)
			{
				row["COLUMN_SIZE"] = Int32.MaxValue;
			}

			if (dbType == FbDbType.Char || dbType == FbDbType.VarChar)
			{
				if (!row.IsNull("CHARACTER_MAX_LENGTH"))
				{
					row["COLUMN_SIZE"] = row["CHARACTER_MAX_LENGTH"];
				}
			}
			else
			{
				row["CHARACTER_OCTET_LENGTH"] = 0;
			}

			if (row["NUMERIC_PRECISION"] == DBNull.Value)
			{
				row["NUMERIC_PRECISION"] = 0;
			}

			if ((dbType == FbDbType.Decimal || dbType == FbDbType.Numeric) &&
				(row["NUMERIC_PRECISION"] == DBNull.Value || Convert.ToInt32(row["NUMERIC_PRECISION"]) == 0))
			{
				row["NUMERIC_PRECISION"] = row["COLUMN_SIZE"];
			}

			row["NUMERIC_SCALE"] = (-1) * scale;

			var domainName = row["DOMAIN_NAME"].ToString();
			if (domainName != null && domainName.StartsWith("RDB$"))
			{
				row["DOMAIN_NAME"] = null;
			}

		}


		schema.EndLoadData();
		schema.AcceptChanges();

		// Remove not more needed columns
		schema.Columns.Remove("FIELD_TYPE");
		schema.Columns.Remove("CHARACTER_MAX_LENGTH");

		// Diag.Trace("Rows returned: " + schema.Rows.Count);

	}

	#endregion





	// =========================================================================================================
	#region Child classes - DslColumns
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// A container class for additional columns.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class ColumnType
	{
		public readonly string Column;
		public readonly string Type;

		public ColumnType(string column, string type)
		{
			Column = column;
			Type = type;
		}
	}


	#endregion Child classes
}
