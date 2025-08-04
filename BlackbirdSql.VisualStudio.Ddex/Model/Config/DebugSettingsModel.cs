// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using System.Drawing.Design;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Controls.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

using GlobalizedCategoryAttribute = BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel.GlobalizedCategoryAttribute;
using GlobalizedDescriptionAttribute = BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel.GlobalizedDescriptionAttribute;
using GlobalizedDisplayNameAttribute = BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel.GlobalizedDisplayNameAttribute;



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

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - DebugSettingsModel
	// ---------------------------------------------------------------------------------


	public DebugSettingsModel(IBsSettingsProvider transientSettings)
		: base(C_Package, C_Group, C_PropertyPrefix, transientSettings)
	{

	}

	public DebugSettingsModel() : this(null)
	{
	}


	#endregion Constructors / Destructors





	// =====================================================================================================
	#region Constants - DebugSettingsModel
	// =====================================================================================================


	private const string C_Package = "Ddex";
	private const string C_Group = "Debug";
	private const string C_PropertyPrefix = "DdexDebug";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - DebugSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\Ddex.DebugSettings";




	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedSourceLevels
	{
		[GlobalizedRadio("EnSourceLevels_Off")]
		Off = 0,
		[GlobalizedRadio("EnSourceLevels_Critical")]
		Critical = 1,
		[GlobalizedRadio("EnSourceLevels_Error")]
		Error = 3,
		[GlobalizedRadio("EnSourceLevels_Warning")]
		Warning = 7,
		[GlobalizedRadio("EnSourceLevels_Information")]
		Information = 0xF,
		[GlobalizedRadio("EnSourceLevels_TraceOnly")]
		TraceOnly = 0x10,
		[GlobalizedRadio("EnSourceLevels_DebugOnly")]
		DebugOnly = 0x20,
		[GlobalizedRadio("EnSourceLevels_TraceDebug")]
		TraceDebug = 0x30,
		[GlobalizedRadio("EnSourceLevels_Verbose")]
		Verbose = 0x1F,
		[GlobalizedRadio("EnSourceLevels_ActivityTracing")]
		ActivityTracing = 0xFF00,
		[GlobalizedRadio("EnSourceLevels_All")]
		All = 0xFFFF
	}




	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableTrace")]
	[GlobalizedDescription("OptionDescriptionDebugEnableTrace")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableTrace { get; set; } = false;


	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableActivityLogging")]
	[GlobalizedDescription("OptionDescriptionDebugEnableActivityLogging")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableActivityLogging { get; set; } = false;

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableEventSourceLogging")]
	[GlobalizedDescription("OptionDescriptionDebugEnableEventSourceLogging")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableEventSourceLogging { get; set; } = false;


	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableDiagnosticsLog")]
	[GlobalizedDescription("OptionDescriptionDebugEnableDiagnosticsLog")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableDiagnosticsLog { get; set; } = false;

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableDebugExceptions")]
	[GlobalizedDescription("OptionDescriptionDebugEnableDebugExceptions")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableDebugExceptions { get; set; } = false;

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableExpected")]
	[GlobalizedDescription("OptionDescriptionDebugEnableExpected")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableExpected { get; set; } = false;

	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableSaveExtrapolatedXml")]
	[GlobalizedDescription("OptionDescriptionDebugSaveExtrapolatedXml")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool EnableSaveExtrapolatedXml { get; set; } = false;


	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableActivityTracing")]
	[GlobalizedDescription("OptionDescriptionDebugEnableActivityTracing")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableActivityTracing { get; set; } = false;


	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugLogFile")]
	[GlobalizedDescription("OptionDescriptionDebugLogFile")]
	[Editor(typeof(AdvancedFileNameEditor), typeof(UITypeEditor)), Parameters("OptionDialogLogFile", "OptionDialogFilterLogFile")]
	[DefaultValue("/temp/vsdiag.log")]
	public string LogFile { get; set; } = "/temp/vsdiag.log";



	[GlobalizedCategory("OptionCategoryEvsLevel")]
	[GlobalizedDisplayName("OptionDisplayEvsLevelDdex")]
	[GlobalizedDescription("OptionDescriptionEvsLevelDdex")]
	[DefaultValue(EnGlobalizedSourceLevels.Off)]
	public EnGlobalizedSourceLevels EvsLevelDdex { get; set; } = EnGlobalizedSourceLevels.Off;

	[GlobalizedCategory("OptionCategoryEvsLevel")]
	[GlobalizedDisplayName("OptionDisplayEvsLevelController")]
	[GlobalizedDescription("OptionDescriptionEvsLevelController")]
	[DefaultValue(EnGlobalizedSourceLevels.Off)]
	public EnGlobalizedSourceLevels EvsLevelController { get; set; } = EnGlobalizedSourceLevels.Off;

	[GlobalizedCategory("OptionCategoryEvsLevel")]
	[GlobalizedDisplayName("OptionDisplayEvsLevelLanguage")]
	[GlobalizedDescription("OptionDescriptionEvsLevelLanguage")]
	[DefaultValue(EnGlobalizedSourceLevels.Off)]
	public EnGlobalizedSourceLevels EvsLevelLanguage { get; set; } = EnGlobalizedSourceLevels.Off;

	[GlobalizedCategory("OptionCategoryEvsLevel")]
	[GlobalizedDisplayName("OptionDisplayEvsLevelEditor")]
	[GlobalizedDescription("OptionDescriptionEvsLevelEditor")]
	[DefaultValue(EnGlobalizedSourceLevels.Off)]
	public EnGlobalizedSourceLevels EvsLevelEditor { get; set; } = EnGlobalizedSourceLevels.Off;

	[GlobalizedCategory("OptionCategoryEvsLevel")]
	[GlobalizedDisplayName("OptionDisplayEvsLevelShared")]
	[GlobalizedDescription("OptionDescriptionEvsLevelShared")]
	[DefaultValue(EnGlobalizedSourceLevels.Off)]
	public EnGlobalizedSourceLevels EvsLevelShared { get; set; } = EnGlobalizedSourceLevels.Off;

	[GlobalizedCategory("OptionCategoryEvsLevel")]
	[GlobalizedDisplayName("OptionDisplayEvsLevelCore")]
	[GlobalizedDescription("OptionDescriptionEvsLevelCore")]
	[DefaultValue(EnGlobalizedSourceLevels.Off)]
	public EnGlobalizedSourceLevels EvsLevelCore { get; set; } = EnGlobalizedSourceLevels.Off;

	[GlobalizedCategory("OptionCategoryEvsLevel")]
	[GlobalizedDisplayName("OptionDisplayEvsLevelData")]
	[GlobalizedDescription("OptionDescriptionEvsLevelData")]
	[DefaultValue(EnGlobalizedSourceLevels.Off)]
	public EnGlobalizedSourceLevels EvsLevelData { get; set; } = EnGlobalizedSourceLevels.Off;

	[GlobalizedCategory("OptionCategoryEvsLevel")]
	[GlobalizedDisplayName("OptionDisplayEvsLevelSys")]
	[GlobalizedDescription("OptionDescriptionEvsLevelSys")]
	[DefaultValue(EnGlobalizedSourceLevels.Off)]
	public EnGlobalizedSourceLevels EvsLevelSys { get; set; } = EnGlobalizedSourceLevels.Off;


	#endregion Property Accessors

}
