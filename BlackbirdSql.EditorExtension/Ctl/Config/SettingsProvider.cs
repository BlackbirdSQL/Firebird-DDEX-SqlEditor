// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Controls.Config;
using BlackbirdSql.EditorExtension.Model.Config;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.EditorExtension.Ctl.Config;

// =========================================================================================================
//										OptionsProvider Class
//
/// <summary>
/// Provider for <see cref="ProvideOptionPageAttribute"/> in <see cref="EditorExtensionAsyncPackage"/>
/// </summary>
// =========================================================================================================
public class SettingsProvider
{
	public const string CategoryName = "BlackbirdSql Server Tools";
	public const string SubCategoryName = "SqlEditor";
	public const string GeneralSettingsPageName = "General";
	public const string TabAndStatusBarSettingsPageName = "Editor Tab and Status Bar";
	public const string ExecutionSettingsPageName = "Query Execution";
	public const string ExecutionGeneralSettingsPageName = "General";
	public const string ExecutionAdvancedSettingsPageName = "Query Execution Advanced";
	public const string ResultsSettingsPageName = "Query Results";
	public const string ResultsGeneralSettingsPageName = "General";
	public const string ResultsGridSettingsPageName = "Query Results Grid";
	public const string ResultsTextSettingsPageName = "Query Results Text";


	[ComVisible(true)]
	[Guid(PackageData.GeneralSettingsGuid)]
	public class GeneralSettingsPage : AbstractSettingsPage<GeneralSettingsPage, GeneralSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.TabAndStatusBarSettingsGuid)]
	public class TabAndStatusBarSettingsPage : AbstractSettingsPage<TabAndStatusBarSettingsPage, TabAndStatusBarSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ExecutionSettingsGuid)]
	public class ExecutionSettingsPage : AbstractSettingsPage<ExecutionSettingsPage, ExecutionSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ExecutionAdvancedSettingsGuid)]
	public class ExecutionAdvancedSettingsPage : AbstractSettingsPage<ExecutionAdvancedSettingsPage, ExecutionAdvancedSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ResultsSettingsGuid)]
	public class ResultsSettingsPage : AbstractSettingsPage<ResultsSettingsPage, ResultsSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ResultsGridSettingsGuid)]
	public class ResultsGridSettingsPage : AbstractSettingsPage<ResultsGridSettingsPage, ResultsGridSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ResultsTextSettingsGuid)]
	public class ResultsTextSettingsPage : AbstractSettingsPage<ResultsTextSettingsPage, ResultsTextSettingsModel> { }


	// Custom defined page > DebugSettingsDialogPage.
	// public class DebugSettingsPage : AbstractSettingsPage<DebugSettingsModel> { }
}
