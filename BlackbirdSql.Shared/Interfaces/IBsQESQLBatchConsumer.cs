#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Events;


// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Shared.Interfaces
{
	public interface IBsQESQLBatchConsumer : IDisposable
	{
		int CurrentErrorCount { get; }
		int CurrentMessageCount { get; set; }
		int TotalInfoMessageCount { get; }
		int MaxCharsPerColumn { get; set; }

		bool DiscardResults { get; set; }

		void OnCancelling(object sender, EventArgs args);

		Task<bool> OnNewResultSetAsync(object sender, BatchNewResultSetEventArgs args);

		void OnMessage(object sender, BatchMessageEventArgs args);

		void OnErrorMessage(object sender, BatchErrorMessageEventArgs args);

		void OnFinishedProcessingResultSet(object sender, EventArgs args);

		void OnSpecialAction(object sender, BatchSpecialActionEventArgs args);

		void OnStatementCompleted(object sender, BatchStatementCompletedEventArgs args);

		void Cleanup();

		void CleanupAfterFinishingExecution();
	}
}
