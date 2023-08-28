// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Framework.SectionControl

using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

using BlackbirdSql.Core;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Interfaces;
using Cmd = BlackbirdSql.Common.Cmd;

namespace BlackbirdSql.Wpf.Widgets
{
	/// <summary>
	/// Interaction logic for SectionControl.xaml
	/// </summary>
	public partial class SectionControl : StackPanel, IBSectionControl, IComponentConnector
	{
		public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(SectionControl));

		public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(SectionControl), new PropertyMetadata(string.Empty));

		public static readonly DependencyProperty HeaderToolTipProperty = DependencyProperty.Register("HeaderToolTip", typeof(string), typeof(SectionControl), new PropertyMetadata(string.Empty));

		public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsConnectionExpanded", typeof(bool), typeof(SectionControl), new PropertyMetadata(false, IsExpandedChangedCallback));

		public static readonly DependencyProperty ExpanderButtonVisibilityProperty = DependencyProperty.Register("ExpanderButtonVisibility", typeof(Visibility), typeof(SectionControl), new PropertyMetadata(Visibility.Visible));

		public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(SectionControl), new PropertyMetadata(false, IsBusyChangedCallback));

		public static readonly DependencyProperty ErrorWarningTextProperty = DependencyProperty.Register("ErrorWarningText", typeof(string), typeof(SectionControl), new PropertyMetadata(string.Empty));

		public static readonly DependencyProperty StateGridVisibilityProperty = DependencyProperty.Register("StateGridVisibility", typeof(Visibility), typeof(SectionControl), new PropertyMetadata(Visibility.Collapsed));

		public static readonly DependencyProperty IsSectionEmptyProperty = DependencyProperty.Register("IsSectionEmpty", typeof(bool), typeof(SectionControl), new PropertyMetadata(true));

		public static readonly DependencyProperty ShowProgressWhenBusyProperty = DependencyProperty.Register("ShowProgressWhenBusy", typeof(bool), typeof(SectionControl), new PropertyMetadata(false));


		public object Content
		{
			get
			{
				return GetValue(ContentProperty);
			}
			set
			{
				SetValue(ContentProperty, value);
			}
		}

		public string HeaderText
		{
			get
			{
				return (string)GetValue(HeaderTextProperty);
			}
			set
			{
				SetValue(HeaderTextProperty, value);
			}
		}

		public string HeaderToolTip
		{
			get
			{
				return (string)GetValue(HeaderToolTipProperty);
			}
			set
			{
				SetValue(HeaderToolTipProperty, value);
			}
		}

		public bool IsExpanded
		{
			get
			{
				return (bool)GetValue(IsExpandedProperty);
			}
			set
			{
				ExpandCollapseState expandCollapseState = IsExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
				ExpandCollapseState expandCollapseState2 = value ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
				SetValue(IsExpandedProperty, value);
				if (expandCollapseState != expandCollapseState2)
				{
					UIElementAutomationPeer.CreatePeerForElement(borderHeader)?.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, expandCollapseState, expandCollapseState2);
				}
			}
		}

		public Visibility ExpanderButtonVisibility
		{
			get
			{
				return (Visibility)GetValue(ExpanderButtonVisibilityProperty);
			}
			set
			{
				SetValue(ExpanderButtonVisibilityProperty, value);
			}
		}

		public string ErrorText
		{
			get
			{
				return ErrorWarningText;
			}
			set
			{
				ErrorWarningText = value;
				SetErrorTextAndIcon(value, CoreIconsCollection.Instance.GetImage(CoreIconsCollection.Instance.Error_16));
			}
		}

		public string WarningText
		{
			get
			{
				return ErrorWarningText;
			}
			set
			{
				ErrorWarningText = value;
				SetErrorTextAndIcon(value, CoreIconsCollection.Instance.GetImage(CoreIconsCollection.Instance.Warning_16));
			}
		}

		public bool IsBusy
		{
			get
			{
				return (bool)GetValue(IsBusyProperty);
			}
			set
			{
				SetValue(IsBusyProperty, value);
			}
		}

		public string ErrorWarningText
		{
			get
			{
				return (string)GetValue(ErrorWarningTextProperty);
			}
			set
			{
				SetValue(ErrorWarningTextProperty, value);
			}
		}

		public Visibility StateGridVisibility
		{
			get
			{
				return (Visibility)GetValue(StateGridVisibilityProperty);
			}
			set
			{
				SetValue(StateGridVisibilityProperty, value);
			}
		}

		public bool IsSectionEmpty
		{
			get
			{
				return (bool)GetValue(IsSectionEmptyProperty);
			}
			set
			{
				SetValue(IsSectionEmptyProperty, value);
			}
		}

		public bool ShowProgressWhenBusy
		{
			get
			{
				return (bool)GetValue(ShowProgressWhenBusyProperty);
			}
			set
			{
				SetValue(ShowProgressWhenBusyProperty, value);
			}
		}

		public event RoutedEventHandler Collapsed;

		public event RoutedEventHandler Expanded;



		public SectionControl()
		{
			InitializeComponent();
			UpdateContentVisibility();
			IsExpanded = true;
			borderHeader.KeyDown += BorderHeader_KeyDown;
			borderHeader.MouseLeftButtonUp += ExpanderButton_Click;
			borderHeader.DragEnter += BorderHeader_DragMove;
			borderHeader.DragOver += BorderHeader_DragMove;
			borderHeader.Drop += BorderHeader_Drop;
		}



		protected virtual void OnCollapsed()
		{
		}

		protected virtual void OnExpanded()
		{
		}

		private void BorderHeader_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Right)
			{
				e.Handled = true;
				IsExpanded = true;
			}
			else if (e.Key == Key.Left)
			{
				e.Handled = true;
				IsExpanded = false;
			}
			else if (e.Key == Key.Space)
			{
				e.Handled = true;
				IsExpanded = !IsExpanded;
			}
		}

		private void ExpanderButton_Click(object sender, RoutedEventArgs e)
		{
			IsExpanded = !IsExpanded;
		}

		private void UpdateContentVisibility()
		{
			contentArea.Visibility = !IsExpanded ? Visibility.Collapsed : Visibility.Visible;
		}

		private void NotifyExpansionChanged(bool expanded)
		{
			if (expanded)
			{
				OnExpanded();
			}
			else
			{
				OnCollapsed();
			}
			(expanded ? Expanded : Collapsed)?.Invoke(this, new RoutedEventArgs());
		}

		private void BorderHeader_DragMove(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void BorderHeader_Drop(object sender, DragEventArgs e)
		{
			e.Handled = true;
		}

		private static void IsExpandedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SectionControl sectionControl)
			{
				bool flag = (bool)e.NewValue;
				sectionControl.expanderButton.IsChecked = flag;
				sectionControl.UpdateContentVisibility();
				sectionControl.NotifyExpansionChanged(flag);
				if (!flag && sectionControl.IsKeyboardFocusWithin)
				{
					Trace.TraceInformation("Section collapsed while containing focus, focus reset to header");
					sectionControl.borderHeader.Focus();
				}
			}
		}

		private static void IsBusyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is not SectionControl sectionControl)
			{
				return;
			}
			if ((bool)e.NewValue)
			{
				sectionControl.errorWarningIcon.Visibility = Visibility.Collapsed;
				if (sectionControl.ShowProgressWhenBusy)
				{
					sectionControl.progressGrid.Children.Add(new ProgressStrip
					{
						ShowProgress = true,
						Visibility = Visibility.Visible
					});
				}
				sectionControl.StateGridVisibility = Visibility.Visible;
			}
			else
			{
				if (sectionControl.progressGrid.Children.Count > 0 && sectionControl.progressGrid.Children[0] is ProgressStrip strip)
				{
					strip.ShowProgress = false;
				}
				sectionControl.progressGrid.Children.Clear();
				sectionControl.StateGridVisibility = Visibility.Collapsed;
			}
		}

		private void SetErrorTextAndIcon(string text, BitmapImage icon)
		{
			AutomationProperties.SetName(errorWarningIcon, text);
			if (!string.IsNullOrEmpty(text))
			{
				errorWarningIcon.Source = icon;
				errorWarningIcon.Visibility = Visibility.Visible;
				StateGridVisibility = Visibility.Visible;
			}
			else
			{
				errorWarningIcon.Visibility = Visibility.Collapsed;
				StateGridVisibility = Visibility.Collapsed;
			}
		}

		public void SetFocusOnHeader()
		{
			borderHeader.Focus();
		}

		public bool IsFocusOnHeader()
		{
			return borderHeader.IsFocused;
		}

		public bool SetFocusOnContent()
		{
			if (GetFirstFocusableChild() != null)
			{
				content.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
				return true;
			}
			return false;
		}

		public bool IsFocusOnFirstFocusableChild()
		{
			return GetFirstFocusableChild()?.IsFocused ?? false;
		}

		private UIElement GetFirstFocusableChild()
		{
			if (content.PredictFocus(FocusNavigationDirection.Down) is UIElement uIElement && Cmd.FindVisualParent<SectionControl>(uIElement) == this)
			{
				return uIElement;
			}
			return null;
		}

	}
}
