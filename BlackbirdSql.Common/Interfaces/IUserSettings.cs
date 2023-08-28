#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces
namespace BlackbirdSql.Common.Interfaces;


public interface IUserSettings : ICloneable
{
	IGeneralSettings General { get; set; }

	IEditorTabAndStatusBarSettings StatusBar { get; set; }

	IQueryExecutionSettings Execution { get; set; }

	IQueryExecutionResultsSettings ExecutionResults { get; set; }

	IEditorContextSettings EditorContext { get; set; }

	void ResetToDefault();
}
