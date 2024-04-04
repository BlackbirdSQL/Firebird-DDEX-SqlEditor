// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using BlackbirdSql.Core;



namespace BlackbirdSql.LanguageExtension;


// =========================================================================================================
//										PackageData Class
//
/// <summary>
/// Container class for all package specific constants
/// </summary>
// =========================================================================================================
static class PackageData
{
	public const string LanguageServiceName = "BlackbirdSql Language Service";
	public const string LanguageLongName = "Firebird-SQL";
	public const string RegistrySettingsKey = "BlackbirdSql\\LanguageService";

	public const string DefaultMessagePrefix = "FB-SQL: ";



	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - PackageData
	// ---------------------------------------------------------------------------------------------------------

	// Blackbird Language Service
	public const string LanguageServiceGuid = "B482BCAE-A222-4DAD-BFD1-0676C38D4DC0";

	public const string MandatedExpressionEvaluatorGuid = VS.TSqlExpressionEvaluatorGuid;

	// Language extension preferences Advanced options guids
	public const string LanguageLegacyPreferencesPageGuid = "2F05143F-C9D5-4487-86FF-F6B485562DE1";
	public const string LanguageLegacyPreferencesModelGuid = "0F51223F-31F3-4487-A3E7-CF35DFBF5E2A";
	public const string LanguagePreferencesPageGuid = "2375DEE4-E3CE-4308-A98F-828B8632D0FD";
	public const string LanguagePreferencesModelGuid = "CDE27EC5-AA5D-4410-A334-178C0896AEA4";
	public const string TransientLanguagePreferencesPageGuid = "3DA153A2-306F-4274-9D49-D652CBD29212";

	public const string LanguagePreferencesGuid = "A0DB53AB-0DD4-4CCC-ADE2-AE68E0110723";


	#endregion Package Guids - PackageData




	// ---------------------------------------------------------------------------------
	#region Security tokens
	// ---------------------------------------------------------------------------------


	public const string PublicTokenString = "d39a163eb11ac91a"; // Wrong
	public const string PublicTokenStringNET = "";

	public const string PublicHashString = "0024000004800000940000000602000000240000525341310004000001000100e56f1f433a7cfdaa1b03c850acd81a030af2492cb01d3d52649c181557ef64f685083941c730754605909ae95727ce7e54e2bbab0f8c20afe0ca770a17874cceac0e5bf326c668298d9045d6a2aed3b47dd5b7499f22dfbaadbbf7056e9b229fdd64eee4918bfbfe6cc39691825d216c6864b4bfbc2f5ce792e07d67ac8e15d1";

	public const string PublicHashStringNET = "";


	#endregion Security tokens

};