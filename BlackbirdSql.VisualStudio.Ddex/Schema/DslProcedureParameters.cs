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



using BlackbirdSql.VisualStudio.Ddex.Extensions;



namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslProcedureParameters : DslColumns
{
	public DslProcedureParameters(LinkageParser parser) : base(parser)
	{
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

		_ParentType = "StoredProcedure";
		_ObjectType = "StoredProcedureParameter";
		_ParentColumn = "r.rdb$procedure_name";
		_ChildColumn = "r.rdb$parameter_name";
		_GeneratorSelector = null;
		_OrderingField = "r.rdb$parameter_type";
		_FromClause = "rdb$procedure_parameters r";

		_RequiredColumns["ORDINAL_POSITION"] = "r.rdb$parameter_number";

		_AdditionalColumns.Add("PROCEDURE_CATALOG", new(null, "varchar(10)"));
		_AdditionalColumns.Add("PROCEDURE_SCHEMA", new(null, "varchar(10)"));
		_AdditionalColumns.Add("PROCEDURE_NAME", new("r.rdb$procedure_name", "varchar(50)"));
		_AdditionalColumns.Add("PARAMETER_NAME", new("r.rdb$parameter_name", "varchar(50)"));
		_AdditionalColumns.Add("PARAMETER_DIRECTION", new("CAST(r.rdb$parameter_type AS integer)", "int"));
		_AdditionalColumns.Add("PACKAGE_NAME", new(packageName, "varchar(10)"));

	}


}
