// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//									TConnectionEquivalencyComparer Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionEquivalencyComparer"/> interface
/// </summary>
// =========================================================================================================
public class TConnectionEquivalencyComparer : DataConnectionEquivalencyComparer
{

	// ---------------------------------------------------------------------------------
	#region Constructors
	// ---------------------------------------------------------------------------------


	public TConnectionEquivalencyComparer() : base()
	{
		Tracer.Trace(GetType(), "TConnectionEquivalencyComparer.TConnectionEquivalencyComparer");
	}


	#endregion Constructors




	// =========================================================================================================
	#region Methods and Implementations - TConnectionEquivalencyComparer
	// =========================================================================================================





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not connection property objects are equivalent
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
		Tracer.Trace(GetType(), "TConnectionEquivalencyComparer.AreEquivalent");

		// The only interception we can make when a new query lists tables or views is when the
		// Microsoft.VisualStudio.Data.Package.DataConnectionManager checks if the connection it requires
		// is equivalent to this connection. We don't have access to the DataStore so to avoid a complete rewrite
		// we're going to hack it by invalidating the connection.
		// 

		if (CommandProperties.CommandNodeSystemType != CommandProperties.EnNodeSystemType.None)
		{
			Tracer.Trace(GetType(), "TConnectionEquivalencyComparer.AreEquivalent", "Returning false. CommandProperties.CommandNodeSystemType: {0}", CommandProperties.CommandNodeSystemType);
			return false;
		}

		FbConnectionStringBuilder csb1 = new(connectionProperties1.ToString());
		FbConnectionStringBuilder csb2 = new(connectionProperties2.ToString());

		return ModelPropertySet.AreEquivalent(csb1, csb2);

	}


	#endregion Methods and Implementations





	// =========================================================================================================
	#region Methods - TConnectionEquivalencyComparer
	// =========================================================================================================





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Standardizes the DataSource (Server hostname) [Deprecated]
	/// </summary>
	/// <param name="dataSource"></param>
	/// <returns>The standardized hostname</returns>
	// ---------------------------------------------------------------------------------
	protected static string StandardizeDataSource(string dataSource)
	{
		dataSource = dataSource.ToUpperInvariant();
		string[] array = new string[2] { ".", "localhost" };
		foreach (string text in array)
		{
			if (dataSource.Equals(text, StringComparison.Ordinal))
			{
				dataSource = Environment.MachineName.ToUpperInvariant(); // + dataSource[text.Length..];
				break;
			}
		}

		return dataSource;
	}


	#endregion Methods

}

