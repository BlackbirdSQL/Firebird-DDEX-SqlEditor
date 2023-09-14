// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.SqlResultsGeneralOptionsPage
using System.Runtime.InteropServices;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls.ToolsOptions;
using BlackbirdSql.Core.Ctl.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.EditorExtension.Controls.Config;

[Guid(LibraryData.SqlResultsGeneralOptionsGuid)]
public class SqlResultsGeneralOptionsPage : AbstractToolsOptionsDialogPage
{
	public SqlResultsGeneralOptionsPage()
		: base(new SqlResultsGeneralSettingsDlg())
	{
	}

	public override void LoadSettingsFromStorage()
	{
		Tracer.Trace(GetType(), "SqlResultsGeneralOptionsPage.LoadSettings", "", null);
		base.LoadSettingsFromStorage();
		if (_control != null)
		{
			((SqlResultsGeneralSettingsDlg)_control)?.SetIVsUIShell((IVsUIShell)GetService(typeof(IVsUIShell)));
		}
	}

	public override void SaveSettingsToStorage()
	{
		Tracer.Trace(GetType(), "SqlResultsGeneralOptionsPage.SaveSettings", "", null);
		base.SaveSettingsToStorage();
	}
}
