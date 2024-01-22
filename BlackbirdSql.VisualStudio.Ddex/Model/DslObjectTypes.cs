//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Model;


// =========================================================================================================
//										DslObjectTypes Class
//
/// <summary>
/// Identifier utility class and container for all identifier object constants used by the
/// <see cref="IVsDataObjectIdentifierResolver"/> implementation
/// </summary>
// =========================================================================================================
internal static class DslObjectTypes
{
	public const string Root = "";
	public const string Domain = "Domain";
	public const string Database = "Database";
	public const string Table = "Table";
	public const string TableColumn = "TableColumn";
	public const string TableIndex = "TableIndex";
	public const string TableIndexColumn = "TableIndexColumn";
	public const string TableUniqueKey = "TableUniqueKey";
	public const string TableUniqueKeyColumn = "TableUniqueKeyColumn";
	public const string TableForeignKey = "TableForeignKey";
	public const string TableForeignKeyColumn = "TableForeignKeyColumn";
	public const string Trigger = "Trigger";
	public const string IdentityTrigger = "IdentityTrigger";
	public const string StandardTrigger = "StandardTrigger";
	public const string SystemTrigger = "SystemTrigger";
	public const string TriggerColumn = "TriggerColumn";
	public const string View = "View";
	public const string ViewColumn = "ViewColumn";
	public const string StoredProcedure = "StoredProcedure";
	public const string StoredProcedureParameter = "StoredProcedureParameter";
	public const string StoredProcedureColumn = "StoredProcedureColumn";
	public const string ScalarFunction = "ScalarFunction";
	public const string ScalarFunctionParameter = "ScalarFunctionParameter";



	/// <summary>
	/// Gets the identifier length used by <see cref="TObjectIdentifierResolver"/>.
	/// </summary>
	/// <param name="typeName"></param>
	/// <returns></returns>
	public static int GetIdentifierLength(string typeName)
	{
		switch (typeName)
		{
			case DslObjectTypes.Root:
				return 0;

			case DslObjectTypes.Database:
				return 1;

			case DslObjectTypes.Table:
			case DslObjectTypes.ScalarFunction:
			case DslObjectTypes.StoredProcedure:
			case DslObjectTypes.View:
				return 3;

			case DslObjectTypes.TableColumn:
			case DslObjectTypes.TableIndex:
			case DslObjectTypes.TableUniqueKey:
			case DslObjectTypes.TableForeignKey:
			case DslObjectTypes.IdentityTrigger:
			case DslObjectTypes.StandardTrigger:
			case DslObjectTypes.SystemTrigger:
			case DslObjectTypes.TriggerColumn:
			case DslObjectTypes.ViewColumn:
			case DslObjectTypes.StoredProcedureParameter:
			case DslObjectTypes.StoredProcedureColumn:
			case DslObjectTypes.ScalarFunctionParameter:
				return 4;

			case DslObjectTypes.TableIndexColumn:
			case DslObjectTypes.TableUniqueKeyColumn:
			case DslObjectTypes.TableForeignKeyColumn:
				return 5;

			default:
				Diag.StackException("DslObjectType not found: " + typeName);
				return -1;
		}
	}
}
