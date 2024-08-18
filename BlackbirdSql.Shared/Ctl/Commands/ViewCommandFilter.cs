// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorViewFilter

using System;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class ViewCommandFilter : AbstractViewCommandFilter
{

	public ViewCommandFilter(IBsTabbedEditorPane windowPane)
	{
		WindowPane = windowPane;
	}




	private IBsTabbedEditorPane WindowPane { get; set; }



	private AbstractCommand CreateCommand(IBsTabbedEditorPane window, EnCommandSet cmdId)
	{
		AbstractCommand command = null;

		switch (cmdId)
		{
			case EnCommandSet.CmdIdConnect:
				command = new CommandConnect(window);
				break;
			case EnCommandSet.CmdIdDisconnect:
				command = new CommandDisconnect(window);
				break;
			case EnCommandSet.CmdIdDisconnectAllQueries:
				command = new CommandDisconnectAllQueries(window);
				break;
			case EnCommandSet.CmdIdModifyConnection:
				command = new CommandModifyConnection(window);
				break;
			case EnCommandSet.CmdIdExecuteQuery:
				command = new CommandExecuteQuery(window);
				break;
			case EnCommandSet.CmdIdCancelQuery:
				command = new CommandCancelQuery(window);
				break;
			case EnCommandSet.CmdIdShowEstimatedPlan:
				command = new CommandShowEstimatedPlan(window);
				break;
			case EnCommandSet.CmdIdToggleClientStatistics:
				command = new CommandToggleClientStatistics(window);
				break;
			case EnCommandSet.CmdIdToggleExecutionPlan:
				command = new CommandToggleExecutionPlan(window);
				break;
			case EnCommandSet.CmdIdToggleIntellisense:
				command = new CommandToggleIntellisense(window);
				break;
			case EnCommandSet.CmdIdQuerySettings:
			case EnCommandSet.CmdIdQuerySettings2:
				command = new CommandQuerySettings(window);
				break;
			case EnCommandSet.CmdIdResultsAsText:
				command = new CommandResultsAsText(window);
				break;
			case EnCommandSet.CmdIdResultsAsGrid:
				command = new CommandResultsAsGrid(window);
				break;
			case EnCommandSet.CmdIdResultsAsFile:
				command = new CommandResultsAsFile(window);
				break;
			case EnCommandSet.CmdIdToggleResultsPane:
				command = new CommandToggleResultsPane(window);
				break;
			case EnCommandSet.CmbIdDatabaseSelect:
				command = new CommandDatabaseSelect(window);
				break;
			case EnCommandSet.CmbIdDatabaseList:
				command = new CommandDatabaseList(window);
				break;
			case EnCommandSet.CmdIdCloneQuery:
				command = new CommandCloneQueryWindow(window);
				break;
			case EnCommandSet.CmdIdNewQuery:
				command = new CommandNewQuery(window);
				break;
			case EnCommandSet.CmdIdTransactionCommit:
				command = new CommandTransactionCommit(window);
				break;
			case EnCommandSet.CmdIdTransactionRollback:
				command = new CommandTransactionRollback(window);
				break;
			case EnCommandSet.CmdIdToggleTTS:
				command = new CommandToggleTTS(window);
				break;
		}

		return command;

	}

	public override int Exec(ref Guid pguidCmdGroup, uint cmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{

		if (pguidCmdGroup == CommandProperties.ClsidCommandSet)
		{
			AbstractCommand command = CreateCommand(WindowPane, (EnCommandSet)cmdId);

			if (command != null)
				return command.Exec(nCmdexecopt, pvaIn, pvaOut);
		}

		return base.Exec(ref pguidCmdGroup, cmdId, nCmdexecopt, pvaIn, pvaOut);
	}




	public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		int hresult = (int)Constants.MSOCMDERR_E_NOTSUPPORTED;

		for (int i = 0; i < cCmds; i++)
		{
			AuxilliaryDocData auxDocData;
			EnCommandSet cmdId = (EnCommandSet)prgCmds[i].cmdID;

			if (pguidCmdGroup == CommandProperties.ClsidCommandSet)
			{
				AbstractCommand command = CreateCommand(WindowPane, cmdId);

				if (command != null)
					hresult = command.QueryStatus(ref prgCmds[i], pCmdText);
			}
			else if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
			{
				switch ((VSConstants.VSStd2KCmdID)cmdId)
				{
					case VSConstants.VSStd2KCmdID.SETBREAKPOINT:
						prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						hresult = VSConstants.S_OK;
						break;
					case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
					case VSConstants.VSStd2KCmdID.SURROUNDWITH:
						auxDocData = WindowPane.AuxDocData;

						if (auxDocData != null && auxDocData.QryMgr != null && !auxDocData.QryMgr.IsLocked)
						{
							prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
						}
						else
						{
							prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						}

						hresult = VSConstants.S_OK;
						break;
					case VSConstants.VSStd2KCmdID.FORMATSELECTION:
						prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						hresult = VSConstants.S_OK;
						break;
				}
			}
			else if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet97_guid)
			{
				switch ((VSConstants.VSStd97CmdID)cmdId)
				{
					case VSConstants.VSStd97CmdID.RunToCursor:
					case VSConstants.VSStd97CmdID.RunToCallstCursor:
					case VSConstants.VSStd97CmdID.InsertBreakpoint:
						prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						hresult = VSConstants.S_OK;
						break;
					case VSConstants.VSStd97CmdID.GotoDecl:
					case VSConstants.VSStd97CmdID.GotoRef:
						prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						hresult = VSConstants.S_OK;
						break;
					case VSConstants.VSStd97CmdID.GotoDefn:
					case VSConstants.VSStd97CmdID.FindReferences:
						prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						/*
						auxDocData = WindowsPane.AuxDocData;
						if (auxDocData != null && (auxDocData.StrategyFactory.IsOnline || auxDocData.StrategyFactory is ConnectionStrategyFactory))
						{
							prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
						}
						else
						{
							prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
						}
						*/
						hresult = VSConstants.S_OK;
						break;
					case VSConstants.VSStd97CmdID.ShowProperties:
						prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
						hresult = VSConstants.S_OK;
						break;
				}
			}
			else if (pguidCmdGroup == VS.ClsidVSDebugCommand)
			{
				prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
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
