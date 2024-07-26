#region Assembly Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.DataStorage.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Collections;


namespace BlackbirdSql.Shared.Interfaces;

public interface IBsSortingArray
{
	int InsertAt(int iGroup, object val);

	int InsertWith(int iGroup, object val);

	object GetElementAt(int iRow);

	void SetElementAt(int iRow, int iNewRowNumber);

	void DeleteElementAt(int iRow);

	ArrayList GetGroupAt(int iGroup);

	int RowCount { get; }

	int NumGroups();

	void ResetElements();
}
