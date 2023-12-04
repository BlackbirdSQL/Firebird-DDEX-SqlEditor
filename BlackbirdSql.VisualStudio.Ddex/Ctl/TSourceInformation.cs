// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using FirebirdSql.Data.Services;
using System.Data.Common;
using System.Data;
using System.Text;
using System;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TSourceInformation Class
//
/// <summary>
/// Implementation of <see cref="IVsDataSourceInformation"/> interface
/// </summary>
// =========================================================================================================
public class TSourceInformation : AdoDotNetSourceInformation
{



	// ---------------------------------------------------------------------------------
	#region Property Accessors - TSourceInformation
	// ---------------------------------------------------------------------------------


	#endregion Property Accessors




	// =========================================================================================================
	#region Constructors / Destructors - TSourceInformation
	// =========================================================================================================


	public TSourceInformation() : base()
	{
		AddExtendProperties();
	}

	public TSourceInformation(IVsDataConnection connection) : base(connection)
	{
		AddExtendProperties();
	}




	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - TSourceInformation
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
							? "Firebird " + FbServerProperties.ParseServerVersion(Connection.ServerVersion).ToString()
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
	#region Event handlers - TSourceInformation
	// =========================================================================================================


	#endregion Event handlers

}
