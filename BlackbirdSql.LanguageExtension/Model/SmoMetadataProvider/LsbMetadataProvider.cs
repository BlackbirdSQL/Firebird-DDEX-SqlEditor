// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.SmoMetadataProvider
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using BlackbirdSql.LanguageExtension.Model.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.Win32;
using static BlackbirdSql.LanguageExtension.Ctl.Config.SmoConfig;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbMetadataProvider Class
//
/// <summary>
/// Language service IMetadataProvider implementation.
/// </summary>
// =========================================================================================================
public abstract class LsbMetadataProvider : AbstractMetadataProvider
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbSource
	// ---------------------------------------------------------------------------------


	private LsbMetadataProvider(/* Microsoft.SqlServer.Management.Smo.Server */ LsbSmoServer server, bool isConnected)
	// : base(SmoBuiltInFunctionLookup.Instance, SmoCollationLookup.Instance, SmoSystemDataTypeLookup.Instance, SmoMetadataFactory.Instance)
	{
		_SmoServer = server;
		_Server = new(_SmoServer, isConnected);

	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbMetadataProvider
	// =========================================================================================================


	// private readonly Microsoft.SqlServer.Management.Smo.Server _SmoServer;
	private readonly LsbSmoServer _SmoServer;
	// private readonly Microsoft.SqlServer.Management.SmoMetadataProvider.Server _Server;
	private readonly LsbMetadataServer _Server;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbMetadataProvider
	// =========================================================================================================


	public override IServer Server => _Server;

	// public Microsoft.SqlServer.Management.Smo.Server SmoServer => _SmoServer;
	public LsbSmoServer SmoServer => _SmoServer;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbMetadataProvider
	// =========================================================================================================


	public static LsbMetadataProvider CreateConnectedProvider(IDbConnection connection)
	{
		return ConnectedMetadataProviderI.Create(connection);
	}

	public static LsbMetadataProvider CreateConnectedProvider(IDbConnection connection, int refreshDbListMillisecond)
	{
		return ConnectedMetadataProviderI.Create(connection, refreshDbListMillisecond);
	}

	public static LsbMetadataProvider CreateDisconnectedProvider(/* Microsoft.SqlServer.Management.Smo.Server */ LsbSmoServer server)
	{
		return DisconnectedLsbMetadataProvider.Create(server);
	}


	#endregion Methods





	// =========================================================================================================
	#region								Nested types - LsbMetadataProvider
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// ConnectedMetadataProviderI Sub-class
	/// </summary>
	// ---------------------------------------------------------------------------------
	private sealed class ConnectedMetadataProviderI : LsbMetadataProvider
	{
		private static int DefaultRefreshDbListMillisecond;

		private readonly IDbConnection _ServerConnection;
		private readonly int _RefreshDbListMillisecond;
		private int _LastRefreshTimestamp;

		private const string C_RegPath = "Software\\BlackbirdSql\\Firebird-SQL Language Service\\ConnectedMetadataProvider";

		public override MetadataProviderEventHandler BeforeBindHandler => OnBeforeBind;

		public override MetadataProviderEventHandler AfterBindHandler => OnAfterBind;

		static ConnectedMetadataProviderI()
		{
			DefaultRefreshDbListMillisecond = 120000;
			try
			{
				using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(C_RegPath);
				if (registryKey != null)
				{
					DefaultRefreshDbListMillisecond = Convert.ToInt32(registryKey.GetValue("DbRefreshDelayMs", DefaultRefreshDbListMillisecond));
				}
			}
			catch /* (Exception ex) */
			{
				// TraceHelper.TraceContext.TraceCatch(ex);
			}
		}

		public static ConnectedMetadataProviderI Create(IDbConnection connection)
		{
			// return null;

			// TBC: ConnectedMetadataProviderI

			if (connection == null)
				throw new ArgumentNullException("connection");

			return Create(connection, /* (connection.DatabaseEngineType == DatabaseEngineType.SqlAzureDatabase) ? int.MaxValue : */ DefaultRefreshDbListMillisecond);
		}

		public static ConnectedMetadataProviderI Create(IDbConnection connection, int refreshDbListMillisecond)
		{
			// return null;

			// TBC: ConnectedMetadataProviderI

			if (connection == null)
				throw new ArgumentNullException("connection");

			if (refreshDbListMillisecond < 0)
				throw new ArgumentOutOfRangeException("refreshDbListMillisecond", "Value must be >= 0!");

			// Microsoft.SqlServer.Management.Smo.Server server = new Microsoft.SqlServer.Management.Smo.Server(connection);
			LsbSmoServer server = new(connection);

			return new ConnectedMetadataProviderI(connection, server, refreshDbListMillisecond);

		}

		private ConnectedMetadataProviderI(IDbConnection connection, LsbSmoServer server, int refreshDbListMillisecond)
			: base(server, isConnected: true)
		{
			Evs.Trace(typeof(ConnectedMetadataProviderI), ".ctor");

			Diag.ThrowIfInstanceNull(connection, typeof(IDbConnection));
			// TraceHelper.TraceContext.Assert(connection != null, "SmoMetadataProvider Assert", "connection != null");
			if (refreshDbListMillisecond < 0)
				Diag.ThrowException(new ArgumentException("refreshDbListMillisecond < 0."));
			// TraceHelper.TraceContext.Assert(refreshDbListMillisecond >= 0, "SmoMetadataProvider Assert", "refreshDbListMillisecond >= 0");
			_ServerConnection = connection;
			_ = _ServerConnection;

			_RefreshDbListMillisecond = refreshDbListMillisecond;
			_ = _RefreshDbListMillisecond;
			_LastRefreshTimestamp = refreshDbListMillisecond; // For error suppression for now.
			_ = _LastRefreshTimestamp;
		}

		private void OnBeforeBind(object sender, MetadataProviderEventArgs e)
		{
			/*
			using Microsoft.SqlServer.Diagnostics.STrace.MethodContext methodContext = TraceHelper.TraceContext.GetMethodContext("OnBeforeBind");
			int tickCount = Environment.TickCount;
			if (tickCount - _LastRefreshTimestamp <= _RefreshDbListMillisecond)
			{
				return;
			}
			lock (m_server)
			{
				if (tickCount <= _LastRefreshTimestamp)
				{
					return;
				}
				using Microsoft.SqlServer.Diagnostics.STrace.MethodContext methodContext2 = methodContext.GetActivityContext("Refresh database list");
				try
				{
					_MetadataServer.RefreshDatabaseList();
				}
				catch (Exception ex)
				{
					methodContext2.TraceError("Failed to refresh database list due to a an exception.");
					methodContext2.TraceCatch(ex);
				}
				finally
				{
					_LastRefreshTimestamp = tickCount;
				}
			}
			*/
		}

		private void OnAfterBind(object sender, MetadataProviderEventArgs e)
		{
			// _ServerConnection.Disconnect();
		}
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// DisconnectedLsbMetadataProvider Sub-class
	/// </summary>
	// ---------------------------------------------------------------------------------
	private sealed class DisconnectedLsbMetadataProvider : LsbMetadataProvider
	{
		public override MetadataProviderEventHandler BeforeBindHandler => null;

		public override MetadataProviderEventHandler AfterBindHandler => null;

		public static DisconnectedLsbMetadataProvider Create(LsbSmoServer server)
		{
			if (server == null)
			{
				throw new ArgumentNullException("server");
			}
			return new DisconnectedLsbMetadataProvider(server);
		}

		private DisconnectedLsbMetadataProvider(LsbSmoServer server)
			: base(server, isConnected: false)
		{
			Evs.Trace(typeof(DisconnectedLsbMetadataProvider), ".ctor");
		}
	}


	#endregion Nested types

}
