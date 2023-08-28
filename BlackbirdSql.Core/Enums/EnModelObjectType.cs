
using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Data.Services;

namespace BlackbirdSql.Core.Enums;


public enum EnModelObjectType
{
	Unknown,
	Column,
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
	FunctionParameter
}

public static class EnModelObjectTypeExtensions
{
	public static string ToString(this EnModelObjectType value)
	{
		return value switch
		{
			EnModelObjectType.StoredProcedure => "StoredProcedure",
			EnModelObjectType.Function => "Function",
			EnModelObjectType.Trigger => "Trigger",
			EnModelObjectType.Index => "Index",
			EnModelObjectType.ForeignKey => "ForeignKey",
			EnModelObjectType.View => "View",
			EnModelObjectType.Column => "Column",
			EnModelObjectType.IndexColumn => "IndexColumn",
			EnModelObjectType.ForeignKeyColumn => "ForeignKeyColumn",
			EnModelObjectType.TriggerColumn => "TriggerColumn",
			EnModelObjectType.ViewColumn => "ViewColumn",
			EnModelObjectType.StoredProcedureParameter => "StoredProcedureParameter",
			EnModelObjectType.FunctionParameter => "FunctionParameter",
			_ => "Unknown"
		};

	}

	public static EnModelObjectType ToModelObjectType(this IVsDataObjectType nodeType)
	{
		return nodeType.Name.ToUpperInvariant() switch
		{
			"STOREDPROCEDURE" => EnModelObjectType.StoredProcedure,
			"FUNCTION" => EnModelObjectType.Function,
			"TRIGGER" => EnModelObjectType.Trigger,
			"INDEX" => EnModelObjectType.Index,
			"FOREIGN KEY" => EnModelObjectType.ForeignKey,
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



	public static string ToUrlString(this EnModelObjectType value)
	{
		return value switch
		{
			EnModelObjectType.StoredProcedure => "Stored Procedure",
			EnModelObjectType.Function => "Function",
			EnModelObjectType.Trigger => "Trigger",
			EnModelObjectType.Index => "Index",
			EnModelObjectType.ForeignKey => "Foreign Key",
			EnModelObjectType.View => "View",
			EnModelObjectType.Column => "Column",
			EnModelObjectType.IndexColumn => "Index Column",
			EnModelObjectType.ForeignKeyColumn => "Foreign Key Column",
			EnModelObjectType.TriggerColumn => "Trigger Column",
			EnModelObjectType.ViewColumn => "View Column",
			EnModelObjectType.StoredProcedureParameter => "Stored Procedure Parameter",
			EnModelObjectType.FunctionParameter => "Function Parameter",
			_ => "Unknown"
		};
	}

	public static EnModelObjectType ToModelObjectType(this string urlType)
	{
		return urlType.ToUpperInvariant() switch
		{
			"STORED PROCEDURE" => EnModelObjectType.StoredProcedure,
			"FUNCTION" => EnModelObjectType.Function,
			"TRIGGER" => EnModelObjectType.Trigger,
			"INDEX" => EnModelObjectType.Index,
			"FOREIGN KEY" => EnModelObjectType.ForeignKey,
			"VIEW" => EnModelObjectType.View,
			"COLUMN" => EnModelObjectType.Column,
			"INDEX COLUMN" => EnModelObjectType.IndexColumn,
			"FOREIGN KEY COLUMN" => EnModelObjectType.ForeignKeyColumn,
			"TRIGGER COLUMN" => EnModelObjectType.TriggerColumn,
			"VIEW COLUMN" => EnModelObjectType.ViewColumn,
			"STORED PROCEDURE PARAMETER" => EnModelObjectType.StoredProcedureParameter,
			"FUNCTION PARAMETER" => EnModelObjectType.FunctionParameter,
			_ => EnModelObjectType.Unknown
		};
	}


}
