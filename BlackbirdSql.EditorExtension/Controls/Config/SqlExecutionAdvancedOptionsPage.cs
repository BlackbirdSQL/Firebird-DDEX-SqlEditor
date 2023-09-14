// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.SqlExecutionAdvancedOptionsPage
using System.Runtime.InteropServices;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls.ToolsOptions;

namespace BlackbirdSql.EditorExtension.Controls.Config;

[Guid(LibraryData.SqlExecutionAdvancedOptionsGuid)]
public class SqlExecutionAdvancedOptionsPage : AbstractToolsOptionsDialogPage
{
	public SqlExecutionAdvancedOptionsPage()
		: base(new SqlExecutionAdvancedSettingsDlg())
	{
	}
}
