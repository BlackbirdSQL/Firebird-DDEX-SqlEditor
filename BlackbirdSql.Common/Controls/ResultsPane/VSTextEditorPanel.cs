// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.VSTextEditorPanel

using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Enums;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using Tracer = BlackbirdSql.Core.Ctl.Diagnostics.Tracer;


namespace BlackbirdSql.Common.Controls.ResultsPane
{
	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
		Justification = "Class is UIThread compliant.")]

	public class VSTextEditorPanel : AbstractResultsPanel, IOleCommandTarget
	{
		protected TextResultsViewContol _TextViewCtl;

		protected ShellBufferWriter _TextWriter;

		protected Guid _ClsidLanguageService = Guid.Empty;

		private readonly bool _xmlEditor;

		private bool _shouldBeReadOnly = true;



		public TextResultsViewContol TextViewCtl => _TextViewCtl;

		public bool HasValidWriter
		{
			get
			{
				if (_TextViewCtl != null && _TextViewCtl.TextBuffer != null)
				{
					return _TextWriter != null;
				}

				return false;
			}
		}

		public ShellBufferWriter ResultsWriter
		{
			get
			{
				if (_TextWriter == null)
				{
					Exception ex = new InvalidOperationException(ControlsResources.ErrTextWriterNull);
					Tracer.LogExThrow(GetType(), ex);
					throw ex;
				}

				return _TextWriter;
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
				_ClsidLanguageService = value;
			}
		}

		public bool UndoEnabled
		{
			get
			{
				if (_TextViewCtl != null && _TextViewCtl.TextBuffer != null)
				{
					return _TextViewCtl.TextBuffer.UndoEnabled;
				}

				return false;
			}
			set
			{
				if (_TextViewCtl != null && _TextViewCtl.TextBuffer != null)
				{
					_TextViewCtl.TextBuffer.UndoEnabled = value;
				}
			}
		}

		public bool TextReadOnly
		{
			get
			{
				return _shouldBeReadOnly;
			}
			set
			{
				if (value == _shouldBeReadOnly)
				{
					return;
				}

				_shouldBeReadOnly = value;
				if (_TextViewCtl.IsHandleCreated)
				{
					Native.ThrowOnFailure(_TextViewCtl.TextBuffer.TextStream.GetStateFlags(out uint pdwReadOnlyFlags));
					uint num = !value ? pdwReadOnlyFlags & 0xFFFFFFFEu : pdwReadOnlyFlags | (uint)BUFFERSTATEFLAGS.BSF_USER_READONLY;
					if (num != pdwReadOnlyFlags)
					{
						Native.ThrowOnFailure(_TextViewCtl.TextBuffer.TextStream.SetStateFlags(num));
					}
				}
			}
		}

		public VSTextEditorPanel(string defaultResultsDirectory, bool xmlEditor)
			: base(defaultResultsDirectory)
		{
			_xmlEditor = xmlEditor;
			SuspendLayout();
			using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
			{
				_TextViewCtl = new TextResultsViewContol();
			}

			_TextViewCtl.BorderStyle = BorderStyle.None;
			_TextViewCtl.CodeWindowStyle = false;
			_TextViewCtl.Dock = DockStyle.Fill;
			_TextViewCtl.Text = "shellTextViewControl1";
			_TextViewCtl.WithSelectionMargin = true;
			_TextViewCtl.WithWidgetMargin = false;
			_TextViewCtl.WantCustomPopupMenu = true;
			_TextViewCtl.ShowPopupMenuEvent += OnShowPopupMenu;
			Controls.Add(_TextViewCtl);
			ResumeLayout(performLayout: false);
			MenuCommand menuCommand = new MenuCommand(OnSaveAs,
				new CommandID(LibraryData.CLSID_CommandSet,
				(int)EnCommandSet.CmdIdSaveResultsAs));
			MenuService.AddRange(new MenuCommand[1] { menuCommand });
		}

		protected override void Dispose(bool bDisposing)
		{
			Tracer.Trace(GetType(), "VSTextEditorPanel.Dispose", "", null);
			if (bDisposing && _TextViewCtl != null)
			{
				_TextViewCtl.Dispose();
				_TextViewCtl = null;
			}

			if (_TextWriter != null)
			{
				_TextWriter = null;
			}

			base.Dispose(bDisposing);
		}

		public override void Initialize(object sp)
		{
			Tracer.Trace(GetType(), "VSTextEditorTabPage.Initialize", "", null);

			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			base.Initialize(sp);
			if (_ClsidLanguageService != Guid.Empty)
			{
				_TextViewCtl.ClsidLanguageService = _ClsidLanguageService;
				TextResultsViewContol textView = _TextViewCtl;
				Guid fontAndColorCategoryStandardTextEditor = VS.CLSID_FontAndColorsTextEditorCategory;
				textView.ColorCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
				TextResultsViewContol textView2 = _TextViewCtl;
				fontAndColorCategoryStandardTextEditor = VS.CLSID_FontAndColorsTextEditorCategory;
				textView2.FontCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
			}
			else
			{
				TextResultsViewContol textView3 = _TextViewCtl;
				Guid fontAndColorCategoryStandardTextEditor = VS.CLSID_FontAndColorsSqlResultsTextCategory;
				textView3.ColorCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
				TextResultsViewContol textView4 = _TextViewCtl;
				fontAndColorCategoryStandardTextEditor = VS.CLSID_FontAndColorsSqlResultsTextCategory;
				textView4.FontCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
			}

			_TextViewCtl.CreateAndInitTextBuffer(sp, null);
			_TextWriter = new ShellBufferWriter(_TextViewCtl.TextBuffer);
			CreateAndInitVSTextEditor();
			if (_serviceProvider.GetService(VS.CLSID_TextManager) is not IVsTextManager vsTextManager)
			{
				return;
			}

			try
			{
				Native.ThrowOnFailure(vsTextManager.GetRegisteredMarkerTypeID(ref VS.CLSID_TSqlEditorMessageErrorMarker, out ShellTextBuffer.markerTypeError));
			}
			catch
			{
			}
			finally
			{
				Marshal.ReleaseComObject(vsTextManager);
			}
		}

		public override void Clear()
		{
			Tracer.Trace(GetType(), "VSTextEditorTabPage.Clear", "", null);
			_TextViewCtl?.TextBuffer.Clear();

			_TextWriter?.Reset();
		}

		public void ScrollTextViewToMaxScrollUnit()
		{
			Tracer.Trace(GetType(), "VSTextEditorTabPage.ScrollTextViewToMaxScrollUnit", "", null);
			if (!IsHandleCreated)
			{
				CreateHandle();
			}

			Native.ThrowOnFailure(_TextViewCtl.TextView.GetScrollInfo(1, out var _, out var piMaxUnit, out var piVisibleUnits, out var _));
			int iFirstVisibleUnit = Math.Max(0, piMaxUnit - piVisibleUnits);
			Native.ThrowOnFailure(_TextViewCtl.TextView.SetScrollPosition(1, iFirstVisibleUnit));
		}

		private void OnShowPopupMenu(object sender, SpecialEditorCommandEventArgs a)
		{
			_TextViewCtl.GetCoordinatesForPopupMenu(a.VariantIn, out var x, out var y);
			CommonUtils.ShowContextMenuEvent((int)EnCommandSet.ContextIdMessageWindow, x, y, this);
		}

		private void CreateAndInitVSTextEditor()
		{
			Tracer.Trace(GetType(), "VSTextEditorTabPage.CreateAndInitVSTextEditor", "", null);
			_TextViewCtl.CreateAndInitEditorWindow(_ObjServiceProvider);
			if (_shouldBeReadOnly)
			{
				Native.ThrowOnFailure(_TextViewCtl.TextBuffer.TextStream.GetStateFlags(out uint pdwReadOnlyFlags));
				Native.ThrowOnFailure(_TextViewCtl.TextBuffer.TextStream.SetStateFlags(pdwReadOnlyFlags | (uint)BUFFERSTATEFLAGS.BSF_USER_READONLY));
			}
		}

		public int QueryStatus(ref Guid guidGroup, uint cmdId, OLECMD[] oleCmd, IntPtr oleText)
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			int num = TextViewCtl.QueryStatus(ref guidGroup, cmdId, oleCmd, oleText);

			if (num == 0)
				return num;

			CommandID commandID = new CommandID(guidGroup, (int)oleCmd[0].cmdID);
			MenuCommand menuCommand = MenuService.FindCommand(commandID);
			if (menuCommand == null)
			{
				return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;
			}

			if (guidGroup.Equals(LibraryData.CLSID_CommandSet))
			{
				bool visible = menuCommand.Supported = true;
				menuCommand.Visible = visible;
				if (commandID.ID == (int)EnCommandSet.CmdIdSaveResultsAs)
				{
					menuCommand.Enabled = true;
				}

				oleCmd[0].cmdf = (uint)menuCommand.OleStatus;
				return VSConstants.S_OK;
			}

			return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;
		}

		public int Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr variantIn, IntPtr variantOut)
		{
			MenuCommand menuCommand = MenuService.FindCommand(new CommandID(guidGroup, (int)nCmdId));

			if (menuCommand != null && guidGroup.Equals(LibraryData.CLSID_CommandSet))
			{
				menuCommand.Invoke();
				return VSConstants.S_OK;
			}

			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			return _TextViewCtl.Exec(ref guidGroup, nCmdId, nCmdExcept, variantIn, variantOut);
		}

		private void OnSaveAs(object sender, EventArgs a)
		{
			Tracer.Trace(GetType(), "VSTextEditorTabPage.OnSaveAs", "", null);
			Control textView = _TextViewCtl;
			Cursor current = Cursor.Current;
			try
			{
				if (textView == null)
				{
					return;
				}

				string intialDirectory = DefaultResultsDirectory;
				TextWriter textWriterForQueryResultsToFile = CommonUtils.GetTextWriterForQueryResultsToFile(_xmlEditor, ref intialDirectory);
				if (textWriterForQueryResultsToFile != null)
				{
					try
					{
						Cursor.Current = Cursors.WaitCursor;
						ShellTextViewControl shellTextViewControl = (ShellTextViewControl)textView;
						textWriterForQueryResultsToFile.Write(shellTextViewControl.TextBuffer.Text);
						textWriterForQueryResultsToFile.Flush();
						DefaultResultsDirectory = intialDirectory;
					}
					catch (Exception e)
					{
						Tracer.LogExCatch(GetType(), e);
						Cmd.ShowExceptionInDialog(ControlsResources.ErrWhileSavingResults, e);
					}
				}
			}
			finally
			{
				Cursor.Current = current;
			}
		}
	}
}
