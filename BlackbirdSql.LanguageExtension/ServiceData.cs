// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;



namespace BlackbirdSql.LanguageExtension;


// =========================================================================================================
//											ServiceData Class
//
/// <summary>
/// The current data provider specific constants and statics
/// </summary>
// =========================================================================================================
public static class ServiceData
{
	public const string LanguageServiceName = "Blackbird SQL Language Service";
	public const string EditorApplicationName = "BlackbirdSql Server Data Tools, T-SQL Editor";
	public const string ColorServiceName = "Blackbird SQL Color Service";

	/// <summary>
	/// The path to the provider's configured connections xml (in this case FlameRobin for Firebird).
	/// </summary>

	public const string ColorServiceGuid = "D3B291B0-3E5F-414B-A035-8D5FCA5B5D7A";
	public const string ColorServiceCategoryGuid = "C69E7C87-FEF0-43C8-8D53-082556FB2DA3";

	// TBC
	public const string PropertyOleSqlGuid = "F78AEC67-32DB-445e-B1AA-97BFB5BB5163";
	public const string PropertyBatchSeparatorGuid = "8F2F533D-81AF-4270-84CF-BB8EDF7B5A76";
	public const string PropertySqlVersionGuid = "C856A011-E8D4-4095-AC48-B46814D9FC2F";
	public static Guid CLSID_DatabaseChanged => new("D63AB40F-C17E-44a4-8017-0770EEF27FF5");

	// Vs Guids
	public const string IntelliSenseEnabledGuid = "097A840C-BDDA-4573-8F6D-671EBB21746D";

};