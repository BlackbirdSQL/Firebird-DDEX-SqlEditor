// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorSqlDatabaseCommand

using System;
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
		{
			// SelectedValue changed, probably by user from dropdown.

			if (!CanDisposeTransaction(Resources.ExChangeConnectionCaption))
				return VSConstants.S_OK;

			string selectedQualifiedName = (string)Marshal.GetObjectForNativeVariant(pvaIn);

			// Tracer.Trace(GetType(), "OnExec()", "pvaIn selectedDatasetKey: {0}", selectedDatasetKey);

			try
			{
				SetDatasetKeyDisplayMember(selectedQualifiedName);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				StoredQryMgr?.GetUpdateTransactionsStatus(true);
				return VSConstants.S_OK;
			}
		}
		else if (pvaOut != IntPtr.Zero)
		{
			// Check if underlying value changed. Drift detection.

			object objQualifiedName = string.Empty;
			IBsConnectionInfo connInfo = StoredQryMgr.Strategy.ConnInfo;

			if (connInfo == null /* || connection.State != ConnectionState.Open */ || string.IsNullOrEmpty(connInfo.Database))
			{
				// Tracer.Trace(GetType(), "OnExec()", "pvaOut Current selection is empty.");
				StoredCsa = null;
			}
			else
			{
				if (StoredCsa == null)
				{
					if (RctManager.ShutdownState)
					{
						StoredQryMgr?.GetUpdateTransactionsStatus(true);
						return VSConstants.S_OK;
					}

					// Tracer.Trace(GetType(), "OnExec()", "_Csa invalidated.");

					try
					{
						if (string.IsNullOrEmpty(connInfo?.Database))
						{
							// Tracer.Trace(GetType(), "OnExec()", "Live connection set to null Selection dead.");
							StoredCsa = null;
						}
						else
						{
							// Tracer.Trace(GetType(), "OnExec()", "Live connection okay. Creating new _Csa");

							StoredCsa = RctManager.CloneVolatile(connInfo);

							// Tracer.Trace(GetType(), "OnExec()", "Renewed Csb.DatasetKey: {0}.", _Csa == null ? "null" : _Csa.DatasetKey);
						}
					}
					catch (Exception ex)
					{
						Diag.Dug(ex);
						throw;
					}
				}

				// Tracer.Trace(GetType(), "OnExec()", "pvaOut Current selection DatasetKey: {0}.", _Csa == null ? "Csa is null" : _Csa.DatasetKey);

			}

			if (StoredCsa != null)
				objQualifiedName = StoredCsa.AdornedQualifiedName;

			Marshal.GetNativeVariantForObject(objQualifiedName, pvaOut);
		}

		StoredQryMgr?.GetUpdateTransactionsStatus(true);

		return VSConstants.S_OK;
	}

	private void SetDatasetKeyDisplayMember(string selectedQualifiedName)
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


		if (csa == null || csa.AdornedQualifiedName != selectedQualifiedName)
		{
			if (RctManager.ShutdownState)
				return;

			try
			{
				csa = RctManager.CloneRegistered(selectedQualifiedName, EnRctKeyType.AdornedQualifiedName);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}

		string connectionUrl = csa.SafeDatasetMoniker;

		if (string.IsNullOrWhiteSpace(connectionUrl))
		{
			ArgumentNullException ex = new("ConnectionUrl is null");
			Diag.Dug(ex);
			throw ex;
		}


		ConnectionStrategy strategy = StoredAuxDocData.QryMgr.Strategy;

		strategy.SetDatasetKeyOnConnection(selectedQualifiedName, csa);

		Guid clsid = VS.CLSID_PropDatabaseChanged;
		___(vsUserData.SetData(ref clsid, connectionUrl));

		// Tracer.Trace(GetType(), "SetDatasetKeyDisplayMember()", "csa.ConnectionString: {0}", csa.ConnectionString);

		Guid clsid2 = new(LibraryData.UserDataCsbGuid);
		___(vsUserData.SetData(ref clsid2, (object)csa));

		csa.RefreshDriftDetectionState();

		if (!ReferenceEquals(StoredCsa, csa))
		{
			StoredRctStamp = RctManager.Stamp;
			StoredCsa = csa;
		}
	}

}
