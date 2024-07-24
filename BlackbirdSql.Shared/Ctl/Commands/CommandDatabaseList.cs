// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorSqlDatabaseListCommand

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandDatabaseList : AbstractCommand
{

	public CommandDatabaseList()
	{
		// Tracer.Trace();
	}

	public CommandDatabaseList(IBSqlEditorWindowPane windowPane) : base(windowPane)
	{
		// Tracer.Trace();
	}





	private string[] DatabaseList
	{
		get
		{
			if (RctManager.ShutdownState)
				return [];

			if (StoredDatabaseList == null || StoredRctStamp != RctManager.Stamp)
			{
				// Tracer.Trace(typeof(CommandDatabaseList), "get_DatabaseList", "Rebuilding list. Csb.DatasetKey: {0}.", _Csa == null ? "null" : _Csa.DatasetKey);

				string nodeDisplayMember = StoredCsa?.AdornedQualifiedName;

				List<string> list = [];

				bool hasSelectedMember = false;

				foreach (string qualifiedName in RctManager.AdornedQualifiedNames)
				{
					if (nodeDisplayMember != null && nodeDisplayMember.Equals(qualifiedName))
					{
						hasSelectedMember = true;
					}
					else
					{
						list.Add(qualifiedName);
					}
				}

				list.Sort(StringComparer.InvariantCulture);

				if (hasSelectedMember)
					list.Insert(0, nodeDisplayMember);




				StoredRctStamp = RctManager.Stamp;
				StoredDatabaseList = [.. list];
			}

			return StoredDatabaseList;
		}
	}



	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ExecutionLocked || RctManager.ShutdownState)
			return VSConstants.S_OK;


		IBsConnectionInfo connInfo = StoredQryMgr.Strategy.ConnInfo;

		if (StoredRctStamp != RctManager.Stamp || StoredCsa == null || connInfo == null
			|| string.IsNullOrEmpty(connInfo.Database))
		{
			StoredRctStamp = RctManager.Stamp;
		}

		if (StoredCsa == null && connInfo != null && !string.IsNullOrEmpty(connInfo.Database))
		{
			StoredCsa = RctManager.CloneVolatile(connInfo);
		}

		// qryMgrForEditor.Strategy.GetAvailableDatabases();
		if (DatabaseList.Length > 0)
			Marshal.GetNativeVariantForObject((object)StoredDatabaseList, pvaOut);

		return VSConstants.S_OK;
	}


}
