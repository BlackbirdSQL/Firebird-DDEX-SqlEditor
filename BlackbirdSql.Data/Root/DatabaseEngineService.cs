
using System;
using System.Collections.Generic;
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
using FirebirdSql.Data.Isql;
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





	public void AsyncEnsureLinkageLoading_(IVsDataExplorerConnection root, int delay = 0, int multiplier = 1)
	{
		LinkageParser.AsyncEnsureLoading(root, delay, multiplier);
	}


	public DbConnection CastToNativeConnection_(object connection)
	{
		return connection as FbConnection;
	}



	public string ConvertDataTypeToSql_(object type, object length, object precision, object scale)
	{
		return DbTypeHelper.ConvertDataTypeToSql(type, length, precision, scale);
	}

	public DbCommand CreateDbCommand_(string cmdText = null)
	{
		return new FbCommand(cmdText);
	}


	/// <summary>
	/// Creates a Firebird connection using a connection string.
	/// </summary>
	public IDbConnection CreateDbConnection_(string  connectionString)
	{
		return new FbConnection(connectionString);
	}

	public IBsNativeDbBatchParser CreateDbBatchParser_(EnSqlExecutionType executionType, IBsQueryManager qryMgr, string script)
	{
		return new DbBatchParser(executionType, qryMgr, script);
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decorates a DDL raw script to it's executable form.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetDecoratedDdlSource_(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		// Tracer.Trace(typeof(Moniker), "GetDecoratedDdlSource()");

		IVsDataObject obj = node.Object;

		if (obj == null)
			return "Node has no object";

		string prop = GetNodeScriptProperty(obj).ToUpper();
		string src = ((string)obj.Properties[prop]).Replace("\r\n", "\n").Replace("\r", "\n");

		string dirtysrc = "";

		while (src != dirtysrc)
		{
			dirtysrc = src;
			src = dirtysrc.Replace("\n\n\n", "\n\n");
		}


		switch (node.NodeBaseType())
		{
			case EnModelObjectType.Function:
				return GetDecoratedDdlSourceFunction(src, node, obj, targetType);
			case EnModelObjectType.StoredProcedure:
				return GetDecoratedDdlSourceProcedure(src, node, obj, targetType);
			case EnModelObjectType.Table:
				return GetDecoratedDdlSourceTable(src);
			case EnModelObjectType.Trigger:
				return GetDecoratedDdlSourceTrigger(src, obj, targetType);
			case EnModelObjectType.View:
				return GetDecoratedDdlSourceView(src, node, obj, targetType);
			default:
				return src;
		}
	}



	private static string GetDecoratedDdlSourceFunction(string src, IVsDataExplorerNode node,
		IVsDataObject obj, EnModelTargetType targetType)
	{
		int direction;
		string flddef;
		string script = "";
		string strout = "";

		IVsDataExplorerChildNodeCollection nodes = node.GetChildren(false);


		foreach (IVsDataExplorerNode child in nodes)
		{
			if (child.Object == null)
				continue;

			if (child.Object.Type.Name != "FunctionParameter" && child.Object.Type.Name != "FunctionReturnValue")
				continue;

			direction = (short)child.Object.Properties["ORDINAL_POSITION"] == 0 ? 1 : 0;

			flddef = (direction == 0 ? child.Object.Properties["ARGUMENT_NAME"] + " " : "")
					+ NativeDb.ConvertDataTypeToSql(child.Object.Properties["FIELD_DATA_TYPE"],
					child.Object.Properties["FIELD_SIZE"], child.Object.Properties["NUMERIC_PRECISION"],
					child.Object.Properties["NUMERIC_SCALE"]);

			if (direction == 0) // In
				script += (script != "" ? ",\n\t" : "\n\t(") + flddef;

			if (direction > 0) // Out
				strout += (strout != "" ? ",\n\t" : "") + flddef;
		}

		if (script != "")
			script += ")";

		if (strout != "")
			strout = $"\nRETURNS {strout}";

		if (strout != "")
			script += strout;

		if (targetType == EnModelTargetType.AlterScript)
			script = $"ALTER FUNCTION {obj.Properties["FUNCTION_NAME"]}{script}\n";
		else
			script = $"CREATE FUNCTION {obj.Properties["FUNCTION_NAME"]}{script}\n";

		src = $"{script}AS\n{src}";

		return WrapScriptWithTerminators(src);
	}



	private static string GetDecoratedDdlSourceProcedure(string src, IVsDataExplorerNode node,
		IVsDataObject obj, EnModelTargetType targetType)
	{
		int direction;
		string flddef;
		string script = "";
		string strout = "";

		IVsDataExplorerChildNodeCollection nodes = node.GetChildren(false);


		foreach (IVsDataExplorerNode child in nodes)
		{
			if (child.Object == null || child.Object.Type.Name != "StoredProcedureParameter")
				continue;
			direction = Convert.ToInt32(child.Object.Properties["PARAMETER_DIRECTION"]);

			flddef = child.Object.Properties["PARAMETER_NAME"] + " "
					+ NativeDb.ConvertDataTypeToSql(child.Object.Properties["FIELD_DATA_TYPE"],
					child.Object.Properties["FIELD_SIZE"], child.Object.Properties["NUMERIC_PRECISION"],
					child.Object.Properties["NUMERIC_SCALE"]);

			if (direction == 0 || direction == 3) // In
			{
				if (targetType == EnModelTargetType.AlterScript)
					script += (script != "" ? ",\n\t" : "\n\t(") + flddef;
				else
					script += (script != "" ? "\n" : "\n-- The following input parameters need to be initialized after the BEGIN statement\n")
						+ "DECLARE VARIABLE " + flddef + ";";
			}
			if (direction > 0) // Out
				strout += (strout != "" ? ",\n\t" : "\n\t(") + flddef;
		}
		if (script != "" && targetType == EnModelTargetType.AlterScript)
			script += ")";
		if (strout != "")
			strout = $"\nRETURNS{strout})";

		if (strout != "" && targetType == EnModelTargetType.AlterScript)
			script += strout;

		if (targetType == EnModelTargetType.AlterScript)
			script = $"ALTER PROCEDURE {obj.Properties["PROCEDURE_NAME"]}{script}\n";
		else
			strout = $"EXECUTE BLOCK{strout}\n";

		if (targetType == EnModelTargetType.AlterScript)
			src = $"{script}AS\n{src}";
		else
			src = $"{strout}AS{script}\n-- End of parameter declarations\n{src}";

		return WrapScriptWithTerminators(src);
	}



	private static string GetDecoratedDdlSourceTable(string src)
	{
		src = $"SELECT * FROM {src.ToUpperInvariant()}";
		return src;
	}



	private static string GetDecoratedDdlSourceTrigger(string src, IVsDataObject obj,
		EnModelTargetType targetType)
	{
		string script;
		string active = (bool)obj.Properties["IS_INACTIVE"] ? "INACTIVE" : "ACTIVE";

		if (targetType == EnModelTargetType.AlterScript)
			script = $"ALTER TRIGGER {obj.Properties["TRIGGER_NAME"]}";
		else
			script = $"CREATE TRIGGER {obj.Properties["TRIGGER_NAME"]} FOR {obj.Properties["TABLE_NAME"]}";

		src = $"{script} {active}\n"
			+ $"{GetTriggerEventType((long)obj.Properties["TRIGGER_TYPE"])} POSITION {(short)obj.Properties["PRIORITY"]}\n"
			+ src;
		return WrapScriptWithTerminators(src);
	}



	private static string GetDecoratedDdlSourceView(string src, IVsDataExplorerNode node,
		IVsDataObject obj, EnModelTargetType targetType)
	{
		if (targetType != EnModelTargetType.AlterScript)
			return src;

		string script = "";
		IVsDataExplorerChildNodeCollection nodes = node.GetChildren(false);

		foreach (IVsDataExplorerNode child in nodes)
			script += (script != "" ? ",\n\t" : "\n\t(") + child.Object.Properties["COLUMN_NAME"];

		if (script != "")
			script += ")";

		src = $"ALTER VIEW {obj.Properties["VIEW_NAME"]}{script}\nAS\n{src}";
		return src;
	}



	public byte GetErrorClass_(object error)
	{
		return ((FbError)error).Class;
	}

	public int GetErrorLineNumber_(object error)
	{
		return ((FbError)error).LineNumber;
	}



	public string GetErrorMessage_(object error)
	{
		return ((FbError)error).Message;
	}


	public int GetErrorNumber_(object error)
	{
		return ((FbError)error).Number;
	}

	public IBsNativeDbLinkageParser GetLinkageParserInstance_(IVsDataExplorerConnection root) => LinkageParser.GetInstance(root);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the Source script of a node if it exists else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static string GetNodeScriptProperty(IVsDataObject obj)
	{
		if (obj != null)
			return GetNodeScriptProperty(obj.Type.Name);

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the Source script of a node if it exists else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static string GetNodeScriptProperty(string type)
	{
		if (type.EndsWith("Column") || type.EndsWith("Trigger")
			 || type == "Index" || type == "ForeignKey")
		{
			return "Expression";
		}
		else if (type == "Table")
		{
			return "Table_Name";
		}
		else if (type == "View")
		{
			return "Definition";
		}
		else if (type == "StoredProcedure" || type == "Function")
		{
			return "Source";
		}

		return type;
	}


	public int GetObjectTypeIdentifierLength_(string typeName)
	{
		return DslObjectTypes.GetIdentifierLength(typeName);
	}


	public ICollection<object> GetErrorEnumerator_(IList<object> errors)
	{
		if (errors == null)
			return null;

		return errors;
	}



	// ---------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------
	private static string GetTriggerEventType(long eventType)
	{
		return eventType switch
		{
			1 => "BEFORE INSERT",
			2 => "AFTER INSERT",
			3 => "BEFORE UPDATE",
			4 => "AFTER UPDATE",
			5 => "BEFORE DELETE",
			6 => "AFTER DELETE",
			17 => "BEFORE INSERT OR UPDATE",
			18 => "AFTER INSERT OR UPDATE",
			25 => "BEFORE INSERT OR DELETE",
			26 => "AFTER INSERT OR DELETE",
			27 => "BEFORE UPDATE OR DELETE",
			28 => "AFTER UPDATE OR DELETE",
			113 => "BEFORE INSERT OR UPDATE OR DELETE",
			114 => "AFTER INSERT OR UPDATE OR DELETE",
			8192 => "ON CONNECT",
			8193 => "ON DISCONNECT",
			8194 => "ON TRANSACTION BEGIN",
			8195 => "ON TRANSACTION COMMIT",
			8196 => "ON TRANSACTION ROLLBACK",
			_ => "",
		};
	}



	public bool HasTransactions_(IDbTransaction @this)
	{
		if (@this == null)
			return false;

		FbConnection connection = (FbConnection)@this.Connection;

		if (connection == null || connection.State != ConnectionState.Open)
			return false;


		FbDatabaseInfo dbInfo = new(connection);

		return dbInfo.GetActiveTransactionsCount() > 0;
	}


	public bool IsSupportedCommandType_(object command)
	{
		return command is IBsNativeDbStatementWrapper || command is FbCommand || command is FbBatchExecution;
	}

	public bool IsSupportedConnection_(IDbConnection connection)
	{
		return connection is FbConnection;
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



	public void OpenConnection_(DbConnection connection) => ((FbConnection)connection).Open();


	public void UnlockLoadedParser_() => LinkageParser.UnlockLoadedParser();



	public bool TransactionCompleted_(IDbTransaction @this)
	{
		if (@this?.Connection == null)
			return true;

		return (bool)Reflect.GetPropertyValue(@this, "IsCompleted");
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
		return await ((FbDataReader)@this).NextResultAsync(cancelToken);
	}

	public async Task<bool> ReaderReadAsync_(IDataReader @this, CancellationToken cancelToken)
	{
		return await ((FbDataReader)@this).ReadAsync(cancelToken);
	}


	private static string WrapScriptWithTerminators(string script)
	{
		return $"SET TERM GO ;\n{script}\nGO\nSET TERM ; GO";
	}

}
