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


public class SqlEditorDatabaseListCommand : AbstractSqlEditorCommand
{
	private static Csb _Csa = null;
	private static long _Stamp = -1;
	private static string[] _DatabaseList = null;

	private static string[] DatabaseList
	{
		get
		{
			if (RctManager.ShutdownState)
				return [];

			if (_DatabaseList == null || _Stamp != RctManager.Stamp)
			{
				// Tracer.Trace(typeof(SqlEditorDatabaseListCommand), "get_DatabaseList", "Rebuilding list. Csb.DatasetKey: {0}.", _Csa == null ? "null" : _Csa.DatasetKey);

				string nodeDisplayMember = _Csa?.DatasetKey;

				List<string> list = [];

				bool hasSelectedMember = false;

				foreach (string dataset in RctManager.RegisteredDatasets)
				{
					if (nodeDisplayMember != null && nodeDisplayMember.Equals(dataset))
					{
						hasSelectedMember = true;
					}
					else
					{
						list.Add(dataset);
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

	public SqlEditorDatabaseListCommand()
	{
		// Tracer.Trace();
	}

	public SqlEditorDatabaseListCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
		// Tracer.Trace();
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

		IDbConnection connection = StoredQryMgr.ConnectionStrategy.Connection;

		if (_Stamp != RctManager.Stamp || _Csa == null || connection == null
			|| string.IsNullOrEmpty(connection.Database) || _Csa.IsInvalidated(connection))
		{
			_Csa = null;
			_DatabaseList = null;
		}

		if (_Csa == null && connection != null && !string.IsNullOrEmpty(connection.Database))
		{ 
			_Csa = RctManager.CloneVolatile(connection);

			// Tracer.Trace(GetType(), "HandleExec()", "Renewing Csb.DatasetKey: {0}.", _Csa == null ? "null" : _Csa.DatasetKey);
		}

		// qryMgrForEditor.ConnectionStrategy.GetAvailableDatabases();
		if (DatabaseList.Length > 0)
			Marshal.GetNativeVariantForObject((object)_DatabaseList, pvaOut);

		return VSConstants.S_OK;
	}


}
