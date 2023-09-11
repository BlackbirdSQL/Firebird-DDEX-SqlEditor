// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionEventTelemetryContext

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Model;

using FirebirdSql.Data.FirebirdClient;
using BlackbirdSql.Common.Enums;

namespace BlackbirdSql.Wpf.Model;


public class ConnectionEventTelemetryContext : ConnectionDialogTelemetryContext, IDisposable
{
    // private const string C_LocalDbPrefix = "(localdb)";

    // private AuthenticationTypes _authType;

    private IDbConnection _connection;

    private bool _isClonedConnection;

    private IBServerDefinition _serverDefinition;

    private int _historyTabCounter;

    private int _browseTabCounter;

    private int _recentConnectionCounter;

    private int _favoriteConnectionCounter;

    private int _errorOccuranceCounter;

    // private int _firewallRuleNeededCounter;

    private readonly ConcurrentDictionary<string, int> _connectionsAtEnd = new ConcurrentDictionary<string, int>();

    private bool _connectionPropertiesAdded;

    // private readonly object _LockObject = new object();


    public override void EnsureContextProperties()
    {
        SetCounterProperties();
        SetConnectionProperties();
    }

    public void SetConnectionInfo(IBServerDefinition serverDefinition, IDbConnection connection)
    {
        // _authType = authenticationTypes;
        _serverDefinition = serverDefinition;
        if (connection != null)
        {
            TryCloneConnection(connection);
        }
    }

    public void SetDialogInfo(HistoryPageViewModel historyPage, bool isHistory, bool success)
    {
        if (historyPage != null)
        {
            _recentConnectionCounter = historyPage.HasRecentConnections ? historyPage.RecentConnectionList.Count : 0;
            _favoriteConnectionCounter = historyPage.HasFavoriteConnections ? historyPage.FavoriteConnectionList.Count : 0;
        }
        SetProperty(TelemetryConstants.C_KeyTabOpen, isHistory ? TelemetryConstants.C_KeyHistoryTab : TelemetryConstants.C_KeyBrowseTab);
        SetProperty(TelemetryConstants.C_KeyConnectionSuccess, success);
    }

    public void UpdateNumberofConnectionsLoaded(ConnectionsLoadedEventArgs connectionsLoadedEventArgs)
    {
        try
        {
            if (connectionsLoadedEventArgs.ServerDefinition != null)
            {
                _connectionsAtEnd.AddOrUpdate(connectionsLoadedEventArgs.ServerDefinition.Key, connectionsLoadedEventArgs.NumberOfConnections, (string s, int i) => connectionsLoadedEventArgs.NumberOfConnections);
            }
        }
        catch (Exception ex)
        {
            UiTracer.TraceSource.TraceException(TraceEventType.Error, 14, ex, ex.Message,
                77, "ConnectionEventTelemetryContext.cs", "UpdateNumberofConnectionsLoaded");
        }
    }

    public void UpdateTabCount(EnConnectionDialogTab tab)
    {
        switch (tab)
        {
            case EnConnectionDialogTab.Browse:
                _browseTabCounter++;
                break;
            case EnConnectionDialogTab.History:
                _historyTabCounter++;
                break;
        }
    }

    public void UpdateErrorCounter()
    {
        _errorOccuranceCounter++;
    }

    /*
	public void UpdateFirewallRuleNeededCount()
	{
		// _firewallRuleNeededCounter++;
	}
	*/

    private void SetCounterProperties()
    {
        SetProperty(TelemetryConstants.C_KeyHistoryTab, _historyTabCounter);
        SetProperty(TelemetryConstants.C_KeyBrowseTab, _browseTabCounter);
        SetProperty(TelemetryConstants.C_KeyCountRecentConnections, _recentConnectionCounter);
        SetProperty(TelemetryConstants.C_KeyCountFavoriteConnections, _favoriteConnectionCounter);
        SetProperty(TelemetryConstants.C_KeyCountConnectionErrors, _errorOccuranceCounter);
        // SetProperty(ConnectionDlgConstants.FirewallErrors, _firewallRuleNeededCounter);
        if (_connectionsAtEnd == null || _connectionsAtEnd.Count <= 0)
        {
            return;
        }
        foreach (KeyValuePair<string, int> item in _connectionsAtEnd)
        {
            SetProperty(TelemetryConstants.C_CountEnginePrefix + item.Key, item.Value);
        }
    }

    private void SetConnectionProperties()
    {
        if (_connectionPropertiesAdded)
        {
            return;
        }
        lock (_LockObject)
        {
            if (_connectionPropertiesAdded)
            {
                return;
            }
            _connectionPropertiesAdded = true;
            // SetProperty(ConnectionDlgConstants.AuthenticationType, _authType);
            if (_serverDefinition != null)
            {
                EngineProduct = _serverDefinition.EngineProduct;
                EngineType = _serverDefinition.EngineType;
            }
            if (_connection == null)
            {
                return;
            }

            bool opened = false;

            try
            {

                if (_isClonedConnection && _connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                    opened = true;
                }

                bool getOpened;
                Version serverVersion;

                (serverVersion, getOpened) = GetServerVersion(_connection);

                opened |= getOpened;

                if (serverVersion != null)
                    ServerVersion = serverVersion;
                FbConnectionStringBuilder sqlConnectionStringBuilder = new(_connection.ConnectionString);
                IsLocalDb = GetIsLocalDb(sqlConnectionStringBuilder.DataSource);
            }
            catch (Exception exception)
            {
                UiTracer.TraceSource.TraceException(TraceEventType.Error, 14, exception,
                    "Failed to set sql version telemetry property", 159, "ConnectionEventTelemetryContext.cs", "SetConnectionProperties");
            }
            finally
            {
                if (opened)
                    _connection.Close();
            }
        }
    }

    private static bool GetIsLocalDb(string serverName)
    {
        return PropertySet.IsLocalIpAddress(serverName);
        /*
		if (!string.IsNullOrEmpty(serverName))
		{
			if (!serverName.StartsWith(C_LocalDbPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return serverName.Contains("\\\\.\\pipe\\LOCALDB#");
			}
			return true;
		}
		return false;
		*/
    }



    private void TryCloneConnection(IDbConnection connection)
    {
        try
        {
            if (connection == null)
            {
                UiTracer.TraceSource.TraceEvent(TraceEventType.Error, 14, "TryCloneAndSetConnection: null connection passed to method");
            }
            else if (connection is ICloneable cloneable)
            {
                if (cloneable.Clone() is not IDbConnection connection2)
                {
                    UiTracer.TraceSource.TraceEvent(TraceEventType.Error, 14, "TryCloneAndSetConnection: Cloning a cloneable IDbConnection should always return an IDbConnection!");
                    return;
                }
                _connection = connection2;
                _isClonedConnection = true;
            }
            else
            {
                UiTracer.TraceSource.TraceEvent(TraceEventType.Error, 14,
                    "TryCloneAndSetConnection: Expected cloneable connection like SqlConnection but got object of type {0}",
                    connection.GetType().ToString());

                _isClonedConnection = false;
                _connection = connection;
            }
        }
        catch (Exception exception)
        {
            UiTracer.TraceSource.TraceException(TraceEventType.Error, 14, exception,
                "TryCloneAndSetConnection: failed to clone the connection", 259,
                "ConnectionEventTelemetryContext.cs", "TryCloneConnection");
        }
    }


    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        try
        {
            if (disposing && _connection != null && _isClonedConnection)
            {
                _connection.Dispose();
            }
        }
        catch (Exception exception)
        {
            UiTracer.TraceSource.TraceException(TraceEventType.Error, 14, exception,
                "Failed to dispose the connection used by telemetry context", 284, "ConnectionEventTelemetryContext.cs", "Dispose");
        }
    }
}
