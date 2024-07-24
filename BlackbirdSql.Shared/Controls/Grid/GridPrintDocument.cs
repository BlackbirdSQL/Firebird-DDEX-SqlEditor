// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridPrintDocument

using System.Drawing.Printing;



namespace BlackbirdSql.Shared.Controls.Grid;


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
