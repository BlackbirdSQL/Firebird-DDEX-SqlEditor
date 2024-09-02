// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabbedEditorWindowPane

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using DpiAwareness = Microsoft.VisualStudio.Utilities.DpiAwareness;
using DpiAwarenessContext = Microsoft.VisualStudio.Utilities.DpiAwarenessContext;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;



namespace BlackbirdSql.Shared.Controls;


// =========================================================================================================
//
//											AbstractTabbedEditorPane Class
//
/// <summary>
/// Abstract base class for <see cref="TabbedEditorPane"/>.
/// </summary>
// =========================================================================================================
public abstract class AbstractTabbedEditorPane : WindowPane, IBsEditorPaneServiceProvider,
	IVsDesignerInfo, IVsWindowFrameNotify3, IVsMultiViewDocumentView,
	IVsHasRelatedSaveItems, IVsDocumentLockHolder, IVsBroadcastMessageEvents, IVsDocOutlineProvider,
	IVsDocOutlineProvider2, IVsToolboxActiveUserHook, IVsToolboxUser, IVsToolboxPageChooser,
	IVsDefaultToolboxTabState, IVsCodeWindow, IVsExtensibleObject
{

	// ----------------------------------------------------------------
	#region Constructors / Destructors - AbstractTabbedEditorPane
	// ----------------------------------------------------------------


	public AbstractTabbedEditorPane(System.IServiceProvider provider, IBsEditorPackage package, object docData,
		bool cloned, Guid toolbarGuid, uint toolbarID) : base(provider)
	{
		Diag.ThrowIfNotOnUIThread();

		_ExtensionInstance = package;
		_DocData = docData as IVsTextLines;
		_IsClone = cloned;

		_RequestedView = VSConstants.LOGVIEWID_Designer;

		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			_TabbedEditorUICtl = CreateTabbedEditorUIControl(toolbarGuid, toolbarID);
			_TabbedEditorUICtl.CreateControl();
		}
	}



	protected override void Dispose(bool disposing)
	{
		if (_IsDisposed || !disposing)
			return;

		try
		{
			ApcManager.OnElementValueChangedEvent -= OnElementValueChanged;

			if (_LockHolderCookie != 0)
			{
				RdtManager.UnregisterDocumentLockHolder(_LockHolderCookie);
				_LockHolderCookie = 0u;
			}

			_TabbedEditorUICtl?.Dispose();
			_TextEditor = null;
			_IsDisposed = true;
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
		/* if (ThreadHelper.CheckAccess() && _BroadcastMessageEventsCookie == 0
			&& Package.GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
		{
			___(vsShell.AdviseBroadcastMessages(this, out _BroadcastMessageEventsCookie));
		}
		*/

		ApcManager.OnElementValueChangedEvent += OnElementValueChanged;

		if (GetService(typeof(SVsTrackSelectionEx)) is IVsTrackSelectionEx vsTrackSelectionEx)
		{
			vsTrackSelectionEx.OnElementValueChange((uint)VSConstants.VSSELELEMID.SEID_PropertyBrowserSID, 1, null);
			vsTrackSelectionEx.OnElementValueChange((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, 1, null);
		}

		_ = GetService(typeof(SVsWindowFrame)) is IVsWindowFrame;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - AbstractTabbedEditorPane
	// =========================================================================================================


	public enum EnDocumentsFlag
	{
		DirtyDocuments,
		DirtyOrPrimary,
		DirtyExceptPrimary,
		AllDocuments
	}


	#endregion Constants





	// =========================================================================================================
	#region Fields - AbstractTabbedEditorPane
	// =========================================================================================================


	protected object _LockObject = new();

	private readonly IVsTextLines _DocData;
	private readonly IBsEditorPackage _ExtensionInstance;
	private bool _FirstTimeShowEventHandled;
	private bool _IsAppActivated = true;
	private readonly bool _IsClone = false;
	protected bool _IsDisposed = false;
	private bool _IsHelpInitialized;
	private bool _IsLoading;
	private bool _IsClosing;
	private bool _IsInUpdateCmdUIContext;
	private uint _LockHolderCookie;
	private IList<uint> _OverrideSaveDocCookieList;
	private Guid _RequestedView;
	private readonly EditorUIControl _TabbedEditorUICtl;
	private IBsTextEditor _TextEditor;
	private static CommandMapper _CmdMapper;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractTabbedEditorPane
	// =========================================================================================================


	AbstruseEditorTab IBsEditorPaneServiceProvider.ActiveTab => _TabbedEditorUICtl.TopEditorTab;

	public AuxilliaryDocData AuxDocData => _ExtensionInstance.GetAuxilliaryDocData(DocData);

	public virtual IVsTextLines DocData => _DocData;

	public string DocumentMoniker => GetDocumentMoniker(Frame);

	public IBsEditorPackage ExtensionInstance => _ExtensionInstance;

	public IVsWindowFrame Frame => GetService(typeof(SVsWindowFrame)) as IVsWindowFrame;

	public bool IsClone => _IsClone;

	public bool IsToolboxInitialized => ApcManager.IsToolboxInitialized;


	public uint PrimaryCookie
	{
		get
		{
			IVsWindowFrame frame = Frame;

			if (frame == null)
				return 0u;

			return GetPrimaryCookie(frame);
		}
	}


	protected QueryManager QryMgr => AuxDocData?.QryMgr;


	IBsTextEditor IBsEditorPaneServiceProvider.TextEditor
	{
		get { return _TextEditor; }
		set { _TextEditor = value; }
	}


	public EditorUIControl TabbedEditorUiCtl => _TabbedEditorUICtl;

	public IVsWindowFrame TabFrame => Frame;


	public static CommandMapper CmdMapper =>
		_CmdMapper ??= new CommandMapper();


	public override IWin32Window Window => _TabbedEditorUICtl;


	private IVsCodeWindow XamlCodeWindow
	{
		get
		{
			IVsCodeWindow vsCodeWindow = null;
			EnsureTabs();
			AbstruseEditorTab editorTab = null;

			foreach (AbstruseEditorTab tab in _TabbedEditorUICtl.Tabs)
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


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractTabbedEditorPane
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);




	void IBsEditorPaneServiceProvider.Activate(Guid logicalView, EnTabViewMode mode)
	{
		ActivateView(ref logicalView, mode);
	}



	int IVsMultiViewDocumentView.ActivateLogicalView(ref Guid rguidLogicalView)
	{
		if (rguidLogicalView.Equals(VSConstants.LOGVIEWID_Any) || rguidLogicalView.Equals(VSConstants.LOGVIEWID_Primary))
		{
			return VSConstants.S_OK;
		}

		return ActivateView(ref rguidLogicalView, EnTabViewMode.Default);
	}



	public virtual void ActivateNextTab()
	{
		_TabbedEditorUICtl.SplitViewContainer.CycleToNextButton();
	}



	public virtual void ActivatePreviousTab()
	{
		_TabbedEditorUICtl.SplitViewContainer.CycleToPreviousButton();
	}



	private int ActivateView(ref Guid rguidLogicalView, EnTabViewMode mode)
	{
		Guid clsid = _RequestedView = GetTabLogicalView(rguidLogicalView);

		if (_TabbedEditorUICtl.Tabs.Count > 0)
		{
			foreach (AbstruseEditorTab tab in _TabbedEditorUICtl.Tabs)
			{
				if (tab.LogicalView.Equals(clsid))
				{
					TrackTabSwitches(track: false);
					_TabbedEditorUICtl.ActivateTab(tab, mode);
					TrackTabSwitches(track: true);
					break;
				}
			}
		}

		return VSConstants.S_OK;
	}



	private T ActiveTab<T>() where T : class
	{
		T val = null;

		if (_TabbedEditorUICtl != null)
		{
			AbstruseEditorTab activeTab = _TabbedEditorUICtl.ActiveTab;

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



	int IVsCodeWindow.Close()
	{
		// Tracer.Trace(GetType(), "Close()");

		return XamlCodeWindow?.Close() ?? VSConstants.E_NOTIMPL;
	}



	public async Task CloseCloneAsync()
	{
		lock (_LockObject)
		{
			if (_IsClosing)
				return;

			_IsClosing = true;
		}

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		Frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
	}



	int IVsDocumentLockHolder.CloseDocumentHolder(uint dwSaveOptions)
	{
		// Tracer.Trace(GetType(), "CloseDocumentHolder()");

		if (_LockHolderCookie != 0)
			RdtManager.UnregisterDocumentLockHolder(_LockHolderCookie);

		return VSConstants.S_OK;
	}



	protected abstract AbstruseEditorTab CreateEditorTab(AbstractTabbedEditorPane tabbedEditor, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType);



	protected AbstruseEditorTab CreateEditorTabWithButton(AbstractTabbedEditorPane tabbedEditor, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
	{
		switch (editorTabType)
		{
			case EnEditorTabType.TopDesign:
			case EnEditorTabType.BottomDesign:
				_TabbedEditorUICtl.SplitViewContainer.SplitterBar.EnsureButtonInDesignPane(logicalView);
				break;
			case EnEditorTabType.TopXaml:
			case EnEditorTabType.BottomXaml:
				_TabbedEditorUICtl.SplitViewContainer.SplitterBar.EnsureButtonInXamlPane(logicalView);
				break;
		}

		return CreateEditorTab(tabbedEditor, logicalView, editorLogicalView, editorTabType);
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



	private EditorUIControl CreateTabbedEditorUIControl(Guid toolbarGuid, uint toolbarId /*, string[] buttonTexts = null */)
	{
		return new EditorUIControl((IBsTabbedEditorPane)this, toolbarGuid, toolbarId);
	}



	private void EnsureTabs()
	{
		EnsureTabs(activateTextView: false);
	}


	public virtual bool EnsureTabs(bool activateTextView)
	{
		if (_TabbedEditorUICtl == null || _TabbedEditorUICtl.Tabs.Count > 0 || _IsLoading || _IsClosing || ApcManager.SolutionClosing)
		{
			return false;
		}

		try
		{
			_TabbedEditorUICtl.DesignerMessage = "";
			_IsLoading = true;
			GetService(typeof(SVsWindowFrame));
			IVsUIShell vsUIShell = GetService(typeof(IVsUIShell)) as IVsUIShell;
			vsUIShell?.SetWaitCursor();
			Guid guid = VSConstants.LOGVIEWID_TextView;
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

			_TabbedEditorUICtl.LoadWindowState("");
			_TabbedEditorUICtl.Update();
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

			AbstruseEditorTab editorTab = CreateEditorTabWithButton(this, VSConstants.LOGVIEWID_Designer, VSConstants.LOGVIEWID_Primary, editorTabType);
			AbstruseEditorTab editorTab2 = CreateEditorTabWithButton(this, VSConstants.LOGVIEWID_TextView, VSConstants.LOGVIEWID_Primary, editorTabType2);
			_TabbedEditorUICtl.Tabs.Add(editorTab);
			_TabbedEditorUICtl.Tabs.Add(editorTab2);

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

			if (_TabbedEditorUICtl != null)
				_TabbedEditorUICtl.DesignerMessage = null;
		}

		return true;
	}



	private void EnsureToolbarAssociatedWithTabs()
	{
		if (_TabbedEditorUICtl == null)
			return;

		IVsToolWindowToolbarHost vsToolbarHost = _TabbedEditorUICtl.GetVsToolbarHost();

		if (vsToolbarHost == null)
			return;

		foreach (AbstruseEditorTab tab in _TabbedEditorUICtl.Tabs)
		{
			tab.CurrentFrame?.SetProperty((int)__VSFPROPID.VSFPROPID_ToolbarHost, vsToolbarHost);
		}
	}



	private static IEnumerable<uint> EnumerateOpenedDocuments(IBsEditorPaneServiceProvider designerService, __VSRDTSAVEOPTIONS rdtSaveOptions)
	{
		EnDocumentsFlag enumerateDocumentsFlag = GetDesignerDocumentFlagFromSaveOption(rdtSaveOptions);

		return EnumerateOpenedDocuments(designerService, enumerateDocumentsFlag);
	}



	private static IEnumerable<uint> EnumerateOpenedDocuments(IBsEditorPaneServiceProvider designerService, EnDocumentsFlag requestedDocumentsFlag)
	{
		foreach (uint editableDocument in designerService.GetEditableDocuments())
		{
			if (TryGetDocDataFromCookie(editableDocument, out var docData))
			{
				bool isDirty = IsDirty(docData);
				bool isPrimary = editableDocument == designerService.PrimaryCookie;
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



	int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		int hresult = OnExec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

		if (hresult == VSConstants.S_OK)
			return hresult;

		CommandID cmdId = new (pguidCmdGroup, (int)nCmdID);

		if (CmdMapper.TryGetCommandHandler(GetType(), cmdId, out IBsCommandHandler commandHandler))
		{
			hresult = commandHandler.OnExec((IBsTabbedEditorPane)this, nCmdexecopt, pvaIn, pvaOut);

			return hresult;
		}

		if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97
			&& nCmdID == (uint)VSConstants.VSStd97CmdID.PaneActivateDocWindow && _IsLoading)
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



	/// <summary>
	/// IVsDesignerInfo.get_DesignerTechnology implementation 
	/// </summary>
	/// <param name="pbstrTechnology"></param>
	/// <returns></returns>
	int IVsDesignerInfo.get_DesignerTechnology(out string pbstrTechnology)
	{
		pbstrTechnology = "";

		return VSConstants.S_OK;
	}



	int IVsMultiViewDocumentView.GetActiveLogicalView(out Guid pguidLogicalView)
	{
		pguidLogicalView = Guid.Empty;

		return VSConstants.S_OK;
	}



	int IVsExtensibleObject.GetAutomationObject(string pszPropName, out object ppDisp)
	{
		return GetAutomationObjectImpl(pszPropName, out ppDisp);
	}



	private int GetAutomationObjectImpl(string pszPropName, out object ppDisp)
	{
		ppDisp = this;

		return VSConstants.S_OK;
	}



	int IVsCodeWindow.GetBuffer(out IVsTextLines ppBuffer)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetBuffer(out ppBuffer);

		ppBuffer = null;

		return VSConstants.E_NOTIMPL;
	}



	int IVsDefaultToolboxTabState.GetDefaultTabExpansion(string pszTabID, out int pfExpanded)
	{
		pfExpanded = 0;

		return VSConstants.S_OK;
	}



	private static EnDocumentsFlag GetDesignerDocumentFlagFromSaveOption(__VSRDTSAVEOPTIONS saveOption)
	{
		if ((saveOption & __VSRDTSAVEOPTIONS.RDTSAVEOPT_ForceSave) == 0)
			return EnDocumentsFlag.DirtyDocuments;

		return EnDocumentsFlag.DirtyOrPrimary;
	}



	protected string GetDocumentMoniker(IVsWindowFrame frame)
	{
		if (frame == null)
			return null;

		frame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out var pvar);

		return pvar as string;
	}



	public IEnumerable<uint> GetEditableDocuments()
	{
		return GetEditableDocumentsForTab();
	}



	private IEnumerable<uint> GetEditableDocumentsForTab()
	{
		uint primaryDocCookie = PrimaryCookie;

		if (primaryDocCookie != 0)
			yield return primaryDocCookie;
	}



	int IVsCodeWindow.GetEditorCaption(READONLYSTATUS dwReadOnly, out string pbstrEditorCaption)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetEditorCaption(dwReadOnly, out pbstrEditorCaption);

		pbstrEditorCaption = null;

		return VSConstants.E_NOTIMPL;
	}




	public virtual string GetHelpKeywordForCodeWindowTextView()
	{
		return "";
	}



	int IVsCodeWindow.GetLastActiveView(out IVsTextView ppView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetLastActiveView(out ppView);

		ppView = null;

		return VSConstants.E_NOTIMPL;
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



	int IVsToolboxPageChooser.GetPreferredToolboxPage(out Guid pguidPage)
	{
		pguidPage = Guid.Empty;

		return VSConstants.S_FALSE;
	}



	protected uint GetPrimaryCookie(IVsWindowFrame frame)
	{
		if (frame == null)
			return 0u;

		___(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out var pvar));

		return (uint)(int)pvar;
	}



	int IVsCodeWindow.GetPrimaryView(out IVsTextView ppView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetPrimaryView(out ppView);

		ppView = null;

		return VSConstants.E_NOTIMPL;
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



	int IVsCodeWindow.GetSecondaryView(out IVsTextView ppView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetSecondaryView(out ppView);

		ppView = null;

		return VSConstants.E_NOTIMPL;
	}



	protected override object GetService(Type serviceType)
	{
		if (serviceType == null)
			Diag.ThrowException(new ArgumentNullException("serviceType"));

		if (serviceType == typeof(IOleCommandTarget))
			return this;

		if (serviceType == typeof(IBsEditorPaneServiceProvider))
			return this;

		return base.GetService(serviceType);
	}



	public AbstruseEditorTab GetTab(ref Guid rguidLogicalView)
	{
		Guid tabLogicalView = GetTabLogicalView(rguidLogicalView);

		if (_TabbedEditorUICtl.Tabs.Count == 0)
			return null;

		foreach (AbstruseEditorTab tab in _TabbedEditorUICtl.Tabs)
		{
			if (tab.LogicalView.Equals(tabLogicalView))
				return tab;
		}

		return null;
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




	int IVsCodeWindow.GetViewClassID(out Guid pclsidView)
	{
		IVsCodeWindow xamlCodeWindow = XamlCodeWindow;

		if (xamlCodeWindow != null)
			return xamlCodeWindow.GetViewClassID(out pclsidView);

		pclsidView = Guid.Empty;

		return VSConstants.E_NOTIMPL;
	}



	private int HandleCloseEditorOrDesigner(AuxilliaryDocData auxDocData)
	{
		// Tracer.Trace(GetType(), "HandleCloseEditorOrDesigner()");

		if (!auxDocData.RequestDeactivateQuery())
			return VSConstants.OLE_E_PROMPTSAVECANCELLED;

		return VSConstants.S_OK;
	}



	int IVsToolboxActiveUserHook.InterceptDataObject(Microsoft.VisualStudio.OLE.Interop.IDataObject pIn, out Microsoft.VisualStudio.OLE.Interop.IDataObject ppOut)
	{
		IVsToolboxActiveUserHook vsToolboxActiveUserHook = ActiveTab<IVsToolboxActiveUserHook>();

		if (vsToolboxActiveUserHook != null)
			return vsToolboxActiveUserHook.InterceptDataObject(pIn, out ppOut);

		ppOut = null;

		return VSConstants.E_NOTIMPL;
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



	int IVsMultiViewDocumentView.IsLogicalViewActive(ref Guid rguidLogicalView, out int pIsActive)
	{
		Guid tabLogicalView = GetTabLogicalView(rguidLogicalView);
		AbstruseEditorTab activeTab = _TabbedEditorUICtl.ActiveTab;

		if (activeTab != null)
			pIsActive = activeTab.LogicalView.Equals(tabLogicalView) ? 1 : 0;
		else
			pIsActive = _RequestedView.Equals(tabLogicalView) ? 1 : 0;

		return VSConstants.S_OK;
	}



	int IVsToolboxUser.IsSupported(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		return ActiveTab<IVsToolboxUser>()?.IsSupported(pDO) ?? VSConstants.E_NOTIMPL;
	}



	bool IBsEditorPaneServiceProvider.IsTabVisible(Guid logicalView)
	{
		foreach (AbstruseEditorTab tab in _TabbedEditorUICtl.Tabs)
		{
			if (tab.LogicalView == logicalView)
				return tab.IsOnScreen;
		}

		return false;
	}



	int IVsToolboxUser.ItemPicked(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		return ActiveTab<IVsToolboxUser>()?.ItemPicked(pDO) ?? VSConstants.E_NOTIMPL;
	}



	public void LoadDesignerPane(AbstruseEditorTab designerTab, bool asPrimary, bool showSplitter)
	{
		if (asPrimary)
		{
			if (_TabbedEditorUICtl.TopEditorTab == null)
				_TabbedEditorUICtl.TopEditorTab = designerTab;
			else
				designerTab.EnsureFrameCreated();
		}
		else if (_TabbedEditorUICtl.BottomEditorTab == null)
		{
			_TabbedEditorUICtl.BottomEditorTab = designerTab;
		}
		else
		{
			designerTab.EnsureFrameCreated();
		}
	}



	public void LoadXamlPane(AbstruseEditorTab xamlTab, bool asPrimary, bool showSplitter)
	{
		if (asPrimary)
		{
			if (_TabbedEditorUICtl.TopEditorTab == null)
				_TabbedEditorUICtl.TopEditorTab = xamlTab;
			else
				xamlTab.EnsureFrameCreated();
		}
		else if (showSplitter)
		{
			if (_TabbedEditorUICtl.BottomEditorTab == null)
				_TabbedEditorUICtl.BottomEditorTab = xamlTab;
			else
				xamlTab.EnsureFrameCreated();
		}

		xamlTab.CreateTextEditor();
	}



	int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		int hresult = OnQueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

		if (hresult == VSConstants.S_OK)
			return hresult;

		for (int i = 0; i < cCmds; i++)
		{
			CommandID cmdId = new (pguidCmdGroup, (int)prgCmds[i].cmdID);

			if (CmdMapper.TryGetCommandHandler(GetType(), cmdId, out IBsCommandHandler commandHandler))
			{
				return commandHandler.OnQueryStatus((IBsTabbedEditorPane)this, ref prgCmds[i], pCmdText);
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



	int IVsDocOutlineProvider.ReleaseOutline(IntPtr hwnd, IOleCommandTarget pCmdTarget)
	{
		return ActiveTab<IVsDocOutlineProvider>()?.ReleaseOutline(hwnd, pCmdTarget) ?? VSConstants.E_NOTIMPL;
	}



	protected virtual int SaveFiles(ref uint saveFlags)
	{
		// Tracer.Trace(GetType(), "SaveFiles()");

		if (VsShellUtilities.IsInAutomationFunction(this))
			saveFlags = (uint)__FRAMECLOSE.FRAMECLOSE_NoSave;

		int hresult = VSConstants.S_OK;

		if (saveFlags != (uint)__FRAMECLOSE.FRAMECLOSE_NoSave)
		{
			if (Frame == null)
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

			uint primaryDocCookie = PrimaryCookie;

			hresult = RdtManager.SaveDocuments(saveOpts, null, uint.MaxValue, primaryDocCookie);

			if (__(hresult))
				saveFlags = (uint)__FRAMECLOSE.FRAMECLOSE_NoSave;
		}

		return hresult;
	}



	int IVsCodeWindow.SetBaseEditorCaption(string[] pszBaseEditorCaption)
	{
		return XamlCodeWindow?.SetBaseEditorCaption(pszBaseEditorCaption) ?? VSConstants.E_NOTIMPL;
	}



	int IVsCodeWindow.SetBuffer(IVsTextLines pBuffer)
	{
		return XamlCodeWindow?.SetBuffer(pBuffer) ?? VSConstants.E_NOTIMPL;
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



	int IVsCodeWindow.SetViewClassID(ref Guid clsidView)
	{
		return XamlCodeWindow?.SetViewClassID(ref clsidView) ?? VSConstants.E_NOTIMPL;
	}



	int IVsDocumentLockHolder.ShowDocumentHolder()
	{
		return VSConstants.S_OK;
	}



	public void SuppressChangeTracking(bool suppress)
	{
		___(Frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar));

		if (pvar is not IVsCodeWindow codeWindow)
			throw Diag.ExceptionInstance(typeof(IVsCodeWindow));

		RdtManager.SuppressChangeTracking(null, codeWindow, suppress);
	}


	int IVsToolboxActiveUserHook.ToolboxSelectionChanged(Microsoft.VisualStudio.OLE.Interop.IDataObject pSelected)
	{
		return ActiveTab<IVsToolboxActiveUserHook>()?.ToolboxSelectionChanged(pSelected) ?? VSConstants.E_NOTIMPL;
	}



	private void TrackTabSwitches(bool track)
	{
		if (track)
			_TabbedEditorUICtl.TabActivatedEvent += OnTabActivated;
		else
			_TabbedEditorUICtl.TabActivatedEvent -= OnTabActivated;
	}



	int IVsDocOutlineProvider2.TranslateAccelerator(MSG[] lpMsg)
	{
		return ActiveTab<IVsDocOutlineProvider2>()?.TranslateAccelerator(lpMsg) ?? -2147467259;
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
			if (_TabbedEditorUICtl == null)
				return;

			AbstruseEditorTab activeTab = _TabbedEditorUICtl.ActiveTab;

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


	protected virtual async Task<bool> UpdateTabsButtonTextAsync(ExecutionCompletedEventArgs args)
	{
		return await Task.FromResult(!args.SyncToken.Cancelled() && _TabbedEditorUICtl != null
			&& _TabbedEditorUICtl.Tabs.Count > 0 && !_IsClosing && !ApcManager.SolutionClosing);
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstractTabbedEditorPane
	// =========================================================================================================



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



	protected override void OnClose()
	{
		// Tracer.Trace(GetType(), "OnClose()");

		_IsClosing = true;

		base.OnClose();
	}



	int IVsWindowFrameNotify3.OnClose(ref uint frameSaveOptions)
	{
		// Tracer.Trace(GetType(), "OnClose()");

		if (IsClone && (frameSaveOptions & (uint)__FRAMECLOSE.FRAMECLOSE_NoSave) > 0)
		{
			AuxDocData.CloneRemove();
			return VSConstants.S_OK;
		}

		IEnumerable<uint> enumerable = null;

		if ((frameSaveOptions & (uint)__FRAMECLOSE.FRAMECLOSE_NoSave) > 0)
		{
			enumerable = EnumerateOpenedDocuments(this, EnDocumentsFlag.DirtyExceptPrimary);

			if (enumerable.Any())
				frameSaveOptions = (uint)__FRAMECLOSE.FRAMECLOSE_PromptSave;
		}

		AuxilliaryDocData auxDocData = null;

		if ((frameSaveOptions & (uint)__FRAMECLOSE.FRAMECLOSE_NoSave) > 0)
		{
			auxDocData = AuxDocData;

			if (!((auxDocData?.QryMgr?.LiveTransactions) ?? false))
			{
				return VSConstants.S_FALSE;
			}

			frameSaveOptions = (uint)__FRAMECLOSE.FRAMECLOSE_PromptSave;
		}



		bool keepDocAlive = false;
		uint primaryCookie = PrimaryCookie;
		List<uint> saveCookieList = [];

		enumerable ??= EnumerateOpenedDocuments(this, EnDocumentsFlag.DirtyExceptPrimary);
		List<uint> cookieList = new List<uint>(enumerable) { primaryCookie };

		foreach (uint cookie in cookieList)
		{
			if (RdtManager.ShouldKeepDocDataAliveOnClose(cookie))
				keepDocAlive = true;
			else
				saveCookieList.Add(cookie);
		}


		int hresult = VSConstants.S_FALSE;

		if (keepDocAlive)
		{
			if (saveCookieList.Count == 0)
			{
				hresult = VSConstants.S_OK;
			}
			else
			{
				_OverrideSaveDocCookieList = saveCookieList;

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


		if (!__(hresult))
			return hresult;

		hresult = HandleCloseEditorOrDesigner(auxDocData ?? AuxDocData);

		if (__(hresult))
			_IsClosing = true;

		return hresult;
	}



	protected override void OnCreate()
	{
		RdtManager.RegisterDocumentLockHolder(0u, PrimaryCookie, this, out _LockHolderCookie);
	}



	int IVsWindowFrameNotify3.OnDockableChange(int fDockable, int x, int y, int w, int h)
	{
		return VSConstants.S_OK;
	}



	private int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
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

			AbstruseEditorTab editorTab = _TabbedEditorUICtl.ActiveTab;

			if (vsWindowFrame2 == tabFrame)
			{
				_TabbedEditorUICtl.BeginInvoke(new MethodInvoker(UpdateCmdUIContext));
			}
			else if (vsWindowFrame2 != null)
			{
				bool flag = false;

				foreach (AbstruseEditorTab tab in _TabbedEditorUICtl.Tabs)
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
					foreach (AbstruseEditorTab tab2 in _TabbedEditorUICtl.Tabs)
					{
						if (tab2 == editorTab)
						{
							_TabbedEditorUICtl.ActivateTab(tab2, EnTabViewMode.Default);
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



	public virtual int OnExec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97
			&& nCmdID == (uint)VSConstants.VSStd97CmdID.F1Help)
		{
			SetupF1Help();

			return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
		}

		return VSConstants.E_NOTIMPL;
	}



	protected async Task<bool> OnExecutionCompletedAsync(object sender, ExecutionCompletedEventArgs args)
	{
		if (!args.Launched)
			return true;

		bool result = true;

		try
		{
			await UpdateTabsButtonTextAsync(args);
		}
		catch (Exception ex)
		{
			result = false;
			Diag.Dug(ex);
		}

		args.Result &= result;

		return result;
	}



	int IVsWindowFrameNotify3.OnMove(int x, int y, int w, int h)
	{
		return VSConstants.S_OK;
	}



	int IVsDocOutlineProvider.OnOutlineStateChange(uint dwMask, uint dwState)
	{
		return ActiveTab<IVsDocOutlineProvider>()?.OnOutlineStateChange(dwMask, dwState) ?? 0;
	}



	public virtual int OnQueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		return VSConstants.E_NOTIMPL;
	}



	int IVsWindowFrameNotify3.OnShow(int fShow)
	{
		__FRAMESHOW fShowStatus = (__FRAMESHOW)fShow;

		switch (fShowStatus)
		{
			case __FRAMESHOW.FRAMESHOW_WinShown:
				using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
				{
					EnsureTabs();
					EnsureToolbarAssociatedWithTabs();
					RaiseShow(fShowStatus);
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
				RaiseShow(fShowStatus);

				foreach (AbstruseEditorTab tab in _TabbedEditorUICtl.Tabs)
					((IVsWindowFrameNotify3)tab).OnShow(fShow);

				break;

			case __FRAMESHOW.FRAMESHOW_TabDeactivated:
				RaiseShow(fShowStatus);


				foreach (AbstruseEditorTab tab in _TabbedEditorUICtl.Tabs)
					((IVsWindowFrameNotify3)tab).OnShow(fShow);

				break;
		}

		return VSConstants.S_OK;
	}




	int IVsWindowFrameNotify3.OnSize(int x, int y, int w, int h)
	{
		return VSConstants.S_OK;
	}



	private void OnTabActivated(object sender, EventArgs e)
	{
		_RequestedView = Guid.Empty;

		if (Frame == null)
			return;

		Guid rguid = Guid.Empty;

		if (sender is AbstruseEditorTab editorTab)
		{
			if (editorTab.EditorTabType != EnEditorTabType.TopXaml)
				_ = editorTab.EditorTabType;

			rguid = editorTab.CommandUIGuid;
		}

		Frame.SetGuidProperty((int)__VSFPROPID.VSFPROPID_CmdUIGuid, ref rguid);
	}



	private void OnToolboxItemPicked(object sender, ToolboxEventArgs e)
	{
		if (_TabbedEditorUICtl != null && _TabbedEditorUICtl.IsSplitterVisible)
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



	protected virtual void RaiseShow(__FRAMESHOW fShow)
	{
	}


	#endregion Event Handling
}
