// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Extensions;
using BlackbirdSql.Common.Providers;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using BlackbirdSql.VisualStudio.Ddex.Extensions;
using EnvDTE;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TSourceInformation Class
//
/// <summary>
/// Implementation of <see cref="IVsDataSourceInformation"/> interface
/// </summary>
// =========================================================================================================
internal class TSourceInformation : DataSourceInformation, IVsDataSourceInformation
{
	private DataTable _SourceInformation;
	private readonly object _UnretrievedValue = new object();



	// ---------------------------------------------------------------------------------
	#region Property Accessors - TSourceInformation
	// ---------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a data source information property with the specified name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	object IVsDataSourceInformation.this[string propertyName]
	{
		get
		{
			object value;

			if (ConnectionResources.RootSynonyms.TryGetValue(propertyName, out string param)
				&& SourceInformation != null && SourceInformation.Columns.Contains(param))
			{
				DataColumn col = SourceInformation.Columns[param];
				value = SourceInformation.Rows[0][col.Ordinal];


				if (value == null || value == DBNull.Value
					|| (ConnectionResources.TypeOf(propertyName) == typeof(int) && (int)value == int.MinValue))
				{
					value = RetrieveValue(param);
				}
			}
			else
			{
				value = base[propertyName];

			}

			return value;
		}
	}



	// ---------------------------------------------------------------------------------
		/// <summary>
		/// Creates or returns the data source information table.
		/// </summary>
		// ---------------------------------------------------------------------------------
	protected DataTable SourceInformation
	{
		get
		{
			if (_SourceInformation == null && Connection != null)
			{
				Site.EnsureConnected();

				_SourceInformation = CreateSourceInformationSchema();

				_SourceInformation ??= new DataTable
				{
					Locale = CultureInfo.InvariantCulture
				};
			}

			return _SourceInformation;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Return the undelying db connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected DbConnection Connection => (DbConnection)GetConnection(Site);


	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - TSourceInformation
	// =========================================================================================================


	public TSourceInformation() : base()
	{
		// Diag.Trace();
		AddStandardProperties();
		AddExtendedProperties();
	}

	public TSourceInformation(IVsDataConnection connection) : base(connection)
	{
		// Diag.Trace();
		AddStandardProperties();
		AddExtendedProperties();
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
		try
		{
			AddProperty(CatalogSupported, false);
			AddProperty(CatalogSupportedInDml, false);
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
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

	}



	private void AddStandardProperties()
	{
		foreach (KeyValuePair<string, object> pair in ConnectionResources.RootDefaultValues)
		{
			object value = pair.Value;

			if (value != null && ConnectionResources.RootTypeOf(pair.Key) == typeof(int) && (int)value == int.MinValue)
			{ 
				value = null;
			}

			if (value == null)
				AddProperty(pair.Key, _UnretrievedValue);
			else
				AddProperty(pair.Key, pair.Value);
		}

		AddProperty("SupportsNestedTransactions", false);
		AddProperty("CommandPrepareSupport", 1.ToString(CultureInfo.InvariantCulture));
		AddProperty("CommandDeriveParametersSupport", 4.ToString(CultureInfo.InvariantCulture));
		AddProperty("CommandDeriveSchemaSupport", 1.ToString(CultureInfo.InvariantCulture));
		AddProperty("CommandExecuteSupport", 1.ToString(CultureInfo.InvariantCulture));
		AddProperty("CommandParameterSupport", 7);
		AddProperty("SupportsCommandTimeout", true);
		AddProperty("SupportsAnsi92Sql");
		AddProperty("IdentifierPartsCaseSensitive");
		AddProperty("QuotedIdentifierPartsCaseSensitive");
		AddProperty("ReservedWords");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a Boolean value indicating whether the specified property is contained
	/// in the data source information instance.
	/// </summary>
	// ---------------------------------------------------------------------------------
	bool IVsDataSourceInformation.Contains(string propertyName)
	{
		if (SourceInformation != null && SourceInformation.Columns.Contains(propertyName))
			return true;

		return Contains(propertyName);
	}


	private DataTable CreateSourceInformationSchema()
	{
		DataTable schema = CreateSourceInformationSchema(Site);



		return schema;
	}


	internal static DataTable CreateSourceInformationSchema(IVsDataConnection site)
	{
		object value;
		DataTable schema;

		FbConnection connection = (FbConnection)GetConnection(site);

		// Diag.Trace();

		schema = DslSchemaFactory.GetSchema(connection, "DataSourceInformation", null);

		// Rename each column in the schema to it's AdoDotNet root column name
		foreach (DataColumn col in schema.Columns)
		{
			if (ConnectionResources.RootSynonyms.TryGetValue(col.ColumnName, out string key))
			{

				if (key != col.ColumnName)
					col.ColumnName = key;
			}
		}

		schema.AcceptChanges();


		// Add in the root types
		foreach (KeyValuePair<string, Type> pair in ConnectionResources.RootTypes)
		{
			if (!schema.Columns.Contains(pair.Key))
				schema.Columns.Add(pair.Key, pair.Value);
		}

		schema.AcceptChanges();


		IVsDataConnectionProperties connectionProperties = GetConnectionProperties(site);
		PropertyDescriptorCollection descriptors = GetDescriptors(site, connectionProperties);

		schema.BeginLoadData();


		DataRow row = schema.Rows[0];
		PropertyDescriptor descriptor;
		string descriptorName;



		// Update the row values for each descriptor
		if (descriptors != null)
		{
			foreach (DataColumn col in schema.Columns)
			{
				if (ConnectionResources.Synonyms.TryGetValue(col.ColumnName, out string key)
					&& (descriptorName = ConnectionResources.Descriptor(key)) != null)
				{
					descriptor = descriptors.Find(descriptorName, true);

					if (descriptor != null)
					{
						value = descriptor.GetValueX(connectionProperties);


						if (value != null)
							row[col.ColumnName] = value;
					}
				}
			}
		}



		schema.EndLoadData();
		schema.AcceptChanges();


		/*
		string txt = "";
		foreach (DataColumn col in schema.Columns)
		{
			txt += col.ColumnName + ":" + (schema.Rows[0][col.Ordinal] == null ? "null" : schema.Rows[0][col.Ordinal].ToString()) + ", ";
		}

		Diag.Trace(txt);
		*/

		return schema;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the connection string descriptor for a given connection string
	/// parameter or data source information property, if it exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PropertyDescriptor FindDescriptor(string paramName)
	{
		return FindDescriptor(Site, paramName);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the connection string descriptor for a given connection string
	/// parameter or data source information property, if it exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static PropertyDescriptor FindDescriptor(IVsDataConnection site, string paramName)
	{
		PropertyDescriptorCollection descriptors = GetDescriptors(site);

		if (descriptors == null)
			return null;

		PropertyDescriptor descriptor = null;
		string descriptorName = null;

		if (ConnectionResources.Synonyms.TryGetValue(paramName, out string key)
			&& (descriptorName = ConnectionResources.Descriptor(key)) != null)
		{
			descriptor = descriptors.Find(paramName, true);
		}

		if (descriptor == null)
		{
			key ??= "null";
			descriptorName ??= "null";

			ObjectNotFoundException ex = new($"Descriptor {descriptorName} for {paramName}->{key} not found");
			Diag.Dug(ex);
			throw ex;
		}


		return descriptor;
	}



	internal static object GetAdoPropertyValue(DataTable sourceInformation, string propertyName)
	{
		object result = null;
		if (sourceInformation != null && sourceInformation.Columns.Contains(propertyName))
		{
			result = sourceInformation.Rows[0][propertyName];
		}

		return result;
	}



	internal static DbConnection GetConnection(IVsDataConnection site)
	{
		if (site != null)
		{
			if (site.GetService(typeof(IVsDataConnectionSupport)) is IVsDataConnectionSupport vsDataConnectionSupport)
				return vsDataConnectionSupport.ProviderObject as DbConnection;
		}

		return null;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the connection properties object of the current connection string.
	/// </summary>
	/// <returns>
	/// The <see cref="IVsDataConnectionProperties"/> object associated with this root node.
	/// </returns>
	// ---------------------------------------------------------------------------------
	internal static IVsDataConnectionProperties GetConnectionProperties(IVsDataConnection site)
	{
		IVsDataConnectionProperties connectionProperties;

		IServiceProvider serviceProvider = site.GetService(typeof(IServiceProvider)) as IServiceProvider;


		Hostess host = new(serviceProvider);

		connectionProperties = host.GetService<IVsDataProviderManager>().Providers[site.Provider].TryCreateObject<IVsDataConnectionUIProperties>(site.Source);
		connectionProperties ??= host.GetService<IVsDataProviderManager>().Providers[site.Provider].TryCreateObject<IVsDataConnectionProperties>(site.Source);

		return connectionProperties;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the property descriptor collection for the current connection string.
	/// </summary>
	/// <returns>The property descriptor collection.</returns>
	// ---------------------------------------------------------------------------------
	internal static PropertyDescriptorCollection GetDescriptors(IVsDataConnection site, IVsDataConnectionProperties connectionProperties = null)
	{
		PropertyDescriptorCollection descriptors = null;

		connectionProperties ??= GetConnectionProperties(site);

		if (connectionProperties != null)
		{
			connectionProperties.Parse(site.SafeConnectionString);

			descriptors = TypeDescriptor.GetProperties(connectionProperties);
		}

		if (descriptors == null)
		{
			DataException ex = new("Connection descriptors is null");
			Diag.Dug(ex);
			throw ex;
		}

		return descriptors;
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
		{
			ArgumentNullException ex = new("propertyName");
			Diag.Dug(ex);
			throw ex;
		}


		if (!ConnectionResources.RootSynonyms.TryGetValue(propertyName, out string param))
		{
			return base.GetType(propertyName);
		}


		KeyValuePair<string, Type> pair = Array.Find(ConnectionResources.RootTypes,
			(KeyValuePair<string, Type> obj) => obj.Key.Equals(param, StringComparison.OrdinalIgnoreCase));

		if (pair.Key != null)
			return pair.Value;


		return base.GetType(param);
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
		if (!ConnectionResources.RootSynonyms.TryGetValue(propertyName, out string param))
		{
			return base.RetrieveValue(propertyName);
		}


		object retval = RetrieveValue(Site, SourceInformation, param);

		retval ??= base.RetrieveValue(propertyName);

		return retval;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves a value for a specified data source information property.
	/// </summary>
	/// <param name="propertyName">May not be a synonym</param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public static object RetrieveValue(IVsDataConnection site, DataTable sourceInformation, string propertyName)
	{
		// Diag.Trace(propertyName);

		string str;
		object retval = null;

		DbConnection connection;
		LinkageParser parser;

		PropertyDescriptor descriptor;
		IVsDataConnectionProperties connectionProperties;


		try
		{
			switch (propertyName)
			{
				case ConnectionResources.DefaultKeyRootDataSourceName:
					connection = GetConnection(site);
					retval = connection.DataSource;
					break;
				case ConnectionResources.DefaultKeyRootPortNumber:
					descriptor = FindDescriptor(site, propertyName);
					connectionProperties = GetConnectionProperties(site);
					if ((retval = descriptor.GetValueX(connectionProperties)) == null)
						retval = ConnectionResources.DefaultValuePortNumber;
					break;
				case ConnectionResources.DefaultKeyRootDefaultCatalog:
					descriptor = FindDescriptor(site, propertyName);
					connectionProperties = GetConnectionProperties(site);
					retval = descriptor.GetValueX(connectionProperties);
					break;
				case ConnectionResources.DefaultKeyRootServerType:
					retval = ConnectionResources.DefaultValueServerType;
					descriptor = FindDescriptor(site, propertyName);
					connectionProperties = GetConnectionProperties(site);
					if ((retval = descriptor.GetValueX(connectionProperties)) == null)
						retval = ConnectionResources.DefaultValueServerType;
					break;
				case ConnectionResources.DefaultKeyRootUserId:
					descriptor = FindDescriptor(site, ConnectionResources.DefaultKeyUserId);
					connectionProperties = GetConnectionProperties(site);
					retval = descriptor.GetValueX(connectionProperties);
					break;
				case ConnectionResources.DefaultKeyRootDataset:
					descriptor = FindDescriptor(site, ConnectionResources.DefaultKeyCatalog);
					connectionProperties = GetConnectionProperties(site);
					str = (string)descriptor.GetValueX(connectionProperties);
					retval = Path.GetFileNameWithoutExtension(str);
					break;
				case ConnectionResources.DefaultKeyRootDataSourceVersion:
					connection = GetConnection(site);
					if (connection != null && (connection.State & ConnectionState.Open) != 0)
					{
						retval = "Firebird " + FbServerProperties.ParseServerVersion(connection.ServerVersion).ToString();
					}
					break;
				case ConnectionResources.DefaultKeyRootDataSourceProductVersion:
					connection = GetConnection(site);
					if (connection != null && (connection.State & ConnectionState.Open) != 0)
					{
						retval = connection.ServerVersion;
					}
					break;
				case ConnectionResources.DefaultKeyRootDataSourceProduct:
					retval = GetAdoPropertyValue(sourceInformation, propertyName);
					break;
				case ConnectionResources.DefaultKeyRootDesktopDataSource:
					retval = ConnectionResources.DefaultValueRootDesktopDataSource;
					break;
				case ConnectionResources.DefaultKeyRootLocalDatabase:
					retval = ConnectionResources.DefaultValueRootLocalDatabase;
					break;
				case ConnectionResources.DefaultKeyRootMemoryUsage:
					connection = GetConnection(site);
					retval = ConnectionResources.DefaultValueRootMemoryUsage;
					if (connection != null && (connection.State & ConnectionState.Open) != 0)
					{
						parser = LinkageParser.Instance(site);
						parser.SyncEnter();
						FbDatabaseInfo info = new((FbConnection)connection);
						retval = info.GetCurrentMemory();
						parser.SyncExit();
					}
					break;
				case ConnectionResources.DefaultKeyRootActiveUsers:
					connection = GetConnection(site);
					retval = ConnectionResources.DefaultValueRootActiveUsers;
					if (connection != null && (connection.State & ConnectionState.Open) != 0)
					{
						parser = LinkageParser.Instance(site);
						parser.SyncEnter();
						FbDatabaseInfo info = new((FbConnection)connection);
						retval = info.GetActiveUsers().Count;
						parser.SyncExit();
					}
					break;
				case "SupportsAnsi92Sql":
					retval = false;
					object adoProperty = GetAdoPropertyValue(sourceInformation, DbMetaDataColumnNames.SupportedJoinOperators);
					if (adoProperty is SupportedJoinOperators supportedJoinOperators)
					{
						if ((supportedJoinOperators & SupportedJoinOperators.LeftOuter) > SupportedJoinOperators.None
							|| (supportedJoinOperators & SupportedJoinOperators.RightOuter) > SupportedJoinOperators.None
							|| (supportedJoinOperators & SupportedJoinOperators.FullOuter) > SupportedJoinOperators.None)
						{
							retval = true;
						}
					}
					break;
				case "IdentifierPartsCaseSensitive":
					retval = false;
					object adoProperty2 = GetAdoPropertyValue(sourceInformation, DbMetaDataColumnNames.IdentifierCase);
					if (adoProperty2 is int)
					{
						IdentifierCase identifierCase = (IdentifierCase)adoProperty2;
						if (identifierCase == IdentifierCase.Sensitive)
						{
							retval = true;
						}

						_ = 1;
					}
					break;
				case "QuotedIdentifierPartsCaseSensitive":
					retval = true;
					object adoProperty3 = GetAdoPropertyValue(sourceInformation, DbMetaDataColumnNames.QuotedIdentifierCase);
					if (adoProperty3 is int)
					{
						switch ((IdentifierCase)adoProperty3)
						{
							case IdentifierCase.Sensitive:
								retval = true;
								break;
							case IdentifierCase.Insensitive:
								retval = false;
								break;
							default:
								break;
						}
					}

					break;
				case "ReservedWords":
					DataTable dataTable = null;
					connection = GetConnection(site);
					if (connection != null)
					{
						// Site.EnsureConnected();
						dataTable = connection.GetSchema(DbMetaDataCollectionNames.ReservedWords);
					}

					if (dataTable != null)
					{
						using (dataTable)
						{
							StringBuilder stringBuilder = new StringBuilder();
							foreach (DataRow row in dataTable.Rows)
							{
								stringBuilder.Append(row[0]);
								stringBuilder.Append(",");
							}

							retval = stringBuilder.ToString().TrimEnd(',');
						}
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

		return retval;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - TSourceInformation
	// =========================================================================================================





	protected override void OnSiteChanged(EventArgs e)
	{
		base.OnSiteChanged(e);

		if (Site == null && _SourceInformation != null)
		{
			_SourceInformation.Dispose();
			_SourceInformation = null;
		}

		if (Connection != null && (Connection.State & ConnectionState.Open) != 0)
		{
			LinkageParser parser = LinkageParser.Instance((FbConnection)Connection);

			if (parser.ClearToLoadAsync)
				parser.AsyncExecute(50, 10);
		}

	}


	#endregion Event handlers

}
