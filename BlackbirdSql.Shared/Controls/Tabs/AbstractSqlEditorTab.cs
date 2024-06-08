// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorTabBase

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Controls.Tabs;


public abstract class AbstractSqlEditorTab : AbstractEditorTab
{

	public AbstractSqlEditorTab(AbstractTabbedEditorWindowPane editorPane, Guid logicalView, EnEditorTabType editorTabType)
		: base(editorPane, logicalView, editorTabType)
	{

	}


	public static readonly string S_PhysicalViewString = "ResultFrame";

	protected virtual Guid ClsidEditorTabEditorFactory => Guid.Empty;
	protected override Guid ClsidEditorFactory => new(SystemData.EditorFactoryGuid);
	protected abstract Guid ClsidLogicalView { get; }


	protected override string GetPhysicalViewString()
	{
		return S_PhysicalViewString;
	}

	protected override IVsWindowFrame CreateWindowFrame()
	{
		Diag.ThrowIfNotOnUIThread();

		if (WindowPaneServiceProvider.GetService(typeof(SVsUIShellOpenDocument)) is not IVsUIShellOpenDocument shellOpenDocumentSvc)
			throw Diag.ExceptionService(typeof(IVsUIShellOpenDocument));

		if (WindowPaneServiceProvider.GetService(typeof(SVsUIShell)) is not IVsUIShell shellSvc)
			throw Diag.ExceptionService(typeof(IVsUIShell));

		if (!RdtManager.ServiceAvailable)
			throw Diag.ExceptionService(typeof(IVsRunningDocumentTable));

		_ = Cursor.Current;
		IntPtr ppunkDocData = IntPtr.Zero;
		IntPtr ppunkDocView = IntPtr.Zero;
		IntPtr ppunkDocDataExisting = IntPtr.Zero;
		Guid rguidLogicalView = ClsidLogicalView;
		Guid pguidEditorType = ClsidEditorTabEditorFactory;

		IDisposable disposableWaitCursor = WaitCursorHelper.NewWaitCursor();

		try
		{
			int hresult;
			uint[] pitemidOpen = new uint[1];
			string documentMoniker = DocumentMoniker;
			Guid rguidLogicalView2 = rguidLogicalView;


			hresult = shellOpenDocumentSvc.IsDocumentOpen(null, 0u, documentMoniker, ref rguidLogicalView2,
				0u, out IVsUIHierarchy ppHierOpen, pitemidOpen, out IVsWindowFrame ppWindowFrame, out _);
			___(hresult);

			hresult = RdtManager.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, documentMoniker,
				out IVsHierarchy ppHier, out uint pitemid, out ppunkDocData, out _);
			___(hresult);

			hresult = shellOpenDocumentSvc.GetStandardEditorFactory(VS.dwReserved, ref pguidEditorType, null,
				ref rguidLogicalView, out string pbstrPhysicalView, out IVsEditorFactory ppEditorFactory);
			___(hresult);


			uint createFlags = (uint)(__VSCREATEEDITORFLAGS.CEF_OPENFILE | __VSCREATEEDITORFLAGS.CEF_SILENT);

			hresult = ppEditorFactory.CreateEditorInstance(createFlags, documentMoniker, pbstrPhysicalView,
				ppHierOpen, pitemidOpen[0], ppunkDocDataExisting, out ppunkDocView, out ppunkDocDataExisting,
				out _, out Guid pguidCmdUI, out int pgrfCDW);
			___(hresult);



			createFlags = (uint)((ulong)pgrfCDW | (ulong)_VSRDTFLAGS.RDT_DontSave
				| (ulong)__VSCREATEDOCWIN.CDW_RDTFLAGS_MASK | (ulong)__VSCREATEDOCWIN.CDW_fCreateNewWindow);
			IVsUIHierarchy uiHierarchy = ppHierOpen ?? ppHier as IVsUIHierarchy;

			hresult = shellSvc.CreateDocumentWindow(createFlags, documentMoniker, uiHierarchy, pitemid, ppunkDocView,
				ppunkDocDataExisting, ref pguidEditorType, pbstrPhysicalView, ref pguidCmdUI, ApcManager.OleServiceProvider,
				string.Empty, string.Empty, null, out ppWindowFrame);
			___(hresult);

			IVsWindowFrame vsWindowFrame = WindowPaneServiceProvider.GetService(typeof(SVsWindowFrame)) as IVsWindowFrame
				?? throw new ServiceUnavailableException(typeof(IVsWindowFrame));

			SetFrameProperties(vsWindowFrame, ppWindowFrame);

			return ppWindowFrame;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			disposableWaitCursor?.Dispose();

			if (ppunkDocData != IntPtr.Zero)
				Marshal.Release(ppunkDocData);

			if (ppunkDocView != IntPtr.Zero)
				Marshal.Release(ppunkDocView);

			if (ppunkDocDataExisting != IntPtr.Zero)
				Marshal.Release(ppunkDocDataExisting);
		}
	}



}
