// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.TabbedEditor.SqlEditorCodeTab

using System;
using System.Windows.Forms;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



namespace BlackbirdSql.Shared.Controls.Tabs;


public class EditorCodeTab : AbstractEditorTab
{

	public EditorCodeTab(IBsTabbedEditorPane tabbedEditor, Guid logicalView, Guid editorLogicalView, EnEditorTabType editorTabType)
		: base(tabbedEditor, logicalView, editorTabType)
	{
		_EditorLogicalView = editorLogicalView;
	}


	protected Guid _EditorLogicalView;


	private Guid _ClsidLogicalView = VSConstants.LOGVIEWID_TextView;

	public static readonly string S_FramePhysicalViewString = "CodeFrame";

	private Guid _ClsidEditorFactory = VSConstants.GUID_TextEditorFactory;

	protected override Guid ClsidLogicalView => Guid.Empty;

	protected override string GetPhysicalViewString()
	{
		return S_FramePhysicalViewString;
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

		int hresult = vsUIShellOpenDocument.IsDocumentOpen(null, 0u, documentMoniker, ref _ClsidLogicalView, 0u, out _, array, out _, out _);
		___(hresult);

		IVsWindowFrame vsWindowFrame = WindowPaneServiceProvider.GetService(typeof(SVsWindowFrame)) as IVsWindowFrame;

		Cursor current = Cursor.Current;
		Cursor.Current = Cursors.WaitCursor;

		_ = (IBsPackageController)Package.GetGlobalService(typeof(IBsAsyncPackage));
		try
		{
			ref Guid editorGuid = ref _ClsidEditorFactory;
			Guid rguidLogicalView = _ClsidLogicalView;
			__VSSPECIFICEDITORFLAGS flags = __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen
				| __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor;

			___(vsUIShellOpenDocument.OpenDocumentViaProjectWithSpecific(documentMoniker, (uint)flags, ref editorGuid, null, ref rguidLogicalView, out IOleServiceProvider ppSP, out IVsUIHierarchy ppHierOpen, out array[0], out IVsWindowFrame ppWindowFrame));

			SetFrameProperties(vsWindowFrame, ppWindowFrame);

			return ppWindowFrame;
		}
		finally
		{
			Cursor.Current = current;
		}
	}
}
