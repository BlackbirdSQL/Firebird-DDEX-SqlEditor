// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.CachedResourceDictionary
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace BlackbirdSql.Common.Ctl.Wpf;


[EditorBrowsable(EditorBrowsableState.Never)]
public class CachedResourceDictionary : ResourceDictionary
{
	private static readonly Dictionary<Uri, ResourceDictionary> _SCachedDictionaries = new Dictionary<Uri, ResourceDictionary>();

	private Uri _Source;

	public new Uri Source
	{
		get
		{
			return _Source;
		}
		set
		{
			_Source = value;
			if (_SCachedDictionaries.TryGetValue(value, out var value2))
			{
				MergedDictionaries.Add(value2);
				return;
			}
			base.Source = value;
			_SCachedDictionaries.Add(value, this);
		}
	}
}
