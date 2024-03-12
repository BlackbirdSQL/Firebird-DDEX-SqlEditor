// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;


namespace BlackbirdSql.Core;

// =========================================================================================================
//											SystemData Class
//
/// <summary>
/// System wide and the current data provider (in this case Firebird) specific constants and statics
/// </summary>
// =========================================================================================================
public static class SystemData
{

	public const string Invariant = "FirebirdSql.Data.FirebirdClient";
	public const string ProviderFactoryName = "FirebirdClient Data Provider";
	public const string ProviderFactoryClassName = "FirebirdSql.Data.FirebirdClient.FirebirdClientFactory";
	public const string ProviderFactoryDescription = ".NET Framework Data Provider for Firebird";

	public const string EFProvider = "EntityFramework.Firebird";
	public const string EFProviderServices = "EntityFramework.Firebird.FbProviderServices";
	public const string EFConnectionFactory = "EntityFramework.Firebird.FbConnectionFactory";

	public const string DataProviderName = "Firebird Server"; // Firebird

	public const string UIContextName = "BlackbirdSql UIContext Autoload";

	public static Type ProviderFactoryType = typeof(FirebirdSql.Data.FirebirdClient.FirebirdClientFactory);


	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>
	public static string ExternalUtilityConfigurationPath
		= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
		+ "\\flamerobin\\fr_databases.conf";





	// ---------------------------------------------------------------------------------------------------------
	#region Extension wide constants & statics - SystemData
	// ---------------------------------------------------------------------------------------------------------


	/// <summary>
	/// The base protocol used by monikers
	/// </summary>
	public const string Protocol = "fbsql++";
	public const string Scheme = Protocol + "://";
	public const string Extension = ".fbsql";

	public const char UnixFieldSeparator = '/';
	public const char WinFieldSeparator = '\\';
	public const char CompositeSeparator = '.';


	public const string DatasetKeyFmt = "{0} ({1})";
	public const string DatasetKeyAlternateFmt = "Database[\"{0} ({1})\"]";

	public const string ServiceFolder = "ServerxExplorer";
	public const string TempSqlFolder = "SqlTemporaryFiles";


	public const GenericUriParserOptions UriParserOptions =
		GenericUriParserOptions.AllowEmptyAuthority | GenericUriParserOptions.NoPort
		| GenericUriParserOptions.NoQuery | GenericUriParserOptions.NoFragment
		| GenericUriParserOptions.DontCompressPath
		| GenericUriParserOptions.DontUnescapePathDotsAndSlashes;


	#endregion Extension wide constants & statics





	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - SystemData
	// ---------------------------------------------------------------------------------------------------------


	/// <summary>
	/// The Package guid for DemandLoadPackage().
	/// </summary>
	public const string PackageGuid = "0B100D64-7249-4208-8748-2810B511E90C";

	/// <summary>
	/// The AsyncPackage guid for IBAsyncPackage.
	/// </summary>
	public const string AsyncPackageGuid = "7787981E-E42A-412F-A42B-9AD07A7DE169";

	/// <summary>
	/// The UIContext for autoload.
	/// </summary>
	public const string UIContextGuid = "8838E01E-D709-486D-A933-46D30A864D51";

	/// <summary>
	/// The DataProvider registration guid.
	/// </summary>
	public const string ProviderGuid = "43015f6e-757f-408b-966e-c2bce34686ba";

	/// <summary>
	/// The factory service guid.
	/// </summary>
	public const string ProviderObjectFactoryServiceGuid = "B0640FC7-F798-4CC0-81F9-2587762D4957";

	/// <summary>
	/// FirebirdClient.
	/// </summary>
	public const string DataSourceGuid = "2979569E-416D-4DD8-B06B-EBCB70DE7A4E";

	/// <summary>
	/// BlackbirdSql Output pane.
	/// </summary>
	public const string OutputPaneGuid = "9E2B946C-4D46-4067-ABEB-E181F3B3768E";

	public const string PackageControllerGuid = "CF77D510-C1DB-44EA-85F5-8201089D6FAF";
	public const string ProviderSchemaFactoryGuid = "9FDE2679-7080-4C88-A734-71AEC4CB099A";
	
	public const string DesignerExplorerServicesGuid = "4D30B519-9FB6-4FFD-A0CE-92863B1C37EA";
	public const string DesignerOnlineServicesGuid = "27F3F968-74EB-46B7-A1FF-6CCA57C0D894";
	
	public const string MessageBoxParentGuid = "4BE829EA-E33E-49AB-BE5C-ADC6E6DE8AA7";


	#endregion Package Guids





	// ---------------------------------------------------------------------------------------------------------
	#region Editor and Language Service Guids
	// ---------------------------------------------------------------------------------------------------------


	// Mandated services for each service type that will be used by BlackbirdSql 
	public const string MandatedSqlEditorFactoryGuid = DslEditorFactoryGuid;
	// public const string MandatedSqlEditorFactoryGuid = VS.SqlEditorFactoryGuid;
	public const string MandatedSqlLanguageServiceGuid = VS.SSDTLanguageServiceGuid;
	public const string MandatedXmlLanguageServiceGuid = VS.XmlLanguageServiceGuid;
	public const string MandatedExpressionEvaluatorGuid = VS.TSqlExpressionEvaluatorGuid;
	public const string MandatedEventProviderGuid = VS.SqlEventProviderGuid;

	// BlackbirdSql Guids
	public const string DslEditorFactoryGuid = "D5797F62-22B1-41BC-9B8C-E248EE895966";
	public const string DslEditorEncodedFactoryGuid = "A548D241-D8B6-4219-9B07-C69733805F73";
	public const string DslEditorCommandsGuid = "993564A4-0186-4382-B143-7388277887B9";
	public const string DslEditorPaneGuid = "ADE35F8D-9953-4E4C-9190-A0DDE7075840";


	public const string DslEventProviderGuid = "2025C8D6-6399-45DD-98B4-EEF358BDF87D";


	#endregion Editor and Language Service Guids


}