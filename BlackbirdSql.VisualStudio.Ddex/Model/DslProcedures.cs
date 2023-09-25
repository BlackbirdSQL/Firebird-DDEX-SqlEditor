/*
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
using System.Text;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.VisualStudio.Ddex.Model;

internal class DslProcedures : AbstractDslSchema
{
	#region Protected Methods

	protected override StringBuilder GetCommandText(string[] restrictions)
	{
		Tracer.Trace(GetType(), "DslProcedures.GetCommandText");

		var sql = new StringBuilder();
		var where = new StringBuilder();

		sql.AppendFormat(
			@"SELECT
					null AS PROCEDURE_CATALOG,
					null AS PROCEDURE_SCHEMA,
					rdb$procedure_name AS PROCEDURE_NAME,
					rdb$procedure_inputs AS INPUTS,
					rdb$procedure_outputs AS OUTPUTS,
					(CASE WHEN rdb$system_flag <> 1 THEN
						 0
					ELSE
						 1
					END) AS IS_SYSTEM_FLAG,
					(CASE WHEN rdb$procedure_source IS NULL AND rdb$procedure_blr IS NOT NULL THEN
						 cast(rdb$procedure_blr as blob sub_type 1)
					ELSE
						 rdb$procedure_source
					END) AS SOURCE,
					rdb$description AS DESCRIPTION,
					{0} AS PACKAGE_NAME
				FROM rdb$procedures",
			MajorVersionNumber >= 3 ? "rdb$package_name" : "null");

		if (restrictions != null)
		{
			var index = 0;

			/* PROCEDURE_CATALOG */
			if (restrictions.Length >= 1 && restrictions[0] != null)
			{
			}

			/* PROCEDURE_SCHEMA */
			if (restrictions.Length >= 2 && restrictions[1] != null)
			{
			}

			/* PROCEDURE_NAME */
			if (restrictions.Length >= 3 && restrictions[2] != null)
			{
				where.AppendFormat("rdb$procedure_name = @p{0}", index++);
			}
		}

		if (where.Length > 0)
		{
			sql.AppendFormat(" WHERE {0} ", where.ToString());
		}

		sql.Append(" ORDER BY PACKAGE_NAME, PROCEDURE_NAME");

		return sql;
	}

	protected override void ProcessResult(DataTable schema)
	{
		Tracer.Trace(GetType(), "DslProcedures.ProcessResult");

		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			if (row["INPUTS"] == DBNull.Value)
			{
				row["INPUTS"] = 0;
			}
			if (row["OUTPUTS"] == DBNull.Value)
			{
				row["OUTPUTS"] = 0;
			}
		}

		schema.EndLoadData();
		schema.AcceptChanges();
	}

	#endregion
}
