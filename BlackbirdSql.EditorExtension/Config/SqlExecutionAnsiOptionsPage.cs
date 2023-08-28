// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.SqlExecutionAnsiOptionsPage
using System.Runtime.InteropServices;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls.ToolsOptions;

namespace BlackbirdSql.EditorExtension.Config;

[Guid(LibraryData.SqlExecutionAnsiOptionsGuid)]
public class SqlExecutionAnsiOptionsPage : AbstractToolsOptionsDialogPage
{
	public SqlExecutionAnsiOptionsPage()
		: base(new SqlExecutionAnsiSettingsDlg())
	{
	}
}
