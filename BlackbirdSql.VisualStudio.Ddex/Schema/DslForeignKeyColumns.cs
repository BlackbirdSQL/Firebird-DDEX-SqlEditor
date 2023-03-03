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


using System.Collections.Generic;

namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslForeignKeyColumns : DslColumns
{
	public DslForeignKeyColumns() : base()
	{
		_ParentType = "ForeignKey";
		_ObjectType = "TableForeignKeyColumn";
		_ParentRestriction = "co.rdb$constraint_name";
		_OrdinalPosition = "coidxseg.rdb$field_position";
		_OrderingField = "co.rdb$constraint_name";
		_FromClause = @"rdb$relation_constraints co
				INNER JOIN rdb$ref_constraints ref 
                    ON co.rdb$constraint_type = 'FOREIGN KEY' AND co.rdb$constraint_name = ref.rdb$constraint_name
				INNER JOIN rdb$indices tempidx 
                    ON co.rdb$index_name = tempidx.rdb$index_name
				INNER JOIN rdb$index_segments coidxseg 
                    ON co.rdb$index_name = coidxseg.rdb$index_name
				INNER JOIN rdb$indices refidx 
                    ON refidx.rdb$index_name = tempidx.rdb$foreign_key
				INNER JOIN rdb$index_segments refidxseg 
                    ON refidxseg.rdb$index_name = refidx.rdb$index_name AND refidxseg.rdb$field_position = coidxseg.rdb$field_position
				INNER JOIN rdb$relation_fields r
                    ON r.rdb$relation_name = co.rdb$relation_name AND r.rdb$field_name = coidxseg.rdb$field_name";

		_AdditionalColumns.Add("CONSTRAINT_CATALOG", new(null, "varchar(10)"));
		_AdditionalColumns.Add("CONSTRAINT_SCHEMA", new(null, "varchar(10)"));
		_AdditionalColumns.Add("CONSTRAINT_NAME", new("co.rdb$constraint_name", "varchar(50)"));
		_AdditionalColumns.Add("INDEX_NAME", new("co.rdb$index_name", "varchar(50)"));
		_AdditionalColumns.Add("REFERENCED_TABLE_CATALOG", new(null, "varchar(10)"));
		_AdditionalColumns.Add("REFERENCED_TABLE_SCHEMA", new(null, "varchar(10)"));
		_AdditionalColumns.Add("REFERENCED_TABLE_NAME", new("refidx.rdb$relation_name", "varchar(50)"));
		_AdditionalColumns.Add("REFERENCED_INDEX_NAME", new("refidx.rdb$index_name", "varchar(50)"));
		_AdditionalColumns.Add("REFERENCED_COLUMN_NAME", new("refidxseg.rdb$field_name", "varchar(50)"));
	}


}
