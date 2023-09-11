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
	public sealed class QESQLBatchStatementCompletedEventArgs : EventArgs
	{
		private readonly long _RecordCount;

		private readonly bool _IsDebugging;

		private readonly bool _IsParseOnly;

		public bool IsParseOnly => _IsParseOnly;

		public long RecordCount => _RecordCount;

		public bool IsDebugging => _IsDebugging;


		public QESQLBatchStatementCompletedEventArgs(long recordCount, bool isParseOnly, bool isDebugging)
		{
			_RecordCount = recordCount;
			_IsDebugging = isDebugging;
			_IsParseOnly = isParseOnly;
		}
	}
}
