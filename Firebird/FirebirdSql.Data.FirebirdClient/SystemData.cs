// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using Microsoft.VisualStudio.VCProjectEngine;

namespace FirebirdSql.Data;


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
	public const string TechnologyGuid = "77AB9A9D-78B9-4ba7-91AC-873F5338F1D2"; // Visual Studio DataProviders


	public const string DesignerExplorerServicesGuid = "4D30B519-9FB6-4FFD-A0CE-92863B1C37EA";
	public const string DesignerOnlineServicesGuid = "27F3F968-74EB-46B7-A1FF-6CCA57C0D894";

	public const string EventProviderGuid = "DB9B059D-ED1E-43EB-94A0-51C5AF15D397";
	
	// BlackbirdSql Output pane
	public const string OutputPaneGuid = "9E2B946C-4D46-4067-ABEB-E181F3B3768E";



	#endregion Package Guids

}