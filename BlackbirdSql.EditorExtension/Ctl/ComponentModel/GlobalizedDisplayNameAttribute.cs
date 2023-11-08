using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.EditorExtension.Properties;


namespace BlackbirdSql.EditorExtension.Ctl.ComponentModel;

public sealed class GlobalizedDisplayNameAttribute : AbstractGlobalizedDisplayNameAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;


	public GlobalizedDisplayNameAttribute(string resourceName) : base(resourceName)
	{
	}
}
