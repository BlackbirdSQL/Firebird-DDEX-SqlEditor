// Microsoft.VisualStudio.Data.Tools.Design, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.ISqlServerObjectExplorerService

using System.Data.SqlClient;
using System.Runtime.InteropServices;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Services;

[ComImport]
[Guid(SystemData.DesignerExplorerServicesGuid)]
public interface IBDesignerExplorerServices
{
	void ViewCode(IVsDataExplorerNode node, bool alternate);

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
