// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//									VxbUIConnector Class
//
/// <summary>
/// Implementation of <see cref="Microsoft.VisualStudio.Data.Services.SupportEntities.IVsDataConnectionUIConnector"/> interface
/// </summary>
// =========================================================================================================
public class VxbUIConnector : DataConnectionUIConnector
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbUIConnector
	// ---------------------------------------------------------------------------------


	public VxbUIConnector() :base()
	{
		// Evs.Trace(typeof(VxbUIConnector), ".ctor");
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Implementations - VxbUIConnector
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens the data connection in the context of a connection UI (for example, the connection dialog box).
	/// </summary>
	/// <param name="connection">
	/// A data connection object representing the connection to the data source.
	/// </param>
	public override void Connect(IVsDataConnection connection)
	{
		// Evs.Trace(GetType(), nameof(Connect));

		if (connection == null)
		{
			ArgumentNullException ex = new ArgumentNullException("connection");
			Diag.Ex(ex);
			throw ex;
		}


		if (connection.State != DataConnectionState.Open)
		{
			try
			{
				// Evs.Debug(GetType(), "Connect", $"IVsDataConnection.Open: Type: {connection.GetType()}\nConnectionString: {connection}");
				connection.Open();
			}
			catch (Exception ex)
			{
				Diag.Expected(ex, $"\nConnectionString: {connection?.DecryptedConnectionString()}");
				throw;
			}
		}
	}


	#endregion Implementations

}
