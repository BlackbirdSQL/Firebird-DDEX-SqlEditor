// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Controls.Config;
using BlackbirdSql.EditorExtension.Model.Config;
using BlackbirdSql.Sys.Interfaces;
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
	[Guid(PackageData.C_GeneralSettingsGuid)]
	public class GeneralSettingsPage : AbstractPersistentSettingsPage<GeneralSettingsPage, GeneralSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.C_TabAndStatusBarSettingsGuid)]
	public class TabAndStatusBarSettingsPage : AbstractPersistentSettingsPage<TabAndStatusBarSettingsPage, TabAndStatusBarSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.C_ExecutionSettingsGuid)]
	public class ExecutionSettingsPage : AbstractPersistentSettingsPage<ExecutionSettingsPage, ExecutionSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.C_ExecutionAdvancedSettingsGuid)]
	public class ExecutionAdvancedSettingsPage : AbstractPersistentSettingsPage<ExecutionAdvancedSettingsPage, ExecutionAdvancedSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.C_ResultsSettingsGuid)]
	public class ResultsSettingsPage : AbstractPersistentSettingsPage<ResultsSettingsPage, ResultsSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.C_ResultsGridSettingsGuid)]
	public class ResultsGridSettingsPage : AbstractPersistentSettingsPage<ResultsGridSettingsPage, ResultsGridSettingsModel> { }

	[ComVisible(true)]
	[Guid(PackageData.C_ResultsTextSettingsGuid)]
	public class ResultsTextSettingsPage : AbstractPersistentSettingsPage<ResultsTextSettingsPage, ResultsTextSettingsModel> { }



	[ComVisible(true)]
	[Guid(PackageData.C_TransientExecutionSettingsGuid)]
	public class TransientExecutionSettingsPage(IBsSettingsProvider transientSettings)
		: AbstractTransientSettingsPage<TransientExecutionSettingsPage, ExecutionSettingsModel>(transientSettings) { }

	[ComVisible(true)]
	[Guid(PackageData.C_TransientExecutionAdvancedSettingsGuid)]
	public class TransientExecutionAdvancedSettingsPage(IBsSettingsProvider transientSettings)
		: AbstractTransientSettingsPage<TransientExecutionAdvancedSettingsPage, ExecutionAdvancedSettingsModel>(transientSettings) { }

	[ComVisible(true)]
	[Guid(PackageData.C_TransientResultsSettingsGuid)]
	public class TransientResultsSettingsPage(IBsSettingsProvider transientSettings)
		: AbstractTransientSettingsPage<TransientResultsSettingsPage, ResultsSettingsModel>(transientSettings)	{ }

	[ComVisible(true)]
	[Guid(PackageData.C_TransientResultsGridSettingsGuid)]
	public class TransientResultsGridSettingsPage(IBsSettingsProvider transientSettings)
		: AbstractTransientSettingsPage<TransientResultsGridSettingsPage, ResultsGridSettingsModel>(transientSettings) { }

	[ComVisible(true)]
	[Guid(PackageData.C_TransientResultsTextSettingsGuid)]
	public class TransientResultsTextSettingsPage(IBsSettingsProvider transientSettings)
		: AbstractTransientSettingsPage<TransientResultsTextSettingsPage, ResultsTextSettingsModel>(transientSettings) { }


	// Custom defined page > DebugSettingsDialogPage.
	// public class DebugSettingsPage : AbstractSettingsPage<DebugSettingsModel> { }
}
