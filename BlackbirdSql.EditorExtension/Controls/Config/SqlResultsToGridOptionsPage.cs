// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.SqlResultsToGridOptionsPage
using System.Runtime.InteropServices;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls.ToolsOptions;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.EditorExtension.Controls.Config;

[Guid(LibraryData.SqlResultsToGridOptionsGuid)]
public class SqlResultsToGridOptionsPage : AbstractToolsOptionsDialogPage
{
	public SqlResultsToGridOptionsPage()
		: base(new SqlResultsToGridSettingsDlg())
	{
	}

	public override void SaveSettingsToStorage()
	{
		Tracer.Trace(GetType(), "SqlResultsToGridOptionsPage.ResultsSettingsOption", "", null);
		base.SaveSettingsToStorage();
	}
}
