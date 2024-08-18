// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.LanguageExtension.Ctl.ComponentModel;

using GlobalizedCategoryAttribute = BlackbirdSql.LanguageExtension.Ctl.ComponentModel.GlobalizedCategoryAttribute;
using GlobalizedDescriptionAttribute = BlackbirdSql.LanguageExtension.Ctl.ComponentModel.GlobalizedDescriptionAttribute;
using GlobalizedDisplayNameAttribute = BlackbirdSql.LanguageExtension.Ctl.ComponentModel.GlobalizedDisplayNameAttribute;



namespace BlackbirdSql.LanguageExtension.Ctl.Config;

[Guid(PackageData.C_LanguagePreferencesModelGuid)]


// =========================================================================================================
//										AdvancedPreferencesModel Class
//
/// <summary>
/// Option Model for Language Service Advanced options
/// </summary>
// =========================================================================================================
public class AdvancedPreferencesModel : AbstractSettingsModel<AdvancedPreferencesModel>
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AdvancedPreferencesModel
	// ---------------------------------------------------------------------------------


	public AdvancedPreferencesModel() : this(null)
	{
	}


	public AdvancedPreferencesModel(IBsTransientSettings transientSettings)
		: base(C_Package, C_Group, C_PropertyPrefix, transientSettings)
	{
	}


	#endregion Constructors / Destructors




	// =====================================================================================================
	#region Constants - AdvancedPreferencesModel
	// =====================================================================================================


	private const string C_Package = "LanguageService";
	private const string C_Group = "Advanced";
	private const string C_PropertyPrefix = "LanguageServiceAdvanced";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - AdvancedPreferencesModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\LanguageService.Advanced";


	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedScriptSize
	{
		[GlobalizedRadio("EnScriptSize_100KB")]
		Kilobytes100 = 102400,
		[GlobalizedRadio("EnScriptSize_500KB")]
		Kilobytes500 = 512000,
		[GlobalizedRadio("EnScriptSize_1MB")]
		Megabytes1 = 1048576,
		[GlobalizedRadio("EnScriptSize_2MB")]
		Megabytes2 = 2097152,
		[GlobalizedRadio("EnScriptSize_5MB")]
		Megabytes5 = 5242880,
		[GlobalizedRadio("EnScriptSize_Unlimited")]
		Unlimited = 2147483647
	}

	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedCasingStyle
	{
		[GlobalizedRadio("EnCasingStyle_Uppercase")]
		Uppercase = 0,
		[GlobalizedRadio("EnCasingStyle_Lowercase")]
		Lowercase = 1
	}




	[GlobalizedCategory("OptionCategoryLanguage")]
	[GlobalizedDisplayName("OptionDisplayLanguageEnableIntellisense")]
	[GlobalizedDescription("OptionDescriptionLanguageEnableIntellisense")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[Automator, RefreshProperties(RefreshProperties.All)]
	[DefaultValue(true)]
	public bool EnableIntellisense { get; set; } = true;

	[GlobalizedCategory("OptionCategoryLanguage")]
	[GlobalizedDisplayName("OptionDisplayLanguageAutoOutlining")]
	[GlobalizedDescription("OptionDescriptionLanguageAutoOutlining")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	[Automator("EnableIntellisense"), ReadOnly(true)]
	public bool AutoOutlining { get; set; } = true;

	[GlobalizedCategory("OptionCategoryLanguage")]
	[GlobalizedDisplayName("OptionDisplayLanguageUnderlineErrors")]
	[GlobalizedDescription("OptionDescriptionLanguageUnderlineErrors")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	[Automator("EnableIntellisense"), ReadOnly(true)]
	public bool UnderlineErrors { get; set; } = true;

	[GlobalizedCategory("OptionCategoryLanguage")]
	[GlobalizedDisplayName("OptionDisplayLanguageMaxScriptSize")]
	[GlobalizedDescription("OptionDescriptionLanguageMaxScriptSize")]
	[Automator("EnableIntellisense"), ReadOnly(true)]
	[DefaultValue(EnGlobalizedScriptSize.Megabytes1)]
	public EnGlobalizedScriptSize MaxScriptSize { get; set; } = EnGlobalizedScriptSize.Megabytes1;

	[GlobalizedCategory("OptionCategoryLanguage")]
	[GlobalizedDisplayName("OptionDisplayLanguageTextCasing")]
	[GlobalizedDescription("OptionDescriptionLanguageTextCasing")]
	[Automator("EnableIntellisense"), ReadOnly(true)]
	[DefaultValue(EnGlobalizedCasingStyle.Uppercase)]
	public EnGlobalizedCasingStyle TextCasing { get; set; } = EnGlobalizedCasingStyle.Uppercase;


	#endregion Property Accessors




	// =====================================================================================================
	#region Methods - AdvancedPreferencesModel
	// =====================================================================================================




	#endregion Methods



}
