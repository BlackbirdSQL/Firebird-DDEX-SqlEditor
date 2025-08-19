// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITabbedEditorService
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Model;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBsEditorPaneServiceProvider : IOleCommandTarget, System.IServiceProvider
{
	AbstruseEditorTab ActiveTab { get; }
	AuxilliaryDocData AuxDocData { get; }
	string DocumentMoniker { get; }
	bool IsClone { get; }
	uint PrimaryCookie { get; }
	EditorUIControl TabbedEditorUiCtl { get; }
	IVsWindowFrame TabFrame { get; }
	IBsTextEditor TextEditor { get; set; }


	void Activate(Guid logicalView, EnTabViewMode mode);
	Task CloseCloneEuiAsync();
	IEnumerable<uint> GetEditableDocuments();
	bool IsTabVisible(Guid logicalView);
	int OnExec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);
	int OnQueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText);

}
