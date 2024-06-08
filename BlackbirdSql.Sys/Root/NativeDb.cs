// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Interfaces;

namespace BlackbirdSql;


// =========================================================================================================
//											DbNativeServices Class
//
/// <summary>
/// Central class for database specific class native services.
/// </summary>
// =========================================================================================================
public static class NativeDb
{
	private static IBsNativeDatabaseEngine _DatabaseEngineSvc = null;
	private static IBsNativeProviderSchemaFactory _ProviderSchemaFactorySvc = null;
	private static IBsNativeDatabaseInfo _DatabaseInfoSvc = null;
	private static IBsNativeDbCommand _DbCommandSvc = null;
	private static IBsNativeDbException _DbExceptionSvc = null;
	private static IBsNativeDbConnection _DbConnectionSvc = null;

	private static Type _CsbType = null;

	private static string[] _EquivalencyKeys = [];
	private static bool _OnDemandLinkage = false;
	private static int _LinkageTimeout = 120;




	public static Type CsbType
	{
		get { return _CsbType; }
		set { _CsbType = value; }
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Equivalency keys are comprised of the mandatory connection properties
	/// <see cref="FbConnectionStringBuilder.DataSource"/>, <see cref="FbConnectionStringBuilder.Port"/>,
	/// <see cref="FbConnectionStringBuilder.Database"/>, <see cref="FbConnectionStringBuilder.UserID"/>,
	/// <see cref="FbConnectionStringBuilder.ServerType"/>, <see cref="FbConnectionStringBuilder.Role"/>,
	/// <see cref="FbConnectionStringBuilder.Charset"/>, <see cref="FbConnectionStringBuilder.Dialect"/>
	/// and <see cref="FbConnectionStringBuilder.NoDatabaseTriggers"/>, and any additional optional
	/// properties defined in the BlackbirdSQL Server Tools user options.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string[] EquivalencyKeys
	{
		get { return _EquivalencyKeys; }
		set { _EquivalencyKeys = value; }
	}


	public static bool OnDemandLinkage
	{
		get { return _OnDemandLinkage; }
		set { _OnDemandLinkage = value; }
	}

	public static int LinkageTimeout
	{
		get { return _LinkageTimeout; }
		set { _LinkageTimeout = value; }
	}


	public static IBsNativeDatabaseEngine DatabaseEngineSvc
	{
		get { return _DatabaseEngineSvc; }
		set { _DatabaseEngineSvc = value; }
	}

	public static IBsNativeProviderSchemaFactory ProviderSchemaFactorySvc
	{
		get { return _ProviderSchemaFactorySvc; }
		set { _ProviderSchemaFactorySvc = value; }
	}

	public static IBsNativeDatabaseInfo DatabaseInfoSvc
	{
		get { return _DatabaseInfoSvc; }
		set { _DatabaseInfoSvc = value; }
	}

	public static IBsNativeDbCommand DbCommandSvc
	{
		get { return _DbCommandSvc; }
		set { _DbCommandSvc = value; }
	}

	public static IBsNativeDbConnection DbConnectionSvc
	{
		get { return _DbConnectionSvc; }
		set { _DbConnectionSvc = value; }
	}

	public static IBsNativeDbException DbExceptionSvc
	{
		get { return _DbExceptionSvc; }
		set { _DbExceptionSvc = value; }
	}



	public static string AssemblyQualifiedName => DatabaseEngineSvc.AssemblyQualifiedName_;
	public static string ClientVersion => DatabaseEngineSvc.ClientVersion_;
	public static Type ClientFactoryType => DatabaseEngineSvc.ClientFactoryType_;
	public static Type ConnectionType => DatabaseEngineSvc.ConnectionType_;
	public static DescriberDictionary Describers => DatabaseEngineSvc.Describers_;
	public static Type ExceptionType => DatabaseEngineSvc.ExceptionType_;
	public static string Invariant => DatabaseEngineSvc.Invariant_;
	public static string ProviderFactoryName => DatabaseEngineSvc.ProviderFactoryName_;
	public static string ProviderFactoryClassName => DatabaseEngineSvc.ProviderFactoryClassName_;
	public static string ProviderFactoryDescription => DatabaseEngineSvc.ProviderFactoryDescription_;

	public static string EFProvider => DatabaseEngineSvc.EFProvider_;
	public static string EFProviderServices => DatabaseEngineSvc.EFProviderServices_;
	public static string EFConnectionFactory => DatabaseEngineSvc.EFConnectionFactory_;


	public static string Extension => DatabaseEngineSvc.Extension_;
	public static string Protocol => DatabaseEngineSvc.Protocol_;
	public static string Scheme => DatabaseEngineSvc.Scheme_;
	public static string DbEngineName => DatabaseEngineSvc.DbEngineName_;
	public static string DataProviderName => DatabaseEngineSvc.DataProviderName_;
	public static string ExternalUtilityConfigurationPath => DatabaseEngineSvc.ExternalUtilityConfigurationPath_;
	public static string XmlActualPlanColumn => DatabaseEngineSvc.XmlActualPlanColumn_;
	public static string XmlEstimatedPlanColumn => DatabaseEngineSvc.XmlEstimatedPlanColumn_;


	public static string RootObjectTypeName => DatabaseEngineSvc.RootObjectTypeName_;


	public static DbConnection CastToAssemblyConnection(object connection)
	{
		return DatabaseEngineSvc.CastToAssemblyConnection_(connection);
	}


	public static string ConvertDataTypeToSql(string type, int length, int precision, int scale) =>
		DatabaseEngineSvc.ConvertDataTypeToSql_(type, length, precision, scale);


	public static string ConvertDataTypeToSql(object type, object length, object precision, object scale) =>
		DatabaseEngineSvc.ConvertDataTypeToSql_(type, length, precision, scale);


	public static DbCommand CreateDbCommand(string cmdText = null)
	{
		return DatabaseEngineSvc.CreateDbCommand_(cmdText);
	}


	/// <summary>
	/// Creates a native database connection using a connection string.
	/// </summary>
	public static IDbConnection CreateDbConnection(string connectionString)
	{
		return DatabaseEngineSvc.CreateDbConnection_(connectionString);
	}




	/// <summary>
	/// Creates a native database DbConnectionStringBuilder.
	/// </summary>
	public static DbConnectionStringBuilder CreateDbConnectionStringBuilder()
	{
		return DatabaseEngineSvc.CreateDbConnectionStringBuilder_();
	}



	/// <summary>
	/// Creates a native database DbConnectionStringBuilder using a connection string.
	/// </summary>
	public static DbConnectionStringBuilder CreateDbConnectionStringBuilder(string connectionString)
	{
		return DatabaseEngineSvc.CreateDbConnectionStringBuilder_(connectionString);
	}


	public static IBsNativeDbConnectionWrapper CreateDbConnectionWrapper(IDbConnection connection, Action<DbConnection> sqlConnectionCreatedObserver = null)
	{
		return DatabaseEngineSvc.CreateDbConnectionWrapper_(connection, sqlConnectionCreatedObserver);
	}




	public static IBsNativeDbStatementWrapper CreateDbStatementWrapper(IBsNativeDbBatchParser owner, object statement, int statementIndex)
	{
		return DatabaseEngineSvc.CreateDbStatementWrapper_(owner, statement, statementIndex);
	}

	public static IBsNativeDbBatchParser CreateDbBatchParser(EnSqlExecutionType executionType, IBQueryManager qryMgr, string script)
	{
		return DatabaseEngineSvc.CreateDbBatchParser_(executionType, qryMgr, script);
	}


	public static byte GetErrorClass(object error) => DatabaseEngineSvc.GetErrorClass_(error);
	public static int GetErrorLineNumber(object error) => DatabaseEngineSvc.GetErrorLineNumber_(error);
	public static string GetErrorMessage(object error) => DatabaseEngineSvc.GetErrorMessage_(error);
	public static int GetErrorNumber(object error) => DatabaseEngineSvc.GetErrorNumber_(error);
	public static int GetObjectTypeIdentifierLength(string typeName) => DatabaseEngineSvc.GetObjectTypeIdentifierLength_(typeName);
	public static IList<object> GetInfoMessageEventArgsErrors(DbInfoMessageEventArgs e) => DatabaseEngineSvc.GetInfoMessageEventArgsErrors_(e);
	public static ICollection<object> GetErrorEnumerator(IList<object> errors) => DatabaseEngineSvc.GetErrorEnumerator_(errors);


	public static bool IsSupportedCommandType(object command) => DatabaseEngineSvc.IsSupportedCommandType_(command);

	public static bool IsSupportedConnection(object connection) => DatabaseEngineSvc.IsSupportedConnection_(connection);

	public static bool TransactionCompleted(IDbTransaction @this) => DatabaseEngineSvc.TransactionCompleted_(@this);
}
