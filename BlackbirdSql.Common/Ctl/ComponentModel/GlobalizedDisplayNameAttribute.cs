// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities.GlobalizedDisplayNameAttribute

using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.Ctl.ComponentModel;

namespace BlackbirdSql.Common.Ctl.ComponentModel;

public sealed class GlobalizedDisplayNameAttribute : AbstractGlobalizedDisplayNameAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;


	public GlobalizedDisplayNameAttribute(string resourceName) : base(resourceName)
	{
	}
}
