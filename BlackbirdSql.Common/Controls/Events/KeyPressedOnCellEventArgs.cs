// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.KeyPressedOnCellEventArgs

using System;
using System.Windows.Forms;


namespace BlackbirdSql.Common.Controls.Events;

public class KeyPressedOnCellEventArgs : EventArgs
{
	private readonly long m_RowIndex;

	private readonly int m_ColumnIndex;

	private readonly Keys m_Key;

	private readonly Keys m_Modifiers;

	public long RowIndex => m_RowIndex;

	public int ColumnIndex => m_ColumnIndex;

	public Keys Key => m_Key;

	public Keys Modifiers => m_Modifiers;

	public KeyPressedOnCellEventArgs(long nCurRow, int nCurCol, Keys k, Keys m)
	{
		m_RowIndex = nCurRow;
		m_ColumnIndex = nCurCol;
		m_Key = k;
		m_Modifiers = m;
	}

	protected KeyPressedOnCellEventArgs()
	{
	}
}
