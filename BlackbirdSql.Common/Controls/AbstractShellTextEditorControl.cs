#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Controls.ResultsPane;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using Constants = Microsoft.VisualStudio.OLE.Interop.Constants;
using BlackbirdSql.Common.Events;
using BlackbirdSql.Common.Structs;

namespace BlackbirdSql.Common.Controls;


public abstract class AbstractShellTextEditorControl : Control, IDisposable, IOleCommandTarget, IVsWindowFrameNotify, IVsTextViewEvents
{
	private Guid _MandatedXmlLanguageServiceClsid = Guid.Empty;

	protected static readonly string STName = "VSEditor";

	protected BorderStyle _borderStyle = BorderStyle.Fixed3D;

	protected ShellTextBuffer _textBuffer;

	private IntPtr _editorHandle = IntPtr.Zero;

	protected ServiceProvider _OleServiceProvider;

	private bool _parentedEditorWithParkingWindow;

	protected IOleCommandTarget _textCmdTarget;

	protected IVsWindowPane _textWindowPane;

	protected IVsWindowFrameNotify _textWndFrameNotify;

	protected bool _bWantCustomPopupMenu;

	protected Guid _ClsidLanguageService = Guid.Empty;

	protected Guid _ClsidLanguageServiceDefault;

	private static IVsTextManager _VsTextManager = null;

	private bool _withEncoding;

	// private const int C_WM_DESTROY = 2;

	// private const int C_WM_NCDESTROY = 130;

	protected override CreateParams CreateParams
	{
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			Tracer.Trace(GetType(), "AbstractShellTextEditorControl.CreateParams", "", null);
			CreateParams createParams = base.CreateParams;
			if (_borderStyle != 0)
			{
				createParams.Style |= 8388608;
			}

			createParams.Style |= 1174405120;
			return createParams;
		}
	}

	public object DocData
	{
		get
		{
			if (_textBuffer != null)
			{
				return _textBuffer.TextStream;
			}

			return null;
		}
	}


	protected virtual Guid MandatedXmlLanguageServiceClsid
	{
		get
		{
			if (_MandatedXmlLanguageServiceClsid == Guid.Empty)
				_MandatedXmlLanguageServiceClsid = new(SystemData.MandatedXmlLanguageServiceGuid);

			return _MandatedXmlLanguageServiceClsid;
		}
	}



	public ShellTextBuffer TextBuffer
	{
		get
		{
			if (_textBuffer != null)
			{
				return _textBuffer;
			}

			return null;
		}
	}

	public abstract IVsFindTarget FindTarget { get; }

	public abstract IVsTextView CurrentView { get; }

	public BorderStyle BorderStyle
	{
		get
		{
			return _borderStyle;
		}
		set
		{
			Tracer.Trace(GetType(), "AbstractShellTextEditorControl.BorderStyle", "value = {0}", value.ToString());
			if (Enum.IsDefined(typeof(BorderStyle), value))
			{
				_borderStyle = value;
				if (IsHandleCreated)
				{
					RecreateHandle();
				}
			}
		}
	}

	public bool WantCustomPopupMenu
	{
		get
		{
			return _bWantCustomPopupMenu;
		}
		set
		{
			VerifyBeforeInstanceProperty();
			_bWantCustomPopupMenu = true;
		}
	}

	public Guid ClsidLanguageService
	{
		get
		{
			return _ClsidLanguageService;
		}
		set
		{
			Tracer.Trace(GetType(), "AbstractShellTextEditorControl.LanguageService", "value = {0}", value.ToString("D", CultureInfo.CurrentCulture));
			if (!_ClsidLanguageService.Equals(value))
			{
				_ClsidLanguageService = value;
				if (!_ClsidLanguageService.Equals(Guid.Empty) && _textBuffer != null)
				{
					ApplyLS(value);
				}
			}
		}
	}

	public Guid ClsidLanguageServiceDefault
	{
		get
		{
			return _ClsidLanguageServiceDefault;
		}
		set
		{
			Tracer.Trace(GetType(), "AbstractShellTextEditorControl.ClsidLanguageServiceDefault", "value = {0}", value.ToString("D", CultureInfo.CurrentCulture));
			_ClsidLanguageServiceDefault = value;
			_textBuffer.DetectLangSid = false;
		}
	}


	public bool WithEncoding
	{
		get
		{
			return _withEncoding;
		}
		set
		{
			_withEncoding = value;
		}
	}

	protected IntPtr EditorHandle
	{
		get
		{
			return _editorHandle;
		}
		set
		{
			_editorHandle = value;
		}
	}

	protected abstract bool IsEditorInstanceCreated { get; }

	public event SpecialEditorCommandEventHandler ShowPopupMenuEvent;

	public AbstractShellTextEditorControl()
	{
		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.AbstractShellTextEditorControl", "", null);
	}

	protected override void Dispose(bool disposing)
	{
		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.Dispose", "", null);
		if (!IsDisposed)
		{
			ShowPopupMenuEvent = null;
			if (disposing)
			{
				if (_textBuffer != null)
				{
					_textBuffer.Dispose();
					_textBuffer = null;
				}

				if (_OleServiceProvider != null)
				{
					(_OleServiceProvider as IDisposable)?.Dispose();
					_OleServiceProvider = null;
				}
			}

			if (_editorHandle != IntPtr.Zero)
			{
				_editorHandle = IntPtr.Zero;
			}

			UnsinkEventsAndFreeInterfaces();
			if (_OleServiceProvider != null)
			{
				_OleServiceProvider = null;
			}

			base.Dispose(disposing);
		}
		else
		{
			Tracer.Trace(GetType(), "AbstractShellTextEditorControl.Dispose", "already disposed");
		}

		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.Dispose", "returning");
	}

	public virtual int QueryStatus(ref Guid guidGroup, uint nCmdId, OLECMD[] oleCmd, IntPtr oleText)
	{
		if (_textCmdTarget != null)
		{
			return _textCmdTarget.QueryStatus(ref guidGroup, nCmdId, oleCmd, oleText);
		}

		return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;
	}

	public virtual int Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr pobIn, IntPtr pvaOut)
	{
		if (_textCmdTarget != null)
		{
			return _textCmdTarget.Exec(ref guidGroup, nCmdId, nCmdExcept, pobIn, pvaOut);
		}

		return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;
	}

	void IVsTextViewEvents.OnSetFocus(IVsTextView pView)
	{
		base.OnGotFocus(EventArgs.Empty);
		IContainerControl containerControl = GetContainerControl();
		if (containerControl != null)
		{
			containerControl.ActiveControl = this;
		}
	}

	void IVsTextViewEvents.OnKillFocus(IVsTextView pView)
	{
		base.OnLostFocus(EventArgs.Empty);
	}

	void IVsTextViewEvents.OnSetBuffer(IVsTextView pView, IVsTextLines pBuffer)
	{
	}

	void IVsTextViewEvents.OnChangeScrollInfo(IVsTextView pView, int iBar, int iMinUnit, int iMaxUnits, int iVisibleUnits, int iFirstVisibleUnit)
	{
	}

	void IVsTextViewEvents.OnChangeCaretLine(IVsTextView pView, int iNewLine, int iOldLine)
	{
	}

	public virtual int OnShow(int frameShow)
	{
		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.OnShow", "frameShow = {0}", frameShow);
		if (_textWndFrameNotify != null)
		{
			return _textWndFrameNotify.OnShow(frameShow);
		}

		return VSConstants.S_OK;
	}

	public virtual int OnMove()
	{
		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.OnMove", "", null);
		if (_textWndFrameNotify != null)
		{
			return _textWndFrameNotify.OnMove();
		}

		return VSConstants.S_OK;
	}

	public virtual int OnSize()
	{
		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.OnSize", "", null);
		if (_textWndFrameNotify != null)
		{
			return _textWndFrameNotify.OnSize();
		}

		return VSConstants.S_OK;
	}

	public virtual int OnDockableChange(int fDockable)
	{
		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.OnDockableChange", "fDockable = {0}", fDockable);
		if (_textWndFrameNotify != null)
		{
			return _textWndFrameNotify.OnDockableChange(fDockable);
		}

		return VSConstants.S_OK;
	}

	public int LoadViewState(IStream state)
	{
		if (_textWindowPane != null)
		{
			return _textWindowPane.LoadViewState(state);
		}

		return VSConstants.E_NOTIMPL;
	}

	public int SaveViewState(IStream state)
	{
		if (_textWindowPane != null)
		{
			return _textWindowPane.SaveViewState(state);
		}

		return VSConstants.E_NOTIMPL;
	}

	public void PrepareForHandleRecreation()
	{
		if (!ShouldDistroyNativeControl())
		{
			using Control control = new Control();
			IntPtr parent = GetParent(new HandleRef(control, control.Handle));
			SetParent(new HandleRef(this, Handle), new HandleRef(this, parent));
			_parentedEditorWithParkingWindow = true;
		}
	}

	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg != 133 || !DrawManager.DrawNCBorder(ref m))
		{
			base.WndProc(ref m);
		}
	}

	[UIPermission(SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
	protected override void CreateHandle()
	{
		base.CreateHandle();
		if (_parentedEditorWithParkingWindow)
		{
			_parentedEditorWithParkingWindow = false;
			if (IsWindow(_editorHandle) && Handle != GetParent(_editorHandle))
			{
				SetParent(new HandleRef(this, _editorHandle), new HandleRef(this, Handle));
			}
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.OnSizeChanged", "", null);
		if (_editorHandle != IntPtr.Zero)
		{
			Tracer.Trace(GetType(), "AbstractShellTextEditorControl.OnSizeChanged", "adjusting text view size");
			Rectangle clientRectangle = ClientRectangle;
			Native.SetWindowPos(_editorHandle, IntPtr.Zero, clientRectangle.X, clientRectangle.Y, clientRectangle.Width, clientRectangle.Height, 4);
			OnSize();
		}

		base.OnSizeChanged(e);
	}

	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public override bool PreProcessMessage(ref Message msg)
	{
		return false;
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		if (_editorHandle != IntPtr.Zero)
		{
			Native.SetFocus(_editorHandle);
		}
	}

	public void GetCoordinatesForPopupMenu(object[] vIn, out int x, out int y)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		Point mousePosition = MousePosition;
		if (vIn != null && vIn.Length == 1 && vIn[0] != null)
		{
			x = (short)vIn[0];
			if (x == mousePosition.X)
			{
				y = mousePosition.Y;
				return;
			}

			try
			{
				IVsTextView currentView = CurrentView;
				Native.ThrowOnFailure(currentView.GetCaretPos(out var piLine, out var piColumn), (string)null);
				POINT[] array = new POINT[1];
				Native.ThrowOnFailure(currentView.GetPointOfLineColumn(piLine, piColumn, array), (string)null);

				if (Native.GetClientRect(currentView.GetWindowHandle(), out UIRECT val)
					&& (array[0].y < val.Top || array[0].x > val.Bottom || array[0].x < val.Left || array[0].y > val.Right))
				{
					array[0].x = 0;
					array[0].y = 0;
				}

				y = PointToScreen(new Point(array[0].x, array[0].y)).Y;
			}
			catch
			{
				y = mousePosition.Y;
			}
		}
		else
		{
			x = mousePosition.X;
			y = mousePosition.Y;
		}
	}

	public void CreateAndInitEditorWindow(object serviceProvider)
	{
		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.CreateAndInitEditorWindow", "", null);
		if (_textBuffer == null)
		{
			CreateAndInitTextBuffer(serviceProvider, null);
		}

		_OleServiceProvider = new ServiceProvider(serviceProvider as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
		bool flag = true;
		if (EditorHandle == IntPtr.Zero)
		{
			CreateEditorWindow(serviceProvider);
			flag = false;
		}

		_VsTextManager ??= _OleServiceProvider.GetService(VS.CLSID_TextManager) as IVsTextManager;

		if (!flag)
		{
			SinkEventsAndCacheInterfaces();
		}

		if (_textCmdTarget == null || _textWindowPane == null || _VsTextManager == null)
		{
			Exception ex = new InvalidOperationException(ControlsResources.ErrCannotInitNewEditorInst);
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}
	}

	public void CreateAndInitTextBuffer(object sp, IVsTextStream existingDocData)
	{
		Tracer.Trace(GetType(), "AbstractShellTextEditorControl.CreateAndInitTextBuffer", "", null);
		if (existingDocData != null)
		{
			_textBuffer = new(existingDocData, sp)
			{
				WithEncoding = _withEncoding
			};
		}
		else
		{
			_textBuffer = new()
			{
				WithEncoding = _withEncoding
			};
			if (GetType().Name == "TextResultsViewContol")
			{
				_textBuffer.InitContent = true;
			}

			_textBuffer.SetSite(sp);
		}

		OnTextBufferCreated(_textBuffer);
	}

	public static void ResetFontAndColor(Font font, Guid fontCategory, Guid colorCategory)
	{
		Tracer.Trace(typeof(AbstractShellTextEditorControl), "AbstractShellTextEditorControl.ResetFontAndColor", "", null);
		if (_VsTextManager != null)
		{
			IConnectionPointContainer connectionPointContainer = _VsTextManager as IConnectionPointContainer;
			Microsoft.VisualStudio.OLE.Interop.CONNECTDATA[] array = new Microsoft.VisualStudio.OLE.Interop.CONNECTDATA[1];
			uint pcFetched = 1u;
			FONTCOLORPREFERENCES fONTCOLORPREFERENCES = default;
			IntPtr intPtr = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			try
			{
				Guid riid = typeof(IVsTextManagerEvents).GUID;
				connectionPointContainer.FindConnectionPoint(ref riid, out var ppCP);
				ppCP.EnumConnections(out var ppEnum);
				intPtr = Marshal.AllocHGlobal(16);
				Marshal.Copy(fontCategory.ToByteArray(), 0, intPtr, 16);
				intPtr2 = Marshal.AllocHGlobal(16);
				Marshal.Copy(colorCategory.ToByteArray(), 0, intPtr2, 16);
				fONTCOLORPREFERENCES.pguidFontCategory = intPtr;
				fONTCOLORPREFERENCES.pguidColorCategory = intPtr2;
				if (font != null)
				{
					fONTCOLORPREFERENCES.hRegularViewFont = font.ToHfont();
					fONTCOLORPREFERENCES.hBoldViewFont = font.ToHfont();
				}

				while (pcFetched == 1)
				{
					Native.ThrowOnFailure(ppEnum.Next(1u, array, out pcFetched), (string)null);
					if (pcFetched == 1)
					{
						(array[0].punk as IVsTextManagerEvents).OnUserPreferencesChanged(null, null, null, new FONTCOLORPREFERENCES[1] { fONTCOLORPREFERENCES });
					}
				}
			}
			catch (Exception e)
			{
				Tracer.LogExCatch(typeof(AbstractShellTextEditorControl), e);
			}
			finally
			{
				if (intPtr != (IntPtr)0)
				{
					Marshal.FreeHGlobal(intPtr);
				}

				if (intPtr2 != (IntPtr)0)
				{
					Marshal.FreeHGlobal(intPtr2);
				}
			}
		}
		else
		{
			Tracer.Trace(typeof(CommonUtils), "AbstractShellTextEditorControl.ResetFontAndColor", "s_vsTextManager is null");
		}
	}

	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);

	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetParent(HandleRef hWnd);

	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetParent(IntPtr hWnd);

	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool IsWindow(IntPtr hWnd);

	protected bool ShouldDistroyNativeControl()
	{
		if (Disposing)
		{
			return true;
		}

		for (Control parent = Parent; parent != null; parent = parent.Parent)
		{
			if (parent.Disposing)
			{
				return true;
			}

			if (parent.RecreatingHandle)
			{
				return false;
			}
		}

		return false;
	}

	protected void Release(object o)
	{
		if (Marshal.IsComObject(o))
		{
			for (int num = Marshal.ReleaseComObject(o); num > 0; num = Marshal.ReleaseComObject(o))
			{
			}
		}
	}

	protected void VerifyBeforeInstanceProperty()
	{
		if (IsEditorInstanceCreated)
		{
			InvalidOperationException ex = new("this property must be set BEFORE editor instance is created");
			Diag.Dug(ex);
			throw ex;
		}
	}

	public void OnSpecialEditorCommandEventHandler(object sender, SpecialEditorCommandEventArgs a)
	{
		if (a.CommandID == (int)VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU && ShowPopupMenuEvent != null)
		{
			ShowPopupMenuEvent(this, a);
		}
	}

	protected void ApplyLS(Guid lsGuid)
	{
		Tracer.Trace(GetType(), "SqlTextViewControl.ApplyLS", "lsGuid = {0}", lsGuid);
		if (_textBuffer == null)
		{
			return;
		}

		if (MandatedXmlLanguageServiceClsid == lsGuid)
		{
			if (_textBuffer.TextStream is IVsUserData vsUserData)
			{
				Guid riidKey = LibraryData.CLSID_PropertyDisableXmlEditorPropertyWindowIntegration;
				Native.ThrowOnFailure(vsUserData.SetData(ref riidKey, true), (string)null);
				riidKey = LibraryData.CLSID_PropertyOverrideXmlEditorSaveAsFileFilter;
				Native.ThrowOnFailure(vsUserData.SetData(ref riidKey, ControlsResources.SaveAsXmlaFilterString), (string)null);
			}
		}

		Native.ThrowOnFailure(_textBuffer.TextStream.SetLanguageServiceID(ref lsGuid), (string)null);
	}

	protected abstract void CreateEditorWindow(object nativeSP);

	protected abstract void SinkEventsAndCacheInterfaces();

	protected abstract void UnsinkEventsAndFreeInterfaces();

	protected virtual void OnTextBufferCreated(ShellTextBuffer buf)
	{
	}
}
