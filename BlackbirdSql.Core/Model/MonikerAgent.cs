
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model.Enums;
using Microsoft.VisualStudio.Data.Services;


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
	#region Fields - MonikerAgent
	// =========================================================================================================


	private string _Database = CoreConstants.C_DefaultDatabase;
	private string _DataSource = CoreConstants.C_DefaultDataSource;
	private string _ExplorerTreeName = ModelConstants.C_DefaultExExplorerTreeName;
	private bool _IsUnique = ModelConstants.C_DefaultExIsUnique;
	private string _MiscDocumentMoniker = null;
	private string _MiscDocumentMonikerPath = null;
	private string _ObjectName = ModelConstants.C_DefaultExObjectName;
	private EnModelObjectType _ObjectType = ModelConstants.C_DefaultExObjectType;
	private EnModelTargetType _TargetType = ModelConstants.C_DefaultExTargetType;
	private long _UniqueId;

	#endregion Fields







	// =========================================================================================================
	#region Property accessors - MonikerAgent
	// =========================================================================================================


	/// <summary>
	/// The name of the database path on the server.
	/// </summary>
	private string Database => _Database;


	/// <summary>
	/// Returns the unique document moniker url prefix in the form
	/// fbsql://server/database_lc_serialized//
	/// </summary>
	private string DatabaseMoniker => BuildUniqueDatabaseUrl();


	/// <summary>
	/// The name of the database host.
	/// </summary>
	private string DataSource => _DataSource;


	/// <summary>
	/// The display name of the explorer connection tree.
	/// </summary>
	private string ExplorerTreeName => _ExplorerTreeName;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a full identifier for a node including it's root node
	/// identifier and type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public string[] FullIdentifier
	{
		get
		{
			// Tracer.Trace(GetType(), "GetFullIdentifier()");

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
	}



	public string[] Identifier => ObjectName.Split(C_CompositeSeparator);


	/// <summary>
	/// For document monikers, true if reopening a document opens it into a new window,
	/// otherwise for false the same window is used.
	/// </summary>
	private bool IsUnique => _IsUnique;


	public string MiscDocumentMoniker => _MiscDocumentMoniker ??= BuildMiscDocumentMoniker(false);

	public string MiscDocumentMonikerPath => _MiscDocumentMonikerPath ??= BuildMiscDocumentMoniker(true);


	/// <summary>
	/// The dot delimited node object identifier.
	/// </summary>
	private string ObjectName => _ObjectName;


	/// <summary>
	/// The Server Explorer node object type.
	/// </summary>
	public EnModelObjectType ObjectType => _ObjectType;


	public object[] OriginalIdentifier
	{
		get
		{
			object[] nodeIdentifier = Identifier;

			string[] identifier = new string[nodeIdentifier.Length + 2];

			identifier[0] = identifier[1] = "";

			for (int i = 0; i < nodeIdentifier.Length; i++)
			{
				identifier[i + 2] = nodeIdentifier[i]?.ToString();
			}

			return identifier;
		}
	}


	/// <summary>
	/// The IDE window target type of the data object.
	/// </summary>
	[DefaultValue(EnModelTargetType.Unknown)]
	private EnModelTargetType TargetType => _TargetType;



	public long UniqueId => _UniqueId;



	#endregion Property accessors




	// =========================================================================================================
	#region Constructors / Destructors - MonikerAgent
	// =========================================================================================================


	public MonikerAgent(IVsDataExplorerNode node, EnModelTargetType targetType, bool isUnique = false)
	{
		_IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		_TargetType = targetType;

		Extract(node);
	}


	public MonikerAgent(string server, string database, EnModelObjectType objectType,
			IList<string> identifierList, EnModelTargetType targetType, bool isUnique = false)
	{
		_IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		_TargetType = targetType;

		Initialize(server, database, objectType, [.. identifierList], targetType);
	}


	public MonikerAgent(IVsDataExplorerConnection explorerConnection, EnModelTargetType targetType, bool isUnique = false)
	{
		Extract(explorerConnection, targetType, isUnique);
	}


	#endregion Property Constructors / Destructors




	// =========================================================================================================
	#region Methods - MonikerAgent
	// =========================================================================================================


	private string BuildMiscDocumentMoniker(bool includeExtension)
	{
		string text = BuildUnsafeDocumentMoniker(includeExtension);
		string extension = includeExtension ? Path.GetExtension(text) : "";
		string name = includeExtension
			? text[..text.LastIndexOf(extension, StringComparison.OrdinalIgnoreCase)]
			: text;
		string result = text;

		// Tracer.Trace(GetType(), "BuildMiscDocumentMoniker()", "Split Misc DocumentMoniker name:{0}, extension: {1}.", name, extension);

		if (IsUnique)
		{
			for (int i = 2; i < 1000; i++)
			{
				if (!RdtManager.Instance.IsFileInRdt(result))
					break;

				if (i > 100)
					_UniqueId = DateTime.Now.Ticks;
				else
					_UniqueId = i;

				result = string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", name, _UniqueId, extension);
			}
		}
		else
		{
			result = string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, extension);
		}

		// Tracer.Trace(GetType(), "BuildMiscDocumentMoniker()", "Result Misc DocumentMoniker: {0}", result);

		return result;
	}



	public static string BuildMiscDocumentMonikerPath(IVsDataExplorerNode node,
		ref IList<string> identifierArray, EnModelTargetType targetType, bool isUnique)
	{
		MonikerAgent moniker = new(node, targetType, isUnique);
		identifierArray = moniker.Identifier;

		// Tracer.Trace(typeof(MonikerAgent), "BuildMiscDocumentMoniker(IVsDataExplorerNode)", "ObjectName: {0}, ", moniker.ObjectName, moniker.ObjectType.ToString());

		string result = moniker.BuildMiscDocumentMoniker(true);

		// Tracer.Trace(typeof(MonikerAgent), "BuildMiscDocumentMoniker(IVsDataExplorerNode)", "DocumentMoniker: {0}", result);

		return result;
	}



	public static string BuildMiscDocumentMonikerPath(string server, string database, EnModelObjectType elementType,
		ref IList<string> identifierArray, EnModelTargetType targetType, bool isUnique)
	{
		MonikerAgent moniker = new(server, database, elementType, identifierArray, targetType, isUnique);
		identifierArray = moniker.Identifier;

		string result = moniker.BuildMiscDocumentMoniker(true);

		// Tracer.Trace(typeof(MonikerAgent), "BuildMiscDocumentMoniker(server, database, identifierArray)", "DocumentMoniker: {0}", result);

		return result;
	}


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
	private string BuildUniqueDatabaseUrl()
	{
		// We'll use UriBuilder for the url.

		UriBuilder urlb = new()
		{
			Scheme = C_Scheme,
			Host = DataSource.ToLowerInvariant(),
		};


		// Append the serialized database path.

		// Serialize the db path.
		string str = StringUtils.Serialize64(Database.ToLowerInvariant());
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
	private string ConstructFullTemporaryDirectory(string appDataPath)
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

		EnModelObjectType objType = node.ModelObjectType();

		// Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)", "Node type for node '{0}' is {1}.", node.Name, objType.ToString());

		IVsDataObject @dbObj;

		if (objType == EnModelObjectType.Database)
			@dbObj = @nodeObj;
		else
			@dbObj = node.ExplorerConnection.ConnectionNode.Object;

		_ExplorerTreeName = node.ExplorerConnection.DisplayName;

		if (@dbObj != null && @nodeObj != null)
		{
			_DataSource = (string)@dbObj.Properties[CoreConstants.C_KeyDataSource];
			_Database = (string)@dbObj.Properties[CoreConstants.C_KeyDatabase];

			_ObjectType = objType;

			_ObjectName = "";

			object[] identifier = [.. @nodeObj.Identifier];

			if (identifier != null && identifier.Length > 0)
			{
				_ObjectName = identifier[0] != null ? identifier[0].ToString() : "";
				for (int i = 1; i < identifier.Length; i++)
				{
					_ObjectName += C_CompositeSeparator
						+ (identifier[i] != null ? identifier[i].ToString() : "");
				}
				_ObjectName = _ObjectName.Trim(C_CompositeSeparator);
			}
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Extracts moniker information from a Server Explorer connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void Extract(IVsDataExplorerConnection explorerConnection, EnModelTargetType targetType, bool isUnique = false)
	{
		// Tracer.Trace(GetType(), "Extract(IVsDataExplorerConnection)");

		_IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		_TargetType = targetType;

		IVsDataObject @nodeObj = explorerConnection.ConnectionNode.Object;

		if (@nodeObj == null)
		{
			ArgumentNullException ex = new($"{explorerConnection.DisplayName} Object is null");
			Diag.Dug(ex);
			return;
		}

		EnModelObjectType objType;

		switch (targetType)
		{
			case EnModelTargetType.QueryScript:
			case EnModelTargetType.AlterScript:
				objType = EnModelObjectType.NewSqlQuery;
				break;
			case EnModelTargetType.DesignData:
				objType = EnModelObjectType.NewDesignerQuery;
				break;
			default:
				objType = EnModelObjectType.NewSqlQuery;
				break;
		}

		string objectName = objType.ToString();


		// Tracer.Trace(GetType(), "Extract(IVsDataExplorerNode)", "Node type is {0}.", objType);

		IVsDataObject @dbObj = @nodeObj;

		_ExplorerTreeName = explorerConnection.DisplayName;

		if (@dbObj != null && @nodeObj != null)
		{
			_DataSource = (string)@dbObj.Properties[CoreConstants.C_KeyDataSource];
			_Database = (string)@dbObj.Properties[CoreConstants.C_KeyDatabase];

			_ObjectType = objType;
			_ObjectName = objectName;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decorates a DDL raw script to it's executable form.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetDecoratedDdlSource(IVsDataExplorerNode node, EnModelTargetType targetType)
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


		switch (node.NodeBaseType())
		{
			case EnModelObjectType.Trigger:
				string active = (bool)obj.Properties["IS_INACTIVE"] ? "INACTIVE" : "ACTIVE";
				if (targetType == EnModelTargetType.AlterScript)
					str = $"ALTER TRIGGER {obj.Properties["TRIGGER_NAME"]}";
				else
					str = $"CREATE TRIGGER {obj.Properties["TRIGGER_NAME"]} FOR {obj.Properties["TABLE_NAME"]}";

				src = $"{str} {active}\n"
					+ $"{GetTriggerEvent((long)obj.Properties["TRIGGER_TYPE"])} POSITION {(short)obj.Properties["PRIORITY"]}\n"
					+ src;
				return src;
			case EnModelObjectType.Table:
				src = $"SELECT * FROM {src.ToUpperInvariant()}";
				return src;
			case EnModelObjectType.View:
				if (targetType != EnModelTargetType.AlterScript)
					return src;

				nodes = node.GetChildren(false);
				foreach (IVsDataExplorerNode child in nodes)
					str += (str != "" ? ",\n\t" : "\n\t(") + child.Object.Properties["COLUMN_NAME"];

				if (str != "")
					str += ")";
				src = $"ALTER VIEW {obj.Properties["VIEW_NAME"]}{str}\nAS\n{src}";
				return src;
			case EnModelObjectType.StoredProcedure:
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
						if (targetType == EnModelTargetType.AlterScript)
							str += (str != "" ? ",\n\t" : "\n\t(") + flddef;
						else
							str += (str != "" ? "\n" : "\n-- The following input parameters need to be initialized after the BEGIN statement\n")
								+ "DECLARE VARIABLE " + flddef + ";";
					}
					if (direction > 0) // Out
						strout += (strout != "" ? ",\n\t" : "\n\t(") + flddef;
				}
				if (str != "" && targetType == EnModelTargetType.AlterScript)
					str += ")";
				if (strout != "")
					strout = $"\nRETURNS{strout})";

				if (strout != "" && targetType == EnModelTargetType.AlterScript)
					str += strout;

				if (targetType == EnModelTargetType.AlterScript)
					str = $"ALTER PROCEDURE {obj.Properties["PROCEDURE_NAME"]}{str}\n";
				else
					strout = $"EXECUTE BLOCK{strout}\n";

				if (targetType == EnModelTargetType.AlterScript)
					src = $"{str}AS\n{src}";
				else
					src = $"{strout}AS{str}\n-- End of parameter declarations\n{src}";

				return src;
			case EnModelObjectType.Function:
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

				if (targetType == EnModelTargetType.AlterScript)
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



	private void Initialize(string server, string database, EnModelObjectType objectType, object[] identifier, EnModelTargetType targetType)
	{
		_DataSource = server;
		_Database = database;

		_ObjectType = objectType;
		_ObjectName = "";

		if (identifier != null && identifier.Length > 0)
		{
			_ObjectName = identifier[0] != null ? identifier[0].ToString() : "";
			for (int i = 1; i < identifier.Length; i++)
			{
				_ObjectName += C_CompositeSeparator
					+ (identifier[i] != null ? identifier[i].ToString() : "");
			}
		}

		_TargetType = targetType;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a document moniker unique to a database independent of connection
	/// settings
	/// </summary>
	// ---------------------------------------------------------------------------------
	private string BuildUnsafeDocumentMoniker(bool includeExtension, bool prefixOnly = false)
	{
		StringBuilder stringBuilder = new StringBuilder(DatabaseMoniker);


		if (!prefixOnly && ObjectType != EnModelObjectType.Unknown)
		{
			int len;

			stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}_{1}/", ObjectType.ToString(), TargetType.ToString());

			if (Identifier != null && Identifier.Length > 0)
			{
				len = Identifier.Length;
				for (int i = 0; i < len; i++)
				{
					stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}.", Identifier[i]);
				}
				stringBuilder.Length--;
			}

			if (includeExtension)
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

		// Tracer.Trace(GetType(), "BuildUnsafeDocumentMoniker()", "DocumentMoniker: {0}", result);

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
		string str = StringUtils.Serialize64(Database.ToLowerInvariant());
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
	private static string GetTriggerEvent(long eventType)
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
