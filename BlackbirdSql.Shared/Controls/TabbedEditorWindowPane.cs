// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorTabbedEditorPane

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Shared.Controls.PropertiesWindow;
using BlackbirdSql.Shared.Controls.ResultsPanels;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Controls.Widgets;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Commands;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.Shared.Controls;


public class TabbedEditorWindowPane : AbstractTabbedEditorWindowPane, IBSqlEditorWindowPane, IBsEditorWindowPane, IVsFindTarget, IVsFindTarget2, IBsVsFindTarget3
{
	public TabbedEditorWindowPane(System.IServiceProvider provider, Package package, object docData, string fileName, bool autoExecute, string editorId = "")
	: base(provider, package, docData, Guid.Empty, 0u)
	{
		FileName = fileName;
		EditorId = editorId;
		_AutoExecute = autoExecute;


		VSFindTargetAdapter = new FindTargetAdapter(this);
	}

	private DisplaySQLResultsControl _ResultsControl;

	private ViewCommandFilter _ViewFilter;

	private bool _IsFirstTimeToShow = true;
	private bool _AutoExecute = false;

	private bool _IsFirstTimePaneCustomizationForResults = true;

	private readonly string _HelpString = "BlackbirdSql.Shared.Controls.SqlEditor";

	private PropertiesWindowManager _ProperyWindowManager;

	private FindTargetAdapter VSFindTargetAdapter { get; set; }


	public ViewCommandFilter ViewFilter => _ViewFilter;

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

	// public bool IsExecutionPlanTabVisible => GetSqlExecutionPlanTab().IsVisible;

	// public bool IsExecutionPlanButtonChecked => IsTabButtonChecked(GetSqlExecutionPlanTab());

	/*
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
	*/

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

	/*
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
	*/

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


	public string WindowBaseName
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			string text = Path.GetFileNameWithoutExtension(DocumentMoniker);

			IVsWindowFrame vsWindowFrame = (IVsWindowFrame)GetService(typeof(IVsWindowFrame));
			if (vsWindowFrame == null)
				return text;

			___(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out object pHierarchy));
			if (pHierarchy is not IVsHierarchy vsHierarchy)
				return text;

			___(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out object pItemId));
			if ((int)pItemId < 0)
				return text;

			uint itemid = Convert.ToUInt32(pItemId, CultureInfo.InvariantCulture);

			___(vsHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out object pName));

			text = Path.GetFileNameWithoutExtension((string)pName);

			return text;
		}
	}




	protected override void Initialize()
	{
		if (ApcManager.SolutionClosing)
			return;

		TabbedEditorUiCtl.SuspendLayout();
		TabbedEditorUiCtl.SplitViewContainer.SuspendLayout();

		try
		{
			base.Initialize();
			_ViewFilter = CreateViewFilter();
			SplitViewContainer splitViewContainer = TabbedEditorUiCtl.SplitViewContainer;
			splitViewContainer.CustomizeSplitterBarButton(VSConstants.LOGVIEWID_TextView, EnSplitterBarButtonDisplayStyle.Text, ControlsResources.ToolStripButton_Sql_Button_Text, ControlsResources.ImgTSQL);
			splitViewContainer.CustomizeSplitterBarButton(VSConstants.LOGVIEWID_Designer, EnSplitterBarButtonDisplayStyle.Text, ControlsResources.ToolStripButton_ResultsGrid_Button_Text, ControlsResources.ImgResults);
			splitViewContainer.SplittersVisible = false;
			splitViewContainer.IsSplitterVisible = false;
			splitViewContainer.SwapButtons();
			splitViewContainer.PathStripVisibleInSplitMode = false;
			TabbedEditorUiCtl.TabActivatedEvent += TabActivatedHandler;
			AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);
			if (auxDocData != null && auxDocData.QryMgr != null)
			{
				auxDocData.QryMgr.StatusChangedEvent += OnUpdateTooltipAndWindowCaption;
				auxDocData.QryMgr.ExecutionCompletedEvent += OnQueryExecutionCompleted;
			}

			UpdateWindowCaption();
			UpdateToolTip();
		}
		finally
		{
			TabbedEditorUiCtl.SplitViewContainer.ResumeLayout(performLayout: false);
			TabbedEditorUiCtl.ResumeLayout(performLayout: false);
		}

		TabbedEditorUiCtl.PerformLayout();
	}

	private void TabActivatedHandler(object sender, EventArgs args)
	{
		// Tracer.Trace(GetType(), "TabActivatedHandler", "SenderType: {0}, View: {1}.", sender.GetType().Name, (sender as AbstractEditorTab).GetView());
		((sender as AbstractEditorTab).GetView() as ResultWindowPane)?.SetFocus();
	}

	protected virtual ViewCommandFilter CreateViewFilter()
	{
		return new ViewCommandFilter(this);
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

		/*
		if (logicalView == new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid))
		{
			return new SqlEditorResultsTab(this, logicalView, editorLogicalView, editorTabType);
		}
		*/

		return result;
	}

	public override bool EnsureTabs(bool activateTextView)
	{
		if (ApcManager.SolutionClosing)
			return TabbedEditorUiCtl.Tabs.Count > 0;



		AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);

		if (auxDocData == null)
			return false;

		SplitViewContainer splitViewContainer = TabbedEditorUiCtl.SplitViewContainer;
		QueryManager qryMgr = auxDocData.QryMgr;

		bool hasActualPlan = qryMgr.LiveSettings.ExecutionType == EnSqlExecutionType.QueryWithPlan;

		// string btnPlanText = hasActualPlan
		//	? ControlsResources.ToolStripButton_ActualPlan_Button_Text
		//	: ControlsResources.ToolStripButton__Plan_Button_Text;
		string btnTextPlanText = hasActualPlan
			? ControlsResources.ToolStripButton_ActualTextPlan_Button_Text
			: ControlsResources.ToolStripButton_TextPlan_Button_Text;


		if (TabbedEditorUiCtl.Tabs.Count > 0)
		{
			// splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid),
			// 	EnSplitterBarButtonDisplayStyle.Text, btnPlanText, ControlsResources.ImgExecutionPlan);

			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, btnTextPlanText, ControlsResources.ImgTextPlan);

			return true;
		}


		if (!base.EnsureTabs(activateTextView))
			return false;


		SqlEditorResultsTab resultsTab = GetSqlEditorResultsTab();

		if (resultsTab == null)
		{
			Exception ex = new("GetSqlEditorResultsTab returned null");
			Diag.Dug(ex);
			throw ex;
		}

		resultsTab.Hide();

		TabbedEditorUiCtl.SuspendLayout();

		try
		{

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

			/*
			AbstractEditorTab editorTab3 = CreateEditorTabWithButton(this, new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab3);
			LoadDesignerPane(editorTab3, asPrimary: false, showSplitter: false);
			editorTab3.Hide();
			*/
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
				EnSplitterBarButtonDisplayStyle.Text, ControlsResources.ToolStripButton_Message_Button_Text,
				ControlsResources.ImgMessages);
			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlTextResultsTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, ControlsResources.ToolStripButton_ResultsText_Button_Text,
				ControlsResources.ImgMessages);

			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlStatisticsTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text,
				ControlsResources.ToolStripButton_StatisticsSnapshot_Button_Text,
				ControlsResources.ImgStatistics);

			// splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid),
			//	EnSplitterBarButtonDisplayStyle.Text, btnPlanText, ControlsResources.ImgExecutionPlan);
			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid),
				EnSplitterBarButtonDisplayStyle.Text, btnTextPlanText, ControlsResources.ImgTextPlan);

			TabbedEditorUiCtl.SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(new Guid(LibraryData.SqlTextResultsTabLogicalViewGuid), visible: false);
			// TabbedEditorUiCtl.SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid), visible: false);
			TabbedEditorUiCtl.SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(new Guid(LibraryData.SqlTextPlanTabLogicalViewGuid), visible: false);

			IVsTextView codeEditorTextView = GetCodeEditorTextView();

			if (codeEditorTextView != null)
				AbstractViewCommandFilter.AddFilterToView(codeEditorTextView, _ViewFilter);

			EnsureDisplayResultsControl();

			_ProperyWindowManager = new PropertiesWindowManager(this);

			qryMgr.CurrentWorkingDirectoryPath = GetCurrentWorkingDirectory;

			SqlEditorCodeTab sqlEditorCodeTab = GetSqlEditorCodeTab();

			ConfigureTextViewForAutonomousFind(sqlEditorCodeTab.CurrentFrame, GetCodeEditorTextView(sqlEditorCodeTab));
			ConfigureTextViewForAutonomousFind(GetSqlEditorTextResultsTab().CurrentFrame, _ResultsControl.TextResultsPaneTextView);
			ConfigureTextViewForAutonomousFind(GetSqlEditorMessageTab().CurrentFrame, _ResultsControl.MessagesPaneTextView);

			if (_ResultsControl.TextPlanPaneTextView != null)
				ConfigureTextViewForAutonomousFind(GetSqlTextPlanTab().CurrentFrame, _ResultsControl.TextPlanPaneTextView);

			// Doesn't work to activate Intellisense.
			// if (activateTextView)
			//	sqlEditorCodeTab.Activate(true);

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



	public override bool UpdateTabsButtonText(QueryExecutionCompletedEventArgs args)
	{
		if (!base.UpdateTabsButtonText(args))
			return false;

		if (!ThreadHelper.CheckAccess())
		{
			_ = Task.Factory.StartNew(() => UpdateTabsButtonTextAsync(args),
					default, TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach,
					TaskScheduler.Default);
			return true;
		}

		return UpdateTabsButtonTextImpl(args);
	}

	private async Task<bool> UpdateTabsButtonTextAsync(QueryExecutionCompletedEventArgs args)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		return UpdateTabsButtonTextImpl(args);
	}

	private bool UpdateTabsButtonTextImpl(QueryExecutionCompletedEventArgs args)
	{
		int messageCount = args.ErrorCount + args.MessageCount;
		int snapshotCount = args.WithClientStats && _ResultsControl != null && _ResultsControl.ClientStatisticsCollection != null
			? _ResultsControl.ClientStatisticsCollection.Count : 0;

		SplitViewContainer splitViewContainer = TabbedEditorUiCtl.SplitViewContainer;

		bool statsUpdated = !args.WithClientStats;
		bool resultsUpdated = false; // args.OutputMode == EnSqlOutputMode.ToFile;
		bool messagesUpdated = false;
		int i;

		string statsButtonText = null;
		string resultsButtonText = null;
		string messagesButtonText;

		if (!statsUpdated)
		{
			statsButtonText = snapshotCount == 0
				? ControlsResources.ToolStripButton_StatisticsSnapshot_Button_Text
				: ControlsResources.ToolStripButton_StatisticsSnapshotCount_Button_Text.FmtRes(snapshotCount);
		}


		if (!resultsUpdated)
		{
			if (args.OutputMode == EnSqlOutputMode.ToGrid)
			{
				resultsButtonText = args.TotalRowsSelected == 0
					? ControlsResources.ToolStripButton_ResultsGrid_Button_Text
					: ControlsResources.ToolStripButton_ResultsGridCount_Button_Text.FmtRes(args.TotalRowsSelected, args.StatementCount);
			}
			else
			{
				resultsButtonText = args.TotalRowsSelected == 0
					? ControlsResources.ToolStripButton_ResultsText_Button_Text
					: ControlsResources.ToolStripButton_ResultsTextCount_Button_Text.FmtRes(args.TotalRowsSelected, args.StatementCount);
			}
		}

		messagesButtonText = messageCount == 0
			? ControlsResources.ToolStripButton_Message_Button_Text
			: ControlsResources.ToolStripButton_MessageCount_Button_Text.FmtRes(messageCount);


		Guid clsidMessagesTab = new Guid(LibraryData.SqlMessageTabLogicalViewGuid);
		Guid clsidStatisticsTab = new Guid(LibraryData.SqlStatisticsTabLogicalViewGuid);
		Guid clsidResultsTab = args.OutputMode == EnSqlOutputMode.ToGrid
			? VSConstants.LOGVIEWID_Designer : new Guid(LibraryData.SqlTextResultsTabLogicalViewGuid);


		Exception expected = null;
		bool result = false;

		for (i = 0; i < 50 && (!statsUpdated || !resultsUpdated || !messagesUpdated); i++)
		{
			if (!splitViewContainer.EventEnter())
			{
				Thread.Sleep(10);
				continue;
			}

			try
			{
				result = UpdateSplitViewContainerButtonText(splitViewContainer, clsidResultsTab, clsidMessagesTab,
					clsidStatisticsTab, resultsButtonText, messagesButtonText, statsButtonText,
					ref resultsUpdated, ref messagesUpdated, ref statsUpdated, ref expected);
			}
			finally
			{
				splitViewContainer.EventExit();
			}

			if (!result)
				Thread.Sleep(10);
		}


		if (expected != null)
		{
			Diag.Expected(expected, $"Attempted SplitterBarButton update {i} times. Messages tab button update {(messagesUpdated ? "" : "un")}successful. Results tab button update {(resultsUpdated ? "" : "un")}successful. Stats tab button update {(statsUpdated ? "" : "un")}successful.");
		}

		return resultsUpdated && statsUpdated && messagesUpdated;
	}

	private bool UpdateSplitViewContainerButtonText(SplitViewContainer splitViewContainer, 
		Guid clsidResultsTab, Guid clsidMessagesTab, Guid clsidStatisticsTab,
		string resultsButtonText, string messagesButtonText, string statsButtonText,
		ref bool resultsUpdated, ref bool messagesUpdated, ref bool statsUpdated, ref Exception expected)
	{
		bool result = false;

		TabbedEditorUiCtl.SuspendLayout();

		try
		{
			if (!messagesUpdated)
			{
				splitViewContainer.CustomizeSplitterBarButtonText(clsidMessagesTab, messagesButtonText);
				messagesUpdated = true;
				result = true;
			}

			if (!statsUpdated)
			{
				splitViewContainer.CustomizeSplitterBarButtonText(clsidStatisticsTab, statsButtonText);
				statsUpdated = true;
				result = true;
			}

			if (!resultsUpdated)
			{
				splitViewContainer.CustomizeSplitterBarButtonText(clsidResultsTab, resultsButtonText);
				resultsUpdated = true;
				result = true;
			}

		}
		catch (Exception ex)
		{
			expected = ex;
		}
		finally
		{
			TabbedEditorUiCtl.ResumeLayout();
		}

		return result;

	}

	protected override int HandleCloseEditorOrDesigner()
	{
		// Tracer.Trace(GetType(), "HandleCloseEditorOrDesigner()");

		int result = 0;

		if (Cmd.ShouldStopCloseDialog(((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData), GetType()))
		{
			result = VSConstants.OLE_E_PROMPTSAVECANCELLED;
		}

		return result;
	}

	protected override int SaveFiles(ref uint pgrfSaveOptions)
	{
		// Tracer.Trace(GetType(), "SaveFiles()");

		AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);

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

			if (_AutoExecute)
			{
				_AutoExecute = false;
				AsyncAutoExecuteQuery();
			}
		}
	}

	public override int OnExec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (pguidCmdGroup == CommandProperties.ClsidCommandSet)
		{
			AbstractCommand sqlEditorCommand = null;

			EnCommandSet cmd = (EnCommandSet)nCmdID;
			switch (cmd)
			{
				case EnCommandSet.CmdIdResultsAsText:
					sqlEditorCommand = new CommandResultsAsText(this);
					break;
				case EnCommandSet.CmdIdResultsAsGrid:
					sqlEditorCommand = new CommandResultsAsGrid(this);
					break;
				case EnCommandSet.CmdIdResultsAsFile:
					sqlEditorCommand = new CommandResultsAsFile(this);
					break;
				case EnCommandSet.CmdIdToggleResultsPane:
					sqlEditorCommand = new CommandToggleResultsPane(this);
					break;
			}

			if (sqlEditorCommand != null)
			{
				return sqlEditorCommand.Exec(nCmdexecopt, pvaIn, pvaOut);
			}
		}

		// Tracer.Trace(GetType(), "OnExec()", "pguidCmdGroup: {0}, nCmdId: {1}.", pguidCmdGroup, nCmdID);

		/* No extended command handlers
		 * 
		AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);

		if (auxDocData != null && auxDocData.StrategyFactory != null)
		{
			IBsExtendedCommandHandler extendedCommandHandler = auxDocData.StrategyFactory.ExtendedCommandHandler;

			if (extendedCommandHandler != null && extendedCommandHandler.OnExec(this, ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut) == 0)
			{
				return VSConstants.S_OK;
			}
		}
		*/

		return base.OnExec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
	}

	public override int OnQueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		for (int i = 0; i < cCmds; i++)
		{
			uint cmdID = prgCmds[i].cmdID;
			if (pguidCmdGroup == CommandProperties.ClsidCommandSet)
			{
				AbstractCommand sqlEditorCommand = null;
				EnCommandSet cmd = (EnCommandSet)cmdID;

				switch (cmd)
				{
					case EnCommandSet.CmdIdResultsAsText:
						sqlEditorCommand = new CommandResultsAsText(this);
						break;
					case EnCommandSet.CmdIdResultsAsGrid:
						sqlEditorCommand = new CommandResultsAsGrid(this);
						break;
					case EnCommandSet.CmdIdResultsAsFile:
						sqlEditorCommand = new CommandResultsAsFile(this);
						break;
					case EnCommandSet.CmdIdToggleResultsPane:
						sqlEditorCommand = new CommandToggleResultsPane(this);
						break;
				}

				if (sqlEditorCommand != null)
				{
					return sqlEditorCommand.QueryStatus(ref prgCmds[i], pCmdText);
				}
			}
		}

		/* No extended command handlers
		 * 
		AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);

		if (auxDocData != null && auxDocData.StrategyFactory != null)
		{
			IBsExtendedCommandHandler extendedCommandHandler = auxDocData.StrategyFactory.ExtendedCommandHandler;
			if (extendedCommandHandler != null && extendedCommandHandler.OnQueryStatus(this, ref pguidCmdGroup, cCmds, prgCmds, pCmdText) == 0)
			{
				return VSConstants.S_OK;
			}
		}
		*/

		return base.OnQueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
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

	public IVsTextView GetCodeEditorTextView(object editorCodeTab = null)
	{
		IVsTextView ppView = null;

		SqlEditorCodeTab sqlEditorCodeTab;

		if (editorCodeTab == null)
			sqlEditorCodeTab = GetSqlEditorCodeTab();
		else
			sqlEditorCodeTab = editorCodeTab as SqlEditorCodeTab;


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
		if (ApcManager.GetService<SComponentModel>() is not IComponentModel componentModelSvc)
			return null;

		if (componentModelSvc.GetService<IVsEditorAdaptersFactoryService>() is not IVsEditorAdaptersFactoryService factorySvc)
			return null;

		return factorySvc.GetWpfTextView(textView);
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

	/*
	public SqlEditorResultsTab GetSqlExecutionPlanTab()
	{
		Guid guidSqlExecutionPlanTabLogicalView = new Guid(LibraryData.SqlExecutionPlanTabLogicalViewGuid);
		return GetSqlEditorTab<SqlEditorResultsTab>(guidSqlExecutionPlanTabLogicalView);
	}
	*/

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
			return tab as T;

		Diag.StackException($"Could not get editor tab type: {typeof(T)}  Guid:{rguidLogicalView}");
		return result;
	}

	public DisplaySQLResultsControl EnsureDisplayResultsControl()
	{
		if (_ResultsControl == null && !ApcManager.SolutionClosing)
		{
			ResultWindowPane resultsGridPanel = GetSqlEditorResultsTab().GetView() as ResultWindowPane;
			ResultWindowPane spatialPane = null;
			ResultWindowPane messagePanel = GetSqlEditorMessageTab().GetView() as ResultWindowPane;
			ResultWindowPane textResultsPanel = GetSqlEditorTextResultsTab().GetView() as ResultWindowPane;
			ResultWindowPane statisticsPanel = GetSqlEditorStatisticsTab().GetView() as ResultWindowPane;
			// ResultWindowPane executionPlanPanel = GetSqlExecutionPlanTab().GetView() as ResultWindowPane;
			ResultWindowPane textPlanPanel = GetSqlTextPlanTab().GetView() as ResultWindowPane;
			_ResultsControl = new DisplaySQLResultsControl(resultsGridPanel, messagePanel, textResultsPanel,
				statisticsPanel, /* executionPlanPanel,*/ textPlanPanel, spatialPane, this);
			_ResultsControl.SetSite(ApcManager.OleServiceProvider);
		}

		return _ResultsControl;
	}

	public void AsyncExecuteQuery(EnSqlExecutionType executionType)
	{
		// Tracer.Trace(GetType(), "AsyncExecuteQuery()");


		using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(Microsoft.VisualStudio.Utilities.DpiAwarenessContext.SystemAware))
		{
			SqlTextSpan sqlTextSpan = GetSelectedCodeEditorTextSpan2();
			if (sqlTextSpan.Text == null || sqlTextSpan.Text.Length == 0)
			{
				sqlTextSpan = GetAllCodeEditorTextSpan2();
			}

			if (sqlTextSpan != null && !string.IsNullOrEmpty(sqlTextSpan.Text))
			{
				AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);
				QueryManager qryMgr = auxDocData.QryMgr;

				// Tracer.Trace(GetType(), "AsyncExecuteQuery", "calling QryMgr.AsyncExecute()");


				// ----------------------------------------------------------------------------------- //
				// ******************** Execution Point (1) - TabbedEditorWindowPane.AsyncExecuteQuery() ******************** //
				// ----------------------------------------------------------------------------------- //
				qryMgr.AsyncExecute(sqlTextSpan, executionType);

			}
		}
	}


	private void AsyncAutoExecuteQuery()
	{
		// Tracer.Trace(GetType(), "OnExec()", "ExecutionType: {0}.", executionType);

		// ----------------------------------------------------------------------------------- //
		// ******************** Execution Point (0) - AbstractEditorFactory.ExecuteQuery() ******************** //
		// ----------------------------------------------------------------------------------- //
		_ = Task.Run(() => AutoExecuteQueryAsync(5));
	}



	private async Task<bool> AutoExecuteQueryAsync(int delay)
	{
		// Tracer.Trace(GetType(), "ExecuteQueryAsync()");

		// Give editor time to breath.
		if (delay > 0)
			System.Threading.Thread.Sleep(delay);

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		AsyncExecuteQuery(EnSqlExecutionType.QueryOnly);

		return true;
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


	protected override void Dispose(bool disposing)
	{
		if (_IsDisposed)
			return;

		if (disposing)
		{
			_ViewFilter?.Dispose();
			_ResultsControl?.Dispose();
			_ResultsControl = null;

			AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);
			if (auxDocData != null && auxDocData.QryMgr != null)
			{
				auxDocData.QryMgr.StatusChangedEvent -= OnUpdateTooltipAndWindowCaption;
				auxDocData.QryMgr.ExecutionCompletedEvent -= OnQueryExecutionCompleted;
			}

			_ProperyWindowManager?.Dispose();
		}

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
		try
		{
			AbstractEditorTab sqlEditorCodeTab = GetSqlEditorCodeTab();
			ActivateTab(sqlEditorCodeTab);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
	}

	/*
	public void ActivateExecutionPlanTab()
	{
		AbstractEditorTab sqlExecutionPlanTab = GetSqlExecutionPlanTab();
		ActivateTab(sqlExecutionPlanTab);
	}
	*/

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
		AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);
		EnSqlOutputMode sqlExecutionMode = auxDocData.SqlOutputMode;
		AbstractEditorTab sqlEditorResultsTab = GetSqlEditorResultsTab();
		GetSqlEditorMessageTab();
		AbstractEditorTab sqlEditorTextResultsTab = GetSqlEditorTextResultsTab();
		AbstractEditorTab sqlEditorStatisticsTab = GetSqlEditorStatisticsTab();
		// AbstractEditorTab sqlExecutionPlanTab = GetSqlExecutionPlanTab();
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

		sqlEditorStatisticsTab.TabButtonVisible = auxDocData.ClientStatisticsEnabled && auxDocData.ExecutionType != EnSqlExecutionType.PlanOnly;
		// sqlExecutionPlanTab.TabButtonVisible = false; // Parser not completed
		sqlTextPlanTab.TabButtonVisible = auxDocData.HasExecutionPlan;

		if (sqlExecutionMode == EnSqlOutputMode.ToFile || sqlExecutionMode == EnSqlOutputMode.ToText
			|| auxDocData.ExecutionType == EnSqlExecutionType.PlanOnly)
		{
			if (auxDocData.ExecutionType == EnSqlExecutionType.PlanOnly)
				ActivateTextResultsTab();

			if (sqlEditorResultsTab != null)
				sqlEditorResultsTab.TabButtonVisible = false;

			if (sqlEditorTextResultsTab != null)
				sqlEditorTextResultsTab.TabButtonVisible = auxDocData.ExecutionType != EnSqlExecutionType.PlanOnly;

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

		if (auxDocData.ExecutionType == EnSqlExecutionType.PlanOnly)
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
		AuxilliaryDocData auxDocData;

		try
		{
			TabbedEditorUiCtl.SuspendLayout();

			try
			{
				auxDocData =
					((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData)
					?? throw new NullReferenceException("(AuxilliaryDocData)auxDocData");

				if (auxDocData.StrategyFactory == null)
					throw new NullReferenceException("auxDocData.StrategyFactory");
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			Guid clsidCmdSet = CommandProperties.ClsidCommandSet;
			uint menuId;


			switch (auxDocData.StrategyFactory.Mode)
			{
				case EnEditorMode.CustomOnline:
					menuId = (uint)EnCommandSet.ToolbarIdOnlineWindow;
					TabbedEditorUiCtl.InitializeToolbarHost(this, clsidCmdSet, menuId);
					break;
				case EnEditorMode.Standard:
					menuId = (uint)EnCommandSet.ToolbarIdEditorWindow;
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

	public void OnUpdateTooltipAndWindowCaption(object sender, QueryStatusChangedEventArgs args)
	{
		_ = Task.Run(() => OnUpdateTooltipAndWindowCaptionAsync(sender, args));

	}

	public async Task OnUpdateTooltipAndWindowCaptionAsync(object sender, QueryStatusChangedEventArgs args)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		// if (args.Change == QueryManager.EnStatusType.Connection || args.Change == QueryManager.EnStatusType.Connected)
		// {
			UpdateToolTip();
		// }


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
			return;

		___(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out object pHierarchy));
		if (pHierarchy is not IVsHierarchy vsHierarchy)
			return;

		___(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out object pItemId));
		if ((int)pItemId < 0)
			return;
		uint itemid = Convert.ToUInt32(pItemId, CultureInfo.InvariantCulture);

		// vsHierarchy.SetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ShowOnlyItemCaption, true);

		AuxilliaryDocData auxDocData = null; // ((IBEditorsPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);

		string text;

		// There are no Database projects for the Firebird port so this will never execute
		if (auxDocData != null && auxDocData.StrategyFactory != null
			&& (auxDocData.StrategyFactory.Mode == EnEditorMode.CustomOnline
			|| auxDocData.StrategyFactory.Mode == EnEditorMode.CustomProject))
		{
			text = GetAdditionalTooltipOrWindowCaption(toolTip: true);
			string text2 = Environment.NewLine;
			if (string.IsNullOrEmpty(text))
			{
				text2 = string.Empty;
			}

			if (auxDocData.StrategyFactory.Mode == EnEditorMode.CustomProject)
			{
				// Never happen - projects not supported in Firebird port.
				text = DocumentMoniker + text2 + text;
			}
			else if (auxDocData.StrategyFactory.Mode == EnEditorMode.CustomOnline)
			{
				// Never happen - online not supported ATM
				___(vsHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out object pName));
				text = (string)pName + text2 + text;
			}
		}
		else
		{
			text = DocumentMoniker;
		}

		vsHierarchy.SetProperty(itemid, (int)__VSHPROPID4.VSHPROPID_DescriptiveName, text);
	}


	private string GetAdditionalTooltipOrWindowCaption(bool toolTip)
	{
		string text = string.Empty;
		AuxilliaryDocData auxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(DocData);
		if (auxDocData != null && auxDocData.QryMgr != null)
		{
			QueryManager qryMgr = auxDocData.QryMgr;
			text = qryMgr.Strategy.GetEditorCaption(toolTip);
			// IBSqlEditorUserSettings current = SqlEditorUserSettings.Instance.Current;

			if (!toolTip && (PersistentSettings.EditorStatusTabTextIncludeDatabaseName
				|| PersistentSettings.EditorStatusTabTextIncludeLoginName
				|| PersistentSettings.EditorStatusTabTextIncludeServerName))
			{
				if (qryMgr.IsConnected || qryMgr.IsConnecting)
				{
					if (qryMgr.IsExecuting)
						text += " " + ControlsResources.ConnectionStateExecuting;
				}
				else
				{
					text += " " + ControlsResources.TabbedEditorWindowPane_ConnectionStateNotConnected;
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

		wpfTextView?.Options.SetOptionValue("Enable Autonomous Find", true);

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

	int IBsVsFindTarget3.IsNewUISupported
	{
		get { return VSFindTargetAdapter.IsNewUISupported; }
	}

	int IBsVsFindTarget3.NotifyShowingNewUI()
	{
		return VSFindTargetAdapter.NotifyShowingNewUI();
	}

	bool IBsWindowPane.IsDisposed
	{
		get
		{
			return _IsDisposed;
		}
	}
}
