// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Spinner

using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;




namespace BlackbirdSql.Wpf.Widgets
{
	/// <summary>
	/// Interaction logic for Spinner.xaml
	/// </summary>
	public partial class Spinner : UserControl, IComponentConnector
	{
		private readonly Storyboard storyboardRotate;

		private bool Paused { get; set; }




		public Spinner()
		{
			InitializeComponent();
			storyboardRotate = (Storyboard)base.Resources["storyboardRotate"];
		}




		public void Pause()
		{
			storyboardRotate.Pause(this);
			Paused = true;
		}

		public void Resume()
		{
			storyboardRotate.Resume(this);
			Paused = false;
		}

		private void OnCanvasLoaded(object sender, RoutedEventArgs e)
		{
			storyboardRotate.Begin(this, isControllable: true);
			if (Paused)
			{
				Pause();
			}
		}
	}
}
