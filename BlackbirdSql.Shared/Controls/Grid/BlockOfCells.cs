// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.BlockOfCells

using System;



namespace BlackbirdSql.Shared.Controls.Grid;


public sealed class BlockOfCells
{
	private int m_X = -1;

	private long m_Y = -1L;

	private int m_Right = -1;

	private long m_Bottom = -1L;

	private int m_OriginalX = -1;

	private long m_OriginalY = -1L;

	public long LastUpdatedRow
	{
		get
		{
			if (m_Y == m_OriginalY)
			{
				return m_Bottom;
			}

			return m_Y;
		}
	}

	public int LastUpdatedCol
	{
		get
		{
			if (m_X == m_OriginalX)
			{
				return m_Right;
			}

			return m_X;
		}
	}

	public bool IsEmpty
	{
		get
		{
			if (m_X != -1)
			{
				return m_Y == -1;
			}

			return true;
		}
	}

	public int X
	{
		get
		{
			return m_X;
		}
		set
		{
			m_X = value;
		}
	}

	public long Y
	{
		get
		{
			return m_Y;
		}
		set
		{
			m_Y = value;
		}
	}

	public int Right => m_Right;

	public long Bottom => m_Bottom;

	public int Width
	{
		get
		{
			if (IsEmpty)
			{
				return 0;
			}

			return m_Right - m_X + 1;
		}
		set
		{
			if (value <= 0)
			{
				m_X = m_Right = -1;
			}
			else
			{
				m_Right = m_X + value - 1;
			}
		}
	}

	public long Height
	{
		get
		{
			if (IsEmpty)
			{
				return 0L;
			}

			return m_Bottom - m_Y + 1;
		}
		set
		{
			if (value <= 0)
			{
				m_Y = m_Bottom = -1L;
			}
			else
			{
				m_Bottom = m_Y + value - 1;
			}
		}
	}

	public int OriginalX => m_OriginalX;

	public long OriginalY => m_OriginalY;

	public BlockOfCells(long nRowIndex, int nColIndex)
	{
		InitNewBlock(nRowIndex, nColIndex);
	}

	public bool Contains(long nRowIndex, int nColIndex)
	{
		if (nColIndex >= m_X && nColIndex <= m_Right && nRowIndex >= m_Y)
		{
			return nRowIndex <= m_Bottom;
		}

		return false;
	}

	public void SetOriginalCell(long rowIndex, int columnIndex)
	{
		if (!Contains(rowIndex, columnIndex))
		{
			ArgumentException ex = new("", "rowIndex or columnIndex");
			Diag.Ex(ex);
			throw ex;
		}

		if (!IsEmpty)
		{
			_ = Width;
			_ = Height;
			m_OriginalY = rowIndex;
			m_OriginalX = columnIndex;
		}
	}

	public void UpdateBlock(long nRowIndex, int nColIndex)
	{
		if (IsEmpty)
		{
			InitNewBlock(nRowIndex, nColIndex);
			return;
		}

		if (nRowIndex < m_OriginalY)
		{
			m_Bottom = m_OriginalY;
			m_Y = nRowIndex;
		}
		else
		{
			m_Y = m_OriginalY;
			m_Bottom = nRowIndex;
		}

		if (nColIndex < m_OriginalX)
		{
			m_Right = m_OriginalX;
			m_X = nColIndex;
		}
		else
		{
			m_X = m_OriginalX;
			m_Right = nColIndex;
		}
	}

	private BlockOfCells()
	{
	}

	private void InitNewBlock(long nRowIndex, int nColIndex)
	{
		m_OriginalX = m_X = m_Right = nColIndex;
		m_OriginalY = m_Y = m_Bottom = nRowIndex;
	}
}
