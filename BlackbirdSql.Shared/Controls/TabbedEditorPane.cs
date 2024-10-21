// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorTabbedEditorPane

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Controls.PropertiesWindow;
using BlackbirdSql.Shared.Controls.Results;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Controls.Widgets;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Commands;
using BlackbirdSql.Shared.Ctl.Config;
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

using DpiAwareness = Microsoft.VisualStudio.Utilities.DpiAwareness;
using DpiAwarenessContext = Microsoft.VisualStudio.Utilities.DpiAwarenessContext;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



namespace BlackbirdSql.Shared.Controls;


// =========================================================================================================
//
//										TabbedEditorPane Class
//
/// <summary>
/// The containing editor window pane.
/// </summary>
// =========================================================================================================
public class TabbedEditorPane : AbstractTabbedEditorPane, IBsTabbedEditorPane
{

	// ----------------------------------------------------------------
	#region Constructors / Destructors - TabbedEditorPane
	// ----------------------------------------------------------------


	public TabbedEditorPane(System.IServiceProvider provider, IBsEditorPackage package,
			object docData, string fileName, bool cloned, bool autoExecute, string editorId = "")
		: base(provider, package, docData, cloned, Guid.Empty, 0u)
	{
		_FileName = fileName;
		_AutoExecute = autoExecute;
		_EditorId = editorId;

		_TargetAdapter = new FindTargetAdapter(this);
	}



	protected override void Dispose(bool disposing)
	{
		if (_IsDisposed || !disposing)
			return;


		try
		{
			_ViewFilter?.Dispose();
			_ResultsControl?.Dispose();
			_ProperyWindowManager?.Dispose();

			QueryManager qryMgr = QryMgr;

			if (qryMgr != null)
			{
				qryMgr.StatusChangedEvent -= OnUpdateTooltipAndWindowCaption;
				qryMgr.ExecutionCompletedEventAsync -= OnExecutionCompletedAsync;
			}
		}
		finally
		{
			base.Dispose(disposing);
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
			TabbedEditorUiCtl.TabActivatedEvent += OnTabActivated;

			QueryManager qryMgr = QryMgr;

			if (qryMgr != null)
			{
				qryMgr.StatusChangedEvent += OnUpdateTooltipAndWindowCaption;
				qryMgr.ExecutionCompletedEventAsync += OnExecutionCompletedAsync;
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


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - TabbedEditorPane
	// =========================================================================================================


	private const string C_HelpString = "BlackbirdSql.Shared.Controls.TabbedEditorPane";



	#endregion Constants





	// =========================================================================================================
	#region Fields - TabbedEditorPane
	// =========================================================================================================


	private bool _AutoExecute = false;
	private readonly string _EditorId = null;
	private readonly string _FileName = null;
	private bool _IsFirstTimePaneCustomizationForResults = true;
	private bool _IsFirstTimeToShow = true;
	private PropertiesWindowManager _ProperyWindowManager = null;
	private ResultsHandler _ResultsControl = null;
	private readonly FindTargetAdapter _TargetAdapter = null;
	private ViewCommandFilter _ViewFilter = null;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - TabbedEditorPane
	// =========================================================================================================


	public IOleCommandTarget CommandTargetMessagePane => PaneMessage;

	protected string EditorId => _EditorId;


	private EditorCodeTab EditorTabCode =>
		GetEditorTab<EditorCodeTab>(VSConstants.LOGVIEWID_TextView);


	/*
	private EditorResultsTab EditorTabExecutionPlan =>
		GetEditorTab<EditorResultsTab>(new Guid(LibraryData.C_ExecutionPlanTabGuid));
	*/


	private EditorMessageTab EditorTabMessage =>
		GetEditorTab<EditorMessageTab>(new Guid(LibraryData.C_MessageTabGuid));


	private EditorResultsTab EditorTabResults =>
		GetEditorTab<EditorResultsTab>(VSConstants.LOGVIEWID_Designer);


	private EditorResultsTab EditorTabStatistics =>
		GetEditorTab<EditorResultsTab>(new Guid(LibraryData.C_StatisticsTabGuid));


	public EditorMessageTab EditorTabTextPlan =>
		GetEditorTab<EditorMessageTab>(new Guid(LibraryData.C_TextPlanTabGuid));


	private EditorMessageTab EditorTabTextResults =>
		GetEditorTab<EditorMessageTab>(new Guid(LibraryData.C_TextResultsTabGuid));


	public string FileName => _FileName;


	public bool IsButtonCheckedCode => IsTabButtonChecked(EditorTabCode);
	// public bool IsButtonCheckedExecutionPlan => IsTabButtonChecked(EditorTabExecutionPlan);
	public bool IsButtonCheckedMessages => IsTabButtonChecked(EditorTabMessage);
	public bool IsButtonCheckedResultsGrid => IsTabButtonChecked(EditorTabResults);
	public bool IsButtonCheckedStatistics => IsTabButtonChecked(EditorTabStatistics);
	public bool IsButtonCheckedTextPlan => IsTabButtonChecked(EditorTabTextPlan);
	public bool IsButtonCheckedTextResults => IsTabButtonChecked(EditorTabTextResults);


	public bool IsButtonVisibleCode
	{
		get { return EditorTabCode.TabButtonVisible; }
		set { EditorTabCode.TabButtonVisible = value; }
	}

	/*
	public bool IsButtonVisibleExecutionPlan
	{
		get { return EditorTabExecutionPlan.TabButtonVisible; }
		set { EditorTabExecutionPlan.TabButtonVisible = value; }
	}
	*/

	public bool IsButtonVisibleMessages
	{
		get { return EditorTabMessage.TabButtonVisible; }
		set { EditorTabMessage.TabButtonVisible = value; }
	}

	public bool IsButtonVisibleResultsGrid
	{
		get { return EditorTabResults.TabButtonVisible; }
		set { EditorTabResults.TabButtonVisible = value; }
	}

	public bool IsButtonVisibleStatistics
	{
		get { return EditorTabStatistics.TabButtonVisible; }
		set { EditorTabStatistics.TabButtonVisible = value; }
	}

	public bool IsButtonVisibleTextPlan
	{
		get { return EditorTabTextPlan.TabButtonVisible; }
		set { EditorTabTextPlan.TabButtonVisible = value; }
	}

	public bool IsButtonVisibleTextResults
	{
		get { return EditorTabTextResults.TabButtonVisible; }
		set { EditorTabTextResults.TabButtonVisible = value; }
	}


	bool IBsWindowPane.IsDisposed => _IsDisposed;


	private bool IsGridCheckedAndMoreThanOneResultSet
	{
		get
		{
			if (IsButtonCheckedResultsGrid)
			{
				if (PanelGridResults == null)
				{
					return PanelGridResults.NumberOfGrids > 1;
				}

				return true;
			}

			return false;
		}
	}



	int IBsVsFindTarget3.IsNewUISupported => TargetAdapter.IsNewUISupported;


	public bool IsSplitterVisible
	{
		get { return TabbedEditorUiCtl.SplitViewContainer.IsSplitterVisible; }
		set { TabbedEditorUiCtl.SplitViewContainer.IsSplitterVisible = value; }
	}


	public bool IsTabVisibleCode => EditorTabCode.IsVisible;
	// public bool IsTabVisibleExecutionPlan => EditorTabExecutionPlan.IsVisible;
	public bool IsTabVisibleMessages => EditorTabMessage.IsVisible;
	public bool IsTabVisibleResultsGrid => EditorTabResults.IsVisible;
	public bool IsTabVisibleStatistics => EditorTabStatistics.IsVisible;
	public bool IsTabVisibleTextPlan => EditorTabTextPlan.IsVisible;
	public bool IsTabVisibleTextResults => EditorTabTextResults.IsVisible;


	/*
	private ExecutionPlanPanel PanelExecutionPlan
	{
		get
		{
			Panel obj = (EditorTabExecutionPlan.GetView() as ResultWindowPane).Window as Panel;
			ExecutionPlanPanel executionPlanPanel = null;

			foreach (Control control in obj.Controls)
			{
				executionPlanPanel = control as ExecutionPlanPanel;

				if (executionPlanPanel != null)
					return executionPlanPanel;
			}

			return executionPlanPanel;
		}
	}
	*/


	private GridResultsPanel PanelGridResults
	{
		get
		{
			Panel obj = (EditorTabResults.GetView() as ResultPane).Window as Panel;
			GridResultsPanel gridResultsPanel = null;

			foreach (Control control in obj.Controls)
			{
				gridResultsPanel = control as GridResultsPanel;

				if (gridResultsPanel != null)
					return gridResultsPanel;
			}

			return gridResultsPanel;
		}
	}


	protected VSTextEditorPanel PanelTextEditor
	{
		get
		{
			Panel obj = (EditorTabTextPlan.GetView() as ResultPane).Window as Panel;
			VSTextEditorPanel textEditorPanel = null;

			foreach (Control control in obj.Controls)
			{
				textEditorPanel = control as VSTextEditorPanel;

				if (textEditorPanel != null)
					return textEditorPanel;
			}

			return textEditorPanel;
		}
	}


	private VSTextEditorPanel PaneMessage
	{
		get
		{
			Panel obj = (EditorTabMessage.GetView() as ResultPane).Window as Panel;
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


	private VSTextEditorPanel PaneTextResults
	{
		get
		{
			Panel obj = (EditorTabTextResults.GetView() as ResultPane).Window as Panel;
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


	public bool SplittersVisible
	{
		set { TabbedEditorUiCtl.SplitViewContainer.SplittersVisible = value; }
	}


	public string TabTextMessages =>
		PaneMessage?.TextViewCtl.TextBuffer.Text ?? "";


	public string TabTextTextResults =>
		PaneTextResults?.TextViewCtl.TextBuffer.Text ?? "";
	

	private FindTargetAdapter TargetAdapter => _TargetAdapter;

	public ViewCommandFilter ViewFilter => _ViewFilter;


	public string WindowBaseName
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			IVsWindowFrame frame = Frame;
			if (frame == null)
				return null;

			string text;
			string moniker = GetDocumentMoniker(frame);

			try
			{
				text = Cmd.GetFileNameWithoutExtension(moniker);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex, $"Moniker: {moniker}.");
				throw;
			}

			___(frame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out object pHierarchy));
			if (pHierarchy is not IVsHierarchy vsHierarchy)
				return text;

			___(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out object pItemId));
			if ((int)pItemId < 0)
				return text;

			uint itemid = Convert.ToUInt32(pItemId, CultureInfo.InvariantCulture);

			___(vsHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out object pName));

			try
			{
				text = Cmd.GetFileNameWithoutExtension((string)pName);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex, $"File name: {(string)pName}.");
				throw;
			}

			return text;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - TabbedEditorPane
	// =========================================================================================================


	public override void ActivateNextTab()
	{
		if (IsGridCheckedAndMoreThanOneResultSet)
		{
			for (int i = 0; i < PanelGridResults.NumberOfGrids - 1; i++)
			{
				if (PanelGridResults.GridContainers[i].GridCtl.ContainsFocus)
				{
					ScrollGridIntoView(PanelGridResults.GridContainers[i + 1].GridCtl);
					return;
				}
			}
		}

		base.ActivateNextTab();

		if (IsGridCheckedAndMoreThanOneResultSet)
			ScrollGridIntoView(PanelGridResults.GridContainers[0].GridCtl);
	}



	public override void ActivatePreviousTab()
	{
		if (IsGridCheckedAndMoreThanOneResultSet)
		{
			for (int i = PanelGridResults.NumberOfGrids - 1; i > 0; i--)
			{
				if (PanelGridResults.GridContainers[i].GridCtl.ContainsFocus)
				{
					ScrollGridIntoView(PanelGridResults.GridContainers[i - 1].GridCtl);
					return;
				}
			}
		}

		base.ActivatePreviousTab();

		if (IsGridCheckedAndMoreThanOneResultSet)
			ScrollGridIntoView(PanelGridResults.GridContainers[PanelGridResults.NumberOfGrids - 1].GridCtl);
	}



	public void ActivateTab(AbstruseEditorTab tab)
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


	public void ActivateCodeTab() => ActivateTab(EditorTabCode);
	// public void ActivateExecutionPlanTab() => ActivateTab(EditorTabExecutionPlan);
	public void ActivateMessageTab() => ActivateTab(EditorTabMessage);
	public void ActivateResultsTab() => ActivateTab(EditorTabResults);
	public void ActivateStatisticsTab() => ActivateTab(EditorTabStatistics);
	public void ActivateTextPlanTab() => ActivateTab(EditorTabTextPlan);
	public void ActivateTextResultsTab() => ActivateTab(EditorTabTextResults);



	/// <summary>
	/// [Launch async]: Auto-execute query.
	/// </summary>
	private void AsyncAutoExecuteQuery()
	{
		// Evs.Trace(GetType(), nameof(AsyncAutoExecuteQuery), "ExecutionType: {0}.", executionType);

		if (!_AutoExecute)
			return;

		_AutoExecute = false;

		// Fire and forget

		Task.Factory.StartNew(
			async () =>
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				UpdateWindowCaption();
			},
			default, TaskCreationOptions.PreferFairness, TaskScheduler.Default).Forget();


		// ----------------------------------------------------------------------------------- //
		// ****** Execution Point (0) - TabbedEditorPane.AsyncAutoExecuteQuery() ******* //
		// ----------------------------------------------------------------------------------- //
		AsyncExecuteQuery(EnSqlExecutionType.QueryOnly);
	}



	/// <summary>
	/// [Launch async]: Execute query.
	/// </summary>
	public void AsyncExecuteQuery(EnSqlExecutionType executionType)
	{
		// Evs.Trace(GetType(), nameof(AsyncExecuteQuery));


		using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(Microsoft.VisualStudio.Utilities.DpiAwarenessContext.SystemAware))
		{
			TextSpanEx textSpanInfo = GetSelectedCodeEditorTextSpan();
			if (textSpanInfo.Text == null || textSpanInfo.Text.Length == 0)
			{
				textSpanInfo = GetAllCodeEditorTextSpan();
			}

			if (textSpanInfo != null && !string.IsNullOrEmpty(textSpanInfo.Text))
			{
				QueryManager qryMgr = QryMgr;

				// Evs.Trace(GetType(), "AsyncExecuteQuery", "calling QryMgr.AsyncExecute()");


				// ----------------------------------------------------------------------------------- //
				// ******* Execution Point (1) - TabbedEditorPane.AsyncExecuteQuery() ********** //
				// ----------------------------------------------------------------------------------- //
				qryMgr.AsyncExecute(textSpanInfo, executionType);

			}
		}
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
		IOleServiceProvider site = pvar as IOleServiceProvider;
		if (textView is not IObjectWithSite objectWithSite)
		{
			// Evs.Trace(typeof(AbstractSqlEditorTab), "ConfigureTextViewForAutonomousFind", "Couldn't cast textView to IObjectWithsite!");
			return;
		}

		objectWithSite.SetSite(site);
		ITextView wpfTextView = GetWpfTextView(textView);

		wpfTextView?.Options.SetOptionValue("Enable Autonomous Find", true);

	}



	protected override AbstruseEditorTab CreateEditorTab(AbstractTabbedEditorPane tabbedEditor, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
	{
		AbstruseEditorTab result = null;
		if (logicalView == VSConstants.LOGVIEWID_Designer)
		{
			return new EditorResultsTab(this, logicalView, editorLogicalView, editorTabType);
		}

		if (logicalView == VSConstants.LOGVIEWID_TextView)
		{
			return new EditorCodeTab(this, logicalView, editorLogicalView, editorTabType);
		}

		if (logicalView == new Guid(LibraryData.C_MessageTabGuid)
			|| logicalView == new Guid(LibraryData.C_TextResultsTabGuid)
			|| logicalView == new Guid(LibraryData.C_TextPlanTabGuid))
		{
			return new EditorMessageTab(this, logicalView, editorLogicalView, editorTabType);
		}

		if (logicalView == new Guid(LibraryData.C_StatisticsTabGuid))
		{
			return new EditorResultsTab(this, logicalView, editorLogicalView, editorTabType);
		}

		/*
		if (logicalView == new Guid(LibraryData.C_ExecutionPlanTabGuid))
		{
			return new EditorResultsTab(this, logicalView, editorLogicalView, editorTabType);
		}
		*/

		return result;
	}



	protected virtual ViewCommandFilter CreateViewFilter()
	{
		return new ViewCommandFilter(this);
	}



	public void CustomizeTabsForResultsSetting(bool isParseOnly)
	{
		EnsureTabs(false);
		AuxilliaryDocData auxDocData = AuxDocData;
		EnSqlOutputMode sqlExecutionMode = auxDocData.SqlOutputMode;
		AbstruseEditorTab sqlEditorResultsTab = EditorTabResults;
		_ = EditorTabMessage;
		AbstruseEditorTab sqlEditorTextResultsTab = EditorTabTextResults;
		AbstruseEditorTab sqlEditorStatisticsTab = EditorTabStatistics;
		// AbstractEditorTab sqlExecutionPlanTab = EditorTabExecutionPlan;
		AbstruseEditorTab sqlTextPlanTab = EditorTabTextPlan;

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



	public ResultsHandler EnsureDisplayResultsControl()
	{
		if (_ResultsControl == null && !ApcManager.SolutionClosing)
		{
			ResultPane resultsGridPanel = EditorTabResults.GetView() as ResultPane;
			ResultPane spatialPane = null;
			ResultPane messagePanel = EditorTabMessage.GetView() as ResultPane;
			ResultPane textResultsPanel = EditorTabTextResults.GetView() as ResultPane;
			ResultPane statisticsPanel = EditorTabStatistics.GetView() as ResultPane;
			// ResultWindowPane executionPlanPanel = EditorTabExecutionPlan.GetView() as ResultWindowPane;
			ResultPane textPlanPanel = EditorTabTextPlan.GetView() as ResultPane;
			_ResultsControl = new ResultsHandler(resultsGridPanel, messagePanel, textResultsPanel,
				statisticsPanel, /* executionPlanPanel,*/ textPlanPanel, spatialPane, this);
			_ResultsControl.SetSite(ApcManager.OleServiceProvider);
		}

		return _ResultsControl;
	}



	public override bool EnsureTabs(bool activateTextView)
	{
		if (TabbedEditorUiCtl == null)
			Diag.ThrowException(new ApplicationException("TabbedEditorUiCtl is null"));

		if (ApcManager.SolutionClosing)
			return TabbedEditorUiCtl.Tabs.Count > 0;

		AuxilliaryDocData auxDocData = AuxDocData;
		if (auxDocData == null)
			Diag.ThrowException(new ApplicationException("AuxDocData is null"));

		SplitViewContainer splitViewContainer = TabbedEditorUiCtl.SplitViewContainer;
		if (splitViewContainer == null)
			Diag.ThrowException(new ApplicationException("SplitViewContainer is null"));

		QueryManager qryMgr = auxDocData.QryMgr;
		if (qryMgr == null)
			Diag.ThrowException(new ApplicationException("QryMgr is null"));

		bool hasActualPlan = qryMgr.LiveSettings.ExecutionType == EnSqlExecutionType.QueryWithPlan;
		string btnTextPlanText = hasActualPlan
			? ControlsResources.ToolStripButton_ActualTextPlan_Button_Text
			: ControlsResources.ToolStripButton_TextPlan_Button_Text;


		if (TabbedEditorUiCtl.Tabs.Count > 0)
		{
			splitViewContainer.CustomizeSplitterBarButtonText(new Guid(LibraryData.C_TextPlanTabGuid),
				btnTextPlanText);

			return true;
		}


		if (!base.EnsureTabs(activateTextView))
			return false;


		EditorResultsTab resultsTab = EditorTabResults;

		if (resultsTab == null)
		{
			ApplicationException ex = new("GetSqlEditorResultsTab returned null");
			Diag.Dug(ex);
			throw ex;
		}

		resultsTab.Hide();

		TabbedEditorUiCtl.SuspendLayout();

		try
		{

			AbstruseEditorTab editorTab = CreateEditorTabWithButton(this, new Guid(LibraryData.C_TextResultsTabGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab);
			LoadDesignerPane(editorTab, asPrimary: false, showSplitter: false);
			editorTab.Hide();

			AbstruseEditorTab editorTab2 = CreateEditorTabWithButton(this, new Guid(LibraryData.C_StatisticsTabGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab2);
			LoadDesignerPane(editorTab2, asPrimary: false, showSplitter: false);
			editorTab2.Hide();

			/*
			AbstractEditorTab editorTab3 = CreateEditorTabWithButton(this, new Guid(LibraryData.C_ExecutionPlanTabGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab3);
			LoadDesignerPane(editorTab3, asPrimary: false, showSplitter: false);
			editorTab3.Hide();
			*/
			AbstruseEditorTab editorTab4 = CreateEditorTabWithButton(this, new Guid(LibraryData.C_TextPlanTabGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab4);
			LoadDesignerPane(editorTab4, asPrimary: false, showSplitter: false);
			editorTab4.Hide();

			AbstruseEditorTab editorTab5 = CreateEditorTabWithButton(this, new Guid(LibraryData.C_MessageTabGuid),
				VSConstants.LOGVIEWID_TextView, EnEditorTabType.BottomDesign);
			TabbedEditorUiCtl.Tabs.Add(editorTab5);
			LoadDesignerPane(editorTab5, asPrimary: false, showSplitter: false);
			editorTab5.Hide();

			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.C_MessageTabGuid),
				EnSplitterBarButtonDisplayStyle.Text, ControlsResources.ToolStripButton_Message_Button_Text,
				ControlsResources.ImgMessages);
			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.C_TextResultsTabGuid),
				EnSplitterBarButtonDisplayStyle.Text, ControlsResources.ToolStripButton_ResultsText_Button_Text,
				ControlsResources.ImgMessages);

			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.C_StatisticsTabGuid),
				EnSplitterBarButtonDisplayStyle.Text,
				ControlsResources.ToolStripButton_StatisticsSnapshot_Button_Text,
				ControlsResources.ImgStatistics);

			// splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.C_ExecutionPlanTabGuid),
			//	EnSplitterBarButtonDisplayStyle.Text, btnPlanText, ControlsResources.ImgExecutionPlan);
			splitViewContainer.CustomizeSplitterBarButton(new Guid(LibraryData.C_TextPlanTabGuid),
				EnSplitterBarButtonDisplayStyle.Text, btnTextPlanText, ControlsResources.ImgTextPlan);

			TabbedEditorUiCtl.SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(new Guid(LibraryData.C_TextResultsTabGuid), visible: false);
			// TabbedEditorUiCtl.SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(new Guid(LibraryData.C_ExecutionPlanTabGuid), visible: false);
			TabbedEditorUiCtl.SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(new Guid(LibraryData.C_TextPlanTabGuid), visible: false);

			IVsTextView codeEditorTextView = GetCodeEditorTextView();

			if (codeEditorTextView != null)
				AbstractViewCommandFilter.AddFilterToView(codeEditorTextView, _ViewFilter);

			EnsureDisplayResultsControl();

			_ProperyWindowManager = new PropertiesWindowManager(this);

			qryMgr.CurrentWorkingDirectoryPath = GetCurrentWorkingDirectory;

			EditorCodeTab editorCodeTab = EditorTabCode;

			ConfigureTextViewForAutonomousFind(editorCodeTab.CurrentFrame, GetCodeEditorTextView(editorCodeTab));
			ConfigureTextViewForAutonomousFind(EditorTabTextResults.CurrentFrame, _ResultsControl.TextResultsPaneTextView);
			ConfigureTextViewForAutonomousFind(EditorTabMessage.CurrentFrame, _ResultsControl.MessagesPaneTextView);

			if (_ResultsControl.TextPlanPaneTextView != null)
				ConfigureTextViewForAutonomousFind(EditorTabTextPlan.CurrentFrame, _ResultsControl.TextPlanPaneTextView);

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



	int IVsFindTarget.Find(string pszSearch, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out uint pResult)
	{
		return TargetAdapter.Find(pszSearch, grfOptions, fResetStartPoint, pHelper, out pResult);
	}



	private string GetAdditionalTooltipOrWindowCaption(bool toolTip)
	{
		string text = "";
		QueryManager qryMgr = QryMgr;

		if (qryMgr != null)
		{
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



	public TextSpanEx GetAllCodeEditorTextSpan()
	{
		IVsTextView codeEditorTextView = GetCodeEditorTextView();
		int anchorCol = 0;
		___(codeEditorTextView.GetBuffer(out var ppBuffer));
		___(ppBuffer.GetLastLineIndex(out var piLine, out var piIndex));
		return new TextSpanEx(0, anchorCol, piLine, piIndex, GetAllCodeEditorText(), codeEditorTextView);
	}



	int IVsFindTarget.GetCapabilities(bool[] pfImage, uint[] pgrfOptions)
	{
		return TargetAdapter.GetCapabilities(pfImage, pgrfOptions);
	}



	public IVsTextView GetCodeEditorTextView(object editorCodeTab = null)
	{
		IVsTextView ppView = null;

		EditorCodeTab sqlEditorCodeTab;

		if (editorCodeTab == null)
			sqlEditorCodeTab = EditorTabCode;
		else
			sqlEditorCodeTab = editorCodeTab as EditorCodeTab;


		if (sqlEditorCodeTab != null)
		{
			Diag.ThrowIfNotOnUIThread();

			IVsWindowFrame currentFrame = sqlEditorCodeTab.CurrentFrame;

			___(currentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar));
			___(((IVsCodeWindow)pvar).GetPrimaryView(out ppView));
		}

		return ppView;
	}



	public IWpfTextView GetCodeEditorWpfTextView()
	{
		return GetWpfTextView(GetCodeEditorTextView());
	}



	public string GetCodeText()
	{
		TextSpanEx textSpanInfo = GetSelectedCodeEditorTextSpan();
		if (textSpanInfo.Text == null || textSpanInfo.Text.Length == 0)
		{
			textSpanInfo = GetAllCodeEditorTextSpan();
		}

		if (textSpanInfo == null || string.IsNullOrEmpty(textSpanInfo.Text))
			return "";

		return textSpanInfo.Text;
	}



	int IVsFindTarget.GetCurrentSpan(TextSpan[] pts)
	{
		return TargetAdapter.GetCurrentSpan(pts);
	}



	private string GetCurrentWorkingDirectory()
	{
		string result;

		try
		{
			result = Cmd.GetDirectoryName(DocumentMoniker);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"DocumentMoniker: {DocumentMoniker}.");
			throw;
		}

		return result;
	}



	public T GetEditorTab<T>(Guid guidTab) where T : class
	{
		Guid rguidLogicalView = guidTab;
		AbstruseEditorTab tab = GetTab(ref rguidLogicalView);

		T result = null;

		if (tab != null)
			return tab as T;

		Diag.StackException($"Could not get editor tab type: {typeof(T)}  Guid:{rguidLogicalView}");
		return result;
	}



	int IVsFindTarget.GetFindState(out object ppunk)
	{
		return TargetAdapter.GetFindState(out ppunk);
	}



	public override string GetHelpKeywordForCodeWindowTextView()
	{
		return C_HelpString;
	}



	int IVsFindTarget.GetMatchRect(RECT[] prc)
	{
		return TargetAdapter.GetMatchRect(prc);
	}



	int IVsFindTarget.GetProperty(uint propid, out object pvar)
	{
		return TargetAdapter.GetProperty(propid, out pvar);
	}



	int IVsFindTarget.GetSearchImage(uint grfOptions, IVsTextSpanSet[] ppSpans, out IVsTextImage ppTextImage)
	{
		return TargetAdapter.GetSearchImage(grfOptions, ppSpans, out ppTextImage);
	}



	public string GetSelectedCodeEditorText()
	{
		IVsTextView codeEditorTextView = GetCodeEditorTextView();
		___(codeEditorTextView.GetSelectedText(out string pbstrText));
		return pbstrText;
	}



	public TextSpanEx GetSelectedCodeEditorTextSpan()
	{
		IVsTextView codeEditorTextView = GetCodeEditorTextView();
		___(codeEditorTextView.GetSelection(out var piAnchorLine, out var piAnchorCol, out var piEndLine, out var piEndCol));
		return new TextSpanEx(piAnchorLine, piAnchorCol, piEndLine, piEndCol, GetSelectedCodeEditorText(), codeEditorTextView);
	}



	private static IWpfTextView GetWpfTextView(IVsTextView textView)
	{
		if (ApcManager.GetService<SComponentModel>() is not IComponentModel componentModelSvc)
			return null;

		if (componentModelSvc.GetService<IVsEditorAdaptersFactoryService>() is not IVsEditorAdaptersFactoryService factorySvc)
			return null;

		return factorySvc.GetWpfTextView(textView);
	}



	private bool IsTabButtonChecked(AbstruseEditorTab tab)
	{
		if (tab != null)
		{
			return TabbedEditorUiCtl.SplitViewContainer.GetButton(tab).Checked;
		}

		return false;
	}



	int IVsFindTarget.MarkSpan(TextSpan[] pts)
	{
		return TargetAdapter.MarkSpan(pts);
	}



	int IVsFindTarget.NavigateTo(TextSpan[] pts)
	{
		return TargetAdapter.NavigateTo(pts);
	}



	public int NavigateTo2(IVsTextSpanSet pSpans, TextSelMode iSelMode)
	{
		return TargetAdapter.NavigateTo2(pSpans, iSelMode);
	}



	int IVsFindTarget.NotifyFindTarget(uint notification)
	{
		return TargetAdapter.NotifyFindTarget(notification);
	}



	int IBsVsFindTarget3.NotifyShowingNewUI()
	{
		return TargetAdapter.NotifyShowingNewUI();
	}



	int IVsFindTarget.Replace(string pszSearch, string pszReplace, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out int pfReplaced)
	{
		return TargetAdapter.Replace(pszSearch, pszReplace, grfOptions, fResetStartPoint, pHelper, out pfReplaced);
	}



	protected override int SaveFiles(ref uint pgrfSaveOptions)
	{
		// Evs.Trace(GetType(), nameof(SaveFiles));

		if (pgrfSaveOptions == (uint)__FRAMECLOSE.FRAMECLOSE_PromptSave)
		{
			if (AuxDocData.SuppressSavePrompt)
				pgrfSaveOptions = (uint)__FRAMECLOSE.FRAMECLOSE_NoSave;
		}

		return base.SaveFiles(ref pgrfSaveOptions);
	}



	private void ScrollGridIntoView(IBsGridControl2 gridCtl)
	{
		gridCtl.Focus();

		// The original MS SqlEditor only performed a focus. This is useless when
		// focusing on a hidden grid in a multi-resultset query, so we're going to
		// ensure the grid is autoscrolled into view.

		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			Control ctl = (Control)gridCtl;
			ScrollableControl parent = (ScrollableControl)ctl.Parent.Parent;

			// Back off 36 pixels so that the bottom of previous grid is visible and
			// the user is given a visual context in a multi-grid result set.
			// Subtracting 'offset' could result in a negative scrollpos, which will
			// default to zero.
			// Also, we won't back off more than a third of the client area height.

			int offset = 36;
			int height = parent.ClientRectangle.Height;

			if (offset * 3 > height)
				offset = height / 3;

			// The parent of 'gridCtl' is actually MultiControlPanel.SmartPanelWithLayout, so the
			// AutoScrollPosition of 'parent' (MultiControlPanel) must be added in.

			offset = ctl.Top + parent.AutoScrollPosition.Y - offset;

			// The simplest way to scroll the grid control into view is by using a dummy control
			// placed where we want to auto scroll to.
			// Then we can use ScrollControlIntoView() to do all the hard work for us.

			using (Control dummy = new Control() { Parent = parent, Height = height, Top = offset })
			{
				parent.ScrollControlIntoView(dummy);
			}


			if (gridCtl.ScrollMgr.RowCount > 0)
			{
				long row = gridCtl.SelectionMgr.CurrentRow;
				if (row == -1)
					row = 0;
				gridCtl.ScrollMgr.EnsureRowIsVisible(row, false);
			}
		}

		return;

	}



	public void SetCodeEditorSelection(int start, int length)
	{
		IWpfTextView codeEditorWpfTextView = GetCodeEditorWpfTextView();
		ITextSnapshot currentSnapshot = codeEditorWpfTextView.TextBuffer.CurrentSnapshot;
		SnapshotSpan selectionSpan = new SnapshotSpan(currentSnapshot, new Span(start, length));
		codeEditorWpfTextView.Selection.Select(selectionSpan, isReversed: false);
	}




	int IVsFindTarget.SetFindState(object pUnk)
	{
		return TargetAdapter.SetFindState(pUnk);
	}



	private void SetupUI()
	{
		try
		{
			TabbedEditorUiCtl.SuspendLayout();

			AuxilliaryDocData auxDocData;

			try
			{
				auxDocData = AuxDocData
					?? throw new NullReferenceException("(AuxilliaryDocData)auxDocData");

				// if (auxDocData.StrategyFactory == null)
				//	throw new NullReferenceException("auxDocData.StrategyFactory");
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			Guid clsidCmdSet = CommandProperties.ClsidCommandSet;
			uint menuId;


			switch (auxDocData.Mode)
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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the tabs' text.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override async Task<bool> UpdateTabsButtonTextAsync(ExecutionCompletedEventArgs args)
	{
		if (!await base.UpdateTabsButtonTextAsync(args))
			return true;

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

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


		Guid clsidMessagesTab = new Guid(LibraryData.C_MessageTabGuid);
		Guid clsidStatisticsTab = new Guid(LibraryData.C_StatisticsTabGuid);
		Guid clsidResultsTab = args.OutputMode == EnSqlOutputMode.ToGrid
			? VSConstants.LOGVIEWID_Designer : new Guid(LibraryData.C_TextResultsTabGuid);


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

		result = resultsUpdated && statsUpdated && messagesUpdated;

		args.Result &= result;

		return result;
	}



	private void UpdateToolTip()
	{
		Diag.ThrowIfNotOnUIThread();

		IVsWindowFrame frame = Frame;
		if (frame == null)
			return;

		___(frame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out object pHierarchy));
		if (pHierarchy is not IVsHierarchy vsHierarchy)
			return;

		___(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out object pItemId));
		if ((int)pItemId < 0)
			return;
		uint itemid = Convert.ToUInt32(pItemId, CultureInfo.InvariantCulture);

		// vsHierarchy.SetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ShowOnlyItemCaption, true);

		AuxilliaryDocData auxDocData = null; // AuxDocData;

		string text;

		// There are no Database projects for the FB-SQL port so this will never execute
		if (auxDocData != null && (auxDocData.Mode == EnEditorMode.CustomOnline
			|| auxDocData.Mode == EnEditorMode.CustomProject))
		{
			text = GetAdditionalTooltipOrWindowCaption(toolTip: true);
			string text2 = Environment.NewLine;
			if (string.IsNullOrEmpty(text))
			{
				text2 = "";
			}

			if (auxDocData.Mode == EnEditorMode.CustomProject)
			{
				// Never happen - projects not supported in FB-SQL port.
				text = GetDocumentMoniker(frame) + text2 + text;
			}
			else if (auxDocData.Mode == EnEditorMode.CustomOnline)
			{
				// Never happen - online not supported ATM
				___(vsHierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out object pName));
				text = (string)pName + text2 + text;
			}
		}
		else
		{
			text = GetDocumentMoniker(frame);
		}

		vsHierarchy.SetProperty(itemid, (int)__VSHPROPID4.VSHPROPID_DescriptiveName, text);
	}



	private void UpdateWindowCaption()
	{
		Diag.ThrowIfNotOnUIThread();

		IVsWindowFrame frame = Frame;
		if (frame == null)
			return;

		___(frame.SetProperty((int)__VSFPROPID.VSFPROPID_EditorCaption, ""));
		string text = GetAdditionalTooltipOrWindowCaption(toolTip: false);
		string text2 = " - ";
		if (string.IsNullOrEmpty(text))
		{
			text2 = "";
			text = " ";
		}

		if (PersistentSettings.EditorStatusTabTextIncludeFileName)
		{
			text = "%2" + text2 + text;
		}

		string moniker = GetDocumentMoniker(frame);

		if (moniker != null && moniker.StartsWith(Path.GetTempPath(), StringComparison.InvariantCultureIgnoreCase))
		{
			text = Resources.QueryCaptionGlyphFormat.FmtRes(SystemData.C_SessionTitleGlyph, text);
		}

		___(frame.SetProperty((int)__VSFPROPID.VSFPROPID_OwnerCaption, text));
	}



	#endregion Methods





	// =========================================================================================================
	#region Event Handling - TabbedEditorPane
	// =========================================================================================================


	public override int OnExec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		IBsCommand command = null;

		if (pguidCmdGroup == CommandProperties.ClsidCommandSet)
		{
			EnCommandSet cmd = (EnCommandSet)nCmdID;
			switch (cmd)
			{
				case EnCommandSet.CmdIdResultsAsText:
					command = new CommandResultsAsText(this);
					break;
				case EnCommandSet.CmdIdResultsAsGrid:
					command = new CommandResultsAsGrid(this);
					break;
				case EnCommandSet.CmdIdResultsAsFile:
					command = new CommandResultsAsFile(this);
					break;
				case EnCommandSet.CmdIdToggleResultsPane:
					command = new CommandToggleResultsPane(this);
					break;
			}

		}

		if (command != null)
			return command.Exec(nCmdexecopt, pvaIn, pvaOut);


		// Evs.Trace(GetType(), nameof(OnExec), "pguidCmdGroup: {0}, nCmdId: {1}.", pguidCmdGroup, nCmdID);

		/* No extended command handlers
		 * 
		AuxilliaryDocData auxDocData = AuxDocData;

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
			IBsCommand command = null;

			if (pguidCmdGroup == CommandProperties.ClsidCommandSet)
			{
				EnCommandSet cmd = (EnCommandSet)cmdID;

				switch (cmd)
				{
					case EnCommandSet.CmdIdResultsAsText:
						command = new CommandResultsAsText(this);
						break;
					case EnCommandSet.CmdIdResultsAsGrid:
						command = new CommandResultsAsGrid(this);
						break;
					case EnCommandSet.CmdIdResultsAsFile:
						command = new CommandResultsAsFile(this);
						break;
					case EnCommandSet.CmdIdToggleResultsPane:
						command = new CommandToggleResultsPane(this);
						break;
				}
			}

			if (command != null)
				return command.QueryStatus(ref prgCmds[i], pCmdText);
		}

		/* No extended command handlers
		 * 
		AuxilliaryDocData auxDocData = AuxDocData;

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



	private void OnTabActivated(object sender, EventArgs args)
	{
		// Evs.Trace(GetType(), "TabActivatedHandler", "SenderType: {0}, View: {1}.", sender.GetType().Name, (sender as AbstractEditorTab).GetView());
		((sender as AbstruseEditorTab).GetView() as ResultPane)?.SetFocus();
	}



	public void OnUpdateTooltipAndWindowCaption(object sender, QueryStateChangedEventArgs args)
	{
		_ = Task.Run(() => OnUpdateTooltipAndWindowCaptionAsync(sender, args));

	}



	public async Task OnUpdateTooltipAndWindowCaptionAsync(object sender, QueryStateChangedEventArgs args)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		// if (args.Change == QueryManager.EnStatusType.Connection || args.Change == QueryManager.EnStatusType.Connected)
		// {
		UpdateToolTip();
		// }


		UpdateWindowCaption();
	}



	protected override void RaiseShow(__FRAMESHOW fShow)
	{
		base.RaiseShow(fShow);

		if (_IsFirstTimeToShow && fShow == __FRAMESHOW.FRAMESHOW_WinShown)
		{
			if (IsClone)
			{
				_IsFirstTimeToShow = false;
				Task.Run(CloseCloneAsync).Forget();
				return;
			}

			RctManager.EnsureLoaded();

			SetupUI();
			_IsFirstTimeToShow = false;

			IVsWindowFrame frame = Frame;
			AuxilliaryDocData auxDocData = AuxDocData;
			uint cookie = 0;
			uint auxDocDataCookie = auxDocData.DocCookie;
			string moniker = GetDocumentMoniker(frame);
			string auxDocDataMoniker = auxDocData.InternalMoniker;

			// Evs.Trace(GetType(), nameof(OnShow),
			//	"PrimaryCookie: {0}, AuxDocData.DocCookie: {1}, DocumentMoniker: {2}, AuxDocData.InternalMoniker: {3}.",
			//	GetPrimaryCookie(frame), auxDocDataCookie, moniker, auxDocDataMoniker);


			if (auxDocDataCookie == 0)
			{
				cookie = GetPrimaryCookie(frame);

				if (cookie == 0)
					Diag.ThrowException(new ApplicationException($"Failed to get Primary DocCookie for {moniker}."));

				auxDocData.DocCookie = cookie;
			}

			if (string.IsNullOrEmpty(moniker))
			{
				Diag.ThrowException(new ApplicationException($"DocumentMoniker is null for document cookie {(cookie == 0 ? GetPrimaryCookie(frame) : cookie)}."));
			}

			if (string.IsNullOrEmpty(auxDocDataMoniker) || !moniker.Equals(auxDocDataMoniker))
			{
				auxDocData.InternalMoniker = moniker;
			}

			if (_AutoExecute)
			{
				AsyncAutoExecuteQuery();
			}
			else
			{
				// Fire and forget

				Task.Factory.StartNew(
					async () =>
					{
						await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
						UpdateWindowCaption();
					},
					default, TaskCreationOptions.PreferFairness, TaskScheduler.Default).Forget();
			}
		}
		else if (fShow == __FRAMESHOW.FRAMESHOW_TabDeactivated &&
			ReferenceEquals(this, ExtensionInstance.CurrentTabbedEditor))
		{
			ExtensionInstance.CurrentTabbedEditor = null;
			AuxilliaryDocData auxDocdata = AuxDocData;

			if (auxDocdata != null)
				auxDocdata.IntellisenseActive = auxDocdata.IntellisenseEnabled != null && auxDocdata.IntellisenseEnabled.Value;
		}

	}


	#endregion Event Handling

}
