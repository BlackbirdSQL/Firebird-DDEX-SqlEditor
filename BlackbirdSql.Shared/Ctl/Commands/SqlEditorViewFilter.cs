#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.CommandProviders;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Ctl.Commands;



public class SqlEditorViewFilter : AbstractViewFilter
{

	public SqlEditorViewFilter(IBSqlEditorWindowPane editorWindow)
	{
		Editor = editorWindow;
	}




	public IBSqlEditorWindowPane Editor { get; private set; }


	public override int Exec(ref Guid pguidCmdGroup, uint cmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AbstractSqlEditorCommand sqlEditorCommand = null;

		EnCommandSet cmd = (EnCommandSet)cmdId;
		if (pguidCmdGroup == CommandProperties.ClsidCommandSet)
		{
			switch (cmd)
			{
				case EnCommandSet.CmdIdConnect:
					sqlEditorCommand = new SqlEditorConnectCommand(Editor);
					break;
				case EnCommandSet.CmdIdDisconnect:
					sqlEditorCommand = new SqlEditorDisconnectCommand(Editor);
					break;
				case EnCommandSet.CmdIdDisconnectAllQueries:
					sqlEditorCommand = new SqlEditorDisconnectAllQueriesCommand(Editor);
					break;
				case EnCommandSet.CmdIdChangeConnection:
					sqlEditorCommand = new SqlEditorChangeConnectionCommand(Editor);
					break;
				case EnCommandSet.CmdIdExecuteQuery:
					sqlEditorCommand = new SqlEditorExecuteQueryCommand(Editor);
					break;
				case EnCommandSet.CmdIdCancelQuery:
					sqlEditorCommand = new SqlEditorCancelQueryCommand(Editor);
					break;
				case EnCommandSet.CmdIdShowEstimatedPlan:
					sqlEditorCommand = new SqlEditorShowEstimatedPlanCommand(Editor);
					break;
				case EnCommandSet.CmdIdToggleClientStatistics:
					sqlEditorCommand = new SqlEditorToggleClientStatisticsCommand(Editor);
					break;
				case EnCommandSet.CmdIdToggleExecutionPlan:
					sqlEditorCommand = new SqlEditorToggleExecutionPlanCommand(Editor);
					break;
				case EnCommandSet.CmdIdToggleIntellisense:
					sqlEditorCommand = new SqlEditorToggleIntellisenseCommand(Editor);
					break;
				case EnCommandSet.CmdIdToggleSQLCMDMode:
					sqlEditorCommand = new SqlEditorToggleSqlCmdModeCommand(Editor);
					break;
				case EnCommandSet.CmdIdQuerySettings:
					sqlEditorCommand = new SqlEditorQuerySettingsCommand(Editor);
					break;
				case EnCommandSet.CmdIdResultsAsText:
					sqlEditorCommand = new SqlEditorResultsAsTextCommand(Editor);
					break;
				case EnCommandSet.CmdIdResultsAsGrid:
					sqlEditorCommand = new SqlEditorResultsAsGridCommand(Editor);
					break;
				case EnCommandSet.CmdIdResultsAsFile:
					sqlEditorCommand = new SqlEditorResultsAsFileCommand(Editor);
					break;
				case EnCommandSet.CmdIdToggleResultsPane:
					sqlEditorCommand = new SqlEditorToggleResultsPaneCommand(Editor);
					break;
				case EnCommandSet.CmbIdSqlDatabases:
					sqlEditorCommand = new SqlEditorDatabaseCommand(Editor);
					break;
				case EnCommandSet.CmbIdSqlDatabasesGetList:
					sqlEditorCommand = new SqlEditorDatabaseListCommand(Editor);
					break;
				case EnCommandSet.CmdIdNewSqlQuery:
					sqlEditorCommand = new SqlEditorNewQueryCommand(Editor);
					break;
				case EnCommandSet.CmdIdTransactionCommit:
					sqlEditorCommand = new SqlEditorTransactionCommitCommand(Editor);
					break;
				case EnCommandSet.CmdIdToggleTTS:
					sqlEditorCommand = new SqlEditorToggleTTSCommand(Editor);
					break;
			}

			if (sqlEditorCommand != null)
				return sqlEditorCommand.Exec(nCmdexecopt, pvaIn, pvaOut);
		}

		return base.Exec(ref pguidCmdGroup, cmdId, nCmdexecopt, pvaIn, pvaOut);
	}

	public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		int hresult = (int)Constants.MSOCMDERR_E_NOTSUPPORTED;

		for (int i = 0; i < cCmds; i++)
		{
			EnCommandSet cmdID = (EnCommandSet)prgCmds[i].cmdID;
			if (pguidCmdGroup == CommandProperties.ClsidCommandSet)
			{
				AbstractSqlEditorCommand sqlEditorCommand = null;
				switch (cmdID)
				{
					case EnCommandSet.CmdIdConnect:
						sqlEditorCommand = new SqlEditorConnectCommand(Editor);
						break;
					case EnCommandSet.CmdIdDisconnect:
						sqlEditorCommand = new SqlEditorDisconnectCommand(Editor);
						break;
					case EnCommandSet.CmdIdDisconnectAllQueries:
						sqlEditorCommand = new SqlEditorDisconnectAllQueriesCommand(Editor);
						break;
					case EnCommandSet.CmdIdChangeConnection:
						sqlEditorCommand = new SqlEditorChangeConnectionCommand(Editor);
						break;
					case EnCommandSet.CmdIdExecuteQuery:
						sqlEditorCommand = new SqlEditorExecuteQueryCommand(Editor);
						break;
					case EnCommandSet.CmdIdCancelQuery:
						sqlEditorCommand = new SqlEditorCancelQueryCommand(Editor);
						break;
					case EnCommandSet.CmdIdShowEstimatedPlan:
						sqlEditorCommand = new SqlEditorShowEstimatedPlanCommand(Editor);
						break;
					case EnCommandSet.CmdIdToggleClientStatistics:
						sqlEditorCommand = new SqlEditorToggleClientStatisticsCommand(Editor);
						break;
					case EnCommandSet.CmdIdToggleExecutionPlan:
						sqlEditorCommand = new SqlEditorToggleExecutionPlanCommand(Editor);
						break;
					case EnCommandSet.CmdIdToggleIntellisense:
						sqlEditorCommand = new SqlEditorToggleIntellisenseCommand(Editor);
						break;
					case EnCommandSet.CmdIdToggleSQLCMDMode:
						sqlEditorCommand = new SqlEditorToggleSqlCmdModeCommand(Editor);
						break;
					case EnCommandSet.CmdIdQuerySettings:
						sqlEditorCommand = new SqlEditorQuerySettingsCommand(Editor);
						break;
					case EnCommandSet.CmdIdResultsAsText:
						sqlEditorCommand = new SqlEditorResultsAsTextCommand(Editor);
						break;
					case EnCommandSet.CmdIdResultsAsGrid:
						sqlEditorCommand = new SqlEditorResultsAsGridCommand(Editor);
						break;
					case EnCommandSet.CmdIdResultsAsFile:
						sqlEditorCommand = new SqlEditorResultsAsFileCommand(Editor);
						break;
					case EnCommandSet.CmdIdToggleResultsPane:
						sqlEditorCommand = new SqlEditorToggleResultsPaneCommand(Editor);
						break;
					case EnCommandSet.CmbIdSqlDatabases:
						sqlEditorCommand = new SqlEditorDatabaseCommand(Editor);
						break;
					case EnCommandSet.CmbIdSqlDatabasesGetList:
						sqlEditorCommand = new SqlEditorDatabaseListCommand(Editor);
						break;
					case EnCommandSet.CmdIdCloneQuery:
						sqlEditorCommand = new SqlEditorCloneQueryWindowCommand(Editor);
						break;
					case EnCommandSet.CmdIdNewSqlQuery:
						sqlEditorCommand = new SqlEditorNewQueryCommand(Editor);
						break;
					case EnCommandSet.CmdIdTransactionCommit:
						sqlEditorCommand = new SqlEditorTransactionCommitCommand(Editor);
						break;
					case EnCommandSet.CmdIdToggleTTS:
						sqlEditorCommand = new SqlEditorToggleTTSCommand(Editor);
						break;
				}

				if (sqlEditorCommand != null)
					hresult = sqlEditorCommand.QueryStatus(ref prgCmds[i], pCmdText);
			}

			if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
			{
				switch ((VSConstants.VSStd2KCmdID)cmdID)
				{
					case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
					case VSConstants.VSStd2KCmdID.SURROUNDWITH:
						{
							AuxilliaryDocData auxDocData = ((IBEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(Editor.DocData);
							if (auxDocData != null && auxDocData.QryMgr != null && !auxDocData.QryMgr.IsExecuting)
							{
								prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
							}
							else
							{
								prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
							}

							hresult = VSConstants.S_OK;
							break;
						}
					case VSConstants.VSStd2KCmdID.FORMATSELECTION:
						prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						hresult = VSConstants.S_OK;
						break;
				}
			}

			if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet97_guid)
			{
				switch ((VSConstants.VSStd97CmdID)cmdID)
				{
					case VSConstants.VSStd97CmdID.RunToCursor:
					case VSConstants.VSStd97CmdID.InsertBreakpoint:
					case VSConstants.VSStd97CmdID.GotoDecl:
					case VSConstants.VSStd97CmdID.GotoRef:
						prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						hresult = VSConstants.S_OK;
						break;
					case VSConstants.VSStd97CmdID.GotoDefn:
					case VSConstants.VSStd97CmdID.FindReferences:
						{
							AuxilliaryDocData auxillaryDocData2 = ((IBEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(Editor.DocData);
							if (auxillaryDocData2 != null && (auxillaryDocData2.Strategy.IsOnline || auxillaryDocData2.Strategy is DefaultSqlEditorStrategy))
							{
								prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
							}
							else
							{
								prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
							}

							hresult = VSConstants.S_OK;
							break;
						}
					case VSConstants.VSStd97CmdID.ShowProperties:
						prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
						hresult = VSConstants.S_OK;
						break;
				}
			}

			if (pguidCmdGroup == VS.ClsidVSDebugCommand)
			{
				prgCmds[i].cmdf = (uint)(/* OLECMDF.OLECMDF_SUPPORTED | */ OLECMDF.OLECMDF_INVISIBLE);
				hresult = VSConstants.S_OK;
			}
			else if (pguidCmdGroup == VSConstants.VsStd14 && prgCmds[i].cmdID == (uint)VSConstants.VSStd14CmdID.ShowQuickFixesForPosition)
			{
				prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
				hresult = VSConstants.S_OK;
			}
			/*
			else if (pguidCmdGroup == SqlGuidList.cmdSetGuidTeamSystemData && cmdID == EnCommandSet.MenuIdToplevel)
			{
				prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
				num = 0;
			}
			*/
		}

		if (!__(hresult))
			hresult = base.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

		return hresult;
	}
}
