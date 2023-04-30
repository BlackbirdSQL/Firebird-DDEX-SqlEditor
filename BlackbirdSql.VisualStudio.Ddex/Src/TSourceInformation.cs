//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using BlackbirdSql.VisualStudio.Ddex.Extensions;
using System.Data.Common;
using Microsoft.VisualStudio.LanguageServer.Client;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TSourceInformation Class
//
/// <summary>
/// Implementation of <see cref="IVsDataSourceInformation"/> interface
/// </summary>
// =========================================================================================================
internal class TSourceInformation : AdoDotNetSourceInformation, IVsDataSourceInformation
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TSourceInformation
	// ---------------------------------------------------------------------------------


	public TSourceInformation() : base()
	{
		// Diag.Trace();
		AddExtendedProperties();
	}

	public TSourceInformation(IVsDataConnection connection) : base(connection)
	{
		// Diag.Trace();
		AddExtendedProperties();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Implementations - TSourceInformation
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a data source information property with the specified name.
	/// </summary>
	/// <param name="propertyName">
	/// The name of the data source information property to retrieve.
	/// </param>
	/// <returns>
	/// The data source information property with the specified name.
	/// </returns>
	// ---------------------------------------------------------------------------------
	object IVsDataSourceInformation.this[string propertyName]
	{
		get
		{
			object obj = base[propertyName];

			if (obj == null && SourceInformation != null && SourceInformation.Columns.Contains(propertyName))
			{
				obj = SourceInformation.Rows[0][propertyName];
			}

			return obj;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves a Boolean value indicating whether the specified property is contained
	/// in the data source information instance.
	/// </summary>
	/// <param name="propertyName">
	/// The name of a data source information property.
	/// </param>
	/// <returns>
	/// true if the specified property is contained in the data source information instance;
	/// otherwise, false.
	/// </returns>
	// ---------------------------------------------------------------------------------
	bool IVsDataSourceInformation.Contains(string propertyName)
	{
		if (!Contains(propertyName))
		{
			if (SourceInformation != null)
				return SourceInformation.Columns.Contains(propertyName);

			return false;
		}

		return true;
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

		object retval = null;

		PropertyDescriptor descriptor;
		IVsDataConnectionProperties connectionProperties;

		if (!DslConnectionString.Synonyms.TryGetValue(propertyName, out string param))
			return base.RetrieveValue(propertyName);

		try
		{
			switch (param)
			{
				case DslConnectionString.DefaultKeyDataSourceProduct:
					retval = Properties.Resources.DataSource_Product;
					break;
				case DslConnectionString.DefaultKeyServerVersion:
					if (Connection != null && Connection.ServerVersion != null)
					{
						retval = "Firebird " + FbServerProperties.ParseServerVersion(Connection.ServerVersion).ToString();
					}
					break;
				case DslConnectionString.DefaultKeyPortNumber:
					retval = DslConnectionString.DefaultValuePortNumber;
					if ((descriptor = FindDescriptor(param)) == null)
						break;
					connectionProperties = GetConnectionProperties();
					if ((retval = descriptor.GetValueX(connectionProperties)) == null)
						retval = DslConnectionString.DefaultValuePortNumber;
					break;
				case DslConnectionString.DefaultKeyServerType:
					retval = DslConnectionString.DefaultValueServerType;
					if ((descriptor = FindDescriptor(param)) == null)
						break;
					connectionProperties = GetConnectionProperties();
					if ((retval = descriptor.GetValueX(connectionProperties)) == null)
						retval = DslConnectionString.DefaultValueServerType;
					break;
				case DslConnectionString.DefaultKeyCatalog:
					if ((descriptor = FindDescriptor(param)) == null)
						break;
					connectionProperties = GetConnectionProperties();
					retval = descriptor.GetValueX(connectionProperties);
					break;
				case DslConnectionString.DefaultKeyUserId:
					if ((descriptor = FindDescriptor(param)) == null)
						break;
					connectionProperties = GetConnectionProperties();
					retval = descriptor.GetValueX(connectionProperties);
					break;
				case DslConnectionString.DefaultKeyMemoryUsage:
					retval = -1;
					if (Connection != null && Connection.State != ConnectionState.Closed && Connection.State != ConnectionState.Broken)
					{
						FbDatabaseInfo info = new((FbConnection)Connection);
						retval = info.GetCurrentMemory();
					}
					break;
				case DslConnectionString.DefaultKeyActiveUsers:
					retval = -1;
					if (Connection != null && Connection.State != ConnectionState.Closed && Connection.State != ConnectionState.Broken)
					{
						FbDatabaseInfo info = new((FbConnection)Connection);
						retval = info.GetActiveUsers();
					}
					break;
				default:
					retval = base.RetrieveValue(propertyName);
					break;
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

		return retval;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the connection string descriptor for a given connection string
	/// parameter or data source information property, if it exists.
	/// </summary>
	/// <param name="paramName">
	/// The connection string parameter or data source information property
	/// name.
	/// </param>
	/// <returns>
	/// The connection string parameter or data source information property
	/// connection descriptor, or null if it does not exist.
	/// </returns>
	// ---------------------------------------------------------------------------------
	protected PropertyDescriptor FindDescriptor(string paramName)
	{
		PropertyDescriptorCollection descriptors = GetDescriptors();

		if (descriptors == null)
			return null;

		PropertyDescriptor descriptor;

		if (!DslConnectionString.Synonyms.ContainsKey(paramName))
			return null;

		if ((descriptor = descriptors.Find(paramName, true)) != null)
			return descriptor;

		foreach (string key in DslConnectionString.Synonyms.Values)
		{
			if ((descriptor = descriptors.Find(key, true)) != null)
				return descriptor;
		}

		return null;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves the System.Type value indicating the type of a specified property,
	/// thus enabling appropriate conversion of a retrieved value to the correct type.
	/// </summary>
	/// <param name="propertyName">
	/// The name of the property for which to get the type.
	/// </param>
	/// <returns>
	/// A System.Type value indicating the type of a specified property.
	/// </returns>
	/// <exception cref="ArgumentNullException"></exception>
	// ---------------------------------------------------------------------------------
	protected override Type GetType(string propertyName)
	{
		if (propertyName == null)
			throw new ArgumentNullException("propertyName");


		if (!DslConnectionString.Synonyms.TryGetValue(propertyName, out string param))
			return base.GetType(propertyName);

		if (DslConnectionString.RootTypes.TryGetValue(param, out Type retval))
			return retval;

		if (DslConnectionString.SystemTypes.TryGetValue(param, out retval))
			return retval;

		return base.GetType(propertyName);
	}


	#endregion Implementations





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
		try
		{
			AddProperty(CatalogSupported, false);
			AddProperty(CatalogSupportedInDml, false);
			AddProperty(DefaultCatalog, null);
			AddProperty(DefaultSchema, null);
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

			foreach (KeyValuePair<string, Type> pair in DslConnectionString.RootTypes)
			{
				AddProperty(pair.Key.DescriptorCase());
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the property descriptor collection for the current connection string.
	/// </summary>
	/// <returns>The property descriptor collection.</returns>
	// ---------------------------------------------------------------------------------
	protected PropertyDescriptorCollection GetDescriptors()
	{
		PropertyDescriptorCollection descriptors = null;
		IVsDataConnectionProperties connectionProperties = GetConnectionProperties();

		if (connectionProperties != null)
		{
			connectionProperties.Parse(Site.SafeConnectionString);

			descriptors = TypeDescriptor.GetProperties(connectionProperties);
		}

		return descriptors;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the connection properties object of the current connection string.
	/// </summary>
	/// <returns>
	/// The <see cref="IVsDataConnectionProperties"/> object associated with this root node.
	/// </returns>
	// ---------------------------------------------------------------------------------
	protected IVsDataConnectionProperties GetConnectionProperties()
	{
		IVsDataConnectionProperties connectionProperties;

		IServiceProvider serviceProvider = Site.GetService(typeof(IServiceProvider)) as IServiceProvider;


		using (Hostess host = new(serviceProvider))
		{
			connectionProperties = host.GetService<IVsDataProviderManager>().Providers[Site.Provider].TryCreateObject<IVsDataConnectionUIProperties>(Site.Source);
			connectionProperties ??= host.GetService<IVsDataProviderManager>().Providers[Site.Provider].TryCreateObject<IVsDataConnectionProperties>(Site.Source);
		}

		return connectionProperties;
	}



	protected override void OnSiteChanged(EventArgs e)
	{
		base.OnSiteChanged(e);

		if (Connection != null && (Connection.State & ConnectionState.Open) != 0)
		{
			// Bad place
			// LinkageParser parser = LinkageParser.Instance((FbConnection)Connection);

			// if (parser.ClearToLoadAsync)
			//	parser.AsyncExecute();
		}

	}


	#endregion Methods

}
