
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Extensions;

using Microsoft.VisualStudio.Data.Services;

using EnNodeSystemType = BlackbirdSql.Core.Ctl.CommandProviders.CommandProperties.EnNodeSystemType;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//
//											MonikerAgent Class
//
/// <summary>
/// This class and it's descendents centrally control document monikers for the extension.
/// A BlackbirdSql moniker is a url (fbsql://) based on the Server, Database and identifier keys of a
/// document.
/// </summary>
/// <remarks>
/// Monikers must be uniquely identifiable by the server->databasePath document moniker prefix and
/// also by the identifier keys of a document.
/// </remarks>
// =========================================================================================================
public class MonikerAgent
{

	// ---------------------------------------------------------------------------------
	#region Constants - MonikerAgent
	// ---------------------------------------------------------------------------------

	public const string C_SqlExtension = ".fbsql";
	protected const string C_ServiceFolder = "ServerxExplorer";
	protected const string C_TempSqlFolder = "SqlTemporaryFiles";
	private const string C_Scheme = "fbsql";
	protected const string C_DatasetKeyFmt = "{0} ({1})";
	private const char C_CompositeSeparator = '.';

	#endregion Constants




	// =========================================================================================================
	#region Variables - MonikerAgent
	// =========================================================================================================


	private bool _Alternate = ModelConstants.C_DefaultExAlternate;
	private string _Database = CoreConstants.C_DefaultDatabase;
	private string _DataSource = CoreConstants.C_DefaultDataSource;
	private string _ExplorerTreeName = ModelConstants.C_DefaultExExplorerTreeName;
	private bool _IsUnique = ModelConstants.C_DefaultExIsUnique;
	private string _ObjectName = ModelConstants.C_DefaultExObjectName;
	private EnModelObjectType _ObjectType = ModelConstants.C_DefaultExObjectType;

	#endregion Variables




	// =========================================================================================================
	#region Property accessors - MonikerAgent
	// =========================================================================================================


	[Category("Extended")]
	[DisplayName("Alternate")]
	[Description("True if the editor content of a document moniker has been decorated with it's alternate form, usually it's Alter format.")]
	[DefaultValue(ModelConstants.C_DefaultExAlternate)]
	public bool Alternate
	{
		get { return _Alternate; }
		set { _Alternate = value; }
	}


	[Category("Source")]
	[DisplayName("Database")]
	[Description("The name of the database path on the server.")]
	[DefaultValue(CoreConstants.C_DefaultDatabase)]
	public string Database
	{
		get { return _Database; }
		set { _Database = value; }
	}


	/// <summary>
	/// Returns the unique document moniker url prefix in the form
	/// fbsql://server/database_lc_serialized//
	/// </summary>
	[Browsable(false)]
	protected string DatabaseMoniker => BuildUniqueDatabaseUrl();


	[Category("Source")]
	[DisplayName("DataSource")]
	[Description("The name of the database host.")]
	[DefaultValue(CoreConstants.C_DefaultDataSource)]
	public string DataSource
	{
		get { return _DataSource; }
		set { _DataSource = value; }
	}


	[Browsable(false)]
	public string DocumentMoniker => ToDocumentMoniker();


	[Category("Extended")]
	[DisplayName("Explorer Tree Name")]
	[Description("The display name of the explorer connection tree.")]
	[DefaultValue(ModelConstants.C_DefaultExExplorerTreeName)]
	public string ExplorerTreeName
	{
		get { return _ExplorerTreeName; }
		set { _ExplorerTreeName = value; }
	}


	[Browsable(false)]
	public string[] Identifier => ObjectName.Split(C_CompositeSeparator);


	[Category("Extended")]
	[DisplayName("IsUnique")]
	[Description("For document monikers, true if reopening a document opens it into a new window, otherwise for false the same window is used.")]
	[DefaultValue(ModelConstants.C_DefaultExIsUnique)]
	public bool IsUnique
	{
		get { return _IsUnique; }
		set { _IsUnique = value; }
	}


	[Category("Extended")]
	[DisplayName("ObjectName")]
	[Description("The dot delimited node object identifier.")]
	[DefaultValue(ModelConstants.C_DefaultExObjectName)]
	protected string ObjectName
	{
		get { return _ObjectName; }
		set { _ObjectName = value; }
	}


	[Category("Extended")]
	[DisplayName("ObjectType")]
	[Description("The Server Explorer node object type.")]
	[DefaultValue(ModelConstants.C_DefaultExObjectType)]
	public EnModelObjectType ObjectType
	{
		get { return _ObjectType; }
		set { _ObjectType = value; }
	}


	[Browsable(false)]
	public object[] OriginalIdentifier => GetOriginalIdentifier();


	#endregion Property accessors




	// =========================================================================================================
	#region Constructors / Destructors - MonikerAgent
	// =========================================================================================================


	public MonikerAgent(IVsDataExplorerNode node, bool isUnique = false, bool alternate = false)
	{
		Extract(node);

		IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		Alternate = alternate;
	}


	public MonikerAgent(string server, string database, EnModelObjectType objectType,
			IList<string> identifierList, bool isUnique = false, bool alternate = false)
	{
		IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		Alternate = alternate;

		Initialize(server, database, objectType, identifierList.ToArray<object>());
	}


	#endregion Property Constructors / Destructors




	// =========================================================================================================
	#region Methods - MonikerAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable database url. Database urls are used for
	/// uniquely naming document monikers and are unique to a database / database path.
	/// </summary>
	/// <returns>
	/// The unique database url in format:
	/// fbsql://server/database_lc_serialized//
	/// </returns>
	// ---------------------------------------------------------------------------------
	protected string BuildUniqueDatabaseUrl()
	{
		// We'll use UriBuilder for the url.

		UriBuilder urlb = new()
		{
			Scheme = C_Scheme,
			Host = DataSource,
		};


		// Append the serialized database path.

		// Serialize the db path.
		string str = StringUtils.Serialize64(Database);
		// string str = string.IsNullOrEmpty(_Database) ? "" : JsonConvert.SerializeObject(_Database.ToLowerInvariant());

		// Tracer.Trace(GetType(), "BuildUniqueDatabaseUrl()", "Serialized dbpath: {0}", str);

		StringBuilder stringBuilder = new(str);
		stringBuilder.Append("//");

		urlb.Path = stringBuilder.ToString();

		string result = urlb.Uri.ToString();

		// Tracer.Trace(GetType(), "BuildUniqueDatabaseUrl()", "Url: {0}", result);

		// We have a unique connection url
		return result;
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

		// Tracer.Trace(typeof(MonikerAgent), "ConstructFullTemporaryDirectory()", "TemporaryDirectory: {0}", path);

		return path;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Extracts moniker information from a Server Explorer node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void Extract(IVsDataExplorerNode node)
	{
		// Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)");

		IVsDataObject @nodeObj = node.Object;

		if (@nodeObj == null)
		{
			ArgumentNullException ex = new($"{node.Name} Object is null");
			Diag.Dug(ex);
			return;
		}

		EnModelObjectType objType = nodeObj.Type.ToModelObjectType();
		if (objType == EnModelObjectType.AlterDatabase)
			objType = EnModelObjectType.Database;

		// Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)", "Node type is {0}.", objType);

		IVsDataObject @dbObj;

		if (objType == EnModelObjectType.Database)
			@dbObj = @nodeObj;
		else
			@dbObj = node.ExplorerConnection.ConnectionNode.Object;

		ExplorerTreeName = node.ExplorerConnection.DisplayName;

		if (@dbObj != null && @nodeObj != null)
		{
			DataSource = (string)@dbObj.Properties[CoreConstants.C_KeyDataSource];
			Database = (string)@dbObj.Properties[CoreConstants.C_KeyDatabase];

			ObjectType = objType;

			ObjectName = "";

			object[] identifier = @nodeObj.Identifier.ToArray();

			if (identifier != null && identifier.Length > 0)
			{
				ObjectName = identifier[0] != null ? identifier[0].ToString() : "";
				for (int i = 1; i < identifier.Length; i++)
				{
					ObjectName += C_CompositeSeparator
						+ (identifier[i] != null ? identifier[i].ToString() : "");
				}
				ObjectName = ObjectName.Trim(C_CompositeSeparator);
			}
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decorates a DDL raw script to it's executable form.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetDecoratedDdlSource(IVsDataExplorerNode node, bool alternate)
	{
		// Tracer.Trace(typeof(MonikerAgent), "GetDecoratedDdlSource()");

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
					direction = Convert.ToInt32(child.Object.Properties["PARAMETER_DIRECTION"]);

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
		// Tracer.Trace(GetType(), "GetFullIdentifier()");

		// 	public const string UrlPrefix = "fbsql://{0}@{1}({2})";

		string[] nodeIdentifier = Identifier;

		string[] identifier = new string[nodeIdentifier.Length + 2];

		identifier[0] = ExplorerTreeName;
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



	private void Initialize(string server, string database, EnModelObjectType objectType, object[] identifier)
	{
		DataSource = server;
		Database = database;

		ObjectType = objectType;
		ObjectName = "";

		if (identifier != null && identifier.Length > 0)
		{
			ObjectName = identifier[0] != null ? identifier[0].ToString() : "";
			for (int i = 1; i < identifier.Length; i++)
			{
				ObjectName += C_CompositeSeparator
					+ (identifier[i] != null ? identifier[i].ToString() : "");
			}
		}

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
		EnModelObjectType objectType = (!Alternate || (int)ObjectType >= 20) ? ObjectType : (ObjectType + 20);

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
				if (!char.IsLetterOrDigit(c) && c != C_CompositeSeparator && c != '/')
				{
					stringBuilder[j] = '_';
				}
			}
		}

		string result = stringBuilder.ToString();

		// Tracer.Trace(GetType(), "ToDocumentMoniker()", "DocumentMoniker: {0}", result);

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

		string moniker = C_DatasetKeyFmt.FmtRes(DataSource, str);

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

		// Tracer.Trace(GetType(), "ToPath()", path);


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
