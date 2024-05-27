// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using System;

namespace BlackbirdSql.Data;

// =========================================================================================================
//											SystemData Class
//
/// <summary>
/// System wide and the current data provider (in this case Firebird) specific constants and statics
/// </summary>
// =========================================================================================================
public static class LibraryData
{
	public const string Invariant = "FirebirdSql.Data.FirebirdClient";
	public const string ProviderFactoryName = "FirebirdClient Data Provider";
	public const string ProviderFactoryClassName = "FirebirdSql.Data.FirebirdClient.FirebirdClientFactory";
	public const string ProviderFactoryDescription = ".NET Framework Data Provider for Firebird";

	public const string EFProvider = "EntityFramework.Firebird";
	public const string EFProviderServices = "EntityFramework.Firebird.FbProviderServices";
	public const string EFConnectionFactory = "EntityFramework.Firebird.FbConnectionFactory";

	public const string DataProviderName = "Firebird Server"; // Firebird
	public const string DbEngineName = "Firebird";
	public const string SchemaMetaDataXml = "FirebirdSql.Data.Schema.FbMetaData.xml";

	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>
	public static string ExternalUtilityConfigurationPath
		=> Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
		+ "\\flamerobin\\fr_databases.conf";


	public const string Protocol = "fbsql++";
	public static string Scheme => Protocol + "://";
	public const string Extension = ".fbsql";

	public const string SqlLanguageName = "FB-SQL";

	public const string XmlActualPlanColumn = "Firebird_SQL_Server_XML_ActualPlan";
	public const string XmlEstimatedPlanColumn = "Firebird_SQL_Server_XML_EstimatedPlan";

}