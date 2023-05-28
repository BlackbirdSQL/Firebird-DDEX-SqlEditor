// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using BlackbirdSql.Common.Commands;

using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IObjectWithSite = Microsoft.VisualStudio.OLE.Interop.IObjectWithSite;

namespace BlackbirdSql.Common.Providers;



/// <summary>
/// Editor related Host services.
/// </summary>
public class Hostess : AbstractHostess
{

	public Hostess(IServiceProvider serviceProvider) : base(serviceProvider)
	{
	}


	/// <summary>
	/// Activates or opens a physical file for editing
	/// </summary>
	/// <param name="fileIdentifier"></param>
	/// <param name="doNotShowWindowFrame"></param>
	/// <returns></returns>
	internal IVsWindowFrame ActivateOrOpenDocument(IVsDataExplorerNode node, bool doNotShowWindowFrame)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int result;

		SqlMonikerHelper moniker = new(node);
		string source = moniker.GetDecoratedDdlSource(node.Object);
		string mkDocument = moniker.ToString();
		string path = moniker.ToPath(ApplicationDataDirectory);
		uint grfIDO = 0; // 1u;

		IVsWindowFrame vsWindowFrame;

		Guid langGuid = new(CommandProperties.LangUSql);
		Guid editorGuid = new(CommandProperties.SqlEditorGuid);
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
			ArgumentNullException ex = new("Service for <SVsUIShellOpenDocument, IVsUIShellOpenDocument> returned null");
			Diag.Dug(ex);
			throw ex;
		}

		// Check if the document is already open.
		try
		{
			_ = NativeMethods.WrapComCall(service.IsDocumentOpen(null, uint.MaxValue, mkDocument, ref logicalViewGuid,
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
				(uint)(__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen | __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor),
				editorGuid, physicalView, logicalViewGuid, out IOleServiceProvider ppSP, out hierarchy, out uint itemId,
				out vsWindowFrame);

			if (result != VSConstants.S_OK)
				throw new InvalidOperationException($"OpenDocumentViaProjectWithSpecific [{result}]");

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
			NativeMethods.WrapComCall(vsWindowFrame.GetProperty(-4004, out pvar));
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
				NativeMethods.WrapComCall(vsTextLines.GetStateFlags(out uint pdwReadOnlyFlags));
				// pdwReadOnlyFlags |= 1u;
				NativeMethods.WrapComCall(vsTextLines.SetStateFlags(pdwReadOnlyFlags));
				if (langGuid != Guid.Empty)
					vsTextLines.SetLanguageServiceID(langGuid);

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
				NativeMethods.WrapComCall(vsWindowFrame.Show());
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				File.Delete(path);
				throw ex;
			}
		}

		File.Delete(path);

		return vsWindowFrame;
	}



	internal void RegisterHierarchy(IVsUIHierarchy hierarchy)
	{
		IPackageController controller;

		try
		{
			controller = GetService<IPackageController>();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (controller == null)
		{
			ServiceUnavailableException ex = new(typeof(IPackageController));
			Diag.Dug(ex);
			throw ex;
		}

		controller.RegisterMiscHierarchy(hierarchy);
	}


	/// <summary>
	/// Activates or opens a database expression for editing. Not operational. Do not use
	/// </summary>
	/// <param name="fileIdentifier"></param>
	/// <param name="doNotShowWindowFrame"></param>
	/// <returns></returns>
	internal IVsWindowFrame ActivateOrOpenVirtualDocument(Package package, IVsDataExplorerNode node, bool doNotShowWindowFrame)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int result;

		SqlMonikerHelper moniker = new(node);
		string prop = SqlMonikerHelper.GetNodeScriptProperty(node.Object).ToUpper();
		string source = (string)node.Object.Properties[prop];
		string mkDocument = moniker.ToString();

		// Diag.Trace($"Moniker: {node.DocumentMoniker}");

		// Check if the document is already open.
		IVsWindowFrame vsWindowFrame = ActivateDocumentIfOpen(mkDocument, doNotShowWindowFrame);

		if (vsWindowFrame != null)
			return vsWindowFrame;



		IVsUIShellOpenDocument service = HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		if (service == null)
		{
			ArgumentNullException ex = new("Service for <SVsUIShellOpenDocument, IVsUIShellOpenDocument> returned null");
			Diag.Dug(ex);
			throw ex;
		}

		Guid langGuid = new(CommandProperties.LangUSql);
		Guid editorGuid = new(CommandProperties.SqlEditorGuid);
		Guid logicalViewGuid = VSConstants.LOGVIEWID_TextView;
		string physicalView = null;
		IOleServiceProvider ppSP;
		uint itemId;
		IVsUIHierarchy hierarchy;


		try
		{
			// This will return the IOleServiceProvider, the hierarchy, it's itemid and window frame
			result = service.OpenDocumentViaProjectWithSpecific(mkDocument, 0, editorGuid, physicalView, logicalViewGuid,
				out ppSP, out hierarchy, out itemId, out vsWindowFrame);

			if (result != VSConstants.S_OK)
				throw new InvalidOperationException($"OpenDocumentViaProjectWithSpecific [{result}]");

			Diag.Trace("Hierarchy is " + (hierarchy == null ? "null" : hierarchy.ToString()) + " itemid: " + itemId + "  and windowFrame is " + (vsWindowFrame == null ? "NULL" : "NOT NULL") + "  and ppSP is " + (ppSP == null ? "NULL" : "NOT NULL"));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		ppSP = package;

		string editorCaption;
		IntPtr docData;

		try
		{
			result = CreateDocDataFromText(package, langGuid, source, out docData);

			if (docData == null)
			{
				throw new ArgumentNullException("CreateDocDataFromText returned null");
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		try
		{
			editorCaption = "My Editor Caption";
			result = service.OpenSpecificEditor(0, mkDocument, editorGuid, null, logicalViewGuid, editorCaption,
				hierarchy, itemId, docData, ppSP, out vsWindowFrame);
			// service.OpenSpecificEditor(0, mkDocument, ref editorGuid, null, VSConstants.LOGVIEWID.TextView_guid,
			//	editorCaption, hierarchy, itemId, docData, ppSP, out vsWindowFrame);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (vsWindowFrame == null)
		{
			InvalidOperationException ex = new($"OpenSpecificEditor returned a null window frame [{result}].");
			Diag.Dug(ex);
			throw ex;
		}

		if (!doNotShowWindowFrame)
		{
			try
			{
				NativeMethods.WrapComCall(vsWindowFrame.Show());
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
		}

		return vsWindowFrame;
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
			NativeMethods.WrapComCall(service.CreateDocumentWindow((uint)attributes, documentMoniker, owningHierarchy, (uint)owningItemId, iUnknownForObject, iUnknownForObject2, ref editorType, physicalView, ref commandUIGuid, serviceProvider, ownerCaption, editorCaption, null, out IVsWindowFrame ppWindowFrame));
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
