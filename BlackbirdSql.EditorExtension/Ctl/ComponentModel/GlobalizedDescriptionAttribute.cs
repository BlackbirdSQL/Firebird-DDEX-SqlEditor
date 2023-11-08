using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.EditorExtension.Properties;


namespace BlackbirdSql.EditorExtension.Ctl.ComponentModel;

public sealed class GlobalizedDescriptionAttribute : AbstractGlobalizedDescriptionAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;

	public GlobalizedDescriptionAttribute(string resourceName) :base(resourceName)
	{
	}
}
