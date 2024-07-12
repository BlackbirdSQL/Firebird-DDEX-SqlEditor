#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Model;
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



	private Csb _Csa = null;
	private long _Stamp = -1;
	private string[] _DatabaseList = null;

	private string[] DatabaseList
	{
		get
		{
			if (RctManager.ShutdownState)
				return [];

			if (_DatabaseList == null || _Stamp != RctManager.Stamp)
			{
				// Tracer.Trace(typeof(CommandDatabaseList), "get_DatabaseList", "Rebuilding list. Csb.DatasetKey: {0}.", _Csa == null ? "null" : _Csa.DatasetKey);

				string nodeDisplayMember = _Csa?.FullDisplayName;

				List<string> list = [];

				bool hasSelectedMember = false;

				foreach (string fullDisplayName in RctManager.RegisteredDatasets)
				{
					if (nodeDisplayMember != null && nodeDisplayMember.Equals(fullDisplayName))
					{
						hasSelectedMember = true;
					}
					else
					{
						list.Add(fullDisplayName);
					}
				}

				list.Sort(StringComparer.InvariantCulture);

				if (hasSelectedMember)
					list.Insert(0, nodeDisplayMember);




				_DatabaseList = [.. list];
				_Stamp = RctManager.Stamp;
			}

			return _DatabaseList;
		}
	}



	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (RctManager.ShutdownState)
			return VSConstants.S_OK;


		if (QryMgr == null)
		{
			ArgumentNullException ex = new("QryMgr is null");
			Diag.Dug(ex);
			throw ex;
		}

		IDbConnection connection = StoredQryMgr.Strategy.Connection;

		if (_Stamp != RctManager.Stamp || _Csa == null || connection == null
			|| string.IsNullOrEmpty(connection.Database) || _Csa.IsInvalidated)
		{
			_Csa = null;
			_DatabaseList = null;
		}

		if (_Csa == null && connection != null && !string.IsNullOrEmpty(connection.Database))
		{ 
			_Csa = RctManager.CloneVolatile(connection);
		}

		// qryMgrForEditor.Strategy.GetAvailableDatabases();
		if (DatabaseList.Length > 0)
			Marshal.GetNativeVariantForObject((object)_DatabaseList, pvaOut);

		return VSConstants.S_OK;
	}


}
