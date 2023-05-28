// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;

using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using BlackbirdSql.VisualStudio.Ddex.Extensions;
namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TSourceInformation Class
//
/// <summary>
/// Implementation of <see cref="IVsDataSourceInformation"/> interface
/// </summary>
// =========================================================================================================
public class TSourceInformation : AbstractSourceInformation
{
	IVsDataConnection _InstanceSite = null;

	/// <summary>
	/// Per site SourceInformation instances xref.
	/// </summary>
	protected static Dictionary<IVsDataConnection, object> _Instances = null;





	// ---------------------------------------------------------------------------------
	#region Property Accessors - TSourceInformation
	// ---------------------------------------------------------------------------------



	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - TSourceInformation
	// =========================================================================================================


	public TSourceInformation() : base()
	{
		// Diag.Trace();
		AddExtendedProperties();
	}

	public TSourceInformation(IVsDataConnection connection) : base(connection)
	{
		if (_Instances != null && _Instances.ContainsKey(connection))
		{
			DuplicateNameException ex = new("Source information for site already exists");
			Diag.Dug(ex);
			throw ex;
		}


		Instance(connection, this);


		// Diag.Trace();
		AddExtendedProperties();
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns an instance of the IVsDataSourceInformation for a site or creates one if it
	/// doesn't exist.
	/// </summary>
	/// <param name="site">
	/// The Site uniquely and distinctly associated with the IVsDataSourceInformation
	/// instance.
	/// </param>
	/// <returns>The distinctly unique IVsDataSourceInformation instance associated
	/// with the Site.</returns>
	// ---------------------------------------------------------------------------------
	internal static TSourceInformation Instance(IVsDataConnection site)
	{
		return Instance(site, null);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns an instance of the IVsDataSourceInformation for a site or creates one if it
	/// doesn't exist.
	/// </summary>
	/// <param name="site">
	/// The Site uniquely and distinctly associated with the IVsDataSourceInformation
	/// instance.
	/// </param>
	/// <returns>The distinctly unique IVsDataSourceInformation instance associated
	/// with the Site.</returns>
	// ---------------------------------------------------------------------------------
	protected static TSourceInformation Instance(IVsDataConnection site, TSourceInformation instance)
	{
		if (site == null)
		{
			ArgumentNullException ex = new ArgumentNullException("Attempt to add a null site");
			Diag.Dug(ex);
			throw ex;
		}

		if (_Instances != null)
		{
			if (instance == null && _Instances.TryGetValue(site, out object instanceObject))
			{
				// Diag.Trace("Found existing source information");
				return (TSourceInformation)instanceObject;
			}
		}

		if (instance == null)
			return new(site);

		_Instances ??= new();

		_Instances.Add(site, instance);
		instance._InstanceSite = site;

		return instance;
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
	private void AddExtendedProperties()
	{
		AddProperty(CatalogSupported, false);
		AddProperty(CatalogSupportedInDml, false);
		AddProperty(DefaultSchema, null);
		AddProperty(DefaultCatalog, null);
		AddProperty(IdentifierOpenQuote, "\"");
		AddProperty(IdentifierCloseQuote, "\"");
		AddProperty(ParameterPrefix, "@");
		AddProperty(ParameterPrefixInName, true);
		AddProperty(ProcedureSupported, true);
		AddProperty(QuotedIdentifierPartsCaseSensitive, true);
		AddProperty(SchemaSupported, false);
		AddProperty(SchemaSupportedInDml, true);
		AddProperty(ServerSeparator, ".");
		AddProperty(SupportsAnsi92Sql, true);
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
		// Diag.Trace(propertyName);

		string str;
		object retval;

		LinkageParser parser;

		PropertyDescriptor descriptor;
		IVsDataConnectionProperties connectionProperties;


		try
		{
			switch (propertyName)
			{
				case DslProperties.DefaultKeyDslPortNumber:
					descriptor = FindDescriptor(propertyName);
					connectionProperties = GetConnectionProperties();
					if ((retval = descriptor.GetValueX(connectionProperties)) == null)
						retval = DslProperties.DefaultValuePortNumber;
					break;
				case DslProperties.DefaultKeyDslServerType:
					retval = DslProperties.DefaultValueServerType;
					descriptor = FindDescriptor(propertyName);
					connectionProperties = GetConnectionProperties();
					if ((retval = descriptor.GetValueX(connectionProperties)) == null)
						retval = DslProperties.DefaultValueServerType;
					break;
				case DslProperties.DefaultKeyDslUserId:
					descriptor = FindDescriptor(DslProperties.DefaultKeyUserId);
					connectionProperties = GetConnectionProperties();
					retval = descriptor.GetValueX(connectionProperties);
					break;
				case DslProperties.DefaultKeyDslDataset:
					str = (string)GetAdoProperty(DslProperties.DefaultKeyDslCatalog);
					retval = Path.GetFileNameWithoutExtension(str);
					break;
				case DslProperties.DefaultKeyDslMemoryUsage:
					retval = -1;
					if ((Connection.State & ConnectionState.Open) != 0)
					{
						parser = LinkageParser.Instance(Site, false);
						parser?.SyncEnter();
						FbDatabaseInfo info = new((FbConnection)Connection);
						retval = info.GetCurrentMemory();
						parser?.SyncExit();
					}
					break;
				case DslProperties.DefaultKeyDslActiveUsers:
					retval = DslProperties.DefaultValueDslActiveUsers;
					if ((Connection.State & ConnectionState.Open) != 0)
					{
						parser = LinkageParser.Instance(Site, false);
						parser?.SyncEnter();
						FbDatabaseInfo info = new((FbConnection)Connection);
						retval = info.GetActiveUsers().Count;
						parser?.SyncExit();
					}
					break;
				default:
					retval = null;
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





	protected override void OnSiteChanged(EventArgs e)
	{
		if (Site == null || Site != _InstanceSite)
		{
			if (_InstanceSite != null)
			{
				_Instances.Remove(_InstanceSite);
				_InstanceSite = null;
			}

			if (Site != null)
			{
				Instance(Site, this);
			}
		}

		base.OnSiteChanged(e);

	}


	#endregion Event handlers

}
