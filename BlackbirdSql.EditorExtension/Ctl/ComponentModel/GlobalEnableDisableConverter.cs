// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities.GlobalBoolConverter

using BlackbirdSql.Core.Ctl.ComponentModel;



namespace BlackbirdSql.EditorExtension.Ctl.ComponentModel;


/// <summary>
/// Localized dll globalized Enable/Diable bool type convertor.
/// Resources are in AbstractBoolConverter AttributeResources.resx.
/// </summary>
public class GlobalEnableDisableConverter :  AbstractBoolConverter
{

	public override string LiteralFalseResource => "GlobalizedLiteralDisabled";
	public override string LiteralTrueResource => "GlobalizedLiteralEnabled";

}
