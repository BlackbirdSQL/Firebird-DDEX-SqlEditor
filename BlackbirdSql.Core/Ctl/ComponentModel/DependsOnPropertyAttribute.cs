// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.DependsOnPropertyAttribute

using System;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
internal abstract class DependsOnPropertyAttribute : Attribute
{
	private readonly string[] _PropertyNames;

	public string[] PropertyNames => _PropertyNames;

	public DependsOnPropertyAttribute(string propertyName)
	{
		_PropertyNames = [propertyName];
	}

	public DependsOnPropertyAttribute(string propertyName1, string propertyName2)
	{
		Cmd.CheckStringForNullOrEmpty(propertyName1, "propertyName1");
		Cmd.CheckStringForNullOrEmpty(propertyName2, "propertyName2");
		_PropertyNames = [propertyName1, propertyName2];
	}

	public DependsOnPropertyAttribute(string propertyName1, string propertyName2, string propertyName3)
	{
		Cmd.CheckStringForNullOrEmpty(propertyName1, "propertyName1");
		Cmd.CheckStringForNullOrEmpty(propertyName2, "propertyName2");
		Cmd.CheckStringForNullOrEmpty(propertyName3, "propertyName3");
		_PropertyNames = [propertyName1, propertyName2, propertyName3];
	}

	public DependsOnPropertyAttribute(string propertyName1, string propertyName2, string propertyName3, string propertyName4)
	{
		Cmd.CheckStringForNullOrEmpty(propertyName1, "propertyName1");
		Cmd.CheckStringForNullOrEmpty(propertyName2, "propertyName2");
		Cmd.CheckStringForNullOrEmpty(propertyName3, "propertyName3");
		Cmd.CheckStringForNullOrEmpty(propertyName4, "propertyName4");
		_PropertyNames = [propertyName1, propertyName2, propertyName3, propertyName4];
	}

	public DependsOnPropertyAttribute(string propertyName1, string propertyName2, string propertyName3, string propertyName4, string propertyName5)
	{
		Cmd.CheckStringForNullOrEmpty(propertyName1, "propertyName1");
		Cmd.CheckStringForNullOrEmpty(propertyName2, "propertyName2");
		Cmd.CheckStringForNullOrEmpty(propertyName3, "propertyName3");
		Cmd.CheckStringForNullOrEmpty(propertyName4, "propertyName4");
		Cmd.CheckStringForNullOrEmpty(propertyName5, "propertyName5");
		_PropertyNames = [propertyName1, propertyName2, propertyName3, propertyName4, propertyName5];
	}
}
