// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Controls.Config;
using BlackbirdSql.VisualStudio.Ddex.Model.Config;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.Config;

// =========================================================================================================
//										OptionsProvider Class
//
/// <summary>
/// Provider for <see cref="ProvideOptionPageAttribute"/> in <see cref="BlackbirdSqlDdexExtension"/>
/// </summary>
// =========================================================================================================
public class SettingsProvider
{
	public const string CategoryName = "BlackbirdSql Server Tools";
	public const string SubCategoryName = "Ddex Provider";
	public const string GeneralSettingsPageName = "General";
	public const string DebugSettingsPageName = "Debugging";

	[ComVisible(true)]
	[Guid(PackageData.GeneralSettingsGuid)]
	public class GeneralSettingsPage : AbstractSettingsPage<GeneralSettingsPage, GeneralSettingsModel> { }

	public class DebugSettingsPage : AbstractSettingsPage<DebugSettingsPage, DebugSettingsModel> { }
}
