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



namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslColumns : DslSchema
{
	/// <summary>
	/// The parent DslSchema this column collection belongs to
	/// </summary>
	protected string _ParentType = "Table";

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
	protected string _OrderingAlias = "TABLE_NAME";

	/// <summary>
	/// The FROM expression clause that returns the rdb$relation_fields
	/// row set using the alias 'r'
	/// </summary>
	protected string _FromClause = @"FROM rdb$relation_fields r";

	/// <summary>
	/// Clause for additional columns to be added to the SELECT returned columns
	/// </summary>
	protected string _ColumnsClause = "";
	/*
	 * Example
	 * 
	   protected string _ColumnsClause = @"null AS VIEW_CATALOG,
					null AS VIEW_SCHEMA,
					r.rdb$relation_name AS VIEW_NAME";
	*/



	public DslColumns() : base()
	{
	}

	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{

		// Diag.Trace();
		var sql = new StringBuilder();
		var where = new StringBuilder();
		string identityType = MajorVersionNumber >= 3 ? "r.rdb$identity_type" : "null";

		if (_ColumnsClause != "")
			_ColumnsClause = @",
					" + _ColumnsClause;


		int defaultRestrictionsLen = DslObjectTypes.GetIdentifierLength("TableColumn");
		int derivedRestrictionsLen = DslObjectTypes.GetIdentifierLength(_ParentType + "Column");

		// BlackbirdSql added IN_PRIMARYKEY
		// BlackbirdSql added TRIGGER_NAME
		// BlackbirdSql added IS_AUTOINCREMENT
		/*
		 * What this does in addition to FbColumns() (all on a LEFT OUTER JOIN)...
		 * 
		 * For the Primary Key...
		 * Selects the constraint that is 'PRIMARY KEY' then
		 * Selects the segment that matches the field as IN_PRIMARYKEY then
		 * In ProcessResult() checks if it is the only IN_PRIMARYKEY and
		 *	sets IS_UNIQUE to false if it is not.
		 *	
		 *	For Auto Increment...
		 *	Selects the Trigger that implies an Auto incremement on the 'PRIMARY KEY' 
		 *		constraint/segment for the current field then
		 *	Selects the dependent on that trigger matching the current field
		 * In ProcessResult() checks if it is the only IN_PRIMARYKEY and
		 *	sets IS_UNIQUE and IS_AUTOINCREMENT to false if it is not.
		 *	
		 *	All this establishes if the field is a singular primary key and only
		 *	then establishes if it's auto-increment.
		 *	
		 *	We don't care for cases where an IN_PRIMARYKEY is in a multi-part
		 *	index and also a multi-part auto increment, which is possible but violates
		 *	acceptable relation db design.
		 *	So you can have multiple primary key fields but not multiple 
		 *	auto-increment fields
		 *	
		 *	Because triggers have a subset of DslColumns, DslTriggerColumns inherits from 
		 *	this class but overrides this function
		 *	
		*/

		sql.AppendFormat(
			@"SELECT
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					r.rdb$relation_name AS TABLE_NAME,
					r.rdb$field_name AS COLUMN_NAME,
					r_dep.rdb$dependent_name AS TRIGGER_NAME,
				    null AS COLUMN_DATA_TYPE,
				    r_fld.rdb$field_sub_type AS COLUMN_SUB_TYPE,
					CAST(r_fld.rdb$field_length AS integer) AS COLUMN_SIZE,
					CAST(r_fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
					CAST(r_fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
					CAST(r_fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
					CAST(r_fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
					{0} AS ORDINAL_POSITION,
					null AS DOMAIN_CATALOG,
					null AS DOMAIN_SCHEMA,
					r.rdb$field_source AS DOMAIN_NAME,
					null AS SYSTEM_DATA_TYPE,
					r.rdb$default_source AS COLUMN_DEFAULT,
					(CASE WHEN r_fld.rdb$computed_source IS NULL AND r_fld.rdb$computed_blr IS NOT NULL THEN
						 cast(r_fld.rdb$computed_blr as blob sub_type 1)
					ELSE
						 r_fld.rdb$computed_source
					END) AS EXPRESSION,
					(CASE WHEN r_fld.rdb$computed_source IS NULL AND r_fld.rdb$computed_blr IS NULL THEN
						 FALSE
					ELSE
						 TRUE
					END) AS IS_COMPUTED,
					r_fld.rdb$dimensions AS COLUMN_ARRAY,
					coalesce(r_fld.rdb$null_flag, r.rdb$null_flag) AS COLUMN_NULLABLE,
				    0 AS IS_READONLY,
					r_fld.rdb$field_type AS FIELD_TYPE,
					null AS CHARACTER_SET_CATALOG,
					null AS CHARACTER_SET_SCHEMA,
					r_cs.rdb$character_set_name AS CHARACTER_SET_NAME,
					null AS COLLATION_CATALOG,
					null AS COLLATION_SCHEMA,
					r_coll.rdb$collation_name AS COLLATION_NAME,
					r.rdb$description AS DESCRIPTION,
					{1} AS IDENTITY_TYPE,
					(CASE WHEN r_seg.rdb$field_name IS NULL THEN
						FALSE
					ELSE
						TRUE
					END) AS IN_PRIMARYKEY,
					(CASE WHEN r_dep.rdb$dependent_name IS NOT NULL AND r_trg.rdb$trigger_name IS NOT NULL AND r_trg.rdb$trigger_sequence = 1 AND r_trg.rdb$flags = 1 and r_trg.rdb$trigger_type = 1 THEN
						true
					ELSE
						false
					END) AS IS_AUTOINCREMENT,
					(SELECT COUNT(*)
                        FROM rdb$dependencies r_fd
                        WHERE r_fd.rdb$field_name IS NOT NULL AND r_fd.rdb$dependent_name = r_trg.rdb$trigger_name AND r_fd.rdb$depended_on_name = r_trg.rdb$relation_name
						GROUP BY r_fd.rdb$dependent_name, r_fd.rdb$depended_on_name)
                    AS TRIGGER_DEPENDENCYCOUNT,
					'{2}' AS PARENT_TYPE{3}
				{4}
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
                    ON r_trg.rdb$relation_name = r_con.rdb$relation_name AND r_trg.rdb$trigger_sequence = 1 AND r_trg.rdb$flags = 1 and r_trg.rdb$trigger_type = 1
                        AND r_seg.rdb$index_name = r_con.rdb$index_name AND r_seg.rdb$field_name = r.rdb$field_name
				LEFT OUTER JOIN rdb$dependencies r_dep
					ON r_dep.rdb$field_name IS NOT NULL AND r_dep.rdb$depended_on_name = r_trg.rdb$relation_name AND r_dep.rdb$dependent_name = r_trg.rdb$trigger_name AND r_dep.rdb$field_name = r_seg.rdb$field_name",
			_OrdinalPosition, identityType, _ParentType, _ColumnsClause, _FromClause);


		if (restrictions != null)
		{
			var index = 0;

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
				if (derivedRestrictionsLen > defaultRestrictionsLen)
					where.AppendFormat("r.rdb$relation_name = @p{0}", index++);
				else
					where.AppendFormat("{0} = @p{1}", _ParentRestriction, index++);
			}

			/* CONSTRAINT_NAME */
			if (derivedRestrictionsLen > defaultRestrictionsLen)
			{
				if (restrictions.Length >= 4 && restrictions[3] != null)
				{
					if (where.Length > 0)
						where.Append(" AND ");

					where.AppendFormat(" AND {0} = @p{1}", _ParentRestriction, index++);
				}
			}


			/* COLUMN_NAME */
			if (restrictions.Length >= derivedRestrictionsLen && restrictions[derivedRestrictionsLen = 1] != null)
			{
				if (where.Length > 0)
					where.Append(" AND ");

				where.AppendFormat(" AND r.rdb$field_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.AppendFormat(" ORDER BY {0}, ORDINAL_POSITION", _OrderingAlias);

		// Diag.Trace(sql.ToString());


		return sql;
	}



	protected override void ProcessResult(DataTable schema)
	{
		schema.BeginLoadData();
		schema.Columns.Add("IS_NULLABLE", typeof(bool));
		schema.Columns.Add("IS_ARRAY", typeof(bool));
		schema.Columns.Add("IS_IDENTITY", typeof(bool));
		// BackbirdSql added
		schema.Columns.Add("IS_UNIQUE", typeof(bool));

		int keyCount = 0;

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

			row["IS_NULLABLE"] = (row["COLUMN_NULLABLE"] == DBNull.Value);
			row["IS_ARRAY"] = (row["COLUMN_ARRAY"] != DBNull.Value);

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

			row["IS_IDENTITY"] = row["IDENTITY_TYPE"] != DBNull.Value;

			if ((bool)row["IN_PRIMARYKEY"])
			{
				row["IS_UNIQUE"] = true;
				keyCount++;
			}
			else
			{
				row["IS_UNIQUE"] = false;
			}


			if (row["TRIGGER_DEPENDENCYCOUNT"] == DBNull.Value)
			{
				row["TRIGGER_DEPENDENCYCOUNT"] = 0;
				row["IS_AUTOINCREMENT"] = false;
			}
			else if (Convert.ToInt32(row["TRIGGER_DEPENDENCYCOUNT"]) != 1)
			{
				row["IS_AUTOINCREMENT"] = false;
			}


		}

		if (keyCount > 1)
		{
			foreach (DataRow row in schema.Rows)
			{
				if ((bool)row["IN_PRIMARYKEY"])
				{
					row["IS_UNIQUE"] = false;
					row["IS_AUTOINCREMENT"] = false;
				}
			}
		}

		schema.EndLoadData();
		schema.AcceptChanges();

		// Remove not more needed columns
		schema.Columns.Remove("COLUMN_NULLABLE");
		schema.Columns.Remove("COLUMN_ARRAY");
		schema.Columns.Remove("FIELD_TYPE");
		schema.Columns.Remove("CHARACTER_MAX_LENGTH");
		schema.Columns.Remove("IDENTITY_TYPE");
		schema.Columns.Remove("TRIGGER_DEPENDENCYCOUNT");

	}

	#endregion
}
