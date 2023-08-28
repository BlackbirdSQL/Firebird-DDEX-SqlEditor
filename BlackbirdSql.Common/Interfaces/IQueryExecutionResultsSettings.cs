#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

using BlackbirdSql.Common.Enums;

namespace BlackbirdSql.Common.Interfaces;


public interface IQueryExecutionResultsSettings : ICloneable
{
	char ColumnDelimiterForText { get; set; }

	bool DiscardResultsForGrid { get; set; }

	bool DiscardResultsForText { get; set; }

	bool DisplayResultInSeparateTabForGrid { get; set; }

	bool DisplayResultInSeparateTabForText { get; set; }

	bool IncludeColumnHeadersWhileSavingGridResults { get; set; }

	int MaxCharsPerColumnForGrid { get; set; }

	int MaxCharsPerColumnForText { get; set; }

	int MaxCharsPerColumnForXml { get; set; }

	bool OutputQueryForGrid { get; set; }

	bool OutputQueryForText { get; set; }

	bool PrintColumnHeadersForText { get; set; }

	bool ProvideFeedbackWithSounds { get; set; }

	bool QuoteStringsContainingCommas { get; set; }

	string ResultsDirectory { get; set; }

	bool RightAlignNumericsForText { get; set; }

	bool ScrollResultsAsReceivedForText { get; set; }

	bool ShowAllGridsInTheSameTab { get; set; }

	bool ShowGridLinesInMap { get; set; }

	bool ShowMessagesInNewTabForText { get; set; }

	EnSqlExecutionMode SqlExecutionMode { get; set; }

	bool SwitchToResultsTabAfterQueryExecutesForGrid { get; set; }

	bool SwitchToResultsTabAfterQueryExecutesForText { get; set; }

	void ResetToDefault();
}
