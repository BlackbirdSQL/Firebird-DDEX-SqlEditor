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

namespace EdmxTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource sH_CONSIGNMENTTYPEViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("sH_CONSIGNMENTTYPEViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // sH_CONSIGNMENTTYPEViewSource.Source = [generic data source]
        }
    }
}
