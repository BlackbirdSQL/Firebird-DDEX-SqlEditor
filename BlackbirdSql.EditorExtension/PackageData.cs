// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;



namespace BlackbirdSql.EditorExtension;


// =========================================================================================================
//										PackageData Class
//
/// <summary>
/// Container class for all package specific constants
/// </summary>
// =========================================================================================================
static class PackageData
{
	public const string ServiceName = "BlackbirdSql Editor";


	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - PackageData
	// ---------------------------------------------------------------------------------------------------------

	
	// Private Proffered Services and Object CLSIDs
	public static readonly Guid CLSID_FontAndColorService = new("2EF6AAA5-7BAE-452B-AA43-6472FB2FFFFB");
	public static readonly Guid CLSID_EditorMarkerService = new("984A3634-C7DE-41D9-992E-CE35638B513F");


	// Settings Guids
	public const string GeneralSettingsGuid = "8D99B934-CBF9-4C90-B937-458D032D557D";
	public const string TabAndStatusBarSettingsGuid = "7E59163E-26C4-4C55-8F18-F9FD8505BCA1";
	public const string ExecutionSettingsGuid = "80F3E998-427E-424C-BFB4-C793B023B65F";
	public const string ExecutionAdvancedSettingsGuid = "8B7CEE6C-1BA3-4C5A-B1F2-9237A1A1F4C0";
	public const string ResultsSettingsGuid = "1F698997-4A6A-4FCD-9B17-093C80F840A1";
	public const string ResultsGridSettingsGuid = "EC215887-72B8-428A-9CA5-6A6FF14CAAE7";
	public const string ResultsTextSettingsGuid = "6BB319F9-DF44-4DBD-B9C4-B4E6258188BD";

	public const string TransientExecutionSettingsGuid = "8C719ABB-2F88-47F7-9663-0CD20FBF3757";
	public const string TransientExecutionAdvancedSettingsGuid = "D73E05E2-3384-4CEA-85AB-D105ACB1FB6A";
	public const string TransientResultsSettingsGuid = "1B4928F1-38D7-4DFA-8637-36BE9ACA3905";
	public const string TransientResultsGridSettingsGuid = "C9456FF6-A97E-4F8E-9FF7-49ABFF4944B8";
	public const string TransientResultsTextSettingsGuid = "4C3E170F-C567-442C-ABB4-559F43610AEC";


	#endregion Package Guids - PackageData




	// ---------------------------------------------------------------------------------
	#region Security tokens
	// ---------------------------------------------------------------------------------


	public const string PublicTokenString = "d39a163eb11ac91a"; // Wrong
	public const string PublicTokenStringNET = "";

	public const string PublicHashString = "0024000004800000940000000602000000240000525341310004000001000100512159ca39877ea5b1d2c6f742c9af8cf1fd8e0f3bd338089da21978db61662bf6b6ba2dc5ab49326fe882ae8dd25e7512252cd33f873e45e2bd430180cd549442221d3bead54c726ccde9a12be022b222309dece12161b8d4d251124912f6e83bac2d6f1ee6a2679323024ff86f26fa6dc58fd420fb1f2f6d378b7f9a1b6ab7";
	public const string PublicHashStringNET = "";


	#endregion Security tokens

};