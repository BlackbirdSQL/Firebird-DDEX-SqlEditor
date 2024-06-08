#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using ColumnWidthChangedEventHandler = BlackbirdSql.Shared.Events.ColumnWidthChangedEventHandler;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Interfaces
{
	// [CLSCompliant(false)]
	public interface IBGridControl
	{
		bool FocusEditorOnNavigation { get; set; }

		IBGridStorage GridStorage { get; set; }

		int ColumnsNumber { get; }

		int VisibleRowsNum { get; }

		bool WithHeader { get; set; }

		Font HeaderFont { get; set; }

		EnGridLineType GridLineType { get; set; }

		EnGridSelectionType SelectionType { get; set; }

		BorderStyle BorderStyle { get; set; }

		int FirstScrollableColumn { get; set; }

		uint FirstScrollableRow { get; set; }

		int AutoScrollInterval { get; set; }

		int MarginsWidth { get; }

		BlockOfCellsCollection SelectedCells { get; set; }

		int HeaderHeight { get; }

		int RowHeight { get; }

		Color HighlightColor { get; }

		bool AlwaysHighlightSelection { get; set; }

		PrintDocument PrintDocument { get; }

		bool ColumnsReorderableByDefault { get; set; }

		GridColumnInfoCollection GridColumnsInfo { get; }

		event CustomizeCellGDIObjectsEventHandler CustomizeCellGDIObjectsEvent;

		event MouseButtonClickingEventHandler MouseButtonClickingEvent;

		event MouseButtonClickedEventHandler MouseButtonClickedEvent;

		event MouseButtonDoubleClickedEventHandler MouseButtonDoubleClickedEvent;

		event HeaderButtonClickedEventHandler HeaderButtonClickedEvent;

		event ColumnWidthChangedEventHandler ColumnWidthChangedEvent;

		event SelectionChangedEventHandler SelectionChangedEvent;

		event StandardKeyProcessingEventHandler StandardKeyProcessingEvent;

		event TooltipDataNeededEventHandler TooltipDataNeededEvent;

		event KeyPressedOnCellEventHandler KeyPressedOnCellEvent;

		event EmbeddedControlContentsChangedEventHandler EmbeddedControlContentsChangedEvent;

		event ColumnReorderRequestedEventHandler ColumnReorderRequestedEvent;

		event ColumnsReorderedEventHandler ColumnsReorderedEvent;

		event GridSpecialEventHandler GridSpecialEvent;

		void UpdateGrid();

		void UpdateGrid(bool bRecalcRows);

		void InsertColumn(int nIndex, GridColumnInfo ci);

		void AddColumn(GridColumnInfo ci);

		void DeleteColumn(int nIndex);

		void SetHeaderInfo(int nColIndex, string strText, Bitmap bmp);

		void SetHeaderInfo(int colIndex, string strText, EnGridCheckBoxState checkboxState);

		void GetHeaderInfo(int colIndex, out string headerText, out Bitmap headerBitmap);

		void GetHeaderInfo(int colIndex, out string headerText, out EnGridCheckBoxState headerCheckBox);

		void ResetGrid();

		void EnsureCellIsVisible(long nRowIndex, int nColIndex);

		void RegisterEmbeddedControl(int editableCellType, Control embeddedControl);

		bool StartCellEdit(long nRowIndex, int nColIndex);

		bool IsACellBeingEdited(out long nRowNum, out int nColNum);

		bool StopCellEdit(bool bCommitIntoStorage);

		void SetColumnWidth(int nColIndex, EnGridColumnWidthType widthType, int nWidth);

		int GetColumnWidth(int nColIndex);

		void SetMergedHeaderResizeProportion(int colIndex, float proportion);

		void SetBitmapsForCheckBoxColumn(int nColIndex, Bitmap checkedState, Bitmap uncheckedState, Bitmap indeterminateState, Bitmap disabledState);

		HitTestInfo HitTest(int mouseX, int mouseY);

		DataObject GetDataObject(bool bOnlyCurrentSelBlock);

		int GetUIColumnIndexByStorageIndex(int indexInStorage);

		int GetStorageColumnIndexByUIIndex(int indexInUI);

		void GetCurrentCell(out long rowIndex, out int columnIndex);

		Rectangle GetVisibleCellRectangle(long rowIndex, int columnIndex);

		void ResizeColumnToShowAllContents(int columnIndex);

		GridColumnInfo GetGridColumnInfo(int columnIndex);
	}
}
