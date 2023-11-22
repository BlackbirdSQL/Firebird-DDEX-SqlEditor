// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorTabBase
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Core;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.Common.Controls;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

public abstract class AbstractSqlEditorTab : AbstractEditorTab
{
	public static readonly string S_PhysicalViewString = "ResultFrame";

	public AbstractSqlEditorTab(AbstractTabbedEditorPane editorPane, Guid logicalView, EnEditorTabType editorTabType)
		: base(editorPane, logicalView, editorTabType)
	{
	}


	protected override string GetPhysicalViewString()
	{
		return S_PhysicalViewString;
	}

	protected override IVsWindowFrame CreateWindowFrame()
	{
		_ = Cursor.Current;
		IntPtr ppunkDocData = IntPtr.Zero;
		IntPtr ppunkDocView = IntPtr.Zero;
		IntPtr ppunkDocData2 = IntPtr.Zero;
		Guid rguidLogicalView = GetLogicalView();
		Guid pguidEditorType = GetEditorTabEditorFactoryGuid();
		IDisposable disposable = WaitCursorHelper.NewWaitCursor();
		try
		{
			Microsoft.VisualStudio.OLE.Interop.IServiceProvider instance = Controller.OleServiceProvider;

			if (WindowPaneServiceProvider.GetService(typeof(SVsUIShellOpenDocument)) is not IVsUIShellOpenDocument shell)
				throw new ServiceUnavailableException(typeof(IVsUIShellOpenDocument));

			if (WindowPaneServiceProvider.GetService(typeof(SVsUIShell)) is not IVsUIShell vsUIShell)
				throw new ServiceUnavailableException(typeof(IVsUIShell));

			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			IVsRunningDocumentTable vsRunningDocumentTable =
				WindowPaneServiceProvider.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable
				?? throw new ServiceUnavailableException(typeof(IVsRunningDocumentTable));

			uint[] array = new uint[1];
			string documentMoniker = DocumentMoniker;
			Guid rguidLogicalView2 = rguidLogicalView;
			Native.ThrowOnFailure(shell.IsDocumentOpen(null, 0u, documentMoniker, ref rguidLogicalView2, 0u, out var ppHierOpen, array, out IVsWindowFrame ppWindowFrame, out _), (string)null);
			ErrorHandler.ThrowOnFailure(vsRunningDocumentTable.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, documentMoniker, out var ppHier, out var pitemid, out ppunkDocData, out _));

			Native.ThrowOnFailure(shell.GetStandardEditorFactory(VS.dwReserved, ref pguidEditorType, null, ref rguidLogicalView, out var pbstrPhysicalView, out var ppEF), (string)null);
			Native.ThrowOnFailure(ppEF.CreateEditorInstance((uint)(__VSCREATEEDITORFLAGS.CEF_OPENFILE | __VSCREATEEDITORFLAGS.CEF_SILENT), documentMoniker, pbstrPhysicalView, ppHierOpen, array[0], ppunkDocData2, out ppunkDocView, out ppunkDocData2, out _, out var pguidCmdUI, out var pgrfCDW), (string)null);
			Native.ThrowOnFailure(vsUIShell.CreateDocumentWindow((uint)((ulong)pgrfCDW | 0x20uL | 0xFFFFF | 0x400000), documentMoniker, ppHierOpen ?? ppHier as IVsUIHierarchy, pitemid, ppunkDocView, ppunkDocData2, ref pguidEditorType, pbstrPhysicalView, ref pguidCmdUI, instance, string.Empty, string.Empty, null, out ppWindowFrame), (string)null);

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
			disposable?.Dispose();
			if (ppunkDocData != IntPtr.Zero)
			{
				Marshal.Release(ppunkDocData);
			}

			if (ppunkDocView != IntPtr.Zero)
			{
				Marshal.Release(ppunkDocView);
			}

			if (ppunkDocData2 != IntPtr.Zero)
			{
				Marshal.Release(ppunkDocData2);
			}
		}
	}

	protected virtual Guid GetEditorTabEditorFactoryGuid()
	{
		return Guid.Empty;
	}

	protected override Guid GetEditorFactoryGuid()
	{
		return new Guid(SystemData.DslEditorFactoryGuid);
	}

	protected abstract Guid GetLogicalView();
}
