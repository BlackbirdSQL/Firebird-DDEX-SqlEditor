#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using BlackbirdSql.Wpf.Model;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Common.Model.Wpf;

namespace BlackbirdSql.Wpf.Controls
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public partial class HistoryPageView : UserControl, IDisposable, IComponentConnector, IStyleConnector
	{
		private bool _IsDisposed;

		/*
		public HistoryPageView projectSectionView;

		public Grid MainGrid;

		public Grid topLineGrid;

		public ScrollViewer contentViewer;

		public Grid itemsGrid;

		public WrapPanel favoriteTitle;

		public ListView favoriteConnectionList;

		public Grid lineGrid;

		public WrapPanel recentTitle;

		public TextBlock emptyListDescriptionText;

		public ListView recentConnectionList;

		public Border borderHeader;

		public Grid header;

		public ToggleButton expanderButton;

		public TextBlock headerText;

		public ConnectionPropertySectionView connectionPropertySection;

		public Grid BottomLineGrid;

		private bool _contentLoaded;

		*/

		public Traceable Trace => ViewModel.Trace;

		private HistoryPageViewModel ViewModel => DataContext as HistoryPageViewModel;

		public HistoryPageView()
		{
			InitializeComponent();
			recentConnectionList.MouseDoubleClick += ConnectionList_MouseDoubleClick;
			recentConnectionList.SelectionChanged += ConnectionList_SelectionChanged;
			recentConnectionList.MouseLeftButtonUp += ConnectionList_MouseLeftButtonUp;
			favoriteConnectionList.MouseDoubleClick += ConnectionList_MouseDoubleClick;
			favoriteConnectionList.SelectionChanged += ConnectionList_SelectionChanged;
			favoriteConnectionList.MouseLeftButtonUp += ConnectionList_MouseLeftButtonUp;
			borderHeader.MouseLeftButtonUp += ExpanderButton_Click;
			borderHeader.KeyDown += BorderHeader_KeyDown;
			contentViewer.PreviewMouseWheel += ContentViewer_PreviewMouseWheel;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_IsDisposed)
			{
				if (disposing)
				{
					ViewModel.Dispose();
				}

				_IsDisposed = true;
			}
		}

		private void ExpanderButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleConnectionPropertySectionState();
		}

		private void BorderHeader_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				ToggleConnectionPropertySectionState();
			}
		}

		private void ToggleConnectionPropertySectionState()
		{
			ViewModel.IsConnectionExpanded = !ViewModel.IsConnectionExpanded;
		}

		private ConnectionInfo GetFocusedNode(IList selectedItems)
		{
			if (selectedItems.Count == 1)
			{
				return selectedItems[0] as ConnectionInfo;
			}

			return null;
		}

		private void ConnectionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			try
			{
				ListView listView = sender as ListView;
				if (GetFocusedNode(listView.SelectedItems) != null)
				{
					ViewModel.OnMakeConnection();
				}

				e.Handled = true;
			}
			catch (Exception ex)
			{
				Trace.TraceException(TraceEventType.Error, EnUiTraceId.HistoryPage, ex, "HistoryPageView.connectionList_MouseDoubleClick: " + ex.Message, 154, "BlackbirdSql.Wpf.Controls\\HistoryPageView.xaml.cs", "ConnectionList_MouseDoubleClick");
			}
		}

		private void ConnectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				ListView listView = sender as ListView;
				ConnectionInfo focusedNode = GetFocusedNode(listView.SelectedItems);
				if (focusedNode != null)
				{
					ViewModel.UpdateSelectedConnection(focusedNode);
				}

				e.Handled = true;
			}
			catch (Exception ex)
			{
				Trace.TraceException(TraceEventType.Error, EnUiTraceId.HistoryPage, ex, "HistoryPageView.ConnectionList_SelectionChanged: " + ex.Message, 176, "BlackbirdSql.Wpf.Controls\\HistoryPageView.xaml.cs", "ConnectionList_SelectionChanged");
			}
		}

		private void ConnectionList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				ListView listView = sender as ListView;
				ConnectionInfo focusedNode = GetFocusedNode(listView.SelectedItems);
				if (focusedNode != null)
				{
					ViewModel.UpdateSelectedConnection(focusedNode);
				}

				e.Handled = true;
			}
			catch (Exception ex)
			{
				Trace.TraceException(TraceEventType.Error, EnUiTraceId.HistoryPage, ex, "HistoryPageView.ConnectionList_MouseLeftButtonUp: " + ex.Message, 198, "BlackbirdSql.Wpf.Controls\\HistoryPageView.xaml.cs", "ConnectionList_MouseLeftButtonUp");
			}
		}

		private void ContentViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer scrollViewer = (ScrollViewer)sender;
			if (scrollViewer != null && e != null)
			{
				scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
				e.Handled = true;
			}
		}

		private void PinUnpin_Clicked(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem menuItem && menuItem.DataContext != null)
			{
				ViewModel.PinUnpinCommand.Execute(menuItem.DataContext);
			}
		}

		private void Remove_Clicked(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem menuItem && menuItem.DataContext != null)
			{
				ViewModel.RemoveCommand.Execute(menuItem.DataContext);
			}
		}

		/*
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "7.0.5.0")]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				Uri resourceLocator = new Uri("/BlackbirdSql.EditorExtension;component/HistoryPageView.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "7.0.5.0")]
		public Delegate _CreateDelegate(Type delegateType, string handler)
		{
			return Delegate.CreateDelegate(delegateType, this, handler);
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "7.0.5.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
				case 1:
					projectSectionView = (HistoryPageView)target;
					break;
				case 4:
					MainGrid = (Grid)target;
					break;
				case 5:
					topLineGrid = (Grid)target;
					break;
				case 6:
					contentViewer = (ScrollViewer)target;
					break;
				case 7:
					itemsGrid = (Grid)target;
					break;
				case 8:
					favoriteTitle = (WrapPanel)target;
					break;
				case 9:
					favoriteConnectionList = (ListView)target;
					break;
				case 10:
					lineGrid = (Grid)target;
					break;
				case 11:
					recentTitle = (WrapPanel)target;
					break;
				case 12:
					emptyListDescriptionText = (TextBlock)target;
					break;
				case 13:
					recentConnectionList = (ListView)target;
					break;
				case 14:
					borderHeader = (Border)target;
					break;
				case 15:
					header = (Grid)target;
					break;
				case 16:
					expanderButton = (ToggleButton)target;
					expanderButton.Click += ExpanderButton_Click;
					break;
				case 17:
					headerText = (TextBlock)target;
					break;
				case 18:
					connectionPropertySection = (ConnectionPropertySectionView)target;
					break;
				case 19:
					BottomLineGrid = (Grid)target;
					break;
				default:
					_contentLoaded = true;
					break;
			}
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "7.0.5.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
				case 2:
					((MenuItem)target).Click += PinUnpin_Clicked;
					break;
				case 3:
					((MenuItem)target).Click += Remove_Clicked;
					break;
			}
		}
		*/
	}
}
