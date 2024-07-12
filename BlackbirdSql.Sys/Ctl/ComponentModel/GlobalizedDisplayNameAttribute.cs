
using BlackbirdSql.Sys.Properties;



namespace BlackbirdSql.Sys.Ctl.ComponentModel;


public sealed class GlobalizedDisplayNameAttribute(string resourceName)
	: AbstractGlobalizedDisplayNameAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
