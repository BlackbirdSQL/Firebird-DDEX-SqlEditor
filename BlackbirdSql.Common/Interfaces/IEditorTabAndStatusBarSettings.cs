#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;

using BlackbirdSql.Common.Config.Enums;

namespace BlackbirdSql.Common.Interfaces;


public interface IEditorTabAndStatusBarSettings : ICloneable
{
	EnDisplayTimeOptions ShowTimeOption { get; set; }

	bool StatusBarIncludeServerName { get; set; }

	bool StatusBarIncludeDatabaseName { get; set; }

	bool StatusBarIncludeLoginName { get; set; }

	bool StatusBarIncludeRowCount { get; set; }

	Color StatusBarColor { get; set; }

	bool TabTextIncludeServerName { get; set; }

	bool TabTextIncludeDatabaseName { get; set; }

	bool TabTextIncludeLoginName { get; set; }

	bool TabTextIncludeFileName { get; set; }

	bool LayoutPropertyChanged { get; set; }

	void ResetToDefault();
}
