// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITabbedEditorService
using System;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Controls;
using Microsoft.VisualStudio.Shell.Interop;




namespace BlackbirdSql.Common.Interfaces;


public interface ITabbedEditorService
{
	AbstractEditorTab ActiveTab { get; }

	TabbedEditorUI TabbedEditorControl { get; }

	IVsWindowFrame TabFrame { get; }

	Guid InitialLogicalView { get; }

	ITextEditor TextEditor { get; set; }

	void Activate(Guid logicalView, EnTabViewMode mode);

	bool IsTabVisible(Guid logicalView);
}
