// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.FillControlWithDataEventArgs

using System;
using BlackbirdSql.Common.Controls.Interfaces;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Common.Controls.Events;


public class FillControlWithDataEventArgs : EventArgs
{
	private readonly int m_RowNum;

	private readonly int m_ColNum;

	private readonly IGridEmbeddedControl m_Control;

	public int RowIndex => m_RowNum;

	public int ColumnIndex => m_ColNum;

	public IGridEmbeddedControl Control => m_Control;

	public FillControlWithDataEventArgs(int nRow, int nCol, IGridEmbeddedControl control)
	{
		m_RowNum = nRow;
		m_ColNum = nCol;
		m_Control = control;
	}

	protected FillControlWithDataEventArgs()
	{
	}
}
