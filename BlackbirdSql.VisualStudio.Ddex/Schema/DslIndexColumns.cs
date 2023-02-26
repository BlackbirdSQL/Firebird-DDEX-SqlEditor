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

using BlackbirdSql.Common;

namespace BlackbirdSql.VisualStudio.Ddex.Schema;


internal class DslIndexColumns : DslColumns
{
	public DslIndexColumns() : base()
	{
		_ParentType = "Index";
		_ObjectType = "TableIndexColumn";
		_ParentRestriction = "idx.rdb$index_name";
		_OrdinalPosition = "idxseg.rdb$field_position";
		_OrderingAlias = "INDEX_NAME";
		_FromClause = @"FROM rdb$index_segments idxseg
				INNER JOIN rdb$indices idx
					ON idxseg.rdb$index_name = idx.rdb$index_name
                INNER JOIN rdb$relation_fields r
                    ON r.rdb$relation_name = idx.rdb$relation_name AND r.rdb$field_name = idxseg.rdb$field_name";
		_ColumnsClause = @"idx.rdb$index_name AS INDEX_NAME,
					null AS CONSTRAINT_CATALOG,
					null AS CONSTRAINT_SCHEMA,
					idx.rdb$index_name AS CONSTRAINT_NAME,
					r_dep.rdb$dependent_name AS TRIGGER_NAME";
	}

}
