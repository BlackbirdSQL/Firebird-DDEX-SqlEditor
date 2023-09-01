// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.IEventsChannel

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using BlackbirdSql.Core.Events;



namespace BlackbirdSql.Core.Interfaces;

public interface IBEventsChannel
{
	event EventHandler<SelectedConnectionChangedEventArgs> SelectedConnectionChanged;

	// event EventHandler<WebBrowseEventArgs> WebBrowseRequested;

	// event EventHandler<FirewallRuleEventArgs> FirewallRuleDetected;

	// event EventHandler<FirewallRuleEventArgs> FirewallRuleCreated;

	event EventHandler<EventArgs> MakeConnection;

	event EventHandler<EventArgs> TestConnection;

	event EventHandler<MessageEventArgs> ShowMessage;

	event EventHandler<CloseWindowEventArgs> CloseWindow;

	event EventHandler<EventArgs> ResetConnectionProperty;

	event EventHandler<EventArgs> AuthenticationTypeChanged;

	event EventHandler<MakeConnectionCompletedEventArgs> MakeConnectionCompleted;

	event EventHandler<AdvancedPropertiesRequestedEventArgs> AdvancedPropertiesRequested;

	event EventHandler<ConnectionsLoadedEventArgs> ConnectionsLoaded;

	event EventHandler<EventArgs> ConnectionPropertiesChanged;

	event EventHandler<ExceptionOccurredEventArgs> ExceptionOccurred;

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

	void OnConnectionsLoaded(IBServerDefinition serverDefinition, int numberOfConnections);

	void OnConnectionPropertiesChanged();

	void OnExceptionOccurred(IEnumerable<Exception> exceptions /*, EnFirewallRuleSource source */);
}
