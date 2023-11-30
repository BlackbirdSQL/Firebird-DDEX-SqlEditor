// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

namespace BlackbirdSql.VisualStudio.Ddex.Model.Config;

// =========================================================================================================
//										GeneralSettingsModel Class
//
/// <summary>
/// Option Model for General options
/// </summary>
// =========================================================================================================
public class GeneralSettingsModel : AbstractSettingsModel<GeneralSettingsModel>
{

	private const string C_Package = "Ddex";
	private const string C_Group = "General";
	private const string C_LivePrefix = "DdexGeneral";

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\Ddex.GeneralSettings";




	// =====================================================================================================
	#region Model Properties - GeneralSettingsModel
	// =====================================================================================================


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralIncludeAppConnections")]
	[GlobalizedDescription("OptionDescriptionGeneralIncludeAppConnections")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool IncludeAppConnections { get; set; } = true;

	[GlobalizedCategory("OptionCategoryQueryDesigner")]
	[GlobalizedDisplayName("OptionDisplayGeneralShowDiagramPane")]
	[GlobalizedDescription("OptionDescriptionGeneralShowDiagramPane")]
	[TypeConverter(typeof(GlobalShowHideConverter))]
	[DefaultValue(true)]
	public bool ShowDiagramPane { get; set; } = true;

	[GlobalizedCategory("OptionCategoryDiagnostics")]
	[GlobalizedDisplayName("OptionDisplayGeneralEnableDiagnostics")]
	[GlobalizedDescription("OptionDescriptionGeneralEnableDiagnostics")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	public bool EnableDiagnostics { get; set; } = true;

	[GlobalizedCategory("OptionCategoryDiagnostics")]
	[GlobalizedDisplayName("OptionDisplayGeneralEnableTaskLog")]
	[GlobalizedDescription("OptionDescriptionGeneralEnableTaskLog")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	public bool EnableTaskLog { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEntityFramework")]
	[GlobalizedDisplayName("OptionDisplayGeneralValidateConfig")]
	[GlobalizedDescription("OptionDescriptionGeneralValidateConfig")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool ValidateConfig { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEntityFramework")]
	[GlobalizedDisplayName("OptionDisplayGeneralValidateEdmx")]
	[GlobalizedDescription("OptionDescriptionGeneralValidateEdmx")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool ValidateEdmx { get; set; } = true;


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
