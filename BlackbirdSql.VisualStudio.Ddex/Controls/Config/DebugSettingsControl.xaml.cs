using System.Windows.Controls;
using BlackbirdSql.VisualStudio.Ddex.Model.Config;

namespace BlackbirdSql.VisualStudio.Ddex.Controls.Config
{
	/// <summary>
	/// Interaction logic for DebugSettingsControl.xaml
	/// This is depracated for now because we have implemented check boxes, and every
	/// other control we are using atm, in the standard DialogPage PropertyGrid.
	/// </summary>
	public partial class DebugSettingsControl : UserControl
	{
		public DebugSettingsControl()
		{
			InitializeComponent();
		}
		internal DebugSettingsDialogPage SettingsDialogPage;

		public void Initialize()
		{
			CbEnableTrace.IsChecked = DebugSettingsModel.Instance.EnableTrace;
			CbEnableTracer.IsChecked = DebugSettingsModel.Instance.EnableTracer;
			CbPersistentValidation.IsChecked = DebugSettingsModel.Instance.PersistentValidation;
			CbEnableDiagnosticsLog.IsChecked = DebugSettingsModel.Instance.EnableDiagnosticsLog;
			TxtLogFile.Text = DebugSettingsModel.Instance.LogFile;
			CbEnableFbDiagnostics.IsChecked = DebugSettingsModel.Instance.EnableFbDiagnostics;
			TxtFbLogFile.Text = DebugSettingsModel.Instance.FbLogFile;
			DebugSettingsModel.Instance.Save();
		}

		private void CbEnableTrace_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.EnableTrace = (bool)CbEnableTrace.IsChecked;
			DebugSettingsModel.Instance.Save();
		}

		private void CbEnableTrace_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.EnableTrace = (bool)CbEnableTrace.IsChecked;
			DebugSettingsModel.Instance.Save();
		}

		private void CbEnableTracer_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.EnableTracer = (bool)CbEnableTracer.IsChecked;
			DebugSettingsModel.Instance.Save();
		}

		private void CbEnableTracer_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.EnableTracer = (bool)CbEnableTracer.IsChecked;
			DebugSettingsModel.Instance.Save();
		}


		private void CbPersistentValidation_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.PersistentValidation = (bool)CbPersistentValidation.IsChecked;
			DebugSettingsModel.Instance.Save();
		}

		private void CbPersistentValidation_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.PersistentValidation = (bool)CbPersistentValidation.IsChecked;
			DebugSettingsModel.Instance.Save();
		}

		private void CbEnableDiagnosticsLog_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.EnableDiagnosticsLog = (bool)CbEnableDiagnosticsLog.IsChecked;
			DebugSettingsModel.Instance.Save();
		}

		private void CbEnableDiagnosticsLog_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.EnableDiagnosticsLog = (bool)CbEnableDiagnosticsLog.IsChecked;
			DebugSettingsModel.Instance.Save();
		}

		private void TxtLogFile_TextChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.LogFile = TxtLogFile.Text;
			DebugSettingsModel.Instance.Save();
		}

		private void CbEnableFbDiagnostics_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.EnableFbDiagnostics = (bool)CbEnableFbDiagnostics.IsChecked;
			DebugSettingsModel.Instance.Save();
		}

		private void CbEnableFbDiagnostics_Unchecked(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.EnableFbDiagnostics = (bool)CbEnableFbDiagnostics.IsChecked;
			DebugSettingsModel.Instance.Save();
		}

		private void TxtFbLogFile_TextChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			DebugSettingsModel.Instance.FbLogFile = TxtFbLogFile.Text;
			DebugSettingsModel.Instance.Save();
		}
	}
}
