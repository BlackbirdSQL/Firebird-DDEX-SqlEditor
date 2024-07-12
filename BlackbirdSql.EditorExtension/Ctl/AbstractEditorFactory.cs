// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.BaseEditorFactory/SqlEditorFactory

using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.EditorExtension.Ctl;


public abstract class AbstractEditorFactory : AbstruseEditorFactory
{

	public AbstractEditorFactory(bool withEncoding) : base(withEncoding)
	{

	}




	protected static volatile int _EditorId;



	public override Guid ClsidEditorFactory
	{
		get
		{
			if (WithEncoding)
			{
				return new Guid(SystemData.EditorEncodedFactoryGuid);
			}

			return new Guid(SystemData.EditorFactoryGuid);
		}
	}

	protected string EditorId { get; set; }


	public override int CreateEditorInstance(uint createFlags, string moniker, string physicalViewName,
		IVsHierarchy hierarchy, uint itemId, IntPtr pExistingDocData, out IntPtr pDocView,
		out IntPtr pDocData, out string caption, out Guid cmdUIGuid, out int hresult)
	{

		pDocView = IntPtr.Zero;
		pDocData = IntPtr.Zero;
		caption = "";
		cmdUIGuid = Guid.Empty;
		hresult = VSConstants.S_FALSE;

		if (ApcManager.SolutionClosing)
			return VSConstants.E_FAIL;

		RctManager.EnsureLoaded();

		EditorId = "Editor" + _EditorId++;


		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			Cursor current = Cursor.Current;

			try
			{
				if (!string.IsNullOrEmpty(physicalViewName) && physicalViewName != "CodeFrame")
				{
					ArgumentException ex = new("physicalView is not CodeFrame or empty: " + physicalViewName);
					Diag.Dug(ex);
					return VSConstants.E_INVALIDARG;
				}

				uint flagSilentOrOpen = (uint)(__VSCREATEEDITORFLAGS.CEF_OPENFILE | __VSCREATEEDITORFLAGS.CEF_SILENT);

				if ((createFlags & flagSilentOrOpen) == 0)
				{
					ArgumentException ex = new("invalid create flags: " + createFlags);
					Diag.Dug(ex);
					return VSConstants.E_INVALIDARG;
				}

				IVsTextLines vsTextLines = null;

				if (pExistingDocData != IntPtr.Zero)
				{
					if (WithEncoding)
					{
						DataException ex = new("Data is not encoding compatible");
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					object objExistingDocData = Marshal.GetObjectForIUnknown(pExistingDocData);
					vsTextLines = objExistingDocData as IVsTextLines;

					if (vsTextLines == null)
					{
						DataException ex = new("Data is not IVsTextLines compatible");
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					if (objExistingDocData is not IVsUserData)
					{
						DataException ex = new("Data is not IVsUserData compatible");
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					Guid clsidExistingLangService = Guid.Empty;
					vsTextLines.GetLanguageServiceID(out clsidExistingLangService);

					if (!(clsidExistingLangService == MandatedSqlLanguageServiceClsid)
						&& !(clsidExistingLangService == VS.CLSID_LanguageServiceDefault))
					{
						InvalidOperationException ex = new("Invalid language service: " + clsidExistingLangService);
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

				}

				hresult = VSConstants.S_OK;

				Cursor.Current = Cursors.WaitCursor;
				IVsTextLines vsTextLines2 = null;

				if (vsTextLines == null)
				{
					Diag.ThrowIfNotOnUIThread();

					Guid clsid = typeof(VsTextBufferClass).GUID;
					Guid iid = VSConstants.IID_IUnknown;
					object objTextBuffer = EditorExtensionPackage.Instance.CreateInstance(ref clsid, ref iid, typeof(object));

					if (WithEncoding)
					{
						IVsUserData vsUserData = objTextBuffer as IVsUserData;
						Guid riidKey = VSConstants.VsTextBufferUserDataGuid.VsBufferEncodingPromptOnLoad_guid;
						vsUserData.SetData(ref riidKey, 1u);
					}

					(objTextBuffer as IObjectWithSite)?.SetSite(OleServiceProvider);
					vsTextLines2 = objTextBuffer as IVsTextLines;
				}
				else
				{
					vsTextLines2 = vsTextLines;
				}

				if (vsTextLines2 == null)
					return VSConstants.E_FAIL;

				EnsureAuxilliaryDocData(hierarchy, moniker, vsTextLines2);


				TabbedEditorWindowPane editorPane = CreateTabbedEditorPane(vsTextLines2, moniker);


				pDocView = Marshal.GetIUnknownForObject(editorPane);
				pDocData = Marshal.GetIUnknownForObject(vsTextLines2);
				caption = string.Empty;

				cmdUIGuid = VSConstants.GUID_TextEditorFactory;

				Guid clsidLangService = MandatedSqlLanguageServiceClsid;

				vsTextLines2.SetLanguageServiceID(ref clsidLangService);
				IVsUserData vsUserData2 = (IVsUserData)vsTextLines2;
				Guid clsidDetectLang = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
				___(vsUserData2.SetData(ref clsidDetectLang, false));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				if (ex is NullReferenceException || ex is ApplicationException || ex is ArgumentException || ex is InvalidOperationException)
				{
					MessageCtl.ShowEx(SharedResx.BaseEditorFactory_FailedToCreateEditor, ex);
					return VSConstants.E_FAIL;
				}

				throw;
			}
			finally
			{
				Cursor.Current = current;
			}
		}

		return VSConstants.S_OK;
	}

	protected virtual TabbedEditorWindowPane CreateTabbedEditorPane(IVsTextLines vsTextLines, string moniker)
	{
		return new TabbedEditorWindowPane(ServiceProvider, EditorExtensionPackage.Instance, vsTextLines, moniker);
	}

	protected virtual void EnsureAuxilliaryDocData(IVsHierarchy hierarchy, string documentMoniker, object docData)
	{
		EditorExtensionPackage.Instance.EnsureAuxilliaryDocData(hierarchy, documentMoniker, docData);
	}


}
