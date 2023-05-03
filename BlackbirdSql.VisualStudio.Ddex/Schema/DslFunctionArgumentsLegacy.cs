/*
 *	This is an override of the FirebirdClient Schema
 *	We're maintaining the same structure so that it's easy to overload any GetSchema's that may need it.
 *	We still use the original Firebird metadata manifest pulled from the Firebird assembly
 *	
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Globalization;
using System.Text;

using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslFunctionArgumentsLegacy : DslSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		// Diag.Trace();
		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.AppendFormat(
			@"SELECT
					null AS FUNCTION_CATALOG,
					null AS FUNCTION_SCHEMA,
					fa.rdb$function_name AS FUNCTION_NAME,
					fa.rdb$argument_name AS ARGUMENT_NAME,
					null AS PARAMETER_DATA_TYPE,
					fld.rdb$field_sub_type AS PARAMETER_SUB_TYPE,
					fa.rdb$argument_position AS ORDINAL_POSITION,
					(CASE WHEN fa.rdb$system_flag <> 1 THEN 0 ELSE 1 END) AS IS_SYSTEM_FLAG,
					CAST(fld.rdb$field_length AS integer) AS PARAMETER_SIZE,
					CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
					CAST(fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
					CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
					CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
					coalesce(fld.rdb$null_flag, fa.rdb$null_flag) AS COLUMN_NULLABLE,
					null AS CHARACTER_SET_CATALOG,
					null AS CHARACTER_SET_SCHEMA,
					cs.rdb$character_set_name AS CHARACTER_SET_NAME,
					null AS COLLATION_CATALOG,
					null AS COLLATION_SCHEMA,
					coll.rdb$collation_name AS COLLATION_NAME,
					null AS COLLATION_CATALOG,
					null AS COLLATION_SCHEMA,
					fa.rdb$description AS DESCRIPTION,
					fld.rdb$field_type AS FIELD_TYPE,
					{0} AS PACKAGE_NAME
				FROM rdb$function_arguments fa
					LEFT OUTER JOIN rdb$fields fld ON fa.rdb$field_source = fld.rdb$field_name
					LEFT OUTER JOIN rdb$character_sets cs ON cs.rdb$character_set_id = fld.rdb$character_set_id
					LEFT OUTER JOIN rdb$collations coll ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)",
			MajorVersionNumber >= 3 ? "fa.rdb$package_name" : "(CASE WHEN rdb$system_flag <> 1 THEN 'USER' ELSE 'SYSTEM' END)");

		if (restrictions != null)
		{
			var index = 0;

			/* FUNCTION_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* FUNCTION_SCHEMA	*/
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* FUNCTION_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat("fa.rdb$function_name = @p{0}", index++);
			}

			/* PARAMETER_NAME */
			if (restrictions.Length >= 4 && restrictions[3] != null)
			{
				if (where.Length > 0)
				{
					where.Append(" AND ");
				}

				where.AppendFormat("fa.rdb$argument_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY PACKAGE_NAME, FUNCTION_NAME, ORDINAL_POSITION");

		return sql;
	}

	protected override void ProcessResult(DataTable schema)
	{
		// schema.Columns[8].ColumnName = "NumericPrecision";

		schema.BeginLoadData();
		schema.Columns.Add("NULLABLE", typeof(bool));
		/*
		 * This is frustrating...
		 * 
		 * BlackbirdSql added PSEUDO_NAME to pass the return argument's name as
		 * the name of the function.
		 * 
		 */
		schema.Columns.Add("PSEUDO_NAME", typeof(string));

		foreach (DataRow row in schema.Rows)
		{
			var blrType = Convert.ToInt32(row["FIELD_TYPE"], CultureInfo.InvariantCulture);

			var subType = 0;
			if (row["PARAMETER_SUB_TYPE"] != DBNull.Value)
			{
				subType = Convert.ToInt32(row["PARAMETER_SUB_TYPE"], CultureInfo.InvariantCulture);
			}

			var scale = 0;
			if (row["NUMERIC_SCALE"] != DBNull.Value)
			{
				scale = Convert.ToInt32(row["NUMERIC_SCALE"], CultureInfo.InvariantCulture);
			}

			row["NULLABLE"] = (row["COLUMN_NULLABLE"] == DBNull.Value);

			var dbType = (FbDbType)DslTypeHelper.GetDbDataTypeFromBlrType(blrType, subType, scale);
			row["PARAMETER_DATA_TYPE"] = DslTypeHelper.GetDataTypeName((DslDbDataType)dbType).ToLowerInvariant();

			if (dbType == FbDbType.Char || dbType == FbDbType.VarChar)
			{
				row["PARAMETER_SIZE"] = row["CHARACTER_MAX_LENGTH"];
			}
			else
			{
				row["CHARACTER_OCTET_LENGTH"] = 0;
			}

			if (dbType == FbDbType.Binary || dbType == FbDbType.Text)
			{
				row["PARAMETER_SIZE"] = Int32.MaxValue;
			}

			if (row["NUMERIC_PRECISION"] == DBNull.Value)
			{
				row["NUMERIC_PRECISION"] = 0;
			}

			if ((dbType == FbDbType.Decimal || dbType == FbDbType.Numeric) &&
				(row["NUMERIC_PRECISION"] == DBNull.Value || Convert.ToInt32(row["NUMERIC_PRECISION"]) == 0))
			{
				row["NUMERIC_PRECISION"] = row["PARAMETER_SIZE"];
			}

			row["NUMERIC_SCALE"] = (-1) * scale;

			if (row["ARGUMENT_NAME"] == DBNull.Value)
				row["PSEUDO_NAME"] = "@RETVAL";  // row["FUNCTION_NAME"];
			else
				row["PSEUDO_NAME"] = row["ARGUMENT_NAME"];
		}

		schema.EndLoadData();
		schema.AcceptChanges();

		// Remove not more needed columns
		schema.Columns.Remove("COLUMN_NULLABLE");
		schema.Columns.Remove("FIELD_TYPE");
		schema.Columns.Remove("CHARACTER_MAX_LENGTH");
	}

	#endregion
}
