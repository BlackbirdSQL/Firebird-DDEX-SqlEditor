// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.MyRenderer

using System.Drawing;
using System.Windows.Forms;



namespace BlackbirdSql.Core.Controls.Widgets;


public class PrivateRenderer : ToolStripSystemRenderer
{
	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		e.Graphics.Clear(SystemColors.Control);
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
	}
}
