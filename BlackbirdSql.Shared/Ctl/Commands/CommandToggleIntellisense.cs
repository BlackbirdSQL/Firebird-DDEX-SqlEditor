// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorToggleIntellisenseCommand

using System;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandToggleIntellisense : AbstractCommand
{
	public CommandToggleIntellisense()
	{
	}

	public CommandToggleIntellisense(IBsTabbedEditorPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;


		if (CachedAuxDocData.IntellisenseEnabled != null && CachedAuxDocData.IntellisenseEnabled.Value)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (!ExecutionLocked && CachedAuxDocData != null)
		{
			bool enabled = CachedAuxDocData.IntellisenseEnabled != null
				&& CachedAuxDocData.IntellisenseEnabled.Value;

			CachedAuxDocData.IntellisenseEnabled = !enabled;
		}

		return VSConstants.S_OK;
	}
}
