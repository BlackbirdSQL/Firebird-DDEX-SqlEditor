//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.Data.Model.Schema;


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
	internal const string Root = "";
	internal const string Domain = "Domain";
	internal const string Database = "Database";
	internal const string Table = "Table";
	internal const string TableColumn = "TableColumn";
	internal const string TableIndex = "TableIndex";
	internal const string TableIndexColumn = "TableIndexColumn";
	internal const string TableUniqueKey = "TableUniqueKey";
	internal const string TableUniqueKeyColumn = "TableUniqueKeyColumn";
	internal const string TableForeignKey = "TableForeignKey";
	internal const string TableForeignKeyColumn = "TableForeignKeyColumn";
	internal const string Trigger = "Trigger";
	internal const string IdentityTrigger = "IdentityTrigger";
	internal const string StandardTrigger = "StandardTrigger";
	internal const string SystemTrigger = "SystemTrigger";
	internal const string TriggerColumn = "TriggerColumn";
	internal const string View = "View";
	internal const string ViewColumn = "ViewColumn";
	internal const string StoredProcedure = "StoredProcedure";
	internal const string StoredProcedureParameter = "StoredProcedureParameter";
	internal const string StoredProcedureColumn = "StoredProcedureColumn";
	internal const string Function = "Function";
	internal const string FunctionParameter = "FunctionParameter";
	internal const string FunctionColumn = "FunctionColumn";
	internal const string User = "User";
	internal const string Role = "Role";



	/// <summary>
	/// Gets the identifier length used by <see cref="VxbObjectIdentifierResolver"/>.
	/// </summary>
	/// <param name="typeName"></param>
	/// <returns></returns>
	internal static int GetIdentifierLength(string typeName)
	{
		switch (typeName)
		{
			case DslObjectTypes.Root:
				return 0;

			case DslObjectTypes.Database:
			case DslObjectTypes.User:
			case DslObjectTypes.Role:
				return 1;

			case DslObjectTypes.Table:
			case DslObjectTypes.Function:
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
			case DslObjectTypes.FunctionParameter:
			case DslObjectTypes.FunctionColumn:
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
