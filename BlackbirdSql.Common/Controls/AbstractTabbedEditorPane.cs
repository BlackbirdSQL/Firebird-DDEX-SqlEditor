// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabbedEditorPane
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Events;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace BlackbirdSql.Common.Controls;

public abstract class AbstractTabbedEditorPane : WindowPane, IVsDesignerInfo, IOleCommandTarget, IVsWindowFrameNotify3, IVsMultiViewDocumentView, IVsHasRelatedSaveItems, IVsDocumentLockHolder, IVsSelectionEvents, IVsBroadcastMessageEvents, IDesignerDocumentService, ITabbedEditorService, IVsDocOutlineProvider, IVsDocOutlineProvider2, IVsToolboxActiveUserHook, IVsToolboxUser, IVsToolboxPageChooser, IVsDefaultToolboxTabState, IVsCodeWindow, IVsExtensibleObject
{
	private static TabbedEditorToolbarHandlerManager _ToolbarManager;

	private Package _package;

	private TabbedEditorUI _TabbedEditorUI;

	private Guid _requestedView;

	private uint _selectionMonitorCookie;

	private IVsMonitorSelection _SelectionMonitor;

	private bool _isAppActivated = true;

	private ITextEditor _textEditor;

	private bool _isLoading;

	private bool _isClosing;

	private bool _isInUpdateCmdUIContext;

	private uint _ToolboxCmdUICookie;

	private uint _lockHolderCookie;

	// private Guid _toolbarGuid;

	// private readonly uint _toolbarID;

	private IVsTextLines _docData;

	private IList<uint> _overrideSaveFileList;

	private bool _firstTimeShowEventHandled;

	private bool isHelpInitialized;

	public static TabbedEditorToolbarHandlerManager ToolbarManager
	{
		get
		{
			_ToolbarManager ??= new TabbedEditorToolbarHandlerManager();
			return _ToolbarManager;
		}
	}

	private IVsMonitorSelection SelectionMonitor
	{
		get
		{
			_SelectionMonitor ??= GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
			return _SelectionMonitor;
		}
	}

	private uint ToolboxCmdUICookie
	{
		get
		{
			if (_ToolboxCmdUICookie == 0 && SelectionMonitor != null)
			{
				Guid rguidCmdUI = new Guid(VSConstants.UICONTEXT.ToolboxInitialized_string);
				SelectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out _ToolboxCmdUICookie);
			}
			return _ToolboxCmdUICookie;
		}
	}

	private bool IsToolboxInitialized
	{
		get
		{
			if (SelectionMonitor != null)
			{
				Native.ThrowOnFailure(SelectionMonitor.IsCmdUIContextActive(ToolboxCmdUICookie, out int pfActive));
				return pfActive != 0;
			}
			return true;
		}
	}

	public bool IsDisposed { get; private set; }

	public virtual IVsTextLines DocData => _docData;

	ITextEditor ITabbedEditorService.TextEditor
	{
		get
		{
			return _textEditor;
		}
		set
		{
			_textEditor = value;
		}
	}

	public override IWin32Window Window => _TabbedEditorUI;

	public IVsWindowFrame Frame => GetService(typeof(SVsWindowFrame)) as IVsWindowFrame;

	protected virtual Guid PrimaryViewGuid => VSConstants.LOGVIEWID_Designer;

	AbstractEditorTab ITabbedEditorService.ActiveTab => _TabbedEditorUI.TopEditorTab;

	IVsWindowFrame ITabbedEditorService.TabFrame => GetService(typeof(IVsWindowFrame)) as IVsWindowFrame;

	public TabbedEditorUI TabbedEditorControl => _TabbedEditorUI;

	Guid ITabbedEditorService.InitialLogicalView => _requestedView;

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
		_package = package;
		_ = _package; // Suppression
		_docData = docData as IVsTextLines;
		_requestedView = VSConstants.LOGVIEWID_Designer;
		// _toolbarGuid = toolbarGuid;
		// _toolbarID = toolbarID;
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			_TabbedEditorUI = CreateTabbedEditorUI(toolbarGuid, toolbarID);
			_TabbedEditorUI.CreateControl();
		}
		SelectionMonitor?.AdviseSelectionEvents(this, out _selectionMonitorCookie);
	}

	protected override void Initialize()
	{
		base.Initialize();
		if (GetService(typeof(SVsTrackSelectionEx)) is IVsTrackSelectionEx vsTrackSelectionEx)
		{
			vsTrackSelectionEx.OnElementValueChange((uint)VSConstants.VSSELELEMID.SEID_PropertyBrowserSID, 1, null);
			vsTrackSelectionEx.OnElementValueChange((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, 1, null);
		}
		_ = GetService(typeof(SVsWindowFrame)) is IVsWindowFrame;
	}

	protected override void OnCreate()
	{
		(GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable).RegisterDocumentLockHolder(0u, GetPrimaryDocCookie(), this, out _lockHolderCookie);
	}

	bool ITabbedEditorService.IsTabVisible(Guid logicalView)
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
				if (_lockHolderCookie != 0)
				{
					(GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable).UnregisterDocumentLockHolder(_lockHolderCookie);
					_lockHolderCookie = 0u;
				}
				if (_selectionMonitorCookie != 0 && _SelectionMonitor != null)
				{
					_SelectionMonitor.UnadviseSelectionEvents(_selectionMonitorCookie);
				}
				_selectionMonitorCookie = 0u;
				_SelectionMonitor = null;
				if (_TabbedEditorUI != null)
				{
					_TabbedEditorUI.Dispose();
					_TabbedEditorUI = null;
				}
				_docData = null;
				_package = null;
				_textEditor = null;
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
		_isClosing = true;
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
		if (_TabbedEditorUI == null || _TabbedEditorUI.Tabs.Count > 0 || _isLoading || _isClosing)
		{
			if (_TabbedEditorUI == null)
				Diag.Dug(true, "TabbedEditorUI is null");
			return false;
		}

		try
		{
			_TabbedEditorUI.DesignerMessage = "";
			_isLoading = true;
			GetService(typeof(SVsWindowFrame));
			IVsUIShell vsUIShell = GetService(typeof(IVsUIShell)) as IVsUIShell;
			vsUIShell?.SetWaitCursor();
			Guid guid = PrimaryViewGuid;
			bool flag = true;
			int num;
			if (!activateTextView && !(_requestedView == VSConstants.LOGVIEWID_TextView))
			{
				num = ((_requestedView == VSConstants.LOGVIEWID_Debugging) ? 1 : 0);
				if (num == 0)
				{
					goto IL_00c0;
				}
			}
			else
			{
				num = 1;
			}
			if (!flag && guid != VSConstants.LOGVIEWID_TextView)
			{
				guid = VSConstants.LOGVIEWID_TextView;
			}
			goto IL_00c0;
		IL_00c0:
			_TabbedEditorUI.LoadWindowState("");
			_TabbedEditorUI.Update();
			EnEditorTabType editorTabType;
			EnEditorTabType editorTabType2;
			bool flag2;
			if (guid.Equals(VSConstants.LOGVIEWID_TextView))
			{
				editorTabType = EnEditorTabType.BottomDesign;
				editorTabType2 = EnEditorTabType.TopXaml;
				flag2 = true;
			}
			else
			{
				editorTabType = EnEditorTabType.TopDesign;
				editorTabType2 = EnEditorTabType.BottomXaml;
				flag2 = false;
			}
			AbstractEditorTab editorTab = CreateEditorTabWithButton(this, VSConstants.LOGVIEWID_Designer, VSConstants.LOGVIEWID_Primary, editorTabType);
			AbstractEditorTab editorTab2 = CreateEditorTabWithButton(this, VSConstants.LOGVIEWID_TextView, VSConstants.LOGVIEWID_Primary, editorTabType2);
			_TabbedEditorUI.Tabs.Add(editorTab);
			_TabbedEditorUI.Tabs.Add(editorTab2);
			vsUIShell?.SetWaitCursor();
			LoadXamlPane(editorTab2, flag2, flag);
			LoadDesignerPane(editorTab, !flag2, flag);
			if (((uint)num | (flag2 ? 1u : 0u)) != 0)
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
			_isLoading = false;
			if (_TabbedEditorUI != null)
			{
				_TabbedEditorUI.DesignerMessage = null;
			}
		}
		return true;
	}

	public void LoadXamlPane(AbstractEditorTab xamlTab, bool primary, bool showSplitter)
	{
		if (primary)
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

	public void LoadDesignerPane(AbstractEditorTab designerTab, bool primary, bool showSplitter)
	{
		if (primary)
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
		if (serviceType == typeof(ITabbedEditorService))
		{
			return this;
		}
		return base.GetService(serviceType);
	}

	private void OnSaveOptions(object sender, EventArgs e)
	{
		if (_TabbedEditorUI != null)
		{
			_ = _TabbedEditorUI.TopEditorTab;
		}
	}

	private void OnTabActivated(object sender, EventArgs e)
	{
		_requestedView = Guid.Empty;
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
				_ = 3;
			}
			rguid = editorTab.CommandUIGuid;
		}
		vsWindowFrame.SetGuidProperty(-4007, ref rguid);
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
		if (VsShellUtilities.IsInAutomationFunction(this))
		{
			saveFlags = (uint)__FRAMECLOSE.FRAMECLOSE_NoSave;
		}
		int num = 0;
		if (saveFlags != (uint)__FRAMECLOSE.FRAMECLOSE_NoSave)
		{
			if ((IVsWindowFrame)GetService(typeof(SVsWindowFrame)) == null)
			{
				return 0;
			}
			uint saveOpts = (__FRAMECLOSE)saveFlags switch
			{
				__FRAMECLOSE.FRAMECLOSE_PromptSave => (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_PromptSave,
				__FRAMECLOSE.FRAMECLOSE_SaveIfDirty => (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_SaveIfDirty,
				_ => (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_DocClose,
			};
			uint primaryDocCookie = GetPrimaryDocCookie();
			num = (GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable).SaveDocuments(saveOpts, null, uint.MaxValue, primaryDocCookie);
			if (Native.Succeeded(num))
			{
				saveFlags = (uint)__FRAMECLOSE.FRAMECLOSE_NoSave;
			}
		}
		return num;
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
			_TabbedEditorUI.TabActivated += OnTabActivated;
		}
		else
		{
			_TabbedEditorUI.TabActivated -= OnTabActivated;
		}
	}

	private void UpdateCmdUIContext()
	{
		_isInUpdateCmdUIContext = true;
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
					selectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out var pdwCmdUICookie);
					selectionMonitor.SetCmdUIContext(pdwCmdUICookie, 1);
					activeTab.Activate();
				}
			}
		}
		finally
		{
			_isInUpdateCmdUIContext = false;
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
		Guid g = (_requestedView = GetTabLogicalView(rguidLogicalView));
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

	void ITabbedEditorService.Activate(Guid logicalView, EnTabViewMode mode)
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
		if (ToolbarManager.TryGetCommandHandler(GetType(), guidId, out ITabbedEditorToolbarCommandHandler commandHandler))
		{
			return commandHandler.HandleExec(this, nCmdexecopt, pvaIn, pvaOut);
		}
		if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 && nCmdID == 289 && _isLoading)
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
			if (ToolbarManager.TryGetCommandHandler(GetType(), guidId, out ITabbedEditorToolbarCommandHandler commandHandler))
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
			pIsActive = (_requestedView.Equals(tabLogicalView) ? 1 : 0);
		}
		return 0;
	}

	protected virtual int HandleCloseEditorOrDesigner()
	{
		return 0;
	}

	int IVsWindowFrameNotify3.OnClose(ref uint pgrfSaveOptions)
	{
		int num = 0;
		uint num2 = (uint)__FRAMECLOSE.FRAMECLOSE_NoSave;
		IEnumerable<uint> enumerable = null;
		if ((pgrfSaveOptions & num2) == num2)
		{
			enumerable = CommonVsUtilities.EnumerateOpenedDocuments(this, CommonVsUtilities.EnDocumentsFlag.DirtyExceptPrimary);
			if (enumerable.Any())
			{
				pgrfSaveOptions = (uint)__FRAMECLOSE.FRAMECLOSE_PromptSave;
			}
		}
		if ((pgrfSaveOptions & num2) != num2)
		{
			bool flag = false;
			List<uint> list = new List<uint>();
			uint primaryDocCookie = GetPrimaryDocCookie();
			enumerable ??= CommonVsUtilities.EnumerateOpenedDocuments(this, CommonVsUtilities.EnDocumentsFlag.DirtyExceptPrimary);
			foreach (uint item in new List<uint>(enumerable) { primaryDocCookie })
			{
				if (RdtManager.Instance.ShouldKeepDocDataAliveOnClose(item))
				{
					flag = true;
				}
				else
				{
					list.Add(item);
				}
			}
			if (flag)
			{
				if (list.Count == 0)
				{
					num = 0;
				}
				else
				{
					try
					{
						_overrideSaveFileList = list;
						num = SaveFiles(ref pgrfSaveOptions);
					}
					finally
					{
						_overrideSaveFileList = null;
					}
				}
			}
			else
			{
				num = SaveFiles(ref pgrfSaveOptions);
			}
		}
		if (Native.Succeeded(num))
		{
			num = HandleCloseEditorOrDesigner();
			if (Native.Succeeded(num))
			{
				_isClosing = true;
			}
		}
		return num;
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
				_firstTimeShowEventHandled = true;
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

	int IVsSelectionEvents.OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
	{
		return 0;
	}

	int IVsSelectionEvents.OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
	{
		if (_isLoading || _isInUpdateCmdUIContext || !_isAppActivated)
		{
			return 0;
		}
		if (elementid == 1)
		{
			IVsWindowFrame tabFrame = ((ITabbedEditorService)this).TabFrame;
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

	int IVsSelectionEvents.OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
	{
		return 0;
	}

	int IVsBroadcastMessageEvents.OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
	{
		if (msg == 28)
		{
			_isAppActivated = (int)wParam != 0;
			if (_isAppActivated)
			{
				UpdateCmdUIContext();
			}
		}
		return 0;
	}

	int IVsCodeWindow.Close()
	{
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
		if (_overrideSaveFileList == null)
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
			pcActual = (uint)_overrideSaveFileList.Count;
			if (rgSaveTreeItems != null)
			{
				for (int i = 0; i < _overrideSaveFileList.Count; i++)
				{
					rgSaveTreeItems[i].docCookie = _overrideSaveFileList[i];
				}
			}
		}
		return 0;
	}

	int IVsDocumentLockHolder.CloseDocumentHolder(uint dwSaveOptions)
	{
		if (_lockHolderCookie != 0)
		{
			(GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable).UnregisterDocumentLockHolder(_lockHolderCookie);
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
		if (isHelpInitialized || string.IsNullOrEmpty(helpKeywordForCodeWindowTextView))
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
			isHelpInitialized = true;
		}
	}

	[Conditional("DEBUG")]
#pragma warning disable IDE0051 // Remove unused private members
	private void VerifyTabShowFalse()
#pragma warning restore IDE0051 // Remove unused private members
	{
		if (_firstTimeShowEventHandled)
		{
			return;
		}
		foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
		{
			_ = tab;
		}
	}
}
