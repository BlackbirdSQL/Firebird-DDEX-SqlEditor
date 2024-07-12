// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

using GlobalizedCategoryAttribute = BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel.GlobalizedCategoryAttribute;
using GlobalizedDescriptionAttribute = BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel.GlobalizedDescriptionAttribute;
using GlobalizedDisplayNameAttribute = BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel.GlobalizedDisplayNameAttribute;



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

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - GeneralSettingsModel
	// ---------------------------------------------------------------------------------


	public GeneralSettingsModel(IBTransientSettings transientSettings)
		: base(C_Package, C_Group, C_LivePrefix, transientSettings)
	{

	}


	public GeneralSettingsModel() : this(null)
	{
	}


	#endregion Constructors / Destructors





	// =====================================================================================================
	#region Constants - GeneralSettingsModel
	// =====================================================================================================


	private const string C_Package = "Ddex";
	private const string C_Group = "General";
	private const string C_LivePrefix = "DdexGeneral";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - GeneralSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\Ddex.GeneralSettings";


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralAutoCloseOffScreenEdmx")]
	[GlobalizedDescription("OptionDescriptionGeneralAutoCloseOffScreenEdmx")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	public bool AutoCloseOffScreenEdmx { get; set; } = true;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralAutoCloseXsdDatasets")]
	[GlobalizedDescription("OptionDescriptionGeneralAutoCloseXsdDatasets")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool AutoCloseXsdDatasets { get; set; } = false;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralIncludeAppConnections")]
	[GlobalizedDescription("OptionDescriptionGeneralIncludeAppConnections")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool IncludeAppConnections { get; set; } = true;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralOnDemandLinkage")]
	[GlobalizedDescription("OptionDescriptionGeneralOnDemandLinkage")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool OnDemandLinkage { get; set; } = false;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayGeneralLinkageTimeout")]
	[GlobalizedDescription("OptionDescriptionGeneralLinkageTimeout")]
#if DEBUG
	[TypeConverter(typeof(UomConverter)), LiteralRange(3, 120, "Seconds")]
#else
	[TypeConverter(typeof(UomConverter)), LiteralRange(10, 120, "Seconds")]
#endif
	[DefaultValue(30)]
	public int LinkageTimeout { get; set; } = 30;



	[GlobalizedCategory("OptionCategoryQueryDesigner")]
	[GlobalizedDisplayName("OptionDisplayGeneralShowDiagramPane")]
	[GlobalizedDescription("OptionDescriptionGeneralShowDiagramPane")]
	[TypeConverter(typeof(GlobalShowHideConverter))]
	[DefaultValue(true)]
	public bool ShowDiagramPane { get; set; } = true;

	[GlobalizedCategory("OptionCategoryDiagnostics")]
	[GlobalizedDisplayName("OptionDisplayGeneralEnableDiagnostics")]
	[GlobalizedDescription("OptionDescriptionGeneralEnableDiagnostics")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool EnableDiagnostics { get; set; } = true;

	[GlobalizedCategory("OptionCategoryDiagnostics")]
	[GlobalizedDisplayName("OptionDisplayGeneralEnableTaskLog")]
	[GlobalizedDescription("OptionDescriptionGeneralEnableTaskLog")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool EnableTaskLog { get; set; } = true;


	[GlobalizedCategory("OptionCategoryEntityFramework")]
	[GlobalizedDisplayName("OptionDisplayGeneralValidateSessionConnectionOnFormAccept")]
	[GlobalizedDescription("OptionDescriptionGeneralValidateSessionConnectionOnFormAccept")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	public bool ValidateSessionConnectionOnFormAccept { get; set; } = true;

	// To be defaulted to false once Npgsql is fixed.
	[GlobalizedCategory("OptionCategoryEntityFramework")]
	[GlobalizedDisplayName("OptionDisplayGeneralValidateProviderFactories")]
	[GlobalizedDescription("OptionDescriptionGeneralValidateProviderFactories")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool ValidateProviderFactories { get; set; } = false;


	#endregion Property Accessors

}
