// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;

using GlobalizedCategoryAttribute = BlackbirdSql.EditorExtension.Ctl.ComponentModel.GlobalizedCategoryAttribute;
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


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetRowCount")]
	[GlobalizedDescription("OptionDescriptionExecutionSetRowCount")]
	[TypeConverter(typeof(RangeConverter)), Range(0, 999999999)]
	[DefaultValue(ModelConstants.C_DefaultSetRowCount)]
	public int SetRowCount { get; set; } = ModelConstants.C_DefaultSetRowCount;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionSetBlobDisplay")]
	[GlobalizedDescription("OptionDescriptionExecutionSetBlobDisplay")]
	[DefaultValue((EnGlobalizedBlobSubType)ModelConstants.C_DefaultSetBlobDisplay)]
	public EnGlobalizedBlobSubType SetBlobDisplay { get; set; } = (EnGlobalizedBlobSubType)ModelConstants.C_DefaultSetBlobDisplay;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionDefaultOleScripting")]
	[GlobalizedDescription("OptionDescriptionExecutionDefaultOleScripting")]
	[Browsable(false)]
	[TypeConverter(typeof(GlobalOnOffConverter))]
	[DefaultValue(ModelConstants.C_DefaultDefaultOleScripting)]
	public bool DefaultOleScripting { get; set; } = ModelConstants.C_DefaultDefaultOleScripting;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayExecutionTimeout")]
	[GlobalizedDescription("OptionDescriptionExecutionTimeout")]
	[TypeConverter(typeof(UomConverter)), LiteralRange(0, 32000, "SecondsUnlimited")]
	[DefaultValue(ModelConstants.C_DefaultCommandTimeout)]
	public int Timeout { get; set; } = ModelConstants.C_DefaultCommandTimeout;


	#endregion Property Accessors

}
