// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.DependsOnPropertyAttribute

using System;




namespace BlackbirdSql.Core;



[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
internal abstract class DependsOnPropertyAttribute : Attribute
{
	private readonly string[] m_propertyNames;

	public string[] PropertyNames => m_propertyNames;

	public DependsOnPropertyAttribute(string propertyName)
	{
		m_propertyNames = new string[1] { propertyName };
	}

	public DependsOnPropertyAttribute(string propertyName1, string propertyName2)
	{
		Cmd.CheckStringForNullOrEmpty(propertyName1, "propertyName1");
		Cmd.CheckStringForNullOrEmpty(propertyName2, "propertyName2");
		m_propertyNames = new string[2] { propertyName1, propertyName2 };
	}

	public DependsOnPropertyAttribute(string propertyName1, string propertyName2, string propertyName3)
	{
		Cmd.CheckStringForNullOrEmpty(propertyName1, "propertyName1");
		Cmd.CheckStringForNullOrEmpty(propertyName2, "propertyName2");
		Cmd.CheckStringForNullOrEmpty(propertyName3, "propertyName3");
		m_propertyNames = new string[3] { propertyName1, propertyName2, propertyName3 };
	}

	public DependsOnPropertyAttribute(string propertyName1, string propertyName2, string propertyName3, string propertyName4)
	{
		Cmd.CheckStringForNullOrEmpty(propertyName1, "propertyName1");
		Cmd.CheckStringForNullOrEmpty(propertyName2, "propertyName2");
		Cmd.CheckStringForNullOrEmpty(propertyName3, "propertyName3");
		Cmd.CheckStringForNullOrEmpty(propertyName4, "propertyName4");
		m_propertyNames = new string[4] { propertyName1, propertyName2, propertyName3, propertyName4 };
	}

	public DependsOnPropertyAttribute(string propertyName1, string propertyName2, string propertyName3, string propertyName4, string propertyName5)
	{
		Cmd.CheckStringForNullOrEmpty(propertyName1, "propertyName1");
		Cmd.CheckStringForNullOrEmpty(propertyName2, "propertyName2");
		Cmd.CheckStringForNullOrEmpty(propertyName3, "propertyName3");
		Cmd.CheckStringForNullOrEmpty(propertyName4, "propertyName4");
		Cmd.CheckStringForNullOrEmpty(propertyName5, "propertyName5");
		m_propertyNames = new string[5] { propertyName1, propertyName2, propertyName3, propertyName4, propertyName5 };
	}
}
