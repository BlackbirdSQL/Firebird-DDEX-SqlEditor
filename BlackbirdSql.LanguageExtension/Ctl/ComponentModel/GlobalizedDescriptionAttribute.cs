
using BlackbirdSql.LanguageExtension.Properties;
using BlackbirdSql.Sys.Ctl.ComponentModel;



namespace BlackbirdSql.LanguageExtension.Ctl.ComponentModel;


public sealed class GlobalizedDescriptionAttribute(string resourceName)
	: AbstractGlobalizedDescriptionAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
