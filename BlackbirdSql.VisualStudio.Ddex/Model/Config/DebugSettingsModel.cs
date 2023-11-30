// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using System.Drawing.Design;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.VisualStudio.Ddex.Controls.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;


namespace BlackbirdSql.VisualStudio.Ddex.Model.Config;

// =========================================================================================================
//										DebugSettingsModel Class
//
/// <summary>
/// Settings Model for Debug options
/// </summary>
// =========================================================================================================
public class DebugSettingsModel : AbstractSettingsModel<DebugSettingsModel>
{

	private const string C_Package = "Ddex";
	private const string C_Group = "Debug";
	private const string C_LivePrefix = "DdexDebug";

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\Ddex.DebugSettings";




	// =====================================================================================================
	#region Model Properties - DebugSettingsModel
	// =====================================================================================================


	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableTrace")]
	[GlobalizedDescription("OptionDescriptionDebugEnableTrace")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
#if DEBUG
	[DefaultValue(true)]
	public bool EnableTrace { get; set; } = true;
#else
	[DefaultValue(false)]
	public bool EnableTrace { get; set; } = false;
#endif

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableTracer")]
	[GlobalizedDescription("OptionDescriptionDebugEnableTracer")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableTracer { get; set; } = false;

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugPersistentValidation")]
	[GlobalizedDescription("OptionDescriptionDebugPersistentValidation")]
	[TypeConverter(typeof(GlobalBoolConverter))]
#if DEBUG
	[DefaultValue(false)]
	public bool PersistentValidation { get; set; } = false;
#else
	[DefaultValue(true)]
	public bool PersistentValidation { get; set; } = true;
#endif

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableDiagnosticsLog")]
	[GlobalizedDescription("OptionDescriptionDebugEnableDiagnosticsLog")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
#if DEBUG
	[DefaultValue(true)]
	public bool EnableDiagnosticsLog { get; set; } = true;
#else
	[DefaultValue(false)]
	public bool EnableDiagnosticsLog { get; set; } = false;
#endif

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableSaveExtrapolatedXml")]
	[GlobalizedDescription("OptionDescriptionDebugSaveExtrapolatedXml")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool EnableSaveExtrapolatedXml { get; set; } = false;


	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugLogFile")]
	[GlobalizedDescription("OptionDescriptionDebugLogFile")]
	[Editor(typeof(AdvancedFileNameEditor), typeof(UITypeEditor)), Parameters("OptionDialogLogFile", "OptionDialogFilterLogFile")]
	[DefaultValue("/temp/vsdiag.log")]
	public string LogFile { get; set; } = "/temp/vsdiag.log";

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableFbDiagnostics")]
	[GlobalizedDescription("OptionDescriptionDebugEnableFbDiagnostics")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableFbDiagnostics { get; set; } = false;

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugFbLogFile")]
	[GlobalizedDescription("OptionDescriptionDebugFbLogFile")]
	[Editor(typeof(AdvancedFileNameEditor), typeof(UITypeEditor)), Parameters("OptionDialogFbLogFile", "OptionDialogFilterFbLogFile")]
	[DefaultValue("/temp/vsdiagfb.log")]
	public string FbLogFile { get; set; } = "/temp/vsdiagfb.log";


	#endregion Model Properties




	// =====================================================================================================
	#region Constructors / Destructors - DebugSettingsModel
	// =====================================================================================================


	public DebugSettingsModel() : this(null)
	{
	}

	public DebugSettingsModel(IBLiveSettings liveSettings)
		: base(C_Package, C_Group, C_LivePrefix, liveSettings)
	{
	}


	#endregion Constructors / Destructors

}
