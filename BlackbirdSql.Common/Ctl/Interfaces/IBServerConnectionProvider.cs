#region Assembly Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBServerConnectionProvider // : IBExportable
{
	Guid ServerType { get; }

	string GetConnectionString(UIConnectionInfo ci, EnEngineType serverEngine);

	IDbConnection CreateConnection(UIConnectionInfo ci, EnEngineType serverEngine);

	IDbConnection CreateConnection(string connectionString);
}
