#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


using System.Drawing;
using BlackbirdSql.Common.Controls.Enums;

namespace BlackbirdSql.Common.Controls.Interfaces;
// namespace Microsoft.SqlServer.Management.UI.Grid


public interface IBGridStorage
{
	long RowCount { get; }

	long EnsureRowsInBuf(long FirstRowIndex, long LastRowIndex);

	string GetCellDataAsString(long nRowIndex, int nColIndex);

	int IsCellEditable(long nRowIndex, int nColIndex);

	Bitmap GetCellDataAsBitmap(long nRowIndex, int nColIndex);

	void GetCellDataForButton(long nRowIndex, int nColIndex, out EnButtonCellState state, out Bitmap image, out string buttonLabel);

	EnGridCheckBoxState GetCellDataForCheckBox(long nRowIndex, int nColIndex);

	void FillControlWithData(long nRowIndex, int nColIndex, IBGridEmbeddedControl control);

	bool SetCellDataFromControl(long nRowIndex, int nColIndex, IBGridEmbeddedControl control);
}
