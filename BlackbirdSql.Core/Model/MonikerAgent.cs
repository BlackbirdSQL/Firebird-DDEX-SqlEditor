
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Services;

using DataObjectType = BlackbirdSql.Core.CommandProviders.CommandProperties.DataObjectType;




namespace BlackbirdSql.Core.Model;


public class MonikerAgent
{
	// Url format: fbsql://user:password@server:port/dataset[/database_serialized/nodeType/id2.dd3...]
	public const string C_SqlExtension = ".fbsql";

	protected const string C_ServiceFolder = "ServerxExplorer";
	protected const string C_TempSqlFolder = "SqlTemporaryFiles";
	protected const string C_Scheme = "fbsql";
	protected const string C_PathPrefix = "{0}({1})";
	protected const char C_CompositeIdentifierSeparator = '.';

	protected readonly bool _IsUnique = false;

	protected bool _LowerCaseDatabase = true;

	protected string _TypeSuffix = "";

	protected string _Explorer = "";

	protected string _User = "";
	protected string _Password = "";
	protected string _Server = "";

	protected int _Port = CoreConstants.C_DefaultPortNumber;

	protected string _Database = "";

	protected string _Dataset = null;


	protected EnModelObjectType _ObjectType;

	protected string _ObjectName;

	public string DocumentMoniker => ToDocumentMoniker();


	public bool LowerCaseDatabase
	{
		get { return _LowerCaseDatabase; }
		set { _LowerCaseDatabase = value; }
	}


	public string Server
	{
		get { return _Server; }
		set { _Server = value; }
	}

	public int Port
	{
		get { return _Port; }
		set { _Port = value; }
	}

	public string Database
	{
		get { return _Database; }
		set { _Database = value; }
	}

	public string Dataset
	{
		get
		{
			if (string.IsNullOrEmpty(_Dataset))
			{
				if (string.IsNullOrEmpty(_Database))
					return "";

				return Path.GetFileNameWithoutExtension(_Database);
			}

			return _Dataset;
		}
		set { _Dataset = value; }
	}

	public string[] Identifier => _ObjectName.Split('.');


	public string User
	{
		get { return _User; }
		set { _User = value; }
	}

	public string Password
	{
		get { return _Password; }
		set { _Password = value; }
	}


	public EnModelObjectType ObjectType
	{
		get { return _ObjectType; }
		set { _ObjectType = value; }
	}


	public string ObjectName
	{
		get { return _ObjectName; }
		set { _ObjectName = value; }
	}

	public object[] OriginalIdentifier => GetOriginalIdentifier();


	public string Moniker => ToString();


	public MonikerAgent(bool isUnique = false, bool lowercaseDatabase = true, string typeSuffix = "")
	{
		_IsUnique = isUnique;
		_LowerCaseDatabase = lowercaseDatabase;
		_TypeSuffix = typeSuffix;
	}


	public MonikerAgent(string fbSqlUrl, bool isUnique = false, bool lowercaseDatabase = true, string typeSuffix = "")
		: this(isUnique, lowercaseDatabase, typeSuffix)
	{
		Parse(fbSqlUrl);
	}

	public MonikerAgent(IBPropertyAgent ci, bool isUnique = false, bool lowercaseDatabase = true, string typeSuffix = "")
		: this(isUnique, lowercaseDatabase, typeSuffix)
	{
		Parse(ci);
	}


	public MonikerAgent(IVsDataExplorerNode node, bool isUnique = false, bool lowercaseDatabase = true, string typeSuffix = "")
		: this(isUnique, lowercaseDatabase, typeSuffix)
	{
		Extract(node);
	}

	public MonikerAgent(string server, string database, string user, EnModelObjectType objectType,
		IList<string> identifierList, bool isUnique = false, bool lowercaseDatabase = true, string typeSuffix = "")
		: this(server, database, user, objectType, (object[])identifierList.ToArray(), isUnique, lowercaseDatabase, typeSuffix)
	{
	}


	public MonikerAgent(string server, string database, string user, EnModelObjectType objectType,
		object[] identifier, bool isUnique = false, bool lowercaseDatabase = true, string typeSuffix = "")
		: this(isUnique, lowercaseDatabase, typeSuffix)
	{
		_Server = server;
		_Database = database;
		_User = user;
		_ObjectType = objectType;

		_ObjectName = "";

		if (identifier != null && identifier.Length > 0)
		{
			_ObjectName = identifier[0] != null ? identifier[0].ToString() : "";
			for (int i = 1; i < identifier.Length; i++)
				_ObjectName += C_CompositeIdentifierSeparator
					+ (identifier[i] != null ? identifier[i].ToString() : "");
		}

	}



	public static string GetDecoratedDdlSource(IVsDataExplorerNode node, bool alternate)
	{
		IVsDataObject obj = node.Object;
		IVsDataExplorerChildNodeCollection nodes;

		if (obj == null)
			return "Node has no object";

		int direction;
		string prop = GetNodeScriptProperty(obj).ToUpper();
		string src = ((string)obj.Properties[prop]).Replace("\r\n", "\n").Replace("\r", "\n");
		string str = "";
		string strout = "";
		string flddef;

		string dirtysrc = "";
		while (src != dirtysrc)
		{
			dirtysrc = src;
			src = dirtysrc.Replace("\n\n\n", "\n\n");
		}


		switch (GetNodeBaseType(obj))
		{
			case "Trigger":
				string active = (bool)obj.Properties["IS_INACTIVE"] ? "INACTIVE" : "ACTIVE";
				if (alternate)
					str = $"ALTER TRIGGER {obj.Properties["TRIGGER_NAME"]}";
				else
					str = $"CREATE TRIGGER {obj.Properties["TRIGGER_NAME"]} FOR {obj.Properties["TABLE_NAME"]}";

				src = $"{str} {active}\n"
					+ $"{GetTriggerEvent((long)obj.Properties["TRIGGER_TYPE"])} POSITION {(short)obj.Properties["PRIORITY"]}\n"
					+ src;
				return src;
			case "View":
				if (!alternate)
					return src;

				nodes = node.GetChildren(false);
				foreach (IVsDataExplorerNode child in nodes)
					str += (str != "" ? ",\n\t" : "\n\t(") + child.Object.Properties["COLUMN_NAME"];

				if (str != "")
					str += ")";
				src = $"ALTER VIEW {obj.Properties["VIEW_NAME"]}{str}\nAS\n{src}";
				return src;
			case "StoredProcedure":
				nodes = node.GetChildren(false);

				foreach (IVsDataExplorerNode child in nodes)
				{
					if (child.Object == null || child.Object.Type.Name != "StoredProcedureParameter")
						continue;
					direction = (int)child.Object.Properties["PARAMETER_DIRECTION"];

					flddef = child.Object.Properties["PARAMETER_NAME"] + " "
							+ TypeHelper.ConvertDataTypeToSql(child.Object.Properties["FIELD_DATA_TYPE"],
							child.Object.Properties["FIELD_SIZE"], child.Object.Properties["NUMERIC_PRECISION"],
							child.Object.Properties["NUMERIC_SCALE"]);

					if (direction == 0 || direction == 3) // In
					{
						if (alternate)
							str += (str != "" ? ",\n\t" : "\n\t(") + flddef;
						else
							str += (str != "" ? "\n" : "\n-- The following input parameters need to be initialized after the BEGIN statement\n")
								+ "DECLARE VARIABLE " + flddef + ";";
					}
					if (direction > 0) // Out
						strout += (strout != "" ? ",\n\t" : "\n\t(") + flddef;
				}
				if (str != "" && alternate)
					str += ")";
				if (strout != "")
					strout = $"\nRETURNS{strout})";

				if (strout != "" && alternate)
					str += strout;

				if (alternate)
					str = $"ALTER PROCEDURE {obj.Properties["PROCEDURE_NAME"]}{str}\n";
				else
					strout = $"EXECUTE BLOCK{strout}\n";

				if (alternate)
					src = $"{str}AS\n{src}";
				else
					src = $"{strout}AS{str}\n-- End of parameter declarations\n{src}";

				return src;
			case "Function":
				nodes = node.GetChildren(false);

				foreach (IVsDataExplorerNode child in nodes)
				{
					if (child.Object == null)
						continue;
					if (child.Object.Type.Name != "FunctionParameter" && child.Object.Type.Name != "FunctionReturnValue")
						continue;
					direction = (short)child.Object.Properties["ORDINAL_POSITION"] == 0 ? 1 : 0;

					flddef = (direction == 0 ? child.Object.Properties["ARGUMENT_NAME"] + " " : "")
							+ TypeHelper.ConvertDataTypeToSql(child.Object.Properties["FIELD_DATA_TYPE"],
							child.Object.Properties["FIELD_SIZE"], child.Object.Properties["NUMERIC_PRECISION"],
							child.Object.Properties["NUMERIC_SCALE"]);

					if (direction == 0) // In
						str += (str != "" ? ",\n\t" : "\n\t(") + flddef;
					if (direction > 0) // Out
						strout += (strout != "" ? ",\n\t" : "") + flddef;
				}
				if (str != "")
					str += ")";
				if (strout != "")
					strout = $"\nRETURNS {strout}";

				if (strout != "")
					str += strout;

				if (alternate)
					str = $"ALTER FUNCTION {obj.Properties["FUNCTION_NAME"]}{str}\n";
				else
					str = $"CREATE FUNCTION {obj.Properties["FUNCTION_NAME"]}{str}\n";

				src = $"{str}AS\n{src}";

				return src;
			default:
				return src;
		}
	}



	public void Extract(IVsDataExplorerNode node)
	{
		_Explorer = node.ExplorerConnection.DisplayName;

		IVsDataObject @rootObj = node.ExplorerConnection.ConnectionNode.Object;
		IVsDataObject @nodeObj = node.Object;

		if (@rootObj != null && @nodeObj != null)
		{
			_Server = (string)@rootObj.Properties[ModelConstants.C_KeyRootDataSourceName];
			_Port = (int)@rootObj.Properties[ModelConstants.C_KeySIPortNumber];
			// This could be ambiguous
			_Dataset = (string)@rootObj.Properties[ModelConstants.C_KeyRootDataset];
			_Database = (string)@rootObj.Properties[ModelConstants.C_KeySICatalog];
			_User = (string)@rootObj.Properties[ModelConstants.C_KeySIUserId];
			_Password = (string)@rootObj.Properties[ModelConstants.C_KeySIPassword];

			_ObjectType = nodeObj.Type.ToModelObjectType();
			_ObjectName = @nodeObj.Identifier.ToString(DataObjectIdentifierFormat.None);
		}
	}





	public string[] GetFullIdentifier()
	{
		// 	public const string UrlPrefix = "fbsql://{0}@{1}({2})";

		string[] nodeIdentifier = Identifier;

		string[] identifier = new string[nodeIdentifier.Length + 2];

		identifier[0] = _Explorer;
		identifier[1] = _ObjectType.ToString();

		for (int i = 0; i < nodeIdentifier.Length; i++)
		{
			identifier[i+2] = nodeIdentifier[i];
		}

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


	public static string GetNodeBaseType(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			return GetNodeBaseType(node.Object);
		}

		return "";
	}


	public static string GetNodeBaseType(IVsDataObject @object)
	{
		if (ModelObjectTypeIn(@object, EnModelObjectType.Index, EnModelObjectType.ForeignKey,
			EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
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



	public static string GetNodeAlterExpression(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (ModelObjectTypeIn(@object, EnModelObjectType.Index, EnModelObjectType.ForeignKey,
				EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
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




	public static string GetNodeObjectType(DataObjectType objectType)
	{
		if (objectType == DataObjectType.System)
			return "System";
		else if (objectType == DataObjectType.User)
			return "User";

		return "";
	}


	protected object[] GetOriginalIdentifier()
	{
		// 	public const string UrlPrefix = "fbsql://{0}@{1}({2})";

		object[] nodeIdentifier = Identifier;

		string[] identifier = new string[nodeIdentifier.Length + 2];

		identifier[0] = identifier[1] = "";

		for (int i = 0; i < nodeIdentifier.Length; i++)
		{
			identifier[i + 2] = nodeIdentifier[i]?.ToString();
		}

		return identifier;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if typeName exists in the values array else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ModelObjectTypeIn(IVsDataObject @object, params EnModelObjectType[] types)
	{
		EnModelObjectType objecttype = @object.Type.ToModelObjectType();

		foreach (EnModelObjectType type in types)
		{
			if (type == objecttype)
				return true;
		}

		return false;
	}






	public void Parse(string fbSqlUrl)
	{
		UriBuilder urlb = new UriBuilder(fbSqlUrl);


		// fbsql://username:password@server/databasename/database/type/id0.id2.id3
		// SqlFiles/Firebird/Type/user.server(dataset).id0.id1.id2

		if (urlb.Scheme == C_Scheme)
		{
			/* TBC
			string protocol = Regex.Escape(ProtocolPrefix);
			string user = "[^@]+";
			Regex regex = new($"{protocol}({user})");
			*/

			_User = urlb.UserName;
			_Password = urlb.Password;
			_Server = urlb.Host;
			_Port = urlb.Port;


			string[] arr = urlb.Path.Split('/');

			int i;

			_Dataset = null;
			if (arr.Length > 0)
				_Dataset = arr[0];

			_Database = "";
			if (arr.Length > 1)
				_Database = JsonSerializer.Deserialize<string>(arr[1]);

			_ObjectType = EnModelObjectType.Unknown;
			if (arr.Length > 2)
				_ObjectType = arr[2].ToModelObjectType();

			_ObjectName = "";

			if (arr.Length > 3)
			{
				_ObjectName = arr[3];
				for (i = 4; i < arr.Length; i++)
					_ObjectName += C_CompositeIdentifierSeparator + arr[i];
			}
		}
	}



	public void Parse(IBPropertyAgent ci)
	{
		_User = (string)(ci["UserID"] ?? string.Empty);
		_Password = (string)(ci["Password"] ?? string.Empty);
		_Server = string.IsNullOrEmpty((string)ci["DataSource"]) ? "localhost" : (string)ci["DataSource"];
		_Database = (string)ci["Database"];
		_Port = (int)ci["Port"];
		_Dataset = ci.Dataset;

		_ObjectName = null;
		_ObjectType = EnModelObjectType.Unknown;
	}



	/// <summary>
	/// Converts the moniker to it's url format.
	/// </summary>
	/// <remarks>
	/// Url format: fbsql://user:password@server:port/dataset[/database_serialized/nodeType/id2.dd3...]
	/// </remarks>
	public override string ToString()
	{
		// 	public const string UrlPrefix = "fbsql://{0}@{1}({2})";

		UriBuilder urlb = new()
		{
			Scheme = C_Scheme,
			// UserName = _User,
			// Password = _Password,
			Host = _Server,
			Port = _Port
		};



		StringBuilder stringBuilder = new(Dataset);

		stringBuilder.Append("/");

		string str = string.IsNullOrEmpty(_Database) ? "" : JsonSerializer.Serialize(_Database, typeof(string));


		stringBuilder.Append(str);

		stringBuilder.Append("/");

		if (_ObjectType != EnModelObjectType.Unknown)
			stringBuilder.Append(_ObjectType);


		if (_ObjectName != null)
		{
			stringBuilder.Append("/");
			stringBuilder.Append(_ObjectName);
		}

		urlb.Path = stringBuilder.ToString();

		return urlb.Uri.ToString();
	}



	public DbConnectionStringBuilder ToCsb()
	{
		FbConnectionStringBuilder csb = new()
		{
			DataSource = _Server,
			Port = _Port,
			Database = _Database,
			UserID = _User,
			Password = _Password
		};
		csb.Add("DatasetKey", $"{_Server} ({Dataset})");

		return csb;

	}



	protected string ToDocumentMoniker()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}/", Server);
		stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}/", _LowerCaseDatabase ? Database.ToLower() : Database);
		stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}/", User);
		stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}/", ObjectType.ToString() + _TypeSuffix);

		int len;

		if (Identifier != null && Identifier.Length > 0)
		{
			len = Identifier.Length;
			for (int i = 0; i < len; i++)
			{
				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}.", Identifier[i]);
			}
			stringBuilder.Length--;
		}

		stringBuilder.Append(C_SqlExtension);

		len = stringBuilder.Length;

		for (int j = 0; j < len; j++)
		{
			char c = stringBuilder[j];
			if (!char.IsLetterOrDigit(c) && c != '.' && c != '/')
			{
				stringBuilder[j] = '_';
			}
		}

		stringBuilder.Insert(0, string.Format(CultureInfo.InvariantCulture, "{0}/", "__SQL"));

		return stringBuilder.ToString();
	}


	public string ToPath(string appDataPath)
	{
		string moniker = string.Format(CultureInfo.InvariantCulture, C_PathPrefix, _Server, _Dataset);

		moniker = moniker.Replace("\\", "."); // "{backslash}");
		moniker = moniker.Replace("/", "."); // "{slash}");
		moniker = moniker.Replace(":", "{colon}");
		moniker = moniker.Replace("*", "{asterisk}");
		moniker = moniker.Replace("?", "{questionmark}");
		moniker = moniker.Replace("\"", "{quote}");
		moniker = moniker.Replace("<", "{openbracket}");
		moniker = moniker.Replace(">", "{closebracket}");
		moniker = moniker.Replace("|", "{bar}");

		moniker = $"{_ObjectName}[{moniker}.{_ObjectType}]{C_SqlExtension}";

		string path = GetTemporaryDataDirectory(appDataPath);
		// path = Path.Combine(path, $"{GetNodePathType(_ObjectType)} ({GetNodeScriptProperty(_ObjectType)}s)");
		path = Path.Combine(path, moniker);

		return path;
	}

	public static string GetTemporaryDataDirectory(string appDataPath)
	{
		string path = appDataPath;

		path = Path.Combine(path, C_ServiceFolder);
		path = Path.Combine(path, C_TempSqlFolder);

		return path;
	}



	/// <summary>
	/// Converts an identifier string array to a moniker string - not used
	/// </summary>
	public static string ToString(string[] identifierParts, DataObjectIdentifierFormat format = DataObjectIdentifierFormat.None)
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
				stringBuilder.Append(C_CompositeIdentifierSeparator);
			}
		}

		string text = stringBuilder.ToString().Trim(C_CompositeIdentifierSeparator);
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
