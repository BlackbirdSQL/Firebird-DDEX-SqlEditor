
using System;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.Data.Services;

namespace BlackbirdSql.Common.Providers;


internal class SqlMonikerHelper
{
	public const string Prefix = "fbsql:://";

	public const string PrefixFormat = "fbsql:://{0}/{1}/{2}";

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
			identifier[i+2] = keys[i];

		return identifier;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the Source script of a node if it exists else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetNodeScriptProperty(IVsDataObject obj)
	{
		if (obj.Type.Name.EndsWith("Column") || obj.Type.Name.EndsWith("Trigger")
			 || obj.Type.Name == "Index" || obj.Type.Name == "ForeignKey")
		{
			return "EXPRESSION";
		}
		else if (obj.Type.Name == "View")
		{
			return "DEFINITION";
		}
		else if (obj.Type.Name == "StoredProcedure" || obj.Type.Name == "Function")
		{
			return "SOURCE";
		}

		return null;
	}



	public void Parse(string fbSqlUrl)
	{
		if (fbSqlUrl.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
		{
			string[] array = fbSqlUrl[Prefix.Length..].Split('/');
			_Server = array[0];
			_Database = array[1];
			_User = array[2];
			_ObjectType = null;
			_ObjectName = null;

			if (array.Length > 3)
			{
				_ObjectType = UrlTypeToObjectType(array[3]);
			}

			if (array.Length > 4)
			{
				_ObjectName = array[4];
			}
		}
	}


	public override string ToString()
	{
		StringBuilder stringBuilder = new(string.Format(CultureInfo.InvariantCulture, "{0}{1}/{2}/{3}", Prefix, _Server, _Database, _User));

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
}
