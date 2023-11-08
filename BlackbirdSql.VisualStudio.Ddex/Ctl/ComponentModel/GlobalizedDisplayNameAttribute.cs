using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Properties;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

public sealed class GlobalizedDisplayNameAttribute : AbstractGlobalizedDisplayNameAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;


	public GlobalizedDisplayNameAttribute(string resourceName) : base(resourceName)
	{
	}
}
