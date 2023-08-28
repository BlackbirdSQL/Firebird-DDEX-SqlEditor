#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution

namespace BlackbirdSql.Common.Model.Events;


public class QeSqlCmdMessageFromAppEventArgs : QESQLBatchMessageEventArgs
{
	private readonly bool stdOut;

	public bool StdOut => stdOut;

	private QeSqlCmdMessageFromAppEventArgs()
	{
	}

	public QeSqlCmdMessageFromAppEventArgs(string message, bool stdOut)
		: base(message)
	{
		this.stdOut = stdOut;
	}
}
