// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Server

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using BlackbirdSql.LanguageExtension.Model.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.Win32;
using static Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataProviderUtils.Names;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaServer Class
//
/// <summary>
/// The connection impersonating an SQL Server Smo Server for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaServer : AbstractSmoMetaDatabaseObjectBase, IServer, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaServer
	// ---------------------------------------------------------------------------------


	public SmoMetaServer(LsbSmoServer smoMetadataObject, bool isConnected)
	{
		Diag.ThrowIfInstanceNull(smoMetadataObject, typeof(LsbSmoServer));
		// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.Assert(smoMetadataObject != null, "SmoMetadataProvider Assert", "smoMetadataObject != null");
		_SmoMetadataObject = smoMetadataObject;
		_IsConnected = isConnected;
		// using (Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.GetActivityContext("Refresh Server."))
		// {
			if (IsConnected)
			{
				foreach (SmoConfig.SmoInitFields allInitField in SmoConfig.SmoInitFields.GetAllInitFields())
				{
					_SmoMetadataObject.SetDefaultInitFields(allInitField.Type, _SmoMetadataObject.EngineEdition, allInitField.Optimized);
				}
				_SmoMetadataObject.Refresh();
			}
			PopulateDatabasesCollection(_SmoMetadataObject.Connection, _SmoMetadataObject);
			_CollationInfo = Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo.Default;
			Microsoft.SqlServer.Management.Smo.Database database = null;
			try
			{
				database = ((_SmoMetadataObject.ServerType != DatabaseEngineType.SqlAzureDatabase) ? _SmoMetadataObject.Databases[C_MasterDatabaseName] : null);
				if (database != null)
				{
					string collation = database.Collation;
					_CollationInfo = Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo.GetCollationInfo(collation);
				}
			}
			catch (ConnectionFailureException)
			{
			}
			Microsoft.SqlServer.Management.SqlParser.MetadataProvider.DatabaseCollection databaseCollection = new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.DatabaseCollection((database == null) ? 1 : _SmoMetadataObject.Databases.Count, _CollationInfo);
			if (database != null)
			{
				foreach (Microsoft.SqlServer.Management.Smo.Database database2 in _SmoMetadataObject.Databases)
				{
					databaseCollection.Add(new SmoMetaDatabase(database2, this));
				}
			}
			else
			{
				Microsoft.SqlServer.Management.Smo.Database smoMetadataObject3 = _SmoMetadataObject.Databases[_SmoMetadataObject.Connection.Database];
				databaseCollection.Add(new SmoMetaDatabase(smoMetadataObject3, this));
			}
			SetDatabases(databaseCollection);
			_Credentials = new CredentialCollectionHelperI(this);
			_Logins = new LoginCollectionHelperI(this);
			_Triggers = new TriggerCollectionHelperI(this);
		// }
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - SmoMetaServer
	// =========================================================================================================


	private const string C_MasterDatabaseName = "master";


	#endregion Constants





	// =========================================================================================================
	#region Fields - SmoMetaServer
	// =========================================================================================================


	// private readonly Microsoft.SqlServer.Management.Smo.Server _SmoMetadataObject;
	private readonly LsbSmoServer _SmoMetadataObject;


	private readonly bool _IsConnected;

	private IMetadataCollection<IDatabase> _Databases;

	private readonly CredentialCollectionHelperI _Credentials;

	private readonly LoginCollectionHelperI _Logins;

	private readonly TriggerCollectionHelperI _Triggers;

	private Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo _CollationInfo;

	private IDatabase _MasterDatabase;

	// private readonly object _SyncRoot = new object();


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaServer
	// =========================================================================================================


	public bool IsConnected => _IsConnected;

	public IDatabaseObject Parent => null;

	public bool IsSystemObject => false;

	public bool IsVolatile => false;

	public string Name => _SmoMetadataObject.Name;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo CollationInfo => _CollationInfo;

	public IMetadataCollection<ICredential> Credentials => _Credentials.MetadataCollection;

	public IMetadataCollection<IDatabase> Databases => _Databases;

	public IMetadataCollection<ILogin> Logins => _Logins.MetadataCollection;

	public IMetadataCollection<IServerDdlTrigger> Triggers => _Triggers.MetadataCollection;

	public IDatabase MasterDatabase
	{
		get
		{
			Diag.ThrowIfInstanceNull(_MasterDatabase, typeof(IDatabase));
			return _MasterDatabase;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaServer
	// =========================================================================================================


	public T Accept<T>(IDatabaseObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

	public T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		return Accept((IDatabaseObjectVisitor<T>)visitor);
	}




	// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
	// Microsoft.SqlServer.Management.SmoMetadataProvider.Utils
	public static IDatabase GetDatabase(IServer server, string databaseName)
	{
		return server.Databases[databaseName];
	}



	public void RefreshDatabaseList()
	{
		if (!IsConnected)
			Diag.ThrowException(new ConnectionException("Must be in connected mode."));

		// Evs.Trace(GetType(), nameof(RefreshDatabaseList));

		// using Microsoft.SqlServer.Diagnostics.STrace.MethodContext methodContext = Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.GetMethodContext("RefreshDatabaseList");

		IDbConnection connectionContext = _SmoMetadataObject.Connection;
		LsbSmoServer server = new (connectionContext);
		server.SetDefaultInitFields(SmoConfig.SmoInitFields.Database.Type, _SmoMetadataObject.EngineEdition, SmoConfig.SmoInitFields.Database.Safe);
		PopulateDatabasesCollection(connectionContext, server);

		// Evs.Trace(GetType(), nameof(RefreshDatabaseList), $"Found {server.Databases.Count} databases on server {server.Name}");

		Microsoft.SqlServer.Management.Smo.DatabaseCollection databases = _SmoMetadataObject.Databases;
		Microsoft.SqlServer.Management.Smo.DatabaseCollection databases2 = server.Databases;
		bool flag = false;
		if (databases.Count == databases2.Count)
		{
			foreach (Microsoft.SqlServer.Management.Smo.Database item in databases2)
			{
				Microsoft.SqlServer.Management.Smo.Database database2 = databases[item.Name];

				// Evs.Trace(GetType(), nameof(RefreshDatabaseList), $"Checking IsAccessible for {item.Name} and {(database2 == null ? "<none>" : database2.Name)}");

				if (database2 == null || Cmd.GetPropertyValue(item, "IsAccessible", defaultValue: true) != Cmd.GetPropertyValue(database2, "IsAccessible", defaultValue: true))
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}

		// Evs.Trace(GetType(), nameof(RefreshDatabaseList), "Database collection has changed");

		Microsoft.SqlServer.Management.SqlParser.MetadataProvider.DatabaseCollection databaseCollection = new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.DatabaseCollection(databases2.Count, _CollationInfo);
		_SmoMetadataObject.SetDefaultInitFields(SmoConfig.SmoInitFields.Database.Type, (DatabaseEngineEdition)connectionContext.GetEngineEdition(), SmoConfig.SmoInitFields.Database.Safe);

		foreach (SmoMetaDatabase database7 in (IMetadataCollection<SmoMetaDatabase>)Databases)
		{
			string name = database7.Name;
			Microsoft.SqlServer.Management.Smo.Database database4 = databases[name];
			Microsoft.SqlServer.Management.Smo.Database database5 = databases2[name];
			Diag.ThrowIfInstanceNull(database4, typeof(Microsoft.SqlServer.Management.Smo.Database));
			// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.Assert(database4 != null, "Bind Assert", "curSmoDatabase != null");
			if (database5 != null)
			{
				// Evs.Trace(GetType(), nameof(RefreshDatabaseList), $"Checking IsAccessible for {database5.Name} and {database4.Name}");

				if (Cmd.GetPropertyValue(database4, "IsAccessible", defaultValue: true) == Cmd.GetPropertyValue(database5, "IsAccessible", defaultValue: true))
				{
					databaseCollection.Add(database7);
					continue;
				}
				database4.Refresh();
				databaseCollection.Add(new SmoMetaDatabase(database4, this));
			}
		}

		// Evs.Trace(GetType(), nameof(RefreshDatabaseList), "Refreshing Smo database collection");

		TryRefreshSmoCollection(databases, SmoConfig.SmoInitFields.Database);
		if (databases2.Count == databases.Count)
			Diag.ThrowException(new ArgumentException("Number of databases must match that of the latest snapshot"));
		// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.Assert(databases2.Count == databases.Count, "Bind Assert", "Number of databases must match that of the latest snapshot!");

		if (databases.Count > databaseCollection.Count)
		{
			foreach (Microsoft.SqlServer.Management.Smo.Database item2 in databases)
			{
				if (!databaseCollection.Contains(item2.Name))
				{
					databaseCollection.Add(new SmoMetaDatabase(item2, this));
				}
			}
		}
		SetDatabases(databaseCollection);
	}

	private static void PopulateDatabasesCollection(IDbConnection serverConnection, LsbSmoServer newServer)
	{
		if ((DatabaseEngineType)serverConnection.GetEngineEdition() == DatabaseEngineType.SqlAzureDatabase)
		{
			string value = ((!string.IsNullOrEmpty(serverConnection.Database)) ? serverConnection.Database : ((!string.IsNullOrEmpty(serverConnection.Database)) ? serverConnection.Database : C_MasterDatabaseName));
			newServer.Databases.ClearAndInitialize($"[@Name='{Microsoft.SqlServer.Management.Sdk.Sfc.Urn.EscapeString(value)}']", null);
		}
		else
		{
			newServer.Databases.Refresh();
		}
	}

	public void TryRefreshSmoCollection(SmoCollectionBase collection, SmoConfig.SmoInitFields initFields)
	{
		Diag.ThrowIfInstanceNull(collection, typeof(SmoCollectionBase));

		// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.Assert(collection != null, "MetadataProvider Assert", "collection != null");
		// Evs.Trace(GetType(), nameof(TryRefreshSmoCollection));
		// using Microsoft.SqlServer.Diagnostics.STrace.MethodContext methodContext = Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.GetMethodContext("TryRefreshSmoCollection");

		if (!IsConnected)
		{
			return;
		}
		try
		{
			if (initFields != null)
			{
				try
				{
					_SmoMetadataObject.SetDefaultInitFields(initFields.Type, (DatabaseEngineEdition)_SmoMetadataObject.Connection.GetEngineEdition(), initFields.Optimized);
					collection.Refresh();
					return;
				}
				catch (Exception)
				{
					_SmoMetadataObject.SetDefaultInitFields(initFields.Type, _SmoMetadataObject.EngineEdition, initFields.Safe);
					collection.Refresh();
					return;
				}
			}
			collection.Refresh();
		}
		catch (Microsoft.SqlServer.Management.Sdk.Sfc.InvalidVersionEnumeratorException ex2)
		{
			Diag.Ex(ex2);
		}
		catch (UnsupportedVersionException)
		{
		}
		catch (Exception ex4)
		{
			Diag.Ex(ex4);
		}
	}

	private void SetDatabases(IMetadataCollection<IDatabase> collection)
	{
		Diag.ThrowIfInstanceNull(collection, typeof(IMetadataCollection<IDatabase>));
		// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.Assert(collection != null, "MetadataProvider Assert {0}", "collection != null");
		IDatabase database = collection[C_MasterDatabaseName];
		if (database == null)
		{
			database = SmoMetaSmoMetadataFactory.Instance.Database.CreateEmptyDatabase(this, C_MasterDatabaseName, _CollationInfo, isSystemObject: true);
			collection = Collection<IDatabase>.Merge(Collection<IDatabase>.CreateOrderedCollection(_CollationInfo, database), collection);
		}
		Diag.ThrowIfInstanceNull(database, typeof(IDatabase));
		// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.Assert(database != null, "MetadataProvider Assert {0}", "masterDb != null");
		if (collection[C_MasterDatabaseName] != database)
			Diag.ThrowException(new ArgumentException($"MetadataProvider Assert collection[MasterDatabaseName] == masterDb"));
		// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.Assert(collection[C_MasterDatabaseName] == database, "MetadataProvider Assert {0}", "collection[MasterDatabaseName] == masterDb");
		_Databases = collection;
		_MasterDatabase = database;
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - SmoMetaServer
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: CollectionHelper<T, S>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private abstract class CollectionHelperI<T, S> : UnorderedCollectionHelperBaseI<T, S> where T : class, IServerOwnedObject where S : NamedSmoObject
	{
		protected readonly SmoMetaServer _Server;

		protected override SmoMetaServer Server => _Server;

		public CollectionHelperI(SmoMetaServer server)
		{
			Diag.ThrowIfInstanceNull(server, typeof(SmoMetaServer));
			// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.Assert(server != null, "SmoMetadataProvider Assert", "server != null");
			_Server = server;
		}

		protected override Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo()
		{
			return _Server._CollationInfo;
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: CredentialCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private class CredentialCollectionHelperI : CollectionHelperI<ICredential, Microsoft.SqlServer.Management.Smo.Credential>
	{
		public CredentialCollectionHelperI(SmoMetaServer server)
			: base(server)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.Credential> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<Microsoft.SqlServer.Management.Smo.Credential>(_Server, _Server._SmoMetadataObject.Credentials);
		}

		protected override IMutableMetadataCollection<ICredential> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.CredentialCollection(initialCapacity, collationInfo);
		}

		protected override ICredential CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Credential smoObject)
		{
			return new SmoMetaCredential(smoObject, _Server);
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: LoginCollectionHelperI.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private class LoginCollectionHelperI : CollectionHelperI<ILogin, Microsoft.SqlServer.Management.Smo.Login>
	{
		public LoginCollectionHelperI(SmoMetaServer server)
			: base(server)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.Login> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<Microsoft.SqlServer.Management.Smo.Login>(_Server, _Server._SmoMetadataObject.Logins);
		}

		protected override IMutableMetadataCollection<ILogin> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.LoginCollection(initialCapacity, collationInfo);
		}

		protected override ILogin CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Login smoObject)
		{
			return AbstractSmoMetaLogin.CreateLogin(smoObject, _Server);
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: TriggerCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private class TriggerCollectionHelperI : CollectionHelperI<IServerDdlTrigger, Microsoft.SqlServer.Management.Smo.ServerDdlTrigger>
	{
		public TriggerCollectionHelperI(SmoMetaServer server)
			: base(server)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.ServerDdlTrigger> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<Microsoft.SqlServer.Management.Smo.ServerDdlTrigger>(_Server, _Server._SmoMetadataObject.Triggers);
		}

		protected override IMutableMetadataCollection<IServerDdlTrigger> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.ServerDdlTriggerCollection(initialCapacity, collationInfo);
		}

		protected override IServerDdlTrigger CreateMetadataObject(Microsoft.SqlServer.Management.Smo.ServerDdlTrigger smoObject)
		{
			return new SmoMetaServerDdlTrigger(smoObject, _Server);
		}
	}


	#endregion Nested types
}
