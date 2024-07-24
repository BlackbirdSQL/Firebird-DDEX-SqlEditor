
using System;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
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



	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{

			IBsDesignerExplorerServices service = ApcManager.EnsureService<IBsDesignerExplorerServices>();

			service.NewQuery(StoredQryMgr?.Strategy?.CurrentDatasetKey, Resources.NewQueryBaseName, null);
		}

		return VSConstants.S_OK;
	}
}
