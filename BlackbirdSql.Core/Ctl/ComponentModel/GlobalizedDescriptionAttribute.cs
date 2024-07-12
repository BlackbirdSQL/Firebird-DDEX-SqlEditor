
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Ctl.ComponentModel;



namespace BlackbirdSql.Core.Ctl.ComponentModel;


public sealed class GlobalizedDescriptionAttribute(string resourceName)
	: AbstractGlobalizedDescriptionAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
