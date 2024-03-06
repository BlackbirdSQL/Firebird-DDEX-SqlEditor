// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITabbedEditorToolbarCommandHandler

using System;
using BlackbirdSql.Common.Controls;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Common.Ctl.Interfaces;


public interface IBTabbedEditorToolbarCommandHandler
{
	GuidId GuidId { get; }

	int HandleQueryStatus(AbstractTabbedEditorWindowPane tabbedEditorPane, ref OLECMD prgCmd, IntPtr pCmdText);

	int HandleExec(AbstractTabbedEditorWindowPane tabbedEditorPane, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);
}
