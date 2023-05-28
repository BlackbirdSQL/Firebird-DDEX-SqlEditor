
using System;
using System.Globalization;
using System.Text;

using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common.Properties;

using DataObjectType = BlackbirdSql.Common.Commands.CommandProperties.DataObjectType;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.DataTools.Interop;
using System.Xml.Linq;
using Microsoft.VisualStudio.GraphModel.CodeSchema;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.ServiceHub.Resources;
using Newtonsoft.Json.Linq;

namespace BlackbirdSql.Common.Providers;


internal class SqlMonikerHelper
{
	private const string _ServiceFolder = "ServerExplorer";
	private const string _TempSqlFolder = "SqlTemporaryFiles";

	public const string Protocol = "fbsql";

	public const string ProtocolPrefix = "fbsql://";

	public const string NamePrefix = "{0}@{1}({2})";

	public const string UrlPrefix = "fbsql://{0}@{1}({2})";

	public const string DisplayFormat = "{0} - {1} [{2}]";

	private const char _CompositeIdentifierSeparator = '.';

	private string _Server;

	private string _Database;

	private string _User;

	private string _ObjectType;

	private string _ObjectName;


	public string Server
	{
		get
		{
			return _Server;
		}
		set
		{
			_Server = value;
		}
	}

	public string Database
	{
		get
		{
			return _Database;
		}
		set
		{
			_Database = value;
		}
	}

	public string User
	{
		set
		{
			_User = value;
		}
	}


	public string ObjectType
	{
		get
		{
			return _ObjectType;
		}
		set
		{
			_ObjectType = value;
		}
	}


	public string ObjectName
	{
		get
		{
			return _ObjectName;
		}
		set
		{
			_ObjectName = value;
		}
	}


	public string Moniker
	{
		get { return ToString(); }
	}

	public SqlMonikerHelper()
	{
	}

	public SqlMonikerHelper(string fbSqlUrl)
	{
		Parse(fbSqlUrl);
	}



	public SqlMonikerHelper(IVsDataExplorerNode node)
	{
		Extract(node);
	}




	internal string GetDecoratedDdlSource(IVsDataObject obj)
	{
		string prop = GetNodeScriptProperty(obj).ToUpper();
		string src = ((string)obj.Properties[prop]).Replace("\r\n", "\n").Replace("\r", "\n");

		switch (GetNodeBaseType(obj))
		{
			case "Trigger":
				string active = (bool)obj.Properties["IS_INACTIVE"] ? "INACTIVE" : "ACTIVE";
				src = $"CREATE TRIGGER {obj.Properties["TRIGGER_NAME"]} FOR {obj.Properties["TABLE_NAME"]} {active}\n"
					+ $"{GetTriggerEvent((long)obj.Properties["TRIGGER_TYPE"])} POSITION {(short)obj.Properties["PRIORITY"]}\n"
					+ src;
				return src;
			default:
				return src;
		}
	}



	public void Extract(IVsDataExplorerNode node)
	{
		IVsDataObject @rootObj = node.ExplorerConnection.ConnectionNode.Object;
		IVsDataObject @nodeObj = node.Object;

		if (@rootObj != null && @nodeObj != null)
		{
			_Server = (string)@rootObj.Properties["Server"];
			// This could be ambiguous
			_Database = (string)@rootObj.Properties["Database"];
			_User = (string)@rootObj.Properties["UserId"]; ;

			_ObjectType = (string)@nodeObj.Type.Name;

			_ObjectName = @nodeObj.Identifier.ToString(DataObjectIdentifierFormat.None);
		}
	}


	public object[] GetIdentifier()
	{
		object[] keys = _ObjectName.Split('.');
		object[] identifier = new object[keys.Length + 1];

		identifier[0] = identifier[1] = null;

		for (int i = 0; i < keys.Length; i++)
			identifier[i + 2] = keys[i];

		return identifier;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the Source script of a node if it exists else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetNodeScriptProperty(IVsDataObject obj)
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
	public static string GetNodeScriptProperty(string type)
	{
		if (type.EndsWith("Column") || type.EndsWith("Trigger")
			 || type == "Index" || type == "ForeignKey")
		{
			return "Expression";
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


	internal static string GetNodeBaseType(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			return GetNodeBaseType(node.Object);
		}

		return "";
	}


	internal static string GetNodeBaseType(IVsDataObject @object)
	{
		if (TypeNameIn(@object.Type.Name, "Index", "ForeignKey", "View", "StoredProcedure", "Function"))
		{
			return @object.Type.Name;
		}
		else if (@object.Type.Name.EndsWith("Column"))
		{
			return "Column";
		}
		else if (@object.Type.Name.EndsWith("Parameter"))
		{
			return "Parameter";
		}
		else if (@object.Type.Name.EndsWith("Trigger"))
		{
			return "Trigger";
		}

		return "";
	}

	internal static string GetNodePathType(string type)
	{
		if (type == "Index")
		{
			return "Index";
		}
		else if (TypeNameIn(type, "ForeignKey", "View", "StoredProcedure", "Function"))
		{
			return type;
		}
		else if (type.EndsWith("IndexColumn"))
		{
			return "IndexColumn";
		}
		else if (type.EndsWith("ForeignKeyColumn"))
		{
			return "ForeignKeyColumn";
		}
		else if (type.EndsWith("ViewColumn"))
		{
			return "ViewColumn";
		}
		else if (type.EndsWith("TriggerColumn"))
		{
			return "TriggerColumn";
		}
		else if (type.EndsWith("IndexColumn"))
		{
			return "IndexColumn";
		}
		else if (type.EndsWith("Column"))
		{
			return "Column";
		}
		else if (type.EndsWith("ProcedureParameter"))
		{
			return "ProcedureParameter";
		}
		else if (type.EndsWith("FunctionParameter"))
		{
			return "FunctionParameter";
		}
		else if (type.EndsWith("Parameter"))
		{
			return "Parameter";
		}
		else if (type.EndsWith("Trigger"))
		{
			return "Trigger";
		}
		else
		{
			return type;
		}
	}

	internal static string GetNodeAlterExpression(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (TypeNameIn(@object.Type.Name, "Index", "ForeignKey", "View", "StoredProcedure", "Function"))
			{
				return @object.Type.Name;
			}
			else if (@object.Type.Name.EndsWith("Column"))
			{
				return "Column";
			}
			else if (@object.Type.Name.EndsWith("Parameter"))
			{
				return "Parameter";
			}
			else if (@object.Type.Name.EndsWith("Trigger"))
			{
				return "Trigger";
			}

		}

		return "";
	}




	internal static string GetString(string resource)
	{
		return Resources.ResourceManager.GetString(resource);
	}



	internal static string GetNodeObjectType(DataObjectType objectType)
	{
		if (objectType == DataObjectType.System)
			return "System";
		else if (objectType == DataObjectType.User)
			return "User";

		return "";
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if typeName exists in the values array else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool TypeNameIn(string typeName, params string[] values)
	{
		foreach (string value in values)
		{
			if (typeName.Equals(value, StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}



	public void Parse(string fbSqlUrl)
	{
		// fbsql://username@server(dataset)/type/id0.id2.id3
		// SqlFiles/Firebird/Type/user.server(dataset).id0.id1.id2

		if (fbSqlUrl.StartsWith(ProtocolPrefix, StringComparison.OrdinalIgnoreCase))
		{
			/* TBC
			string protocol = Regex.Escape(ProtocolPrefix);
			string user = "[^@]+";
			Regex regex = new($"{protocol}({user})");
			*/

			string[] arr = fbSqlUrl[ProtocolPrefix.Length..].Split('/');

			int pos = arr[0].IndexOf('@');

			if (pos != -1)
				_User = arr[0][..pos];
			else
				_User = "";

			pos++;

			string str = arr[0][pos..];
			pos = str.IndexOf("(");

			_Server = str[..pos];
			pos++;
			_Database = str[pos..^1];
			_ObjectType = null;
			_ObjectName = null;

			if (arr.Length > 1)
			{
				_ObjectType = UrlTypeToObjectType(arr[1]);
			}

			if (arr.Length > 2)
			{
				_ObjectName = arr[2];
				for (int i = 3; i < arr.Length; i++)
					_ObjectName += _CompositeIdentifierSeparator + arr[i];
			}
		}
	}


	public override string ToString()
	{
		// 	public const string UrlPrefix = "fbsql://{0}@{1}({2})";

		StringBuilder stringBuilder = new(string.Format(CultureInfo.InvariantCulture, UrlPrefix, _User, _Server, _Database));

		if (_ObjectType != null)
		{
			stringBuilder.Append("/").Append(_ObjectType);
		}

		if (_ObjectName != null)
		{
			stringBuilder.Append("/").Append(_ObjectName);
		}

		return stringBuilder.ToString();
	}


	public string ToPath(string appDataPath)
	{
		string moniker = string.Format(CultureInfo.InvariantCulture, NamePrefix, _User, _Server, _Database);

		moniker = moniker.Replace("\\", "."); // "{backslash}");
		moniker = moniker.Replace("/", "."); // "{slash}");
		moniker = moniker.Replace(":", "{colon}");
		moniker = moniker.Replace("*", "{asterisk}");
		moniker = moniker.Replace("?", "{questionmark}");
		moniker = moniker.Replace("\"", "{quote}");
		moniker = moniker.Replace("<", "{openbracket}");
		moniker = moniker.Replace(">", "{closebracket}");
		moniker = moniker.Replace("|", "{bar}");

		moniker = $"{_ObjectName}[{moniker}.{GetNodePathType(_ObjectType)}].sql";

		string path = GetTemporaryDataDirectory(appDataPath);
		// path = Path.Combine(path, $"{GetNodePathType(_ObjectType)} ({GetNodeScriptProperty(_ObjectType)}s)");
		path = Path.Combine(path, moniker);

		return path;
	}

	public static string GetTemporaryDataDirectory(string appDataPath)
	{
		string path = appDataPath;

		path = Path.Combine(path, _ServiceFolder);
		path = Path.Combine(path, _TempSqlFolder);

		return path;
	}



	/// <summary>
	/// Converts an identifier string array to a moniker string
	/// </summary>
	public static string ToString(string[] identifierParts, DataObjectIdentifierFormat format = DataObjectIdentifierFormat.Default)
	{
		StringBuilder stringBuilder = new StringBuilder();

		int num = identifierParts.Length;
		if ((format & DataObjectIdentifierFormat.ForDisplay) != 0 && num > 0)
		{
			num--;
		}

		for (int i = 0; i < num; i++)
		{
			if (identifierParts[i] != null || stringBuilder.Length != 0)
			{
				stringBuilder.Append(identifierParts[i]);
				stringBuilder.Append(_CompositeIdentifierSeparator);
			}
		}

		string text = stringBuilder.ToString().Trim(_CompositeIdentifierSeparator);
		if ((format & DataObjectIdentifierFormat.ForDisplay) != 0)
		{
			if (text.Length > 0)
			{
				return identifierParts[^1] + " (" + text + ")";
			}

			return identifierParts[^1];
		}

		return text;
	}


	/// <summary>
	/// Converts an identifier object array to a moniker string
	/// </summary>
	public static string ToString(object[] identifierParts, DataObjectIdentifierFormat format = DataObjectIdentifierFormat.Default)
	{
		string[] strings = new string[identifierParts.Length];

		for (int i = 0; i < identifierParts.Length; i++)
			strings[i] = identifierParts[i].ToString();

		return ToString(strings, format);
	}



	private static string UrlTypeToObjectType(string urlType)
	{
		return urlType.ToUpperInvariant() switch
		{
			"STORED PROCEDURE" => "StoredProcedure",
			"FUNCTION" => "Function",
			"TRIGGER" => "Trigger",
			"INDEX" => "Index",
			"FOREIGN KEY" => "ForeignKey",
			"VIEW" => "View",
			"COLUMN" => "Column",
			"INDEX COLUMN" => "IndexColumn",
			"FOREIGN KEY COLUMN" => "ForeignKeyColumn",
			"TRIGGER COLUMN" => "TriggerColumn",
			"VIEW COLUMN" => "ViewColumn",
			"STORE PROCEDURE PARAMETER" => "StoredProcedureParameter",
			"FUNCTION PARAMETER" => "FunctionParameter",
			_ => urlType
		};
	}


	protected static string ObjectTypeToUrlType(string objectType)
	{
		return objectType.ToUpperInvariant() switch
		{
			"STOREDPROCEDURE" => "Stored Procedure",
			"FUNCTION" => "Function",
			"TRIGGER" => "Trigger",
			"INDEX" => "Index",
			"FOREIGNKEY" => "Foreign Key",
			"VIEW" => "View",
			"COLUMN" => "Column",
			"INDEXCOLUMN" => "Index Column",
			"FOREIGNKEYCOLUMN" => "Foreign Key Column",
			"TRIGGERCOLUMN" => "Trigger Column",
			"VIEWCOLUMN" => "View Column",
			"STOREPROCEDUREPARAMETER" => "Stored Procedure Parameter",
			"STOREDPROCEDURECOLUMN" => "Stored Procedure Parameter",
			"FUNCTIONPARAMETER" => "Function Parameter",
			_ => null
		};
	}

	public static string GetTriggerEvent(long eventType)
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
}
