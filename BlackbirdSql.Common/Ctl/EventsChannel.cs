#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

using BlackbirdSql.Core.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Properties;





namespace BlackbirdSql.Common.Ctl;


public sealed class EventsChannel : IBEventsChannel
{
	public event EventHandler<SelectedConnectionChangedEventArgs> SelectedConnectionChanged;

	// public event EventHandler<WebBrowseEventArgs> WebBrowseRequested;

	// public event EventHandler<FirewallRuleEventArgs> FirewallRuleDetected;

	// public event EventHandler<FirewallRuleEventArgs> FirewallRuleCreated;

	public event EventHandler<EventArgs> MakeConnection;

	public event EventHandler<EventArgs> TestConnection;

	public event EventHandler<MessageEventArgs> ShowMessage;

	public event EventHandler<CloseWindowEventArgs> CloseWindow;

	public event EventHandler<EventArgs> ResetConnectionProperty;

	public event EventHandler<EventArgs> AuthenticationTypeChanged;

	public event EventHandler<MakeConnectionCompletedEventArgs> MakeConnectionCompleted;

	public event EventHandler<AdvancedPropertiesRequestedEventArgs> AdvancedPropertiesRequested;

	public event EventHandler<ConnectionsLoadedEventArgs> ConnectionsLoaded;

	public event EventHandler<ExceptionOccurredEventArgs> ExceptionOccurred;

	public event EventHandler<EventArgs> ConnectionPropertiesChanged;

	public void OnSelectedConnectionChanged(IBPropertyAgent connectionInfo)
	{
		SelectedConnectionChanged?.Invoke(this, new SelectedConnectionChangedEventArgs(connectionInfo));
	}

	/*
	public void OnWebBrowseRequested(string navigateLink)
	{
		WebBrowseRequested?.Invoke(this, new WebBrowseEventArgs(navigateLink));
	}

	public void OnFirewallRuleDetected(IPAddress clientIpAddress, string serverName, EnFirewallRuleSource source)
	{
		FirewallRuleDetected?.Invoke(this, new FirewallRuleEventArgs
		{
			ClientIpAddress = clientIpAddress,
			ServerName = serverName,
			Source = source
		});
	}

	public void OnFirewallRuleCreated(IPAddress clientIpAddress, string serverName, EnFirewallRuleSource source)
	{
		FirewallRuleCreated?.Invoke(this, new FirewallRuleEventArgs
		{
			ClientIpAddress = clientIpAddress,
			ServerName = serverName,
			Source = source
		});
	}
	*/

	public void OnMakeConnection()
	{
		MakeConnection?.Invoke(this, new EventArgs());
	}

	public void OnTestConnection()
	{
		TestConnection?.Invoke(this, new EventArgs());
	}

	public void OnShowMessage(string message, MessageBoxImage icon = MessageBoxImage.Hand, string title = null)
	{
		if (ShowMessage != null)
		{
			title ??= SharedResx.Error;

			ShowMessage(this, new MessageEventArgs(message, icon, title));
		}
	}

	public void OnShowMessage(Exception ex, MessageBoxImage icon = MessageBoxImage.Hand, string title = null)
	{
		if (ShowMessage != null)
		{
			title ??= SharedResx.Error;

			ShowMessage(this, new MessageEventArgs(ex.Message, icon, title, ex));
		}
	}

	public void OnCloseWindow(bool success)
	{
		CloseWindow?.Invoke(this, new CloseWindowEventArgs(success));
	}

	public void OnResetConnectionProperty()
	{
		ResetConnectionProperty?.Invoke(this, new EventArgs());
	}

	public void OnAuthenticationTypeChanged()
	{
		AuthenticationTypeChanged?.Invoke(this, new EventArgs());
	}

	public void OnMakeConnectionCompleted(IDbConnection connection, string connectionString)
	{
		MakeConnectionCompleted?.Invoke(this, new MakeConnectionCompletedEventArgs(connection, connectionString));
	}

	public void OnAdvancedPropertiesRequested(IBPropertyAgent connectionProperties)
	{
		AdvancedPropertiesRequested?.Invoke(this, new AdvancedPropertiesRequestedEventArgs(connectionProperties));
	}

	public void OnConnectionsLoaded(IBServerDefinition serverDefinition, int numberOfConnections)
	{
		ConnectionsLoaded?.Invoke(this, new ConnectionsLoadedEventArgs(serverDefinition, numberOfConnections));
	}

	public void OnExceptionOccurred(IEnumerable<Exception> exceptions /*, EnFirewallRuleSource source */)
	{
		ExceptionOccurred?.Invoke(this, new ExceptionOccurredEventArgs(exceptions /*, source */));
	}

	public void OnConnectionPropertiesChanged()
	{
		ConnectionPropertiesChanged?.Invoke(this, new EventArgs());
	}
}
