﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.TextResultsViewContol
// using System;

using System;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Controls.Results;


public class TextResultsViewContol : ShellTextViewControl, IOleCommandTarget, IVsTextViewEvents
{
	private IOleCommandTarget nextTarget;

	protected override void CreateEditorWindow(object nativeServiceProvier)
	{
		base.CreateEditorWindow(nativeServiceProvier);
		___(TextView.AddCommandFilter(this, out nextTarget));
	}

	protected override void Dispose(bool disposing)
	{
		if (TextView != null)
		{
			___(TextView.RemoveCommandFilter(this));
			nextTarget = null;
		}

		base.Dispose(disposing);
	}

	int IOleCommandTarget.QueryStatus(ref Guid guidGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		if (guidGroup.Equals(VS.CLSID_CTextViewCommandGroup))
		{
			switch ((VSConstants.VSStd2KCmdID)prgCmds[0].cmdID)
			{
				case VSConstants.VSStd2KCmdID.PARAMINFO:
				case VSConstants.VSStd2KCmdID.COMPLETEWORD:
				case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
				case VSConstants.VSStd2KCmdID.FORMATSELECTION:
				case VSConstants.VSStd2KCmdID.QUICKINFO:
				case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
				case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
				case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
					return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
			}
		}

		if (guidGroup.Equals(VS.CLSID_XmlUiCmds))
		{
			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		}

		if (guidGroup.Equals(VSConstants.CMDSETID.StandardCommandSet97_guid)
			&& (prgCmds[0].cmdID == (int)VSConstants.VSStd97CmdID.SetNextStatement
			|| prgCmds[0].cmdID == (int)VSConstants.VSStd97CmdID.ViewForm))
		{
			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		}

		if (guidGroup.Equals(VSConstants.CMDSETID.StandardCommandSet2K_guid)
			&& prgCmds[0].cmdID == (int)VSConstants.VSStd2KCmdID.DOUBLECLICK)
		{
			prgCmds[0].cmdf |= (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
			return VSConstants.S_OK;
		}

		if (nextTarget != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return nextTarget.QueryStatus(ref guidGroup, cCmds, prgCmds, pCmdText);
		}

		return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
	}

	int IOleCommandTarget.Exec(ref Guid guidGroup, uint commandId, uint nCmdExcept, IntPtr vIn, IntPtr vOut)
	{
		if (guidGroup.Equals(VS.CLSID_CTextViewCommandGroup))
		{
			switch ((VSConstants.VSStd2KCmdID)commandId)
			{
				case VSConstants.VSStd2KCmdID.PARAMINFO:
				case VSConstants.VSStd2KCmdID.COMPLETEWORD:
				case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
				case VSConstants.VSStd2KCmdID.FORMATSELECTION:
				case VSConstants.VSStd2KCmdID.QUICKINFO:
				case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
				case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
				case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
					return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
			}
		}

		if (guidGroup.Equals(VS.CLSID_XmlUiCmds))
		{
			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		}

		if (guidGroup.Equals(VSConstants.CMDSETID.StandardCommandSet97_guid)
			&& (commandId == 258 || commandId == 332))
		{
			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		}

		if (guidGroup.Equals(VSConstants.CMDSETID.StandardCommandSet2K_guid)
			&& commandId == 134 && vIn != IntPtr.Zero)
		{
			int num = -1;
			int num2 = -1;
			try
			{
				num = (int)Marshal.GetObjectForNativeVariant(vIn);
				num2 = (int)Marshal.GetObjectForNativeVariant(vIn + 16);
			}
			catch
			{
			}

			if (num >= 0 && num2 >= 0)
			{
				GetTextMarkerAtPosition(num, num2)?.ExecMarkerCommand(258);
			}
		}

		if (nextTarget != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return nextTarget.Exec(ref guidGroup, commandId, nCmdExcept, vIn, vOut);
		}

		return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
	}

	private IVsTextMarker GetTextMarkerAtPosition(int lineNum, int columnNum)
	{
		TextBuffer.VsTextBuffer.GetPositionOfLine(lineNum, out int piPosition);
		piPosition += columnNum;
		TextBuffer.TextStream.FindMarkerByPosition(ShellTextBuffer.markerTypeError, piPosition + 1, 1u, out IVsTextStreamMarker ppMarker);
		if (ppMarker != null)
		{
			ppMarker.GetCurrentSpan(out int piPos, out int piLen);
			if (piPosition >= piPos && piPosition <= piPos + piLen)
			{
				return ppMarker;
			}
		}

		TextBuffer.TextStream.FindMarkerByPosition(ShellTextBuffer.markerTypeError, piPosition, 0u, out ppMarker);
		if (ppMarker != null)
		{
			ppMarker.GetCurrentSpan(out int piPos2, out int piLen2);
			if (piPosition >= piPos2 && piPosition <= piPos2 + piLen2)
			{
				return ppMarker;
			}
		}

		return null;
	}
}
