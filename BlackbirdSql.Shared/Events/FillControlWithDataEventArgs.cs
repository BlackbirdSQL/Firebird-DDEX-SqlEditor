// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.FillControlWithDataEventHandler
// Microsoft.SqlServer.Management.UI.Grid.FillControlWithDataEventArgs

using System;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Events;


public delegate void FillControlWithDataEventHandler(object sender, FillControlWithDataEventArgs e);


public class FillControlWithDataEventArgs : EventArgs
{
	private readonly int m_RowNum;

	private readonly int m_ColNum;

	private readonly IBsGridEmbeddedControl m_Control;

	public int RowIndex => m_RowNum;

	public int ColumnIndex => m_ColNum;

	public IBsGridEmbeddedControl Control => m_Control;

	public FillControlWithDataEventArgs(int nRow, int nCol, IBsGridEmbeddedControl control)
	{
		m_RowNum = nRow;
		m_ColNum = nCol;
		m_Control = control;
	}

	protected FillControlWithDataEventArgs()
	{
	}
}
