#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;

using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Model;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.VSHelp80;


namespace BlackbirdSql.Common.Ctl.Commands;

public class SqlEditorSqlDatabaseListCommand : AbstractSqlEditorCommand
{
	private static CsbAgent _Csa = null;
	private static string[] _DatabaseList = null;
	private static int _Id = -1;

	private static string[] DatabaseList
	{
		get
		{
			if (_DatabaseList == null || _Id != CsbAgent.Id)
			{
				int k = 1;
				int i = 1;
				string nodeDisplayMember = null;

				if (_Csa != null)
					nodeDisplayMember = _Csa.DatasetKey;

				foreach (KeyValuePair<string, string> pair in CsbAgent.RegisteredDatasets)
				{
					if (i > 0)
					{
						if (nodeDisplayMember.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
						{
							k--;
							i--;
						}
					}

					k++;
				}


				_DatabaseList = new string[k];

				if (i > 0)
					_DatabaseList[0] = _Csa.DatasetKey;

				foreach (KeyValuePair<string, string> pair in CsbAgent.RegisteredDatasets)
				{
					_DatabaseList[i] = pair.Key;
					i++;
				}

				_Id = CsbAgent.Id;
			}

			Array.Sort(_DatabaseList, StringComparer.InvariantCulture);

			return _DatabaseList;
		}
	}

	public SqlEditorSqlDatabaseListCommand()
	{
		// Diag.Trace();
	}

	public SqlEditorSqlDatabaseListCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
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
		QueryManager qryMgrForEditor = GetQueryManagerForEditor();
		if (qryMgrForEditor != null)
		{


			if (EditorWindow != null && (_Csa == null || _Id != CsbAgent.Id))
			{
				// Load csb derived from the ServerExplorer node if it is still available
				// in IVsUserData.
				// The DatabaseList accessor will include it in it's list if there is no duplicate.
				IVsTextView codeEditorTextView = EditorWindow.GetCodeEditorTextView();
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
								_Csa = (CsbAgent)objData;
						}
						catch (Exception) { }
					}
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
