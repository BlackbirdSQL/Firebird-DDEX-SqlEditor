// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.EditorExtension.Ctl.ComponentModel;



namespace BlackbirdSql.EditorExtension.Model.Config;


// =========================================================================================================
//										ContextSettingsModel Class
//
/// <summary>
/// Option Model for Pane context options
/// </summary>
// =========================================================================================================
public class ContextSettingsModel : AbstractSettingsModel<ContextSettingsModel>
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - ContextSettingsModel
	// ---------------------------------------------------------------------------------


	public ContextSettingsModel() : this(null)
	{
	}


	public ContextSettingsModel(IBsTransientSettings transientSettings)
		: base(C_Package, C_Group, C_PropertyPrefix, transientSettings)
	{
	}


	#endregion Constructors / Destructors




	// =====================================================================================================
	#region Constants - ContextSettingsModel
	// =====================================================================================================


	private const string C_Package = "Editor";
	private const string C_Group = "Context";
	private const string C_PropertyPrefix = "EditorContext";


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


	#endregion Property Accessors

}
