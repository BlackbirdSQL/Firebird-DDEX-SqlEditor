﻿#region Assembly Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.DataStorage.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Threading;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Ctl.Interfaces;

namespace BlackbirdSql.Common.Ctl;


public class SortingArray : IBSortingArray
{
	protected const int C_IncrementalBufferSize = 100;

	protected ArrayList[] m_arrGroups;

	protected int m_iGroupsBufferLength;

	protected int m_iGroups;

	protected int m_iRows;

	public SortingArray()
	{
		ResetElements();
	}

	public void ResetElements()
	{
		m_iGroupsBufferLength = C_IncrementalBufferSize;
		m_arrGroups = new ArrayList[m_iGroupsBufferLength];
		m_iGroups = 0;
		m_iRows = 0;
	}

	public void IncreaseBufferIfNeeded()
	{
		if (m_iGroups == m_iGroupsBufferLength)
		{
			ArrayList[] array = new ArrayList[m_iGroupsBufferLength + C_IncrementalBufferSize];
			m_iGroupsBufferLength += C_IncrementalBufferSize;
			Array.Copy(m_arrGroups, 0, array, 0, m_iGroups);
			m_arrGroups = array;
		}
	}

	public int InsertAt(int iGroup, object val)
	{
		if (iGroup <= m_iGroups)
		{
			IncreaseBufferIfNeeded();
			for (int num = m_iGroups; num > iGroup; num--)
			{
				m_arrGroups[num] = m_arrGroups[num - 1];
			}

			m_arrGroups[iGroup] = new ArrayList();
			Interlocked.Increment(ref m_iGroups);
			return InsertWith(iGroup, val);
		}

		Exception ex = new(ControlsResources.IncorrectGroupNumber);
		Diag.Dug(ex);
		throw ex;
	}

	public int InsertWith(int iGroup, object val)
	{
		if (iGroup < m_iGroups)
		{
			m_arrGroups[iGroup].Add(val);
			Interlocked.Increment(ref m_iRows);
			return m_iRows;
		}

		Exception ex = new(ControlsResources.IncorrectGroupNumber);
		Diag.Dug(ex);
		throw ex;
	}

	public void FindElementPosition(int iRow, ref int iGroup, ref int iIndex)
	{
		if (iRow >= m_iRows)
		{
			Exception ex = new(ControlsResources.IncorrectRowNumber);
			Diag.Dug(ex);
			throw ex;
		}

		int num = 0;
		for (iGroup = 0; iGroup < m_iGroups; iGroup++)
		{
			int num2 = num;
			num += m_arrGroups[iGroup].Count;
			if (num > iRow)
			{
				iIndex = iRow - num2;
				break;
			}
		}
	}

	public object GetElementAt(int iRow)
	{
		int iGroup = 0;
		int iIndex = 0;
		FindElementPosition(iRow, ref iGroup, ref iIndex);
		return m_arrGroups[iGroup][iIndex];
	}

	public void SetElementAt(int iRow, int iNewRowNumber)
	{
		int iGroup = 0;
		int iIndex = 0;
		FindElementPosition(iRow, ref iGroup, ref iIndex);
		m_arrGroups[iGroup][iIndex] = iNewRowNumber;
	}

	public void DeleteElementAt(int iRow)
	{
		int iGroup = 0;
		int iIndex = 0;
		FindElementPosition(iRow, ref iGroup, ref iIndex);
		ArrayList arrayList = m_arrGroups[iGroup];
		arrayList.RemoveAt(iIndex);
		if (arrayList.Count == 0)
		{
			for (int i = iGroup + 1; i < m_iGroups; i++)
			{
				m_arrGroups[i - 1] = m_arrGroups[i];
			}

			Interlocked.Decrement(ref m_iGroups);
		}

		Interlocked.Decrement(ref m_iRows);
	}

	public ArrayList GetGroupAt(int iGroup)
	{
		return m_arrGroups[iGroup];
	}

	public int RowCount => m_iRows;


	public int NumGroups()
	{
		return m_iGroups;
	}
}
