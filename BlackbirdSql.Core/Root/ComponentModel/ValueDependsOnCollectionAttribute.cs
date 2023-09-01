// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.ValueDependsOnCollectionAttribute

using System;

namespace BlackbirdSql.Core.ComponentModel;
// namespace Microsoft.SqlServer.ConnectionDlg.UI.MVVM


[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
internal sealed class ValueDependsOnCollectionAttribute : Attribute
{
	private readonly string m_sourceName;

	public string SourceName => m_sourceName;

	public override object TypeId => this;

	public ValueDependsOnCollectionAttribute(string sourceName)
	{
		m_sourceName = sourceName;
	}
}
