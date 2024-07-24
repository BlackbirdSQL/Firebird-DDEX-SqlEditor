// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorWindowPane

using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Core.Interfaces;

public interface IBsEditorWindowPane
{
	IVsTextView GetCodeEditorTextView(object editorCodeTab = null);
}
