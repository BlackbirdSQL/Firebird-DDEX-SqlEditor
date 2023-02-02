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
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;

namespace BlackbirdSql.VisualStudio.DataTools;

/// <summary>
/// Provides information about an ADO.NET data source in the form of 
/// properties passed as name/value pairs.
/// </summary>
internal class SourceInformation : AdoDotNetDataSourceInformation
{
	#region · Constructors ·

	public SourceInformation(DataConnection connection) : base(connection)
	{
		base.AddProperty(CatalogSupported, false);
		base.AddProperty(CatalogSupportedInDml, false);
		base.AddProperty(DefaultSchema);
		base.AddProperty(DefaultCatalog, null);
		base.AddProperty(DefaultSchema, null);
		base.AddProperty(IdentifierOpenQuote, "\"");
		base.AddProperty(IdentifierCloseQuote, "\"");
		base.AddProperty(ParameterPrefix, "@");
		base.AddProperty(ParameterPrefixInName, true);
		base.AddProperty(ProcedureSupported, true);
		base.AddProperty(QuotedIdentifierPartsCaseSensitive, true);
		base.AddProperty(SchemaSupported, false);
		base.AddProperty(SchemaSupportedInDml, false);
		base.AddProperty(ServerSeparator, ".");
		base.AddProperty(SupportsAnsi92Sql, true);
		base.AddProperty(SupportsQuotedIdentifierParts, true);
		base.AddProperty(SupportsCommandTimeout, false);
		base.AddProperty(SupportsQuotedIdentifierParts, true);
		base.AddProperty("DesktopDataSource", true);
		base.AddProperty("LocalDatabase", true);
	}

	#endregion
}
