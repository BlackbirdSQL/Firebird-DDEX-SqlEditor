// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.FileStreamWrapper

using System;
using System.IO;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Ctl.IO;


public class FileStreamWrapper : IBsFileStreamWrapper, IDisposable
{
	public const int DEFAULT_BUFFER_SIZE = 8192;

	private bool m_bInitialized;

	private byte[] m_buf;

	private int m_iBufferSize;

	private int m_iBufferDataSize;

	private FileStream m_fs;

	private long m_i64StartOffset;

	private long _CurrentOffset;

	public FileStreamWrapper()
	{
		m_bInitialized = false;
		m_iBufferSize = 0;
		m_iBufferDataSize = 0;
		m_i64StartOffset = 0L;
		_CurrentOffset = 0L;
	}

	public void Dispose()
	{
		if (m_fs != null)
		{
			m_fs.Close();
			m_fs = null;
		}
	}

	public void Init(string sFileName, bool bOpenExisting)
	{
		Init(sFileName, bOpenExisting, 8192);
	}

	public void Init(string sFileName, bool bOpenExisting, int iBufferSize)
	{
		if (m_bInitialized)
		{
			throw new Exception(Resources.ExFileStreamAlreadyInitialized);
		}
		if (iBufferSize <= 0)
		{
			throw new Exception(Resources.ExBufferSizeShouldBePositive);
		}
		m_iBufferSize = iBufferSize;
		m_fs = new FileStream(sFileName, bOpenExisting ? FileMode.Open : FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, m_iBufferSize, useAsync: false);
		FileInfo fileInfo = new FileInfo(sFileName);
		if (fileInfo.Exists)
		{
			fileInfo.Attributes |= FileAttributes.Hidden;
		}
		m_buf = new byte[m_iBufferSize];
		m_bInitialized = true;
	}

	protected internal void MoveTo(long i64Offset)
	{
		if (m_iBufferSize > m_iBufferDataSize || i64Offset < m_i64StartOffset || i64Offset >= m_i64StartOffset + m_iBufferSize)
		{
			m_i64StartOffset = i64Offset;
			m_fs.Seek(m_i64StartOffset, SeekOrigin.Begin);
			m_iBufferDataSize = m_fs.Read(m_buf, 0, m_iBufferSize);
		}
		_CurrentOffset = i64Offset;
	}

	public int ReadData(byte[] buf, int iBytes)
	{
		return ReadData(buf, iBytes, _CurrentOffset);
	}

	public int ReadData(byte[] buf, int iBytes, long i64Offset)
	{
		if (!m_bInitialized)
		{
			throw new Exception(Resources.ExFileStreamNeedsToBeInitializedFirst);
		}
		MoveTo(i64Offset);
		int num = 0;
		while (num < iBytes)
		{
			int num2 = (int)(_CurrentOffset - m_i64StartOffset);
			int num3 = iBytes - num;
			if (num3 > m_iBufferDataSize - num2)
			{
				num3 = m_iBufferDataSize - num2;
			}
			Buffer.BlockCopy(m_buf, num2, buf, num, num3);
			num += num3;
			if (num < iBytes && m_iBufferDataSize == m_iBufferSize)
			{
				MoveTo(m_i64StartOffset + m_iBufferSize);
			}
			else
			{
				_CurrentOffset += num3;
			}
		}
		return num;
	}

	public int WriteData(byte[] buf, int iBytes)
	{
		if (!m_bInitialized)
		{
			throw new Exception(Resources.ExFileStreamNeedsToBeInitializedFirst);
		}
		int num = 0;
		while (num < iBytes)
		{
			int num2 = (int)(_CurrentOffset - m_i64StartOffset);
			int num3 = iBytes - num;
			if (num3 > m_iBufferSize - num2)
			{
				num3 = m_iBufferSize - num2;
			}
			Buffer.BlockCopy(buf, num, m_buf, num2, num3);
			num += num3;
			_CurrentOffset += num3;
			if (num < iBytes)
			{
				FlushBuffer();
			}
		}
		return num;
	}

	public int WriteDataDirect(long i64Offset, byte[] buf, int iBytes)
	{
		long position = m_fs.Position;
		int num = 0;
		try
		{
			m_fs.Seek(i64Offset, SeekOrigin.Begin);
			m_fs.Write(buf, num, iBytes);
			m_fs.Flush();
			return iBytes;
		}
		finally
		{
			m_fs.Position = position;
		}
	}

	public void FlushBuffer()
	{
		int num = (int)(_CurrentOffset - m_i64StartOffset);
		m_fs.Write(m_buf, 0, num);
		m_i64StartOffset += num;
		m_fs.Flush();
	}
}
