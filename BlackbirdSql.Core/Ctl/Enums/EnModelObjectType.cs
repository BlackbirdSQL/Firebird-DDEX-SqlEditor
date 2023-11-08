
using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Core.Ctl.Enums;

public enum EnModelObjectType
{
	Unknown = 0,
	Column,
	Database,
	Index,
	IndexColumn,
	ForeignKeyColumn,
	ForeignKey,
	View,
	ViewColumn,
	Trigger,
	TriggerColumn,
	StoredProcedure,
	StoredProcedureParameter,
	Function,
	FunctionParameter,
	AlterUnknown = 20,
	AlterColumn,
	AlterDatabase,
	AlterIndex,
	AlterIndexColumn,
	AlterForeignKeyColumn,
	AlterForeignKey,
	AlterView,
	AlterViewColumn,
	AlterTrigger,
	AlterTriggerColumn,
	AlterStoredProcedure,
	AlterStoredProcedureParameter,
	AlterFunction,
	AlterFunctionParameter
}

public static class EnModelObjectTypeExtensions
{


	public static EnModelObjectType ToModelObjectType(this IVsDataObjectType nodeType)
	{
		return nodeType.Name.ToUpperInvariant() switch
		{
			"DATABASE" => EnModelObjectType.Database,
			"STOREDPROCEDURE" => EnModelObjectType.StoredProcedure,
			"FUNCTION" => EnModelObjectType.Function,
			"TRIGGER" => EnModelObjectType.Trigger,
			"INDEX" => EnModelObjectType.Index,
			"FOREIGNKEY" => EnModelObjectType.ForeignKey,
			"VIEW" => EnModelObjectType.View,
			"COLUMN" => EnModelObjectType.Column,
			"INDEXCOLUMN" => EnModelObjectType.IndexColumn,
			"FOREIGNKEYCOLUMN" => EnModelObjectType.ForeignKeyColumn,
			"TRIGGERCOLUMN" => EnModelObjectType.TriggerColumn,
			"VIEWCOLUMN" => EnModelObjectType.ViewColumn,
			"STOREDPROCEDUREPARAMETER" => EnModelObjectType.StoredProcedureParameter,
			"FUNCTIONPARAMETER" => EnModelObjectType.FunctionParameter,
			_ =>  EnModelObjectType.Unknown
		};
	}


	public static EnModelObjectType ToModelObjectType(this string monikerType)
	{
		return monikerType.ToUpperInvariant() switch
		{
			"DATABASE" => EnModelObjectType.Database,
			"STOREDPROCEDURE" => EnModelObjectType.StoredProcedure,
			"FUNCTION" => EnModelObjectType.Function,
			"TRIGGER" => EnModelObjectType.Trigger,
			"INDEX" => EnModelObjectType.Index,
			"FOREIGNKEY" => EnModelObjectType.ForeignKey,
			"VIEW" => EnModelObjectType.View,
			"COLUMN" => EnModelObjectType.Column,
			"INDEXCOLUMN" => EnModelObjectType.IndexColumn,
			"FOREIGNKEYCOLUMN" => EnModelObjectType.ForeignKeyColumn,
			"TRIGGERCOLUMN" => EnModelObjectType.TriggerColumn,
			"VIEWCOLUMN" => EnModelObjectType.ViewColumn,
			"STOREDPROCEDUREPARAMETER" => EnModelObjectType.StoredProcedureParameter,
			"FUNCTIONPARAMETER" => EnModelObjectType.FunctionParameter,
			"ALTERDATABASE" => EnModelObjectType.AlterDatabase,
			"ALTERSTOREDPROCEDURE" => EnModelObjectType.AlterStoredProcedure,
			"ALTERFUNCTION" => EnModelObjectType.AlterFunction,
			"ALTERTRIGGER" => EnModelObjectType.AlterTrigger,
			"ALTERINDEX" => EnModelObjectType.AlterIndex,
			"ALTERFOREIGNKEY" => EnModelObjectType.AlterForeignKey,
			"ALTERVIEW" => EnModelObjectType.AlterView,
			"ALTERCOLUMN" => EnModelObjectType.AlterColumn,
			"ALTERINDEXCOLUMN" => EnModelObjectType.AlterIndexColumn,
			"ALTERFOREIGNKEYCOLUMN" => EnModelObjectType.AlterForeignKeyColumn,
			"ALTERTRIGGERCOLUMN" => EnModelObjectType.AlterTriggerColumn,
			"ALTERVIEWCOLUMN" => EnModelObjectType.AlterViewColumn,
			"ALTERSTOREDPROCEDUREPARAMETER" => EnModelObjectType.AlterStoredProcedureParameter,
			"ALTERFUNCTIONPARAMETER" => EnModelObjectType.AlterFunctionParameter,
			_ => EnModelObjectType.Unknown
		};
	}


}
