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

using System.Data;
using BlackbirdSql.Sys.Interfaces;
using FirebirdSql.Data.FirebirdClient;

namespace BlackbirdSql.Data.Model.Schema;


internal class DslProcedureParameters : DslColumns
{
	public DslProcedureParameters() : base()
	{
		// Evs.Trace(GetType(), "DslProcedureParameters.DslProcedureParameters");
	}



	protected override void InitializeParameters(IDbConnection connection)
	{
		InitializeColumnsList(connection, "RDB$PROCEDURE_PARAMETERS");


		string packageName;

		if (MajorVersionNumber >= 3)
		{
			packageName = @"(CASE WHEN r.RDB$PACKAGE_NAME IS NULL THEN
				(CASE WHEN r.RDB$SYSTEM_FLAG <> 1 THEN 'USER' ELSE 'SYSTEM' END)
			ELSE
				r.RDB$PACKAGE_NAME
			END)
		END)";
		}
		else
		{
			packageName = "(CASE WHEN r.RDB$SYSTEM_FLAG <> 1 THEN 'USER' ELSE 'SYSTEM' END)";
		}

		_ParentType = "StoredProcedure";
		_ObjectType = "StoredProcedureParameter";
		_ParentColName = "r.RDB$PROCEDURE_NAME";
		_ChildColName = "r.RDB$PARAMETER_NAME";
		_GeneratorColName = null;
		_OrderingColumn = "r.RDB$PARAMETER_TYPE";
		_FromClause = "RDB$PROCEDURE_PARAMETERS r";

		_RequiredColumns["ORDINAL_POSITION"] = "r.RDB$PARAMETER_NUMBER";
		// Direction 0: IN, 1: OUT, 3: IN/OUT, 6: RETVAL
		_RequiredColumns["DIRECTION_FLAG"] = "CAST(r.RDB$PARAMETER_TYPE AS integer)";

		_AdditionalColumns.Add("PROCEDURE_CATALOG", new(null, "varchar(10)"));
		_AdditionalColumns.Add("PROCEDURE_SCHEMA", new(null, "varchar(10)"));
		_AdditionalColumns.Add("PROCEDURE_NAME", new("r.RDB$PROCEDURE_NAME", "varchar(50)"));
		_AdditionalColumns.Add("PARAMETER_NAME", new("r.RDB$PARAMETER_NAME", "varchar(50)"));
		_AdditionalColumns.Add("PACKAGE_NAME", new(packageName, "varchar(10)"));

	}


}
