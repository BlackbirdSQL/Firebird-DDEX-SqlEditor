// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;


namespace BlackbirdSql.Core;

// =========================================================================================================
//											SystemData Class
//
/// <summary>
/// System wide and the current data provider specific constants and statics.
/// </summary>
// =========================================================================================================
public static class SystemData
{

	// ---------------------------------------------------------------------------------------------------------
	#region Extension wide constants & statics - SystemData
	// ---------------------------------------------------------------------------------------------------------

	/// <summary>
	/// This key is the globals persistent key for the solution stream and .user xml
	/// UserProperties node attribute.
	/// </summary>
	public const string C_PersistentKey = "GlobalBlackbirdPersistent";

	public const string C_UIContextName = "BlackbirdSql UIContext Autoload";

	// Glyphs for Rct connection sources.
	public const char C_SessionTitleGlyph = '\u25cf'; // Small solid circle
	public const char C_EdmTitleGlyph = '\u26ee'; // Gear with handles
	public const char C_ProjectTitleGlyph = '\u26ed'; // Gear without hub
	public const char C_UtilityTitleGlyph = '\u29c9'; // 2 joined squares

	public const char C_SessionDatasetGlyph = '\u2b24'; // Solid circle
	public const char C_EdmDatasetGlyph = '\u26ee'; // Gear with handles
	public const char C_ProjectDatasetGlyph = '\u26ed'; // Gear without hub
	public const char C_UtilityDatasetGlyph = '\u29c9'; // 2 joined squares
	// '\u2732'; // Open center asterisk
	// '\u2733'; // 8 spoked asterisk
	// '\u25cb'; // White circle

	public const char C_UnixFieldSeparator = '/';
	public const char C_WinFieldSeparator = '\\';
	public const char C_CompositeSeparator = '.';


	public const string C_ServiceFolder = "ServerxExplorer";
	public const string C_TempSqlFolder = "SqlTemporaryFiles";

	public const GenericUriParserOptions C_UriParserOptions =
		GenericUriParserOptions.AllowEmptyAuthority | GenericUriParserOptions.NoPort
		| GenericUriParserOptions.NoQuery | GenericUriParserOptions.NoFragment
		| GenericUriParserOptions.DontCompressPath
		| GenericUriParserOptions.DontUnescapePathDotsAndSlashes;


	#endregion Extension wide constants & statics





	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - SystemData
	// ---------------------------------------------------------------------------------------------------------


	/// <summary>
	/// The underlying package guid (Not used).
	/// </summary>
	public const string PackageGuid = "0EB9BA1B-3114-397D-822F-2D306DB02058";

	/// <summary>
	/// The UIContext for autoload.
	/// </summary>
	public const string UIContextGuid = "8838E01E-D709-486D-A933-46D30A864D51";

	/// <summary>
	/// The DataProvider registration guid.
	/// </summary>
	public const string ProviderGuid = "43015F6E-757F-408B-966E-C2BCE34686BA";

	/// <summary>
	/// The factory service guid.
	/// </summary>
	public const string ProviderObjectFactoryServiceGuid = "B0640FC7-F798-4CC0-81F9-2587762D4957";

	/// <summary>
	/// FirebirdClient.
	/// </summary>
	public const string DataSourceGuid = "2979569E-416D-4DD8-B06B-EBCB70DE7A4E";


	public const string DesignerExplorerServicesGuid = "4D30B519-9FB6-4FFD-A0CE-92863B1C37EA";
	public const string DesignerOnlineServicesGuid = "27F3F968-74EB-46B7-A1FF-6CCA57C0D894";
	
	public const string MessageBoxParentGuid = "4BE829EA-E33E-49AB-BE5C-ADC6E6DE8AA7";


	#endregion Package Guids





	// ---------------------------------------------------------------------------------------------------------
	#region Editor and Event Service Guids
	// ---------------------------------------------------------------------------------------------------------


	// BlackbirdSql Guids
	public const string EditorFactoryGuid = "D5797F62-22B1-41BC-9B8C-E248EE895966";
	public const string EditorEncodedFactoryGuid = "A548D241-D8B6-4219-9B07-C69733805F73";


	// Mandated services for each service type that will be used by BlackbirdSql 
	public const string MandatedSqlEditorFactoryGuid = EditorFactoryGuid;
	// public const string MandatedSqlEditorFactoryGuid = VS.SqlEditorFactoryGuid;


	#endregion Editor and Language Service Guids


}