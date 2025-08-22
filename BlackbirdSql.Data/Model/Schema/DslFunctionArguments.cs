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
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Shell.Interop;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace BlackbirdSql.Data.Model.Schema;


internal class DslFunctionArguments : DslColumns
{
	public DslFunctionArguments() : base()
	{
	}

	protected override void InitializeParameters(IDbConnection connection)
	{
		InitializeColumnsList(connection, "RDB$FUNCTION_ARGUMENTS");

		string packageName;

		if (!HasColumn("RDB$SYSTEM_FLAG"))
			_SystemFlagColName = "r_func.RDB$SYSTEM_FLAG";

		if (HasColumn("RDB$PACKAGE_NAME"))
		{
			packageName = $@"(CASE WHEN r.RDB$PACKAGE_NAME IS NULL THEN
				(CASE WHEN {_SystemFlagColName} <> 1 THEN 'USER' ELSE 'SYSTEM' END)
			ELSE
				r.RDB$PACKAGE_NAME
			END)";
		}
		else
		{
			packageName = $"(CASE WHEN {_SystemFlagColName} <> 1 THEN 'USER' ELSE 'SYSTEM' END)";
		}

		_ParentType = "Function";
		_ObjectType = "FunctionParameter";
		_ParentColName = "r.RDB$FUNCTION_NAME";

		_ChildColName = HasColumn("RDB$ARGUMENT_NAME") ? "r.RDB$ARGUMENT_NAME" : "CAST(r.RDB$ARGUMENT_POSITION AS varchar(50))";

		_OrderingColumn = "r.RDB$FUNCTION_NAME";
		_FromClause = @"RDB$FUNCTION_ARGUMENTS r
		INNER JOIN RDB$FUNCTIONS r_func
			ON r_func.RDB$FUNCTION_NAME = r.RDB$FUNCTION_NAME";


		_RequiredColumns["ORDINAL_POSITION"] = "r.RDB$ARGUMENT_POSITION";
		// Direction 0: IN, 1: OUT, 3: IN/OUT, 6: RETVAL
		_RequiredColumns["DIRECTION_FLAG"] = "(CASE WHEN r.RDB$ARGUMENT_POSITION = 0 THEN 6 ELSE 0 END)";

		_AdditionalColumns.Add("FUNCTION_CATALOG", new(null, "varchar(10)"));
		_AdditionalColumns.Add("FUNCTION_SCHEMA", new(null, "varchar(10)"));
		_AdditionalColumns.Add("FUNCTION_NAME", new("r.RDB$FUNCTION_NAME", "varchar(50)"));

		string argumentClause = HasColumn("RDB$ARGUMENT_NAME")
			? "r.RDB$ARGUMENT_NAME IS NULL" : "r.RDB$ARGUMENT_POSITION = 0";


		_AdditionalColumns.Add("ARGUMENT_NAME",
			new($"(CASE WHEN {argumentClause} THEN '' ELSE {_ChildColName} END)", "varchar(50)"));

		_AdditionalColumns.Add("PSEUDO_NAME",
			new($"(CASE WHEN {argumentClause} THEN '___RETURN_VALUE___' ELSE {_ChildColName} END)", "varchar(50)"));
		_AdditionalColumns.Add("PACKAGE_NAME", new(packageName, "varchar(10)"));

	}





	#region Protected Methods


	protected override void ProcessResult(DataTable schema, string connectionString, string[] restrictions)
	{
		base.ProcessResult(schema, connectionString, restrictions);


		schema.BeginLoadData();


		foreach (DataRow row in schema.Rows)
		{
			if (row["PSEUDO_NAME"].ToString().Trim() == "___RETURN_VALUE___")
				row["PSEUDO_NAME"] = "@RETURN_VALUE";
		}


		schema.EndLoadData();

	}


	#endregion Protected Methods

}
