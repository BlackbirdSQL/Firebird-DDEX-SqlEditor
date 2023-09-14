using System.Windows.Controls;
using BlackbirdSql.VisualStudio.Ddex.Ctl.Config;

namespace BlackbirdSql.VisualStudio.Ddex.Controls.Config
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
		internal DebugOptionsDialogPage DebugOptionDialogPage;

		public void Initialize()
		{
			CbEnableTrace.IsChecked = DebugOptionModel.Instance.EnableTrace;
			CbEnableTracer.IsChecked = DebugOptionModel.Instance.EnableTracer;
			CbPersistentValidation.IsChecked = DebugOptionModel.Instance.PersistentValidation;
			CbEnableDiagnosticsLog.IsChecked = DebugOptionModel.Instance.EnableDiagnosticsLog;
			TxtLogFile.Text = DebugOptionModel.Instance.LogFile;
			CbEnableFbDiagnostics.IsChecked = DebugOptionModel.Instance.EnableFbDiagnostics;
			TxtFbLogFile.Text = DebugOptionModel.Instance.FbLogFile;
			DebugOptionModel.Instance.Save();
		}

		private void CbEnableTrace_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.EnableTrace = (bool)CbEnableTrace.IsChecked;
			DebugOptionModel.Instance.Save();
		}

		private void CbEnableTrace_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.EnableTrace = (bool)CbEnableTrace.IsChecked;
			DebugOptionModel.Instance.Save();
		}

		private void CbEnableTracer_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.EnableTracer = (bool)CbEnableTracer.IsChecked;
			DebugOptionModel.Instance.Save();
		}

		private void CbEnableTracer_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.EnableTracer = (bool)CbEnableTracer.IsChecked;
			DebugOptionModel.Instance.Save();
		}


		private void CbPersistentValidation_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.PersistentValidation = (bool)CbPersistentValidation.IsChecked;
			DebugOptionModel.Instance.Save();
		}

		private void CbPersistentValidation_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.PersistentValidation = (bool)CbPersistentValidation.IsChecked;
			DebugOptionModel.Instance.Save();
		}

		private void CbEnableDiagnosticsLog_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.EnableDiagnosticsLog = (bool)CbEnableDiagnosticsLog.IsChecked;
			DebugOptionModel.Instance.Save();
		}

		private void CbEnableDiagnosticsLog_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.EnableDiagnosticsLog = (bool)CbEnableDiagnosticsLog.IsChecked;
			DebugOptionModel.Instance.Save();
		}

		private void TxtLogFile_TextChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.LogFile = TxtLogFile.Text;
			DebugOptionModel.Instance.Save();
		}

		private void CbEnableFbDiagnostics_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.EnableFbDiagnostics = (bool)CbEnableFbDiagnostics.IsChecked;
			DebugOptionModel.Instance.Save();
		}

		private void CbEnableFbDiagnostics_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.EnableFbDiagnostics = (bool)CbEnableFbDiagnostics.IsChecked;
			DebugOptionModel.Instance.Save();
		}

		private void TxtFbLogFile_TextChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugOptionModel.Instance.FbLogFile = TxtFbLogFile.Text;
			DebugOptionModel.Instance.Save();
		}
	}
}
