#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Events;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBQueryExecutionHandler : IBExecutionHandler
{
	EnSqlOutputMode SqlOutputMode { get; set; }


	IBEditorTransientSettings LiveSettings { get; }

	string DefaultResultsDirectory { get; set; }

	bool CanAddMoreGrids { get; }

	void ClearResultsTabs();

	void AddStringToInfoMessages(string message, bool flush);

	void AddStringToMessages(string message, bool flush);

	void AddStringToErrors(string message, int lineNumber, IBTextSpan textSpan, bool flush);

	void ProcessSpecialActionOnBatch(QESQLBatchSpecialActionEventArgs args);

	void ProcessNewXml(string xmlString, bool cleanPreviousResults);

	void MarkAsCouldNotAddMoreGrids();

	void AddGridContainer(ResultSetAndGridContainer gridContainer);

	void AddResultSetSeparatorMsg();

	void AddStringToResults(string result, bool flush);
}
