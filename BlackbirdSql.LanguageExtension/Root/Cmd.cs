// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Utils

using System;
using System.Collections.Generic;
using System.Data;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Parser;

using CollationInfo = Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo;



namespace BlackbirdSql.LanguageExtension;


// =========================================================================================================
//											Cmd Class
//
/// <summary>
/// Central location for accessing of utility static members. 
/// </summary>
// =========================================================================================================
internal abstract class Cmd : BlackbirdSql.Cmd
{


	// ---------------------------------------------------------------------------------
	#region Constants and Static Fields - Cmd
	// ---------------------------------------------------------------------------------


	private static readonly ParseOptions ParseOptionsQuotedIdentifierSet = new ParseOptions(isQuotedIdentifierSet: true);

	private static readonly ParseOptions ParseOptionsQuotedIdentifierNotSet = new ParseOptions(isQuotedIdentifierSet: false);

	#endregion Constants and Static Fields





	// =========================================================================================================
	#region Static Methods - Cmd
	// =========================================================================================================


	public static string EscapeSqlIdentifier(string value)
	{
		return "[" + value.Replace("]", "]]") + "]";
	}



	public static IDatabasePrincipal GetDatabasePrincipal(IDatabase database, string prinipalName)
	{
		IDatabasePrincipal databasePrincipal = database.Users[prinipalName];
		if (databasePrincipal != null)
		{
			return databasePrincipal;
		}
		databasePrincipal = database.Roles[prinipalName];
		if (databasePrincipal != null)
		{
			return databasePrincipal;
		}
		return database.ApplicationRoles[prinipalName];
	}



	public static ICollation GetCollation(string name)
	{
		return CollationInfo.GetCollationInfo(name);
	}



	public static CollationInfo GetCollationInfo(string name)
	{
		return CollationInfo.GetCollationInfo(name);
	}



	public static IDataType GetDataType(IDatabase database, DataType metadataType)
	{
		// IDataType dataType = null;
		switch (metadataType.SqlDataType)
		{
			case Microsoft.SqlServer.Management.Smo.SqlDataType.UserDefinedDataType:
				{
					string schema3 = metadataType.Schema;
					string name3 = metadataType.Name;
					return database.Schemas[schema3].UserDefinedDataTypes[name3];
				}
			case Microsoft.SqlServer.Management.Smo.SqlDataType.UserDefinedTableType:
				{
					string schema2 = metadataType.Schema;
					string name2 = metadataType.Name;
					return database.Schemas[schema2].UserDefinedTableTypes[name2];
				}
			case Microsoft.SqlServer.Management.Smo.SqlDataType.UserDefinedType:
				{
					string schema = metadataType.Schema;
					string name = metadataType.Name;
					return database.Schemas[schema].UserDefinedClrTypes[name];
				}
			default:
				return BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaSmoSystemDataTypeLookup.Instance.Find(metadataType);
		}
	}



	public static IExecutionContext GetExecutionContext(IDatabase database, Microsoft.SqlServer.Management.Smo.StoredProcedure smoStoredProc)
	{
		return SmoExecutionContextInfoI.Create(database, smoStoredProc).GetExecutionContext();
	}

	public static IExecutionContext GetExecutionContext(IDatabase database, Microsoft.SqlServer.Management.Smo.UserDefinedFunction smoFunction)
	{
		return SmoExecutionContextInfoI.Create(database, smoFunction).GetExecutionContext();
	}

	public static IExecutionContext GetExecutionContext(IDatabase database, Trigger smoDmlTrigger)
	{
		return SmoExecutionContextInfoI.Create(database, smoDmlTrigger).GetExecutionContext();
	}

	public static IExecutionContext GetExecutionContext(IDatabase database, Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger smoDatabaseDdlTrigger)
	{
		return SmoExecutionContextInfoI.Create(database, smoDatabaseDdlTrigger).GetExecutionContext();
	}

	public static IExecutionContext GetExecutionContext(IServer server, Microsoft.SqlServer.Management.Smo.ServerDdlTrigger smoServerDdlTrigger)
	{
		return SmoExecutionContextInfoI.Create(server, smoServerDdlTrigger).GetExecutionContext();
	}



	public static T GetPropertyObject<T>(SqlSmoObject smoObject, string propertyName) where T : class
	{
		return RetrievePropertyValue<T>(smoObject, propertyName);
	}



	public static T? GetPropertyValue<T>(SqlSmoObject smoObject, string propertyName) where T : struct
	{
		return RetrievePropertyValue<T?>(smoObject, propertyName);
	}



	public static T GetPropertyValue<T>(SqlSmoObject smoObject, string propertyName, T defaultValue) where T : struct
	{
		T? propertyValue = GetPropertyValue<T>(smoObject, propertyName);
		if (!propertyValue.HasValue)
		{
			return defaultValue;
		}
		return propertyValue.Value;
	}



	public static bool IsDesignMode(SqlSmoObject smoObject)
	{
		if (smoObject is ISfcSupportsDesignMode sfcSupportsDesignMode)
		{
			return sfcSupportsDesignMode.IsDesignMode;
		}
		return false;
	}



	public static bool IsRoleConvertableToSchema(Microsoft.SqlServer.Management.Smo.DatabaseRole role)
	{
		if (!role.IsFixedRole)
		{
			return !role.Name.Equals("public", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}



	public static bool IsShilohDatabase(Microsoft.SqlServer.Management.Smo.Database database)
	{
		Microsoft.SqlServer.Management.Smo.Server parent = database.Parent;

		if (parent != null)
			return parent.VersionMajor == 999999; // 8;

		return false;
	}



	public static bool IsUserConvertableToSchema(Microsoft.SqlServer.Management.Smo.User user)
	{
		bool num = user.Name.Equals("dbo", StringComparison.OrdinalIgnoreCase);
		bool flag = user.Name.Equals("guest", StringComparison.OrdinalIgnoreCase);
		bool isSystemObject = user.IsSystemObject;
		if (!num)
		{
			if (!isSystemObject)
			{
				return !flag;
			}
			return false;
		}
		return true;
	}



	public static string RetrieveFunctionBody(string sql, bool isQuotedIdentifierOn)
	{
		return RetrieveModuleBody(sql, isQuotedIdentifierOn, isTrigger: false);
	}



	public static string RetrieveModuleBody(string sql, bool isQuotedIdentifierOn, bool isTrigger)
	{
		ParseOptions parseOptions = new ParseOptions
		{
			IsQuotedIdentifierSet = isQuotedIdentifierOn,
			TransactSqlVersion = Microsoft.SqlServer.Management.SqlParser.Common.TransactSqlVersion.Current
		};
		return (string)(isTrigger ? ParseUtils.RetrieveTriggerDefinition(sql, parseOptions) : ParseUtils.RetrieveModuleDefinition(sql, parseOptions))[PropertyKeys.BodyDefinition];
	}



	private static T RetrievePropertyValue<T>(SqlSmoObject smoObject, string propertyName)
	{
		Property property = ((!IsDesignMode(smoObject)) ? smoObject.Properties.GetPropertyObject(propertyName) : smoObject.Properties.GetPropertyObject(propertyName, doNotLoadPropertyValues: false));
		return (T)property.Value;
	}



	public static string RetrieveStoredProcedureBody(string sql, bool isQuotedIdentifierOn)
	{
		return RetrieveModuleBody(sql, isQuotedIdentifierOn, isTrigger: false);
	}



	public static bool TryGetPropertyObject<T>(SqlSmoObject smoObject, string propertyName, out T value) where T : class
	{
		bool result = true;
		try
		{
			value = GetPropertyObject<T>(smoObject, propertyName);
		}
		catch (UnknownPropertyException)
		{
			value = null;
			result = false;
		}
		catch (PropertyCannotBeRetrievedException)
		{
			value = null;
			result = false;
		}
		catch (PropertyNotAvailableException)
		{
			value = null;
			result = false;
		}
		return result;
	}



	public static bool TryGetPropertyValue<T>(SqlSmoObject smoObject, string propertyName, out T? value) where T : struct
	{
		bool result = true;
		try
		{
			value = GetPropertyValue<T>(smoObject, propertyName);
		}
		catch (UnknownPropertyException)
		{
			value = null;
			result = false;
		}
		catch (PropertyCannotBeRetrievedException)
		{
			value = null;
			result = false;
		}
		catch (PropertyNotAvailableException)
		{
			value = null;
			result = false;
		}
		return result;
	}


	#endregion Static Methods





	// =========================================================================================================
	#region									Nested types - Cmd
	// =========================================================================================================


	public static class DdlTriggerI
	{
		public static TriggerEventTypeSet GetDatabaseTriggerEvents(Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger trigger)
		{
			new TriggerEventTypeSet();
			string sqlCommand = $"select type_desc from sys.trigger_events where object_id={trigger.ID}";
			return GetTriggerEvents(trigger.Parent.ExecuteWithResults(sqlCommand));
		}

		public static TriggerEventTypeSet GetServerTriggerEvents(Microsoft.SqlServer.Management.Smo.ServerDdlTrigger trigger)
		{
			Microsoft.SqlServer.Management.Smo.Database database = trigger.Parent.Databases["master"];
			new TriggerEventTypeSet();
			string sqlCommand = $"select type_desc from sys.server_trigger_events where object_id={trigger.ID}";
			return GetTriggerEvents(database.ExecuteWithResults(sqlCommand));
		}

		private static TriggerEventTypeSet GetTriggerEvents(DataSet dataSet)
		{
			DataTable dataTable = dataSet.Tables[0];
			TriggerEventTypeSet triggerEventTypeSet = [];
			foreach (DataRow row in dataTable.Rows)
			{
				string item = (string)row[0];
				triggerEventTypeSet.Add(item);
			}
			return triggerEventTypeSet;
		}
	}



	public static class ModuleI
	{
		public static string GetDefinitionTest(NamedSmoObject module)
		{
			TryGetPropertyObject<string>(module, "Text", out var value);
			if (value == null)
			{
				TryGetPropertyObject<string>(module, "TextHeader", out var value2);
				TryGetPropertyObject<string>(module, "TextBody", out var value3);
				if (value2 != null && value3 != null)
				{
					return value2 + " " + value3;
				}
			}
			return value;
		}
	}



	public abstract class OrderedCollectionHelperI<T, S> : BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.AbstractSmoMetaDatabaseObjectBase.OrderedCollectionHelperBaseI<T, S> where T : class, IMetadataObject where S : NamedSmoObject
	{
		public OrderedCollectionHelperI(BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase database)
		{
			_Database = database;
		}


		protected readonly BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase _Database;

		protected override BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaServer Server => _Database.Server;


		protected override Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo()
		{
			return _Database.CollationInfo;
		}
	}



	public class StatisticsCollectionHelperI : OrderedCollectionHelperI<IStatistics, Statistic>
	{
		public StatisticsCollectionHelperI(BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase database, IDatabaseTable dbTable, StatisticCollection smoCollection)
			: base(database)
		{
			this.dbTable = dbTable;
			this.smoCollection = smoCollection;
		}


		private readonly StatisticCollection smoCollection;

		private readonly IDatabaseTable dbTable;


		protected override BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.AbstractSmoMetaDatabaseObjectBase.IMetadataListI<Statistic> RetrieveSmoMetadataList()
		{
			return new BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI<Statistic>(_Database.Server, smoCollection);
		}

		protected override IStatistics CreateMetadataObject(Statistic smoObject)
		{
			return new BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaStatistics(_Database, dbTable, smoObject);
		}
	}



	public class StatisticsColumnCollectionHelperI : BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.AbstractSmoMetaDatabaseObjectBase.CollectionHelperBaseI<IColumn, IMetadataOrderedCollection<IColumn>>
	{
		public StatisticsColumnCollectionHelperI(BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase database, IDatabaseTable dbTable, StatisticColumnCollection smoCollection)
		{
			this._Database = database;
			this._DbTable = dbTable;
			this._SmoCollection = smoCollection;
		}


		private readonly BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase _Database;

		private readonly StatisticColumnCollection _SmoCollection;

		private readonly IDatabaseTable _DbTable;

		protected override BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaServer Server => _Database.Server;


		protected override IMetadataOrderedCollection<IColumn> CreateMetadataCollection()
		{
			Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo = _DbTable.CollationInfo;
			BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI<StatisticColumn> smoCollectionMetadataList = new BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI<StatisticColumn>(Server, _SmoCollection);
			IColumn[] array = new IColumn[((BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.AbstractSmoMetaDatabaseObjectBase.IMetadataListI<StatisticColumn>)smoCollectionMetadataList).Count];
			Collection<IColumn>.CreateOrderedCollection(collationInfo);
			int num = 0;
			foreach (StatisticColumn item in (IEnumerable<StatisticColumn>)smoCollectionMetadataList)
			{
				array[num++] = _DbTable.Columns[item.Name];
			}
			return Collection<IColumn>.CreateOrderedCollection(collationInfo, array);
		}

		protected override IMetadataOrderedCollection<IColumn> GetEmptyCollection()
		{
			return Collection<IColumn>.EmptyOrdered;
		}
	}



	private abstract class SmoExecutionContextInfoI
	{

		protected abstract ExecutionContextType ContextType { get; }

		protected virtual IServer Server => null;

		protected virtual IDatabase Database => null;

		protected virtual string UserName => null;

		protected virtual string LoginName => null;

		public IExecutionContext GetExecutionContext()
		{
			IExecutionContextFactory executionContext = BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaSmoMetadataFactory.Instance.ExecutionContext;
			switch (ContextType)
			{
				case ExecutionContextType.Caller:
					return executionContext.CreateExecuteAsCaller();
				case ExecutionContextType.Owner:
					return executionContext.CreateExecuteAsOwner();
				case ExecutionContextType.Self:
					return executionContext.CreateExecuteAsSelf();
				case ExecutionContextType.ExecuteAsUser:
					{
						IDatabase database = Database;
						string userName = UserName;
						IUser user = database.Users[userName];
						return executionContext.CreateExecuteAsUser(user);
					}
				case ExecutionContextType.ExecuteAsLogin:
					{
						IServer server = Server;
						string loginName = LoginName;
						ILogin login = server.Logins[loginName];
						return executionContext.CreateExecuteAsLogin(login);
					}
				default:
					return null;
			}
		}

		public static SmoExecutionContextInfoI Create(IDatabase database, Microsoft.SqlServer.Management.Smo.StoredProcedure storedProcedure)
		{
			return new StoredProcedureContextInfoII(database, storedProcedure);
		}

		public static SmoExecutionContextInfoI Create(IDatabase database, Microsoft.SqlServer.Management.Smo.UserDefinedFunction userDefinedFunction)
		{
			return new UserDefinedFunctionContextInfoII(database, userDefinedFunction);
		}

		public static SmoExecutionContextInfoI Create(IDatabase database, Trigger dmlTrigger)
		{
			return new DmlTriggerContextInfoII(database, dmlTrigger);
		}

		public static SmoExecutionContextInfoI Create(IDatabase database, Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger databaseDdlTrigger)
		{
			return new DatabaseDdlTriggerContextInfoII(database, databaseDdlTrigger);
		}

		public static SmoExecutionContextInfoI Create(IServer server, Microsoft.SqlServer.Management.Smo.ServerDdlTrigger serverDdlTrigger)
		{
			return new ServerDdlTriggerContextInfoII(server, serverDdlTrigger);
		}

		protected static ExecutionContextType GetContextType(ExecutionContext executionContext)
		{
			return executionContext switch
			{
				ExecutionContext.Caller => ExecutionContextType.Caller,
				ExecutionContext.ExecuteAsUser => ExecutionContextType.ExecuteAsUser,
				ExecutionContext.Owner => ExecutionContextType.Owner,
				ExecutionContext.Self => ExecutionContextType.Self,
				_ => ExecutionContextType.Caller,
			};
		}

		protected static ExecutionContextType GetContextType(DatabaseDdlTriggerExecutionContext executionContext)
		{
			return executionContext switch
			{
				DatabaseDdlTriggerExecutionContext.Caller => ExecutionContextType.Caller,
				DatabaseDdlTriggerExecutionContext.ExecuteAsUser => ExecutionContextType.ExecuteAsUser,
				DatabaseDdlTriggerExecutionContext.Self => ExecutionContextType.Self,
				_ => ExecutionContextType.Caller,
			};
		}

		protected static ExecutionContextType GetContextType(ServerDdlTriggerExecutionContext executionContext)
		{
			return executionContext switch
			{
				ServerDdlTriggerExecutionContext.Caller => ExecutionContextType.Caller,
				ServerDdlTriggerExecutionContext.ExecuteAsLogin => ExecutionContextType.ExecuteAsLogin,
				ServerDdlTriggerExecutionContext.Self => ExecutionContextType.Self,
				_ => ExecutionContextType.Caller,
			};
		}



		private class StoredProcedureContextInfoII : SmoExecutionContextInfoI
		{
			private readonly IDatabase database;

			private readonly Microsoft.SqlServer.Management.Smo.StoredProcedure storedProcedure;

			protected override ExecutionContextType ContextType
			{
				get
				{
					TryGetPropertyValue((SqlSmoObject)storedProcedure, "ExecutionContext", out ExecutionContext? value);
					if (value.HasValue && value != (ExecutionContext)0)
					{
						return GetContextType(value.Value);
					}
					return ExecutionContextType.Caller;
				}
			}

			protected override IDatabase Database => database;

			protected override string UserName => storedProcedure.ExecutionContextPrincipal;

			public StoredProcedureContextInfoII(IDatabase database, Microsoft.SqlServer.Management.Smo.StoredProcedure storedProcedure)
			{
				this.database = database;
				this.storedProcedure = storedProcedure;
			}
		}

		private class UserDefinedFunctionContextInfoII : SmoExecutionContextInfoI
		{
			private readonly IDatabase database;

			private readonly Microsoft.SqlServer.Management.Smo.UserDefinedFunction userDefinedFunction;

			protected override ExecutionContextType ContextType
			{
				get
				{
					TryGetPropertyValue((SqlSmoObject)userDefinedFunction, "ExecutionContext", out ExecutionContext? value);
					if (value.HasValue && value != (ExecutionContext)0)
					{
						return GetContextType(value.Value);
					}
					return ExecutionContextType.Caller;
				}
			}

			protected override IDatabase Database => database;

			protected override string UserName => userDefinedFunction.ExecutionContextPrincipal;

			public UserDefinedFunctionContextInfoII(IDatabase database, Microsoft.SqlServer.Management.Smo.UserDefinedFunction userDefinedFunction)
			{
				this.database = database;
				this.userDefinedFunction = userDefinedFunction;
			}
		}

		private class DmlTriggerContextInfoII : SmoExecutionContextInfoI
		{
			private readonly IDatabase database;

			private readonly Trigger dmlTrigger;

			protected override ExecutionContextType ContextType
			{
				get
				{
					TryGetPropertyValue((SqlSmoObject)dmlTrigger, "ExecutionContext", out ExecutionContext? value);
					if (value.HasValue && value != (ExecutionContext)0)
					{
						return GetContextType(value.Value);
					}
					return ExecutionContextType.Caller;
				}
			}

			protected override IDatabase Database => database;

			protected override string UserName => dmlTrigger.ExecutionContextPrincipal;

			public DmlTriggerContextInfoII(IDatabase database, Trigger dmlTrigger)
			{
				this.database = database;
				this.dmlTrigger = dmlTrigger;
			}
		}

		private class DatabaseDdlTriggerContextInfoII : SmoExecutionContextInfoI
		{
			private readonly IDatabase database;

			private readonly Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger databaseDdlTrigger;

			protected override ExecutionContextType ContextType => GetContextType(databaseDdlTrigger.ExecutionContext);

			protected override IDatabase Database => database;

			protected override string UserName => databaseDdlTrigger.ExecutionContextUser;

			public DatabaseDdlTriggerContextInfoII(IDatabase database, Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger databaseDdlTrigger)
			{
				this.database = database;
				this.databaseDdlTrigger = databaseDdlTrigger;
			}
		}

		private class ServerDdlTriggerContextInfoII : SmoExecutionContextInfoI
		{
			private readonly IServer server;

			private readonly Microsoft.SqlServer.Management.Smo.ServerDdlTrigger serverDdlTrigger;

			protected override ExecutionContextType ContextType => GetContextType(serverDdlTrigger.ExecutionContext);

			protected override IServer Server => server;

			protected override string LoginName => serverDdlTrigger.ExecutionContextLogin;

			public ServerDdlTriggerContextInfoII(IServer server, Microsoft.SqlServer.Management.Smo.ServerDdlTrigger serverDdlTrigger)
			{
				this.server = server;
				this.serverDdlTrigger = serverDdlTrigger;
			}
		}
	}



	public static class StoredProcedureI
	{
		public static IMetadataOrderedCollection<IParameter> CreateParameterCollection(BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase database, StoredProcedureParameterCollection metadataCollection)
		{
			_ = database.Parent;
			IParameterFactory parameter = BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaSmoMetadataFactory.Instance.Parameter;
			database.Parent.TryRefreshSmoCollection(metadataCollection, SmoConfig.SmoInitFields.GetInitFields(typeof(StoredProcedureParameter)));
			ParameterCollection parameterCollection = new ParameterCollection(metadataCollection.Count, database.CollationInfo);
			foreach (StoredProcedureParameter item in metadataCollection)
			{
				IParameter parameter2;
				TryGetPropertyValue((SqlSmoObject)item, "IsCursorParameter", out bool? value);
				if (value.GetValueOrDefault())
				{
					parameter2 = parameter.CreateCursorParameter(item.Name);
				}
				else
				{
					try
					{
						IDataType dataType = GetDataType(database, item.DataType);
						if (dataType is IScalarDataType dataType2)
						{
							TryGetPropertyObject<string>(item, "DefaultValue", out var value2);
							parameter2 = parameter.CreateScalarParameter(item.Name, dataType2, item.IsOutputParameter, string.IsNullOrEmpty(value2) ? null : value2);
						}
						else
						{
							ITableDataType dataType3 = dataType as ITableDataType;
							parameter2 = parameter.CreateTableParameter(item.Name, dataType3);
						}
					}
					catch (SmoException)
					{
						IScalarDataType unknownScalar = BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaSmoMetadataFactory.Instance.DataType.UnknownScalar;
						parameter2 = parameter.CreateScalarParameter(item.Name, unknownScalar, item.IsOutputParameter, item.DefaultValue);
					}
				}
				parameterCollection.Add(parameter2);
			}
			return parameterCollection;
		}

		public static IMetadataOrderedCollection<IParameter> CreateParameterCollection(BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase database, Microsoft.SqlServer.Management.Smo.StoredProcedure metadataStoredProc)
		{
			IMetadataOrderedCollection<IParameter> metadataOrderedCollection = null;
			try
			{
				if (metadataStoredProc.IsSystemObject && !metadataStoredProc.IsEncrypted)
				{
					metadataOrderedCollection = MetadataProviderUtils.GetStoredProcParameters(metadataStoredProc.TextHeader, parseOptions: metadataStoredProc.QuotedIdentifierStatus ? ParseOptionsQuotedIdentifierSet : ParseOptionsQuotedIdentifierNotSet, metadataFactory: BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaSmoMetadataFactory.Instance, dataTypeLookup: BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaSmoSystemDataTypeLookup.Instance, collationInfo: database.CollationInfo);
				}
			}
			catch (PropertyCannotBeRetrievedException)
			{
				metadataOrderedCollection = null;
			}
			catch (Microsoft.SqlServer.Management.SqlParser.SqlParserException)
			{
				metadataOrderedCollection = null;
			}
			metadataOrderedCollection ??= CreateParameterCollection(database, metadataStoredProc.Parameters);

			return metadataOrderedCollection;
		}
	}



	public abstract class UnorderedCollectionHelperI<T, S> : BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.AbstractSmoMetaDatabaseObjectBase.UnorderedCollectionHelperBaseI<T, S> where T : class, IMetadataObject where S : NamedSmoObject
	{
		public UnorderedCollectionHelperI(BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase database)
		{
			_Database = database;
		}


		protected readonly BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase _Database;

		protected override BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaServer Server => _Database.Server;


		protected override Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo()
		{
			return _Database.CollationInfo;
		}
	}



	public static class UserDefinedFunctionI
	{
		public static ParameterCollection CreateParameterCollection(Model.SmoMetadataProvider.SmoMetaDatabase database, ParameterCollectionBase metadataCollection, IDictionary<string, object> moduleInfo)
		{
			_ = database.Parent;
			IParameterFactory parameter = BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaSmoMetadataFactory.Instance.Parameter;
			database.Parent.TryRefreshSmoCollection(metadataCollection, SmoConfig.SmoInitFields.GetInitFields(typeof(UserDefinedFunctionParameter)));
			ParameterCollection parameterCollection = new ParameterCollection(metadataCollection.Count, database.CollationInfo);
			IList<IDictionary<string, object>> parametersInfo = ((moduleInfo != null) ? ((IList<IDictionary<string, object>>)moduleInfo[PropertyKeys.Parameters]) : null);
			foreach (ParameterBase item in metadataCollection)
			{
				IParameter parameter2;
				IDataType dataType;
				try
				{
					dataType = GetDataType(database, item.DataType);
				}
				catch (SmoException)
				{
					dataType = BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaSmoMetadataFactory.Instance.DataType.UnknownScalar;
				}
				if (dataType is IScalarDataType dataType2)
				{
					IDictionary<string, object> parameterInfo = GetParameterInfo(parametersInfo, item.Name);
					string text = ((parameterInfo != null) ? ((string)parameterInfo[PropertyKeys.DefaultValue]) : null);
					parameter2 = ((!string.IsNullOrEmpty(text)) ? parameter.CreateScalarParameter(item.Name, dataType2, isOutput: false, text) : parameter.CreateScalarParameter(item.Name, dataType2));
				}
				else
				{
					ITableDataType dataType3 = dataType as ITableDataType;
					parameter2 = parameter.CreateTableParameter(item.Name, dataType3);
				}
				parameterCollection.Add(parameter2);
			}
			return parameterCollection;
		}

		private static IDictionary<string, object> GetParameterInfo(IList<IDictionary<string, object>> parametersInfo, string parameterName)
		{
			if (parametersInfo != null)
			{
				foreach (IDictionary<string, object> item in parametersInfo)
				{
					if ((string)item[PropertyKeys.Name] == parameterName)
					{
						return item;
					}
				}
			}
			return null;
		}
	}

	#endregion Nested types

}
