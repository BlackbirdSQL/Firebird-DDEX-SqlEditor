// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.IsNullConverter

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;




namespace BlackbirdSql.Common.Controls.Converters;


[ValueConversion(typeof(object), typeof(bool))]
public class IsNullConverter : IValueConverter
{
	public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(bool)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed bool");
		return ConverterHelper.GetInvert(parameter) ? value != null : value == null;
	}

	public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, "Not implemented");
		return Binding.DoNothing;
	}
}