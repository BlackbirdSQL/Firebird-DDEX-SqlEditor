// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities.GlobalBoolConverter

using System.Security.Permissions;
using BlackbirdSql.Core.Ctl.ComponentModel;


namespace BlackbirdSql.EditorExtension.Ctl.ComponentModel;

/// <summary>
/// Localized dll globalized Yes/No bool type convertor.
/// Resources are in AbstractBoolConverter AttributeResources.resx.
/// </summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class GlobalYesNoConverter :  AbstractBoolConverter
{

	public override string LiteralFalseResource => "GlobalizedLiteralNo";
	public override string LiteralTrueResource => "GlobalizedLiteralYes";

}
