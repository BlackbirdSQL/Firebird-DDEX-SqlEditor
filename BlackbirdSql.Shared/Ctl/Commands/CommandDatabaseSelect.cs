// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorSqlDatabaseCommand

using System;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandDatabaseSelect : AbstractCommand
{

	public CommandDatabaseSelect()
	{
		// Tracer.Trace();
	}

	public CommandDatabaseSelect(IBsTabbedEditorWindowPane windowPane) : base(windowPane)
	{
		// Tracer.Trace();
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
		if (ExecutionLocked)
			return VSConstants.S_OK;


		if (pvaIn != IntPtr.Zero)
			return OnExecSet(pvaIn);

		if (pvaOut != IntPtr.Zero)
			return OnExecGet(pvaOut);

		return VSConstants.S_OK;
	}


	private int OnExecGet(IntPtr pvaOut)
	{
		object objQualifiedName = StoredLiveMdlCsb?.AdornedQualifiedTitle ?? string.Empty;

		StoredSelectedName = (string)objQualifiedName;

		Marshal.GetNativeVariantForObject(objQualifiedName, pvaOut);

		return VSConstants.S_OK;
	}



	private int OnExecSet(IntPtr pvaIn)
	{
		// SelectedValue changed, probably by user from dropdown.

		if (ExecutionLocked || !RequestDeactivateQuery(Resources.MsgQueryAbort_UncommittedTransactions))
			return VSConstants.S_OK;

		string selectedQualifiedName = (string)Marshal.GetObjectForNativeVariant(pvaIn);

		try
		{
			SetDatasetKeyDisplayMember(selectedQualifiedName);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return VSConstants.S_OK;
		}

		StoredSelectedName = selectedQualifiedName;

		return VSConstants.S_OK;
	}


	private bool SetDatasetKeyDisplayMember(string selectedQualifiedName)
	{
		IBsModelCsb mdlCsb = StoredMdlCsb;

		if (mdlCsb == null || mdlCsb.IsInvalidated || mdlCsb.AdornedQualifiedTitle != selectedQualifiedName)
		{
			if (!(StoredStrategy?.SetDatasetKeyOnConnection(selectedQualifiedName, mdlCsb) ?? false))
				return false;
		}

		string moniker = StoredMdlCsb.Moniker;

		IVsUserData vsUserData = StoredAuxDocData.VsUserData;
		Diag.ThrowIfInstanceNull(vsUserData, typeof(IVsUserData));

		Guid clsid = VS.CLSID_PropDatabaseChanged;
		___(vsUserData.SetData(ref clsid, moniker));

		// Tracer.Trace(GetType(), "SetDatasetKeyDisplayMember()", "csa.ConnectionString: {0}", csa.ConnectionString);

		StoredRctStamp = RctManager.Stamp;

		return true;
	}

}
