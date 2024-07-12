
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Ctl.ComponentModel;



namespace BlackbirdSql.Shared.Ctl.ComponentModel;


public sealed class GlobalizedDescriptionAttribute : AbstractGlobalizedDescriptionAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;

	public GlobalizedDescriptionAttribute(string resourceName) : base(resourceName)
	{
	}
}
