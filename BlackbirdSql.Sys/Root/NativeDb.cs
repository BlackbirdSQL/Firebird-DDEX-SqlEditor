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
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Ctl.Config;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Properties;
using EnvDTE;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



namespace BlackbirdSql;


// =========================================================================================================
//											DbNative Class
//
/// <summary>
/// Central class for database specific class native services.
/// </summary>
// =========================================================================================================
internal static class NativeDb
{

	// ----------------------
	#region Fields - DbNative
	// ----------------------


	internal static object _LockGlobal = new();

	private static IBsNativeDatabaseEngine _DatabaseEngineSvc = null;
	private static IBsNativeProviderSchemaFactory _ProviderSchemaFactorySvc = null;
	private static IBsNativeDatabaseInfo _DatabaseInfoSvc = null;
	private static IBsNativeDbException _DbExceptionSvc = null;
	private static IBsNativeDbServerExplorerService _DbServerExplorerSvc = null;

	private static string _EfAssemblyQualifiedName = null;
	private static string _EfVersion = null;
	private static Type _CsbType = null;

	private static int _ReindexEvsIndex = -1;
	private static int _EventReindexingCardinal = 0;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - DbNative
	// =========================================================================================================


	internal static Type CsbType
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
	internal static string[] EquivalencyKeys => PersistentSettings.EquivalencyKeys;


	internal static bool OnDemandLinkage => PersistentSettings.OnDemandLinkage;

	internal static int LinkageTimeout => PersistentSettings.LinkageTimeout;


	internal static IBsNativeDatabaseEngine DatabaseEngineSvc
	{
		get { return _DatabaseEngineSvc; }
		set { _DatabaseEngineSvc = value; }
	}

	internal static IBsNativeProviderSchemaFactory ProviderSchemaFactorySvc
	{
		get { return _ProviderSchemaFactorySvc; }
		set { _ProviderSchemaFactorySvc = value; }
	}

	internal static IBsNativeDatabaseInfo DatabaseInfoSvc
	{
		get { return _DatabaseInfoSvc; }
		set { _DatabaseInfoSvc = value; }
	}

	internal static IBsNativeDbException DbExceptionSvc
	{
		get { return _DbExceptionSvc; }
		set { _DbExceptionSvc = value; }
	}

	internal static IBsNativeDbServerExplorerService DbServerExplorerSvc
	{
		get { return _DbServerExplorerSvc; }
		set { _DbServerExplorerSvc = value; }
	}


	internal static string AssemblyQualifiedName => DatabaseEngineSvc.AssemblyQualifiedName_;
	internal static string ClientVersion => DatabaseEngineSvc.ClientVersion_;
	internal static Assembly ClientFactoryAssembly => DatabaseEngineSvc.ClientFactoryAssembly_;
	internal static Type ClientFactoryType => DatabaseEngineSvc.ClientFactoryType_;
	internal static Type ConnectionType => DatabaseEngineSvc.ConnectionType_;
	internal static DescriberDictionary Describers => DatabaseEngineSvc.Describers_;
	internal static Assembly EntityFrameworkAssembly => DatabaseEngineSvc.EntityFrameworkAssembly_;
	internal static string Invariant => DatabaseEngineSvc.Invariant_;
	internal static string ProviderFactoryName => DatabaseEngineSvc.ProviderFactoryName_;
	internal static string ProviderFactoryClassName => DatabaseEngineSvc.ProviderFactoryClassName_;
	internal static string ProviderFactoryDescription => DatabaseEngineSvc.ProviderFactoryDescription_;

	internal static string EFProvider => DatabaseEngineSvc.EFProvider_;
	internal static string EFProviderServices => DatabaseEngineSvc.EFProviderServices_;
	internal static string EFConnectionFactory => DatabaseEngineSvc.EFConnectionFactory_;


	internal static string Extension => DatabaseEngineSvc.Extension_;
	internal static string Protocol => DatabaseEngineSvc.Protocol_;
	internal static string Scheme => DatabaseEngineSvc.Scheme_;
	internal static string DbEngineName => DatabaseEngineSvc.DbEngineName_;
	internal static string DataProviderName => DatabaseEngineSvc.DataProviderName_;
	internal static string ExternalUtilityConfigurationPath => DatabaseEngineSvc.ExternalUtilityConfigurationPath_;

	internal static string XmlActualPlanColumn => DatabaseEngineSvc.XmlActualPlanColumn_;
	internal static string XmlEstimatedPlanColumn => DatabaseEngineSvc.XmlEstimatedPlanColumn_;


	internal static string RootObjectTypeName => DatabaseEngineSvc.RootObjectTypeName_;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - DbNative
	// =========================================================================================================


	internal static DbConnection CastToNativeConnection(object connection)
	{
		return DatabaseEngineSvc.CastToNativeConnection_(connection);
	}



	internal static DbCommand CreateDbCommand(string cmdText = null)
	{
		return DatabaseEngineSvc.CreateDbCommand_(cmdText);
	}


	/// <summary>
	/// Creates a native database connection using a connection string.
	/// </summary>
	internal static IDbConnection CreateDbConnection(string connectionString)
	{
		return DatabaseEngineSvc.CreateDbConnection_(connectionString);
	}



	internal static IBsNativeDbStatementWrapper CreateDbStatementWrapper(IBsNativeDbBatchParser owner, object statement, int statementIndex)
	{
		return DatabaseEngineSvc.CreateDbStatementWrapper_(owner, statement, statementIndex);
	}

	internal static IBsNativeDbBatchParser CreateDbBatchParser(EnSqlExecutionType executionType, IBsQueryManager qryMgr, string script)
	{
		return DatabaseEngineSvc.CreateDbBatchParser_(executionType, qryMgr, script);
	}



	internal static string GetDecoratedDdlSource(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		return DbServerExplorerSvc.GetDecoratedDdlSource_(node, targetType);
	}



	internal static bool MatchesEntityFrameworkAssembly(string assemblyName) =>
		DatabaseEngineSvc.MatchesEntityFrameworkAssembly_(assemblyName);



	internal static bool MatchesInvariantAssembly(string assemblyName) =>
		DatabaseEngineSvc.MatchesInvariantAssembly_(assemblyName);



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
		string projectName = project != null
			? Resources.ReindexAssemblies_ProjectName.Fmt(project.Name)
			: Resources.ReindexAssemblies_All;

		_ReindexEvsIndex = Evs.Start(typeof(NativeDb), nameof(ReindexEntityFrameworkAssemblies), projectName,
			nameof(ReindexEntityFrameworkAssemblies), _ReindexEvsIndex);

		try
		{
			List<Project> projects = project == null
				? UnsafeCmd.RecursiveGetDesignTimeProjects()
				: (project.EditableObject() != null ? [project] : []);

			if (projects.Count == 0)
				return projects;

			Evs.Trace(typeof(NativeDb), nameof(ReindexEntityFrameworkAssemblies), $"Project count: {projects.Count}.");

			ServiceProvider serviceProvider = null;
			DynamicTypeService dynamicTypeService = null;
			IVsSolution solution = null;

			ITypeResolutionService typeResolutionService;

			Type providerServicesType = DatabaseEngineSvc.EFProviderServicesType_;

			VSProject projectObject;
			Reference efReference;

			foreach (Project proj in projects)
			{
				projectObject = proj.EditableObject();

				if (projectObject == null)
					continue;

				try
				{
					if (projectObject.References == null)
						continue;

					efReference = projectObject.References.Find(EFProvider);

					if (efReference == null)
						continue;
				}
				catch
				{
					continue;
				}

				// Evs.Debug(typeof(NativeDb), "ReindexEntityFrameworkAssemblies()", $"Loop Project: {proj.Name}.");

				if (serviceProvider == null)
				{
					serviceProvider = new((IOleServiceProvider)ApcManager.Dte);
					dynamicTypeService = serviceProvider.GetService(typeof(DynamicTypeService)) as DynamicTypeService;
					Diag.ThrowIfServiceUnavailable(dynamicTypeService, typeof(DynamicTypeService));

					solution = serviceProvider.GetService(typeof(IVsSolution)) as IVsSolution;

					ReindexVersioningFacade(proj, efReference);
				}

				solution.GetProjectOfUniqueName(proj.UniqueName, out IVsHierarchy vsHierarchy);
				typeResolutionService = dynamicTypeService.GetTypeResolutionService(vsHierarchy);
				Diag.ThrowIfServiceUnavailable(typeResolutionService, typeof(ITypeResolutionService));

				if (_EfAssemblyQualifiedName == null)
				{
					_EfAssemblyQualifiedName = providerServicesType.AssemblyQualifiedName;
					_EfVersion = providerServicesType.Assembly.GetName().Version.ToString();
				}

				string key = _EfAssemblyQualifiedName;
				Dictionary<string, Type> typeCache = (Dictionary<string, Type>)Reflect.GetFieldValue(typeResolutionService, "_typeCache");

				if (typeCache == null)
				{
					typeCache = new Dictionary<string, Type>(127);
					Reflect.SetFieldValue(typeResolutionService, "_typeCache", typeCache);
				}
				else if (typeCache.ContainsKey(key))
				{
					continue;
				}

				// Evs.Trace(typeof(Cmd), nameof(ReindexEntityFrameworkAssemblies), "Reindexing EntityFrameworkAssemblies for project: {0}.", proj.Name);

				typeCache[key] = providerServicesType;

				key = _EfAssemblyQualifiedName.Replace(_EfVersion, efReference.Version);

				typeCache[key] = providerServicesType;

				/*
				foreach (string version in DatabaseEngineSvc.EntityFrameworkVersions_)
				{
					key = _EfAssemblyQualifiedName.Replace(_EfVersion, version);

					typeCache[key] = providerServicesType;
				}
				*/
			}

			return projects;

		}
		finally
		{
			Evs.Stop(typeof(NativeDb), nameof(ReindexEntityFrameworkAssemblies), projectName,
				nameof(ReindexEntityFrameworkAssemblies), _ReindexEvsIndex);
		}

	}



	// -----------------------------------------------------------------------------------------------------
	/// <summary>
	/// [Async on UI thread]: Reindex the EntityFramwork ProviderService type in the TypeResolutionService. 
	/// </summary>
	// -----------------------------------------------------------------------------------------------------
	internal static void ReindexEntityFrameworkAssembliesAsyui(Project project = null)
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
					// again after checking for SolutionClosing and switching to the main
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



	// -----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Currently diabled. Reindexes the Entity Framework VersioningFacade.DependencyResolver.
	/// </summary>
	// -----------------------------------------------------------------------------------------------------
	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Caller has checked.")]
	internal static bool ReindexVersioningFacade(Project project, Reference efReference = null)
	{
		// TODO: Always exits because reindex still results in edmx "Create database" failure.
		if (project != null)
			return false;

		if (project == null)
			return false;

		VSProject projectObject = project.EditableObject();

		if (projectObject == null)
			return false;


		if (efReference == null)
		{
			try
			{
				if (projectObject.References == null)
					return false;

				efReference = projectObject.References.Find(EFProvider);

				if (efReference == null)
					return false;
			}
			catch
			{
				return false;
			}
		}


		try
		{
			Type edmResolverType = Type.GetType("Microsoft.Data.Entity.Design.VersioningFacade.DependencyResolver, " +
					"Microsoft.Data.Entity.Design.VersioningFacade", true, true)
				?? throw new TypeAccessException(Resources.ExceptionEdmVersioningFacadeDependencyResolverType);

			object edmResolver = Reflect.GetFieldValue(edmResolverType, "Instance")
				?? throw new TypeAccessException(Resources.ExceptionEdmVersioningFacadeDependencyResolverInstance);

			Reflect.InvokeMethod(edmResolver, "RegisterProvider", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
				[DatabaseEngineSvc.EFProviderServicesType_, Invariant]);

			// Evs.Debug(typeof(NativeDb), "ReindexEntityFrameworkAssemblies()", $"Registered Project: {proj.Name}.");
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			return false;
		}

		return true;
	}



	/*
	internal static ITypeResolutionService GetTypeResolutionService(System.IServiceProvider serviceProvider, EnvDTE.Project project)
	{
		if (serviceProvider == null)
		{
			ServiceProvider oleServiceProvider = new((IOleServiceProvider)ApcManager.Dte);
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
			NativeMethods.ThrowOnFailure(vsSolution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy ppHierarchy));
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


	internal static byte GetErrorClass(object error) => DbExceptionSvc.GetErrorClass_(error);
	internal static int GetErrorLineNumber(object error) => DbExceptionSvc.GetErrorLineNumber_(error);
	internal static string GetErrorMessage(object error) => DbExceptionSvc.GetErrorMessage_(error);
	internal static int GetErrorNumber(object error) => DbExceptionSvc.GetErrorNumber_(error);
	internal static int GetObjectTypeIdentifierLength(string typeName) => DbServerExplorerSvc.GetObjectTypeIdentifierLength_(typeName);
	internal static Version ParseServerVersion(IDbConnection connection) => connection.ParseServerVersion();
	internal static ICollection<object> GetErrorEnumerator(IList<object> errors) => DbExceptionSvc.GetErrorEnumerator_(errors);
	internal static bool LockLoadedParser(string originalString, string updatedString) => DatabaseEngineSvc.LockLoadedParser_(originalString, updatedString);
	internal static void UnlockLoadedParser() => DatabaseEngineSvc.UnlockLoadedParser_();


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
			// Evs.Trace(GetType(), nameof(EventProjectEnter), "_EventProjectCardinal: {0}, increment: {1}.", _EventProjectCardinal, increment);

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
				ApplicationException ex = new(Resources.ExceptionEventReindexingExit.Fmt(_EventReindexingCardinal));
				Diag.Ex(ex);
				throw ex;
			}

			_EventReindexingCardinal--;
		}
	}


	#endregion Events and Event handling

}
