// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.SetCellDataFromControlEventArgs

using System;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Events;


internal delegate void SetCellDataFromControlEventHandler(object sender, SetCellDataFromControlEventArgs e);


internal class SetCellDataFromControlEventArgs : EventArgs
{
	private readonly int m_RowNum;

	private readonly int m_ColNum;

	private readonly IBsGridEmbeddedControl m_Control;

	private bool m_Valid;

	internal int RowIndex => m_RowNum;

	internal int ColumnIndex => m_ColNum;

	internal IBsGridEmbeddedControl Control => m_Control;

	internal bool Valid
	{
		get
		{
			return m_Valid;
		}
		set
		{
			m_Valid = value;
		}
	}

	public SetCellDataFromControlEventArgs(int nRow, int nCol, IBsGridEmbeddedControl control)
	{
		m_RowNum = nRow;
		m_ColNum = nCol;
		m_Control = control;
		m_Valid = true;
	}

	protected SetCellDataFromControlEventArgs()
	{
	}
}
