// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.PasswordBoxAssistant
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;




namespace BlackbirdSql.Common.Ctl;


public static class PasswordBoxAssistant
{
	public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxAssistant), new FrameworkPropertyMetadata(C_MagicValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged, null, isAnimationProhibited: true, UpdateSourceTrigger.PropertyChanged));

	private const string C_MagicValue = "\0\0x1\n\r\b\0\0x5\0x1";

	public static string GetPassword(DependencyObject obj)
	{
		return (string)obj.GetValue(PasswordProperty);
	}

	public static void SetPassword(DependencyObject obj, string value)
	{
		obj.SetValue(PasswordProperty, value);
	}

	private static void OnPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
	{
		if (dp is PasswordBox passwordBox)
		{
			passwordBox.PasswordChanged -= HandlePasswordChanged;
			string text = (string)args.NewValue;
			if (string.Equals(text, C_MagicValue, StringComparison.Ordinal))
			{
				text = string.Empty;
			}
			if (!string.Equals(passwordBox.Password, text, StringComparison.Ordinal))
			{
				passwordBox.Password = text;
			}
			passwordBox.PasswordChanged += HandlePasswordChanged;
		}
	}

	private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
	{
		PasswordBox passwordBox = (PasswordBox)sender;
		SetPassword(passwordBox, passwordBox.Password);
	}
}
