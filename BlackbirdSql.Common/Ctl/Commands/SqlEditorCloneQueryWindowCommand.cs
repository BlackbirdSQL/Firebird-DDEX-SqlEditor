#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Utilities;

namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorCloneQueryWindowCommand : AbstractSqlEditorCommand
{
	public SqlEditorCloneQueryWindowCommand()
	{
	}

	public SqlEditorCloneQueryWindowCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			DesignerExplorerServices.OpenNewMiscellaneousSqlFile();

			AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
			IBSqlEditorWindowPane lastFocusedSqlEditor = ((IBEditorPackage)ApcManager.DdexPackage).LastFocusedSqlEditor;
			if (lastFocusedSqlEditor != null)
			{
				AuxiliaryDocData auxDocData = ((IBEditorPackage)ApcManager.DdexPackage).GetAuxiliaryDocData(lastFocusedSqlEditor.DocData);
				if (auxDocData != null)
				{
					QueryManager qryMgr = auxiliaryDocDataForEditor.QryMgr;
					AbstractConnectionStrategy connectionStrategy = auxDocData.QryMgr.ConnectionStrategy;
					connectionStrategy.SetConnectionInfo(qryMgr.ConnectionStrategy.ConnectionInfo);
					IDbConnection connection = connectionStrategy.Connection;
					if (qryMgr.IsConnected && connection.State != ConnectionState.Open)
					{
						connection.Open();
					}

					// auxDocData.IsVirtualWindow = auxiliaryDocDataForEditor.IsVirtualWindow;
				}
			}
		}

		return VSConstants.S_OK;
	}
}
