// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorMessageTab

using System;
using BlackbirdSql.Shared.Controls.Results;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Controls.Tabs;


public class EditorMessageTab : AbstractEditorTab
{

	public EditorMessageTab(IBsTabbedEditorPane tabbedEditor, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
		: base(tabbedEditor, logicalView, editorTabType)
	{
		_EditorLogicalView = editorLogicalView;
	}


	protected Guid _EditorLogicalView;



	private Guid _ClsidLogicalView = VSConstants.LOGVIEWID_Designer;

	private Guid _ClsidTabEditorFactory = new Guid(LibraryData.C_ResultsEditorFactoryGuid);

	protected override Guid ClsidEditorTabEditorFactory => _ClsidTabEditorFactory;

	protected override Guid ClsidLogicalView => _ClsidLogicalView;


	public override IVsFindTarget GetFindTarget()
	{
		IVsFindTarget result = null;
		if (GetView() is ResultPane resultWindowPane && resultWindowPane.CommandTarget is VSTextEditorPanel vSTextEditorPanel)
		{
			result = vSTextEditorPanel.TextViewCtl.FindTarget;
		}
		return result;
	}
}
