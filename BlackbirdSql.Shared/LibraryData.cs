// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.Shared.Model;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared;


// =========================================================================================================
//											ServiceData Class
//
/// <summary>
/// Contains extension constants for additional core BlackbirdSql services (Editor/Language services).
/// </summary>
// =========================================================================================================
public static class LibraryData
{
	public const int C_ConnectionValidationModulus = 20;
	public const string C_ShowPlanNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";

	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - LibraryData
	// ---------------------------------------------------------------------------------------------------------


	#endregion Package Guids


	// Private Service and Factory Guids
	public const string EditorPaneGuid = "ADE35F8D-9953-4E4C-9190-A0DDE7075840";
	public const string SqlResultsEditorFactoryGuid = "31AD6A1B-B7A2-4B16-AADA-28ADEADF7F2E";


	/// <summary>
	/// Cross-reference in <see cref="IVsTextLines"/> of a document's <see cref="AuxilliaryDocData"/>.
	/// </summary>
	public const string AuxilliaryDocDataGuid = "C0FB89E1-CE99-47E2-B18A-C13CF7766452";




	// Tabs
	public const string SqlMessageTabLogicalViewGuid = "DCB777D2-E346-42A7-9619-E1D60DD1C098";
	// public const string SqlExecutionPlanTabLogicalViewGuid = "10308C26-2E83-4DF5-BA5E-17935360B543";
	public const string SqlTextPlanTabLogicalViewGuid = "200E716C-607B-4729-8D8C-C857A6F0FDF3";
	public const string SqlStatisticsTabLogicalViewGuid = "C7601C1A-F370-4560-83B3-07DE8331EA8F";
	public const string SqlTextResultsTabLogicalViewGuid = "2C5E6F49-4D0D-44E2-B6C1-DAF685928BD9";

};