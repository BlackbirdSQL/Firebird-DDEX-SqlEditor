#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;

namespace BlackbirdSql.Shared.Controls.Grid;
// namespace Microsoft.SqlServer.Management.UI.Grid


public sealed class SelectionManager
{
	private EnGridSelectionType m_selType = EnGridSelectionType.SingleCell;

	private bool m_bOnlyOneSelItem = true;

	private readonly BlockOfCellsCollection m_selBlocks = [];

	private int m_curSelBlockIndex = -1;

	private long m_curRowIndex = -1L;

	private int m_curColIndex = -1;

	public bool SingleRowOrColumnSelectedInMultiSelectionMode
	{
		get
		{
			if (m_selBlocks.Count == 1)
			{
				if (m_selBlocks[0].Height != 1 || m_selType != EnGridSelectionType.RowBlocks)
				{
					if (m_selBlocks[0].Width == 1)
					{
						return m_selType == EnGridSelectionType.ColumnBlocks;
					}

					return false;
				}

				return true;
			}

			return false;
		}
	}

	public long LastUpdatedRow
	{
		get
		{
			if (m_curSelBlockIndex == -1)
			{
				return -1L;
			}

			return m_selBlocks[m_curSelBlockIndex].LastUpdatedRow;
		}
	}

	public int LastUpdatedColumn
	{
		get
		{
			if (m_curSelBlockIndex == -1)
			{
				return -1;
			}

			return m_selBlocks[m_curSelBlockIndex].LastUpdatedCol;
		}
	}

	public int CurrentSelectionBlockIndex => m_curSelBlockIndex;

	public long CurrentRow
	{
		get
		{
			return m_curRowIndex;
		}
		set
		{
			m_curRowIndex = value;
		}
	}

	public int CurrentColumn
	{
		get
		{
			return m_curColIndex;
		}
		set
		{
			m_curColIndex = value;
		}
	}

	public EnGridSelectionType SelectionType
	{
		get
		{
			return m_selType;
		}
		set
		{
			if (m_selType != value)
			{
				m_selType = value;
				m_bOnlyOneSelItem = m_selType == EnGridSelectionType.SingleCell || m_selType == EnGridSelectionType.SingleColumn || m_selType == EnGridSelectionType.SingleRow;
			}
		}
	}

	public bool OnlyOneCellSelected
	{
		get
		{
			if (SelectedBlocks.Count != 1)
			{
				return false;
			}

			if (SelectedBlocks[0].Width == 1)
			{
				return SelectedBlocks[0].Height == 1;
			}

			return false;
		}
	}

	public bool OnlyOneSelItem => m_bOnlyOneSelItem;

	public BlockOfCellsCollection SelectedBlocks => m_selBlocks;

	public void Clear()
	{
		Clear(bClearCurrentCell: true);
	}

	public void Clear(bool bClearCurrentCell)
	{
		m_selBlocks.Clear();
		ResetCurrentBlock();
		if (bClearCurrentCell)
		{
			m_curColIndex = -1;
			m_curRowIndex = -1L;
		}
	}

	public void ResetCurrentBlock()
	{
		m_curSelBlockIndex = -1;
	}

	public void StartNewBlock(long nRowIndex, int nColIndex)
	{
		m_curRowIndex = nRowIndex;
		m_curColIndex = nColIndex;
		BlockOfCells node = new BlockOfCells(nRowIndex, nColIndex);
		if (m_bOnlyOneSelItem)
		{
			m_selBlocks.Clear();
		}

		m_curSelBlockIndex = m_selBlocks.Add(node);
	}

	public void UpdateCurrentBlock(long nRowIndex, int nColIndex)
	{
		if (m_bOnlyOneSelItem || m_selBlocks.Count == 0 || m_curSelBlockIndex == -1)
		{
			StartNewBlock(nRowIndex, nColIndex);
		}
		else
		{
			m_selBlocks[m_curSelBlockIndex].UpdateBlock(nRowIndex, nColIndex);
		}
	}

	public bool StartNewBlockOrExcludeCell(long nRowIndex, int nColIndex)
	{
		int selBlockIndexForCell = GetSelBlockIndexForCell(nRowIndex, nColIndex);
		if (selBlockIndexForCell == -1 || m_bOnlyOneSelItem || m_selType == EnGridSelectionType.CellBlocks)
		{
			StartNewBlock(nRowIndex, nColIndex);
			return true;
		}

		ResetCurrentBlock();
		while (selBlockIndexForCell != -1)
		{
			if (m_selType == EnGridSelectionType.ColumnBlocks)
			{
				SplitOneColumnsBlock(nRowIndex, nColIndex, selBlockIndexForCell);
			}
			else
			{
				SplitOneRowsBlock(nRowIndex, nColIndex, selBlockIndexForCell);
			}

			selBlockIndexForCell = GetSelBlockIndexForCell(nRowIndex, nColIndex);
		}

		if (m_selBlocks.Count == 0)
		{
			m_curRowIndex = -1L;
			m_curColIndex = -1;
		}

		return false;
	}

	public bool IsCellSelected(long nRowIndex, int nColIndex)
	{
		foreach (BlockOfCells selBlock in m_selBlocks)
		{
			if (IsCellSelectedInt(selBlock, nRowIndex, nColIndex))
			{
				return true;
			}
		}

		return false;
	}

	public int GetSelecttionBlockNumberForCell(long nRowIndex, int nColIndex)
	{
		for (int i = 0; i < m_selBlocks.Count; i++)
		{
			if (IsCellSelectedInt(m_selBlocks[i], nRowIndex, nColIndex))
			{
				return i;
			}
		}

		return -1;
	}

	public bool SetCurrentCell(long rowIndex, int columnIndex)
	{
		if (m_curSelBlockIndex == -1)
		{
			return false;
		}

		BlockOfCells blockOfCells = m_selBlocks[m_curSelBlockIndex];
		if (!blockOfCells.Contains(rowIndex, columnIndex))
		{
			return false;
		}

		m_curColIndex = columnIndex;
		m_curRowIndex = rowIndex;
		blockOfCells.SetOriginalCell(rowIndex, columnIndex);
		return true;
	}

	private int GetSelBlockIndexForCell(long nRowIndex, int nColIndex)
	{
		for (int i = 0; i < m_selBlocks.Count; i++)
		{
			if (IsCellSelectedInt(m_selBlocks[i], nRowIndex, nColIndex))
			{
				return i;
			}
		}

		return -1;
	}

	private void SplitOneColumnsBlock(long nRowIndex, int nColIndex, int nIndexOfBlock)
	{
		BlockOfCells blockOfCells = m_selBlocks[nIndexOfBlock];
		if (blockOfCells.Width == 1)
		{
			m_selBlocks.RemoveAt(nIndexOfBlock);
			return;
		}

		if (blockOfCells.X == nColIndex)
		{
			blockOfCells.X++;
			return;
		}

		if (blockOfCells.Right == nColIndex)
		{
			blockOfCells.Width--;
			return;
		}

		m_selBlocks.RemoveAt(nIndexOfBlock);
		BlockOfCells blockOfCells2 = new BlockOfCells(blockOfCells.Y, blockOfCells.X);
		blockOfCells2.UpdateBlock(blockOfCells.Bottom, nColIndex - 1);
		m_selBlocks.Add(blockOfCells2);
		blockOfCells2 = new BlockOfCells(blockOfCells.Y, nColIndex + 1);
		blockOfCells2.UpdateBlock(blockOfCells.Bottom, blockOfCells.Right);
		m_selBlocks.Add(blockOfCells2);
	}

	private void SplitOneRowsBlock(long nRowIndex, int nColIndex, int nIndexOfBlock)
	{
		BlockOfCells blockOfCells = m_selBlocks[nIndexOfBlock];
		if (blockOfCells.Height == 1)
		{
			m_selBlocks.RemoveAt(nIndexOfBlock);
			return;
		}

		if (blockOfCells.Y == nRowIndex)
		{
			blockOfCells.Y++;
			return;
		}

		if (blockOfCells.Bottom == nRowIndex)
		{
			blockOfCells.Height--;
			return;
		}

		m_selBlocks.RemoveAt(nIndexOfBlock);
		BlockOfCells blockOfCells2 = new BlockOfCells(blockOfCells.Y, blockOfCells.X);
		blockOfCells2.UpdateBlock(nRowIndex - 1, blockOfCells.Right);
		m_selBlocks.Add(blockOfCells2);
		blockOfCells2 = new BlockOfCells(nRowIndex + 1, blockOfCells.X);
		blockOfCells2.UpdateBlock(blockOfCells.Bottom, blockOfCells.Right);
		m_selBlocks.Add(blockOfCells2);
	}

	private bool IsCellSelectedInt(BlockOfCells block, long nRowIndex, int nColIndex)
	{
		if (m_selType == EnGridSelectionType.CellBlocks || m_selType == EnGridSelectionType.SingleCell)
		{
			if (block.Contains(nRowIndex, nColIndex))
			{
				return true;
			}
		}
		else if (m_selType == EnGridSelectionType.ColumnBlocks || m_selType == EnGridSelectionType.SingleColumn)
		{
			if (nColIndex >= block.X && nColIndex <= block.Right)
			{
				return true;
			}
		}
		else if (nRowIndex >= block.Y && nRowIndex <= block.Bottom)
		{
			return true;
		}

		return false;
	}
}
