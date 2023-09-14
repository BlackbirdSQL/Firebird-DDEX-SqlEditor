// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Schema.Common.Diagnostics.EtwProvider

using System;
using System.Diagnostics.Eventing;
using BlackbirdSql.Core.Ctl.Enums;


namespace BlackbirdSql.Core.Ctl.Diagnostics;


internal static class EtwProvider
{

	private static readonly bool _EtwLoggingEnabled;

	internal static SqlEventProvider _Provider;

	// private static Guid _ReverseEngineerId;

	// private static Guid _CoreModelId;

	private static EventDescriptor _Populate;

	private static EventDescriptor _ExecutePopulator;

	private static EventDescriptor _ExecuteComposedPopulator;

	private static EventDescriptor _DeleteElements;

	private static EventDescriptor _SchemaCompare;

	private static EventDescriptor _SchemaCompareError;

	private static EventDescriptor _SqlEditorExecute;

	private static EventDescriptor _ProjectLoad;

	private static EventDescriptor _ProjectOpen;

	private static EventDescriptor _ProjectWizardImportSchemaFinish;

	private static EventDescriptor _ProjectBuild;

	private static EventDescriptor _ProjectBuildError;

	private static EventDescriptor _ProjectDeploy;

	private static EventDescriptor _DeploymentExecute;

	private static EventDescriptor _DeploymentFailure;

	private static EventDescriptor _DeploymentError;

	private static EventDescriptor _DisplayAdapterSchemaObjectChangeDone;

	private static EventDescriptor _SchemaViewNodePopulationComplete;

	private static EventDescriptor _ConnectionStringPersistedInRegistry;

	private static EventDescriptor _DataSchemaModelRecycle;

	private static EventDescriptor _ModelStoreQueryExecutionTimes;

	private static EventDescriptor _ModelStoreFileSizeOnDispose;

	private static EventDescriptor _ReverseEngineerPopulateAll;

	private static EventDescriptor _ReverseEngineerPopulateSingle;

	private static EventDescriptor _ReverseEngineerPopulateChildren;

	private static EventDescriptor _ReverseEngineerExecuteReader;

	private static EventDescriptor _ReverseEngineerElementsPopulated;

	private static EventDescriptor _ImportSchema;

	private static EventDescriptor _ImportSchemaFinish;

	private static EventDescriptor _ImportSchemaFinishError;

	private static EventDescriptor _ImportSchemaGenerateAllScripts;

	private static EventDescriptor _ImportSchemaGenerateSingleScript;

	private static EventDescriptor _ImportSchemaAddAllScriptsToProject;

	private static EventDescriptor _ImportSchemaAddSingleScriptToProject;

	private static EventDescriptor _ImportSchemaGenerateProjectMapForType;

	private static EventDescriptor _ImportSchemaGenerateProjectMapForElement;

	private static EventDescriptor _ImportSchemaAddScriptsToProjectForType;

	private static EventDescriptor _ImportScript;

	private static EventDescriptor _ModelProcessingTasks;

	private static EventDescriptor _ResolveAll;

	private static EventDescriptor _ResolveBatch;

	private static EventDescriptor _SingleTaskProcessAll;

	private static EventDescriptor _SingleTaskProcessBatch;

	private static EventDescriptor _ModelBuilder;

	private static EventDescriptor _ParseAndInterpret;

	private static EventDescriptor _Parse;

	private static EventDescriptor _Interpret;

	private static EventDescriptor _InterpretError;

	private static EventDescriptor _InterpretCritical;

	private static EventDescriptor _AnalyzeIdentifiedElement;

	private static EventDescriptor _AnalyzeIdentifiedRelationship;

	private static EventDescriptor _AnalyzeIdentifiedRelationshipError;

	private static EventDescriptor _AnalyzeIdentifiedSupportingStatement;

	private static EventDescriptor _AnalyzeIdentifiedAmbiguousRelationship;

	private static EventDescriptor _AnalyzeIdentifiedAmbiguousRelationshipError;

	private static EventDescriptor _SemanticVerification;

	private static EventDescriptor _SerializationWriteStore;

	private static EventDescriptor _SerializationWriteElement;

	private static EventDescriptor _SerializationWriteProperties;

	private static EventDescriptor _SerializationWriteRelationship;

	private static EventDescriptor _SerializationWriteAnnotations;

	private static EventDescriptor _SerializationWriteStoreAnnotations;

	private static EventDescriptor _SerializationWriteRelationshipEntryPeer;

	private static EventDescriptor _SerializationWriteRelationshipEntryComposing;

	private static EventDescriptor _SerializationGetDisambiguatorMap;

	private static EventDescriptor _SerializationGetRootElements;

	private static EventDescriptor _SerializationGetExternalSourceExternalName;

	private static EventDescriptor _DataSchemaModelSerialization;

	private static EventDescriptor _DataSchemaModelDeserialization;

	private static EventDescriptor _DataSchemaModelDeserializationError;

	private static EventDescriptor _ModelCompare;

	private static EventDescriptor _Commit;

	private static EventDescriptor _BuildDac;

	private static EventDescriptor _RunValidationRule;

	private static EventDescriptor _RunExtendedValidation;

	private static EventDescriptor _LogCritical;

	private static EventDescriptor _LogError;

	private static EventDescriptor _LogWarning;

	private static EventDescriptor _LogInformational;

	private static EventDescriptor _LogVerbose;

	private static EventDescriptor _TableDesignerUpdateContextView;

	private static EventDescriptor _TableDesignerAddNewTable;

	private static EventDescriptor _TableDesignerOpenTable;

	private static EventDescriptor _TableDesignerSpecifyTableProperties;

	private static EventDescriptor _TableDesignerAddColumns;

	private static EventDescriptor _TableDesignerAddObjectFromCtxPane;

	private static EventDescriptor _TableDesignerRefactorRename;

	private static EventDescriptor _TableDesignerDeleteColumns;

	private static EventDescriptor _SchemaCompareDataPopulationJob;

	private static EventDescriptor _SchemaCompareDataPopulationCancel;

	private static EventDescriptor _SchemaCompareScriptPopulationJob;

	private static EventDescriptor _SchemaCompareScriptPopulationCancel;

	private static EventDescriptor _SchemaCompareGetAndResolveDataSchemaModel;

	private static EventDescriptor _SchemaCompareModelCompare;

	private static EventDescriptor _SchemaCompareGenerateVisual;

	private static EventDescriptor _SchemaCompareUpdateTargetJob;

	private static EventDescriptor _SchemaCompareUpdateTargetCancel;

	private static EventDescriptor _ProjectSystemSnapshot;

	private static EventDescriptor _ProjectSystemSnapshotBuildFailed;

	private static EventDescriptor _ProjectSystemPublishing;

	private static EventDescriptor _ProjectSystemPublishCreateDeploymentPlan;

	private static EventDescriptor _ProjectSystemPublishCreatePublishScripts;

	private static EventDescriptor _ProjectSystemPublishShowScript;

	private static EventDescriptor _ProjectSystemPublishExecuteScript;

	private static EventDescriptor _ProjectSystemPublishResults;

	private static EventDescriptor _QueryResultExecuteQuery;

	private static EventDescriptor _QueryResultCreateScript;

	private static EventDescriptor _QueryResultsLoaded;

	private static EventDescriptor _ProjectSystemImportSnapshot;

	private static EventDescriptor _FileOpen;

	private static EventDescriptor _LoadTSqlDocData;

	private static EventDescriptor _TSqlEditorFrameCreate;

	private static EventDescriptor _TSqlEditorActivate;

	private static EventDescriptor _TSqlEditorTabSwitch;

	private static EventDescriptor _TSqlEditorLaunch;

	private static EventDescriptor _TSqlOnlineEditorDocumentLoad;

	private static EventDescriptor _ServerExplorerServerPropertiesRetrieved;

	private static EventDescriptor _GotoDefinition;

	private static EventDescriptor _FindAllReferences;

	private static EventDescriptor _Refactor;

	private static EventDescriptor _RefactorContributeChanges;

	private static EventDescriptor _RefactorApplyChanges;

	private static bool GetIsEtwEnabled()
	{
		try
		{
			return Environment.OSVersion.Version.Major > 5;
		}
		catch (InvalidOperationException)
		{
			return false;
		}
	}

	public static bool IsEnabled()
	{
		if (_EtwLoggingEnabled && _Provider != null)
		{
			return _Provider.IsEnabled();
		}

		return false;
	}

	public static bool IsEnabled(byte level, long keywords)
	{
		if (_EtwLoggingEnabled && _Provider != null)
		{
			return _Provider.IsEnabled(level, keywords);
		}

		return false;
	}

	public static bool IsModelStoreQueryExecutionTimesEnabled()
	{
		return IsEnabled(_ModelStoreQueryExecutionTimes.Level, _ModelStoreQueryExecutionTimes.Keywords);
	}

	public static bool IsExecutePopulatorEnabled()
	{
		return IsEnabled(_ExecutePopulator.Level, _ExecutePopulator.Keywords);
	}

	public static bool IsModelStoreFileSizeOnDisposeEnabled()
	{
		return IsEnabled(_ModelStoreFileSizeOnDispose.Level, _ModelStoreFileSizeOnDispose.Keywords);
	}

	public static bool IsSingleTaskProcessBatchEnabled()
	{
		return IsEnabled(_SingleTaskProcessBatch.Level, _SingleTaskProcessBatch.Keywords);
	}

	public static bool IsExecuteComposedPopulatorEnabled()
	{
		return IsEnabled(_ExecuteComposedPopulator.Level, _ExecuteComposedPopulator.Keywords);
	}

	public static bool IsLoggingEnabled(EnWinEventTracingLevel level)
	{
		return IsEnabled((byte)level, _LogCritical.Keywords);
	}

	static EtwProvider()
	{
		_EtwLoggingEnabled = GetIsEtwEnabled();
		_Provider = new SqlEventProvider(new Guid(SystemData.DslEventProviderGuid));
		// _ReverseEngineerId = new Guid("0c270cbc-3e29-449d-8a86-97dd3efb008b");
		// _CoreModelId = new Guid("ee88552a-6edb-48c3-8730-47e357196a05");
		_Populate = new EventDescriptor(0, 1, 0, 1, 11, 1, 5L);
		_ExecutePopulator = new EventDescriptor(1, 1, 0, 4, 11, 1, 5L);
		_ExecuteComposedPopulator = new EventDescriptor(2, 1, 0, 5, 11, 1, 5L);
		_DeleteElements = new EventDescriptor(3, 1, 0, 5, 11, 1, 5L);
		_SchemaCompare = new EventDescriptor(4, 1, 0, 4, 0, 3, 513L);
		_SchemaCompareError = new EventDescriptor(69, 1, 0, 2, 0, 3, 513L);
		_SqlEditorExecute = new EventDescriptor(5, 1, 0, 4, 0, 3, 513L);
		_ProjectLoad = new EventDescriptor(6, 1, 0, 4, 0, 3, 513L);
		_ProjectOpen = new EventDescriptor(7, 1, 0, 4, 0, 3, 513L);
		_ProjectWizardImportSchemaFinish = new EventDescriptor(8, 1, 0, 4, 0, 3, 513L);
		_ProjectBuild = new EventDescriptor(9, 1, 0, 1, 0, 3, 513L);
		_ProjectBuildError = new EventDescriptor(70, 1, 0, 2, 0, 3, 513L);
		_ProjectDeploy = new EventDescriptor(10, 1, 0, 1, 0, 3, 513L);
		_DeploymentExecute = new EventDescriptor(11, 1, 0, 4, 0, 3, 513L);
		_DeploymentFailure = new EventDescriptor(12, 1, 0, 4, 0, 3, 513L);
		_DeploymentError = new EventDescriptor(13, 1, 0, 2, 0, 3, 513L);
		_DisplayAdapterSchemaObjectChangeDone = new EventDescriptor(14, 1, 0, 4, 0, 4, 1025L);
		_SchemaViewNodePopulationComplete = new EventDescriptor(15, 1, 0, 4, 0, 4, 1025L);
		_ConnectionStringPersistedInRegistry = new EventDescriptor(16, 1, 0, 4, 0, 4, 1025L);
		_DataSchemaModelRecycle = new EventDescriptor(17, 1, 0, 4, 0, 4, 1025L);
		_ModelStoreQueryExecutionTimes = new EventDescriptor(18, 1, 0, 4, 0, 4, 1025L);
		_ModelStoreFileSizeOnDispose = new EventDescriptor(19, 1, 0, 4, 0, 4, 1025L);
		_ReverseEngineerPopulateAll = new EventDescriptor(20, 1, 0, 4, 0, 1, 5L);
		_ReverseEngineerPopulateSingle = new EventDescriptor(21, 1, 0, 4, 0, 1, 5L);
		_ReverseEngineerPopulateChildren = new EventDescriptor(22, 1, 0, 4, 0, 1, 5L);
		_ReverseEngineerExecuteReader = new EventDescriptor(23, 1, 0, 4, 0, 1, 5L);
		_ReverseEngineerElementsPopulated = new EventDescriptor(24, 1, 0, 4, 0, 1, 5L);
		_ImportSchema = new EventDescriptor(25, 1, 0, 4, 0, 5, 9L);
		_ImportSchemaFinish = new EventDescriptor(26, 1, 0, 4, 0, 5, 9L);
		_ImportSchemaFinishError = new EventDescriptor(68, 1, 0, 4, 0, 5, 9L);
		_ImportSchemaGenerateAllScripts = new EventDescriptor(27, 1, 0, 4, 0, 5, 9L);
		_ImportSchemaGenerateSingleScript = new EventDescriptor(28, 1, 0, 4, 0, 5, 9L);
		_ImportSchemaAddAllScriptsToProject = new EventDescriptor(29, 1, 0, 4, 0, 5, 9L);
		_ImportSchemaAddSingleScriptToProject = new EventDescriptor(30, 1, 0, 4, 0, 5, 9L);
		_ImportSchemaGenerateProjectMapForType = new EventDescriptor(31, 1, 0, 4, 0, 5, 9L);
		_ImportSchemaGenerateProjectMapForElement = new EventDescriptor(32, 1, 0, 4, 0, 5, 9L);
		_ImportSchemaAddScriptsToProjectForType = new EventDescriptor(33, 1, 0, 4, 0, 5, 9L);
		_ImportScript = new EventDescriptor(34, 1, 0, 4, 0, 6, 17L);
		_ModelProcessingTasks = new EventDescriptor(35, 1, 0, 4, 0, 7, 33L);
		_ResolveAll = new EventDescriptor(36, 1, 0, 1, 0, 7, 33L);
		_ResolveBatch = new EventDescriptor(37, 1, 0, 4, 0, 7, 33L);
		_SingleTaskProcessAll = new EventDescriptor(38, 1, 0, 4, 0, 7, 33L);
		_SingleTaskProcessBatch = new EventDescriptor(39, 1, 0, 4, 0, 7, 33L);
		_ModelBuilder = new EventDescriptor(40, 1, 0, 4, 0, 8, 65L);
		_ParseAndInterpret = new EventDescriptor(41, 1, 0, 4, 0, 8, 65L);
		_Parse = new EventDescriptor(42, 1, 0, 4, 0, 8, 65L);
		_Interpret = new EventDescriptor(43, 1, 0, 4, 0, 8, 65L);
		_InterpretError = new EventDescriptor(44, 1, 0, 2, 0, 8, 65L);
		_InterpretCritical = new EventDescriptor(45, 1, 0, 1, 0, 8, 65L);
		_AnalyzeIdentifiedElement = new EventDescriptor(46, 1, 0, 4, 0, 8, 65L);
		_AnalyzeIdentifiedRelationship = new EventDescriptor(47, 1, 0, 4, 0, 8, 65L);
		_AnalyzeIdentifiedRelationshipError = new EventDescriptor(48, 1, 0, 2, 0, 8, 65L);
		_AnalyzeIdentifiedSupportingStatement = new EventDescriptor(49, 1, 0, 4, 0, 8, 65L);
		_AnalyzeIdentifiedAmbiguousRelationship = new EventDescriptor(50, 1, 0, 4, 0, 8, 65L);
		_AnalyzeIdentifiedAmbiguousRelationshipError = new EventDescriptor(51, 1, 0, 2, 0, 8, 65L);
		_SemanticVerification = new EventDescriptor(52, 1, 0, 1, 0, 8, 65L);
		_SerializationWriteStore = new EventDescriptor(53, 1, 0, 4, 0, 9, 129L);
		_SerializationWriteElement = new EventDescriptor(54, 1, 0, 4, 0, 9, 129L);
		_SerializationWriteProperties = new EventDescriptor(55, 1, 0, 4, 0, 9, 129L);
		_SerializationWriteRelationship = new EventDescriptor(56, 1, 0, 4, 0, 9, 129L);
		_SerializationWriteAnnotations = new EventDescriptor(57, 1, 0, 4, 0, 9, 129L);
		_SerializationWriteStoreAnnotations = new EventDescriptor(58, 1, 0, 4, 0, 9, 129L);
		_SerializationWriteRelationshipEntryPeer = new EventDescriptor(59, 1, 0, 4, 0, 9, 129L);
		_SerializationWriteRelationshipEntryComposing = new EventDescriptor(60, 1, 0, 4, 0, 9, 129L);
		_SerializationGetDisambiguatorMap = new EventDescriptor(61, 1, 0, 4, 0, 9, 129L);
		_SerializationGetRootElements = new EventDescriptor(62, 1, 0, 4, 0, 9, 129L);
		_SerializationGetExternalSourceExternalName = new EventDescriptor(63, 1, 0, 4, 0, 9, 129L);
		_DataSchemaModelSerialization = new EventDescriptor(64, 1, 0, 4, 0, 9, 129L);
		_DataSchemaModelDeserialization = new EventDescriptor(65, 1, 0, 4, 0, 9, 129L);
		_DataSchemaModelDeserializationError = new EventDescriptor(66, 1, 0, 2, 0, 9, 129L);
		_ModelCompare = new EventDescriptor(67, 1, 0, 4, 0, 10, 257L);
		_Commit = new EventDescriptor(87, 1, 0, 4, 0, 0, 1L);
		_BuildDac = new EventDescriptor(76, 1, 0, 1, 0, 0, 1L);
		_RunValidationRule = new EventDescriptor(77, 1, 0, 4, 0, 8, 65L);
		_RunExtendedValidation = new EventDescriptor(78, 1, 0, 4, 0, 3, 513L);
		_LogCritical = new EventDescriptor(71, 1, 0, 1, 0, 11, 2048L);
		_LogError = new EventDescriptor(72, 1, 0, 2, 0, 11, 2048L);
		_LogWarning = new EventDescriptor(73, 1, 0, 3, 0, 11, 2048L);
		_LogInformational = new EventDescriptor(74, 1, 0, 4, 0, 11, 2048L);
		_LogVerbose = new EventDescriptor(75, 1, 0, 5, 0, 11, 2048L);
		_TableDesignerUpdateContextView = new EventDescriptor(79, 1, 16, 4, 0, 0, -9223372036854771711L);
		_TableDesignerAddNewTable = new EventDescriptor(80, 1, 0, 4, 0, 0, 4097L);
		_TableDesignerOpenTable = new EventDescriptor(81, 1, 0, 4, 0, 0, 4097L);
		_TableDesignerSpecifyTableProperties = new EventDescriptor(82, 1, 0, 4, 0, 0, 4097L);
		_TableDesignerAddColumns = new EventDescriptor(83, 1, 0, 1, 0, 0, 4097L);
		_TableDesignerAddObjectFromCtxPane = new EventDescriptor(84, 1, 0, 4, 0, 0, 4097L);
		_TableDesignerRefactorRename = new EventDescriptor(85, 1, 0, 4, 0, 0, 4097L);
		_TableDesignerDeleteColumns = new EventDescriptor(86, 1, 0, 4, 0, 0, 4097L);
		_SchemaCompareDataPopulationJob = new EventDescriptor(88, 1, 0, 4, 0, 0, 16385L);
		_SchemaCompareDataPopulationCancel = new EventDescriptor(89, 1, 0, 4, 0, 0, 16385L);
		_SchemaCompareScriptPopulationJob = new EventDescriptor(90, 1, 0, 4, 0, 0, 16385L);
		_SchemaCompareScriptPopulationCancel = new EventDescriptor(91, 1, 0, 4, 0, 0, 16385L);
		_SchemaCompareGetAndResolveDataSchemaModel = new EventDescriptor(92, 1, 0, 4, 0, 0, 16385L);
		_SchemaCompareModelCompare = new EventDescriptor(93, 1, 0, 4, 0, 0, 16385L);
		_SchemaCompareGenerateVisual = new EventDescriptor(94, 1, 0, 4, 0, 0, 16385L);
		_SchemaCompareUpdateTargetJob = new EventDescriptor(95, 1, 0, 4, 0, 0, 16385L);
		_SchemaCompareUpdateTargetCancel = new EventDescriptor(96, 1, 0, 4, 0, 0, 16385L);
		_ProjectSystemSnapshot = new EventDescriptor(120, 1, 0, 4, 0, 0, 513L);
		_ProjectSystemSnapshotBuildFailed = new EventDescriptor(121, 1, 0, 4, 0, 0, 513L);
		_ProjectSystemPublishing = new EventDescriptor(122, 1, 0, 4, 0, 0, 513L);
		_ProjectSystemPublishCreateDeploymentPlan = new EventDescriptor(123, 1, 0, 4, 0, 0, 513L);
		_ProjectSystemPublishCreatePublishScripts = new EventDescriptor(124, 1, 0, 4, 0, 0, 513L);
		_ProjectSystemPublishShowScript = new EventDescriptor(125, 1, 0, 4, 0, 0, 513L);
		_ProjectSystemPublishExecuteScript = new EventDescriptor(126, 1, 0, 4, 0, 0, 513L);
		_ProjectSystemPublishResults = new EventDescriptor(127, 1, 0, 4, 0, 0, 513L);
		_QueryResultExecuteQuery = new EventDescriptor(128, 1, 16, 4, 0, 0, -9223372036854710271L);
		_QueryResultCreateScript = new EventDescriptor(129, 1, 16, 4, 0, 0, -9223372036854710271L);
		_QueryResultsLoaded = new EventDescriptor(130, 1, 16, 4, 0, 0, -9223372036854710271L);
		_ProjectSystemImportSnapshot = new EventDescriptor(131, 1, 16, 4, 0, 0, -9223372036854644735L);
		_FileOpen = new EventDescriptor(150, 1, 0, 4, 0, 3, 33281L);
		_LoadTSqlDocData = new EventDescriptor(151, 1, 0, 4, 0, 12, 32769L);
		_TSqlEditorFrameCreate = new EventDescriptor(152, 1, 0, 1, 0, 12, 32769L);
		_TSqlEditorActivate = new EventDescriptor(153, 1, 0, 1, 0, 12, 32769L);
		_TSqlEditorTabSwitch = new EventDescriptor(154, 1, 0, 1, 0, 12, 32769L);
		_TSqlEditorLaunch = new EventDescriptor(155, 1, 0, 1, 0, 12, 32769L);
		_TSqlOnlineEditorDocumentLoad = new EventDescriptor(156, 1, 16, 4, 0, 0, -9223372036854743039L);
		_ServerExplorerServerPropertiesRetrieved = new EventDescriptor(200, 1, 0, 4, 0, 0, 513L);
		_GotoDefinition = new EventDescriptor(300, 1, 16, 1, 0, 0, -9223372036854513151L);
		_FindAllReferences = new EventDescriptor(301, 1, 16, 1, 0, 0, -9223372036854513151L);
		_Refactor = new EventDescriptor(302, 1, 16, 1, 0, 0, -9223372036854513151L);
		_RefactorContributeChanges = new EventDescriptor(303, 1, 16, 1, 0, 0, -9223372036854513151L);
		_RefactorApplyChanges = new EventDescriptor(304, 1, 16, 1, 0, 0, -9223372036854513151L);
	}

	public static bool EventWritePopulate(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _Populate, IsStart);
	}

	public static bool EventWriteExecutePopulator(bool IsStart, string PopulatorName, int numberOfElements)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplatePopulatorMessage(ref _ExecutePopulator, IsStart, PopulatorName, numberOfElements);
	}

	public static bool EventWriteExecuteComposedPopulator(bool IsStart, string PopulatorName, int numberOfElements)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplatePopulatorMessage(ref _ExecuteComposedPopulator, IsStart, PopulatorName, numberOfElements);
	}

	public static bool EventWriteDeleteElements()
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEventDescriptor(ref _DeleteElements);
	}

	public static bool EventWriteSchemaCompare(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SchemaCompare, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareError(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _SchemaCompareError, message);
		}

		return false;
	}

	public static bool EventWriteSqlEditorExecute(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SqlEditorExecute, IsStart, EventContext);
	}

	public static bool EventWriteProjectLoad(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ProjectLoad, IsStart, EventContext);
	}

	public static bool EventWriteProjectOpen(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ProjectOpen, IsStart, EventContext);
	}

	public static bool EventWriteProjectWizardImportSchemaFinish(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ProjectWizardImportSchemaFinish, IsStart, EventContext);
	}

	public static bool EventWriteProjectBuild(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ProjectBuild, IsStart, EventContext);
	}

	public static bool EventWriteProjectBuildError(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _ProjectBuildError, message);
		}

		return false;
	}

	public static bool EventWriteProjectDeploy(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ProjectDeploy, IsStart, EventContext);
	}

	public static bool EventWriteDeploymentExecute(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _DeploymentExecute, IsStart, EventContext);
	}

	public static bool EventWriteDeploymentFailure(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _DeploymentFailure, message);
		}

		return false;
	}

	public static bool EventWriteDeploymentError(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _DeploymentError, IsStart, EventContext);
	}

	public static bool EventWriteDisplayAdapterSchemaObjectChangeDone(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _DisplayAdapterSchemaObjectChangeDone, IsStart, EventContext);
	}

	public static bool EventWriteSchemaViewNodePopulationComplete(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SchemaViewNodePopulationComplete, IsStart, EventContext);
	}

	public static bool EventWriteConnectionStringPersistedInRegistry(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ConnectionStringPersistedInRegistry, IsStart, EventContext);
	}

	public static bool EventWriteDataSchemaModelRecycle(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _DataSchemaModelRecycle, IsStart, EventContext);
	}

	public static bool EventWriteModelStoreQueryExecutionTimes(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ModelStoreQueryExecutionTimes, IsStart, EventContext);
	}

	public static bool EventWriteModelStoreFileSizeOnDispose(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _ModelStoreFileSizeOnDispose, message);
		}

		return false;
	}

	public static bool EventWriteReverseEngineerPopulateAll(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ReverseEngineerPopulateAll, IsStart, EventContext);
	}

	public static bool EventWriteReverseEngineerPopulateSingle(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ReverseEngineerPopulateSingle, IsStart, EventContext);
	}

	public static bool EventWriteReverseEngineerPopulateChildren(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ReverseEngineerPopulateChildren, IsStart, EventContext);
	}

	public static bool EventWriteReverseEngineerExecuteReader(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ReverseEngineerExecuteReader, IsStart, EventContext);
	}

	public static bool EventWriteReverseEngineerElementsPopulated(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ReverseEngineerElementsPopulated, IsStart, EventContext);
	}

	public static bool EventWriteImportSchema(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportSchema, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaFinish(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportSchemaFinish, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaFinishError(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _ImportSchemaFinishError, message);
		}

		return false;
	}

	public static bool EventWriteImportSchemaGenerateAllScripts(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportSchemaGenerateAllScripts, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaGenerateSingleScript(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportSchemaGenerateSingleScript, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaAddAllScriptsToProject(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportSchemaAddAllScriptsToProject, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaAddSingleScriptToProject(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportSchemaAddSingleScriptToProject, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaGenerateProjectMapForType(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportSchemaGenerateProjectMapForType, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaGenerateProjectMapForElement(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportSchemaGenerateProjectMapForElement, IsStart, EventContext);
	}

	public static bool EventWriteImportSchemaAddScriptsToProjectForType(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportSchemaAddScriptsToProjectForType, IsStart, EventContext);
	}

	public static bool EventWriteImportScript(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ImportScript, IsStart, EventContext);
	}

	public static bool EventWriteModelProcessingTasks(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ModelProcessingTasks, IsStart, EventContext);
	}

	public static bool EventWriteResolveAll(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ResolveAll, IsStart);
	}

	public static bool EventWriteResolveBatch(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ResolveBatch, IsStart);
	}

	public static bool EventWriteSingleTaskProcessAll(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SingleTaskProcessAll, IsStart, EventContext);
	}

	public static bool EventWriteSingleTaskProcessBatch(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SingleTaskProcessBatch, IsStart, EventContext);
	}

	public static bool EventWriteModelBuilder(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ModelBuilder, IsStart, EventContext);
	}

	public static bool EventWriteParseAndInterpret(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ParseAndInterpret, IsStart, EventContext);
	}

	public static bool EventWriteParse(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _Parse, IsStart, EventContext);
	}

	public static bool EventWriteInterpret(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _Interpret, IsStart, EventContext);
	}

	public static bool EventWriteInterpretError(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _InterpretError, message);
		}

		return false;
	}

	public static bool EventWriteInterpretCritical(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _InterpretCritical, message);
		}

		return false;
	}

	public static bool EventWriteAnalyzeIdentifiedElement(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _AnalyzeIdentifiedElement, IsStart);
	}

	public static bool EventWriteAnalyzeIdentifiedRelationship(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _AnalyzeIdentifiedRelationship, IsStart);
	}

	public static bool EventWriteAnalyzeIdentifiedRelationshipError()
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEventDescriptor(ref _AnalyzeIdentifiedRelationshipError);
	}

	public static bool EventWriteAnalyzeIdentifiedSupportingStatement(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _AnalyzeIdentifiedSupportingStatement, IsStart);
	}

	public static bool EventWriteAnalyzeIdentifiedAmbiguousRelationship(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _AnalyzeIdentifiedAmbiguousRelationship, IsStart);
	}

	public static bool EventWriteAnalyzeIdentifiedAmbiguousRelationshipError()
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEventDescriptor(ref _AnalyzeIdentifiedAmbiguousRelationshipError);
	}

	public static bool EventWriteSemanticVerification(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SemanticVerification, IsStart, EventContext);
	}

	public static bool EventWriteSerializationWriteStore(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationWriteStore, IsStart, EventContext);
	}

	public static bool EventWriteSerializationWriteElement(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationWriteElement, IsStart, EventContext);
	}

	public static bool EventWriteSerializationWriteProperties(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationWriteProperties, IsStart, EventContext);
	}

	public static bool EventWriteSerializationWriteRelationship(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationWriteRelationship, IsStart, EventContext);
	}

	public static bool EventWriteSerializationWriteAnnotations(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationWriteAnnotations, IsStart, EventContext);
	}

	public static bool EventWriteSerializationWriteStoreAnnotations(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationWriteStoreAnnotations, IsStart, EventContext);
	}

	public static bool EventWriteSerializationWriteRelationshipEntryPeer(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationWriteRelationshipEntryPeer, IsStart, EventContext);
	}

	public static bool EventWriteSerializationWriteRelationshipEntryComposing(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationWriteRelationshipEntryComposing, IsStart, EventContext);
	}

	public static bool EventWriteSerializationGetDisambiguatorMap(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationGetDisambiguatorMap, IsStart, EventContext);
	}

	public static bool EventWriteSerializationGetRootElements(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationGetRootElements, IsStart, EventContext);
	}

	public static bool EventWriteSerializationGetExternalSourceExternalName(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SerializationGetExternalSourceExternalName, IsStart, EventContext);
	}

	public static bool EventWriteDataSchemaModelSerialization(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _DataSchemaModelSerialization, IsStart);
	}

	public static bool EventWriteDataSchemaModelDeserialization(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _DataSchemaModelDeserialization, IsStart, EventContext);
	}

	public static bool EventWriteDataSchemaModelDeserializationError(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _DataSchemaModelDeserializationError, message);
		}

		return false;
	}

	public static bool EventWriteModelCompare(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _ModelCompare, IsStart, EventContext);
	}

	public static bool EventWriteCommit(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _Commit, IsStart);
	}

	public static bool EventWriteBuildDac(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _BuildDac, IsStart);
	}

	public static bool EventWriteRunValidationRule(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _RunValidationRule, IsStart, EventContext);
	}

	public static bool EventWriteRunExtendedValidation(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _RunExtendedValidation, IsStart);
	}

	public static bool EventWriteLogCritical(uint traceId, string message)
	{
		Diag.Stack($"Schema Critical[{traceId}]: {message}");
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateLoggingMessage(ref _LogCritical, traceId, message);
	}

	public static bool EventWriteLogError(uint traceId, string message)
	{
		Diag.Stack($"Schema Error[{traceId}]: {message}");
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateLoggingMessage(ref _LogError, traceId, message);
	}

	public static bool EventWriteLogWarning(uint traceId, string message)
	{
		Diag.Dug(false, $"Schema Warning[{traceId}]: {message}");
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateLoggingMessage(ref _LogWarning, traceId, message);
	}

	public static bool EventWriteLogInformational(uint traceId, string message)
	{
		Diag.Dug(false, $"Schema Info[{traceId}]: {message}");
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateLoggingMessage(ref _LogInformational, traceId, message);
	}

	public static bool EventWriteLogVerbose(uint traceId, string message)
	{
		Diag.Dug(false, $"Schema Verbose[{traceId}]: {message}");
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateLoggingMessage(ref _LogVerbose, traceId, message);
	}

	public static bool EventWriteTableDesignerUpdateContextView(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _TableDesignerUpdateContextView, IsStart);
	}

	public static bool EventWriteTableDesignerAddNewTable(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _TableDesignerAddNewTable, IsStart);
	}

	public static bool EventWriteTableDesignerOpenTable(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _TableDesignerOpenTable, IsStart);
	}

	public static bool EventWriteTableDesignerSpecifyTableProperties(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _TableDesignerSpecifyTableProperties, IsStart);
	}

	public static bool EventWriteTableDesignerAddColumns(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _TableDesignerAddColumns, IsStart);
	}

	public static bool EventWriteTableDesignerAddObjectFromCtxPane(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _TableDesignerAddObjectFromCtxPane, IsStart);
	}

	public static bool EventWriteTableDesignerRefactorRename(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _TableDesignerRefactorRename, IsStart);
	}

	public static bool EventWriteTableDesignerDeleteColumns(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _TableDesignerDeleteColumns, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareDataPopulationJob(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SchemaCompareDataPopulationJob, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareDataPopulationCancel(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _SchemaCompareDataPopulationCancel, message);
		}

		return false;
	}

	public static bool EventWriteSchemaCompareScriptPopulationJob(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SchemaCompareScriptPopulationJob, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareScriptPopulationCancel(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _SchemaCompareScriptPopulationCancel, message);
		}

		return false;
	}

	public static bool EventWriteSchemaCompareGetAndResolveDataSchemaModel(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SchemaCompareGetAndResolveDataSchemaModel, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareModelCompare(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SchemaCompareModelCompare, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareGenerateVisual(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SchemaCompareGenerateVisual, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareUpdateTargetJob(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _SchemaCompareUpdateTargetJob, IsStart, EventContext);
	}

	public static bool EventWriteSchemaCompareUpdateTargetCancel(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _SchemaCompareUpdateTargetCancel, message);
		}

		return false;
	}

	public static bool EventWriteProjectSystemSnapshot(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ProjectSystemSnapshot, IsStart);
	}

	public static bool EventWriteProjectSystemSnapshotBuildFailed()
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEventDescriptor(ref _ProjectSystemSnapshotBuildFailed);
	}

	public static bool EventWriteProjectSystemPublishing(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ProjectSystemPublishing, IsStart);
	}

	public static bool EventWriteProjectSystemPublishCreateDeploymentPlan(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ProjectSystemPublishCreateDeploymentPlan, IsStart);
	}

	public static bool EventWriteProjectSystemPublishCreatePublishScripts(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ProjectSystemPublishCreatePublishScripts, IsStart);
	}

	public static bool EventWriteProjectSystemPublishShowScript(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ProjectSystemPublishShowScript, IsStart);
	}

	public static bool EventWriteProjectSystemPublishExecuteScript(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ProjectSystemPublishExecuteScript, IsStart);
	}

	public static bool EventWriteProjectSystemPublishResults(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _ProjectSystemPublishResults, message);
		}

		return false;
	}

	public static bool EventWriteQueryResultExecuteQuery(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _QueryResultExecuteQuery, IsStart);
	}

	public static bool EventWriteQueryResultCreateScript(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _QueryResultCreateScript, IsStart);
	}

	public static bool EventWriteQueryResultsLoaded()
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEventDescriptor(ref _QueryResultsLoaded);
	}

	public static bool EventWriteProjectSystemImportSnapshot(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ProjectSystemImportSnapshot, IsStart);
	}

	public static bool EventWriteFileOpen(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _FileOpen, IsStart, EventContext);
	}

	public static bool EventWriteLoadTSqlDocData(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _LoadTSqlDocData, IsStart, EventContext);
	}

	public static bool EventWriteTSqlEditorFrameCreate(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _TSqlEditorFrameCreate, IsStart, EventContext);
	}

	public static bool EventWriteTSqlEditorActivate(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _TSqlEditorActivate, IsStart, EventContext);
	}

	public static bool EventWriteTSqlEditorTabSwitch(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _TSqlEditorTabSwitch, IsStart, EventContext);
	}

	public static bool EventWriteTSqlEditorLaunch(bool IsStart, string EventContext)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateGenericBeginEndMessage(ref _TSqlEditorLaunch, IsStart, EventContext);
	}

	public static bool EventWriteTSqlOnlineEditorDocumentLoad(string message)
	{
		if (IsEnabled())
		{
			return _Provider.WriteEvent(ref _TSqlOnlineEditorDocumentLoad, message);
		}

		return false;
	}

	public static bool EventWriteServerExplorerServerPropertiesRetrieved(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _ServerExplorerServerPropertiesRetrieved, IsStart);
	}

	public static bool EventWriteGotoDefinition(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _GotoDefinition, IsStart);
	}

	public static bool EventWriteFindAllReferences(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _FindAllReferences, IsStart);
	}

	public static bool EventWriteRefactor(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _Refactor, IsStart);
	}

	public static bool EventWriteRefactorContributeChanges(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _RefactorContributeChanges, IsStart);
	}

	public static bool EventWriteRefactorApplyChanges(bool IsStart)
	{
		if (!IsEnabled())
		{
			return true;
		}

		return _Provider.TemplateEmptyBeginEndMessage(ref _RefactorApplyChanges, IsStart);
	}
}
