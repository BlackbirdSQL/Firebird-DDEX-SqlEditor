/*
 *  Visual Studio DDEX Provider for FirebirdClient (BlackbirdSql)
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.blackbirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2023 GA Christos
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    GA Christos
 */

using System;

namespace BlackbirdSql.VisualStudio.Ddex.Schema;

internal static class DslObjectTypes
{
	public const string Root = "";
	public const string Domain = "Domain";
	public const string Table = "Table";
	public const string TableColumn = "TableColumn";
	public const string TableIndex = "TableIndex";
	public const string TableIndexColumn = "TableIndexColumn";
	public const string TableUniqueKey = "TableUniqueKey";
	public const string TableUniqueKeyColumn = "TableUniqueKeyColumn";
	public const string TableForeignKey = "TableForeignKey";
	public const string TableForeignKeyColumn = "TableForeignKeyColumn";
	public const string View = "View";
	public const string ViewColumn = "ViewColumn";
	public const string StoredProcedure = "StoredProcedure";
	public const string StoredProcedureParameter = "StoredProcedureParameter";
	public const string StoredProcedureColumn = "StoredProcedureColumn";
	public const string ScalarFunction = "ScalarFunction";
	public const string ScalarFunctionParameter = "ScalarFunctionParameter";
}
