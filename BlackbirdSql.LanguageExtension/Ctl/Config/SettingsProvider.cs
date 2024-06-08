// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Runtime.InteropServices;
using BlackbirdSql.Core.Controls.Config;
using BlackbirdSql.Core.Interfaces;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.LanguageExtension.Ctl.Config;


// =========================================================================================================
//										OptionsProvider Class
//
/// <summary>
/// Provider for <see cref="ProvideLanguageEditorOptionPageAttribute"/>, <see cref="ProvideProfileAttribute"/>
/// and <see cref="VsProvideEditorAutomationPageAttribute"/> in <see cref="LanguageExtensionPackage"/>
/// </summary>
// =========================================================================================================
public class SettingsProvider
{
	public const string CategoryName = PackageData.LanguageLongName;
	public const string SubCategoryName = "LanguageService";
	public const string AdvancedPreferencesPageName = "Firebird-SQL Intellisense Settings";


	[ComVisible(true)]
	[Guid(PackageData.LanguagePreferencesPageGuid)]
	public class AdvancedPreferencesPage : AbstractPersistentSettingsPage<AdvancedPreferencesPage, AdvancedPreferencesModel> { }



	[ComVisible(true)]
	[Guid(PackageData.TransientLanguagePreferencesPageGuid)]
	public class TransientAdvancedPreferencesPage(IBTransientSettings transientSettings)
		: AbstractTransientSettingsPage<TransientAdvancedPreferencesPage, AdvancedPreferencesModel>(transientSettings) { }

}
