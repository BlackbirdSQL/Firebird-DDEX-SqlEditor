// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.BoolInvertConverter
using System;
using System.Globalization;
using System.Windows.Data;


namespace BlackbirdSql.Common.Ctl.Converters;

[ValueConversion(typeof(bool), typeof(bool))]
public class BoolInvertConverter : IValueConverter
{
	public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ConverterHelper.InvertBool(value, targetType, parameter);
	}

	public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ConverterHelper.InvertBool(value, targetType, parameter);
	}
}
