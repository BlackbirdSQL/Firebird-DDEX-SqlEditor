﻿/*
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
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Diag.Trace();
		var sql = new StringBuilder();
		var where = new StringBuilder();


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
					rfr.rdb$relation_name AS TABLE_NAME,
					rfr.rdb$field_name AS COLUMN_NAME,
					dep.rdb$dependent_name AS TRIGGER_NAME,
				    null AS COLUMN_DATA_TYPE,
				    fld.rdb$field_sub_type AS COLUMN_SUB_TYPE,
					CAST(fld.rdb$field_length AS integer) AS COLUMN_SIZE,
					CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
					CAST(fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
					CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
					CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
					rfr.rdb$field_position AS ORDINAL_POSITION,
					null AS DOMAIN_CATALOG,
					null AS DOMAIN_SCHEMA,
					rfr.rdb$field_source AS DOMAIN_NAME,
					null AS SYSTEM_DATA_TYPE,
					rfr.rdb$default_source AS COLUMN_DEFAULT,
					(CASE WHEN fld.rdb$computed_source IS NULL AND fld.rdb$computed_blr IS NOT NULL THEN
						 cast(fld.rdb$computed_blr as blob sub_type 1)
					ELSE
						 fld.rdb$computed_source
					END) AS EXPRESSION,
					fld.rdb$dimensions AS COLUMN_ARRAY,
					coalesce(fld.rdb$null_flag, rfr.rdb$null_flag) AS COLUMN_NULLABLE,
				    0 AS IS_READONLY,
					fld.rdb$field_type AS FIELD_TYPE,
					null AS CHARACTER_SET_CATALOG,
					null AS CHARACTER_SET_SCHEMA,
					cs.rdb$character_set_name AS CHARACTER_SET_NAME,
					null AS COLLATION_CATALOG,
					null AS COLLATION_SCHEMA,
					coll.rdb$collation_name AS COLLATION_NAME,
					rfr.rdb$description AS DESCRIPTION,
					{0} AS IDENTITY_TYPE,
					(CASE WHEN seg.rdb$field_name IS NULL THEN
						FALSE
					ELSE
						TRUE
					END) AS IN_PRIMARYKEY,
					(CASE WHEN dep.rdb$dependent_name IS NOT NULL AND trg.rdb$trigger_name IS NOT NULL AND trg.rdb$trigger_sequence = 1 AND trg.rdb$flags = 1 and trg.rdb$trigger_type = 1 THEN
						1
					ELSE
						0
					END) AS IS_AUTOINCREMENT,
					(SELECT COUNT(*)
                        FROM rdb$dependencies fd
                        WHERE fd.rdb$field_name IS NOT NULL AND fd.rdb$dependent_name = trg.rdb$trigger_name AND fd.rdb$depended_on_name = trg.rdb$relation_name
						GROUP BY fd.rdb$dependent_name, fd.rdb$depended_on_name)
                    AS TRIGGER_DEPENDENCYCOUNT
				FROM rdb$relation_fields rfr
				JOIN rdb$fields fld
					ON rfr.rdb$field_source = fld.rdb$field_name
				LEFT JOIN rdb$character_sets cs
					ON cs.rdb$character_set_id = fld.rdb$character_set_id
				LEFT JOIN rdb$collations coll
					ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)
				LEFT JOIN rdb$relation_constraints con
					ON con.rdb$relation_name = rfr.rdb$relation_name AND con.rdb$constraint_type = 'PRIMARY KEY'
				LEFT JOIN rdb$index_segments seg 
					ON seg.rdb$index_name = con.rdb$index_name AND seg.rdb$field_name = rfr.rdb$field_name
                LEFT JOIN rdb$triggers trg
                    ON trg.rdb$relation_name = con.rdb$relation_name AND trg.rdb$trigger_sequence = 1 AND trg.rdb$flags = 1 and trg.rdb$trigger_type = 1
                        AND seg.rdb$index_name = con.rdb$index_name AND seg.rdb$field_name = rfr.rdb$field_name
				LEFT JOIN rdb$dependencies dep
					ON dep.rdb$field_name IS NOT NULL AND dep.rdb$depended_on_name = trg.rdb$relation_name AND dep.rdb$dependent_name = trg.rdb$trigger_name AND dep.rdb$field_name = seg.rdb$field_name",
			MajorVersionNumber >= 3 ? "rfr.rdb$identity_type" : "null");

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

			/* TABLE_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat("rfr.rdb$relation_name = @p{0}", index++);
			}

			/* COLUMN_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.AppendFormat("rfr.rdb$field_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY TABLE_NAME, ORDINAL_POSITION");


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
		schema.Columns.Add("COMPUTED", typeof(bool));

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

			row["COMPUTED"] = row["EXPRESSION"] != DBNull.Value;


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
