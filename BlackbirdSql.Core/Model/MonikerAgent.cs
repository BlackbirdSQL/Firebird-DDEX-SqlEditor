
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;

using Microsoft.VisualStudio.Data.Services;

using EnNodeSystemType = BlackbirdSql.Core.Ctl.CommandProviders.CommandProperties.EnNodeSystemType;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//
//											MonikerAgent Class
//
/// <summary>
/// This class and it's descendents centrally control monikers for the extension.
/// A BlackbirdSql moniker is based on Server(Dataset) where Dataset is the database file name.
/// </summary>
/// <remarks>
/// Monikers must be uniquely identifiable by the server->databasePath (document moniker - D prefix) and
/// also by the connection equivalency properties (connection moniker C - prefix) if provided, so
/// non-unique datasets are numbered for a session. This means that a node's moniker may differ
/// from one ide session to another.
/// A connection moniker will always have a doument moniker, but a document moniker may not necessarly
/// have a connection moniker. Connection monikers may also share document monikers if the underlying
/// databases are one and the same.
/// Also, changes to a connection's properties must always call OnMonikerPropertiesChanged() otherwise
/// the agent will break. 
/// </remarks>
// =========================================================================================================
public class MonikerAgent : AbstractMonikerAgent
{

	// ---------------------------------------------------------------------------------
	#region Constants - MonikerAgent
	// ---------------------------------------------------------------------------------

	public const string C_SqlExtension = ".fbsql";

	protected const string C_ServiceFolder = "ServerxExplorer";
	protected const string C_TempSqlFolder = "SqlTemporaryFiles";


	#endregion Constants




	// =========================================================================================================
	#region Variables - MonikerAgent
	// =========================================================================================================


	protected readonly bool _IsUnique = false;


	#endregion Variables




	// =========================================================================================================
	#region Property accessors - MonikerAgent
	// =========================================================================================================



	public string DocumentMoniker => ToDocumentMoniker();


	public string[] Identifier => ObjectName.Split('.');



	public object[] OriginalIdentifier => GetOriginalIdentifier();

	public static IDictionary<string, string> RegisteredDatasets => _SDatasetKeys ?? LoadConfiguredConnections();
	public static IDictionary<string, string> RegisteredConnectionMonikers => _SConnectionMonikers ?? LoadConfiguredConnectionMonikers();


	#endregion Property accessors




	// =========================================================================================================
	#region Constructors / Destructors - MonikerAgent
	// =========================================================================================================


	public MonikerAgent(bool isUnique = false, bool alternate = false) : base()
	{
		_IsUnique = isUnique;
		_Alternate = alternate;
	}


	public MonikerAgent(IDbConnection connection) : base(connection)
	{
		_IsUnique = true;
	}


	public MonikerAgent(IBPropertyAgent ci) : base(ci)
	{
		_IsUnique = true;
	}


	public MonikerAgent(IVsDataExplorerNode node, bool isUnique = false, bool alternate = false)
		: base(node)
	{
		_IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		_Alternate = alternate;
	}


	public MonikerAgent(string server, string database, EnModelObjectType objectType,
			IList<string> identifierList, bool isUnique = false, bool alternate = false)
		: base(null, server, database, objectType, identifierList.ToArray<object>())
	{
		_IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		_Alternate = alternate;
	}


	public MonikerAgent(string displayMember, string dataset, string server, int port, EnDbServerType serverType, string database,
			string user, string password, string role, string charset, int dialect, EnModelObjectType objectType, object[] identifier, bool isUnique)
		: base(displayMember, dataset, server, port, serverType, database, user, password, role, charset, dialect, false, objectType, identifier)
	{
		_IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
	}


	/// <summary>
	/// .ctor for use only by XmlParser for registering FlameRobin datasetKeys.
	/// </summary>
	protected MonikerAgent(string displayMember, string server, int port, EnDbServerType serverType, string database,
		string user, string password, string role, string charset, int dialect, bool noTriggers)
		: base(displayMember, server, port, serverType, database, user, password, role, charset, dialect, noTriggers)
	{
		_IsUnique = true;
	}


	/// <summary>
	/// Reserved for the registration of FlameRobin datasetKeys.
	/// </summary>
	public static MonikerAgent RegisterDatasetKey(string displayMember, string server, int port, EnDbServerType serverType,
		string database, string user, string password, string role, string charset, int dialect, bool noTriggers)
	{
		MonikerAgent moniker = new(displayMember, server, port, serverType, database, user, password, role, charset, dialect, false);
		string datasetKey = moniker.RegisterUniqueConnectionDatsetKey(true);

		if (string.IsNullOrWhiteSpace(datasetKey))
			return null;


		return moniker;
	}

	#endregion Property Constructors / Destructors




	// =========================================================================================================
	#region Methods - MonikerAgent
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decorates a DDL raw script to it's executable form.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetDecoratedDdlSource(IVsDataExplorerNode node, bool alternate)
	{
		Tracer.Trace(typeof(MonikerAgent), "GetDecoratedDdlSource()");

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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a full identifier for a node including it's root node
	/// identifier and type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string[] GetFullIdentifier()
	{
		Tracer.Trace(GetType(), "GetFullIdentifier()");

		// 	public const string UrlPrefix = "fbsql://{0}@{1}({2})";

		string[] nodeIdentifier = Identifier;

		string[] identifier = new string[nodeIdentifier.Length + 2];

		identifier[0] = Explorer;
		identifier[1] = ObjectType.ToString();

		for (int i = 0; i < nodeIdentifier.Length; i++)
		{
			identifier[i + 2] = nodeIdentifier[i];
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


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a node's type as reflected in the IVsObjectSupport xml given the node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetNodeBaseType(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			return GetNodeBaseType(node.Object);
		}

		return "";
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a node's type as reflected in the IVsObjectSupport xml given the node's
	/// Object.
	/// </summary>
	// ---------------------------------------------------------------------------------
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a node's system type - System or User.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetNodeSystemType(EnNodeSystemType nodeSystemType)
	{
		if (nodeSystemType == EnNodeSystemType.System)
			return "System";
		else if (nodeSystemType == EnNodeSystemType.User)
			return "User";

		return "";
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Appends the ServerExplorer and SqlTemporaryFiles folders to the provided
	/// application root directory.
	/// </summary>
	/// <param name="appDataPath">
	/// The root temporary application directory path to save to.
	/// </param>
	// ---------------------------------------------------------------------------------
	public static string ConstructFullTemporaryDirectory(string appDataPath)
	{
		string path = appDataPath;

		path = Path.Combine(path, C_ServiceFolder);
		path = Path.Combine(path, C_TempSqlFolder);

		Tracer.Trace(typeof(MonikerAgent), "ConstructFullTemporaryDirectory()", "TemporaryDirectory: {0}", path);

		return path;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a node's identifier in it's original form in the tree.
	/// </summary>
	// ---------------------------------------------------------------------------------
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






	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a document moniker unique to a database independent of connection
	/// settings
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string ToDocumentMoniker(bool prefixOnly = false)
	{
		EnModelObjectType objectType = (!_Alternate || (int)ObjectType >= 20) ? ObjectType : (ObjectType + 20);

		StringBuilder stringBuilder = new StringBuilder(DatabaseMoniker);


		if (!prefixOnly && objectType != EnModelObjectType.Unknown && objectType != EnModelObjectType.AlterUnknown)
		{
			int len;

			stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}/", objectType.ToString());

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
		}

		string result = stringBuilder.ToString();

		Tracer.Trace(GetType(), "ToDocumentMoniker()", "DocumentMoniker: {0}", result);

		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a moniker into a valid file path for saving to and reading from disk.
	/// </summary>
	/// <param name="appDataPath">
	/// The root temporary application directory path to save to.
	/// </param>
	/// <returns>
	/// The file path in the format tempAppFolderPath\ObjectName[server(serializedDatabasePath.ObjectType)].fbsql
	/// </returns>
	// ---------------------------------------------------------------------------------
	public string ToPath(string appDataPath)
	{
		string str = StringUtils.Serialize64(Database);
		// string str = JsonConvert.SerializeObject(Database.ToLowerInvariant());

		string moniker = C_DatasetKeyFmt.FmtRes(Server, str);

		moniker = moniker.Replace("\\", "."); // "{backslash}");
		moniker = moniker.Replace("/", "."); // "{slash}");
		moniker = moniker.Replace(":", "{colon}");
		moniker = moniker.Replace("*", "{asterisk}");
		moniker = moniker.Replace("?", "{questionmark}");
		moniker = moniker.Replace("\"", "{quote}");
		moniker = moniker.Replace("<", "{openbracket}");
		moniker = moniker.Replace(">", "{closebracket}");
		moniker = moniker.Replace("|", "{bar}");

		moniker = $"{ObjectName}[{moniker}.{ObjectType}]{C_SqlExtension}";

		string path = ConstructFullTemporaryDirectory(appDataPath);

		path = Path.Combine(path, moniker);

		Tracer.Trace(GetType(), "ToPath()", path);


		return path;
	}



	// ---------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------
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

	#endregion Methods

}
