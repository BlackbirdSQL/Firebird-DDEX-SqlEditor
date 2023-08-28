// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.DiskStorageView
using System;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Threading;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Properties;



namespace BlackbirdSql.Common.Model;

public class DiskStorageView : AbstractStorageView, IDataReader, IDisposable, IDataRecord
{
	protected IFileStreamReader _FsReader;

	protected IFileStreamWriter _FsWriter;

	protected IDiskDataStorage _DiskDataStorage;

	protected long _CurrentOffset;

	protected long _StartRow;

	protected long _TotalRowCount;

	protected int m_iPrevCol;

	protected long m_i64PrevRow;

	protected long m_i64CurrentRow;

	private bool readerClosed;

	public int Depth => 0;

	public bool IsClosed => readerClosed;

	public int RecordsAffected => 0;

	public int FieldCount => _DiskDataStorage.ColumnCount;

	public object this[int i] => GetValue(i);

	public object this[string name] => GetValue(GetOrdinal(name));

	protected DiskStorageView()
	{
		throw new Exception(SharedResx.StorageViewDefaultConstructorCannotBeUsed);
	}

	protected internal DiskStorageView(IDiskDataStorage storage)
	{
		_DiskDataStorage = storage;
		_CurrentOffset = 0L;
		_StartRow = 0L;
		_TotalRowCount = 0L;
		m_iPrevCol = -1;
		m_i64PrevRow = 0L;
	}

	protected void InitFileReader()
	{
		if (_FsReader == null)
		{
			if (_FsWriter != null)
			{
				throw new InvalidOperationException();
			}
			_FsReader = new FileStreamReader();
			_FsReader.Init(_DiskDataStorage.GetFileName());
		}
	}

	protected void InitFileWriter()
	{
		if (_FsWriter == null)
		{
			if (_FsReader != null)
			{
				throw new InvalidOperationException();
			}
			_FsWriter = new FileStreamWriter();
			_FsWriter.Init(_DiskDataStorage.GetFileName(), bOpenExisting: true);
		}
	}

	public override void Dispose(bool disposing)
	{
		if (disposing && _FsReader != null)
		{
			_FsReader.Dispose();
			_FsReader = null;
		}
		base.Dispose(disposing);
	}

	public override long EnsureRowsInBuf(long startRow, long totalRowCount)
	{
		_StartRow = startRow;
		_TotalRowCount = totalRowCount;
		return RowCount;
	}

	public override long RowCount => _DiskDataStorage.RowCount;


	public override int ColumnCount => _DiskDataStorage.ColumnCount;


	public override IColumnInfo GetColumnInfo(int iCol)
	{
		return _DiskDataStorage.GetColumnInfo(iCol);
	}

	private void MoveToCellData(long i64Row, int iCol)
	{
		if (i64Row >= RowCount || iCol >= ColumnCount)
		{
			throw new Exception(SharedResx.InvalidArgument);
		}
		if (iCol == 0 || iCol != m_iPrevCol + 1 || i64Row != m_i64PrevRow)
		{
			_CurrentOffset = _DiskDataStorage.GetRowOffset(i64Row);
			for (int i = 0; i < iCol; i++)
			{
				SequentialReadColumn(i, bSkipValue: true);
			}
		}
		m_iPrevCol = iCol;
		m_i64PrevRow = i64Row;
	}

	public override object GetCellData(long i64Row, int iCol)
	{
		InitFileReader();
		MoveToCellData(i64Row, iCol);
		return SequentialReadColumn(iCol, bSkipValue: false);
	}

	internal object SequentialReadColumn(int iCol, bool bSkipValue)
	{
		object result = null;
		bool IsNull = false;
		IColumnInfo columnInfo = _DiskDataStorage.GetColumnInfo(iCol);
		Type type = columnInfo.FieldType;
		bool flag = columnInfo.IsSqlVariant;

		if (flag)
		{
			_CurrentOffset += _FsReader.ReadString(_CurrentOffset, bSkipValue: false, ref IsNull, ref StorageViewDataEntity.StringValue);
			if (!IsNull)
			{
				type = Type.GetType(StorageViewDataEntity.StringValue);
			}
			if (type == null && "System.Data.SqlTypes.SqlSingle" == StorageViewDataEntity.StringValue)
			{
				type = typeof(SqlSingle);
			}
		}
		if (!IsNull)
		{
			if (StorageViewDataEntity.TypeString == type)
			{
				_CurrentOffset += _FsReader.ReadString(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.StringValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.StringValue;
				}
				StorageViewDataEntity.StringValue = null;
			}
			else if (StorageViewDataEntity.TypeSqlString == type || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlString.ToString()))
			{
				_CurrentOffset += _FsReader.ReadString(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.StringValue);
				StorageViewDataEntity.SqlStringValue = StorageViewDataEntity.StringValue;
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.SqlStringValue;
				}
				StorageViewDataEntity.SqlStringValue = null;
			}
			else if (StorageViewDataEntity.TypeInt16 == type)
			{
				_CurrentOffset += _FsReader.ReadInt16(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.Int16Value);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.Int16Value;
				}
			}
			else if (StorageViewDataEntity.TypeSqlInt16 == type || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlInt16.ToString()))
			{
				_CurrentOffset += _FsReader.ReadInt16(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.Int16Value);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlInt16Value = StorageViewDataEntity.Int16Value;
					result = StorageViewDataEntity.SqlInt16Value;
				}
			}
			else if (StorageViewDataEntity.TypeInt32 == type)
			{
				_CurrentOffset += _FsReader.ReadInt32(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.Int32Value);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.Int32Value;
				}
			}
			else if (StorageViewDataEntity.TypeSqlInt32 == type || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlInt32.ToString()))
			{
				_CurrentOffset += _FsReader.ReadInt32(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.Int32Value);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlInt32Value = StorageViewDataEntity.Int32Value;
					result = StorageViewDataEntity.SqlInt32Value;
				}
			}
			else if (StorageViewDataEntity.TypeInt64 == type)
			{
				_CurrentOffset += _FsReader.ReadInt64(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.Int64Value);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.Int64Value;
				}
			}
			else if (StorageViewDataEntity.TypeSqlInt64 == type || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlInt64.ToString()))
			{
				_CurrentOffset += _FsReader.ReadInt64(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.Int64Value);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlInt64Value = StorageViewDataEntity.Int64Value;
					result = StorageViewDataEntity.SqlInt64Value;
				}
			}
			else if (StorageViewDataEntity.TypeByte == type)
			{
				_CurrentOffset += _FsReader.ReadByte(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.ByteValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.ByteValue;
				}
			}
			else if (StorageViewDataEntity.TypeSqlByte == type || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlByte.ToString()))
			{
				_CurrentOffset += _FsReader.ReadByte(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.ByteValue);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlByteValue = StorageViewDataEntity.ByteValue;
					result = StorageViewDataEntity.SqlByteValue;
				}
			}
			else if (StorageViewDataEntity.TypeChar == type)
			{
				_CurrentOffset += _FsReader.ReadChar(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.CharValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.CharValue;
				}
			}
			else if (StorageViewDataEntity.TypeBool == type)
			{
				_CurrentOffset += _FsReader.ReadBoolean(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.BoolValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.BoolValue;
				}
			}
			else if (StorageViewDataEntity.TypeSqlBool == type || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlBool.ToString()))
			{
				_CurrentOffset += _FsReader.ReadBoolean(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.BoolValue);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlBoolValue = StorageViewDataEntity.BoolValue;
					result = StorageViewDataEntity.SqlBoolValue;
				}
			}
			else if (StorageViewDataEntity.TypeDouble == type)
			{
				_CurrentOffset += _FsReader.ReadDouble(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.DoubleValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.DoubleValue;
				}
			}
			else if (StorageViewDataEntity.TypeSqlDouble == type || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlDouble.ToString()))
			{
				_CurrentOffset += _FsReader.ReadDouble(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.DoubleValue);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlDoubleValue = StorageViewDataEntity.DoubleValue;
					result = StorageViewDataEntity.SqlDoubleValue;
				}
			}
			else if (StorageViewDataEntity.TypeSqlSingle == type)
			{
				float singleVal = 0f;
				_CurrentOffset += _FsReader.ReadSingle(_CurrentOffset, bSkipValue, ref IsNull, ref singleVal);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlSingleValue = singleVal;
					result = StorageViewDataEntity.SqlSingleValue;
				}
			}
			else if (StorageViewDataEntity.TypeDecimal == type)
			{
				_CurrentOffset += _FsReader.ReadDecimal(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.DecimalValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.DecimalValue;
				}
			}
			else if (StorageViewDataEntity.TypeSqlDecimal == type || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlDecimal.ToString()))
			{
				_CurrentOffset += _FsReader.ReadSqlDecimal(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.SqlDecimalValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.SqlDecimalValue;
				}
			}
			else if (StorageViewDataEntity.TypeDateTime == type)
			{
				_CurrentOffset += _FsReader.ReadDateTime(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.DateTimeValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.DateTimeValue;
				}
			}
			else if (StorageViewDataEntity.TypeDateTimeOffset == type)
			{
				_CurrentOffset += _FsReader.ReadDateTimeOffset(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.DateTimeOffsetValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.DateTimeOffsetValue;
				}
			}
			else if (StorageViewDataEntity.TypeSqlDateTime == type || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlDateTime.ToString()))
			{
				_CurrentOffset += _FsReader.ReadDateTime(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.DateTimeValue);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlDateTimeValue = StorageViewDataEntity.DateTimeValue;
					result = StorageViewDataEntity.SqlDateTimeValue;
				}
			}
			else if (StorageViewDataEntity.TypeTimeSpan == type)
			{
				_CurrentOffset += _FsReader.ReadTimeSpan(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.TimeSpanValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.TimeSpanValue;
				}
			}
			else if (type == StorageViewDataEntity.TypeBytes)
			{
				_CurrentOffset += _FsReader.ReadBytes(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.BytesValue);
				if (!IsNull && !bSkipValue)
				{
					result = ((!columnInfo.IsUdtField || StorageViewDataEntity.BytesValue.Length != 0) ? StorageViewDataEntity.BytesValue : null);
				}
				StorageViewDataEntity.BytesValue = null;
			}
			else if (type == StorageViewDataEntity.TypeSqlBytes || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlBytes.ToString()))
			{
				_CurrentOffset += _FsReader.ReadBytes(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.BytesValue);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlBytesValue = new SqlBytes(StorageViewDataEntity.BytesValue);
					result = StorageViewDataEntity.SqlBytesValue;
				}
				StorageViewDataEntity.SqlBytesValue = null;
				StorageViewDataEntity.BytesValue = null;
			}
			else if (type == StorageViewDataEntity.TypeSqlBinary || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlBinary.ToString()))
			{
				_CurrentOffset += _FsReader.ReadBytes(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.BytesValue);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlBinaryValue = new SqlBinary(StorageViewDataEntity.BytesValue);
					result = StorageViewDataEntity.SqlBinaryValue;
				}
				StorageViewDataEntity.SqlBinaryValue = null;
				StorageViewDataEntity.BytesValue = null;
			}
			else if (type == StorageViewDataEntity.TypeSqlGuid || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlGuid.ToString()))
			{
				_CurrentOffset += _FsReader.ReadBytes(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.BytesValue);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlGuidValue = new SqlGuid(StorageViewDataEntity.BytesValue);
					result = StorageViewDataEntity.SqlGuidValue;
				}
				StorageViewDataEntity.BytesValue = null;
			}
			else if (type == StorageViewDataEntity.TypeSqlMoney || (flag && StorageViewDataEntity.StringValue == StorageViewDataEntity.TypeSqlMoney.ToString()))
			{
				_CurrentOffset += _FsReader.ReadDecimal(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.DecimalValue);
				if (!IsNull && !bSkipValue)
				{
					StorageViewDataEntity.SqlMoneyValue = new SqlMoney(StorageViewDataEntity.DecimalValue);
					result = StorageViewDataEntity.SqlMoneyValue;
				}
			}
			else
			{
				_CurrentOffset += _FsReader.ReadString(_CurrentOffset, bSkipValue, ref IsNull, ref StorageViewDataEntity.StringValue);
				if (!IsNull && !bSkipValue)
				{
					result = StorageViewDataEntity.StringValue;
				}
			}
		}
		return result;
	}

	public override void DeleteRow(long iRow)
	{
		_DiskDataStorage.DeleteRow(iRow);
	}

	public override bool IsStorageClosed()
	{
		return _DiskDataStorage.IsClosed();
	}

	public void Close()
	{
		readerClosed = true;
	}

	public DataTable GetSchemaTable()
	{
		DataTable dataTable = new DataTable
		{
			Locale = CultureInfo.InvariantCulture
		};
		int num = _DiskDataStorage.ColumnCount;
		for (int i = 0; i < num; i++)
		{
			IColumnInfo columnInfo = _DiskDataStorage.GetColumnInfo(i);
			dataTable.Columns.Add(new DataColumn(columnInfo.ColumnName, columnInfo.FieldType));
		}
		return dataTable;
	}

	public bool NextResult()
	{
		return false;
	}

	public bool Read()
	{
		while (true)
		{
			if (m_i64CurrentRow < _DiskDataStorage.RowCount)
			{
				m_i64CurrentRow++;
				return true;
			}
			if (IsStorageClosed())
			{
				break;
			}
			Thread.Sleep(100);
		}
		return false;
	}

	public bool GetBoolean(int i)
	{
		return (bool)GetCellData(m_i64CurrentRow - 1, i);
	}

	public byte GetByte(int i)
	{
		return (byte)GetCellData(m_i64CurrentRow - 1, i);
	}

	public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
	{
		Buffer.BlockCopy((byte[])GetCellData(m_i64CurrentRow - 1, i), (int)fieldOffset, buffer, bufferoffset, length);
		return length;
	}

	public char GetChar(int i)
	{
		return (char)GetCellData(m_i64CurrentRow - 1, i);
	}

	public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
	{
		Buffer.BlockCopy((char[])GetCellData(m_i64CurrentRow - 1, i), (int)fieldoffset, buffer, bufferoffset, length);
		return length;
	}

	public IDataReader GetData(int i)
	{
		return null;
	}

	public DateTime GetDateTime(int i)
	{
		return (DateTime)GetCellData(m_i64CurrentRow - 1, i);
	}

	public decimal GetDecimal(int i)
	{
		return (decimal)GetCellData(m_i64CurrentRow - 1, i);
	}

	public double GetDouble(int i)
	{
		return (double)GetCellData(m_i64CurrentRow - 1, i);
	}

	public float GetFloat(int i)
	{
		return (float)GetCellData(m_i64CurrentRow - 1, i);
	}

	public Guid GetGuid(int i)
	{
		return (Guid)GetCellData(m_i64CurrentRow - 1, i);
	}

	public short GetInt16(int i)
	{
		return (short)GetCellData(m_i64CurrentRow - 1, i);
	}

	public int GetInt32(int i)
	{
		return (int)GetCellData(m_i64CurrentRow - 1, i);
	}

	public long GetInt64(int i)
	{
		return (long)GetCellData(m_i64CurrentRow - 1, i);
	}

	public string GetString(int i)
	{
		return (string)GetCellData(m_i64CurrentRow - 1, i);
	}

	public bool IsDBNull(int i)
	{
		return GetCellData(m_i64CurrentRow - 1, i) == null;
	}

	public string GetDataTypeName(int i)
	{
		return _DiskDataStorage.GetColumnInfo(i).DataTypeName;
	}

	public Type GetFieldType(int i)
	{
		return _DiskDataStorage.GetColumnInfo(i).FieldType;
	}

	public string GetName(int i)
	{
		return _DiskDataStorage.GetColumnInfo(i).ColumnName;
	}

	public int GetOrdinal(string name)
	{
		int num = _DiskDataStorage.ColumnCount;
		for (int i = 0; i < num; i++)
		{
			IColumnInfo columnInfo = _DiskDataStorage.GetColumnInfo(i);
			if (string.Compare(columnInfo.ColumnName, name, StringComparison.Ordinal) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	public object GetValue(int i)
	{
		return GetCellData(m_i64CurrentRow - 1, i);
	}

	public int GetValues(object[] values)
	{
		int num = _DiskDataStorage.ColumnCount;
		for (int i = 0; i < num; i++)
		{
			values[i] = GetValue(i);
		}
		return num;
	}

	public void WriteInt32(int i, int value)
	{
		if (m_i64CurrentRow - 1 >= RowCount || i >= ColumnCount)
		{
			throw new Exception(SharedResx.InvalidArgument);
		}
		InitFileWriter();
		_CurrentOffset = _DiskDataStorage.GetRowOffset(m_i64CurrentRow - 1);
		for (int j = 0; j < i; j++)
		{
			_CurrentOffset += _FsWriter.ReadLength(_CurrentOffset, out int iLen);
			_CurrentOffset += iLen;
		}
		_CurrentOffset += _FsWriter.WriteInt32(_CurrentOffset, value);
	}
}
