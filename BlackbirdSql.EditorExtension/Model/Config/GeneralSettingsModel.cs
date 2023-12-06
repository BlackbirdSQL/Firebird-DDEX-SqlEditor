// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;


namespace BlackbirdSql.EditorExtension.Model.Config;

// =========================================================================================================
//										GeneralSettingsModel Class
//
/// <summary>
/// Option Model for General options
/// </summary>
// =========================================================================================================
public class GeneralSettingsModel(IBLiveSettings liveSettings)
	: AbstractSettingsModel<GeneralSettingsModel>(C_Package, C_Group, C_LivePrefix, liveSettings)
{

	// ---------------------------------------------------------------------------------
	#region Additional Constructors / Destructors - GeneralSettingsModel
	// ---------------------------------------------------------------------------------


	public GeneralSettingsModel() : this(null)
	{
	}


	#endregion Additional Constructors / Destructors




	// =====================================================================================================
	#region Constants - GeneralSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "General";
	private const string C_LivePrefix = "EditorGeneral";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - GeneralSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.GeneralSettings";



	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralExecuteQueryOnOpen")]
	[GlobalizedDescription("OptionDescriptionGeneralExecuteQueryOnOpen")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool ExecuteQueryOnOpen { get; set; } = true;


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralEnableIntellisense")]
	[GlobalizedDescription("OptionDescriptionGeneralEnableIntellisense")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	public bool EnableIntellisense { get; set; } = true;


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralPromptToSave")]
	[GlobalizedDescription("OptionDescriptionGeneralPromptToSave")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool PromptToSave { get; set; } = false;


	#endregion Property Accessors

}
