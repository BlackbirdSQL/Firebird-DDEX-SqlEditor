// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.ValueDependsOnExternalPropertyAttribute

using System;

namespace BlackbirdSql.Core.Ctl.ComponentModel;
// namespace Microsoft.SqlServer.ConnectionDlg.UI.MVVM


[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValueDependsOnExternalPropertyAttribute : Attribute
{
	private readonly string _SourceName;

	private readonly string _PropertyName;

	public string SourceName => _SourceName;

	public string PropertyName => _PropertyName;

	public override object TypeId => this;

	public ValueDependsOnExternalPropertyAttribute(string sourceName, string propertyName = null)
	{
		_SourceName = sourceName;
		_PropertyName = propertyName ?? "";
	}
}
