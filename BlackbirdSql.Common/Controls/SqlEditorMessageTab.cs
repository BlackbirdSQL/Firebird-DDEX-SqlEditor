// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorMessageTab

using System;

// using Microsoft.Data.Tools.Schema.Utilities.Sql.Common;
using Microsoft.VisualStudio;
// using Microsoft.VisualStudio.Data.Tools.Design.Core.Common.Win32;
// using Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor;
// using Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane;
// using Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor;
using Microsoft.VisualStudio.TextManager.Interop;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls.ResultsPane;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Ctl.Enums;

namespace BlackbirdSql.Common.Controls;


public class SqlEditorMessageTab(AbstractTabbedEditorPane editorPane, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
	: AbstractSqlEditorTab(editorPane, logicalView, editorTabType)
{

	protected Guid _EditorLogicalView = editorLogicalView;

	private Guid _ClsidLogicalView = VSConstants.LOGVIEWID_Designer;

	private Guid _ClsidEditorFactory = new(LibraryData.SqlResultsEditorFactoryGuid);

	protected override Guid GetEditorTabEditorFactoryGuid()
	{
		return _ClsidEditorFactory;
	}

	protected override Guid GetLogicalView()
	{
		return _ClsidLogicalView;
	}

	public override IVsFindTarget GetFindTarget()
	{
		IVsFindTarget result = null;
		if (GetView() is ResultWindowPane resultWindowPane && resultWindowPane.CommandTarget is VSTextEditorPanel vSTextEditorPanel)
		{
			result = vSTextEditorPanel.TextViewCtl.FindTarget;
		}
		return result;
	}
}
