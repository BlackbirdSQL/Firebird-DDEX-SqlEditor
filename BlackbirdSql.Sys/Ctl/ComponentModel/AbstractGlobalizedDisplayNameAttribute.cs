
using System;
using System.ComponentModel;



namespace BlackbirdSql.Sys.Ctl.ComponentModel;


public abstract class AbstractGlobalizedDisplayNameAttribute(string resourceName) : DisplayNameAttribute
{
	private readonly string _ResourceName = resourceName;

	public abstract System.Resources.ResourceManager ResMgr { get; }

	public override string DisplayName
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
}
