// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.FloatTypeConverter
using System;
using System.ComponentModel;
using System.Globalization;


namespace BlackbirdSql.Shared.Controls.Graphing;

public sealed class FloatTypeConverter : TypeConverter
{
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			TypeConverter converter = TypeDescriptor.GetConverter(value);
			if (converter.CanConvertTo(typeof(double)))
			{
				return ((double)converter.ConvertTo(value, typeof(double))).ToString("0.#######", CultureInfo.CurrentCulture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
