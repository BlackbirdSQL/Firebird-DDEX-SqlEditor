#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing.Printing;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Controls.Grid
{
	public class GridPrintDocument : PrintDocument
	{
		private readonly GridControl.GridPrinter m_gridPrinter;

		private GridPrintDocument()
		{
		}

		public GridPrintDocument(GridControl.GridPrinter gridPrinter)
		{
			m_gridPrinter = gridPrinter;
		}

		protected override void OnPrintPage(PrintPageEventArgs ev)
		{
			base.OnPrintPage(ev);
			m_gridPrinter.PrintPage(ev);
		}
	}
}
