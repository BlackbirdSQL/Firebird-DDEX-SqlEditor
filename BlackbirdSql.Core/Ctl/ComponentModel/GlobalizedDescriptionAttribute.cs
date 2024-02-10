using BlackbirdSql.Core.Properties;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

public sealed class GlobalizedDescriptionAttribute(string resourceName)
	: AbstractGlobalizedDescriptionAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
