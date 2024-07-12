// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities.GlobalBoolConverter

using BlackbirdSql.Core.Ctl.ComponentModel;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;


/// <summary>
/// Localized dll globalized On/Off bool type convertor.
/// Resources are in AbstractBoolConverter AttributeResources.resx.
/// </summary>
public class GlobalShowHideConverter :  AbstractBoolConverter
{

	// public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;


	public override string LiteralFalseResource => "GlobalizedLiteralHide";
	public override string LiteralTrueResource => "GlobalizedLiteralShow";

}
