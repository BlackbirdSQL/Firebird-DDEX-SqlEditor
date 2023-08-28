#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Events
{
	public class MoreRowsAvailableEventArgs : EventArgs
	{
		private bool _allRows;

		private long _newRowsNum;

		public bool AllRows => _allRows;

		public long NewRowsNumber => _newRowsNum;

		public void SetEventInfo(bool allRows, long newRows)
		{
			_allRows = allRows;
			_newRowsNum = newRows;
		}
	}
}
