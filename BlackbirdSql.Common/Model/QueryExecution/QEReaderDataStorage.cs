#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


using System;
using System.Collections;
using System.Data;
using System.Threading;

using BlackbirdSql.Core;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Model.QueryExecution;


public sealed class QEReaderDataStorage : IBQEStorage, IBDataStorage, IDisposable
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
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
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
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
			}

			_MaxXmlCharsToStore = value;
		}
	}

	public event StorageNotifyDelegate StorageNotify;

	public QEReaderDataStorage()
	{
		_RowCount = 0L;
		_ColumnInfoArray = new ArrayList();
		_IsClosed = true;
		_DataStorageEnabled = true;
	}

	public void StartConsumingDataWithoutStoring()
	{
		// Tracer.Trace(GetType(), "QEDiskDataStorage.StartConsumingDataWithoutStoring", "", null);
		if (_StorageReader == null)
		{
			InvalidOperationException ex = new(QEResources.ErrQEStorageNoReader);
			Diag.Dug(ex);
			throw ex;
		}

		ConsumeDataWithoutStoring();
	}

	public void Dispose()
	{
		InitiateStopStoringData();
	}

	public IBStorageView GetStorageView()
	{
		QEStorageViewOnReader qEStorageViewOnReader = new QEStorageViewOnReader(this);
		if (MaxCharsToStore > 0)
		{
			qEStorageViewOnReader.MaxNumBytesToDisplay = MaxCharsToStore / C_ColumnSizeIndex;
		}

		return qEStorageViewOnReader;
	}

	public IBSortView GetSortView()
	{
		Exception ex = new NotImplementedException();
		Tracer.LogExThrow(GetType(), ex);
		throw ex;
	}

	public long RowCount => _RowCount;


	public int ColumnCount => _ColumnInfoArray.Count;


	public IBColumnInfo GetColumnInfo(int iCol)
	{
		return (IBColumnInfo)_ColumnInfoArray[iCol];
	}

	public bool IsClosed()
	{
		return _IsClosed;
	}

	public void InitStorage(IDataReader reader, bool textBased)
	{
		if (reader == null)
		{
			ArgumentNullException ex = new("reader");
			Diag.Dug(ex);
			throw ex;
		}

		_StorageReader = new StorageDataReader(reader, textBased ? this : null);
		_StorageReader.GetSchemaTable();
		int fieldCount = _StorageReader.FieldCount;
		for (int i = 0; i < fieldCount; i++)
		{
			_ColumnInfoArray.Add(new ColumnInfo(_StorageReader, i));
		}
	}

	public void StartStoringData()
	{
		if (_StorageReader == null)
		{
			InvalidOperationException ex = new(QEResources.ErrQEStorageNoReader);
			Diag.Dug(ex);
			throw ex;
		}

		if (!_IsClosed)
		{
			InvalidOperationException ex = new(QEResources.ErrQEStorageAlreadyStoring);
			Diag.Dug(ex);
			throw ex;
		}

		_IsClosed = false;
		GetDataFromReader();
	}

	public void InitiateStopStoringData()
	{
		_DataStorageEnabled = false;
	}

	private void ConsumeDataWithoutStoring()
	{
		// Tracer.Trace(GetType(), "QEReaderDataStorage.ConsumeDataWithoutStoring", "", null);
		_IsClosed = false;
		while (_DataStorageEnabled && _StorageReader.Read())
		{
		}

		_DataStorageEnabled = false;
		OnStorageNotify(-1L, bStoredAllData: true);
		_IsClosed = true;
	}

	private void GetDataFromReader()
	{
		// Tracer.Trace(GetType(), "QEReaderDataStorage.GetDataFromReader", "", null);
		try
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QEReaderDataStorage.GetDataFromReader", "_DataStorageEnabled = {0}", _DataStorageEnabled);
			while (_DataStorageEnabled && _StorageReader.Read())
			{
				Interlocked.Increment(ref _RowCount);
				OnStorageNotify(_RowCount, bStoredAllData: false);
			}
		}
		catch (Exception ex)
		{
			Tracer.LogExCatch(GetType(), ex);
			throw;
		}

		_DataStorageEnabled = false;
		OnStorageNotify(_RowCount, bStoredAllData: true);
		_IsClosed = true;
	}

	private void OnStorageNotify(long i64RowsInStorage, bool bStoredAllData)
	{
		StorageNotify?.Invoke(i64RowsInStorage, bStoredAllData);
	}
}
