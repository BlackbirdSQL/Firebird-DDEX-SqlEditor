// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Controls.Config;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.EditorExtension.Model.Config;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.EditorExtension.Ctl.Config;

// =========================================================================================================
//										OptionsProvider Class
//
/// <summary>
/// Provider for <see cref="ProvideOptionPageAttribute"/> in <see cref="EditorExtensionPackage"/>
/// </summary>
// =========================================================================================================
public class SettingsProvider
{
	public const string CategoryName = "BlackbirdSQL Server Tools";
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
	public class GeneralSettingsPage : AbstractPersistentSettingsPage<GeneralSettingsPage, GeneralSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.TabAndStatusBarSettingsGuid)]
	public class TabAndStatusBarSettingsPage : AbstractPersistentSettingsPage<TabAndStatusBarSettingsPage, TabAndStatusBarSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ExecutionSettingsGuid)]
	public class ExecutionSettingsPage : AbstractPersistentSettingsPage<ExecutionSettingsPage, ExecutionSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ExecutionAdvancedSettingsGuid)]
	public class ExecutionAdvancedSettingsPage : AbstractPersistentSettingsPage<ExecutionAdvancedSettingsPage, ExecutionAdvancedSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ResultsSettingsGuid)]
	public class ResultsSettingsPage : AbstractPersistentSettingsPage<ResultsSettingsPage, ResultsSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ResultsGridSettingsGuid)]
	public class ResultsGridSettingsPage : AbstractPersistentSettingsPage<ResultsGridSettingsPage, ResultsGridSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.ResultsTextSettingsGuid)]
	public class ResultsTextSettingsPage : AbstractPersistentSettingsPage<ResultsTextSettingsPage, ResultsTextSettingsModel> { }



	[ComVisible(true)]
	[Guid(PackageData.TransientExecutionSettingsGuid)]
	public class TransientExecutionSettingsPage(IBTransientSettings transientSettings)
		: AbstractTransientSettingsPage<TransientExecutionSettingsPage, ExecutionSettingsModel>(transientSettings) { }

	[ComVisible(true)]
	[Guid(PackageData.TransientExecutionAdvancedSettingsGuid)]
	public class TransientExecutionAdvancedSettingsPage(IBTransientSettings transientSettings)
		: AbstractTransientSettingsPage<TransientExecutionAdvancedSettingsPage, ExecutionAdvancedSettingsModel>(transientSettings) { }

	[ComVisible(true)]
	[Guid(PackageData.TransientResultsSettingsGuid)]
	public class TransientResultsSettingsPage(IBTransientSettings transientSettings)
		: AbstractTransientSettingsPage<TransientResultsSettingsPage, ResultsSettingsModel>(transientSettings)	{ }

	[ComVisible(true)]
	[Guid(PackageData.TransientResultsGridSettingsGuid)]
	public class TransientResultsGridSettingsPage(IBTransientSettings transientSettings)
		: AbstractTransientSettingsPage<TransientResultsGridSettingsPage, ResultsGridSettingsModel>(transientSettings) { }

	[ComVisible(true)]
	[Guid(PackageData.TransientResultsTextSettingsGuid)]
	public class TransientResultsTextSettingsPage(IBTransientSettings transientSettings)
		: AbstractTransientSettingsPage<TransientResultsTextSettingsPage, ResultsTextSettingsModel>(transientSettings) { }


	// Custom defined page > DebugSettingsDialogPage.
	// public class DebugSettingsPage : AbstractSettingsPage<DebugSettingsModel> { }
}
