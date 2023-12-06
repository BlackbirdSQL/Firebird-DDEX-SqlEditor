// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;


namespace BlackbirdSql.EditorExtension.Model.Config;

// =========================================================================================================
//										ResultsTextSettingsModel Class
//
/// <summary>
/// Option Model for Text Results options
/// </summary>
// =========================================================================================================
public class ResultsTextSettingsModel(IBLiveSettings liveSettings)
	: AbstractSettingsModel<ResultsTextSettingsModel>(C_Package, C_Group, C_LivePrefix, liveSettings)
{

	// ---------------------------------------------------------------------------------
	#region Additional Constructors / Destructors - ResultsTextSettingsModel
	// ---------------------------------------------------------------------------------


	public ResultsTextSettingsModel() : this(null)
	{
	}


	#endregion Additional Constructors / Destructors




	// =====================================================================================================
	#region Constants - ResultsTextSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "ResultsText";
	private const string C_LivePrefix = "EditorResultsText";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - ResultsTextSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.ResultsTextSettings";



	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedBytes
	{
		[GlobalizedRadio("EnBytes_Min")]
		BytesMin = 32,
		[GlobalizedRadio("EnBytes_64")]
		Bytes64 = 64,
		[GlobalizedRadio("EnBytes_256")]
		Bytes256 = 256,
		[GlobalizedRadio("EnBytes_1024")]
		Bytes1024 = 1024,
		[GlobalizedRadio("EnBytes_4096")]
		Bytes4096 = 4096,
		[GlobalizedRadio("EnBytes_8192")]
		Bytes8192 = 8192,
	}

	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedOutputFormat
	{
		[GlobalizedRadio("EnOutputFormat_ColAligned")]
		ColAligned,
		[GlobalizedRadio("EnOutputFormat_CommaDelim")]
		CommaDelim,
		[GlobalizedRadio("EnOutputFormat_TabDelim")]
		TabDelim,
		[GlobalizedRadio("EnOutputFormat_SpaceDelim")]
		SpaceDelim,
		[GlobalizedRadio("EnOutputFormat_Custom")]
		Custom,
	}



	// Text results section

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsTextIncludeHeaders")]
	[GlobalizedDescription("OptionDescriptionResultsTextIncludeHeaders")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool IncludeHeaders { get; set; } = true;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsTextOutputQuery")]
	[GlobalizedDescription("OptionDescriptionResultsTextOutputQuery")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool OutputQuery { get; set; } = false;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsTextScrollingResults")]
	[GlobalizedDescription("OptionDescriptionResultsTextScrollingResults")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool ScrollingResults { get; set; } = false;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsTextAlignRightNumerics")]
	[GlobalizedDescription("OptionDescriptionResultsTextAlignRightNumerics")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool AlignRightNumerics { get; set; } = false;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsTextDiscardResults")]
	[GlobalizedDescription("OptionDescriptionResultsTextDiscardResults")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool DiscardResults { get; set; } = false;


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsTextMaxStd")]
	[GlobalizedDescription("OptionDescriptionResultsTextMaxStd")]
	[DefaultValue(ModelConstants.C_DefaultTextMaxCharsPerColumnStd)]
	public EnGlobalizedBytes MaxCharsPerColumnStd { get; set; } = (EnGlobalizedBytes)ModelConstants.C_DefaultTextMaxCharsPerColumnStd;


	[GlobalizedCategory("OptionCategoryTextTabs")]
	[GlobalizedDisplayName("OptionDisplayResultsTextSeparateTabs")]
	[GlobalizedDescription("OptionDescriptionResultsTextSeparateTabs")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	[Automation, RefreshProperties(RefreshProperties.All)]
	public bool SeparateTabs { get; set; } = false;


	// ExecutionResults.SwitchToResultsTabAfterQueryExecutesForText
	[GlobalizedCategory("OptionCategoryTextTabs")]
	[GlobalizedDisplayName("OptionDisplayResultsTextSwitchToResults")]
	[GlobalizedDescription("OptionDescriptionResultsTextSwitchToResults")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	[Automation("SeparateTabs"), ReadOnly(true)]
	public bool SwitchToResults { get; set; } = false;



	[GlobalizedCategory("OptionCategoryTextFormat")]
	[GlobalizedDisplayName("OptionDisplayResultsTextOutputFormat")]
	[GlobalizedDescription("OptionDescriptionResultsTextOutputFormat")]
	[DefaultValue(EnGlobalizedOutputFormat.ColAligned)]
	[Automation, RefreshProperties(RefreshProperties.All)]
	public EnGlobalizedOutputFormat OutputFormat { get; set; } = EnGlobalizedOutputFormat.ColAligned;

	[GlobalizedCategory("OptionCategoryTextFormat")]
	[GlobalizedDisplayName("OptionDisplayResultsTextDelimiter")]
	[GlobalizedDescription("OptionDescriptionResultsTextDelimiter")]
	[TypeConverter(typeof(UomConverter)), LiteralRange("Separator", 1, 1)]
	[DefaultValue(",")]
	[Automation("TextOutputFormat", (int)EnGlobalizedOutputFormat.Custom), ReadOnly(true)]
	public string Delimiter { get; set; } = ",";


	#endregion Property Accessors

}
