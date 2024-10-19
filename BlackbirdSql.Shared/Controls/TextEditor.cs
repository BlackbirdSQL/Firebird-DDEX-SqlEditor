// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TextEditor

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;



namespace BlackbirdSql.Shared.Controls;


public class TextEditor : IOleCommandTarget, IVsTextViewEvents, IVsCodeWindowEvents, IBsTextEditor, IVsToolboxActiveUserHook, IVsToolboxUser, IDisposable
{
	private readonly IVsHierarchy _Hierarchy;

	private readonly IVsCodeWindow _CodeWindow;

	private ConnectionPointCookie _CodeWindowCookie;

	private Dictionary<IVsTextView, ConnectionPointCookie> _ConnectedViews;

	private IVsTextLines _Lines;

	private IVsTextStream _Stream;

	private bool _IsActive;

	private readonly IOleCommandTarget _CmdTarget;

	private Timer _TextTimer;

	private TextEditorProxy _TextEditorProxy;

	private readonly IBsEditorPaneServiceProvider _TabbedEditorService;

	private ServiceProvider _Services;

	private TextEditorProxy TextEditorProxy => _TextEditorProxy;




	private IBsTextEditorEvents TextEditorEvents
	{
		get
		{
			if (TextEditorProxy != null)
			{
				return TextEditorProxy.TextEditorEvents;
			}
			return null;
		}
	}

	public int SelectionStart
	{
		get
		{
			TextView.GetCaretPos(out var piLine, out var piColumn);
			return LineColToOffset(piLine, piColumn);
		}
	}

	private IVsTextView TextView
	{
		get
		{
			if (!__(_CodeWindow.GetLastActiveView(out var ppView)))
			{
				___(_CodeWindow.GetPrimaryView(out ppView));
			}

			return ppView;
		}
	}

	public bool IsActive
	{
		get
		{
			return _IsActive;
		}
		set
		{
			if (_IsActive != value)
			{
				_IsActive = value;
				if (_IsActive)
				{
					ReplacePropGrid(this, EventArgs.Empty);
					OnCaretPositionChanged();
				}
				TextEditorEvents?.OnActiveChanged(_IsActive);
			}
		}
	}

	private IVsTextLines Lines
	{
		get
		{
			if (_Lines == null)
			{
				TextView.GetBuffer(out _Lines);
			}
			return _Lines;
		}
	}

	private IVsTextStream Stream
	{
		get
		{
			_Stream ??= Lines as IVsTextStream;
			return _Stream;
		}
	}

	public TextEditor(IOleServiceProvider vsServices, System.IServiceProvider tabbedEditorServices, IVsCodeWindow codeWindow, IOleCommandTarget commandTarget)
	{
		_CodeWindow = codeWindow;
		_CmdTarget = commandTarget;
		_Services = new ServiceProvider(vsServices);
		ConnectView(TextView);
		_CodeWindowCookie = new ConnectionPointCookie(_CodeWindow, this, typeof(IVsCodeWindowEvents));
		_TextTimer = new Timer
		{
			Interval = 200
		};
		_TextTimer.Tick += OnCaretPositionChangedDelay;
		_TextEditorProxy = new TextEditorProxy(this);
		if (tabbedEditorServices != null)
		{
			Diag.ThrowIfNotOnUIThread();

			_TabbedEditorService = tabbedEditorServices.GetService(typeof(IBsEditorPaneServiceProvider)) as IBsEditorPaneServiceProvider;
			if (_TabbedEditorService != null)
			{
				_TabbedEditorService.TextEditor = TextEditorProxy;
			}
			if (tabbedEditorServices.GetService(typeof(SVsWindowFrame)) is IVsWindowFrame vsWindowFrame)
			{
				vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out var pvar);
				_Hierarchy = (IVsHierarchy)pvar;
				_ = _Hierarchy; // Suppression
			}
		}
	}



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	private void ConnectView(IVsTextView view)
	{
		_ConnectedViews ??= [];
		ConnectionPointCookie value = new ConnectionPointCookie(view, this, typeof(IVsTextViewEvents));
		_ConnectedViews.Add(view, value);
	}


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	private static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);

	private void ReplacePropGrid(object sender, EventArgs e)
	{
		Diag.ThrowIfNotOnUIThread();

#pragma warning disable IDE0059 // Unnecessary assignment of a value
		IVsTrackSelectionEx vsTrackSelectionEx = _Services.GetService(typeof(SVsTrackSelectionEx)) as IVsTrackSelectionEx;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
	}

	private void OnCaretPositionChanged()
	{
		_TextTimer.Enabled = false;
		_TextTimer.Enabled = true;
	}

	private void OnCaretPositionChangedDelay(object sender, EventArgs e)
	{
		_TextTimer.Enabled = false;
		TextEditorEvents?.OnCaretChanged(SelectionStart);
	}

	public void Select(int start, int length)
	{
		if (!IsActive)
		{
			IVsTextView textView = TextView;
			OffsetToLineCol(start, out var line, out var col);
			OffsetToLineCol(start + length, out var line2, out var col2);
			textView.PositionCaretForEditing(line, 0);
			textView.EnsureSpanVisible(new TextSpan
			{
				iStartLine = line,
				iStartIndex = col,
				iEndLine = line2,
				iEndIndex = col2
			});
			textView.SetSelection(line2, col2, line, col);
		}
	}

	private int LineColToOffset(int line, int col)
	{
		Stream.GetPositionOfLineIndex(line, col, out var piPosition);
		return piPosition;
	}

	private void OffsetToLineCol(int offset, out int line, out int col)
	{
		Lines.GetLineIndexOfPosition(offset, out line, out col);
	}

	int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (_CmdTarget != null)
		{
			Diag.ThrowIfNotOnUIThread();

			int hresult = _CmdTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

			if (hresult == (int)OleConstants.OLECMDERR_E_NOTSUPPORTED || hresult == (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP)
			{
				if (VSConstants.GUID_VSStandardCommandSet97.Equals(pguidCmdGroup)
					&& nCmdID == (uint)VSConstants.VSStd97CmdID.ViewForm)
				{
					_TabbedEditorService?.Activate(VSConstants.LOGVIEWID_Designer, EnTabViewMode.Default);
					hresult = VSConstants.S_OK;
				}

				// !__(hresult);
			}

			return hresult;
		}

		return VSConstants.S_OK;
	}



	int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		if (_CmdTarget != null)
		{
			Diag.ThrowIfNotOnUIThread();

			int num = _CmdTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
			if ((num == (int)OleConstants.OLECMDERR_E_NOTSUPPORTED || num == (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP) && VSConstants.GUID_VSStandardCommandSet97.Equals(pguidCmdGroup))
			{
				for (int i = 0; i < cCmds; i++)
				{
					if (prgCmds[i].cmdID == (uint)VSConstants.VSStd97CmdID.ViewForm)
					{
						prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_ENABLED;
						num = 0;
					}
				}
			}
			return num;
		}
		return 0;
	}

	void IVsTextViewEvents.OnChangeCaretLine(IVsTextView pView, int iNewLine, int iOldLine)
	{
	}

	void IVsTextViewEvents.OnChangeScrollInfo(IVsTextView pView, int iBar, int iMinUnit, int iMaxUnits, int iVisibleUnits, int iFirstVisibleUnit)
	{
	}

	void IVsTextViewEvents.OnKillFocus(IVsTextView pView)
	{
		IsActive = false;
	}

	void IVsTextViewEvents.OnSetBuffer(IVsTextView pView, IVsTextLines pBuffer)
	{
	}

	void IVsTextViewEvents.OnSetFocus(IVsTextView pView)
	{
		IsActive = true;
	}

	int IVsToolboxActiveUserHook.InterceptDataObject(Microsoft.VisualStudio.OLE.Interop.IDataObject pIn, out Microsoft.VisualStudio.OLE.Interop.IDataObject ppOut)
	{
		ppOut = null;
		return VSConstants.E_NOTIMPL;
	}

	int IVsToolboxActiveUserHook.ToolboxSelectionChanged(Microsoft.VisualStudio.OLE.Interop.IDataObject pSelected)
	{
		return 0;
	}

	int IVsToolboxUser.IsSupported(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		return 1;
	}

	int IVsToolboxUser.ItemPicked(Microsoft.VisualStudio.OLE.Interop.IDataObject pDO)
	{
		return 1;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		if (_CodeWindowCookie != null)
		{
			_CodeWindowCookie.Dispose();
			_CodeWindowCookie = null;
		}
		if (_ConnectedViews != null)
		{
			foreach (ConnectionPointCookie value in _ConnectedViews.Values)
			{
				value.Dispose();
			}
			_ConnectedViews = null;
		}
		if (_TextEditorProxy != null)
		{
			_TextEditorProxy.Dispose();
			_TextEditorProxy = null;
		}
		if (_TextTimer != null)
		{
			_TextTimer.Dispose();
			_TextTimer = null;
		}
		if (_Services != null)
		{
			if (ThreadHelper.CheckAccess())
				_Services.Dispose();
			_Services = null;
		}
	}

	~TextEditor()
	{
		Dispose(disposing: false);
	}

	public void SetTextEditorEvents(IBsTextEditorEvents events)
	{
	}

	public void SyncState()
	{
		IBsTextEditorEvents textEditorEvents = TextEditorEvents;
		if (textEditorEvents != null)
		{
			textEditorEvents.OnActiveChanged(IsActive);
			if (IsActive)
			{
				textEditorEvents.OnCaretChanged(SelectionStart);
			}
		}
	}

	public void Focus()
	{
		_TabbedEditorService?.Activate(VSConstants.LOGVIEWID_TextView, EnTabViewMode.Default);
	}

	int IVsCodeWindowEvents.OnCloseView(IVsTextView pView)
	{
		// Evs.Trace(GetType(), nameof(OnCloseView));

		if (_ConnectedViews != null && _ConnectedViews.TryGetValue(pView, out var value))
		{
			value.Dispose();
			_ConnectedViews.Remove(pView);
		}

		return 0;
	}

	int IVsCodeWindowEvents.OnNewView(IVsTextView pView)
	{
		ConnectView(pView);
		return 0;
	}
}
