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
	[GlobalizedDisplayName("OptionDisplayEnableTrace")]
	[GlobalizedDescription("OptionDescriptionEnableTrace")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
#if DEBUG
	[DefaultValue(true)]
	public bool EnableTrace { get; set; } = true;
#else
	[DefaultValue(false)]
	public bool EnableTrace { get; set; } = false;
#endif

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayEnableTracer")]
	[GlobalizedDescription("OptionDescriptionEnableTracer")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableTracer { get; set; } = false;

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayPersistentValidation")]
	[GlobalizedDescription("OptionDescriptionPersistentValidation")]
	[TypeConverter(typeof(GlobalBoolConverter))]
#if DEBUG
	[DefaultValue(false)]
	public bool PersistentValidation { get; set; } = false;
#else
	[DefaultValue(true)]
	public bool PersistentValidation { get; set; } = true;
#endif

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayEnableDiagnosticsLog")]
	[GlobalizedDescription("OptionDescriptionEnableDiagnosticsLog")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
#if DEBUG
	[DefaultValue(true)]
	public bool EnableDiagnosticsLog { get; set; } = true;
#else
	[DefaultValue(false)]
	public bool EnableDiagnosticsLog { get; set; } = false;
#endif

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayEnableSaveExtrapolatedXml")]
	[GlobalizedDescription("OptionDescriptionSaveExtrapolatedXml")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
#if DEBUG
	[DefaultValue(true)]
	public bool EnableSaveExtrapolatedXml { get; set; } = true;
#else
	[DefaultValue(false)]
	public bool EnableSaveExtrapolatedXml { get; set; } = false;
#endif


	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayLogFile")]
	[GlobalizedDescription("OptionDescriptionLogFile")]
	[Editor(typeof(AdvancedFileNameEditor), typeof(UITypeEditor)), Parameters("OptionDialogLogFile", "OptionDialogFilterLogFile")]
	[DefaultValue("/temp/vsdiag.log")]
	public string LogFile { get; set; } = "/temp/vsdiag.log";

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayEnableFbDiagnostics")]
	[GlobalizedDescription("OptionDescriptionEnableFbDiagnostics")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	public bool EnableFbDiagnostics { get; set; } = true;

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayFbLogFile")]
	[GlobalizedDescription("OptionDescriptionFbLogFile")]
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
