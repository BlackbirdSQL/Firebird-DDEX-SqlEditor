// Microsoft.SqlServer.Smo, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.Smo.Server

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using BlackbirdSql.Core.Model;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using BlackbirdSql.LanguageExtension.Properties;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Agent;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.Win32;

using DatabaseCollection = Microsoft.SqlServer.Management.Smo.DatabaseCollection;



namespace BlackbirdSql.LanguageExtension.Model.Smo;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbSmoServer Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Server.
/// </summary>
// =========================================================================================================
public class LsbSmoServer
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbSmoServer
	// ---------------------------------------------------------------------------------


	//
	// Summary:
	//     Constructs a new Server object that relies on the given ServerConnection for
	//     connectivity.
	//
	// Parameters:
	//   serverConnection:
	//
	// Remarks:
	//     If serverConnection.ConnectAsUser is true, its NonPooledConnection property must
	//     also be true. Otherwise, Server may attempt to duplicate the ServerConnection
	//     without preserving the ConnectAsUser parameters, leading to either failed connections
	//     or connections as the incorrect user when using integrated security.
	public LsbSmoServer(IDbConnection serverConnection)
	{
		_Connection = serverConnection;
		Initialize();
	}



	private void Initialize()
	{
		/*
		if (serverConnection == null)
		{
			Microsoft.SqlServer.Management.Diagnostics.TraceHelper.Assert(m_ExecutionManager != null, "m_ExecutionManager == null");
			serverConnection = m_ExecutionManager.ConnectionContext;
		}
		*/

		// SetState(SqlSmoState.Existing);
		_State = SqlSmoState.Existing; _ = _State;
		_ObjectInSpace = false;
		// SetObjectKey(new SimpleObjectKey(Name));
		_Key = Csb.CreateConnectionUrl(_Connection.ConnectionString);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - LsbServer
	// =========================================================================================================


	#endregion Constants





	// =========================================================================================================
	#region Fields - LsbServer
	// =========================================================================================================


	private SortedList _CollationCache;
	private readonly IDbConnection _Connection;
	private DatabaseCollection _Databases;
	private string _Key;
	internal bool _ObjectInSpace;
	private SqlSmoState _State;
	//
	// Summary:
	//     Contains the list of initialization fields for each SqlSmoObject type The order
	//     of fields in the list must be maintained, as required fields are always first.
	private IDictionary<Type, IList<string>> _TypeInitFields = null;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbServer
	// =========================================================================================================


	public IDbConnection Connection => _Connection;
	public DatabaseCollection Databases => _Databases ??= Reflect.CreateInstance<DatabaseCollection>(this);

	public DatabaseEngineEdition EngineEdition
	{
		get
		{
			if (_Connection == null)
				return DatabaseEngineEdition.Unknown;

			Csb csb = new Csb(_Connection.ConnectionString, false);

			return csb.ServerType == Sys.Enums.EnServerType.Default ? DatabaseEngineEdition.SqlDatabase : DatabaseEngineEdition.Express;
		}
	}
	

	internal string Key => _Key;
	internal string Name => _Key;

	protected bool ObjectInSpace => _ObjectInSpace;

	public DatabaseEngineType ServerType => DatabaseEngineType.Standalone;

	private IDictionary<Type, IList<string>> TypeInitFields => _TypeInitFields ??= new Dictionary<Type, IList<string>>();


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbServer
	// =========================================================================================================


	private IEnumerable<string> CreateInitFieldsColl(Type typeObject)
	{
		if (typeObject.IsSubclassOf(typeof(ScriptSchemaObjectBase)))
		{
			yield return "Schema";
			yield return "Name";
			if (typeObject.IsSubclassOf(typeof(TableViewBase)))
			{
				yield return "ID";
			}
		}
		else if (typeObject == typeof(NumberedStoredProcedure))
		{
			yield return "Number";
		}
		else if (IsOrderedByID(typeObject))
		{
			yield return "ID";
			yield return "Name";
		}
		else if (typeObject.IsSubclassOf(typeof(MessageObjectBase)))
		{
			yield return "ID";
			yield return "Language";
		}
		else if (typeObject.IsSubclassOf(typeof(SoapMethodObject)))
		{
			yield return "Namespace";
			yield return "Name";
		}
		else if (typeObject.IsSubclassOf(typeof(ScheduleBase)))
		{
			yield return "Name";
			yield return "ID";
		}
		else if (typeObject == typeof(Job))
		{
			yield return "Name";
			yield return "CategoryID";
			yield return "JobID";
		}
		else if (typeObject == typeof(Database))
		{
			yield return "Name";
			yield return "IsFabricDatabase";
			if (EngineEdition == DatabaseEngineEdition.SqlDatabase)
			{
				yield return "RealEngineEdition";
			}
		}
		else if (typeObject.IsSubclassOf(typeof(NamedSmoObject)))
		{
			yield return "Name";
		}
		else if (typeObject == typeof(PhysicalPartition))
		{
			yield return "PartitionNumber";
		}
		else if (typeObject == typeof(DatabaseReplicaState))
		{
			yield return "AvailabilityReplicaServerName";
			yield return "AvailabilityDatabaseName";
		}
		else if (typeObject == typeof(AvailabilityGroupListenerIPAddress))
		{
			yield return "IPAddress";
			yield return "SubnetMask";
			yield return "SubnetIP";
		}
		else if (typeObject == typeof(SecurityPredicate))
		{
			yield return "SecurityPredicateID";
		}
		else if (typeObject == typeof(ColumnEncryptionKeyValue))
		{
			yield return "ColumnMasterKeyID";
		}

		if (typeObject == typeof(Column))
		{
			yield return "DefaultConstraintName";
		}

		if (typeObject == typeof(DefaultConstraint))
		{
			yield return "IsFileTableDefined";
		}

		if (typeObject == typeof(Information))
		{
			yield return "Edition";
		}

		if (typeObject == typeof(DataType))
		{
			yield return "DataType";
			yield return "SystemType";
			yield return "Length";
			yield return "NumericPrecision";
			yield return "NumericScale";
			yield return "XmlSchemaNamespace";
			yield return "XmlSchemaNamespaceSchema";
			yield return "XmlDocumentConstraint";
			yield return "DataTypeSchema";
		}
	}



	internal static bool IsOrderedByID(Type t)
	{
		if (!t.IsSubclassOf(typeof(ParameterBase)) && !t.Equals(typeof(Column)) && !t.Equals(typeof(ForeignKeyColumn)) && !t.Equals(typeof(OrderColumn)) && !t.Equals(typeof(IndexedColumn)) && !t.Equals(typeof(IndexedXmlPath)) && !t.Equals(typeof(StatisticColumn)) && !t.Equals(typeof(JobStep)) && !t.Equals(typeof(PartitionFunctionParameter)))
		{
			return t.Equals(typeof(PartitionSchemeParameter));
		}
		return true;
	}



	public void Refresh()
	{
		/*
		base.Refresh();
		Settings.Refresh();
		if (this.IsSupportedObject<UserOptions>())
		{
			UserOptions.Refresh();
		}

		Information.Refresh();
		Configuration.Refresh();
		if (affinityInfo != null)
		{
			AffinityInfo.Refresh();
		}

		if (this.IsSupportedObject<ResourceGovernor>() && !IsExpressSku())
		{
			ResourceGovernor.Refresh();
		}

		if (this.IsSupportedObject<SmartAdmin>() && !IsExpressSku())
		{
			SmartAdmin.Refresh();
		}
		*/

		_CollationCache = null; _ = _CollationCache;
	}



	//
	// Summary:
	//     Set the default for the fields of the given object type. This function will be
	//     deprecated. Please use the overload function.
	//
	// Parameters:
	//   typeObject:
	//     Type of the object
	//
	//   fields:
	//     List of the fields
	public void SetDefaultInitFields(Type typeObject, StringCollection fields)
	{
		SetDefaultInitFields(typeObject, fields, EngineEdition);
	}

	//
	// Summary:
	//     Set the default fields of the given object type
	//
	// Parameters:
	//   typeObject:
	//     Type of the object
	//
	//   fields:
	//     List of the fields
	//
	//   databaseEngineEdition:
	//     This value is ignored by the method. Field names not relevant for the active
	//     Database will be filtered from the query.
	public void SetDefaultInitFields(Type typeObject, StringCollection fields, DatabaseEngineEdition databaseEngineEdition)
	{
		if (null == typeObject)
		{
			throw new FailedOperationException(Resources.ExceptionSetDefaultInitFields, this, new ArgumentNullException("typeObject"));
		}

		if (fields == null)
		{
			throw new FailedOperationException(Resources.ExceptionSetDefaultInitFields, this, new ArgumentNullException("fields"));
		}

		if (!typeObject.IsSubclassOf(typeof(SqlSmoObject)) || !typeObject.IsSealed)
		{
			throw new FailedOperationException(Resources.ExceptionCannotSetDefInitFlds.Fmt(typeObject.Name)); //.SetHelpContext("CannotSetDefInitFlds");
		}

		List<string> initFields = CreateInitFieldsColl(typeObject).ToList();
		initFields.AddRange(from string f in fields
							where !initFields.Contains(f)
							select f);
		TypeInitFields[typeObject] = initFields;
	}

	//
	// Summary:
	//     Set the default fields of the given object type. This function will be deprecated.
	//     Please use the overload function.
	//
	// Parameters:
	//   typeObject:
	//     Type of the object
	//
	//   fields:
	//     List of the fields
	public void SetDefaultInitFields(Type typeObject, params string[] fields)
	{
		SetDefaultInitFields(typeObject, EngineEdition, fields);
	}

	//
	// Summary:
	//     Set the default fields of the given object type
	//
	// Parameters:
	//   typeObject:
	//     Type of the object
	//
	//   fields:
	//     List of the fields
	//
	//   databaseEngineEdition:
	//     Database edition of the object. This is required a Database and it's children can
	//     have different properties based on the databaseEngineEdition
	public void SetDefaultInitFields(Type typeObject, DatabaseEngineEdition databaseEngineEdition, params string[] fields)
	{
		if (fields == null)
		{
			throw new FailedOperationException(Resources.ExceptionSetDefaultInitFields, this, new ArgumentNullException("fields"));
		}

		StringCollection stringCollection = [.. fields];
		SetDefaultInitFields(typeObject, stringCollection, databaseEngineEdition);
	}



	#endregion Methods
}
