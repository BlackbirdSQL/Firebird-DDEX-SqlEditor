// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.AllTrueToVisibleConverter
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;




namespace BlackbirdSql.Common.Ctl.Converters;


[ValueConversion(typeof(bool[]), typeof(Visibility))]
public class AllTrueToVisibleConverter : IMultiValueConverter
{
	public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(Visibility)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed Visibility");
		return ConverterHelper.GetConditionalVisibility((bool)new IsAllTrueConverter().Convert(values, typeof(bool), parameter, culture), parameter, ignoreInvert: true);
	}

	public virtual object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, "Not implemented");
		return new object[0];
	}
}
