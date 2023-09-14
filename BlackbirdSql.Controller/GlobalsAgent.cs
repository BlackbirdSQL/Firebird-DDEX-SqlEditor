// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl;

namespace BlackbirdSql.Controller;


// =========================================================================================================
//											GlobalsAgent Class
//
/// <summary>
/// Manages Globals and propagates Visual Studio Options events
/// </summary>
// =========================================================================================================
internal class GlobalsAgent : AbstractGlobalsAgent
{


	// =========================================================================================================
	#region Constructors / Destructors - GlobalsAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// GlobalsAgent .ctor
	/// </summary>
	/// <param name="dte"></param>
	// ---------------------------------------------------------------------------------
	protected GlobalsAgent() : base()
	{
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the Singleton GlobalsAgent instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static new IBGlobalsAgent Instance
	{
		get
		{
			return _Instance ??= CreateInstance();
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and/or Gets the Singleton GlobalsAgent instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBGlobalsAgent CreateInstance()
	{
		return new GlobalsAgent();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Event handlers - GlobalsAgent
	// =========================================================================================================


	/// <summary>
	/// Propogates option settings globally across the package and it's services.
	/// This is the centralized method that is customized with any changes to the
	/// option models.
	/// </summary>
	/// <param name="e"></param>
	public override void UpdatePackageGlobals(GlobalEventArgs e)
	{
		if (e.Group == "General" || e.Group == null)
		{
			Diag.EnableDiagnostics = (bool)e["EnableDiagnostics"];
			Diag.EnableTaskLog = (bool)e["EnableTaskLog"];

			_ValidateConfig = (bool)e["ValidateConfig"];
			_ValidateEdmx = (bool)e["ValidateEdmx"];
		}

		if (e.Group == "Debug" || e.Group == null)
		{
			_PersistentValidation = (bool)e["PersistentValidation"];

			Diag.EnableTrace = (bool)e["EnableTrace"];
			Diag.EnableTracer = (bool)e["EnableTracer"];

			Diag.EnableDiagnosticsLog = (bool)e["EnableDiagnosticsLog"];
			Diag.LogFile = (string)e["LogFile"];

			Diag.EnableFbDiagnostics = (bool)e["EnableFbDiagnostics"];
			Diag.FbLogFile = (string)e["FbLogFile"];

#if DEBUG
			G_Persistent = _PersistentValidation;

			if (!_PersistentValidation)
				G_Key = C_TransitoryKey;
			else
				G_Key = C_PersistentKey;
#else
			G_Persistent = true;
			G_Key = G_PersistentKey;
#endif
		}

	}


	#endregion Event handlers

}