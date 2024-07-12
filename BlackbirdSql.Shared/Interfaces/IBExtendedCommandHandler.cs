#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Shared.Controls;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBExtendedCommandHandler
{
	int HandleExec(AbstractTabbedEditorWindowPane editor, ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);

	int HandleQueryStatus(AbstractTabbedEditorWindowPane editor, ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText);
}
