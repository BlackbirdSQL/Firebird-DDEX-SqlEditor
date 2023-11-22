// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.Core.Ctl.CommandProviders;


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
	public const string ApplicationName = "BlackbirdSql Server Data Tools, FB-SQL Editor";
	public const string ColorServiceName = "Blackbird SQL Color Service";

	public const string C_ShowPlanNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";
	public const string C_YukonXmlExecutionPlanColumn = "Firebird_SQL_Server_XML_Showplan";

	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - LibraryData
	// ---------------------------------------------------------------------------------------------------------

	public const string ExceptionMessageBoxParentGuid = "998E39AD-042D-484C-B462-0FB3F6536DE9";


	#endregion Package Guids


	// Private Service and Factory Guids
	public const string SqlResultsEditorFactoryGuid = "31AD6A1B-B7A2-4B16-AADA-28ADEADF7F2E";

	/// <summary>
	/// Unique guid key for saving the DbConnectionStringBuilder object parsed from the
	/// ServerExplorer node into IVsUserData. This connection info is added to the FlameRobin list if
	/// it does not exist. Note that the csb will include the parameter 'DatasetKey' which is the string
	/// used in the toolbar dropdown list.
	/// </summary>
	public const string SqlEditorConnectionStringGuid = "EDD5003E-0797-40FF-8ACF-F93ED2A6C059";


	// Private Proffered Services and Object CLSIDs
	public static readonly Guid CLSID_CommandSet = new(CommandProperties.CommandSetGuid);
	public static readonly Guid CLSID_EditorMarkerService = new("984A3634-C7DE-41D9-992E-CE35638B513F");
	public static readonly Guid CLSID_FontAndColorService = new("2EF6AAA5-7BAE-452B-AA43-6472FB2FFFFB");


	// Property Guids
	public static readonly Guid CLSID_PropertyDatabaseConnectionChanged = new("D63AB40F-C17E-44a4-8017-0770EEF27FF5");
	public static readonly Guid CLSID_IntelliSenseEnabled = new("097A840C-BDDA-4573-8F6D-671EBB21746D");
	public static readonly Guid CLSID_PropertyDisableXmlEditorPropertyWindowIntegration = new("b8b94ef1-79a4-446a-95bb-002419e4453a");
	public static readonly Guid CLSID_PropertyOverrideXmlEditorSaveAsFileFilter = new("8D88CCA5-7567-4b5c-9CD7-67A3AC136D2D");
	public static readonly Guid CLSID_PropertyOleSql = new("F78AEC67-32DB-445e-B1AA-97BFB5BB5163");
	public static readonly Guid CLSID_PropertySqlVersion = new("C856A011-E8D4-4095-AC48-B46814D9FC2F");


	// Tabs
	public const string SqlMessageTabLogicalViewGuid = "32B79718-1118-4BF1-B9DF-4B975328790F";
	public const string SqlExecutionPlanTabLogicalViewGuid = "E313023B-F451-4A3C-9C34-BB16C5CCDB72";
	public const string SqlTextPlanTabLogicalViewGuid = "200E716C-607B-4729-8D8C-C857A6F0FDF3";
	public const string SqlStatisticsTabLogicalViewGuid = "791399A1-6E21-4A73-9AAA-293B6563C77B";
	public const string SqlTextResultsTabLogicalViewGuid = "04BE2EC0-64F0-4D16-9BD1-5D3C95EC7070";

};