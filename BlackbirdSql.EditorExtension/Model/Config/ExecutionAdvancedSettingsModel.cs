// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;

using GlobalizedCategoryAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedCategoryAttribute;
using GlobalizedDescriptionAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedDescriptionAttribute;
using GlobalizedDisplayNameAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedDisplayNameAttribute;



namespace BlackbirdSql.EditorExtension.Model.Config;


// =========================================================================================================
//										ExecutionAdvancedSettingsModel Class
//
/// <summary>
/// Option Model for Advanced execution options
/// </summary>
// =========================================================================================================
public class ExecutionAdvancedSettingsModel : AbstractSettingsModel<ExecutionAdvancedSettingsModel>
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - ExecutionAdvancedSettingsModel
	// ---------------------------------------------------------------------------------


	public ExecutionAdvancedSettingsModel() : this(null)
	{
	}


	public ExecutionAdvancedSettingsModel(IBsTransientSettings transientSettings)
		: base(C_Package, C_Group, C_PropertyPrefix, transientSettings)
	{
	}


	#endregion Constructors / Destructors




	// =====================================================================================================
	#region Constants - ExecutionAdvancedSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "ExecutionAdvanced";
	private const string C_PropertyPrefix = "EditorExecutionAdvanced";


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
	[GlobalizedDisplayName("OptionDisplayExecutionDisconnectOnCompletion")]
	[GlobalizedDescription("OptionDescriptionExecutionDisconnectOnCompletion")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool DisconnectOnCompletion { get; set; } = false;

	[GlobalizedCategory("OptionCategoryAdvanced")]
	[GlobalizedDisplayName("OptionDisplayExecutionIsolationLevel")]
	[GlobalizedDescription("OptionDescriptionExecutionIsolationLevel")]
	[DefaultValue((EnGlobalizedIsolationLevel)SysConstants.C_DefaultIsolationLevel)]
	public EnGlobalizedIsolationLevel IsolationLevel { get; set; } = (EnGlobalizedIsolationLevel)SysConstants.C_DefaultIsolationLevel;


	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetCount")]
	[GlobalizedDescription("OptionDescriptionExecutionSetCount")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetCount)]
	public bool SetCount { get; set; } = SharedConstants.C_DefaultSetCount;


	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetPlanOnly")]
	[GlobalizedDescription("OptionDescriptionExecutionSetPlanOnly")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetPlanOnly)]
	public bool SetNoExec { get; set; } = SharedConstants.C_DefaultSetPlanOnly;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetPlan")]
	[GlobalizedDescription("OptionDescriptionExecutionSetPlan")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetPlan)]
	public bool SetShowplanText { get; set; } = SharedConstants.C_DefaultSetPlan;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetExplain")]
	[GlobalizedDescription("OptionDescriptionExecutionSetExplain")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetExplain)]
	public bool SetPlanXml { get; set; } = SharedConstants.C_DefaultSetExplain;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetParseOnly")]
	[GlobalizedDescription("OptionDescriptionExecutionSetParseOnly")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetParseOnly)]
	public bool SetParseOnly { get; set; } = SharedConstants.C_DefaultSetParseOnly;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetConcatenationNull")]
	[GlobalizedDescription("OptionDescriptionExecutionSetConcatenationNull")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetConcatenationNull)]
	public bool SetConcatenationNull { get; set; } = SharedConstants.C_DefaultSetConcatenationNull;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetBail")]
	[GlobalizedDescription("OptionDescriptionExecutionSetBail")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetBail)]
	public bool SetBail { get; set; } = SharedConstants.C_DefaultSetBail;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetPlanText")]
	[GlobalizedDescription("OptionDescriptionExecutionSetPlanText")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetPlanText)]
	public bool SetPlanText { get; set; } = SharedConstants.C_DefaultSetPlanText;


	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetStats")]
	[GlobalizedDescription("OptionDescriptionExecutionSetStats")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetStats)]
	public bool SetStats { get; set; } = SharedConstants.C_DefaultSetStats;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetWarnings")]
	[GlobalizedDescription("OptionDescriptionExecutionSetWarnings")]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetWarnings)]
	public bool SetWarnings { get; set; } = SharedConstants.C_DefaultSetWarnings;



	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetStatisticsIO")]
	[GlobalizedDescription("OptionDescriptionExecutionSetStatisticsIO")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(SharedConstants.C_DefaultSetStatisticsIO)]
	public bool SetStatisticsIO { get; set; } = SharedConstants.C_DefaultSetStatisticsIO;


	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionDeadlockPriority")]
	[GlobalizedDescription("OptionDescriptionExecutionDeadlockPriority")]
	[Browsable(false)]
	[DefaultValue(EnGlobalizedDeadlockPriority.Low)]
	public EnGlobalizedDeadlockPriority DeadlockPriority { get; set; } = EnGlobalizedDeadlockPriority.Low;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionLockTimeout")]
	[GlobalizedDescription("OptionDescriptionExecutionLockTimeout")]
	[TypeConverter(typeof(UomConverter)), LiteralRange(0, int.MaxValue, "SecondsDisabled")]
	[DefaultValue(SharedConstants.C_DefaultLockTimeout)]
	public int LockTimeout { get; set; } = SharedConstants.C_DefaultLockTimeout;


	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionCostLimit")]
	[GlobalizedDescription("OptionDescriptionExecutionCostLimit")]
	[Browsable(false)]
	[TypeConverter(typeof(UomConverter)), LiteralRange(0, 100000, "Seconds")]
	[DefaultValue(0)]
	public int CostLimit { get; set; } = 0;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSuppressHeaders")]
	[GlobalizedDescription("OptionDescriptionExecutionSuppressHeaders")]
	[TypeConverter(typeof(GlobalOnOffInverter))]
	[DefaultValue(SharedConstants.C_DefaultSuppressHeaders)]
	public bool SuppressHeaders { get; set; } = SharedConstants.C_DefaultSuppressHeaders;


	#endregion Property Accessors

}
