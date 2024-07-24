// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.BaseViewFilter

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public abstract class AbstractViewCommandFilter : IOleCommandTarget, IDisposable
{

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}


	protected virtual void Dispose(bool disposing)
	{
		if (disposing && _TextView != null)
		{
			_TextView.RemoveCommandFilter(this);
			_TextView = null;
		}
	}




	private IOleCommandTarget _NextTarget;

	private IVsTextView _TextView;

	public IVsTextView TextView => _TextView;



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	private void SetView(IVsTextView view)
	{
		if (view != null)
		{
			___(view.AddCommandFilter(this, out _NextTarget));
			_TextView = view;
		}
	}



	public static void AddFilterToView(IVsTextView view, AbstractViewCommandFilter filter)
	{
		filter.SetView(view);
	}



	public virtual int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (_NextTarget != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _NextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}

		return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
	}



	public virtual int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		if (_NextTarget != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _NextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		}

		return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
	}

}
