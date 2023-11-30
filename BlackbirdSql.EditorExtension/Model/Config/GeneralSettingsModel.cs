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
public class GeneralSettingsModel : AbstractSettingsModel<GeneralSettingsModel>
{

	private const string C_Package = "Editor";
	private const string C_Group = "General";
	private const string C_LivePrefix = "EditorGeneral";

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.GeneralSettings";




	// =====================================================================================================
	#region Model Properties - GeneralSettingsModel
	// =====================================================================================================


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


	#endregion Model Properties




	// =====================================================================================================
	#region Constructors / Destructors - GeneralSettingsModel
	// =====================================================================================================


	public GeneralSettingsModel() : this(null)
	{
	}

	public GeneralSettingsModel(IBLiveSettings liveSettings)
		: base(C_Package, C_Group, C_LivePrefix, liveSettings)
	{
	}


	#endregion Constructors / Destructors

}
