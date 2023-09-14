// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.ITextEditorEvents

using System;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBTextEditorEvents : IDisposable
{
	void OnCaretChanged(int offset);

	void SetTextEditor(IBTextEditor editor);

	void OnActiveChanged(bool isActive);
}
