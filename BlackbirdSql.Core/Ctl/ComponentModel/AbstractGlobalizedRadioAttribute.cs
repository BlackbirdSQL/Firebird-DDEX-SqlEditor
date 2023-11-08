#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.ComponentModel;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

public abstract class AbstractGlobalizedRadioAttribute : DescriptionAttribute
{
	private bool _Selected;
	private readonly string _SelectedResourceName;
	private readonly string _UnselectedResourceName;

	public abstract System.Resources.ResourceManager ResMgr { get; }

	public override string Description
	{
		get
		{
			try
			{
				return ResMgr.GetString(_Selected ? _SelectedResourceName : _UnselectedResourceName);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
	public string SelectedDescription => ResMgr.GetString(_SelectedResourceName);
	public string UnselectedDescription => ResMgr.GetString(_UnselectedResourceName);

	public bool Selected
	{
		get { return _Selected; }
		set { _Selected = value; }
	}

	public AbstractGlobalizedRadioAttribute(string resource)
	{
		_SelectedResourceName = resource + "_Selected";
		_UnselectedResourceName = resource + "_Unselected";
	}


	public AbstractGlobalizedRadioAttribute(string selectedResource, string unselectedResource)
	{
		_SelectedResourceName = selectedResource;
		_UnselectedResourceName = unselectedResource;
	}
}
