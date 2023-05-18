// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Shell.Interop;

namespace BlackbirdSql.Common.Providers;


[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]

internal class Hostess : IDisposable
{

	private IVsDataHostService _HostService;

	private string _ApplicationDataDirectory = null;

	protected IVsDataHostService HostService => _HostService;

	private delegate object CreateLocalInstanceDelegate(Guid classId);
	private delegate void ShowMessageDelegate(string message);
	private delegate DialogResult ShowQuestionDelegate(string question, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, string helpId);


	public string ApplicationDataDirectory
	{
		get
		{
			if (_ApplicationDataDirectory == null)
			{
				IVsShell service = _HostService.GetService<IVsShell>();
				NativeMethods.WrapComCall(service.GetProperty(-9021, out object pvar));
				_ApplicationDataDirectory = pvar as string;
			}

			return _ApplicationDataDirectory;
		}
	}

	public Hostess(IServiceProvider serviceProvider)
	{
		// Diag.Trace();
		_HostService = serviceProvider.GetService(typeof(IVsDataHostService)) as IVsDataHostService;

		try { Assumes.Present(_HostService); }
		catch (Exception ex) { Diag.Dug(ex); throw; }
	}

	~Hostess()
	{
		Dispose(disposing: false);
	}


	public object CreateLocalInstance(Guid classId)
	{
		if (Thread.CurrentThread == _HostService.UIThread)
			return CreateLocalInstanceImpl(classId);

		return _HostService.InvokeOnUIThread(new CreateLocalInstanceDelegate(CreateLocalInstanceImpl), classId);
	}



	private object CreateLocalInstanceImpl(Guid classId)
	{
		ILocalRegistry2 service = _HostService.GetService<SLocalRegistry, ILocalRegistry2>();

		NativeMethods.WrapComCall(service.CreateInstance(classId, null, ref NativeMethods.IID_IUnknown, 1u, out IntPtr ppvObj));
		object result = null;
		if (ppvObj != IntPtr.Zero)
		{
			result = Marshal.GetObjectForIUnknown(ppvObj);
			Marshal.Release(ppvObj);
		}

		return result;
	}

	public bool IsDocumentInAProject(string documentMoniker)
	{
		IVsUIShellOpenDocument service = _HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		_ = NativeMethods.WrapComCall(service.IsDocumentInAProject(documentMoniker, out _, out _, out _, out int pDocInProj));
		return pDocInProj != 0;
	}


	public IVsWindowFrame OpenDocumentViaProject(string documentMoniker, Guid rguidLogicalView = default)
	{
		IVsUIShellOpenDocument service = _HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		_ = NativeMethods.WrapComCall(service.OpenDocumentViaProject(documentMoniker, ref rguidLogicalView, out _, out _, out _, out IVsWindowFrame ppWindowFrame));
		return ppWindowFrame;
	}



	public void RenameDocument(string oldDocumentMoniker, string newDocumentMoniker)
	{
		RenameDocument(oldDocumentMoniker, -1, newDocumentMoniker);
	}

	public void RenameDocument(string oldDocumentMoniker, int newItemId, string newDocumentMoniker)
	{
		IVsRunningDocumentTable service = _HostService.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
		NativeMethods.WrapComCall(service.RenameDocument(oldDocumentMoniker, newDocumentMoniker, NativeMethods.HIERARCHY_DONTCHANGE, (uint)newItemId));
	}

	public void ShowMessage(string message)
	{
		if (Thread.CurrentThread == HostService.UIThread)
		{
			ShowMessageImpl(message);
			return;
		}

		_HostService.InvokeOnUIThread(new ShowMessageDelegate(ShowMessageImpl), message);
	}

	private void ShowMessageImpl(string message)
	{
		IUIService service = _HostService.GetService<IUIService>();
		service.ShowMessage(message);
	}


	public DialogResult ShowQuestion(string question, MessageBoxDefaultButton defaultButton)
	{
		return ShowQuestion(question, defaultButton, null);
	}

	public DialogResult ShowQuestion(string question, string helpId)
	{
		return ShowQuestion(question, MessageBoxDefaultButton.Button2, helpId);
	}

	public DialogResult ShowQuestion(string question, MessageBoxDefaultButton defaultButton, string helpId)
	{
		return ShowQuestion(question, MessageBoxButtons.YesNo, defaultButton, helpId);
	}

	public DialogResult ShowQuestion(string question, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, string helpId)
	{
		if (Thread.CurrentThread == _HostService.UIThread)
		{
			return ShowQuestionImpl(question, buttons, defaultButton, helpId);
		}

		return (DialogResult)_HostService.InvokeOnUIThread(new ShowQuestionDelegate(ShowQuestionImpl), question, buttons, defaultButton, helpId);
	}
	private DialogResult ShowQuestionImpl(string question, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, string helpId)
	{
		IVsUIShell service = GetService<SVsUIShell, IVsUIShell>();
		Guid rclsidComp = Guid.Empty;
		OLEMSGDEFBUTTON msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
		if (defaultButton == MessageBoxDefaultButton.Button2)
		{
			msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND;
		}

		NativeMethods.WrapComCall(service.ShowMessageBox(0u, ref rclsidComp, null, question, helpId, 0u, (OLEMSGBUTTON)buttons, msgdefbtn, OLEMSGICON.OLEMSGICON_QUERY, 0, out int pnResult));
		return (DialogResult)pnResult;
	}

	public T TryGetService<T>()
	{
		return _HostService.TryGetService<T>();
	}

	public TInterface TryGetService<TService, TInterface>()
	{
		return _HostService.TryGetService<TService, TInterface>();
	}

	public T TryGetService<T>(Guid serviceGuid)
	{
		return _HostService.TryGetService<T>(serviceGuid);
	}

	public T GetService<T>()
	{
		return _HostService.GetService<T>();
	}

	public TInterface GetService<TService, TInterface>()
	{
		return _HostService.GetService<TService, TInterface>();
	}

	public T GetService<T>(Guid serviceGuid)
	{
		return _HostService.GetService<T>(serviceGuid);
	}

	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && _HostService != null)
		{
			/*
			if (_environment != null)
			{
				((IDisposable)_environment).Dispose();
				_environment = null;
			}
			*/

			_HostService = null;
			// _serviceProvider = null;
		}
	}


	public bool ActivateDocumentIfOpen(string documentMoniker)
	{
		return ActivateDocumentIfOpen(documentMoniker, doNotShowWindowFrame: false) != null;
	}

	public IVsWindowFrame ActivateDocumentIfOpen(string documentMoniker, bool doNotShowWindowFrame)
	{
		IVsUIShellOpenDocument service = _HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		Guid rguidLogicalView = Guid.Empty;
		uint grfIDO = 1u;
		if (doNotShowWindowFrame)
		{
			grfIDO = 0u;
		}

		_ = NativeMethods.WrapComCall(service.IsDocumentOpen(null, uint.MaxValue, documentMoniker, ref rguidLogicalView, grfIDO, out _, null, out IVsWindowFrame ppWindowFrame, out _));
		return ppWindowFrame;
	}

	public IVsWindowFrame CreateDocumentWindow(int attributes, string documentMoniker, string ownerCaption, string editorCaption, Guid editorType, string physicalView, Guid commandUIGuid, object documentView, object documentData, int owningItemId, IVsUIHierarchy owningHierarchy, Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
	{
		IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(documentView);
		IntPtr iUnknownForObject2 = Marshal.GetIUnknownForObject(documentData);
		try
		{
			IVsUIShell service = _HostService.GetService<SVsUIShell, IVsUIShell>();
			NativeMethods.WrapComCall(service.CreateDocumentWindow((uint)attributes, documentMoniker, owningHierarchy, (uint)owningItemId, iUnknownForObject, iUnknownForObject2, ref editorType, physicalView, ref commandUIGuid, serviceProvider, ownerCaption, editorCaption, null, out IVsWindowFrame ppWindowFrame));
			return ppWindowFrame;
		}
		finally
		{
			Marshal.Release(iUnknownForObject2);
			Marshal.Release(iUnknownForObject);
		}
	}

	public bool IsCommandUIContextActive(Guid commandUIGuid)
	{
		IVsMonitorSelection service = _HostService.GetService<IVsMonitorSelection>();
		NativeMethods.WrapComCall(service.GetCmdUIContextCookie(ref commandUIGuid, out uint pdwCmdUICookie));
		NativeMethods.WrapComCall(service.IsCmdUIContextActive(pdwCmdUICookie, out int pfActive));
		return pfActive != 0;
	}

}
