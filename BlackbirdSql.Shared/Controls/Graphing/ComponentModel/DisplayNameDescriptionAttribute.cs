// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.DisplayNameDescriptionAttribute

using System;



namespace BlackbirdSql.Shared.Controls.Graphing.ComponentModel;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]


internal class DisplayNameDescriptionAttribute : Attribute
{
	private readonly string _DisplayName;

	private readonly string _Description;

	internal string DisplayName => _DisplayName;

	internal string Description => _Description;

	public DisplayNameDescriptionAttribute(string _DisplayName)
	{
		this._DisplayName = _DisplayName;
	}

	public DisplayNameDescriptionAttribute(string _DisplayName, string _Description)
	{
		this._DisplayName = _DisplayName;
		this._Description = _Description;
	}
}
