// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;
using BlackbirdSql.Sys;

using GlobalizedCategoryAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedCategoryAttribute;
using GlobalizedDescriptionAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedDescriptionAttribute;
using GlobalizedDisplayNameAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedDisplayNameAttribute;



namespace BlackbirdSql.EditorExtension.Model.Config;


// =========================================================================================================
//										ExecutionSettingsModel Class
//
/// <summary>
/// Option Model for General Execution options
/// </summary>
// =========================================================================================================
public class ExecutionSettingsModel(IBTransientSettings transientSettings)
	: AbstractSettingsModel<ExecutionSettingsModel>(C_Package, C_Group, C_LivePrefix, transientSettings)
{

	// ---------------------------------------------------------------------------------
	#region Additional Constructors / Destructors - ExecutionSettingsModel
	// ---------------------------------------------------------------------------------


	public ExecutionSettingsModel() : this(null)
	{
	}


	#endregion Additional Constructors / Destructors




	// =====================================================================================================
	#region Constants - ExecutionSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "Execution";
	private const string C_LivePrefix = "EditorExecutionGeneral";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - ExecutionSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.ExecutionSettings";



	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedBlobSubType
	{
		[GlobalizedRadio("EnBlobSubType_Off")]
		Off = int.MinValue,
		[GlobalizedRadio("EnBlobSubType_Binary")]
		Binary = 0,
		[GlobalizedRadio("EnBlobSubType_Text")]
		Text = 1,
		[GlobalizedRadio("EnBlobSubType_BLR")]
		BLR = 2,
		[GlobalizedRadio("EnBlobSubType_All")]
		All = int.MaxValue
	}


	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedExecutionTimeout
	{
		[GlobalizedRadio("EnExecutionTimeout_1m")]
		Timeout_1m = 1,
		[GlobalizedRadio("EnExecutionTimeout_2m")]
		Timeout_2m = 2,
		[GlobalizedRadio("EnExecutionTimeout_5m")]
		Timeout_5m = 5,
		[GlobalizedRadio("EnExecutionTimeout_10m")]
		Timeout_10m = 10,
		[GlobalizedRadio("EnExecutionTimeout_15m")]
		Timeout_15m = 15,
		[GlobalizedRadio("EnExecutionTimeout_None")]
		Timeout_None = 0
	}


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionTtsDefault")]
	[GlobalizedDescription("OptionDescriptionExecutionTtsDefault")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(true)]
	public bool TtsDefault { get; set; } = true;


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetRowCount")]
	[GlobalizedDescription("OptionDescriptionExecutionSetRowCount")]
	[TypeConverter(typeof(RangeConverter)), Range(0, 999999999)]
	[DefaultValue(SysConstants.C_DefaultSetRowCount)]
	public int SetRowCount { get; set; } = SysConstants.C_DefaultSetRowCount;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetBlobDisplay")]
	[GlobalizedDescription("OptionDescriptionExecutionSetBlobDisplay")]
	[DefaultValue((EnGlobalizedBlobSubType)SysConstants.C_DefaultSetBlobDisplay)]
	public EnGlobalizedBlobSubType SetBlobDisplay { get; set; } = (EnGlobalizedBlobSubType)SysConstants.C_DefaultSetBlobDisplay;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionDefaultOleScripting")]
	[GlobalizedDescription("OptionDescriptionExecutionDefaultOleScripting")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SysConstants.C_DefaultDefaultOleScripting)]
	public bool DefaultOleScripting { get; set; } = SysConstants.C_DefaultDefaultOleScripting;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionTimeout")]
	[GlobalizedDescription("OptionDescriptionExecutionTimeout")]
	[DefaultValue(SysConstants.C_DefaultExecutionTimeout)]
	public EnGlobalizedExecutionTimeout ExecutionTimeout { get; set; } = SysConstants.C_DefaultExecutionTimeout;


	#endregion Property Accessors

}
