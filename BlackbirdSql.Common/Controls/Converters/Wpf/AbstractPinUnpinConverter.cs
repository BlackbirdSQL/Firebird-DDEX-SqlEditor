// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.PinUnpinConverterBase<T>
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;

namespace BlackbirdSql.Common.Controls.Converters.Wpf;


public abstract class AbstractPinUnpinConverter<T> : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return DoPinConversion(value, targetType);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}

	protected object DoPinConversion(object value, Type targetType)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(T)), TraceEventType.Error, EnUiTraceId.HistoryPage, "TargetType must be typed bool");
		Type type = value?.GetType();
		UiTracer.TraceSource.AssertTraceEvent(type != null && type == typeof(bool) || type == typeof(bool?), TraceEventType.Error, EnUiTraceId.HistoryPage, "Value must be non-null and typed bool or bool?");
		if (type != null)
		{
			bool isFavorite = (bool)value;
			return ConvertValue(isFavorite);
		}
		return null;
	}

	protected abstract object ConvertValue(bool isFavorite);
}
