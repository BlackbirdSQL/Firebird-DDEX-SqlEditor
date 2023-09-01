// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using BlackbirdSql.Core.Interfaces;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;


using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IObjectWithSite = Microsoft.VisualStudio.OLE.Interop.IObjectWithSite;
using BlackbirdSql.Core.Model;

namespace BlackbirdSql.Core.Providers;


/// <summary>
/// Editor related Host services.
/// </summary>
public class Hostess : AbstractHostess
{


	public Hostess(IServiceProvider serviceProvider) : base(serviceProvider)
	{
	}


	/// <summary>
	/// Activates or opens a virtual file for editing
	/// </summary>
	/// <param name="fileIdentifier"></param>
	/// <param name="doNotShowWindowFrame"></param>
	/// <returns></returns>
	internal IVsWindowFrame ActivateOrOpenVirtualDocument(IVsDataExplorerNode node, bool doNotShowWindowFrame)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int result;

		MonikerAgent moniker = new(node);
		string source = MonikerAgent.GetDecoratedDdlSource(node, false);
		string mkDocument = moniker.ToString();
		string path = moniker.ToPath(UserDataDirectory);
		uint grfIDO = (uint)__VSIDOFLAGS.IDO_ActivateIfOpen;


		IVsWindowFrame vsWindowFrame;

		Guid logicalViewGuid = VSConstants.LOGVIEWID_TextView;

		string physicalView = null;

		IVsUIHierarchy hierarchy;

		/*
		if (path.Length > 260)
		{
			NotSupportedException ex = new(Resources.SqlViewTextObjectCommandProvider_PathTooLong);
			Diag.Dug(ex);
			throw ex;
		}
		*/
		if (doNotShowWindowFrame)
			grfIDO = 0u;

		IVsUIShellOpenDocument service;
		try
		{ 
			service = HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (service == null)
		{
			NotSupportedException ex = new("IVsUIShellOpenDocument");
			Diag.Dug(ex);
			throw ex;
		}


		// Check if the document is already open.
		try
		{
			_ = Native.WrapComCall(service.IsDocumentOpen(null, uint.MaxValue, mkDocument, ref logicalViewGuid,
				grfIDO, out hierarchy, null, out vsWindowFrame, out _));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (vsWindowFrame != null)
		{
			RegisterHierarchy(hierarchy);
			return vsWindowFrame;
		}

		if (!Directory.Exists(Path.GetDirectoryName(path)))
			Directory.CreateDirectory(Path.GetDirectoryName(path));


		if (File.Exists(path))
			File.SetAttributes(path, FileAttributes.Normal);


		FileStream fileStream = File.Open(path, FileMode.Create);
		using (fileStream)
		{
			byte[] bytes = Encoding.ASCII.GetBytes((string)source);
			fileStream.Write(bytes, 0, bytes.Length);
		}

		File.SetAttributes(path, FileAttributes.Normal);


		try
		{
			// This will return the IOleServiceProvider, the hierarchy, it's itemid and window frame
			result = service.OpenDocumentViaProjectWithSpecific(path,
				(uint)(__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen | __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor | __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseView),
				MandatedSqlEditorFactoryClsid, physicalView, logicalViewGuid, out IOleServiceProvider ppSP, out hierarchy, out uint itemId,
				out vsWindowFrame);

			if (result != VSConstants.S_OK)
			{
				InvalidOperationException ex = new($"OpenDocumentViaProjectWithSpecific [{result}]");
				throw ex;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			File.Delete(path);
			throw ex;
		}

		if (vsWindowFrame == null)
		{
			InvalidOperationException ex = new($"OpenDocumentViaProjectWithSpecific returned a null window frame [{result}].");
			Diag.Dug(ex);
			File.Delete(path);
			throw ex;
		}

		RegisterHierarchy(hierarchy);


		object pvar;

		try
		{
			Native.WrapComCall(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out pvar));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			File.Delete(path);
			throw ex;
		}

		IVsTextLines vsTextLines = pvar as IVsTextLines;
		if (vsTextLines == null)
		{
			(pvar as IVsTextBufferProvider)?.GetTextBuffer(out vsTextLines);
		}

		if (vsTextLines != null)
		{
			try
			{
				Native.WrapComCall(vsTextLines.GetStateFlags(out uint pdwReadOnlyFlags));
				pdwReadOnlyFlags |= (uint)BUFFERSTATEFLAGS.BSF_USER_READONLY;
				Native.WrapComCall(vsTextLines.SetStateFlags(pdwReadOnlyFlags));
				if (MandatedSqlLanguageServiceClsid != Guid.Empty)
					vsTextLines.SetLanguageServiceID(MandatedSqlLanguageServiceClsid);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				File.Delete(path);
				throw ex;
			}
		}

		if (vsWindowFrame != null && !doNotShowWindowFrame)
		{
			try
			{
				Native.WrapComCall(vsWindowFrame.Show());
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				File.Delete(path);
				throw ex;
			}
		}

		// File.Delete(path);

		return vsWindowFrame;
	}



	internal void RegisterHierarchy(IVsUIHierarchy hierarchy)
	{
		IBPackageController controller;

		try
		{
			controller = GetService<IBPackageController>();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (controller == null)
		{
			ServiceUnavailableException ex = new(typeof(IBPackageController));
			Diag.Dug(ex);
			throw ex;
		}

		controller.RegisterMiscHierarchy(hierarchy);
	}




	/// <summary>
	/// Loads a string into a docData text buffer.
	/// </summary>
	protected int CreateDocDataFromText(Package package, Guid langGuid, string text, out IntPtr docData)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		// Create a new IVsTextLines buffer.
		Type persistDocDataType = typeof(IVsPersistDocData);
		Guid riid = persistDocDataType.GUID;
		Guid clsid = typeof(VsTextBufferClass).GUID;

		// Create the text buffer
		IVsPersistDocData docDataObject = package.CreateInstance(ref clsid, ref riid, persistDocDataType) as IVsPersistDocData;

		// Site the buffer
		IObjectWithSite objectWithSite = (IObjectWithSite)docDataObject!;
		objectWithSite.SetSite(package);
		// objectWithSite.SetSite(ServiceProvider.GetService(typeof(IOleServiceProvider)));

		// Cast the buffer to a textlines buffer to load the text.
		IVsTextLines textLines = (IVsTextLines)docDataObject;
		textLines.InitializeContent(text, text.Length);

		if (langGuid != Guid.Empty)
			textLines.SetLanguageServiceID(langGuid);

		docData = Marshal.GetIUnknownForObject(docDataObject);

		return VSConstants.S_OK;

	}

	/// <summary>
	/// Not operational. Do not use
	/// </summary>
	public IVsWindowFrame CreateDocumentWindow(int attributes, string documentMoniker, string ownerCaption,
		string editorCaption, Guid editorType, string physicalView, Guid commandUIGuid, object documentView,
		object documentData, int owningItemId, IVsUIHierarchy owningHierarchy, IOleServiceProvider serviceProvider)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(documentView);
		IntPtr iUnknownForObject2 = Marshal.GetIUnknownForObject(documentData);
		try
		{
			IVsUIShell service = HostService.GetService<SVsUIShell, IVsUIShell>();
			if (service == null)
			{
				InvalidOperationException ex = new("Service <SVsUIShell, IVsUIShell> not found");
				Diag.Dug(ex);
				throw ex;
			}
			Native.WrapComCall(service.CreateDocumentWindow((uint)attributes, documentMoniker, owningHierarchy, (uint)owningItemId, iUnknownForObject, iUnknownForObject2, ref editorType, physicalView, ref commandUIGuid, serviceProvider, ownerCaption, editorCaption, null, out IVsWindowFrame ppWindowFrame));
			return ppWindowFrame;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			Marshal.Release(iUnknownForObject2);
			Marshal.Release(iUnknownForObject);
		}
	}

}
