#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

// namespace Microsoft.SqlServer.Management.UI.Grid
using BlackbirdSql.Common.Controls.Events;

namespace BlackbirdSql.Common.Controls.Interfaces
{
	public interface IBGridEmbeddedControl
	{
		int ColumnIndex { get; set; }

		long RowIndex { get; set; }

		bool Enabled { get; set; }

		event ContentsChangedEventHandler ContentsChangedEvent;

		void ClearData();

		int AddDataAsString(string Item);

		int SetCurSelectionAsString(string strNewSel);

		void SetCurSelectionIndex(int nIndex);

		int GetCurSelectionIndex();

		string GetCurSelectionAsString();
	}
}
