// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorSqlDatabaseListCommand

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandDatabaseList : AbstractCommand
{

	public CommandDatabaseList()
	{
		// Evs.Trace();
	}

	public CommandDatabaseList(IBsTabbedEditorPane tabbedEditor) : base(tabbedEditor)
	{
		// Evs.Trace();
	}





	private string[] DatabaseList
	{
		get
		{
			if (RctManager.ShutdownState)
				return [];

			if (StoredDatabaseList == null || StoredRctStamp != RctManager.Stamp)
			{
				// Evs.Trace(typeof(CommandDatabaseList), "get_DatabaseList", "Rebuilding list. Csb.DatasetKey: {0}.", _Csa == null ? "null" : _Csa.DatasetKey);

				string nodeDisplayMember = CachedLiveQualifiedName;

				List<string> list = [];

				bool hasSelectedMember = false;

				foreach (string qualifiedName in RctManager.AdornedQualifiedTitles)
				{
					if (nodeDisplayMember?.Equals(qualifiedName) ?? false)
						hasSelectedMember = true;
					else
						list.Add(qualifiedName);
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


		if (DatabaseList.Length > 0)
			Marshal.GetNativeVariantForObject((object)StoredDatabaseList, pvaOut);

		return VSConstants.S_OK;
	}


}
