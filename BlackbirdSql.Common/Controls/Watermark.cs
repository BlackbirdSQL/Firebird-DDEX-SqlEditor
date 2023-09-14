// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Watermark

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;

namespace BlackbirdSql.Common.Controls;


public static class Watermark
{
	public static readonly DependencyProperty WatermarkStateProperty = DependencyProperty.RegisterAttached("WatermarkState", typeof(WatermarkState), typeof(Watermark), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, null, isAnimationProhibited: true, UpdateSourceTrigger.PropertyChanged));

	public static readonly DependencyProperty HasActualTextProperty = DependencyProperty.RegisterAttached("HasActualText", typeof(bool), typeof(Watermark), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHasActualTextChanged, null, isAnimationProhibited: true, UpdateSourceTrigger.PropertyChanged));

	public static readonly DependencyProperty ImageProperty = DependencyProperty.RegisterAttached("Image", typeof(object), typeof(Watermark), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnImageChanged, null, isAnimationProhibited: true, UpdateSourceTrigger.PropertyChanged));

	public static readonly DependencyProperty HintTextProperty = DependencyProperty.RegisterAttached("HintText", typeof(string), typeof(Watermark), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHintTextChanged, null, isAnimationProhibited: true, UpdateSourceTrigger.PropertyChanged));

	public static readonly DependencyProperty ShowToolTipProperty = DependencyProperty.RegisterAttached("ShowToolTip", typeof(bool), typeof(Watermark), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShowToolTipChanged, null, isAnimationProhibited: true, UpdateSourceTrigger.PropertyChanged));

	public static readonly DependencyProperty ShowImageOnRightProperty = DependencyProperty.RegisterAttached("ShowImageOnRight", typeof(bool), typeof(Watermark), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShowImageOnRightChanged, null, isAnimationProhibited: true, UpdateSourceTrigger.PropertyChanged));

	public static WatermarkState GetWatermarkState(DependencyObject obj)
	{
		return (WatermarkState)obj.GetValue(WatermarkStateProperty);
	}

	public static void SetWatermarkState(DependencyObject obj, WatermarkState value)
	{
		obj.SetValue(WatermarkStateProperty, value);
	}

	public static bool GetHasActualText(DependencyObject obj)
	{
		return (bool)obj.GetValue(HasActualTextProperty);
	}

	public static void SetHasActualText(DependencyObject obj, bool value)
	{
		obj.SetValue(HasActualTextProperty, value);
	}

	private static void OnHasActualTextChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
	{
		WatermarkState watermarkState = MakeSureWatermarkStateExists(dp);
		watermarkState.HasActualText = (bool)args.NewValue;
		if ((dp as Control).IsLoaded)
		{
			SetWatermark_ToolTip(watermarkState);
		}
		else
		{
			(dp as Control).Loaded += Watermark_Loaded;
		}
	}

	public static object GetImage(DependencyObject obj)
	{
		return (Image)obj.GetValue(ImageProperty);
	}

	public static void SetImage(DependencyObject obj, object value)
	{
		obj.SetValue(ImageProperty, value);
	}

	private static void OnImageChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
	{
		if (!Equals(args.OldValue, args.NewValue))
		{
			WatermarkState watermarkState = MakeSureWatermarkStateExists(dp);
			watermarkState.Image = args.NewValue;
			watermarkState.OriginalPadding = watermarkState.Control.Padding;
			if ((dp as Control).IsLoaded)
			{
				SetWatermark_Adorner(watermarkState);
				SetWatermark_ToolTip(watermarkState);
			}
			else
			{
				(dp as Control).Loaded += Watermark_Loaded;
			}
		}
	}

	public static string GetHintText(DependencyObject obj)
	{
		return (string)obj.GetValue(HintTextProperty);
	}

	public static void SetHintText(DependencyObject obj, string value)
	{
		obj.SetValue(HintTextProperty, value);
	}

	private static void OnHintTextChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
	{
		if (!string.Equals(args.OldValue as string, args.NewValue as string))
		{
			WatermarkState watermarkState = MakeSureWatermarkStateExists(dp);
			watermarkState.HintText = args.NewValue as string;
			if ((dp as Control).IsLoaded)
			{
				SetWatermark_Adorner(watermarkState);
			}
			else
			{
				(dp as Control).Loaded += Watermark_Loaded;
			}
		}
	}

	public static bool GetShowToolTip(DependencyObject obj)
	{
		return (bool)obj.GetValue(ShowToolTipProperty);
	}

	public static void SetShowToolTip(DependencyObject obj, bool value)
	{
		obj.SetValue(ShowToolTipProperty, value);
	}

	private static void OnShowToolTipChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
	{
		WatermarkState watermarkState = MakeSureWatermarkStateExists(dp);
		watermarkState.ShowToolTip = (bool)args.NewValue;
		if ((dp as Control).IsLoaded)
		{
			SetWatermark_ToolTip(watermarkState);
		}
		else
		{
			(dp as Control).Loaded += Watermark_Loaded;
		}
	}

	public static bool GetShowImageOnRight(DependencyObject obj)
	{
		return (bool)obj.GetValue(ShowImageOnRightProperty);
	}

	public static void SetShowImageOnRight(DependencyObject obj, bool value)
	{
		obj.SetValue(ShowImageOnRightProperty, value);
	}

	private static void OnShowImageOnRightChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
	{
		WatermarkState watermarkState = MakeSureWatermarkStateExists(dp);
		watermarkState.ShowImageOnRight = (bool)args.NewValue;
		if ((dp as Control).IsLoaded)
		{
			SetWatermark_Adorner(watermarkState);
		}
		else
		{
			(dp as Control).Loaded += Watermark_Loaded;
		}
	}

	private static void Watermark_Loaded(object sender, RoutedEventArgs e)
	{
		try
		{
			WatermarkState watermarkState = GetWatermarkState(sender as Control);
			if (!watermarkState.IsInitialized)
			{
				watermarkState.Control.Unloaded += Watermark_Unloaded;
				watermarkState.IsInitialized = true;
				watermarkState.Control.SizeChanged += Control_SizeChanged;
				if (watermarkState.Control is ComboBox comboBox)
				{
					comboBox.ApplyTemplate();
					watermarkState.TextBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
					if (watermarkState.TextBox != null)
					{
						SetWatermarkState(watermarkState.TextBox, watermarkState);
						watermarkState.TextBox.TextChanged += TextBox_TextChanged;
						watermarkState.TextBox.SizeChanged += Control_SizeChanged;
					}
					else
					{
						(watermarkState.Control as ComboBox).SelectionChanged += ComboBox_SelectionChanged;
					}
					comboBox.IsVisibleChanged += TextBox_IsVisibleChanged;
				}
				else
				{
					TextBox obj = watermarkState.Control as TextBox;
					obj.TextChanged += TextBox_TextChanged;
					obj.IsVisibleChanged += TextBox_IsVisibleChanged;
				}
			}
			RebuildWatermarkState(watermarkState);
		}
		catch (Exception ex)
		{
			UiTracer.TraceSource.AssertTraceException2(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, ex, ex.Message, 561, "Watermark.cs", "Watermark_Loaded");
		}
	}

	private static void Watermark_Unloaded(object sender, RoutedEventArgs e)
	{
		try
		{
			WatermarkState watermarkState = GetWatermarkState(sender as Control);
			if (watermarkState == null)
			{
				return;
			}
			watermarkState.Control.Unloaded -= Watermark_Unloaded;
			watermarkState.Control.SizeChanged -= Control_SizeChanged;
			if (watermarkState.Control is ComboBox)
			{
				if (watermarkState.TextBox != null)
				{
					watermarkState.TextBox.TextChanged -= TextBox_TextChanged;
					watermarkState.TextBox.SizeChanged -= Control_SizeChanged;
				}
				else
				{
					(watermarkState.Control as ComboBox).SelectionChanged -= ComboBox_SelectionChanged;
				}
			}
			else if (watermarkState.Control is TextBox)
			{
				(watermarkState.Control as TextBox).TextChanged -= TextBox_TextChanged;
			}
			watermarkState.Control.IsVisibleChanged -= TextBox_IsVisibleChanged;
			watermarkState.IsInitialized = false;
		}
		catch (Exception ex)
		{
			UiTracer.TraceSource.AssertTraceException2(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, ex, ex.Message, 599, "Watermark.cs", "Watermark_Unloaded");
		}
	}

	public static void RebuildWatermarkState(WatermarkState state)
	{
		Thickness originalPadding = state.OriginalPadding;
		if (state.ShowImageOnRight)
		{
			originalPadding.Right += state.Image != null ? 16.0 + originalPadding.Right : 0.0;
		}
		else
		{
			originalPadding.Left += state.Image != null ? 16.0 + originalPadding.Left : 0.0;
		}
		if (state.TextBox != null)
		{
			originalPadding.Left -= state.TextBox.Margin.Left;
		}
		state.Control.Padding = originalPadding;
		SetWatermark_Adorner(state);
		SetWatermark_ToolTip(state);
	}

	private static void Control_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		try
		{
			RebuildWatermarkState(GetWatermarkState(sender as Control));
		}
		catch (Exception ex)
		{
			UiTracer.TraceSource.AssertTraceException2(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, ex, ex.Message, 639, "Watermark.cs", "Control_SizeChanged");
		}
	}

	private static void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		try
		{
			WatermarkState watermarkState = GetWatermarkState(sender as Control);
			if ((bool)e.NewValue)
			{
				RebuildWatermarkState(watermarkState);
			}
			else if (watermarkState.Adorner != null)
			{
				watermarkState.Adorner.Detach();
				watermarkState.Adorner = null;
			}
		}
		catch (Exception ex)
		{
			UiTracer.TraceSource.AssertTraceException2(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, ex, ex.Message, 664, "Watermark.cs", "TextBox_IsVisibleChanged");
		}
	}

	private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		try
		{
			ShowState(GetWatermarkState(sender as Control));
		}
		catch (Exception ex)
		{
			UiTracer.TraceSource.AssertTraceException2(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, ex, ex.Message, 679, "Watermark.cs", "TextBox_TextChanged");
		}
	}

	private static void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		try
		{
			ShowState(GetWatermarkState(sender as Control));
		}
		catch (Exception ex)
		{
			UiTracer.TraceSource.AssertTraceException2(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, ex, ex.Message, 693, "Watermark.cs", "ComboBox_SelectionChanged");
		}
	}

	private static void ShowState(WatermarkState state)
	{
		if (state.Control.IsVisible && state.Adorner != null)
		{
			if (!state.HasActualText && IsEmpty(state.Control))
			{
				state.Adorner.ShowText();
			}
			else
			{
				state.Adorner.HideText();
			}
		}
	}

	/*
	private static void SetReadOnly(Control control, bool value)
	{
		if (control is ComboBox)
		{
			(control as ComboBox).IsReadOnly = value;
		}
		else if (control is TextBox)
		{
			(control as TextBox).IsReadOnly = value;
		}
	}
	*/
	private static void SetWatermark_ToolTip(WatermarkState state)
	{
		if (state.ShowToolTip)
		{
			state.Control.ToolTip = state.HintText;
		}
	}

	private static void SetWatermark_Adorner(WatermarkState state)
	{
		state.Adorner?.Detach();
		if (state.Control.IsVisible)
		{
			state.Adorner = new WatermarkAdorner(state);
			state.Adorner.Attach();
			ShowState(state);
		}
		else
		{
			state.Adorner = null;
		}
	}

	private static bool IsEmpty(Control control)
	{
		if (control is ComboBox)
		{
			ComboBox comboBox = control as ComboBox;
			if (comboBox.IsEditable)
			{
				return string.IsNullOrEmpty((control as ComboBox).Text);
			}
			return comboBox.SelectedIndex < 0;
		}
		if (control is TextBox)
		{
			return string.IsNullOrEmpty((control as TextBox).Text);
		}
		return true;
	}

	private static WatermarkState MakeSureWatermarkStateExists(DependencyObject dp)
	{
		WatermarkState watermarkState = GetWatermarkState(dp);
		if (watermarkState == null)
		{
			watermarkState = new WatermarkState(dp as Control);
			SetWatermarkState(dp, watermarkState);
		}
		return watermarkState;
	}
}
