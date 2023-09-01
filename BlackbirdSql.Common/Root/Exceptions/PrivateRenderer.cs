#region Assembly Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;
using System.Windows.Forms;



namespace BlackbirdSql.Common.Exceptions
{
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
}
