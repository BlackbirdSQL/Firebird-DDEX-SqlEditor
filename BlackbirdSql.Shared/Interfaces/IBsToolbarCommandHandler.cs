// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITabbedEditorToolbarCommandHandler

using System;
using BlackbirdSql.Shared.Ctl;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBsToolbarCommandHandler
{
	GuidId Clsid { get; }

	int OnQueryStatus(IBsTabbedEditorPane editorPane, ref OLECMD prgCmd, IntPtr pCmdText);

	int OnExec(IBsTabbedEditorPane editorPane, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);
}
