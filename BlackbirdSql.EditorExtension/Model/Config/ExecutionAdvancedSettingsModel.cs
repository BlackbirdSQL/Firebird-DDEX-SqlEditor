// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;

using GlobalizedCategoryAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedCategoryAttribute;
using GlobalizedDisplayNameAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedDisplayNameAttribute;
using GlobalizedDescriptionAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedDescriptionAttribute;


namespace BlackbirdSql.EditorExtension.Model.Config;

// =========================================================================================================
//										ExecutionAdvancedSettingsModel Class
//
/// <summary>
/// Option Model for Advanced execution options
/// </summary>
// =========================================================================================================
public class ExecutionAdvancedSettingsModel(IBTransientSettings transientSettings)
	: AbstractSettingsModel<ExecutionAdvancedSettingsModel>(C_Package, C_Group, C_LivePrefix, transientSettings)
{

	// ---------------------------------------------------------------------------------
	#region Additional Constructors / Destructors - ExecutionAdvancedSettingsModel
	// ---------------------------------------------------------------------------------


	public ExecutionAdvancedSettingsModel() : this(null)
	{
	}


	#endregion Additional Constructors / Destructors




	// =====================================================================================================
	#region Constants - ExecutionAdvancedSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "ExecutionAdvanced";
	private const string C_LivePrefix = "EditorExecutionAdvanced";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - ExecutionAdvancedSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.ExecutionAdvancedSettings";



	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedIsolationLevel
	{
		[GlobalizedRadio("EnIsolationLevel_ReadCommitted")]
		ReadCommitted = 0x1000,
		[GlobalizedRadio("EnIsolationLevel_ReadUncommitted")]
		ReadUncommitted = 0x100,
		[GlobalizedRadio("EnIsolationLevel_RepeatableRead")]
		RepeatableRead = 0x10000,
		[GlobalizedRadio("EnIsolationLevel_Serializable")]
		Serializable = 0x100000
	}

	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedDeadlockPriority
	{
		[GlobalizedRadio("EnDeadlockPriority_Normal")]
		Normal,
		[GlobalizedRadio("EnDeadlockPriority_Low")]
		Low
	}




	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetCount")]
	[GlobalizedDescription("OptionDescriptionExecutionSetCount")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetCount)]
	public bool SetCount { get; set; } = ModelConstants.C_DefaultSetCount;


	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetPlanOnly")]
	[GlobalizedDescription("OptionDescriptionExecutionSetPlanOnly")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetPlanOnly)]
	public bool SetNoExec { get; set; } = ModelConstants.C_DefaultSetPlanOnly;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetPlan")]
	[GlobalizedDescription("OptionDescriptionExecutionSetPlan")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetPlan)]
	public bool SetShowplanText { get; set; } = ModelConstants.C_DefaultSetPlan;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetExplain")]
	[GlobalizedDescription("OptionDescriptionExecutionSetExplain")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetExplain)]
	public bool SetPlanXml { get; set; } = ModelConstants.C_DefaultSetExplain;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetParseOnly")]
	[GlobalizedDescription("OptionDescriptionExecutionSetParseOnly")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetParseOnly)]
	public bool SetParseOnly { get; set; } = ModelConstants.C_DefaultSetParseOnly;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetConcatenationNull")]
	[GlobalizedDescription("OptionDescriptionExecutionSetConcatenationNull")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetConcatenationNull)]
	public bool SetConcatenationNull { get; set; } = ModelConstants.C_DefaultSetConcatenationNull;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetBail")]
	[GlobalizedDescription("OptionDescriptionExecutionSetBail")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetBail)]
	public bool SetBail { get; set; } = ModelConstants.C_DefaultSetBail;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetPlanText")]
	[GlobalizedDescription("OptionDescriptionExecutionSetPlanText")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetPlanText)]
	public bool SetPlanText { get; set; } = ModelConstants.C_DefaultSetPlanText;


	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetStats")]
	[GlobalizedDescription("OptionDescriptionExecutionSetStats")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetStats)]
	public bool SetStats { get; set; } = ModelConstants.C_DefaultSetStats;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetWarnings")]
	[GlobalizedDescription("OptionDescriptionExecutionSetWarnings")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetWarnings)]
	public bool SetWarnings { get; set; } = ModelConstants.C_DefaultSetWarnings;



	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetStatisticsIO")]
	[GlobalizedDescription("OptionDescriptionExecutionSetStatisticsIO")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultSetStatisticsIO)]
	public bool SetStatisticsIO { get; set; } = ModelConstants.C_DefaultSetStatisticsIO;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionIsolationLevel")]
	[GlobalizedDescription("OptionDescriptionExecutionIsolationLevel")]
	[DefaultValue((EnGlobalizedIsolationLevel)ModelConstants.C_DefaultIsolationLevel)]
	public EnGlobalizedIsolationLevel IsolationLevel { get; set; } = (EnGlobalizedIsolationLevel)ModelConstants.C_DefaultIsolationLevel;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionDeadlockPriority")]
	[GlobalizedDescription("OptionDescriptionExecutionDeadlockPriority")]
	[Browsable(false)]
	[DefaultValue(EnGlobalizedDeadlockPriority.Low)]
	public EnGlobalizedDeadlockPriority DeadlockPriority { get; set; } = EnGlobalizedDeadlockPriority.Low;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionLockTimeout")]
	[GlobalizedDescription("OptionDescriptionExecutionLockTimeout")]
	[TypeConverter(typeof(UomConverter)), LiteralRange(0, int.MaxValue, "SecondsDisabled")]
	[DefaultValue(ModelConstants.C_DefaultLockTimeout)]
	public int LockTimeout { get; set; } = ModelConstants.C_DefaultLockTimeout;


	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionCostLimit")]
	[GlobalizedDescription("OptionDescriptionExecutionCostLimit")]
	[Browsable(false)]
	[TypeConverter(typeof(UomConverter)), LiteralRange(0, 100000, "Seconds")]
	[DefaultValue(0)]
	public int CostLimit { get; set; } = 0;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionSuppressHeaders")]
	[GlobalizedDescription("OptionDescriptionExecutionSuppressHeaders")]
	[TypeConverter(typeof(GlobalOnOffInverter))]
	[DefaultValue(ModelConstants.C_DefaultSuppressHeaders)]
	public bool SuppressHeaders { get; set; } = ModelConstants.C_DefaultSuppressHeaders;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionDisconnectOnCompletion")]
	[GlobalizedDescription("OptionDescriptionExecutionDisconnectOnCompletion")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool DisconnectOnCompletion { get; set; } = false;


	#endregion Property Accessors

}
