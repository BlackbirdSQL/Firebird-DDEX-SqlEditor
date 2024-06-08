// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.ResultsToTextOrFileBatchConsumer

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;

namespace BlackbirdSql.Shared.Model.QueryExecution;


// =========================================================================================================
//									ResultsToTextOrFileBatchConsumer Class
//
// =========================================================================================================
public sealed class ResultsToTextOrFileBatchConsumer : AbstractQESQLBatchConsumer
{

	// ---------------------------------------------------------------------------------
	#region Constants - ResultsToTextOrFileBatchConsumer
	// ---------------------------------------------------------------------------------


	private const char C_ColumnsSeparatorForColumnsAlignMode = ' ';
	private const int C_ColumnSizeIndex = 2;
	private const int C_NumFirstRowsToFlush = 100;
	private const int C_NewRowsFlushFreq = 500;
	private const int C_InitialRowBuilderCapacity = 1024;
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


	#endregion Constants





	// =========================================================================================================
	#region Fields - ResultsToTextOrFileBatchConsumer
	// =========================================================================================================


	private QEResultSet _ResultSet;
	private int _ColumnCount = -1;
	private readonly StringBuilder _RowBuilder = new StringBuilder(C_InitialRowBuilderCapacity);
	private readonly MoreRowsAvailableEventHandler _MoreRowsAvailableHandler;
	private static readonly Hashtable _TypeToCharNumHashTable;
	private static readonly Hashtable _NumericsTypesHashTable;
	private StringCollection _ColumnsFormatCollection = [];
	private int[] _ColumnWidths;
	private char _ColumnsDelimiter;
	private bool _RightAlignNumerics;
	private bool _PrintColumnHeaders = true;
	private readonly StringBuilder _SbCellData = new StringBuilder(512);
	private readonly IList<KeyValuePair<string[], bool>> _Results = [];
	private readonly IList<int> _MaxColSizes = [];


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - ResultsToTextOrFileBatchConsumer
	// =========================================================================================================


	public char ColumnsDelimiter
	{
		get
		{
			return _ColumnsDelimiter;
		}
		set
		{
			_ColumnsDelimiter = value;
		}
	}

	public bool PrintColumnHeaders
	{
		get
		{
			return _PrintColumnHeaders;
		}
		set
		{
			_PrintColumnHeaders = value;
		}
	}

	public bool RightAlignNumerics
	{
		get
		{
			return _RightAlignNumerics;
		}
		set
		{
			_RightAlignNumerics = value;
		}
	}

	private bool IsColumnAligned => _ColumnsDelimiter == '\0';


	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - ResultsToTextOrFileBatchConsumer
	// =========================================================================================================


	static ResultsToTextOrFileBatchConsumer()
	{
		// Tracer.Trace(typeof(ResultsToTextOrFileBatchConsumer), "static ResultsToTextOrFileBatchConsumer.ResultsToTextOrFileBatchConsumer", "", null);
		_TypeToCharNumHashTable = new Hashtable
		{
			{ "SYSTEM.INT16", short.MinValue.ToString(CultureInfo.InvariantCulture).Length }
		};
		Hashtable typeToCharNumTable = _TypeToCharNumHashTable;
		SqlInt16 minValue = SqlInt16.MinValue;
		typeToCharNumTable.Add("SYSTEM.DATA.SQLTYPES.SQLINT16", minValue.ToString().Length);
		_TypeToCharNumHashTable.Add("SYSTEM.INT32", int.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable2 = _TypeToCharNumHashTable;
		SqlInt32 minValue2 = SqlInt32.MinValue;
		typeToCharNumTable2.Add("SYSTEM.DATA.SQLTYPES.SQLINT32", minValue2.ToString().Length);
		_TypeToCharNumHashTable.Add("SYSTEM.INT64", long.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable3 = _TypeToCharNumHashTable;
		SqlInt64 minValue3 = SqlInt64.MinValue;
		typeToCharNumTable3.Add("SYSTEM.DATA.SQLTYPES.SQLINT64", minValue3.ToString().Length);
		_TypeToCharNumHashTable.Add("SYSTEM.CHAR", '\0'.ToString().Length);
		_TypeToCharNumHashTable.Add("SYSTEM.BYTE", ((byte)0).ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable4 = _TypeToCharNumHashTable;
		SqlByte minValue4 = SqlByte.MinValue;
		typeToCharNumTable4.Add("SYSTEM.DATA.SQLTYPES.SQLBYTE", minValue4.ToString().Length);
		_TypeToCharNumHashTable.Add("SYSTEM.DOUBLE", double.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable5 = _TypeToCharNumHashTable;
		SqlDouble minValue5 = SqlDouble.MinValue;
		typeToCharNumTable5.Add("SYSTEM.DATA.SQLTYPES.SQLDOUBLE", minValue5.ToString().Length);
		_TypeToCharNumHashTable.Add("SYSTEM.SINGLE", float.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable6 = _TypeToCharNumHashTable;
		SqlSingle minValue6 = SqlSingle.MinValue;
		typeToCharNumTable6.Add("SYSTEM.DATA.SQLTYPES.SQLSINGLE", minValue6.ToString().Length);
		_TypeToCharNumHashTable.Add("SYSTEM.DECIMAL", decimal.MinValue.ToString(CultureInfo.InvariantCulture).Length);
		Hashtable typeToCharNumTable7 = _TypeToCharNumHashTable;
		SqlDecimal minValue7 = SqlDecimal.MinValue;
		typeToCharNumTable7.Add("SYSTEM.DATA.SQLTYPES.SQLDECIMAL", minValue7.ToString().Length);
		Hashtable typeToCharNumTable8 = _TypeToCharNumHashTable;
		SqlMoney minValue8 = SqlMoney.MinValue;
		typeToCharNumTable8.Add("SYSTEM.DATA.SQLTYPES.SQLMONEY", minValue8.ToString().Length);
		_NumericsTypesHashTable = new Hashtable
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



	public ResultsToTextOrFileBatchConsumer(IBSqlQueryExecutionHandler resultsControl)
		: base(resultsControl)
	{
		// Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.ResultsToTextOrFileBatchConsumer", "", null);
		_MoreRowsAvailableHandler = OnMoreRowsAvailable;
	}



	protected override void Dispose(bool bDisposing)
	{
		base.Dispose(bDisposing);
		_ColumnsFormatCollection = null;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - ResultsToTextOrFileBatchConsumer
	// =========================================================================================================


	public override void Cleanup()
	{
		// Tracer.Trace(GetType(), "Cleanup()");
		base.Cleanup();
		_Results.Clear();
		_MaxColSizes.Clear();
		if (_ResultSet != null)
		{
			HookupWithEvents(bSubscribe: false);
			_ResultSet.Dispose();
			_ResultSet = null;
			_ColumnCount = -1;
		}
		_ColumnsFormatCollection?.Clear();
		if (_ColumnWidths != null)
		{
			_ColumnWidths = null;
		}
		_RowBuilder.Length = 0;
	}



	private void CreateColumnWidthsAndFormatStrings()
	{
		// Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.CreateColumnWidthsAndFormatStrings", "", null);
		_ColumnWidths = new int[_ResultSet.NumberOfDataColumns];

		string dateFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

		for (int i = 0; i < _ResultSet.NumberOfDataColumns; i++)
		{
			string providerTypeName = _ResultSet.GetProviderSpecificDataTypeName(i).ToUpperInvariant();
			string serverTypeName = _ResultSet.GetServerDataTypeName(i).ToUpperInvariant();
			object obj = _TypeToCharNumHashTable[providerTypeName];
			int width;

			if (obj != null)
			{
				width = (int)obj;
			}
			else
			{
				switch (providerTypeName)
				{
					case "SYSTEM.STRING":
					case "SYSTEM.DATA.SQLTYPES.SQLSTRING":
					case "SYSTEM.OBJECT":
						width = _MaxColSizes[i];
						// width = (int)_ResultSet.GetSchemaRow(i)[C_ColumnSizeIndex];
						break;
					case "SYSTEM.BOOLEAN":
					case "SYSTEM.DATA.SQLTYPES.SQLBOOLEAN":
						width = 5;
						break;
					case "SYSTEM.DATETIME":
					case "SYSTEM.DATA.SQLTYPES.SQLDATETIME":
						if (serverTypeName == "DATE")
						{
							width = "yyyy-MM-dd".Length;
						}
						else if (serverTypeName == "DATETIME2")
						{
							width = dateFormat.Length;
						}
						else
						{
							width = "yyyy-MM-dd HH:mm:ss.fff".Length;
						}
						break;
					default:
						if (serverTypeName == "TIME")
						{
							width = 16;
							break;
						}
						if (serverTypeName == "DATETIMEOFFSET")
						{
							width = 34;
							break;
						}
						switch (providerTypeName)
						{
							case "SYSTEM.BYTE[]":
							case "SYSTEM.DATA.SQLTYPES.SQLBYTE[]":
							case "SYSTEM.DATA.SQLTYPES.SQLBINARY":
								width = _MaxColSizes[i];
								// width = (int)_ResultSet.GetSchemaRow(i)[C_ColumnSizeIndex];
								width = width != int.MaxValue ? width * C_ColumnSizeIndex + C_ColumnSizeIndex : width;
								// width = width != int.MaxValue ? width * C_ColumnSizeIndex + C_ColumnSizeIndex : _MaxCharsPerColumn;
								break;
							case "SYSTEM.GUID":
							case "SYSTEM.DATA.SQLTYPES.SQLGUID":
								width = 36;
								break;
							default:
								if (string.Compare(QEResultSet.SXmlTypeNameOnServer, serverTypeName, StringComparison.OrdinalIgnoreCase) == 0)
								{
									width = _MaxColSizes[i];
									// width = (int)_ResultSet.GetSchemaRow(i)[C_ColumnSizeIndex];
								}
								else
								{
									width = 1; // _MaxCharsPerColumn;
								}
								break;
						}
						break;
				}
			}

			if (_PrintColumnHeaders)
			{
				int headerLen = _ResultSet.ColumnNames[i].Length;
				if (width < headerLen)
					width = headerLen;
			}

			if (width > _MaxCharsPerColumn)
			{
				width = _MaxCharsPerColumn;
				switch (providerTypeName)
				{
					case "SYSTEM.BYTE[]":
					case "SYSTEM.DATA.SQLTYPES.SQLBYTE[]":
					case "SYSTEM.DATA.SQLTYPES.SQLBINARY":
						width += C_ColumnSizeIndex;
						break;
				}
			}

			if (width < 4)
				width = 4;

			_ColumnWidths[i] = width;

			if (!_RightAlignNumerics || _RightAlignNumerics && !_NumericsTypesHashTable.Contains(providerTypeName))
			{
				if (i != _ResultSet.NumberOfDataColumns - 1)
				{
					_ColumnsFormatCollection.Add("{0,-" + width + "}");
				}
				else
				{
					_ColumnsFormatCollection.Add("{0}");
				}
			}
			else
			{
				_ColumnsFormatCollection.Add("{0," + width + "}");
			}
		}
	}



	private void HookupWithEvents(bool bSubscribe)
	{
		// Tracer.Trace(GetType(), "HookupWithEvents()", "bSubscribe = {0}", bSubscribe);
		if (bSubscribe)
		{
			_ResultSet.MoreRowsAvailableEvent += _MoreRowsAvailableHandler;
		}
		else
		{
			_ResultSet.MoreRowsAvailableEvent -= _MoreRowsAvailableHandler;
		}
	}



	private void OutputColumnNames()
	{
		// Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.OutputColumnNames", "", null);
		_RowBuilder.Length = 0;
		int num;
		for (num = 0; num < _ResultSet.ColumnNames.Count; num++)
		{
			if (num > 0)
			{
				if (IsColumnAligned)
				{
					_RowBuilder.Append(C_ColumnsSeparatorForColumnsAlignMode);
				}
				else
				{
					_RowBuilder.Append(_ColumnsDelimiter);
				}
			}
			_SbCellData.Length = 0;
			_SbCellData.Append(_ResultSet.ColumnNames[num]);
			if (_SbCellData.Length > _ColumnWidths[num])
			{
				_SbCellData.Length = _ColumnWidths[num];
			}
			if (IsColumnAligned)
			{
				_RowBuilder.AppendFormat(_ColumnsFormatCollection[num], _SbCellData.ToString());
			}
			else
			{
				_RowBuilder.Append(_SbCellData.ToString());
			}
		}
		_ResultsControl.AddStringToResults(_RowBuilder.ToString(), false);
		if (!IsColumnAligned)
		{
			return;
		}
		_RowBuilder.Length = 0;
		_RowBuilder.Capacity = C_InitialRowBuilderCapacity;
		for (num = 0; num < _ResultSet.ColumnNames.Count; num++)
		{
			for (int i = 0; i < _ColumnWidths[num]; i++)
			{
				_RowBuilder.Append('-');
			}
			if (num != _ResultSet.ColumnNames.Count - 1)
			{
				_RowBuilder.Append(C_ColumnsSeparatorForColumnsAlignMode);
			}
		}
		_ResultsControl.AddStringToResults(_RowBuilder.ToString(), true);
	}



	private void OutputRow(string[] row, bool flush)
	{
		// Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.OnMoreRowsAvailable", "NewRows = {0}, AllData = {1}", a.NewRowsNumber, a.AllRows);

		_RowBuilder.Length = 0;
		for (int i = 0; i < _ColumnCount; i++)
		{
			if (i > 0)
			{
				if (IsColumnAligned)
				{
					_RowBuilder.Append(C_ColumnsSeparatorForColumnsAlignMode);
				}
				else
				{
					_RowBuilder.Append(_ColumnsDelimiter);
				}
			}
			_SbCellData.Length = 0;
			_SbCellData.Append(row[i]);
			if (_SbCellData.Length > _ColumnWidths[i])
			{
				_SbCellData.Length = _ColumnWidths[i];
			}
			if (IsColumnAligned)
			{
				_RowBuilder.AppendFormat(_ColumnsFormatCollection[i], _SbCellData.ToString());
			}
			else
			{
				_RowBuilder.Append(_SbCellData.ToString());
			}
		}

		int length = _RowBuilder.Length;
		for (int j = 0; j < length; j++)
		{
			if (_RowBuilder[j] == '\0')
			{
				_RowBuilder[j] = C_ColumnsSeparatorForColumnsAlignMode;
			}
		}

		// Tracer.Trace(GetType(), "OnMoreRowsAvailable", "Row string: {0}", _RowBuilder.ToString());

		_ResultsControl.AddStringToResults(_RowBuilder.ToString(), flush);

	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - ResultsToTextOrFileBatchConsumer
	// =========================================================================================================


	private void OnMoreRowsAvailable(object sender, MoreRowsAvailableEventArgs a)
	{
		// Tracer.Trace(GetType(), "OnMoreRowsAvailable()", "_ColumnCount: {0}, _MaxColSizes.Count: {1}", _ColumnCount, _MaxColSizes.Count);

		if (a.AllRows)
		{
			CreateColumnWidthsAndFormatStrings();

			if (_PrintColumnHeaders)
				OutputColumnNames();

			foreach (KeyValuePair<string[], bool> pair in _Results)
				OutputRow(pair.Key, pair.Value);

			_Results.Clear();
			_MaxColSizes.Clear();

			return;
		}

		long iRow = a.NewRowsNumber - 1;

		string value;
		string[] row = new string[_ColumnCount];
		int len;

		if (_MaxColSizes.Count == 0)
		{
			for (int i = 0; i < _ColumnCount; i++)
				_MaxColSizes.Add(1);
		}


		for (int i = 0; i < _ColumnCount; i++)
		{
			value = _ResultSet.GetCellDataAsString(iRow, i);

			len = value.Length;

			if (len > _MaxCharsPerColumn)
			{
				len = _MaxCharsPerColumn;
				value = value[..len];
			}

			row[i] = value;


			if (len > _MaxColSizes[i])
				_MaxColSizes[i] = len;
		}

		bool flush = a.NewRowsNumber > C_NumFirstRowsToFlush && a.NewRowsNumber % C_NewRowsFlushFreq == 0L || a.AllRows || a.NewRowsNumber == C_NumFirstRowsToFlush;

		_Results.Add(new KeyValuePair<string[], bool>(row, flush));

		// Tracer.Trace(GetType(), "OnMoreRowsAvailable", "Row string: {0}", _RowBuilder.ToString());
	}



	public override async Task<bool> OnNewResultSetAsync(object sender, QESQLBatchNewResultSetEventArgs args)
	{
		// Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.OnNewResultSet", "", null);
		Cleanup();

		await args.ResultSet.InitializeAsync(true, args.CancelToken);

		if (args.CancelToken.IsCancellationRequested)
			return false;

		if (DiscardResults)
		{
			await HandleNewResultSetForDiscardAsync(args);
			return false;
		}
		_ResultSet = args.ResultSet;
		HookupWithEvents(bSubscribe: true);
		/*
		CreateColumnWidthsAndFormatStrings();
		if (_PrintColumnHeaders)
		{
			OutputColumnNames();
		}
		*/

		_ColumnCount = _ResultSet.ColumnNames.Count;

		await _ResultSet.StartRetrievingDataAsync(_MaxCharsPerColumn, _ResultsControl.LiveSettings.EditorResultsGridMaxCharsPerColumnXml, args.CancelToken);

		return !args.CancelToken.IsCancellationRequested;
	}

	public override void OnFinishedProcessingResultSet(object sender, EventArgs args)
	{
		// Tracer.Trace(GetType(), "OnFinishedProcessingResultSet()");

		_ResultsControl.AddResultSetSeparatorMsg();

		_Results.Clear();
		_MaxColSizes.Clear();
	}



	public override void OnCancelling(object sender, EventArgs args)
	{
		// Tracer.Trace(GetType(), "ResultsToTextOrFileBatchConsumer.OnCancelling", "", null);
		base.OnCancelling(sender, args);
	}


	#endregion Event Handling

}
