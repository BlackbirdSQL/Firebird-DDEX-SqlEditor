using BlackbirdSql.Core.Properties;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

public sealed class GlobalizedDisplayNameAttribute(string resourceName)
	: AbstractGlobalizedDisplayNameAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
