#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Drawing;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Model.Events;

// using Microsoft.SqlServer.Management.UI.Grid;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Interfaces
{
	public interface IGridControl2 : IGridControl
	{
		int NumberOfCharsToShow { get; set; }

		BlockOfCells CurrentSelectedBlock { get; }

		bool ContainsFocus { get; }

		event AdjustSelectionForButtonClickEventHandler AdjustSelectionForButtonClick;

		void SetSelectedCellsAndCurrentCell(BlockOfCellsCollection cells, long currentRow, int currentColumn);

		void SetBkAndForeColors(Color bkColor, Color foreColor);

		void SetSelectedCellColor(Color selectedCellColor);

		void SetInactiveSelectedCellColor(Color inactiveSelectedCellColor);

		void SetIncludeHeadersOnDragAndDrop(bool includeHeaders);

		void InitialColumnResize();

		bool Focus();
	}
}
