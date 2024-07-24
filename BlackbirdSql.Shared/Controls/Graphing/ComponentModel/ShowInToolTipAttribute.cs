// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ShowInToolTipAttribute

using System;



namespace BlackbirdSql.Shared.Controls.Graphing.ComponentModel;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]


public sealed class ShowInToolTipAttribute : Attribute
{
	private bool value = true;

	private bool longString;

	public bool Value => value;

	public bool LongString
	{
		get
		{
			return longString;
		}
		set
		{
			longString = value;
		}
	}

	public ShowInToolTipAttribute()
	{
		value = true;
	}

	public ShowInToolTipAttribute(bool value)
	{
		this.value = value;
	}
}
