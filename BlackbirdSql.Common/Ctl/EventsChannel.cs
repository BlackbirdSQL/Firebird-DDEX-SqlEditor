#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Common.Ctl;

public sealed class EventsChannel : IBEventsChannel
{
	public event EventHandler<SelectedConnectionChangedEventArgs> SelectedConnectionChangedEvent;

	// public event EventHandler<WebBrowseEventArgs> WebBrowseRequestedEvent;

	// public event EventHandler<FirewallRuleEventArgs> FirewallRuleDetectedEvent;

	// public event EventHandler<FirewallRuleEventArgs> FirewallRuleCreatedEvent;

	public event EventHandler<EventArgs> MakeConnectionEvent;

	public event EventHandler<EventArgs> TestConnectionEvent;

	public event EventHandler<MessageEventArgs> ShowMessageEvent;

	public event EventHandler<CloseWindowEventArgs> CloseWindowEvent;

	public event EventHandler<EventArgs> ResetConnectionPropertyEvent;

	public event EventHandler<EventArgs> AuthenticationTypeChangedEvent;

	public event EventHandler<MakeConnectionCompletedEventArgs> MakeConnectionCompletedEvent;

	public event EventHandler<AdvancedPropertiesRequestedEventArgs> AdvancedPropertiesRequestedEvent;

	public event EventHandler<ConnectionsLoadedEventArgs> ConnectionsLoadedEvent;

	public event EventHandler<ExceptionOccurredEventArgs> ExceptionOccurredEvent;

	public event EventHandler<EventArgs> ConnectionPropertiesChangedEvent;

	public void OnSelectedConnectionChanged(IBPropertyAgent connectionInfo)
	{
		SelectedConnectionChangedEvent?.Invoke(this, new SelectedConnectionChangedEventArgs(connectionInfo));
	}

	/*
	public void OnWebBrowseRequested(string navigateLink)
	{
		WebBrowseRequestedEvent?.Invoke(this, new WebBrowseEventArgs(navigateLink));
	}

	public void OnFirewallRuleDetected(IPAddress clientIpAddress, string dataSource, EnFirewallRuleSource source)
	{
		FirewallRuleDetectedEvent?.Invoke(this, new FirewallRuleEventArgs
		{
			ClientIpAddress = clientIpAddress,
			DataSource = dataSource,
			Source = source
		});
	}

	public void OnFirewallRuleCreated(IPAddress clientIpAddress, string dataSource, EnFirewallRuleSource source)
	{
		FirewallRuleCreatedEvent?.Invoke(this, new FirewallRuleEventArgs
		{
			ClientIpAddress = clientIpAddress,
			DataSource = dataSource,
			Source = source
		});
	}
	*/

	public void OnMakeConnection()
	{
		MakeConnectionEvent?.Invoke(this, new EventArgs());
	}

	public void OnTestConnection()
	{
		TestConnectionEvent?.Invoke(this, new EventArgs());
	}

	public void OnShowMessage(string message, MessageBoxImage icon = MessageBoxImage.Hand, string title = null)
	{
		ShowMessageEvent?.Invoke(this, new(message, icon, title ??= SharedResx.Error));
	}

	public void OnShowMessage(Exception ex, MessageBoxImage icon = MessageBoxImage.Hand, string title = null)
	{
		ShowMessageEvent?.Invoke(this, new MessageEventArgs(ex.Message, icon, title ??= SharedResx.Error, ex));
	}

	public void OnCloseWindow(bool success)
	{
		CloseWindowEvent?.Invoke(this, new CloseWindowEventArgs(success));
	}

	public void OnResetConnectionProperty()
	{
		ResetConnectionPropertyEvent.Invoke(this, new EventArgs());
	}

	public void OnAuthenticationTypeChanged()
	{
		AuthenticationTypeChangedEvent?.Invoke(this, new EventArgs());
	}

	public void OnMakeConnectionCompleted(IDbConnection connection, string connectionString)
	{
		MakeConnectionCompletedEvent?.Invoke(this, new MakeConnectionCompletedEventArgs(connection, connectionString));
	}

	public void OnAdvancedPropertiesRequested(IBPropertyAgent connectionProperties)
	{
		AdvancedPropertiesRequestedEvent?.Invoke(this, new AdvancedPropertiesRequestedEventArgs(connectionProperties));
	}

	public void OnConnectionsLoaded(EnEngineType serverEngine, int numberOfConnections)
	{
		ConnectionsLoadedEvent?.Invoke(this, new ConnectionsLoadedEventArgs(serverEngine, numberOfConnections));
	}

	public void OnExceptionOccurred(IEnumerable<Exception> exceptions /*, EnFirewallRuleSource source */)
	{
		ExceptionOccurredEvent?.Invoke(this, new ExceptionOccurredEventArgs(exceptions /*, source */));
	}

	public void OnConnectionPropertiesChanged()
	{
		ConnectionPropertiesChangedEvent?.Invoke(this, new EventArgs());
	}
}
