// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.ResultSetAndGridContainer

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model.QueryExecution;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


public sealed class ResultSetAndGridContainer : IDisposable
{
	private IBsGridControl2 _GridCtl;

	private QEResultSet _QeResultSet;

	private MoreRowsAvailableEventHandler _MoreRowsAvailableHandler;

	private double m_controlToWindowRatio = 1.0;

	private readonly bool _PrintColumnHeaders = true;

	private readonly int _NumberOfCharsToShow = SysConstants.C_DefaultGridMaxCharsPerColumnStd;

	private bool m_bGridHasRows;

	private const int C_NumOfFirstRowsToInitialResizeColumns = 30;

	private const int C_NumOfFirstRowsToNotifyAbout = 100;

	private const int C_NewRowsNotificationFreq = 10000;

	public QEResultSet QEResultSet => _QeResultSet;

	public IBsGridControl2 GridCtl => _GridCtl;

	public double ControlToWindowRatio
	{
		get
		{
			return m_controlToWindowRatio;
		}
		set
		{
			// Tracer.Trace(GetType(), "ResultSetAndGridContainer.ControlToWindowRatio", "value = {0}", value);
			m_controlToWindowRatio = value;
		}
	}

	public ResultSetAndGridContainer(QEResultSet resultSet, bool printColumnHeaders, int numberOfCharsToShow)
	{
		// Tracer.Trace(GetType(), "ResultSetAndGridContainer.ResultSetAndGridContainer", "", null);
		_QeResultSet = resultSet;
		_PrintColumnHeaders = printColumnHeaders;
		_NumberOfCharsToShow = numberOfCharsToShow;
		_MoreRowsAvailableHandler = OnMoreRowsAvailableFromStorage;
	}

	public void Initialize(IBsGridControl2 grid)
	{
		// Tracer.Trace(GetType(), "ResultSetAndGridContainer.Initialize", "", null);
		_GridCtl = grid;
		_QeResultSet.InGridMode = true;
		_QeResultSet.MoreRowsAvailableEvent += _MoreRowsAvailableHandler;
		((ISupportInitialize)_GridCtl).BeginInit();
		_GridCtl.SelectionType = EnGridSelectionType.CellBlocks;
		_GridCtl.GridStorage = _QeResultSet;
		_GridCtl.WithHeader = _PrintColumnHeaders;
		_GridCtl.AlwaysHighlightSelection = true;
		_GridCtl.NumberOfCharsToShow = _NumberOfCharsToShow;
		GridColumnInfo gridColumnInfo = new()
		{
			ColumnType = 2,
			ColumnWidth = 6,
			WidthType = EnGridColumnWidthType.InAverageFontChar
		};
		_GridCtl.AddColumn(gridColumnInfo);
		for (int i = 0; i < _QeResultSet.NumberOfDataColumns; i++)
		{
			gridColumnInfo = new GridColumnInfo();
			if (_QeResultSet.IsXMLColumn(i))
			{
				gridColumnInfo.ColumnType = 5;
			}
			else
			{
				gridColumnInfo.ColumnType = 1;
			}

			_GridCtl.AddColumn(gridColumnInfo);
			string text = _QeResultSet.ColumnNames[i];
			if (text == null || text.Length == 0)
			{
				text = ControlsResources.Grid_ResultsNoColumnTitle;
			}

			_GridCtl.SetHeaderInfo(i + 1, text, null);
		}

		_GridCtl.FirstScrollableColumn = 1;
		_GridCtl.ColumnsReorderableByDefault = true;
		((ISupportInitialize)_GridCtl).EndInit();
		_GridCtl.UpdateGrid();
		m_bGridHasRows = false;
		// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "ResultSetAndGridContainer.Initialize", "returning");
	}

	public void Dispose()
	{
		// Tracer.Trace(GetType(), "ResultSetAndGridContainer.Dispose", "", null);
		if (_QeResultSet != null)
		{
			_QeResultSet.MoreRowsAvailableEvent -= _MoreRowsAvailableHandler;
			_MoreRowsAvailableHandler = null;
			_QeResultSet.Dispose();
			_QeResultSet = null;
		}

		if (_GridCtl != null)
		{
			((IDisposable)_GridCtl).Dispose();
			_GridCtl = null;
		}
	}

	public async Task<bool> StartRetrievingDataAsync(int nMaxNumCharsToDisplay, int nMaxNumXmlCharsToDisplay, CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "ResultSetAndGridContainer.StartRetrievingData", "nMaxNumCharsToDisplay = {0}, nMaxNumXmlCharsToDisplay={1}", nMaxNumCharsToDisplay, nMaxNumXmlCharsToDisplay);
		await _QeResultSet.StartRetrievingDataAsync(nMaxNumCharsToDisplay, nMaxNumXmlCharsToDisplay, cancelToken);

		return !cancelToken.IsCancellationRequested;
	}

	public void UpdateGrid()
	{
		_GridCtl.UpdateGrid();
		_GridCtl.InitialColumnResize();
	}

	private void OnMoreRowsAvailableFromStorage(object sender, MoreRowsAvailableEventArgs a)
	{
		if (a.NewRowsNumber == C_NumOfFirstRowsToInitialResizeColumns || a.AllRows)
		{
			_GridCtl.UpdateGrid();
			_GridCtl.InitialColumnResize();
		}

		if (a.NewRowsNumber > C_NumOfFirstRowsToNotifyAbout && a.NewRowsNumber % C_NewRowsNotificationFreq == 0L || a.AllRows || a.NewRowsNumber == C_NumOfFirstRowsToNotifyAbout)
		{
			_GridCtl.UpdateGrid();
			if (!m_bGridHasRows)
			{
				SelectFirstCellInTheGrid();
				m_bGridHasRows = true;
			}
		}
	}

	private void SelectFirstCellInTheGrid()
	{
		if (_GridCtl.GridStorage.RowCount > 0)
		{
			BlockOfCells node = new BlockOfCells(0L, 1);
			BlockOfCellsCollection blockOfCellsCollection =
			[
				node
			];
			_GridCtl.SelectedCells = blockOfCellsCollection;
		}
	}
}
