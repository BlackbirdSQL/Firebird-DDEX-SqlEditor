// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.WatermarkState

using System.Windows;
using System.Windows.Controls;

namespace BlackbirdSql.Common.Controls.Wpf;


public class WatermarkState
{
	public Control Control { get; set; }

	public bool IsInitialized { get; set; }

	public WatermarkAdorner Adorner { get; set; }

	public object Image { get; set; }

	public TextBox TextBox { get; set; }

	public string HintText { get; set; }

	public Thickness OriginalPadding { get; set; }

	public bool ShowToolTip { get; set; }

	public bool ShowImageOnRight { get; set; }

	public bool HasActualText { get; set; }

	public WatermarkState(Control control)
	{
		Control = control;
		Image = null;
		HintText = string.Empty;
		IsInitialized = false;
		TextBox = null;
		ShowToolTip = false;
		ShowImageOnRight = false;
	}
}
