// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorResultsTab

using System;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;



namespace BlackbirdSql.Shared.Controls.Tabs;


internal class EditorResultsTab: AbstractEditorTab
{

	public EditorResultsTab(IBsTabbedEditorPane tabbedEditor, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
		: base(tabbedEditor, logicalView, editorTabType)
	{

	}



	private Guid _ClsidLogicalView = VSConstants.LOGVIEWID_Designer;

	private Guid _ClsidTabEditorFactory = new(LibraryData.C_ResultsEditorFactoryGuid);

	protected override Guid ClsidEditorTabEditorFactory => _ClsidTabEditorFactory;

	protected override Guid ClsidLogicalView => _ClsidLogicalView;
}