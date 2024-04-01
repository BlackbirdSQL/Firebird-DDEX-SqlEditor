using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.LanguageExtension.Properties;


namespace BlackbirdSql.LanguageExtension.Ctl.ComponentModel;

public sealed class GlobalizedDescriptionAttribute(string resourceName)
	: AbstractGlobalizedDescriptionAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
