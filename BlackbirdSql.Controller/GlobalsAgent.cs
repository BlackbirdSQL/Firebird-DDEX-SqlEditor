// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using BlackbirdSql.Controller.Ctl.Config;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Interfaces;

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
	#region Property Accessors - GlobalsAgent
	// =========================================================================================================


#if DEBUG
	public override string GlobalsKey => !PersistentValidation ? C_TransitoryKey : C_PersistentKey;
#else
	public override string GlobalsKey => C_PersistentKey;
#endif


#if DEBUG
	public override bool PersistentValidation => PersistentSettings.PersistentValidation;
#else
	public override bool PersistentValidation => true;
#endif


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not the app.config may be validated
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool ValidateConfig => PersistentSettings.ValidateConfig;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not edmx files may be validated
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool ValidateEdmx => PersistentSettings.ValidateEdmx;

	#endregion Property Accessors




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


}