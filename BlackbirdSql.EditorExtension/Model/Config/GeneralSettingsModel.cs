// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.EditorExtension.Model.Config;

// =========================================================================================================
//										GeneralSettingsModel Class
//
/// <summary>
/// Option Model for General options
/// </summary>
// =========================================================================================================
public class GeneralSettingsModel : AbstractSettingsModel<GeneralSettingsModel>
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - GeneralSettingsModel
	// ---------------------------------------------------------------------------------


	public GeneralSettingsModel() : this(null)
	{
	}


	public GeneralSettingsModel(IBsSettingsProvider transientSettings)
		: base(C_Package, C_Group, C_PropertyPrefix, transientSettings)
	{

	}


	#endregion Constructors / Destructors




	// =====================================================================================================
	#region Constants - GeneralSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "General";
	private const string C_PropertyPrefix = "EditorGeneral";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - GeneralSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.GeneralSettings";


	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedIntellisensePolicy
	{
		[GlobalizedRadio("EnIntellisensePolicy_ActiveOnly")]
		ActiveOnly,
		[GlobalizedRadio("EnIntellisensePolicy_All")]
		All,
		[GlobalizedRadio("EnIntellisensePolicy_Disabled")]
		Disabled
	}


	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedLanguageService
	{
		[GlobalizedRadio("EnLanguageService_SSDT")]
		SSDT = 0,
		[GlobalizedRadio("EnLanguageService_USql")]
		USql,
		[GlobalizedRadio("EnLanguageService_TSql90")]
		TSql90,
		[GlobalizedRadio("EnLanguageService_FbSql")]
		FbSql
	}


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralExecuteQueryOnOpen")]
	[GlobalizedDescription("OptionDescriptionGeneralExecuteQueryOnOpen")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool ExecuteQueryOnOpen { get; set; } = true;


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralIntellisensePolicy")]
	[GlobalizedDescription("OptionDescriptionGeneralIntellisensePolicy")]
	[DefaultValue(EnGlobalizedIntellisensePolicy.ActiveOnly)]
	public EnGlobalizedIntellisensePolicy IntellisensePolicy { get; set; } = EnGlobalizedIntellisensePolicy.ActiveOnly;


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralPromptSave")]
	[GlobalizedDescription("OptionDescriptionGeneralPromptSave")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	public bool PromptSave { get; set; } = true;


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralLanguageService")]
	[GlobalizedDescription("OptionDescriptionGeneralLanguageService")]
	[DefaultValue(EnGlobalizedLanguageService.FbSql)]
	public EnGlobalizedLanguageService LanguageService { get; set; } = EnGlobalizedLanguageService.FbSql;


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralTtsDefault")]
	[GlobalizedDescription("OptionDescriptionGeneralTtsDefault")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	public bool TtsDefault { get; set; } = true;

	#endregion Property Accessors

}
