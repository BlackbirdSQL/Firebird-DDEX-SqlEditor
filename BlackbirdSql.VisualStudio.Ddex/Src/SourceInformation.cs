/*
 *  Visual Studio DDEX Provider for BlackbirdSql DslClient
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
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;

/// <summary>
/// Provides information about an ADO.NET data source in the form of 
/// properties passed as name/value pairs.
/// </summary>
internal class SourceInformation : AdoDotNetSourceInformation
{
	#region � Constructors �

	public SourceInformation() : this(default)
	{
		Diag.Dug();
	}

	public SourceInformation(IVsDataConnection connection) : base(connection)
	{
		Diag.Dug();

		base.AddProperty(CatalogSupported, false);
		base.AddProperty(CatalogSupportedInDml, false);
		base.AddProperty(DefaultCatalog, null);
		base.AddProperty(DefaultSchema, null);
		base.AddProperty(IdentifierOpenQuote, "\"");
		base.AddProperty(IdentifierCloseQuote, "\"");
		base.AddProperty(ParameterPrefix, "@");
		base.AddProperty(ParameterPrefixInName, true);
		base.AddProperty(ProcedureSupported, true);
		base.AddProperty(QuotedIdentifierPartsCaseSensitive, true);
		base.AddProperty(SchemaSupported, false);
		// BlackbirdSql Debug set to true to see if we can get system objects
		base.AddProperty(SchemaSupportedInDml, true);
		base.AddProperty(ServerSeparator, ".");
		base.AddProperty(SupportsAnsi92Sql, true);
		base.AddProperty(SupportsQuotedIdentifierParts, true);
		base.AddProperty(SupportsCommandTimeout, false);
		base.AddProperty(SupportsQuotedIdentifierParts, true);
		base.AddProperty("DesktopDataSource", true);
		base.AddProperty("LocalDatabase", true);
	}

	#endregion

	protected override object RetrieveValue(string propertyName)
	{
		Diag.Dug("Property: " + propertyName);

		try
		{
			switch (propertyName)
			{
				case DataSourceProduct:
					return "Firebird";
				case DataSourceVersion:
					return Connection.ServerVersion;
				default:
					return base.RetrieveValue(propertyName);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}
	}

}