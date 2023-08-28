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

using BlackbirdSql.VisualStudio.Ddex.Extensions;



namespace BlackbirdSql.VisualStudio.Ddex.Model;


internal class DslIndexColumns : DslColumns
{
	public DslIndexColumns(LinkageParser parser) : base(parser)
	{
		_ParentType = "Index";
		_ObjectType = "TableIndexColumn";
		_ParentColumn = "idx.rdb$index_name";
		_OrderingField = "idx.rdb$index_name";
		_FromClause = @"rdb$index_segments idxseg
				INNER JOIN rdb$indices idx
					ON idxseg.rdb$index_name = idx.rdb$index_name
                INNER JOIN rdb$relation_fields r
                    ON r.rdb$relation_name = idx.rdb$relation_name AND r.rdb$field_name = idxseg.rdb$field_name";

		_RequiredColumns["ORDINAL_POSITION"] = "idxseg.rdb$field_position";

		_AdditionalColumns.Add("INDEX_NAME", new("idx.rdb$index_name", "varchar(50)"));
		_AdditionalColumns.Add("IS_DESCENDING", new("(CASE WHEN idx.rdb$index_type <> 1 THEN false ELSE true END)", "boolean"));
		_AdditionalColumns.Add("IS_INCLUDED", new("true", "boolean"));
	}

}
