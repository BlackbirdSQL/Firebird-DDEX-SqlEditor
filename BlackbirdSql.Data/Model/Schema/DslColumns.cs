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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Data.Model.Schema;


/// <summary>
/// The base class for all column based types.
/// Refer to <see cref="DslForeignKeyColumns"/> to see an example of a more complex
/// derived type.
/// </summary>
internal class DslColumns : AbstractDslSchema
{
	public DslColumns() : base()
	{
	}



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
	/// If the parent column does not match r.RDB$RELATION_NAME then...
	///		1. If the number of restrictions for this column type is the same as for
	///			TableColumn it will replace the r.RDB$RELATION_NAME restriction.
	///		2. If the number of restrictions for this column type is the equal to
	///			the TableColumn restrictions + 1 it will be added as an additional parent
	///			restriction.
	/// </remarks>
	protected string _ParentColName = "r.RDB$RELATION_NAME";
	protected string _ChildColName = "r.RDB$FIELD_NAME";
	protected string _FieldNameColName = "r.RDB$FIELD_NAME";
	protected string _SystemFlagColName = "r.RDB$SYSTEM_FLAG";

	/// <summary>
	/// The identity type column for >= v3 Firebird.
	/// If the RDB$RELATIONS alias is not 'r' then replace r with the correct alias
	/// else null if it cannot be derived.
	/// </summary>
	/// <remarks>
	/// If <see cref="_GeneratorColName"/> is set to null, then _IdentityTypeColName will
	/// be assumed to be null;
	/// </remarks>
	protected string _IdentityTypeColName = "r.RDB$IDENTITY_TYPE";

	/// <summary>
	/// The identity generator selector for >= v3 Firebird.
	/// If the RDB$RELATIONS alias is not 'r' then replace r with the correct alias
	/// </summary>
	/// <remarks>
	/// If the sequence generator name cannot be derived, as for example on procedures
	/// and functions, set the selector variable null.
	/// </remarks>
	protected string _GeneratorColName = "r.RDB$GENERATOR_NAME";


	/// <summary>
	/// The column alias to be used for the owner or parent order
	/// </summary>
	protected string _OrderingColumn = "r.RDB$RELATION_NAME";

	/// <summary>
	/// The FROM expression clause that returns the row set using the alias 'r'.
	/// r will be used to interrogate the RBD$FIELDS table.
	/// </summary>
	protected string _FromClause = "RDB$RELATION_FIELDS r";

	/// <summary>
	/// The list of required columns where the source may change.
	/// You may change the source in your derived class but the keys must remain static.
	/// null values are permitted.
	/// </summary>
	/// <remarks>
	/// Examples
	///  _RequiredColumns["COLUMN_NAME"] = "null";
	///  _RequiredColumns["TRIGGER_NAME"] = "dep.RDB$DEPENDENT_NAME";
	/// </remarks>
	protected readonly IDictionary<string, object> _RequiredColumns = new Dictionary<string, object>()
	{
		{ "ORDINAL_POSITION", "r.RDB$FIELD_POSITION" },
		// Direction 0: IN, 1: OUT, 3: IN/OUT, 6: RETVAL
		{ "DIRECTION_FLAG", "1" },
		{ "TRIGGER_NAME", "r_dep.RDB$DEPENDENT_NAME" }
	};

	/// <summary>
	/// List of additional columns to be added to the SELECT columns in the form
	/// 'alias, columnType' where columnType = {column, type}
	/// null values are permitted for 'column'.
	/// </summary>
	protected readonly Dictionary<string, ColumnType> _AdditionalColumns = [];

	/// <summary>
	/// Any additional conditions to be inserted into the WHERE clause.
	/// </summary>
	protected string _ConditionClause = "";

	#region Protected Methods


	protected override void InitializeParameters(IDbConnection connection)
	{
		InitializeColumnsList(connection, "RDB$RELATION_FIELDS");
	}



	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// return base.GetCommandText(restrictions, columns);



		// Evs.Trace(GetType(), "GetCommandText");

		StringBuilder sql = new();
		StringBuilder where = new();

		// string bigInt = _Dialect != 1 ? "bigint" : "int";
		// string booleanType = MajorVersionNumber >= 3 ? "boolean" : "int";
		// string booleanTrue = MajorVersionNumber >= 3 ? "true" : "1";
		// string booleanFalse = MajorVersionNumber >= 3 ? "false" : "0";

		string returnClause = "";

		foreach (KeyValuePair<string, ColumnType> clause in _AdditionalColumns)
		{
			returnClause += $@",
		{clause.Key} {clause.Value.Type}";
		}


		if (MajorVersionNumber < 3 || _GeneratorColName == null)
		{
			_GeneratorColName = "null";
			_IdentityTypeColName = "null";
		}

		_IdentityTypeColName ??= "null";

		string clauseValue;
		string columnsClause = "";
		string intoClause = "";

		foreach (KeyValuePair<string, object> clause in _RequiredColumns)
		{
			clauseValue = clause.Value == null ? "null" : clause.Value.ToString();

			columnsClause += $@",
			{clauseValue}";

			intoClause += $@",
		:{clause.Key}";
		}

		foreach (KeyValuePair<string, ColumnType> clause in _AdditionalColumns)
		{
			clauseValue = (clause.Value.Column == null ? "null" : clause.Value.Column.ToString());

			columnsClause += $@",
			{clauseValue}";

			intoClause += @",
		:" + clause.Key;
		}


		int defaultRestrictionsLen = DslObjectTypes.GetIdentifierLength("TableColumn");
		int derivedRestrictionsLen = DslObjectTypes.GetIdentifierLength(_ObjectType);

		if (restrictions != null)
		{
			// int index = 0;

			/* CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* Owner / Parent */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				// Cannot pass params to execute block
				if (derivedRestrictionsLen > defaultRestrictionsLen)
					// where.AppendFormat("r.RDB$RELATION_NAME = @p{0}", index++);
					where.Append($"r.RDB$RELATION_NAME = '{restrictions[2]}'");
				else
					// where.AppendFormat("{0} = @p{1}", _ParentRestriction, index++);
					where.Append($"{_ParentColName} = '{restrictions[2]}'");
			}

			/* PARENT */
			if (derivedRestrictionsLen > defaultRestrictionsLen)
			{
				if (restrictions.Length >= 4 && restrictions[3] != null)
				{
					if (where.Length > 0)
						where.Append(" AND ");

					// Cannot pass params to execute block
					// where.AppendFormat("{0} = @p{1}", _ParentRestriction, index++);
					where.Append($"{_ParentColName} = '{restrictions[3]}'");
				}
			}


			/* CHILD OBJECT NAME */
			if (restrictions.Length >= derivedRestrictionsLen && restrictions[derivedRestrictionsLen - 1] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				// Cannot pass params to execute block
				// where.AppendFormat("r.RDB$FIELD_NAME = @p{0}", index++);
				where.Append($"{_ChildColName} = '{restrictions[derivedRestrictionsLen - 1]}'");
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


		string dependencyCountClause = HasColumn("RDB$RELATION_NAME")

			? $@"SELECT COUNT(*)
	-- [RDB$RELATION_FIELDS r]~1~
	FROM {_FromClause}
		INNER JOIN RDB$RELATION_CONSTRAINTS r_con
		ON r_con.RDB$RELATION_NAME = r.RDB$RELATION_NAME AND r_con.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY'
	INNER JOIN RDB$INDEX_SEGMENTS r_seg
		ON r_seg.RDB$INDEX_NAME = r_con.RDB$INDEX_NAME AND r_seg.RDB$FIELD_NAME = r.RDB$FIELD_NAME

	-- Seems we don't need a trigger to determine the dependency count
	-- INNER JOIN RDB$TRIGGERS r_trg
		-- ON r_seg.RDB$FIELD_NAME IS NOT NULL AND r_trg.RDB$RELATION_NAME = r_con.RDB$RELATION_NAME
			-- AND r_trg.RDB$TRIGGER_SEQUENCE = 1 AND r_trg.RDB$TRIGGER_TYPE = 1
	-- INNER JOIN RDB$DEPENDENCIES r_dep
		-- ON r_dep.RDB$DEPENDENT_NAME = r_trg.RDB$TRIGGER_NAME
			-- AND r_dep.RDB$DEPENDED_ON_NAME = r_trg.RDB$RELATION_NAME AND r_dep.RDB$FIELD_NAME = r.RDB$FIELD_NAME{where}

		-- [crlfWHERE r.RDB$RELATION_NAME = '...']~2~directly after ' = r.RDB$FIELD_NAME'
	INTO: PRIMARY_DEPENDENCYCOUNT;"

			: "PRIMARY_DEPENDENCYCOUNT = 0;";

		string relationNameColumn = HasColumn("RDB$RELATION_NAME")
			? "r.RDB$RELATION_NAME" : "null";
		string fieldSourceColumn = HasColumn("RDB$FIELD_SOURCE")
			? "r.RDB$FIELD_SOURCE" : "null";

		// Evs.Trace(GetType(), nameof(GetCommandText), $"fieldSourceColumn: {fieldSourceColumn}");


		string isNullableColumn = HasColumn("RDB$NULL_FLAG")
			? "(CASE WHEN coalesce(r_fld.RDB$NULL_FLAG, r.RDB$NULL_FLAG) IS NULL THEN 1 ELSE 0 END)"
			: "(CASE WHEN r_fld.RDB$NULL_FLAG IS NULL THEN 1 ELSE 0 END)";
		string collationIdColumn = HasColumn("RDB$FIELD_COLLATION_ID")
			? "r.RDB$FIELD_COLLATION_ID" : "null";
		string descriptionColumn = HasColumn("RDB$DESCRIPTION")
			? "r.RDB$DESCRIPTION" : "null";
		string characterSetColumn = HasColumn("RDB$CHARACTER_SET_ID")
			? "r.RDB$CHARACTER_SET_ID" : "null";
		string fieldSubTypeColumn = HasColumn("RDB$FIELD_SUB_TYPE")
			? "r.RDB$FIELD_SUB_TYPE" : "null";
		string fieldLengthColumn = HasColumn("RDB$FIELD_LENGTH")
			? "CAST(r.RDB$FIELD_LENGTH AS integer)" : "0";
		string fieldPrecisionColumn = HasColumn("RDB$FIELD_PRECISION")
			? "CAST(r.RDB$FIELD_PRECISION AS integer)" : "0";
		string fieldScaleColumn = HasColumn("RDB$FIELD_SCALE")
			? "CAST(r.RDB$FIELD_SCALE AS integer)" : "0";
		string characterLengthColumn = HasColumn("RDB$CHARACTER_LENGTH")
			? "CAST(r.RDB$CHARACTER_LENGTH AS integer)" : "0";
		string fieldTypeColumn = HasColumn("RDB$FIELD_TYPE")
			? "r.RDB$FIELD_TYPE" : "0";

		// BlackbirdSql added IN_PRIMARYKEY
		// BlackbirdSql added TRIGGER_NAME
		// BlackbirdSql added SEQUENCE_NAME
		// BlackbirdSql added IS_IDENTITY
		/*
		 * What this does in addition to FbColumns()
		 * 
		 * Firstly gets the FROM clause result set for RDB$RELATION_FIELDS using alias 'r'.
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
		 * {1} _FromClause - default: 'RDB$RELATION_FIELDS r' (ensure r aliases for r dependents are  r_..)
		 * {2} WHERE clause prefixed with "cflfWHERE " and preceded by _ConditionClause.
		 * {3} generatorColumn
		 * {4} _ParentType
		 * {5} columnsClause - _RequiredColumns and _AdditionalColumns
		 * {6} _OrderingColumn
		 * {7} _RequiredColumns["ORDINAL_POSITION"]: - default: 'r.RDB$FIELD_POSITION'
		 * {8} intoClause
		*/

		sql.Append($@"EXECUTE BLOCK
	RETURNS (
        TABLE_CATALOG varchar(10), TABLE_SCHEMA varchar(10), TABLE_NAME varchar(50), COLUMN_NAME varchar(50),
		IS_SYSTEM_FLAG int, FIELD_SUB_TYPE smallint,
        FIELD_SIZE int, NUMERIC_PRECISION int, NUMERIC_SCALE int, CHARACTER_MAX_LENGTH int, CHARACTER_OCTET_LENGTH int,
        DOMAIN_CATALOG varchar(10), DOMAIN_SCHEMA varchar(1), DOMAIN_NAME varchar(50), FIELD_DEFAULT blob sub_type 1,
        EXPRESSION blob sub_type 1, IS_COMPUTED_FLAG int, IS_ARRAY_FLAG int, IS_NULLABLE_FLAG int, READONLY_FLAG smallint, FIELD_TYPE smallint,
        CHARACTER_SET_CATALOG varchar(10), CHARACTER_SET_SCHEMA varchar(10), CHARACTER_SET_NAME varchar(50), 
        COLLATION_CATALOG varchar(10), COLLATION_SCHEMA varchar(10), COLLATION_NAME varchar(50), DESCRIPTION varchar(50), IN_PRIMARYKEY_FLAG int,
			IS_UNIQUE_FLAG int, IS_IDENTITY_FLAG int, SEQUENCE_GENERATOR varchar(50),
        -- [returnClause]~0~ directly after TRIGGER_NAME varchar(50)
		PARENT_TYPE varchar(15), ORDINAL_POSITION smallint, DIRECTION_FLAG int, TRIGGER_NAME varchar(50){returnClause})
AS
DECLARE PRIMARY_DEPENDENCYCOUNT int;
DECLARE SEGMENT_FIELD varchar(50);
BEGIN
	{dependencyCountClause}

	IF (:PRIMARY_DEPENDENCYCOUNT IS NULL) THEN
		PRIMARY_DEPENDENCYCOUNT = 0;

	FOR
		SELECT
			-- :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :COLUMN_NAME
			null, null, {relationNameColumn}, {_FieldNameColName},
			-- :IS_SYSTEM_FLAG,
			(CASE WHEN {_SystemFlagColName} <> 1 THEN 0 ELSE 1 END),
			-- :FIELD_SUB_TYPE
			(CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {fieldSubTypeColumn} ELSE r_fld.RDB$FIELD_SUB_TYPE END),
			-- :FIELD_SIZE
			(CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {fieldLengthColumn} ELSE CAST(r_fld.RDB$FIELD_LENGTH AS integer) END),
			-- :NUMERIC_PRECISION
			(CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {fieldPrecisionColumn} ELSE CAST(r_fld.RDB$FIELD_PRECISION AS integer) END),
			-- :NUMERIC_SCALE
			(CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {fieldScaleColumn} ELSE CAST(r_fld.RDB$FIELD_SCALE AS integer) END),
			-- CHARACTER_MAX_LENGTH
			(CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {characterLengthColumn} ELSE CAST(r_fld.RDB$CHARACTER_LENGTH AS integer) END),
			-- :CHARACTER_OCTET_LENGTH
			(CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {fieldLengthColumn} ELSE CAST(r_fld.RDB$FIELD_LENGTH AS integer) END),
			-- :DOMAIN_CATALOG, :DOMAIN_SCHEMA, :DOMAIN_NAME
			null, null, r_fld.RDB$FIELD_NAME,
			-- :FIELD_DEFAULT
			(CASE WHEN r_fld.RDB$FIELD_NAME is null THEN null ELSE r_fld.RDB$DEFAULT_SOURCE END),
			-- :EXPRESSION
			(CASE WHEN r_fld.RDB$COMPUTED_SOURCE IS NULL AND r_fld.RDB$COMPUTED_BLR IS NOT NULL THEN
					cast(r_fld.RDB$COMPUTED_BLR as blob sub_type 1)
			ELSE
					r_fld.RDB$COMPUTED_SOURCE
			END),
			-- :IS_COMPUTED_FLAG
			(CASE WHEN r_fld.RDB$COMPUTED_SOURCE IS NULL AND r_fld.RDB$COMPUTED_BLR IS NULL THEN
					0
			ELSE
					1
			END),
			-- :IS_ARRAY_FLAG
			(CASE WHEN r_fld.RDB$DIMENSIONS IS NULL THEN 0 ELSE 1 END),
			-- :IS_NULLABLE
			{isNullableColumn},
			-- :READONLY_FLAG
			0,
			-- :FIELD_TYPE
			(CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {fieldTypeColumn} ELSE r_fld.RDB$FIELD_TYPE END),
			-- :CHARACTER_SET_CATALOG, :CHARACTER_SET_SCHEMA, :CHARACTER_SET_NAME
			null, null, r_cs.RDB$CHARACTER_SET_NAME,
			-- :COLLATION_CATALOG, :COLLATION_SCHEMA, :COLLATION_NAME
			null, null, r_coll.RDB$COLLATION_NAME,
			-- :DESCRIPTION
			{descriptionColumn},
			-- :IN_PRIMARYKEY_FLAG
			(CASE WHEN r_seg.RDB$FIELD_NAME IS NULL THEN 0 ELSE 1 END),
			-- :IS_IDENTITY_FLAG
			(CASE WHEN r_dep.RDB$DEPENDENT_NAME IS NOT NULL AND r_trg.RDB$TRIGGER_NAME IS NOT NULL AND r_trg.RDB$TRIGGER_SEQUENCE = 1 and r_trg.RDB$TRIGGER_TYPE = 1 THEN
				1
			ELSE
				0
			END),
			-- :SEGMENT_FIELD
			r_seg.RDB$FIELD_NAME,
			-- :SEQUENCE_GENERATOR,
			{_GeneratorColName},
			-- :PARENT_TYPE - [Table|'ParentType']~4~
			-- [, r_dep.RDB$DEPENDENT_NAME]~5~ (for additional columns)
			'{_ParentType}'{columnsClause}
		-- [RDB$RELATION_FIELDS r]~1~
		FROM {_FromClause}
		LEFT OUTER JOIN RDB$FIELDS r_fld
			ON r_fld.RDB$FIELD_NAME = {fieldSourceColumn}
		LEFT OUTER JOIN RDB$CHARACTER_SETS r_cs
			ON r_cs.RDB$CHARACTER_SET_ID = (CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {characterSetColumn} ELSE r_fld.RDB$CHARACTER_SET_ID END)
		LEFT OUTER JOIN RDB$COLLATIONS r_coll
			ON r_coll.RDB$COLLATION_ID = (CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {collationIdColumn} ELSE r_fld.RDB$COLLATION_ID END)
				AND r_coll.RDB$CHARACTER_SET_ID = (CASE WHEN r_fld.RDB$FIELD_NAME is null THEN {characterSetColumn} ELSE r_fld.RDB$CHARACTER_SET_ID END)
        LEFT OUTER JOIN RDB$RELATION_CONSTRAINTS r_con
			ON r_con.RDB$RELATION_NAME = {relationNameColumn} AND r_con.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY'
        LEFT OUTER JOIN RDB$INDEX_SEGMENTS r_seg 
            ON r_seg.RDB$INDEX_NAME = r_con.RDB$INDEX_NAME AND r_seg.RDB$FIELD_NAME = {_FieldNameColName}
        LEFT OUTER JOIN RDB$TRIGGERS r_trg
            ON r_trg.RDB$RELATION_NAME = r_con.RDB$RELATION_NAME AND r_trg.RDB$TRIGGER_SEQUENCE = 1 AND r_trg.RDB$TRIGGER_TYPE = 1
        LEFT OUTER JOIN RDB$DEPENDENCIES r_dep
            ON r_dep.RDB$DEPENDED_ON_NAME = r_trg.RDB$RELATION_NAME AND r_dep.RDB$FIELD_NAME = r_seg.RDB$FIELD_NAME
		-- [crlfWHERE r.RDB$RELATION_NAME = '...']~2~
                AND r_dep.RDB$DEPENDENT_NAME = r_trg.RDB$TRIGGER_NAME{where}
		-- [r.RDB$RELATION_NAME]~6~ [r.RDB$FIELD_POSITION]~8~
		ORDER BY {_OrderingColumn}, {_RequiredColumns["ORDINAL_POSITION"]}

        INTO :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :COLUMN_NAME, :IS_SYSTEM_FLAG,
		:FIELD_SUB_TYPE, :FIELD_SIZE, :NUMERIC_PRECISION, :NUMERIC_SCALE,
		:CHARACTER_MAX_LENGTH, :CHARACTER_OCTET_LENGTH,
        :DOMAIN_CATALOG, :DOMAIN_SCHEMA, :DOMAIN_NAME, :FIELD_DEFAULT,
        :EXPRESSION, :IS_COMPUTED_FLAG, :IS_ARRAY_FLAG, :IS_NULLABLE_FLAG, :READONLY_FLAG, :FIELD_TYPE,
        :CHARACTER_SET_CATALOG, :CHARACTER_SET_SCHEMA, :CHARACTER_SET_NAME, :COLLATION_CATALOG, :COLLATION_SCHEMA,
        :COLLATION_NAME, :DESCRIPTION,
		:IN_PRIMARYKEY_FLAG, :IS_IDENTITY_FLAG, :SEGMENT_FIELD,
		-- [,crlfTRIGGER_NAME]~8~ directly after :PARENT_TYPE
		:SEQUENCE_GENERATOR, :PARENT_TYPE{intoClause}
	DO BEGIN
		IF (:SEGMENT_FIELD IS NULL OR :PRIMARY_DEPENDENCYCOUNT <> 1) THEN
			IS_IDENTITY_FLAG = 0;

		IF (:IN_PRIMARYKEY_FLAG = 1 AND :PRIMARY_DEPENDENCYCOUNT = 1) THEN
			IS_UNIQUE_FLAG = 1;
		ELSE
			IS_UNIQUE_FLAG = 0;
		

-- End section 2 

-- Begin section 3 - Column row complete

        SUSPEND;            

-- End section 3

-- Finalize

	END
END");


		// Evs.Trace(GetType(), nameof(GetCommandText), $"Sql: {sql}");

		return sql;

	}
		


	protected override void ProcessResult(DataTable schema, string connectionString, string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslColumns.ProcessResult");
		IBsNativeDbLinkageParser parser = null;

		if (connectionString != null)
			parser = LinkageParser.EnsureLoaded(connectionString, restrictions);


		// schema.Columns[6].ColumnName = "NumericPrecision";
		// schema.Columns[18].ColumnName = "Nullable";

		schema.Columns.Add("FIELD_DATA_TYPE", typeof(string));
		schema.Columns.Add("IDENTITY_SEED", typeof(long));
		schema.Columns.Add("IDENTITY_INCREMENT", typeof(int));
		schema.Columns.Add("IDENTITY_CURRENT", typeof(long));

		schema.Columns.Add("IS_COMPUTED", typeof(bool));
		schema.Columns.Add("IS_ARRAY", typeof(bool));
		schema.Columns.Add("IS_NULLABLE", typeof(bool));
		schema.Columns.Add("IN_PRIMARYKEY", typeof(bool));
		schema.Columns.Add("IS_UNIQUE", typeof(bool));
		schema.Columns.Add("IS_IDENTITY", typeof(bool));

		schema.AcceptChanges();
		schema.BeginLoadData();

		DataRow trig;

		foreach (DataRow row in schema.Rows)
		{
			row["IS_COMPUTED"] = Convert.ToBoolean(row["IS_COMPUTED_FLAG"]);
			row["IS_ARRAY"] = Convert.ToBoolean(row["IS_ARRAY_FLAG"]);
			row["IS_NULLABLE"] = Convert.ToBoolean(row["IS_NULLABLE_FLAG"]);
			row["IN_PRIMARYKEY"] = Convert.ToBoolean(row["IN_PRIMARYKEY_FLAG"]);
			row["IS_UNIQUE"] = Convert.ToBoolean(row["IS_UNIQUE_FLAG"]);
			row["IS_IDENTITY"] = Convert.ToBoolean(row["IS_IDENTITY_FLAG"]);


			int blrType = Convert.ToInt32(row["FIELD_TYPE"], CultureInfo.InvariantCulture);

			int subType = 0;

			if (row["FIELD_SUB_TYPE"] != DBNull.Value)
				subType = Convert.ToInt32(row["FIELD_SUB_TYPE"], CultureInfo.InvariantCulture);

			int scale = 0;

			if (row["NUMERIC_SCALE"] != DBNull.Value)
				scale = Convert.ToInt32(row["NUMERIC_SCALE"], CultureInfo.InvariantCulture);

			EnDbDataType dbType = DbTypeHelper.GetDbDataTypeFromBlrType(blrType, subType, scale);

			row["FIELD_DATA_TYPE"] = DbTypeHelper.GetDataTypeName(dbType).ToLowerInvariant();

			if (dbType == EnDbDataType.Binary || dbType == EnDbDataType.Text)
			{
				row["FIELD_SIZE"] = Int32.MaxValue;
			}

			if (dbType == EnDbDataType.Char || dbType == EnDbDataType.VarChar)
			{
				if (!row.IsNull("CHARACTER_MAX_LENGTH"))
				{
					row["FIELD_SIZE"] = row["CHARACTER_MAX_LENGTH"];
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

			if ((dbType == EnDbDataType.Decimal || dbType == EnDbDataType.Numeric) &&
				(row["NUMERIC_PRECISION"] == DBNull.Value || Convert.ToInt32(row["NUMERIC_PRECISION"]) == 0))
			{
				row["NUMERIC_PRECISION"] = row["FIELD_SIZE"];
			}

			row["NUMERIC_SCALE"] = (-1) * scale;

			string domainName = row["DOMAIN_NAME"].ToString();

			if (domainName != null && domainName.StartsWith("RDB$"))
				row["DOMAIN_NAME"] = null;

			trig = null;

			if (parser != null && Convert.ToBoolean(row["IS_IDENTITY"]) == true)
			{
				if (row["TRIGGER_NAME"] != DBNull.Value)
				{
					trig = parser?.FindTrigger(row["TRIGGER_NAME"]);
				}

				if (trig == null || Convert.ToBoolean(trig["IS_IDENTITY"]) == false)
				{
					trig = parser?.LocateIdentityTrigger(row["TABLE_NAME"], row["COLUMN_NAME"]);
				}
			}

			if (trig != null)
			{
				row["SEQUENCE_GENERATOR"] = trig["SEQUENCE_GENERATOR"];
				row["IDENTITY_SEED"] = trig["IDENTITY_SEED"];
				row["IDENTITY_INCREMENT"] = trig["IDENTITY_INCREMENT"];
				row["IDENTITY_CURRENT"] = trig["IDENTITY_CURRENT"];
				row["IS_IDENTITY"] = trig["IS_IDENTITY"];
			}
			else
			{
				row["SEQUENCE_GENERATOR"] = DBNull.Value;
				row["IDENTITY_SEED"] = 0;
				row["IDENTITY_INCREMENT"] = 0;
				row["IDENTITY_CURRENT"] = 0;
				row["IS_IDENTITY"] = false;
			}

		}


		schema.EndLoadData();

		// Remove not more needed columns
		schema.Columns.Remove("FIELD_TYPE");
		schema.Columns.Remove("CHARACTER_MAX_LENGTH");

		schema.Columns.Remove("IS_COMPUTED_FLAG");
		schema.Columns.Remove("IS_ARRAY_FLAG");
		schema.Columns.Remove("IS_NULLABLE_FLAG");
		schema.Columns.Remove("IN_PRIMARYKEY_FLAG");
		schema.Columns.Remove("IS_UNIQUE_FLAG");
		schema.Columns.Remove("IS_IDENTITY_FLAG");

		schema.AcceptChanges();

		// Evs.Trace("Rows returned: " + schema.Rows.Count);

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
	internal class ColumnType(string column, string type)
	{
		internal readonly string Column = column;
		internal readonly string Type = type;
	}


	#endregion Child classes
}
