// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Controls.Config;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.LanguageExtension.Ctl.Config;


// =========================================================================================================
//										SettingsProvider Class
//
/// <summary>
/// Provider for <see cref="ProvideLanguageEditorOptionPageAttribute"/>, <see cref="ProvideProfileAttribute"/>
/// and <see cref="VsProvideEditorAutomationPageAttribute"/> in <see cref="LanguageExtensionPackage"/>
/// </summary>
// =========================================================================================================
public class SettingsProvider
{
	internal const string CategoryName = PackageData.C_LanguageLongName;
	internal const string SubCategoryName = "LanguageService";


	[ComVisible(true)]
	[Guid(PackageData.C_LanguagePreferencesPageGuid)]
	public class AdvancedPreferencesPage : AbstractPersistentSettingsPage<AdvancedPreferencesPage, AdvancedPreferencesModel> { }



	[ComVisible(true)]
	[Guid(PackageData.C_TransientLanguagePreferencesPageGuid)]
	public class TransientAdvancedPreferencesPage(IBsSettingsProvider transientSettings)
		: AbstractTransientSettingsPage<TransientAdvancedPreferencesPage, AdvancedPreferencesModel>(transientSettings) { }

}
