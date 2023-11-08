#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Core;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Common.Ctl.Commands;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

[ComVisible(false)]
public sealed class TextViewCommandFilter : IOleCommandTarget, IDisposable
{
	private IOleCommandTarget _NextTarget;

	private IVsTextView _MyTextView;

	private readonly int[] _RecongizableCommands;



	public event SpecialEditorCommandEventHandler SpecialEditorCommand;




	public TextViewCommandFilter(IVsTextView view, int[] recongizableCommands)
	{
		if (view != null)
		{
			Core.Native.ThrowOnFailure(view.AddCommandFilter(this, out _NextTarget), (string)null);
			_MyTextView = view;
			_RecongizableCommands = recongizableCommands;
		}
	}

	public void Dispose()
	{
		if (_MyTextView != null)
		{
			Core.Native.ThrowOnFailure(_MyTextView.RemoveCommandFilter(this), (string)null);
			_MyTextView = null;
		}

		if (_NextTarget != null)
		{
			_NextTarget = null;
		}

		GC.SuppressFinalize(this);
	}

	public int QueryStatus(ref Guid guidGroup, uint nCmdId, OLECMD[] oleCmd, IntPtr oleText)
	{
		if (guidGroup.Equals(VS.CLSID_CTextViewCommandGroup) && _RecongizableCommands != null)
		{
			int[] recongizableCommands = _RecongizableCommands;
			for (int i = 0; i < recongizableCommands.Length; i++)
			{
				if (recongizableCommands[i] == (int)nCmdId)
				{
					oleCmd[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
					return VSConstants.S_OK;
				}
			}
		}

		if (_NextTarget == null)
		{
			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		}

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		return _NextTarget.QueryStatus(ref guidGroup, nCmdId, oleCmd, oleText);
	}

	public int Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr vIn, IntPtr vOut)
	{
		if (guidGroup.Equals(VS.CLSID_CTextViewCommandGroup) && _RecongizableCommands != null)
		{
			int[] recongizableCommands = _RecongizableCommands;
			for (int i = 0; i < recongizableCommands.Length; i++)
			{
				if (recongizableCommands[i] == (int)nCmdId)
				{
					if (SpecialEditorCommand != null)
					{
						object[] vIn2 = Native.IntPtrToObjectArray(vIn);
						SpecialEditorCommandEventArgs e = new SpecialEditorCommandEventArgs((int)nCmdId, vIn2, vOut);
						SpecialEditorCommand(this, e);
					}

					return VSConstants.S_OK;
				}
			}
		}

		if (_NextTarget == null)
			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		return _NextTarget.Exec(ref guidGroup, nCmdId, nCmdExcept, vIn, vOut);
	}
}
