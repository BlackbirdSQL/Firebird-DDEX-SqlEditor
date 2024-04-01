using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.LanguageExtension.Properties;


namespace BlackbirdSql.LanguageExtension.Ctl.ComponentModel;

public sealed class GlobalizedCategoryAttribute : AbstractGlobalizedCategoryAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;

	public GlobalizedCategoryAttribute(string resourceName) : base(resourceName)
	{
	}

}
