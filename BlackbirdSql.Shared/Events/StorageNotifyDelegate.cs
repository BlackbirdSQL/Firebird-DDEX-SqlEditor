#region Assembly Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.DataStorage.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

// namespace Microsoft.SqlServer.Management.UI.Grid

using System.Threading;
using System.Threading.Tasks;

namespace BlackbirdSql.Shared.Events
{
	public delegate Task<bool> StorageNotifyDelegate(long storageRowCount, bool storedAllData, CancellationToken cancelToken);
}
