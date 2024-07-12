// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabbedEditorPane
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using DpiAwareness = Microsoft.VisualStudio.Utilities.DpiAwareness;
using DpiAwarenessContext = Microsoft.VisualStudio.Utilities.DpiAwarenessContext;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;



namespace BlackbirdSql.Shared.Controls;


public abstract class AbstractTabbedEditorWindowPane : WindowPane, IVsDesignerInfo, IOleCommandTarget,
	IVsWindowFrameNotify3, IVsMultiViewDocumentView, IVsHasRelatedSaveItems, IVsDocumentLockHolder,
	IVsBroadcastMessageEvents, IBTabbedEditorService, IVsDocOutlineProvider, IVsDocOutlineProvider2,
	IVsToolboxActiveUserHook, IVsToolboxUser, IVsToolboxPageChooser, IVsDefaultToolboxTabState, IVsCodeWindow, IVsExtensibleObject
{

	public AbstractTabbedEditorWindowPane(System.IServiceProvider provider, Package package, object docData, Guid toolbarGuid, uint toolbarID)
		: base(provider)
	{
		Diag.ThrowIfNotOnUIThread();

		_Package = package;
		_ = _Package; // Suppression
		_DocData = docData as IVsTextLines;
		_RequestedView = VSConstants.LOGVIEWID_Designer;

		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			_TabbedEditorUI = CreateTabbedEditorUI(toolbarGuid, toolbarID);
			_TabbedEditorUI.CreateControl();
		}
	}



	protected override void Dispose(bool disposing)
	{
		if (_IsDisposed)
			return;

		try
		{
			if (disposing)
			{
				if (ThreadHelper.CheckAccess() && _BroadcastMessageEventsCookie != 0
					&& Package.GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
				{
					___(vsShell.UnadviseBroadcastMessages(_BroadcastMessageEventsCookie));
				}

				if (_LockHolderCookie != 0)
				{
					RdtManager.UnregisterDocumentLockHolder(_LockHolderCookie);
					_LockHolderCookie = 0u;
				}

				ApcManager.OnElementValueChangedEvent -= OnElementValueChanged;

				_TabbedEditorUI?.Dispose();
				_TabbedEditorUI = null;

				_DocData = null;
				_Package = null;
				_TextEditor = null;
				_IsDisposed = true;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}



	protected override void Initialize()
	{

		base.Initialize();

		// TODO: Added to test broadcast messages.
		if (ThreadHelper.CheckAccess() && _BroadcastMessageEventsCookie == 0
			&& Package.GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
		{
			___(vsShell.AdviseBroadcastMessages(this, out _BroadcastMessageEventsCookie));
		}

		ApcManager.OnElementValueChangedEvent += OnElementValueChanged;

		if (GetService(typeof(SVsTrackSelectionEx)) is IVsTrackSelectionEx vsTrackSelectionEx)
		{
			vsTrackSelectionEx.OnElementValueChange((uint)VSConstants.VSSELELEMID.SEID_PropertyBrowserSID, 1, null);
			vsTrackSelectionEx.OnElementValueChange((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, 1, null);
		}

		_ = GetService(typeof(SVsWindowFrame)) is IVsWindowFrame;
	}





	public enum EnDocumentsFlag
	{
		DirtyDocuments,
		DirtyOrPrimary,
		DirtyExceptPrimary,
		AllDocuments
	}



	private static ToolbarCommandMapper _ToolbarManager;

	private Package _Package;
	private AbstractTabbedEditorUIControl _TabbedEditorUI;
	private Guid _RequestedView;
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
	private bool _FirstTimeShowEventHandled;
	private bool _IsHelpInitialized;
	private uint _BroadcastMessageEventsCookie = 0;

	protected bool _IsDisposed = false;



	public static ToolbarCommandMapper ToolbarManager =>
		_ToolbarManager ??= new ToolbarCommandMapper();



	public virtual IVsTextLines DocData => _DocData;

	public bool IsToolboxInitialized => ApcManager.IsToolboxInitialized;


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

	public IVsWindowFrame TabFrame => Frame;

	public AbstractTabbedEditorUIControl TabbedEditorControl => _TabbedEditorUI;

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
				vsCodeWindow = editorTab.GetView() as IVsCodeWindow;

			if (vsCodeWindow == this)
				vsCodeWindow = null;

			return vsCodeWindow;
		}
	}





	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	protected override void OnCreate()
	{
		RdtManager.RegisterDocumentLockHolder(0u, GetPrimaryDocCookie(), this, out _LockHolderCookie);
	}



	private static bool IsDirty(object docData)
	{
		Diag.ThrowIfNotOnUIThread();

		if (docData is IVsPersistDocData vsPersistDocData)
		{
			___(vsPersistDocData.IsDocDataDirty(out var pfDirty));
			return pfDirty != 0;
		}

		return false;
	}



	bool IBTabbedEditorService.IsTabVisible(Guid logicalView)
	{
		foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
		{
			if (tab.LogicalView == logicalView)
				return tab.IsOnScreen;
		}

		return false;
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



	protected AbstractEditorTab CreateEditorTabWithButton(AbstractTabbedEditorWindowPane editorPane, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
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



	protected abstract AbstractEditorTab CreateEditorTab(AbstractTabbedEditorWindowPane editorPane, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType);

	protected abstract AbstractTabbedEditorUIControl CreateTabbedEditorUI(Guid toolbarGuid, uint toolbarId);



	private static IEnumerable<uint> EnumerateOpenedDocuments(IBTabbedEditorService designerService, __VSRDTSAVEOPTIONS rdtSaveOptions)
	{
		EnDocumentsFlag enumerateDocumentsFlag = GetDesignerDocumentFlagFromSaveOption(rdtSaveOptions);

		return EnumerateOpenedDocuments(designerService, enumerateDocumentsFlag);
	}



	private static IEnumerable<uint> EnumerateOpenedDocuments(IBTabbedEditorService designerService, EnDocumentsFlag requestedDocumentsFlag)
	{
		foreach (uint editableDocument in designerService.GetEditableDocuments())
		{
			if (TryGetDocDataFromCookie(editableDocument, out var docData))
			{
				bool isDirty = IsDirty(docData);
				bool isPrimary = editableDocument == designerService.GetPrimaryDocCookie();
				bool validEnumerableDocument = false;

				switch (requestedDocumentsFlag)
				{
					case EnDocumentsFlag.DirtyDocuments:
						validEnumerableDocument = isDirty;
						break;
					case EnDocumentsFlag.DirtyOrPrimary:
						validEnumerableDocument = isDirty || isPrimary;
						break;
					case EnDocumentsFlag.DirtyExceptPrimary:
						validEnumerableDocument = isDirty && !isPrimary;
						break;
					case EnDocumentsFlag.AllDocuments:
						validEnumerableDocument = true;
						break;
				}

				if (validEnumerableDocument)
					yield return editableDocument;
			}
		}
	}



	private static EnDocumentsFlag GetDesignerDocumentFlagFromSaveOption(__VSRDTSAVEOPTIONS saveOption)
	{
		if ((saveOption & __VSRDTSAVEOPTIONS.RDTSAVEOPT_ForceSave) == 0)
			return EnDocumentsFlag.DirtyDocuments;

		return EnDocumentsFlag.DirtyOrPrimary;
	}



	public virtual string GetHelpKeywordForCodeWindowTextView()
	{
		return string.Empty;
	}



	public virtual bool EnsureTabs(bool activateTextView)
	{
		if (_TabbedEditorUI == null || _TabbedEditorUI.Tabs.Count > 0 || _IsLoading || _IsClosing || ApcManager.SolutionClosing)
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
				topIsTextView = _RequestedView == VSConstants.LOGVIEWID_Debugging;
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

			if (topIsTextView || isPrimary)
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
				vsToolbox.SelectItem(null);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			_IsLoading = false;

			if (_TabbedEditorUI != null)
				_TabbedEditorUI.DesignerMessage = null;
		}

		return true;
	}



	public virtual bool UpdateTabs(QESQLQueryDataEventArgs args)
	{
		return _TabbedEditorUI != null && _TabbedEditorUI.Tabs.Count > 0 && !_IsClosing;
	}



	public void LoadXamlPane(AbstractEditorTab xamlTab, bool asPrimary, bool showSplitter)
	{
		if (asPrimary)
		{
			if (_TabbedEditorUI.TopEditorTab == null)
				_TabbedEditorUI.TopEditorTab = xamlTab;
			else
				xamlTab.EnsureFrameCreated();
		}
		else if (showSplitter)
		{
			if (_TabbedEditorUI.BottomEditorTab == null)
				_TabbedEditorUI.BottomEditorTab = xamlTab;
			else
				xamlTab.EnsureFrameCreated();
		}

		xamlTab.CreateTextEditor();
	}

	public void LoadDesignerPane(AbstractEditorTab designerTab, bool asPrimary, bool showSplitter)
	{
		if (asPrimary)
		{
			if (_TabbedEditorUI.TopEditorTab == null)
				_TabbedEditorUI.TopEditorTab = designerTab;
			else
				designerTab.EnsureFrameCreated();
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
		if (logicalView.Equals(VSConstants.LOGVIEWID_Any)
			|| logicalView.Equals(VSConstants.LOGVIEWID_Designer)
			|| logicalView.Equals(VSConstants.LOGVIEWID_Primary))
		{
			return VSConstants.LOGVIEWID_Designer;
		}

		if (logicalView.Equals(VSConstants.LOGVIEWID_Debugging))
			return VSConstants.LOGVIEWID_TextView;

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
			Diag.ThrowException(new ArgumentNullException("serviceType"));

		if (serviceType == typeof(IOleCommandTarget))
			return this;

		if (serviceType == typeof(IBTabbedEditorService))
			return this;

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
	}



	private void OnTabActivated(object sender, EventArgs e)
	{
		_RequestedView = Guid.Empty;

		if (Frame == null)
			return;

		Guid rguid = Guid.Empty;

		if (sender is AbstractEditorTab editorTab)
		{
			if (editorTab.EditorTabType != EnEditorTabType.TopXaml)
				_ = editorTab.EditorTabType;

			rguid = editorTab.CommandUIGuid;
		}

		Frame.SetGuidProperty((int)__VSFPROPID.VSFPROPID_CmdUIGuid, ref rguid);
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
				return VSConstants.S_OK;

			uint saveOpts;

			switch ((__FRAMECLOSE)saveFlags)
			{
				case __FRAMECLOSE.FRAMECLOSE_PromptSave:
					saveOpts = (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_PromptSave;
					break;

				case __FRAMECLOSE.FRAMECLOSE_SaveIfDirty:
					saveOpts = (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_SaveIfDirty;
					break;

				default:
					saveOpts = (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_DocClose;
					break;
			}

			uint primaryDocCookie = GetPrimaryDocCookie();

			hresult = RdtManager.SaveDocuments(saveOpts, null, uint.MaxValue, primaryDocCookie);

			if (__(hresult))
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
			yield return primaryDocCookie;
	}



	private static uint GetFrameDocument(IVsWindowFrame frame)
	{
		___(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out var pvar));

		return (uint)(int)pvar;
	}



	public uint GetPrimaryDocCookie()
	{
		if (Frame == null)
			return 0u;

		return GetFrameDocument(Frame);
	}


	private void TrackTabSwitches(bool track)
	{
		if (track)
			_TabbedEditorUI.TabActivatedEvent += OnTabActivated;
		else
			_TabbedEditorUI.TabActivatedEvent -= OnTabActivated;
	}



	private static bool TryGetDocDataFromCookie(uint cookie, out object docData)
	{
		Diag.ThrowIfNotOnUIThread();

		docData = null;
		bool result = false;

		if (!RdtManager.ServiceAvailable)
			return result;

		IntPtr ppunkDocData = IntPtr.Zero;

		try
		{
			if (__(RdtManager.GetDocumentInfo(cookie, out var _, out var _, out var _, out var _,
				out var _, out var _, out ppunkDocData)))
			{
				docData = Marshal.GetObjectForIUnknown(ppunkDocData);
				result = true;
			}
		}
		finally
		{
			if (ppunkDocData != IntPtr.Zero)
				Marshal.Release(ppunkDocData);
		}

		return result;
	}



	private void UpdateCmdUIContext()
	{
		_IsInUpdateCmdUIContext = true;

		try
		{
			if (_TabbedEditorUI == null)
				return;

			AbstractEditorTab activeTab = _TabbedEditorUI.ActiveTab;

			if (activeTab == null)
				return;

			Guid rguidCmdUI = activeTab.CommandUIGuid;

			IVsMonitorSelection selectionMonitor = ApcManager.SelectionMonitor;

			if (selectionMonitor == null)
				return;

			object service = GetService(typeof(SVsWindowFrame));
			selectionMonitor.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_WindowFrame, out object pvarValue);

			if (pvarValue == service)
			{
				selectionMonitor.GetCmdUIContextCookie(ref rguidCmdUI, out uint pdwCmdUICookie);
				selectionMonitor.SetCmdUIContext(pdwCmdUICookie, 1);
				activeTab.Activate();
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

		if (_TabbedEditorUI.Tabs.Count == 0)
			return null;

		foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
		{
			if (tab.LogicalView.Equals(tabLogicalView))
				return tab;
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
					val = null;
			}
		}

		return val;
	}



	private int ActivateView(ref Guid rguidLogicalView, EnTabViewMode mode)
	{
		Guid clsid = _RequestedView = GetTabLogicalView(rguidLogicalView);

		if (_TabbedEditorUI.Tabs.Count > 0)
		{
			foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
			{
				if (tab.LogicalView.Equals(clsid))
				{
					TrackTabSwitches(track: false);
					_TabbedEditorUI.ActivateTab(tab, mode);
					TrackTabSwitches(track: true);
					break;
				}
			}
		}

		return VSConstants.S_OK;
	}



	void IBTabbedEditorService.Activate(Guid logicalView, EnTabViewMode mode)
	{
		ActivateView(ref logicalView, mode);
	}



	int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		int hresult = HandleExec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

		if (hresult == VSConstants.S_OK)
			return hresult;

		GuidId guidId = new GuidId(pguidCmdGroup, nCmdID);

		if (ToolbarManager.TryGetCommandHandler(GetType(), guidId, out IBToolbarCommandHandler commandHandler))
		{
			return commandHandler.HandleExec(this, nCmdexecopt, pvaIn, pvaOut);
		}

		if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 && nCmdID == 289 && _IsLoading)
		{
			hresult = VSConstants.S_OK;
		}

		IOleCommandTarget oleCommandTarget = ActiveTab<IOleCommandTarget>();

		if (oleCommandTarget != null)
		{
			Guid pguidCmdGroup2 = pguidCmdGroup;

			return oleCommandTarget.Exec(ref pguidCmdGroup2, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}

		return hresult;
	}



	int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		int hresult = HandleQueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

		if (hresult == VSConstants.S_OK)
			return hresult;

		for (int i = 0; i < prgCmds.Length; i++)
		{
			uint cmdID = prgCmds[i].cmdID;
			GuidId guidId = new GuidId(pguidCmdGroup, cmdID);

			if (ToolbarManager.TryGetCommandHandler(GetType(), guidId, out IBToolbarCommandHandler commandHandler))
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

		return hresult;
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
			return VSConstants.S_OK;
		}

		return ActivateView(ref rguidLogicalView, EnTabViewMode.Default);
	}



	int IVsMultiViewDocumentView.GetActiveLogicalView(out Guid pguidLogicalView)
	{
		pguidLogicalView = Guid.Empty;

		return VSConstants.S_OK;
	}



	int IVsMultiViewDocumentView.IsLogicalViewActive(ref Guid rguidLogicalView, out int pIsActive)
	{
		Guid tabLogicalView = GetTabLogicalView(rguidLogicalView);
		AbstractEditorTab activeTab = _TabbedEditorUI.ActiveTab;

		if (activeTab != null)
			pIsActive = activeTab.LogicalView.Equals(tabLogicalView) ? 1 : 0;
		else
			pIsActive = _RequestedView.Equals(tabLogicalView) ? 1 : 0;

		return VSConstants.S_OK;
	}



	protected virtual int HandleCloseEditorOrDesigner()
	{
		// Tracer.Trace(GetType(), "HandleCloseEditorOrDesigner()");

		return VSConstants.S_OK;
	}



	int IVsWindowFrameNotify3.OnClose(ref uint frameSaveOptions)
	{
		// Tracer.Trace(GetType(), "OnClose()");

		int hresult = 0;
		IEnumerable<uint> enumerable = null;

		if ((frameSaveOptions & (uint)__FRAMECLOSE.FRAMECLOSE_NoSave) > 0)
		{
			enumerable = EnumerateOpenedDocuments(this, EnDocumentsFlag.DirtyExceptPrimary);

			if (enumerable.Any())
				frameSaveOptions = (uint)__FRAMECLOSE.FRAMECLOSE_PromptSave;
		}

		bool keepDocAlive = false;
		uint primaryDocCookie = GetPrimaryDocCookie();

		if ((frameSaveOptions & (uint)__FRAMECLOSE.FRAMECLOSE_NoSave) == 0)
		{
			List<uint> saveDocCookieList = [];

			enumerable ??= EnumerateOpenedDocuments(this, EnDocumentsFlag.DirtyExceptPrimary);
			List<uint> docCookieList = new List<uint>(enumerable) { primaryDocCookie };



			foreach (uint docCookie in docCookieList)
			{
				if (RdtManager.ShouldKeepDocDataAliveOnClose(docCookie))
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
					_OverrideSaveDocCookieList = saveDocCookieList;

					try
					{
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

		if (__(hresult))
		{
			hresult = HandleCloseEditorOrDesigner();

			if (__(hresult))
				_IsClosing = true;
		}

		return hresult;

	}



	int IVsWindowFrameNotify3.OnDockableChange(int fDockable, int x, int y, int w, int h)
	{
		return VSConstants.S_OK;
	}



	int IVsWindowFrameNotify3.OnMove(int x, int y, int w, int h)
	{
		return VSConstants.S_OK;
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

				// TODO: Attempt to focus the textview on first show.
				if (!_FirstTimeShowEventHandled)
				{
					Guid clsid = VSConstants.LOGVIEWID_TextView;
					ActivateView(ref clsid, EnTabViewMode.Default);
				}

				_FirstTimeShowEventHandled = true;

				break;
			case __FRAMESHOW.FRAMESHOW_TabActivated:
			case __FRAMESHOW.FRAMESHOW_TabDeactivated:
				OnShow(fShow);

				foreach (AbstractEditorTab tab in _TabbedEditorUI.Tabs)
					((IVsWindowFrameNotify3)tab).OnShow(fShow);

				break;
		}

		return VSConstants.S_OK;
	}

	private void EnsureToolbarAssociatedWithTabs()
	{
		if (_TabbedEditorUI == null)
			return;

		IVsToolWindowToolbarHost vsToolbarHost = _TabbedEditorUI.GetVsToolbarHost();

		if (vsToolbarHost == null)
			return;

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
		return VSConstants.S_OK;
	}



	int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
	{
		// Tracer.Trace(GetType(), "OnElementValueChanged()", "ElementId: {0}.", elementid);

		if (_IsLoading || _IsInUpdateCmdUIContext || !_IsAppActivated || ApcManager.SolutionClosing)
		{
			return VSConstants.S_OK;
		}

		if (elementid == 1)
		{
			IVsWindowFrame tabFrame = TabFrame;
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
					else if (tab.CurrentFrame == vsWindowFrame && vsWindowFrame != null
						&& (!__(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_RDTDocData, out object pvar))
						|| pvar == null))
					{
						return VSConstants.S_OK;
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

		return VSConstants.S_OK;
	}


	int IVsBroadcastMessageEvents.OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
	{
		if (msg == 28)
		{
			_IsAppActivated = (int)wParam != 0;

			if (_IsAppActivated)
				UpdateCmdUIContext();
		}

		return VSConstants.S_OK;
	}

	int IVsCodeWindow.Close()
	{
		// Tracer.Trace(GetType(), "Close()");

		return XamlCodeWindow?.Close() ?? VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.GetBuffer(out IVsTextLines ppBuffer)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetBuffer(out ppBuffer);

		ppBuffer = null;

		return VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.GetEditorCaption(READONLYSTATUS dwReadOnly, out string pbstrEditorCaption)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetEditorCaption(dwReadOnly, out pbstrEditorCaption);

		pbstrEditorCaption = null;

		return VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.GetLastActiveView(out IVsTextView ppView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetLastActiveView(out ppView);

		ppView = null;

		return VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.GetPrimaryView(out IVsTextView ppView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetPrimaryView(out ppView);

		ppView = null;

		return VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.GetSecondaryView(out IVsTextView ppView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetSecondaryView(out ppView);

		ppView = null;

		return VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.GetViewClassID(out Guid pclsidView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetViewClassID(out pclsidView);

		pclsidView = Guid.Empty;

		return VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.SetBaseEditorCaption(string[] pszBaseEditorCaption)
	{
		return XamlCodeWindow?.SetBaseEditorCaption(pszBaseEditorCaption) ?? VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.SetBuffer(IVsTextLines pBuffer)
	{
		return XamlCodeWindow?.SetBuffer(pBuffer) ?? VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.SetViewClassID(ref Guid clsidView)
	{
		return XamlCodeWindow?.SetViewClassID(ref clsidView) ?? VSConstants.E_NOTIMPL;
	}



	int IVsDocOutlineProvider.GetOutline(out IntPtr phwnd, out IOleCommandTarget ppCmdTarget)
	{
		IVsDocOutlineProvider vsDocOutlineProvider = ActiveTab<IVsDocOutlineProvider>();

		if (vsDocOutlineProvider != null)
			return vsDocOutlineProvider.GetOutline(out phwnd, out ppCmdTarget);

		phwnd = IntPtr.Zero;
		ppCmdTarget = null;

		return VSConstants.E_NOTIMPL;
	}



	int IVsDocOutlineProvider.GetOutlineCaption(VSOUTLINECAPTION nCaptionType, out string pbstrCaption)
	{
		IVsDocOutlineProvider vsDocOutlineProvider = ActiveTab<IVsDocOutlineProvider>();

		if (vsDocOutlineProvider != null)
			return vsDocOutlineProvider.GetOutlineCaption(nCaptionType, out pbstrCaption);

		pbstrCaption = null;

		return VSConstants.E_NOTIMPL;
	}



	int IVsDocOutlineProvider.OnOutlineStateChange(uint dwMask, uint dwState)
	{
		return ActiveTab<IVsDocOutlineProvider>()?.OnOutlineStateChange(dwMask, dwState) ?? 0;
	}



	int IVsDocOutlineProvider.ReleaseOutline(IntPtr hwnd, IOleCommandTarget pCmdTarget)
	{
		return ActiveTab<IVsDocOutlineProvider>()?.ReleaseOutline(hwnd, pCmdTarget) ?? VSConstants.E_NOTIMPL;
	}



	int IVsDocOutlineProvider2.TranslateAccelerator(MSG[] lpMsg)
	{
		return ActiveTab<IVsDocOutlineProvider2>()?.TranslateAccelerator(lpMsg) ?? -2147467259;
	}



	int IVsToolboxActiveUserHook.InterceptDataObject(Microsoft.VisualStudio.OLE.Interop.IDataObject pIn, out Microsoft.VisualStudio.OLE.Interop.IDataObject ppOut)
	{
		IVsToolboxActiveUserHook vsToolboxActiveUserHook = ActiveTab<IVsToolboxActiveUserHook>();

		if (vsToolboxActiveUserHook != null)
			return vsToolboxActiveUserHook.InterceptDataObject(pIn, out ppOut);

		ppOut = null;

		return VSConstants.E_NOTIMPL;
	}



	int IVsToolboxActiveUserHook.ToolboxSelectionChanged(Microsoft.VisualStudio.OLE.Interop.IDataObject pSelected)
	{
		return ActiveTab<IVsToolboxActiveUserHook>()?.ToolboxSelectionChanged(pSelected) ?? VSConstants.E_NOTIMPL;
	}



	int IVsToolboxUser.IsSupported(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		return ActiveTab<IVsToolboxUser>()?.IsSupported(pDO) ?? VSConstants.E_NOTIMPL;
	}



	int IVsToolboxUser.ItemPicked(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		return ActiveTab<IVsToolboxUser>()?.ItemPicked(pDO) ?? VSConstants.E_NOTIMPL;
	}



	int IVsToolboxPageChooser.GetPreferredToolboxPage(out Guid pguidPage)
	{
		pguidPage = Guid.Empty;

		return VSConstants.S_FALSE;
	}



	/// <summary>
	/// IVsDesignerInfo.get_DesignerTechnology implementation 
	/// </summary>
	/// <param name="pbstrTechnology"></param>
	/// <returns></returns>
	int IVsDesignerInfo.get_DesignerTechnology(out string pbstrTechnology)
	{
		pbstrTechnology = string.Empty;

		return VSConstants.S_OK;
	}

	int IVsDefaultToolboxTabState.GetDefaultTabExpansion(string pszTabID, out int pfExpanded)
	{
		pfExpanded = 0;

		return VSConstants.S_OK;
	}

	int IVsExtensibleObject.GetAutomationObject(string pszPropName, out object ppDisp)
	{
		return GetAutomationObjectImpl(pszPropName, out ppDisp);
	}

	protected virtual int GetAutomationObjectImpl(string pszPropName, out object ppDisp)
	{
		ppDisp = this;

		return VSConstants.S_OK;
	}



	int IVsHasRelatedSaveItems.GetRelatedSaveTreeItems(VSSAVETREEITEM saveItem, uint celt, VSSAVETREEITEM[] rgSaveTreeItems, out uint pcActual)
	{
		// Tracer.Trace(GetType(), "GetRelatedSaveTreeItems()", "SaveOptFald: {0}.", saveItem.grfSave);

		if (_OverrideSaveDocCookieList == null)
		{
			IEnumerable<uint> enumerable = EnumerateOpenedDocuments(this, (__VSRDTSAVEOPTIONS)saveItem.grfSave);
			pcActual = 0u;

			foreach (uint item in enumerable)
			{
				if (rgSaveTreeItems != null)
					rgSaveTreeItems[pcActual].docCookie = item;

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

		return VSConstants.S_OK;
	}



	int IVsDocumentLockHolder.CloseDocumentHolder(uint dwSaveOptions)
	{
		// Tracer.Trace(GetType(), "CloseDocumentHolder()");

		if (_LockHolderCookie != 0)
			RdtManager.UnregisterDocumentLockHolder(_LockHolderCookie);

		return VSConstants.S_OK;
	}



	public void SuppressChangeTracking(bool suppress)
	{
		___(Frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar));

		if (pvar is not IVsCodeWindow codeWindow)
			throw Diag.ExceptionInstance(typeof(IVsCodeWindow));

		RdtManager.SuppressChangeTracking(null, codeWindow, suppress);
	}



	int IVsDocumentLockHolder.ShowDocumentHolder()
	{
		return VSConstants.S_OK;
	}



	private IVsUserContext CreateF1HelpUserContext()
	{
		IVsUserContext ppContext = null;

		if (GetService(typeof(IVsMonitorUserContext)) is IVsMonitorUserContext vsMonitorUserContext)
		{
			___(vsMonitorUserContext.CreateEmptyContext(out ppContext));
			string helpKeywordForCodeWindowTextView = GetHelpKeywordForCodeWindowTextView();
			___(ppContext.AddAttribute(VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_LookupF1, "keyword", helpKeywordForCodeWindowTextView));
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

			___((ppView as IVsProvideUserContext).GetUserContext(out var ppctx));

			if (vsUserContext != null)
				___(ppctx.AddSubcontext(vsUserContext, 500, out _));

			_IsHelpInitialized = true;
		}
	}

}
