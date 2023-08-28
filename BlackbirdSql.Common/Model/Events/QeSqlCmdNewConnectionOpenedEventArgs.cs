#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Events;


public class QeSqlCmdNewConnectionOpenedEventArgs : EventArgs
{
	private readonly IDbConnection newConnection;

	public IDbConnection Connection => newConnection;

	private QeSqlCmdNewConnectionOpenedEventArgs()
	{
	}

	public QeSqlCmdNewConnectionOpenedEventArgs(IDbConnection con)
	{
		newConnection = con;
	}
}
