// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITabbedEditorToolbarCommandHandler

using System;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Ctl;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBToolbarCommandHandler
{
	GuidId Clsid { get; }

	int HandleQueryStatus(AbstractTabbedEditorWindowPane windowPane, ref OLECMD prgCmd, IntPtr pCmdText);

	int HandleExec(AbstractTabbedEditorWindowPane windowPane, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);
}
