// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.ValueDependsOnCollectionAttribute

using System;

namespace BlackbirdSql.Core.Ctl.ComponentModel;
// namespace Microsoft.SqlServer.ConnectionDlg.UI.MVVM


[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValueDependsOnCollectionAttribute : Attribute
{
	private readonly string _SourceName;

	public string SourceName => _SourceName;

	public override object TypeId => this;

	public ValueDependsOnCollectionAttribute(string sourceName)
	{
		_SourceName = sourceName;
	}
}
