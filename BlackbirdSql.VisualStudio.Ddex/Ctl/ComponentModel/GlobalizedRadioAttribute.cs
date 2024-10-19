using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Properties;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;


public sealed class GlobalizedRadioAttribute : AbstractGlobalizedRadioAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;

	public GlobalizedRadioAttribute(string resource) : base(resource)
	{
	}

	public GlobalizedRadioAttribute(string selectedResource, string unselectedResource)
		: base(selectedResource, unselectedResource)
	{
	}
}
