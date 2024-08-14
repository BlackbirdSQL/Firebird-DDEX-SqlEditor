// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.SmoMetadataProviderProvider

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Sys.Enums;
using Microsoft.SqlServer.Management.SqlParser.Binder;
using Microsoft.SqlServer.Management.SqlParser.Common;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.LanguageExtension.Model;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC`")]


/// <summary>
/// Placeholder. Under development.
/// </summary>
public class LsbMetadataProviderProvider : AbstractMetadataProviderProvider
{
	protected LsbMetadataProviderProvider(IBsModelCsb ci, string cacheKey)
	{
		IsInitialized = false;
		_ = IsInitialized;
		CacheKey = cacheKey;
		DatabaseEngineType = EnServerType.Default;
		MdlCsb = ci.Copy();

		base.BuildEvent.Reset();
		new InitializeMetadataProviderDelegate(Initialize).BeginInvoke(null, null);
	}





	internal class Cache
	{
		private Cache()
		{
		}

		internal static Cache Instance
		{
			get
			{
				LazyInitializer.EnsureInitialized(ref _Instance, () => new Cache());
				return _Instance;
			}
		}




		private readonly object _LockLocal = new();
		private static Cache _Instance;

		private Dictionary<string, LsbMetadataProviderProvider> _CacheTable = null;



		private Dictionary<string, LsbMetadataProviderProvider> CacheTable => _CacheTable ??= [];



		internal LsbMetadataProviderProvider Acquire(QueryManager qryMgr)
		{
			LsbMetadataProviderProvider value = null;

			lock (_LockLocal)
			{
				string qryMgrKey = GetKeyForQueryManager(qryMgr);

				if (qryMgrKey != null)
				{
					if (_CacheTable == null || !_CacheTable.TryGetValue(qryMgrKey, out value))
					{
						value = new LsbMetadataProviderProvider(qryMgr.Strategy.LiveMdlCsb, qryMgrKey);
						CacheTable.Add(qryMgrKey, value);
					}

					value.ReferenceCount++;
				}
			}

			return value;
		}



		internal void Release(LsbMetadataProviderProvider mpp)
		{
			lock (_LockLocal)
			{
				mpp.ReferenceCount--;
				string cacheKey = mpp.CacheKey;

				if (_CacheTable != null && _CacheTable.ContainsKey(cacheKey) && mpp.ReferenceCount == 0)
				{
					_CacheTable.Remove(cacheKey);

					Action action = mpp.Dispose;

					action.BeginInvoke(null, null);
				}
			}
		}

		internal static string GetKeyForQueryManager(QueryManager qryMgr)
		{
			if (!qryMgr.IsConnected)
				return null;

			string text = null;

			if (qryMgr.Strategy is ConnectionStrategy { MdlCsb: IBsCsb csb })
			{
				text = csb.DatasetKey;
			}

			return text;
		}
	}


	private delegate void InitializeMetadataProviderDelegate();

	private readonly object _LockLocal = new();
	private readonly SemaphoreSlim _LockSem = new(1);

	// private Dictionary<string, CatalogStamp> _CatalogStamps = new Dictionary<string, CatalogStamp>();

	// private Type _DspType;

	public static readonly bool CheckForDatabaseChangesAfterQueryExecution = true;

	public ImmutableHashSet<string> _DatabasesToCheckForDrift = null;



	private IBsCsb MdlCsb { get; set; }

	private DbConnection DriftDetectionConnection { get; set; }

	private IDbConnection ServerConnection { get; set; }

	private Version ServerVersion { get; set; }

	private EnServerType DatabaseEngineType { get; set; }

	private bool IsInitialized { get; set; }

	private int ReferenceCount { get; set; }

	private string CacheKey { get; set; }

	public override string Moniker => MdlCsb?.Moniker;

	public bool IsCloudConnection => DatabaseEngineType == EnServerType.Default;

	protected override bool AssertInDestructor => false;

	public void AddDatabaseToDriftDetectionSet(string moniker)
	{
		_DatabasesToCheckForDrift ??= [];

		ThreadingTools.ApplyChangeOptimistically(ref _DatabasesToCheckForDrift, (databases) => databases.Add(moniker));
	}


	private void Initialize()
	{
		_ = InitializeAsync();
	}

	private async Task InitializeAsync()
	{
		await _LockSem.WaitAsync();

		try
		{
			/*
			Csb csb = [];
			FbConnection connection = null;

			try
			{
				ConnectionManager.PopulateConnectionStringBuilder(csb, MdlCsb);

				csb.Pooling = false;
				connection = NativeDb.CreateDbConnection(csb.ToString());

				await connection.OpenAsync().ConfigureAwait(continueOnCapturedContext: false);

				// SetLockAndCommandTimeout(connection);

				// ServerConnection serverConnection = new ServerConnection(connection);
				DatabaseEngineType = csb.ServerType;
			}
			finally
			{
				connection?.Dispose();
			}
			*/

			InitializeMetadataConnection();

			if (ShouldEnableIntellisense())
			{
				// _DspType = DatabaseSchemaProvider.SafeGetCompatibleDatabaseSchemaProviderType("sql", GetMetadataConnectionString()); 
				CreateMetadataProvider();
			}
			IsInitialized = true;
		}
		finally
		{
			_LockSem.Release();
		}
	}

	private void InitializeMetadataConnection()
	{
		lock (_LockLocal)
		{
			DisposeMetadataConnection();
			Csb metadataConnectionStringBuilder = GetMetadataConnectionStringBuilder();

			ServerConnection = NativeDb.CreateDbConnection(metadataConnectionStringBuilder.ConnectionString);

			// if (IsCloudConnection)
			//	ServerConnection.DatabaseName = metadataConnectionStringBuilder.InitialCatalog;
			// ServerConnection.ConnectionString = metadataConnectionStringBuilder.ToString();

			ServerConnection.Open();

			// ConnectionHelperUtils.SetLockAndCommandTimeout(ServerConnection.SqlConnectionObject);
			ServerVersion = ServerConnection.ParseServerVersion();
			DatabaseEngineType = metadataConnectionStringBuilder.IsServerConnection ? EnServerType.Default : EnServerType.Embedded;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
	private void InitializeDriftDetectionConnection()
	{
		lock (_LockLocal)
		{
			DisposeDriftDetectionConnection();
			string metadataConnectionString = GetMetadataConnectionString();
			DriftDetectionConnection = (DbConnection)NativeDb.CreateDbConnection(metadataConnectionString);
			DriftDetectionConnection.Open();
			// ConnectionHelperUtils.SetLockAndCommandTimeout(DriftDetectionConnection);
		}
	}

	public override ParseOptions CreateParseOptions()
	{
		TransactSqlVersion transactSqlVersion = TransactSqlVersion.Current;
		DatabaseCompatibilityLevel compatibilityLevel = DatabaseCompatibilityLevel.Current;
		if (Monitor.TryEnter(_LockLocal))
		{
			try
			{
				if (ServerVersion != null)
				{
					transactSqlVersion = TransactSqlVersion.Current; // GetTransactSqlVersion(ServerVersion, IsCloudConnection);
					compatibilityLevel = DatabaseCompatibilityLevel.Current; // GetDatabaseCompatibilityLevel(ServerVersion, IsCloudConnection);
				}
			}
			finally
			{
				Monitor.Exit(_LockLocal);
			}
		}
		return new ParseOptions(PersistentSettings.EditorContextBatchSeparator, isQuotedIdentifierSet: true, compatibilityLevel, transactSqlVersion);
	}

	internal void CheckForDatabaseChanges()
	{
		if (ShouldEnableIntellisense())
		{
			object localfunc()
			{
				CheckForDatabaseChangesAndRecreateProvider();
				return null;
			}

			base.BinderQueue.EnqueueRecomputeMetadataAction(localfunc);
		}
	}

	private void CreateMetadataProvider()
	{
		if (!ShouldEnableIntellisense())
		{
			return;
		}

		lock (_LockLocal)
		{
			try
			{
				// Tracer.Trace(GetType(), "CreateMetadataProvider()", "Before Reset()");
				base.BuildEvent.Reset();
				// Tracer.Trace(GetType(), "CreateMetadataProvider()", "After Reset()");

				if (ServerConnection.State != ConnectionState.Open)
				{
					InitializeMetadataConnection();
				}
				// base.MetadataProvider = LsbMetadataProvider.CreateConnectedProvider(ServerConnection);
				base.Binder = BinderProvider.CreateBinder(base.MetadataProvider);
				// Tracer.Trace(GetType(), "CreateMetadataProvider()", "After metadata provider is created");
			}
			catch (Exception)
			{
			}
			finally
			{
				base.BuildEvent.Set();
			}
		}
	}

	private bool ShouldEnableIntellisense()
	{
		if (!LanguageExtensionPackage.Instance.LanguageService.Prefs.EnableAzureIntellisense)
		{
			return !IsCloudConnection;
		}
		return true;
	}

	private void CheckForDatabaseChangesAndRecreateProvider()
	{
		// We're unlikely to ever implement any server database drift detection. The only drift detection
		// there will be is changes to a connection's stamp versus the RunningConnectionTable. This is
		// local and affects only changes within a user session.
		/*
		lock (_LockLocal)
		{
			if (base.IsDisposed || !IsInitialized)
			{
				return;
			}
			try
			{
				Dictionary<string, CatalogStamp> dictionary = new Dictionary<string, CatalogStamp>(_catalogStamps.Count);
				bool flag = false;
				if (base.Binder == null || base.MetadataProvider == null)
				{
					CreateMetadataProvider();
					flag = true;
				}
				ImmutableHashSet<string> databasesToCheckForDrift = _databasesToCheckForDrift;
				List<string> list = new List<string>(databasesToCheckForDrift.Count);
				foreach (string item in databasesToCheckForDrift)
				{
					if (DriftDetectionConnection != null && DriftDetectionConnection.State == ConnectionState.Closed)
					{
						DriftDetectionConnection.Open();
					}
					if (DriftDetectionConnection == null || DriftDetectionConnection.State != ConnectionState.Open)
					{
						InitializeDriftDetectionConnection();
					}
					CatalogStamp value = null;
					try
					{
						if (flag)
						{
							DriftDetectionConnection.ChangeDatabase(item);
							dictionary[item] = DatabaseCatalog.CreateBaselineStamp(DriftDetectionConnection, DriftDetectionConnection, _dspType, DriftResolution.Database);
						}
						else if (_catalogStamps.TryGetValue(item, out value))
						{
							DriftDetectionConnection.ChangeDatabase(item);
							DatabaseCatalog.DetectChanges(DriftDetectionConnection, DriftDetectionConnection, value, out var updated, out var delta);
							dictionary[item] = updated;
							if (delta.HasChanges)
							{
								CreateMetadataProvider();
								flag = true;
							}
						}
						else
						{
							CreateMetadataProvider();
							flag = true;
							DriftDetectionConnection.ChangeDatabase(item);
							dictionary[item] = DatabaseCatalog.CreateBaselineStamp(DriftDetectionConnection, DriftDetectionConnection, _dspType, DriftResolution.Database);
						}
					}
					catch (Exception e)
					{
						list.Add(item);
						TraceUtils.LogExCatch(GetType(), e);
					}
				}
				foreach (string dbToRemove in list)
				{
					if (dictionary.ContainsKey(dbToRemove))
					{
						dictionary.Remove(dbToRemove);
					}
					ThreadingTools.ApplyChangeOptimistically(ref _databasesToCheckForDrift, (ImmutableHashSet<string> databases) => databases.Remove(dbToRemove));
				}
				_catalogStamps = dictionary;
			}
			catch (Exception e2)
			{
				TraceUtils.LogExCatch(GetType(), e2);
			}
			finally
			{
				if (DriftDetectionConnection != null)
				{
					DriftDetectionConnection.Close();
				}
			}
		}
		*/
	}

	private string GetMetadataConnectionString()
	{
		return GetMetadataConnectionStringBuilder().ToString();
	}

	private Csb GetMetadataConnectionStringBuilder()
	{
		Csb csb = new(MdlCsb.ConnectionString, false);
		return csb;
	}

	private void DisposeConnections()
	{
		lock (_LockLocal)
		{
			DisposeDriftDetectionConnection();
			DisposeMetadataConnection();
			MdlCsb?.Dispose();
			MdlCsb = null;
		}
	}

	private void DisposeMetadataConnection()
	{
		lock (_LockLocal)
		{
			if (ServerConnection != null)
			{
				ServerConnection.Close();
				ServerConnection.Dispose();
				ServerConnection = null;
			}
		}
	}

	private void DisposeDriftDetectionConnection()
	{
		lock (_LockLocal)
		{
			if (DriftDetectionConnection != null)
			{
				DriftDetectionConnection.Close();
				DriftDetectionConnection.Dispose();
				DriftDetectionConnection = null;
			}
		}
	}


	/*
	private static DatabaseCompatibilityLevel GetDatabaseCompatibilityLevel(Version serverVersion, bool isCloudConnection)
	{
		return DatabaseCompatibilityLevel.Current;
	}

	private static TransactSqlVersion GetTransactSqlVersion(Version serverVersion, bool isCloudConnection)
	{
		return TransactSqlVersion.Current;
	}
	*/


	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			lock (_LockLocal)
			{
				base.Dispose(disposing);
				DisposeConnections();
			}
		}
	}

}
