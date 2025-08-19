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
using BlackbirdSql.Sys.Interfaces;
using FirebirdSql.Data.FirebirdClient;

namespace BlackbirdSql.Data.Model.Schema;


internal class DslIndexColumns : DslColumns
{
	public DslIndexColumns() : base()
	{
		// Evs.Trace(GetType(), "DslIndexColumns.DslIndexColumns");
	}


	protected override void InitializeParameters(IDbConnection connection)
	{
		base.InitializeParameters(connection);

		_ParentType = "Index";
		_ObjectType = "TableIndexColumn";
		_ParentColName = "idx.RDB$INDEX_NAME";
		_OrderingColumn = "idx.RDB$INDEX_NAME";
		_FromClause = @"RDB$INDEX_SEGMENTS idxseg
				INNER JOIN RDB$INDICES idx
					ON idxseg.RDB$INDEX_NAME = idx.RDB$INDEX_NAME
                INNER JOIN RDB$RELATION_FIELDS r
                    ON r.RDB$RELATION_NAME = idx.RDB$RELATION_NAME AND r.RDB$FIELD_NAME = idxseg.RDB$FIELD_NAME";

		_RequiredColumns["ORDINAL_POSITION"] = "idxseg.RDB$FIELD_POSITION";

		_AdditionalColumns.Add("INDEX_NAME", new("idx.RDB$INDEX_NAME", "varchar(50)"));
		_AdditionalColumns.Add("IS_DESCENDING_FLAG", new("(CASE WHEN idx.RDB$INDEX_TYPE <> 1 THEN 0 ELSE 1 END)", "int"));
		_AdditionalColumns.Add("IS_INCLUDED_FLAG", new("1", "int"));
	}

	protected override void ProcessResult(DataTable schema, string connectionString, string[] restrictions)
	{
		// Evs.Trace(GetType(), "DslForeignKeys.ProcessResult");

		schema.Columns.Add("IS_DESCENDING", typeof(bool));
		schema.Columns.Add("IS_INCLUDED", typeof(bool));

		schema.AcceptChanges();
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			row["IS_DESCENDING"] = Convert.ToBoolean(row["IS_DESCENDING_FLAG"]);
			row["IS_INCLUDED"] = Convert.ToBoolean(row["IS_INCLUDED_FLAG"]);
		}

		schema.EndLoadData();

		schema.Columns.Remove("IS_DESCENDING_FLAG");
		schema.Columns.Remove("IS_INCLUDED_FLAG");

		schema.AcceptChanges();

		base.ProcessResult(schema, connectionString, restrictions);
	}

}
