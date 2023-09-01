// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TextEditor
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Ctl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;



namespace BlackbirdSql.Common.Controls;

public class TextEditor : IOleCommandTarget, IVsTextViewEvents, IVsCodeWindowEvents, ITextEditor, IVsToolboxActiveUserHook, IVsToolboxUser, IDisposable
{
	private readonly IVsHierarchy _Hierarchy;

	private readonly IVsCodeWindow _codeWindow;

	private ConnectionPointCookie _codeWindowCookie;

	private Dictionary<IVsTextView, ConnectionPointCookie> _connectedViews;

	private IVsTextLines _lines;

	private IVsTextStream _stream;

	private bool _isActive;

	private readonly IOleCommandTarget _cmdTarget;

	private Timer _textTimer;

	private TextEditorProxy _textEditorProxy;

	private readonly ITabbedEditorService _tabbedEditorService;

	private ServiceProvider _services;

	private TextEditorProxy TextEditorProxy => _textEditorProxy;

	private ITextEditorEvents TextEditorEvents
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
			if (Native.Failed(_codeWindow.GetLastActiveView(out var ppView)))
			{
				Native.ThrowOnFailure(_codeWindow.GetPrimaryView(out ppView));
			}
			return ppView;
		}
	}

	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			if (_isActive != value)
			{
				_isActive = value;
				if (_isActive)
				{
					ReplacePropGrid(this, EventArgs.Empty);
					OnCaretPositionChanged();
				}
				TextEditorEvents?.OnActiveChanged(_isActive);
			}
		}
	}

	private IVsTextLines Lines
	{
		get
		{
			if (_lines == null)
			{
				TextView.GetBuffer(out _lines);
			}
			return _lines;
		}
	}

	private IVsTextStream Stream
	{
		get
		{
			_stream ??= Lines as IVsTextStream;
			return _stream;
		}
	}

	public TextEditor(Microsoft.VisualStudio.OLE.Interop.IServiceProvider vsServices, System.IServiceProvider tabbedEditorServices, IVsCodeWindow codeWindow, IOleCommandTarget commandTarget)
	{
		_codeWindow = codeWindow;
		_cmdTarget = commandTarget;
		_services = new ServiceProvider(vsServices);
		ConnectView(TextView);
		_codeWindowCookie = new ConnectionPointCookie(_codeWindow, this, typeof(IVsCodeWindowEvents));
		_textTimer = new Timer
		{
			Interval = 200
		};
		_textTimer.Tick += OnCaretPositionChangedDelay;
		_textEditorProxy = new TextEditorProxy(this);
		if (tabbedEditorServices != null)
		{
			_tabbedEditorService = tabbedEditorServices.GetService(typeof(ITabbedEditorService)) as ITabbedEditorService;
			if (_tabbedEditorService != null)
			{
				_tabbedEditorService.TextEditor = TextEditorProxy;
			}
			if (tabbedEditorServices.GetService(typeof(SVsWindowFrame)) is IVsWindowFrame vsWindowFrame)
			{
				vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out var pvar);
				_Hierarchy = (IVsHierarchy)pvar;
				_ = _Hierarchy; // Suppression
			}
		}
	}

	private void ConnectView(IVsTextView view)
	{
		_connectedViews ??= new Dictionary<IVsTextView, ConnectionPointCookie>();
		ConnectionPointCookie value = new ConnectionPointCookie(view, this, typeof(IVsTextViewEvents));
		_connectedViews.Add(view, value);
	}

	private void ReplacePropGrid(object sender, EventArgs e)
	{
#pragma warning disable IDE0059 // Unnecessary assignment of a value
		IVsTrackSelectionEx vsTrackSelectionEx = _services.GetService(typeof(SVsTrackSelectionEx)) as IVsTrackSelectionEx;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
	}

	private void OnCaretPositionChanged()
	{
		_textTimer.Enabled = false;
		_textTimer.Enabled = true;
	}

	private void OnCaretPositionChangedDelay(object sender, EventArgs e)
	{
		_textTimer.Enabled = false;
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
		if (_cmdTarget != null)
		{
			int num = _cmdTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
			if (num == (int)OleConstants.OLECMDERR_E_NOTSUPPORTED || num == (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP)
			{
				if (VSConstants.GUID_VSStandardCommandSet97.Equals(pguidCmdGroup) && nCmdID == 332)
				{
					_tabbedEditorService?.Activate(VSConstants.LOGVIEWID_Designer, EnTabViewMode.Default);
					num = 0;
				}
				Native.Failed(num);
			}
			return num;
		}
		return 0;
	}

	int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		if (_cmdTarget != null)
		{
			int num = _cmdTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
			if ((num == (int)OleConstants.OLECMDERR_E_NOTSUPPORTED || num == (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP) && VSConstants.GUID_VSStandardCommandSet97.Equals(pguidCmdGroup))
			{
				for (int i = 0; i < cCmds; i++)
				{
					if (prgCmds[i].cmdID == 332)
					{
						prgCmds[i].cmdf = 2u;
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
		if (_codeWindowCookie != null)
		{
			_codeWindowCookie.Dispose();
			_codeWindowCookie = null;
		}
		if (_connectedViews != null)
		{
			foreach (ConnectionPointCookie value in _connectedViews.Values)
			{
				value.Dispose();
			}
			_connectedViews = null;
		}
		if (_textEditorProxy != null)
		{
			_textEditorProxy.Dispose();
			_textEditorProxy = null;
		}
		if (_textTimer != null)
		{
			_textTimer.Dispose();
			_textTimer = null;
		}
		if (_services != null)
		{
			_services.Dispose();
			_services = null;
		}
	}

	~TextEditor()
	{
		Dispose(disposing: false);
	}

	public void SetTextEditorEvents(ITextEditorEvents events)
	{
	}

	public void SyncState()
	{
		ITextEditorEvents textEditorEvents = TextEditorEvents;
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
		_tabbedEditorService?.Activate(VSConstants.LOGVIEWID_TextView, EnTabViewMode.Default);
	}

	int IVsCodeWindowEvents.OnCloseView(IVsTextView pView)
	{
		if (_connectedViews != null && _connectedViews.TryGetValue(pView, out var value))
		{
			value.Dispose();
			_connectedViews.Remove(pView);
		}
		return 0;
	}

	int IVsCodeWindowEvents.OnNewView(IVsTextView pView)
	{
		ConnectView(pView);
		return 0;
	}
}
