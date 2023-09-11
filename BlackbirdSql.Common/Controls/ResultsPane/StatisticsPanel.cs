// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsPanel

using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;

using BlackbirdSql.Common.Controls.Enums;
using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Enums;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;


namespace BlackbirdSql.Common.Controls.ResultsPane;

public class StatisticsPanel : AbstractGridResultsPanel, IOleCommandTarget
{
	private enum EnStatisticSpecialAction
	{
		NoAction,
		ClientProcessingTimeAction,
		ElapsedTimeFormat,
		DateTimeFormat,
		ByteFormat,
		SIFormat
	}



	private struct StatisticEntity
	{
		public static ResourceManager ResMgr => AttributeResources.ResourceManager;

		public string Name;

		public readonly string DisplayName => ResMgr.GetString("StatisticsPanelStat" + Name);

		public EnStatisticSpecialAction SpecialAction;

		public bool CalculateAverage;

		public StatisticEntity(string name, EnStatisticSpecialAction specialAction, bool calculateAverage = true)
		{
			Name = name;
			SpecialAction = specialAction;
			CalculateAverage = calculateAverage;
		}
	}

	private const int C_MinNumberOfVisibleRows = 8;

	private delegate string GetCategoryValueDelegate(StatisticsSnapshot snapshot);

	private static readonly string[] S_CategoryNames = new string[5]
	{
		AttributeResources.StatisticsPanelCategoryClientExecutionTime,
		AttributeResources.StatisticsPanelCategoryQueryProfileStats,
		AttributeResources.StatisticsPanelCategoryNetworkStats,
		AttributeResources.StatisticsPanelCategoryTimeStats,
		AttributeResources.StatisticsPanelCategoryServerStats
	};

	private static readonly GetCategoryValueDelegate[] S_CategoryValueDelegates = new GetCategoryValueDelegate[5]
	{
		new GetCategoryValueDelegate(GetTimeOfExecution),
		null,
		null,
		null,
		null,
	};


	// Row definitions for each stat group
	private static readonly StatisticEntity[][] Statistics = new StatisticEntity[5][]
	{
		// ClientExecutionTime
		new StatisticEntity[0],

		// QueryProfileStatistics
		new StatisticEntity[6]
		{
			new StatisticEntity("IduRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("InsRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("UpdRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("DelRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("SelectRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("Transactions", EnStatisticSpecialAction.SIFormat)
		},

		// NetworkStatistics
		new StatisticEntity[10]
		{
			new StatisticEntity("ServerRoundtrips", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("BufferCount", EnStatisticSpecialAction.SIFormat, false),
			new StatisticEntity("ReadCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("WriteCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ReadIdxCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ReadSeqCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("PurgeCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ExpungeCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("Marks", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("PacketSize", EnStatisticSpecialAction.ByteFormat, false)
		},

		// TimeStatistics
		new StatisticEntity[3]
		{
			new StatisticEntity("ExecutionStartTimeEpoch", EnStatisticSpecialAction.DateTimeFormat, false),
			new StatisticEntity("ExecutionEndTimeEpoch", EnStatisticSpecialAction.DateTimeFormat, false),
			new StatisticEntity("ExecutionTimeTicks", EnStatisticSpecialAction.ElapsedTimeFormat),
		},

		// ServerStatistics
		new StatisticEntity[6]
		{
			new StatisticEntity("AllocationPages", EnStatisticSpecialAction.SIFormat, false),
			new StatisticEntity("CurrentMemory", EnStatisticSpecialAction.ByteFormat),
			new StatisticEntity("MaxMemory", EnStatisticSpecialAction.ByteFormat),
			new StatisticEntity("DatabaseSizeInPages", EnStatisticSpecialAction.SIFormat, false),
			new StatisticEntity("PageSize", EnStatisticSpecialAction.ByteFormat, false),
			new StatisticEntity("ActiveUserCount", EnStatisticSpecialAction.SIFormat)
		}
	};


	private StatisticsGridsCollection _gridControls = new StatisticsGridsCollection();

	private static Bitmap S_ArrowUp;
	private static Bitmap S_ArrowDown;
	private static Bitmap S_ArrowFlat;
	private static Bitmap S_ArrowBlank;

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
		S_ArrowUp = AttributeResources.StatisticsPanelArrowUp;
		S_ArrowDown = AttributeResources.StatisticsPanelArrowDown;
		S_ArrowFlat = AttributeResources.StatisticsPanelArrowFlat;
		S_ArrowBlank = AttributeResources.StatisticsPanelArrowBlank;
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

		if (_gridControls != null)
		{
			foreach (StatisticsDlgGridControl gridControl in _gridControls)
			{
				gridControl.Dispose();
			}
			_gridControls.Clear();
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 123)
		{
			if (FocusedGrid != null && CommonUtils.GetCoordinatesForPopupMenuFromWM_Context(ref m, out var xPos, out var yPos, FocusedGrid))
			{
				CommonUtils.ShowContextMenuEvent((int)EnCommandSet.ContextIdResultsWindow, xPos, yPos, this);
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
		statisticsDlgGridControl.MouseButtonDoubleClickedEvent += OnMouseButtonDoubleClicked;
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

	private (float, float) CalculateTheValue(StatisticEntity sn, IDictionary snapshotData, out string stringValue)
	{
		float result = 0f;
		float cellValue = 0f;
		long longres = 0;
		switch (sn.SpecialAction)
		{
			case EnStatisticSpecialAction.NoAction:
				result = cellValue = longres = (long)snapshotData[sn.Name];
				stringValue = longres.ToString(CultureInfo.InvariantCulture);
				break;
			case EnStatisticSpecialAction.ClientProcessingTimeAction:
				result = cellValue = longres = (long)snapshotData["ExecutionTime"] - (long)snapshotData["NetworkServerTime"];
				stringValue = longres.FormatForStats();
				break;
			case EnStatisticSpecialAction.ElapsedTimeFormat:
				result = cellValue = longres = (long)snapshotData[sn.Name];
				stringValue = longres.FormatForStats();
				break;
			case EnStatisticSpecialAction.DateTimeFormat:
				result = cellValue = longres = (long)snapshotData[sn.Name];
				stringValue = longres.ToDateTime().ToString("T", CultureInfo.InvariantCulture);
				break;
			case EnStatisticSpecialAction.ByteFormat:
				result = longres = (long)snapshotData[sn.Name];
				(stringValue, cellValue) = longres.ByteSizeFormat();
				break;
			case EnStatisticSpecialAction.SIFormat:
				result = longres = (long)snapshotData[sn.Name];
				(stringValue, cellValue) = longres.SISizeFormat();
				break;
			default:
				stringValue = longres.ToString(CultureInfo.InvariantCulture);
				break;
		}
		return (result, cellValue);
	}

	private string FormatTheValue(StatisticEntity sn, float value)
	{
		if (!sn.CalculateAverage)
			return "";

		string stringValue;

		switch (sn.SpecialAction)
		{
			case EnStatisticSpecialAction.NoAction:
				stringValue = value.ToString("F4", CultureInfo.InvariantCulture);
				break;
			case EnStatisticSpecialAction.ClientProcessingTimeAction:
				stringValue = value.ToString(CultureInfo.InvariantCulture);
				break;
			case EnStatisticSpecialAction.ElapsedTimeFormat:
				stringValue = ((long)value).FormatForStats();
				break;
			case EnStatisticSpecialAction.DateTimeFormat:
				stringValue = "";
				break;
			case EnStatisticSpecialAction.ByteFormat:
				(stringValue, _) = value.ByteSizeFormat();
				break;
			case EnStatisticSpecialAction.SIFormat:
				(stringValue, _) = value.SISizeFormat();
				break;
			default:
				stringValue = value.ToString(CultureInfo.InvariantCulture);
				break;
		}

		return stringValue;

	}


	private void AddStatistic(StatisticsDlgGridControl gridControl, StatisticsConnection connection, StatisticEntity[] statisticNames, int numberOfTries)
	{
		for (int i = 0; i < statisticNames.Length; i++)
		{
			StatisticEntity sn = statisticNames[i];
			GridCellCollection gridCellCollection = new()
				{
					new GridCell("  " + sn.DisplayName)
				};

			float result, nextResult;
			float rowTotal = 0f;
			string strAvg = "";

			for (int j = 0; j < numberOfTries; j++)
			{
				(result, _) = CalculateTheValue(sn, connection[j].Snapshot, out string strValue);

				if (sn.CalculateAverage)
					rowTotal += result;

				if (j != numberOfTries - 1)
					(nextResult, _) = CalculateTheValue(sn, connection[j + 1].Snapshot, out _);
				else
					nextResult = result;

				gridCellCollection.Add(new GridCell(strValue));
				GridCell gridCell =
					!sn.CalculateAverage
						? new GridCell(S_ArrowBlank)
						: (nextResult <= result)
							? (nextResult >= result)
								? new GridCell(S_ArrowFlat)
								: new GridCell(S_ArrowUp)
							: new GridCell(S_ArrowDown);
				gridCellCollection.Add(gridCell);
			}

			if (sn.CalculateAverage)
				strAvg = FormatTheValue(sn, rowTotal / numberOfTries);
			gridCellCollection.Add(new(strAvg));
			gridControl.AddRow(gridCellCollection);

		}
	}

	private void PopulateGrid(StatisticsDlgGridControl gridControl, StatisticsConnection connection)
	{
		GridColumnInfo gridColumnInfo = new GridColumnInfo();

		GridCellCollection[] gridCollections = new GridCellCollection[S_CategoryNames.Length];
		for (int i = 0; i < S_CategoryNames.Length; i++)
			gridCollections[i] = new GridCellCollection();

		/*
		GridCellCollection gridCellCollection = new GridCellCollection();
		GridCellCollection gridCellCollection2 = new GridCellCollection();
		GridCellCollection gridCellCollection3 = new GridCellCollection();
		GridCellCollection gridCellCollection4 = new GridCellCollection();
		*/

		gridControl.AlwaysHighlightSelection = true;
		gridControl.GridLineType = EnGridLineType.Solid;
		gridControl.SelectionType = EnGridSelectionType.RowBlocks;
		gridControl.WithHeader = true;
		gridColumnInfo.ColumnType = 1;
		gridColumnInfo.WidthType = EnGridColumnWidthType.InAverageFontChar;
		gridColumnInfo.IsWithRightGridLine = true;
		gridColumnInfo.IsHeaderMergedWithRight = false;
		gridColumnInfo.ColumnWidth = 50;
		gridControl.AddColumn(gridColumnInfo);

		for (int i = 0; i < gridCollections.Length; i++)
			AddDarkColoredCell(gridCollections[i], S_CategoryNames[i]);

		/*
		AddDarkColoredCell(gridCellCollection, AttributeResources.StatisticsPanelCategoryClientExecutionTime);
		AddDarkColoredCell(gridCellCollection2, AttributeResources.StatisticsPanelCategoryQueryProfileStats);
		AddDarkColoredCell(gridCellCollection3, AttributeResources.StatisticsPanelCategoryNetworkStats);
		AddDarkColoredCell(gridCellCollection4, AttributeResources.StatisticsPanelCategoryTimeStats);
		*/

		int connectionCount = Math.Min(connection.Count, 10);
		for (int i = 0; i < connectionCount; i++)
		{
			gridColumnInfo.ColumnType = 1;
			gridColumnInfo.ColumnWidth = 20;
			gridColumnInfo.WidthType = EnGridColumnWidthType.InAverageFontChar;
			gridColumnInfo.IsWithRightGridLine = false;
			gridColumnInfo.IsHeaderMergedWithRight = true;
			gridControl.AddColumn(gridColumnInfo);


			for (int j = 0; j < gridCollections.Length; j++)
			{
				if (S_CategoryValueDelegates.Length <= j || S_CategoryValueDelegates[j] == null)
					AddDarkColoredCell(gridCollections[j]);
				else
					AddDarkColoredCell(gridCollections[j], S_CategoryValueDelegates[j](connection[i]));
			}

			/*
			AddDarkColoredCell(gridCellCollection, connection[i].TimeOfExecution.ToString("T", CultureInfo.InvariantCulture));
			AddDarkColoredCell(gridCellCollection2);
			AddDarkColoredCell(gridCellCollection3);
			AddDarkColoredCell(gridCellCollection4);
			*/

			gridColumnInfo = new()
			{
				ColumnType = 3,
				WidthType = EnGridColumnWidthType.InPixels,
				IsWithRightGridLine = true,
				IsHeaderMergedWithRight = false,
				ColumnWidth = S_ArrowUp.Width
			};
			gridControl.AddColumn(gridColumnInfo);
			gridControl.SetHeaderInfo(2 * i + 2, string.Format(CultureInfo.CurrentCulture,
				AttributeResources.StatisticsPanelSnapshotClientStatsTrial, connectionCount - i), null);

			for (int j = 0; j < gridCollections.Length; j++)
				AddDarkColoredCell(gridCollections[j], useNullBitmap: true);

			/*
			AddDarkColoredCell(gridCellCollection, useNullBitmap: true);
			AddDarkColoredCell(gridCellCollection2, useNullBitmap: true);
			AddDarkColoredCell(gridCellCollection3, useNullBitmap: true);
			AddDarkColoredCell(gridCellCollection4, useNullBitmap: true);
			*/
		}

		gridColumnInfo.ColumnType = 1;
		gridColumnInfo.ColumnWidth = 22;
		gridColumnInfo.WidthType = EnGridColumnWidthType.InAverageFontChar;
		gridControl.AddColumn(gridColumnInfo);
		gridControl.SetHeaderInfo(2 * connectionCount + 1, AttributeResources.StatisticsPanelSnapshotAverage, null);

		for (int i = 0; i < gridCollections.Length; i++)
			AddDarkColoredCell(gridCollections[i]);

		/*
		AddDarkColoredCell(gridCellCollection);
		AddDarkColoredCell(gridCellCollection2);
		AddDarkColoredCell(gridCellCollection3);
		AddDarkColoredCell(gridCellCollection4);
		*/

		for (int i = 0; i < gridCollections.Length; i++)
		{
			gridControl.AddRow(gridCollections[i]);

			if (Statistics[i].Length > 0)
				AddStatistic(gridControl, connection, Statistics[i], connectionCount);
		}

		/*
		gridControl.AddRow(gridCellCollection);
		gridControl.AddRow(gridCellCollection2);
		StatisticEntity[] array = new StatisticEntity[5];
		array[0].Name = "IduCount";
		array[0].DisplayName = AttributeResources.ResourceManager.StatisticsPanelStatIduCount;
		array[1].Name = "IduRows";
		array[1].DisplayName = AttributeResources.StatisticsPanelStatIduRowCount;
		array[2].Name = "SelectCount";
		array[2].DisplayName = AttributeResources.StatisticsPanelStatReadIdxCount;
		array[3].Name = "SelectRows";
		array[3].DisplayName = AttributeResources.StatisticsPanelStatSelectRowCount;
		array[4].Name = "Transactions";
		array[4].DisplayName = AttributeResources.StatisticsPanelStatTransactions;
		AddStatistic(gridControl, connection, array, num);
		gridControl.AddRow(gridCellCollection3);
		StatisticEntity[] array2 = new StatisticEntity[5];
		array2[0].Name = "ServerRoundtrips";
		array2[0].DisplayName = AttributeResources.StatisticsPanelStatServerRoundtrips;
		array2[1].Name = "BuffersSent";
		array2[1].DisplayName = AttributeResources.StatisticsPanelStatBuffersSent;
		array2[2].Name = "BuffersReceived";
		array2[2].DisplayName = AttributeResources.StatisticsPanelStatBufferCount;
		array2[3].Name = "BytesSent";
		array2[3].DisplayName = AttributeResources.StatisticsPanelStatWriteCount;
		array2[4].Name = "BytesReceived";
		array2[4].DisplayName = AttributeResources.StatisticsPanelStatReadCount;
		AddStatistic(gridControl, connection, array2, num);
		gridControl.AddRow(gridCellCollection4);
		StatisticEntity[] array3 = new StatisticEntity[3];
		array3[0].Name = null;
		array3[0].DisplayName = AttributeResources.StatisticsPanelStatClientProcessingTime;
		array3[0].SpecialAction = EnStatisticSpecialAction.ClientProcessingTimeAction;
		array3[1].Name = "ExecutionTime";
		array3[1].DisplayName = AttributeResources.StatisticsPanelStatExecutionTimeTicks;
		array3[1].SpecialAction = EnStatisticSpecialAction.ElapsedTimeFormat;
		array3[2].Name = "NetworkServerTime";
		array3[2].DisplayName = AttributeResources.StatisticsPanelStatNetworkServerTime;
		array3[2].SpecialAction = EnStatisticSpecialAction.ElapsedTimeFormat;
		AddStatistic(gridControl, connection, array3, num);
		*/

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

	private static string GetTimeOfExecution(StatisticsSnapshot snapshot)
	{
		return snapshot.TimeOfExecution.ToString("T", CultureInfo.InvariantCulture);
	}

}
