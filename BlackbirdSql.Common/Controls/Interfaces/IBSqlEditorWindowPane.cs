// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorWindowPane

using System;
using BlackbirdSql.Common.Controls.ResultsPanels;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Interfaces;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Common.Controls.Interfaces;

public interface IBSqlEditorWindowPane : IBWindowPane
{
	IVsTextLines DocData { get; }

	string FileName { get; }

	string DocumentMoniker { get; }

	IDisposable DisposableWaitCursor { get; set; }

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

	bool IsTextPlanTabVisible { get; }

	bool IsTextPlanButtonVisible { get; }

	bool IsTextPlanButtonChecked { get; }

	bool IsStatisticsTabVisible { get; }

	bool IsStatisticsButtonVisible { get; }

	bool IsStatisticsButtonChecked { get; }

	string TextResultsTabText { get; }

	string MessagesTabText { get; }

	IOleCommandTarget MessagePaneCommandTarget { get; }

	GridResultsPanel GridResultsPanel { get; }


	void ExecuteQuery(bool withTts);

	void ParseQuery();

	void SetCodeEditorSelection(int startIndex, int length);

	IWpfTextView GetCodeEditorWpfTextView();

	string GetSelectedCodeEditorText();

	void ActivateCodeTab();

	void ActivateExecutionPlanTab();

	void ActivateTextPlanTab();

	void ActivateMessageTab();

	void ActivateResultsTab();

	void ActivateTextResultsTab();


	string GetAllCodeEditorText();

	SqlTextSpan GetSelectedCodeEditorTextSpan2();

	SqlTextSpan GetAllCodeEditorTextSpan2();

	string GetCodeText();

	void ConfigureTextViewForAutonomousFind(IVsWindowFrame frame, IVsTextView textView);

	public T GetSqlEditorTab<T>(Guid guidTab) where T : class;

	void CustomizeTabsForResultsSetting(bool isParseOnly);

	DisplaySQLResultsControl EnsureDisplayResultsControl();
}
