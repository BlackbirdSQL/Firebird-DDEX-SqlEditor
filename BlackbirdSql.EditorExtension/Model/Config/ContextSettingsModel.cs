// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;
using BlackbirdSql.Sys;



namespace BlackbirdSql.EditorExtension.Model.Config;


// =========================================================================================================
//										ContextSettingsModel Class
//
/// <summary>
/// Option Model for Pane context options
/// </summary>
// =========================================================================================================
public class ContextSettingsModel(IBTransientSettings transientSettings)
	: AbstractSettingsModel<ContextSettingsModel>(C_Package, C_Group, C_LivePrefix, transientSettings)
{

	// ---------------------------------------------------------------------------------
	#region Additional Constructors / Destructors - ContextSettingsModel
	// ---------------------------------------------------------------------------------


	public ContextSettingsModel() : this(null)
	{
	}


	#endregion Additional Constructors / Destructors




	// =====================================================================================================
	#region Constants - ContextSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "Context";
	private const string C_LivePrefix = "EditorContext";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - ContextSettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.ContextSettings";



	[TypeConverter(typeof(GlobalEnumConverter))]
	public enum EnGlobalizedStatusBarPosition
	{
		[GlobalizedRadio("EnStatusBarPosition_Top")]
		Top,
		[GlobalizedRadio("EnStatusBarPosition_Bottom")]
		Bottom
	}


	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayContextStatusBarPosition")]
	[GlobalizedDescription("OptionDescriptionContextStatusBarPosition")]
	[DefaultValue(EnGlobalizedStatusBarPosition.Bottom)]
	public EnGlobalizedStatusBarPosition StatusBarPosition { get; set; } = EnGlobalizedStatusBarPosition.Bottom;

	[GlobalizedCategory("OptionCategoryGeneral")]
	[GlobalizedDisplayName("OptionDisplayContextBatchSeparator")]
	[GlobalizedDescription("OptionDescriptionContextBatchSeparator")]
	[DefaultValue(SysConstants.C_DefaultBatchSeparator)]
	public string BatchSeparator { get; set; } = SysConstants.C_DefaultBatchSeparator;


	#endregion Property Accessors

}
