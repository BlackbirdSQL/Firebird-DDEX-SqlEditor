#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


using System;
using System.Collections;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Model.IO;


public sealed class QEReaderDataStorage : IBsQEStorage, IBsDataStorage, IDisposable
{
	private const int C_ColumnSizeIndex = 2;

	private long _RowCount;

	private readonly ArrayList _ColumnInfoArray;

	private bool _DataStorageEnabled = true;

	private StorageDataReader _StorageReader;

	private bool _IsClosed = true;

	private int _MaxCharsToStore = -1;

	private int _MaxXmlCharsToStore = -1;

	public StorageDataReader StorageReader => _StorageReader;

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

	public event StorageNotifyDelegate StorageNotifyEventAsync;

	public QEReaderDataStorage()
	{
		_RowCount = 0L;
		_ColumnInfoArray = [];
		_IsClosed = true;
		_DataStorageEnabled = true;
	}

	public async Task<bool> StartConsumingDataWithoutStoringAsync(CancellationToken canceltoken)
	{
		// Evs.Trace(GetType(), "QEDiskDataStorage.StartConsumingDataWithoutStoring", "", null);
		if (_StorageReader == null)
		{
			InvalidOperationException ex = new(Resources.ExStorageNoReader);
			Diag.Dug(ex);
			throw ex;
		}

		await ConsumeDataWithoutStoringAsync(canceltoken);

		return !canceltoken.Cancelled();
	}

	public void Dispose()
	{
		InitiateStopStoringData();
	}

	public IBsStorageView GetStorageView()
	{
		QEStorageViewOnReader qEStorageViewOnReader = new QEStorageViewOnReader(this);
		if (MaxCharsToStore > 0)
		{
			qEStorageViewOnReader.MaxNumBytesToDisplay = MaxCharsToStore / C_ColumnSizeIndex;
		}

		return qEStorageViewOnReader;
	}

	public IBsSortView GetSortView()
	{
		Exception ex = new NotImplementedException();
		Diag.ThrowException(ex);
		return null;
	}

	public long RowCount => _RowCount;


	public int ColumnCount => _ColumnInfoArray.Count;


	public IBsColumnInfo GetColumnInfo(int iCol)
	{
		return (IBsColumnInfo)_ColumnInfoArray[iCol];
	}

	public bool IsClosed()
	{
		return _IsClosed;
	}

	public async Task<bool> InitStorageAsync(IDataReader reader, CancellationToken cancelToken)
	{
		if (reader == null)
		{
			ArgumentNullException ex = new("reader");
			Diag.Dug(ex);
			throw ex;
		}

		_StorageReader = new StorageDataReader(reader);

		// Evs.Trace(GetType(), nameof(InitStorage), "ASYNC GetSchemaTableAsync()");

		await _StorageReader.GetSchemaTableAsync(cancelToken);

		if (cancelToken.Cancelled())
			return false;

		int fieldCount = _StorageReader.FieldCount;

		for (int i = 0; i < fieldCount; i++)
		{
			ColumnInfo columnInfo = new ColumnInfo();
			await columnInfo.InitializeAsync(_StorageReader, i, cancelToken);

			if (cancelToken.Cancelled())
				return false;

			_ColumnInfoArray.Add(columnInfo);
		}

		return true;
	}

	public async Task<bool> StartStoringDataAsync(CancellationToken cancelToken)
	{
		if (_StorageReader == null)
		{
			InvalidOperationException ex = new(Resources.ExStorageNoReader);
			Diag.Dug(ex);
			throw ex;
		}

		if (!_IsClosed)
		{
			InvalidOperationException ex = new(Resources.ExStorageAlreadyStoring);
			Diag.Dug(ex);
			throw ex;
		}

		_IsClosed = false;

		await GetDataFromReaderAsync(cancelToken);

		return !cancelToken.Cancelled();
	}

	public void InitiateStopStoringData()
	{
		_DataStorageEnabled = false;
	}

	private async Task<bool> ConsumeDataWithoutStoringAsync(CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), "QEReaderDataStorage.ConsumeDataWithoutStoring", "", null);
		_IsClosed = false;

		while (_DataStorageEnabled && !cancelToken.Cancelled()
			&& await _StorageReader.ReadAsync(cancelToken));

		_DataStorageEnabled = false;
		await OnStorageNotifyAsync(-1L, true, cancelToken);
		_IsClosed = true;

		return !cancelToken.Cancelled();
	}

	private async Task<bool> GetDataFromReaderAsync(CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), Tracer.EnLevel.Verbose, "QEReaderDataStorage.GetDataFromReader", "_DataStorageEnabled = {0}", _DataStorageEnabled);
		while (_DataStorageEnabled && !cancelToken.Cancelled()
			&& await _StorageReader.ReadAsync(cancelToken))
		{
			Interlocked.Increment(ref _RowCount);
			await OnStorageNotifyAsync(_RowCount, false, cancelToken);
		}

		_DataStorageEnabled = false;
		await OnStorageNotifyAsync(_RowCount, true, cancelToken);
		_IsClosed = true;

		return !cancelToken.Cancelled();
	}

	private async Task<bool> OnStorageNotifyAsync(long i64RowsInStorage, bool bStoredAllData, CancellationToken cancelToken)
	{
		return await StorageNotifyEventAsync?.Invoke(i64RowsInStorage, bStoredAllData, cancelToken);
	}
}
