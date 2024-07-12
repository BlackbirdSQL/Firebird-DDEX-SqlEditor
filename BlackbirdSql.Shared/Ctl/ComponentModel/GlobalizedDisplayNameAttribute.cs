
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Ctl.ComponentModel;



namespace BlackbirdSql.Shared.Ctl.ComponentModel;


public sealed class GlobalizedDisplayNameAttribute : AbstractGlobalizedDisplayNameAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;


	public GlobalizedDisplayNameAttribute(string resourceName) : base(resourceName)
	{
	}
}
