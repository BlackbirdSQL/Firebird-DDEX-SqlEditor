#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Enums;


[Flags]
public enum EnScriptExecutionResult
{
	Success = 0x1,
	Failure = 0x2,
	Cancel = 0x4,
	Timeout = 0x8,
	Halted = 0x10,
	Mask = 0x1F
}
