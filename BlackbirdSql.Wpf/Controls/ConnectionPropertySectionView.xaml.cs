#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Wpf.Model;




namespace BlackbirdSql.Wpf.Controls
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public partial class ConnectionPropertySectionView : UserControl, IDisposable, IComponentConnector
	{
		private bool _IsDisposed;

		/*
		public ConnectionPropertySectionView projectSectionView;

		public Grid Grid1;

		public Label DataSourceLabel;

		public Grid dataSourceGrid;

		public TextBox ServerNameTextBox2;

		public Button DataSourceButtonButton;

		public Label ServerNameLabel;

		public TextBox ServerNameTextBox;

		public Label AuthenticationLabel;

		public ComboBox AuthenticationComboBox;

		public Label UserNameLabel;

		public TextBox UserNameTextBox;

		public Label PasswordLabel;

		public PasswordBox PasswordBox;

		public CheckBox RememberPasswordCheckBox;

		public Label DatabaseNameLabel;

		public ComboBox DatabaseNameComboBox;

		public TextBlock AdvancedPropertiesLink;

		public NotificationTextBlock StatusLiveRegion;


		private bool _contentLoaded;

		*/

		private ConnectionPropertySectionViewModel ViewModel => DataContext as ConnectionPropertySectionViewModel;

		public ConnectionPropertySectionView()
		{
			InitializeComponent();
		}

		private async void DatabaseNameComboBoxOnDropDownOpened(object sender, EventArgs eventArgs)
		{
			StatusLiveRegion.RaiseNotificationEvent(SharedResx.ConnectedText, SharedResx.Connect);
			await ViewModel.LoadDatabasesAsync();
		}

		private void ControlLoaded(object sender, RoutedEventArgs e)
		{
			if (Visibility == Visibility.Visible)
			{
				ServerNameTextBox.Focus();
			}
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


		/*
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "7.0.5.0")]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				Uri resourceLocator = new Uri("/BlackbirdSql.EditorExtension;component/ConnectionPropertySectionView.xaml", UriKind.Relative);
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
					projectSectionView = (ConnectionPropertySectionView)target;
					projectSectionView.Loaded += ControlLoaded;
					break;
				case 2:
					Grid1 = (Grid)target;
					break;
				case 3:
					DataSourceLabel = (Label)target;
					break;
				case 4:
					dataSourceGrid = (Grid)target;
					break;
				case 5:
					ServerNameTextBox2 = (TextBox)target;
					break;
				case 6:
					DataSourceButtonButton = (Button)target;
					break;
				case 7:
					ServerNameLabel = (Label)target;
					break;
				case 8:
					ServerNameTextBox = (TextBox)target;
					break;
				case 9:
					AuthenticationLabel = (Label)target;
					break;
				case 10:
					AuthenticationComboBox = (ComboBox)target;
					break;
				case 11:
					UserNameLabel = (Label)target;
					break;
				case 12:
					UserNameTextBox = (TextBox)target;
					break;
				case 13:
					PasswordLabel = (Label)target;
					break;
				case 14:
					PasswordBox = (PasswordBox)target;
					break;
				case 15:
					RememberPasswordCheckBox = (CheckBox)target;
					break;
				case 16:
					DatabaseNameLabel = (Label)target;
					break;
				case 17:
					DatabaseNameComboBox = (ComboBox)target;
					DatabaseNameComboBox.DropDownOpened += DatabaseNameComboBoxOnDropDownOpened;
					break;
				case 18:
					AdvancedPropertiesLink = (TextBlock)target;
					break;
				case 19:
					StatusLiveRegion = (NotificationTextBlock)target;
					break;
				default:
					_contentLoaded = true;
					break;
			}
		}
		*/
		
	}
}
