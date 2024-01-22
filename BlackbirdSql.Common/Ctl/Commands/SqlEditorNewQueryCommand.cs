#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Utilities;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Common.Ctl.Commands;

public class SqlEditorNewQueryCommand : AbstractSqlEditorCommand
{
	public SqlEditorNewQueryCommand()
	{
	}

	public SqlEditorNewQueryCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
		if (auxiliaryDocDataForEditor == null)
		{
			Exception ex = new("AuxiliaryDocData NOT FOUND");
			Diag.Dug(ex);
			return VSConstants.S_OK;
		}

		QueryManager qryMgr = auxiliaryDocDataForEditor.QryMgr;

		if (qryMgr == null)
		{
			ArgumentNullException ex = new("QryMgr is null");
			Diag.Dug(ex);
			return VSConstants.S_OK;
		}

		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!string.IsNullOrWhiteSpace(qryMgr.ConnectionStrategy.LastDatasetKey))
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
			if (auxiliaryDocDataForEditor == null)
			{
				Exception ex = new("AuxiliaryDocData NOT FOUND");
				Diag.Dug(ex);
				return VSConstants.S_OK;
			}

			QueryManager qryMgr = auxiliaryDocDataForEditor.QryMgr;

			if (qryMgr == null)
			{
				ArgumentNullException ex = new("QryMgr is null");
				Diag.Dug(ex);
				return VSConstants.S_OK;
			}

			IBDesignerExplorerServices service = Controller.GetService<IBDesignerExplorerServices>()
				?? throw Diag.ExceptionService(typeof(IBDesignerExplorerServices));

			service.NewSqlQuery(qryMgr.ConnectionStrategy.LastDatasetKey);
		}

		return VSConstants.S_OK;
	}
}
