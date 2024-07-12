#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandCloneQueryWindow : AbstractCommand
{
	public CommandCloneQueryWindow()
	{
	}

	public CommandCloneQueryWindow(IBSqlEditorWindowPane windowPane) : base(windowPane)
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

			AbstractConnectionStrategy connectionStrategy = newAuxDocData.QryMgr.Strategy;
			connectionStrategy.SetConnectionInfo(StoredQryMgr.Strategy.ConnectionInfo);
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
