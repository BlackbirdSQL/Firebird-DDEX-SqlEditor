#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Diagnostics.CodeAnalysis;

using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Enums;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;


namespace BlackbirdSql.Common.Ctl.Commands;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Base class is UIThread compliant.")]

public class SqlEditorViewFilter : AbstractViewFilter
{
	public IBSqlEditorWindowPane Editor { get; private set; }

	public SqlEditorViewFilter(IBSqlEditorWindowPane editorWindow)
	{
		Editor = editorWindow;
	}

	public override int Exec(ref Guid pguidCmdGroup, uint cmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AbstractSqlEditorCommand sqlEditorCommand = null;

		EnCommandSet cmd = (EnCommandSet)cmdId;
		if (pguidCmdGroup == LibraryData.CLSID_CommandSet)
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
				case EnCommandSet.CmdIdParseQuery:
					sqlEditorCommand = new SqlEditorParseQueryCommand(Editor);
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
					sqlEditorCommand = new SqlEditorSqlDatabaseCommand(Editor);
					break;
				case EnCommandSet.CmbIdSqlDatabasesGetList:
					sqlEditorCommand = new SqlEditorSqlDatabaseListCommand(Editor);
					break;
				case EnCommandSet.CmdIdNewSqlQuery:
					sqlEditorCommand = new SqlEditorNewQueryCommand(Editor);
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
			EnCommandSet cmdID = (EnCommandSet)prgCmds[i].cmdID;
			if (pguidCmdGroup == LibraryData.CLSID_CommandSet)
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
					case EnCommandSet.CmdIdParseQuery:
						sqlEditorCommand = new SqlEditorParseQueryCommand(Editor);
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
						sqlEditorCommand = new SqlEditorSqlDatabaseCommand(Editor);
						break;
					case EnCommandSet.CmbIdSqlDatabasesGetList:
						sqlEditorCommand = new SqlEditorSqlDatabaseListCommand(Editor);
						break;
					case EnCommandSet.CmdIdCloneQuery:
						sqlEditorCommand = new SqlEditorCloneQueryWindowCommand(Editor);
						break;
					case EnCommandSet.CmdIdNewSqlQuery:
						sqlEditorCommand = new SqlEditorNewQueryCommand(Editor);
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
							AuxiliaryDocData auxDocData = ((IBEditorPackage)Controller.DdexPackage).GetAuxiliaryDocData(Editor.DocData);
							if (auxDocData != null && auxDocData.QryMgr != null && !auxDocData.QryMgr.IsExecuting && !auxDocData.QryMgr.IsDebugging)
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
							AuxiliaryDocData auxillaryDocData2 = ((IBEditorPackage)Controller.DdexPackage).GetAuxiliaryDocData(Editor.DocData);
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
			else if (pguidCmdGroup == SqlGuidList.cmdSetGuidTeamSystemData && cmdID == EnCommandSet.MenuIdToplevelMenu)
			{
				prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
				num = 0;
			}
			*/
		}

		if (Core.Cmd.Failed(num))
		{
			num = base.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		}

		return num;
	}
}
