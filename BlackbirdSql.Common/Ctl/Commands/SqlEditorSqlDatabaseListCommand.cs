#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using System.Data;
using BlackbirdSql.Core.Extensions;
using System.Windows.Documents;
using BlackbirdSql.Common.Model;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Data.Common;

namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorSqlDatabaseListCommand : AbstractSqlEditorCommand
{
	private static DbConnectionStringBuilder _Csb = null;
	private static string[] _DatabaseList = null;

	private static string[] DatabaseList
	{
		get
		{
			if (_DatabaseList == null)
			{
				DataTable databases = XmlParser.Databases;

				int k = 1;
				int i = 1;
				string nodeDatasetKey = null;

				if (_Csb != null)
					nodeDatasetKey = (string)_Csb["DatasetKey"];

				foreach (DataRow row in databases.Rows)
				{
					if ((string)row["Name"] == "")
						continue;

					if (i > 0)
					{
						if (nodeDatasetKey.Equals((string)row["DatasetKey"], StringComparison.OrdinalIgnoreCase))
						{
							k--;
							i--;
						}
					}

					k++;
				}


				_DatabaseList = new string[k];

				if (i > 0)
				{
					_DatabaseList[0] = (string)_Csb["DatasetKey"];
				}

				foreach (DataRow row in databases.Rows)
				{
					if ((string)row["Name"] == "")
						continue;

					_DatabaseList[i]  = (string)row["DatasetKey"];
					i++;
				}
			}
			return _DatabaseList;
		}
	}

	public SqlEditorSqlDatabaseListCommand()
	{
		// Diag.Trace();
	}

	public SqlEditorSqlDatabaseListCommand(ISqlEditorWindowPane editor)
		: base(editor)
	{
		// Diag.Trace();
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		QueryExecutor queryExecutorForEditor = GetQueryExecutorForEditor();
		if (queryExecutorForEditor != null)
		{


			if (Editor != null && _Csb == null)
			{
				// Load csb derived from the ServerExplorer node if it is still available
				// in IVsUserData.
				// The DatabaseList accessor will include it in it's list if there is no duplicate.
				IVsTextView codeEditorTextView = Editor.GetCodeEditorTextView();
				if (codeEditorTextView != null)
				{
					IVsUserData userData = (IVsUserData)GetTextLinesForTextView(codeEditorTextView);
					if (userData != null)
					{
						Guid clsid = new(LibraryData.SqlEditorConnectionStringGuid);
						try
						{
							userData.GetData(ref clsid, out object objData);

							if (objData != null)
								_Csb = (DbConnectionStringBuilder)objData;
						}
						catch (Exception) { }
					}
				}
			}

			// queryExecutorForEditor.ConnectionStrategy.GetAvailableDatabases();
			if (DatabaseList.Length > 0)
			{
				Marshal.GetNativeVariantForObject((object)_DatabaseList, pvaOut);
			}

		}
		else
		{
			ArgumentNullException ex = new("QueryExecutor is null");
			Diag.Dug(ex);
			throw ex;
		}



		return VSConstants.S_OK;
	}
}
