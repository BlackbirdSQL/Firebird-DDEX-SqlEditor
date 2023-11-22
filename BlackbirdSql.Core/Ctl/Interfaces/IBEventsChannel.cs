// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.IEventsChannel

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Events;

namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBEventsChannel
{
	event EventHandler<SelectedConnectionChangedEventArgs> SelectedConnectionChangedEvent;

	// event EventHandler<WebBrowseEventArgs> WebBrowseRequestedEvent;

	// event EventHandler<FirewallRuleEventArgs> FirewallRuleDetectedEvent;

	// event EventHandler<FirewallRuleEventArgs> FirewallRuleCreatedEvent;

	event EventHandler<EventArgs> MakeConnectionEvent;

	event EventHandler<EventArgs> TestConnectionEvent;

	event EventHandler<MessageEventArgs> ShowMessageEvent;

	event EventHandler<CloseWindowEventArgs> CloseWindowEvent;

	event EventHandler<EventArgs> ResetConnectionPropertyEvent;

	event EventHandler<EventArgs> AuthenticationTypeChangedEvent;

	event EventHandler<MakeConnectionCompletedEventArgs> MakeConnectionCompletedEvent;

	event EventHandler<AdvancedPropertiesRequestedEventArgs> AdvancedPropertiesRequestedEvent;

	event EventHandler<ConnectionsLoadedEventArgs> ConnectionsLoadedEvent;

	event EventHandler<EventArgs> ConnectionPropertiesChangedEvent;

	event EventHandler<ExceptionOccurredEventArgs> ExceptionOccurredEvent;

	void OnSelectedConnectionChanged(IBPropertyAgent connectionInfo);

	// void OnWebBrowseRequested(string navigateLink);

	// void OnFirewallRuleDetected(IPAddress clientIpAddress, string serverName, EnFirewallRuleSource source);

	// void OnFirewallRuleCreated(IPAddress clientIpAddress, string serverName, EnFirewallRuleSource source);

	void OnMakeConnection();

	void OnTestConnection();

	void OnShowMessage(string message, MessageBoxImage icon = MessageBoxImage.Hand, string title = null);

	void OnShowMessage(Exception ex, MessageBoxImage icon = MessageBoxImage.Hand, string title = null);

	void OnCloseWindow(bool success);

	void OnResetConnectionProperty();

	void OnAuthenticationTypeChanged();

	void OnMakeConnectionCompleted(IDbConnection connection, string connectionString);

	void OnAdvancedPropertiesRequested(IBPropertyAgent connectionProperties);

	void OnConnectionsLoaded(EnEngineType serverEngine, int numberOfConnections);

	void OnConnectionPropertiesChanged();

	void OnExceptionOccurred(IEnumerable<Exception> exceptions /*, EnFirewallRuleSource source */);
}
