// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.EditorTab

using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Controls.Tabs;



public abstract class AbstractEditorTab : IDisposable, IVsDesignerInfo, IVsMultiViewDocumentView,
	IVsWindowFrameNotify3, IOleCommandTarget, IVsToolboxActiveUserHook, IVsToolboxPageChooser,
	IVsDefaultToolboxTabState, IVsToolboxUser
{
	public AbstractEditorTab(AbstractTabbedEditorWindowPane editorPane, Guid logicalView, EnEditorTabType editorTabType)
	{
		_CmdTarget = editorPane;
		_WindowPaneServiceProvider = editorPane;
		_LogicalView = logicalView;
		_EditorTabType = editorTabType;
		_TabbedEditorPane = editorPane;
	}


	protected IOleCommandTarget _CmdTarget;
	private readonly System.IServiceProvider _WindowPaneServiceProvider;
	private Guid _LogicalView;
	private EnEditorTabType _EditorTabType;
	private readonly AbstractTabbedEditorWindowPane _TabbedEditorPane;



	protected IVsWindowFrame _CurrentFrame;
	protected TextEditor _TextEditor;
	private IWin32Window _Owner;
	private ISelectionContainer _SavedSelection;
	private bool _Showing;
	private bool _Visible;
	private bool _IsActive;
	private Rectangle _Bounds;
	private Panel _ParentPanel;
	private ITrackSelection _TrackSelection;
	private ICollection _PropertyWindowSelectedObjects;




	public Guid CommandUIGuid
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			Guid pguid = Guid.Empty;
			_CurrentFrame?.GetGuidProperty((int)__VSFPROPID.VSFPROPID_CmdUIGuid, out pguid);
			return pguid;
		}
	}

	protected abstract Guid ClsidEditorFactory { get; }


	public IVsWindowFrame CurrentFrame => _CurrentFrame;


	protected System.IServiceProvider WindowPaneServiceProvider => _WindowPaneServiceProvider;

	protected AbstractTabbedEditorWindowPane TabbedEditorPane => _TabbedEditorPane;

	protected string DocumentMoniker
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			IVsWindowFrame vsWindowFrame = _CurrentFrame ?? WindowPaneServiceProvider.GetService(typeof(SVsWindowFrame)) as IVsWindowFrame;

			if (!(vsWindowFrame != null))
				return null;

			vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out var pvar);
			return (string)pvar;
		}
	}

	public EnEditorTabType EditorTabType => _EditorTabType;

	protected IVsWindowFrame Frame
	{
		get
		{
			EnsureFrameCreated();
			return _CurrentFrame;
		}
	}

	public ITrackSelection TrackSelection
	{
		get
		{
			Diag.ThrowIfNotOnUIThread();

			if (_TrackSelection == null && GetView() is System.IServiceProvider serviceProvider)
			{
				_TrackSelection = serviceProvider.GetService(typeof(STrackSelection)) as ITrackSelection;
			}
			return _TrackSelection;
		}
	}

	public ICollection PropertyWindowSelectedObjects
	{
		get
		{
			return _PropertyWindowSelectedObjects;
		}
		set
		{
			if (TrackSelection != null)
			{
				Diag.ThrowIfNotOnUIThread();

				SelectionContainer selectionContainer = new SelectionContainer(selectableReadOnly: true, selectedReadOnly: false)
				{
					SelectedObjects = value,
					SelectableObjects = value
				};
				try
				{
					TrackSelection.OnSelectChange(selectionContainer);
					_PropertyWindowSelectedObjects = value;
				}
				catch
				{
				}
			}
		}
	}

	public Panel ParentPanel
	{
		get
		{
			if (_ParentPanel == null)
			{
				_ParentPanel = new Panel
				{
					Name = "_ParentPanel for " + GetType().Name,
					BackColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOW),
					Height = 0,
					Width = 0
				};
				_ParentPanel.SizeChanged += ((AbstractTabbedEditorUIControl)Owner).Panel_SizeChanged;
			}
			return _ParentPanel;
		}
	}

	public bool IsActive => _IsActive;

	public bool IsTopTab
	{
		get
		{
			if (_EditorTabType != EnEditorTabType.TopXaml)
			{
				return _EditorTabType == EnEditorTabType.TopDesign;
			}
			return true;
		}
		set
		{
			if (IsTopTab != value)
			{
				switch (_EditorTabType)
				{
					case EnEditorTabType.TopXaml:
						_EditorTabType = EnEditorTabType.BottomXaml;
						break;
					case EnEditorTabType.BottomXaml:
						_EditorTabType = EnEditorTabType.TopXaml;
						break;
					case EnEditorTabType.TopDesign:
						_EditorTabType = EnEditorTabType.BottomDesign;
						break;
					case EnEditorTabType.BottomDesign:
						_EditorTabType = EnEditorTabType.TopDesign;
						break;
				}
			}
		}
	}

	public bool IsShowing => _Showing;

	public bool IsVisible => _Visible;

	public bool IsClosed { get; private set; }

	public bool TabButtonVisible
	{
		get
		{
			return (TabbedEditorPane.Window as AbstractTabbedEditorUIControl).SplitViewContainer.SplitterBar.GetTabButtonVisibleStatus(LogicalView);
		}
		set
		{
			(TabbedEditorPane.Window as AbstractTabbedEditorUIControl).SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(LogicalView, value);
		}
	}

	public bool IsOnScreen
	{
		get
		{
			if ((_Showing || _Visible) && !_Bounds.IsEmpty && _Bounds.Width > 1 && _Bounds.Height > 1)
			{
				return true;
			}
			return false;
		}
	}

	public Guid LogicalView => _LogicalView;

	public IWin32Window Owner
	{
		get
		{
			return _Owner;
		}
		set
		{
			Diag.ThrowIfNotOnUIThread();

			_Owner = value;
			_CurrentFrame?.SetProperty((int)__VSFPROPID2.VSFPROPID_ParentHwnd, _Owner.Handle);
		}
	}

	public TextEditor TextEditor
	{
		get
		{
			if (_TextEditor == null && (EditorTabType == EnEditorTabType.TopXaml || EditorTabType == EnEditorTabType.BottomXaml))
			{
				CreateTextEditor();
			}
			return _TextEditor;
		}
	}

	protected virtual bool AllowBrowseToFileInExplorer => new Uri(DocumentMoniker, UriKind.Absolute).IsFile;

	public event EventHandler ActiveChangedEvent;

	public event EventHandler ShownEvent;

	public event EventHandler<ToolboxEventArgs> ToolboxItemPickedEvent;


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);


	protected Panel GetPanelForCurrentFrame()
	{
		Diag.ThrowIfNotOnUIThread();

		___(CurrentFrame.GetProperty((int)__VSFPROPID2.VSFPROPID_ParentHwnd, out var pvar));

		return Control.FromHandle((IntPtr)(int)pvar) as Panel;
	}

	public void EnsureFrameCreated()
	{
		if (_CurrentFrame == null)
		{
			using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(Microsoft.VisualStudio.Utilities.DpiAwarenessContext.SystemAware))
			{
				_CurrentFrame = CreateWindowFrame();
			}
		}
	}

	public void Activate(bool setFocus = true)
	{
		if (_CurrentFrame != null && IsVisible)
		{
			UpdateActive(isActive: true);
			if (setFocus)
			{
				Diag.ThrowIfNotOnUIThread();

				((IVsWindowFrame2)Frame).ActivateOwnerDockedWindow();
			}
		}
	}

	public void CreateTextEditor()
	{
		_TextEditor = _CmdTarget as TextEditor;

		if (_TextEditor != null)
			return;

		Diag.ThrowIfNotOnUIThread();

		if (_CurrentFrame == null)
		{
			using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(Microsoft.VisualStudio.Utilities.DpiAwarenessContext.SystemAware))
			{
				_CurrentFrame = CreateWindowFrame();
			}
		}

		object view = GetView();

		if (view is IVsCodeWindow vsCodeWindow && view is IOleCommandTarget oleCommandTarget && view is Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider && _TextEditor == null)
		{
			_TextEditor = new TextEditor(serviceProvider, _WindowPaneServiceProvider, vsCodeWindow, oleCommandTarget);
			_CmdTarget = _TextEditor;
		}
	}

	protected abstract IVsWindowFrame CreateWindowFrame();

	protected void SetFrameProperties(IVsWindowFrame parentFrame, IVsWindowFrame frame)
	{
		if (parentFrame != frame)
			frame.SetProperty((int)__VSFPROPID2.VSFPROPID_ParentFrame, parentFrame);

		frame.SetProperty((int)__VSFPROPID.VSFPROPID_ViewHelper, this);
		frame.SetProperty((int)__VSFPROPID3.VSFPROPID_NotifyOnActivate, true);

		Guid rguid = ClsidEditorFactory;
		frame.SetGuidProperty((int)__VSFPROPID.VSFPROPID_guidEditorType, ref rguid);

		// TODO: Added to test CmdUI.
		Guid rCmdUIGuid = VSConstants.GUID_TextEditorFactory;
		frame.SetGuidProperty((int)__VSFPROPID.VSFPROPID_CmdUIGuid, ref rCmdUIGuid);

		string physicalViewString = GetPhysicalViewString();
		frame.SetProperty((int)__VSFPROPID.VSFPROPID_pszPhysicalView, physicalViewString);

		if (_Owner != null)
		{
			ParentPanel.SuspendLayout();

			try
			{
				Panel panel = new Panel
				{
					Name = "EditorTabFrameWrapper",
					Dock = DockStyle.Fill,
					Height = 0,
					Width = 0,
					BackColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOW)
				};
				frame.SetProperty((int)__VSFPROPID2.VSFPROPID_ParentHwnd, panel.Handle);
				ParentPanel.Controls.Add(panel);
				panel.BringToFront();
			}
			finally
			{
				ParentPanel.ResumeLayout();
			}
		}
	}

	protected virtual string GetPhysicalViewString()
	{
		return string.Empty;
	}

	public virtual IVsFindTarget GetFindTarget()
	{
		return null;
	}

	public void Dispose()
	{
		if (_CurrentFrame != null)
		{
			Diag.ThrowIfNotOnUIThread();

			try
			{
				_CmdTarget = null;
				_CurrentFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
				_CurrentFrame = null;
			}
			catch (Exception ex)
			{
				Tracer.Warning(GetType(), "Dispose()", "Exception encountered while Disposing AbstractEditorTab. CloseFrame threw: {0}.", ex.Message);
			}
		}
		if (_ParentPanel != null)
		{
			_ParentPanel.SuspendLayout();
			_ParentPanel.SizeChanged -= ((AbstractTabbedEditorUIControl)Owner).Panel_SizeChanged;
			_ParentPanel.Dispose();
		}
		if (_TextEditor != null)
		{
			_TextEditor.Dispose();
			_TextEditor = null;
		}
		ShownEvent = null;
	}

	public void Hide()
	{
		// Tracer.Trace(GetType(), "Hide()", "Guid: {0}.", _LogicalView);

		_Visible = false;

		if (_CurrentFrame == null)
			return;

		Diag.ThrowIfNotOnUIThread();

		try
		{
			_SavedSelection = null;
			if (__(_CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out var pvar)))
			{
				using ServiceProvider serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)pvar);
				if (serviceProvider.GetService(typeof(SVsTrackSelectionEx)) is IVsTrackSelectionEx vsTrackSelectionEx
					&& __(vsTrackSelectionEx.GetCurrentSelection(out var ppHier, out var _, out var _, out var ppSC))
					&& ppSC != IntPtr.Zero)
				{
					try
					{
						_SavedSelection = (ISelectionContainer)Marshal.GetObjectForIUnknown(ppSC);
					}
					finally
					{
						if (ppHier != IntPtr.Zero)
						{
							Marshal.Release(ppHier);
						}
						if (ppSC != IntPtr.Zero)
						{
							Marshal.Release(ppSC);
						}
					}
				}
			}
			_CurrentFrame.Hide();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}

	public object GetView()
	{
		if (_CurrentFrame != null)
		{
			Diag.ThrowIfNotOnUIThread();

			_CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar);

			return pvar;
		}
		return null;
	}

	public void UpdateActive(bool isActive)
	{
		if (_IsActive != isActive)
		{
			_IsActive = isActive;
			if (TextEditor != null)
				TextEditor.IsActive = _IsActive;
			ActiveChangedEvent?.Invoke(this, EventArgs.Empty);
		}
	}

	public void SetBounds(Rectangle bounds)
	{
		if (_CurrentFrame != null)
		{
			_Bounds = bounds;
			Panel panelForCurrentFrame = GetPanelForCurrentFrame();
			panelForCurrentFrame.Bounds = bounds;
			Guid rguidRelativeTo = Guid.Empty;

			if (panelForCurrentFrame.Width >= 0 && panelForCurrentFrame.Height >= 0)
			{
				Diag.ThrowIfNotOnUIThread();

				_CurrentFrame.SetFramePos((VSSETFRAMEPOS)(-1073741824), ref rguidRelativeTo, 0, 0, panelForCurrentFrame.Width, panelForCurrentFrame.Height);
			}
			else
			{
				Tracer.Warning(GetType(), "SetBounds()", "Panel width and height must not be negative");
			}
		}
	}

	public void Show()
	{
		Diag.ThrowIfNotOnUIThread();

		// Tracer.Trace(GetType(), "Show()", "Guid: {0}.", _LogicalView);

		try
		{
			_Showing = true;
			Frame.ShowNoActivate();
			_Showing = false;
			_Visible = true;

			((IVsWindowFrameNotify3)this).OnShow(1);
			if (GetView() is IOleCommandTarget cmdTarget)
			{
				_CmdTarget = cmdTarget;
			}
			if (_SavedSelection != null && __(_CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out var pvar)))
			{
				using ServiceProvider serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)pvar);
				(serviceProvider.GetService(typeof(SVsTrackSelectionEx)) as IVsTrackSelectionEx).OnSelectChange(_SavedSelection);
				_SavedSelection = null;
			}

			int hr = _CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out var pvar2);
			if (!__(hr))
				return;

			ServiceProvider serviceProvider2 = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)pvar2);
			using (serviceProvider2)
			{
				IVsHierarchy o = null;
				uint itemid = 0u;

				hr = _CurrentFrame.GetProperty((int)VsFramePropID.Hierarchy, out var pvar3);

				if (__(hr))
				{
					o = (IVsHierarchy)pvar3;
					hr = _CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out var pvar4);

					if (__(hr))
						itemid = (uint)(int)pvar4;
				}

				if (!__(hr) || serviceProvider2.GetService(typeof(SVsTrackSelectionEx)) is not IVsTrackSelectionEx vsTrackSelectionEx)
				{
					return;
				}

				IntPtr pSC = (IntPtr)(-1);
				IntPtr ppv = IntPtr.Zero;
				IntPtr intPtrUnknown = Marshal.GetIUnknownForObject(o);
				try
				{
					Guid iid = typeof(IVsHierarchy).GUID;
					hr = Marshal.QueryInterface(intPtrUnknown, ref iid, out ppv);

					if (__(hr))
					{
						try
						{
							vsTrackSelectionEx.OnSelectChangeEx(ppv, itemid, null, pSC);
							return;
						}
						finally
						{
							Marshal.Release(ppv);
						}
					}
				}
				finally
				{
					Marshal.Release(intPtrUnknown);
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

	}

	int IVsMultiViewDocumentView.ActivateLogicalView(ref Guid rguidLogicalView)
	{
		Diag.ThrowIfNotOnUIThread();

		if (_WindowPaneServiceProvider is IVsMultiViewDocumentView vsMultiViewDocumentView)
		{
			return vsMultiViewDocumentView.ActivateLogicalView(ref rguidLogicalView);
		}

		return VSConstants.E_NOTIMPL;
	}

	int IVsMultiViewDocumentView.GetActiveLogicalView(out Guid pguidLogicalView)
	{
		Diag.ThrowIfNotOnUIThread();

		if (_WindowPaneServiceProvider is IVsMultiViewDocumentView vsMultiViewDocumentView)
		{
			return vsMultiViewDocumentView.GetActiveLogicalView(out pguidLogicalView);
		}
		pguidLogicalView = Guid.Empty;
		return VSConstants.E_NOTIMPL;
	}

	int IVsMultiViewDocumentView.IsLogicalViewActive(ref Guid rguidLogicalView, out int pIsActive)
	{
		Diag.ThrowIfNotOnUIThread();

		if (_WindowPaneServiceProvider is IVsMultiViewDocumentView vsMultiViewDocumentView)
		{
			return vsMultiViewDocumentView.IsLogicalViewActive(ref rguidLogicalView, out pIsActive);
		}
		pIsActive = 0;

		return VSConstants.E_NOTIMPL;
	}

	int IVsWindowFrameNotify3.OnClose(ref uint pgrfSaveOptions)
	{
		// Tracer.Trace(GetType(), "OnClose()");

		IsClosed = true;
		if (_CurrentFrame != null)
		{
			Diag.ThrowIfNotOnUIThread();

			int hresult = _CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar);

			if (__(hresult))
			{
				if (pvar is IVsWindowFrameNotify3 vsWindowFrameNotify)
				{
					uint pgrfSaveOptions2 = pgrfSaveOptions;
					hresult = vsWindowFrameNotify.OnClose(ref pgrfSaveOptions2);
				}
				else if (pvar is IVsWindowFrameNotify2 vsWindowFrameNotify2)
				{
					uint pgrfSaveOptions2 = pgrfSaveOptions;
					hresult = vsWindowFrameNotify2.OnClose(ref pgrfSaveOptions2);
				}
			}

			return hresult;
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
		if (fShow == 1 && ShownEvent != null)
			ShownEvent(this, EventArgs.Empty);

		if (_CurrentFrame == null)
			return VSConstants.S_OK;

		Diag.ThrowIfNotOnUIThread();

		int hresult = _CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar);
		AbstractTabbedEditorWindowPane AbstractTabbedEditorPane = pvar as AbstractTabbedEditorWindowPane;

		if (__(hresult) && AbstractTabbedEditorPane == null)
		{
			if (pvar is IVsWindowFrameNotify3 vsWindowFrameNotify)
			{
				hresult = vsWindowFrameNotify.OnShow(fShow);
			}
			else if (pvar is IVsWindowFrameNotify vsWindowFrameNotify2)
			{
				hresult = vsWindowFrameNotify2.OnShow(fShow);
			}
		}
		return hresult;
	}

	int IVsWindowFrameNotify3.OnSize(int x, int y, int w, int h)
	{
		return 0;
	}

	int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		int num = _TabbedEditorPane.HandleExec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

		if (num == 0)
			return num;

		if (AbstractTabbedEditorWindowPane.ToolbarManager.TryGetCommandHandler(_TabbedEditorPane.GetType(),
			new GuidId(pguidCmdGroup, nCmdID), out var commandHandler))
		{
			return commandHandler.HandleExec(_TabbedEditorPane, nCmdexecopt, pvaIn, pvaOut);
		}
		if (_CmdTarget == null || _CmdTarget is AbstractTabbedEditorWindowPane)
			return 0;

		Diag.ThrowIfNotOnUIThread();

		return _CmdTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
	}

	int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		int hresult = _TabbedEditorPane.HandleQueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

		if (hresult == VSConstants.S_OK)
			return hresult;

		hresult = (int)Microsoft.VisualStudio.OLE.Interop.Constants.MSOCMDERR_E_NOTSUPPORTED;

		for (int i = 0; i < prgCmds.Length; i++)
		{
			if (AbstractTabbedEditorWindowPane.ToolbarManager.TryGetCommandHandler(_TabbedEditorPane.GetType(),
				new GuidId(pguidCmdGroup, prgCmds[i].cmdID), out var commandHandler))
			{
				return commandHandler.HandleQueryStatus(_TabbedEditorPane, ref prgCmds[i], pCmdText);
			}
		}

		if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
		{
			for (int j = 0; j < cCmds; j++)
			{
				if (prgCmds[j].cmdID == (uint)EnCommandSet.ToolbarIdOnlineWindow)
				{
					prgCmds[j].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
					hresult = VSConstants.S_OK;
				}
			}
		}
		if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
		{
			for (int k = 0; k < cCmds; k++)
			{
				if (prgCmds[k].cmdID == (int)VSConstants.VSStd2KCmdID.BrowseToFileInExplorer && !AllowBrowseToFileInExplorer)
				{
					prgCmds[k].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
					hresult = VSConstants.S_OK;
				}
			}
		}

		if (!__(hresult) && _CmdTarget != null)
			hresult = _CmdTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

		return hresult;
	}

	int IVsToolboxActiveUserHook.InterceptDataObject(Microsoft.VisualStudio.OLE.Interop.IDataObject pIn, out Microsoft.VisualStudio.OLE.Interop.IDataObject ppOut)
	{
		Diag.ThrowIfNotOnUIThread();

		IVsToolboxActiveUserHook textEditor = _TextEditor;

		if (textEditor != null && textEditor.InterceptDataObject(pIn, out ppOut) == 0)
			return VSConstants.S_OK;

		if (GetView() is IVsToolboxActiveUserHook vsToolboxActiveUserHook)
			return vsToolboxActiveUserHook.InterceptDataObject(pIn, out ppOut);

		ppOut = null;

		return VSConstants.E_NOTIMPL;
	}

	int IVsToolboxActiveUserHook.ToolboxSelectionChanged(Microsoft.VisualStudio.OLE.Interop.IDataObject pSelected)
	{
		Diag.ThrowIfNotOnUIThread();

		IVsToolboxActiveUserHook textEditor = _TextEditor;

		if (textEditor != null && textEditor.ToolboxSelectionChanged(pSelected) == 0)
			return VSConstants.S_OK;

		if (GetView() is IVsToolboxActiveUserHook vsToolboxActiveUserHook)
			return vsToolboxActiveUserHook.ToolboxSelectionChanged(pSelected);

		return VSConstants.E_NOTIMPL;
	}

	int IVsToolboxUser.IsSupported(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		Diag.ThrowIfNotOnUIThread();

		IVsToolboxUser textEditor = TextEditor;

		if (textEditor != null && textEditor.IsSupported(pDO) == 0)
			return VSConstants.S_OK;

		if (GetView() is IVsToolboxUser vsToolboxUser)
			return vsToolboxUser.IsSupported(pDO);

		return VSConstants.E_NOTIMPL;
	}

	int IVsToolboxUser.ItemPicked(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		if (ToolboxItemPickedEvent != null)
		{
			ToolboxEventArgs toolboxEventArgs = new ToolboxEventArgs(pDO);
			ToolboxItemPickedEvent(this, toolboxEventArgs);

			if (toolboxEventArgs.Handled)
				return toolboxEventArgs.HResult;
		}

		Diag.ThrowIfNotOnUIThread();

		IVsToolboxUser textEditor = TextEditor;

		if (textEditor != null && textEditor.ItemPicked(pDO) == 0)
			return 0;

		if (GetView() is IVsToolboxUser vsToolboxUser)
			return vsToolboxUser.ItemPicked(pDO);

		return VSConstants.E_NOTIMPL;
	}

	/// <summary>
	/// IVsDesignerInfo.get_DesignerTechnology implementation 
	/// </summary>
	/// <param name="pbstrTechnology"></param>
	/// <returns></returns>
	public int get_DesignerTechnology(out string pbstrTechnology)
	{
		pbstrTechnology = "";
		return 0;
	}

	int IVsToolboxPageChooser.GetPreferredToolboxPage(out Guid pguidPage)
	{
		pguidPage = Guid.Empty;
		return 1;
	}

	int IVsDefaultToolboxTabState.GetDefaultTabExpansion(string pszTabID, out int pfExpanded)
	{
		pfExpanded = 0;
		return 0;
	}
}
