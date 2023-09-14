// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.ArithmeticConverter

using System;
using System.Globalization;
using System.Windows.Data;
using BlackbirdSql.Common.Ctl.Enums;




namespace BlackbirdSql.Common.Controls.Converters;


public class ArithmeticConverter : IValueConverter
{
	public EnArithmeticOperator Operator { get; set; }

	public double Operand { get; set; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double num = 0.0;
		if (value != null)
		{
			num = System.Convert.ToDouble(value, culture);
		}
		if (Operator == EnArithmeticOperator.Addition)
		{
			return num + Operand;
		}
		if (Operator == EnArithmeticOperator.Subtraction)
		{
			return num - Operand;
		}
		if (Operator == EnArithmeticOperator.Multiplication)
		{
			return num * Operand;
		}
		if (Operator == EnArithmeticOperator.Division)
		{
			return num / Operand;
		}
		return num;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double num = 0.0;
		if (value != null)
		{
			num = System.Convert.ToDouble(value, culture);
		}
		if (Operator == EnArithmeticOperator.Addition)
		{
			return num - Operand;
		}
		if (Operator == EnArithmeticOperator.Subtraction)
		{
			return num + Operand;
		}
		if (Operator == EnArithmeticOperator.Multiplication)
		{
			return num / Operand;
		}
		if (Operator == EnArithmeticOperator.Division)
		{
			return num * Operand;
		}
		return num;
	}
}
