
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;
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


	public Moniker(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		_TargetType = targetType;
		_IsUnique = ObjectType == EnModelObjectType.Database;

		IVsDataObject @nodeObj = node.Object;

		if (@nodeObj == null)
		{
			ArgumentNullException ex = new($"{node.Name} Object is null");
			Diag.Dug(ex);
			return;
		}

		EnModelObjectType objType = node.ModelObjectType();

		// Evs.Debug(typeof(Moniker), ".ctor(IVsDataExplorerNode, EnModelTargetType)",
		//	$"Node type for node '{node.Name}' is {objType.ToString()}.");

		IVsDataObject @dbObj;

		if (objType == EnModelObjectType.Database)
			@dbObj = @nodeObj;
		else
			@dbObj = node.ExplorerConnection.ConnectionNode.Object;


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
					_ObjectName += SystemData.C_CompositeSeparator
						+ (identifier[i] != null ? identifier[i].ToString() : "");
				}
				_ObjectName = _ObjectName.Trim(SystemData.C_CompositeSeparator);
			}
		}
	}



	public Moniker(string server, string database, EnModelObjectType objectType,
			IList<string> identifierList, EnModelTargetType targetType, bool isUnique, bool isClone)
	{
		_IsUnique = (ObjectType == EnModelObjectType.Database) || isUnique;
		_TargetType = targetType;
		_IsClone = isClone;

		_DataSource = server;
		_Database = database;

		_ObjectType = objectType;
		_ObjectName = "";

		if (identifierList != null && identifierList.Count > 0)
		{
			_ObjectName = identifierList[0] ?? "";

			for (int i = 1; i < identifierList.Count; i++)
			{
				_ObjectName += SystemData.C_CompositeSeparator
					+ (identifierList[i] ?? "");
			}
		}
	}


	public Moniker(IVsDataExplorerConnection explorerConnection, EnModelTargetType targetType)
	{
		_IsUnique = ObjectType == EnModelObjectType.Database;
		_TargetType = targetType;

		IVsDataObject @nodeObj = explorerConnection.ConnectionNode.Object;

		if (@nodeObj == null)
		{
			ArgumentNullException ex = new($"{explorerConnection.SafeName()} Object is null");
			Diag.Dug(ex);
			return;
		}

		EnModelObjectType objType;

		switch (targetType)
		{
			case EnModelTargetType.QueryScript:
			case EnModelTargetType.CreateScript:
			case EnModelTargetType.AlterScript:
				objType = EnModelObjectType.NewQuery;
				break;
			case EnModelTargetType.DesignData:
				objType = EnModelObjectType.NewDesignerQuery;
				break;
			default:
				objType = EnModelObjectType.NewQuery;
				break;
		}

		string objectName = objType.ToString();


		// Evs.Debug(typeof(Moniker), ".ctor(IVsDataExplorerConnection, EnModelTargetType)",
		//	$"Node type is {objType}.");

		IVsDataObject @dbObj = @nodeObj;


		if (@dbObj != null && @nodeObj != null)
		{
			_DataSource = (string)@dbObj.Properties[SysConstants.C_KeyDataSource];
			_Database = (string)@dbObj.Properties[SysConstants.C_KeyDatabase];

			_ObjectType = objType;
			_ObjectName = objectName;
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - Moniker
	// =========================================================================================================

	
	#endregion Constants





	// =========================================================================================================
	#region Fields - Moniker
	// =========================================================================================================



	private string _Database = null;
	private string _DataSource = null;
	private readonly bool _IsClone = false;
	private readonly bool _IsUnique = false;
	private readonly string _ObjectName = "";
	private readonly EnModelObjectType _ObjectType = EnModelObjectType.Unknown;
	private readonly EnModelTargetType _TargetType = EnModelTargetType.Unknown;
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


	public string[] Identifier => ObjectName.Split(SystemData.C_CompositeSeparator);


	/// <summary>
	/// The dot delimited node object identifier.
	/// </summary>
	private string ObjectName => _ObjectName;


	/// <summary>
	/// The Server Explorer node object type.
	/// </summary>
	public EnModelObjectType ObjectType => _ObjectType;


	/// <summary>
	/// The IDE window target type of the data object.
	/// </summary>
	[DefaultValue(EnModelTargetType.Unknown)]
	private EnModelTargetType TargetType => _TargetType;


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
			Scheme = NativeDb.Protocol,
			Host = string.IsNullOrEmpty(DataSource) ? "nodatasource" : DataSource.ToLowerInvariant(),
		};


		// Append the serialized database path.

		// Serialize the db path.
		string str = Serialization.Serialize64(string.IsNullOrEmpty(Database) ? "nodatabase" : Database.ToLowerInvariant());

		// Evs.Debug(GetType(), "BuildDatabaseUrl()", $"Serialized dbpath: {str}");

		urlb.Path = str + SystemData.C_UnixFieldSeparator;

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

		// Evs.Debug(GetType(), "BuildDatabaseUrl()", $"Url: {result}");

		return result;
	}



	/// <summary>
	/// Builds the uniquely identifiable document moniker. If the moniker required must
	/// be unique, it's uniqueness is established against the moniker filename, not the
	/// full moniker.
	/// </summary>
	private string BuildDocumentMoniker(bool includeExtension)
	{
		StringBuilder stringBuilder = new(DatabaseMoniker);

		if (ObjectType != EnModelObjectType.Unknown)
		{
			int len;

			stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0}_{1}{2}",
				ObjectType.ToString(), TargetType.ToString(),
				Identifier != null && Identifier.Length > 0 ? SystemData.C_UnixFieldSeparator.ToString() : "");

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

		string filenameNoExtension = stringBuilder.ToString();
		string filename = filenameNoExtension;
		string extension = "";

		if (includeExtension && ObjectType != EnModelObjectType.Unknown && Identifier != null)
		{
			extension = NativeDb.Extension;
			filename = string.Format(CultureInfo.InvariantCulture, "{0}{1}", filename, extension);
		}

		string result = filename;

		if (_IsUnique)
		{
			string testname;

			try
			{
				testname = Cmd.GetFileName(filenameNoExtension);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex, $"Serialized database url: {filenameNoExtension}.");
				throw;
			}

			string basename = Cmd.GetUniqueIdentifierPrefix(testname);

			// Test if a clone's parent has a suffix. If it does see if we can use the parent's
			// name for the clone.
			if (_IsClone && basename != testname)
			{
				testname = string.Format(CultureInfo.InvariantCulture, "{0}{1}", testname, extension);

				if (!RdtManager.IsInflightMonikerFilenameRegistered(testname))
					return filename;
			}

			testname = string.Format(CultureInfo.InvariantCulture, "{0}{1}", basename, extension);
			filenameNoExtension = Path.Combine(Cmd.GetDirectoryName(filename), basename);

			for (int i = 2; i < 1000; i++)
			{
				if (!_IsClone || i != 2)
				{
					if (!RdtManager.IsInflightMonikerFilenameRegistered(testname))
						break;
				}

				if (i > 100)
					_UniqueId = DateTime.Now.Ticks;
				else
					_UniqueId = i;

				testname = string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", basename, _UniqueId, extension);
				filename = string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", filenameNoExtension, _UniqueId, extension);
			}

			result = filename;
		}

		// Evs.Debug(GetType(), "BuildDocumentMoniker()", $"Result DocumentMoniker: {result}");

		return result;
	}



	public static string BuildDocumentMoniker(IVsDataExplorerNode node,
		ref IList<string> identifierArray, EnModelTargetType targetType)
	{
		Moniker moniker = new(node, targetType);
		identifierArray = moniker.Identifier;

		return moniker.BuildDocumentMoniker(true);
	}



	public static string BuildDocumentMoniker(string server, string database, EnModelObjectType elementType,
	ref IList<string> identifierArray, EnModelTargetType targetType, bool isUnique, bool isClone)
	{
		Moniker moniker = new(server, database, elementType, identifierArray, targetType, isUnique, isClone);
		identifierArray = moniker.Identifier;

		return moniker.BuildDocumentMoniker(true);
	}


	#endregion Methods

}
