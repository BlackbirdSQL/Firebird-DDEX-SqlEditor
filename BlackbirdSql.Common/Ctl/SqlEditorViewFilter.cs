#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Ctl.Commands;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Common.Enums;

namespace BlackbirdSql.Common.Ctl;


public class SqlEditorViewFilter : AbstractViewFilter
{
	public ISqlEditorWindowPane Editor { get; private set; }

	public SqlEditorViewFilter(ISqlEditorWindowPane editor)
	{
		Editor = editor;
	}

	public override int Exec(ref Guid pguidCmdGroup, uint cmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AbstractSqlEditorCommand sqlEditorCommand = null;

		SqlEditorCmdSet cmd = (SqlEditorCmdSet)cmdId;
		if (pguidCmdGroup == LibraryData.CLSID_SqlEditorCommandSet)
		{
			switch (cmd)
			{
				case SqlEditorCmdSet.CmdIdConnect:
					sqlEditorCommand = new SqlEditorConnectCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdDisconnect:
					sqlEditorCommand = new SqlEditorDisconnectCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdDisconnectAllQueries:
					sqlEditorCommand = new SqlEditorDisconnectAllQueriesCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdChangeConnection:
					sqlEditorCommand = new SqlEditorChangeConnectionCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdExecuteQuery:
					sqlEditorCommand = new SqlEditorExecuteQueryCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdParseQuery:
					sqlEditorCommand = new SqlEditorParseQueryCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdCancelQuery:
					sqlEditorCommand = new SqlEditorCancelQueryCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdShowEstimatedPlan:
					sqlEditorCommand = new SqlEditorShowEstimatedPlanCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdToggleClientStatistics:
					sqlEditorCommand = new SqlEditorToggleClientStatisticsCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdToggleExecutionPlan:
					sqlEditorCommand = new SqlEditorToggleExecutionPlanCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdToggleIntellisense:
					sqlEditorCommand = new SqlEditorToggleIntellisenseCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdToggleSQLCMDMode:
					sqlEditorCommand = new SqlEditorToggleSqlCmdModeCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdQueryOptions:
					sqlEditorCommand = new SqlEditorQueryOptionsCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdResultsAsText:
					sqlEditorCommand = new SqlEditorResultsAsTextCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdResultsAsGrid:
					sqlEditorCommand = new SqlEditorResultsAsGridCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdResultsAsFile:
					sqlEditorCommand = new SqlEditorResultsAsFileCommand(Editor);
					break;
				case SqlEditorCmdSet.CmdIdToggleResultsPane:
					sqlEditorCommand = new SqlEditorToggleResultsPaneCommand(Editor);
					break;
				case SqlEditorCmdSet.CmbIdSQLDatabases:
					sqlEditorCommand = new SqlEditorSqlDatabaseCommand(Editor);
					break;
				case SqlEditorCmdSet.CmbIdSQLDatabasesGetList:
					sqlEditorCommand = new SqlEditorSqlDatabaseListCommand(Editor);
					break;
			}

			if (sqlEditorCommand != null)
			{
				return sqlEditorCommand.Exec(nCmdexecopt, pvaIn, pvaOut);
			}
		}

		return base.Exec(ref pguidCmdGroup, cmdId, nCmdexecopt, pvaIn, pvaOut);
	}

	public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		int num = (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		for (int i = 0; i < cCmds; i++)
		{
			SqlEditorCmdSet cmdID = (SqlEditorCmdSet)prgCmds[i].cmdID;
			if (pguidCmdGroup == LibraryData.CLSID_SqlEditorCommandSet)
			{
				AbstractSqlEditorCommand sqlEditorCommand = null;
				switch (cmdID)
				{
					case SqlEditorCmdSet.CmdIdConnect:
						sqlEditorCommand = new SqlEditorConnectCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdDisconnect:
						sqlEditorCommand = new SqlEditorDisconnectCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdDisconnectAllQueries:
						sqlEditorCommand = new SqlEditorDisconnectAllQueriesCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdChangeConnection:
						sqlEditorCommand = new SqlEditorChangeConnectionCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdExecuteQuery:
						sqlEditorCommand = new SqlEditorExecuteQueryCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdParseQuery:
						sqlEditorCommand = new SqlEditorParseQueryCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdCancelQuery:
						sqlEditorCommand = new SqlEditorCancelQueryCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdShowEstimatedPlan:
						sqlEditorCommand = new SqlEditorShowEstimatedPlanCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdToggleClientStatistics:
						sqlEditorCommand = new SqlEditorToggleClientStatisticsCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdToggleExecutionPlan:
						sqlEditorCommand = new SqlEditorToggleExecutionPlanCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdToggleIntellisense:
						sqlEditorCommand = new SqlEditorToggleIntellisenseCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdToggleSQLCMDMode:
						sqlEditorCommand = new SqlEditorToggleSqlCmdModeCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdQueryOptions:
						sqlEditorCommand = new SqlEditorQueryOptionsCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdResultsAsText:
						sqlEditorCommand = new SqlEditorResultsAsTextCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdResultsAsGrid:
						sqlEditorCommand = new SqlEditorResultsAsGridCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdResultsAsFile:
						sqlEditorCommand = new SqlEditorResultsAsFileCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdToggleResultsPane:
						sqlEditorCommand = new SqlEditorToggleResultsPaneCommand(Editor);
						break;
					case SqlEditorCmdSet.CmbIdSQLDatabases:
						sqlEditorCommand = new SqlEditorSqlDatabaseCommand(Editor);
						break;
					case SqlEditorCmdSet.CmbIdSQLDatabasesGetList:
						sqlEditorCommand = new SqlEditorSqlDatabaseListCommand(Editor);
						break;
					case SqlEditorCmdSet.CmdIdCloneQuery:
						sqlEditorCommand = new SqlEditorCloneQueryWindowCommand(Editor);
						break;
				}

				if (sqlEditorCommand != null)
				{
					num = sqlEditorCommand.QueryStatus(ref prgCmds[i], pCmdText);
				}
			}

			if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
			{
				switch ((VSConstants.VSStd2KCmdID)cmdID)
				{
					case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
					case VSConstants.VSStd2KCmdID.SURROUNDWITH:
						{
							AuxiliaryDocData auxillaryDocData = ((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(Editor.DocData);
							if (auxillaryDocData != null && auxillaryDocData.QueryExecutor != null && !auxillaryDocData.QueryExecutor.IsExecuting && !auxillaryDocData.QueryExecutor.IsDebugging)
							{
								prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
							}
							else
							{
								prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
							}

							num = 0;
							break;
						}
					case VSConstants.VSStd2KCmdID.FORMATSELECTION:
						prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						num = 0;
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
						num = 0;
						break;
					case VSConstants.VSStd97CmdID.GotoDefn:
					case VSConstants.VSStd97CmdID.FindReferences:
						{
							AuxiliaryDocData auxillaryDocData2 = ((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(Editor.DocData);
							if (auxillaryDocData2 != null && (auxillaryDocData2.Strategy.IsOnline || auxillaryDocData2.Strategy is DefaultSqlEditorStrategy))
							{
								prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
							}
							else
							{
								prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
							}

							num = 0;
							break;
						}
					case VSConstants.VSStd97CmdID.ShowProperties:
						prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
						num = 0;
						break;
				}
			}

			/*
			if (pguidCmdGroup == VSConstantsInternal.GuidDebugCommandSet && cmdID == 65)
			{
				prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
				num = 0;
			}
			else if (pguidCmdGroup == SqlGuidList.cmdSetGuidTeamSystemData && cmdID == 49665)
			{
				prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
				num = 0;
			}
			*/
		}

		if (BlackbirdSql.Core.Cmd.Failed(num))
		{
			num = base.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		}

		return num;
	}
}
