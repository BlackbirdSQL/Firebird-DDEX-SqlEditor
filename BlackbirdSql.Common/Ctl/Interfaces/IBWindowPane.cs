// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorWindowPane

namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBWindowPane
{
	bool IsDisposed { get; }


	bool IsSplitterVisible { get; set; }

	bool SplittersVisible { set; }

	void ActivateNextTab();

	void ActivatePreviousTab();

}
