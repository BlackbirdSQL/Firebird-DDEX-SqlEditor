#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.InteropServices;

using BlackbirdSql.Core;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using BlackbirdSql.Common.Events;

namespace BlackbirdSql.Common.Controls;


[ComVisible(false)]
public sealed class TextViewCommandFilter : IOleCommandTarget, IDisposable
{
	private IOleCommandTarget m_nextTarget;

	private IVsTextView m_myTextView;

	private readonly int[] m_recongizableCommands;

	public event SpecialEditorCommandEventHandler SpecialEditorCommand;

	public TextViewCommandFilter(IVsTextView view, int[] recongizableCommands)
	{
		if (view != null)
		{
			Native.ThrowOnFailure(view.AddCommandFilter(this, out m_nextTarget), (string)null);
			m_myTextView = view;
			m_recongizableCommands = recongizableCommands;
		}
	}

	public void Dispose()
	{
		if (m_myTextView != null)
		{
			Native.ThrowOnFailure(m_myTextView.RemoveCommandFilter(this), (string)null);
			m_myTextView = null;
		}

		if (m_nextTarget != null)
		{
			m_nextTarget = null;
		}

		GC.SuppressFinalize(this);
	}

	public int QueryStatus(ref Guid guidGroup, uint nCmdId, OLECMD[] oleCmd, IntPtr oleText)
	{
		if (guidGroup.Equals(VS.CLSID_CTextViewCommandGroup) && m_recongizableCommands != null)
		{
			int[] recongizableCommands = m_recongizableCommands;
			for (int i = 0; i < recongizableCommands.Length; i++)
			{
				if (recongizableCommands[i] == (int)nCmdId)
				{
					oleCmd[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
					return VSConstants.S_OK;
				}
			}
		}

		if (m_nextTarget == null)
		{
			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		}

		return m_nextTarget.QueryStatus(ref guidGroup, nCmdId, oleCmd, oleText);
	}

	public int Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr vIn, IntPtr vOut)
	{
		if (guidGroup.Equals(VS.CLSID_CTextViewCommandGroup) && m_recongizableCommands != null)
		{
			int[] recongizableCommands = m_recongizableCommands;
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

		if (m_nextTarget == null)
		{
			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		}

		return m_nextTarget.Exec(ref guidGroup, nCmdId, nCmdExcept, vIn, vOut);
	}
}
