using System.Windows.Controls;



namespace BlackbirdSql.VisualStudio.Ddex.Configuration
{
	/// <summary>
	/// Interaction logic for UIDebugOptionsControl.xaml
	/// </summary>
	public partial class UIDebugOptionsControl : UserControl
	{
		public UIDebugOptionsControl()
		{
			InitializeComponent();
		}
		internal UIDebugOptionsDialogPage DebugOptionDialogPage;

		public void Initialize()
		{
			CbEnableTrace.IsChecked = VsDebugOptionModel.Instance.EnableTrace;
			CbEnableDiagnostics.IsChecked = VsDebugOptionModel.Instance.EnableDiagnostics;
			CbEnableFbDiagnostics.IsChecked = VsDebugOptionModel.Instance.EnableFbDiagnostics;
			VsDebugOptionModel.Instance.Save();
		}

		private void CbEnableTrace_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.EnableTrace = (bool)CbEnableTrace.IsChecked;
			VsDebugOptionModel.Instance.Save();
		}

		private void CbEnableTrace_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.EnableTrace = (bool)CbEnableTrace.IsChecked;
			VsDebugOptionModel.Instance.Save();
		}

		private void CbEnableDiagnostics_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.EnableDiagnostics = (bool)CbEnableDiagnostics.IsChecked;
			VsDebugOptionModel.Instance.Save();
		}

		private void CbEnableDiagnostics_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.EnableDiagnostics = (bool)CbEnableDiagnostics.IsChecked;
			VsDebugOptionModel.Instance.Save();
		}


		private void CbEnableFbDiagnostics_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.EnableFbDiagnostics = (bool)CbEnableFbDiagnostics.IsChecked;
			VsDebugOptionModel.Instance.Save();
		}

		private void CbEnableFbDiagnostics_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.EnableFbDiagnostics = (bool)CbEnableFbDiagnostics.IsChecked;
			VsDebugOptionModel.Instance.Save();
		}
	}
}
