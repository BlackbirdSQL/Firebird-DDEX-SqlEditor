// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.SmoMetadataProvider
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.Win32;



namespace BlackbirdSql.LanguageExtension.Model;

[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


/// <summary>
/// Placeholder. Under development.
/// </summary>
public abstract class LsbMetadataProvider : AbstractMetadataProvider
{
	private sealed class ConnectedLsbMetadataProvider : LsbMetadataProvider
	{
		private static int DefaultRefreshDbListMillisecond;

		private readonly IDbConnection m_serverConnection;
		private readonly int m_refreshDbListMillisecond;
		private int m_lastRefreshTimestamp;

		private const string RegPath = "Software\\BlackbirdSql\\Firebird-SQL Language Service\\ConnectedMetadataProvider";

		public override MetadataProviderEventHandler BeforeBindHandler => OnBeforeBind;

		public override MetadataProviderEventHandler AfterBindHandler => OnAfterBind;

		static ConnectedLsbMetadataProvider()
		{
			DefaultRefreshDbListMillisecond = 120000;
			try
			{
				using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\BlackbirdSql\\Firebird-SQL Language Service\\ConnectedMetadataProvider");
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

		public static ConnectedLsbMetadataProvider Create(IDbConnection connection)
		{
			return null;

			/*
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			return Create(connection, (connection.DatabaseEngineType == DatabaseEngineType.SqlAzureDatabase) ? int.MaxValue : DefaultRefreshDbListMillisecond);
			*/
		}

		public static ConnectedLsbMetadataProvider Create(IDbConnection connection, int refreshDbListMillisecond)
		{
			return null;

			/*
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			if (refreshDbListMillisecond < 0)
			{
				throw new ArgumentOutOfRangeException("refreshDbListMillisecond", "Value must be >= 0!");
			}
			Microsoft.SqlServer.Management.Smo.Server server = new Microsoft.SqlServer.Management.Smo.Server(connection);
			return new ConnectedSmoMetadataProvider(connection, server, refreshDbListMillisecond);
			*/
		}

		private ConnectedLsbMetadataProvider(IDbConnection connection, /* Microsoft.SqlServer.Management.Smo.Server */ object server, int refreshDbListMillisecond)
			: base(server, isConnected: true)
		{
			/*
			TraceHelper.TraceContext.Assert(connection != null, "SmoMetadataProvider Assert", "connection != null");
			TraceHelper.TraceContext.Assert(refreshDbListMillisecond >= 0, "SmoMetadataProvider Assert", "refreshDbListMillisecond >= 0");
			*/
			m_serverConnection = connection;
			_ = m_serverConnection;

			m_refreshDbListMillisecond = refreshDbListMillisecond;
			_ = m_refreshDbListMillisecond;
			m_lastRefreshTimestamp = refreshDbListMillisecond; // For error suppression for now.
			_ = m_lastRefreshTimestamp;
		}

		private void OnBeforeBind(object sender, MetadataProviderEventArgs e)
		{
			/*
			using Microsoft.SqlServer.Diagnostics.STrace.MethodContext methodContext = TraceHelper.TraceContext.GetMethodContext("OnBeforeBind");
			int tickCount = Environment.TickCount;
			if (tickCount - m_lastRefreshTimestamp <= m_refreshDbListMillisecond)
			{
				return;
			}
			lock (m_server)
			{
				if (tickCount <= m_lastRefreshTimestamp)
				{
					return;
				}
				using Microsoft.SqlServer.Diagnostics.STrace.MethodContext methodContext2 = methodContext.GetActivityContext("Refresh database list");
				try
				{
					m_server.RefreshDatabaseList();
				}
				catch (Exception ex)
				{
					methodContext2.TraceError("Failed to refresh database list due to a an exception.");
					methodContext2.TraceCatch(ex);
				}
				finally
				{
					m_lastRefreshTimestamp = tickCount;
				}
			}
			*/
		}

		private void OnAfterBind(object sender, MetadataProviderEventArgs e)
		{
			// m_serverConnection.Disconnect();
		}
	}

	private sealed class DisconnectedLsbMetadataProvider : LsbMetadataProvider
	{
		public override MetadataProviderEventHandler BeforeBindHandler => null;

		public override MetadataProviderEventHandler AfterBindHandler => null;

		public static DisconnectedLsbMetadataProvider Create(/* Microsoft.SqlServer.Management.Smo.Server */ object server)
		{
			if (server == null)
			{
				throw new ArgumentNullException("server");
			}
			return new DisconnectedLsbMetadataProvider(server);
		}

		private DisconnectedLsbMetadataProvider(/* Microsoft.SqlServer.Management.Smo.Server */ object server)
			: base(server, isConnected: false)
		{
		}
	}

	private readonly /* Microsoft.SqlServer.Management.Smo.Server */ object m_smoServer;

	private readonly /* Microsoft.SqlServer.Management.SmoMetadataProvider.Server */ object m_server;

	public override IServer Server => (IServer)m_server;

	public /* Microsoft.SqlServer.Management.Smo.Server */ object SmoServer => m_smoServer;

	private LsbMetadataProvider(/* Microsoft.SqlServer.Management.Smo.Server */ object server, bool isConnected)
		// : base(SmoBuiltInFunctionLookup.Instance, SmoCollationLookup.Instance, SmoSystemDataTypeLookup.Instance, SmoMetadataFactory.Instance)
	{
		m_smoServer = server;
		m_server = new(); // Microsoft.SqlServer.Management.SmoMetadataProvider.Server(m_smoServer, isConnected);

	}

	public static LsbMetadataProvider CreateConnectedProvider(IDbConnection connection)
	{
		return ConnectedLsbMetadataProvider.Create(connection);
	}

	public static LsbMetadataProvider CreateConnectedProvider(IDbConnection connection, int refreshDbListMillisecond)
	{
		return ConnectedLsbMetadataProvider.Create(connection, refreshDbListMillisecond);
	}

	public static LsbMetadataProvider CreateDisconnectedProvider(/* Microsoft.SqlServer.Management.Smo.Server */ object server)
	{
		return DisconnectedLsbMetadataProvider.Create(server);
	}
}
