#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model.IO;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Model.QueryExecution;


public sealed class QueryResultSet : IDisposable, IBsGridStorage
{
	public const int C_MaxCharsToStore = 10485760;

	private static readonly string _SNameOfXMLColumn = "XML_" + VS.IXMLDocumentGuid;

	private static readonly string _SNameOfJSONColumn = "JSON_" + VS.IXMLDocumentGuid;


	private IBsQEStorage _QeStorage;

	private IBsQEStorageView _QeStorageView;

	private StringCollection _ColumnNames;

	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private bool _IsStopping;

	private StorageNotifyDelegate _StorageNotifyEventAsync;

	// private long _M_curRowsNum;

	private bool _InGridMode;

	private DataTable _StorageReaderSchemaTable;

	private bool _StoredAllData;

	private IDataReader _DataReader;

	private readonly int _StatementIndex = -1;
	private readonly int _StatementCount = -1;

	private readonly MoreRowsAvailableEventArgs _CachedMoreInfoEventArgs = new MoreRowsAvailableEventArgs();

	public static string SXmlTypeNameOnServer = "xml";

	public int NumberOfDataColumns
	{
		get
		{
			if (_ColumnNames == null)
			{
				Exception ex = new InvalidOperationException();
				Diag.ThrowException(ex);
			}
			return _ColumnNames.Count;

		}
	}


	public int StatementIndex => _StatementIndex;

	public int StatementCount => _StatementCount;


	public int TotalNumberOfColumns
	{
		get
		{
			int num = NumberOfDataColumns;
			if (num > -1 && _InGridMode)
			{
				num++;
			}

			return num;
		}
	}

	public long TotalNumberOfRows
	{
		get
		{
			if (_QeStorageView != null)
			{
				return _QeStorageView.RowCount;
			}

			return 0L;
		}
	}

	public StringCollection ColumnNames => _ColumnNames;

	public bool InGridMode
	{
		get
		{
			return _InGridMode;
		}
		set
		{
			_InGridMode = value;
		}
	}

	public bool SingleColumnXmlResultSet
	{
		get
		{
			if (_ColumnNames == null)
			{
				Exception ex = new InvalidOperationException();
				Diag.ThrowException(ex);
			}

			if (_ColumnNames.Count == 1)
			{
				if (string.Compare(_ColumnNames[0], _SNameOfXMLColumn, StringComparison.Ordinal) != 0)
				{
					return string.Compare(_ColumnNames[0], _SNameOfJSONColumn, StringComparison.Ordinal) == 0;
				}

				return true;
			}

			return false;
		}
	}


	public bool IsSingleColumnXmlActualPlan
	{
		get
		{
			if (_ColumnNames == null)
			{
				Exception ex = new InvalidOperationException();
				Diag.ThrowException(ex);
			}

			if (_ColumnNames.Count == 1)
			{
				return string.Compare(_ColumnNames[0], NativeDb.XmlActualPlanColumn, StringComparison.Ordinal) == 0;
			}

			return false;

		}
	}

	public bool IsSingleColumnEstimatedPlan
	{
		get
		{
			if (_ColumnNames == null)
			{
				Exception ex = new InvalidOperationException();
				Diag.ThrowException(ex);
			}

			if (_ColumnNames.Count == 1)
			{
				return string.Compare(_ColumnNames[0], NativeDb.XmlEstimatedPlanColumn, StringComparison.Ordinal) == 0;
			}

			return false;

		}
	}

	public bool StoredAllData
	{
		get
		{
			lock (_LockLocal)
			{
				return _StoredAllData;
			}
		}
	}

	public event MoreRowsAvailableEventHandler MoreRowsAvailableEventAsync;

	private QueryResultSet()
	{
		// Evs.Trace(GetType(), ".ctor");
	}

	public QueryResultSet(IDataReader reader, int statementIndex, int statementCount) : this()
	{
		if (reader == null)
		{
			Exception ex = new ArgumentNullException("reader");
			Diag.ThrowException(ex);
		}

		_DataReader = reader;
		_StatementIndex = statementIndex;
		_StatementCount = statementCount;
	}

	public async Task<bool> InitializeAsync(bool forwardOnly, CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), nameof(InitializeAsync), "", null);

		if (_QeStorage != null)
		{
			Exception ex = new InvalidOperationException(Resources.ExResultSetAlreadyInited);
			Diag.ThrowException(ex);
		}

		StorageDataReader storageDataReader = new StorageDataReader(_DataReader);

		_StorageReaderSchemaTable = await storageDataReader.GetSchemaTableAsync(cancelToken);


		if (_IsStopping || cancelToken.Cancelled())
		{
			_IsStopping = false;

			return false;
		}

		int fieldCount = storageDataReader.FieldCount;
		_ColumnNames = [];
		for (int i = 0; i < fieldCount; i++)
		{
			_ColumnNames.Add(storageDataReader.GetName(i));
		}

		lock (_LockLocal)
		{
			if (!forwardOnly)
			{
				_QeStorage = new QEDiskDataStorage();
			}
			else
			{
				_QeStorage = new QEReaderDataStorage();
			}
		}

		await _QeStorage.InitStorageAsync(_DataReader, cancelToken);


		// _M_curRowsNum = 0L;
		_StorageNotifyEventAsync = OnStorageNotifyAsync;
		_QeStorage.StorageNotifyEventAsync += _StorageNotifyEventAsync;

		if (_IsStopping || cancelToken.Cancelled())
		{
			_IsStopping = false;
			_QeStorage.InitiateStopStoringData();
		}

		return true;
	}

	public void Dispose()
	{
		// Evs.Trace(GetType(), nameof(Dispose));
		Dispose(bDisposing: true);
	}

	private void Dispose(bool bDisposing)
	{
		// Evs.Trace(GetType(), nameof(Dispose), "bDisposing = {0}", bDisposing);
		if (_QeStorageView != null)
		{
			_QeStorageView.Dispose();
			_QeStorageView = null;
		}

		lock (_LockLocal)
		{
			if (_QeStorage != null)
			{
				if (_StorageNotifyEventAsync != null)
				{
					_QeStorage.StorageNotifyEventAsync -= _StorageNotifyEventAsync;
				}

				_QeStorage.Dispose();
				_QeStorage = null;
			}
		}

		_DataReader = null;
	}

	public bool IsSyntheticXmlColumn(int nColNum)
	{
		if (nColNum == 0 && (SingleColumnXmlResultSet || IsSingleColumnXmlActualPlan || IsSingleColumnEstimatedPlan))
		{
			return true;
		}

		return false;
	}

	public DataRow GetSchemaRow(int nColNum)
	{
		return _StorageReaderSchemaTable.Rows[nColNum];
	}

	public string GetServerDataTypeName(int columnIndex)
	{
		if (_QeStorage == null)
		{
			Exception ex = new InvalidOperationException();
			Diag.ThrowException(ex);
		}

		return _QeStorage.GetColumnInfo(columnIndex).DataTypeName;
	}

	public Type GetFieldType(int columnIndex)
	{
		if (_QeStorage == null)
		{
			Exception ex = new InvalidOperationException();
			Diag.ThrowException(ex);
		}

		return _QeStorage.GetColumnInfo(columnIndex).FieldType;
	}

	public string GetProviderSpecificDataTypeName(int columnIndex)
	{
		if (_QeStorage == null)
		{
			Exception ex = new InvalidOperationException();
			Diag.ThrowException(ex);
		}

		return _QeStorage.GetColumnInfo(columnIndex).ProviderSpecificDataTypeName;
	}

	public void InitiateStopRetrievingData()
	{
		// Evs.Trace(GetType(), nameof(InitiateStopRetrievingData), "", null);
		_IsStopping = true;
		lock (_LockLocal)
		{
			_QeStorage?.InitiateStopStoringData();
		}
	}

	public bool IsXMLColumn(int nColNum)
	{
		if (nColNum < 0 || nColNum >= TotalNumberOfColumns)
		{
			Exception ex = new ArgumentOutOfRangeException("nColNum");
			Diag.ThrowException(ex);
		}

		if (string.Compare(SXmlTypeNameOnServer, GetServerDataTypeName(nColNum), StringComparison.OrdinalIgnoreCase) == 0)
		{
			return true;
		}

		return IsSyntheticXmlColumn(nColNum);
	}

	public async Task<bool> StartConsumingDataWithoutStoringAsync(CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), nameof(StartConsumingDataWithoutStoring));
		if (_QeStorage == null)
		{
			Exception ex = new InvalidOperationException(Resources.ExResultSetNotInitialized);
			Diag.ThrowException(ex);
		}

		if (!_QeStorage.IsClosed())
		{
			Exception ex2 = new InvalidOperationException(Resources.ExResultSetAlreadyStoring);
			Diag.ThrowException(ex2);
		}

		if (_QeStorage is not QEReaderDataStorage obj)
		{
			obj = null;
			Exception ex3 = new InvalidOperationException();
			Diag.ThrowException(ex3);
		}

		await obj.StartConsumingDataWithoutStoringAsync(cancelToken);

		return !cancelToken.Cancelled();
	}

	public async Task<bool> StartRetrievingDataAsync(int nMaxNumCharsToDisplay, int nMaxNumXmlCharsToDisplay, CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), nameof(StartRetrievingData), "nMaxNumCharsToDisplay = {0}", nMaxNumCharsToDisplay);
		if (_QeStorage == null)
		{
			Exception ex = new InvalidOperationException(Resources.ExResultSetNotInitialized);
			Diag.ThrowException(ex);
		}

		if (!_QeStorage.IsClosed())
		{
			Exception ex2 = new InvalidOperationException(Resources.ExResultSetAlreadyStoring);
			Diag.ThrowException(ex2);
		}

		_QeStorage.MaxCharsToStore = Math.Max(nMaxNumCharsToDisplay, C_MaxCharsToStore);
		_QeStorage.MaxXmlCharsToStore = nMaxNumXmlCharsToDisplay;
		_QeStorageView = (IBsQEStorageView)_QeStorage.GetStorageView();
		_QeStorageView.MaxNumBytesToDisplay = nMaxNumCharsToDisplay / 2;

		await _QeStorage.StartStoringDataAsync(cancelToken);

		return !cancelToken.Cancelled();
	}

	public bool IsCellDataNull(long iRow, int iCol)
	{
		int iCol2 = _InGridMode ? iCol - 1 : iCol;
		lock (_LockLocal)
		{
			return _QeStorageView.GetCellData(iRow, iCol2) == null;
		}
	}

	public long RowCount
	{
		get
		{
			if (_QeStorageView != null)
			{
				if (!_InGridMode)
				{
					return _QeStorageView.RowCount;
				}

				long rowCount = _QeStorageView.RowCount;
				if (!IsSyntheticXmlColumn(0))
				{
					return rowCount;
				}

				if (rowCount > 0)
				{
					return 1L;
				}

				return 0L;
			}

			return 0L;
		}
	}

	public long EnsureRowsInBuf(long FirstRowIndex, long LastRowIndex)
	{
		return RowCount;
	}

	public string GetCellDataAsString(long iRow, int iCol)
	{
		lock (_LockLocal)
		{
			if (_InGridMode)
			{
				return _QeStorageView.GetCellDataAsString(iRow, iCol - 1);
			}

			return _QeStorageView.GetCellDataAsString(iRow, iCol);
		}
	}

	public object GetCellData(long iRow, int iCol)
	{
		lock (_LockLocal)
		{
			if (_InGridMode)
			{
				return _QeStorageView.GetCellData(iRow, iCol - 1);
			}

			return _QeStorageView.GetCellData(iRow, iCol);
		}
	}

	public int IsCellEditable(long nRowIndex, int nColIndex)
	{
		return 0;
	}

	public Bitmap GetCellDataAsBitmap(long nRowIndex, int nColIndex)
	{
		return null;
	}

	public void GetCellDataForButton(long nRowIndex, int nColIndex, out EnButtonCellState state, out Bitmap image, out string buttonLabel)
	{
		image = null;
		state = EnButtonCellState.Normal;
		nRowIndex++;
		buttonLabel = nRowIndex.ToString(CultureInfo.InvariantCulture);
	}

	public EnGridCheckBoxState GetCellDataForCheckBox(long nRowIndex, int nColIndex)
	{
		return EnGridCheckBoxState.None;
	}

	public void FillControlWithData(long nRowIndex, int nColIndex, IBsGridEmbeddedControl control)
	{
		Exception ex = new NotImplementedException();
		Diag.ThrowException(ex);
	}

	public bool SetCellDataFromControl(long nRowIndex, int nColIndex, IBsGridEmbeddedControl control)
	{
		Exception ex = new NotImplementedException();
		Diag.ThrowException(ex);
		return false;
	}

	private async Task<bool> OnStorageNotifyAsync(long storageRowCount, bool storedAllData, CancellationToken cancelToken)
	{
		if (MoreRowsAvailableEventAsync != null && !cancelToken.Cancelled())
		{
			_CachedMoreInfoEventArgs.SetEventInfo(storedAllData, storageRowCount, cancelToken);
			await MoreRowsAvailableEventAsync(this, _CachedMoreInfoEventArgs);
			// _M_curRowsNum = storageRowCount;
		}


		if (storedAllData || cancelToken.Cancelled())
		{
			lock (_LockLocal)
				_StoredAllData = true;
		}

		return !cancelToken.Cancelled() && !storedAllData;
	}
}
