#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.InteropServices;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Common.Controls.ResultsPane
{
	public class ShellTextViewControl : AbstractShellTextEditorControl, VsCodeWindow, IVsCodeWindow
	{
		protected IVsTextView m_textView;

		protected IVsCodeWindowManager vsCodeWindowManager;

		private ConnectionPointCookie textViewEventsCookie;

		protected TextViewInitFlags textViewFlags;

		protected INITVIEW[] textViewInit = new INITVIEW[1];

		protected bool codeWindowStyle;

		protected TextViewCommandFilter _CommandFilter;


		protected string m_strFontCategoryGuid = VS.FontAndColorsTextToolWindows;

		protected string colorCategoryGuid = VS.FontAndColorsTextToolWindows;

		public IVsTextView TextView => m_textView;

		public virtual bool CodeWindowStyle
		{
			get
			{
				return codeWindowStyle;
			}
			set
			{
				VerifyBeforeInstanceProperty();
				codeWindowStyle = value;
			}
		}

		public virtual bool WithSelectionMargin
		{
			get
			{
				return (textViewFlags & TextViewInitFlags.VIF_SET_SELECTION_MARGIN) != 0;
			}
			set
			{
				VerifyBeforeInstanceProperty();
				if (value)
				{
					textViewFlags |= TextViewInitFlags.VIF_SET_SELECTION_MARGIN;
					textViewInit[0].fSelectionMargin = 1u;
				}
				else
				{
					textViewFlags &= ~TextViewInitFlags.VIF_SET_SELECTION_MARGIN;
					textViewInit[0].fSelectionMargin = 0u;
				}
			}
		}

		public virtual bool WithWidgetMargin
		{
			get
			{
				return (textViewFlags & TextViewInitFlags.VIF_SET_WIDGET_MARGIN) != 0;
			}
			set
			{
				VerifyBeforeInstanceProperty();
				if (value)
				{
					textViewFlags |= TextViewInitFlags.VIF_SET_WIDGET_MARGIN;
					textViewInit[0].fWidgetMargin = 1u;
				}
				else
				{
					textViewFlags &= ~TextViewInitFlags.VIF_SET_WIDGET_MARGIN;
					textViewInit[0].fWidgetMargin = 0u;
				}
			}
		}

		public virtual string ColorCategoryGuid
		{
			get
			{
				return colorCategoryGuid;
			}
			set
			{
				VerifyBeforeInstanceProperty();
				if (value == null)
				{
					ArgumentNullException ex = new("value");
					Diag.Dug(ex);
					throw ex;
				}

				if (value.Length == 0)
				{
					ArgumentOutOfRangeException ex = new("value");
					Diag.Dug(ex);
					throw ex;
				}

				colorCategoryGuid = value;
			}
		}

		public virtual string FontCategoryGuid
		{
			get
			{
				return m_strFontCategoryGuid;
			}
			set
			{
				VerifyBeforeInstanceProperty();
				if (value == null)
				{
					ArgumentNullException ex = new("value");
					Diag.Dug(ex);
					throw ex;
				}

				if (value.Length == 0)
				{
					ArgumentException ex = new("", "value");
					Diag.Dug(ex);
					throw ex;
				}

				m_strFontCategoryGuid = value;
			}
		}

		protected override bool IsEditorInstanceCreated => m_textView != null;

		public override IVsFindTarget FindTarget
		{
			get
			{
				if (m_textView != null)
				{
					return (IVsFindTarget)m_textView;
				}

				Exception ex = new COMException("", VSConstants.E_NOINTERFACE);
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
			}
		}

		public override IVsTextView CurrentView
		{
			get
			{
				if (m_textView != null)
				{
					return m_textView;
				}

				Exception ex = new COMException("", VSConstants.E_NOINTERFACE);
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
			}
		}

		public ShellTextViewControl()
		{
			Tracer.Trace(GetType(), "ShellTextViewControl.ShellTextViewControl", "", null);
			textViewFlags = TextViewInitFlags.VIF_SET_OVERTYPE | TextViewInitFlags.VIF_HSCROLL
				| TextViewInitFlags.VIF_VSCROLL | TextViewInitFlags.VIF_UPDATE_STATUS_BAR;
			textViewInit[0].fDragDropMove = 1u;
		}

		protected void DisposeCodeWindowManager()
		{
			if (vsCodeWindowManager != null)
			{
				try
				{
					Native.ThrowOnFailure(vsCodeWindowManager.RemoveAdornments());
				}
				catch (Exception e)
				{
					Tracer.LogExCatch(GetType(), e);
				}

				Release(vsCodeWindowManager);
				vsCodeWindowManager = null;
			}
		}

		protected override void Dispose(bool bDisposing)
		{
			Tracer.Trace(GetType(), "ShellTextViewControl.Dispose", "", null);
			DisposeCodeWindowManager();
			if (bDisposing)
			{
				if (_CommandFilter != null)
				{
					_CommandFilter.Dispose();
					_CommandFilter = null;
				}

				if (textViewEventsCookie != null)
				{
					textViewEventsCookie.Dispose();
					textViewEventsCookie = null;
				}
			}

			base.Dispose(bDisposing);
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "ShellTextViewControl.Dispose()", "returning");
		}

		protected override void OnTextBufferCreated(ShellTextBuffer buf)
		{
			buf.OnNewLangSvc += OnNewLangSvc;
		}

		protected override void SinkEventsAndCacheInterfaces()
		{
			SinkEditorEvents(bSink: true);
			_textWindowPane = (IVsWindowPane)m_textView;
			_textWndFrameNotify = null;
			_textCmdTarget = (IOleCommandTarget)m_textView;
		}

		protected override void UnsinkEventsAndFreeInterfaces()
		{
			SinkEditorEvents(bSink: false);
			if (_CommandFilter != null)
			{
				_CommandFilter.Dispose();
				_CommandFilter = null;
			}

			if (_textCmdTarget != null)
			{
				_textCmdTarget = null;
			}

			if (_textWndFrameNotify != null)
			{
				_textWndFrameNotify = null;
			}

			if (_textWindowPane != null)
			{
				_textWindowPane = null;
			}

			if (m_textView != null)
			{
				Native.ThrowOnFailure(m_textView.CloseView());
				Release(m_textView);
				m_textView = null;
			}
		}

		protected override void CreateEditorWindow(object nativeSP)
		{
			Tracer.Trace(GetType(), "ShellTextViewControl.CreateEditorWindow", "", null);
			ILocalRegistry obj = (ILocalRegistry)_OleServiceProvider.GetService(typeof(ILocalRegistry));
			if (obj == null)
			{
				Exception ex = new COMException(null, VSConstants.E_UNEXPECTED);
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
			}

			Guid riid = typeof(IVsTextView).GUID;
			Native.ThrowOnFailure(obj.CreateInstance(DefGuidList.CLSID_VsTextView, null, ref riid, (uint)CLSCTX.CLSCTX_INPROC_SERVER, out IntPtr ppvObj));
			m_textView = (IVsTextView)Marshal.GetObjectForIUnknown(ppvObj);
			Marshal.Release(ppvObj);
			((IObjectWithSite)m_textView).SetSite(nativeSP);
			Native.ThrowOnFailure(m_textView.Initialize(_textBuffer.TextStream as IVsTextLines, Handle, (uint)textViewFlags, textViewInit));
			IVsTextEditorPropertyCategoryContainer obj2 = (IVsTextEditorPropertyCategoryContainer)m_textView;
			Guid rguidCategory = VSConstants.EditPropyCategoryGuid.ViewMasterSettings_guid;
			Native.ThrowOnFailure(obj2.GetPropertyCategory(ref rguidCategory, out IVsTextEditorPropertyContainer ppProp));

			if (codeWindowStyle)
			{
				object var = true;
				Native.ThrowOnFailure(ppProp.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewComposite_AllCodeWindowDefaults, var));
			}
			else
			{
				if (m_strFontCategoryGuid != null && m_strFontCategoryGuid.Length != 0)
				{
					Native.ThrowOnFailure(ppProp.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewGeneral_FontCategory, m_strFontCategoryGuid));
				}

				if (colorCategoryGuid != null && colorCategoryGuid.Length != 0)
				{
					Native.ThrowOnFailure(ppProp.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewGeneral_ColorCategory, colorCategoryGuid));
				}
			}

			Native.ThrowOnFailure(m_textView.GetScrollInfo(1, out _, out _, out _, out var piFirstVisibleUnit));
			Native.ThrowOnFailure(m_textView.SetScrollPosition(1, piFirstVisibleUnit));
			Native.ThrowOnFailure(m_textView.GetScrollInfo(0, out _, out _, out _, out piFirstVisibleUnit));
			Native.ThrowOnFailure(m_textView.SetScrollPosition(0, piFirstVisibleUnit));
			EditorHandle = m_textView.GetWindowHandle();
			ApplyInitialLanguageService();
			DoPostCreationInit();
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "ShellTextViewControl.CreateEditorWindow", "successfully created window");
		}

		private void SinkEditorEvents(bool bSink)
		{
			if (bSink && m_textView != null)
			{
				textViewEventsCookie ??= new ConnectionPointCookie(m_textView, this, typeof(IVsTextViewEvents));
			}
			else if (textViewEventsCookie != null)
			{
				textViewEventsCookie.Dispose(disposing: true);
				textViewEventsCookie = null;
			}
		}

		private void ApplyInitialLanguageService()
		{
			Tracer.Trace(GetType(), "ShellTextViewControl.ApplyInitialLanguageService", "", null);
			try
			{
				if (!Equals(_ClsidLanguageService, Guid.Empty))
				{
					ApplyLS(_ClsidLanguageService);
					return;
				}

				Guid pguidLangService = Guid.Empty;
				Native.ThrowOnFailure(((IVsTextBuffer)_textBuffer.TextStream).GetLanguageServiceID(out pguidLangService));
				if (pguidLangService == Guid.Empty)
					throw new InvalidOperationException("IVsTextBuffer:GetLanguageServiceID");

				if (pguidLangService == VS.CLSID_LanguageServiceDefault && ClsidLanguageServiceDefault != Guid.Empty)
				{
					ApplyLS(ClsidLanguageServiceDefault);
				}
				else
				{
					OnNewLangSvc(_textBuffer, new LangServiceEventArgs(pguidLangService));
				}
			}
			catch (Exception ex)
			{
				Tracer.LogExCatch(GetType(), ex);
			}
		}

		private void DoPostCreationInit()
		{
			Tracer.Trace(GetType(), "ShellTextViewControl.DoPostCreationInit", "", null);
			OnSizeChanged(new EventArgs());
			if (_bWantCustomPopupMenu)
			{
				_CommandFilter = new TextViewCommandFilter(m_textView, new int[1] { 102 });
				_CommandFilter.SpecialEditorCommand += base.OnSpecialEditorCommandEventHandler;
			}
		}

		private void OnNewLangSvc(object sender, LangServiceEventArgs a)
		{
			Tracer.Trace(GetType(), "SqlTextViewControl.OnNewLangSvc", "", null);

			if (_OleServiceProvider == null)
				return;


			IVsLanguageInfo vsLanguageInfo = (IVsLanguageInfo)_OleServiceProvider.GetService(a.ServiceGuid);
			if (vsLanguageInfo == null)
			{
				return;
			}

			try
			{
				DisposeCodeWindowManager();
				try
				{
					Native.ThrowOnFailure(vsLanguageInfo.GetCodeWindowManager(this, out vsCodeWindowManager));
					Native.ThrowOnFailure(vsCodeWindowManager.OnNewView((VsTextView)m_textView));
				}
				catch (Exception e)
				{
					Tracer.LogExCatch(GetType(), e);
				}
			}
			finally
			{
				Release(vsLanguageInfo);
			}
		}

		int IVsCodeWindow.SetBuffer(IVsTextLines buffer)
		{
			return VSConstants.E_NOTIMPL;
		}

		int IVsCodeWindow.GetBuffer(out IVsTextLines buffer)
		{
			buffer = _textBuffer.TextStream as IVsTextLines;
			return VSConstants.S_OK;
		}

		int IVsCodeWindow.GetPrimaryView(out IVsTextView view)
		{
			view = m_textView as VsTextView;
			return VSConstants.S_OK;
		}

		int IVsCodeWindow.GetSecondaryView(out IVsTextView view)
		{
			view = null;
			return VSConstants.S_OK;
		}

		int IVsCodeWindow.SetViewClassID(ref Guid clsidView)
		{
			return VSConstants.E_NOTIMPL;
		}

		int IVsCodeWindow.GetViewClassID(out Guid clsidView)
		{
			clsidView = Guid.Empty;
			return VSConstants.E_NOTIMPL;
		}

		int IVsCodeWindow.SetBaseEditorCaption(string[] baseEditorCaption)
		{
			return VSConstants.E_NOTIMPL;
		}

		int IVsCodeWindow.GetEditorCaption(READONLYSTATUS status, out string baseEditorCaption)
		{
			baseEditorCaption = string.Empty;
			return VSConstants.E_NOTIMPL;
		}

		int IVsCodeWindow.Close()
		{
			return VSConstants.E_NOTIMPL;
		}

		int IVsCodeWindow.GetLastActiveView(out IVsTextView view)
		{
			view = null;
			return VSConstants.E_NOTIMPL;
		}
	}
}
