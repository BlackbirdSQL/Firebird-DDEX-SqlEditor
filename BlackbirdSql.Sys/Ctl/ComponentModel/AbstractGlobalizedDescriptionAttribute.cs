
using System;
using System.ComponentModel;



namespace BlackbirdSql.Sys.Ctl.ComponentModel;


public abstract class AbstractGlobalizedDescriptionAttribute : DescriptionAttribute
{
	private readonly string _ResourceName;

	public abstract System.Resources.ResourceManager ResMgr { get; }

	public override string Description
	{
		get
		{
			try
			{
				return ResMgr.GetString(_ResourceName);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}

	public AbstractGlobalizedDescriptionAttribute(string resourceName)
	{
		_ResourceName = resourceName;
	}
}
