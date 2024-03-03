// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorResultsTab

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Common;
using Microsoft.VisualStudio;
using BlackbirdSql.Common.Ctl.Enums;

// using Microsoft.Data.Tools.Schema.Utilities.Sql.Common;
// using Microsoft.VisualStudio.Data.Tools.Design.Core.Common.Win32;
// using Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor;
// using Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor;




namespace BlackbirdSql.Common.Controls;


#pragma warning disable CS9113 // Parameter is unread.
public class SqlEditorResultsTab(AbstractTabbedEditorPane editorPane, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
	: AbstractSqlEditorTab(editorPane, logicalView, editorTabType)
{
	private Guid _ClsidLogicalView = VSConstants.LOGVIEWID_Designer;

	private Guid _ClsidTabEditorFactory = new(LibraryData.SqlResultsEditorFactoryGuid);

	protected override Guid ClsidEditorTabEditorFactory => _ClsidTabEditorFactory;

	protected override Guid ClsidLogicalView => _ClsidLogicalView;
}
#pragma warning restore CS9113 // Parameter is unread.
