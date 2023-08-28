// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities.GlobalBoolConverter
using System;
using System.ComponentModel;
using System.Globalization;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Controls.PropertiesWindow;

public sealed class GlobalBoolConverter : BooleanConverter
{
	public GlobalBoolConverter()
	{
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		string text = value as string;
		if (!string.IsNullOrEmpty(text))
		{
			if (ControlsResources.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: true) != null)
			{
				if (string.Equals(text, GetGlobalizedLiteralFalse(culture), StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				if (string.Equals(text, GetGlobalizedLiteralTrue(culture), StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			throw new FormatException();
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value != null && value is bool bvalue && destinationType == typeof(string) && ControlsResources.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: true) != null)
		{
			if (bvalue)
			{
				return GetGlobalizedLiteralTrue(culture);
			}
			return GetGlobalizedLiteralFalse(culture);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private static string GetGlobalizedLiteralTrue(CultureInfo culture)
	{
		return ControlsResources.ResourceManager.GetString("GlobalizedLiteralTrue", culture);
	}

	private static string GetGlobalizedLiteralFalse(CultureInfo culture)
	{
		return ControlsResources.ResourceManager.GetString("GlobalizedLiteralFalse", culture);
	}
}
