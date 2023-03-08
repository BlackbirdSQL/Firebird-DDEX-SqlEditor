//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using BlackbirdSql.Common.Extensions;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TObjectSelectorRoot Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSelector"/> enumerator interface for the root node
/// </summary>
// =========================================================================================================
class TObjectSelectorRoot : AdoDotNetRootObjectSelector
{

	// ---------------------------------------------------------------------------------
	#region Variables - TObjectSelectorRoot
	// ---------------------------------------------------------------------------------


	#endregion Variables





	// =========================================================================================================
	#region Constructors / Destructors - TObjectSelectorRoot
	// =========================================================================================================


	public TObjectSelectorRoot() : base()
	{
		// Diag.Trace();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Implementations - TObjectSelectorRoot
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enumerates the root node object.
	/// </summary>
	/// <param name="typeName"></param>
	/// <param name="restrictions"></param>
	/// <param name="properties"></param>
	/// <param name="parameters"></param>
	/// <returns>A data reader of the root object</returns>
	// ---------------------------------------------------------------------------------
	protected override IVsDataReader SelectObjects(string typeName, object[] restrictions, string[] properties, object[] parameters)
	{
		// Diag.Trace(typeName);

		try
		{
			if (typeName == null || typeName.Length > 0)
				throw new ArgumentNullException("typeName");

			if (Site == null)
				throw new InvalidOperationException("Site is null");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}


		object lockedProviderObject = Site.GetLockedProviderObject();


		if (lockedProviderObject == null || lockedProviderObject is not DbConnection connection)
		{
			NotImplementedException ex = new("Site.GetLockedProviderObject()");
			Diag.Dug(ex);
			throw ex;
		}

		IVsDataReader reader;
		DataTable schema;

		try
		{
			schema = GetRootSchema(connection, parameters);

			reader = new AdoDotNetTableReader(schema);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;

		}
		finally
		{
			Site.UnlockProviderObject();
		}

		return reader;
	}


	#endregion Implementations





	// =========================================================================================================
	#region Methods - TObjectSelectorRoot
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Reads in the data source information schema and adds connection property descriptor
	/// columns to it as well as additional root node properties defined in
	/// <see cref="DslConnectionString.RootTypes"/>.
	/// </summary>
	/// <param name="connection"></param>
	/// <param name="parameters"></param>
	/// <returns>Thr root node ready DataSourceInformation schema.</returns>
	// ---------------------------------------------------------------------------------
	private DataTable GetRootSchema(DbConnection connection, object[] parameters)
	{
		object connectionValue;

		// Diag.Trace();

		DataTable schema = DslSchemaFactory.GetSchema((FbConnection)connection, "DataSourceInformation", null);
		string[] schemaColNames = new string[schema.Columns.Count];
		
		string key;



		// Rename each column in the schema to it's correct connection parameter name
		// for now, if that exists, and store it's DescriptorCase name.
		// If it doesn't exist store it's original name which is in DescriptorCase format
		foreach (DataColumn col in schema.Columns)
		{
			if (DslConnectionString.Synonyms.TryGetValue(col.ColumnName, out key))
			{

				if (key != col.ColumnName)
					col.ColumnName = key;

				schemaColNames[col.Ordinal] = key.DescriptorCase();
			}
			else
			{
				schemaColNames[col.Ordinal] = col.ColumnName;
			}
		}

		schema.AcceptChanges();

		// Add in the root types
		foreach (KeyValuePair<string, Type> pair in DslConnectionString.RootTypes)
		{
			if (!schema.Columns.Contains(pair.Key))
				schema.Columns.Add(pair.Key, pair.Value);
		}

		schema.AcceptChanges();



		IVsDataConnectionProperties connectionProperties = GetConnectionProperties();
		PropertyDescriptorCollection descriptors = GetDescriptors(connectionProperties);

		// Add in any missing connection descriptor columns
		if (descriptors != null)
		{
			foreach (PropertyDescriptor descriptor in descriptors)
			{
				if (DslConnectionString.Synonyms.TryGetValue(descriptor.Name, out key))
				{
					if (!schema.Columns.Contains(key))
						schema.Columns.Add(key, descriptor.PropertyType);
				}
			}
			schema.AcceptChanges();
		}



		Site.EnsureConnected();
		schema.BeginLoadData();


		DataRow row = schema.Rows[0];

		// Update the row values for each descriptor
		if (descriptors != null)
		{
			foreach (PropertyDescriptor descriptor in descriptors)
			{
				if (DslConnectionString.Synonyms.TryGetValue(descriptor.Name, out key))
				{
					connectionValue = descriptor.GetValueX(connectionProperties);

					if (connectionValue != null)
						row[key] = connectionValue;
				}
			}
		}

		FbDatabaseInfo info = new((FbConnection)connection);

		row[DslConnectionString.DefaultKeyDataSourceProduct] = Properties.Resources.DataSource_Product;
		row[DslConnectionString.DefaultKeyServerVersion] = "Firebird " + FbServerProperties.ParseServerVersion(connection.ServerVersion).ToString();
		row[DslConnectionString.DefaultKeyMemoryUsage] = info.GetCurrentMemory();
		row[DslConnectionString.DefaultKeyActiveUsers] = info.GetActiveUsers().Count;


		schema.EndLoadData();
		schema.AcceptChanges();


		string txt = "";
		// Finally for each column set default if null and
		// titlecase it's column name
		foreach (DataColumn col in schema.Columns)
		{
			if (row[col.Ordinal] == null)
			{
				if (DslConnectionString.DefaultValues.TryGetValue(col.ColumnName, out object value))
					row[col.Ordinal] = value;
				else
					row[col.Ordinal] = DBNull.Value;
			}


			if (col.Ordinal < schemaColNames.Length)
				key = schemaColNames[col.Ordinal];
			else
				key = col.ColumnName.DescriptorCase();

			if (col.ColumnName != key)
				col.ColumnName = key;


			txt += key + ":" + (row[col.Ordinal] == DBNull.Value ? "DBNull" : row[col.Ordinal].ToString()) + ", ";
		}

		schema.AcceptChanges();



		if (parameters != null && parameters.Length == 1 && parameters[0] is DictionaryEntry entry)
		{
			if (entry.Value is object[] array)
			{
				IDictionary<string, object> mappings = GetMappings(array);
				ApplyMappings(schema, mappings);
			}
		}


		return schema;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the property descriptor collection for the current connection string.
	/// </summary>
	/// <param name="connectionProperties"></param>
	/// <returns>The property descriptor collection.</returns>
	// ---------------------------------------------------------------------------------
	protected PropertyDescriptorCollection GetDescriptors(IVsDataConnectionProperties connectionProperties)
	{
		PropertyDescriptorCollection descriptors = null;

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


	#endregion Methods

}
