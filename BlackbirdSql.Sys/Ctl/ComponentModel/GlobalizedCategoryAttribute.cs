
using BlackbirdSql.Sys.Properties;



namespace BlackbirdSql.Sys.Ctl.ComponentModel;


public sealed class GlobalizedCategoryAttribute(string resourceName)
	: AbstractGlobalizedCategoryAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
