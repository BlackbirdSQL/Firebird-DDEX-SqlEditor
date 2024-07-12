
using System;
using System.ComponentModel;



namespace BlackbirdSql.Sys.Ctl.ComponentModel;


public abstract class AbstractGlobalizedCategoryAttribute : CategoryAttribute
{
	private readonly string _ResourceName;


	public abstract System.Resources.ResourceManager ResMgr { get; }

	public AbstractGlobalizedCategoryAttribute(string resourceName)
	{
		_ResourceName = resourceName;
	}

	protected override string GetLocalizedString(string value)
	{
		try
		{
			return ResMgr.GetString(_ResourceName);
		}
		catch (Exception)
		{
			return "ControlsResources: " + value;
		}
	}
}
