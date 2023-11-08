// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.TreeViewItemExtensions

using System.Windows;

namespace BlackbirdSql.Common.Controls.Wpf;


public static class TreeViewItemExtensions
{
	public static readonly DependencyProperty IndentWidthProperty = DependencyProperty.RegisterAttached("IndentWidth", typeof(double), typeof(TreeViewItemExtensions), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits));

	public static double GetIndentWidth(DependencyObject obj)
	{
		return (double)obj.GetValue(IndentWidthProperty);
	}

	public static void SetIndentWidth(DependencyObject obj, double indentWidth)
	{
		obj.SetValue(IndentWidthProperty, indentWidth);
	}
}
