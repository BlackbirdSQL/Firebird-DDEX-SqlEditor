// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

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
	internal const string C_LanguageServiceName = "SBsLanguageService";
	internal const string C_LanguageLongName = "Firebird-SQL";
	internal const string C_RegistrySettingsKey = "BlackbirdSql\\LanguageService";

	internal const string C_DefaultMessagePrefix = "FB-SQL: ";

	internal const string C_Extension = ".fbsql";

	internal const char C_DoubleQuote = '"';
	internal const char C_OpenSquareBracket = '[';



	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - PackageData
	// ---------------------------------------------------------------------------------------------------------

	// Blackbird Language Service
	internal const string C_LanguageServiceGuid = "B482BCAE-A222-4DAD-BFD1-0676C38D4DC0";

	internal const string C_MandatedExpressionEvaluatorGuid = VS.TSqlExpressionEvaluatorGuid;

	// Language extension preferences Advanced options guids
	// public const string C_LanguageLegacyPreferencesPageGuid = "2F05143F-C9D5-4487-86FF-F6B485562DE1";
	// public const string C_LanguageLegacyPreferencesModelGuid = "0F51223F-31F3-4487-A3E7-CF35DFBF5E2A";
	internal const string C_LanguagePreferencesPageGuid = "2375DEE4-E3CE-4308-A98F-828B8632D0FD";
	internal const string C_LanguagePreferencesModelGuid = "CDE27EC5-AA5D-4410-A334-178C0896AEA4";
	internal const string C_TransientLanguagePreferencesPageGuid = "3DA153A2-306F-4274-9D49-D652CBD29212";

	internal const string C_LanguagePreferencesGuid = "A0DB53AB-0DD4-4CCC-ADE2-AE68E0110723";


	#endregion Package Guids - PackageData




	// ---------------------------------------------------------------------------------
	#region Security tokens
	// ---------------------------------------------------------------------------------


	internal const string C_PublicTokenString = "d39a163eb11ac91a"; // Wrong
	internal const string C_PublicTokenStringNET = "";

	internal const string C_PublicHashString = "0024000004800000940000000602000000240000525341310004000001000100e56f1f433a7cfdaa1b03c850acd81a030af2492cb01d3d52649c181557ef64f685083941c730754605909ae95727ce7e54e2bbab0f8c20afe0ca770a17874cceac0e5bf326c668298d9045d6a2aed3b47dd5b7499f22dfbaadbbf7056e9b229fdd64eee4918bfbfe6cc39691825d216c6864b4bfbc2f5ce792e07d67ac8e15d1";

	internal const string C_PublicHashStringNET = "";


	#endregion Security tokens

};