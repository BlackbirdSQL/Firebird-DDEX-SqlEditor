// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.SelectionChangedEventArgs

using System;
using BlackbirdSql.Common.Ctl;


namespace BlackbirdSql.Common.Controls.Events;

public class SelectionChangedEventArgs : EventArgs
{
	private readonly BlockOfCellsCollection m_SelectedBlocks;

	public BlockOfCellsCollection SelectedBlocks => m_SelectedBlocks;

	public SelectionChangedEventArgs(BlockOfCellsCollection blocks)
	{
		m_SelectedBlocks = blocks;
	}

	protected SelectionChangedEventArgs()
	{
	}
}
