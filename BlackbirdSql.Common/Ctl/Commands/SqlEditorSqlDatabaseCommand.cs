#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Runtime.InteropServices;

using BlackbirdSql.Core;
using BlackbirdSql.Common;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using FirebirdSql.Data.FirebirdClient;
using System.Data.Common;
using Microsoft.VisualStudio.LanguageServer.Client;
using System.IO;
using BlackbirdSql.Core.Extensions;

namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorSqlDatabaseCommand : AbstractSqlEditorCommand
{
	public SqlEditorSqlDatabaseCommand()
	{
		// Diag.Trace();
	}

	public SqlEditorSqlDatabaseCommand(ISqlEditorWindowPane editor)
		: base(editor)
	{
		// Diag.Trace();
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		if (!IsEditorExecutingOrDebugging())
		{
			// Diag.Trace("SqlEditorSqlDatabaseCommand:HandleQueryStatus enabled");
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
		if (auxiliaryDocDataForEditor != null)
		{
			QueryExecutor queryExecutor = auxiliaryDocDataForEditor.QueryExecutor;
			if (queryExecutor != null)
			{
				if (pvaIn != IntPtr.Zero)
				{
					string selectedDatasetKey = (string)Marshal.GetObjectForNativeVariant(pvaIn);
					SetDatasetKey(auxiliaryDocDataForEditor, selectedDatasetKey);
				}
				else if (pvaOut != IntPtr.Zero)
				{
					// Diag.Trace("SqlEditorSqlDatabaseCommand:HandleExec requesting current selection");
					IDbConnection connection = queryExecutor.ConnectionStrategy.Connection;
					object empty;
					if (connection == null /* || connection.State != ConnectionState.Open */ || string.IsNullOrEmpty(connection.Database))
					{
						empty = string.Empty;
					}
					else
					{
						FbConnectionStringBuilder csb = new()
						{
							ConnectionString = connection.ConnectionString
						};
						empty = $"{csb.DataSource} ({Path.GetFileNameWithoutExtension(csb.Database)})";
					}
					Marshal.GetNativeVariantForObject(empty, pvaOut);
				}
			}
			else
			{
				ArgumentNullException ex = new("QueryExecutor is null");
				Diag.Dug(ex);
				throw ex;
			}
		}
		else
		{
			Exception ex = new("AuxiliaryDocData NOT FOUND");
			Diag.Dug(ex);
			throw ex;
		}


		return VSConstants.S_OK;
	}

	private void SetDatasetKey(AuxiliaryDocData docData, string selectedDatasetKey)
	{
		DbConnectionStringBuilder csb = docData.GetUserDataCsb();

		IVsUserData userData = docData.GetIVsUserData();
		if (userData != null && (csb == null || (string)csb["DatasetKey"] != selectedDatasetKey))
		{
			try
			{
				csb = XmlParser.GetCsbFromDatabases(selectedDatasetKey);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}


		ConnectionStrategy connectionStrategy = docData.QueryExecutor.ConnectionStrategy;
		connectionStrategy.SetDatasetKeyOnConnection(selectedDatasetKey, csb);
		// IDbConnection connection = connectionStrategy.Connection;

		// if (connection != null && connection.State == ConnectionState.Open)
		// {
		if (userData != null)
		{
			Guid clsid = LibraryData.CLSID_PropertyDatabaseChanged;
			Native.ThrowOnFailure(userData.SetData(ref clsid, selectedDatasetKey), (string)null);
		}
		else
		{
			ArgumentNullException ex = new("IVsUserData is null");
			Diag.Dug(ex);
			throw ex;
		}
		// }
	}
}
