// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;



namespace BlackbirdSql;


// =========================================================================================================
//											DbNative Class
//
/// <summary>
/// Central class for database specific class native services.
/// </summary>
// =========================================================================================================
public static class NativeDb
{

	// ----------------------
	#region Fields - DbNative
	// ----------------------


	public static object _LockGlobal = new();

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

	private static int _EventReindexingCardinal = 0;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - DbNative
	// =========================================================================================================


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
	public static Assembly  ClientFactoryAssembly => DatabaseEngineSvc.ClientFactoryAssembly_;
	public static Type ClientFactoryType => DatabaseEngineSvc.ClientFactoryType_;
	public static Type ConnectionType => DatabaseEngineSvc.ConnectionType_;
	public static DescriberDictionary Describers => DatabaseEngineSvc.Describers_;
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


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - DbNative
	// =========================================================================================================


	public static void AsyncReindexEntityFrameworkAssemblies(Project project = null)
	{
		// If it's a solution reindex (project == null), lock entry so that only
		// other solution reindex requests can enter.
		if (project == null)
		{
			if (!EventReindexingEnter(false, true))
				return;
		}
		else
		{
			if (!EventReindexingEnter(true))
				return;
		}


		// Get in behind everyone else so that we're last.

		_ = Task.Factory.StartNew(
				async () =>
				{
					// If it's a solution reindex, unlock. If other solution reindex's
					// are behind us they'll lock us out when we call EventReindexingEnter()
					// again after checking for SolutionClosing and swithching to the main
					// thread..
					// This is perfect code logic because it means only the last solution
					// reindex will get through. Everything else will be discarded. Even
					// requests ahead of it.
					if (project == null)
						EventReindexingExit();

					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

					if (!EventReindexingEnter())
						return;

					try
					{
						ReindexEntityFrameworkAssemblies(project);
					}
					catch (Exception ex)
					{
						Diag.ThrowException(ex);
					}
					finally
					{
						EventReindexingExit();
					}
				},
				default, TaskCreationOptions.None, TaskScheduler.Default);
	}



	public static DbConnection CastToNativeConnection(object connection)
	{
		return DatabaseEngineSvc.CastToNativeConnection_(connection);
	}


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



	public static IBsNativeDbStatementWrapper CreateDbStatementWrapper(IBsNativeDbBatchParser owner, object statement, int statementIndex)
	{
		return DatabaseEngineSvc.CreateDbStatementWrapper_(owner, statement, statementIndex);
	}

	public static IBsNativeDbBatchParser CreateDbBatchParser(EnSqlExecutionType executionType, IBsQueryManager qryMgr, string script)
	{
		return DatabaseEngineSvc.CreateDbBatchParser_(executionType, qryMgr, script);
	}



	public static string GetDecoratedDdlSource(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		return DatabaseEngineSvc.GetDecoratedDdlSource_(node, targetType);
	}



	// -----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Registers all versions of the native db EntityFramework implementation with the
	/// <see cref="ITypeResolutionService"/> of all design time user projects using the entity data model.
	/// If the project argument is supplied, only that project is targeted.
	/// </summary>
	// -----------------------------------------------------------------------------------------------------
	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller has checked.")]
	private static List<Project> ReindexEntityFrameworkAssemblies(Project project = null)
	{
		// Tracer.Trace(typeof(NativeDb), "ReindexEntityFrameworkAssemblies()");

		List<Project> projects = project == null ? UnsafeCmd.RecursiveGetDesignTimeProjects() : (project.IsEditable() ? [project] : []);

		if (projects.Count == 0)
			return projects;

		// Tracer.Trace(typeof(NativeDb), "ReindexEntityFrameworkAssemblies()", "Project count: {0}.", projects.Count);

		ServiceProvider serviceProvider = null;
		DynamicTypeService dynamicTypeService = null;
		IVsSolution solution = null;

		ITypeResolutionService typeResolutionService;

		Type providerServicesType = DatabaseEngineSvc.EFProviderServicesType_;
		string typeKey = DatabaseEngineSvc.EFProviderServicesTypeFullName_;

		Dictionary<string, Type> typeCache;
		VSProject projectObject;
		string key;

		foreach (Project proj in projects)
		{
			if (!proj.IsEditable())
				continue;

			projectObject = proj.Object as VSProject;

			if (projectObject.References.Find(EFProvider) == null)
				continue;


			if (serviceProvider == null)
			{
				serviceProvider = new((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)ApcManager.Dte);
				dynamicTypeService = serviceProvider.GetService(typeof(DynamicTypeService)) as DynamicTypeService;
				Diag.ThrowIfServiceUnavailable(dynamicTypeService, typeof(DynamicTypeService));

				solution = serviceProvider.GetService(typeof(IVsSolution)) as IVsSolution;
			}

			solution.GetProjectOfUniqueName(proj.UniqueName, out IVsHierarchy vsHierarchy);
			typeResolutionService = dynamicTypeService.GetTypeResolutionService(vsHierarchy);
			Diag.ThrowIfServiceUnavailable(typeResolutionService, typeof(ITypeResolutionService));

			typeCache = (Dictionary<string, Type>)Reflect.GetFieldValue(typeResolutionService, "_typeCache");

			key = typeKey.FmtRes("1.0.0.0");

			if (typeCache == null)
			{
				typeCache = new Dictionary<string, Type>(127);
				Reflect.SetFieldValue(typeResolutionService, "_typeCache", typeCache);
			}
			else if (typeCache.ContainsKey(key))
			{
				continue;
			}

			// Tracer.Trace(typeof(Cmd), "ReindexEntityFrameworkAssemblies()", "Reindexing EntityFrameworkAssemblies for project: {0}.", proj.Name);

			typeCache[key] = providerServicesType;

			foreach (string version in DatabaseEngineSvc.EntityFrameworkVersions_)
			{
				key = typeKey.FmtRes(version);

				typeCache[key] = providerServicesType;
			}

		}

		return projects;

	}


	/*
	internal static ITypeResolutionService GetTypeResolutionService(System.IServiceProvider serviceProvider, EnvDTE.Project project)
	{
		if (serviceProvider == null)
		{
			ServiceProvider oleServiceProvider = new((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)ApcManager.Dte);
			serviceProvider = oleServiceProvider.GetService(typeof(IDesignerHost));
			Diag.ThrowIfServiceUnavailable(dynamicTypeService, typeof(DynamicTypeService));
			throw new ArgumentNullException("serviceProvider");
		}
		ITypeResolutionService typeResolutionService = null;
		if (GetServiceFromItemContext(serviceProvider, typeof(ITypeResolutionService)) is ITypeResolutionService result)
		{
			return result;
		}
		typeResolutionService = serviceProvider.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
		if (typeResolutionService == null)
		{
			IVsSolution vsSolution = (IVsSolution)serviceProvider.GetService(typeof(IVsSolution));
			if (vsSolution == null)
			{
				return null;
			}
			if (project == null)
			{
				project = ProjectItemUtil.GetCurrentProject();
				if (project == null)
				{
					return null;
				}
			}
			NativeMethods.ThrowOnFailure(vsSolution.GetProjectOfUniqueName(project.UniqueName, out var ppHierarchy));
			if (ppHierarchy == null)
			{
				return null;
			}
			IVsProject vsproject = ppHierarchy as IVsProject;
			VSDOCUMENTPRIORITY[] priority = new VSDOCUMENTPRIORITY[1];
			ProjectItems projectItems = null;
			try
			{
				projectItems = project.ProjectItems;
			}
			catch (NotImplementedException)
			{
			}
			if (projectItems != null)
			{
				typeResolutionService = GetTypeResolutionServiceFromProjectItems(serviceProvider, projectItems, ppHierarchy, vsproject, priority);
			}
			if (typeResolutionService == null)
			{
				DynamicTypeService dynamicTypeService = serviceProvider.GetService(typeof(DynamicTypeService)) as DynamicTypeService;
				try
				{
					typeResolutionService = dynamicTypeService.GetTypeResolutionService(ppHierarchy, uint.MaxValue);
				}
				catch (Exception)
				{
				}
			}
		}
		return typeResolutionService;
	}
	*/


	public static byte GetErrorClass(object error) => DatabaseEngineSvc.GetErrorClass_(error);
	public static int GetErrorLineNumber(object error) => DatabaseEngineSvc.GetErrorLineNumber_(error);
	public static string GetErrorMessage(object error) => DatabaseEngineSvc.GetErrorMessage_(error);
	public static int GetErrorNumber(object error) => DatabaseEngineSvc.GetErrorNumber_(error);
	public static int GetObjectTypeIdentifierLength(string typeName) => DatabaseEngineSvc.GetObjectTypeIdentifierLength_(typeName);
	public static Version ParseServerVersion(IDbConnection connection) => connection.ParseServerVersion();
	public static ICollection<object> GetErrorEnumerator(IList<object> errors) => DatabaseEngineSvc.GetErrorEnumerator_(errors);


	public static bool IsSupportedCommandType(object command) => DatabaseEngineSvc.IsSupportedCommandType_(command);

	public static bool IsSupportedConnection(IDbConnection connection) => DatabaseEngineSvc.IsSupportedConnection_(connection);

	public static bool LockLoadedParser(string originalString, string updatedString) => DatabaseEngineSvc.LockLoadedParser_(originalString, updatedString);
	public static void OpenConnection(DbConnection connection) => DatabaseEngineSvc.OpenConnection_(connection);

	public static void UnlockLoadedParser() => DatabaseEngineSvc.UnlockLoadedParser_();


	#endregion Methods





	// =========================================================================================================
	#region Events and Event handling - DbNative
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="_EventReindexingCardinal"/> counter when execution
	/// enters a Reindexing event handler to prevent recursion.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool EventReindexingEnter(bool test = false, bool force = false)
	{
		lock (_LockGlobal)
		{
			// Tracer.Trace(GetType(), "EventProjectEnter()", "_EventProjectCardinal: {0}, increment: {1}.", _EventProjectCardinal, increment);

			if (_EventReindexingCardinal > 0 && !force)
				return false;
		}

		if (ApcManager.SolutionClosing)
			return false;

		lock (_LockGlobal)
		{
			if (!test)
				_EventReindexingCardinal++;
		}

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="_EventReindexingCardinal"/> counter that was previously
	/// incremented by <see cref="EventReindexingEnter"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static void EventReindexingExit()
	{
		lock (_LockGlobal)
		{
			if (_EventReindexingCardinal <= 0)
			{
				ApplicationException ex = new($"Attempt to exit Reindexing event when not in a Reindexing event. _EventReindexingCardinal: {_EventReindexingCardinal}");
				Diag.Dug(ex);
				throw ex;
			}

			_EventReindexingCardinal--;
		}
	}


	#endregion Events and Event handling

}
