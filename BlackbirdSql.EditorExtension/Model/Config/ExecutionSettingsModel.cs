// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;
using BlackbirdSql.Sys.Interfaces;

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
public class ExecutionSettingsModel : AbstractSettingsModel<ExecutionSettingsModel>
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - ExecutionSettingsModel
	// ---------------------------------------------------------------------------------


	public ExecutionSettingsModel() : this(null)
	{
	}


	public ExecutionSettingsModel(IBsSettingsProvider transientSettings)
		: base(C_Package, C_Group, C_PropertyPrefix, transientSettings)
	{
	}


	#endregion Constructors / Destructors




	// =====================================================================================================
	#region Constants - ExecutionSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "Execution";
	private const string C_PropertyPrefix = "EditorExecutionGeneral";


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
	[GlobalizedDisplayName("OptionDisplayExecutionTimeout")]
	[GlobalizedDescription("OptionDescriptionExecutionTimeout")]
	[DefaultValue(SharedConstants.C_DefaultExecutionTimeout)]
	public EnGlobalizedExecutionTimeout ExecutionTimeout { get; set; } = SharedConstants.C_DefaultExecutionTimeout;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionBatchSeparator")]
	[GlobalizedDescription("OptionDescriptionExecutionBatchSeparator")]
	[DefaultValue(SharedConstants.C_DefaultBatchSeparator)]
	public string BatchSeparator { get; set; } = SharedConstants.C_DefaultBatchSeparator;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetRowCount")]
	[GlobalizedDescription("OptionDescriptionExecutionSetRowCount")]
	[TypeConverter(typeof(RangeConverter)), Range(0, 999999999)]
	[DefaultValue(SharedConstants.C_DefaultSetRowCount)]
	public int SetRowCount { get; set; } = SharedConstants.C_DefaultSetRowCount;

	[GlobalizedCategory("OptionCategoryObsolete")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetBlobDisplay")]
	[GlobalizedDescription("OptionDescriptionExecutionSetBlobDisplay")]
	[DefaultValue((EnGlobalizedBlobSubType)SharedConstants.C_DefaultSetBlobDisplay)]
	public EnGlobalizedBlobSubType SetBlobDisplay { get; set; } = (EnGlobalizedBlobSubType)SharedConstants.C_DefaultSetBlobDisplay;


	#endregion Property Accessors

}
