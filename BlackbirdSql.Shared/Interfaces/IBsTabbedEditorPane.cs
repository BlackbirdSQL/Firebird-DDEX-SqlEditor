// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorWindowPane

using System;
using System.Threading.Tasks;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Shared.Controls.Results;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Shared.Interfaces;

public interface IBsTabbedEditorPane : IBsEditorPaneServiceProvider, IBsEditorPane,
	IBsWindowPane, IVsFindTarget, IVsFindTarget2, IBsVsFindTarget3
{
	IVsTextLines DocData { get; }
	IBsEditorPackage ExtensionInstance { get; }
	string FileName { get; }
	bool IsButtonCheckedCode { get; }
	// bool IsButtonCheckedExecutionPlan { get; }
	bool IsButtonCheckedMessages { get; }
	bool IsButtonCheckedResultsGrid { get; }
	bool IsButtonCheckedStatistics { get; }
	bool IsButtonCheckedTextPlan { get; }
	bool IsButtonCheckedTextResults { get; }
	bool IsButtonVisibleCode { get; }
	// bool IsButtonVisibleExecutionPlan { get; }
	bool IsButtonVisibleMessages { get; }
	bool IsButtonVisibleResultsGrid { get; set; }
	bool IsButtonVisibleStatistics { get; }
	bool IsButtonVisibleTextPlan { get; }
	bool IsButtonVisibleTextResults { get; set; }
	bool IsTabVisibleCode { get; }
	// bool IsTabVisibleExecutionPlan { get; }
	bool IsTabVisibleMessages { get; }
	bool IsTabVisibleResultsGrid { get; }
	bool IsTabVisibleStatistics { get; }
	bool IsTabVisibleTextPlan { get; }
	bool IsTabVisibleTextResults { get; }
	IOleCommandTarget CommandTargetMessagePane { get; }
	string TabTextTextResults { get; }
	string TabTextMessages { get; }
	string WindowBaseName { get; }



	void ActivateCodeTab();
	// void ActivateExecutionPlanTab();
	void ActivateMessageTab();
	void ActivateResultsTab();
	void ActivateStatisticsTab();
	void ActivateTextPlanTab();
	void ActivateTextResultsTab();
	void AsyncExecuteQuery(EnSqlExecutionType executionType);
	void ConfigureTextViewForAutonomousFind(IVsWindowFrame frame, IVsTextView textView);
	void CustomizeTabsForResultsSetting(bool isParseOnly);
	ResultsHandler EnsureDisplayResultsControl();
	string GetAllCodeEditorText();
	TextSpanEx GetAllCodeEditorTextSpan();
	IWpfTextView GetCodeEditorWpfTextView();
	string GetCodeText();
	public T GetEditorTab<T>(Guid guidTab) where T : class;
	string GetSelectedCodeEditorText();
	TextSpanEx GetSelectedCodeEditorTextSpan();
	void SetCodeEditorSelection(int startIndex, int length);
}
