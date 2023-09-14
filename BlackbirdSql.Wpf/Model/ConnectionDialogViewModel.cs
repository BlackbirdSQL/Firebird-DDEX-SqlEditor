// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionDialogViewModel

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;

using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Wpf.Model;

public class ConnectionDialogViewModel : ViewModelBase
{
    protected const string C_KeyCanExecute = "CanExecute";
    // protected const string C_KeyCompression = "Compression";
    protected const string C_KeyIsConnectMode = "IsConnectMode";
    protected const string C_KeyIsExecuting = "IsExecuting";

    protected static new DescriberDictionary _Describers;


    private readonly HistoryPageViewModel _HistoryPageViewModel;

    private readonly BrowsePageViewModel _BrowsePageViewModel;

    private readonly ConnectionPropertySectionViewModel _ConnectionProperty;

    // private IBEventsChannel _Channel;

    private int _SelectedTabIndex;

    private UIConnectionInfo _UiConnectionInfo;

    private readonly VerifyConnectionDelegate _ConnectionVerifier;
    private readonly ConnectionDialogConfiguration _Config;

    private readonly IBDependencyManager _DependencyManager;

    private readonly IBConnectionMruManager _MruManager;

    private readonly ServiceManager<IBServerConnectionProvider> _ServerConnectionProviderServiceManager;

    private readonly Traceable _Traceable;

    // private TelemetryTracer _TelemetryTracer;

    private CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();

    private readonly ConnectionEventTelemetryContext _telemetryContext = new ConnectionEventTelemetryContext();

    private bool _IsSuccessTelemetrySent;

    // private IFirewallErrorParser _firewallErrorParser = new FirewallErrorParser();

    private DialogModel _Model;


    public UIConnectionInfo UiConnectionInfo
    {
        get { return _UiConnectionInfo; }
        private set { _UiConnectionInfo = value; }
    }

    public IBEventsChannel EventsChannel => _Channel;

    /*
	public IFirewallErrorParser FirewallErrorParser
	{
		get
		{
			if (_firewallErrorParser == null)
			{
				_firewallErrorParser = new FirewallErrorParser();
			}

			return _firewallErrorParser;
		}
		set
		{
			_firewallErrorParser = value;
		}
	}
	*/
    private Traceable Trace { get; set; }

    // public TelemetryTracer TelemetryTracer => _TelemetryTracer;

    public IBAsyncCommand ConnectCommand { get; private set; }

    public override DescriberDictionary Describers
    {
        get
        {
            if (_Describers == null)
                CreateAndPopulatePropertySet(null);

            return _Describers;
        }
    }

    public IBAsyncCommand TestConnectionCommand { get; private set; }

    public DialogModel Model
    {
        get { return _Model; }
        private set { _ = _Model; _Model = value; _ = _Model; }
    }

    public HistoryPageViewModel GetHistoryPageViewModel => _HistoryPageViewModel;

    public BrowsePageViewModel GetBrowsePageViewModel => _BrowsePageViewModel;

    public ConnectionPropertySectionViewModel ConnectionPropertyViewModel => _ConnectionProperty;

    public int SelectedTabIndex
    {
        get
        {
            return _SelectedTabIndex;
        }
        set
        {
            ConnectionPropertyViewModel.InfoConnection.ResetConnectionInfo();
            _telemetryContext.UpdateTabCount((EnConnectionDialogTab)value);
            _SelectedTabIndex = value;
        }
    }

    /*
	public bool Compression
	{
		get { return (bool)GetProperty(C_KeyCompression); }
		set { SetProperty(C_KeyCompression, value); }
	}
	*/

    public bool IsConnectMode
    {
        get { return (bool)GetProperty(C_KeyIsConnectMode); }
        set { SetProperty(C_KeyIsConnectMode, value); }
    }

    public string OkButtonTitle { get; set; }

    public bool CanExecute
    {
        get { return (bool)GetProperty(C_KeyCanExecute); }
        set { SetProperty(C_KeyCanExecute, value); }
    }

    public bool IsExecuting
    {
        get { return (bool)GetProperty(C_KeyIsExecuting); }
        set { SetProperty(C_KeyIsExecuting, value); }
    }

    public event EventHandler<SectionEventArgs> SectionInitializedEvent
    {
        add
        {
            Model.SectionInitializedEvent += value;
        }
        remove
        {
            Model.SectionInitializedEvent -= value;
        }
    }

    public event EventHandler<SectionEventArgs> SectionClosingEvent
	{
        add
        {
            Model.SectionClosingEvent += value;
        }
        remove
        {
            Model.SectionClosingEvent -= value;
        }
    }

    public ConnectionDialogViewModel(IBDependencyManager dependencyManager, IBEventsChannel channel, UIConnectionInfo ci, VerifyConnectionDelegate verifierDelegate, ConnectionDialogConfiguration config) : base(channel)
    {
        Cmd.CheckForNull(dependencyManager, "dependencyManager");
        Cmd.CheckForNull(channel, "channel");
        Cmd.CheckForNull(ci, "ci");
        Cmd.CheckForNull(config, "config");

        ConnectCommand = new AsyncCommand<bool>(async (object p) => await ConnectAsync(p));
        TestConnectionCommand = new AsyncCommand<bool>(async (object p) => await TestConnectionAsync(p));
        ConnectCommand.CanExecuteChanged += OnCanExecuteChanged;
        TestConnectionCommand.CanExecuteChanged += OnCanExecuteChanged;
        Model = new DialogModel(dependencyManager);
        Model.DiscoveryCompletedEvent += Model_DiscoveryCompleted;
        ServiceManager<IBConnectionMruManager> serviceManager = new ServiceManager<IBConnectionMruManager>(dependencyManager);
        _MruManager = serviceManager.GetService(null);
        Cmd.CheckForNull(_MruManager, "mruManager");
        _ServerConnectionProviderServiceManager = new ServiceManager<IBServerConnectionProvider>(dependencyManager);
        _Channel.MakeConnectionEvent += Connect;
        _Channel.TestConnectionEvent += ChannelOnTestConnection;
        _Channel.ConnectionsLoadedEvent += ChannelOnConnectionsLoaded;
        _Channel.ExceptionOccurredEvent += ChannelOnExceptionOccurred;
        // _Channel.FirewallRuleCreatedEvent += OnFirewallRuleCreated;
        _ConnectionProperty = new ConnectionPropertySectionViewModel(_Channel, dependencyManager, ci);
        _HistoryPageViewModel = new HistoryPageViewModel(dependencyManager, _ConnectionProperty, _Channel, _MruManager, ci.Database != null);
        _BrowsePageViewModel = new BrowsePageViewModel(dependencyManager, _ConnectionProperty, _Channel);
        _Config = config;
        _SelectedTabIndex = (int)_Config.InitialTab;
        _UiConnectionInfo = ci;
        IsConnectMode = _Config.IsConnectMode;
        _ConnectionVerifier = verifierDelegate;
        _DependencyManager = dependencyManager;
        _Traceable = new Traceable(dependencyManager);
        // _TelemetryTracer = new TelemetryTracer(dependencyManager);
        Trace = new Traceable(dependencyManager);
        if (IsConnectMode)
        {
            OkButtonTitle = SharedResx.Connect;
        }
        else
        {
            OkButtonTitle = SharedResx.Ok;
        }

        Initialize();
    }


    /*
	public void OnFirewallRuleCreated(object sender, FirewallRuleEventArgs firewallRuleEventArgs)
	{
		if (firewallRuleEventArgs != null)
		{
			switch (firewallRuleEventArgs.Source)
			{
				case EnFirewallRuleSource.Connect:
					_Channel.OnMakeConnection();
					break;
				case EnFirewallRuleSource.TestConnection:
					_Channel.OnTestConnection();
					break;
			}
		}
	}
	*/

    private void ChannelOnExceptionOccurred(object sender, ExceptionOccurredEventArgs exceptionOccurredEventArgs)
    {
        bool flag = false;
        if (exceptionOccurredEventArgs == null)
        {
            return;
        }

        List<Exception> list = new List<Exception>(exceptionOccurredEventArgs.Exceptions);
        foreach (Exception item in list)
        {
            if (item.IsSqlException())
            {
                flag = HandleException(item /*, exceptionOccurredEventArgs.Source */);
                if (flag)
                {
                    break;
                }
            }
        }

        if (flag)
        {
            return;
        }

        if (list.Count == 1)
        {
            _Channel.OnShowMessage(list[0]);
            return;
        }

        string message = string.Join(Environment.NewLine, exceptionOccurredEventArgs.Exceptions.Select((x) => x.GetExceptionMessage()));
        _Channel.OnShowMessage(message);
    }

    private void ChannelOnConnectionsLoaded(object sender, ConnectionsLoadedEventArgs connectionsLoadedEventArgs)
    {
        if (connectionsLoadedEventArgs != null)
        {
            // _telemetryContext.UpdateNumberofConnectionsLoaded(connectionsLoadedEventArgs);
        }
    }

    private void OnCanExecuteChanged(object sender, EventArgs eventArgs)
    {
        CanExecute = _ConnectionProperty.IsComplete && !_CancellationTokenSource.IsCancellationRequested
            && (TestConnectionCommand.CanExecute(sender) || ConnectCommand.CanExecute(sender));
        IsExecuting = !_CancellationTokenSource.IsCancellationRequested && (TestConnectionCommand.IsExecuting || ConnectCommand.IsExecuting);
    }

    public void Initialize()
    {
        Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate
        {
            try
            {
                Model.DiscoverPlugins();
            }
            catch (Exception ex)
            {
                _Channel.OnShowMessage(ex);
            }
        });
    }

    public void HandleCancelClose()
    {
        try
        {
            if (!_IsSuccessTelemetrySent)
            {
                PostTelemetryInfo(success: false);
            }

            _CancellationTokenSource.Cancel();
            Application.Current.Dispatcher.Invoke(delegate
            {
                RaisePropertyChanged(C_KeyIsExecuting);
            });
        }
        catch (Exception exception)
        {
            Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "Failed to cancel", 264, "ConnectionDialogViewModel.cs", "HandleCancelClose");
        }
    }

    private void PostTelemetryInfo(bool success, IDbConnection connection = null)
    {
        try
        {
            if (success)
            {
                _IsSuccessTelemetrySent = true;
            }

            _telemetryContext.SetConnectionInfo(ConnectionPropertyViewModel.InfoConnection.ServerDefinition, connection);
            // _telemetryContext.SetDialogInfo(GetHistoryPageViewModel, _SelectedTabIndex == 0, success);
            // _TelemetryTracer.PostEventWithContextAsync(ConnectionDlgConstants.ConnectionEvent, _telemetryContext);
        }
        catch (Exception exception)
        {
            Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "Failed to post telemetry event", 287, "ConnectionDialogViewModel.cs", "PostTelemetryInfo");
        }
    }

    private async void Connect(object sender, EventArgs e)
    {
        await ConnectCommand.ExecuteAsync(sender);
    }

    private async void ChannelOnTestConnection(object sender, EventArgs eventArgs)
    {
        await TestConnectionAsync(null);
    }

    private async Task<bool> ConnectAsync(object parameter)
    {
        bool result = false;
        _CancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _CancellationTokenSource.Token;
        if (!cancellationToken.IsCancellationRequested && _ConnectionProperty.InfoConnection != null)
        {
            IBServerConnectionProvider serverConnectionProvider = GetServerConnectionProvider();
            _Traceable.AssertTraceEvent(serverConnectionProvider != null, TraceEventType.Error, EnUiTraceId.Connection, "serverConnectionProvider is null");
            if (serverConnectionProvider != null && WriteToConnectionInfo(serverConnectionProvider))
            {
                ConnectionResponse connectionResponse = null;
                string connectionString = null;
                IDbConnection connection = null;
                if (_ConnectionVerifier != null)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        connectionResponse = await OpenConnectionWithVerifierAsync(serverConnectionProvider, cancellationToken);
                        if (connectionResponse != null && connectionResponse.Connection != null)
                        {
                            connection = connectionResponse.Connection;
                            connectionString = connection.ConnectionString;
                        }
                    }
                }
                else if (!cancellationToken.IsCancellationRequested)
                {
                    connectionString = serverConnectionProvider.GetConnectionString(_UiConnectionInfo, _ConnectionProperty.InfoConnection.ServerDefinition);
                    connection = new FbConnection(connectionString);
                    if (IsConnectMode)
                    {
                        connectionResponse = await OpenConnectionAsync(connection, serverConnectionProvider);
                    }
                    else
                    {
                        connectionResponse = new ConnectionResponse
                        {
                            Success = true
                        };
                    }
                }

                result = ProcessConnectionResponse(connectionResponse, connection, connectionString, cancellationToken);
            }
        }

        return result;
    }

    protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
    {
        if (_Describers == null)
        {
            _Describers = new();

            // Initializers for property sets are held externally for this class
            ViewModelBase.CreateAndPopulatePropertySet(_Describers);

            _Describers.Add(C_KeyCanExecute, typeof(bool), false);
            // _Describers.Add(C_KeyCompression, typeof(bool), false);
            _Describers.Add(C_KeyIsConnectMode, typeof(bool), false);
            _Describers.Add(C_KeyIsExecuting, typeof(bool), false);
        }

        // If null then this was a call from our own .ctor so no need to pass anything back
        describers?.AddRange(_Describers);

    }

    private bool ProcessConnectionResponse(ConnectionResponse response, IDbConnection connection, string connectionString, CancellationToken cancellationToken)
    {
        bool flag = false;
        if (response != null && !cancellationToken.IsCancellationRequested)
        {
            flag = response.Success;
            if (response.Success && connection != null && connectionString != null)
            {
                FbConnectionStringBuilder sqlConnectionStringBuilder = new(connection.ConnectionString);
                if (string.IsNullOrEmpty(sqlConnectionStringBuilder.Database))
                {
                    sqlConnectionStringBuilder.Database = connection.Database;
                }

                _MruManager.AddConnection(sqlConnectionStringBuilder.ConnectionString, _ConnectionProperty.InfoConnection.IsFavorite, _ConnectionProperty.InfoConnection.ServerDefinition);
                _Channel.OnMakeConnectionCompleted(connection, connectionString);
                PostTelemetryInfo(flag, connection);
                _Channel.OnCloseWindow(success: true);
            }
            else if (!response.Success && response.Exception != null && !HandleException(response.Exception /*, EnFirewallRuleSource.Connect*/))
            {
                _Traceable.TraceException(TraceEventType.Error, 3, response.Exception, "Exception occurs in ConnectAsync.", 385, "ConnectionDialogViewModel.cs", "ProcessConnectionResponse");
                _Channel.OnShowMessage(response.Exception);
                // _telemetryContext.UpdateErrorCounter();
            }
        }

        return flag;
    }

    public IBServerConnectionProvider GetServerConnectionProvider()
    {
        /*
		if (_ConnectionProperty.InfoConnection.ServerDefinition == null)
		{
			_ConnectionProperty.InfoConnection.ServerDefinition = ServerDefinition.Default;
		}
		*/

        IBServerConnectionProvider service = _ServerConnectionProviderServiceManager.GetService(_ConnectionProperty.InfoConnection.ServerDefinition);
        Trace.AssertTraceEvent(service != null, TraceEventType.Error, EnUiTraceId.UiInfra, "Cannot load IServerConnectionProvider");
        return service;
    }

    private async Task<bool> TestConnectionAsync(object parameter)
    {
        bool result = false;
        if (!_CancellationTokenSource.IsCancellationRequested && _ConnectionProperty.InfoConnection != null)
        {
            IBServerConnectionProvider serverConnectionProvider = GetServerConnectionProvider();
            _Traceable.AssertTraceEvent(serverConnectionProvider != null, TraceEventType.Error, EnUiTraceId.Connection, "serverConnectionProvider is null");
            if (serverConnectionProvider != null && WriteToConnectionInfo(serverConnectionProvider))
            {
                FbConnectionStringBuilder sqlConnectionStringBuilder = new(serverConnectionProvider.GetConnectionString(_UiConnectionInfo, _ConnectionProperty.InfoConnection.ServerDefinition))
                {
                    ConnectionTimeout = ModelConstants.C_TestConnectionTimeout
                };

                ConnectionResponse connectionResponse;

                using (FbConnection connection = new(sqlConnectionStringBuilder.ConnectionString))
                {
                    connectionResponse = await OpenConnectionAsync(connection, serverConnectionProvider);
                    result = connectionResponse.Success;
                }

                if (!_CancellationTokenSource.IsCancellationRequested && connectionResponse != null && connectionResponse.Success)
                {
                    _Channel.OnShowMessage(SharedResx.TestConnection_succeeded,
                        MessageBoxImage.Asterisk, SharedResx.ConnectionDialogTitle);
                }
                else if (!_CancellationTokenSource.IsCancellationRequested && connectionResponse != null
                    && !connectionResponse.Success && connectionResponse.Exception != null
                    && !HandleException(connectionResponse.Exception /*, EnFirewallRuleSource.TestConnection */))
                {
                    _Traceable.TraceException(TraceEventType.Error, 3, connectionResponse.Exception,
                        "Exception occurs in TestConnection.", 443, "ConnectionDialogViewModel.cs", "TestConnectionAsync");

                    if (connectionResponse.Exception is FbException ex && ex.GetNumber() == 4060)
                    {
                        _Channel.OnShowMessage(SharedResx.TestConnection_databaseDoesNotExist);
                    }
                    else
                    {
                        _Channel.OnShowMessage(connectionResponse.Exception);
                    }
                }
            }
        }

        return result;
    }

    public Task<ConnectionResponse> OpenConnectionAsync(IDbConnection connection, IBServerConnectionProvider serverConnectionProvider)
    {
        return Task.Factory.StartNew(() => OpenConnection(connection, serverConnectionProvider), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    private ConnectionResponse OpenConnection(IDbConnection connection, IBServerConnectionProvider serverConnectionProvider)
    {
        ConnectionResponse connectionResponse = new ConnectionResponse();
        try
        {
            connection.Open();
            ValidateConnection(serverConnectionProvider, connection);
            connectionResponse.Success = true;
            return connectionResponse;
        }
        catch (Exception exception)
        {
            connectionResponse.Success = false;
            connectionResponse.Exception = exception;
            return connectionResponse;
        }
    }

    public Task<ConnectionResponse> OpenConnectionWithVerifierAsync(IBServerConnectionProvider serverConnectionProvider, CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() => OpenConnectionWithVerifier(serverConnectionProvider), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    private ConnectionResponse OpenConnectionWithVerifier(IBServerConnectionProvider serverConnectionProvider)
    {
        ConnectionResponse connectionResponse = new ConnectionResponse();
        try
        {
            connectionResponse.Connection = _ConnectionVerifier(_UiConnectionInfo);
            connectionResponse.Success = true;
            return connectionResponse;
        }
        catch (Exception exception)
        {
            connectionResponse.Success = false;
            connectionResponse.Exception = exception;
            return connectionResponse;
        }
    }

    public bool WriteToConnectionInfo(IBServerConnectionProvider serverConnectionProvider)
    {
        _Traceable.TraceEvent(TraceEventType.Information, 3, "Writing to connection info...");
        if (_ConnectionProperty.ValidateConnectionInfo())
        {
            _UiConnectionInfo = (UIConnectionInfo)_ConnectionProperty.InfoConnection.ToUiConnectionInfo();

            return true;
        }

        return false;
    }

    private static void ValidateConnection(IBServerConnectionProvider serverConnectionProvider, IDbConnection connection)
    {
        if (connection != null && connection.State != 0)
        {
            (serverConnectionProvider as IBConnectionValidator)?.CheckConnection(connection);
        }
    }

    private bool HandleException(Exception ex /*, EnFirewallRuleSource source */)
    {
        /*
		SqlException ex2 = ex as SqlException;
		if (ex2 != null && CanHandleFirewallRule(_DependencyManager))
		{
			FirewallParserResponse firewallParserResponse = FirewallErrorParser.ParseException(ex2);
			if (firewallParserResponse != null && firewallParserResponse.FirewallRuleErrorDetected && _ConnectionProperty.InfoConnection != null && _ConnectionProperty.Connection.ServerName != null)
			{
				_telemetryContext.UpdateFirewallRuleNeededCount();
				_Channel.OnFirewallRuleDetected(firewallParserResponse.BlockedIpAddress, _ConnectionProperty.InfoConnection.ServerName, source);
				return true;
			}
		}
		*/

        return false;
    }

    public static bool CanHandleFirewallRule(IBDependencyManager dependencyManager)
    {
        return false;

        // return FirewallRulesDialogViewModel.DependenciesAreValid(dependencyManager);
    }

    private void Model_DiscoveryCompleted(object sender, EventArgs e)
    {
        VerifyAccess();
        Model.CreateSections(_Channel);
        _Traceable.AssertTraceEvent(Model.Sections.Count != 0, TraceEventType.Error, EnUiTraceId.Sections, "There is no browse section");
        foreach (SectionHost section in Model.Sections)
        {
            section.Loaded();
        }
    }

    public void CloseSections()
    {
        Model.CloseSections();
    }

    public override void Dispose()
    {
        if (_Channel != null)
        {
            _Channel.MakeConnectionEvent -= Connect;
            _Channel.TestConnectionEvent -= ChannelOnTestConnection;
			_Channel.ConnectionsLoadedEvent -= ChannelOnConnectionsLoaded;
			_Channel.ExceptionOccurredEvent -= ChannelOnExceptionOccurred;
            // _Channel.FirewallRuleCreatedEvent -= OnFirewallRuleCreated;
        }

        _CancellationTokenSource.Dispose();
        base.Dispose();
        Model.Dispose();
    }


    public override IBPropertyAgent Copy()
    {
        return new ConnectionDialogViewModel(_DependencyManager, _Channel, _UiConnectionInfo, _ConnectionVerifier, _Config);
    }
}
