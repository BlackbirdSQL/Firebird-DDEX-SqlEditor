#region Assembly Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.DataStorage.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Shared.Events;

namespace BlackbirdSql.Shared.Interfaces;
// namespace Microsoft.SqlServer.Management.UI.Grid


public interface IBSortView : IDisposable
{
	event StorageNotifyDelegate StorageNotify;

	void ResetSortKeys();

	void AddSortKey(int iCol);

	void StartSortingData();

	void StopSortingData();

	bool GetNextRowNumber();

	int CompareKeyColumns(object oKey1, object oKey2, int iCol);

	int RowCount { get; }

	int GetAbsoluteRowNumber(int iRelativeRow);

	void DeleteRow(int iRelativeRow, bool fDeleteFromStorage);
}
