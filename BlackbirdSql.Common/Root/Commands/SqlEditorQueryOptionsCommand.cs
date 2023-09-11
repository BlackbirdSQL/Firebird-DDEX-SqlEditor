#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Common.Controls.ToolsOptions;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Commands;


public class SqlEditorQueryOptionsCommand : AbstractSqlEditorCommand
{
	public SqlEditorQueryOptionsCommand(ISqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		QueryManager qryMgrForEditor = GetQueryManagerForEditor();
		if (qryMgrForEditor != null && !qryMgrForEditor.IsExecuting)
		{
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		using (SqlCurrentWndOptionsDlg currentWndSQLOptions = new SqlCurrentWndOptionsDlg())
		{
			AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
			if (auxiliaryDocDataForEditor != null)
			{
				QueryManager qryMgr = auxiliaryDocDataForEditor.QryMgr;
				if (qryMgr != null)
				{
					currentWndSQLOptions.StartPosition = FormStartPosition.CenterParent;
					currentWndSQLOptions.Serialize(qryMgr.QueryExecutionSettings, bToControls: true);
					if (DialogResult.OK == FormUtilities.ShowDialog(currentWndSQLOptions))
					{
						currentWndSQLOptions.Serialize(qryMgr.QueryExecutionSettings, bToControls: false);
						auxiliaryDocDataForEditor.UpdateExecutionSettings();
					}
				}
			}
		}

		return VSConstants.S_OK;
	}
}
