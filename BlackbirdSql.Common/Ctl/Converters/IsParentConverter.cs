// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.IsParentConverter

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;


namespace BlackbirdSql.Common.Ctl.Converters;


public class IsParentConverter : IValueConverter
{
	public Type TypeOfParent { get; set; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is UIElement element)
		{
			return Cmd.FindVisualParent(TypeOfParent, element) != null;
		}
		return false;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, "Not implemented");
		return Binding.DoNothing;
	}
}
