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

	public const string DataProviderName = "Firebird SQL Server"; // Firebird

	public static Type ProviderFactoryType = typeof(FirebirdSql.Data.FirebirdClient.FirebirdClientFactory);

	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>
	public static string ConfiguredConnectionsPath
		= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
		+ "\\flamerobin\\fr_databases.conf";





	// ---------------------------------------------------------------------------------------------------------
	#region Package Guids - SystemData
	// ---------------------------------------------------------------------------------------------------------


	public const string PackageGuid = "0B100D64-7249-4208-8748-2810B511E90C";
	public const string PackageGuideNET = "7787981E-E42A-412F-A42B-9AD07A7DE169";

	public const string PackageControllerGuid = "CF77D510-C1DB-44EA-85F5-8201089D6FAF";

	public const string ObjectFactoryServiceGuid = "B0640FC7-F798-4CC0-81F9-2587762D4957";
	public const string ObjectFactoryServiceGuidNET = "AE2CB68C-0AA2-46A7-910A-CBDA1464DCB0";

	public const string DataSourceGuid = "2979569E-416D-4DD8-B06B-EBCB70DE7A4E"; // Firebird

	public const string DesignerExplorerServicesGuid = "4D30B519-9FB6-4FFD-A0CE-92863B1C37EA";
	public const string DesignerOnlineServicesGuid = "27F3F968-74EB-46B7-A1FF-6CCA57C0D894";

	public const string EventProviderGuid = "DB9B059D-ED1E-43EB-94A0-51C5AF15D397";
	
	// BlackbirdSql Output pane
	public const string OutputPaneGuid = "9E2B946C-4D46-4067-ABEB-E181F3B3768E";



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

	// BlackbirdSql Guids
	public const string DslEditorFactoryGuid = "D5797F62-22B1-41BC-9B8C-E248EE895966";
	public const string DslEditorEncodedFactoryGuid = "A548D241-D8B6-4219-9B07-C69733805F73";
	public const string DslEditorCommandsGuid = "993564A4-0186-4382-B143-7388277887B9";
	public const string DslEditorPaneGuid = "ADE35F8D-9953-4E4C-9190-A0DDE7075840";


	public const string DslEventProviderGuid = "7D445E62-A204-408E-B331-72EF93D0F2F0";


	#endregion Editor and Language Service Guids


}