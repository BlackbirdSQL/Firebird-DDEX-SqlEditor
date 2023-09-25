// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;


using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Model;
using BlackbirdSql.VisualStudio.Ddex.Model;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;

using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;





namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										AbstractSourceInformation Class
//
/// <summary>
/// Replacement for <see cref="Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetSourceInformation"/>
/// </summary>
// =========================================================================================================
public abstract class AbstractSourceInformation : DataSourceInformation, IVsDataSourceInformation
{
	private DataTable _SourceInformation;

	private static PropertyDescriptorCollection _Descriptors = null;

	private readonly object _LocalLock = new object();

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

			if (SourceInformation != null && SourceInformation.Columns.Contains(propertyName))
			{
				DataColumn col = SourceInformation.Columns[propertyName];
				value = SourceInformation.Rows[0][col.Ordinal];


				if (value == null || value == DBNull.Value)
					value = RetrieveValue(propertyName);
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
	/// Gets the property descriptor collection for the current connection string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected static PropertyDescriptorCollection Descriptors
	{
		get
		{
			return _Descriptors ??= TypeDescriptor.GetProperties(typeof(FbConnectionStringBuilder));
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates or returns the data source information table.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal DataTable SourceInformation
	{
		get
		{
			// Tracer.Trace(GetType(), "AbstractSourceInformation.get_SourceInformation");

			lock (_LocalLock)
			{
				if (_SourceInformation == null && Connection != null)
				{
					LinkageParser parser = LinkageParser.GetInstance(Site);
					if (parser != null)
						Tracer.Trace(GetType(), "get_SourceInformation pausing");

					int syncCardinal = parser != null ? parser.SyncEnter() : 0;

					try
					{

						Site.EnsureConnected();

						_SourceInformation = CreateSourceInformationSchema();

						_SourceInformation ??= new DataTable
						{
							Locale = CultureInfo.InvariantCulture
						};

					}
					catch (Exception ex)
					{
						Diag.Dug(ex);
						throw ex;
					}
					finally
					{
						parser?.SyncExit(syncCardinal);
					}
				}


				return _SourceInformation;
			}
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Return the undelying db connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected DbConnection Connection
	{
		get
		{
			if (Site != null)
			{
				if (Site.GetService(typeof(IVsDataConnectionSupport)) is IVsDataConnectionSupport vsDataConnectionSupport)
					return vsDataConnectionSupport.ProviderObject as DbConnection;
			}

			return null;
		}
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - AbstractSourceInformation
	// =========================================================================================================


	public AbstractSourceInformation() : this(default)
	{
		Tracer.Trace(GetType(), "AbstractSourceInformation.AbstractSourceInformation");
	}

	public AbstractSourceInformation(IVsDataConnection connection) : base(connection)
	{
		Tracer.Trace(GetType(), "AbstractSourceInformation.AbstractSourceInformation(IVsDataConnection)");
		AddStandardProperties();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - AbstractSourceInformation
	// =========================================================================================================





	private void AddStandardProperties()
	{
		foreach (Describer descriptor in ModelPropertySet.SourceInformationDescribers)
		{
			object value = descriptor.DefaultValue;

			if (value != null && descriptor.PropertyType == typeof(int) && (int)value == int.MinValue)
			{
				value = null;
			}

			if (value == null)
				AddProperty(descriptor.Name);
			else
				AddProperty(descriptor.Name, value);
		}


		AddProperty(CommandPrepareSupport, 1.ToString(CultureInfo.InvariantCulture));
		AddProperty(CommandDeriveParametersSupport, 4.ToString(CultureInfo.InvariantCulture));
		AddProperty(CommandDeriveSchemaSupport, 1.ToString(CultureInfo.InvariantCulture));
		AddProperty(CommandExecuteSupport, 1.ToString(CultureInfo.InvariantCulture));
		AddProperty(CommandParameterSupport, 7);
		AddProperty(IdentifierPartsCaseSensitive);
		AddProperty(ReservedWords);
		AddProperty(SupportsNestedTransactions, false);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a Boolean value indicating whether the specified property is contained
	/// in the data source information instance.
	/// </summary>
	// ---------------------------------------------------------------------------------
	bool IVsDataSourceInformation.Contains(string propertyName)
	{
		if (Contains(propertyName)
			|| (SourceInformation != null && SourceInformation.Columns.Contains(propertyName)))
		{
			return true;
		}

		return false;
	}


	protected DataTable CreateSourceInformationSchema()
	{
		Tracer.Trace(GetType(), "AbstractSourceInformation.CreateSourceInformationSchema");

		object value;
		DataTable schema;


		schema = DslProviderSchemaFactory.GetSchema((FbConnection)Connection, "DataSourceInformation", null);

		Describer describer;
		// Rename each column in the schema to it's AdoDotNet root column name
		foreach (DataColumn col in schema.Columns)
		{
			describer = ModelPropertySet.GetSchemaColumnSourceInformationDescriber(col.ColumnName);

			if (describer == null)
				continue;

			if (describer.Name != col.ColumnName)
				col.ColumnName = describer.Name;
		}


		schema.AcceptChanges();


		// Add in the root types
		foreach (Describer siDescriber in ModelPropertySet.SourceInformationDescribers)
		{
			if (!schema.Columns.Contains(siDescriber.Name))
				schema.Columns.Add(siDescriber.Name, siDescriber.PropertyType);
		}

		schema.AcceptChanges();


		schema.BeginLoadData();


		DataRow row = schema.Rows[0];
		PropertyDescriptor descriptor;

		/* Descriptor dump
			ParallelWorkers[], IsolationLevel[], Password[], ApplicationName[], FetchSize[], DbCachePages[], Charset[UTF8], ReturnRecordsAffected[], Values[], IsFixedSize[], Role[], initial catalog[C:\Server\Data\smartitplus_databases\MMEINT_SI_DB.FDB], NoGarbageCollect[], Dialect[], DataSource[], MaxPoolSize[], BrowsableConnectionString[], WireCrypt[], port number[55504], ConnectionTimeout[], Port[], ConnectionLifeTime[], Compression[], MinPoolSize[], Keys[], Pooling[], UserID[sysdba], CryptKey[], ClientLibrary[], PacketSize[], CommandTimeout[], Database[], NoDatabaseTriggers[], Count[], Enlist[], ServerType[], IsReadOnly[], data source[MMEI-LT01], ConnectionString[data source=MMEI-LT01;port number=55504;initial catalog=C:\Server\Data\smartitplus_databases\MMEINT_SI_DB.FDB;character set=UTF8;user id=sysdba]
		*/

		// Update the row values for each descriptor
		IVsDataConnectionProperties connectionProperties = GetConnectionProperties();

		foreach (DataColumn col in schema.Columns)
		{
			descriptor = FindColumnPropertyDescriptor(col.ColumnName, false);

			if (descriptor == null)
				continue;

			value = descriptor.GetValueX(connectionProperties);

			if (value != null)
				row[col.ColumnName] = value;
		}



		schema.EndLoadData();
		schema.AcceptChanges();



		/*
		LinkageParser parser = LinkageParser.Instance(Site);

		if (parser.ClearToLoadAsync)
			parser.AsyncExecute(50, 20);
		*/

		return schema;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the connection string descriptor for a given source information column,
	/// else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PropertyDescriptor FindColumnPropertyDescriptor(string name, bool throwIfNotFound = true)
	{
		// Get the SI property set descriptor for the column
		Describer describer = ModelPropertySet.GetSourceInformationDescriber(name);

		if (describer == null)
		{
			if (throwIfNotFound)
			{
				ArgumentException ex = new($"Source information describer not found for Source Information column '{name}'.");
				Diag.Dug(ex);
				throw ex;
			}

			return null;
		}


		// The SI descriptor parameter field holds the name of the property set descriptor
		if (describer.Parameter == null)
		{
			if (throwIfNotFound)
			{
				ArgumentException ex = new($"Source information describer '{name}' has no property descriptor name.");
				Diag.Dug(ex);
				throw ex;
			}

			return null;
		}


		PropertyDescriptor descriptor = Descriptors.Find(describer.Parameter, true);

		if (throwIfNotFound && descriptor == null)
		{
			ArgumentException ex = new($"Property Descriptor '{describer.Parameter}' not found Source Information column '{name}'.");
			Diag.Dug(ex);
			throw ex;
		}

		return descriptor;
	}



	internal object GetAdoPropertyValue(string propertyName)
	{
		if (SourceInformation == null)
		{
			DataException ex = new("Source information table is null.");
			Diag.Dug(ex);
			return null;
		}


		if (!SourceInformation.Columns.Contains(propertyName))
		{
			ArgumentException ex = new($"ADO property name '{propertyName}' not found in Source Information table.");
			Diag.Dug(ex);
			throw ex;
		}

		return SourceInformation.Rows[0][propertyName];
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


		Hostess host = new(serviceProvider);

		connectionProperties = host.GetService<IVsDataProviderManager>().Providers[Site.Provider].TryCreateObject<IVsDataConnectionUIProperties>(Site.Source);
		connectionProperties ??= host.GetService<IVsDataProviderManager>().Providers[Site.Provider].TryCreateObject<IVsDataConnectionProperties>(Site.Source);

		
		connectionProperties.Parse(Connection.ConnectionString);
		// connectionProperties.Parse(Site.SafeConnectionString);

		return connectionProperties;
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
			ArgumentNullException ex = new(propertyName);
			Diag.Dug(ex);
			throw ex;
		}

		Type type = ModelPropertySet.GetSourceInformationType(propertyName) ?? base.GetType(propertyName);


		if (type == null)
		{
			ArgumentException ex = new(propertyName);
			Diag.Dug(ex);
			throw ex;
		}


		return type;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves a value for a specified data source information property.
	/// </summary>
	/// <param name="propertyName"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public object RetrieveSourceInformationValue(string propertyName)
	{
		return RetrieveValue(propertyName);
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
		object adoValue;
		object retval = null;


		try
		{
			switch (propertyName)
			{
				case DataSourceName:
					retval = Connection.DataSource;
					break;

				case DataSourceProduct:
					retval = GetAdoPropertyValue(propertyName);
					break;

				case DataSourceVersion:
					retval = "Firebird " + FbServerProperties.ParseServerVersion(Connection.ServerVersion).ToString();
					break;

				case DefaultCatalog:
					if (Connection.Database != null && Connection.Database.Length > 0)
						retval = Connection.Database;
					break;

				case ReservedWords:
					DataTable dataTable = null;
					if (Connection != null)
					{
						Site.EnsureConnected();
						try
						{
							dataTable = Connection.GetSchema(DbMetaDataCollectionNames.ReservedWords);
						}
						catch
						{
						}
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

				case SupportsAnsi92Sql:
					retval = false;
					adoValue = GetAdoPropertyValue(DbMetaDataColumnNames.SupportedJoinOperators);
					if (adoValue is SupportedJoinOperators supportedJoinOperators)
					{
						if ((supportedJoinOperators & SupportedJoinOperators.LeftOuter) > SupportedJoinOperators.None
							|| (supportedJoinOperators & SupportedJoinOperators.RightOuter) > SupportedJoinOperators.None
							|| (supportedJoinOperators & SupportedJoinOperators.FullOuter) > SupportedJoinOperators.None)
						{
							retval = true;
						}
					}
					break;

				case IdentifierPartsCaseSensitive:
					retval = false;
					adoValue = GetAdoPropertyValue(DbMetaDataColumnNames.IdentifierCase);
					if (adoValue is int)
					{
						IdentifierCase identifierCase = (IdentifierCase)adoValue;
						if (identifierCase == IdentifierCase.Sensitive)
						{
							retval = true;
						}

						_ = 1;
					}

					break;

				case QuotedIdentifierPartsCaseSensitive:
					retval = true;
					adoValue = GetAdoPropertyValue(DbMetaDataColumnNames.QuotedIdentifierCase);
					if (adoValue is int)
					{
						switch ((IdentifierCase)adoValue)
						{
							case IdentifierCase.Sensitive:
								retval = true;
								break;
							case IdentifierCase.Insensitive:
								retval = false;
								break;
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

		retval ??= base.RetrieveValue(propertyName);

		return retval;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - AbstractSourceInformation
	// =========================================================================================================





	protected override void OnSiteChanged(EventArgs e)
	{

		base.OnSiteChanged(e);

		if (Site == null && _SourceInformation != null)
		{
			lock (_LocalLock)
			{
				_SourceInformation.Dispose();
				_SourceInformation = null;
			}
		}

	}


	#endregion Event handlers

}
