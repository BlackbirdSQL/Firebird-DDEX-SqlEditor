
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys.Ctl.ComponentModel;



namespace BlackbirdSql.Core.Ctl.ComponentModel;


public sealed class GlobalizedVerbTextAttribute : AbstractGlobalizedDisplayNameAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;


	public GlobalizedVerbTextAttribute(string resourceName) : base(resourceName)
	{
	}
}
