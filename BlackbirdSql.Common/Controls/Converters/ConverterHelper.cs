// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.ConverterHelper

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;


namespace BlackbirdSql.Common.Controls.Converters;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ConverterHelper
{
	private static readonly char[] s_commaSeparator = new char[1] { ',' };

	public static ImageSource ImageToImageSource(Image image)
	{
		if (image == null)
		{
			return null;
		}
		if (image is not Bitmap bitmap)
		{
			bitmap = new Bitmap(image);
		}
		IntPtr hbitmap = bitmap.GetHbitmap();
		try
		{
			BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
			if (bitmapSource.CanFreeze)
			{
				bitmapSource.Freeze();
			}
			return bitmapSource;
		}
		finally
		{
			Native.DeleteObject(hbitmap);
		}
	}

	public static Visibility TrueToVisible(object value, Type targetType, object parameter)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(Visibility)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed Visibility");
		bool show = false;
		Type type = value?.GetType();
		UiTracer.TraceSource.AssertTraceEvent(type != null && type == typeof(bool) || type == typeof(bool?), TraceEventType.Error, EnUiTraceId.UiInfra, "value must be non-null and typed bool or bool?");
		if (type != null)
		{
			if (type == typeof(bool))
			{
				show = (bool)value;
			}
			else if (type == typeof(bool?))
			{
				bool? flag = (bool?)value;
				show = flag.HasValue && flag.Value;
			}
		}
		return GetConditionalVisibility(show, parameter);
	}

	public static Visibility IntToVisible(object value, Type targetType, object parameter)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(Visibility)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed Visibility");
		bool show = false;
		Type type = value?.GetType();
		UiTracer.TraceSource.AssertTraceEvent(type != null && type == typeof(int) || type == typeof(int?), TraceEventType.Error, EnUiTraceId.UiInfra, "value must be non-null and typed int or int?");
		if (type != null)
		{
			if (type == typeof(int))
			{
				show = (int)value != 0;
			}
			else if (type == typeof(int?))
			{
				int? num = (int?)value;
				show = num.HasValue && num.Value != 0;
			}
		}
		return GetConditionalVisibility(show, parameter);
	}

	public static Visibility InvertVisibility(object value, Type targetType, object parameter)
	{
		Type type = value?.GetType();
		UiTracer.TraceSource.AssertTraceEvent(type == typeof(Visibility), TraceEventType.Error, EnUiTraceId.UiInfra, "Value must be typed Visibility");
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(Visibility)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed Visibility");
		Visibility result = Visibility.Visible;
		if (type == typeof(Visibility))
		{
			result = (Visibility)value == Visibility.Visible ? GetCollapsedOrHidden(parameter) : Visibility.Visible;
		}
		return result;
	}

	public static bool InvertBool(object value, Type targetType, object parameter)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(bool)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed bool");
		if (value is bool v)
			return !v;

		string text = parameter as string;
		if (!string.IsNullOrEmpty(text) && text.Equals("DefaultFalse", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}

	public static bool IntToBool(object value, Type targetType, object parameter)
	{
		UiTracer.TraceSource.AssertTraceEvent(targetType.IsAssignableFrom(typeof(bool)), TraceEventType.Error, EnUiTraceId.UiInfra, "TargetType must be typed bool");
		bool flag = false;
		Type type = value?.GetType();
		UiTracer.TraceSource.AssertTraceEvent(type != null && type == typeof(int) || type == typeof(int?), TraceEventType.Error, EnUiTraceId.UiInfra, "value must be non-null and typed int or int?");
		if (type != null)
		{
			if (type == typeof(int))
			{
				flag = (int)value != 0;
			}
			else if (type == typeof(int?))
			{
				int? num = (int?)value;
				flag = num.HasValue && num.Value != 0;
			}
		}
		if (!GetInvert(parameter))
		{
			return flag;
		}
		return !flag;
	}

	public static Visibility GetCollapsedOrHidden(object parameter)
	{
		if (!ParameterContains(parameter, "Hidden"))
		{
			return Visibility.Collapsed;
		}
		return Visibility.Hidden;
	}

	public static Visibility GetConditionalVisibility(bool show, object parameter)
	{
		return GetConditionalVisibility(show, parameter, ignoreInvert: false);
	}

	public static Visibility GetConditionalVisibility(bool show, object parameter, bool ignoreInvert)
	{
		bool flag = show;
		if (!ignoreInvert && GetInvert(parameter))
		{
			flag = !show;
		}
		if (!flag)
		{
			return GetCollapsedOrHidden(parameter);
		}
		return Visibility.Visible;
	}

	public static Visibility GetConditionalVisibility(bool show, object parameter, bool ignoreInvert, int argIndex)
	{
		bool flag = show;
		if (!ignoreInvert && GetInvert(parameter, argIndex))
		{
			flag = !show;
		}
		if (!flag)
		{
			return GetCollapsedOrHiddenFromArray(parameter, argIndex);
		}
		return Visibility.Visible;
	}

	public static bool GetDefaultFalse(object parameter)
	{
		return !ParameterContains(parameter, "DefaultFalse");
	}

	public static Visibility GetCollapsedOrHiddenFromArray(object parameter, int argIndex)
	{
		if (parameter is object[] array && array.Length >= argIndex + 1)
		{
			return GetCollapsedOrHidden(array[argIndex] as string);
		}
		return Visibility.Collapsed;
	}

	public static string GetNullDateTimeDefaultString(object parameter)
	{
		return GetStringFromArrayInParameter(parameter, 1, defaultNull: false);
	}

	public static string GetDateTimeFormatString(object parameter)
	{
		if (parameter is string result)
		{
			return result;
		}
		return GetStringFromArrayInParameter(parameter, 0, defaultNull: false);
	}

	public static bool GetInvert(object parameter)
	{
		string text = parameter as string;
		if (!string.IsNullOrEmpty(text))
		{
			string[] array = text.Split(s_commaSeparator, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text2 in array)
			{
				if (text2.Equals("Not", StringComparison.OrdinalIgnoreCase) || text2.Equals("!", StringComparison.OrdinalIgnoreCase) || text2.Equals("Invert", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool GetInvert(object parameter, int argIndex)
	{
		return GetInvert(GetStringFromArrayInParameter(parameter, argIndex, defaultNull: false));
	}

	public static string GetSecondCompareString(object parameter)
	{
		string text = parameter as string;
		if (parameter == null || text != null)
		{
			return text;
		}
		return GetStringFromArrayInParameter(parameter, 0, defaultNull: true);
	}

	private static bool ParameterContains(object parameter, string option)
	{
		string text = parameter as string;
		if (!string.IsNullOrEmpty(text))
		{
			string[] array = text.Split(s_commaSeparator, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Equals(option, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static string GetStringFromArrayInParameter(object parameter, int index, bool defaultNull)
	{
		if (parameter != null && parameter.GetType() == typeof(object[]))
		{
			object[] array = (object[])parameter;
			if (array.Length >= index + 1 && array[index] != null)
			{
				return array[index] as string;
			}
		}
		if (!defaultNull)
		{
			return string.Empty;
		}
		return null;
	}
}
