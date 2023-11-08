// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.ComponentModel;
using System.Drawing.Design;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Controls.ComponentModel;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;

namespace BlackbirdSql.EditorExtension.Model.Config;

// =========================================================================================================
//										ResultsGridSettingsModel Class
//
/// <summary>
/// Option Model for Results options
/// </summary>
// =========================================================================================================
public class ResultsGridSettingsModel : AbstractSettingsModel<ResultsGridSettingsModel>
{

	private const string C_Package = "Editor";
	private const string C_Group = "ResultsGrid";
	private const string C_LivePrefix = "EditorResultsGrid";

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.ResultsGridSettings";




	// =====================================================================================================
	#region Model Properties - ResultsGridSettingsModel
	// =====================================================================================================


	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedMegabytes
	{
		[GlobalizedRadio("EnMegabytes_1")]
		Megabytes1 = 1048576,
		[GlobalizedRadio("EnMegabytes_2")]
		Megabytes2 = 2097152,
		[GlobalizedRadio("EnMegabytes_5")]
		Megabytes5 = 5242880,
		[GlobalizedRadio("EnMegabytes_Max")]
		MegabytesMax = int.MaxValue - 1
	}

	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedKilobytes
	{
		[GlobalizedRadio("EnKilobytes_Min")]
		KilobytesMin = 32,
		[GlobalizedRadio("EnKilobytes_Qtr")]
		KilobytesQtr = 256,
		[GlobalizedRadio("EnKilobytes_1")]
		Kilobytes1 = 1024,
		[GlobalizedRadio("EnKilobytes_4")]
		Kilobytes4 = 4096,
		[GlobalizedRadio("EnKilobytes_16")]
		Kilobytes16 = 16384,
		[GlobalizedRadio("EnKilobytes_64")]
		Kilobytes64 = 65535,
	}



	// Grid results section

	// ExecutionResults.OutputQueryForGrid
	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsGridOutputQuery")]
	[GlobalizedDescription("OptionDescriptionResultsGridOutputQuery")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool OutputQuery { get; set; } = false;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsGridSingleTab")]
	[GlobalizedDescription("OptionDescriptionResultsGridSingleTab")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(true)]
	public bool SingleTab { get; set; } = true;

	// ExecutionResults.IncludeColumnHeadersWhileSavingGridResults
	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsGridSaveIncludeHeaders")]
	[GlobalizedDescription("OptionDescriptionResultsGridSaveIncludeHeaders")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool SaveIncludeHeaders { get; set; } = false;

	// ExecutionResults.QuoteStringsContainingCommas
	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsGridCsvQuoteStringsCommas")]
	[GlobalizedDescription("OptionDescriptionResultsGridCsvQuoteStringsCommas")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	public bool CsvQuoteStringsCommas { get; set; } = false;

	// ExecutionResults.DiscardResultsForGrid
	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayResultsGridDiscardResults")]
	[GlobalizedDescription("OptionDescriptionResultsGridDiscardResults")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	public bool DiscardResults { get; set; } = false;

	// ExecutionResults.DisplayResultInSeparateTabForGrid
	[GlobalizedCategory("OptionCategoryGridTabs")]
	[GlobalizedDisplayName("OptionDisplayResultsGridSeparateTabs")]
	[GlobalizedDescription("OptionDescriptionResultsGridSeparateTabs")]
	[TypeConverter(typeof(GlobalYesNoConverter))]
	[DefaultValue(false)]
	[Automation, RefreshProperties(RefreshProperties.All)]
	public bool SeparateTabs { get; set; } = false;


	// ExecutionResults.SwitchToResultsTabAfterQueryExecutesForGrid
	[GlobalizedCategory("OptionCategoryGridTabs")]
	[GlobalizedDisplayName("OptionDisplayResultsGridSwitchToResults")]
	[GlobalizedDescription("OptionDescriptionResultsGridSwitchToResults")]
	[TypeConverter(typeof(GlobalEnableDisableConverter))]
	[DefaultValue(false)]
	[Automation("SeparateTabs"), ReadOnly(true)]
	public bool SwitchToResults { get; set; } = false;

	[GlobalizedCategory("OptionCategoryGridULimits")]
	[GlobalizedDisplayName("OptionDisplayResultsGridMaxXml")]
	[GlobalizedDescription("OptionDescriptionResultsGridMaxXml")]
	[DefaultValue(ModelConstants.C_DefaultGridMaxCharsPerColumnXml)]
	public EnGlobalizedMegabytes MaxCharsPerColumnXml { get; set; } = (EnGlobalizedMegabytes)ModelConstants.C_DefaultGridMaxCharsPerColumnXml;

	[GlobalizedCategory("OptionCategoryGridULimits")]
	[GlobalizedDisplayName("OptionDisplayResultsGridMaxStd")]
	[GlobalizedDescription("OptionDescriptionResultsGridMaxStd")]
	[DefaultValue(ModelConstants.C_DefaultGridMaxCharsPerColumnStd)]
	public EnGlobalizedKilobytes MaxCharsPerColumnStd { get; set; } = (EnGlobalizedKilobytes)ModelConstants.C_DefaultGridMaxCharsPerColumnStd;


	#endregion Model Properties




	// =====================================================================================================
	#region Constructors / Destructors - ResultsGridSettingsModel
	// =====================================================================================================


	public ResultsGridSettingsModel() : this(null)
	{
	}

	public ResultsGridSettingsModel(IBLiveSettings liveSettings)
		: base(C_Package, C_Group, C_LivePrefix, liveSettings)
	{
	}


	#endregion Constructors / Destructors

}
