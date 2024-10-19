// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.Host

using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Sys;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IServiceProvider = System.IServiceProvider;



namespace BlackbirdSql.Core.Ctl;



/// <summary>
/// Core Host services are defined in this class,
/// </summary>
public abstract class AbstractHostess(IServiceProvider dataViewHierarchyServiceProvider) : IDisposable
{

	private readonly IServiceProvider _ServiceProvider = dataViewHierarchyServiceProvider;

	~AbstractHostess()
	{
		Dispose(disposing: false);
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
			// _ServiceProvider = null;
		}
	}



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
			_HostService ??= _ServiceProvider.GetService<IVsDataHostService, IVsDataHostService>();

			if (_HostService == null)
				throw Diag.ExceptionService(typeof(IVsDataHostService));

			return _HostService;
		}
	}

	public bool IsUIThread => Thread.CurrentThread == HostService.UIThread;


	public IVsUIShell ShellService
	{
		get
		{
			if (_ShellService != null)
				return _ShellService;

			Diag.ThrowIfNotOnUIThread();

			_ShellService = HostService.GetService<SVsUIShell, IVsUIShell>();

			_ShellService ??= Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;

			if (_ShellService == null)
				throw Diag.ExceptionService(typeof(IVsUIShell));

			return _ShellService;
		}
	}


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	public static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);




	public bool ActivateDocumentIfOpen(string documentMoniker)
	{
		Evs.Trace(GetType(), nameof(ActivateDocumentIfOpen), "documentMoniker: {documentMoniker}.");

		Diag.ThrowIfNotOnUIThread();

		return ActivateDocumentIfOpen(documentMoniker, false) != null;
	}



	public IVsWindowFrame ActivateDocumentIfOpen(string mkDocument, bool doNotShowWindowFrame)
	{
		Evs.Trace(GetType(), "ActivateDocumentIfOpen(string, bool)",
			$"mkDocument: {mkDocument}, doNotShowWindowFrame: {doNotShowWindowFrame}.");

		IVsUIShellOpenDocument service = HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>()
			?? throw Diag.ExceptionService(typeof(IVsUIShellOpenDocument));

		Diag.ThrowIfNotOnUIThread();

		IVsWindowFrame ppWindowFrame;

		try
		{
			Guid rguidLogicalView = Guid.Empty;
			uint grfIDO = doNotShowWindowFrame ? 0u : (uint)__VSIDOFLAGS.IDO_ActivateIfOpen;

			_ = ___(service.IsDocumentOpen(null, uint.MaxValue, mkDocument, ref rguidLogicalView,
				grfIDO, out IVsUIHierarchy ppHierOpen, null, out ppWindowFrame, out int pfOpen));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return ppWindowFrame;

	}




	public void PostExecuteCommand(CommandID command, int delay = 0)
	{
		Evs.Trace(GetType(), nameof(PostExecuteCommand), "command: {command}.");

		_ = ShellService;

		if (delay == 0 && IsUIThread)
		{
			PostExecuteCommandImpl(command);
			return;
		}

		// Fallback if Site does not have an IVsDataHostService service
		if (delay > 0)
		{
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
		Diag.ThrowIfNotOnUIThread();

		Guid pguidCmdGroup = command.Guid;
		uint iD = (uint)command.ID;
		object pvaIn = null;
		___(ShellService.PostExecCommand(ref pguidCmdGroup, iD, 0u, ref pvaIn));
	}


}
