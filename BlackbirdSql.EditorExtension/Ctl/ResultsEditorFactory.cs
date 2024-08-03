// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlResultsEditorFactory

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.EditorExtension.Properties;
using BlackbirdSql.Shared.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using LibraryData = BlackbirdSql.Shared.LibraryData;



namespace BlackbirdSql.EditorExtension.Ctl;

[Guid(LibraryData.C_SqlResultsEditorFactoryGuid)]
[ProvideMenuResource("Menus.ctmenu", 1)]
// [Name("BlackbirdSql Results")]


public sealed class ResultsEditorFactory : AbstruseEditorFactory
{
	public ResultsEditorFactory() : base(withEncoding: false)
	{
	}


	public override Guid ClsidEditorFactory => new(LibraryData.C_SqlResultsEditorFactoryGuid);

	public override int CreateEditorInstance(uint createFlags, string moniker, string physicalViewName,
		IVsHierarchy hierarchy, uint itemId, IntPtr pExistingDocData, out IntPtr pDocView,
		out IntPtr pDocData, out string caption, out Guid cmdUIGuid, out int hresult)
	{
		RctManager.EnsureLoaded();

		pDocView = IntPtr.Zero;
		pDocData = IntPtr.Zero;
		caption = "";
		cmdUIGuid = Guid.Empty;
		hresult = VSConstants.S_FALSE;
		Cursor current = null;

		try
		{
			// Tracer.Trace(GetType(), "IVsEditorFactory.CreateEditorInstance", "nCreateFlags = {0}, strMoniker = {1}, strPhysicalView = {2}, itemid = {3}", createFlags, moniker, physicalView, itemId);

			if (!string.IsNullOrEmpty(physicalViewName))
				return VSConstants.E_INVALIDARG;

			if ((createFlags & 6) == 0)
				return VSConstants.E_INVALIDARG;

			if (pExistingDocData != IntPtr.Zero)
				return VSConstants.VS_E_INCOMPATIBLEDOCDATA;

			current = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;

			hresult = VSConstants.S_OK;

			ResultWindowPane resultWindowPane = CreateResultsWindowPane();
			caption = Resources.ResultsEditorFactory_Caption;
			pDocView = Marshal.GetIUnknownForObject(resultWindowPane);


			Guid clsid = typeof(VsTextBufferClass).GUID;
			Guid iid = VSConstants.IID_IUnknown;

			object objTextBuffer = EditorExtensionPackage.Instance.CreateInstance(ref clsid, ref iid, typeof(object));

			(objTextBuffer as IObjectWithSite)?.SetSite(OleServiceProvider);
			IVsTextLines vsTextLines = objTextBuffer as IVsTextLines;
			pDocData = Marshal.GetIUnknownForObject(vsTextLines);

			// pDocData = Marshal.GetIUnknownForObject(resultWindowPane); SqlEditor bug!!!

			cmdUIGuid = VSConstants.GUID_TextEditorFactory;
		}
		catch (Exception ex)
		{
			if (ex is NullReferenceException || ex is ApplicationException || ex is ArgumentException || ex is InvalidOperationException)
			{
				MessageCtl.ShowEx(Resources.ExFailedToCreateEditor, ex);
				return VSConstants.E_FAIL;
			}

			throw;
		}
		finally
		{
			if (current != null)
				Cursor.Current = current;
		}

		return VSConstants.S_OK;
	}

	public ResultWindowPane CreateResultsWindowPane()
	{
		return new ResultWindowPane();
	}
}
