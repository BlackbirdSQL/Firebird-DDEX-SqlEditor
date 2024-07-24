#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Events;

// using Microsoft.SqlServer.Management.UI.Grid;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Shared.Interfaces
{
	public interface IBsGridControl2 : IBsGridControl
	{
		int NumberOfCharsToShow { get; set; }

		BlockOfCells CurrentSelectedBlock { get; }

		bool ContainsFocus { get; }

		event AdjustSelectionForButtonClickEventHandler AdjustSelectionForButtonClickEvent;

		void SetSelectedCellsAndCurrentCell(BlockOfCellsCollection cells, long currentRow, int currentColumn);

		void SetBkAndForeColors(Color bkColor, Color foreColor);

		void SetSelectedCellColor(Color selectedCellColor);

		void SetInactiveSelectedCellColor(Color inactiveSelectedCellColor);

		void SetIncludeHeadersOnDragAndDrop(bool includeHeaders);

		void InitialColumnResize();

		bool Focus();
	}
}
