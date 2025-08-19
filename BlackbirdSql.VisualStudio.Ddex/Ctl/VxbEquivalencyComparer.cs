// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//									VxbEquivalencyComparer Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionEquivalencyComparer"/> interface
/// </summary>
// =========================================================================================================
public class VxbEquivalencyComparer : DataConnectionEquivalencyComparer
{

	// ---------------------------------------------------------------------------------
	#region Constructors
	// ---------------------------------------------------------------------------------


	public VxbEquivalencyComparer() : base()
	{
		// Evs.Trace(typeof(VxbEquivalencyComparer), ".ctor");
	}


	#endregion Constructors




	// =========================================================================================================
	#region Methods and Implementations - VxbEquivalencyComparer
	// =========================================================================================================





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not connection property/parameter objects are equivalent
	/// </summary>
	/// <param name="connectionProperties1"></param>
	/// <param name="connectionProperties2"></param>
	/// <returns>true if equivalent else false</returns>
	/// <remarks>
	/// We consider connections equivalent if they will produce the same results. The connection properties
	/// that determine this equivalency are defined in <see cref="CoreProperties.EquivalencyKeys"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected override bool AreEquivalent(IVsDataConnectionProperties connectionProperties1, IVsDataConnectionProperties connectionProperties2)
	{
		// Evs.Trace(GetType(), nameof(AreEquivalent));

		// The only interception we can make when a new query lists tables or views is when the
		// Microsoft.VisualStudio.Data.Package.DataConnectionManager checks if the connection it requires
		// is equivalent to this connection. We don't have access to the DataStore so to avoid a complete rewrite
		// we're going to hack it by invalidating the connection.
		// 

		if (CommandProperties.CommandNodeSystemType != EnNodeSystemType.Undefined)
		{
			// Evs.Trace(GetType(), "VxbEquivalencyComparer.AreEquivalent", "Returning false. CommandProperties.CommandNodeSystemType: {0}", CommandProperties.CommandNodeSystemType);
			return false;
		}

		Csb csa1 = new(connectionProperties1.ToString(), false);
		Csb csa2 = new(connectionProperties2.ToString(), false);

		return Csb.AreEquivalent(csa1, csa2);

	}


	#endregion Methods and Implementations





	// =========================================================================================================
	#region Methods - VxbEquivalencyComparer
	// =========================================================================================================



	#endregion Methods

}

