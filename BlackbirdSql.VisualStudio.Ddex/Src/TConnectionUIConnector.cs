using System;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;


using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


internal class TConnectionUIConnector : DataConnectionUIConnector
{
	//
	// Summary:
	//     Initializes a new instance of the TConnectionUIConnector
	//     class.
	public TConnectionUIConnector() :base()
	{
		Diag.Trace();
	}


	//
	// Summary:
	//     Opens the data connection in the context of a connection UI (for example, the
	//     data connection dialog box).
	//
	// Parameters:
	//   connection:
	//     A data connection object representing the connection to the data source.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     The connection parameter is null.
	public override void Connect(IVsDataConnection connection)
	{
		Diag.Trace();

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

}
