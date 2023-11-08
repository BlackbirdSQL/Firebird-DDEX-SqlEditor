// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.ListViewHelper

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Ctl.Wpf;


[EditorBrowsable(EditorBrowsableState.Never)]
public static class ListViewHelper
{
	private class CollectionFilterProvider
	{
		private string m_filterText;

		public FrameworkElement ParentElement { get; private set; }

		private IBListViewHelperFilterProvider FilterProvider { get; set; }

		public string FilterText
		{
			get
			{
				return m_filterText;
			}
			set
			{
				m_filterText = value;
				m_filterText = m_filterText.Trim();
				m_filterText = m_filterText.Replace("\"", string.Empty);
			}
		}

		public CollectionFilterProvider(FrameworkElement parentElement)
		{
			ParentElement = parentElement;
			if (parentElement != null)
			{
				FilterProvider = GetFilterProvider(parentElement);
			}
		}

		public bool FilterCallback(object item)
		{
			if (FilterProvider != null)
			{
				return FilterProvider.FilterItem(item, FilterText);
			}
			return true;
		}

		public void StartFilterPass()
		{
			FilterProvider?.StartFilterPass();
		}

		public void FilterPassComplete()
		{
			FilterProvider?.FilterPassComplete();
		}

		public void FilterCleared()
		{
			FilterProvider?.FilterCleared();
		}
	}

	// private static readonly string[] m_traceKeywords;

	public static readonly DependencyProperty MultiFilterTextProperty;

	private static readonly DependencyProperty TargetListBoxHoldersProperty;

	public static readonly DependencyProperty FilterTextProperty;

	private static readonly DependencyProperty TargetListBoxProperty;

	private static readonly DependencyProperty TargetListBoxItemsSourceProperty;

	public static readonly DependencyProperty FilterProviderProperty;

	private static readonly DependencyProperty PrivateFilterProviderProperty;

	static ListViewHelper()
	{
		MultiFilterTextProperty = DependencyProperty.RegisterAttached("MultiFilterText", typeof(string), typeof(ListViewHelper), new UIPropertyMetadata(MultiFilterTextChanged));
		TargetListBoxHoldersProperty = DependencyProperty.RegisterAttached("TargetListBoxHolders", typeof(List<FrameworkElement>), typeof(ListViewHelper));
		FilterTextProperty = DependencyProperty.RegisterAttached("FilterText", typeof(string), typeof(ListViewHelper), new UIPropertyMetadata(FilterTextChanged));
		TargetListBoxProperty = DependencyProperty.RegisterAttached("TargetListBox", typeof(ListBox), typeof(ListViewHelper));
		TargetListBoxItemsSourceProperty = DependencyProperty.RegisterAttached("TargetListBoxItemsSource", typeof(IEnumerable), typeof(ListViewHelper), new UIPropertyMetadata(TargetListBoxItemsSourceChanged));
		FilterProviderProperty = DependencyProperty.RegisterAttached("FilterProvider", typeof(IBListViewHelperFilterProvider), typeof(ListViewHelper));
		PrivateFilterProviderProperty = DependencyProperty.RegisterAttached("PrivateFilterProvider", typeof(CollectionFilterProvider), typeof(ListViewHelper));
		// m_traceKeywords = new string[1] { "ListViewHelper" };
	}

	public static string GetMultiFilterText(FrameworkElement target)
	{
		if (target != null)
		{
			return (string)target.GetValue(FilterTextProperty);
		}
		return null;
	}

	public static void SetMultiFilterText(FrameworkElement target, string value)
	{
		target?.SetValue(FilterTextProperty, value);
	}

	private static void MultiFilterTextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		try
		{
			List<FrameworkElement> targetListBoxHolders = GetTargetListBoxHolders(sender as FrameworkElement);
			if (targetListBoxHolders == null)
			{
				return;
			}
			DateTime now = DateTime.Now;
			foreach (FrameworkElement item in targetListBoxHolders)
			{
				FilterTextChanged(item, e);
			}
			DateTime now2 = DateTime.Now;
			UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.MultiFilterTextChanged total filter time={0}ms", (long)now2.Subtract(now).TotalMilliseconds);
		}
		catch (Exception exception)
		{
			UiTracer.TraceSource.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "ListViewHelper.MultiFilterTextChanged", 133, "ListViewHelper.cs", "MultiFilterTextChanged");
		}
	}

	public static List<FrameworkElement> GetTargetListBoxHolders(FrameworkElement target)
	{
		if (target != null)
		{
			return (List<FrameworkElement>)target.GetValue(TargetListBoxHoldersProperty);
		}
		return null;
	}

	public static void SetTargetListBoxHolders(FrameworkElement target, List<FrameworkElement> value)
	{
		target?.SetValue(TargetListBoxHoldersProperty, value);
	}

	public static string GetFilterText(FrameworkElement target)
	{
		if (target != null)
		{
			return (string)target.GetValue(FilterTextProperty);
		}
		return null;
	}

	public static void SetFilterText(FrameworkElement target, string value)
	{
		target?.SetValue(FilterTextProperty, value);
	}

	private static void FilterTextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		try
		{
			ListBox listBoxForFiltering = GetListBoxForFiltering(sender);
			if (listBoxForFiltering != null)
			{
				string value = e.OldValue as string;
				string text = e.NewValue as string;
				bool num = string.IsNullOrEmpty(value);
				bool flag = string.IsNullOrEmpty(text);
				bool flag2 = num && !flag;
				bool flag3 = !num && flag;
				DateTime now = DateTime.Now;
				if (flag2)
				{
					InstallCollectionFilter((FrameworkElement)sender, listBoxForFiltering.ItemsSource, text);
				}
				else if (flag3)
				{
					RemoveCollectionFilter((FrameworkElement)sender, listBoxForFiltering.ItemsSource, removeBinding: true);
				}
				else if (!flag)
				{
					RefreshCollectionFilter((FrameworkElement)sender, listBoxForFiltering.ItemsSource, text);
				}
				DateTime now2 = DateTime.Now;
				UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.FilterTextChanged filter time={0}ms", (long)now2.Subtract(now).TotalMilliseconds);
			}
		}
		catch (Exception exception)
		{
			UiTracer.TraceSource.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "ListViewHelper.FilterTextChanged", 274, "ListViewHelper.cs", "FilterTextChanged");
		}
	}

	public static ListBox GetTargetListBox(FrameworkElement target)
	{
		if (target != null)
		{
			return (ListBox)target.GetValue(TargetListBoxProperty);
		}
		return null;
	}

	public static void SetTargetListBox(FrameworkElement target, ListBox value)
	{
		target?.SetValue(TargetListBoxProperty, value);
	}

	private static void TargetListBoxItemsSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		try
		{
			if (sender is not ListBox target)
				return;

			CollectionFilterProvider privateFilterProvider = GetPrivateFilterProvider(target);
			if (privateFilterProvider != null)
			{
				UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.TargetListBoxItemsSourceChanged updating collection filter");
				FrameworkElement parentElement = privateFilterProvider.ParentElement;
				DateTime now = DateTime.Now;
				IEnumerable itemsSource = e.OldValue as IEnumerable;
				IEnumerable itemsSource2 = e.NewValue as IEnumerable;
				string filterText = GetFilterText(parentElement);
				if (!string.IsNullOrEmpty(filterText))
				{
					RemoveCollectionFilter(parentElement, itemsSource, removeBinding: false);
					InstallCollectionFilter(parentElement, itemsSource2, filterText);
				}
				DateTime now2 = DateTime.Now;
				UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.TargetListBoxItemsSourceChanged filter time={0}ms", (long)now2.Subtract(now).TotalMilliseconds);
			}
			else
			{
				UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.TargetListBoxItemsSourceChanged ignored because no collection filter is installed");
			}
		}
		catch (Exception exception)
		{
			UiTracer.TraceSource.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "ListViewHelper.TargetListBoxItemsSourceChanged", 374, "ListViewHelper.cs", "TargetListBoxItemsSourceChanged");
		}
	}

	public static IBListViewHelperFilterProvider GetFilterProvider(FrameworkElement target)
	{
		if (target != null)
		{
			return (IBListViewHelperFilterProvider)target.GetValue(FilterProviderProperty);
		}
		return null;
	}

	public static void SetFilterProvider(FrameworkElement target, IBListViewHelperFilterProvider value)
	{
		target?.SetValue(FilterProviderProperty, value);
	}

	private static ListBox GetListBoxForFiltering(object obj)
	{
		ListBox listBox = obj as ListBox;
		if (listBox == null && obj is FrameworkElement element)
		{
			listBox = GetTargetListBox(element);
		}
		return listBox;
	}

	private static void InstallCollectionFilter(FrameworkElement element, IEnumerable itemsSource, string filterText)
	{
		ICollectionView defaultView = CollectionViewSource.GetDefaultView(itemsSource);
		if (defaultView != null)
		{
			UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.InstallCollectionFilter");
			ListBox listBoxForFiltering = GetListBoxForFiltering(element);
			if (listBoxForFiltering != null && BindingOperations.GetBinding(listBoxForFiltering, TargetListBoxItemsSourceProperty) == null)
			{
				UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.InstallCollectionFilter binding ListBox.ItemsSourceProperty to detect whole collection replacement");
				BindingOperations.SetBinding(listBoxForFiltering, TargetListBoxItemsSourceProperty, new Binding
				{
					Path = new PropertyPath("ItemsSource"),
					Source = listBoxForFiltering
				});
			}
			CollectionFilterProvider collectionFilterProvider = new(element)
			{
				FilterText = filterText
			};

			SetPrivateFilterProvider(element, collectionFilterProvider);

			if (listBoxForFiltering != null && element != listBoxForFiltering)
			{
				SetPrivateFilterProvider(listBoxForFiltering, collectionFilterProvider);
			}

			try
			{
				collectionFilterProvider.StartFilterPass();
				defaultView.Filter = collectionFilterProvider.FilterCallback;
			}
			finally
			{
				collectionFilterProvider.FilterPassComplete();
			}
			UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.InstallCollectionFilter completed");
		}
	}

	private static void RemoveCollectionFilter(FrameworkElement element, IEnumerable itemsSource, bool removeBinding)
	{
		ICollectionView defaultView = CollectionViewSource.GetDefaultView(itemsSource);
		if (defaultView != null)
		{
			UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.RemoveCollectionFilter");
			GetPrivateFilterProvider(element)?.FilterCleared();
			defaultView.Filter = null;
			SetPrivateFilterProvider(element, null);
			ListBox listBoxForFiltering = GetListBoxForFiltering(element);
			if (listBoxForFiltering != null && element != listBoxForFiltering)
			{
				SetPrivateFilterProvider(listBoxForFiltering, null);
			}
			if (removeBinding && listBoxForFiltering != null)
			{
				UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.RemoveCollectionFilter unbinding ListBox.ItemsSourceProperty");
				BindingOperations.ClearBinding(listBoxForFiltering, TargetListBoxItemsSourceProperty);
			}
			object obj = listBoxForFiltering?.SelectedItem;
			if (obj != null)
			{
				listBoxForFiltering.ScrollIntoView(obj);
			}
			UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.RemoveCollectionFilter completed");
		}
	}

	private static void RefreshCollectionFilter(FrameworkElement element, IEnumerable itemsSource, string filterText)
	{
		ICollectionView defaultView = CollectionViewSource.GetDefaultView(itemsSource);
		if (defaultView == null)
		{
			return;
		}
		UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.RefreshCollectionFilter");
		CollectionFilterProvider privateFilterProvider = GetPrivateFilterProvider(element);
		if (privateFilterProvider != null)
		{
			privateFilterProvider.FilterText = filterText;
			try
			{
				privateFilterProvider.StartFilterPass();
				defaultView.Refresh();
				ListBox listBoxForFiltering = GetListBoxForFiltering(element);
				object obj = listBoxForFiltering?.SelectedItem;
				if (obj != null)
				{
					listBoxForFiltering.ScrollIntoView(obj);
				}
			}
			finally
			{
				privateFilterProvider.FilterPassComplete();
			}
		}
		UiTracer.TraceSource.TraceEvent(TraceEventType.Verbose, EnUiTraceId.UiInfra, "ListViewHelper.RefreshCollectionFilter completed");
	}

	private static CollectionFilterProvider GetPrivateFilterProvider(FrameworkElement target)
	{
		if (target != null)
		{
			return (CollectionFilterProvider)target.GetValue(PrivateFilterProviderProperty);
		}
		return null;
	}

	private static void SetPrivateFilterProvider(FrameworkElement target, CollectionFilterProvider value)
	{
		target?.SetValue(PrivateFilterProviderProperty, value);
	}
}
