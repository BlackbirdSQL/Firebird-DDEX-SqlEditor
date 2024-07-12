#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandNewQuery : AbstractCommand
{

	public CommandNewQuery()
	{
	}

	public CommandNewQuery(IBSqlEditorWindowPane windowPane) : base(windowPane)
	{
	}



	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		if (AuxDocData == null)
		{
			Exception ex = new("AuxilliaryDocData NOT FOUND");
			Diag.Dug(ex);
			return VSConstants.S_OK;
		}

		if (QryMgr == null)
		{
			ArgumentNullException ex = new("QryMgr is null");
			Diag.Dug(ex);
			return VSConstants.S_OK;
		}

		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!string.IsNullOrWhiteSpace(StoredQryMgr.Strategy.LastDatasetKey))
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			if (AuxDocData == null)
			{
				Exception ex = new("AuxilliaryDocData NOT FOUND");
				Diag.Dug(ex);
				return VSConstants.S_OK;
			}

			if (QryMgr == null)
			{
				ArgumentNullException ex = new("QryMgr is null");
				Diag.Dug(ex);
				return VSConstants.S_OK;
			}

			IBDesignerExplorerServices service = ApcManager.EnsureService<IBDesignerExplorerServices>();

			service.NewQuery(StoredQryMgr.Strategy.LastDatasetKey);
		}

		return VSConstants.S_OK;
	}
}
