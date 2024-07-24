using System;
using System.Data;
using System.Data.Common;
using System.Text;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//										AbstractModelPropertyAgent Class
//
/// <summary>
/// 
/// The database properties conglomerate base class for supporting all of... the Sql client UIConnectionInfo
/// class and UI Connection and Model classes, and implementing the IDataConnectionProperties,
/// ICustomTypeDescriptor and all other known connection interfaces.
/// </summary>
// =========================================================================================================
public abstract class AbstractModelPropertyAgent : AbstractPropertyAgent, IBsModelPropertyAgent
{

	// ------------------------------------------------------------
	#region Constructors / Destructors - AbstractModelPropertyAgent
	// ------------------------------------------------------------

	/// <summary>
	/// Universal .ctor.
	/// </summary>
	public AbstractModelPropertyAgent(IBsModelPropertyAgent rhs, bool generateNewId)
		: base(rhs, generateNewId)
	{
	}


	public AbstractModelPropertyAgent(bool generateNewId) : this(null, generateNewId)
	{
	}


	public AbstractModelPropertyAgent() : this(null, true)
	{
	}


	public AbstractModelPropertyAgent(IBsModelPropertyAgent rhs) : this(rhs, true)
	{
	}

	public AbstractModelPropertyAgent(string server, int port, string database, string userId, string password)
		: this(null, true)
	{
		DataSource = server;
		Port = port;
		Database = database;
		UserID = userId;
		Password = password;
	}


	static AbstractModelPropertyAgent()
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

		DisposeTransaction(true);

		try
		{
			_ConnectionChangedEvent?.Invoke(this, new(null, _DataConnection));
		}
		catch { }

		if (_DataConnection != null)
			DisposeConnection();

		return true;
	}



	public void DisposeConnection()
	{
		if (_DataConnection == null)
			return;

		DisposeTransaction(true);

		bool hasException = false;

		try
		{
			if (_DataConnection.State == ConnectionState.Open)
				_DataConnection.Close();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);

			hasException = true;
		}


		try
		{
			_DataConnection.Dispose();
		}
		catch (Exception ex)
		{
			if (!hasException)
				Diag.Expected(ex);
		}

		_DataConnection = null;
	}



	public void DisposeTransaction(bool force)
	{
		if (_DataTransaction == null)
			return;

		bool hasTransactions = false;
		bool hasException = false;

		try
		{
			hasTransactions = _DataTransaction.HasTransactions();
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);

			hasException = true;
		}



		if (!force && hasTransactions)
			Diag.ThrowException(new DataException("Attempt to dispose of database Transaction object that has pending transactions."));

		try
		{
			if (hasTransactions)
				_DataTransaction.Rollback();
		}
		catch (Exception ex)
		{
			if (!hasException)
				Diag.Expected(ex);

			hasException = true;
		}


		try
		{
			_DataTransaction.Dispose();
		}
		catch (Exception ex)
		{
			if (!hasException)
				Diag.Expected(ex);
		}


		_DataTransaction = null;
	}



	public abstract override IBsPropertyAgent Copy();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The daisy-chained static mehod for creating a class's static private property
	/// set.
	/// </summary>
	/// <param name="propertyTypes">
	/// The property types list this class will add it's properties to. As a rule this
	/// parameter will be null when called by it's own .ctor so that we can
	/// distinguish between calls from descendent CreateAndPopulatePropertySet()
	/// methods and the .ctor.
	/// </param>
	/// <remarks>
	/// (Terminology: an object's Owner is it's direct child descendent class's
	/// instance. an Initiator is the final descendent child class instance. ie it
	/// has no Owner. A Sub-instance is an object with an Owner.)
	/// Whenever an instance is the Initiator it must make a call to this method with
	/// a null argument to initiate the creation of it's class's private property set,
	/// if it does not exist. If a sub-class is simply adding it's properties to a
	/// child's property set it may at the same time create it's own property set,
	/// which can then be used to populate the <paramref name="propertyTypes"/> list's
	/// of subsequent child classes, in addition to having the property set already
	/// available for it's own final class instantiations as Initiator. That is a
	/// performance issue to be determined for each class.
	/// If a Class's property set is a replica of it's parent's, it may force it's
	/// parent to create the common shared property set by passing null to the parent
	/// class's CreatePropertySet() method. For details on property sets refer to
	/// the <see cref="ModelPropertySet"/> class.
	/// </remarks>
	protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
	{
		if (_Describers == null)
		{
			_Describers = [];

			// Initializers for property sets are held externally for this class
			ModelPropertySet.CreateAndPopulatePropertySetFromStatic(_Describers);
		}

		// If null then this was a call from our own .ctor so no need to pass anything back
		describers?.AddRange(_Describers);

	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - AbstractModelPropertyAgent
	// =========================================================================================================


	protected IDbConnection _DataConnection = null;
	protected IDbTransaction _DataTransaction = null;

	protected static new DescriberDictionary _Describers = null;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AbstractModelPropertyAgent
	// =========================================================================================================


	protected override DescriberDictionary Describers
	{
		get
		{
			if (_Describers == null)
				CreateAndPopulatePropertySet(null);

			return _Describers;
		}
	}



	public IDbConnection DataConnection => _DataConnection;



	public int PacketSize
	{
		get { return (int)GetProperty("PacketSize"); }
		set { SetProperty("PacketSize", value); }
	}


	public string Role
	{
		get { return (string)GetProperty("Role"); }
		set { SetProperty("Role", value); }
	}


	public int Dialect
	{
		get { return (int)GetProperty("Dialect"); }
		set { SetProperty("Dialect", value); }
	}


	public string Charset
	{
		get { return (string)GetProperty("Charset"); }
		set { SetProperty("Charset", value); }
	}


	public int ConnectionTimeout
	{
		get { return (int)GetProperty("ConnectionTimeout"); }
		set { SetProperty("ConnectionTimeout", value); }
	}


	public bool Pooling
	{
		get { return (bool)GetProperty("Pooling"); }
		set { SetProperty("Pooling", value); }
	}


	public int ConnectionLifeTime
	{
		get { return (int)GetProperty("ConnectionLifeTime"); }
		set { SetProperty("ConnectionLifeTime", value); }
	}


	public int MinPoolSize
	{
		get { return (int)GetProperty("MinPoolSize"); }
		set { SetProperty("MinPoolSize", value); }
	}



	public int MaxPoolSize
	{
		get { return (int)GetProperty("MaxPoolSize"); }
		set { SetProperty("MaxPoolSize", value); }
	}


	public int FetchSize
	{
		get { return (int)GetProperty("FetchSize"); }
		set { SetProperty("FetchSize", value); }
	}


	public IsolationLevel IsolationLevel
	{
		get { return (IsolationLevel)GetProperty("IsolationLevel"); }
		set { SetProperty("IsolationLevel", value); }
	}


	public bool ReturnRecordsAffected
	{
		get { return (bool)GetProperty("ReturnRecordsAffected"); }
		set { SetProperty("ReturnRecordsAffected", value); }
	}


	public bool Enlist
	{
		get { return (bool)GetProperty("Enlist"); }
		set { SetProperty("Enlist", value); }
	}


	public string ClientLibrary
	{
		get { return (string)GetProperty("ClientLibrary"); }
		set { SetProperty("ClientLibrary", value); }
	}


	public int DbCachePages
	{
		get { return (int)GetProperty("DbCachePages"); }
		set { SetProperty("DbCachePages", value); }
	}


	public bool NoDatabaseTriggers
	{
		get { return (bool)GetProperty("NoDatabaseTriggers"); }
		set { SetProperty("NoDatabaseTriggers", value); }
	}


	public bool NoGarbageCollect
	{
		get { return (bool)GetProperty("NoGarbageCollect"); }
		set { SetProperty("NoGarbageCollect", value); }
	}


	public bool Compression
	{
		get { return (bool)GetProperty("Compression"); }
		set { SetProperty("Compression", value); }
	}


	public byte[] CryptKey
	{
		get { return (byte[])Encoding.Default.GetBytes((string)GetProperty("CryptKey")); }
		set { SetProperty("CryptKey", value); }
	}


	public EnWireCrypt WireCrypt
	{
		get { return (EnWireCrypt)GetProperty("WireCrypt"); }
		set { SetProperty("WireCrypt", value); }
	}


	public string ApplicationName
	{
		get { return (string)GetProperty("ApplicationName"); }
		set { SetProperty("ApplicationName", value); }
	}


	public int CommandTimeout
	{
		get { return (int)GetProperty("CommandTimeout"); }
		set { SetProperty("CommandTimeout", value); }
	}


	public int ParallelWorkers
	{
		get { return (int)GetProperty("ParallelWorkers"); }
		set { SetProperty("ParallelWorkers", value); }
	}


	public Version ServerVersion
	{
		get { return (Version)GetProperty("ServerVersion"); }
		set { SetProperty("ServerVersion", value); }
	}




	public string ConnectionString => _Csa?.ConnectionString;

	public string Moniker => _Moniker ??= Csa.SafeDatasetMoniker;


	public IDbTransaction DataTransaction
	{
		get
		{
			lock (_LockObject)
			{
				if (_DataTransaction == null)
					return null;

				bool transactionCompleted = true;

				try
				{
					transactionCompleted = _DataTransaction.Completed();
				}
				catch (Exception ex)
				{
					Diag.Expected(ex);
				}


				if (transactionCompleted)
					DisposeTransaction(true);

				return _DataTransaction;
			}
		}
	}


	public bool HasTransactions
	{
		get
		{
			try
			{
				return DataTransaction != null && _DataTransaction.HasTransactions();
			}
			catch
			{
				DisposeTransaction(true);

				throw;
			}
		}
	}


	public ConnectionState State => _DataConnection == null ? ConnectionState.Closed : _DataConnection.State;


	#endregion 	Property Accessors





	// =========================================================================================================
	#region Methods - AbstractModelPropertyAgent
	// =========================================================================================================


	public void BeginTransaction(IsolationLevel isolationLevel)
	{
		if (_DataTransaction != null)
			return;

		lock (_LockObject)
			_DataTransaction = DataConnection.BeginTransaction(isolationLevel);

	}



	public override bool CloseConnection()
	{
		if (_DataConnection == null)
			return true;

		DisposeTransaction(true);

		try
		{
			_DataConnection.Close();
			return true;
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);
		}

		try
		{
			_ConnectionChangedEvent?.Invoke(this, new(null, _DataConnection));
		}
		catch { }

		DisposeConnection();

		return false;
	}



	public DbCommand CreateCommand(string cmd = null)
	{
		return NativeDb.CreateDbCommand(cmd);
	}


	protected override (Version, bool) GetServerVersion()
	{
		if (DataConnection is not DbConnection dbConnection)
			return (null, false);

		bool opened = false;

		if (dbConnection.State != ConnectionState.Open)
		{
			try
			{
				dbConnection.Open();
			}
			catch
			{ }

			if (dbConnection.State != ConnectionState.Open)
				return (null, false);

			opened = true;
		}


		Version version = new(dbConnection.ServerVersion);

		return (version, opened);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ServerVersion property and sets it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived ServerVersion else null and
	/// a boolean indicating wehther or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public (Version, bool) GetSet_ServerVersion()
	{
		if (State != ConnectionState.Open && !IsComplete)
			return (null, false);


		bool opened;
		Version version;

		(version, opened) = GetServerVersion();

		if (version != null)
			ServerVersion = version;

		return (version, opened);
	}



	public override void Parse(string connectionString)
	{
		Parse(new Csb(connectionString));
	}



	public override void Test()
	{
		IDbConnection conn = NativeDb.CreateDbConnection(ConnectionString);

		conn.Open();
		conn.Close();
		conn.Dispose();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Attempts to derive a value for a property and sets it if successful.
	/// </summary>
	/// <param name="name">The property name</param>
	/// <param name="value">The derived value or fallback value else null</param>
	/// <returns>
	/// Returns true if a useable value was derived, even if the value could not be used
	/// to set the property, else false if no useable property could be derived. An
	/// example of a useable derived property that will not be used to set the property
	/// is the ServerError Icon property when a connection cannot be established.
	/// </returns>
	/// <remarks>
	/// This method may be recursively called because a derived property may be dependent
	/// on other derived properties, any of which may require access to an open database
	/// connection. Each get/set method returns a boolean indicating whether or not it
	/// opened the connection. We track that and only close the connection (if necessary)
	/// when we exit the recursion to avoid repetitively opening and closing the connection. 
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected override bool TryGetSetDerivedProperty(string name, out object value)
	{
		bool connectionOpened = false;
		bool result = false;

		_GetSetCardinal++;

		try
		{
			switch (name)
			{
				case "ServerVersion":
					(value, connectionOpened) = GetSet_ServerVersion();
					result = value != null;
					break;
				default:
					result = base.TryGetSetDerivedProperty(name, out value);
					break;
			}
		}
		finally
		{
			_GetSetConnectionOpened |= connectionOpened;
			_GetSetCardinal--;

			if (_GetSetCardinal == 0 && _GetSetConnectionOpened)
			{
				CloseConnection();

				_GetSetConnectionOpened = false;
			}
		}

		return result;

	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstractModelPropertyAgent
	// =========================================================================================================



	#endregion Event Handling

}
