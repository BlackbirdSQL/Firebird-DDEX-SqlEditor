// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.ProgressStrip

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;




namespace BlackbirdSql.Wpf.Widgets
{
	/// <summary>
	/// Interaction logic for ProgressStrip.xaml
	/// </summary>
	public partial class ProgressStrip : UserControl, IComponentConnector
	{
		private const double C_MinWidth = 100.0;

		public static readonly DependencyProperty ShowProgressProperty = DependencyProperty.Register("ShowProgress", typeof(bool), typeof(ProgressStrip), new PropertyMetadata(false, OnShowProgressChanged));

		public static readonly DependencyProperty ProgressBackgroundProperty = DependencyProperty.Register("ProgressBackground", typeof(Brush), typeof(ProgressStrip));

		public bool ShowProgress
		{
			get
			{
				return (bool)GetValue(ShowProgressProperty);
			}
			set
			{
				SetValue(ShowProgressProperty, value);
			}
		}

		public Brush ProgressBackground
		{
			get
			{
				return (Brush)GetValue(ProgressBackgroundProperty);
			}
			set
			{
				SetValue(ProgressBackgroundProperty, value);
			}
		}



		public ProgressStrip()
		{
			InitializeComponent();
			innerProgress.Visibility = Visibility.Hidden;
			Binding binding = new Binding("Background")
			{
				Source = this
			};
			SetBinding(ProgressBackgroundProperty, binding);
		}

		private static void OnShowProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ProgressStrip progressStrip)
			{
				progressStrip.innerProgress.Visibility = !(bool)e.NewValue ? Visibility.Hidden : Visibility.Visible;
				progressStrip.innerProgress.IsIndeterminate = (bool)e.NewValue;
			}
		}

		protected override Size MeasureOverride(Size constraint)
		{
			return base.MeasureOverride(new Size(C_MinWidth, constraint.Height));
		}


	}
}
