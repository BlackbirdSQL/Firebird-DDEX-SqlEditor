// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions2.SqlEditorTabAndStatusBarSettingsDialogPage
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Ctl.ComponentModel;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;

using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.EditorExtension.Controls.Config;

[Guid(LibraryData.SqlEditorTabAndStatusBarSettingsGuid)]
public class SqlEditorTabAndStatusBarSettingsDialogPage : DialogPage
{
	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum ShowTimeOption
	{
		[GlobalizedDescription("ToolOptionShowTimeOptionNone")]
		None,
		[GlobalizedDescription("ToolOptionShowTimeOptionElapsed")]
		Elapsed,
		[GlobalizedDescription("ToolOptionShowTimeOptionEnd")]
		End
	}

	[GlobalizedDisplayName("ToolOptionExecutionTimeDisplayName")]
	[GlobalizedDescription("ToolOptionExecutionTimeDescription")]
	[GlobalizedCategory("ToolOptionStatusBarCategory")]
	public ShowTimeOption ExecutionTime { get; set; }

	[GlobalizedDisplayName("ToolOptionIncludeDBNameDisplayName")]
	[GlobalizedDescription("ToolOptionStatusBarIncludeDBNameDescription")]
	[GlobalizedCategory("ToolOptionStatusBarCategory")]
	[TypeConverter(typeof(GlobalBoolConverter))]
	public bool StatusBarIncludeDatabaseName { get; set; }

	[GlobalizedDisplayName("ToolOptionIncludeLoginNameDisplayName")]
	[GlobalizedDescription("ToolOptionStatusBarIncludeLoginNameDescription")]
	[GlobalizedCategory("ToolOptionStatusBarCategory")]
	[TypeConverter(typeof(GlobalBoolConverter))]
	public bool StatusBarIncludeLoginName { get; set; }

	[GlobalizedDisplayName("ToolOptionIncludeRowCountDisplayName")]
	[GlobalizedDescription("ToolOptionStatusBarIncludeRowCountDescription")]
	[GlobalizedCategory("ToolOptionStatusBarCategory")]
	[TypeConverter(typeof(GlobalBoolConverter))]
	public bool StatusBarIncludeRowCount { get; set; }

	[GlobalizedDisplayName("ToolOptionIncludeServerNameDisplayName")]
	[GlobalizedDescription("ToolOptionStatusBarIncludeServerNameDescription")]
	[GlobalizedCategory("ToolOptionStatusBarCategory")]
	[TypeConverter(typeof(GlobalBoolConverter))]
	public bool StatusBarIncludeServerName { get; set; }

	[GlobalizedDisplayName("ToolOptionStatusBarDefaultColorDisplayName")]
	[GlobalizedDescription("ToolOptionStatusBarDefaultColorDescription")]
	[GlobalizedCategory("ToolOptionStatusBarColorCategory")]
	[TypeConverter(typeof(GlobalBoolConverter))]
	public Color StatusBarColor { get; set; }

	[GlobalizedDisplayName("ToolOptionIncludeDBNameDisplayName")]
	[GlobalizedDescription("ToolOptionTabTextIncludeDatabaseNameDescription")]
	[GlobalizedCategory("ToolOptionTabTextCategory")]
	[TypeConverter(typeof(GlobalBoolConverter))]
	public bool TabTextIncludeDatabaseName { get; set; }

	[GlobalizedDisplayName("ToolOptionIncludeLoginNameDisplayName")]
	[GlobalizedDescription("ToolOptionTabTextIncludeLoginNameDescription")]
	[GlobalizedCategory("ToolOptionTabTextCategory")]
	[TypeConverter(typeof(GlobalBoolConverter))]
	public bool TabTextIncludeLoginName { get; set; }

	[GlobalizedDisplayName("ToolOptionIncludeFileNameDisplayName")]
	[GlobalizedDescription("ToolOptionTabTextIncludeFileNameDescription")]
	[GlobalizedCategory("ToolOptionTabTextCategory")]
	[TypeConverter(typeof(GlobalBoolConverter))]
	public bool TabTextIncludeFileName { get; set; }

	[GlobalizedDisplayName("ToolOptionIncludeServerNameDisplayName")]
	[GlobalizedDescription("ToolOptionTabTextIncludeServerNameDescription")]
	[GlobalizedCategory("ToolOptionTabTextCategory")]
	[TypeConverter(typeof(GlobalBoolConverter))]
	public bool TabTextIncludeServerName { get; set; }

	public override void LoadSettingsFromStorage()
	{
		LoadSettings(UserSettings.Instance.Current);
	}

	public override void SaveSettingsToStorage()
	{
		UserSettings.Instance.Current.StatusBar.ShowTimeOption = ConvertShowTimeOptionToDisplayTimeOption(ExecutionTime);
		UserSettings.Instance.Current.StatusBar.StatusBarIncludeDatabaseName = StatusBarIncludeDatabaseName;
		UserSettings.Instance.Current.StatusBar.StatusBarIncludeLoginName = StatusBarIncludeLoginName;
		UserSettings.Instance.Current.StatusBar.StatusBarIncludeRowCount = StatusBarIncludeRowCount;
		UserSettings.Instance.Current.StatusBar.StatusBarIncludeServerName = StatusBarIncludeServerName;
		UserSettings.Instance.Current.StatusBar.StatusBarColor = StatusBarColor;
		UserSettings.Instance.Current.StatusBar.TabTextIncludeDatabaseName = TabTextIncludeDatabaseName;
		UserSettings.Instance.Current.StatusBar.TabTextIncludeFileName = TabTextIncludeFileName;
		UserSettings.Instance.Current.StatusBar.TabTextIncludeLoginName = TabTextIncludeLoginName;
		UserSettings.Instance.Current.StatusBar.TabTextIncludeServerName = TabTextIncludeServerName;
		UserSettings.Instance.Save();
	}

	public override void ResetSettings()
	{
		LoadSettings(UserSettings.Instance.Default);
	}

	private void LoadSettings(IBUserSettings settings)
	{
		EnDisplayTimeOptions showTimeOption = settings.StatusBar.ShowTimeOption;
		ExecutionTime = ConvertDisplayTimeOptionToShowTimeOption(showTimeOption);
		StatusBarIncludeServerName = settings.StatusBar.StatusBarIncludeServerName;
		StatusBarIncludeDatabaseName = settings.StatusBar.StatusBarIncludeDatabaseName;
		StatusBarIncludeLoginName = settings.StatusBar.StatusBarIncludeLoginName;
		StatusBarIncludeRowCount = settings.StatusBar.StatusBarIncludeRowCount;
		StatusBarColor = settings.StatusBar.StatusBarColor;
		TabTextIncludeDatabaseName = settings.StatusBar.TabTextIncludeDatabaseName;
		TabTextIncludeFileName = settings.StatusBar.TabTextIncludeFileName;
		TabTextIncludeLoginName = settings.StatusBar.TabTextIncludeLoginName;
		TabTextIncludeServerName = settings.StatusBar.TabTextIncludeServerName;
	}

	private ShowTimeOption ConvertDisplayTimeOptionToShowTimeOption(EnDisplayTimeOptions modelEnum)
	{
		return modelEnum switch
		{
			EnDisplayTimeOptions.Elapsed => ShowTimeOption.Elapsed,
			EnDisplayTimeOptions.End => ShowTimeOption.End,
			EnDisplayTimeOptions.None => ShowTimeOption.None,
			_ => ShowTimeOption.Elapsed,
		};
	}

	private EnDisplayTimeOptions ConvertShowTimeOptionToDisplayTimeOption(ShowTimeOption uiEnum)
	{
		return uiEnum switch
		{
			ShowTimeOption.Elapsed => EnDisplayTimeOptions.Elapsed,
			ShowTimeOption.End => EnDisplayTimeOptions.End,
			ShowTimeOption.None => EnDisplayTimeOptions.None,
			_ => EnDisplayTimeOptions.Elapsed,
		};
	}
}
