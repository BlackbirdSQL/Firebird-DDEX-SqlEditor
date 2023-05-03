// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;



namespace BlackbirdSql.Common;


// =========================================================================================================
//											SystemData Class
//
/// <summary>
/// The current data provider (in this case Firebird) specific constants and statics
/// </summary>
// =========================================================================================================
public static class SystemData
{
	public const string Invariant = "FirebirdSql.Data.FirebirdClient";
	public const string ProviderFactoryName = "FirebirdClient Data Provider";
	public const string ProviderFactoryType = "FirebirdSql.Data.FirebirdClient.FirebirdClientFactory";
	public const string ProviderFactoryDescription = ".NET Framework Data Provider for Firebird";

	public const string EFProvider = "EntityFramework.Firebird";
	public const string EFProviderServices = "EntityFramework.Firebird.FbProviderServices";
	public const string EFConnectionFactory = "EntityFramework.Firebird.FbConnectionFactory";

	public const string DataProviderName = "Firebird SQL Server"; // Firebird
	public const string DataSourceGuid = "2979569E-416D-4DD8-B06B-EBCB70DE7A4E"; // Firebird
	public const string TechnologyGuid = "77AB9A9D-78B9-4ba7-91AC-873F5338F1D2"; // Visual Studio DataProviders

	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>
	public static string ConfiguredConnectionsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
		+ "\\flamerobin\\fr_databases.conf";

};