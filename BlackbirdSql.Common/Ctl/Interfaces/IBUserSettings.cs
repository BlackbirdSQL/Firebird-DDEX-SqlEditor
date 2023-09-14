#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces
namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBUserSettings : ICloneable
{
	IBGeneralSettings General { get; set; }

	IBEditorTabAndStatusBarSettings StatusBar { get; set; }

	IBQueryExecutionSettings Execution { get; set; }

	IBQueryExecutionResultsSettings ExecutionResults { get; set; }

	IBEditorContextSettings EditorContext { get; set; }

	void ResetToDefault();
}
