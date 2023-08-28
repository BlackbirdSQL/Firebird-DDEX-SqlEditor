// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions2.SqlEditorGeneralSettingsDialogPage
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Config;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.EditorExtension.Config;

[Guid(LibraryData.SqlEditorGeneralSettingsGuid)]
public class SqlEditorGeneralSettingsDialogPage : DialogPage
{
	private readonly SqlEditorGeneralSettingsUserControl _optionsControl;

	protected override IWin32Window Window => _optionsControl;

	public SqlEditorGeneralSettingsDialogPage()
	{
		_optionsControl = new SqlEditorGeneralSettingsUserControl();
	}

	public override void LoadSettingsFromStorage()
	{
		UserSettings.Instance.Load();
		_optionsControl.InitializeUIFromOptions(UserSettings.Instance.Current);
	}

	public override void SaveSettingsToStorage()
	{
		_optionsControl.ApplyUIToOptions(UserSettings.Instance.Current);
		UserSettings.Instance.Save();
	}

	public override void ResetSettings()
	{
		_optionsControl.ResetSettings();
	}
}
