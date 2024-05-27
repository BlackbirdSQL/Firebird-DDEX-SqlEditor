
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using BlackbirdSql.Sys;
using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//
//											Moniker Class
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
public class Moniker
{

	// -----------------------------------------------------------------------------------------------------
	#region Constructors / Destructors - Moniker
	// -----------------------------------------------------------------------------------------------------


	public Moniker(IVsDataExplorerNode node, EnModelTargetType targetType, bool isUnique = false)
	{
		_IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		_TargetType = targetType;

		Extract(node);
	}


	public Moniker(string server, string database, EnModelObjectType objectType,
			IList<string> identifierList, EnModelTargetType targetType, bool isUnique = false)
	{
		_IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		_TargetType = targetType;

		Initialize(server, database, objectType, [.. identifierList], targetType);
	}


	public Moniker(IVsDataExplorerConnection explorerConnection, EnModelTargetType targetType, bool isUnique = false)
	{
		Extract(explorerConnection, targetType, isUnique);
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
			_DataSource = (string)@dbObj.Properties[SysConstants.C_KeyDataSource];
			_Database = (string)@dbObj.Properties[SysConstants.C_KeyDatabase];

			_ObjectType = objType;
			_ObjectName = objectName;
		}
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


		if (@dbObj != null && @nodeObj != null && @dbObj.Properties != null)
		{
			_DataSource = (string)@dbObj.Properties[SysConstants.C_KeyDataSource];
			_Database = (string)@dbObj.Properties[SysConstants.C_KeyDatabase];

			_ObjectType = objType;

			_ObjectName = "";

			object[] identifier = [.. @nodeObj.Identifier];

			if (identifier != null && identifier.Length > 0)
			{
				_ObjectName = identifier[0] != null ? identifier[0].ToString() : "";
				for (int i = 1; i < identifier.Length; i++)
				{
					_ObjectName += SystemData.CompositeSeparator
						+ (identifier[i] != null ? identifier[i].ToString() : "");
				}
				_ObjectName = _ObjectName.Trim(SystemData.CompositeSeparator);
			}
		}
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
				_ObjectName += SystemData.CompositeSeparator
					+ (identifier[i] != null ? identifier[i].ToString() : "");
			}
		}

		_TargetType = targetType;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - Moniker
	// =========================================================================================================

	private const string C_StdSchemeDelimiterFmt = "{0}://";

	#endregion Constants





	// =========================================================================================================
	#region Fields - Moniker
	// =========================================================================================================



	private string _Database = null;
	private string _DataSource = null;
	private string _ExplorerTreeName = SysConstants.C_DefaultExExplorerTreeName;
	private bool _IsUnique = SysConstants.C_DefaultExIsUnique;
	private string _DocumentMoniker = null;
	private string _ObjectName = SysConstants.C_DefaultExObjectName;
	private EnModelObjectType _ObjectType = SysConstants.C_DefaultExObjectType;
	private EnModelTargetType _TargetType = SysConstants.C_DefaultExTargetType;
	private long _UniqueId;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - Moniker
	// =========================================================================================================



	/// <summary>
	/// The name of the database path on the server.
	/// </summary>
	private string Database => _Database ??= (string)Csb.Describers[SysConstants.C_KeyDatabase].DefaultValue;


		/// <summary>
	/// Returns the unique document moniker url prefix in the form
	/// fbsql://server/database_lc_serialized//
	/// </summary>
	private string DatabaseMoniker => BuildDatabaseUrl();


	/// <summary>
	/// The name of the database host.
	/// </summary>
	private string DataSource => _DataSource ??= (string)Csb.Describers[SysConstants.C_KeyDataSource].DefaultValue;


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



	public string[] Identifier => ObjectName.Split(SystemData.CompositeSeparator);


	/// <summary>
	/// For document monikers, true if reopening a document opens it into a new window,
	/// otherwise for false the same window is used.
	/// </summary>
	private bool IsUnique => _IsUnique;


	public string DocumentMoniker => _DocumentMoniker ??= BuildDocumentMoniker(true, true);



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
	#region Methods - Moniker
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Builds a uniquely identifiable database url.
	/// </summary>
	/// <returns>
	/// The unique url in format: 
	/// fbsql{schemeDelimiter}server/database_lc_serialized/
	/// </returns>
	/// <remarks>
	/// Database urls are used for uniquely naming document monikers and are unique to
	/// a database / database path.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private string BuildDatabaseUrl()
	{
		// We'll use UriBuilder for the url.


		UriBuilder urlb = new()
		{
			Scheme = DbNative.Protocol,
			Host = DataSource.ToLowerInvariant(),
		};


		// Append the serialized database path.

		// Serialize the db path.
		string str = StringUtils.Serialize64(Database.ToLowerInvariant());

		// Tracer.Trace(GetType(), "BuildStorageDatabaseUrl()", "Serialized dbpath: {0}", str);

		urlb.Path = str + SystemData.UnixFieldSeparator;

		string result;

		try
		{
			result = urlb.Uri.ToString();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		// Tracer.Trace(GetType(), "BuildDatabaseUrl()", "Url: {0}", result);

		return result;
	}



	private string BuildDocumentMoniker(bool includeExtension, bool canBeUnique)
	{
		// Imitates ...
		// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
		// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager.OpenOnlineEditorImpl()

		StringBuilder stringBuilder = new(DatabaseMoniker);

		if (ObjectType != EnModelObjectType.Unknown)
		{
			int len;

			stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}_{1}{2}",
				ObjectType.ToString(), TargetType.ToString(),
				Identifier != null && Identifier.Length > 0 ? SystemData.UnixFieldSeparator.ToString() : "");

			if (Identifier != null && Identifier.Length > 0)
			{
				len = Identifier.Length;
				for (int i = 0; i < len; i++)
				{
					stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}.", Identifier[i]);
				}
				stringBuilder.Length--;
			}

		}

		string filename = stringBuilder.ToString();
		string fullname = filename;
		string extension = "";

		if (includeExtension && ObjectType != EnModelObjectType.Unknown && Identifier != null)
		{
			extension = DbNative.Extension;
			fullname += extension;
		}

		string result = fullname;


		if (IsUnique && canBeUnique)
		{
			for (int i = 2; i < 1000; i++)
			{
				if (!RdtManager.IsInflightMonikerRegistered(fullname))
					break;

				if (i > 100)
					_UniqueId = DateTime.Now.Ticks;
				else
					_UniqueId = i;

				fullname = string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", filename, _UniqueId, extension);
			}
			result = fullname;
		}

		// Tracer.Trace(GetType(), "BuildDocumentMoniker()", "Result DocumentMoniker: {0}", result);

		return result;
	}



	public static string BuildDocumentMoniker(IVsDataExplorerNode node,
	ref IList<string> identifierArray, EnModelTargetType targetType, bool isUnique)
	{
		Moniker moniker = new(node, targetType, isUnique);
		identifierArray = moniker.Identifier;

		return moniker.BuildDocumentMoniker(true, true);
	}



	public static string BuildDocumentMoniker(string server, string database, EnModelObjectType elementType,
	ref IList<string> identifierArray, EnModelTargetType targetType, bool isUnique)
	{
		Moniker moniker = new(server, database, elementType, identifierArray, targetType, isUnique);
		identifierArray = moniker.Identifier;

		return moniker.BuildDocumentMoniker(true, true);
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

		path = Path.Combine(path, SystemData.ServiceFolder);
		path = Path.Combine(path, SystemData.TempSqlFolder);

		// Tracer.Trace(typeof(Moniker), "ConstructFullTemporaryDirectory()", "TemporaryDirectory: {0}", path);

		return path;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decorates a DDL raw script to it's executable form.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetDecoratedDdlSource(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		// Tracer.Trace(typeof(Moniker), "GetDecoratedDdlSource()");

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
					+ $"{GetTriggerEventType((long)obj.Properties["TRIGGER_TYPE"])} POSITION {(short)obj.Properties["PRIORITY"]}\n"
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
							+ DbNative.ConvertDataTypeToSql(child.Object.Properties["FIELD_DATA_TYPE"],
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
							+ DbNative.ConvertDataTypeToSql(child.Object.Properties["FIELD_DATA_TYPE"],
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

		string moniker = SysConstants.C_DatasetKeyFmt.FmtRes(DataSource, str);

		moniker = moniker.Replace("\\", "."); // "{backslash}");
		moniker = moniker.Replace("/", "."); // "{slash}");
		moniker = moniker.Replace(":", "{colon}");
		moniker = moniker.Replace("*", "{asterisk}");
		moniker = moniker.Replace("?", "{questionmark}");
		moniker = moniker.Replace("\"", "{quote}");
		moniker = moniker.Replace("<", "{openbracket}");
		moniker = moniker.Replace(">", "{closebracket}");
		moniker = moniker.Replace("|", "{bar}");

		moniker = $"{ObjectName}[{moniker}.{ObjectType}]{DbNative.Extension}";

		string path = ConstructFullTemporaryDirectory(appDataPath);

		path = Path.Combine(path, moniker);

		// Tracer.Trace(GetType(), "ToPath()", path);


		return path;
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

	#endregion Methods

}
