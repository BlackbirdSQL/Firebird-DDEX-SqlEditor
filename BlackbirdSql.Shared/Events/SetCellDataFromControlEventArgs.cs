// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.SetCellDataFromControlEventArgs

using System;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Events;


public delegate void SetCellDataFromControlEventHandler(object sender, SetCellDataFromControlEventArgs e);


public class SetCellDataFromControlEventArgs : EventArgs
{
	private readonly int m_RowNum;

	private readonly int m_ColNum;

	private readonly IBsGridEmbeddedControl m_Control;

	private bool m_Valid;

	public int RowIndex => m_RowNum;

	public int ColumnIndex => m_ColNum;

	public IBsGridEmbeddedControl Control => m_Control;

	public bool Valid
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
