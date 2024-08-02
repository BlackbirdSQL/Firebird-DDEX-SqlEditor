// Microsoft.SqlServer.RegSvrEnum, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.Smo.RegSvrEnum.UIConnectionInfo

using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Interfaces;



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

	private long _ConnectionId = 0;

	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - ModelCsb
	// =========================================================================================================


	public long ConnectionId => _ConnectionId;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the connection using the latest available properties. If it's properties
	/// are outdated, closes the connection and applies the latest properties without
	/// reopening. Returns null if no connection exists. If a Close() fails, disposes of
	/// the connection.
	/// </summary>
	// ---------------------------------------------------------------------------------
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

				if (!IsInvalidated)
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


	public event ConnectionChangedDelegate ConnectionChangedEvent
	{
		add { _ConnectionChangedEvent += value; }
		remove { _ConnectionChangedEvent -= value; }
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - ModelCsb
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a new data connection. If a connection already exists, disposes of the
	/// connection.
	/// Always use this method to create connections because it invokes
	/// ConnectionChangedEvent.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void CreateDataConnection()
	{
		// Tracer.Trace(GetType(), "CreateDataConnection()");


		DbConnection connection = DataConnection;
		DbConnection newConnection = null;

		try
		{
			newConnection = (DbConnection)NativeDb.CreateDbConnection(ConnectionString);
			_ConnectionChangedEvent?.Invoke(this, new(newConnection, connection));
		}
		catch (Exception ex)
		{
			Diag.Debug(ex);
		}

		if (newConnection == null)
			return;

		if (connection != null)
			DisposeConnection();

		DataConnection = newConnection;
		_ConnectionId++;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens or verifies a connection. The Connection must exists.
	/// Throws an exception on failure.
	/// Always use this method to open connections because it disposes of the connection
	/// and invokes ConnectionChangedEvent on failure.
	/// Do not call before ensuring IsComplete.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool OpenOrVerifyConnection()
	{
		// Tracer.Trace(GetType(), "CreateDataConnection()");

		DbConnection connection = DataConnection;

		if (connection == null)
			Diag.ThrowException(new InvalidOperationException("Connection is null"));

		try
		{
			connection.OpenOrVerify();
		}
		catch (Exception ex)
		{
			try
			{
				_ConnectionChangedEvent?.Invoke(this, new(null, null));
			}
			catch { }

			DisposeConnection();

			Diag.Expected(ex);
			throw ex;
		}

		Exception exd = null;

		if (connection.State != ConnectionState.Open)
		{
			try
			{
				_ConnectionChangedEvent?.Invoke(this, new(null, null));
			}
			catch (Exception ex)
			{
				exd = ex;
			}

			DisposeConnection();

			exd ??= new DataException("Failed to open connection");

			Diag.Expected(exd);
			throw exd;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens or verifies a connection. The Connection must exists.
	/// Throws an exception on failure.
	/// Always use this method to open connections because it disposes of the connection
	/// and invokes ConnectionChangedEvent on failure.
	/// Do not call before ensuring IsComplete.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task<bool> OpenOrVerifyConnectionAsync()
	{
		// Tracer.Trace(GetType(), "CreateDataConnection()");

		DbConnection connection = DataConnection;

		if (connection == null)
			Diag.ThrowException(new InvalidOperationException("Connection is null"));

		bool result = false;

		try
		{
			result = await connection.OpenOrVerifyAsync();
		}
		catch (Exception ex)
		{
			_ConnectionChangedEvent?.Invoke(this, new(null, connection));

			DisposeConnection();

			Diag.Expected(ex);
			throw ex;
		}


		if (connection.State != ConnectionState.Open)
		{
			_ConnectionChangedEvent?.Invoke(this, new(null, null));

			DisposeConnection();

			Exception exd = new DataException("Failed to open connection");

			Diag.Expected(exd);
			throw exd;
		}

		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Closes a connection if open and disposes of it if it is broken on close,
	/// then applies the current PropertyAgent values to it. 
	/// </summary>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	private bool RefreshDataConnection()
	{
		// Tracer.Trace(GetType(), "CreateDataConnection()");

		CloseConnection();

		DbConnection connection = DataConnection;

		if (connection == null)
			return false;

		connection.ConnectionString = ConnectionString;

		// Tracer.Trace(GetType(), "RefreshDataConnection()", "Connection refreshed with connectiongstring: {0}.", Csa.ConnectionString);

		return true;
	}


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
