// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsPanel

using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Model.QueryExecution;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using EnStatisticSpecialAction = BlackbirdSql.Shared.Model.QueryExecution.StatisticsPropertySet.EnStatisticSpecialAction;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using StatisticEntity = BlackbirdSql.Shared.Model.QueryExecution.StatisticsPropertySet.StatisticEntity;



namespace BlackbirdSql.Shared.Controls.Results;


public class StatisticsPanel : AbstractGridResultsPanel, IOleCommandTarget
{
	private const int C_MinNumberOfVisibleRows = 8;

	private static Bitmap S_ArrowUp;
	private static Bitmap S_ArrowDown;
	private static Bitmap S_ArrowFlat;
	private static Bitmap S_ArrowBlank;


	private StatisticsGridsCollection _GridControls = [];


	public int NumberOfGrids
	{
		get
		{
			if (_GridControls != null)
			{
				return _GridControls.Count;
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
		// Tracer.Trace(GetType(), "ClientStatistics.Dispose", "", null);
		Clear();
		_GridControls = null;
		base.Dispose(bDisposing);
	}

	public override void Initialize(object sp)
	{
		// Tracer.Trace(GetType(), ".Initialize", "", null);
		base.Initialize(sp);
		_FirstGridPanel.Dock = DockStyle.Fill;
		_FirstGridPanel.Height = ClientRectangle.Height;
		_FirstGridPanel.Tag = -1;
		Controls.Add(_FirstGridPanel);
	}

	public override void Clear()
	{
		// Tracer.Trace(GetType(), "ClientStatistics.Clear", "", null);
		base.Clear();

		if (_GridControls != null)
		{
			foreach (StatisticsDlgGridControl gridControl in _GridControls)
			{
				gridControl.Dispose();
			}
			_GridControls.Clear();
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == Native.WM_CONTEXTMENU)
		{
			if (FocusedGrid != null && GetCoordinatesForPopupMenuFromWM_Context(ref m, out var xPos, out var yPos, FocusedGrid))
			{
				UnsafeCmd.ShowContextMenuEvent(CommandProperties.ClsidCommandSet, (int)EnCommandSet.ContextIdResultsWindow, xPos, yPos, this);
			}
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	public void PopulateFromStatisticsCollection(StatisticsSnapshotCollection snapshots, Font curGridFont, Color curBkColor, Color curFkColor, Color selectedCellColor, Color inactiveSelectedCellColor, Color highlightedCellColor)
	{
		// foreach (ConnectionSnapshotCollection item in control)
		// {
			AddGrid(snapshots, curGridFont, curBkColor, curFkColor, selectedCellColor, inactiveSelectedCellColor, highlightedCellColor);
		// }
	}

	private void AddGrid(StatisticsSnapshotCollection snapshots, Font curGridFont, Color curBkColor, Color curFkColor, Color selectedCellColor, Color inactiveSelectedCellColor, Color highlightedCellColor)
	{
		StatisticsDlgGridControl statisticsDlgGridControl = new StatisticsDlgGridControl(curBkColor, curFkColor, selectedCellColor, inactiveSelectedCellColor, highlightedCellColor);
		DlgStorage dlgStorage = new DlgStorage(statisticsDlgGridControl);
		statisticsDlgGridControl.DlgStorage = dlgStorage;
		statisticsDlgGridControl.BeginInit();
		statisticsDlgGridControl.Dock = DockStyle.Fill;
		statisticsDlgGridControl.Tag = _GridControls.Count - 1;
		statisticsDlgGridControl.GotFocus += OnGridGotFocus;
		statisticsDlgGridControl.MouseButtonDoubleClickedEvent += OnMouseButtonDoubleClicked;
		statisticsDlgGridControl.Font = curGridFont;
		statisticsDlgGridControl.HeaderFont = curGridFont;
		try
		{
			PopulateGrid(statisticsDlgGridControl, snapshots);
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);

			MessageCtl.ShowEx(ex, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Hand, null);

			return;
		}

		if (_FirstGridPanel.HostedControlsCount == 0)
		{
			_FirstGridPanel.HostedControlsMinInitialSize = C_MinNumberOfVisibleRows * (statisticsDlgGridControl.RowHeight + 1) + statisticsDlgGridControl.HeaderHeight + 1 + Cmd.GetExtraSizeForBorderStyle(statisticsDlgGridControl.BorderStyle);
			_FirstGridPanel.HostedControlsMinSize = statisticsDlgGridControl.RowHeight + 1 + statisticsDlgGridControl.HeaderHeight + 1 + Cmd.GetExtraSizeForBorderStyle(statisticsDlgGridControl.BorderStyle);
		}
		statisticsDlgGridControl.EndInit();
		_FirstGridPanel.AddControl(statisticsDlgGridControl, limitMaxControlHeightToClientArea: false);
		_GridControls.Add(statisticsDlgGridControl);
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

	private (float, float) CalculateValue(StatisticEntity sn, IDictionary snapshotData, out string stringValue)
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
				stringValue = longres.FmtStats();
				break;
			case EnStatisticSpecialAction.ElapsedTimeFormat:
				result = cellValue = longres = (long)snapshotData[sn.Name];
				stringValue = longres.FmtStats();
				break;
			case EnStatisticSpecialAction.DateTimeFormat:
				result = cellValue = longres = (long)snapshotData[sn.Name];
				stringValue = longres.ToUtcDateTime().ToString("T", CultureInfo.InvariantCulture);
				break;
			case EnStatisticSpecialAction.ByteFormat:
				result = longres = (long)snapshotData[sn.Name];
				(stringValue, cellValue) = longres.FmtByteSize();
				break;
			case EnStatisticSpecialAction.SIFormat:
				result = longres = (long)snapshotData[sn.Name];
				(stringValue, cellValue) = longres.FmtExpSize();
				break;
			default:
				stringValue = longres.ToString(CultureInfo.InvariantCulture);
				break;
		}
		return (result, cellValue);
	}

	private string FormatValue(StatisticEntity sn, float value)
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
				stringValue = ((long)value).FmtStats();
				break;
			case EnStatisticSpecialAction.DateTimeFormat:
				stringValue = "";
				break;
			case EnStatisticSpecialAction.ByteFormat:
				(stringValue, _) = value.FmtByteSize();
				break;
			case EnStatisticSpecialAction.SIFormat:
				(stringValue, _) = value.FmtExpSize();
				break;
			default:
				stringValue = value.ToString(CultureInfo.InvariantCulture);
				break;
		}

		return stringValue;

	}


	private void AddStatistic(StatisticsDlgGridControl gridControl, StatisticsSnapshotCollection snapshots, StatisticEntity[] statisticNames, int numberOfTries)
	{
		for (int i = 0; i < statisticNames.Length; i++)
		{
			StatisticEntity sn = statisticNames[i];
			GridCellCollection gridCellCollection =
				[
					new GridCell("  " + sn.DisplayName)
				];

			float result, nextResult;
			float rowTotal = 0f;
			string strAvg = "";
			string strValue;

			for (int j = 0; j < numberOfTries; j++)
			{
				try
				{
					(result, _) = CalculateValue(sn, snapshots[j].Snapshot, out strValue);
				}
				catch (Exception ex)
				{
					Diag.Dug(ex, $"Statistic: {sn.Name}.");
					throw;
				}

				if (sn.CalculateAverage)
					rowTotal += result;

				if (j != numberOfTries - 1)
				{
					try
					{
						(nextResult, _) = CalculateValue(sn, snapshots[j + 1].Snapshot, out _);
					}
					catch (Exception ex)
					{
						Diag.Dug(ex, $"Statistic: {sn.Name}.");
						throw;
					}
				}
				else
				{
					nextResult = result;
				}

				gridCellCollection.Add(new GridCell(strValue));

				GridCell gridCell =
					!sn.CalculateAverage || j == numberOfTries - 1
						? new GridCell(S_ArrowBlank)
						: nextResult <= result
							? nextResult >= result
								? new GridCell(S_ArrowFlat)
								: new GridCell(S_ArrowUp)
							: new GridCell(S_ArrowDown);
				gridCellCollection.Add(gridCell);
			}

			if (sn.CalculateAverage)
				strAvg = FormatValue(sn, rowTotal / numberOfTries);
			gridCellCollection.Add(new(strAvg));
			gridControl.AddRow(gridCellCollection);

		}
	}


	/// <summary>
	/// This has all changed for the FB-SQL port and is now automated using the values
	/// in StatisticsPropertySet
	/// </summary>
	private void PopulateGrid(StatisticsDlgGridControl gridControl, StatisticsSnapshotCollection snapshots)
	{
		GridColumnInfo gridColumnInfo = new GridColumnInfo();
		GridCellCollection[] gridCollections = new GridCellCollection[StatisticsPropertySet.SCategoryNames.Length];

		for (int i = 0; i < StatisticsPropertySet.SCategoryNames.Length; i++)
			gridCollections[i] = [];

		gridControl.AlwaysHighlightSelection = true;
		gridControl.GridLineType = EnGridLineType.Solid;
		gridControl.SelectionType = EnGridSelectionType.RowBlocks;
		gridControl.WithHeader = true;

		// Add the Description column to the grid control
		gridColumnInfo.ColumnType = 1;
		gridColumnInfo.WidthType = EnGridColumnWidthType.InAverageFontChar;
		gridColumnInfo.IsWithRightGridLine = true;
		gridColumnInfo.IsHeaderMergedWithRight = false;
		gridColumnInfo.ColumnWidth = 50;
		gridControl.AddColumn(gridColumnInfo);

		// Add Cell to each category cell collection with the category title
		for (int i = 0; i < gridCollections.Length; i++)
		{
			if (StatisticsPropertySet.SCategoryNames[i] != null)
				AddDarkColoredCell(gridCollections[i], StatisticsPropertySet.SCategoryNames[i]);
		}

		// Now for each trial add a column to the grid control;
		int connectionCount = Math.Min(snapshots.Count, 10);
		for (int i = 0; i < connectionCount; i++)
		{
			gridColumnInfo.ColumnType = 1;
			gridColumnInfo.ColumnWidth = 20;
			gridColumnInfo.WidthType = EnGridColumnWidthType.InAverageFontChar;
			gridColumnInfo.IsWithRightGridLine = false;
			gridColumnInfo.IsHeaderMergedWithRight = false; // Set to false to allow resizing.
			gridControl.AddColumn(gridColumnInfo);

			// For each category cell collection add it's first right side cell value.
			// The value is calculated using delegates from the delegate array.
			for (int j = 0; j < gridCollections.Length; j++)
			{
				if (StatisticsPropertySet.SCategoryValueDelegates.Length <= j
					|| StatisticsPropertySet.SCategoryValueDelegates[j] == null)
				{
					AddDarkColoredCell(gridCollections[j]);
				}
				else
				{
					AddDarkColoredCell(gridCollections[j], StatisticsPropertySet.SCategoryValueDelegates[j](snapshots[i]));
				}
			}

			// For each trial column append a bitmap column.
			gridColumnInfo = new()
			{
				ColumnType = 3,
				WidthType = EnGridColumnWidthType.InPixels,
				IsWithRightGridLine = true,
				IsHeaderMergedWithRight = false,
				ColumnWidth = S_ArrowUp.Width,
				IsUserResizable = false // Set to false to allow col to it's left to resize.
			};
			gridControl.AddColumn(gridColumnInfo);

			// Set the header text for the trial column. Changed +2 to +1 because bitmap header column
			// and column to it's left are no longer merged.
			gridControl.SetHeaderInfo(2 * i + 1, string.Format(CultureInfo.CurrentCulture,
				AttributeResources.StatisticsPanelSnapshotClientStatsTrial, snapshots.Count - i), null);

			for (int j = 0; j < gridCollections.Length; j++)
				AddDarkColoredCell(gridCollections[j], useNullBitmap: true);
		}

		gridColumnInfo.ColumnType = 1;
		gridColumnInfo.ColumnWidth = 22;
		gridColumnInfo.WidthType = EnGridColumnWidthType.InAverageFontChar;
		gridControl.AddColumn(gridColumnInfo);
		gridControl.SetHeaderInfo(2 * connectionCount + 1, AttributeResources.StatisticsPanelSnapshotAverage, null);

		for (int i = 0; i < gridCollections.Length; i++)
			AddDarkColoredCell(gridCollections[i]);

		for (int i = 0; i < gridCollections.Length; i++)
		{
			gridControl.AddRow(gridCollections[i]);

			if (StatisticsPropertySet.SStatisticEntities[i].Length > 0)
				AddStatistic(gridControl, snapshots, StatisticsPropertySet.SStatisticEntities[i], connectionCount);
		}


		gridControl.UpdateGrid(bRecalcRows: true);
		gridControl.SelectedRows = new int[1];
	}



	public void ApplyCurrentGridFont(Font font)
	{
		if (_GridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _GridControls)
		{
			gridControl.Font = font;
			gridControl.HeaderFont = font;
		}
	}

	public void ApplyCurrentGridColor(Color bkColor, Color fkColor)
	{
		if (_GridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _GridControls)
		{
			gridControl.SetBkAndForeColors(bkColor, fkColor);
		}
	}

	public void ApplySelectedCellColor(Color selectedCellColor)
	{
		if (_GridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _GridControls)
		{
			gridControl.SetSelectedCellColor(selectedCellColor);
		}
	}

	public void ApplyInactiveSelectedCellColor(Color inactiveCellColor)
	{
		if (_GridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _GridControls)
		{
			gridControl.SetInactiveSelectedCellColor(inactiveCellColor);
		}
	}

	public void ApplyHighlightedCellColor(Color highlightedColor)
	{
		if (_GridControls == null)
		{
			return;
		}
		foreach (StatisticsDlgGridControl gridControl in _GridControls)
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
			BlockOfCellsCollection blockOfCellsCollection = [];
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
			return (int)OleConstants.MSOCMDERR_E_UNKNOWNGROUP;

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
