// System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System.Windows.Forms.OpacityConverter
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlackbirdSql.Core.Ctl.ComponentModel;


namespace BlackbirdSql.EditorExtension.Ctl.ComponentModel;

/// <summary>
/// Controls input range.
/// </summary>
public class RangeConverter : TypeConverter
{
	private bool _Registered = false;
	private int _Min = int.MinValue, _Max = int.MaxValue;


	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		// Tracer.Trace("CanConvertFrom: " + context.PropertyDescriptor.Name);
		RegisterModel(context);

		if (sourceType == typeof(string))
			return true;

		return base.CanConvertFrom(context, sourceType);
	}



	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		// Tracer.Trace("ConvertFrom: " + context.PropertyDescriptor.Name);
		RegisterModel(context);

		if (value is string text)
		{
			int num = int.Parse(text, CultureInfo.CurrentCulture);

			if (num < _Min)
				num = _Min;
			if (num > _Max)
				num = _Max;

			return num;
		}

		return base.ConvertFrom(context, culture, value);
	}



	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
			throw new ArgumentNullException("destinationType");

		// Tracer.Trace("ConvertTo: " + context.PropertyDescriptor.Name);

		RegisterModel(context);

		if (destinationType == typeof(string))
		{
			int num = (int)value;
			return num.ToString(CultureInfo.CurrentCulture);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public RangeConverter()
	{
	}


	private void RegisterModel(ITypeDescriptorContext context)
	{
		if (_Registered)
			return;

		if (context.PropertyDescriptor.Attributes[typeof(LiteralRangeAttribute)] is LiteralRangeAttribute minmaxAttr)
		{
			_Min = minmaxAttr.Min;
			_Max = minmaxAttr.Max;
		}
		else if (context.PropertyDescriptor.Attributes[typeof(RangeAttribute)] is RangeAttribute rangeAttr)
		{
			_Min = (int)rangeAttr.Minimum;
			_Max = (int)rangeAttr.Maximum;
		}
		_Registered = true;
	}

}
