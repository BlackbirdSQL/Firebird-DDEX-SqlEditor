// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.FileStreamReader
using System;
using System.Data.SqlTypes;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Ctl;

public class FileStreamReader : IFileStreamReader, IDisposable
{
	public const int DEFAULT_BUFFER_SIZE = 8192;

	private byte[] m_buf;

	private int m_iBufLen;

	private FileStreamWrapper _FsWriter;

	protected short[] m_arrI16;

	protected int[] m_arrI32;

	protected long[] m_arrI64;

	protected char[] m_arrChar;

	protected double[] m_arrDouble;

	protected float[] m_arrFloat;

	public FileStreamReader()
	{
		m_iBufLen = 0;
	}

	public void Dispose()
	{
		if (_FsWriter != null)
		{
			_FsWriter.Dispose();
			_FsWriter = null;
		}
	}

	protected void AssureBufferLength(int iNewBufLen)
	{
		if (iNewBufLen > m_iBufLen)
		{
			m_iBufLen = iNewBufLen;
			m_buf = new byte[m_iBufLen];
		}
	}

	public void Init(string sFileName)
	{
		_FsWriter = new FileStreamWrapper();
		_FsWriter.Init(sFileName, bOpenExisting: true, 8192);
		m_iBufLen = 8192;
		m_buf = new byte[m_iBufLen];
		m_arrI16 = new short[1];
		m_arrI32 = new int[1];
		m_arrI64 = new long[1];
		m_arrChar = new char[1];
		m_arrDouble = new double[1];
		m_arrFloat = new float[1];
	}

	public int ReadInt16(long i64Offset, bool bSkipValue, ref bool IsNull, ref short iVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			Buffer.BlockCopy(m_buf, 0, m_arrI16, 0, iLen);
			iVal = m_arrI16[0];
		}
		return num;
	}

	public int ReadInt32(long i64Offset, bool bSkipValue, ref bool IsNull, ref int iVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			Buffer.BlockCopy(m_buf, 0, m_arrI32, 0, iLen);
			iVal = m_arrI32[0];
		}
		return num;
	}

	public int ReadInt64(long i64Offset, bool bSkipValue, ref bool IsNull, ref long iVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			Buffer.BlockCopy(m_buf, 0, m_arrI64, 0, iLen);
			iVal = m_arrI64[0];
		}
		return num;
	}

	public int ReadByte(long i64Offset, bool bSkipValue, ref bool IsNull, ref byte bVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			bVal = m_buf[0];
		}
		return num;
	}

	public int ReadChar(long i64Offset, bool bSkipValue, ref bool IsNull, ref char chVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			Buffer.BlockCopy(m_buf, 0, m_arrChar, 0, iLen);
			chVal = m_arrChar[0];
		}
		return num;
	}

	public int ReadBoolean(long i64Offset, bool bSkipValue, ref bool IsNull, ref bool bVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			if (m_buf[0] == 1)
			{
				bVal = true;
			}
			else
			{
				bVal = false;
			}
		}
		return num;
	}

	public int ReadSingle(long i64Offset, bool bSkipValue, ref bool IsNull, ref float singleVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			Buffer.BlockCopy(m_buf, 0, m_arrFloat, 0, iLen);
			singleVal = m_arrFloat[0];
		}
		return num;
	}

	public int ReadDouble(long i64Offset, bool bSkipValue, ref bool IsNull, ref double dVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			Buffer.BlockCopy(m_buf, 0, m_arrDouble, 0, iLen);
			dVal = m_arrDouble[0];
		}
		return num;
	}

	public int ReadSqlDecimal(long i64Offset, bool bSkipValue, ref bool IsNull, ref SqlDecimal sqlDecimalValue)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			int[] array = new int[(iLen - 3) / 4];
			Buffer.BlockCopy(m_buf, 3, array, 0, iLen - 3);
			sqlDecimalValue = new SqlDecimal(m_buf[0], m_buf[1], (1 == m_buf[2]), array);
		}
		return num;
	}

	public int ReadDecimal(long i64Offset, bool bSkipValue, ref bool IsNull, ref decimal decimalVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		IsNull = iLen == 0;
		if (bSkipValue | IsNull)
		{
			num += iLen;
		}
		else
		{
			IsNull = false;
			num += _FsWriter.ReadData(m_buf, iLen);
			int[] array = new int[iLen / 4];
			Buffer.BlockCopy(m_buf, 0, array, 0, iLen);
			decimalVal = new decimal(array);
		}
		return num;
	}

	public int ReadDateTime(long i64Offset, bool bSkipValue, ref bool IsNull, ref DateTime dtVal)
	{
		long iVal = 0L;
		int result = ReadInt64(i64Offset, bSkipValue, ref IsNull, ref iVal);
		if (!bSkipValue && !IsNull)
		{
			dtVal = new DateTime(iVal);
		}
		return result;
	}

	public int ReadTimeSpan(long i64Offset, bool bSkipValue, ref bool IsNull, ref TimeSpan timeSpan)
	{
		long iVal = 0L;
		int result = ReadInt64(i64Offset, bSkipValue, ref IsNull, ref iVal);
		if (!bSkipValue && !IsNull)
		{
			timeSpan = new TimeSpan(iVal);
		}
		return result;
	}

	public int ReadDateTimeOffset(long i64Offset, bool bSkipValue, ref bool IsNull, ref DateTimeOffset dtoVal)
	{
		long iVal = 0L;
		long iVal2 = 0L;
		int num = ReadInt64(i64Offset, bSkipValue, ref IsNull, ref iVal);
		if (num > 0 && !IsNull)
		{
			num += ReadInt64(i64Offset + num, bSkipValue, ref IsNull, ref iVal2);
			if (!bSkipValue && !IsNull)
			{
				dtoVal = new DateTimeOffset(new DateTime(iVal), new TimeSpan(iVal2));
			}
		}
		return num;
	}

	public int ReadString(long i64Offset, bool bSkipValue, ref bool IsNull, ref string sVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		if (bSkipValue)
		{
			num += iLen;
		}
		else if (iLen == 0)
		{
			if (5 == num)
			{
				IsNull = false;
				sVal = string.Empty;
			}
			else
			{
				IsNull = true;
			}
		}
		else
		{
			IsNull = false;
			AssureBufferLength(iLen);
			num += _FsWriter.ReadData(m_buf, iLen);
			char[] array = new char[iLen / 2];
			Buffer.BlockCopy(m_buf, 0, array, 0, iLen);
			sVal = new string(array);
		}
		return num;
	}

	public int ReadBytes(long i64Offset, bool bSkipValue, ref bool IsNull, ref byte[] bytesVal)
	{
		int num = ReadLength(i64Offset, out int iLen);
		if (bSkipValue)
		{
			num += iLen;
		}
		else if (iLen == 0)
		{
			if (5 == num)
			{
				IsNull = false;
				bytesVal = new byte[0];
			}
			else
			{
				IsNull = true;
			}
		}
		else
		{
			IsNull = false;
			bytesVal = new byte[iLen];
			num += _FsWriter.ReadData(bytesVal, iLen);
		}
		return num;
	}

	internal int ReadLength(long i64Offset, out int iLen)
	{
		int num = _FsWriter.ReadData(m_buf, 1, i64Offset);
		if (m_buf[0] != byte.MaxValue)
		{
			iLen = Convert.ToInt32(m_buf[0]);
		}
		else
		{
			num += _FsWriter.ReadData(m_buf, 4);
			Buffer.BlockCopy(m_buf, 0, m_arrI32, 0, 4);
			iLen = m_arrI32[0];
		}
		return num;
	}

	public void Flush()
	{
		_FsWriter.FlushBuffer();
	}
}
