#region Assembly Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.DataStorage.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Threading;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Shared.Ctl.IO;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Events;

namespace BlackbirdSql.Shared.Model.IO;

internal class SortView : IBsSortView, IDisposable
{
	protected IBsStorageView m_StorageView;

	protected IBsSortingArray m_SortingArray;

	protected ArrayList m_SortingKeys;

	protected ArrayList m_KeyValues;

	protected int m_iCurrentRow;

	protected bool m_bKeepSortingData;

	protected Thread m_sortingThread;

	public event StorageNotifyDelegate StorageNotifyEventAsync;

	protected SortView()
	{
		Exception ex = new(Resources.ExceptionSortViewDefaultConstructorCannotBeUsed);
		Diag.Ex(ex);
		throw ex;
	}

	public SortView(IBsStorageView view)
	{
		m_StorageView = view;
		m_SortingArray = new SortingArray();
		m_SortingKeys = [];
		m_KeyValues = [];
		m_iCurrentRow = -1;
		m_bKeepSortingData = false;
		m_sortingThread = null;
	}

	public virtual void Dispose()
	{
		StopSortingData();
		if (m_StorageView != null)
		{
			m_StorageView.Dispose();
			m_StorageView = null;
		}
	}

	public virtual void ResetSortKeys()
	{
		StopSortingData();
		m_SortingKeys.Clear();
	}

	public virtual void AddSortKey(int iCol)
	{
		if (m_bKeepSortingData)
		{
			Exception ex = new(Resources.ExceptionAlreadySortingData);
			Diag.Ex(ex);
			throw ex;
		}

		m_SortingKeys.Add(iCol);
	}

	public void StartSortingData()
	{
		if (m_bKeepSortingData)
		{
			Exception ex = new(Resources.ExceptionAlreadySortingData);
			Diag.Ex(ex);
			throw ex;
		}

		m_SortingArray.ResetElements();
		m_iCurrentRow = -1;
		m_bKeepSortingData = true;
		m_sortingThread = new Thread(SortData);
		m_sortingThread.Start();
	}

	public void StopSortingData()
	{
		m_bKeepSortingData = false;
		if (m_sortingThread != null && m_sortingThread.IsAlive)
		{
			Thread.Sleep(50);
		}
	}

	internal virtual void SortData()
	{
		while (m_bKeepSortingData)
		{
			bool closedflag = m_StorageView.IsStorageClosed;
			if (GetNextRowNumber())
			{
				InsertNextRow();

				StorageNotifyEventAsync?.Invoke(m_SortingArray.RowCount, false, default).AwaiterResult();
			}
			else
			{
				if (closedflag)
				{
					break;
				}

				Thread.Sleep(250);
			}
		}

		m_bKeepSortingData = false;

		StorageNotifyEventAsync?.Invoke(m_SortingArray.RowCount, true, default).AwaiterResult();
	}

	public virtual bool GetNextRowNumber()
	{
		bool result = false;
		if (m_StorageView.RowCount > m_iCurrentRow + 1)
		{
			m_iCurrentRow++;
			result = true;
		}

		return result;
	}

	internal void InsertNextRow()
	{
		int num2 = 0;
		int num4 = 0;
		int num;
		if (m_iCurrentRow == 0)
		{
			num = -1;
			num4 = 0;
		}
		else
		{
			m_KeyValues.Clear();
			for (int i = 0; i < m_SortingKeys.Count; i++)
			{
				object cellData = m_StorageView.GetCellData(m_iCurrentRow, (int)m_SortingKeys[i]);
				m_KeyValues.Add(cellData);
			}

			num = 0;
			int num3 = m_SortingArray.NumGroups();
			if (num3 == 1)
			{
				num = CompareWithGroupAt(0);
				num4 = num > 0 ? 1 : 0;
			}
			else
			{
				num3--;
				while (num2 <= num3)
				{
					num4 = (num2 + num3) / 2;
					num = CompareWithGroupAt(num4);
					if (num < 0)
					{
						num3 = num4 - 1;
					}
					else
					{
						if (num <= 0)
						{
							break;
						}

						num2 = num4 + 1;
					}

					if (num3 - num2 == 0L)
					{
						num = CompareWithGroupAt(num3);
						num4 = num > 0 ? num3 + 1 : num3;
						break;
					}
				}
			}
		}

		if (num == 0)
		{
			m_SortingArray.InsertWith(num4, m_iCurrentRow);
		}
		else
		{
			m_SortingArray.InsertAt(num4, m_iCurrentRow);
		}
	}

	internal int CompareWithGroupAt(int iGroup)
	{
		int num = 0;
		int num2 = (int)m_SortingArray.GetGroupAt(iGroup)[0];
		for (int i = 0; i < m_SortingKeys.Count; i++)
		{
			object cellData = m_StorageView.GetCellData(num2, (int)m_SortingKeys[i]);
			num = CompareKeyColumns(m_KeyValues[i], cellData, (int)m_SortingKeys[i]);
			if (num != 0)
			{
				break;
			}
		}

		return num;
	}

	public virtual int CompareKeyColumns(object oKey1, object oKey2, int iCol)
	{
		int num = 0;
		if (oKey1 == null && oKey2 == null)
		{
			num = 0;
		}
		else if (oKey1 == null)
		{
			num = -1;
		}
		else if (oKey2 == null)
		{
			num = 1;
		}
		else
		{
			switch (m_StorageView.GetColumnInfo(iCol).FieldType.ToString())
			{
				case "System.String":
					num = string.Compare((string)oKey1, (string)oKey2, StringComparison.Ordinal);
					break;
				case "System.Int16":
					if ((short)oKey1 < (short)oKey2)
					{
						num = -1;
					}
					else if ((short)oKey1 > (short)oKey2)
					{
						num = 1;
					}

					break;
				case "System.Int32":
					if ((int)oKey1 < (int)oKey2)
					{
						num = -1;
					}
					else if ((int)oKey1 > (int)oKey2)
					{
						num = 1;
					}

					break;
				case "System.Int64":
					if ((long)oKey1 < (long)oKey2)
					{
						num = -1;
					}
					else if ((long)oKey1 > (long)oKey2)
					{
						num = 1;
					}

					break;
				case "System.Char":
					if ((char)oKey1 < (char)oKey2)
					{
						num = -1;
					}
					else if ((char)oKey1 > (char)oKey2)
					{
						num = 1;
					}

					break;
				case "System.Byte":
					if ((byte)oKey1 < (byte)oKey2)
					{
						num = -1;
					}
					else if ((byte)oKey1 > (byte)oKey2)
					{
						num = 1;
					}

					break;
				case "System.Double":
					if ((double)oKey1 < (double)oKey2)
					{
						num = -1;
					}
					else if ((double)oKey1 > (double)oKey2)
					{
						num = 1;
					}

					break;
				case "System.Decimal":
					if ((decimal)oKey1 < (decimal)oKey2)
					{
						num = -1;
					}
					else if ((decimal)oKey1 > (decimal)oKey2)
					{
						num = 1;
					}

					break;
				case "System.DateTime":
					if ((DateTime)oKey1 < (DateTime)oKey2)
					{
						num = -1;
					}
					else if ((DateTime)oKey1 > (DateTime)oKey2)
					{
						num = 1;
					}

					break;
				case "System.Boolean":
					if (!(bool)oKey1 && (bool)oKey2)
					{
						num = -1;
					}
					else if ((bool)oKey1 && !(bool)oKey2)
					{
						num = 1;
					}

					break;
				case "System.Byte[]":
					{
						byte[] array = (byte[])oKey1;
						byte[] array2 = (byte[])oKey2;
						int num2 = array.Length < array2.Length ? array.Length : array2.Length;
						for (int i = 0; i < num2; i++)
						{
							if (array[i] < array2[i])
							{
								num = -1;
								break;
							}

							if (array[i] > array2[i])
							{
								num = 1;
								break;
							}
						}

						if (num == 0)
						{
							if (array.Length < array2.Length)
							{
								num = -1;
							}
							else if (array.Length > array2.Length)
							{
								num = 1;
							}
						}

						break;
					}
				default:
					{
						string strA = oKey1.ToString();
						string strB = oKey2.ToString();
						num = string.Compare(strA, strB, StringComparison.Ordinal);
						break;
					}
			}
		}

		return num;
	}

	public int RowCount
	{
		get
		{
			int result = 0;
			if (m_SortingArray != null)
			{
				result = m_SortingArray.RowCount;
			}

			return result;
		}
	}

	public void DeleteRow(int iRelativeRow, bool fDeleteFromStorage)
	{
		int num = 0;
		if (fDeleteFromStorage)
		{
			num = GetAbsoluteRowNumber(iRelativeRow);
		}

		m_SortingArray.DeleteElementAt(iRelativeRow);
		if (!fDeleteFromStorage)
		{
			return;
		}

		m_StorageView.DeleteRow(Convert.ToInt64(num));
		for (int i = 0; i < RowCount; i++)
		{
			int num2 = (int)m_SortingArray.GetElementAt(i);
			if (num < num2)
			{
				m_SortingArray.SetElementAt(i, num2 - 1);
			}
		}
	}

	public int GetAbsoluteRowNumber(int iRelativeRow)
	{
		int result = -1;
		if (m_SortingArray != null && m_SortingArray.RowCount > iRelativeRow)
		{
			result = (int)m_SortingArray.GetElementAt(iRelativeRow);
		}

		return result;
	}
}
