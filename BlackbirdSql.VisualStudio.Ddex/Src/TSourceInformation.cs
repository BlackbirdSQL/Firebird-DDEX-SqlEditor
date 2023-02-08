//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TSourceInformation Class
//
/// <summary>
/// Implementation of <see cref="IVsDataSourceInformation"/> interface
/// </summary>
// =========================================================================================================
internal class TSourceInformation : AdoDotNetSourceInformation
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TSourceInformation
	// ---------------------------------------------------------------------------------


	public TSourceInformation() : base()
	{
		AddExtendedProperties();
	}

	public TSourceInformation(IVsDataConnection connection) : base(connection)
	{
		AddExtendedProperties();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods and Implementations - TSourceInformation
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds additional properties applicable to this data source type
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void AddExtendedProperties()
	{
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
		base.AddProperty(SchemaSupportedInDml, true);
		base.AddProperty(ServerSeparator, ".");
		base.AddProperty(SupportsAnsi92Sql, true);
		base.AddProperty(SupportsCommandTimeout, false);
		base.AddProperty(SupportsQuotedIdentifierParts, true);
		base.AddProperty("DesktopDataSource", true);
		base.AddProperty("LocalDatabase", true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves a value for a specified data source information property.
	/// </summary>
	/// <param name="propertyName"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	protected override object RetrieveValue(string propertyName)
	{
		Diag.Trace();
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


	#endregion Methods and Implementations

}
