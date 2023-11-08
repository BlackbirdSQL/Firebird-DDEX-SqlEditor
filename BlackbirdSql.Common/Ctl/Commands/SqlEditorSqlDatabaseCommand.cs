#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Runtime.InteropServices;

using BlackbirdSql.Core;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using FirebirdSql.Data.FirebirdClient;
using System.Data.Common;
using Microsoft.VisualStudio.LanguageServer.Client;
using System.IO;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Extensions;

namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorSqlDatabaseCommand : AbstractSqlEditorCommand
{
	public SqlEditorSqlDatabaseCommand()
	{
		// Diag.Trace();
	}

	public SqlEditorSqlDatabaseCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
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
			QueryManager qryMgr = auxiliaryDocDataForEditor.QryMgr;
			if (qryMgr != null)
			{
				if (pvaIn != IntPtr.Zero)
				{
					string selectedDisplayMember = (string)Marshal.GetObjectForNativeVariant(pvaIn);
					SetDatasetDisplayMember(auxiliaryDocDataForEditor, selectedDisplayMember);
				}
				else if (pvaOut != IntPtr.Zero)
				{
					// Diag.Trace("SqlEditorSqlDatabaseCommand:HandleExec requesting current selection");
					IDbConnection connection = qryMgr.ConnectionStrategy.Connection;
					object empty;
					if (connection == null /* || connection.State != ConnectionState.Open */ || string.IsNullOrEmpty(connection.Database))
					{
						empty = string.Empty;
					}
					else
					{
						MonikerAgent moniker = new(connection);
						empty = moniker.DatasetKey;
					}
					Marshal.GetNativeVariantForObject(empty, pvaOut);
				}
			}
			else
			{
				ArgumentNullException ex = new("QryMgr is null");
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

	private void SetDatasetDisplayMember(AuxiliaryDocData docData, string selectedDisplayMember)
	{
		DbConnectionStringBuilder csb = docData.GetUserDataCsb();

		IVsUserData userData = docData.GetIVsUserData();
		if (userData != null && (csb == null || (string)csb["DisplayMember"] != selectedDisplayMember))
		{
			try
			{
				csb = XmlParser.GetCsbFromDatabases(selectedDisplayMember);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}


		AbstractConnectionStrategy connectionStrategy = docData.QryMgr.ConnectionStrategy;
		connectionStrategy.SetDatasetDisplayMemberOnConnection(selectedDisplayMember, csb);
		// IDbConnection connection = connectionStrategy.Connection;

		// if (connection != null && connection.State == ConnectionState.Open)
		// {
		if (userData != null)
		{
			Guid clsid = LibraryData.CLSID_PropertyDatabaseChanged;
			Core.Native.ThrowOnFailure(userData.SetData(ref clsid, selectedDisplayMember), (string)null);
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
