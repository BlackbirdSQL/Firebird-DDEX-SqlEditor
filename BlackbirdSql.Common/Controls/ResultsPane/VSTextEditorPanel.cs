// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.VSTextEditorPanel

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Events;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using Tracer = BlackbirdSql.Core.Diagnostics.Tracer;


namespace BlackbirdSql.Common.Controls.ResultsPane
{
	public class VSTextEditorPanel : AbstractResultsPanel, IOleCommandTarget
	{
		protected TextResultsViewContol _TextView;

		protected ShellBufferWriter _TextWriter;

		protected Guid _ClsidLanguageService = Guid.Empty;

		private readonly bool _xmlEditor;

		private bool _shouldBeReadOnly = true;



		public TextResultsViewContol TextView => _TextView;

		public bool HasValidWriter
		{
			get
			{
				if (_TextView != null && _TextView.TextBuffer != null)
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
					Exception ex = new InvalidOperationException(SharedResx.ErrTextWriterNull);
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
				if (_TextView != null && _TextView.TextBuffer != null)
				{
					return _TextView.TextBuffer.UndoEnabled;
				}

				return false;
			}
			set
			{
				if (_TextView != null && _TextView.TextBuffer != null)
				{
					_TextView.TextBuffer.UndoEnabled = value;
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
				if (_TextView.IsHandleCreated)
				{
					Native.ThrowOnFailure(_TextView.TextBuffer.TextStream.GetStateFlags(out uint pdwReadOnlyFlags));
					uint num = !value ? pdwReadOnlyFlags & 0xFFFFFFFEu : pdwReadOnlyFlags | (uint)BUFFERSTATEFLAGS.BSF_USER_READONLY;
					if (num != pdwReadOnlyFlags)
					{
						Native.ThrowOnFailure(_TextView.TextBuffer.TextStream.SetStateFlags(num));
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
				_TextView = new TextResultsViewContol();
			}

			_TextView.BorderStyle = BorderStyle.None;
			_TextView.CodeWindowStyle = false;
			_TextView.Dock = DockStyle.Fill;
			_TextView.Text = "shellTextViewControl1";
			_TextView.WithSelectionMargin = true;
			_TextView.WithWidgetMargin = false;
			_TextView.WantCustomPopupMenu = true;
			_TextView.ShowPopupMenu += OnShowPopupMenu;
			Controls.Add(_TextView);
			ResumeLayout(performLayout: false);
			MenuCommand menuCommand = new MenuCommand(OnSaveAs,
				new CommandID(LibraryData.CLSID_SqlEditorCommandSet,
				(int)EnCommandSet.CmdidSaveResultsAs));
			MenuService.AddRange(new MenuCommand[1] { menuCommand });
		}

		protected override void Dispose(bool bDisposing)
		{
			Tracer.Trace(GetType(), "VSTextEditorPanel.Dispose", "", null);
			if (bDisposing && _TextView != null)
			{
				_TextView.Dispose();
				_TextView = null;
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
			base.Initialize(sp);
			if (_ClsidLanguageService != Guid.Empty)
			{
				_TextView.ClsidLanguageService = _ClsidLanguageService;
				TextResultsViewContol textView = _TextView;
				Guid fontAndColorCategoryStandardTextEditor = VS.CLSID_FontAndColorsTextEditorCategory;
				textView.ColorCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
				TextResultsViewContol textView2 = _TextView;
				fontAndColorCategoryStandardTextEditor = VS.CLSID_FontAndColorsTextEditorCategory;
				textView2.FontCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
			}
			else
			{
				TextResultsViewContol textView3 = _TextView;
				Guid fontAndColorCategoryStandardTextEditor = VS.CLSID_FontAndColorsSqlResultsTextCategory;
				textView3.ColorCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
				TextResultsViewContol textView4 = _TextView;
				fontAndColorCategoryStandardTextEditor = VS.CLSID_FontAndColorsSqlResultsTextCategory;
				textView4.FontCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
			}

			_TextView.CreateAndInitTextBuffer(sp, null);
			_TextWriter = new ShellBufferWriter(_TextView.TextBuffer);
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
			_TextView?.TextBuffer.Clear();

			_TextWriter?.Reset();
		}

		public void ScrollTextViewToMaxScrollUnit()
		{
			Tracer.Trace(GetType(), "VSTextEditorTabPage.ScrollTextViewToMaxScrollUnit", "", null);
			if (!IsHandleCreated)
			{
				CreateHandle();
			}

			Native.ThrowOnFailure(_TextView.TextView.GetScrollInfo(1, out var _, out var piMaxUnit, out var piVisibleUnits, out var _));
			int iFirstVisibleUnit = Math.Max(0, piMaxUnit - piVisibleUnits);
			Native.ThrowOnFailure(_TextView.TextView.SetScrollPosition(1, iFirstVisibleUnit));
		}

		private void OnShowPopupMenu(object sender, SpecialEditorCommandEventArgs a)
		{
			_TextView.GetCoordinatesForPopupMenu(a.VariantIn, out var x, out var y);
			CommonUtils.ShowContextMenu((int)EnCommandSet.ContextIdMessageWindow, x, y, this);
		}

		private void CreateAndInitVSTextEditor()
		{
			Tracer.Trace(GetType(), "VSTextEditorTabPage.CreateAndInitVSTextEditor", "", null);
			_TextView.CreateAndInitEditorWindow(_rawServiceProvider);
			if (_shouldBeReadOnly)
			{
				Native.ThrowOnFailure(_TextView.TextBuffer.TextStream.GetStateFlags(out uint pdwReadOnlyFlags));
				Native.ThrowOnFailure(_TextView.TextBuffer.TextStream.SetStateFlags(pdwReadOnlyFlags | (uint)BUFFERSTATEFLAGS.BSF_USER_READONLY));
			}
		}

		public int QueryStatus(ref Guid guidGroup, uint cmdId, OLECMD[] oleCmd, IntPtr oleText)
		{
			int num = TextView.QueryStatus(ref guidGroup, cmdId, oleCmd, oleText);

			if (num == 0)
				return num;

			CommandID commandID = new CommandID(guidGroup, (int)oleCmd[0].cmdID);
			MenuCommand menuCommand = MenuService.FindCommand(commandID);
			if (menuCommand == null)
			{
				return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;
			}

			if (guidGroup.Equals(LibraryData.CLSID_SqlEditorCommandSet))
			{
				bool visible = menuCommand.Supported = true;
				menuCommand.Visible = visible;
				if (commandID.ID == (int)EnCommandSet.CmdidSaveResultsAs)
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
			if (menuCommand != null && guidGroup.Equals(LibraryData.CLSID_SqlEditorCommandSet))
			{
				menuCommand.Invoke();
				return VSConstants.S_OK;
			}

			return _TextView.Exec(ref guidGroup, nCmdId, nCmdExcept, variantIn, variantOut);
		}

		private void OnSaveAs(object sender, EventArgs a)
		{
			Tracer.Trace(GetType(), "VSTextEditorTabPage.OnSaveAs", "", null);
			Control textView = _TextView;
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
						Cmd.ShowExceptionInDialog(SharedResx.ErrWhileSavingResults, e);
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
