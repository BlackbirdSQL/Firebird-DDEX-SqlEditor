// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;
namespace BlackbirdSql.EditorExtension.Model.Config;

// =========================================================================================================
//										ContextSettingsModel Class
//
/// <summary>
/// Option Model for General options
/// </summary>
// =========================================================================================================
public class ContextSettingsModel : AbstractSettingsModel<ContextSettingsModel>
{

	private const string C_Package = "Editor";
	private const string C_Group = "Context";
	private const string C_LivePrefix = "EditorContext";

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\SqlEditor.ContextSettings";




	// =====================================================================================================
	#region Model Properties - ContextSettingsModel
	// =====================================================================================================



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
	[DefaultValue(ModelConstants.C_DefaultBatchSeparator)]
	public string BatchSeparator { get; set; } = ModelConstants.C_DefaultBatchSeparator;


	#endregion Model Properties




	// =====================================================================================================
	#region Constructors / Destructors - ContextSettingsModel
	// =====================================================================================================


	public ContextSettingsModel() : this(null)
	{
	}

	public ContextSettingsModel(IBLiveSettings liveSettings)
		: base(C_Package, C_Group, C_LivePrefix, liveSettings)
	{
	}


	#endregion Constructors / Destructors

}
