// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorTabbedEditorPane

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Controls.PropertiesWindow;
using BlackbirdSql.Common.Controls.ResultsPanels;
using BlackbirdSql.Common.Controls.Tabs;
using BlackbirdSql.Common.Controls.Widgets;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Commands;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls.Interfaces;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Common.Controls;

public class TabbedEditorWindowPane : AbstractTabbedEditorWindowPane, IBSqlEditorWindowPane, IBEditorWindowPane, IVsFindTarget, IVsFindTarget2, IBVsFindTarget3
{
	public TabbedEditorWindowPane(System.IServiceProvider provider, Package package, object docData, string fileName, string editorId = "")
	: base(provider, package, docData, Guid.Empty, 0u)
	{
		FileName = fileName;
		EditorId = editorId;
		VSFindTargetAdapter = new FindTargetAdapter(this);
	}

	private DisplaySQLResultsControl _ResultsControl;

	private SqlEditorViewFilter _ViewFilter;

	private bool _IsFirstTimeToShow = true;

	private bool _IsFirstTimePaneCustomizationForResults = true;

	private readonly string _HelpString = "BlackbirdSql.Common.Controls.SqlEditor";

	private PropertiesWindowManager _ProperyWindowManager;

	private FindTargetAdapter VSFindTargetAdapter { get; set; }


	public SqlEditorViewFilter ViewFilter => _ViewFilter;

	public string FileName { get; private set; }

	protected string EditorId { get; set; }

	protected override Guid PrimaryViewGuid => VSConstants.LOGVIEWID_TextView;

	public TabbedEditorUIControl TabbedEditorUiCtl => (TabbedEditorUIControl)TabbedEditorControl;

	public string DocumentMoniker
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			string result = null;

			if (GetService(typeof(SVsWindowFrame)) is IVsWindowFrame vsWindowFrame)
			{
				vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out var pvar);
				result = pvar as string;
			}

			return result;
		}
	}


	public bool IsResultsGridTabVisible => GetSqlEditorResultsTab().IsVisible;

	public bool IsResultsGridButtonChecked => IsTabButtonChecked(GetSqlEditorResultsTab());

	public bool IsResultsGridButtonVisible
	{
		get
		{
			return GetSqlEditorResultsTab().TabButtonVisible;
		}
		set
		{
			GetSqlEditorResultsTab().TabButtonVisible = value;
		}
	}

	public bool IsMessagesTabVisible => GetSqlEditorMessageTab().IsVisible;

	public bool IsMessagesButtonChecked => IsTabButtonChecked(GetSqlEditorMessageTab());

	public bool IsMessagesButtonVisible
	{
		get
		{
			return GetSqlEditorMessageTab().TabButtonVisible;
		}
		set
		{
			GetSqlEditorMessageTab().TabButtonVisible = value;
		}
	}

	public bool IsTextResultsTabVisible => GetSqlEditorTextResultsTab().IsVisible;

	public bool IsTextResultsButtonChecked => IsTabButtonChecked(GetSqlEditorTextResultsTab());

	public bool IsTextResultsButtonVisible
	{
		get
		{
			return GetSqlEditorTextResultsTab().TabButtonVisible;
		}
		set
		{
			GetSqlEditorTextResultsTab().TabButtonVisible = value;
		}
	}

	public bool IsCodeTabVisible => GetSqlEditorCodeTab().IsVisible;

	public bool IsCodeButtonChecked => IsTabButtonChecked(GetSqlEditorCodeTab());

	public bool IsCodeTabButtonVisible
	{
		get
		{
			return GetSqlEditorCodeTab().TabButtonVisible;
		}
		set
		{
			GetSqlEditorCodeTab().TabButtonVisible = value;
		}
	}

	public bool IsExecutionPlanTabVisible => GetSqlExecutionPlanTab().IsVisible;

	public bool IsExecutionPlanButtonChecked => IsTabButtonChecked(GetSqlExecutionPlanTab());

	public bool IsExecutionPlanButtonVisible
	{
		get
		{
			return GetSqlExecutionPlanTab().TabButtonVisible;
		}
		set
		{
			GetSqlExecutionPlanTab().TabButtonVisible = value;
		}
	}

	public bool IsTextPlanTabVisible => GetSqlTextPlanTab().IsVisible;

	public bool IsTextPlanButtonChecked => IsTabButtonChecked(GetSqlTextPlanTab());

	public bool IsTextPlanButtonVisible
	{
		get
		{
			return GetSqlTextPlanTab().TabButtonVisible;
		}
		set
		{
			GetSqlTextPlanTab().TabButtonVisible = value;
		}
	}

	public bool IsStatisticsTabVisible => GetSqlEditorStatisticsTab().IsVisible;

	public bool IsStatisticsButtonChecked => IsTabButtonChecked(GetSqlEditorStatisticsTab());

	public bool IsStatisticsButtonVisible
	{
		get
		{
			return GetSqlEditorStatisticsTab().TabButtonVisible;
		}
		set
		{
			GetSqlEditorStatisticsTab().TabButtonVisible = value;
		}
	}

	public bool IsSplitterVisible
	{
		get
		{
			return TabbedEditorUiCtl.SplitViewContainer.IsSplitterVisible;
		}
		set
		{
			TabbedEditorUiCtl.SplitViewContainer.IsSplitterVisible = value;
		}
	}

	public bool SplittersVisible
	{
		set
		{
			TabbedEditorUiCtl.SplitViewContainer.SplittersVisible = value;
		}
	}

	public string MessagesTabText
	{
		get
		{
			VSTextEditorPanel messagePaneTextEditorPanel = MessagePaneTextEditorPanel;
			if (messagePaneTextEditorPanel != null)
			{
				return messagePaneTextEditorPanel.TextViewCtl.TextBuffer.Text;
			}

			return string.Empty;
		}
	}

	public string TextResultsTabText
	{
		get
		{
			VSTextEditorPanel textResultsPaneTextEditorPanel = TextResultsPaneTextEditorPanel;
			if (textResultsPaneTextEditorPanel != null)
			{
				return textResultsPaneTextEditorPanel.TextViewCtl.TextBuffer.Text;
			}

			return string.Empty;
		}
	}

	public IOleCommandTarget MessagePaneCommandTarget => MessagePaneTextEditorPanel;

	private VSTextEditorPanel MessagePaneTextEditorPanel
	{
		get
		{
			Panel obj = (GetSqlEditorMessageTab().GetView() as ResultWindowPane).Window as Panel;
			VSTextEditorPanel vSTextEditorPanel = null;
			foreach (Control control in obj.Controls)
			{
				vSTextEditorPanel = control as VSTextEditorPanel;
				if (vSTextEditorPanel != null)
				{
					return vSTextEditorPanel;
				}
			}

			return vSTextEditorPanel;
		}
	}

	private VSTextEditorPanel TextResultsPaneTextEditorPanel
	{
		get
		{
			Panel obj = (GetSqlEditorTextResultsTab().GetView() as ResultWindowPane).Window as Panel;
			VSTextEditorPanel vSTextEditorPanel = null;
			foreach (Control control in obj.Controls)
			{
				vSTextEditorPanel = control as VSTextEditorPanel;
				if (vSTextEditorPanel != null)
				{
					return vSTextEditorPanel;
				}
			}

			return vSTextEditorPanel;
		}
	}

	public GridResultsPanel GridResultsPanel
	{
		get
		{
			Panel obj = (GetSqlEditorResultsTab().GetView() as ResultWindowPane).Window as Panel;
			GridResultsPanel gridResultsPanel = null;
			foreach (Control control in obj.Controls)
			{
				gridResultsPanel = control as GridResultsPanel;
				if (gridResultsPanel != null)
				{
					return gridResultsPanel;
				}
			}

			return gridResultsPanel;
		}
	}

	public ExecutionPlanPanel ExecutionPlanPanel
	{
		get
		{
			Panel obj = (GetSqlExecutionPlanTab().GetView() as ResultWindowPane).Window as Panel;
			ExecutionPlanPanel executionPlanPanel = null;
			foreach (Control control in obj.Controls)
			{
				executionPlanPanel = control as ExecutionPlanPanel;
				if (executionPlanPanel != null)
				{
					return executionPlanPanel;
				}
			}

			return executionPlanPanel;
		}
	}


	public VSTextEditorPanel TextPlanPanel
	{
		get
		{
			Panel obj = (GetSqlTextPlanTab().GetView() as ResultWindowPane).Window as Panel;
			VSTextEditorPanel vSTextEditorPanel = null;
			foreach (Control control in obj.Controls)
			{
				vSTextEditorPanel = control as VSTextEditorPanel;
				if (vSTextEditorPanel != null)
				{
					return vSTextEditorPanel;
				}
			}

			return vSTextEditorPanel;
		}
	}



	protected override void Initialize()
	{
		try
		{
			TabbedEditorUiCtl.SuspendLayout();
			TabbedEditorUiCtl.SplitViewContainer.SuspendLayout();
			base.Initialize();
			_ViewFilter = CreateViewFilter();
			SplitViewContainer splitViewContainer = TabbedEditorUiCtl.SplitViewContainer;
			splitViewContainer.CustomizeSplitterBarButton(VSConstants.LOGVIEWID_TextView, EnSplitterBarButtonDisplayStyle.Text, ControlsResources.SqlEditorTabbedEditorPane_Sql_Button_Text, ControlsResources.TSQL);
			splitViewContainer.CustomizeSplitterBarButton(VSConstants.LOGVIEWID_Designer, EnSplitterBarButtonDisplayStyle.Text, ControlsResources.SqlEditorTabbedEditorPane_Results_Button_Text, ControlsResources.Results);
			splitViewContainer.SplittersVisible = false;
			splitViewContainer.IsSplitterVisible = false;
			splitViewContainer.SwapButtons();
			splitViewContainer.PathStripVisibleInSplitMode = false;
			TabbedEditorUiCtl.TabActivatedEvent += TabActivatedHandler;
			AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);
			if (auxDocData != null && auxDocData.QryMgr != null)
			{
				auxDocData.QryMgr.StatusChangedEvent += OnUpdateTooltipAndWindowCaption;
				auxDocData.QryMgr.ScriptExecutionCompletedEvent += OnQueryScriptExecutionCompleted;
			}

			UpdateWindowCaption();
			UpdateToolTip();
		}
		finally
		{
			TabbedEditorUiCtl.SplitViewContainer.ResumeLayout(performLayout: false);
			TabbedEditorUiCtl.ResumeLayout(performLayout: false);
			TabbedEditorUiCtl.PerformLayout();
		}
	}

	private void TabActivatedHandler(object sender, EventArgs args)
	{
		((sender as AbstractEditorTab).GetView() as ResultWindowPane)?.SetFocus();
	}

	protected virtual SqlEditorViewFilter CreateViewFilter()
	{
		return new SqlEditorViewFilter(this);
	}

	protected override AbstractTabbedEditorUIControl CreateTabbedEditorUI(Guid toolbarGuid, uint toolbarId /*, string[] buttonTexts = null */)
	{
		return new TabbedEditorUIControl(this, toolbarGuid, toolbarId);
	}

	public override string GetHelpKeywordForCodeWindowTextView()
	{
		return _HelpString;
	}

	protected override AbstractEditorTab CreateEditorTab(AbstractTabbedEditorWindowPane editorPane, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
	{
		AbstractEditorTab result = null;
		if (logicalView == VSConstants.LOGVIEWID_Designer)
		{
			return new SqlEditorResultsTab(this, logicalView, editorLogicalView, editorTabType);
		}

		if (logicalView == VSConstants.LOGVIEWID_TextView)
		{
			return new SqlEditorCodeTab(this, logicalView, editorLogicalView, editorTabType);
		}

		if (logicalView == new Guid(LibraryData.SqlMessageTabLogicalViewGuid)
			|| logicalView == new Guid(LibraryData.SqlTextResultsTabLogicalViewGuid)
			|| logicalView == new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid))
		{
			return new SqlEditorMessageTab(this, logicalView, editorLogicalView, editorTabType);
		}

		if (logicalView == new Guid(LibraryData.SqlStatisticsTabLogicalViewGuid))
		{
			return new SqlEditorResultsTab(this, logicalView, editorLogicalView, editorTabType);
		}

		if (logicalView == new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid))
		{
			return new SqlEditorResultsTab(this, logicalView, editorLogicalView, editorTabType);
		}


		return result;
	}

	public override bool EnsureTabs(bool activateTextView)
	{

		SplitViewContainer splitViewContainer = TabbedEditorUiCtl.SplitViewContainer;
		AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);
		QueryManager qryMgr = auxDocData.QryMgr;

		bool isActual = qryMgr.LiveSettings.WithExecutionPlan && !qryMgr.LiveSettings.WithEstimatedExecutionPlan;

		string btnPlanText = isActual
			? ControlsResources.SqlEditorTabbedEditorPane_ActualPlan_Button_Text
			: ControlsResources.SqlEditorTabbedEditorPane_Plan_Button_Text;
		string btnTextPlanText = isActual
			? ControlsResources.SqlEditorTabbedEditorPane_ActualTextPlan_Button_Text
			: ControlsResources.SqlEditorTabbedEditorPane_TextPlan_Button_Text;


		if (TabbedEditorUiCtl.Tabs.Count > 0)
		{
			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, btnPlanText, ControlsResources.ExecutionPlan);
			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, btnTextPlanText, ControlsResources.TextPlan);

			return true;
		}


		if (!base.EnsureTabs(activateTextView))
		{
			return false;
		}

		SqlEditorResultsTab resultsTab = GetSqlEditorResultsTab();

		if (resultsTab == null)
		{
			Exception ex = new("GetSqlEditorResultsTab returned null");
			Diag.Dug(ex);
			throw ex;
		}

		resultsTab.Hide();

		try
		{
			TabbedEditorUiCtl.SuspendLayout();

			AbstractEditorTab editorTab = CreateEditorTabWithButton(this, new Guid(LibraryData.SqlTextResultsTabLogicalViewGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab);
			LoadDesignerPane(editorTab, asPrimary: false, showSplitter: false);
			editorTab.Hide();

			AbstractEditorTab editorTab2 = CreateEditorTabWithButton(this, new Guid(LibraryData.SqlStatisticsTabLogicalViewGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab2);
			LoadDesignerPane(editorTab2, asPrimary: false, showSplitter: false);
			editorTab2.Hide();

			AbstractEditorTab editorTab3 = CreateEditorTabWithButton(this, new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab3);
			LoadDesignerPane(editorTab3, asPrimary: false, showSplitter: false);
			editorTab3.Hide();

			AbstractEditorTab editorTab4 = CreateEditorTabWithButton(this, new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab4);
			LoadDesignerPane(editorTab4, asPrimary: false, showSplitter: false);
			editorTab4.Hide();

			AbstractEditorTab editorTab5 = CreateEditorTabWithButton(this, new Guid(LibraryData.SqlMessageTabLogicalViewGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab5);
			LoadDesignerPane(editorTab5, asPrimary: false, showSplitter: false);
			editorTab5.Hide();

			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlMessageTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, ControlsResources.SqlEditorTabbedEditorPane_Message_Button_Text,
				ControlsResources.Messages);
			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlTextResultsTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, ControlsResources.SqlEditorTabbedEditorPane_Results_Button_Text,
				ControlsResources.Messages);
			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlStatisticsTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, ControlsResources.SqlEditorTabbedEditorPane_Statistics_Button_Text,
				ControlsResources.Statistics);

			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, btnPlanText, ControlsResources.ExecutionPlan);
			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, btnTextPlanText, ControlsResources.TextPlan);

			TabbedEditorUiCtl.SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(new Guid(LibraryData.SqlTextResultsTabLogicalViewGuid), visible: false);
			TabbedEditorUiCtl.SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid), visible: false);
			TabbedEditorUiCtl.SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid), visible: false);

			IVsTextView codeEditorTextView = GetCodeEditorTextView();
			if (codeEditorTextView != null)
			{
				AbstractViewFilter.AddFilterToView(codeEditorTextView, _ViewFilter);
			}

			EnsureDisplayResultsControl();

			_ProperyWindowManager = new PropertiesWindowManager(this);

			qryMgr.IsWithOleSQLScripting = qryMgr.LiveSettings.EditorExecutionDefaultOleScripting;
			qryMgr.CurrentWorkingDirectoryPath = GetCurrentWorkingDirectory;

			ConfigureTextViewForAutonomousFind(GetSqlEditorCodeTab().CurrentFrame, GetCodeEditorTextView());
			ConfigureTextViewForAutonomousFind(GetSqlEditorTextResultsTab().CurrentFrame, _ResultsControl.TextResultsPaneTextView);
			ConfigureTextViewForAutonomousFind(GetSqlEditorMessageTab().CurrentFrame, _ResultsControl.MessagesPaneTextView);

			if (_ResultsControl.TextPlanPaneTextView != null)
				ConfigureTextViewForAutonomousFind(GetSqlTextPlanTab().CurrentFrame, _ResultsControl.TextPlanPaneTextView);

			return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			TabbedEditorUiCtl.ResumeLayout();
		}
	}

	protected override int HandleCloseEditorOrDesigner()
	{
		// Tracer.Trace(GetType(), "HandleCloseEditorOrDesigner()");

		int result = 0;

		if (Cmd.ShouldStopCloseDialog(((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData), GetType()))
		{
			result = VSConstants.OLE_E_PROMPTSAVECANCELLED;
		}

		return result;
	}

	protected override int SaveFiles(ref uint pgrfSaveOptions)
	{
		// Tracer.Trace(GetType(), "SaveFiles()");

		AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);

		if (pgrfSaveOptions == (uint)__FRAMECLOSE.FRAMECLOSE_PromptSave && auxDocData.SuppressSavePrompt)
		{
			pgrfSaveOptions = (uint)__FRAMECLOSE.FRAMECLOSE_NoSave;
		}

		return base.SaveFiles(ref pgrfSaveOptions);
	}

	protected override void OnShow(int fShow)
	{
		base.OnShow(fShow);

		if (_IsFirstTimeToShow && fShow == 1)
		{
			SetupUI();
			_IsFirstTimeToShow = false;
		}
	}

	public override int HandleExec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (pguidCmdGroup == LibraryData.CLSID_CommandSet)
		{
			AbstractSqlEditorCommand sqlEditorCommand = null;

			EnCommandSet cmd = (EnCommandSet)nCmdID;
			switch (cmd)
			{
				case EnCommandSet.CmdIdResultsAsText:
					sqlEditorCommand = new SqlEditorResultsAsTextCommand(this);
					break;
				case EnCommandSet.CmdIdResultsAsGrid:
					sqlEditorCommand = new SqlEditorResultsAsGridCommand(this);
					break;
				case EnCommandSet.CmdIdResultsAsFile:
					sqlEditorCommand = new SqlEditorResultsAsFileCommand(this);
					break;
				case EnCommandSet.CmdIdToggleResultsPane:
					sqlEditorCommand = new SqlEditorToggleResultsPaneCommand(this);
					break;
			}

			if (sqlEditorCommand != null)
			{
				return sqlEditorCommand.Exec(nCmdexecopt, pvaIn, pvaOut);
			}
		}

		// Tracer.Trace(GetType(), "HandleExec()", "pguidCmdGroup: {0}, nCmdId: {1}.", pguidCmdGroup, nCmdID);

		AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);
		if (auxDocData != null && auxDocData.Strategy != null)
		{
			IBSqlEditorExtendedCommandHandler extendedCommandHandler = auxDocData.Strategy.ExtendedCommandHandler;
			if (extendedCommandHandler != null && extendedCommandHandler.HandleExec(this, ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut) == 0)
			{
				return VSConstants.S_OK;
			}
		}

		return base.HandleExec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
	}

	public override int HandleQueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		for (int i = 0; i < cCmds; i++)
		{
			uint cmdID = prgCmds[i].cmdID;
			if (pguidCmdGroup == LibraryData.CLSID_CommandSet)
			{
				AbstractSqlEditorCommand sqlEditorCommand = null;
				EnCommandSet cmd = (EnCommandSet)cmdID;

				switch (cmd)
				{
					case EnCommandSet.CmdIdResultsAsText:
						sqlEditorCommand = new SqlEditorResultsAsTextCommand(this);
						break;
					case EnCommandSet.CmdIdResultsAsGrid:
						sqlEditorCommand = new SqlEditorResultsAsGridCommand(this);
						break;
					case EnCommandSet.CmdIdResultsAsFile:
						sqlEditorCommand = new SqlEditorResultsAsFileCommand(this);
						break;
					case EnCommandSet.CmdIdToggleResultsPane:
						sqlEditorCommand = new SqlEditorToggleResultsPaneCommand(this);
						break;
				}

				if (sqlEditorCommand != null)
				{
					return sqlEditorCommand.QueryStatus(ref prgCmds[i], pCmdText);
				}
			}
		}

		AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);
		if (auxDocData != null && auxDocData.Strategy != null)
		{
			IBSqlEditorExtendedCommandHandler extendedCommandHandler = auxDocData.Strategy.ExtendedCommandHandler;
			if (extendedCommandHandler != null && extendedCommandHandler.HandleQueryStatus(this, ref pguidCmdGroup, cCmds, prgCmds, pCmdText) == 0)
			{
				return VSConstants.S_OK;
			}
		}

		return base.HandleQueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
	}

	public string GetSelectedCodeEditorText()
	{
		IVsTextView codeEditorTextView = GetCodeEditorTextView();
		___(codeEditorTextView.GetSelectedText(out string pbstrText));
		return pbstrText;
	}

	public string GetAllCodeEditorText()
	{
		IVsTextView codeEditorTextView = GetCodeEditorTextView();
		int iStartLine = 0;
		int iStartIndex = 0;
		___(codeEditorTextView.GetBuffer(out var ppBuffer));
		___(ppBuffer.GetLastLineIndex(out var piLine, out var piIndex));
		___(ppBuffer.GetLineText(iStartLine, iStartIndex, piLine, piIndex, out var pbstrBuf));
		return pbstrBuf;
	}

	public TextSpan GetSelectedCodeEditorTextSpan()
	{
		___(GetCodeEditorTextView().GetSelection(out var piAnchorLine, out var piAnchorCol, out var piEndLine, out var piEndCol));
		TextSpan result = default;
		result.iStartLine = piAnchorLine;
		result.iStartIndex = piAnchorCol;
		result.iEndLine = piEndLine;
		result.iEndIndex = piEndCol;
		return result;
	}

	public SqlTextSpan GetAllCodeEditorTextSpan2()
	{
		IVsTextView codeEditorTextView = GetCodeEditorTextView();
		int anchorCol = 0;
		___(codeEditorTextView.GetBuffer(out var ppBuffer));
		___(ppBuffer.GetLastLineIndex(out var piLine, out var piIndex));
		return new SqlTextSpan(0, anchorCol, piLine, piIndex, GetAllCodeEditorText(), codeEditorTextView);
	}

	public SqlTextSpan GetSelectedCodeEditorTextSpan2()
	{
		IVsTextView codeEditorTextView = GetCodeEditorTextView();
		___(codeEditorTextView.GetSelection(out var piAnchorLine, out var piAnchorCol, out var piEndLine, out var piEndCol));
		return new SqlTextSpan(piAnchorLine, piAnchorCol, piEndLine, piEndCol, GetSelectedCodeEditorText(), codeEditorTextView);
	}

	public IVsTextView GetCodeEditorTextView()
	{
		IVsTextView ppView = null;
		SqlEditorCodeTab sqlEditorCodeTab = GetSqlEditorCodeTab();
		if (sqlEditorCodeTab != null)
		{
			Diag.ThrowIfNotOnUIThread();

			IVsWindowFrame currentFrame = sqlEditorCodeTab.CurrentFrame;
			___(currentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar));
			___(((IVsCodeWindow)pvar).GetPrimaryView(out ppView));
		}

		return ppView;
	}

	public void SetCodeEditorSelection(int start, int length)
	{
		IWpfTextView codeEditorWpfTextView = GetCodeEditorWpfTextView();
		ITextSnapshot currentSnapshot = codeEditorWpfTextView.TextBuffer.CurrentSnapshot;
		SnapshotSpan selectionSpan = new SnapshotSpan(currentSnapshot, new Span(start, length));
		codeEditorWpfTextView.Selection.Select(selectionSpan, isReversed: false);
	}

	public IWpfTextView GetCodeEditorWpfTextView()
	{
		return GetWpfTextView(GetCodeEditorTextView());
	}

	private static IWpfTextView GetWpfTextView(IVsTextView textView)
	{
		return ApcManager.GetService<SComponentModel, IComponentModel>().GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(textView);
	}

	public SqlEditorCodeTab GetSqlEditorCodeTab()
	{
		Guid lOGVIEWID_TextView = VSConstants.LOGVIEWID_TextView;
		return GetSqlEditorTab<SqlEditorCodeTab>(lOGVIEWID_TextView);
	}

	public SqlEditorResultsTab GetSqlEditorResultsTab()
	{
		Guid lOGVIEWID_Designer = VSConstants.LOGVIEWID_Designer;
		return GetSqlEditorTab<SqlEditorResultsTab>(lOGVIEWID_Designer);
	}

	public SqlEditorMessageTab GetSqlEditorMessageTab()
	{
		Guid guidSqlMessageTabLogicalView = new Guid(LibraryData.SqlMessageTabLogicalViewGuid);
		return GetSqlEditorTab<SqlEditorMessageTab>(guidSqlMessageTabLogicalView);
	}

	public SqlEditorMessageTab GetSqlEditorTextResultsTab()
	{
		Guid guidSqlTextResultsTabLogicalView = new Guid(LibraryData.SqlTextResultsTabLogicalViewGuid);
		return GetSqlEditorTab<SqlEditorMessageTab>(guidSqlTextResultsTabLogicalView);
	}

	public SqlEditorResultsTab GetSqlEditorStatisticsTab()
	{
		Guid guidSqlStatisticsTabLogicalView = new Guid(LibraryData.SqlStatisticsTabLogicalViewGuid);
		return GetSqlEditorTab<SqlEditorResultsTab>(guidSqlStatisticsTabLogicalView);
	}

	public SqlEditorResultsTab GetSqlExecutionPlanTab()
	{
		Guid guidSqlExecutionPlanTabLogicalView = new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid);
		return GetSqlEditorTab<SqlEditorResultsTab>(guidSqlExecutionPlanTabLogicalView);
	}

	public SqlEditorMessageTab GetSqlTextPlanTab()
	{
		Guid guidSqlTextPlanTabLogicalView = new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid);
		return GetSqlEditorTab<SqlEditorMessageTab>(guidSqlTextPlanTabLogicalView);
	}

	public T GetSqlEditorTab<T>(Guid guidTab) where T : class
	{
		Guid rguidLogicalView = guidTab;
		AbstractEditorTab tab = GetTab(ref rguidLogicalView);
		T result = null;
		if (tab != null)
		{
			return tab as T;
		}

		Diag.StackException($"Could not get editor tab type: {typeof(T)}  Guid:{rguidLogicalView}");
		return result;
	}

	public DisplaySQLResultsControl EnsureDisplayResultsControl()
	{
		if (_ResultsControl == null)
		{
			ResultWindowPane resultsGridPanel = GetSqlEditorResultsTab().GetView() as ResultWindowPane;
			ResultWindowPane spatialPane = null;
			ResultWindowPane messagePanel = GetSqlEditorMessageTab().GetView() as ResultWindowPane;
			ResultWindowPane textResultsPanel = GetSqlEditorTextResultsTab().GetView() as ResultWindowPane;
			ResultWindowPane statisticsPanel = GetSqlEditorStatisticsTab().GetView() as ResultWindowPane;
			ResultWindowPane executionPlanPanel = GetSqlExecutionPlanTab().GetView() as ResultWindowPane;
			ResultWindowPane textPlanPanel = GetSqlTextPlanTab().GetView() as ResultWindowPane;
			_ResultsControl = new DisplaySQLResultsControl(resultsGridPanel, messagePanel, textResultsPanel,
				statisticsPanel, executionPlanPanel, textPlanPanel, spatialPane, this);
			_ResultsControl.SetSite(ApcManager.OleServiceProvider);
		}

		return _ResultsControl;
	}

	public void ExecuteQuery(bool withTts)
	{
		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ExecuteQuery", "calling ExecuteOrParseQuery");

		// ------------------------------------------------------------------------------ //
		// ******************** Execution Point (1) - ExecuteQuery() ******************** //
		// ------------------------------------------------------------------------------ //

		ExecuteOrParseQuery(true, withTts);
	}

	public void ParseQuery()
	{
		ExecuteOrParseQuery(false, false);
	}

	public string GetCodeText()
	{
		SqlTextSpan sqlTextSpan = GetSelectedCodeEditorTextSpan2();
		if (sqlTextSpan.Text == null || sqlTextSpan.Text.Length == 0)
		{
			sqlTextSpan = GetAllCodeEditorTextSpan2();
		}

		if (sqlTextSpan == null || string.IsNullOrEmpty(sqlTextSpan.Text))
			return string.Empty;

		return sqlTextSpan.Text;
	}

	private void ExecuteOrParseQuery(bool isExecute, bool withTts)
	{
		using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(Microsoft.VisualStudio.Utilities.DpiAwarenessContext.SystemAware))
		{
			SqlTextSpan sqlTextSpan = GetSelectedCodeEditorTextSpan2();
			if (sqlTextSpan.Text == null || sqlTextSpan.Text.Length == 0)
			{
				sqlTextSpan = GetAllCodeEditorTextSpan2();
			}

			if (sqlTextSpan != null && !string.IsNullOrEmpty(sqlTextSpan.Text))
			{
				AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);
				QueryManager qryMgr = auxDocData.QryMgr;

				// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ExecuteOrParseQuery", "AuxiliaryDocData.EstimatedExecutionPlanEnabled: " + auxDocData.EstimatedExecutionPlanEnabled);


				DisposableWaitCursor = WaitCursorHelper.NewWaitCursor();

				try
				{

					// ----------------------------------------------------------------------------------- //
					// ******************** Execution Point (2) ExecuteOrParseQuery() ******************** //
					// ----------------------------------------------------------------------------------- //
					if (isExecute)
						qryMgr.Run(sqlTextSpan, withTts);
					else
						qryMgr.Parse(sqlTextSpan);
				}
				catch
				{
					DisposableWaitCursor = null;
				}

			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_ViewFilter.Dispose();
			_ResultsControl?.Dispose();

			AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);
			if (auxDocData != null && auxDocData.QryMgr != null)
			{
				auxDocData.QryMgr.StatusChangedEvent -= OnUpdateTooltipAndWindowCaption;
				auxDocData.QryMgr.ScriptExecutionCompletedEvent -= OnQueryScriptExecutionCompleted;
			}

			_ProperyWindowManager?.Dispose();
		}

		_ResultsControl = null;
		base.Dispose(disposing);
	}

	public void ActivateTab(AbstractEditorTab tab)
	{
		if (TabbedEditorUiCtl.IsSplitterVisible)
		{
			TabbedEditorUiCtl.ActivateTab(tab, EnTabViewMode.Split);
		}
		else
		{
			TabbedEditorUiCtl.ActivateTab(tab, EnTabViewMode.Maximize);
		}
	}

	public void ActivateMessageTab()
	{
		AbstractEditorTab sqlEditorMessageTab = GetSqlEditorMessageTab();
		ActivateTab(sqlEditorMessageTab);
	}

	public void ActivateTextResultsTab()
	{
		AbstractEditorTab sqlEditorTextResultsTab = GetSqlEditorTextResultsTab();
		ActivateTab(sqlEditorTextResultsTab);
	}

	public void ActivateResultsTab()
	{
		AbstractEditorTab sqlEditorResultsTab = GetSqlEditorResultsTab();
		ActivateTab(sqlEditorResultsTab);
	}

	public void ActivateCodeTab()
	{
		AbstractEditorTab sqlEditorCodeTab = GetSqlEditorCodeTab();
		ActivateTab(sqlEditorCodeTab);
	}

	public void ActivateExecutionPlanTab()
	{
		AbstractEditorTab sqlExecutionPlanTab = GetSqlExecutionPlanTab();
		ActivateTab(sqlExecutionPlanTab);
	}

	public void ActivateTextPlanTab()
	{
		AbstractEditorTab sqlTextPlanTab = GetSqlTextPlanTab();
		ActivateTab(sqlTextPlanTab);
	}

	private string GetCurrentWorkingDirectory()
	{
		string result = null;
		try
		{
			result = Path.GetDirectoryName(DocumentMoniker);
			return result;
		}
		catch
		{
			return result;
		}
	}

	public void CustomizeTabsForResultsSetting(bool isParseOnly)
	{
		EnsureTabs(false);
		AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);
		EnSqlOutputMode sqlExecutionMode = auxDocData.SqlOutputMode;
		AbstractEditorTab sqlEditorResultsTab = GetSqlEditorResultsTab();
		GetSqlEditorMessageTab();
		AbstractEditorTab sqlEditorTextResultsTab = GetSqlEditorTextResultsTab();
		AbstractEditorTab sqlEditorStatisticsTab = GetSqlEditorStatisticsTab();
		AbstractEditorTab sqlExecutionPlanTab = GetSqlExecutionPlanTab();
		AbstractEditorTab sqlTextPlanTab = GetSqlTextPlanTab();

		TabbedEditorUiCtl.SplitViewContainer.SplittersVisible = true;
		if (_IsFirstTimePaneCustomizationForResults)
		{
			TabbedEditorUiCtl.SplitViewContainer.IsSplitterVisible = true;
			_IsFirstTimePaneCustomizationForResults = false;
		}

		ActivateMessageTab();
		if (isParseOnly)
		{
			if (sqlEditorResultsTab != null)
				sqlEditorResultsTab.TabButtonVisible = false;

			if (sqlEditorTextResultsTab != null)
				sqlEditorTextResultsTab.TabButtonVisible = false;

			return;
		}

		sqlEditorStatisticsTab.TabButtonVisible = auxDocData.ClientStatisticsEnabled;
		sqlExecutionPlanTab.TabButtonVisible = false; // Parser not completed
		sqlTextPlanTab.TabButtonVisible = auxDocData.ActualExecutionPlanEnabled || auxDocData.EstimatedExecutionPlanEnabled;

		if (sqlExecutionMode == EnSqlOutputMode.ToFile || sqlExecutionMode == EnSqlOutputMode.ToText || auxDocData.EstimatedExecutionPlanEnabled)
		{
			if (!auxDocData.EstimatedExecutionPlanEnabled)
				ActivateTextResultsTab();

			if (sqlEditorResultsTab != null)
				sqlEditorResultsTab.TabButtonVisible = false;

			if (sqlEditorTextResultsTab != null)
				sqlEditorTextResultsTab.TabButtonVisible = !auxDocData.EstimatedExecutionPlanEnabled;

			if (auxDocData.LiveSettings.EditorResultsTextSeparateTabs)
			{
				TabbedEditorUiCtl.SplitViewContainer.IsSplitterVisible = false;
			}
		}
		else if (sqlExecutionMode == EnSqlOutputMode.ToGrid)
		{
			if (sqlEditorResultsTab != null)
			{
				sqlEditorResultsTab.TabButtonVisible = true;
			}

			if (sqlEditorTextResultsTab != null)
			{
				sqlEditorTextResultsTab.TabButtonVisible = false;
			}

			if (auxDocData.LiveSettings.EditorResultsGridSeparateTabs)
			{
				TabbedEditorUiCtl.SplitViewContainer.IsSplitterVisible = false;
			}
		}

		if (auxDocData.EstimatedExecutionPlanEnabled)
		{
			// ActivateExecutionPlanTab();
			ActivateTextPlanTab();
		}
		else
		{
			ActivateMessageTab();
		}
	}

	private void SetupUI()
	{
		try
		{
			TabbedEditorUiCtl.SuspendLayout();

			AuxiliaryDocData auxDocData;

			try
			{
				auxDocData =
					((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData)
					?? throw new NullReferenceException("(AuxiliaryDocData)auxDocData");

				if (auxDocData.Strategy == null)
					throw new NullReferenceException("auxDocData.Strategy");
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			Guid clsidCmdSet = LibraryData.CLSID_CommandSet;
			uint menuId;


			switch (auxDocData.Strategy.Mode)
			{
				case EnEditorMode.CustomOnline:
					menuId = (uint)EnCommandSet.MenuIdOnlineToolbar;
					TabbedEditorUiCtl.InitializeToolbarHost(this, clsidCmdSet, menuId);
					break;
				case EnEditorMode.Standard:
					menuId = (uint)EnCommandSet.MenuIdEditorToolbar;
					TabbedEditorUiCtl.InitializeToolbarHost(this, clsidCmdSet, menuId);
					break;
				default:
					return;
			}

			try
			{
				TabbedEditorUiCtl.StatusBar.Show();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
		}
		finally
		{
			TabbedEditorUiCtl.ResumeLayout();
		}
	}

	public void OnUpdateTooltipAndWindowCaption(object sender, QueryManager.StatusChangedEventArgs args)
	{
		_ = Task.Run(() => OnUpdateTooltipAndWindowCaptionAsync(sender, args));

	}

	public async Task OnUpdateTooltipAndWindowCaptionAsync(object sender, QueryManager.StatusChangedEventArgs args)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		if (args.Change == QueryManager.EnStatusType.Connection || args.Change == QueryManager.EnStatusType.Connected)
		{
			UpdateToolTip();
		}


		UpdateWindowCaption();
	}



	private void UpdateWindowCaption()
	{
		Diag.ThrowIfNotOnUIThread();

		IVsWindowFrame vsWindowFrame = (IVsWindowFrame)GetService(typeof(IVsWindowFrame));
		if (vsWindowFrame != null)
		{
			___(vsWindowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_EditorCaption, ""));
			string text = GetAdditionalTooltipOrWindowCaption(toolTip: false);
			string text2 = " - ";
			if (string.IsNullOrEmpty(text))
			{
				text2 = string.Empty;
				text = " ";
			}

			if (PersistentSettings.EditorStatusTabTextIncludeFileName)
			{
				text = "%2" + text2 + text;
			}

			___(vsWindowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_OwnerCaption, text));
		}
	}

	private void UpdateToolTip()
	{
		Diag.ThrowIfNotOnUIThread();

		IVsWindowFrame vsWindowFrame = (IVsWindowFrame)GetService(typeof(IVsWindowFrame));
		if (vsWindowFrame == null)
		{
			return;
		}

		___(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out object pvar));
		___(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out object pvar2));
		if ((int)pvar2 < 0)
		{
			return;
		}

		uint itemid = Convert.ToUInt32(pvar2, CultureInfo.InvariantCulture);
		IVsHierarchy vsHierarchy = pvar as IVsHierarchy;
		vsHierarchy?.SetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ShowOnlyItemCaption, true);
		AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);

		// There are no Firebird projects so this will never execute
		if (auxDocData != null && auxDocData.Strategy != null
			&& (auxDocData.Strategy.Mode == EnEditorMode.CustomOnline
			|| auxDocData.Strategy.Mode == EnEditorMode.CustomProject))
		{
			string text = GetAdditionalTooltipOrWindowCaption(toolTip: true);
			string text2 = Environment.NewLine;
			if (string.IsNullOrEmpty(text))
			{
				text2 = string.Empty;
			}

			if (auxDocData.Strategy.Mode == EnEditorMode.CustomProject)
			{
				// Never happen - projects not suppoerted in Firebird
				text = DocumentMoniker + text2 + text;
			}
			else if (auxDocData.Strategy.Mode == EnEditorMode.CustomOnline)
			{
				// Never happen - online not suppoerted ATM
				___(vsHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out var pvar3));
				text = (string)pvar3 + text2 + text;
			}

			vsHierarchy.SetProperty((uint)(int)pvar2, (int)__VSHPROPID4.VSHPROPID_DescriptiveName, text);
		}
	}

	private string GetAdditionalTooltipOrWindowCaption(bool toolTip)
	{
		string text = string.Empty;
		AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(DocData);
		if (auxDocData != null && auxDocData.QryMgr != null)
		{
			QueryManager qryMgr = auxDocData.QryMgr;
			text = qryMgr.ConnectionStrategy.GetEditorCaption(toolTip);
			// IBSqlEditorUserSettings current = SqlEditorUserSettings.Instance.Current;
			if (!toolTip && (PersistentSettings.EditorStatusTabTextIncludeDatabaseName
				|| PersistentSettings.EditorStatusTabTextIncludeLoginName || PersistentSettings.EditorStatusTabTextIncludeServerName))
			{
				if (qryMgr.IsConnected || qryMgr.IsConnecting)
				{
					if (qryMgr.IsExecuting)
						text += " " + ControlsResources.ConnectionStateExecuting;
				}
				else
				{
					text += " " + ControlsResources.ConnectionStateNotConnected;
				}
			}
		}

		return text;
	}

	private bool IsTabButtonChecked(AbstractEditorTab tab)
	{
		if (tab != null)
		{
			return TabbedEditorUiCtl.SplitViewContainer.GetButton(tab).Checked;
		}

		return false;
	}

	public void ConfigureTextViewForAutonomousFind(IVsWindowFrame frame, IVsTextView textView)
	{
		if (frame == null)
		{
			ArgumentNullException ex = new("frame");
			Diag.Dug(ex);
			throw ex;
		}

		if (textView == null)
		{
			ArgumentNullException ex = new("textView");
			Diag.Dug(ex);
			throw ex;
		}

		Diag.ThrowIfNotOnUIThread();

		frame.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out object pvar);
		Microsoft.VisualStudio.OLE.Interop.IServiceProvider site = pvar as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
		if (textView is not IObjectWithSite objectWithSite)
		{
			// Tracer.Trace(typeof(AbstractSqlEditorTab), "ConfigureTextViewForAutonomousFind", "Couldn't cast textView to IObjectWithsite!");
			return;
		}

		objectWithSite.SetSite(site);
		ITextView wpfTextView = GetWpfTextView(textView);
		if (wpfTextView == null)
		{
			// Tracer.Trace(typeof(AbstractSqlEditorTab), "ConfigureTextViewForAutonomousFind", "Couldn't get ITextView from IVsTextView!");
		}
		else
		{
			wpfTextView.Options.SetOptionValue("Enable Autonomous Find", true);
		}
	}

	private bool IsGridCheckedAndMoreThanOneResultSet()
	{
		if (IsResultsGridButtonChecked)
		{
			if (GridResultsPanel == null)
			{
				return GridResultsPanel.NumberOfGrids > 1;
			}

			return true;
		}

		return false;
	}

	public override void ActivateNextTab()
	{
		if (IsGridCheckedAndMoreThanOneResultSet())
		{
			for (int i = 0; i < GridResultsPanel.NumberOfGrids - 1; i++)
			{
				if (GridResultsPanel.GridContainers[i].GridCtl.ContainsFocus)
				{
					GridResultsPanel.GridContainers[i + 1].GridCtl.Focus();
					return;
				}
			}
		}

		base.ActivateNextTab();
		if (IsGridCheckedAndMoreThanOneResultSet())
		{
			GridResultsPanel.GridContainers[0].GridCtl.Focus();
		}
	}

	public override void ActivatePreviousTab()
	{
		if (IsGridCheckedAndMoreThanOneResultSet())
		{
			for (int num = GridResultsPanel.NumberOfGrids - 1; num > 0; num--)
			{
				if (GridResultsPanel.GridContainers[num].GridCtl.ContainsFocus)
				{
					GridResultsPanel.GridContainers[num - 1].GridCtl.Focus();
					return;
				}
			}
		}

		base.ActivatePreviousTab();
		if (IsGridCheckedAndMoreThanOneResultSet())
		{
			GridResultsPanel.GridContainers[GridResultsPanel.NumberOfGrids - 1].GridCtl.Focus();
		}
	}



	int IVsFindTarget.Find(string pszSearch, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out uint pResult)
	{
		return VSFindTargetAdapter.Find(pszSearch, grfOptions, fResetStartPoint, pHelper, out pResult);
	}

	int IVsFindTarget.GetCapabilities(bool[] pfImage, uint[] pgrfOptions)
	{
		return VSFindTargetAdapter.GetCapabilities(pfImage, pgrfOptions);
	}

	int IVsFindTarget.GetCurrentSpan(TextSpan[] pts)
	{
		return VSFindTargetAdapter.GetCurrentSpan(pts);
	}

	int IVsFindTarget.GetFindState(out object ppunk)
	{
		return VSFindTargetAdapter.GetFindState(out ppunk);
	}

	int IVsFindTarget.GetMatchRect(RECT[] prc)
	{
		return VSFindTargetAdapter.GetMatchRect(prc);
	}

	int IVsFindTarget.GetProperty(uint propid, out object pvar)
	{
		return VSFindTargetAdapter.GetProperty(propid, out pvar);
	}

	int IVsFindTarget.GetSearchImage(uint grfOptions, IVsTextSpanSet[] ppSpans, out IVsTextImage ppTextImage)
	{
		return VSFindTargetAdapter.GetSearchImage(grfOptions, ppSpans, out ppTextImage);
	}

	int IVsFindTarget.MarkSpan(TextSpan[] pts)
	{
		return VSFindTargetAdapter.MarkSpan(pts);
	}

	int IVsFindTarget.NavigateTo(TextSpan[] pts)
	{
		return VSFindTargetAdapter.NavigateTo(pts);
	}

	int IVsFindTarget.NotifyFindTarget(uint notification)
	{
		return VSFindTargetAdapter.NotifyFindTarget(notification);
	}

	int IVsFindTarget.Replace(string pszSearch, string pszReplace, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out int pfReplaced)
	{
		return VSFindTargetAdapter.Replace(pszSearch, pszReplace, grfOptions, fResetStartPoint, pHelper, out pfReplaced);
	}

	int IVsFindTarget.SetFindState(object pUnk)
	{
		return VSFindTargetAdapter.SetFindState(pUnk);
	}

	public int NavigateTo2(IVsTextSpanSet pSpans, TextSelMode iSelMode)
	{
		return VSFindTargetAdapter.NavigateTo2(pSpans, iSelMode);
	}

	int IBVsFindTarget3.IsNewUISupported
	{
		get { return VSFindTargetAdapter.IsNewUISupported; }
	}

	int IBVsFindTarget3.NotifyShowingNewUI()
	{
		return VSFindTargetAdapter.NotifyShowingNewUI();
	}

	bool IBWindowPane.IsDisposed
	{
		get
		{
			return IsDisposed;
		}
	}
}
