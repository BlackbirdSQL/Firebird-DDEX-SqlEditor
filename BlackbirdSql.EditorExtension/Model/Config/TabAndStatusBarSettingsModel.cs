// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using System.Drawing;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;

using GlobalizedCategoryAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedCategoryAttribute;
using GlobalizedDisplayNameAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedDisplayNameAttribute;
using GlobalizedDescriptionAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedDescriptionAttribute;


namespace BlackbirdSql.EditorExtension.Model.Config;

// =========================================================================================================
//										TabAndStatusBarSettingsModel Class
//
/// <summary>
/// Option Model for Tabs ad Statuc bar options
/// </summary>
// =========================================================================================================
public class TabAndStatusBarSettingsModel(IBTransientSettings transientSettings)
	: AbstractSettingsModel<TabAndStatusBarSettingsModel>(C_Package, C_Group, C_LivePrefix, transientSettings)
{

	// ---------------------------------------------------------------------------------
	#region Additional Constructors / Destructors - TabAndStatusBarSettingsModel
	// ---------------------------------------------------------------------------------


	public TabAndStatusBarSettingsModel() : this(null)
	{
	}


	#endregion Additional Constructors / Destructors




	// =====================================================================================================
	#region Constants - ResultsSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "TabAndStatusBar";
	private const string C_LivePrefix = "EditorStatus";
	private const string C_DefaultControlColor = "Control";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - ResultsSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.TabAndStatusBarSettings";



	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedExecutionTimeMethod
	{
		[GlobalizedRadio("EnShowTimeOption_None")]
		None,
		[GlobalizedRadio("EnShowTimeOption_Elapsed")]
		Elapsed,
		[GlobalizedRadio("EnShowTimeOption_End")]
		End
	}


	[GlobalizedCategory("OptionCategoryStatusBar")]
	[GlobalizedDisplayName("OptionDisplayStatusBarExecutionTimeMethod")]
	[GlobalizedDescription("OptionDescriptionStatusBarExecutionTimeMethod")]
	[DefaultValue(EnGlobalizedExecutionTimeMethod.Elapsed)]
	public EnGlobalizedExecutionTimeMethod BarExecutionTimeMethod { get; set; } = EnGlobalizedExecutionTimeMethod.Elapsed;

	[GlobalizedCategory("OptionCategoryStatusBar")]
	[GlobalizedDisplayName("OptionDisplayStatusBarIncludeDatabaseName")]
	[GlobalizedDescription("OptionDescriptionStatusBarIncludeDatabaseName")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool BarIncludeDatabaseName { get; set; } = true;

	[GlobalizedCategory("OptionCategoryStatusBar")]
	[GlobalizedDisplayName("OptionDisplayStatusBarIncludeLoginName")]
	[GlobalizedDescription("OptionDescriptionStatusBarIncludeLoginName")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool BarIncludeLoginName { get; set; } = true;

	[GlobalizedCategory("OptionCategoryStatusBar")]
	[GlobalizedDisplayName("OptionDisplayStatusBarIncludeRowCount")]
	[GlobalizedDescription("OptionDescriptionStatusBarIncludeRowCount")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool BarIncludeRowCount { get; set; } = true;

	[GlobalizedCategory("OptionCategoryStatusBar")]
	[GlobalizedDisplayName("OptionDisplayStatusBarIncludeServerName")]
	[GlobalizedDescription("OptionDescriptionStatusBarIncludeServerName")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool BarIncludeServerName { get; set; } = true;



	[GlobalizedCategory("OptionCategoryStatusBarColor")]
	[GlobalizedDisplayName("OptionDisplayStatusBarBackgroundColor")]
	[GlobalizedDescription("OptionDescriptionStatusBarBackgroundColor")]
	[OverrideDataType(EnSettingDataType.String, true)]
	[DefaultValue(typeof(Color), C_DefaultControlColor)]
	public Color BarBackgroundColor { get; set; } = SystemColors.Control;



	[GlobalizedCategory("OptionCategoryTabText")]
	[GlobalizedDisplayName("OptionDisplayTabTextIncludeDatabaseName")]
	[GlobalizedDescription("OptionDescriptionTabTextIncludeDatabaseName")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool TabIncludeDatabaseName { get; set; } = false;

	[GlobalizedCategory("OptionCategoryTabText")]
	[GlobalizedDisplayName("OptionDisplayTabTextIncludeLoginName")]
	[GlobalizedDescription("OptionDescriptionTabTextIncludeLoginName")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool TabIncludeLoginName { get; set; } = false;

	[GlobalizedCategory("OptionCategoryTabText")]
	[GlobalizedDisplayName("OptionDisplayTabTextIncludeFileName")]
	[GlobalizedDescription("OptionDescriptionTabTextIncludeFileName")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool TabIncludeFileName { get; set; } = true;

	[GlobalizedCategory("OptionCategoryTabText")]
	[GlobalizedDisplayName("OptionDisplayTabTextIncludeServerName")]
	[GlobalizedDescription("OptionDescriptionTabTextIncludeServerName")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool TabIncludeServerName { get; set; } = false;


	#endregion Property Accessors

}
