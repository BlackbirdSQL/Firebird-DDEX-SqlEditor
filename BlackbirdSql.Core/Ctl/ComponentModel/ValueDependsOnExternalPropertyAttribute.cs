// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.ValueDependsOnExternalPropertyAttribute

using System;

namespace BlackbirdSql.Core.Ctl.ComponentModel;
// namespace Microsoft.SqlServer.ConnectionDlg.UI.MVVM


[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValueDependsOnExternalPropertyAttribute : Attribute
{
	private readonly string m_sourceName;

	private readonly string m_propertyName;

	public string SourceName => m_sourceName;

	public string PropertyName => m_propertyName;

	public override object TypeId => this;

	public ValueDependsOnExternalPropertyAttribute(string sourceName, string propertyName = null)
	{
		m_sourceName = sourceName;
		m_propertyName = propertyName ?? "";
	}
}
