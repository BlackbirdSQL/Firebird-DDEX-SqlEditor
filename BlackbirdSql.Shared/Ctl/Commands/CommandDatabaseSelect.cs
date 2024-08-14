// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorSqlDatabaseCommand

using System;
using System.Runtime.InteropServices;
using BlackbirdSql.Shared.Interfaces;
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

	public CommandDatabaseSelect(IBsTabbedEditorPane editorPane) : base(editorPane)
	{
		// Tracer.Trace();
	}




	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked && CachedStrategy != null)
		{
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		int result = VSConstants.S_OK;

		if (pvaIn != IntPtr.Zero)
			result = OnExecSet(pvaIn);
		else if (pvaOut != IntPtr.Zero)
			result = OnExecGet(pvaOut);

		return result;
	}


	private int OnExecGet(IntPtr pvaOut)
	{
		string qualifiedName = StoredSelectedName;

		Marshal.GetNativeVariantForObject(qualifiedName, pvaOut);

		return VSConstants.S_OK;
	}



	private int OnExecSet(IntPtr pvaIn)
	{
		// SelectedValue changed, probably by user from dropdown.

		if (ExecutionLocked || !RequestDisposeTts(Resources.ExChangeConnectionCaption))
			return VSConstants.S_OK;


		string selectedQualifiedName = (string)Marshal.GetObjectForNativeVariant(pvaIn);


		if (CachedLiveQualifiedName != selectedQualifiedName)
		{
			bool result;

			result = CachedQryMgr?.SetDatasetKeyOnConnection(selectedQualifiedName) ?? false;

			if (!result)
				return VSConstants.S_OK;
		}

		StoredSelectedName = selectedQualifiedName;


		string moniker = CachedMdlCsb.Moniker;

		IVsUserData vsUserData = CachedAuxDocData.VsUserData;
		Diag.ThrowIfInstanceNull(vsUserData, typeof(IVsUserData));

		Guid clsid = VS.CLSID_PropDatabaseChanged;
		___(vsUserData.SetData(ref clsid, moniker));

		return VSConstants.S_OK;
	}


}
