// Microsoft.SqlServer.RegSvrEnum, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.Smo.RegSvrEnum.UIConnectionInfo

using System;
using System.ComponentModel;
using System.Data.Common;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Sys.Ctl;

using static BlackbirdSql.SharedConstants;
using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.Shared.Model;


// =========================================================================================================
//
//											ModelCsb Class
//
/// <summary>
/// Csb class for supporting the SqlEditor query model.
/// </summary>
// =========================================================================================================
public class ModelCsb : ConnectionCsb, IBsModelCsb
{

	// ------------------------------------------
	#region Constructors / Destructors - ModelCsb
	// ------------------------------------------


	public ModelCsb(string connectionString) : base(connectionString)
	{
	}

	public ModelCsb(IBsCsb lhs) : base(lhs)
	{
	}



	public ModelCsb() : base()
	{
	}



	static ModelCsb()
	{
		// Extension specific describers.

		Describers.AddRange(
		[
			new Describer(C_KeyExCreationFlags, typeof(EnCreationFlags), C_DefaultExCreationFlags, D_Extended)
		]);

		// Diag.DebugTrace("Added model describers");
	}



	public override void Dispose()
	{
		base.Dispose();
	}

	protected override bool Dispose(bool disposing)
	{
		if (!base.Dispose(disposing))
			return false;

		return true;
	}



	public override IBsCsb Copy()
	{
		return new ModelCsb(this);
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - ModelCsb
	// =========================================================================================================


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - ModelCsb
	// =========================================================================================================


	[Browsable(false)]
	public EnCreationFlags CreationFlags
	{
		get { return (EnCreationFlags)GetValue(C_KeyExCreationFlags); }
		set { SetValue(C_KeyExCreationFlags, value); }
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the connection using the latest available properties. If it's properties
	/// are outdated, closes the connection and applies the latest properties without
	/// reopening. Returns null if no connection exists. If a Close() fails, disposes of
	/// the connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
	[Browsable(false)]
	public DbConnection LiveConnection
	{
		get
		{
			lock (_LockObject)
			{
				DbConnection connection = DataConnection;

				if (connection == null)
					return null;

				// We have to ensure the connection hasn't changed.

				if (!RctManager.Loaded || !IsInvalidated)
					return connection;

				// Get the connection string of the current connection adorned with the additional Csa properties
				// so that we don't get a negative equivalency because of missing stripped Csa properties in the
				// connection's connection string.
				string connectionString = RctManager.AdornConnectionStringFromRegistration(connection);

				if (connectionString == null)
					return connection;

				Csb csaCurrent = new(connectionString, false);
				Csb csaRegistered = RctManager.CloneRegistered(this);

				// Compare the current connection with the registered connection.
				if (Csb.AreEquivalent(csaRegistered, csaCurrent, Csb.DescriberKeys))
				{
					// Nothing's changed.
					return connection;
				}

				// Tracer.Trace(GetType(), "get_Connection()", "Connections are not equivalent: \nCurrent: {0}\nRegistered: {1}",
				//	csaCurrent.ConnectionString, csaRegistered.ConnectionString);

				if (Csb.AreEquivalent(csaRegistered, csaCurrent, Csb.EquivalencyKeys))
				{
					// The connection is the same but it's adornments have changed.
					lock (_LockObject)
					{
						ConnectionString = csaRegistered.ConnectionString;

						RaisePropertyChanged(null);
						RefreshDataConnection();

						return connection;
					}
				}

				// If we're here it's a reset.
				// Tracer.Trace(GetType(), "get_LiveConnection()", "The connection was reset because it is is no longer equivalent.");

				return null;
			}
		}
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - ModelCsb
	// =========================================================================================================


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - ModelCsb
	// =========================================================================================================


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - ModelCsb
	// =========================================================================================================



	#endregion Sub-Classes

}
