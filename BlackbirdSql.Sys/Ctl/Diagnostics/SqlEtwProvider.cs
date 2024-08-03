// Microsoft.Data.Tools.Components, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Components.Diagnostics.SqlEtwProvider
using System;
using System.Diagnostics.Eventing;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Sys.Ctl.Diagnostics;


public static class SqlEtwProvider
{

	static SqlEtwProvider()
	{
		S_EventProvider = new EventProviderVersionTwo(new Guid(LibraryData.C_MandatedEventProviderGuid));

		SchemaCompare = new EventDescriptor(4, 1, 0, 4, 0, 3, 513L);
		SchemaCompareError = new EventDescriptor(69, 1, 0, 2, 0, 3, 513L);
		SqlEditorExecute = new EventDescriptor(5, 1, 0, 4, 0, 3, 513L);
		ProjectLoad = new EventDescriptor(6, 1, 0, 4, 0, 3, 513L);
		ProjectOpen = new EventDescriptor(7, 1, 0, 4, 0, 3, 513L);
		ProjectWizardImportSchemaFinish = new EventDescriptor(8, 1, 0, 4, 0, 3, 513L);
		ProjectBuild = new EventDescriptor(9, 1, 0, 1, 0, 3, 513L);
		ProjectBuildError = new EventDescriptor(70, 1, 0, 2, 0, 3, 513L);
		ProjectDeploy = new EventDescriptor(10, 1, 0, 1, 0, 3, 513L);
		DisplayAdapterSchemaObjectChangeDone = new EventDescriptor(14, 1, 0, 4, 0, 4, 1025L);
		SchemaViewNodePopulationComplete = new EventDescriptor(15, 1, 0, 4, 0, 4, 1025L);
		ConnectionStringPersistedInRegistry = new EventDescriptor(16, 1, 0, 4, 0, 4, 1025L);
		DataSchemaModelRecycle = new EventDescriptor(17, 1, 0, 4, 0, 4, 1025L);
		ImportSchema = new EventDescriptor(25, 1, 0, 4, 0, 5, 9L);
		ImportSchemaFinish = new EventDescriptor(26, 1, 0, 4, 0, 5, 9L);
		ImportSchemaFinishError = new EventDescriptor(68, 1, 0, 4, 0, 5, 9L);
		ImportSchemaGenerateAllScripts = new EventDescriptor(27, 1, 0, 4, 0, 5, 9L);
		ImportSchemaGenerateSingleScript = new EventDescriptor(28, 1, 0, 4, 0, 5, 9L);
		ImportSchemaAddAllScriptsToProject = new EventDescriptor(29, 1, 0, 4, 0, 5, 9L);
		ImportSchemaAddSingleScriptToProject = new EventDescriptor(30, 1, 0, 4, 0, 5, 9L);
		ImportSchemaGenerateProjectMapForType = new EventDescriptor(31, 1, 0, 4, 0, 5, 9L);
		ImportSchemaGenerateProjectMapForElement = new EventDescriptor(32, 1, 0, 4, 0, 5, 9L);
		ImportSchemaAddScriptsToProjectForType = new EventDescriptor(33, 1, 0, 4, 0, 5, 9L);
		ImportScript = new EventDescriptor(34, 1, 0, 4, 0, 6, 17L);
		ModelCompare = new EventDescriptor(67, 1, 0, 4, 0, 10, 257L);
		Commit = new EventDescriptor(87, 1, 0, 4, 0, 0, 1L);
		LogCritical = new EventDescriptor(71, 1, 0, 1, 0, 11, 2048L);
		LogError = new EventDescriptor(72, 1, 0, 2, 0, 11, 2048L);
		LogWarning = new EventDescriptor(73, 1, 0, 3, 0, 11, 2048L);
		LogInformational = new EventDescriptor(74, 1, 0, 4, 0, 11, 2048L);
		LogVerbose = new EventDescriptor(75, 1, 0, 5, 0, 11, 2048L);
		TableDesignerUpdateContextView = new EventDescriptor(79, 1, 16, 4, 0, 0, -9223372036854771711L);
		TableDesignerAddNewTable = new EventDescriptor(80, 1, 0, 4, 0, 0, 4097L);
		TableDesignerOpenTable = new EventDescriptor(81, 1, 0, 4, 0, 0, 4097L);
		TableDesignerSpecifyTableProperties = new EventDescriptor(82, 1, 0, 4, 0, 0, 4097L);
		TableDesignerAddColumns = new EventDescriptor(83, 1, 0, 1, 0, 0, 4097L);
		TableDesignerAddObjectFromCtxPane = new EventDescriptor(84, 1, 0, 4, 0, 0, 4097L);
		TableDesignerRefactorRename = new EventDescriptor(85, 1, 0, 4, 0, 0, 4097L);
		TableDesignerDeleteColumns = new EventDescriptor(86, 1, 0, 4, 0, 0, 4097L);
		SchemaCompareDataPopulationJob = new EventDescriptor(88, 1, 0, 4, 0, 0, 16385L);
		SchemaCompareDataPopulationCancel = new EventDescriptor(89, 1, 0, 4, 0, 0, 16385L);
		SchemaCompareScriptPopulationJob = new EventDescriptor(90, 1, 0, 4, 0, 0, 16385L);
		SchemaCompareScriptPopulationCancel = new EventDescriptor(91, 1, 0, 4, 0, 0, 16385L);
		SchemaCompareGetAndResolveDataSchemaModel = new EventDescriptor(92, 1, 0, 4, 0, 0, 16385L);
		SchemaCompareModelCompare = new EventDescriptor(93, 1, 0, 4, 0, 0, 16385L);
		SchemaCompareGenerateVisual = new EventDescriptor(94, 1, 0, 4, 0, 0, 16385L);
		SchemaCompareUpdateTargetJob = new EventDescriptor(95, 1, 0, 4, 0, 0, 16385L);
		SchemaCompareUpdateTargetCancel = new EventDescriptor(96, 1, 0, 4, 0, 0, 16385L);
		ProjectSystemSnapshot = new EventDescriptor(120, 1, 0, 4, 0, 0, 513L);
		ProjectSystemSnapshotBuildFailed = new EventDescriptor(121, 1, 0, 4, 0, 0, 513L);
		ProjectSystemPublishing = new EventDescriptor(122, 1, 0, 4, 0, 0, 513L);
		ProjectSystemPublishCreateDeploymentPlan = new EventDescriptor(123, 1, 0, 4, 0, 0, 513L);
		ProjectSystemPublishCreatePublishScripts = new EventDescriptor(124, 1, 0, 4, 0, 0, 513L);
		ProjectSystemPublishShowScript = new EventDescriptor(125, 1, 0, 4, 0, 0, 513L);
		ProjectSystemPublishExecuteScript = new EventDescriptor(126, 1, 0, 4, 0, 0, 513L);
		ProjectSystemPublishResults = new EventDescriptor(127, 1, 0, 4, 0, 0, 513L);
		QueryResultExecuteQuery = new EventDescriptor(128, 1, 16, 4, 0, 0, -9223372036854710271L);
		QueryResultCreateScript = new EventDescriptor(129, 1, 16, 4, 0, 0, -9223372036854710271L);
		QueryResultsLoaded = new EventDescriptor(130, 1, 16, 4, 0, 0, -9223372036854710271L);
		ProjectSystemImportSnapshot = new EventDescriptor(131, 1, 16, 4, 0, 0, -9223372036854644735L);
		FileOpen = new EventDescriptor(150, 1, 0, 4, 0, 3, 33281L);
		LoadTSqlDocData = new EventDescriptor(151, 1, 0, 4, 0, 12, 32769L);
		TSqlEditorFrameCreate = new EventDescriptor(152, 1, 0, 1, 0, 12, 32769L);
		TSqlEditorActivate = new EventDescriptor(153, 1, 0, 1, 0, 12, 32769L);
		TSqlEditorTabSwitch = new EventDescriptor(154, 1, 0, 1, 0, 12, 32769L);
		TSqlEditorLaunch = new EventDescriptor(155, 1, 0, 1, 0, 12, 32769L);
		TSqlOnlineEditorDocumentLoad = new EventDescriptor(156, 1, 16, 4, 0, 0, -9223372036854743039L);
		ServerExplorerServerPropertiesRetrieved = new EventDescriptor(200, 1, 0, 4, 0, 0, 513L);
		GotoDefinition = new EventDescriptor(300, 1, 16, 1, 0, 0, -9223372036854513151L);
		FindAllReferences = new EventDescriptor(301, 1, 16, 1, 0, 0, -9223372036854513151L);
		Refactor = new EventDescriptor(302, 1, 16, 1, 0, 0, -9223372036854513151L);
		RefactorContributeChanges = new EventDescriptor(303, 1, 16, 1, 0, 0, -9223372036854513151L);
		RefactorApplyChanges = new EventDescriptor(304, 1, 16, 1, 0, 0, -9223372036854513151L);
	}



	public static EventProviderVersionTwo S_EventProvider;

	private static EventDescriptor SchemaCompare;

	private static EventDescriptor SchemaCompareError;

	private static EventDescriptor SqlEditorExecute;

	private static EventDescriptor ProjectLoad;

	private static EventDescriptor ProjectOpen;

	private static EventDescriptor ProjectWizardImportSchemaFinish;

	private static EventDescriptor ProjectBuild;

	private static EventDescriptor ProjectBuildError;

	private static EventDescriptor ProjectDeploy;

	private static EventDescriptor DisplayAdapterSchemaObjectChangeDone;

	private static EventDescriptor SchemaViewNodePopulationComplete;

	private static EventDescriptor ConnectionStringPersistedInRegistry;

	private static EventDescriptor DataSchemaModelRecycle;

	private static EventDescriptor ImportSchema;

	private static EventDescriptor ImportSchemaFinish;

	private static EventDescriptor ImportSchemaFinishError;

	private static EventDescriptor ImportSchemaGenerateAllScripts;

	private static EventDescriptor ImportSchemaGenerateSingleScript;

	private static EventDescriptor ImportSchemaAddAllScriptsToProject;

	private static EventDescriptor ImportSchemaAddSingleScriptToProject;

	private static EventDescriptor ImportSchemaGenerateProjectMapForType;

	private static EventDescriptor ImportSchemaGenerateProjectMapForElement;

	private static EventDescriptor ImportSchemaAddScriptsToProjectForType;

	private static EventDescriptor ImportScript;

	private static EventDescriptor ModelCompare;

	private static EventDescriptor Commit;

	private static EventDescriptor LogCritical;

	private static EventDescriptor LogError;

	private static EventDescriptor LogWarning;

	private static EventDescriptor LogInformational;

	private static EventDescriptor LogVerbose;

	private static EventDescriptor TableDesignerUpdateContextView;

	private static EventDescriptor TableDesignerAddNewTable;

	private static EventDescriptor TableDesignerOpenTable;

	private static EventDescriptor TableDesignerSpecifyTableProperties;

	private static EventDescriptor TableDesignerAddColumns;

	private static EventDescriptor TableDesignerAddObjectFromCtxPane;

	private static EventDescriptor TableDesignerRefactorRename;

	private static EventDescriptor TableDesignerDeleteColumns;

	private static EventDescriptor SchemaCompareDataPopulationJob;

	private static EventDescriptor SchemaCompareDataPopulationCancel;

	private static EventDescriptor SchemaCompareScriptPopulationJob;

	private static EventDescriptor SchemaCompareScriptPopulationCancel;

	private static EventDescriptor SchemaCompareGetAndResolveDataSchemaModel;

	private static EventDescriptor SchemaCompareModelCompare;

	private static EventDescriptor SchemaCompareGenerateVisual;

	private static EventDescriptor SchemaCompareUpdateTargetJob;

	private static EventDescriptor SchemaCompareUpdateTargetCancel;

	private static EventDescriptor ProjectSystemSnapshot;

	private static EventDescriptor ProjectSystemSnapshotBuildFailed;

	private static EventDescriptor ProjectSystemPublishing;

	private static EventDescriptor ProjectSystemPublishCreateDeploymentPlan;

	private static EventDescriptor ProjectSystemPublishCreatePublishScripts;

	private static EventDescriptor ProjectSystemPublishShowScript;

	private static EventDescriptor ProjectSystemPublishExecuteScript;

	private static EventDescriptor ProjectSystemPublishResults;

	private static EventDescriptor QueryResultExecuteQuery;

	private static EventDescriptor QueryResultCreateScript;

	private static EventDescriptor QueryResultsLoaded;

	private static EventDescriptor ProjectSystemImportSnapshot;

	private static EventDescriptor FileOpen;

	private static EventDescriptor LoadTSqlDocData;

	private static EventDescriptor TSqlEditorFrameCreate;

	private static EventDescriptor TSqlEditorActivate;

	private static EventDescriptor TSqlEditorTabSwitch;

	private static EventDescriptor TSqlEditorLaunch;

	private static EventDescriptor TSqlOnlineEditorDocumentLoad;

	private static EventDescriptor ServerExplorerServerPropertiesRetrieved;

	private static EventDescriptor GotoDefinition;

	private static EventDescriptor FindAllReferences;

	private static EventDescriptor Refactor;

	private static EventDescriptor RefactorContributeChanges;

	private static EventDescriptor RefactorApplyChanges;



	public static bool IsEnabled()
	{
		return S_EventProvider.IsEnabled();
	}

	public static bool IsEnabled(byte level, long keywords)
	{
		return S_EventProvider.IsEnabled(level, keywords);
	}

	public static bool IsLoggingEnabled(EnWinEventTracingLevel level)
	{
		return S_EventProvider.IsEnabled((byte)level, LogCritical.Keywords);
	}


	public static bool EventWriteSchemaCompare(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref SchemaCompare, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareError(string message)
	{
		return S_EventProvider.WriteEvent(ref SchemaCompareError, message);
	}

	public static bool EventWriteSqlEditorExecute(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref SqlEditorExecute, IsStart, EventContext);
	}

	public static bool EventWriteProjectLoad(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ProjectLoad, IsStart, EventContext);
	}

	public static bool EventWriteProjectOpen(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ProjectOpen, IsStart, EventContext);
	}

	public static bool EventWriteProjectWizardImportSchemaFinish(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ProjectWizardImportSchemaFinish, IsStart, EventContext);
	}

	public static bool EventWriteProjectBuild(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ProjectBuild, IsStart, EventContext);
	}

	public static bool EventWriteProjectBuildError(string message)
	{
		return S_EventProvider.WriteEvent(ref ProjectBuildError, message);
	}

	public static bool EventWriteProjectDeploy(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ProjectDeploy, IsStart, EventContext);
	}

	public static bool EventWriteDisplayAdapterSchemaObjectChangeDone(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref DisplayAdapterSchemaObjectChangeDone, IsStart, EventContext);
	}

	public static bool EventWriteSchemaViewNodePopulationComplete(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref SchemaViewNodePopulationComplete, IsStart, EventContext);
	}

	public static bool EventWriteConnectionStringPersistedInRegistry(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ConnectionStringPersistedInRegistry, IsStart, EventContext);
	}

	public static bool EventWriteDataSchemaModelRecycle(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref DataSchemaModelRecycle, IsStart, EventContext);
	}

	public static bool EventWriteImportSchema(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportSchema, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaFinish(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportSchemaFinish, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaFinishError(string message)
	{
		return S_EventProvider.WriteEvent(ref ImportSchemaFinishError, message);
	}

	public static bool EventWriteImportSchemaGenerateAllScripts(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportSchemaGenerateAllScripts, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaGenerateSingleScript(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportSchemaGenerateSingleScript, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaAddAllScriptsToProject(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportSchemaAddAllScriptsToProject, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaAddSingleScriptToProject(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportSchemaAddSingleScriptToProject, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaGenerateProjectMapForType(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportSchemaGenerateProjectMapForType, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaGenerateProjectMapForElement(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportSchemaGenerateProjectMapForElement, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaAddScriptsToProjectForType(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportSchemaAddScriptsToProjectForType, IsStart, EventContext);
	}

	public static bool EventWriteImportScript(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ImportScript, IsStart, EventContext);
	}

	public static bool EventWriteModelCompare(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref ModelCompare, IsStart, EventContext);
	}

	public static bool EventWriteCommit(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref Commit, IsStart);
	}

	public static bool EventWriteLogCritical(uint traceId, string message)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateLoggingMessage(ref LogCritical, traceId, message);
	}

	public static bool EventWriteLogError(uint traceId, string message)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateLoggingMessage(ref LogError, traceId, message);
	}

	public static bool EventWriteLogWarning(uint traceId, string message)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateLoggingMessage(ref LogWarning, traceId, message);
	}

	public static bool EventWriteLogInformational(uint traceId, string message)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateLoggingMessage(ref LogInformational, traceId, message);
	}

	public static bool EventWriteLogVerbose(uint traceId, string message)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateLoggingMessage(ref LogVerbose, traceId, message);
	}

	public static bool EventWriteTableDesignerUpdateContextView(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref TableDesignerUpdateContextView, IsStart);
	}

	public static bool EventWriteTableDesignerAddNewTable(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref TableDesignerAddNewTable, IsStart);
	}

	public static bool EventWriteTableDesignerOpenTable(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref TableDesignerOpenTable, IsStart);
	}

	public static bool EventWriteTableDesignerSpecifyTableProperties(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref TableDesignerSpecifyTableProperties, IsStart);
	}

	public static bool EventWriteTableDesignerAddColumns(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref TableDesignerAddColumns, IsStart);
	}

	public static bool EventWriteTableDesignerAddObjectFromCtxPane(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref TableDesignerAddObjectFromCtxPane, IsStart);
	}

	public static bool EventWriteTableDesignerRefactorRename(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref TableDesignerRefactorRename, IsStart);
	}

	public static bool EventWriteTableDesignerDeleteColumns(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref TableDesignerDeleteColumns, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareDataPopulationJob(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref SchemaCompareDataPopulationJob, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareDataPopulationCancel(string message)
	{
		return S_EventProvider.WriteEvent(ref SchemaCompareDataPopulationCancel, message);
	}

	public static bool EventWriteSchemaCompareScriptPopulationJob(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref SchemaCompareScriptPopulationJob, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareScriptPopulationCancel(string message)
	{
		return S_EventProvider.WriteEvent(ref SchemaCompareScriptPopulationCancel, message);
	}

	public static bool EventWriteSchemaCompareGetAndResolveDataSchemaModel(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref SchemaCompareGetAndResolveDataSchemaModel, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareModelCompare(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref SchemaCompareModelCompare, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareGenerateVisual(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref SchemaCompareGenerateVisual, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareUpdateTargetJob(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref SchemaCompareUpdateTargetJob, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareUpdateTargetCancel(string message)
	{
		return S_EventProvider.WriteEvent(ref SchemaCompareUpdateTargetCancel, message);
	}

	public static bool EventWriteProjectSystemSnapshot(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref ProjectSystemSnapshot, IsStart);
	}

	public static bool EventWriteProjectSystemSnapshotBuildFailed()
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEventDescriptor(ref ProjectSystemSnapshotBuildFailed);
	}

	public static bool EventWriteProjectSystemPublishing(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref ProjectSystemPublishing, IsStart);
	}

	public static bool EventWriteProjectSystemPublishCreateDeploymentPlan(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref ProjectSystemPublishCreateDeploymentPlan, IsStart);
	}

	public static bool EventWriteProjectSystemPublishCreatePublishScripts(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref ProjectSystemPublishCreatePublishScripts, IsStart);
	}

	public static bool EventWriteProjectSystemPublishShowScript(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref ProjectSystemPublishShowScript, IsStart);
	}

	public static bool EventWriteProjectSystemPublishExecuteScript(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref ProjectSystemPublishExecuteScript, IsStart);
	}

	public static bool EventWriteProjectSystemPublishResults(string message)
	{
		return S_EventProvider.WriteEvent(ref ProjectSystemPublishResults, message);
	}

	public static bool EventWriteQueryResultExecuteQuery(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref QueryResultExecuteQuery, IsStart);
	}

	public static bool EventWriteQueryResultCreateScript(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref QueryResultCreateScript, IsStart);
	}

	public static bool EventWriteQueryResultsLoaded()
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEventDescriptor(ref QueryResultsLoaded);
	}

	public static bool EventWriteProjectSystemImportSnapshot(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref ProjectSystemImportSnapshot, IsStart);
	}

	public static bool EventWriteFileOpen(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref FileOpen, IsStart, EventContext);
	}

	public static bool EventWriteLoadTSqlDocData(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref LoadTSqlDocData, IsStart, EventContext);
	}

	public static bool EventWriteTSqlEditorFrameCreate(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref TSqlEditorFrameCreate, IsStart, EventContext);
	}

	public static bool EventWriteTSqlEditorActivate(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref TSqlEditorActivate, IsStart, EventContext);
	}

	public static bool EventWriteTSqlEditorTabSwitch(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref TSqlEditorTabSwitch, IsStart, EventContext);
	}

	public static bool EventWriteTSqlEditorLaunch(bool IsStart, string EventContext)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateGenericBeginEndMessage(ref TSqlEditorLaunch, IsStart, EventContext);
	}

	public static bool EventWriteTSqlOnlineEditorDocumentLoad(string message)
	{
		return S_EventProvider.WriteEvent(ref TSqlOnlineEditorDocumentLoad, message);
	}

	public static bool EventWriteServerExplorerServerPropertiesRetrieved(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref ServerExplorerServerPropertiesRetrieved, IsStart);
	}

	public static bool EventWriteGotoDefinition(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref GotoDefinition, IsStart);
	}

	public static bool EventWriteFindAllReferences(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref FindAllReferences, IsStart);
	}

	public static bool EventWriteRefactor(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref Refactor, IsStart);
	}

	public static bool EventWriteRefactorContributeChanges(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref RefactorContributeChanges, IsStart);
	}

	public static bool EventWriteRefactorApplyChanges(bool IsStart)
	{
		if (!S_EventProvider.IsEnabled())
		{
			return true;
		}
		return S_EventProvider.TemplateEmptyBeginEndMessage(ref RefactorApplyChanges, IsStart);
	}
}
