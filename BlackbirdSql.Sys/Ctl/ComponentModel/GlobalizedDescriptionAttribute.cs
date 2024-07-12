
using BlackbirdSql.Sys.Properties;



namespace BlackbirdSql.Sys.Ctl.ComponentModel;


public sealed class GlobalizedDescriptionAttribute(string resourceName)
	: AbstractGlobalizedDescriptionAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
