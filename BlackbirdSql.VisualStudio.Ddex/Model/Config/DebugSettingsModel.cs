// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using System.Drawing.Design;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model.Config;
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


	public DebugSettingsModel(IBsTransientSettings transientSettings)
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
