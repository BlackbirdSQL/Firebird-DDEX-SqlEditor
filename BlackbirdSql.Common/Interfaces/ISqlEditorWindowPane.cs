// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorWindowPane

using BlackbirdSql.Common.Controls.ResultsPane;
using BlackbirdSql.Common.Ctl;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Common.Interfaces;

public interface ISqlEditorWindowPane : IBWindowPane
{
	IVsTextLines DocData { get; }

	string FileName { get; }

	string DocumentMoniker { get; }

	bool IsResultsGridTabVisible { get; }

	bool IsResultsGridButtonVisible { get; set; }

	bool IsResultsGridButtonChecked { get; }

	bool IsMessagesTabVisible { get; }

	bool IsMessagesButtonVisible { get; }

	bool IsMessagesButtonChecked { get; }

	bool IsTextResultsTabVisible { get; }

	bool IsTextResultsButtonVisible { get; set; }

	bool IsTextResultsButtonChecked { get; }

	bool IsCodeTabVisible { get; }

	bool IsCodeTabButtonVisible { get; }

	bool IsCodeButtonChecked { get; }

	bool IsExecutionPlanTabVisible { get; }

	bool IsExecutionPlanButtonVisible { get; }

	bool IsExecutionPlanButtonChecked { get; }

	bool IsStatisticsTabVisible { get; }

	bool IsStatisticsButtonVisible { get; }

	bool IsStatisticsButtonChecked { get; }

	string TextResultsTabText { get; }

	string MessagesTabText { get; }

	IOleCommandTarget MessagePaneCommandTarget { get; }

	GridResultsPanel GridResultsPanel { get; }

	// ExecutionPlanPanel ExecutionPlanPanel { get; }

	void ExecuteQuery();

	void ParseQuery();

	void SetCodeEditorSelection(int startIndex, int length);

	IWpfTextView GetCodeEditorWpfTextView();

	string GetSelectedCodeEditorText();

	void ActivateCodeTab();

	void ActivateExecutionPlanTab();

	void ActivateMessageTab();

	void ActivateResultsTab();

	void ActivateTextResultsTab();


	string GetAllCodeEditorText();

	SqlTextSpan GetSelectedCodeEditorTextSpan2();

	SqlTextSpan GetAllCodeEditorTextSpan2();

	IVsTextView GetCodeEditorTextView();

	void CustomizeTabsForResultsSetting(bool isParseOnly);

	DisplaySQLResultsControl EnsureDisplayResultsControl();
}
