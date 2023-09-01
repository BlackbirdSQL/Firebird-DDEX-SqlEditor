// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.NonEmptyStringToVisibleConverter
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;




namespace BlackbirdSql.Common.Converters;


[ValueConversion(typeof(string), typeof(Visibility))]
public class NonEmptyStringToVisibleConverter : IValueConverter
{
	public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(Visibility)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed Visibility");
		return ConverterHelper.GetConditionalVisibility(!string.IsNullOrEmpty(value as string), parameter);
	}

	public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, "Not implemented");
		return Binding.DoNothing;
	}
}
