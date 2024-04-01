using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.LanguageExtension.Properties;

namespace BlackbirdSql.LanguageExtension.Ctl.ComponentModel;

public sealed class GlobalizedDisplayNameAttribute(string resourceName)
	: AbstractGlobalizedDisplayNameAttribute(resourceName)
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
