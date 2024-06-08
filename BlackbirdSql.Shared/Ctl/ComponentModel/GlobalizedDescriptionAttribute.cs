// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities.GlobalizedDescriptionAttribute

using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Core.Ctl.ComponentModel;

namespace BlackbirdSql.Shared.Ctl.ComponentModel;

public sealed class GlobalizedDescriptionAttribute : AbstractGlobalizedDescriptionAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;

	public GlobalizedDescriptionAttribute(string resourceName) : base(resourceName)
	{
	}
}
