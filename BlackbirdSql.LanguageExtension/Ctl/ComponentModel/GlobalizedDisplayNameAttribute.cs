
using BlackbirdSql.LanguageExtension.Properties;
using BlackbirdSql.Sys.Ctl.ComponentModel;



namespace BlackbirdSql.LanguageExtension.Ctl.ComponentModel;


public sealed class GlobalizedDisplayNameAttribute(string resourceName)
	: AbstractGlobalizedDisplayNameAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
