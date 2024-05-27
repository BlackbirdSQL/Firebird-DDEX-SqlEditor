#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
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

			AuxilliaryDocData auxDocData = AuxDocData;

			IBSqlEditorWindowPane lastFocusedSqlEditor = ((IBEditorPackage)ApcManager.PackageInstance).LastFocusedSqlEditor;

			if (lastFocusedSqlEditor == null)
				return VSConstants.S_OK;

			AuxilliaryDocData newAuxDocData = ((IBEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(lastFocusedSqlEditor.DocData);

			if (newAuxDocData == null || QryMgr == null)
				return VSConstants.S_OK;

			AbstractConnectionStrategy connectionStrategy = newAuxDocData.QryMgr.ConnectionStrategy;
			connectionStrategy.SetConnectionInfo(StoredQryMgr.ConnectionStrategy.ConnectionInfo);
			IDbConnection connection = connectionStrategy.Connection;

			if (StoredQryMgr.IsConnected && connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

			// auxDocData.IsVirtualWindow = auxDocData.IsVirtualWindow;
		}

		return VSConstants.S_OK;
	}
}
