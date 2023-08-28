// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.ColumnReorderRequestedEventArgs

using System;



namespace BlackbirdSql.Common.Controls.Events;

public class ColumnReorderRequestedEventArgs : EventArgs
{
	private readonly int m_colIndex = -1;

	private bool m_bAllowReorder;

	public int ColumnIndex => m_colIndex;

	public bool AllowReorder
	{
		get
		{
			return m_bAllowReorder;
		}
		set
		{
			m_bAllowReorder = value;
		}
	}

	public ColumnReorderRequestedEventArgs(int nColumnIndex, bool reordableByDefault)
	{
		m_colIndex = nColumnIndex;
		m_bAllowReorder = reordableByDefault;
	}
}
