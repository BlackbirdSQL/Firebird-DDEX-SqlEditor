// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.VSTextEditorPanel

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Ctl.IO;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls;
using BlackbirdSql.Core.Ctl.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.Common.Controls.ResultsPanels;


public class VSTextEditorPanel : AbstractResultsPanel, IOleCommandTarget
{

	public VSTextEditorPanel(string defaultResultsDirectory, bool xmlEditor) : base(defaultResultsDirectory)
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
		// Tracer.Trace(GetType(), "VSTextEditorPanel.Dispose", "", null);
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
				Diag.ThrowException(ex);
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
				_ = ___(_TextViewCtl.TextBuffer.TextStream.GetStateFlags(out uint pdwReadOnlyFlags));
				uint num = !value ? pdwReadOnlyFlags & 0xFFFFFFFEu : pdwReadOnlyFlags | (uint)BUFFERSTATEFLAGS.BSF_USER_READONLY;
				if (num != pdwReadOnlyFlags)
				{
					_ = ___(_TextViewCtl.TextBuffer.TextStream.SetStateFlags(num));
				}
			}
		}
	}


	public override void Initialize(object sp)
	{
		// Tracer.Trace(GetType(), "VSTextEditorTabPage.Initialize", "", null);

		Diag.ThrowIfNotOnUIThread();

		base.Initialize(sp);
		if (_ClsidLanguageService != Guid.Empty)
		{
			_TextViewCtl.ClsidLanguageService = _ClsidLanguageService;
			TextResultsViewContol textView = _TextViewCtl;
			Guid fontAndColorCategoryStandardTextEditor = Core.VS.CLSID_FontAndColorsTextEditorCategory;
			textView.ColorCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
			TextResultsViewContol textView2 = _TextViewCtl;
			fontAndColorCategoryStandardTextEditor = Core.VS.CLSID_FontAndColorsTextEditorCategory;
			textView2.FontCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
		}
		else
		{
			TextResultsViewContol textView3 = _TextViewCtl;
			Guid fontAndColorCategoryStandardTextEditor = Core.VS.CLSID_FontAndColorsSqlResultsTextCategory;
			textView3.ColorCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
			TextResultsViewContol textView4 = _TextViewCtl;
			fontAndColorCategoryStandardTextEditor = Core.VS.CLSID_FontAndColorsSqlResultsTextCategory;
			textView4.FontCategoryGuid = "{" + fontAndColorCategoryStandardTextEditor.ToString() + "}";
		}

		_TextViewCtl.CreateAndInitTextBuffer(sp, null);
		_TextWriter = new ShellBufferWriter(_TextViewCtl.TextBuffer);
		CreateAndInitVSTextEditor();
		if (_ServiceProvider.GetService(Core.VS.CLSID_TextManager) is not IVsTextManager vsTextManager)
		{
			return;
		}

		try
		{
			_ = ___(vsTextManager.GetRegisteredMarkerTypeID(ref Core.VS.CLSID_TSqlEditorMessageErrorMarker, out ShellTextBuffer.markerTypeError));
		}
		catch
		{
		}
		finally
		{
			_ = Marshal.ReleaseComObject(vsTextManager);
		}
	}

	public override void Clear()
	{
		// Tracer.Trace(GetType(), "VSTextEditorTabPage.Clear", "", null);
		_TextViewCtl?.TextBuffer.Clear();

		_TextWriter?.Reset();
	}

	public void ScrollTextViewToMaxScrollUnit()
	{
		if (!IsHandleCreated)
		{
			CreateHandle();
		}

		_ = Task.Run(ScrollTextViewToMaxScrollUnitAsync);
	}



	protected async Task<bool> ScrollTextViewToMaxScrollUnitAsync()
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		___(_TextViewCtl.TextView.GetScrollInfo(1, out _, out var piMaxUnit, out var piVisibleUnits, out _));

		int iFirstVisibleUnit = Math.Max(0, piMaxUnit - piVisibleUnits);

		___(_TextViewCtl.TextView.SetScrollPosition(1, iFirstVisibleUnit));

		return true;
	}



	public void ScrollTextViewToTop()
	{
		if (!IsHandleCreated)
		{
			CreateHandle();
		}

		_ = Task.Run(() => ScrollTextViewToTopAsync());
	}



	protected async Task<bool> ScrollTextViewToTopAsync()
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		___(_TextViewCtl.TextView.SetScrollPosition(1, 0));

		return true;
	}



	private void OnShowPopupMenu(object sender, SpecialEditorCommandEventArgs a)
	{
		_TextViewCtl.GetCoordinatesForPopupMenu(a.VariantIn, out var x, out var y);
		VS.ShowContextMenuEvent((int)EnCommandSet.ContextIdMessageWindow, x, y, this);
	}

	private void CreateAndInitVSTextEditor()
	{
		// Tracer.Trace(GetType(), "VSTextEditorTabPage.CreateAndInitVSTextEditor", "", null);
		_TextViewCtl.CreateAndInitEditorWindow(_ObjServiceProvider);
		if (_shouldBeReadOnly)
		{
			_ = ___(_TextViewCtl.TextBuffer.TextStream.GetStateFlags(out uint pdwReadOnlyFlags));
			_ = ___(_TextViewCtl.TextBuffer.TextStream.SetStateFlags(pdwReadOnlyFlags | (uint)BUFFERSTATEFLAGS.BSF_USER_READONLY));
		}
	}

	public int QueryStatus(ref Guid guidGroup, uint cmdId, OLECMD[] oleCmd, IntPtr oleText)
	{
		Diag.ThrowIfNotOnUIThread();

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

		Diag.ThrowIfNotOnUIThread();

		return _TextViewCtl.Exec(ref guidGroup, nCmdId, nCmdExcept, variantIn, variantOut);
	}

	private void OnSaveAs(object sender, EventArgs a)
	{
		// Tracer.Trace(GetType(), "OnSaveAs()");

		Control textView = _TextViewCtl;
		Cursor current = Cursor.Current;

		try
		{
			if (textView == null)
			{
				return;
			}

			string intialDirectory = DefaultResultsDirectory;
			TextWriter textWriterForQueryResultsToFile = VS.GetTextWriterForQueryResultsToFile(_xmlEditor, ref intialDirectory);
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
					Diag.Dug(e);
					MessageCtl.ShowEx(ControlsResources.ErrWhileSavingResults, e);
				}
			}
		}
		finally
		{
			Cursor.Current = current;
		}
	}
}
