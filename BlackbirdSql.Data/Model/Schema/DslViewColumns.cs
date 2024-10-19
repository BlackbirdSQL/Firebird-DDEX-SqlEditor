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



namespace BlackbirdSql.Data.Model.Schema;


internal class DslViewColumns : DslColumns
{

	public DslViewColumns() : base()
	{
		// Evs.Trace(GetType(), "DslViewColumns.DslViewColumns");

		_ParentType = "View";
		_ObjectType = "ViewColumn";

		_AdditionalColumns.Add("VIEW_CATALOG", new(null, "varchar(10)"));
		_AdditionalColumns.Add("VIEW_SCHEMA", new(null, "varchar(10)"));
		_AdditionalColumns.Add("VIEW_NAME", new("r.rdb$relation_name", "varchar(50)"));
	}

}
