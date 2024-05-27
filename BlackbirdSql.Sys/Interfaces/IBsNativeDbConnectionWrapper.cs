// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;



namespace BlackbirdSql.Sys;

[Guid(LibraryData.NativeDbConnectionWrapperServiceGuid)]


// =========================================================================================================
//									IBsNativeDbConnectionWrapper Interface
/// <summary>
/// Interface for native DbConnectionWrapper extension methods service.
/// </summary>
// =========================================================================================================
public interface IBsNativeDbConnectionWrapper
{
	string DataSource { get; }
	string ServerVersion { get; }

	event EventHandler<DbInfoMessageEventArgs> InfoMessageEvent;


	DbConnection CloneAndOpenConnection();

}