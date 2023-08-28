#region Assembly Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;

using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;

namespace BlackbirdSql.Common.Interfaces;


public interface IServerConnectionProvider : IBExportable
{
	Guid ServerType { get; }

	string GetConnectionString(UIConnectionInfo ci, IBServerDefinition serverDefinition);

	IDbConnection CreateConnection(UIConnectionInfo ci, IBServerDefinition serverDefinition);

	IDbConnection CreateConnection(string connectionString);
}
