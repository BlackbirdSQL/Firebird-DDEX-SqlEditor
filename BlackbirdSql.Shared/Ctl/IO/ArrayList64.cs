// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.ArrayList64

using System;
using System.Collections;



namespace BlackbirdSql.Shared.Ctl.IO;


internal class ArrayList64
{
	protected ArrayList m_arr32;

	protected long _Count;

	protected ArrayList m_arrArr;

	internal long Count => _Count;

	public ArrayList64()
	{
		m_arr32 = [];
		_Count = 0L;
	}

	internal long Add(object val)
	{
		if (_Count <= int.MaxValue)
		{
			m_arr32.Add(val);
		}
		else
		{
			m_arrArr ??=
				[
					m_arr32
				];
			int num = (int)(_Count / int.MaxValue);
			ArrayList arrayList;
			if (m_arrArr.Count <= num)
			{
				arrayList = [];
				m_arrArr.Add(arrayList);
			}
			else
			{
				arrayList = (ArrayList)m_arrArr[num];
			}
			arrayList.Add(val);
		}
		return ++_Count;
	}

	internal void RemoveAt(long index)
	{
		if (_Count <= int.MaxValue)
		{
			int index2 = Convert.ToInt32(index);
			m_arr32.RemoveAt(index2);
		}
		else
		{
			int num = (int)(index / int.MaxValue);
			ArrayList obj = (ArrayList)m_arrArr[num];
			int index3 = (int)(index % int.MaxValue);
			obj.RemoveAt(index3);
			int num2 = (int)(_Count / int.MaxValue);
			for (int i = num + 1; i < num2; i++)
			{
				ArrayList obj2 = (ArrayList)m_arrArr[i - 1];
				ArrayList arrayList = (ArrayList)m_arrArr[i];
				obj2.Add(arrayList[2147483646]);
				arrayList.RemoveAt(0);
			}
		}
		_Count--;
	}

	internal object GetItem(long index)
	{
		object result = null;
		if (_Count <= int.MaxValue)
		{
			int index2 = Convert.ToInt32(index);
			result = m_arr32[index2];
		}
		else
		{
			int num = (int)(_Count / int.MaxValue);
			if (m_arrArr.Count > num)
			{
				ArrayList arrayList = (ArrayList)m_arrArr[num];
				int num2 = (int)(_Count % int.MaxValue);
				if (arrayList.Count > num2)
				{
					result = arrayList[num2];
				}
			}
		}
		return result;
	}
}
