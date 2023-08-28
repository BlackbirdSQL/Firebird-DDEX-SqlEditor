// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITabbedEditorToolbarCommandHandler
using System;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Ctl;
using Microsoft.VisualStudio.OLE.Interop;

namespace BlackbirdSql.Common.Interfaces;


public interface ITabbedEditorToolbarCommandHandler
{
	GuidId GuidId { get; }

	int HandleQueryStatus(AbstractTabbedEditorPane tabbedEditorPane, ref OLECMD prgCmd, IntPtr pCmdText);

	int HandleExec(AbstractTabbedEditorPane tabbedEditorPane, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);
}
