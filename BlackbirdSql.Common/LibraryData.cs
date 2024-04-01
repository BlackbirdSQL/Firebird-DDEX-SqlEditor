// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using BlackbirdSql.Common.Model;
using Microsoft.VisualStudio.TextManager.Interop;



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
	public const string LanguageName = "FB-SQL";

	public const string C_ShowPlanNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";
	public const string C_YukonXmlExecutionPlanColumn = "Firebird_SQL_Server_XML_Showplan";

	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - LibraryData
	// ---------------------------------------------------------------------------------------------------------


	#endregion Package Guids


	// Private Service and Factory Guids
	public const string EditorPaneGuid = "ADE35F8D-9953-4E4C-9190-A0DDE7075840";
	public const string SqlResultsEditorFactoryGuid = "31AD6A1B-B7A2-4B16-AADA-28ADEADF7F2E";

	/// <summary>
	/// Unique guid key for saving the DbConnectionStringBuilder object parsed from the
	/// ServerExplorer node into IVsUserData. This connection info is added to the FlameRobin list if
	/// it does not exist. Note that the csb will include the parameter 'DatasetKey' which is the string
	/// used in the toolbar dropdown list.
	/// </summary>
	public const string UserDataCsbGuid = "EDD5003E-0797-40FF-8ACF-F93ED2A6C059";

	/// <summary>
	/// Cross-reference in <see cref="IVsTextLines"/> of a document's <see cref="AuxilliaryDocData"/>.
	/// </summary>
	public const string AuxilliaryDocDataGuid = "C0FB89E1-CE99-47E2-B18A-C13CF7766452";




	// Tabs
	public const string SqlMessageTabLogicalViewGuid = "32B79718-1118-4BF1-B9DF-4B975328790F";
	public const string SqlExecutionPlanTabLogicalViewGuid = "E313023B-F451-4A3C-9C34-BB16C5CCDB72";
	public const string SqlTextPlanTabLogicalViewGuid = "200E716C-607B-4729-8D8C-C857A6F0FDF3";
	public const string SqlStatisticsTabLogicalViewGuid = "791399A1-6E21-4A73-9AAA-293B6563C77B";
	public const string SqlTextResultsTabLogicalViewGuid = "04BE2EC0-64F0-4D16-9BD1-5D3C95EC7070";


};