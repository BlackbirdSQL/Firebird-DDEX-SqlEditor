using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Enums;

using static BlackbirdSql.Common.Model.TelemetryConstants;




namespace BlackbirdSql.Common.Model;


// =========================================================================================================
//										TelemetryPropertySet Class
//
/// <summary>
/// Static Telemetry PropertySet for use by the base class AbstractTelemetryPropertyAgent (derived from
/// the universal connection base class AbstractPropertyAgent).
/// The static constants base class for use by connection classes derived from AbstractPropertyAgent.
/// Refer to the <seealso cref="PropertySet"/> class for more information.
/// </summary>
// =========================================================================================================
public abstract class TelemetryPropertySet : CorePropertySet
{

	#region Constants


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// FbStringBuilder Descriptor/Parameter Properties.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly new Describer[] Describers =
	[
		new Describer(C_KeyCountConnections, typeof(int), 0),
		new Describer(C_KeyEngineProduct, typeof(string), null),
		new Describer(C_KeyEngineType, typeof(EnEngineType), EnEngineType.Unknown),
		new Describer(C_KeyTabOpen, typeof(string), null),
		new Describer(C_KeyHistoryTab, typeof(int), 0),
		new Describer(C_KeyBrowseTab, typeof(int), 0),
		new Describer(C_KeyCountConnectionErrors, typeof(int), 0),
		new Describer(C_KeyConnectionSuccess, typeof(int), 0),
		new Describer(C_KeyCountRecentConnections, typeof(int), 0),
		new Describer(C_KeyCountFavoriteConnections, typeof(int), 0),
		new Describer(C_KeyIsLocalDb, typeof(bool), 0)
	];


	#endregion





	// =========================================================================================================
	#region Static Methods
	// =========================================================================================================



	#endregion Static Methods


}
