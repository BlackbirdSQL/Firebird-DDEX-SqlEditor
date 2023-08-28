// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsPanel

using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Controls.Enums;
using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Properties;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using BlackbirdSql.Common.Enums;

namespace BlackbirdSql.Common.Controls.ResultsPane;


public class StatisticsPanel : AbstractGridResultsPanel, IOleCommandTarget
{
	private enum StatisticsNameSpecialAction
	{
		NoAction,
		ClientProcessingTimeAction
	}

	private enum StatisticsNameDisplayFormat
	{
		Normal,
		Millions,
		Kilobytes
	}

	private struct StatisticName
	{
		public string Name;

		public string DisplayName;

		public StatisticsNameSpecialAction SpecialAction;
	}

	private StatisticsGridsCollection _gridControls = new StatisticsGridsCollection();

	private const int C_MinNumberOfVisibleRows = 8;

	private static Bitmap _arrowUp;

	private static Bitmap _arrowDown;

	private static Bitmap _arrowFlat;

	public int NumberOfGrids
	{
		get
		{
			if (_gridControls != null)
			{
				return _gridControls.Count;
			}
			return 0;
		}
	}

	public StatisticsPanel(string defaultResultsDirectory)
		: base(defaultResultsDirectory)
	{
		MenuCommand menuCommand = new MenuCommand(OnSelectAll,
			new CommandID(VSConstants.CMDSETID.StandardCommandSet97_guid,
			(int)VSConstants.VSStd97CmdID.SelectAll));
		MenuCommand menuCommand2 = new MenuCommand(OnCopy,
			new CommandID(VSConstants.CMDSETID.StandardCommandSet97_guid,
			(int)VSConstants.VSStd97CmdID.Copy));
		MenuCommand menuCommand3 = new MenuCommand(OnPrint,
			new CommandID(VSConstants.CMDSETID.StandardCommandSet97_guid,
			(int)VSConstants.VSStd97CmdID.Print));
		MenuCommand menuCommand4 = new MenuCommand(OnPrintPageSetup,
			new CommandID(VSConstants.CMDSETID.StandardCommandSet97_guid,
			(int)VSConstants.VSStd97CmdID.PageSetup));
		MenuService.AddRange(new MenuCommand[4] { menuCommand, menuCommand2, menuCommand4, menuCommand3 });
		_arrowUp = ControlsResources.arrowUp;
		_arrowDown = ControlsResources.arrowDown;
		_arrowFlat = ControlsResources.arrowFlat;
	}

	protected override void Dispose(bool bDisposing)
	{
		Tracer.Trace(GetType(), "ClientStatistics.Dispose", "", null);
		Clear();
		_gridControls = null;
		base.Dispose(bDisposing);
	}

	public override void Initialize(object sp)
	{
		Tracer.Trace(GetType(), ".Initialize", "", null);
		base.Initialize(sp);
		_firstGridPanel.Dock = DockStyle.Fill;
		_firstGridPanel.Height = ClientRectangle.Height;
		_firstGridPanel.Tag = -1;
		Controls.Add(_firstGridPanel);
	}

	public override void Clear()
	{
		Tracer.Trace(GetType(), "ClientStatistics.Clear", "", null);
		base.Clear();
		Tracer.Trace(GetType(), "ClientStatistics.Clear", "disposing grid containers", null);
		if (_gridControls != null)
		{
			foreach (StatisticsDlgGridControl gridControl in _gridControls)
			{
				gridControl.Dispose();
			}
			_gridControls.Clear();
		}
		Tracer.Trace(GetType(), "ClientStatistics.Clear", "returning", null);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 123)
		{
			if (FocusedGrid != null && CommonUtils.GetCoordinatesForPopupMenuFromWM_Context(ref m, out var xPos, out var yPos, FocusedGrid))
			{
				CommonUtils.ShowContextMenu((int)SqlEditorCmdSet.ContextIdResultsWindow, xPos, yPos, this);
			}
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	public void PopulateFromStatisticsControl(StatisticsControl control, Font curGridFont, Color curBkColor, Color curFkColor, Color selectedCellColor, Color inactiveSelectedCellColor, Color highlightedCellColor)
	{
		foreach (StatisticsConnection item in control)
		{
			AddGrid(item, curGridFont, curBkColor, curFkColor, selectedCellColor, inactiveSelectedCellColor, highlightedCellColor);
		}
	}

	private void AddGrid(StatisticsConnection connection, Font curGridFont, Color curBkColor, Color curFkColor, Color selectedCellColor, Color inactiveSelectedCellColor, Color highlightedCellColor)
	{
		StatisticsDlgGridControl statisticsDlgGridControl = new StatisticsDlgGridControl(curBkColor, curFkColor, selectedCellColor, inactiveSelectedCellColor, highlightedCellColor);
		DlgStorage dlgStorage = new DlgStorage(statisticsDlgGridControl);
		statisticsDlgGridControl.DlgStorage = dlgStorage;
		statisticsDlgGridControl.BeginInit();
		statisticsDlgGridControl.Dock = DockStyle.Fill;
		statisticsDlgGridControl.Tag = _gridControls.Count - 1;
		statisticsDlgGridControl.GotFocus += OnGridGotFocus;
		statisticsDlgGridControl.MouseButtonDoubleClicked += OnMouseButtonDoubleClicked;
		statisticsDlgGridControl.Font = curGridFont;
		statisticsDlgGridControl.HeaderFont = curGridFont;
		try
		{
			PopulateGrid(statisticsDlgGridControl, connection);
		}
		catch (Exception ex)
		{
			Cmd.ShowMessageBoxEx(string.Empty, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		if (_firstGridPanel.HostedControlsCount == 0)
		{
			_firstGridPanel.HostedControlsMinInitialSize = C_MinNumberOfVisibleRows * (statisticsDlgGridControl.RowHeight + 1) + statisticsDlgGridControl.HeaderHeight + 1 + CommonUtils.GetExtraSizeForBorderStyle(statisticsDlgGridControl.BorderStyle);
			_firstGridPanel.HostedControlsMinSize = statisticsDlgGridControl.RowHeight + 1 + statisticsDlgGridControl.HeaderHeight + 1 + CommonUtils.GetExtraSizeForBorderStyle(statisticsDlgGridControl.BorderStyle);
		}
		statisticsDlgGridControl.EndInit();
		_firstGridPanel.AddControl(statisticsDlgGridControl, limitMaxControlHeightToClientArea: false);
		_gridControls.Add(statisticsDlgGridControl);
	}

	private void AddDarkColoredCell(GridCellCollection cellCollection, string text, bool useNullBitmap)
	{
		GridCell gridCell;
		if (useNullBitmap)
		{
			gridCell = new GridCell((Bitmap)null);
		}
		else
		{
			text ??= "";
			gridCell = new GridCell(text);
		}
		gridCell.BkBrush = new SolidBrush(SystemColors.Control);
		cellCollection.Add(gridCell);
	}

	private void AddDarkColoredCell(GridCellCollection cellCollection, string text)
	{
		AddDarkColoredCell(cellCollection, text, useNullBitmap: false);
	}

	private void AddDarkColoredCell(GridCellCollection cellCollection)
	{
		AddDarkColoredCell(cellCollection, null, useNullBitmap: false);
	}

	private void AddDarkColoredCell(GridCellCollection cellCollection, bool useNullBitmap)
	{
		AddDarkColoredCell(cellCollection, null, useNullBitmap);
	}

	private float CalculateTheValue(StatisticName sn, IDictionary tryData, out string stringValue)
	{
		float result = 0f;
		switch (sn.SpecialAction)
		{
			case StatisticsNameSpecialAction.NoAction:
				result = (long)tryData[sn.Name];
				stringValue = result.ToString(CultureInfo.InvariantCulture);
				break;
			case StatisticsNameSpecialAction.ClientProcessingTimeAction:
				result = (long)tryData["ExecutionTime"] - (long)tryData["NetworkServerTime"];
				stringValue = result.ToString(CultureInfo.InvariantCulture);
				break;
			default:
				stringValue = result.ToString(CultureInfo.InvariantCulture);
				break;
		}
		return result;
	}

	private void AddStatistic(StatisticsDlgGridControl gridControl, StatisticsConnection connection, StatisticName[] statisticNames, int numberOfTries)
	{
		for (int i = 0; i < statisticNames.Length; i++)
		{
			StatisticName sn = statisticNames[i];
			GridCellCollection gridCellCollection = new()
			{
				new GridCell("  " + sn.DisplayName)
			};
			float num = 0f;
			for (int j = 0; j < numberOfTries; j++)
			{
				float num3 = CalculateTheValue(sn, connection[j].TryData, out var stringValue);
				num += num3;
				float num2 = j != numberOfTries - 1 ? CalculateTheValue(sn, connection[j + 1].TryData, out _) : num3;
				gridCellCollection.Add(new GridCell(stringValue));
				GridCell gridCell = !(num2 > num3) ? !(num2 < num3) ? new GridCell(_arrowFlat) : new GridCell(_arrowUp) : new GridCell(_arrowDown);
				gridCellCollection.Add(gridCell);
			}
			gridCellCollection.Add(new GridCell((num / numberOfTries).ToString("F4", CultureInfo.InvariantCulture)));
			gridControl.AddRow(gridCellCollection);
		}
	}

	private void PopulateGrid(StatisticsDlgGridControl gridControl, StatisticsConnection connection)
	{
		GridColumnInfo gridColumnInfo = new GridColumnInfo();
		GridCellCollection gridCellCollection = new GridCellCollection();
		GridCellCollection gridCellCollection2 = new GridCellCollection();
		GridCellCollection gridCellCollection3 = new GridCellCollection();
		GridCellCollection gridCellCollection4 = new GridCellCollection();
		gridControl.AlwaysHighlightSelection = true;
		gridControl.GridLineType = EnGridLineType.Solid;
		gridControl.SelectionType = EnGridSelectionType.RowBlocks;
		gridControl.WithHeader = true;
		gridColumnInfo.ColumnType = 1;
		gridColumnInfo.WidthType = EnGridColumnWidthType.InAverageFontChar;
		gridColumnInfo.IsWithRightGridLine = true;
		gridColumnInfo.IsHeaderMergedWithRight = false;
		gridColumnInfo.ColumnWidth = 60;
		gridControl.AddColumn(gridColumnInfo);
		AddDarkColoredCell(gridCellCollection, SharedResx.ClientExecutionTime);
		AddDarkColoredCell(gridCellCollection2, SharedResx.QueryProfileStatistics);
		AddDarkColoredCell(gridCellCollection3, SharedResx.NetworkStatistics);
		AddDarkColoredCell(gridCellCollection4, SharedResx.TimeStatistics);
		int num = Math.Min(connection.Count, 10);
		for (int i = 0; i < num; i++)
		{
			gridColumnInfo.ColumnType = 1;
			gridColumnInfo.ColumnWidth = 15;
			gridColumnInfo.WidthType = EnGridColumnWidthType.InAverageFontChar;
			gridColumnInfo.IsWithRightGridLine = false;
			gridColumnInfo.IsHeaderMergedWithRight = true;
			gridControl.AddColumn(gridColumnInfo);
			AddDarkColoredCell(gridCellCollection, connection[i].TimeOfExecution.ToString("T", CultureInfo.InvariantCulture));
			AddDarkColoredCell(gridCellCollection2);
			AddDarkColoredCell(gridCellCollection3);
			AddDarkColoredCell(gridCellCollection4);
			gridColumnInfo = new()
			{
				ColumnType = 3,
				WidthType = EnGridColumnWidthType.InPixels,
				IsWithRightGridLine = true,
				IsHeaderMergedWithRight = false,
				ColumnWidth = _arrowUp.Width
			};
			gridControl.AddColumn(gridColumnInfo);
			gridControl.SetHeaderInfo(2 * i + 2, string.Format(CultureInfo.CurrentCulture, SharedResx.ClientStatsTrial, num - i), null);
			AddDarkColoredCell(gridCellCollection, useNullBitmap: true);
			AddDarkColoredCell(gridCellCollection2, useNullBitmap: true);
			AddDarkColoredCell(gridCellCollection3, useNullBitmap: true);
			AddDarkColoredCell(gridCellCollection4, useNullBitmap: true);
		}
		gridColumnInfo.ColumnType = 1;
		gridColumnInfo.ColumnWidth = 20;
		gridColumnInfo.WidthType = EnGridColumnWidthType.InAverageFontChar;
		gridControl.AddColumn(gridColumnInfo);
		gridControl.SetHeaderInfo(2 * num + 1, SharedResx.Average, null);
		AddDarkColoredCell(gridCellCollection);
		AddDarkColoredCell(gridCellCollection2);
		AddDarkColoredCell(gridCellCollection3);
		AddDarkColoredCell(gridCellCollection4);
		gridControl.AddRow(gridCellCollection);
		gridControl.AddRow(gridCellCollection2);
		StatisticName[] array = new StatisticName[5];
		array[0].Name = "IduCount";
		array[0].DisplayName = SharedResx.IduCountStat;
		array[1].Name = "IduRows";
		array[1].DisplayName = SharedResx.IduRowsStat;
		array[2].Name = "SelectCount";
		array[2].DisplayName = SharedResx.SelectCountStat;
		array[3].Name = "SelectRows";
		array[3].DisplayName = SharedResx.SelectRowsStat;
		array[4].Name = "Transactions";
		array[4].DisplayName = SharedResx.TransactionsStat;
		AddStatistic(gridControl, connection, array, num);
		gridControl.AddRow(gridCellCollection3);
		StatisticName[] array2 = new StatisticName[5];
		array2[0].Name = "ServerRoundtrips";
		array2[0].DisplayName = SharedResx.ServerRoundtripsStat;
		array2[1].Name = "BuffersSent";
		array2[1].DisplayName = SharedResx.BuffersSentStat;
		array2[2].Name = "BuffersReceived";
		array2[2].DisplayName = SharedResx.BuffersReceivedStat;
		array2[3].Name = "BytesSent";
		array2[3].DisplayName = SharedResx.BytesSentStat;
		array2[4].Name = "BytesReceived";
		array2[4].DisplayName = SharedResx.BytesReceivedStat;
		AddStatistic(gridControl, connection, array2, num);
		gridControl.AddRow(gridCellCollection4);
		StatisticName[] array3 = new StatisticName[3];
		array3[0].Name = null;
		array3[0].DisplayName = SharedResx.ClientProcessingTime;
		array3[0].SpecialAction = StatisticsNameSpecialAction.ClientProcessingTimeAction;
		array3[1].Name = "ExecutionTime";
		array3[1].DisplayName = SharedResx.ExecutionTimeStat;
		array3[2].Name = "NetworkServerTime";
		array3[2].DisplayName = SharedResx.NetworkServerTimeStat;
		AddStatistic(gridControl, connection, array3, num);
		gridControl.UpdateGrid(bRecalcRows: true);
		gridControl.SelectedRows = new int[1];
	}

	public void ApplyCurrentGridFont(Font font)
	{
		if (_gridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _gridControls)
		{
			gridControl.Font = font;
			gridControl.HeaderFont = font;
		}
	}

	public void ApplyCurrentGridColor(Color bkColor, Color fkColor)
	{
		if (_gridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _gridControls)
		{
			gridControl.SetBkAndForeColors(bkColor, fkColor);
		}
	}

	public void ApplySelectedCellColor(Color selectedCellColor)
	{
		if (_gridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _gridControls)
		{
			gridControl.SetSelectedCellColor(selectedCellColor);
		}
	}

	public void ApplyInactiveSelectedCellColor(Color inactiveCellColor)
	{
		if (_gridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _gridControls)
		{
			gridControl.SetInactiveSelectedCellColor(inactiveCellColor);
		}
	}

	public void ApplyHighlightedCellColor(Color highlightedColor)
	{
		if (_gridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _gridControls)
		{
			gridControl.SetHighlightedCellColor(highlightedColor);
		}
	}

	private void OnGridGotFocus(object sender, EventArgs e)
	{
		if (sender is GridControl lastFocusedControl)
		{
			_lastFocusedControl = lastFocusedControl;
		}
	}

	private void OnMouseButtonDoubleClicked(object sender, MouseButtonDoubleClickedEventArgs e)
	{
		if (e.HitTest == EnHitTestResult.ColumnResize && e.Button == MouseButtons.Left)
		{
			((GridControl)sender).ResizeColumnToShowAllContents(e.ColumnIndex);
		}
	}

	private void OnSelectAll(object sender, EventArgs e)
	{
		if (FocusedGrid is StatisticsDlgGridControl statisticsDlgGridControl)
		{
			BlockOfCellsCollection blockOfCellsCollection = new BlockOfCellsCollection();
			BlockOfCells blockOfCells = new(0L, 0)
			{
				Width = statisticsDlgGridControl.ColumnsNumber,
				Height = statisticsDlgGridControl.RowCount
			};
			if (blockOfCells.Height > 0)
			{
				blockOfCellsCollection.Add(blockOfCells);
				statisticsDlgGridControl.SelectedCells = blockOfCellsCollection;
			}
		}
	}

	int IOleCommandTarget.QueryStatus(ref Guid guidGroup, uint cmds, OLECMD[] oleCmd, IntPtr cmdText)
	{
		CommandID commandID = new CommandID(guidGroup, (int)oleCmd[0].cmdID);
		MenuCommand menuCommand = MenuService.FindCommand(commandID);

		if (menuCommand == null)
			return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;

		if (guidGroup.Equals(VSConstants.CMDSETID.StandardCommandSet97_guid))
		{
			switch ((VSConstants.VSStd97CmdID)commandID.ID)
			{
				case VSConstants.VSStd97CmdID.Copy:
				case VSConstants.VSStd97CmdID.Print:
				case VSConstants.VSStd97CmdID.SelectAll:
				case VSConstants.VSStd97CmdID.PageSetup:
					{
						if (FocusedGrid == null)
						{
							break;
						}
						bool visible = menuCommand.Supported = true;
						menuCommand.Visible = visible;
						if (commandID.ID == (int)VSConstants.VSStd97CmdID.Copy
							|| commandID.ID == (int)VSConstants.VSStd97CmdID.SelectAll)
						{
							menuCommand.Enabled = false;
							GridControl focusedGrid = FocusedGrid;
							if (focusedGrid != null)
							{
								BlockOfCellsCollection selectedCells = focusedGrid.SelectedCells;
								if (selectedCells != null && selectedCells.Count > 0)
								{
									menuCommand.Enabled = true;
								}
							}
						}
						oleCmd[0].cmdf = (uint)menuCommand.OleStatus;
						return 0;
					}
			}
			oleCmd[0].cmdf = (uint)menuCommand.OleStatus;
			return 0;
		}
		return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
	}

	int IOleCommandTarget.Exec(ref Guid guidGroup, uint cmdId, uint cmdExecOpt, IntPtr variantIn, IntPtr variantOut)
	{
		MenuCommand menuCommand = MenuService.FindCommand(new CommandID(guidGroup, (int)cmdId));
		if (menuCommand != null)
		{
			menuCommand.Invoke();
			return 0;
		}
		return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
	}
}
