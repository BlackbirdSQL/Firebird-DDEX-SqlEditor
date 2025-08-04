// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Runtime.InteropServices;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.C_NativeDbServerExplorerServiceGuid)]


// =========================================================================================================
//										IBsNativeDbServerExplorerService Interface
//
/// <summary>
/// Interface for native db Server Explorer service.
/// </summary>
// =========================================================================================================
internal interface IBsNativeDbServerExplorerService
{
	string GetDecoratedDdlSource_(IVsDataExplorerNode node, EnModelTargetType targetType);
	int GetObjectTypeIdentifierLength_(string typeName);
}