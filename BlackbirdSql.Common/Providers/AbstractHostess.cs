// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace BlackbirdSql.Common.Providers;


/// <summary>
/// Core Host services are defined in this class,
/// </summary>
public abstract class AbstractHostess : IDisposable
{
	private readonly IServiceProvider _ServiceProvider;
	private IVsDataHostService _HostService;

	private string _ApplicationDataDirectory = null;


	private delegate object CreateLocalInstanceDelegate(Guid classId);
	private delegate void ShowMessageDelegate(string message);
	private delegate DialogResult ShowQuestionDelegate(string question, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, string helpId);


	public IVsDataHostService HostService
	{
		get
		{
			_HostService ??= _ServiceProvider.GetService(typeof(IVsDataHostService)) as IVsDataHostService;

			try { Assumes.Present(_HostService); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			return _HostService;
		}
	}


	public string ApplicationDataDirectory
	{
		get
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			if (_ApplicationDataDirectory == null)
			{
				IVsShell service = HostService.GetService<IVsShell>();
				try
				{
					NativeMethods.WrapComCall(service.GetProperty((int)__VSSPROPID.VSSPROPID_AppDataDir, out object pvar));
					_ApplicationDataDirectory = pvar as string;
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
					throw ex;
				}
			}

			return _ApplicationDataDirectory;
		}
	}

	public AbstractHostess(IServiceProvider serviceProvider)
	{
		// Diag.Trace();
		_ServiceProvider = serviceProvider;
	}

	~AbstractHostess()
	{
		Dispose(disposing: false);
	}





	public bool ActivateDocumentIfOpen(string documentMoniker)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		return ActivateDocumentIfOpen(documentMoniker, doNotShowWindowFrame: false) != null;
	}



	public IVsWindowFrame ActivateDocumentIfOpen(string mkDocument, bool doNotShowWindowFrame)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		IVsUIShellOpenDocument service = HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		Guid rguidLogicalView = VSConstants.LOGVIEWID_TextView;

		uint grfIDO = 1u;
		if (doNotShowWindowFrame)
		{
			grfIDO = 0u;
		}

		IVsWindowFrame ppWindowFrame;

		try
		{
			_ = NativeMethods.WrapComCall(service.IsDocumentOpen(null, uint.MaxValue, mkDocument, ref rguidLogicalView, grfIDO, out _, null, out ppWindowFrame, out _));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return ppWindowFrame;
	}

	public object CreateLocalInstance(Guid classId)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (Thread.CurrentThread == HostService.UIThread)
			return CreateLocalInstanceImpl(classId);

		return HostService.InvokeOnUIThread(new CreateLocalInstanceDelegate(CreateLocalInstanceImpl), classId);
	}



	private object CreateLocalInstanceImpl(Guid classId)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		ILocalRegistry2 service = HostService.GetService<SLocalRegistry, ILocalRegistry2>();

		object result = null;
		try
		{
			NativeMethods.WrapComCall(service.CreateInstance(classId, null, ref NativeMethods.IID_IUnknown, 1u, out IntPtr ppvObj));
			if (ppvObj != IntPtr.Zero)
			{
				result = Marshal.GetObjectForIUnknown(ppvObj);
				Marshal.Release(ppvObj);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return result;
	}


	public bool IsCommandUIContextActive(Guid commandUIGuid)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		IVsMonitorSelection service = HostService.GetService<IVsMonitorSelection>();
		int pfActive;

		try
		{
			NativeMethods.WrapComCall(service.GetCmdUIContextCookie(ref commandUIGuid, out uint pdwCmdUICookie));
			NativeMethods.WrapComCall(service.IsCmdUIContextActive(pdwCmdUICookie, out pfActive));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		return pfActive != 0;
	}



	public bool IsDocumentInAProject(string documentMoniker)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int pDocInProj;

		IVsUIShellOpenDocument service = HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		try
		{
			_ = NativeMethods.WrapComCall(service.IsDocumentInAProject(documentMoniker, out _, out _, out _, out pDocInProj));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return pDocInProj != 0;
	}




	public void RenameDocument(string oldDocumentMoniker, string newDocumentMoniker)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		RenameDocument(oldDocumentMoniker, -1, newDocumentMoniker);
	}

	public void RenameDocument(string oldDocumentMoniker, int newItemId, string newDocumentMoniker)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		IVsRunningDocumentTable service = HostService.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
		try
		{
			NativeMethods.WrapComCall(service.RenameDocument(oldDocumentMoniker, newDocumentMoniker, NativeMethods.HIERARCHY_DONTCHANGE, (uint)newItemId));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}

	public void ShowMessage(string message)
	{
		if (Thread.CurrentThread == HostService.UIThread)
		{
			ShowMessageImpl(message);
			return;
		}


		HostService.InvokeOnUIThread(new ShowMessageDelegate(ShowMessageImpl), message);
	}

	private void ShowMessageImpl(string message)
	{
		IUIService service = HostService.GetService<IUIService>();
		service.ShowMessage(message);
	}


	public DialogResult ShowQuestion(string question, MessageBoxDefaultButton defaultButton)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		return ShowQuestion(question, defaultButton, null);
	}

	public DialogResult ShowQuestion(string question, string helpId)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		return ShowQuestion(question, MessageBoxDefaultButton.Button2, helpId);
	}

	public DialogResult ShowQuestion(string question, MessageBoxDefaultButton defaultButton, string helpId)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		return ShowQuestion(question, MessageBoxButtons.YesNo, defaultButton, helpId);
	}

	public DialogResult ShowQuestion(string question, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, string helpId)
	{
		if (Thread.CurrentThread == HostService.UIThread)
		{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
			return ShowQuestionImpl(question, buttons, defaultButton, helpId);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
		}

		return (DialogResult)HostService.InvokeOnUIThread(new ShowQuestionDelegate(ShowQuestionImpl), question, buttons, defaultButton, helpId);
	}
	private DialogResult ShowQuestionImpl(string question, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, string helpId)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		IVsUIShell service = GetService<SVsUIShell, IVsUIShell>();
		Guid rclsidComp = Guid.Empty;
		OLEMSGDEFBUTTON msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
		if (defaultButton == MessageBoxDefaultButton.Button2)
		{
			msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND;
		}

		int pnResult;

		try
		{
			NativeMethods.WrapComCall(service.ShowMessageBox(0u, ref rclsidComp, null, question, helpId, 0u, (OLEMSGBUTTON)buttons, msgdefbtn, OLEMSGICON.OLEMSGICON_QUERY, 0, out pnResult));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		return (DialogResult)pnResult;
	}

	public T TryGetService<T>()
	{
		return HostService.TryGetService<T>();
	}

	public TInterface TryGetService<TService, TInterface>()
	{
		return HostService.TryGetService<TService, TInterface>();
	}

	public T TryGetService<T>(Guid serviceGuid)
	{
		return HostService.TryGetService<T>(serviceGuid);
	}

	public T GetService<T>()
	{
		return HostService.GetService<T>();
	}

	public TInterface GetService<TService, TInterface>()
	{
		return HostService.GetService<TService, TInterface>();
	}

	public T GetService<T>(Guid serviceGuid)
	{
		return HostService.GetService<T>(serviceGuid);
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

}
