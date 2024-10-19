using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Threading;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Sys.Ctl;
using Microsoft.Data.ConnectionUI;

using static BlackbirdSql.CoreConstants;
using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//											ConnectionCsb Class
//
/// <summary>
/// Adds basic data connection and IDataConnection functionality to the Csb class.
/// </summary>
// =========================================================================================================
public class ConnectionCsb : Csb, IBsConnectionCsb
{

	// ------------------------------------------------------------
	#region Constructors / Destructors - ConnectionCsb
	// ------------------------------------------------------------


	public ConnectionCsb(string connectionString) : base(connectionString)
	{
	}

	public ConnectionCsb(IBsCsb rhs) : base(rhs)
	{
	}


	public ConnectionCsb() : base()
	{
	}




	public ConnectionCsb(string server, int port, string database, string userId, string password)
		: base()
	{
		DataSource = server;
		Port = port;
		Database = database;
		UserID = userId;
		Password = password;
	}


	static ConnectionCsb()
	{
		// Connection specific describers.

		Describers.AddRange(
		[
			new Describer(C_KeyExDataConnection, typeof(DbConnection), C_DefaultExDataConnection, D_Internal),
			new Describer(C_KeyExDataTransaction, typeof(DbTransaction), C_DefaultExDataTransaction, D_Internal)
		]);

		// Diag.DebugTrace("Added connection describers");
	}



	public override void Dispose()
	{
		base.Dispose();
	}



	protected override bool Dispose(bool disposing)
	{
		if (!base.Dispose(disposing))
			return false;

		DisposeTransaction();

		DbConnection connection = DataConnection;

		_ConnectionChangedEvent?.Invoke(this, new(null, connection));

		if (connection != null)
			DisposeConnection();

		return true;
	}



	public void DisposeConnection()
	{
		DisposeTransaction();

		DbConnection connection = DataConnection;

		if (connection == null)
			return;

		bool hasException = false;

		try
		{
			if (connection.State == ConnectionState.Open)
				connection.Close();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);

			hasException = true;
		}


		try
		{
			connection.Dispose();
		}
		catch (Exception ex)
		{
			if (!hasException)
				Diag.Expected(ex);
		}

		Remove(C_KeyExDataConnection);
	}



	public void DisposeTransaction()
	{
		DbTransaction transaction = (DbTransaction)GetValue(C_KeyExDataTransaction);

		if (transaction == null)
			return;

		bool hasTransactions = false;
		bool hasException = false;

		try
		{
			hasTransactions = transaction.HasTransactions();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);

			hasException = true;
		}



		try
		{
			if (hasTransactions)
				transaction.Rollback();
		}
		catch (Exception ex)
		{
			if (!hasException)
				Diag.Expected(ex);

			hasException = true;
		}


		try
		{
			transaction.Dispose();
		}
		catch (Exception ex)
		{
			if (!hasException)
				Diag.Expected(ex);
		}


		DataTransaction = null;
	}



	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - AbstractConnectionCsb
	// =========================================================================================================


	private const DbConnection C_DefaultExDataConnection = null;
	private const DbTransaction C_DefaultExDataTransaction = null;
	private const string C_KeyExDataConnection = "DataConnection";
	private const string C_KeyExDataTransaction = "DataTransaction";


	private EventHandler _PropertyChangedEvent = null;


	#endregion Fields





	// =========================================================================================================
	#region Fields - AbstractConnectionCsb
	// =========================================================================================================


	private long _ConnectionId = 0;

	protected ConnectionChangedDelegate _ConnectionChangedEvent;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractConnectionCsb
	// =========================================================================================================


	[Browsable(false)]
	public long ConnectionId => _ConnectionId;


	[Browsable(false)]
	public DbConnection DataConnection
	{
		get
		{
			return (DbConnection)GetValue(C_KeyExDataConnection);
		}
		set
		{
			if (value == null)
				DisposeTransaction();
			SetValue(C_KeyExDataConnection, value);
		}
	}


	[Browsable(false)]
	public DbTransaction DataTransaction
	{
		get
		{
			lock (_LockObject)
			{
				DbTransaction value = (DbTransaction)GetValue(C_KeyExDataTransaction);

				if (value == null)
					return null;

				bool transactionCompleted = true;

				try
				{
					transactionCompleted = value.Completed();
				}
				catch (Exception ex)
				{
					Diag.Expected(ex);
				}


				if (transactionCompleted)
				{
					DisposeTransaction();
					value = C_DefaultExDataTransaction;
				}

				return value;
			}
		}
		set
		{
			SetValue(C_KeyExDataTransaction, value);
		}
	}


	public bool IsExtensible => true;


	[Browsable(false)]
	public bool IsComplete => IsCompleteMandatory;


	[Browsable(false)]
	public Version ServerVersion
	{
		get
		{
			Version value = (Version)GetValue(C_KeyExServerVersion);

			if (value != null)
				return value;

			DbConnection connection = DataConnection;

			if (connection == null || connection.State != ConnectionState.Open)
				return value;

			value = connection.ParseServerVersion();

			ServerVersion = value;

			return value;
		}
		set
		{
			SetValue(C_KeyExServerVersion, value);
		}
	}


	[Browsable(false)]
	public bool HasTransactions
	{
		get
		{
			try
			{
				return DataTransaction?.HasTransactions() ?? false;
			}
			catch
			{
				DisposeTransaction();
				throw;
			}
		}
	}


	[Browsable(false)]
	public bool PeekTransactions
	{
		get
		{
			try
			{
				return DataTransaction?.HasTransactions() ?? false;
			}
			catch
			{
				return false;
			}
		}
	}


	[Browsable(false)]
	public ConnectionState State
	{
		get
		{
			DbConnection connection = DataConnection;
			return connection == null ? ConnectionState.Closed : connection.State;
		}
	}




	[Browsable(false)]
	public event ConnectionChangedDelegate ConnectionChangedEvent
	{
		add { _ConnectionChangedEvent += value; }
		remove { _ConnectionChangedEvent -= value; }
	}


	[Browsable(false)]
	public event EventHandler PropertyChanged
	{
		add { _PropertyChangedEvent += value; }
		remove { _PropertyChangedEvent -= value; }
	}

	#endregion 	Property Accessors





	// =========================================================================================================
	#region Methods - AbstractConnectionCsb
	// =========================================================================================================


	void IDataConnectionProperties.Add(string propertyName)
	{
		// Do nothing. Csbn propertiesare dynamic.
	}



	public bool BeginTransaction(IsolationLevel isolationLevel)
	{
		DbTransaction transaction = DataTransaction;

		if (transaction != null)
		{
			Diag.Debug(new ApplicationException("BeginTransaction failed. Transaction already exists."));
			return true;
		}

		DbConnection connection = DataConnection;

		if (connection == null || connection.State != ConnectionState.Open)
		{
			Diag.Debug(new ApplicationException("BeginTransaction failed. Connection is nul or not open."));
			return false;
		}

		lock (_LockObject)
		{
			transaction = connection.BeginTransaction(isolationLevel);
			DataTransaction = transaction;
		}

		return true;
	}



	public bool CloseConnection()
	{
		DisposeTransaction();

		DbConnection connection = DataConnection;

		if (connection == null)
			return true;


		try
		{
			if (connection.State == ConnectionState.Open)
				connection.Close();
			return true;
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);
		}

		_ConnectionChangedEvent?.Invoke(this, new(null, connection));

		DisposeConnection();

		return false;
	}



	bool IDataConnectionProperties.Contains(string propertyName) =>
		ContainsKey(propertyName);



	public DbCommand CreateCommand(string cmd = null)
	{
		return NativeDb.CreateDbCommand(cmd);
	}



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
		// Evs.Debug(GetType(), nameof(CreateDataConnection));


		DbConnection connection = DataConnection;
		DbConnection newConnection = null;

		try
		{
			newConnection = (DbConnection)NativeDb.CreateDbConnection(ConnectionString);
		}
		catch (Exception ex)
		{
			Diag.Debug(ex);
		}

		_ConnectionChangedEvent?.Invoke(this, new(newConnection, connection));

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
	public (bool, bool) OpenOrVerifyConnection()
	{
		// Evs.Debug(GetType(), nameof(OpenOrVerifyConnection));

		DbConnection connection = DataConnection;

		bool isOpen = false;
		bool hasTransactions = false;

		if (connection == null)
		{
			DisposeTransaction();
			return (isOpen, hasTransactions);
		}


		try
		{
			(isOpen, hasTransactions) = connection.OpenOrVerify();
		}
		catch (Exception ex)
		{
			_ConnectionChangedEvent?.Invoke(this, new(null, null));

			DisposeConnection();

			Diag.Expected(ex);
			throw ex;
		}

		Exception exd = null;

		if (connection.State != ConnectionState.Open)
		{
			_ConnectionChangedEvent?.Invoke(this, new(null, null));

			DisposeConnection();

			exd ??= new DataException("Failed to open connection");

			Diag.Expected(exd);
			throw exd;
		}

		return (isOpen, hasTransactions);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens or verifies a connection. The Connection must exists.
	/// Throws an exception on failure.
	/// Always use this method to open connections because it disposes of the connection
	/// and invokes ConnectionChangedEvent on failure.
	/// Do not call before ensuring IsComplete.
	/// The cardinal must be either zero, if no keepalive reads are required, or an
	/// incremental value that can be used to execute a select statement unique from
	/// the previous call to this method.
	/// </summary>
	/// <returns>
	/// Boolean tuple with Item1: True if open / verification succeeded and
	/// Item2: HasTransactions.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public async Task<(bool, bool)> OpenOrVerifyConnectionAsync(CancellationToken cancelToken)
	{
		// Evs.Debug(GetType(), nameof(OpenOrVerifyConnectionAsync));

		DbConnection connection = DataConnection;

		bool isOpen = false;
		bool hasTransactions = false;

		if (connection == null)
		{
			DisposeTransaction();
			return (isOpen, hasTransactions);
		}


		try
		{
			(isOpen, hasTransactions) = await connection.OpenOrVerifyAsync(DataTransaction, cancelToken);
		}
		catch (Exception ex)
		{
			_ConnectionChangedEvent?.Invoke(this, new(null, connection));

			DisposeConnection();

			Diag.Expected(ex);
			throw ex;
		}

		if (cancelToken.Cancelled())
			return (isOpen, hasTransactions);

		if (connection.State != ConnectionState.Open)
		{
			_ConnectionChangedEvent?.Invoke(this, new(null, null));

			DisposeConnection();

			Exception exd = new DataException("Failed to open connection");

			Diag.Expected(exd);
			throw exd;
		}


		return (isOpen, hasTransactions);
	}



	public void Parse(string connectionString)
	{
		ConnectionString = connectionString;
		RaisePropertyChanged(null);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Closes a connection if open and disposes of it if it is broken on close,
	/// then applies the current PropertyAgent values to it. 
	/// </summary>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	protected bool RefreshDataConnection()
	{
		// Evs.Debug(GetType(), nameof(RefreshDataConnection));

		CloseConnection();

		DbConnection connection = DataConnection;

		if (connection == null)
			return false;

		connection.ConnectionString = ConnectionString;

		// Evs.Debug(GetType(), "RefreshDataConnection()", $"Connection refreshed with connectiongstring: {ConnectionString}.");

		return true;
	}



	void IDataConnectionProperties.Remove(string propertyName)
		=> Remove(propertyName);


	public virtual void Reset()
	{
		if (string.IsNullOrEmpty(ConnectionString))
			return;

		Clear();
		_Moniker = null;
		_UnsafeMoniker = null;

		RaisePropertyChanged(null);
	}


	public virtual void Reset(string propertyName)
	{
		if (Remove(propertyName))
			RaisePropertyChanged(propertyName);
	}



	public void Test()
	{
		IDbConnection conn = NativeDb.CreateDbConnection(ConnectionString);

		conn.Open();
		conn.Close();
		conn.Dispose();
	}



	public virtual string ToFullString()
	{
		return ConnectionString ?? "";
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstractConnectionCsb
	// =========================================================================================================


	protected override void RaisePropertyChanged(string propertyName)
	{
		_PropertyChangedEvent?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}



	#endregion Event Handling

}
