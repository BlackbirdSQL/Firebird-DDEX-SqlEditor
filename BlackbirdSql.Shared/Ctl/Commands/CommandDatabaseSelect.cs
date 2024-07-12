#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Enums;
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

	public CommandDatabaseSelect(IBSqlEditorWindowPane windowPane) : base(windowPane)
	{
		// Tracer.Trace();
	}



	/// <summary>
	/// Records the last moniker created so that we can do a fast equivalency comparison
	/// on the connection and use this static for the DatasetKey if they are equivalent.
	/// This avoids repeatedly creating a new Moniker and going through the
	/// registration process each time.
	/// </summary>
	private Csb _Csa = null;

	public Csb Csa => _Csa;



	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (AuxDocData == null)
		{
			Exception ex = new("AuxilliaryDocData NOT FOUND");
			Diag.Dug(ex);
			return VSConstants.S_OK;
		}

		if (StoredQryMgr == null)
		{
			ArgumentNullException ex = new("QryMgr is null");
			Diag.Dug(ex);
			return VSConstants.S_OK;
		}


		if (pvaIn != IntPtr.Zero)
		{
			// SelectedValue changed, probably by user from dropdown.

			if (!CanDisposeTransaction(ControlsResources.ErrChangeConnectionCaption))
				return VSConstants.S_OK;

			string selectedDisplayName = (string)Marshal.GetObjectForNativeVariant(pvaIn);

			// Tracer.Trace(GetType(), "HandleExec()", "pvaIn selectedDatasetKey: {0}", selectedDatasetKey);

			try
			{
				SetDatasetKeyDisplayMember(selectedDisplayName);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				QryMgr?.GetUpdateTransactionsStatus();
				return VSConstants.S_OK;
			}
		}
		else if (pvaOut != IntPtr.Zero)
		{
			// Check if underlying value changed. Drift detection.

			IDbConnection connection = StoredQryMgr.Strategy.Connection;
			object objDisplayName;

			if (connection == null /* || connection.State != ConnectionState.Open */ || string.IsNullOrEmpty(connection.Database))
			{
				// Tracer.Trace(GetType(), "HandleExec()", "pvaOut Current selection is empty.");
				objDisplayName = string.Empty;
				_Csa = null;
			}
			else
			{
				if (_Csa == null || _Csa.IsInvalidated)
				{
					if (RctManager.ShutdownState)
					{
						QryMgr?.GetUpdateTransactionsStatus();
						return VSConstants.S_OK;
					}

					// Tracer.Trace(GetType(), "HandleExec()", "Invalidated Csb.DatasetKey: {0}.", _Csa == null ? "null" : _Csa.DatasetKey);

					_Csa = RctManager.CloneVolatile(connection);

					// Tracer.Trace(GetType(), "HandleExec()", "Renewed Csb.DatasetKey: {0}.", _Csa == null ? "null" : _Csa.DatasetKey);
				}

				// Tracer.Trace(GetType(), "HandleExec()", "pvaOut Current selection DatasetKey: {0}.", _Csa == null ? "Csa is null" : _Csa.DatasetKey);

				objDisplayName = _Csa.FullDisplayName;
			}

			Marshal.GetNativeVariantForObject(objDisplayName, pvaOut);
		}

		QryMgr?.GetUpdateTransactionsStatus();

		return VSConstants.S_OK;
	}

	private void SetDatasetKeyDisplayMember(string selectedDisplayName)
	{
		IVsUserData vsUserData = StoredAuxDocData.VsUserData;

		if (vsUserData == null)
		{
			ArgumentNullException ex = new("IVsUserData is null");
			Diag.Dug(ex);
			throw ex;
		}

		RctManager.Invalidate();

		Csb csa = (Csb)StoredAuxDocData.UserDataCsb;

		if (csa != null && csa.DatasetKey == null)
		{
			// csa.RegisterDataset();
			Exception ex = new Exception("Csb from docData.GetUserDataCsb() has no DatasetKey.");
			Diag.Dug(ex);
			throw ex;
		}


		string connectionString;

		if (csa == null || csa.FullDisplayName != selectedDisplayName)
		{
			if (RctManager.ShutdownState)
				return;

			try
			{
				csa = RctManager.CloneRegistered(selectedDisplayName, EnRctKeyType.DisplayName);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}

		connectionString = csa.ConnectionString;

		if (string.IsNullOrWhiteSpace(connectionString))
		{
			ArgumentNullException ex = new("ConnectionString is null");
			Diag.Dug(ex);
			throw ex;
		}


		AbstractConnectionStrategy connectionStrategy = StoredAuxDocData.QryMgr.Strategy;

		connectionStrategy.SetDatasetKeyOnConnection(selectedDisplayName, csa);

		Guid clsid = VS.CLSID_PropDatabaseChanged;
		___(vsUserData.SetData(ref clsid, connectionString));

		// Tracer.Trace(GetType(), "SetDatasetKeyDisplayMember()", "csa.ConnectionString: {0}", csa.ConnectionString);

		Guid clsid2 = new(LibraryData.UserDataCsbGuid);
		___(vsUserData.SetData(ref clsid2, (object)csa));

		csa.RefreshDriftDetectionState();

		_Csa = csa;
	}

}
