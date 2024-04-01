// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.SmoMetadataProviderProvider
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core.Model;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using Microsoft.SqlServer.Management.SqlParser.Binder;
using Microsoft.SqlServer.Management.SqlParser.Common;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.LanguageExtension.Model;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]
[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC`")]
[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "TBC")]


/// <summary>
/// Placeholder. Under development.
/// </summary>
public class LsbMetadataProviderProvider : AbstractMetadataProviderProvider
{
	protected LsbMetadataProviderProvider(ConnectionPropertyAgent uici, string cacheKey)
	{
		IsInitialized = false;
		CacheKey = cacheKey;
		DatabaseEngineType = FbServerType.Default;
		ConnectionInfo = (ConnectionPropertyAgent)uici.Copy();

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
						value = new LsbMetadataProviderProvider(qryMgr.ConnectionStrategy.ConnectionInfo, qryMgrKey);
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

		internal static string GetKeyForQueryManager(QueryManager value)
		{
			string text = null;
			return text;
		}
	}


	private delegate void InitializeMetadataProviderDelegate();

	private readonly object _LockLocal = new();
	private SemaphoreSlim _LockSem = new SemaphoreSlim(1);

	// private Dictionary<string, CatalogStamp> _CatalogStamps = new Dictionary<string, CatalogStamp>();

	private Type _DspType;

	public static readonly bool CheckForDatabaseChangesAfterQueryExecution = true;

	public ImmutableHashSet<string> _DatabasesToCheckForDrift = null;



	private ConnectionPropertyAgent ConnectionInfo { get; set; }

	private FbConnection DriftDetectionConnection { get; set; }

	private FbConnection ServerConnection { get; set; }

	private Version ServerVersion { get; set; }

	private FbServerType DatabaseEngineType { get; set; }

	private bool IsInitialized { get; set; }

	private int ReferenceCount { get; set; }

	private string CacheKey { get; set; }

	public override string DatabaseName => null;

	public bool IsCloudConnection => DatabaseEngineType == FbServerType.Default;

	protected override bool AssertInDestructor => false;

	public void AsyncAddDatabaseToDriftDetectionSet(string databaseName)
	{
		_DatabasesToCheckForDrift ??= ImmutableHashSet<string>.Empty;

		ThreadingTools.ApplyChangeOptimistically(ref _DatabasesToCheckForDrift, (databases) => databases.Add(databaseName));
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
			CsbAgent csb = [];
			FbConnection connection = null;

			try
			{
				SqlConnectionStrategy.PopulateConnectionStringBuilder(csb, ConnectionInfo);

				csb.Pooling = false;
				connection = new FbConnection(csb.ToString());

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
			CsbAgent metadataConnectionStringBuilder = GetMetadataConnectionStringBuilder();

			ServerConnection = new (metadataConnectionStringBuilder.ConnectionString);

			// if (IsCloudConnection)
			//	ServerConnection.DatabaseName = metadataConnectionStringBuilder.InitialCatalog;
			// ServerConnection.ConnectionString = metadataConnectionStringBuilder.ToString();

			ServerConnection.Open();

			// ConnectionHelperUtils.SetLockAndCommandTimeout(ServerConnection.SqlConnectionObject);
			ServerVersion = FbServerProperties.ParseServerVersion(ServerConnection.ServerVersion);
			DatabaseEngineType = metadataConnectionStringBuilder.ServerType;
		}
	}

	private void InitializeDriftDetectionConnection()
	{
		lock (_LockLocal)
		{
			DisposeDriftDetectionConnection();
			string metadataConnectionString = GetMetadataConnectionString();
			DriftDetectionConnection = new (metadataConnectionString);
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
		return new ParseOptions("GO", isQuotedIdentifierSet: true, compatibilityLevel, transactSqlVersion);
	}

	internal void AsyncCheckForDatabaseChanges()
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

	private CsbAgent GetMetadataConnectionStringBuilder()
	{
		return null;
		/*
		UIConnectionInfo uIConnectionInfo = null;
		if (!IsCloudConnection)
		{
			uIConnectionInfo = ConnectionInfo.Copy();
			SqlServerConnectionService.SetDatabaseName(uIConnectionInfo, string.Empty);
		}
		else
		{
			uIConnectionInfo = ConnectionInfo;
		}
		SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
		SqlConnectionStrategy.PopulateConnectionStringBuilder(sqlConnectionStringBuilder, uIConnectionInfo);
		return sqlConnectionStringBuilder;
		*/
	}

	private void DisposeConnections()
	{
		lock (_LockLocal)
		{
			DisposeDriftDetectionConnection();
			DisposeMetadataConnection();
		}
	}

	private void DisposeMetadataConnection()
	{
		/*
		lock (_LockLocal)
		{
			if (ServerConnection != null)
			{
				ServerConnection.Disconnect();
				ServerConnection = null;
			}
		}
		*/
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

	private static DatabaseCompatibilityLevel GetDatabaseCompatibilityLevel(Version serverVersion, bool isCloudConnection)
	{
		if (isCloudConnection)
		{
			return DatabaseCompatibilityLevel.Version150;
		}
		return Math.Max(serverVersion.Major, 8) switch
		{
			8 => DatabaseCompatibilityLevel.Version80,
			9 => DatabaseCompatibilityLevel.Version90,
			10 => DatabaseCompatibilityLevel.Version100,
			11 => DatabaseCompatibilityLevel.Version110,
			12 => DatabaseCompatibilityLevel.Version120,
			13 => DatabaseCompatibilityLevel.Version130,
			14 => DatabaseCompatibilityLevel.Version140,
			15 => DatabaseCompatibilityLevel.Version150,
			_ => DatabaseCompatibilityLevel.Current,
		};
	}

	private static TransactSqlVersion GetTransactSqlVersion(Version serverVersion, bool isCloudConnection)
	{
		return TransactSqlVersion.Current;

		/*

		if (isCloudConnection)
		{
			return TransactSqlVersion.Version150;
		}
		switch (Math.Max(serverVersion.Major, 9))
		{
			case 9:
			case 10:
				return TransactSqlVersion.Version105;
			case 11:
				return TransactSqlVersion.Version110;
			case 12:
				return TransactSqlVersion.Version120;
			case 13:
				return TransactSqlVersion.Version130;
			case 14:
				return TransactSqlVersion.Version140;
			case 15:
				return TransactSqlVersion.Version150;
			default:
				return TransactSqlVersion.Current;
		}
		*/
	}

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
