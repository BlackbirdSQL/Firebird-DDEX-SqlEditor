#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Data;
using BlackbirdSql.Core.Model;

// using Microsoft.SqlServer.ConnectionDlg.Core;
// using Microsoft.SqlServer.Management.Smo.RegSvrEnum;


namespace BlackbirdSql.Common.Ctl.Events;
// namespace Microsoft.SqlServer.ConnectionDlg.UI


public delegate IDbConnection VerifyConnectionDelegate(ConnectionPropertyAgent ci /*, IServerConnectionProvider serverType */);
