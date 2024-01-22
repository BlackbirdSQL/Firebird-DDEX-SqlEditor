// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.Host

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using IObjectWithSite = Microsoft.VisualStudio.OLE.Interop.IObjectWithSite;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;


namespace BlackbirdSql.Core.Ctl;

/// <summary>
/// Editor related Host services.
/// </summary>
[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread")]
public class Hostess : AbstractHostess
{
	public Hostess() : base()
	{
		// Tracer.Trace(GetType(), "Hostess.Hostess()");
	}

	public Hostess(IServiceProvider dataViewHierarchyServiceProvider) : base(dataViewHierarchyServiceProvider)
	{
		// Tracer.Trace(GetType(), "Hostess.Hostess(IServiceProvider dataViewHierarchyServiceProvider)");
	}


	/// <summary>
	/// Activates or opens a virtual file for editing
	/// </summary>
	/// <param name="fileIdentifier"></param>
	/// <param name="doNotShowWindowFrame"></param>
	/// <returns></returns>
	internal IVsWindowFrame ActivateOrOpenVirtualDocument(IVsDataExplorerNode node, bool doNotShowWindowFrame)
	{
		// Tracer.Trace(GetType(), "Hostess.ActivateOrOpenVirtualDocument");

		int result;

		MonikerAgent moniker = new(node, EnModelTargetType.QueryScript);
		string source = MonikerAgent.GetDecoratedDdlSource(node, EnModelTargetType.QueryScript);
		string mkDocument = moniker.MiscDocumentMonikerPath;
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

		IVsUIShellOpenDocument service = HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>()
			?? throw Diag.ExceptionService(typeof(IVsUIShellOpenDocument));

		Diag.ThrowIfNotOnUIThread();

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
			byte[] bytes = Encoding.ASCII.GetBytes(source);
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


	public static IVsWindowFrame FindToolWindow(Guid toolWindowGuid)
	{
		Diag.ThrowIfNotOnUIThread();

		IVsUIShell uiShell = Package.GetGlobalService(typeof(IVsUIShell)) as IVsUIShell
			?? throw Diag.ExceptionService(typeof(IVsUIShell));

		int hr = uiShell.FindToolWindow(0u, ref toolWindowGuid, out IVsWindowFrame ppWindowFrame);

		if (Native.Failed(hr, -2147467259))
			return null;

		return ppWindowFrame;
	}

	public static Type GetManagedTypeFromCLSID(Guid classId)
	{
		Type type = Type.GetTypeFromCLSID(classId);

		if (type != null && type.IsCOMObject)
			type = null;

		return type;
	}


	public static Type GetTypeFromAssembly(Assembly assembly, string typeName, bool throwOnError = false)
	{
		return assembly.GetType(typeName, throwOnError);
	}


	public static Assembly LoadAssemblyFrom(string fileName)
		=> Assembly.LoadFrom(fileName);


	internal void RegisterHierarchy(IVsUIHierarchy hierarchy)
	{
		IBPackageController controller = GetService<IBPackageController>()
			?? throw Diag.ExceptionService(typeof(IBPackageController));

		controller.RegisterMiscHierarchy(hierarchy);
	}




	/// <summary>
	/// Loads a string into a docData text buffer.
	/// </summary>
	protected int CreateDocDataFromText(Package package, Guid langGuid, string text, out IntPtr docData)
	{
		Diag.ThrowIfNotOnUIThread();

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
		IVsUIShell service = HostService.GetService<SVsUIShell, IVsUIShell>()
			?? throw Diag.ExceptionService(typeof(IVsUIShell));

		Diag.ThrowIfNotOnUIThread();

		IntPtr intPtrUnknown = IntPtr.Zero;
		IntPtr intPtrUnknown2 = IntPtr.Zero;

		try
		{
			intPtrUnknown = Marshal.GetIUnknownForObject(documentView);
			intPtrUnknown2 = Marshal.GetIUnknownForObject(documentData);

			Native.WrapComCall(service.CreateDocumentWindow((uint)attributes, documentMoniker,
				owningHierarchy, (uint)owningItemId, intPtrUnknown, intPtrUnknown2, ref editorType,
				physicalView, ref commandUIGuid, serviceProvider, ownerCaption, editorCaption,
				null, out IVsWindowFrame ppWindowFrame));
			return ppWindowFrame;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			Marshal.Release(intPtrUnknown2);
			Marshal.Release(intPtrUnknown);
		}
	}


	public static object CreateManagedInstance(Guid classId)
	{
		Type managedTypeFromCLSID = GetManagedTypeFromCLSID(classId)
			?? throw new TypeLoadException(classId.ToString("B"));
		return Activator.CreateInstance(managedTypeFromCLSID);
	}



	public static object CreateInstance(Type type, params object[] args)
	{
		try
		{
			if (type.IsInterface)
				throw new TypeAccessException($"Cannot create interface instance: {type.Name}.");

			return Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
		}
		catch (TargetInvocationException ex)
		{
			Diag.Dug(ex);
			throw ex.InnerException;
		}
	}
}
