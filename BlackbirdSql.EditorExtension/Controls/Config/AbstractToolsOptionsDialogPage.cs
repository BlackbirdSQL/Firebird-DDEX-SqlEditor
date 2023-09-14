// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.ToolsOptionsDialogPage
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.ToolsOptions;
using BlackbirdSql.Common.Ctl.Config;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.EditorExtension.Controls.Config;

public abstract class AbstractToolsOptionsDialogPage : DialogPage
{
	protected ToolsOptionsBaseControl _control;

	protected override IWin32Window Window => _control;

	public AbstractToolsOptionsDialogPage(ToolsOptionsBaseControl control)
	{
		_control = control;
	}

	public override void LoadSettingsFromStorage()
	{
		_control.LoadSettings(UserSettings.Instance.Current);
	}

	public override void SaveSettingsToStorage()
	{
		if (_control.ValidateValuesInControls())
		{
			_control.SaveSettings(UserSettings.Instance.Current);
		}
	}

	public override void ResetSettings()
	{
		_control.ResetSettings();
	}
}
