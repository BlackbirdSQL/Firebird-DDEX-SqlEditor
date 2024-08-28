
using System;
using BlackbirdSql.Data.Model;
using BlackbirdSql.Data.Model.Schema;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.Data;


// =========================================================================================================
//											DbServerExplorerService Class
//
/// <summary>
/// Provides native db services for Server Explorer.
/// </summary>
// =========================================================================================================
public class DbServerExplorerService : SBsNativeDbServerExplorerService, IBsNativeDbServerExplorerService
{
	private DbServerExplorerService()
	{
	}

	public static IBsNativeDbServerExplorerService EnsureInstance() => _Instance ??= new DbServerExplorerService();


	public static IBsNativeDbServerExplorerService _Instance = null;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decorates a DDL raw script to it's executable form.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string GetDecoratedDdlSource_(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		// Tracer.Trace(typeof(Moniker), "GetDecoratedDdlSource()");

		IVsDataObject obj = node.Object;

		if (obj == null)
			return "Node has no object";

		string prop = GetNodeScriptProperty(obj).ToUpper();
		string src = ((string)obj.Properties[prop]).Replace("\r\n", "\n").Replace("\r", "\n");

		string dirtysrc = "";

		while (src != dirtysrc)
		{
			dirtysrc = src;
			src = dirtysrc.Replace("\n\n\n", "\n\n");
		}


		switch (node.NodeBaseType())
		{
			case EnModelObjectType.Function:
				return GetDecoratedDdlSourceFunction(src, node, obj, targetType);
			case EnModelObjectType.StoredProcedure:
				return GetDecoratedDdlSourceProcedure(src, node, obj, targetType);
			case EnModelObjectType.Table:
				return GetDecoratedDdlSourceTable(src);
			case EnModelObjectType.Trigger:
				return GetDecoratedDdlSourceTrigger(src, obj, targetType);
			case EnModelObjectType.View:
				return GetDecoratedDdlSourceView(src, node, obj, targetType);
			default:
				return src;
		}
	}



	private static string GetDecoratedDdlSourceFunction(string src, IVsDataExplorerNode node,
		IVsDataObject obj, EnModelTargetType targetType)
	{
		int direction;
		string flddef;
		string script = "";
		string strout = "";

		IVsDataExplorerChildNodeCollection nodes = node.GetChildren(false);


		foreach (IVsDataExplorerNode child in nodes)
		{
			if (child.Object == null)
				continue;

			if (child.Object.Type.Name != "FunctionParameter" && child.Object.Type.Name != "FunctionReturnValue")
				continue;

			direction = (short)child.Object.Properties["ORDINAL_POSITION"] == 0 ? 1 : 0;

			flddef = (direction == 0 ? child.Object.Properties["ARGUMENT_NAME"] + " " : "")
					+ DbTypeHelper.ConvertDataTypeToSql(child.Object.Properties["FIELD_DATA_TYPE"],
					child.Object.Properties["FIELD_SIZE"], child.Object.Properties["NUMERIC_PRECISION"],
					child.Object.Properties["NUMERIC_SCALE"]);

			if (direction == 0) // In
				script += (script != "" ? ",\n\t" : "\n\t(") + flddef;

			if (direction > 0) // Out
				strout += (strout != "" ? ",\n\t" : "") + flddef;
		}

		if (script != "")
			script += ")";

		if (strout != "")
			strout = $"\nRETURNS {strout}";

		if (strout != "")
			script += strout;

		if (targetType == EnModelTargetType.AlterScript)
			script = $"ALTER FUNCTION {obj.Properties["FUNCTION_NAME"]}{script}\n";
		else
			script = $"CREATE FUNCTION {obj.Properties["FUNCTION_NAME"]}{script}\n";

		src = $"{script}AS\n{src}";

		return WrapScriptWithTerminators(src);
	}



	private static string GetDecoratedDdlSourceProcedure(string src, IVsDataExplorerNode node,
		IVsDataObject obj, EnModelTargetType targetType)
	{
		int direction;
		string flddef;
		string script = "";
		string strout = "";

		IVsDataExplorerChildNodeCollection nodes = node.GetChildren(false);


		foreach (IVsDataExplorerNode child in nodes)
		{
			if (child.Object == null || child.Object.Type.Name != "StoredProcedureParameter")
				continue;
			direction = Convert.ToInt32(child.Object.Properties["PARAMETER_DIRECTION"]);

			flddef = child.Object.Properties["PARAMETER_NAME"] + " "
					+ DbTypeHelper.ConvertDataTypeToSql(child.Object.Properties["FIELD_DATA_TYPE"],
					child.Object.Properties["FIELD_SIZE"], child.Object.Properties["NUMERIC_PRECISION"],
					child.Object.Properties["NUMERIC_SCALE"]);

			if (direction == 0 || direction == 3) // In
			{
				if (targetType == EnModelTargetType.AlterScript)
					script += (script != "" ? ",\n\t" : "\n\t(") + flddef;
				else
					script += (script != "" ? "\n" : "\n-- The following input parameters need to be initialized after the BEGIN statement\n")
						+ "DECLARE VARIABLE " + flddef + ";";
			}
			if (direction > 0) // Out
				strout += (strout != "" ? ",\n\t" : "\n\t(") + flddef;
		}
		if (script != "" && targetType == EnModelTargetType.AlterScript)
			script += ")";
		if (strout != "")
			strout = $"\nRETURNS{strout})";

		if (strout != "" && targetType == EnModelTargetType.AlterScript)
			script += strout;

		if (targetType == EnModelTargetType.AlterScript)
			script = $"ALTER PROCEDURE {obj.Properties["PROCEDURE_NAME"]}{script}\n";
		else
			strout = $"EXECUTE BLOCK{strout}\n";

		if (targetType == EnModelTargetType.AlterScript)
			src = $"{script}AS\n{src}";
		else
			src = $"{strout}AS{script}\n-- End of parameter declarations\n{src}";

		return WrapScriptWithTerminators(src);
	}



	private static string GetDecoratedDdlSourceTable(string src)
	{
		src = $"SELECT * FROM {src.ToUpperInvariant()}";
		return src;
	}



	private static string GetDecoratedDdlSourceTrigger(string src, IVsDataObject obj,
		EnModelTargetType targetType)
	{
		string script;
		string active = (bool)obj.Properties["IS_INACTIVE"] ? "INACTIVE" : "ACTIVE";

		if (targetType == EnModelTargetType.AlterScript)
			script = $"ALTER TRIGGER {obj.Properties["TRIGGER_NAME"]}";
		else
			script = $"CREATE TRIGGER {obj.Properties["TRIGGER_NAME"]} FOR {obj.Properties["TABLE_NAME"]}";

		src = $"{script} {active}\n"
			+ $"{GetTriggerEventType((long)obj.Properties["TRIGGER_TYPE"])} POSITION {(short)obj.Properties["PRIORITY"]}\n"
			+ src;
		return WrapScriptWithTerminators(src);
	}



	private static string GetDecoratedDdlSourceView(string src, IVsDataExplorerNode node,
		IVsDataObject obj, EnModelTargetType targetType)
	{
		if (targetType != EnModelTargetType.AlterScript)
			return src;

		string script = "";
		IVsDataExplorerChildNodeCollection nodes = node.GetChildren(false);

		foreach (IVsDataExplorerNode child in nodes)
			script += (script != "" ? ",\n\t" : "\n\t(") + child.Object.Properties["COLUMN_NAME"];

		if (script != "")
			script += ")";

		src = $"ALTER VIEW {obj.Properties["VIEW_NAME"]}{script}\nAS\n{src}";
		return src;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the Source script of a node if it exists else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static string GetNodeScriptProperty(IVsDataObject obj)
	{
		if (obj != null)
			return GetNodeScriptProperty(obj.Type.Name);

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the Source script of a node if it exists else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static string GetNodeScriptProperty(string type)
	{
		if (type.EndsWith("Column") || type.EndsWith("Trigger")
			 || type == "Index" || type == "ForeignKey")
		{
			return "Expression";
		}
		else if (type == "Table")
		{
			return "Table_Name";
		}
		else if (type == "View")
		{
			return "Definition";
		}
		else if (type == "StoredProcedure" || type == "Function")
		{
			return "Source";
		}

		return type;
	}


	// ---------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------
	private static string GetTriggerEventType(long eventType)
	{
		return eventType switch
		{
			1 => "BEFORE INSERT",
			2 => "AFTER INSERT",
			3 => "BEFORE UPDATE",
			4 => "AFTER UPDATE",
			5 => "BEFORE DELETE",
			6 => "AFTER DELETE",
			17 => "BEFORE INSERT OR UPDATE",
			18 => "AFTER INSERT OR UPDATE",
			25 => "BEFORE INSERT OR DELETE",
			26 => "AFTER INSERT OR DELETE",
			27 => "BEFORE UPDATE OR DELETE",
			28 => "AFTER UPDATE OR DELETE",
			113 => "BEFORE INSERT OR UPDATE OR DELETE",
			114 => "AFTER INSERT OR UPDATE OR DELETE",
			8192 => "ON CONNECT",
			8193 => "ON DISCONNECT",
			8194 => "ON TRANSACTION BEGIN",
			8195 => "ON TRANSACTION COMMIT",
			8196 => "ON TRANSACTION ROLLBACK",
			_ => "",
		};
	}



	public int GetObjectTypeIdentifierLength_(string typeName)
	{
		return DslObjectTypes.GetIdentifierLength(typeName);
	}



	private static string WrapScriptWithTerminators(string script)
	{
		return $"SET TERM GO ;\n{script}\nGO\nSET TERM ; GO";
	}
}
