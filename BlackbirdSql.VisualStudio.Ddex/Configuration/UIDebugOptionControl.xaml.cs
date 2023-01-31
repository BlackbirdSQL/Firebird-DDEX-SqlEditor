using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static BlackbirdSql.VisualStudio.Ddex.Configuration.VsOptionsProvider;

namespace BlackbirdSql.VisualStudio.Ddex.Configuration
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class UIDebugOptionControl : UserControl
	{
		public UIDebugOptionControl()
		{
			InitializeComponent();
		}
		internal UIDebugOptionDialogPage DebugOptionDialogPage;

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
