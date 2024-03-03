// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITabbedEditorService
using System;
using System.Collections.Generic;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Ctl.Enums;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBTabbedEditorService
{
	AbstractEditorTab ActiveTab { get; }

	TabbedEditorUI TabbedEditorControl { get; }

	IVsWindowFrame TabFrame { get; }

	Guid InitialLogicalView { get; }

	IBTextEditor TextEditor { get; set; }

	void Activate(Guid logicalView, EnTabViewMode mode);

	IEnumerable<uint> GetEditableDocuments();

	uint GetPrimaryDocCookie();

	bool IsTabVisible(Guid logicalView);

}
