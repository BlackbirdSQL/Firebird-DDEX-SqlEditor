﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using BlackbirdSql.Shared.Model;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared;


// =========================================================================================================
//											LibraryData Class
//
/// <summary>
/// Contains extension constants for additional (Shared) BlackbirdSql services (Editor/Language services).
/// </summary>
// =========================================================================================================
public static class LibraryData
{
	public const int C_ChangeDatabaseErrorNumber = 5701;
	public const long C_KeepAliveOffscreenModulus = 16L; // Approx 8 seconds.
	public const long C_KeepAliveValidationModulus = 4L; // Approx 2 seconds.

	public const string C_ShowPlanNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";



	// ---------------------------------------------------------------------------------------------------------
	#region Library Guids - LibraryData
	// ---------------------------------------------------------------------------------------------------------


	// Private Service and Factory Guids
	public const string C_EditorPaneGuid = "ADE35F8D-9953-4E4C-9190-A0DDE7075840";
	public const string C_ResultsEditorFactoryGuid = "31AD6A1B-B7A2-4B16-AADA-28ADEADF7F2E";


	/// <summary>
	/// Cross-reference in <see cref="IVsTextLines"/> of a document's <see cref="AuxilliaryDocData"/>.
	/// </summary>
	public const string C_AuxilliaryDocDataGuid = "C0FB89E1-CE99-47E2-B18A-C13CF7766452";




	// Tabs
	public const string C_MessageTabGuid = "DCB777D2-E346-42A7-9619-E1D60DD1C098";
	// public const string C_ExecutionPlanTabGuid = "10308C26-2E83-4DF5-BA5E-17935360B543";
	public const string C_TextPlanTabGuid = "200E716C-607B-4729-8D8C-C857A6F0FDF3";
	public const string C_StatisticsTabGuid = "C7601C1A-F370-4560-83B3-07DE8331EA8F";
	public const string C_TextResultsTabGuid = "2C5E6F49-4D0D-44E2-B6C1-DAF685928BD9";


	#endregion Library Guids

};