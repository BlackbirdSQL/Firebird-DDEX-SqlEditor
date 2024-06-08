// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.Data;
using System.Diagnostics.CodeAnalysis;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Interfaces;
using EnvDTE80;



namespace BlackbirdSql.Core;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Using Diag.ThrowIfNotOnUIThread()")]



// =========================================================================================================
//										AbstractPackageController Class
//
/// <summary>
/// Manages package events and settings. This is the PackageController base class.
/// </summary>
/// <remarks>
/// Also updates the app.config for DbProvider and EntityFramework and updates existing .edmx models that
/// are using a legacy provider.
/// Also ensures we never do validations of a solution and project app.config and .edmx models twice.
/// Aslo performs cleanups of any sql editor documents that may be left dangling on solution close.
/// </remarks>
// =========================================================================================================
public abstract class AbstractPackageController : AbstrusePackageController
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractPackageController
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	protected AbstractPackageController(IBsAsyncPackage ddex) : base(ddex)
	{
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - AbstractPackageController
	// =========================================================================================================


	#endregion Fields




	// =========================================================================================================
	#region Property Accessors - AbstractPackageController
	// =========================================================================================================

	#endregion Property Accessors






	// =========================================================================================================
	#region Methods - AbstractPackageController
	// =========================================================================================================

	public override string CreateConnectionUrl(IDbConnection connection) => Csb.CreateConnectionUrl(connection);
	public override string CreateConnectionUrl(string connectionString) => Csb.CreateConnectionUrl(connectionString);

	public override string GetRegisterConnectionDatasetKey(IDbConnection connection)
	{
		Csb csa = RctManager.CloneRegistered(connection);
		return csa == null ? SysConstants.C_DefaultExDatasetKey : csa.DatasetKey;
	}

	public override void InvalidateRctManager() => RctManager.Invalidate();

	public override bool IsConnectionEquivalency(string connectionString1, string connectionString2)
		=> Csb.AreEquivalent(connectionString1, connectionString2, Csb.ConnectionKeys);
	public override bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2)
		=> Csb.AreEquivalent(connectionString1, connectionString2, Csb.WeakEquivalencyKeys);

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Disables solution event invocation
	/// </summary>
	/// <param name="disposing"></param>
	// ---------------------------------------------------------------------------------
	public override void UnadviseEvents(bool disposing)
	{
		// Diag.ThrowIfNotOnUIThread();

		_ = disposing;

		if (VsSolution != null && _HSolutionEvents != uint.MaxValue)
		{
			try
			{
				VsSolution.UnadviseSolutionEvents(_HSolutionEvents);
			}
			catch { }


			if (Dte != null)
			{
				try
				{
					Events2 events2 = Dte.Events as Events2;

					events2.ProjectItemsEvents.ItemAdded -= OnProjectItemAdded;
					events2.ProjectItemsEvents.ItemRemoved -= OnProjectItemRemoved;
					events2.ProjectItemsEvents.ItemRenamed -= OnProjectItemRenamed;
				}
				catch { }
			}
		}

		try
		{
			if (RdtManager.ServiceAvailable && _HDocTableEvents != uint.MaxValue)
				RdtManager.UnadviseRunningDocTableEvents(_HDocTableEvents);
		}
		catch { }

		try
		{
			if (_MonitorSelection != null && _HSelectionEvents != uint.MaxValue)
				_MonitorSelection.UnadviseSelectionEvents(_HSelectionEvents);
		}
		catch { }

		_HSolutionEvents = uint.MaxValue;
		_HDocTableEvents = uint.MaxValue;
		_MonitorSelection = null;
		_HSelectionEvents = uint.MaxValue;

	}


	#endregion Methods


}