// Microsoft.SqlServer.RegSvrEnum, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.Smo.RegSvrEnum.UIConnectionInfo

using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Ctl;

using static BlackbirdSql.Shared.Ctl.QueryExecution.QueryManager;



namespace BlackbirdSql.Shared.Model;


// =========================================================================================================
//
//									ConnectionInfoPropertyAgent Class
//
/// <summary>
/// PropertyAgent class for supporting query connections.
/// </summary>
// =========================================================================================================
public class ConnectionInfoPropertyAgent : AbstractModelPropertyAgent, IBsConnectionInfo
{

	// -------------------------------------------------------------
	#region Constructors / Destructors - ConnectionInfoPropertyAgent
	// -------------------------------------------------------------


	/// <summary>
	/// Universal .ctor.
	/// </summary>
	public ConnectionInfoPropertyAgent(IBsConnectionInfo lhs, bool generateNewId) : base(lhs, generateNewId)
	{

	}



	public ConnectionInfoPropertyAgent() : base(null, true)
	{
	}



	public ConnectionInfoPropertyAgent(IBsConnectionInfo lhs) : base(lhs, true)
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



	public override IBsPropertyAgent Copy()
	{
		return new ConnectionInfoPropertyAgent(this, true);
	}



	protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
	{
		// Just pass any request down. This class uses it's parent's descriptor.
		AbstractModelPropertyAgent.CreateAndPopulatePropertySet(describers);
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - ConnectionInfoPropertyAgent
	// =========================================================================================================

	private long _ConnectionId = 0;

	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - ConnectionInfoPropertyAgent
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
	public IDbConnection LiveConnection
	{
		get
		{
			lock (_LockObject)
			{
				if (_DataConnection == null)
					return null;

				// We have to ensure the connection hasn't changed.

				// Get the connection string of the current connection adorned with the additional Csa properties
				// so that we don't get a negative equivalency because of missing stripped Csa properties in the
				// connection's connection string.
				string connectionString = RctManager.AdornConnectionStringFromRegistration(_DataConnection);

				if (connectionString == null)
					return _DataConnection;

				Csb csaCurrent = new(connectionString, false);
				Csb csaRegistered = RctManager.CloneRegistered(this);

				// Compare the current connection with the registered connection.
				if (Csb.AreEquivalent(csaRegistered, csaCurrent, Csb.DescriberKeys))
				{
					// Nothing's changed.
					return _DataConnection;
				}

				// Tracer.Trace(GetType(), "get_Connection()", "Connections are not equivalent: \nCurrent: {0}\nRegistered: {1}",
				//	csaCurrent.ConnectionString, csaRegistered.ConnectionString);

				if (Csb.AreEquivalent(csaRegistered, csaCurrent, Csb.EquivalencyKeys))
				{
					// The connection is the same but it's adornments have changed.
					lock (_LockObject)
					{
						EventUpdateEnter(true, true);

						try
						{
							Parse(csaRegistered.ConnectionString);
						}
						finally
						{
							EventUpdateExit();
						}

						// Tracer.Trace(GetType(), "get_LiveConnection()", "Parsed with new connection. IsComplete? {0}.", IsComplete);

						RefreshDataConnection();

						return _DataConnection;
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
	#region Methods - ConnectionInfoPropertyAgent
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

		IDbConnection newConnection = (DbConnection)NativeDb.CreateDbConnection(Csa.ConnectionString);

		try
		{
			_ConnectionChangedEvent?.Invoke(this, new(newConnection, _DataConnection));
		}
		catch (Exception ex)
		{
			Diag.Debug(ex);
		}

		if (_DataConnection != null)
			DisposeConnection();

		_DataConnection = newConnection;
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

		if (_DataConnection == null)
			Diag.ThrowException(new InvalidOperationException("Connection is null"));

		try
		{
			_DataConnection.OpenOrVerify();
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

		if (_DataConnection.State != ConnectionState.Open)
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

		if (_DataConnection == null)
			Diag.ThrowException(new InvalidOperationException("Connection is null"));

		bool result = false;

		try
		{
			result = await _DataConnection.OpenOrVerifyAsync();
		}
		catch (Exception ex)
		{
			_ConnectionChangedEvent?.Invoke(this, new(null, _DataConnection));

			DisposeConnection();

			Diag.Expected(ex);
			throw ex;
		}


		if (_DataConnection.State != ConnectionState.Open)
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

		if (_DataConnection == null)
			return false;

		_DataConnection.ConnectionString = Csa.ConnectionString;

		// Tracer.Trace(GetType(), "RefreshDataConnection()", "Connection refreshed with connectiongstring: {0}.", Csa.ConnectionString);

		return true;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - ConnectionInfoPropertyAgent
	// =========================================================================================================


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - ConnectionInfoPropertyAgent
	// =========================================================================================================



	#endregion Sub-Classes

}
