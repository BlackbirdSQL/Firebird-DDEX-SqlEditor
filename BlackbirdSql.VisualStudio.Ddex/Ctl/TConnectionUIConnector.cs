// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//									TConnectionUIConnector Class
//
/// <summary>
/// Implementation of <see cref="Microsoft.VisualStudio.Data.Services.SupportEntities.IVsDataConnectionUIConnector"/> interface
/// </summary>
// =========================================================================================================
public class TConnectionUIConnector : DataConnectionUIConnector
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TConnectionUIConnector
	// ---------------------------------------------------------------------------------


	public TConnectionUIConnector() :base()
	{
		// Tracer.Trace(GetType(), "TConnectionUIConnector.TConnectionUIConnector()");
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Implementations - TConnectionUIConnector
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
		// Tracer.Trace(GetType(), "Connect()");

		if (connection == null)
		{
			ArgumentNullException ex = new ArgumentNullException("connection");
			Diag.Dug(ex);
			throw ex;
		}

		if (connection.State != DataConnectionState.Open)
		{
			connection.Open();
		}
	}


	#endregion Implementations

}
