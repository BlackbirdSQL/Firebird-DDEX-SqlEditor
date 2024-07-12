
using BlackbirdSql.VisualStudio.Ddex.Properties;
using BlackbirdSql.Sys.Ctl.ComponentModel;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;


public sealed class GlobalizedDescriptionAttribute(string resourceName)
	: AbstractGlobalizedDescriptionAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
