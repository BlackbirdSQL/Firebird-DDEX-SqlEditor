// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.ColumnsReorderedEventArgs

using System;



namespace BlackbirdSql.Shared.Events;

public class ColumnsReorderedEventArgs : EventArgs
{
	private readonly int m_origColumnIndex = -1;

	private readonly int m_newColumnIndex = -1;

	public int OriginalColumnIndex => m_origColumnIndex;

	public int NewColumnIndex => m_newColumnIndex;

	public ColumnsReorderedEventArgs(int origIndex, int newIndex)
	{
		m_origColumnIndex = origIndex;
		m_newColumnIndex = newIndex;
	}
}
