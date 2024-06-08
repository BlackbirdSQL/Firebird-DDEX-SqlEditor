// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITabbedEditorService
using System;
using System.Collections.Generic;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Enums;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Shared.Interfaces;

public interface IBTabbedEditorService
{
	AbstractEditorTab ActiveTab { get; }

	AbstractTabbedEditorUIControl TabbedEditorControl { get; }

	IVsWindowFrame TabFrame { get; }

	Guid InitialLogicalView { get; }

	IBTextEditor TextEditor { get; set; }

	void Activate(Guid logicalView, EnTabViewMode mode);

	IEnumerable<uint> GetEditableDocuments();

	uint GetPrimaryDocCookie();

	bool IsTabVisible(Guid logicalView);

}
