// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITextEditor

namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBTextEditor
{
	void Select(int offset, int length);

	void SetTextEditorEvents(IBTextEditorEvents events);

	void SyncState();

	void Focus();
}
