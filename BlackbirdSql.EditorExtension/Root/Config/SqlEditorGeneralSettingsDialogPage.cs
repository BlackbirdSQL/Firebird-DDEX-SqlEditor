// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions2.SqlEditorGeneralSettingsDialogPage
using System.ComponentModel;
using System.Runtime.InteropServices;
using BlackbirdSql.Common;
using BlackbirdSql.Common.ComponentModel;
using BlackbirdSql.Common.Config;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Core.ComponentModel;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.EditorExtension.Config;

[Guid(LibraryData.SqlEditorGeneralSettingsGuid)]
public class SqlEditorGeneralSettingsDialogPage : DialogPage
{
	[GlobalizedDisplayName("ToolOptionPromptToSaveDisplayName")]
	[GlobalizedDescription("ToolOptionPromptToSaveDescription")]
	[GlobalizedCategory("ToolOptionGeneralCategory")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	public bool PromptToSaveWhenClosingQueryWindow { get; set; }


	public override void LoadSettingsFromStorage()
	{
		LoadSettings(UserSettings.Instance.Current);
	}

	public override void SaveSettingsToStorage()
	{
		UserSettings.Instance.Current.General.PromptForSaveWhenClosingQueryWindows = PromptToSaveWhenClosingQueryWindow;
		UserSettings.Instance.Save();
	}

	public override void ResetSettings()
	{
		LoadSettings(UserSettings.Instance.Default);
	}

	private void LoadSettings(IUserSettings settings)
	{
		PromptToSaveWhenClosingQueryWindow = settings.General.PromptForSaveWhenClosingQueryWindows;
	}
}
