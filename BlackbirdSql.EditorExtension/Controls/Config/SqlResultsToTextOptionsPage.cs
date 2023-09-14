// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.SqlResultsToTextOptionsPage
using System.Runtime.InteropServices;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls.ToolsOptions;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.EditorExtension.Controls.Config;

[Guid(LibraryData.SqlResultsToTextOptionsGuid)]
public class SqlResultsToTextOptionsPage : AbstractToolsOptionsDialogPage
{
	public SqlResultsToTextOptionsPage()
		: base(new SqlResultToTextSettingsDlg())
	{
	}

	public override void SaveSettingsToStorage()
	{
		Tracer.Trace(GetType(), "SqlResultsToTextOptionsPage.SaveSettings", "", null);
		base.SaveSettingsToStorage();
	}
}
