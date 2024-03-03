#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Common.Ctl.Commands;

public class SqlEditorDatabaseListCommand : AbstractSqlEditorCommand
{
	private static CsbAgent _Csa = null;
	private static long _Seed = -1;
	private static string[] _DatabaseList = null;

	private static string[] DatabaseList
	{
		get
		{
			if (RctManager.ShutdownState)
				return new string[0];

			if (_DatabaseList == null || _Seed != RctManager.Seed)
			{
				string nodeDisplayMember = _Csa?.DatasetKey;

				List<string> list = [];

				bool hasSelectedMember = false;

				foreach (string dataset in RctManager.RegisteredDatasets)
				{
					if (nodeDisplayMember != null && nodeDisplayMember.Equals(dataset, StringComparison.OrdinalIgnoreCase))
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
				_Seed = RctManager.Seed;
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
		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (RctManager.ShutdownState)
			return VSConstants.S_OK;

		QueryManager qryMgr = GetQueryManagerForEditor();

		if (qryMgr != null)
		{


			if (EditorWindow != null && (_Csa == null || _Seed != RctManager.Seed))
			{
				_Csa = null;
				_DatabaseList = null;
			}

			if (_Csa == null)
			{
				IDbConnection connection = qryMgr.ConnectionStrategy.Connection;

				if (connection != null && !string.IsNullOrEmpty(connection.Database))
				{
					_Csa = RctManager.CloneVolatile(connection);
				}
			}

			// qryMgrForEditor.ConnectionStrategy.GetAvailableDatabases();
			if (DatabaseList.Length > 0)
			{
				Marshal.GetNativeVariantForObject((object)_DatabaseList, pvaOut);
			}

		}
		else
		{
			ArgumentNullException ex = new("QryMgr is null");
			Diag.Dug(ex);
			throw ex;
		}



		return VSConstants.S_OK;
	}


}
