
using System;
using System.Collections.Concurrent;
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
internal class DatabaseEngineService : SBsNativeDatabaseEngine, IBsNativeDatabaseEngine
{
	private DatabaseEngineService()
	{
	}

	private static ConcurrentDictionary<string, bool> _DependencyCache = null;
	private static IBsNativeDatabaseEngine _Instance = null;
	internal static DatabaseEngineService Instance_ => (DatabaseEngineService)_Instance;
	internal static IBsNativeDatabaseEngine EnsureInstance() => _Instance ??= new DatabaseEngineService();
	private static bool _PrerequisiteLoaded45 = false;
	private static bool _PrerequisiteLoaded3 = false;

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
	// public string[] EntityFrameworkVersions_ => LibraryData.S_EntityFrameworkVersions;
	public string Extension_ => LibraryData.C_Extension;


	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>
	public string ExternalUtilityConfigurationPath_ => LibraryData.S_ExternalUtilityConfigurationPath;


	public string Invariant_ => LibraryData.C_Invariant;
	public string Protocol_ => LibraryData.C_Protocol;
	public Assembly InvariantAssembly_ => typeof(FbConnection).Assembly;
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



	public void EnsureLinkageLoadingAsyin_(IVsDataExplorerConnection root, int delay = 0, int multiplier = 1)
	{
		LinkageParser.EnsureLoadingAsyin(root, delay, multiplier);
	}



	/// <summary>
	/// Hack to address Fb Client bug and ensure embedded versions are connected/opened in the correct order.
	/// Initial Databases must be opened in the order 1. FbServerType.Default then (2) Firebird Embedded 3
	/// then (3) Firebird Embedded 2.
	/// This method ensures this order is maintained by opening prerequisite dummies. This only happens
	/// initially and as and when required. This resolves the issue even if the dummies fail to open.
	/// </summary>
	private bool EnsureVersionConnectOrder(string connectionString)
	{

		// Uncomment if fails to fix bug.
		// _PrerequisiteLoaded45 = true;
		// _PrerequisiteLoaded3 = true;

		FbConnectionStringBuilder csb = new(connectionString);

		if (csb.ServerType == FbServerType.Default)
			return false;



		string path;
		FbConnection conn;
		FbConnectionStringBuilder loadCsb;

		if (!_PrerequisiteLoaded45)
		{
			_PrerequisiteLoaded45 = true;

			if (csb.ClientLibrary == "fbembed45\\fbclient")
				return false;

			try
			{
				path = System.IO.Path.GetDirectoryName(typeof(FbConnection).Assembly.Location) + "\\";
			}
			catch
			{
				path = "";
			}


			loadCsb = new()
			{
				DataSource = "localhost",
				Database = $"{path}FBEMBEDDUMMY45.fdb",
				ServerType = FbServerType.Embedded,
				ClientLibrary = "fbembed45\\fbclient",
				UserID = "SYSDBA",
				Password = "masterkey",
				Pooling = false,
				Enlist = false
			};


			conn = new(loadCsb.ConnectionString);


			try
			{
				// Evs.Debug(GetType(), "EnsureVersionConnectOrder", $"Server FbConnection.Open:\nConnectionString: {conn.ConnectionString}");
				conn.Open();
				conn.Close();
			}
			catch (Exception ex)
			{
				Diag.Expected(ex, $"Location: {typeof(FbConnection).Assembly.Location}, ConnectionString: {loadCsb.ConnectionString}");
			}

			try
			{
				conn.Dispose();
			}
			catch
			{
			}
		}
		else if (csb.ClientLibrary == "fbembed45\\fbclient")
		{
			return EnsureVersionDependencyCache_(FbServerType.Embedded, "fbembed45\\fbclient");
		}

		if (_PrerequisiteLoaded3)
			return EnsureVersionDependencyCache_(FbServerType.Embedded, csb.ClientLibrary);

		bool result = EnsureVersionDependencyCache_(FbServerType.Embedded, "fbclient");

		if (csb.ClientLibrary == "fbclient")
		{
			_PrerequisiteLoaded3 = true;
			return result;
		}


		_PrerequisiteLoaded3 = true;

		try
		{
			path = System.IO.Path.GetDirectoryName(typeof(FbConnection).Assembly.Location) + "\\";
		}
		catch
		{
			path = "";
		}


		loadCsb = new()
		{
			DataSource = "localhost",
			Database = $"{path}FBEMBEDDUMMY3.fdb",
			ServerType = FbServerType.Embedded,
			ClientLibrary = "fbclient",
			UserID = "SYSDBA",
			Password = "masterkey",
			Pooling = false,
			Enlist = false
		};


		conn = new(loadCsb.ConnectionString);


		try
		{
			// Evs.Debug(GetType(), "EnsureVersionConnectOrderAsync", $"{prerequisiteEmbedLibrary} FbConnection.Open:\nConnectionString: {conn.ConnectionString}");
			conn.Open();
			conn.Close();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex, $"Location: {path}, ConnectionString: {loadCsb.ConnectionString}");
		}

		try
		{
			conn.Dispose();
		}
		catch
		{
		}

		return EnsureVersionDependencyCache_(FbServerType.Embedded, csb.ClientLibrary);
	}



	/// <summary>
	/// Hack to address Fb Client bug and ensure embedded versions are connected/opened in the correct order.
	/// Initial Databases must be opened in the order 1. FbServerType.Default then (2) Firebird Embedded 3
	/// then (3) Firebird Embedded 2.
	/// This method ensures this order is maintained by opening prerequisite dummies. This only happens
	/// initially and as and when required. This resolves the issue even if the dummies fail to open.
	/// </summary>
	private async Task<bool> EnsureVersionConnectOrderAsync(string connectionString, CancellationToken cancelToken)
	{
		// Uncomment if fails to fix bug.
		// _PrerequisiteLoaded45 = true;
		// _PrerequisiteLoaded3 = true;

		if (cancelToken.IsCancellationRequested)
			return false;


		FbConnectionStringBuilder csb = new(connectionString);

		if (csb.ServerType == FbServerType.Default)
			return false;



		string path;
		FbConnection conn;
		FbConnectionStringBuilder loadCsb;

		if (!_PrerequisiteLoaded45)
		{
			_PrerequisiteLoaded45 = true;

			if (csb.ClientLibrary == "fbembed45\\fbclient")
				return false;

			try
			{
				path = System.IO.Path.GetDirectoryName(typeof(FbConnection).Assembly.Location) + "\\";
			}
			catch
			{
				path = "";
			}


			loadCsb = new()
			{
				DataSource = "localhost",
				Database = $"{path}FBEMBEDDUMMY45.fdb",
				ServerType = FbServerType.Embedded,
				ClientLibrary = "fbembed45\\fbclient",
				UserID = "SYSDBA",
				Password = "masterkey",
				Pooling = false,
				Enlist = false
			};


			conn = new(loadCsb.ConnectionString);


			try
			{
				// Evs.Debug(GetType(), "EnsureVersionConnectOrder", $"Server FbConnection.Open:\nConnectionString: {conn.ConnectionString}");
				await conn.OpenAsync(cancelToken);
				await conn.CloseAsync();
			}
			catch (Exception ex)
			{
				Diag.Expected(ex, $"Location: {typeof(FbConnection).Assembly.Location}, ConnectionString: {loadCsb.ConnectionString}");
			}

			try
			{
				conn.Dispose();
			}
			catch
			{
			}
		}
		else if (csb.ClientLibrary == "fbembed45\\fbclient")
		{
			return EnsureVersionDependencyCache_(FbServerType.Embedded, "fbembed45\\fbclient");
		}

		if (_PrerequisiteLoaded3)
			return EnsureVersionDependencyCache_(FbServerType.Embedded, csb.ClientLibrary);

		bool result = EnsureVersionDependencyCache_(FbServerType.Embedded, "fbclient");

		if (csb.ClientLibrary == "fbclient")
		{
			_PrerequisiteLoaded3 = true;
			return result;
		}


		_PrerequisiteLoaded3 = true;

		try
		{
			path = System.IO.Path.GetDirectoryName(typeof(FbConnection).Assembly.Location) + "\\";
		}
		catch
		{
			path = "";
		}


		loadCsb = new()
		{
			DataSource = "localhost",
			Database = $"{path}FBEMBEDDUMMY3.fdb",
			ServerType = FbServerType.Embedded,
			ClientLibrary = "fbclient",
			UserID = "SYSDBA",
			Password = "masterkey",
			Pooling = false,
			Enlist = false
		};


		conn = new(loadCsb.ConnectionString);


		try
		{
			// Evs.Debug(GetType(), "EnsureVersionConnectOrderAsync", $"{prerequisiteEmbedLibrary} FbConnection.Open:\nConnectionString: {conn.ConnectionString}");
			await conn.OpenAsync(cancelToken);
			await conn.CloseAsync();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex, $"Location: {path}, ConnectionString: {loadCsb.ConnectionString}");
		}

		try
		{
			conn.Dispose();
		}
		catch
		{
		}

		return EnsureVersionDependencyCache_(FbServerType.Embedded, csb.ClientLibrary);
	}



	public bool EnsureVersionDependencyCache_(FbServerType serverType, string clientLibrary)
	{
		if (serverType != FbServerType.Embedded)
			return false;

		if (clientLibrary != "fbembed" && clientLibrary != "fbclient" && clientLibrary != "fbembed45\\fbclient")
			return false;


		if (_DependencyCache == null)
		{
			string typeName = typeof(FbConnection).AssemblyQualifiedName
				.Replace("FirebirdSql.Data.FirebirdClient.FbConnection", "FirebirdSql.Data.Common.NativeHelpers");

			_DependencyCache = (ConcurrentDictionary<string, bool>)Reflect.GetFieldValue(typeName, "_cache");
		}

		bool result = false;

		if (!_DependencyCache.TryGetValue("fb_dsql_set_timeout", out bool exists))
			return false;

		if (clientLibrary == "fbembed" || clientLibrary == "fbclient")
		{
			if (exists)
			{
				_DependencyCache["fb_dsql_set_timeout"] = false;
				result = true;
			}
		}
		else
		{
			if (!exists)
			{
				_DependencyCache["fb_dsql_set_timeout"] = true;
				result = true;
			}
		}

		return result;
	}


	public bool EnsureVersionDependencyCache_(IDbConnection connection)
	{
		FbConnectionStringBuilder csb = new(connection.ConnectionString);

		return EnsureVersionDependencyCache_(csb.ServerType, csb.ClientLibrary);
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
			if (LibraryData.C_EntityFrameworkAssemblyFullName.Fmt(version).Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
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
			if (LibraryData.C_InvariantAssemblyFullName.Fmt(version).Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
				return true;
		}
		*/

		return false;
	}



	public bool OpenDb_(IDbConnection @this)
	{
		if (@this is not FbConnection connection)
			return false;

		EnsureVersionConnectOrder(@this.ConnectionString);
		connection.Open();

		return true;
	}



	public async Task<bool> OpenDbAsync_(IDbConnection @this, CancellationToken cancelToken)
	{
		if (@this is not FbConnection connection)
			return false;

		await EnsureVersionConnectOrderAsync(@this.ConnectionString, cancelToken);
		await connection.OpenAsync(cancelToken);

		return true;
	}



	public (bool, bool) OpenOrVerifyConnection_(IDbConnection @this)
	{
		if (@this is not FbConnection connection)
			return (false, false);

		if (connection.State != ConnectionState.Open)
		{
			// Evs.Debug(GetType(), "OpenOrVerifyConnection_", $"FbConnection.Open:\nConnectionString: {connection.ConnectionString}");
			connection.OpenDb();
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
			Diag.Ex(ex);
			throw ex;
		}

		if (connection.State != ConnectionState.Open)
		{
			// Evs.Debug(GetType(), "OpenOrVerifyConnectionAsync_", $"FbConnection.OpenDbAsync:\nConnectionString: {connection.ConnectionString}");
			await connection.OpenDbAsync(cancelToken);
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
		// Evs.Trace(GetType(), nameof(ReaderCloseAsync_));

		try
		{
			await ((FbDataReader)@this).CloseAsync();
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		// Evs.Trace(GetType(), nameof(ReaderCloseAsync_), "Completed.");

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
			Diag.Expected(ex);

			if (cancelToken.Cancelled())
			{
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
			Diag.Expected(ex);

			if (cancelToken.Cancelled())
			{
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
