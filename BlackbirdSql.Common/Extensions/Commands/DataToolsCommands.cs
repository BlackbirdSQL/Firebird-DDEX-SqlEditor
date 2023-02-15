﻿using System;
using System.ComponentModel.Design;



namespace BlackbirdSql.Common.Extensions.Commands;


// =========================================================================================================
//											DataToolsCommands Class
//
/// <summary>
/// Class containing constants and statics for use in data tools commands. 
/// </summary>
// =========================================================================================================
public static class DataToolsCommands
{
	#region Statics


	/// <summary>
	/// The package-wide flag indicating whether or not the current node in the SE is a 'IsSystemObject'
	/// </summary>
	public static DataObjectType CommandObjectType = DataObjectType.None;

	/// <summary>
	/// The package-wide flag indicating the last command type
	/// </summary>
	public static DataObjectType CommandLastObjectType = DataObjectType.None;

	#endregion Statics





	// =========================================================================================================
	#region GUIDs - DataToolsCommands
	// =========================================================================================================


	public const string SystemQueryCommandProviderGuid = "C253F0FC-D24B-4BE4-A2DE-57502D677A09";
	public const string UserQueryCommandProviderGuid = "B07EDD71-7F1E-4A14-8BB0-38A30C72251D";
	public const string UniversalQueryCommandProviderGuid = "CD332A3B-B404-45B7-988F-587672086727";
	public const string NativeMethodsGuid = "6E7A99C4-ED01-4EAC-972E-AEC9080638E6";

	public const string MenuGroupGuid = "501822E1-B5AF-11d0-B4DC-00A0C91506EF";
	public const string MenuGroupDavGuid = "732ABE75-CD80-11d0-A2DB-00AA00A3EFFF";

	#endregion GUIDs





	// =========================================================================================================
	#region IDs - DataToolsCommands
	// =========================================================================================================


	private const int icmdSENewQuery = 13587;
	private const int icmdSELocalNewQuery = 13608;
	private const int cmdidAddTableViewForQRY = 39;
	private const int icmdSEEditTextObject = 12385;
	private const int icmdSERetrieveData = 12384;
	private const int icmdSERun = 12386;


	#endregion IDs





	// =========================================================================================================
	#region Command IDs - DataToolsCommands
	// =========================================================================================================


	public static CommandID GlobalNewQuery = new CommandID(new Guid(MenuGroupGuid), icmdSENewQuery);
	public static CommandID NewQuery = new CommandID(new Guid(MenuGroupGuid), icmdSELocalNewQuery);
	public static CommandID ShowAddTableDialog = new CommandID(new Guid(MenuGroupDavGuid), cmdidAddTableViewForQRY);
	public static CommandID OpenTextObject = new CommandID(new Guid(MenuGroupGuid), icmdSEEditTextObject);
	public static CommandID RetrieveData = new CommandID(new Guid(MenuGroupGuid), icmdSERetrieveData);
	public static CommandID ExecuteTextObject = new CommandID(new Guid(MenuGroupGuid), icmdSERun);


	#endregion Command IDs





	// =========================================================================================================
	#region Child classes - DataToolsCommands
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enumerator for flagging the current SE node as 'IsSystemObject' or not
	/// </summary>
	// ---------------------------------------------------------------------------------
	public enum DataObjectType
	{
		None = 0,
		User = 1,
		System = 2
	};


	#endregion Child classes

}