// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.TreeViewItemBehaviors

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;

namespace BlackbirdSql.Common.Controls.Wpf;


public static class TreeViewItemBehaviors
{
	public static readonly DependencyProperty SelectAndFocusOnRightClickProperty = DependencyProperty.RegisterAttached("SelectAndFocusOnRightClick", typeof(bool), typeof(TreeViewItemBehaviors), new PropertyMetadata(OnSelectAndFocusOnRightClickChanged));

	public static bool GetSelectAndFocusOnRightClick(TreeViewItem treeViewItem)
	{
		return (bool)treeViewItem.GetValue(SelectAndFocusOnRightClickProperty);
	}

	public static void SetSelectAndFocusOnRightClick(TreeViewItem treeViewItem, bool ignore)
	{
		treeViewItem.SetValue(SelectAndFocusOnRightClickProperty, ignore);
	}

	private static void OnSelectAndFocusOnRightClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TreeViewItem treeViewItem = (TreeViewItem)d;
		UiTracer.TraceSource.AssertTraceEvent(treeViewItem != null, TraceEventType.Error, EnUiTraceId.UiInfra, "element is null");
		if (treeViewItem != null && e.NewValue != e.OldValue)
		{
			if ((bool)e.NewValue)
			{
				treeViewItem.MouseRightButtonDown += OnMouseRightButtonDown;
			}
			else
			{
				treeViewItem.MouseRightButtonDown -= OnMouseRightButtonDown;
			}
		}
	}

	private static void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		TreeViewItem treeViewItem = sender as TreeViewItem;
		UiTracer.TraceSource.AssertTraceEvent(treeViewItem != null, TraceEventType.Error, EnUiTraceId.UiInfra, "item is null");
		if (treeViewItem != null && !treeViewItem.IsSelected)
		{
			treeViewItem.IsSelected = true;
			treeViewItem.Focus();
		}
		e.Handled = true;
	}
}
