﻿// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITabbedEditorToolbarCommandHandler

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBsCommandHandler
{
	CommandID CmdId { get; }

	int OnQueryStatus(IBsTabbedEditorPane tabbedEditor, ref OLECMD prgCmd, IntPtr pCmdText);

	int OnExec(IBsTabbedEditorPane tabbedEditor, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);
}
