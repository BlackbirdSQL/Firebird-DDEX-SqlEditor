// Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.DiskDataStorage

using System;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Ctl.IO;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Threading;

namespace BlackbirdSql.Shared.Model.IO;

internal abstract class AbstractDiskDataStorage : IBsDiskDataStorage, IBsDataStorage, IDisposable
{
	protected string _FileName;

	protected long _RowCount;

	protected long _CurrentOffset;

	protected ArrayList64 _OffsetsArray;

	protected ArrayList _ColumnInfoArray;

	protected StorageDataReader _StorageReader;

	protected IBsFileStreamWriter _FsWriter;

	protected bool _DataStorageEnabled;

	protected EnLauncherPayloadLaunchState _AsyncWorkerState = EnLauncherPayloadLaunchState.Inactive;

	private CancellationTokenSource _AsyncWorkerCancelTokenSource = null;
	private CancellationToken _AsyncWorkerCancelToken;

	private Task<bool> _AsyncWorkerTask;

	protected int _MaxCharsToStore = 65535;

	protected int _MaxXmlCharsToStore = 2097152;

	protected bool _HasBlobs;

	private readonly StorageDataEntity _DiskDataEntity;

	public int MaxCharsToStore
	{
		get
		{
			return _MaxCharsToStore;
		}
		set
		{
			if (value < -1)
			{
				Exception ex = new ArgumentOutOfRangeException("value");
				Diag.ThrowException(ex);
			}
			_MaxCharsToStore = value;
		}
	}

	public int MaxXmlCharsToStore
	{
		get
		{
			return _MaxXmlCharsToStore;
		}
		set
		{
			if (value < 1)
			{
				Exception ex = new ArgumentOutOfRangeException("value");
				Diag.ThrowException(ex);
			}
			_MaxXmlCharsToStore = value;
		}
	}

	internal StorageDataEntity DiskDataEntity => _DiskDataEntity;

	public event StorageNotifyDelegate StorageNotifyEventAsync;

	public virtual IBsStorageView GetStorageView()
	{
		return new DiskStorageView(this);
	}

	public IBsSortView GetSortView()
	{
		return new SortView(GetStorageView());
	}

	public AbstractDiskDataStorage()
	{
		_RowCount = 0L;
		_CurrentOffset = 0L;
		_DataStorageEnabled = false;

		_AsyncWorkerCancelTokenSource?.Dispose();
		_AsyncWorkerCancelTokenSource = null;
		_AsyncWorkerTask = null;

		_OffsetsArray = new ArrayList64();
		_ColumnInfoArray = [];
		_DiskDataEntity = new StorageDataEntity();
	}

	public virtual void Dispose()
	{
		StopStoringData();
		if (_FsWriter != null)
		{
			_FsWriter.Dispose();
			_FsWriter = null;
			File.Delete(_FileName);
		}
	}

	public virtual async Task<bool> InitStorageAsync(IDataReader storageReader, CancellationToken cancelToken)
	{
		if (storageReader == null)
		{
			throw new Exception(Resources.ExceptionReaderCannotBeNull);
		}

		_StorageReader = new StorageDataReader(storageReader);

		_FileName = Path.GetTempFileName();

		if (_FileName.Length == 0)
		{
			throw new Exception(Resources.ExceptionFailedToGetTempFileName);
		}
		_FsWriter = new FileStreamWriter();
		_FsWriter.Init(_FileName);

		for (int i = 0; i < _StorageReader.FieldCount; i++)
		{
			ColumnInfo columnInfo = new ColumnInfo();
			await columnInfo.InitializeAsync(_StorageReader, i, cancelToken);

			if (cancelToken.Cancelled())
				return false;

			_ColumnInfoArray.Add(columnInfo);
			_HasBlobs |= columnInfo.IsBlobField;
		}

		return true;
	}

	public virtual async Task<bool> AddToStorageAsync(IDataReader storageReader, CancellationToken cancelToken)
	{
		if (_StorageReader == null)
		{
			await InitStorageAsync(storageReader, cancelToken);

			if (cancelToken.Cancelled())
				return false;

			return true;
		}
		if (storageReader == null)
		{
			throw new ArgumentException(Resources.ExceptionReaderCannotBeNull);
		}
		if (storageReader.FieldCount != _ColumnInfoArray.Count)
		{
			throw new ArgumentException(Resources.ExceptionColumnsCountDoesNotMatch);
		}
		for (int i = 0; i < storageReader.FieldCount; i++)
		{
			IBsColumnInfo columnInfo = GetColumnInfo(i);
			if (columnInfo.ColumnName != storageReader.GetName(i) || columnInfo.DataTypeName != storageReader.GetDataTypeName(i))
			{
				throw new ArgumentException(Resources.ExceptionColumnsDoNotMatch);
			}
		}
		if (_DataStorageEnabled)
		{
			throw new Exception(Resources.ExceptionCannotAddReaderWhenStoringData);
		}
		_StorageReader = new StorageDataReader(storageReader);

		return true;
	}

	public async Task<bool> StartStoringDataAsync(CancellationToken cancelToken)
	{
		if (_DataStorageEnabled)
			throw new Exception(Resources.ExceptionAlreadyStoringData);

		_DataStorageEnabled = true;


		_AsyncWorkerCancelToken = default;
		_AsyncWorkerCancelTokenSource?.Dispose();
		_AsyncWorkerCancelTokenSource = new();
		_AsyncWorkerCancelToken = _AsyncWorkerCancelTokenSource.Token;

		// The following for brevity.
		TaskCreationOptions creationOptions = TaskCreationOptions.PreferFairness
			| TaskCreationOptions.AttachedToParent;

		Task<bool> payloadAsync() =>
			SerializeDataAsync(_AsyncWorkerCancelToken);

		// Fire and remember

		_AsyncWorkerTask = Task.Factory.StartNew(payloadAsync, default, creationOptions, TaskScheduler.Default).Unwrap();

		return await Task.FromResult(true);

	}

	public virtual void StopStoringData()
	{
		_DataStorageEnabled = false;

		if (_AsyncWorkerTask == null || _AsyncWorkerTask.IsCompleted
			|| _AsyncWorkerCancelTokenSource == null
			|| _AsyncWorkerCancelTokenSource.IsCancellationRequested)
		{
			return;
		}

		_AsyncWorkerCancelTokenSource.Cancel();

		Thread.Sleep(50);
	}

	public async Task<bool> StoreDataAsync(CancellationToken cancelToken)
	{
		if (_DataStorageEnabled)
		{
			throw new InvalidOperationException(Resources.ExceptionAlreadyStoringData);
		}

		if (_StorageReader == null)
		{
			throw new InvalidOperationException(Resources.ExceptionStorageNotInitialized);
		}

		_DataStorageEnabled = true;

		await SerializeDataAsync(cancelToken);

		return !cancelToken.Cancelled();
	}

	public virtual async Task<bool> SerializeDataAsync(CancellationToken cancelToken)
	{
		Type type = null;
		IBsColumnInfo columnInfo = null;


		object[] array = new object[_ColumnInfoArray.Count];

		while (_DataStorageEnabled && !cancelToken.Cancelled() && await _StorageReader.ReadAsync(cancelToken))
		{ 
			_OffsetsArray.Add(_CurrentOffset);
			if (!_HasBlobs)
			{
				_StorageReader.GetValues(array);
			}
			for (int i = 0; i < _ColumnInfoArray.Count && !cancelToken.Cancelled(); i++)
			{
				if (_HasBlobs)
				{
					if (_StorageReader.IsDBNull(i))
					{
						array[i] = DBNull.Value;
					}
					else
					{
						columnInfo = GetColumnInfo(i);
						if (!columnInfo.IsBlobField)
						{
							if (columnInfo.FieldType == typeof(string))
								array[i] = await _StorageReader.GetValueAsync<string>(i, cancelToken);
							else
								array[i] = _StorageReader.GetValue(i);
						}
						else if (columnInfo.IsBytesField)
						{
							array[i] = await _StorageReader.GetBytesWithMaxCapacityAsync(i, MaxCharsToStore, cancelToken);
						}
						else if (columnInfo.IsCharsField)
						{
							array[i] = await _StorageReader.GetCharsWithMaxCapacityAsync(i, MaxCharsToStore, cancelToken);
						}
						else if (columnInfo.IsXml)
						{
							array[i] = _StorageReader.GetXmlWithMaxCapacity(i, MaxXmlCharsToStore, () => _DataStorageEnabled);
						}
						else
						{
							array[i] = _StorageReader.GetValue(i);
						}
					}
				}

				if (cancelToken.Cancelled())
					break;

				type = array[i] != null ? array[i].GetType() : DiskDataEntity.TypeDbNull;
				if (DiskDataEntity.TypeDbNull == type)
				{
					_CurrentOffset += _FsWriter.WriteNull();
					continue;
				}
				if (((IBsColumnInfo)_ColumnInfoArray[i]).IsSqlVariant)
				{
					DiskDataEntity.StringValue = type.ToString();
					_CurrentOffset += _FsWriter.WriteString(DiskDataEntity.StringValue);
					DiskDataEntity.StringValue = null;
				}
				if (DiskDataEntity.TypeString == type)
				{
					DiskDataEntity.StringValue = (string)array[i];
					_CurrentOffset += _FsWriter.WriteString(DiskDataEntity.StringValue);
					DiskDataEntity.StringValue = null;
				}
				else if (DiskDataEntity.TypeSqlString == type)
				{
					DiskDataEntity.SqlStringValue = (SqlString)array[i];
					if (DiskDataEntity.SqlStringValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteString(DiskDataEntity.SqlStringValue.Value);
					}
					DiskDataEntity.SqlStringValue = null;
				}
				else if (DiskDataEntity.TypeInt16 == type)
				{
					DiskDataEntity.Int16Value = (short)array[i];
					_CurrentOffset += _FsWriter.WriteInt16(DiskDataEntity.Int16Value);
				}
				else if (DiskDataEntity.TypeSqlInt16 == type)
				{
					DiskDataEntity.SqlInt16Value = (SqlInt16)array[i];
					if (DiskDataEntity.SqlInt16Value.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteInt16(DiskDataEntity.SqlInt16Value.Value);
					}
				}
				else if (DiskDataEntity.TypeInt32 == type)
				{
					DiskDataEntity.Int32Value = (int)array[i];
					_CurrentOffset += _FsWriter.WriteInt32(DiskDataEntity.Int32Value);
				}
				else if (DiskDataEntity.TypeSqlInt32 == type)
				{
					DiskDataEntity.SqlInt32Value = (SqlInt32)array[i];
					if (DiskDataEntity.SqlInt32Value.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteInt32(DiskDataEntity.SqlInt32Value.Value);
					}
				}
				else if (DiskDataEntity.TypeInt64 == type)
				{
					DiskDataEntity.Int64Value = (long)array[i];
					_CurrentOffset += _FsWriter.WriteInt64(DiskDataEntity.Int64Value);
				}
				else if (DiskDataEntity.TypeSqlInt64 == type)
				{
					DiskDataEntity.SqlInt64Value = (SqlInt64)array[i];
					if (DiskDataEntity.SqlInt64Value.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteInt64(DiskDataEntity.SqlInt64Value.Value);
					}
				}
				else if (DiskDataEntity.TypeByte == type)
				{
					DiskDataEntity.ByteValue = (byte)array[i];
					_CurrentOffset += _FsWriter.WriteByte(DiskDataEntity.ByteValue);
				}
				else if (DiskDataEntity.TypeSqlByte == type)
				{
					DiskDataEntity.SqlByteValue = (SqlByte)array[i];
					if (DiskDataEntity.SqlByteValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteByte(DiskDataEntity.SqlByteValue.Value);
					}
				}
				else if (DiskDataEntity.TypeChar == type)
				{
					DiskDataEntity.CharValue = (char)array[i];
					_CurrentOffset += _FsWriter.WriteChar(DiskDataEntity.CharValue);
				}
				else if (DiskDataEntity.TypeBool == type)
				{
					DiskDataEntity.BoolValue = (bool)array[i];
					_CurrentOffset += _FsWriter.WriteBoolean(DiskDataEntity.BoolValue);
				}
				else if (DiskDataEntity.TypeSqlBool == type)
				{
					DiskDataEntity.SqlBoolValue = (SqlBoolean)array[i];
					if (DiskDataEntity.SqlBoolValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteBoolean(DiskDataEntity.SqlBoolValue.Value);
					}
				}
				else if (DiskDataEntity.TypeDouble == type)
				{
					DiskDataEntity.DoubleValue = (double)array[i];
					_CurrentOffset += _FsWriter.WriteDouble(DiskDataEntity.DoubleValue);
				}
				else if (DiskDataEntity.TypeSqlDouble == type)
				{
					DiskDataEntity.SqlDoubleValue = (SqlDouble)array[i];
					if (DiskDataEntity.SqlDoubleValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteDouble(DiskDataEntity.SqlDoubleValue.Value);
					}
				}
				else if (DiskDataEntity.TypeSqlSingle == type)
				{
					DiskDataEntity.SqlSingleValue = (SqlSingle)array[i];
					if (DiskDataEntity.SqlSingleValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteSingle(DiskDataEntity.SqlSingleValue.Value);
					}
				}
				else if (DiskDataEntity.TypeDecimal == type)
				{
					DiskDataEntity.DecimalValue = (decimal)array[i];
					_CurrentOffset += _FsWriter.WriteDecimal(DiskDataEntity.DecimalValue);
				}
				else if (DiskDataEntity.TypeSqlDecimal == type)
				{
					DiskDataEntity.SqlDecimalValue = (SqlDecimal)array[i];
					if (DiskDataEntity.SqlDecimalValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteSqlDecimal(DiskDataEntity.SqlDecimalValue);
					}
				}
				else if (DiskDataEntity.TypeDateTime == type)
				{
					DiskDataEntity.DateTimeValue = (DateTime)array[i];
					_CurrentOffset += _FsWriter.WriteDateTime(DiskDataEntity.DateTimeValue);
				}
				else if (DiskDataEntity.TypeDateTimeOffset == type)
				{
					DiskDataEntity.DateTimeOffsetValue = (DateTimeOffset)array[i];
					_CurrentOffset += _FsWriter.WriteDateTimeOffset(DiskDataEntity.DateTimeOffsetValue);
				}
				else if (DiskDataEntity.TypeSqlDateTime == type)
				{
					DiskDataEntity.SqlDateTimeValue = (SqlDateTime)array[i];
					if (DiskDataEntity.SqlDateTimeValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						_CurrentOffset += _FsWriter.WriteDateTime(DiskDataEntity.SqlDateTimeValue.Value);
					}
				}
				else if (DiskDataEntity.TypeTimeSpan == type)
				{
					DiskDataEntity.TimeSpanValue = (TimeSpan)array[i];
					_CurrentOffset += _FsWriter.WriteTimeSpan(DiskDataEntity.TimeSpanValue);
				}
				else if (type == DiskDataEntity.TypeBytes)
				{
					DiskDataEntity.BytesValue = (byte[])array[i];
					_CurrentOffset += _FsWriter.WriteBytes(DiskDataEntity.BytesValue, DiskDataEntity.BytesValue.Length);
					DiskDataEntity.BytesValue = null;
				}
				else if (type == DiskDataEntity.TypeSqlBytes)
				{
					DiskDataEntity.SqlBytesValue = (SqlBytes)array[i];
					if (DiskDataEntity.SqlBytesValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						DiskDataEntity.BytesValue = DiskDataEntity.SqlBytesValue.Value;
						_CurrentOffset += _FsWriter.WriteBytes(DiskDataEntity.BytesValue, DiskDataEntity.BytesValue.Length);
					}
					DiskDataEntity.SqlBytesValue = null;
					DiskDataEntity.BytesValue = null;
				}
				else if (DiskDataEntity.TypeSqlBinary == type)
				{
					DiskDataEntity.SqlBinaryValue = (SqlBinary)array[i];
					if (DiskDataEntity.SqlBinaryValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
					}
					else
					{
						DiskDataEntity.BytesValue = DiskDataEntity.SqlBinaryValue.Value;
						_CurrentOffset += _FsWriter.WriteBytes(DiskDataEntity.BytesValue, DiskDataEntity.BytesValue.Length);
					}
					DiskDataEntity.SqlBinaryValue = null;
					DiskDataEntity.BytesValue = null;
				}
				else if (DiskDataEntity.TypeSqlGuid == type)
				{
					DiskDataEntity.SqlGuidValue = (SqlGuid)array[i];
					if (DiskDataEntity.SqlGuidValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
						continue;
					}
					DiskDataEntity.BytesValue = DiskDataEntity.SqlGuidValue.ToByteArray();
					_CurrentOffset += _FsWriter.WriteBytes(DiskDataEntity.BytesValue, DiskDataEntity.BytesValue.Length);
				}
				else if (DiskDataEntity.TypeSqlMoney == type)
				{
					DiskDataEntity.SqlMoneyValue = (SqlMoney)array[i];
					if (DiskDataEntity.SqlMoneyValue.IsNull)
					{
						_CurrentOffset += _FsWriter.WriteNull();
						continue;
					}
					DiskDataEntity.DecimalValue = DiskDataEntity.SqlMoneyValue.Value;
					_CurrentOffset += _FsWriter.WriteDecimal(DiskDataEntity.DecimalValue);
				}
				else
				{
					Diag.StackException("Data type not found: " + type.FullName);
					DiskDataEntity.StringValue = array[i].ToString();
					_CurrentOffset += _FsWriter.WriteString(DiskDataEntity.StringValue);
				}
			}

			if (cancelToken.Cancelled())
				break;

			_FsWriter.FlushBuffer();
			Interlocked.Increment(ref _RowCount);


			await OnStorageNotifyAsync(_RowCount, false, cancelToken);
		}
		_DataStorageEnabled = false;

		if (cancelToken.Cancelled())
			return false;

		await OnStorageNotifyAsync(_RowCount, true, cancelToken);

		return !cancelToken.Cancelled();
	}

	public void DeleteRow(long index)
	{
		_OffsetsArray.RemoveAt(index);
		Interlocked.Decrement(ref _RowCount);
	}

	public string GetFileName()
	{
		return _FileName;
	}

	public long RowCount => _RowCount;


	public long GetRowOffset(long index)
	{
		return (long)_OffsetsArray.GetItem(index);
	}

	public int ColumnCount => _ColumnInfoArray.Count;


	public IBsColumnInfo GetColumnInfo(int index)
	{
		return (IBsColumnInfo)_ColumnInfoArray[index];
	}

	public bool IsClosed()
	{
		return !_DataStorageEnabled;
	}

	protected virtual async Task<bool> OnStorageNotifyAsync(long rowCount, bool storedAllData, CancellationToken cancelToken)
	{
		return await (StorageNotifyEventAsync?.Invoke(rowCount, storedAllData, cancelToken) ?? Task.FromResult(false));
	}
}
