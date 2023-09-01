// Microsoft.VisualStudio.Data.Tools.Design, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.ISqlServerObjectExplorerService

using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;



[ComImport]
[Guid(SystemData.DesignerOnlineServicesGuid)]
public interface IBDesignerOnlineServices
{
	void ViewCode(DbConnectionStringBuilder connection, EnModelObjectType objectType, bool alternate, IList<string> identifierList, string script);

	/*
	void Add(SqlConnectionStringBuilder connection, SqlServerObjectType objectType);

	void Browse(SqlConnectionStringBuilder connection);

	void Delete(SqlConnectionStringBuilder connection, SqlServerObjectType objectType, params string[] name);

	void Execute(SqlConnectionStringBuilder connection, params string[] name);

	void OpenQuery(SqlConnectionStringBuilder connection);

	void ViewData(SqlConnectionStringBuilder connection, SqlServerObjectType objectType, params string[] name);

	void ViewDesigner(SqlConnectionStringBuilder connection, SqlServerObjectType objectType, params string[] name);
	*/
}
