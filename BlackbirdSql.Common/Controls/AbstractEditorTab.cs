// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.EditorTab

using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;


namespace BlackbirdSql.Common.Controls;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

public abstract class AbstractEditorTab : IDisposable, IVsDesignerInfo, IVsMultiViewDocumentView, IVsWindowFrameNotify3, IOleCommandTarget, IVsToolboxActiveUserHook, IVsToolboxPageChooser, IVsDefaultToolboxTabState, IVsToolboxUser
{
	protected IVsWindowFrame _CurrentFrame;

	protected TextEditor _TextEditor;

	protected IOleCommandTarget _CmdTarget;

	private readonly System.IServiceProvider _WindowPaneServiceProvider;

	private Guid _LogicalView;

	private IWin32Window _Owner;

	private ISelectionContainer _SavedSelection;

	private bool _Showing;

	private bool _Visible;

	private bool _IsActive;

	private EnEditorTabType _EditorTabType;

	private readonly AbstractTabbedEditorPane _TabbedEditorPane;

	private Rectangle _Bounds;

	private Panel _ParentPanel;

	private ITrackSelection _TrackSelection;

	private ICollection _PropertyWindowSelectedObjects;




	public Guid CommandUIGuid
	{
		get
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			Guid pguid = Guid.Empty;
			_CurrentFrame?.GetGuidProperty((int)__VSFPROPID.VSFPROPID_CmdUIGuid, out pguid);
			return pguid;
		}
	}

	public IVsWindowFrame CurrentFrame => _CurrentFrame;

	protected System.IServiceProvider WindowPaneServiceProvider => _WindowPaneServiceProvider;

	protected AbstractTabbedEditorPane TabbedEditorPane => _TabbedEditorPane;

	protected string DocumentMoniker
	{
		get
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			IVsWindowFrame vsWindowFrame = _CurrentFrame ?? WindowPaneServiceProvider.GetService(typeof(SVsWindowFrame)) as IVsWindowFrame;
			if (vsWindowFrame == null)
			{
				return null;
			}
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
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

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
				if (!ThreadHelper.CheckAccess())
				{
					COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
					Diag.Dug(exc);
					throw exc;
				}

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
				_ParentPanel.SizeChanged += ((TabbedEditorUI)Owner).Panel_SizeChanged;
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
			return (TabbedEditorPane.Window as TabbedEditorUI).SplitViewContainer.SplitterBar.GetTabButtonVisibleStatus(LogicalView);
		}
		set
		{
			(TabbedEditorPane.Window as TabbedEditorUI).SplitViewContainer.SplitterBar.SetTabButtonVisibleStatus(LogicalView, value);
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
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

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

	public AbstractEditorTab(AbstractTabbedEditorPane editorPane, Guid logicalView, EnEditorTabType editorTabType)
	{
		_EditorTabType = editorTabType;
		_WindowPaneServiceProvider = editorPane;
		_LogicalView = logicalView;
		_CmdTarget = editorPane;
		_TabbedEditorPane = editorPane;
	}

	protected Panel GetPanelForCurrentFrame()
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		ErrorHandler.ThrowOnFailure(CurrentFrame.GetProperty((int)__VSFPROPID2.VSFPROPID_ParentHwnd, out var pvar));

		return Control.FromHandle((IntPtr)(int)pvar) as Panel;
	}

	public void EnsureFrameCreated()
	{
		if (_CurrentFrame == null)
		{
			using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
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
				if (!ThreadHelper.CheckAccess())
				{
					COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
					Diag.Dug(exc);
					throw exc;
				}

				((IVsWindowFrame2)Frame).ActivateOwnerDockedWindow();
			}
		}
	}

	public void CreateTextEditor()
	{
		_TextEditor = _CmdTarget as TextEditor;

		if (_TextEditor != null)
			return;

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (_CurrentFrame == null)
		{
			using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
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

	protected abstract Guid GetEditorFactoryGuid();

	protected void SetFrameProperties(IVsWindowFrame parentFrame, IVsWindowFrame frame)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (parentFrame != frame)
			frame.SetProperty((int)__VSFPROPID2.VSFPROPID_ParentFrame, parentFrame);

		frame.SetProperty((int)__VSFPROPID.VSFPROPID_ViewHelper, this);
		frame.SetProperty((int)__VSFPROPID3.VSFPROPID_NotifyOnActivate, true);
		Guid rguid = GetEditorFactoryGuid();
		frame.SetGuidProperty(-4009, ref rguid);
		string physicalViewString = GetPhysicalViewString();
		frame.SetProperty((int)__VSFPROPID.VSFPROPID_pszPhysicalView, physicalViewString);
		if (_Owner != null)
		{
			ParentPanel.SuspendLayout();
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
			ParentPanel.ResumeLayout();
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
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			try
			{
				_CmdTarget = null;
				_CurrentFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
				_CurrentFrame = null;
			}
			catch (Exception ex)
			{
				SqlTracer.DebugTraceEvent(TraceEventType.Warning, EnSqlTraceId.SqlEditorAndLanguageServices, "Exception encountered while Disposing AbstractEditorTab. CloseFrame threw: " + ex.Message);
			}
		}
		if (_ParentPanel != null)
		{
			_ParentPanel.SuspendLayout();
			_ParentPanel.SizeChanged -= ((TabbedEditorUI)Owner).Panel_SizeChanged;
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
		try
		{
			_Visible = false;

			if (_CurrentFrame == null)
				return;

			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			_SavedSelection = null;
			if (Native.Succeeded(_CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out var pvar)))
			{
				using ServiceProvider serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)pvar);
				if (serviceProvider.GetService(typeof(SVsTrackSelectionEx)) is IVsTrackSelectionEx vsTrackSelectionEx && Native.Succeeded(vsTrackSelectionEx.GetCurrentSelection(out var ppHier, out var _, out var _, out var ppSC)) && ppSC != IntPtr.Zero)
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
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			_CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar);

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
				if (!ThreadHelper.CheckAccess())
				{
					COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
					Diag.Dug(exc);
					throw exc;
				}

				_CurrentFrame.SetFramePos((VSSETFRAMEPOS)(-1073741824), ref rguidRelativeTo, 0, 0, panelForCurrentFrame.Width, panelForCurrentFrame.Height);
			}
			else
			{
				SqlTracer.AssertTraceEvent(condition: false, TraceEventType.Warning, EnSqlTraceId.VSShell, "Panel width and height must not be negative");
			}
		}
	}

	public void Show()
	{
		try
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			_Showing = true;
			Frame.ShowNoActivate();
			_Showing = false;
			_Visible = true;

			((IVsWindowFrameNotify3)this).OnShow(1);
			if (GetView() is IOleCommandTarget cmdTarget)
			{
				_CmdTarget = cmdTarget;
			}
			if (_SavedSelection != null && Native.Succeeded(_CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out var pvar)))
			{
				using ServiceProvider serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)pvar);
				(serviceProvider.GetService(typeof(SVsTrackSelectionEx)) as IVsTrackSelectionEx).OnSelectChange(_SavedSelection);
				_SavedSelection = null;
			}
			int property = _CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out var pvar2);
			if (!Native.Succeeded(property))
			{
				return;
			}
			ServiceProvider serviceProvider2 = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)pvar2);
			using (serviceProvider2)
			{
				IVsHierarchy o = null;
				uint itemid = 0u;
				property = _CurrentFrame.GetProperty((int)VsFramePropID.Hierarchy, out var pvar3);
				if (Native.Succeeded(property))
				{
					o = (IVsHierarchy)pvar3;
					property = _CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out var pvar4);
					if (Native.Succeeded(property))
					{
						itemid = (uint)(int)pvar4;
					}
				}
				if (!Native.Succeeded(property) || serviceProvider2.GetService(typeof(SVsTrackSelectionEx)) is not IVsTrackSelectionEx vsTrackSelectionEx)
				{
					return;
				}
				IntPtr pSC = (IntPtr)(-1);
				IntPtr ppv = IntPtr.Zero;
				IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(o);
				try
				{
					Guid iid = typeof(IVsHierarchy).GUID;
					property = Marshal.QueryInterface(iUnknownForObject, ref iid, out ppv);
					if (Native.Succeeded(property))
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
					Marshal.Release(iUnknownForObject);
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

	}

	int IVsMultiViewDocumentView.ActivateLogicalView(ref Guid rguidLogicalView)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (_WindowPaneServiceProvider is IVsMultiViewDocumentView vsMultiViewDocumentView)
		{
			return vsMultiViewDocumentView.ActivateLogicalView(ref rguidLogicalView);
		}
		return VSConstants.E_NOTIMPL;
	}

	int IVsMultiViewDocumentView.GetActiveLogicalView(out Guid pguidLogicalView)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (_WindowPaneServiceProvider is IVsMultiViewDocumentView vsMultiViewDocumentView)
		{
			return vsMultiViewDocumentView.GetActiveLogicalView(out pguidLogicalView);
		}
		pguidLogicalView = Guid.Empty;
		return VSConstants.E_NOTIMPL;
	}

	int IVsMultiViewDocumentView.IsLogicalViewActive(ref Guid rguidLogicalView, out int pIsActive)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		if (_WindowPaneServiceProvider is IVsMultiViewDocumentView vsMultiViewDocumentView)
		{
			return vsMultiViewDocumentView.IsLogicalViewActive(ref rguidLogicalView, out pIsActive);
		}
		pIsActive = 0;
		return VSConstants.E_NOTIMPL;
	}

	int IVsWindowFrameNotify3.OnClose(ref uint pgrfSaveOptions)
	{
		IsClosed = true;
		if (_CurrentFrame != null)
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			int num = _CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object pvar);
			if (Native.Succeeded(num))
			{
				if (pvar is IVsWindowFrameNotify3 vsWindowFrameNotify)
				{
					uint pgrfSaveOptions2 = pgrfSaveOptions;
					num = vsWindowFrameNotify.OnClose(ref pgrfSaveOptions2);
				}
				else if (pvar is IVsWindowFrameNotify2 vsWindowFrameNotify2)
				{
					uint pgrfSaveOptions2 = pgrfSaveOptions;
					num = vsWindowFrameNotify2.OnClose(ref pgrfSaveOptions2);
				}
			}
			return num;
		}
		return 0;
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
		{
			ShownEvent(this, EventArgs.Empty);
		}

		int result = 0;
		if (_CurrentFrame == null)
			return result;

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		result = _CurrentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar);
		AbstractTabbedEditorPane AbstractTabbedEditorPane = pvar as AbstractTabbedEditorPane;
		if (Native.Succeeded(result) && AbstractTabbedEditorPane == null)
		{
			if (pvar is IVsWindowFrameNotify3 vsWindowFrameNotify)
			{
				result = vsWindowFrameNotify.OnShow(fShow);
			}
			else if (pvar is IVsWindowFrameNotify vsWindowFrameNotify2)
			{
				result = vsWindowFrameNotify2.OnShow(fShow);
			}
		}
		return result;
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

		if (AbstractTabbedEditorPane.ToolbarManager.TryGetCommandHandler(_TabbedEditorPane.GetType(),
			new GuidId(pguidCmdGroup, nCmdID), out var commandHandler))
		{
			return commandHandler.HandleExec(_TabbedEditorPane, nCmdexecopt, pvaIn, pvaOut);
		}
		if (_CmdTarget == null || _CmdTarget is AbstractTabbedEditorPane)
			return 0;

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		return _CmdTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
	}

	int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		int num = _TabbedEditorPane.HandleQueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		if (num == 0)
			return num;

		num = (int)Microsoft.VisualStudio.OLE.Interop.Constants.MSOCMDERR_E_NOTSUPPORTED;

		for (int i = 0; i < prgCmds.Length; i++)
		{
			if (AbstractTabbedEditorPane.ToolbarManager.TryGetCommandHandler(_TabbedEditorPane.GetType(),
				new GuidId(pguidCmdGroup, prgCmds[i].cmdID), out var commandHandler))
			{
				return commandHandler.HandleQueryStatus(_TabbedEditorPane, ref prgCmds[i], pCmdText);
			}
		}
		if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
		{
			for (int j = 0; j < cCmds; j++)
			{
				if (prgCmds[j].cmdID == (uint)EnCommandSet.MenuIdOnlineToolbar)
				{
					prgCmds[j].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
					num = 0;
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
					num = 0;
				}
			}
		}
		if (Native.Failed(num) && _CmdTarget != null)
		{
			if (_CmdTarget is AbstractTabbedEditorPane)
			{
				SqlTracer.AssertTraceEvent(condition: false, TraceEventType.Error, EnSqlTraceId.TabEditor, "Parent tab not found");
			}
			else
			{
				if (!ThreadHelper.CheckAccess())
				{
					COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
					Diag.Dug(exc);
					throw exc;
				}

				num = _CmdTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
			}
		}
		return num;
	}

	int IVsToolboxActiveUserHook.InterceptDataObject(Microsoft.VisualStudio.OLE.Interop.IDataObject pIn, out Microsoft.VisualStudio.OLE.Interop.IDataObject ppOut)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		IVsToolboxActiveUserHook textEditor = _TextEditor;
		if (textEditor != null && textEditor.InterceptDataObject(pIn, out ppOut) == 0)
		{
			return 0;
		}
		if (GetView() is IVsToolboxActiveUserHook vsToolboxActiveUserHook)
		{
			return vsToolboxActiveUserHook.InterceptDataObject(pIn, out ppOut);
		}
		ppOut = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsToolboxActiveUserHook.ToolboxSelectionChanged(Microsoft.VisualStudio.OLE.Interop.IDataObject pSelected)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		IVsToolboxActiveUserHook textEditor = _TextEditor;

		if (textEditor != null && textEditor.ToolboxSelectionChanged(pSelected) == 0)
		{
			return 0;
		}
		if (GetView() is IVsToolboxActiveUserHook vsToolboxActiveUserHook)
		{
			return vsToolboxActiveUserHook.ToolboxSelectionChanged(pSelected);
		}
		return VSConstants.E_NOTIMPL;
	}

	int IVsToolboxUser.IsSupported(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		IVsToolboxUser textEditor = TextEditor;
		if (textEditor != null && textEditor.IsSupported(pDO) == 0)
		{
			return 0;
		}
		if (GetView() is IVsToolboxUser vsToolboxUser)
		{
			return vsToolboxUser.IsSupported(pDO);
		}
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

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		IVsToolboxUser textEditor = TextEditor;
		if (textEditor != null && textEditor.ItemPicked(pDO) == 0)
		{
			return 0;
		}
		if (GetView() is IVsToolboxUser vsToolboxUser)
		{
			return vsToolboxUser.ItemPicked(pDO);
		}
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
