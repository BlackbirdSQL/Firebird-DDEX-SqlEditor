#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Ctl.Enums;

// using Microsoft.SqlServer.Management.QueryExecution;
// using Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces;
// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution




namespace BlackbirdSql.Common.Model.Interfaces;


public interface ISqlQueryExecutionHandler : IQueryExecutionHandler
{
	EnSqlExecutionMode SqlExecutionMode { get; set; }

	IBQueryExecutionResultsSettings ResultsSettings { get; set; }

	QESQLExecutionOptions SqlExecutionOptions { get; }

	string DefaultResultsDirectory { get; set; }

	bool CanAddMoreGrids { get; }

	void ClearResultsTabs();

	void AddStringToMessages(string message, bool flush);

	void AddStringToErrors(string message, int lineNumber, IBTextSpan textSpan, bool flush);

	void ProcessSpecialActionOnBatch(QESQLBatchSpecialActionEventArgs args);

	void ProcessNewXml(string xmlString, bool cleanPreviousResults);

	void MarkAsCouldNotAddMoreGrids();

	void AddGridContainer(ResultSetAndGridContainer gridContainer);

	void AddResultSetSeparatorMsg();

	void AddStringToResults(string result, bool flush);
}
