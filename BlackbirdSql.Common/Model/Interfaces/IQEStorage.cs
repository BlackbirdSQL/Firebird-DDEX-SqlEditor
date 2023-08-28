#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using BlackbirdSql.Common.Model.Events;
// using Microsoft.SqlServer.Management.UI.Grid;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Interfaces
{
	public interface IQEStorage : IDataStorage, IDisposable
	{
		int MaxCharsToStore { get; set; }

		int MaxXmlCharsToStore { get; set; }

		event StorageNotifyDelegate StorageNotify;

		void InitStorage(IDataReader reader);

		void StartStoringData();

		void InitiateStopStoringData();
	}
}
