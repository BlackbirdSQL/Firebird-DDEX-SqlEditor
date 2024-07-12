
using BlackbirdSql.EditorExtension.Properties;
using BlackbirdSql.Sys.Ctl.ComponentModel;



namespace BlackbirdSql.EditorExtension.Ctl.ComponentModel;


public sealed class GlobalizedCategoryAttribute : AbstractGlobalizedCategoryAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;

	public GlobalizedCategoryAttribute(string resourceName) : base(resourceName)
	{
	}

}
