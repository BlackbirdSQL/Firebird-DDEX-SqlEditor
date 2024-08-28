// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.ICommandExecuter


using System;
using Microsoft.VisualStudio.OLE.Interop;

namespace BlackbirdSql.Shared.Interfaces;

public interface IBsCommand
{
	IBsTabbedEditorPane TabbedEditor { get; set; }

	int QueryStatus(ref OLECMD prgCmd, IntPtr pCmdText);
	int Exec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);
}
