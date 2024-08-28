
using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Data.Model;
using BlackbirdSql.Data.Model.Schema;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using EntityFramework.Firebird;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.Data;


// =========================================================================================================
//											Firebird DatabaseEngineService Class
//
/// <summary>
/// Central class for database specific class methods. The intention is ultimately provide these as a
/// service so that BlackbirdSql can provide support for additional database engines.
/// </summary>
// =========================================================================================================
public class DatabaseEngineService : SBsNativeDatabaseEngine, IBsNativeDatabaseEngine
{
	private DatabaseEngineService()
	{
	}



	private static IBsNativeDatabaseEngine _Instance = null;
	public static IBsNativeDatabaseEngine EnsureInstance() => _Instance ??= new DatabaseEngineService();

	public string AssemblyQualifiedName_ => typeof(FirebirdClientFactory).AssemblyQualifiedName;
	public Assembly ClientFactoryAssembly_ => typeof(FirebirdClientFactory).Assembly;
	public string ClientVersion_ => $"FirebirdSql {typeof(FbConnection).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version}";
	public Type ClientFactoryType_ => typeof(FirebirdClientFactory);
	public Type ConnectionType_ => typeof(FbConnection);
	public string DataProviderName_ => LibraryData.C_DataProviderName;
	public string DbEngineName_ => LibraryData.C_DbEngineName;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Describer collection of uniform properties used by the FbConnectionStringBuilder
	/// replacement, Csb.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public DescriberDictionary Describers_ => new(LibraryData.Describers, LibraryData.DescriberSynonyms);


	public string EFConnectionFactory_ => LibraryData.C_EFConnectionFactory;
	public string EFProvider_ => LibraryData.C_EFProvider;
	public string EFProviderServices_ => LibraryData.C_EFProviderServices;
	public Type EFProviderServicesType_ => typeof(FbProviderServices);
	public string EFProviderServicesTypeFullName_ => LibraryData.C_EFProviderServicesTypeFullName;
	public Assembly EntityFrameworkAssembly_ => typeof(FbProviderServices).Assembly;
	public string[] EntityFrameworkVersions_ => LibraryData.S_EntityFrameworkVersions;
	public string Extension_ => LibraryData.C_Extension;


	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>
	public string ExternalUtilityConfigurationPath_ => LibraryData.S_ExternalUtilityConfigurationPath;


	public string Invariant_ => LibraryData.C_Invariant;
	public string Protocol_ => LibraryData.C_Protocol;
	public string ProviderFactoryName_ => LibraryData.C_ProviderFactoryName;
	public string ProviderFactoryClassName_ => LibraryData.C_ProviderFactoryClassName;
	public string ProviderFactoryDescription_ => LibraryData.C_ProviderFactoryDescription;
	public string RootObjectTypeName_ => DslObjectTypes.Root;
	public string Scheme_ => LibraryData.S_Scheme;
	public string SqlLanguageName_ => LibraryData.C_SqlLanguageName;
	public string XmlActualPlanColumn_ => LibraryData.C_XmlActualPlanColumn;
	public string XmlEstimatedPlanColumn_ => LibraryData.C_XmlEstimatedPlanColumn;





	/// <summary>
	/// Adds a parameter suffixed with an index to DbCommand.Parameters.
	/// </summary>
	/// <returns>The index of the parameter in DbCommand.Parameters else -1.</returns>
	public int AddCommandParameter_(DbCommand @this, string name, int index, object value)
	{
		return ((FbCommand)@this).AddParameter(name, index, value);
	}



	public void AsyncEnsureLinkageLoading_(IVsDataExplorerConnection root, int delay = 0, int multiplier = 1)
	{
		LinkageParser.AsyncEnsureLoading(root, delay, multiplier);
	}



	public DbConnection CastToNativeConnection_(object connection)
	{
		return connection as FbConnection;
	}



	public object CreateDatabaseInfoObject_(DbConnection @this)
	{
		FbDatabaseInfo info = new((FbConnection)@this);

		return info;
	}



	public IBsNativeDbBatchParser CreateDbBatchParser_(EnSqlExecutionType executionType, IBsQueryManager qryMgr, string script)
	{
		return new DbBatchParser(executionType, qryMgr, script);
	}



	public DbCommand CreateDbCommand_(string cmdText = null)
	{
		return new FbCommand(cmdText);
	}



	/// <summary>
	/// Creates a Firebird connection using a connection string.
	/// </summary>
	public IDbConnection CreateDbConnection_(string connectionString)
	{
		return new FbConnection(connectionString);
	}



	public IBsNativeDbStatementWrapper CreateDbStatementWrapper_(IBsNativeDbBatchParser owner, object statement, int index)
	{
		return new DbStatementWrapper(owner, statement, index);
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disposes of a parser given an IVsDataConnection site.
	/// </summary>
	/// <param name="site">
	/// The IVsDataConnection explorer connection object
	/// </param>
	/// <param name="disposing">
	/// If disposing is set to true, then all parsers with weak equivalency will
	/// be tagged as intransient, meaning their trigger linkage databases cannot
	/// be copied to another parser with weak equivalency. 
	/// </param>
	/// <returns>True of the parser was found and disposed else false.</returns>
	// -------------------------------------------------------------------------
	public bool DisposeLinkageParserInstance_(IVsDataExplorerConnection root, bool disposing)
	{
		return LinkageParser.DisposeInstance(root, disposing);
	}



	/// <summary>
	/// Gets the connection datasource.
	/// </summary>
	public string GetConnectionDataSource_(IDbConnection @this)
	{
		if (@this is not FbConnection connection)
			return "";

		return connection.DataSource;
	}



	/// <summary>
	/// Parses and converts a server version string to it's Version format.
	/// </summary>
	public string GetConnectionDataSourceVersion_(IDbConnection @this)
	{
		if (@this is not FbConnection connection)
			return "";

		if (connection.State != ConnectionState.Open)
			return "";

		return "Firebird " + FbServerProperties.ParseServerVersion(connection.ServerVersion);
	}



	public int GetConnectionPacketSize_(DbConnection @this)
	{
		return ((FbConnection)@this).PacketSize;
	}



	public IBsNativeDbLinkageParser GetLinkageParserInstance_(IVsDataExplorerConnection root) => LinkageParser.GetInstance(root);



	public bool HasTransactions_(IDbTransaction @this)
	{
		if (@this == null)
			return false;

		if (@this == null || @this.Connection is not FbConnection connection)
			return false;

		if (connection == null || connection.State != ConnectionState.Open)
			return false;


		FbDatabaseInfo dbInfo = new(connection);

		return dbInfo.GetActiveTransactionsCount() > 0;
	}



	public bool LockLoadedParser_(string originalString, string updatedString) =>
		LinkageParser.LockLoadedParser(originalString, updatedString);



	public bool MatchesEntityFrameworkAssembly_(string assemblyName)
	{
		if (assemblyName.StartsWith(LibraryData.C_EntityFrameworkAssemblyPrefix, StringComparison.OrdinalIgnoreCase)
			&& assemblyName.EndsWith(LibraryData.C_EntityFrameworkAssemblySuffix, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		/*
		foreach (string version in LibraryData.S_EntityFrameworkVersions)
		{
			if (LibraryData.C_EntityFrameworkAssemblyFullName.FmtRes(version).Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
				return true;
		}
		*/

		return false;
	}



	public bool MatchesInvariantAssembly_(string assemblyName)
	{
		if (assemblyName.StartsWith(LibraryData.C_InvariantAssemblyPrefix, StringComparison.OrdinalIgnoreCase)
			&& assemblyName.EndsWith(LibraryData.C_InvariantAssemblySuffix, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		/*
		foreach (string version in LibraryData.S_InvariantVersions)
		{
			if (LibraryData.C_InvariantAssemblyFullName.FmtRes(version).Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
				return true;
		}
		*/

		return false;
	}



	public (bool, bool) OpenOrVerifyConnection_(IDbConnection @this)
	{
		if (@this is not FbConnection connection)
			return (false, false);

		if (connection.State != ConnectionState.Open)
		{
			connection.Open();
			return (true, false);
		}

		FbDatabaseInfo info = new(connection);
		bool result = info.GetActiveTransactionsCount() > 0;

		return (true, result);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens or verifies a connection. The Connection must exists.
	/// Throws an exception on failure.
	/// Do not call before ensuring IsComplete.
	/// The cardinal must be either zero, if no keepalive reads are required, or an
	/// incremental value that can be used to execute a select statement unique from
	/// the previous call to this method.
	/// </summary>
	/// <returns>
	/// Boolean tuple with Item1: True if open / verification succeeded and
	/// Item2: HasTransactions.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public async Task<(bool, bool)> OpenOrVerifyConnectionAsync_(IDbConnection @this,
		IDbTransaction transaction, CancellationToken cancelToken)
	{
		if (@this is not FbConnection connection)
		{
			ArgumentException ex = new("IDbConnection is not of type FbConnection");
			Diag.Dug(ex);
			throw ex;
		}

		if (connection.State != ConnectionState.Open)
		{
			await connection.OpenAsync(cancelToken);
			return (true, false);
		}


		FbDatabaseInfo info = new(connection);

		if (transaction == null)
		{
			_ = await info.GetDatabaseSizeInPagesAsync(cancelToken);
			return (true, false);
		}


		bool hasTransactions = await info.GetActiveTransactionsCountAsync(cancelToken) > 0;

		return (true, hasTransactions);
	}



	/// <summary>
	/// Parses and converts a server version string to it's Version format.
	/// </summary>
	public Version ParseConnectionServerVersion_(IDbConnection @this)
	{
		if (@this is not FbConnection connection)
			return new();

		if (connection.State != ConnectionState.Open)
			return new();

		return FbServerProperties.ParseServerVersion(connection.ServerVersion);
	}



	public async Task<bool> ReaderCloseAsync_(IDataReader @this, CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ReaderCloseAsync_()");

		try
		{
			await ((FbDataReader)@this).CloseAsync();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		// Tracer.Trace(GetType(), "ReaderCloseAsync_()", "Completed.");

		return true;
	}



	public async Task<DataTable> ReaderGetSchemaTableAsync_(IDataReader @this, CancellationToken cancelToken)
	{
		return await ((FbDataReader)@this).GetSchemaTableAsync(cancelToken);
	}



	public async Task<bool> ReaderNextResultAsync_(IDataReader @this, CancellationToken cancelToken)
	{
		try
		{
			return await ((FbDataReader)@this).NextResultAsync(cancelToken);
		}
		catch (Exception ex)
		{
			if (cancelToken.Cancelled())
			{
				Diag.Expected(ex);

				try
				{
					await ((FbDataReader)@this).CloseAsync(default);
				}
				catch { }

				return false;
			}
			throw;
		}
	}



	public async Task<bool> ReaderReadAsync_(IDataReader @this, CancellationToken cancelToken)
	{
		try
		{
			return await ((FbDataReader)@this).ReadAsync(cancelToken);
		}
		catch (Exception ex)
		{
			if (cancelToken.Cancelled())
			{
				Diag.Expected(ex);

				try
				{
					await ((FbDataReader)@this).CloseAsync(default);
				}
				catch { }

				return false;
			}
			throw;
		}
	}



	public bool TransactionCompleted_(IDbTransaction @this)
	{
		if (@this?.Connection == null)
			return true;

		return (bool)Reflect.GetPropertyValue(@this, "IsCompleted");
	}



	public void UnlockLoadedParser_() => LinkageParser.UnlockLoadedParser();

}
