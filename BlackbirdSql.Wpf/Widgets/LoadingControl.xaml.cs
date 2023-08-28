// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.LoadingControl

using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Wpf.Widgets
{
	/// <summary>
	/// Interaction logic for LoadingControl.xaml
	/// </summary>
	public partial class LoadingControl : UserControl, IComponentConnector
	{
		public string LoadingText { get; set; }



		public LoadingControl()
		{
			InitializeComponent();
			base.Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			LoadingLabel.Content = LoadingText ?? SharedResx.LoadingText;
		}
	}
}
