// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions2.GlobalEnumConverter
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;



namespace BlackbirdSql.Common.Controls.PropertiesWindow;

public class GlobalEnumConverter : EnumConverter
{
	private readonly Dictionary<CultureInfo, Dictionary<string, object>> _lookupTables;

	public GlobalEnumConverter(Type type)
		: base(type)
	{
		_lookupTables = new Dictionary<CultureInfo, Dictionary<string, object>>();
	}

	private Dictionary<string, object> GetLookupTable(CultureInfo culture)
	{
		culture ??= CultureInfo.CurrentCulture;
		if (!_lookupTables.TryGetValue(culture, out Dictionary<string, object> value))
		{
			value = new Dictionary<string, object>();
			foreach (object standardValue in GetStandardValues())
			{
				string text = ConvertToString(null, culture, standardValue);
				if (text != null)
				{
					value.Add(text, standardValue);
				}
			}
			_lookupTables.Add(culture, value);
		}
		return value;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			Dictionary<string, object> lookupTable = GetLookupTable(culture);
			if (!lookupTable.TryGetValue(value as string, out object value2))
			{
				value2 = base.ConvertFrom(context, culture, value);
			}
			return value2;
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value is Enum @enum && destinationType == typeof(string))
		{
			GlobalizedDescriptionAttribute[] array = (GlobalizedDescriptionAttribute[])@enum.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(GlobalizedDescriptionAttribute), inherit: false);
			if (array.Length != 0)
			{
				return array[0].Description;
			}
			return value.ToString();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
