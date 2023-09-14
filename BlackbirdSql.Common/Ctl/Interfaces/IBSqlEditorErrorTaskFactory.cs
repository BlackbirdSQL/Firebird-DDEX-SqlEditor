#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBSqlEditorErrorTaskFactory
{
	DocumentTask CreateErrorTaskItem(TextSpan span, MARKERTYPE markerType, string filename, IVsTextLines textLines);
}
