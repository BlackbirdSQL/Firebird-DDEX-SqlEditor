// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio;

namespace BlackbirdSql.Common;


// =========================================================================================================
//											ServiceData Class
//
/// <summary>
/// Contains extension constants for additional core BlackbirdSql services (Editor/Language services).
/// </summary>
// =========================================================================================================
public static class LibraryData
{
	public const string ServiceName = "Blackbird SQL Editor Service";
	public const string ApplicationName = "BlackbirdSql Server Data Tools, FB-SQL Editor";
	public const string ColorServiceName = "Blackbird SQL Color Service";

	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>

	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - LibraryData
	// ---------------------------------------------------------------------------------------------------------

	public const string DslLanguageServiceGuid = "20375AE3-933E-4D15-BF52-833DA09A971F";

	public const string ExceptionMessageBoxParentGuid = "998E39AD-042D-484C-B462-0FB3F6536DE9";


	#endregion Package Guids


	// Private Service and Factory Guids
	public const string ScriptFactoryGuid = "FEECEF69-3E37-4D46-9C27-84E30B7BC802";
	public const string SqlResultsEditorFactoryGuid = "31AD6A1B-B7A2-4B16-AADA-28ADEADF7F2E";
	public const string SqlEditorCommandSetGuid = "13CD7876-FC84-4DDA-91BF-4CDBF893B134";

	/// <summary>
	/// Unique guid key for saving the DbConnectionStringBuilder object parsed from the
	/// ServerExplorer node into IVsUserData. This connection info is added to the FlameRobin list if
	/// it does not exist. Note that the csb will include the parameter 'DatasetKey' which is the string
	/// used in the toolbar dropdown list.
	/// </summary>
	public const string SqlEditorConnectionStringGuid = "EDD5003E-0797-40FF-8ACF-F93ED2A6C059";


	// Private Proffered Services and Object CLSIDs
	public static readonly Guid CLSID_SqlEditorCommandSet = new(SqlEditorCommandSetGuid);
	public static readonly Guid CLSID_EditorMarkerService = new("984A3634-C7DE-41D9-992E-CE35638B513F");
	public static readonly Guid CLSID_FontAndColorService = new("2EF6AAA5-7BAE-452B-AA43-6472FB2FFFFB");


	// Settings Guids
	public const string SqlEditorGeneralSettingsGuid = "FDE3AD9E-51F4-45FB-9706-99A470BD9C53";
	public const string SqlEditorTabAndStatusBarSettingsGuid = "FF344DD3-79C5-4563-B45A-EE07CFA2B8F5";
	public const string SqlExecutionAdvancedOptionsGuid = "7F2B377B-AFEA-4545-BDAC-69E5F348BE66";
	public const string SqlExecutionAnsiOptionsGuid = "CF83838C-B3BE-49D2-9236-361133D09BAB";
	public const string SqlExecutionGeneralOptionsGuid = "E1772E9C-9ECD-4CB6-85B4-DB411E97472A";
	public const string SqlResultsGeneralOptionsGuid = "7B823EE3-B3CC-4423-A37E-DD250EB9233B";
	public const string SqlResultsToGridOptionsGuid = "68ED5512-1DC7-4320-AB4D-F4D883C4DD42";
	public const string SqlResultsToTextOptionsGuid = "71D54055-BA60-4389-AE22-FC2CCB9CF97E";

	// Property Guids
	public static readonly Guid CLSID_PropertyDatabaseChanged = new("D63AB40F-C17E-44a4-8017-0770EEF27FF5");
	public static readonly Guid CLSID_IntelliSenseEnabled = new("097A840C-BDDA-4573-8F6D-671EBB21746D");
	public static readonly Guid CLSID_PropertyDisableXmlEditorPropertyWindowIntegration = new("b8b94ef1-79a4-446a-95bb-002419e4453a");
	public static readonly Guid CLSID_PropertyOverrideXmlEditorSaveAsFileFilter = new("8D88CCA5-7567-4b5c-9CD7-67A3AC136D2D");
	public static readonly Guid CLSID_PropertyOleSql = new("F78AEC67-32DB-445e-B1AA-97BFB5BB5163");
	public static readonly Guid CLSID_PropertySqlVersion = new("C856A011-E8D4-4095-AC48-B46814D9FC2F");


	public const string SqlMessageTabLogicalViewGuid = "32B79718-1118-4BF1-B9DF-4B975328790F";
	public const string SqlExecutionPlanTabLogicalViewGuid = "E313023B-F451-4A3C-9C34-BB16C5CCDB72";
	public const string SqlStatisticsTabLogicalViewGuid = "791399A1-6E21-4A73-9AAA-293B6563C77B";
	public const string SqlTextResultsTabLogicalViewGuid = "04BE2EC0-64F0-4D16-9BD1-5D3C95EC7070";


};