﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion
using System;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;




namespace BlackbirdSql.Common.Ctl;

public abstract class AbstractViewFilter : IOleCommandTarget, IDisposable
{
	private IOleCommandTarget _NextTarget;

	private IVsTextView _TextView;

	public IVsTextView TextView => _TextView;

	private void SetView(IVsTextView view)
	{
		if (view != null)
		{
			___(view.AddCommandFilter(this, out _NextTarget));
			_TextView = view;
		}
	}

	public static void AddFilterToView(IVsTextView view, AbstractViewFilter filter)
	{
		filter.SetView(view);
	}

	/// <summary>
	/// ThrowOnFailure token
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);

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
}
