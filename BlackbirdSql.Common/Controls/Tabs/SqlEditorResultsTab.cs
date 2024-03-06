// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorResultsTab

using System;
using BlackbirdSql.Common.Ctl.Enums;
using Microsoft.VisualStudio;



namespace BlackbirdSql.Common.Controls.Tabs;


public class SqlEditorResultsTab: AbstractSqlEditorTab
{

	public SqlEditorResultsTab(AbstractTabbedEditorWindowPane editorPane, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
		: base(editorPane, logicalView, editorTabType)
	{

	}



	private Guid _ClsidLogicalView = VSConstants.LOGVIEWID_Designer;

	private Guid _ClsidTabEditorFactory = new(LibraryData.SqlResultsEditorFactoryGuid);

	protected override Guid ClsidEditorTabEditorFactory => _ClsidTabEditorFactory;

	protected override Guid ClsidLogicalView => _ClsidLogicalView;
}