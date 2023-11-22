
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows.Media.Imaging;

using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Properties;

using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//										AbstractModelPropertyAgent Class
//
/// <summary>
/// 
/// The Method Members partial class of the AbstractModelPropertyAgent class.
/// A conglomerate base class for supporting all of... the Sql client UIConnectionInfo class and UI
/// Connection and Model classes, and implementing the IDataConnectionProperties, ICustomTypeDescriptor
/// and all other known connection interfaces.
/// </summary>
/// <remarks>
/// Steps for adding an additional property in descendents:
/// 1. Add the core property descriptor support using
/// <see cref="Add(string, Type, object)"/>.
/// 2. Add the property accessor using <see cref="GetProperty(string)"/> and 
/// <see cref="SetProperty(string, object)"/>.
/// 3. If the property type cannot be supported by the built-in types in 
/// <see cref="SetProperty(string, object)"/>, overload the method to support the new type.
/// 4. If (3) applies include a Set_[NewType]Property() method to support the new type using the builtin
/// Set_[Type]Property() methods as a template.
/// 5. If (4) applies add a Will[NewType]Change() method if an existing builtin method cannot be used.  
/// Note: Additional properties will not be included in the connection string or have actual property
/// descriptors. <see cref="ICustomTypeDescriptor"/> members will still need to be overloaded.
/// </remarks>
// =========================================================================================================
public abstract class AbstractModelPropertyAgent : AbstractPropertyAgent
{


	// ---------------------------------------------------------------------------------
	#region Variables - AbstractModelPropertyAgent
	// ---------------------------------------------------------------------------------

	protected static new DescriberDictionary _Describers = null;


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - AbstractModelPropertyAgent
	// =========================================================================================================


	public override DbConnection DataConnection
	{
		get
		{
			_DataConnection ??= new FbConnection(ConnectionStringBuilder.ConnectionString);

			return _DataConnection;
		}
	}


	public override BitmapImage IconImage => ModelIconsCollection.Instance.GetImage(
		Isset("Icon") ? (IBIconType)GetProperty("Icon") : ModelIconsCollection.Instance.ServerError_32);


	// public override string[] AdvancedOptions => CorePropertySet.AdvancedOptions;

	public override DescriberDictionary Describers
	{
		get
		{
			if (_Describers == null)
				CreateAndPopulatePropertySet(null);

			return _Describers;
		}
	}



	public virtual string ServerNameNoDot
	{
		get
		{
			string text = DataSource ?? "localhost";


			if (text.Length == 0 || text == ".")
				return "localhost";

			if (text.StartsWith(".\\", StringComparison.Ordinal))
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", "localhost", text[1..]);
			}

			return text;
		}
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Descriptors Property Accessors - AbstractModelPropertyAgent
	// =========================================================================================================


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


	public FbWireCrypt WireCrypt
	{
		get { return (FbWireCrypt)GetProperty("WireCrypt"); }
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


	#endregion 	Descriptors Property Accessors





	// =========================================================================================================
	#region Property Getters/Setters - AbstractModelPropertyAgent
	// =========================================================================================================



	public override (IBIconType, bool) GetSet_Icon()
	{
		if (ServerType == FbServerType.Embedded)
		{
			Icon = CoreIconsCollection.Instance.EmbeddedDatabase_32;

			return (CoreIconsCollection.Instance.EmbeddedDatabase_32, false);
		}

		if (ServerEngine == EnEngineType.Unknown)
			return (CoreIconsCollection.Instance.ServerError_32, false);

		IBIconType iconType;

		switch (ServerEngine)
		{
			case EnEngineType.LocalClassicServer:
				iconType = ModelIconsCollection.Instance.LocalClassicServer_32;
				break;
			case EnEngineType.LocalSuperClassic:
				iconType = ModelIconsCollection.Instance.LocalSuperClassic_32;
				break;
			case EnEngineType.LocalSuperServer:
				iconType = ModelIconsCollection.Instance.LocalSuperServer_32;
				break;
			case EnEngineType.ClassicServer:
				iconType = ModelIconsCollection.Instance.ClassicServer_32;
				break;
			case EnEngineType.SuperClassic:
				iconType = ModelIconsCollection.Instance.SuperClassic_32;
				break;
			case EnEngineType.SuperServer:
				iconType = ModelIconsCollection.Instance.SuperServer_32;
				break;
			default:
				InvalidOperationException ex = new(string.Format(Resources.EngineTypeIconNotfound, ServerEngine));
				Diag.Dug(ex);
				throw ex;
		}

		Icon = iconType;

		return (iconType, false);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ServerEngine property and sets it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived ServerEngine else null and
	/// a boolean indicating wehther or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public override (EnEngineType, bool) GetSet_ServerEngine()
	{
		EnEngineType serverEngine;

		if (ServerType == FbServerType.Embedded)
		{
			serverEngine = EnEngineType.EmbeddedDatabase;
			return (serverEngine, false);
		}
		else if (DataConnection.State != ConnectionState.Open && !IsComplete)
		{
			return (EnEngineType.Unknown, false);
		}

		Version version = ServerVersion;

		if (version == null)
			return (EnEngineType.Unknown, false);

		bool opened = false;

		string serverClass = null;

		if (version.Major < 4)
		{
			if (version.Major < 3)
				serverClass = "Classic";
			else
				serverClass = "SuperServer";
		}
		else
		{

			if (DataConnection.State != ConnectionState.Open)
			{
				try
				{
					DataConnection.Open();
				}
				catch
				{ }

				if (DataConnection.State != ConnectionState.Open)
					return (EnEngineType.Unknown, false);

				opened = true;
			}

			DbCommand cmd = CreateCommand("select rdb$config_value from RDB$CONFIG where rdb$config_name='ServerMode'");

			try
			{
				serverClass = (string)cmd.ExecuteScalar();
			}
			catch { }


			if (string.IsNullOrEmpty(serverClass))
			{
				ArgumentException ex = new("The Firebird server return an empty argument for ServerMode");
				Diag.Dug(ex);
				throw ex;
			}
		}

		string host = DataSource;

		if (string.IsNullOrEmpty(host))
			host = Dns.GetHostName();

		bool isLocalHost = PropertySet.IsLocalIpAddress(host);


		switch (serverClass.ToLower())
		{
			case "classic":
				serverEngine = isLocalHost ? EnEngineType.LocalClassicServer : EnEngineType.ClassicServer;
				break;
			case "superclassic":
				serverEngine = isLocalHost ? EnEngineType.LocalSuperClassic : EnEngineType.SuperClassic;
				break;
			case "superserver":
				serverEngine = isLocalHost ? EnEngineType.LocalSuperServer : EnEngineType.SuperServer;
				break;
			default:
				ArgumentException ex = new("The Firebird server returned a bad argument for ServerMode: " + serverClass);
				Diag.Dug(ex);
				throw ex;
		}

		ServerEngine = serverEngine;

		return (serverEngine, opened);
	}


	#endregion Property Getters/Setters




	// =========================================================================================================
	#region Constructors / Destructors - AbstractModelPropertyAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Universal .ctor
	/// </summary>
	/// <remarks>
	/// It is the final owning (child) class's responsibility to perform the once off
	/// creation of the static private property set for it's class by calling the static
	/// CreatePropertySet(). The final child class is the class that initiated
	/// instanciation and is identifiable in that it has no _Owner private instance
	/// variable.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public AbstractModelPropertyAgent(IBEventsChannel channel, IBPropertyAgent rhs, bool generateNewId)
		: base(channel, rhs, generateNewId)
	{
	}

	public AbstractModelPropertyAgent(IBPropertyAgent rhs, bool generateNewId) : this(null, rhs, generateNewId)
	{
	}

	public AbstractModelPropertyAgent(bool generateNewId) : this(null, null, generateNewId)
	{
	}


	public AbstractModelPropertyAgent() : this(null, null, true)
	{
	}


	public AbstractModelPropertyAgent(IBPropertyAgent rhs) : this(null, rhs, true)
	{
	}

	public AbstractModelPropertyAgent(string server, int port, string database, string userId, string password)
		: this(null, null, true)
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
			_Describers = new();

			// Initializers for property sets are held externally for this class
			ModelPropertySet.CreateAndPopulatePropertySetFromStatic(_Describers);
		}

		// If null then this was a call from our own .ctor so no need to pass anything back
		describers?.AddRange(_Describers);

	}

	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - AbstractModelPropertyAgent
	// =========================================================================================================


	public abstract override IBPropertyAgent Copy();



	public override DbCommand CreateCommand(string cmd = null)
	{
		if (cmd == null)
			return new FbCommand();

		return new FbCommand(cmd);
	}


	public override void Parse(string s)
	{
		Parse(new CsbAgent(s));
	}



	public override object GetProperty(string name)
	{
		if (_AssignedConnectionProperties.TryGetValue(name, out object value))
			return value;

		return base.GetProperty(name);
	}



	public override IBPropertyAgent ToUiConnectionInfo()
	{
		return new UIConnectionInfo(this);
	}


	#endregion Methods


}
