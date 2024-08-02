#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Events;

// using Microsoft.SqlServer.Management.UI.Grid;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Shared.Interfaces
{
	public interface IBsQEStorage : IBsDataStorage, IDisposable
	{
		int MaxCharsToStore { get; set; }

		int MaxXmlCharsToStore { get; set; }

		event StorageNotifyDelegate StorageNotifyEventAsync;

		Task<bool> InitStorageAsync(IDataReader reader, CancellationToken cancelToken);

		Task<bool> StartStoringDataAsync(CancellationToken cancelToken);

		void InitiateStopStoringData();
	}
}
