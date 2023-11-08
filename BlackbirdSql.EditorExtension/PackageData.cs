// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using BlackbirdSql.Core.Ctl.CommandProviders;

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
	public const string ServiceName = "Blackbird SQL Editor Service";


	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - PackageData
	// ---------------------------------------------------------------------------------------------------------


	// Settings Guids
	public const string GeneralSettingsGuid = "8D99B934-CBF9-4C90-B937-458D032D557D";
	public const string TabAndStatusBarSettingsGuid = "7E59163E-26C4-4C55-8F18-F9FD8505BCA1";
	public const string ExecutionSettingsGuid = "80F3E998-427E-424C-BFB4-C793B023B65F";
	public const string ExecutionAdvancedSettingsGuid = "8B7CEE6C-1BA3-4C5A-B1F2-9237A1A1F4C0";
	public const string ResultsSettingsGuid = "1F698997-4A6A-4FCD-9B17-093C80F840A1";
	public const string ResultsGridSettingsGuid = "EC215887-72B8-428A-9CA5-6A6FF14CAAE7";
	public const string ResultsTextSettingsGuid = "6BB319F9-DF44-4DBD-B9C4-B4E6258188BD";

	public const string SqlEditorGeneralOptionsGuid = "FDE3AD9E-51F4-45FB-9706-99A470BD9C53";
	public const string SqlEditorTabAndStatusBarOptionsGuid = "FF344DD3-79C5-4563-B45A-EE07CFA2B8F5";
	public const string SqlExecutionAdvancedOptionsGuid = "7F2B377B-AFEA-4545-BDAC-69E5F348BE66";
	public const string SqlExecutionAnsiOptionsGuid = "CF83838C-B3BE-49D2-9236-361133D09BAB";
	public const string SqlExecutionGeneralOptionsGuid = "E1772E9C-9ECD-4CB6-85B4-DB411E97472A";
	public const string SqlResultsGeneralOptionsGuid = "7B823EE3-B3CC-4423-A37E-DD250EB9233B";
	public const string SqlResultsToGridOptionsGuid = "68ED5512-1DC7-4320-AB4D-F4D883C4DD42";
	public const string SqlResultsToTextOptionsGuid = "71D54055-BA60-4389-AE22-FC2CCB9CF97E";


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