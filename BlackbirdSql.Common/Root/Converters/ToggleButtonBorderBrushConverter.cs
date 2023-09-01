// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.ToggleButtonBorderBrushConverter
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;




namespace BlackbirdSql.Common.Converters;


public class ToggleButtonBorderBrushConverter : IMultiValueConverter
{
	public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(SolidColorBrush)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed SolidColorBrush");
		if (values.Length < 2 || values[0] is not bool || values[1] is not bool)
		{
			return null;
		}
		bool isExpanded = (bool)values[0];
		bool isMouseOverToggleButton = (bool)values[1];
		bool isListItemSelected;
		bool isMouseOverListItem;
		bool isListFocused;
		if (values.Length == 5)
		{
			isListItemSelected = (values[2] as bool?).GetValueOrDefault();
			isMouseOverListItem = (values[3] as bool?).GetValueOrDefault();
			isListFocused = (values[4] as bool?).GetValueOrDefault();
		}
		else
		{
			isListItemSelected = false;
			isMouseOverListItem = false;
			isListFocused = false;
		}
		return ToggleButtonConverterHelper.GetButtonColorBrush(isExpanded, isMouseOverToggleButton, isListItemSelected, isMouseOverListItem, isListFocused);
	}

	public virtual object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, "Not implemented");
		return new object[0];
	}
}
