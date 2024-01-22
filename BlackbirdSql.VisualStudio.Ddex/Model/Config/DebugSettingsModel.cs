// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using System.Drawing.Design;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.VisualStudio.Ddex.Controls.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

using GlobalizedCategoryAttribute = BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel.GlobalizedCategoryAttribute;
using GlobalizedDisplayNameAttribute = BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel.GlobalizedDisplayNameAttribute;


namespace BlackbirdSql.VisualStudio.Ddex.Model.Config;

// =========================================================================================================
//										DebugSettingsModel Class
//
/// <summary>
/// Settings Model for Debug options
/// </summary>
// =========================================================================================================
public class DebugSettingsModel(IBTransientSettings transientSettings)
	: AbstractSettingsModel<DebugSettingsModel>(C_Package, C_Group, C_LivePrefix, transientSettings)
{

	// ---------------------------------------------------------------------------------
	#region Additional Constructors / Destructors - DebugSettingsModel
	// ---------------------------------------------------------------------------------


	public DebugSettingsModel() : this(null)
	{
	}


	#endregion Additional Constructors / Destructors




	// =====================================================================================================
	#region Constants - DebugSettingsModel
	// =====================================================================================================


	private const string C_Package = "Ddex";
	private const string C_Group = "Debug";
	private const string C_LivePrefix = "DdexDebug";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - DebugSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\Ddex.DebugSettings";



	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableTrace")]
	[GlobalizedDescription("OptionDescriptionDebugEnableTrace")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableTrace { get; set; } = false;


	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableTracer")]
	[GlobalizedDescription("OptionDescriptionDebugEnableTracer")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableTracer { get; set; } = false;


	[GlobalizedCategory("OptionCategoryDebugging")]
	[GlobalizedDisplayName("OptionDisplayDebugEnableDiagnosticsLog")]
	[GlobalizedDescription("OptionDescriptionDebugEnableDiagnosticsLog")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool EnableDiagnosticsLog { get; set; } = false;

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


	#endregion Property Accessors

}
