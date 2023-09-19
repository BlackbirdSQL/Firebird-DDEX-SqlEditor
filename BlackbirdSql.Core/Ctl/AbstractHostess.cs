// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.Host

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IServiceProvider = System.IServiceProvider;



namespace BlackbirdSql.Core.Ctl;


/// <summary>
/// Core Host services are defined in this class,
/// </summary>
public abstract class AbstractHostess : IDisposable
{
	private Guid _MandatedSqlEditorFactoryClsid = Guid.Empty;
	private Guid _MandatedSqlLanguageServiceClsid = Guid.Empty;

	private readonly IServiceProvider _ServiceProvider;
	private IVsDataHostService _HostService;
	private IVsUIShell _ShellService;

	private delegate void PostExecuteCommandDelegate(CommandID command);


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

	public IVsUIShell ShellService
	{
		get
		{
			if (_ShellService != null)
				return _ShellService;

			_HostService ??= _ServiceProvider.GetService(typeof(IVsDataHostService)) as IVsDataHostService;

			if (_HostService != null)
				_ShellService = _HostService.GetService<SVsUIShell, IVsUIShell>();

			_ShellService ??= Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;

			try { Assumes.Present(_ShellService); }
			catch (Exception ex) { Diag.Dug(ex); throw; }

			return _ShellService;
		}
	}


	public string UserDataDirectory => Controller.Instance.UserDataDirectory;


	protected virtual Guid MandatedSqlEditorFactoryClsid
	{
		get
		{
			if (_MandatedSqlEditorFactoryClsid == Guid.Empty)
				_MandatedSqlEditorFactoryClsid = new(SystemData.MandatedSqlEditorFactoryGuid);

			return _MandatedSqlEditorFactoryClsid;
		}
	}

	protected virtual Guid MandatedSqlLanguageServiceClsid
	{
		get
		{
			if (_MandatedSqlLanguageServiceClsid == Guid.Empty)
				_MandatedSqlLanguageServiceClsid = new(SystemData.MandatedSqlLanguageServiceGuid);

			return _MandatedSqlLanguageServiceClsid;
		}
	}


	public AbstractHostess(IServiceProvider serviceProvider)
	{
		Tracer.Trace(GetType(), "AbstractHostess.AbstractHostess(IServiceProvider)");

		_ServiceProvider = serviceProvider;
	}

	~AbstractHostess()
	{
		Dispose(disposing: false);
	}





	public bool ActivateDocumentIfOpen(string documentMoniker)
	{
		Tracer.Trace(GetType(), "AbstractHostess.ActivateDocumentIfOpen", "documentMoniker: {0}", documentMoniker);

		ThreadHelper.ThrowIfNotOnUIThread();
		return ActivateDocumentIfOpen(documentMoniker, doNotShowWindowFrame: false) != null;
	}



	public IVsWindowFrame ActivateDocumentIfOpen(string mkDocument, bool doNotShowWindowFrame)
	{
		Tracer.Trace(GetType(), "AbstractHostess.ActivateDocumentIfOpen", "mkDocument: {0}, doNotShowWindowFrame: {1}", mkDocument, doNotShowWindowFrame);

		ThreadHelper.ThrowIfNotOnUIThread();

		IVsUIShellOpenDocument service = HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		if (service == null)
		{
			ServiceUnavailableException ex = new(typeof(IVsUIShellOpenDocument));
			Diag.Dug(ex);
			throw ex;
		}

		Guid rguidLogicalView = VSConstants.LOGVIEWID_TextView;

		uint grfIDO = (uint)__VSIDOFLAGS.IDO_ActivateIfOpen;
		if (doNotShowWindowFrame)
		{
			grfIDO = 0u;
		}

		IVsWindowFrame ppWindowFrame;

		try
		{
			_ = Native.WrapComCall(service.IsDocumentOpen(null, uint.MaxValue, mkDocument, ref rguidLogicalView, grfIDO, out _, null, out ppWindowFrame, out _));
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

		if (service == null)
		{
			ServiceUnavailableException ex = new(typeof(ILocalRegistry2));
			Diag.Dug(ex);
			throw ex;
		}

		object result = null;
		try
		{
			Guid riid = VSConstants.IID_IUnknown;

			Native.WrapComCall(service.CreateInstance(classId, null, ref riid, (uint)CLSCTX.CLSCTX_INPROC_SERVER, out IntPtr ppvObj));
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




	public bool IsDocumentInAProject(string documentMoniker)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int pDocInProj;

		IVsUIShellOpenDocument service = HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		if (service == null)
		{
			ServiceUnavailableException ex = new(typeof(IVsUIShellOpenDocument));
			Diag.Dug(ex);
			throw ex;
		}

		try
		{
			_ = Native.WrapComCall(service.IsDocumentInAProject(documentMoniker, out _, out _, out _, out pDocInProj));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return pDocInProj != 0;
	}



	public void PostExecuteCommand(CommandID command, int delay = 0)
	{
		Tracer.Trace(GetType(), "AbstractHostess.PostExecuteCommand", "command: {0}", command);

		_ = ShellService;

		if (delay == 0 && _HostService != null && Thread.CurrentThread == HostService.UIThread)
		{
			PostExecuteCommandImpl(command);
			return;
		}

		// Fallback if Site does not have an IVsDataHostService service
		if (_HostService == null || delay > 0)
		{
			if (delay == 0 && ThreadHelper.CheckAccess())
				PostExecuteCommandImpl(command);
			else
				_ = Task.Run(() => PostExecuteCommandAsync(command, delay));
			return;
		}

		HostService.BeginInvokeOnUIThread(new PostExecuteCommandDelegate(PostExecuteCommandImpl), command);
	}

	private async Task<bool> PostExecuteCommandAsync(CommandID command, int delay)
	{
		if (delay > 0)
			Thread.Sleep(delay);

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
		PostExecuteCommandImpl(command);
		return true;
	}


	private void PostExecuteCommandImpl(CommandID command)
	{
		Guid pguidCmdGroup = command.Guid;
		uint iD = (uint)command.ID;
		object pvaIn = null;
		Native.WrapComCall(ShellService.PostExecCommand(ref pguidCmdGroup, iD, 0u, ref pvaIn));
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

		if (service == null)
		{
			ServiceUnavailableException ex = new(typeof(IVsRunningDocumentTable));
			Diag.Dug(ex);
			throw ex;
		}

		try
		{
			Native.WrapComCall(service.RenameDocument(oldDocumentMoniker, newDocumentMoniker,
				VSConstants.HIERARCHY_DONTCHANGE, (uint)newItemId));
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
		if (service == null)
		{
			ServiceUnavailableException ex = new(typeof(IUIService));
			Diag.Dug(ex);
			throw ex;
		}
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

		Guid rclsidComp = Guid.Empty;
		OLEMSGDEFBUTTON msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
		if (defaultButton == MessageBoxDefaultButton.Button2)
		{
			msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND;
		}

		int pnResult;

		try
		{
			Native.WrapComCall(ShellService.ShowMessageBox(0u, ref rclsidComp, null, question, helpId, 0u, (OLEMSGBUTTON)buttons, msgdefbtn, OLEMSGICON.OLEMSGICON_QUERY, 0, out pnResult));
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
		T service = HostService.TryGetService<T>();
		if (service == null)
		{
			ServiceUnavailableException ex = new(typeof(T));
			Diag.Dug(ex);
			throw ex;
		}
		return service;

	}

	public TInterface TryGetService<TService, TInterface>()
	{
		TInterface service = HostService.TryGetService<TService, TInterface>();
		if (service == null)
		{
			InvalidOperationException ex = new($"{typeof(TService).Name} : {typeof(TInterface).Name} service not found");
			Diag.Dug(ex);
			throw ex;
		}
		return service;
	}

	public T TryGetService<T>(Guid serviceGuid)
	{
		T service = HostService.TryGetService<T>(serviceGuid);
		if (service == null)
		{
			InvalidOperationException ex = new($"{typeof(T).Name} service (Guid: {serviceGuid}) not found");
			Diag.Dug(ex);
			throw ex;
		}
		return service;
	}

	public T GetService<T>()
	{
		Tracer.Trace(GetType(), "AbstractHostess.GetService<T>", "T: {0}", typeof(T).FullName);

		T service = HostService.GetService<T>();
		if (service == null)
		{
			ServiceUnavailableException ex = new(typeof(T));
			Diag.Dug(ex);
			throw ex;
		}
		return service;
	}

	public TInterface GetService<TService, TInterface>()
	{
		TInterface service = HostService.GetService<TService, TInterface>();
		if (service == null)
		{
			InvalidOperationException ex = new($"{typeof(TService).Name} : {typeof(TInterface).Name} service not found");
			Diag.Dug(ex);
			throw ex;
		}
		return service;
	}

	public T GetService<T>(Guid serviceGuid)
	{
		T service = HostService.GetService<T>(serviceGuid);
		if (service == null)
		{
			InvalidOperationException ex = new($"{typeof(T).Name} service (Guid: {serviceGuid}) not found");
			Diag.Dug(ex);
			throw ex;
		}
		return service;
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
