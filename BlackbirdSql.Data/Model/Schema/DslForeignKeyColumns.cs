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


internal class DslForeignKeyColumns : DslColumns
{
	public DslForeignKeyColumns() : base()
	{
		// Evs.Trace(GetType(), "DslForeignKeyColumns.DslForeignKeyColumns");
	}

	protected override void InitializeParameters(IDbConnection connection)
	{
		base.InitializeParameters(connection);

		_ParentType = "ForeignKey";
		_ObjectType = "TableForeignKeyColumn";
		_ParentColName = "co.RDB$CONSTRAINT_NAME";
		_OrderingColumn = "co.RDB$CONSTRAINT_NAME";
		_FromClause = @"RDB$RELATION_CONSTRAINTS co
				INNER JOIN RDB$REF_CONSTRAINTS ref 
                    ON co.RDB$CONSTRAINT_TYPE = 'FOREIGN KEY' AND co.RDB$CONSTRAINT_NAME = ref.RDB$CONSTRAINT_NAME
				INNER JOIN RDB$INDICES tempidx 
                    ON co.RDB$INDEX_NAME = tempidx.RDB$INDEX_NAME
				INNER JOIN RDB$INDEX_SEGMENTS coidxseg 
                    ON co.RDB$INDEX_NAME = coidxseg.RDB$INDEX_NAME
				INNER JOIN RDB$INDICES refidx 
                    ON refidx.RDB$INDEX_NAME = tempidx.RDB$FOREIGN_KEY
				INNER JOIN RDB$INDEX_SEGMENTS refidxseg 
                    ON refidxseg.RDB$INDEX_NAME = refidx.RDB$INDEX_NAME AND refidxseg.RDB$FIELD_POSITION = coidxseg.RDB$FIELD_POSITION
				INNER JOIN RDB$RELATION_FIELDS r
                    ON r.RDB$RELATION_NAME = co.RDB$RELATION_NAME AND r.RDB$FIELD_NAME = coidxseg.RDB$FIELD_NAME";

		_RequiredColumns["ORDINAL_POSITION"] = "coidxseg.RDB$FIELD_POSITION";

		_AdditionalColumns.Add("CONSTRAINT_CATALOG", new(null, "varchar(10)"));
		_AdditionalColumns.Add("CONSTRAINT_SCHEMA", new(null, "varchar(10)"));
		_AdditionalColumns.Add("CONSTRAINT_NAME", new("co.RDB$CONSTRAINT_NAME", "varchar(50)"));
		_AdditionalColumns.Add("INDEX_NAME", new("co.RDB$INDEX_NAME", "varchar(50)"));
		_AdditionalColumns.Add("REFERENCED_TABLE_CATALOG", new(null, "varchar(10)"));
		_AdditionalColumns.Add("REFERENCED_TABLE_SCHEMA", new(null, "varchar(10)"));
		_AdditionalColumns.Add("REFERENCED_TABLE_NAME", new("refidx.RDB$RELATION_NAME", "varchar(50)"));
		_AdditionalColumns.Add("REFERENCED_INDEX_NAME", new("refidx.RDB$INDEX_NAME", "varchar(50)"));
		_AdditionalColumns.Add("REFERENCED_COLUMN_NAME", new("refidxseg.RDB$FIELD_NAME", "varchar(50)"));
		_AdditionalColumns.Add("UPDATE_ACTION",
			new("CASE(ref.RDB$UPDATE_RULE) WHEN 'CASCADE' THEN 1 WHEN 'SET NULL' THEN 2 WHEN 'SET_DEFAULT' THEN 3 ELSE 0 END", "int"));
		_AdditionalColumns.Add("DELETE_ACTION",
			new("CASE(ref.RDB$DELETE_RULE) WHEN 'CASCADE' THEN 1 WHEN 'SET NULL' THEN 2 WHEN 'SET_DEFAULT' THEN 3 ELSE 0 END", "int"));

	}


}
