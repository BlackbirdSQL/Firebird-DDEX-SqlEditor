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
using EnvDTE;
using static System.Net.Mime.MediaTypeNames;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


public sealed class ResultSetAndGridContainer : IDisposable
{
	private IBsGridControl2 _GridCtl;

	private QueryResultSet _ResultSet;

	private MoreRowsAvailableEventHandler _MoreRowsAvailableEventAsync;

	private double m_controlToWindowRatio = 1.0;

	private readonly bool _PrintColumnHeaders = true;

	private readonly int _NumberOfCharsToShow = SharedConstants.C_DefaultGridMaxCharsPerColumnStd;

	private bool m_bGridHasRows;

	private const int C_NumOfFirstRowsToInitialResizeColumns = 30;

	private const int C_NumOfFirstRowsToNotifyAbout = 100;

	private const int C_NewRowsNotificationFreq = 10000;

	public QueryResultSet ResultSet => _ResultSet;

	public IBsGridControl2 GridCtl => _GridCtl;

	public double ControlToWindowRatio
	{
		get
		{
			return m_controlToWindowRatio;
		}
		set
		{
			// Evs.Trace(GetType(), "ResultSetAndGridContainer.ControlToWindowRatio", "value = {0}", value);
			m_controlToWindowRatio = value;
		}
	}

	public ResultSetAndGridContainer(QueryResultSet resultSet, bool printColumnHeaders, int numberOfCharsToShow)
	{
		// Evs.Trace(GetType(), "ResultSetAndGridContainer.ResultSetAndGridContainer", "", null);
		_ResultSet = resultSet;
		_PrintColumnHeaders = printColumnHeaders;
		_NumberOfCharsToShow = numberOfCharsToShow;
		_MoreRowsAvailableEventAsync = OnMoreRowsAvailableFromStorageAsync;
	}

	public void Initialize(IBsGridControl2 grid)
	{
		// Evs.Trace(GetType(), "ResultSetAndGridContainer.Initialize", "", null);
		_GridCtl = grid;
		_ResultSet.InGridMode = true;
		_ResultSet.MoreRowsAvailableEventAsync += _MoreRowsAvailableEventAsync;
		((ISupportInitialize)_GridCtl).BeginInit();
		_GridCtl.SelectionType = EnGridSelectionType.CellBlocks;
		_GridCtl.GridStorage = _ResultSet;
		_GridCtl.WithHeader = _PrintColumnHeaders;
		_GridCtl.AlwaysHighlightSelection = true;
		_GridCtl.NumberOfCharsToShow = _NumberOfCharsToShow;
		GridColumnInfo gridColumnInfo = new()
		{
			ColumnType = 2,
			ColumnWidth = 7,
			WidthType = EnGridColumnWidthType.InAverageFontChar
		};

		_GridCtl.AddColumn(gridColumnInfo);

		if (ResultSet.StatementIndex > -1 && ResultSet.StatementCount > 1)
			_GridCtl.SetHeaderInfo(0, ControlsResources.Grid_StatementLabel.FmtRes(ResultSet.StatementIndex+1, ResultSet.StatementCount), null);

		for (int i = 0; i < _ResultSet.NumberOfDataColumns; i++)
		{
			gridColumnInfo = new GridColumnInfo();
			if (_ResultSet.IsXMLColumn(i))
			{
				gridColumnInfo.ColumnType = 5;
			}
			else
			{
				gridColumnInfo.ColumnType = 1;
			}

			_GridCtl.AddColumn(gridColumnInfo);

			string text = _ResultSet.ColumnNames[i];
			if (text == null || text.Length == 0)
				text = ControlsResources.Grid_ResultsNoColumnTitle;

			_GridCtl.SetHeaderInfo(i + 1, text, null);
		}

		_GridCtl.FirstScrollableColumn = 1;
		_GridCtl.ColumnsReorderableByDefault = true;
		((ISupportInitialize)_GridCtl).EndInit();
		_GridCtl.UpdateGrid();
		m_bGridHasRows = false;
		// Evs.Trace(GetType(), Tracer.EnLevel.Information, "ResultSetAndGridContainer.Initialize", "returning");
	}

	public void Dispose()
	{
		// Evs.Trace(GetType(), "ResultSetAndGridContainer.Dispose", "", null);
		if (_ResultSet != null)
		{
			_ResultSet.MoreRowsAvailableEventAsync -= _MoreRowsAvailableEventAsync;
			_MoreRowsAvailableEventAsync = null;
			_ResultSet.Dispose();
			_ResultSet = null;
		}

		if (_GridCtl != null)
		{
			((IDisposable)_GridCtl).Dispose();
			_GridCtl = null;
		}
	}

	public async Task<bool> StartRetrievingDataAsync(int nMaxNumCharsToDisplay, int nMaxNumXmlCharsToDisplay, CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), "ResultSetAndGridContainer.StartRetrievingData", "nMaxNumCharsToDisplay = {0}, nMaxNumXmlCharsToDisplay={1}", nMaxNumCharsToDisplay, nMaxNumXmlCharsToDisplay);
		await _ResultSet.StartRetrievingDataAsync(nMaxNumCharsToDisplay, nMaxNumXmlCharsToDisplay, cancelToken);

		return !cancelToken.Cancelled();
	}

	public void UpdateGrid()
	{
		_GridCtl.UpdateGrid();
		_GridCtl.InitialColumnResize();
	}

	private async Task<bool> OnMoreRowsAvailableFromStorageAsync(object sender, MoreRowsAvailableEventArgs args)
	{
		if (args.CancelToken.Cancelled())
			return await Task.FromResult(false);

		if (args.NewRowsNumber == C_NumOfFirstRowsToInitialResizeColumns || args.AllRows)
		{
			_GridCtl.UpdateGrid();
			_GridCtl.InitialColumnResize();
		}

		if (args.NewRowsNumber > C_NumOfFirstRowsToNotifyAbout
			&& args.NewRowsNumber % C_NewRowsNotificationFreq == 0L || args.AllRows
			|| args.NewRowsNumber == C_NumOfFirstRowsToNotifyAbout)
		{
			_GridCtl.UpdateGrid();

			if (!m_bGridHasRows)
			{
				SelectFirstCellInTheGrid();
				m_bGridHasRows = true;
			}
		}

		return await Task.FromResult(true);
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
