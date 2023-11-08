#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Model.Events;

// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Interfaces
{
	public interface IBQESQLBatchConsumer : IDisposable
	{
		int MaxCharsPerColumn { get; set; }

		bool DiscardResults { get; set; }

		void OnCancelling(object sender, EventArgs args);

		void OnNewResultSet(object sender, QESQLBatchNewResultSetEventArgs args);

		void OnMessage(object sender, QESQLBatchMessageEventArgs args);

		void OnErrorMessage(object sender, QESQLBatchErrorMessageEventArgs args);

		void OnFinishedProcessingResultSet(object sender, EventArgs args);

		void OnSpecialAction(object sender, QESQLBatchSpecialActionEventArgs args);

		void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args);

		void Cleanup();

		void CleanupAfterFinishingExecution();
	}
}
