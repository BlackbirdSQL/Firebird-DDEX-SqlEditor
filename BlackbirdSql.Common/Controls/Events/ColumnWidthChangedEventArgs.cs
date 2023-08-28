// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.ColumnWidthChangedEventArgs

using System;



namespace BlackbirdSql.Common.Controls.Events;

public class ColumnWidthChangedEventArgs : EventArgs
{
	private readonly int m_ColumnIndex;

	private readonly int m_NewColumnWidth;

	public int ColumnIndex => m_ColumnIndex;

	public int NewColumnWidth => m_NewColumnWidth;

	public ColumnWidthChangedEventArgs(int nColIndex, int nNewColWidth)
	{
		m_ColumnIndex = nColIndex;
		m_NewColumnWidth = nNewColWidth;
	}

	protected ColumnWidthChangedEventArgs()
	{
	}
}
