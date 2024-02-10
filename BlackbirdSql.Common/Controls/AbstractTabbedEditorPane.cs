// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabbedEditorPane
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using DpiAwareness = Microsoft.VisualStudio.Utilities.DpiAwareness;
using DpiAwarenessContext = Microsoft.VisualStudio.Utilities.DpiAwarenessContext;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;


namespace BlackbirdSql.Common.Controls;

public abstract class AbstractTabbedEditorPane : WindowPane, IVsDesignerInfo, IOleCommandTarget, IVsWindowFrameNotify3, IVsMultiViewDocumentView, IVsHasRelatedSaveItems, IVsDocumentLockHolder, IVsBroadcastMessageEvents, IBDesignerDocumentService, IBTabbedEditorService, IVsDocOutlineProvider, IVsDocOutlineProvider2, IVsToolboxActiveUserHook, IVsToolboxUser, IVsToolboxPageChooser, IVsDefaultToolboxTabState, IVsCodeWindow, IVsExtensibleObject
{
	private static TabbedEditorToolbarHandlerManager _ToolbarManager;

	private Package _Package;
	private TabbedEditorUI _TabbedEditorUI;
	private Guid _RequestedView;
	private IDisposable _DisposableWaitCursor;
	// private uint _selectionMonitorCookie;
	// private IVsMonitorSelection _SelectionMonitor;
	private bool _IsAppActivated = true;
	private IBTextEditor _TextEditor;
	private bool _IsLoading;
	private bool _IsClosing;
	private bool _IsInUpdateCmdUIContext;
	private uint _LockHolderCookie;
	// private Guid _toolbarGuid;
	// private readonly uint _toolbarID;
	private IVsTextLines _DocData;
	private IList<uint> _OverrideSaveDocCookieList;
	// private bool _FirstTimeShowEventHandled;
	private bool _IsHelpInitialized;



	public static TabbedEditorToolbarHandlerManager ToolbarManager =>
		_ToolbarManager ??= new TabbedEditorToolbarHandlerManager();


	public bool IsDisposed { get; private set; }

	public IDisposable DisposableWaitCursor
	{
		get
		{
			return _DisposableWaitCursor;
		}
		set
		{
			if (_DisposableWaitCursor != null)
			{
				_DisposableWaitCursor.Dispose();
				_DisposableWaitCursor = value;

				Controller.DisposableWaitCursor = value;
			}
			else if (Controller.DisposableWaitCursor == null)
			{
				_DisposableWaitCursor = value;
				Controller.DisposableWaitCursor = value;
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				value?.Dispose();
			}
		}
	}


	public virtual IVsTextLines DocData => _DocData;

	public bool IsToolboxInitialized => Controller.Instance.IsToolboxInitialized;

	protected IVsMonitorSelection SelectionMonitor => Controller.SelectionMonitor;


	IBTextEditor IBTabbedEditorService.TextEditor
	{
		get
		{
			return _TextEditor;
		}
		set
		{
			_TextEditor = value;
		}
	}

	public override IWin32Window Window => _TabbedEditorUI;

	public IVsWindowFrame Frame => GetService(typeof(SVsWindowFrame)) as IVsWindowFrame;

	protected virtual Guid PrimaryViewGuid => VSConstants.LOGVIEWID_Designer;

	AbstractEditorTab IBTabbedEditorService.ActiveTab => _TabbedEditorUI.TopEditorTab;

	IVsWindowFrame IBTabbedEditorService.TabFrame => GetService(typeof(IVsWindowFrame)) as IVsWindowFrame;

	public TabbedEditorUI TabbedEditorControl => _TabbedEditorUI;

	Guid IBTabbedEditorService.InitialLogicalView => _RequestedView;

	private IVsCodeWindow XamlCodeWindow
	{
		get
		{
			IVsCodeWindow vsCodeWindow = null;
			EnsureTabs();
			AbstractEditorTab editorTab = null;
			foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
			{
				if (tab.EditorTabType == EnEditorTabType.BottomXaml || tab.EditorTabType == EnEditorTabType.TopXaml)
				{
					editorTab = tab;
					break;
				}
			}
			if (editorTab != null && editorTab.IsVisible)
			{
				vsCodeWindow = editorTab.GetView() as IVsCodeWindow;
			}
			if (vsCodeWindow == this)
			{
				vsCodeWindow = null;
			}
			return vsCodeWindow;
		}
	}

	public AbstractTabbedEditorPane(System.IServiceProvider provider, Package package, object docData, Guid toolbarGuid, uint toolbarID)
		: base(provider)
	{
		Diag.ThrowIfNotOnUIThread();

		_Package = package;
		_ = _Package; // Suppression
		_DocData = docData as IVsTextLines;
		_RequestedView = VSConstants.LOGVIEWID_Designer;
		// _toolbarGuid = toolbarGuid;
		// _toolbarID = toolbarID;
		using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			_TabbedEditorUI = CreateTabbedEditorUI(toolbarGuid, toolbarID);
			_TabbedEditorUI.CreateControl();
		}
		// SelectionMonitor?.AdviseSelectionEvents(this, out _selectionMonitorCookie);
	}

	protected override void Initialize()
	{

		base.Initialize();

		Controller.Instance.OnElementValueChangedEvent += OnElementValueChanged;

		if (GetService(typeof(SVsTrackSelectionEx)) is IVsTrackSelectionEx vsTrackSelectionEx)
		{
			vsTrackSelectionEx.OnElementValueChange((uint)VSConstants.VSSELELEMID.SEID_PropertyBrowserSID, 1, null);
			vsTrackSelectionEx.OnElementValueChange((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, 1, null);
		}
		_ = GetService(typeof(SVsWindowFrame)) is IVsWindowFrame;
	}

	protected override void OnCreate()
	{
		(GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable).RegisterDocumentLockHolder(0u, GetPrimaryDocCookie(), this, out _LockHolderCookie);
	}

	bool IBTabbedEditorService.IsTabVisible(Guid logicalView)
	{
		foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
		{
			if (tab.LogicalView == logicalView)
			{
				return tab.IsOnScreen;
			}
		}
		return false;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				if (_LockHolderCookie != 0)
				{
					(GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable).UnregisterDocumentLockHolder(_LockHolderCookie);
					_LockHolderCookie = 0u;
				}

				Controller.Instance.OnElementValueChangedEvent -= OnElementValueChanged;
				/*
				if (_selectionMonitorCookie != 0 && _SelectionMonitor != null)
				{
					_SelectionMonitor.UnadviseSelectionEvents(_selectionMonitorCookie);
				}
				_selectionMonitorCookie = 0u;
				_SelectionMonitor = null;
				*/

				if (_TabbedEditorUI != null)
				{
					_TabbedEditorUI.Dispose();
					_TabbedEditorUI = null;
				}
				_DocData = null;
				_Package = null;
				_TextEditor = null;
				IsDisposed = true;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	protected override void OnClose()
	{
		// Tracer.Trace(GetType(), "OnClose()");

		_IsClosing = true;

		base.OnClose();
	}

	private void EnsureTabs()
	{
		EnsureTabs(activateTextView: false);
	}

	protected AbstractEditorTab CreateEditorTabWithButton(AbstractTabbedEditorPane editorPane, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
	{
		switch (editorTabType)
		{
			case EnEditorTabType.TopDesign:
			case EnEditorTabType.BottomDesign:
				_TabbedEditorUI.SplitViewContainer.SplitterBar.EnsureButtonInDesignPane(logicalView);
				break;
			case EnEditorTabType.TopXaml:
			case EnEditorTabType.BottomXaml:
				_TabbedEditorUI.SplitViewContainer.SplitterBar.EnsureButtonInXamlPane(logicalView);
				break;
		}
		return CreateEditorTab(editorPane, logicalView, editorLogicalView, editorTabType);
	}

	protected abstract AbstractEditorTab CreateEditorTab(AbstractTabbedEditorPane editorPane, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType);

	protected virtual TabbedEditorUI CreateTabbedEditorUI(Guid toolbarGuid, uint toolbarId)
	{
		return new TabbedEditorUI(this, toolbarGuid, toolbarId);
	}

	public virtual string GetHelpKeywordForCodeWindowTextView()
	{
		return string.Empty;
	}

	public virtual bool EnsureTabs(bool activateTextView)
	{
		if (_TabbedEditorUI == null || _TabbedEditorUI.Tabs.Count > 0 || _IsLoading || _IsClosing)
		{
			return false;
		}

		try
		{
			_TabbedEditorUI.DesignerMessage = "";
			_IsLoading = true;
			GetService(typeof(SVsWindowFrame));
			IVsUIShell vsUIShell = GetService(typeof(IVsUIShell)) as IVsUIShell;
			vsUIShell?.SetWaitCursor();
			Guid guid = PrimaryViewGuid;
			bool showSplitter = true;

			bool topIsTextView;
			if (!activateTextView && !(_RequestedView == VSConstants.LOGVIEWID_TextView))
			{
				topIsTextView = (_RequestedView == VSConstants.LOGVIEWID_Debugging);
			}
			else
			{
				topIsTextView = true;
			}

			/* Never happens
			if (topIsTextView && !showSplitter && guid != VSConstants.LOGVIEWID_TextView)
			{
				guid = VSConstants.LOGVIEWID_TextView;
			}
			*/

			_TabbedEditorUI.LoadWindowState("");
			_TabbedEditorUI.Update();
			EnEditorTabType editorTabType;
			EnEditorTabType editorTabType2;
			bool isPrimary;
			if (guid.Equals(VSConstants.LOGVIEWID_TextView))
			{
				editorTabType = EnEditorTabType.BottomDesign;
				editorTabType2 = EnEditorTabType.TopXaml;
				isPrimary = true;
			}
			else
			{
				editorTabType = EnEditorTabType.TopDesign;
				editorTabType2 = EnEditorTabType.BottomXaml;
				isPrimary = false;
			}
			AbstractEditorTab editorTab = CreateEditorTabWithButton(this, VSConstants.LOGVIEWID_Designer, VSConstants.LOGVIEWID_Primary, editorTabType);
			AbstractEditorTab editorTab2 = CreateEditorTabWithButton(this, VSConstants.LOGVIEWID_TextView, VSConstants.LOGVIEWID_Primary, editorTabType2);
			_TabbedEditorUI.Tabs.Add(editorTab);
			_TabbedEditorUI.Tabs.Add(editorTab2);
			vsUIShell?.SetWaitCursor();
			LoadXamlPane(editorTab2, isPrimary, showSplitter);
			LoadDesignerPane(editorTab, !isPrimary, showSplitter);
			if (topIsTextView | isPrimary)
			{
				editorTab2.Activate(setFocus: false);
				editorTab.UpdateActive(isActive: false);
			}
			else
			{
				editorTab.Activate(setFocus: false);
				editorTab2.UpdateActive(isActive: false);
			}
			TrackTabSwitches(track: true);
			if (IsToolboxInitialized && GetService(typeof(SVsToolbox)) is IVsToolbox vsToolbox)
			{
				vsToolbox.SelectItem(null);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			_IsLoading = false;
			if (_TabbedEditorUI != null)
			{
				_TabbedEditorUI.DesignerMessage = null;
			}
		}
		return true;
	}

	public void LoadXamlPane(AbstractEditorTab xamlTab, bool asPrimary, bool showSplitter)
	{
		if (asPrimary)
		{
			if (_TabbedEditorUI.TopEditorTab == null)
			{
				_TabbedEditorUI.TopEditorTab = xamlTab;
			}
			else
			{
				xamlTab.EnsureFrameCreated();
			}
		}
		else if (showSplitter)
		{
			if (_TabbedEditorUI.BottomEditorTab == null)
			{
				_TabbedEditorUI.BottomEditorTab = xamlTab;
			}
			else
			{
				xamlTab.EnsureFrameCreated();
			}
		}
		xamlTab.CreateTextEditor();
	}

	public void LoadDesignerPane(AbstractEditorTab designerTab, bool asPrimary, bool showSplitter)
	{
		if (asPrimary)
		{
			if (_TabbedEditorUI.TopEditorTab == null)
			{
				_TabbedEditorUI.TopEditorTab = designerTab;
			}
			else
			{
				designerTab.EnsureFrameCreated();
			}
		}
		else if (_TabbedEditorUI.BottomEditorTab == null)
		{
			_TabbedEditorUI.BottomEditorTab = designerTab;
		}
		else
		{
			designerTab.EnsureFrameCreated();
		}
	}

	private static Guid GetTabLogicalView(Guid logicalView)
	{
		if (logicalView.Equals(VSConstants.LOGVIEWID_Any) || logicalView.Equals(VSConstants.LOGVIEWID_Designer) || logicalView.Equals(VSConstants.LOGVIEWID_Primary))
		{
			return VSConstants.LOGVIEWID_Designer;
		}
		if (logicalView.Equals(VSConstants.LOGVIEWID_Debugging))
		{
			return VSConstants.LOGVIEWID_TextView;
		}
		return logicalView;
	}

	public virtual void ActivateNextTab()
	{
		_TabbedEditorUI.SplitViewContainer.CycleToNextButton();
	}

	public virtual void ActivatePreviousTab()
	{
		_TabbedEditorUI.SplitViewContainer.CycleToPreviousButton();
	}

	protected override object GetService(Type serviceType)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceType == typeof(IOleCommandTarget))
		{
			return this;
		}
		if (serviceType == typeof(IBTabbedEditorService))
		{
			return this;
		}
		return base.GetService(serviceType);
	}

	private void OnSaveOptions(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnSaveOptions()");

		if (_TabbedEditorUI != null)
			_ = _TabbedEditorUI.TopEditorTab;
	}

	protected void OnQueryScriptExecutionCompleted(object sender, ScriptExecutionCompletedEventArgs args)
	{
		DisposableWaitCursor = null;
	}



	private void OnTabActivated(object sender, EventArgs e)
	{
		_RequestedView = Guid.Empty;
		if (GetService(typeof(SVsWindowFrame)) is not IVsWindowFrame vsWindowFrame)
		{
			return;
		}
		Guid rguid = Guid.Empty;
		if (sender is AbstractEditorTab editorTab)
		{
			if (editorTab.EditorTabType != EnEditorTabType.TopXaml)
			{
				_ = editorTab.EditorTabType;
			}
			rguid = editorTab.CommandUIGuid;
		}
		vsWindowFrame.SetGuidProperty((int)__VSFPROPID.VSFPROPID_CmdUIGuid, ref rguid);
	}

	private void OnToolboxItemPicked(object sender, ToolboxEventArgs e)
	{
		if (_TabbedEditorUI != null && _TabbedEditorUI.IsSplitterVisible)
		{
			Guid rguidLogicalView = VSConstants.LOGVIEWID_Designer;
			IVsToolboxUser tab = GetTab(ref rguidLogicalView);
			if (tab != null)
			{
				e.HResult = tab.ItemPicked(e.Data);
				e.Handled = true;
			}
		}
	}

	protected virtual int SaveFiles(ref uint saveFlags)
	{
		// Tracer.Trace(GetType(), "SaveFiles()");

		if (VsShellUtilities.IsInAutomationFunction(this))
			saveFlags = (uint)__FRAMECLOSE.FRAMECLOSE_NoSave;

		int hresult = 0;

		if (saveFlags != (uint)__FRAMECLOSE.FRAMECLOSE_NoSave)
		{
			if ((IVsWindowFrame)GetService(typeof(SVsWindowFrame)) == null)
			{
				return 0;
			}

			uint saveOpts = (__FRAMECLOSE)saveFlags switch
			{
				__FRAMECLOSE.FRAMECLOSE_PromptSave
					=> (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_PromptSave,

				__FRAMECLOSE.FRAMECLOSE_SaveIfDirty
					=> (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_SaveIfDirty,

				_ => (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_DocClose,
			};

			uint primaryDocCookie = GetPrimaryDocCookie();

			hresult = (GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable).SaveDocuments(saveOpts, null, uint.MaxValue, primaryDocCookie);

			if (Native.Succeeded(hresult))
				saveFlags = (uint)__FRAMECLOSE.FRAMECLOSE_NoSave;
		}

		return hresult;
	}

	public IEnumerable<uint> GetEditableDocuments()
	{
		return GetEditableDocumentsForTab();
	}

	protected virtual IEnumerable<uint> GetEditableDocumentsForTab()
	{
		uint primaryDocCookie = GetPrimaryDocCookie();
		if (primaryDocCookie != 0)
		{
			yield return primaryDocCookie;
		}
	}

	private static uint GetFrameDocument(IVsWindowFrame frame)
	{
		Native.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out var pvar));
		return (uint)(int)pvar;
	}

	public uint GetPrimaryDocCookie()
	{
		if (GetService(typeof(SVsWindowFrame)) is not IVsWindowFrame frame)
		{
			return 0u;
		}
		return GetFrameDocument(frame);
	}

	private void TrackTabSwitches(bool track)
	{
		if (track)
		{
			_TabbedEditorUI.TabActivatedEvent += OnTabActivated;
		}
		else
		{
			_TabbedEditorUI.TabActivatedEvent -= OnTabActivated;
		}
	}

	private void UpdateCmdUIContext()
	{
		_IsInUpdateCmdUIContext = true;
		try
		{
			if (_TabbedEditorUI == null)
			{
				return;
			}
			AbstractEditorTab activeTab = _TabbedEditorUI.ActiveTab;
			if (activeTab == null)
			{
				return;
			}
			Guid rguidCmdUI = activeTab.CommandUIGuid;
			IVsMonitorSelection selectionMonitor = SelectionMonitor;
			if (selectionMonitor != null)
			{
				object service = GetService(typeof(SVsWindowFrame));
				selectionMonitor.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_WindowFrame, out var pvarValue);
				if (pvarValue == service)
				{
					selectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out uint pdwCmdUICookie);
					selectionMonitor.SetCmdUIContext(pdwCmdUICookie, 1);
					activeTab.Activate();
				}
			}
		}
		finally
		{
			_IsInUpdateCmdUIContext = false;
		}
	}

	public AbstractEditorTab GetTab(ref Guid rguidLogicalView)
	{
		Guid tabLogicalView = GetTabLogicalView(rguidLogicalView);
		if (_TabbedEditorUI.Tabs.Count > 0)
		{
			foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
			{
				if (tab.LogicalView.Equals(tabLogicalView))
				{
					return tab;
				}
			}
		}
		return null;
	}

	private T ActiveTab<T>() where T : class
	{
		T val = null;
		if (_TabbedEditorUI != null)
		{
			AbstractEditorTab activeTab = _TabbedEditorUI.ActiveTab;
			if (activeTab != null)
			{
				val = activeTab as T;
				val ??= activeTab.GetView() as T;
				if (val == this)
				{
					val = null;
				}
			}
		}
		return val;
	}

	private int ActivateView(ref Guid rguidLogicalView, EnTabViewMode mode)
	{
		Guid g = (_RequestedView = GetTabLogicalView(rguidLogicalView));
		if (_TabbedEditorUI.Tabs.Count > 0)
		{
			foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
			{
				if (tab.LogicalView.Equals(g))
				{
					TrackTabSwitches(track: false);
					_TabbedEditorUI.ActivateTab(tab, mode);
					TrackTabSwitches(track: true);
					break;
				}
			}
		}
		return 0;
	}

	void IBTabbedEditorService.Activate(Guid logicalView, EnTabViewMode mode)
	{
		ActivateView(ref logicalView, mode);
	}

	int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		int num = HandleExec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		if (num == 0)
		{
			return num;
		}
		GuidId guidId = new GuidId(pguidCmdGroup, nCmdID);
		if (ToolbarManager.TryGetCommandHandler(GetType(), guidId, out IBTabbedEditorToolbarCommandHandler commandHandler))
		{
			return commandHandler.HandleExec(this, nCmdexecopt, pvaIn, pvaOut);
		}
		if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 && nCmdID == 289 && _IsLoading)
		{
			num = 0;
		}
		IOleCommandTarget oleCommandTarget = ActiveTab<IOleCommandTarget>();
		if (oleCommandTarget != null)
		{
			Guid pguidCmdGroup2 = pguidCmdGroup;
			return oleCommandTarget.Exec(ref pguidCmdGroup2, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}
		return num;
	}

	int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		int num = HandleQueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		if (num == 0)
		{
			return num;
		}
		for (int i = 0; i < prgCmds.Length; i++)
		{
			uint cmdID = prgCmds[i].cmdID;
			GuidId guidId = new GuidId(pguidCmdGroup, cmdID);
			if (ToolbarManager.TryGetCommandHandler(GetType(), guidId, out IBTabbedEditorToolbarCommandHandler commandHandler))
			{
				return commandHandler.HandleQueryStatus(this, ref prgCmds[i], pCmdText);
			}
		}
		IOleCommandTarget oleCommandTarget = ActiveTab<IOleCommandTarget>();
		if (oleCommandTarget != null)
		{
			Guid pguidCmdGroup2 = pguidCmdGroup;
			return oleCommandTarget.QueryStatus(ref pguidCmdGroup2, cCmds, prgCmds, pCmdText);
		}
		return num;
	}

	public virtual int HandleExec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 && nCmdID == 377)
		{
			SetupF1Help();
			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}
		return VSConstants.E_NOTIMPL;
	}

	public virtual int HandleQueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		return VSConstants.E_NOTIMPL;
	}

	int IVsMultiViewDocumentView.ActivateLogicalView(ref Guid rguidLogicalView)
	{
		if (rguidLogicalView.Equals(VSConstants.LOGVIEWID_Any) || rguidLogicalView.Equals(VSConstants.LOGVIEWID_Primary))
		{
			return 0;
		}
		return ActivateView(ref rguidLogicalView, EnTabViewMode.Default);
	}

	int IVsMultiViewDocumentView.GetActiveLogicalView(out Guid pguidLogicalView)
	{
		pguidLogicalView = Guid.Empty;
		return 0;
	}

	int IVsMultiViewDocumentView.IsLogicalViewActive(ref Guid rguidLogicalView, out int pIsActive)
	{
		Guid tabLogicalView = GetTabLogicalView(rguidLogicalView);
		AbstractEditorTab activeTab = _TabbedEditorUI.ActiveTab;
		if (activeTab != null)
		{
			pIsActive = (activeTab.LogicalView.Equals(tabLogicalView) ? 1 : 0);
		}
		else
		{
			pIsActive = (_RequestedView.Equals(tabLogicalView) ? 1 : 0);
		}
		return 0;
	}

	protected virtual int HandleCloseEditorOrDesigner()
	{
		return 0;
	}

	int IVsWindowFrameNotify3.OnClose(ref uint frameSaveOptions)
	{
		// Tracer.Trace(GetType(), "OnClose()");

		int hresult = 0;
		IEnumerable<uint> enumerable = null;

		if ((frameSaveOptions & (uint)__FRAMECLOSE.FRAMECLOSE_NoSave) > 0)
		{
			enumerable = CommonVsUtilities.EnumerateOpenedDocuments(this, CommonVsUtilities.EnDocumentsFlag.DirtyExceptPrimary);

			if (enumerable.Any())
				frameSaveOptions = (uint)__FRAMECLOSE.FRAMECLOSE_PromptSave;
		}

		bool keepDocAlive = false;
		uint primaryDocCookie = GetPrimaryDocCookie();

		if ((frameSaveOptions & (uint)__FRAMECLOSE.FRAMECLOSE_NoSave) == 0)
		{
			List<uint> saveDocCookieList = [];

			enumerable ??= CommonVsUtilities.EnumerateOpenedDocuments(this, CommonVsUtilities.EnDocumentsFlag.DirtyExceptPrimary);
			List<uint> docCookieList = new List<uint>(enumerable) { primaryDocCookie };



			foreach (uint docCookie in docCookieList)
			{
				if (RdtManager.Instance.ShouldKeepDocDataAliveOnClose(docCookie))
					keepDocAlive = true;
				else
					saveDocCookieList.Add(docCookie);
			}


			if (keepDocAlive)
			{
				if (saveDocCookieList.Count == 0)
				{
					hresult = VSConstants.S_OK;
				}
				else
				{
					try
					{
						_OverrideSaveDocCookieList = saveDocCookieList;
						hresult = SaveFiles(ref frameSaveOptions);
					}
					finally
					{
						_OverrideSaveDocCookieList = null;
					}
				}
			}
			else
			{
				hresult = SaveFiles(ref frameSaveOptions);
			}

		}

		if (Native.Succeeded(hresult))
		{
			hresult = HandleCloseEditorOrDesigner();

			if (Native.Succeeded(hresult))
			{
				_IsClosing = true;

				// Remove documents the user may have saved to disk from the misc project
				// so that they're not deleted.
				if (!keepDocAlive)
				{
					IVsRunningDocumentTable rdt = RdtManager.Instance.GetRunningDocumentTable();
					rdt.GetDocumentInfo(primaryDocCookie, out _, out _, out _, out string moniker, out IVsHierarchy ppHier, out uint pitemid, out _);

					if (!moniker.StartsWith(SystemData.WinScheme, StringComparison.InvariantCultureIgnoreCase)
						&& moniker.EndsWith(SystemData.Extension, StringComparison.InvariantCultureIgnoreCase))
					{
						RemovePhysicalDocumentFromMiscProject(ppHier, pitemid);
					}
				}
			}
		}

		return hresult;

	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Cleans up any SE sql editor documents that may have been left dangling.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private int RemovePhysicalDocumentFromMiscProject(IVsHierarchy ppHier, uint itemId)
	{
		Diag.ThrowIfNotOnUIThread();

		object objProj;

		try
		{
			ppHier.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out objProj);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		// Tracer.Trace(GetType(), "RemovePhysicalDocumentFromMiscProject()", "Item type: {0}.", objProj.GetType().FullName);

		if (objProj == null || objProj is not ProjectItem projectItem)
		{
			ArgumentException ex = new($"Could not get project item in hierarchy {ppHier}, itemId: {itemId}.");
			Diag.Dug(ex);
			throw ex;
		}


		// Tracer.Trace("Validating project item: " + projectItem.Name + ":  Kind: " + Kind(projectItem.Kind) + " FileCount: "
		//	+ projectItem.FileCount);

		string path = projectItem.FileNames[1];

		if (!path.StartsWith(SystemData.WinScheme, StringComparison.InvariantCultureIgnoreCase)
			&& path.EndsWith(SystemData.Extension, StringComparison.InvariantCultureIgnoreCase))
		{
			try
			{
				projectItem.Remove();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}

		return VSConstants.S_OK;
	}


	int IVsWindowFrameNotify3.OnDockableChange(int fDockable, int x, int y, int w, int h)
	{
		return 0;
	}

	int IVsWindowFrameNotify3.OnMove(int x, int y, int w, int h)
	{
		return 0;
	}

	int IVsWindowFrameNotify3.OnShow(int fShow)
	{
		switch ((__FRAMESHOW)fShow)
		{
			case __FRAMESHOW.FRAMESHOW_WinShown:
				using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
				{
					EnsureTabs();
					EnsureToolbarAssociatedWithTabs();
					OnShow(fShow);
				}
				// _FirstTimeShowEventHandled = true;
				break;
			case __FRAMESHOW.FRAMESHOW_TabActivated:
			case __FRAMESHOW.FRAMESHOW_TabDeactivated:
				OnShow(fShow);
				foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
				{
					((IVsWindowFrameNotify3)tab).OnShow(fShow);
				}
				break;
		}
		return 0;
	}

	private void EnsureToolbarAssociatedWithTabs()
	{
		if (_TabbedEditorUI == null)
		{
			return;
		}
		IVsToolWindowToolbarHost vsToolbarHost = _TabbedEditorUI.GetVsToolbarHost();
		if (vsToolbarHost == null)
		{
			return;
		}
		foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
		{
			tab.CurrentFrame?.SetProperty((int)__VSFPROPID.VSFPROPID_ToolbarHost, vsToolbarHost);
		}
	}

	protected virtual void OnShow(int fShow)
	{
	}

	int IVsWindowFrameNotify3.OnSize(int x, int y, int w, int h)
	{
		return 0;
	}


	int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
	{
		if (_IsLoading || _IsInUpdateCmdUIContext || !_IsAppActivated)
		{
			return 0;
		}
		if (elementid == 1)
		{
			IVsWindowFrame tabFrame = ((IBTabbedEditorService)this).TabFrame;
			IVsWindowFrame vsWindowFrame = varValueOld as IVsWindowFrame;
			IVsWindowFrame vsWindowFrame2 = varValueNew as IVsWindowFrame;
			AbstractEditorTab editorTab = _TabbedEditorUI.ActiveTab;
			if (vsWindowFrame2 == tabFrame)
			{
				_TabbedEditorUI.BeginInvoke(new MethodInvoker(UpdateCmdUIContext));
			}
			else if (vsWindowFrame2 != null)
			{
				bool flag = false;
				foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
				{
					if (tab.CurrentFrame == vsWindowFrame2 && tab.IsVisible)
					{
						flag = true;
						editorTab = tab;
					}
					else if (tab.CurrentFrame == vsWindowFrame && vsWindowFrame != null && (!Native.Succeeded(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_RDTDocData, out object pvar)) || pvar == null))
					{
						return 0;
					}
				}
				if (flag)
				{
					foreach (AbstractEditorTab tab2 in _TabbedEditorUI.Tabs)
					{
						if (tab2 == editorTab)
						{
							_TabbedEditorUI.ActivateTab(tab2, EnTabViewMode.Default);
							tab2.UpdateActive(isActive: true);
						}
						else
						{
							tab2.UpdateActive(isActive: false);
						}
					}
				}
			}
		}
		return 0;
	}


	int IVsBroadcastMessageEvents.OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
	{
		if (msg == 28)
		{
			_IsAppActivated = (int)wParam != 0;
			if (_IsAppActivated)
			{
				UpdateCmdUIContext();
			}
		}
		return 0;
	}

	int IVsCodeWindow.Close()
	{
		// Tracer.Trace(GetType(), "Close()");

		return XamlCodeWindow?.Close() ?? (VSConstants.E_NOTIMPL);
	}

	int IVsCodeWindow.GetBuffer(out IVsTextLines ppBuffer)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;
		if (xamlCodeWindow != null)
		{
			return xamlCodeWindow.GetBuffer(out ppBuffer);
		}
		ppBuffer = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsCodeWindow.GetEditorCaption(READONLYSTATUS dwReadOnly, out string pbstrEditorCaption)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;
		if (xamlCodeWindow != null)
		{
			return xamlCodeWindow.GetEditorCaption(dwReadOnly, out pbstrEditorCaption);
		}
		pbstrEditorCaption = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsCodeWindow.GetLastActiveView(out IVsTextView ppView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;
		if (xamlCodeWindow != null)
		{
			return xamlCodeWindow.GetLastActiveView(out ppView);
		}
		ppView = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsCodeWindow.GetPrimaryView(out IVsTextView ppView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;
		if (xamlCodeWindow != null)
		{
			return xamlCodeWindow.GetPrimaryView(out ppView);
		}
		ppView = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsCodeWindow.GetSecondaryView(out IVsTextView ppView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;
		if (xamlCodeWindow != null)
		{
			return xamlCodeWindow.GetSecondaryView(out ppView);
		}
		ppView = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsCodeWindow.GetViewClassID(out Guid pclsidView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;
		if (xamlCodeWindow != null)
		{
			return xamlCodeWindow.GetViewClassID(out pclsidView);
		}
		pclsidView = Guid.Empty;
		return VSConstants.E_NOTIMPL;
	}

	int IVsCodeWindow.SetBaseEditorCaption(string[] pszBaseEditorCaption)
	{
		return XamlCodeWindow?.SetBaseEditorCaption(pszBaseEditorCaption) ?? (VSConstants.E_NOTIMPL);
	}

	int IVsCodeWindow.SetBuffer(IVsTextLines pBuffer)
	{
		return XamlCodeWindow?.SetBuffer(pBuffer) ?? (VSConstants.E_NOTIMPL);
	}

	int IVsCodeWindow.SetViewClassID(ref Guid clsidView)
	{
		return XamlCodeWindow?.SetViewClassID(ref clsidView) ?? (VSConstants.E_NOTIMPL);
	}

	int IVsDocOutlineProvider.GetOutline(out IntPtr phwnd, out IOleCommandTarget ppCmdTarget)
	{
		IVsDocOutlineProvider vsDocOutlineProvider = ActiveTab<IVsDocOutlineProvider>();
		if (vsDocOutlineProvider != null)
		{
			return vsDocOutlineProvider.GetOutline(out phwnd, out ppCmdTarget);
		}
		phwnd = IntPtr.Zero;
		ppCmdTarget = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsDocOutlineProvider.GetOutlineCaption(VSOUTLINECAPTION nCaptionType, out string pbstrCaption)
	{
		IVsDocOutlineProvider vsDocOutlineProvider = ActiveTab<IVsDocOutlineProvider>();
		if (vsDocOutlineProvider != null)
		{
			return vsDocOutlineProvider.GetOutlineCaption(nCaptionType, out pbstrCaption);
		}
		pbstrCaption = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsDocOutlineProvider.OnOutlineStateChange(uint dwMask, uint dwState)
	{
		return ActiveTab<IVsDocOutlineProvider>()?.OnOutlineStateChange(dwMask, dwState) ?? 0;
	}

	int IVsDocOutlineProvider.ReleaseOutline(IntPtr hwnd, IOleCommandTarget pCmdTarget)
	{
		return ActiveTab<IVsDocOutlineProvider>()?.ReleaseOutline(hwnd, pCmdTarget) ?? (VSConstants.E_NOTIMPL);
	}

	int IVsDocOutlineProvider2.TranslateAccelerator(MSG[] lpMsg)
	{
		return ActiveTab<IVsDocOutlineProvider2>()?.TranslateAccelerator(lpMsg) ?? (-2147467259);
	}

	int IVsToolboxActiveUserHook.InterceptDataObject(Microsoft.VisualStudio.OLE.Interop.IDataObject pIn, out Microsoft.VisualStudio.OLE.Interop.IDataObject ppOut)
	{
		IVsToolboxActiveUserHook vsToolboxActiveUserHook = ActiveTab<IVsToolboxActiveUserHook>();
		if (vsToolboxActiveUserHook != null)
		{
			return vsToolboxActiveUserHook.InterceptDataObject(pIn, out ppOut);
		}
		ppOut = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsToolboxActiveUserHook.ToolboxSelectionChanged(Microsoft.VisualStudio.OLE.Interop.IDataObject pSelected)
	{
		return ActiveTab<IVsToolboxActiveUserHook>()?.ToolboxSelectionChanged(pSelected) ?? (VSConstants.E_NOTIMPL);
	}

	int IVsToolboxUser.IsSupported(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		return ActiveTab<IVsToolboxUser>()?.IsSupported(pDO) ?? (VSConstants.E_NOTIMPL);
	}

	int IVsToolboxUser.ItemPicked(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		return ActiveTab<IVsToolboxUser>()?.ItemPicked(pDO) ?? (VSConstants.E_NOTIMPL);
	}

	int IVsToolboxPageChooser.GetPreferredToolboxPage(out Guid pguidPage)
	{
		pguidPage = Guid.Empty;
		return 1;
	}

	/// <summary>
	/// IVsDesignerInfo.get_DesignerTechnology implementation 
	/// </summary>
	/// <param name="pbstrTechnology"></param>
	/// <returns></returns>
	public int get_DesignerTechnology(out string pbstrTechnology)
	{
		pbstrTechnology = string.Empty;
		return 0;
	}

	int IVsDefaultToolboxTabState.GetDefaultTabExpansion(string pszTabID, out int pfExpanded)
	{
		pfExpanded = 0;
		return 0;
	}

	int IVsExtensibleObject.GetAutomationObject(string pszPropName, out object ppDisp)
	{
		return GetAutomationObjectImpl(pszPropName, out ppDisp);
	}

	protected virtual int GetAutomationObjectImpl(string pszPropName, out object ppDisp)
	{
		ppDisp = this;
		return 0;
	}

	int IVsHasRelatedSaveItems.GetRelatedSaveTreeItems(VSSAVETREEITEM saveItem, uint celt, VSSAVETREEITEM[] rgSaveTreeItems, out uint pcActual)
	{
		if (_OverrideSaveDocCookieList == null)
		{
			IEnumerable<uint> enumerable = CommonVsUtilities.EnumerateOpenedDocuments(this, (__VSRDTSAVEOPTIONS)saveItem.grfSave);
			pcActual = 0u;
			foreach (uint item in enumerable)
			{
				if (rgSaveTreeItems != null)
				{
					rgSaveTreeItems[pcActual].docCookie = item;
				}
				pcActual++;
			}
		}
		else
		{
			pcActual = (uint)_OverrideSaveDocCookieList.Count;
			if (rgSaveTreeItems != null)
			{
				for (int i = 0; i < _OverrideSaveDocCookieList.Count; i++)
				{
					rgSaveTreeItems[i].docCookie = _OverrideSaveDocCookieList[i];
				}
			}
		}
		return 0;
	}

	int IVsDocumentLockHolder.CloseDocumentHolder(uint dwSaveOptions)
	{
		// Tracer.Trace(GetType(), "CloseDocumentHolder()");

		if (_LockHolderCookie != 0)
		{
			(GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable).UnregisterDocumentLockHolder(_LockHolderCookie);
		}

		return 0;
	}

	int IVsDocumentLockHolder.ShowDocumentHolder()
	{
		return 0;
	}

	private IVsUserContext CreateF1HelpUserContext()
	{
		IVsUserContext ppContext = null;
		if (GetService(typeof(IVsMonitorUserContext)) is IVsMonitorUserContext vsMonitorUserContext)
		{
			Native.ThrowOnFailure(vsMonitorUserContext.CreateEmptyContext(out ppContext));
			string helpKeywordForCodeWindowTextView = GetHelpKeywordForCodeWindowTextView();
			Native.ThrowOnFailure(ppContext.AddAttribute(VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_LookupF1, "keyword", helpKeywordForCodeWindowTextView));
		}
		return ppContext;
	}

	private void SetupF1Help()
	{
		string helpKeywordForCodeWindowTextView = GetHelpKeywordForCodeWindowTextView();
		if (_IsHelpInitialized || string.IsNullOrEmpty(helpKeywordForCodeWindowTextView))
		{
			return;
		}
		IVsUserContext vsUserContext = CreateF1HelpUserContext();
		if (vsUserContext != null && this != null)
		{
			((IVsCodeWindow)this).GetPrimaryView(out IVsTextView ppView);
			Native.ThrowOnFailure((ppView as IVsProvideUserContext).GetUserContext(out var ppctx));
			if (vsUserContext != null)
			{
				Native.ThrowOnFailure(ppctx.AddSubcontext(vsUserContext, 500, out _));
			}
			_IsHelpInitialized = true;
		}
	}

}
