// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.BaseEditorFactory/SqlEditorFactory

using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.EditorExtension.Properties;
using BlackbirdSql.Shared.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.EditorExtension;


// =========================================================================================================
//
//											AbstractEditorFactory Class
//
/// <summary>
/// Abstract base code editor factory class for <see cref="EditorFactory"/> and
/// <see cref="EditorFactoryEncoded"/>.
/// </summary>
// =========================================================================================================
public abstract class AbstractEditorFactory : AbstruseEditorFactory
{

	// -------------------------------------------------------
	#region Constructors / Destructors - AbstractEditorFactory
	// -------------------------------------------------------


	public AbstractEditorFactory(bool encoded) : base(encoded)
	{

	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractEditorFactory
	// =========================================================================================================


	private string _EditorId = null;
	private static volatile int _Id;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractEditorFactory
	// =========================================================================================================


	public override Guid ClsidEditorFactory => Encoded
		? new Guid(SystemData.C_EditorEncodedFactoryGuid)
		: new Guid(SystemData.C_EditorFactoryGuid);


	protected string EditorId => _EditorId;

	private EditorExtensionPackage ExtensionInstance => EditorExtensionPackage.Instance;



	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractEditorFactory
	// =========================================================================================================


	public override int CreateEditorInstance(uint createFlags, string moniker, string physicalViewName,
		IVsHierarchy hierarchy, uint itemId, IntPtr pExistingDocData, out IntPtr pDocView,
		out IntPtr pDocData, out string caption, out Guid cmdUIGuid, out int hresult)
	{
		pDocView = IntPtr.Zero;
		pDocData = IntPtr.Zero;
		caption = "";
		cmdUIGuid = Guid.Empty;
		hresult = VSConstants.S_FALSE;

		bool autoExecute;
		bool isClone = pExistingDocData != IntPtr.Zero;

		if (ApcManager.SolutionClosing)
			return VSConstants.E_FAIL;

		_EditorId = "Editor" + _Id++;


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

				if (isClone)
				{
					if (Encoded)
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
					object objTextBuffer = ExtensionInstance.CreateInstance(ref clsid, ref iid, typeof(object));

					if (Encoded)
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

				autoExecute = ExtensionInstance.EnsureAuxilliaryDocData(hierarchy, moniker, vsTextLines2, isClone, out _);

				TabbedEditorPane editorPane = CreateTabbedEditorPane(vsTextLines2, moniker, isClone, autoExecute);


				pDocView = Marshal.GetIUnknownForObject(editorPane);
				pDocData = Marshal.GetIUnknownForObject(vsTextLines2);
				caption = string.Empty;

				cmdUIGuid = VSConstants.GUID_TextEditorFactory;

				Guid clsidLangService = MandatedSqlLanguageServiceClsid;

				vsTextLines2.SetLanguageServiceID(ref clsidLangService);
				IVsUserData vsUserData2 = (IVsUserData)vsTextLines2;
				Guid clsidDetectLang = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
				___(vsUserData2.SetData(ref clsidDetectLang, false));

				if (isClone)
					Task.Run(editorPane.CloseCloneAsync).Forget();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				if (ex is NullReferenceException || ex is ApplicationException || ex is ArgumentException || ex is InvalidOperationException)
				{
					MessageCtl.ShowEx(Resources.ExFailedToCreateEditor, ex);
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



	private TabbedEditorPane CreateTabbedEditorPane(IVsTextLines vsTextLines, string moniker,
		bool clone, bool autoExecute)
	{
		return new TabbedEditorPane(ServiceProvider, ExtensionInstance, vsTextLines, moniker,
			clone, autoExecute);
	}



	#endregion Methods

}
