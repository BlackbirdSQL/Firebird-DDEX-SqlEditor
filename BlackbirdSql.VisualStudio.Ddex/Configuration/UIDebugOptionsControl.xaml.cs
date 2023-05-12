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
			CbEnableDiagnosticsLog.IsChecked = VsDebugOptionModel.Instance.EnableDiagnosticsLog;
			TxtLogFile.Text = VsDebugOptionModel.Instance.LogFile;
			CbEnableFbDiagnostics.IsChecked = VsDebugOptionModel.Instance.EnableFbDiagnostics;
			TxtFbLogFile.Text = VsDebugOptionModel.Instance.FbLogFile;
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

		private void CbEnableDiagnosticsLog_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.EnableDiagnosticsLog = (bool)CbEnableDiagnosticsLog.IsChecked;
			VsDebugOptionModel.Instance.Save();
		}

		private void CbEnableDiagnosticsLog_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.EnableDiagnosticsLog = (bool)CbEnableDiagnosticsLog.IsChecked;
			VsDebugOptionModel.Instance.Save();
		}

		private void TxtLogFile_TextChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.LogFile = TxtLogFile.Text;
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

		private void TxtFbLogFile_TextChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			VsDebugOptionModel.Instance.FbLogFile = TxtFbLogFile.Text;
			VsDebugOptionModel.Instance.Save();
		}
	}
}
