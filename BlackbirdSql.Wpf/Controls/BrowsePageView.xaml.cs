// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.BrowsePageView

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Wpf.Model;
using BlackbirdSql.Wpf.Widgets;

namespace BlackbirdSql.Wpf.Controls;


[EditorBrowsable(EditorBrowsableState.Never)]
public partial class BrowsePageView : UserControl, IDisposable, IComponentConnector
{
	private bool _IsDisposed;

	private readonly List<FrameworkElement> _FilterableSections = new List<FrameworkElement>();

	/*
	public BrowsePageView projectSectionView;

	public Grid itemsGrid;

	public Grid topLineGrid;

	public ScrollViewer contentViewer;

	public StackPanel sectionsPanel;

	public Grid lineGrid;

	public ConnectionPropertySectionView connectionPropertySection;

	public Grid bottomLineGrid;

	private bool _contentLoaded;
	*/
	private BrowsePageViewModel ViewModel => DataContext as BrowsePageViewModel;

	public BrowsePageView()
	{
		InitializeComponent();
		contentViewer.PreviewMouseWheel += ContentViewer_PreviewMouseWheel;
	}

	public void AddSection(SectionControl sectionControl)
	{
		sectionsPanel.Children.Add(sectionControl);
		UpdateFilterableSectionContents(sectionControl);
		TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
		sectionsPanel.MoveFocus(request);
	}

	private void UpdateFilterableSectionContents(SectionControl sectionControl)
	{
		FrameworkElement frameworkElement = sectionControl.Content as FrameworkElement;
		if (ListViewHelper.GetTargetListBox(frameworkElement) != null)
		{
			_FilterableSections.Add(frameworkElement);
			ListViewHelper.SetTargetListBoxHolders(sectionsPanel, _FilterableSections);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}


	public void Dispose(bool disposing)
	{
		if (!_IsDisposed)
		{
			if (disposing)
			{
				contentViewer.PreviewMouseWheel -= ContentViewer_PreviewMouseWheel;
				ViewModel.Dispose();
			}
			_IsDisposed = true;
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

	/*
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "7.0.5.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BlackbirdSql.EditorExtension;component/BrowsePageView.xaml", UriKind.Relative);
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
				projectSectionView = (BrowsePageView)target;
				break;
			case 2:
				itemsGrid = (Grid)target;
				break;
			case 3:
				topLineGrid = (Grid)target;
				break;
			case 4:
				contentViewer = (ScrollViewer)target;
				break;
			case 5:
				sectionsPanel = (StackPanel)target;
				break;
			case 6:
				lineGrid = (Grid)target;
				break;
			case 7:
				connectionPropertySection = (ConnectionPropertySectionView)target;
				break;
			case 8:
				bottomLineGrid = (Grid)target;
				break;
			default:
				_contentLoaded = true;
				break;
		}
	}
	*/
}
