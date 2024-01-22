#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Interfaces;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common.Controls;

public class SqlEditorCodeTab(AbstractTabbedEditorPane editorPane, Guid logicalView,
		Guid editorLogicalView, EnEditorTabType editorTabType)
	: AbstractSqlEditorTab(editorPane, logicalView, editorTabType)
{
	protected Guid _EditorLogicalView = editorLogicalView;



	private Guid _ClsidLogicalView = VSConstants.LOGVIEWID_TextView;

	public static readonly string S_FramePhysicalViewString = "CodeFrame";

	private Guid _ClsidEditorFactory = VSConstants.GUID_TextEditorFactory;

	protected override string GetPhysicalViewString()
	{
		return S_FramePhysicalViewString;
	}

	protected override Guid GetLogicalView()
	{
		return Guid.Empty;
	}

	protected override IVsWindowFrame CreateWindowFrame()
	{
		if (WindowPaneServiceProvider.GetService(typeof(SVsUIShellOpenDocument))
			is not IVsUIShellOpenDocument vsUIShellOpenDocument)
		{
			throw Diag.ExceptionService(typeof(IVsUIShellOpenDocument));
		}

		Diag.ThrowIfNotOnUIThread();

		uint[] array = new uint[1];
		string documentMoniker = DocumentMoniker;
		Native.ThrowOnFailure(vsUIShellOpenDocument.IsDocumentOpen(null, 0u, documentMoniker, ref _ClsidLogicalView, 0u, out var _, array, out var _, out var _), (string)null);
		IVsWindowFrame vsWindowFrame = WindowPaneServiceProvider.GetService(typeof(SVsWindowFrame)) as IVsWindowFrame;
		Cursor current = Cursor.Current;
		Cursor.Current = Cursors.WaitCursor;

		_ = (IBPackageController)Package.GetGlobalService(typeof(IBAsyncPackage)); 
		try
		{
			ref Guid editorGuid = ref _ClsidEditorFactory;
			Guid rguidLogicalView = _ClsidLogicalView;
			__VSSPECIFICEDITORFLAGS flags = __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen
				| __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor;

			Native.ThrowOnFailure(vsUIShellOpenDocument.OpenDocumentViaProjectWithSpecific(documentMoniker, (uint)flags, ref editorGuid, null, ref rguidLogicalView, out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP, out IVsUIHierarchy ppHierOpen, out array[0], out IVsWindowFrame ppWindowFrame), (string)null);

			SetFrameProperties(vsWindowFrame, ppWindowFrame);

			return ppWindowFrame;
		}
		finally
		{
			Cursor.Current = current;
		}
	}
}
