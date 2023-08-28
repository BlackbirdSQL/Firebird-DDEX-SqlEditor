// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.ImageToImageSourceConverter
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;



namespace BlackbirdSql.Common.Ctl.Converters;


[ValueConversion(typeof(Image), typeof(ImageSource))]
public class ImageToImageSourceConverter : IValueConverter
{
	public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(ImageSource)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed System.Windows.Media.ImageSource");
		return ConverterHelper.ImageToImageSource(value as Image);
	}

	public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, "Not implemented");
		return Binding.DoNothing;
	}
}
