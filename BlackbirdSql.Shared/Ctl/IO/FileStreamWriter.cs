// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.FileStreamWriter
using System;
using System.Data.SqlTypes;
using BlackbirdSql.Shared.Interfaces;

namespace BlackbirdSql.Shared.Ctl.IO;

public class FileStreamWriter : IBFileStreamWriter, IDisposable
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

	public FileStreamWriter()
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
		Init(sFileName, bOpenExisting: false);
	}

	public void Init(string sFileName, bool bOpenExisting)
	{
		_FsWriter = new FileStreamWrapper();
		_FsWriter.Init(sFileName, bOpenExisting, 8192);
		m_iBufLen = 8192;
		m_buf = new byte[m_iBufLen];
		m_arrI16 = new short[1];
		m_arrI32 = new int[1];
		m_arrI64 = new long[1];
		m_arrChar = new char[1];
		m_arrDouble = new double[1];
		m_arrFloat = new float[1];
	}

	public int WriteNull()
	{
		m_buf[0] = 0;
		return _FsWriter.WriteData(m_buf, 1);
	}

	public int WriteInt16(short iVal)
	{
		m_buf[0] = 2;
		m_arrI16[0] = iVal;
		Buffer.BlockCopy(m_arrI16, 0, m_buf, 1, 2);
		return _FsWriter.WriteData(m_buf, 3);
	}

	public int WriteInt32(int iVal)
	{
		m_buf[0] = 4;
		m_arrI32[0] = iVal;
		Buffer.BlockCopy(m_arrI32, 0, m_buf, 1, 4);
		return _FsWriter.WriteData(m_buf, 5);
	}

	public int WriteInt32(long i64Offset, int iVal)
	{
		m_buf[0] = 4;
		m_arrI32[0] = iVal;
		Buffer.BlockCopy(m_arrI32, 0, m_buf, 1, 4);
		return _FsWriter.WriteDataDirect(i64Offset, m_buf, 5);
	}

	public int WriteInt64(long iVal)
	{
		m_buf[0] = 8;
		m_arrI64[0] = iVal;
		Buffer.BlockCopy(m_arrI64, 0, m_buf, 1, 8);
		return _FsWriter.WriteData(m_buf, 9);
	}

	public int WriteChar(char chVal)
	{
		m_buf[0] = 2;
		m_arrChar[0] = chVal;
		Buffer.BlockCopy(m_arrChar, 0, m_buf, 1, 2);
		return _FsWriter.WriteData(m_buf, 3);
	}

	public int WriteBoolean(bool bVal)
	{
		m_buf[0] = 1;
		if (bVal)
		{
			m_buf[1] = 1;
		}
		else
		{
			m_buf[1] = 0;
		}
		return _FsWriter.WriteData(m_buf, 2);
	}

	public int WriteByte(byte bVal)
	{
		m_buf[0] = 1;
		m_buf[1] = bVal;
		return _FsWriter.WriteData(m_buf, 2);
	}

	public int WriteSingle(float singleVal)
	{
		m_buf[0] = 4;
		m_arrFloat[0] = singleVal;
		Buffer.BlockCopy(m_arrFloat, 0, m_buf, 1, 4);
		return _FsWriter.WriteData(m_buf, 5);
	}

	public int WriteDouble(double dVal)
	{
		m_buf[0] = 8;
		m_arrDouble[0] = dVal;
		Buffer.BlockCopy(m_arrDouble, 0, m_buf, 1, 8);
		return _FsWriter.WriteData(m_buf, 9);
	}

	public int WriteSqlDecimal(SqlDecimal sqlDecimalValue)
	{
		int[] data = sqlDecimalValue.Data;
		int num = 3 + data.Length * 4;
		int num2 = WriteLength(num);
		m_buf[0] = sqlDecimalValue.Precision;
		m_buf[1] = sqlDecimalValue.Scale;
		m_buf[2] = (byte)(sqlDecimalValue.IsPositive ? 1u : 0u);
		Buffer.BlockCopy(data, 0, m_buf, 3, num - 3);
		return num2 + _FsWriter.WriteData(m_buf, num);
	}

	public int WriteDecimal(decimal decimalVal)
	{
		int[] bits = decimal.GetBits(decimalVal);
		int num = bits.Length * 4;
		int num2 = WriteLength(num);
		Buffer.BlockCopy(bits, 0, m_buf, 0, num);
		return num2 + _FsWriter.WriteData(m_buf, num);
	}

	public int WriteDateTime(DateTime dtVal)
	{
		return WriteInt64(dtVal.Ticks);
	}

	public int WriteDateTimeOffset(DateTimeOffset dtoVal)
	{
		return WriteInt64(dtoVal.Ticks) + WriteInt64(dtoVal.Offset.Ticks);
	}

	public int WriteTimeSpan(TimeSpan timeSpan)
	{
		return WriteInt64(timeSpan.Ticks);
	}

	public int WriteString(string sVal)
	{
		if (sVal.Length == 0)
		{
			int num = 5;
			AssureBufferLength(num);
			m_buf[0] = byte.MaxValue;
			m_buf[1] = 0;
			m_buf[2] = 0;
			m_buf[3] = 0;
			m_buf[4] = 0;
			return _FsWriter.WriteData(m_buf, num);
		}
		int num2 = sVal.Length * 2;
		int num3 = WriteLength(num2);
		AssureBufferLength(num2);
		Buffer.BlockCopy(sVal.ToCharArray(), 0, m_buf, 0, num2);
		return num3 + _FsWriter.WriteData(m_buf, num2);
	}

	public int WriteBytes(byte[] bytesVal, int iLen)
	{
		if (iLen == 0)
		{
			iLen = 5;
			AssureBufferLength(iLen);
			m_buf[0] = byte.MaxValue;
			m_buf[1] = 0;
			m_buf[2] = 0;
			m_buf[3] = 0;
			m_buf[4] = 0;
			return _FsWriter.WriteData(m_buf, iLen);
		}
		int num = WriteLength(iLen);
		return num + _FsWriter.WriteData(bytesVal, iLen);
	}

	internal int WriteLength(int iLen)
	{
		if (iLen < 255)
		{
			int value = iLen & 0xFF;
			m_buf[0] = Convert.ToByte(value);
			return _FsWriter.WriteData(m_buf, 1);
		}
		m_buf[0] = byte.MaxValue;
		m_arrI32[0] = iLen;
		Buffer.BlockCopy(m_arrI32, 0, m_buf, 1, 4);
		return _FsWriter.WriteData(m_buf, 5);
	}

	public int ReadLength(long i64Offset, out int iLen)
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

	public void FlushBuffer()
	{
		_FsWriter.FlushBuffer();
	}
}
