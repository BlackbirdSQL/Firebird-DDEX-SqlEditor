// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbSourceInformation Class
//
/// <summary>
/// Implementation of <see cref="IVsDataSourceInformation"/> interface
/// </summary>
// =========================================================================================================
public class VxbSourceInformation : AdoDotNetSourceInformation
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbSourceInformation
	// ---------------------------------------------------------------------------------


	public VxbSourceInformation() : base()
	{
		// Evs.Trace(typeof(VxbSourceInformation), ".ctor");

		AddExtendProperties();
	}

	public VxbSourceInformation(IVsDataConnection connection) : base(connection)
	{
		// Evs.Trace(typeof(VxbSourceInformation), ".ctor(IVsDataConnection)");

		AddExtendProperties();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property Accessors - VxbSourceInformation
	// =========================================================================================================


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - VxbSourceInformation
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Adds additional properties applicable to this data source type
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void AddExtendProperties()
	{
		AddProperty(CatalogSupported, false);
		AddProperty(CatalogSupportedInDml, false);
		AddProperty(DefaultSchema);
		AddProperty(DefaultCatalog, null);
		AddProperty(DefaultSchema, null);
		AddProperty(IdentifierOpenQuote, "\"");
		AddProperty(IdentifierCloseQuote, "\"");
		AddProperty(ParameterPrefix, "@");
		AddProperty(ParameterPrefixInName, true);
		AddProperty(ProcedureSupported, true);
		AddProperty(QuotedIdentifierPartsCaseSensitive, true);
		AddProperty(SchemaSupported, false);
		AddProperty(SchemaSupportedInDml, false);
		AddProperty(ServerSeparator, ".");
		AddProperty(SupportsAnsi92Sql, true);
		AddProperty(SupportsQuotedIdentifierParts, true);
		AddProperty(SupportsCommandTimeout, false);
		AddProperty(SupportsQuotedIdentifierParts, true);
		AddProperty("DesktopDataSource", true);
		AddProperty("LocalDatabase", true);
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
		object retval = null;

		try
		{
			switch (propertyName)
			{
				case DataSourceVersion:
					try
					{
						string str = Connection.ServerVersion;
						retval = str != null
							? Connection.GetDataSourceVersion()
							: "";
					}
					catch { retval = ""; }
					break;

				default:
					break;
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

		retval ??= base.RetrieveValue(propertyName);

		return retval;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - VxbSourceInformation
	// =========================================================================================================


	#endregion Event handlers

}
