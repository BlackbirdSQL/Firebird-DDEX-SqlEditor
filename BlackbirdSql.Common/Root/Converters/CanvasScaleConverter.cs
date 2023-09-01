// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.CanvasScaleConverter
using System;
using System.Globalization;
using System.Windows.Data;
using BlackbirdSql.Core;

namespace BlackbirdSql.Common.Converters;


public class CanvasScaleConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double num = 120.0;
		return (double)value / num;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		NotImplementedException ex = new();
		Diag.Dug(ex);
		throw ex;
	}
}
