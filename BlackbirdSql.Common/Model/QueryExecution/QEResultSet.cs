﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Globalization;

using BlackbirdSql.Core;
using BlackbirdSql.Common.Controls.Enums;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Common.Model.IO;

namespace BlackbirdSql.Common.Model.QueryExecution;


public sealed class QEResultSet : IDisposable, IBGridStorage
{
	public const int C_MaxCharsToStore = 10485760;

	private static readonly string _SNameOfXMLColumn = "XML_" + VS.IXMLDocumentGuid;

	private static readonly string _SNameOfJSONColumn = "JSON_" + VS.IXMLDocumentGuid;


	private IBQEStorage _QeStorage;

	private IBQEStorageView _QeStorageView;

	private StringCollection _ColumnNames;

	private readonly QueryManager _QryMgr;

	private readonly string _Script;

	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private bool _IsStopping;

	private StorageNotifyDelegate _NotifyDelegate;

	// private long _M_curRowsNum;

	private bool _InGridMode;

	private DataTable _StorageReaderSchemaTable;

	private bool _StoredAllData;

	private IDataReader _DataReader;

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

	public bool SingleColumnXmlExecutionPlan
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
				return string.Compare(_ColumnNames[0], LibraryData.C_YukonXmlExecutionPlanColumn, StringComparison.Ordinal) == 0;
			}

			return false;

		}
	}

	public bool StoredAllData
	{
		get
		{
			lock (this)
			{
				return _StoredAllData;
			}
		}
	}

	public event MoreRowsAvailableEventHandler MoreRowsAvailableEvent;

	private QEResultSet()
	{
		// Tracer.Trace(GetType(), "QEResultSet.QEResultSet", "", null);
	}

	public QEResultSet(IDataReader reader, QueryManager qryMgr, string script)
		: this()
	{
		if (reader == null)
		{
			Exception ex = new ArgumentNullException("reader");
			Diag.ThrowException(ex);
		}

		_DataReader = reader;
		_QryMgr = qryMgr;
		_Script = script;
	}

	public void Initialize(bool forwardOnly)
	{
		// Tracer.Trace(GetType(), "QEResultSet.Initialize", "", null);
		if (_QeStorage != null)
		{
			Exception ex = new InvalidOperationException(QEResources.ErrQEResultSetAlreadyInited);
			Diag.ThrowException(ex);
		}

		StorageDataReader storageDataReader = new StorageDataReader(_DataReader);
		_StorageReaderSchemaTable = storageDataReader.GetSchemaTable();
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

		_QeStorage.InitStorage(_DataReader);
		// _M_curRowsNum = 0L;
		_NotifyDelegate = OnStorageNotify;
		_QeStorage.StorageNotify += _NotifyDelegate;
		if (_IsStopping)
		{
			_IsStopping = false;
			_QeStorage.InitiateStopStoringData();
		}
	}

	public void Dispose()
	{
		// Tracer.Trace(GetType(), "QEResultSet.Dispose", "", null);
		Dispose(bDisposing: true);
	}

	private void Dispose(bool bDisposing)
	{
		// Tracer.Trace(GetType(), "QEResultSet.Dispose", "bDisposing = {0}", bDisposing);
		if (_QeStorageView != null)
		{
			_QeStorageView.Dispose();
			_QeStorageView = null;
		}

		lock (_LockLocal)
		{
			if (_QeStorage != null)
			{
				if (_NotifyDelegate != null)
				{
					_QeStorage.StorageNotify -= _NotifyDelegate;
				}

				_QeStorage.Dispose();
				_QeStorage = null;
			}
		}

		_DataReader = null;
	}

	public bool IsSyntheticXmlColumn(int nColNum)
	{
		if (nColNum == 0 && (SingleColumnXmlResultSet || SingleColumnXmlExecutionPlan))
		{
			return true;
		}

		return _QryMgr.ConnectionStrategy?.ShouldShowColumnWithXmlHyperLinkEnabled(this, nColNum, _Script) ?? false;
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
		// Tracer.Trace(GetType(), "QEResultSet.InitiateStopRetrievingData", "", null);
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

	public void StartConsumingDataWithoutStoring()
	{
		// Tracer.Trace(GetType(), "QEResultSet.StartConsumingDataWithoutStoring", "", null);
		if (_QeStorage == null)
		{
			Exception ex = new InvalidOperationException(QEResources.ErrQEResultSetNotInited);
			Diag.ThrowException(ex);
		}

		if (!_QeStorage.IsClosed())
		{
			Exception ex2 = new InvalidOperationException(QEResources.ErrQEResultSetAlreadyStoring);
			Diag.ThrowException(ex2);
		}

		if (_QeStorage is not QEReaderDataStorage obj)
		{
			obj = null;
			Exception ex3 = new InvalidOperationException();
			Diag.ThrowException(ex3);
		}

		obj.StartConsumingDataWithoutStoring();
	}

	public void StartRetrievingData(int nMaxNumCharsToDisplay, int nMaxNumXmlCharsToDisplay)
	{
		// Tracer.Trace(GetType(), "QEResultSet.StartRetrievingData", "nMaxNumCharsToDisplay = {0}", nMaxNumCharsToDisplay);
		if (_QeStorage == null)
		{
			Exception ex = new InvalidOperationException(QEResources.ErrQEResultSetNotInited);
			Diag.ThrowException(ex);
		}

		if (!_QeStorage.IsClosed())
		{
			Exception ex2 = new InvalidOperationException(QEResources.ErrQEResultSetAlreadyStoring);
			Diag.ThrowException(ex2);
		}

		_QeStorage.MaxCharsToStore = Math.Max(nMaxNumCharsToDisplay, C_MaxCharsToStore);
		_QeStorage.MaxXmlCharsToStore = nMaxNumXmlCharsToDisplay;
		_QeStorageView = (IBQEStorageView)_QeStorage.GetStorageView();
		_QeStorageView.MaxNumBytesToDisplay = nMaxNumCharsToDisplay / 2;
		_QeStorage.StartStoringData();
	}

	public bool IsCellDataNull(long iRow, int iCol)
	{
		int iCol2 = _InGridMode ? iCol - 1 : iCol;
		lock (this)
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
		lock (this)
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
		lock (this)
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

	public void FillControlWithData(long nRowIndex, int nColIndex, IBGridEmbeddedControl control)
	{
		Exception ex = new NotImplementedException();
		Diag.ThrowException(ex);
	}

	public bool SetCellDataFromControl(long nRowIndex, int nColIndex, IBGridEmbeddedControl control)
	{
		Exception ex = new NotImplementedException();
		Diag.ThrowException(ex);
		return false;
	}

	private void OnStorageNotify(long storageRowCount, bool storedAllData)
	{
		if (MoreRowsAvailableEvent != null)
		{
			_CachedMoreInfoEventArgs.SetEventInfo(storedAllData, storageRowCount);
			MoreRowsAvailableEvent(this, _CachedMoreInfoEventArgs);
			// _M_curRowsNum = storageRowCount;
		}


		if (storedAllData)
		{
			lock (this)
			{
				_StoredAllData = true;
			}
		}
	}
}
