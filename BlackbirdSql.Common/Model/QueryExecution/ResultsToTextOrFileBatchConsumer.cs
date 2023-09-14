// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.ResultsToTextOrFileBatchConsumer

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Model.QueryExecution;


public sealed class ResultsToTextOrFileBatchConsumer : AbstractQESQLBatchConsumer
{
	private QEResultSet _curResultSet;

	private int _numOfColumnsInCurRS = -1;

	private const int C_InitialRowBuilderCapacity = 1024;

	private readonly StringBuilder _rowBuilder = new StringBuilder(C_InitialRowBuilderCapacity);

	private readonly MoreRowsAvailableEventHandler _MoreRowsAvailableHandler;

	private static readonly Hashtable _typeToCharNumTable;

	private static readonly Hashtable _numericsTypes;

	private StringCollection _columnsFormatCollection = new StringCollection();

	private int[] _columnWidths;

	private char _columnsDelimiter;

	private bool _bRightAlignNumerics;

	private bool _bPrintColumnHeaders = true;

	private readonly StringBuilder _sbCellData = new StringBuilder(512);

	private const char _columnsSeparatorForColumnsAlignMode = ' ';

	private const int C_ColumnSizeIndex = 2;

	private const int C_NumFirstRowsToFlush = 100;

	private const int C_NewRowsFlushFreq = 500;

	// private const string _stringType = "SYSTEM.STRING";

	// private const string _sqlStringType = "SYSTEM.DATA.SQLTYPES.SQLSTRING";

	// private const string _objectType = "SYSTEM.OBJECT";

	// private const string _boolType = "SYSTEM.BOOLEAN";

	// private const string _sqlBoolType = "SYSTEM.DATA.SQLTYPES.SQLBOOLEAN";

	// private const string _dateTimeType = "SYSTEM.DATETIME";

	// private const string _sqlDateTimeType = "SYSTEM.DATA.SQLTYPES.SQLDATETIME";

	// private const string _byteArrayType = "SYSTEM.BYTE[]";

	// private const string _sqlByteArrayType = "SYSTEM.DATA.SQLTYPES.SQLBYTE[]";

	// private const string _sqlBinaryType = "SYSTEM.DATA.SQLTYPES.SQLBINARY";

	// private const string _guidType = "SYSTEM.GUID";

	// private const string _sqlGuidType = "SYSTEM.DATA.SQLTYPES.SQLGUID";

	// private const string _xmlServerSideTypeName = "XML";

	// private const string _dateServerSideTypeName = "DATE";

	// private const string _timeServerSideTypeName = "TIME";

	// private const string _datetime2ServerSideTypeName = "DATETIME2";

	// private const string _datetimeoffsetServerSideTypeName = "DATETIMEOFFSET";

	public char ColumnsDelimiter
	{
		get
		{
			return _columnsDelimiter;
		}
		set
		{
			_columnsDelimiter = value;
		}
	}

	public bool PrintColumnHeaders
	{
		get
		{
			return _bPrintColumnHeaders;
		}
		set
		{
			_bPrintColumnHeaders = value;
		}
	}

	public bool RightAlignNumerics
	{
		get
		{
			return _bRightAlignNumerics;
		}
		set
		{
			_bRightAlignNumerics = value;
		}
	}

	private bool IsColumnAligned => _columnsDelimiter == '\0';

	static ResultsToTextOrFileBatchConsumer()
	{
		Tracer.Trace(typeof(ResultsToTextOrFileBatchConsumer), "static ResultsToTextOrFileBatchConsumer.ResultsToTextOrFileBatchConsumer", "", null);
		_typeToCharNumTable = new Hashtable
		{
			{ "SYSTEM.INT16", short.MinValue.ToString(CultureInfo.InvariantCulture).Length }
		};
		Hashtable typeToCharNumTable = _typeToCharNumTable;
		SqlInt16 minValue = SqlInt16.MinValue;
		typeToCharNumTable.Add("SYSTEM.DATA.SQLTYPES.SQLINT16", minValue.ToString().Length);
		_typeToCharNumTable.Add("SYSTEM.INT32", int.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable2 = _typeToCharNumTable;
		SqlInt32 minValue2 = SqlInt32.MinValue;
		typeToCharNumTable2.Add("SYSTEM.DATA.SQLTYPES.SQLINT32", minValue2.ToString().Length);
		_typeToCharNumTable.Add("SYSTEM.INT64", long.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable3 = _typeToCharNumTable;
		SqlInt64 minValue3 = SqlInt64.MinValue;
		typeToCharNumTable3.Add("SYSTEM.DATA.SQLTYPES.SQLINT64", minValue3.ToString().Length);
		_typeToCharNumTable.Add("SYSTEM.CHAR", '\0'.ToString().Length);
		_typeToCharNumTable.Add("SYSTEM.BYTE", ((byte)0).ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable4 = _typeToCharNumTable;
		SqlByte minValue4 = SqlByte.MinValue;
		typeToCharNumTable4.Add("SYSTEM.DATA.SQLTYPES.SQLBYTE", minValue4.ToString().Length);
		_typeToCharNumTable.Add("SYSTEM.DOUBLE", double.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable5 = _typeToCharNumTable;
		SqlDouble minValue5 = SqlDouble.MinValue;
		typeToCharNumTable5.Add("SYSTEM.DATA.SQLTYPES.SQLDOUBLE", minValue5.ToString().Length);
		_typeToCharNumTable.Add("SYSTEM.SINGLE", float.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable6 = _typeToCharNumTable;
		SqlSingle minValue6 = SqlSingle.MinValue;
		typeToCharNumTable6.Add("SYSTEM.DATA.SQLTYPES.SQLSINGLE", minValue6.ToString().Length);
		_typeToCharNumTable.Add("SYSTEM.DECIMAL", decimal.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable7 = _typeToCharNumTable;
		SqlDecimal minValue7 = SqlDecimal.MinValue;
		typeToCharNumTable7.Add("SYSTEM.DATA.SQLTYPES.SQLDECIMAL", minValue7.ToString().Length);
		Hashtable typeToCharNumTable8 = _typeToCharNumTable;
		SqlMoney minValue8 = SqlMoney.MinValue;
		typeToCharNumTable8.Add("SYSTEM.DATA.SQLTYPES.SQLMONEY", minValue8.ToString().Length);
		_numericsTypes = new Hashtable
		{
			{ "SYSTEM.INT16", 0 },
			{ "SYSTEM.DATA.SQLTYPES.SQLINT16", 0 },
			{ "SYSTEM.INT32", 0 },
			{ "SYSTEM.DATA.SQLTYPES.SQLINT32", 0 },
			{ "SYSTEM.INT64", 0 },
			{ "SYSTEM.DATA.SQLTYPES.SQLINT64", 0 },
			{ "SYSTEM.BYTE", 0 },
			{ "SYSTEM.DATA.SQLTYPES.SQLBYTE", 0 },
			{ "SYSTEM.DOUBLE", 0 },
			{ "SYSTEM.DATA.SQLTYPES.SQLDOUBLE", 0 },
			{ "SYSTEM.SINGLE", 0 },
			{ "SYSTEM.DATA.SQLTYPES.SQLSINGLE", 0 },
			{ "SYSTEM.DECIMAL", 0 },
			{ "SYSTEM.DATA.SQLTYPES.SQLDECIMAL", 0 },
			{ "SYSTEM.DATA.SQLTYPES.SQLMONEY", 0 }
		};
	}

	public ResultsToTextOrFileBatchConsumer(ISqlQueryExecutionHandler resultsControl)
		: base(resultsControl)
	{
		Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.ResultsToTextOrFileBatchConsumer", "", null);
		_MoreRowsAvailableHandler = OnMoreRowsAvailable;
	}

	public override void OnNewResultSet(object sender, QESQLBatchNewResultSetEventArgs args)
	{
		Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.OnNewResultSet", "", null);
		Cleanup();
		args.ResultSet.Initialize(forwardOnly: true);
		if (DiscardResults)
		{
			HandleNewResultSetForDiscard(args);
			return;
		}
		_curResultSet = args.ResultSet;
		HookupWithEvents(bSubscribe: true);
		CreateColumnWidthsAndFormatStrings();
		if (_bPrintColumnHeaders)
		{
			OutputColumnNames();
		}
		_numOfColumnsInCurRS = _curResultSet.ColumnNames.Count;
		_curResultSet.StartRetrievingData(_MaxCharsPerColumn, _ResultsControl.ResultsSettings.MaxCharsPerColumnForXml);
		Tracer.Trace(GetType(), Tracer.Level.Information, "ResultsToTextOrFileBatchConsumer.OnNewResultSet", "returning");
	}

	public override void OnFinishedProcessingResultSet(object sender, EventArgs args)
	{
		Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.OnFinishedProcessingResultSet", "", null);
		_ResultsControl.AddResultSetSeparatorMsg();
	}

	public override void OnCancelling(object sender, EventArgs args)
	{
		Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.OnCancelling", "", null);
		base.OnCancelling(sender, args);
	}

	public override void Cleanup()
	{
		Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.Cleanup", "", null);
		base.Cleanup();
		if (_curResultSet != null)
		{
			HookupWithEvents(bSubscribe: false);
			_curResultSet.Dispose();
			_curResultSet = null;
			_numOfColumnsInCurRS = -1;
		}
		_columnsFormatCollection?.Clear();
		if (_columnWidths != null)
		{
			_columnWidths = null;
		}
		_rowBuilder.Length = 0;
	}

	protected override void Dispose(bool bDisposing)
	{
		base.Dispose(bDisposing);
		_columnsFormatCollection = null;
	}

	private void OnMoreRowsAvailable(object sender, MoreRowsAvailableEventArgs a)
	{
		Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.OnMoreRowsAvailable", "NewRows = {0}, AllData = {1}", a.NewRowsNumber, a.AllRows);
		if (a.AllRows)
		{
			return;
		}
		long iRow = a.NewRowsNumber - 1;
		_rowBuilder.Length = 0;
		for (int i = 0; i < _numOfColumnsInCurRS; i++)
		{
			if (i > 0)
			{
				if (IsColumnAligned)
				{
					_rowBuilder.Append(_columnsSeparatorForColumnsAlignMode);
				}
				else
				{
					_rowBuilder.Append(_columnsDelimiter);
				}
			}
			_sbCellData.Length = 0;
			_sbCellData.Append(_curResultSet.GetCellDataAsString(iRow, i));
			if (_sbCellData.Length > _columnWidths[i])
			{
				_sbCellData.Length = _columnWidths[i];
			}
			if (IsColumnAligned)
			{
				_rowBuilder.AppendFormat(_columnsFormatCollection[i], _sbCellData.ToString());
			}
			else
			{
				_rowBuilder.Append(_sbCellData.ToString());
			}
		}
		bool flush = a.NewRowsNumber > C_NumFirstRowsToFlush && a.NewRowsNumber % C_NewRowsFlushFreq == 0L || a.AllRows || a.NewRowsNumber == C_NumFirstRowsToFlush;
		int length = _rowBuilder.Length;
		for (int j = 0; j < length; j++)
		{
			if (_rowBuilder[j] == '\0')
			{
				_rowBuilder[j] = _columnsSeparatorForColumnsAlignMode;
			}
		}
		_ResultsControl.AddStringToResults(_rowBuilder.ToString(), flush);
	}

	private void HookupWithEvents(bool bSubscribe)
	{
		Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.HookupWithEvents", "bSubscribe = {0}", bSubscribe);
		if (bSubscribe)
		{
			_curResultSet.MoreRowsAvailableEvent += _MoreRowsAvailableHandler;
		}
		else
		{
			_curResultSet.MoreRowsAvailableEvent -= _MoreRowsAvailableHandler;
		}
	}

	private void CreateColumnWidthsAndFormatStrings()
	{
		Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.CreateColumnWidthsAndFormatStrings", "", null);
		_columnWidths = new int[_curResultSet.NumberOfDataColumns];
		string text = "yyyy-MM-dd HH:mm:ss.fffffff";
		for (int i = 0; i < _curResultSet.NumberOfDataColumns; i++)
		{
			string text2 = _curResultSet.GetProviderSpecificDataTypeName(i).ToUpperInvariant();
			string text3 = _curResultSet.GetServerDataTypeName(i).ToUpperInvariant();
			object obj = _typeToCharNumTable[text2];
			int num;
			if (obj != null)
			{
				Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: found hash entry of col {0}, type = {1}", i, text2);
				num = (int)obj;
			}
			else
			{
				switch (text2)
				{
					case "SYSTEM.STRING":
					case "SYSTEM.DATA.SQLTYPES.SQLSTRING":
					case "SYSTEM.OBJECT":
						Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign:  col {0} is String", i);
						num = (int)_curResultSet.GetSchemaRow(i)[C_ColumnSizeIndex];
						break;
					case "SYSTEM.BOOLEAN":
					case "SYSTEM.DATA.SQLTYPES.SQLBOOLEAN":
						Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: col {0} is Boolean", i);
						num = 5;
						break;
					case "SYSTEM.DATETIME":
					case "SYSTEM.DATA.SQLTYPES.SQLDATETIME":
						if (text3 == "DATE")
						{
							Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: col {0} is Date", i);
							num = "yyyy-MM-dd".Length;
						}
						else if (text3 == "DATETIME2")
						{
							Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: col {0} is DateTime2", i);
							num = text.Length;
						}
						else
						{
							Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: col {0} is DataTime", i);
							num = "yyyy-MM-dd HH:mm:ss.fff".Length;
						}
						break;
					default:
						if (text3 == "TIME")
						{
							Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: col {0} is Time", i);
							num = 16;
							break;
						}
						if (text3 == "DATETIMEOFFSET")
						{
							Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: col {0} is DateTimeOffset", i);
							num = 34;
							break;
						}
						switch (text2)
						{
							case "SYSTEM.BYTE[]":
							case "SYSTEM.DATA.SQLTYPES.SQLBYTE[]":
							case "SYSTEM.DATA.SQLTYPES.SQLBINARY":
								Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: col {0} is Byte[]", i);
								num = (int)_curResultSet.GetSchemaRow(i)[C_ColumnSizeIndex];
								num = num != int.MaxValue ? num * C_ColumnSizeIndex + C_ColumnSizeIndex : _MaxCharsPerColumn;
								break;
							case "SYSTEM.GUID":
							case "SYSTEM.DATA.SQLTYPES.SQLGUID":
								Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: col {0} is Guid", i);
								num = 36;
								break;
							default:
								if (string.Compare(QEResultSet.SXmlTypeNameOnServer, text3, StringComparison.OrdinalIgnoreCase) == 0)
								{
									Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings",				"ColumnAlign: col {0} is XML", i);
									num = (int)_curResultSet.GetSchemaRow(i)[C_ColumnSizeIndex];
								}
								else
								{
									Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings",				"ColumnAlign: col {0} is unexpected type {1}", i, text2);
									num = _MaxCharsPerColumn;
								}
								break;
						}
						break;
				}
			}
			Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "CreateColumnWidthsAndFormatStrings", "ColumnAlign: initial length for col {0} is {1}", i, num);
			int num2 = _curResultSet.ColumnNames[i].Length;
			if (num < num2 && _bPrintColumnHeaders)
			{
				Tracer.Trace(GetType(), Tracer.Level.Warning, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: adjusting col {0} length for column name length", i);
				num = num2;
			}
			if (num > _MaxCharsPerColumn)
			{
				Tracer.Trace(GetType(), Tracer.Level.Warning, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: adjusting col {0} length for max chars", i);
				num = _MaxCharsPerColumn;
				switch (text2)
				{
					case "SYSTEM.BYTE[]":
					case "SYSTEM.DATA.SQLTYPES.SQLBYTE[]":
					case "SYSTEM.DATA.SQLTYPES.SQLBINARY":
						num += C_ColumnSizeIndex;
						break;
				}
			}
			if (num < 4)
			{
				Tracer.Trace(GetType(), Tracer.Level.Warning, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: adjusting col {0} length for NULL", i);
				num = 4;
			}
			Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: final length of col {0} is {1}", i, num);
			_columnWidths[i] = num;
			if (!_bRightAlignNumerics || _bRightAlignNumerics && !_numericsTypes.Contains(text2))
			{
				if (i != _curResultSet.NumberOfDataColumns - 1)
				{
					_columnsFormatCollection.Add("{0,-" + num + "}");
				}
				else
				{
					_columnsFormatCollection.Add("{0}");
				}
			}
			else
			{
				_columnsFormatCollection.Add("{0," + num + "}");
			}
			Tracer.Trace(GetType(), Tracer.Level.Information, "CreateColumnWidthsAndFormatStrings", "ColumnAlign: format string of col {0} is \"{1}\", m_bRightAlignNumerics = {2}", i, _columnsFormatCollection[i], _bRightAlignNumerics);
		}
	}

	private void OutputColumnNames()
	{
		Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.OutputColumnNames", "", null);
		_rowBuilder.Length = 0;
		int num;
		for (num = 0; num < _curResultSet.ColumnNames.Count; num++)
		{
			if (num > 0)
			{
				if (IsColumnAligned)
				{
					_rowBuilder.Append(_columnsSeparatorForColumnsAlignMode);
				}
				else
				{
					_rowBuilder.Append(_columnsDelimiter);
				}
			}
			_sbCellData.Length = 0;
			_sbCellData.Append(_curResultSet.ColumnNames[num]);
			if (_sbCellData.Length > _columnWidths[num])
			{
				_sbCellData.Length = _columnWidths[num];
			}
			if (IsColumnAligned)
			{
				_rowBuilder.AppendFormat(_columnsFormatCollection[num], _sbCellData.ToString());
			}
			else
			{
				_rowBuilder.Append(_sbCellData.ToString());
			}
		}
		_ResultsControl.AddStringToResults(_rowBuilder.ToString(), flush: false);
		if (!IsColumnAligned)
		{
			return;
		}
		_rowBuilder.Length = 0;
		_rowBuilder.Capacity = C_InitialRowBuilderCapacity;
		for (num = 0; num < _curResultSet.ColumnNames.Count; num++)
		{
			for (int i = 0; i < _columnWidths[num]; i++)
			{
				_rowBuilder.Append('-');
			}
			if (num != _curResultSet.ColumnNames.Count - 1)
			{
				_rowBuilder.Append(_columnsSeparatorForColumnsAlignMode);
			}
		}
		_ResultsControl.AddStringToResults(_rowBuilder.ToString(), flush: true);
	}
}
