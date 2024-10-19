#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Shared.Interfaces;

namespace BlackbirdSql.Shared.Model.IO;


public sealed class QEStorageViewOnReader : AbstractStorageView, IBsQEStorageView, IBsStorageView, IDisposable
{
	private readonly QEReaderDataStorage _QeReaderStorage;

	private StorageDataReader _StorageReader;

	private const int C_ColumnSizeIndex = 2;

	private int _MaxCharsToDisplay = -1;

	private readonly bool[] _CharsGetFlags;

	private readonly bool[] _BytesGetFlags;

	private readonly bool[] _XmlFlags;

	public int MaxNumBytesToDisplay
	{
		get
		{
			return _MaxBytesToDisplay;
		}
		set
		{
			if (_MaxBytesToDisplay != value)
			{
				// Evs.Trace(GetType(), "QueryExecution", "QEDiskStorageView.MaxNumBytesToDisplay", "value = {0}", value);
				_MaxBytesToDisplay = value;
				_SbWork.Capacity = _MaxBytesToDisplay * C_ColumnSizeIndex + C_ColumnSizeIndex;
			}

			_MaxCharsToDisplay = _MaxBytesToDisplay * C_ColumnSizeIndex;
		}
	}

	public QEStorageViewOnReader(QEReaderDataStorage readerDataStorage)
	{
		// Evs.Trace(GetType(), "QEStorageViewOnReader.QEStorageViewOnReader", "", null);
		_QeReaderStorage = readerDataStorage;
		_StorageReader = readerDataStorage.StorageReader;
		int num = readerDataStorage.ColumnCount;
		_CharsGetFlags = new bool[num];
		_BytesGetFlags = new bool[num];
		_XmlFlags = new bool[num];
		for (int i = 0; i < num; i++)
		{
			IBsColumnInfo columnInfo = readerDataStorage.GetColumnInfo(i);
			_CharsGetFlags[i] = columnInfo.IsCharsField;
			_BytesGetFlags[i] = columnInfo.IsBytesField;
			_XmlFlags[i] = columnInfo.IsXml;
		}
	}

	public override long EnsureRowsInBuf(long startRow, long totalRowCount)
	{
		Exception ex = new NotImplementedException();
		Diag.ThrowException(ex);
		return -1;
	}

	public override void DeleteRow(long iRow)
	{
		Exception ex = new NotImplementedException();
		Diag.ThrowException(ex);
	}

	public override long RowCount => _QeReaderStorage.RowCount;

	public override int ColumnCount => _QeReaderStorage.ColumnCount;


	public override IBsColumnInfo GetColumnInfo(int iCol)
	{
		return _QeReaderStorage.GetColumnInfo(iCol);
	}

	public override bool IsStorageClosed()
	{
		return _QeReaderStorage.IsClosed();
	}

	public override object GetCellData(long i64Row, int iCol)
	{
		if (!_StorageReader.IsDBNull(iCol))
		{
			if (_BytesGetFlags[iCol])
			{
				// return _StorageReader.GetSerializedWithMaxCapacity(iCol, MaxNumBytesToDisplay);
				return _StorageReader.GetBytesWithMaxCapacity(iCol, MaxNumBytesToDisplay);
			}

			if (_CharsGetFlags[iCol])
			{
				return _StorageReader.GetCharsWithMaxCapacity(iCol, _MaxCharsToDisplay);
			}

			/*
			if (_XmlFlags[iCol])
			{
				return _StorageReader.GetXmlWithMaxCapacity(iCol, _MaxCharsToDisplay);
			}
			*/
			return _StorageReader.GetValue(iCol);
		}

		return null;
	}

	public override void Dispose(bool disposing)
	{
		_StorageReader = null;
		base.Dispose(disposing);
	}
}
