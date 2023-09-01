// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.IndentWidthToMarginConverter
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BlackbirdSql.Core;

namespace BlackbirdSql.Common.Converters;

public class IndentWidthToMarginConverter : IValueConverter
{
	private static readonly IndentWidthToMarginConverter _default = new IndentWidthToMarginConverter();

	public static IndentWidthToMarginConverter Default => _default;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return new Thickness(System.Convert.ToDouble(value, culture), 0.0, 0.0, 0.0);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		NotSupportedException ex = new();
		Diag.Dug(ex);
		throw ex;
	}
}
