﻿/*
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

using BlackbirdSql.Sys.Interfaces;

namespace BlackbirdSql.Data.Model.Schema;


internal class DslFunctionArguments : DslColumns
{
	public DslFunctionArguments() : base()
	{
		// Evs.Trace(GetType(), "DslFunctionArguments.DslFunctionArguments");

		string packageName;

		if (MajorVersionNumber >= 3)
		{
			packageName = @"(CASE WHEN r.rdb$package_name IS NULL THEN
				(CASE WHEN r.rdb$system_flag <> 1 THEN 'USER' ELSE 'SYSTEM' END)
			ELSE
				r.rdb$package_name
			END)
		END)";
		}
		else
		{
			packageName = "(CASE WHEN r.rdb$system_flag <> 1 THEN 'USER' ELSE 'SYSTEM' END)";
		}

		_ParentType = "Function";
		_ObjectType = "FunctionParameter";
		_ParentColumn = "r.rdb$function_name";
		_ChildColumn = "r.rdb$argument_name";
		_GeneratorSelector = null;
		_OrderingField = "r.rdb$function_name";
		_FromClause = "rdb$function_arguments r";


		_RequiredColumns["ORDINAL_POSITION"] = "r.rdb$argument_position";
		// Direction 0: IN, 1: OUT, 3: IN/OUT, 6: RETVAL
		_RequiredColumns["DIRECTION_FLAG"] = "(CASE WHEN r.rdb$argument_position = 0 THEN 6 ELSE 0 END)";

		_AdditionalColumns.Add("FUNCTION_CATALOG", new(null, "varchar(10)"));
		_AdditionalColumns.Add("FUNCTION_SCHEMA", new(null, "varchar(10)"));
		_AdditionalColumns.Add("FUNCTION_NAME", new("r.rdb$function_name", "varchar(50)"));
		_AdditionalColumns.Add("ARGUMENT_NAME",
			new("(CASE WHEN r.rdb$argument_name IS NULL THEN '' ELSE r.rdb$argument_name END)", "varchar(50)"));

		_AdditionalColumns.Add("PSEUDO_NAME",
			new("(CASE WHEN r.rdb$argument_name IS NULL THEN '@RETURN_VALUE' ELSE r.rdb$argument_name END)", "varchar(50)"));
		_AdditionalColumns.Add("PACKAGE_NAME", new(packageName, "varchar(10)"));

	}


}
