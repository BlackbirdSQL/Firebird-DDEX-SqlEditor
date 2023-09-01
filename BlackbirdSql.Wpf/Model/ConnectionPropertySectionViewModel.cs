#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Core.Extensions;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Properties;

using FirebirdSql.Data.FirebirdClient;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Common.Ctl;
using MonikerAgent = BlackbirdSql.Common.Model.MonikerAgent;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Wpf.Model;


public class ConnectionPropertySectionViewModel : ViewModelBase
{
    public const string C_KeyInfoConnection = "InfoConnection";
    public const string C_KeyIsUserNameEnabled = "IsUserNameEnabled";
    public const string C_KeyIsPasswordEnabled = "IsPasswordEnabled";
    public const string C_KeyIsConnectionExpanded = "IsConnectionExpanded";
    public const string C_KeyAuthenticationOptions = "AuthenticationOptions";

    protected static new DescriberDictionary _Describers;

    // rivate readonly Dictionary<AuthenticationTypes, string> _AuthenticationOptions;


    // private ServiceManager<IAuthenticationTypeProvider> _authenticationTypeServiceManager;

    // private ServiceManager<IDatabaseDiscoveryProvider> _databaseDiscoveryProviderServiceManager;

    private ServiceManager<IServerConnectionProvider> _ServerConnectionProviderServiceManager;

    private ServiceManager<IConnectionPropertiesProvider> _ConnectionPropertiesManager;

    // private Dictionary<AuthenticationTypes, string> _AuthenticationCollection;



    private bool _IsUpdatingProperties;

    private Traceable _Traceable;

    private readonly ThreadSafeCollection<string> _ThreadSafeCollection = new ThreadSafeCollection<string>();

    // private CancellationTokenSource _DatabaseCancellationTokenSource = new CancellationTokenSource();

    // private readonly object _ConnectionTaskLock = new object();

    private readonly object _DatabaseLoadingLock = new object();

    private readonly object _UpdatingPropertiesLock = new object();

    // public const AuthenticationTypes DefaultAuth = AuthenticationTypes.SqlPassword;

    private bool DatabasesAreLoaded { get; set; }

    public ICommand AdvancedPropertiesCommand { get; private set; }

    public ICommand EnterPressedCommand { get; private set; }

    public ConnectionInfo InfoConnection
    {
        get { return (ConnectionInfo)GetProperty(C_KeyInfoConnection); }
        set { SetProperty(C_KeyInfoConnection, value); }
    }


    /*
	public Dictionary<AuthenticationTypes, string> AuthenticationOptions
	{
		get
		{
			return (Dictionary<AuthenticationTypes, string>)GetProperty(C_KeyAuthenticationOptions);
		}
		set
		{
			SetProperty(C_KeyAuthenticationOptions, value);

			if (value.ContainsKey(InfoConnection.AuthenticationType))
				return;

			using Dictionary<AuthenticationTypes, string>.KeyCollection.Enumerator enumerator = value.Keys.GetEnumerator();
			if (enumerator.MoveNext())
			{
				AuthenticationTypes current = enumerator.Current;
				InfoConnection.AuthenticationType = current;
			}
		}
	}
	*/

    public override DescriberDictionary Describers
    {
        get
        {
            if (_Describers == null)
                CreateAndPopulatePropertySet(null);

            return _Describers;
        }
    }


    public bool IsUserNameEnabled
    {
        get { return (bool)GetProperty(C_KeyIsUserNameEnabled); }
        set { SetProperty(C_KeyIsUserNameEnabled, value); }
    }

    public bool IsPasswordEnabled
    {
        get { return (bool)GetProperty(C_KeyIsPasswordEnabled); }
        set { SetProperty(C_KeyIsPasswordEnabled, value); }
    }

    public ReadOnlyObservableCollection<string> DatabaseList => _ThreadSafeCollection.CloneList;

    public bool IsConnectionExpanded
    {
        get { return (bool)GetProperty(C_KeyIsConnectionExpanded); }
        set { SetProperty(C_KeyIsConnectionExpanded, value); }
    }


    public ConnectionPropertySectionViewModel(IBEventsChannel channel, IBDependencyManager dependencyManager,
        IBPropertyAgent ci, ConnectionPropertySectionViewModel rhs)
        : base(System.Windows.Threading.Dispatcher.CurrentDispatcher, channel, null, rhs)
    {
        Initialize(channel, dependencyManager, rhs);
    }

    public ConnectionPropertySectionViewModel(IBEventsChannel channel, IBDependencyManager dependencyManager, IBPropertyAgent ci = null)
        : this(channel, dependencyManager, ci, null)
    {
    }


    protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
    {
        if (_Describers == null)
        {
            _Describers = new();

            // Initializers for property sets are held externally for this class
            ViewModelBase.CreateAndPopulatePropertySet(_Describers);
        }

        // If null then this was a call from our own .ctor so no need to pass anything back
        describers?.AddRange(_Describers);

    }

    protected void Initialize(IBEventsChannel channel, IBDependencyManager dependencyManager, IBPropertyAgent ci)
    {
        Cmd.CheckForNull(dependencyManager, "dependencyManager");
        Cmd.CheckForNull(channel, "channel");

        Add(C_KeyInfoConnection, typeof(ConnectionInfo), new ConnectionInfo(channel));
        Add(C_KeyIsUserNameEnabled, typeof(bool), true);
        Add(C_KeyIsPasswordEnabled, typeof(bool), false);
        Add(C_KeyIsConnectionExpanded, typeof(bool), true);

        _Traceable = new Traceable(dependencyManager);
        // _authenticationTypeServiceManager = new ServiceManager<IAuthenticationTypeProvider>(dependencyManager);
        // _databaseDiscoveryProviderServiceManager = new ServiceManager<IDatabaseDiscoveryProvider>(dependencyManager);
        _ServerConnectionProviderServiceManager = new ServiceManager<IServerConnectionProvider>(dependencyManager);
        _ConnectionPropertiesManager = new ServiceManager<IConnectionPropertiesProvider>(dependencyManager);
        _Channel.ResetConnectionProperty += ResetAuthenticationAndDatabases;
        _Channel.AuthenticationTypeChanged += OnAuthenticationTypeChanged;
        _Channel.ConnectionPropertiesChanged += ChannelOnConnectionPropertiesChanged;
        _Traceable = new Traceable(dependencyManager);
        _ThreadSafeCollection.Trace = _Traceable;
        SetDatabaseList(new ObservableCollection<string>());
        // InfoConnection.AuthenticationType = AuthenticationTypes.SqlPassword;
        AdvancedPropertiesCommand = new RelayCommand(AdvancedProperties);
        EnterPressedCommand = new RelayCommand(OnEnterPressed);
        // InitAuthenticationTypes();
        // InitSupportedAuthTypes();
        InfoConnection.UpdatePropertyInfo(ci);
    }

    private void ChannelOnConnectionPropertiesChanged(object sender, EventArgs eventArgs)
    {
        ResetDatabaseList();
    }

    private void AdvancedProperties()
    {
        /*
		if (ConnectionInfo.ServerDefinition == null)
		{
			ConnectionInfo.ServerDefinition = ServerDefinition.Default;
		}
		*/

        UIConnectionInfo uiConnectionInfo = (UIConnectionInfo)InfoConnection.ToUiConnectionInfo();

        FbConnectionStringBuilder csb = new();

		ConnectionStrategy.PopulateConnectionStringBuilder(csb, uiConnectionInfo);


        IServerConnectionProvider service = _ServerConnectionProviderServiceManager.GetService(InfoConnection.ServerDefinition);
        _Traceable.AssertTraceEvent(service != null, TraceEventType.Error, EnUiTraceId.Connection, "serverConnectionProvider is null");
        if (service == null)
        {
            return;
        }

        try
        {
            string connectionString = service.GetConnectionString((UIConnectionInfo)InfoConnection.ToUiConnectionInfo(), InfoConnection.ServerDefinition);
            IBPropertyAgent connectionProperties = _ConnectionPropertiesManager.GetService(InfoConnection.ServerDefinition).GetConnectionProperties(connectionString);
            _Channel.OnAdvancedPropertiesRequested(connectionProperties);
        }
        catch (Exception ex)
        {
            if (ValidateConnectionInfo())
            {
                _Channel.OnShowMessage(ex);
            }
        }
    }

    private void OnEnterPressed()
    {
        /*
		if (ConnectionInfo.ServerDefinition == null)
		{
			ConnectionInfo.ServerDefinition = ServerDefinition.Default;
		}
		*/

        if (CanTryToConnect())
        {
            _Channel.OnMakeConnection();
        }
    }

    private bool CanTryToConnect()
    {
        return !string.IsNullOrEmpty(InfoConnection.DataSource) && !string.IsNullOrEmpty(InfoConnection.Database);
    }

    public override IBPropertyAgent Copy()
    {
        return new ConnectionPropertySectionViewModel(Channel, _Traceable.DependencyManager, InfoConnection, this);
    }

    /*
	private CancellationToken CreateNewCancellationToken()
	{
		lock (_ConnectionTaskLock)
		{
			_DatabaseCancellationTokenSource.Cancel();
			_DatabaseCancellationTokenSource = new CancellationTokenSource();
			return _DatabaseCancellationTokenSource.Token;
		}
	}
	*/

    public bool ValidateConnectionInfo()
    {
        if (!InfoConnection.IsComplete)
        {
            _Channel.OnShowMessage(SharedResx.Error_MissingSomeConnectionProperties);
            return false;
        }

        return true;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task LoadDatabasesAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        try
        {
            if (!SetDatabaseAreLoaded(loaded: true))
                return;


            SetDatabaseList(new ObservableCollection<string>(new List<string> { SharedResx.LoadingText }));

            /*
			CancellationToken cancellationToken = CreateNewCancellationToken();
			UIConnectionInfo uiConnectionInfo = (UIConnectionInfo)InfoConnection.ToUiConnectionInfo();
			IDatabaseDiscoveryProvider service = _databaseDiscoveryProviderServiceManager.GetService(InfoConnection.ServerDefinition);
			if (service != null)
			{
				ServiceResponse<DatabaseInstanceInfo> serviceResponse = await service.GetDatabaseInstancesAsync(uiConnectionInfo, cancellationToken);
				if (!cancellationToken.IsCancellationRequested)
				{
					ObservableCollection<string> dataList = serviceResponse.Data == null ? new ObservableCollection<string>() : new ObservableCollection<string>(serviceResponse.Data.Select((DatabaseInstanceInfo x) => x.Name));
					SetDatabaseList(dataList);
					if (serviceResponse.HasError)
					{
						_Channel.OnExceptionOccurred(serviceResponse.Errors, EnFirewallRuleSource.DatabaseList);
						SetDatabaseAreLoaded(loaded: false);
					}
				}
			}
			else
			{
				ResetDatabaseList();
			}
			*/
        }
        catch (Exception exception)
        {
            _Traceable.TraceException(TraceEventType.Error, EnUiTraceId.Connection, exception, "Failed to load databases", 224, "ConnectionPropertySectionViewModel.cs", "LoadDatabasesAsync");
        }
    }

    private bool SetDatabaseAreLoaded(bool loaded)
    {
        bool result = false;
        lock (_DatabaseLoadingLock)
        {
            if (DatabasesAreLoaded != loaded)
            {
                DatabasesAreLoaded = loaded;
                return true;
            }

            return result;
        }
    }

    /*
	private ServerDefinition GetServerDefinition(Guid ServerType)
	{
		if (ServerType == SqlServerConnectionProvider.ServerType)
		{
			return new ServerDefinition("SqlServer", null);
		}

		return null;
	}
	*/

    /*
	private void InitAuthenticationTypes()
	{
		_AuthenticationCollection = new Dictionary<AuthenticationTypes, string>
		{
			{ AuthenticationTypes.SqlPassword, SharedResx.Authentication_SqlPassword }
		};
		_AuthenticationCollection.Add(AuthenticationTypes.Integrated, SharedResx.Authentication_Integrated);
		_AuthenticationCollection.Add(AuthenticationTypes.ActiveDirectoryPassword, SharedResx.Authentication_ActiveDirectoryPassword);
		_AuthenticationCollection.Add(AuthenticationTypes.ActiveDirectoryIntegrated, SharedResx.Authentication_ActiveDirectoryIntegrated);
		if (CommonUtil.IsInteractiveAuthenicationSupported())
		{
			_AuthenticationCollection.Add(AuthenticationTypes.ActiveDirectoryInteractive, SharedResx.Authentication_ActiveDirectoryInteractive);
		}
	}
	*/

    public override void Dispose(bool disposing)
    {
        if (!_IsDisposed && disposing)
        {
            if (_Channel != null)
            {
                _Channel.ResetConnectionProperty -= ResetAuthenticationAndDatabases;
                _Channel.AuthenticationTypeChanged -= OnAuthenticationTypeChanged;
                _Channel.ConnectionPropertiesChanged -= ChannelOnConnectionPropertiesChanged;
            }

            _IsDisposed = true;
        }
    }

    /*
	private void InitSupportedAuthTypes(IBServerDefinition serverDefinition = null)
	{
		if (!SetSupportedAuthenticationTypes(serverDefinition))
		{
			AuthenticationOptions = _AuthenticationCollection;
		}
	}
	*/

    public void UpdateConnectionProperty(ConnectionInfo connectionInfo = null)
    {
        lock (_UpdatingPropertiesLock)
        {
            _IsUpdatingProperties = true;
            IBServerDefinition serverDefinition = connectionInfo?.ServerDefinition;
            // InitSupportedAuthTypes(serverDefinition);
            if (connectionInfo != null)
            {
                InfoConnection.SetConnectionInfo(connectionInfo);
            }

            string ciMoniker = null;
            if (connectionInfo != null && connectionInfo.IsComplete)
            {
                MonikerAgent moniker = new(connectionInfo, true);
                ciMoniker = moniker.Moniker;
            }


            _IsUpdatingProperties = false;
            ResetDatabaseList(ciMoniker);
            if (!string.IsNullOrEmpty(ciMoniker) && !InfoConnection.AreEquivalent(connectionInfo))
            {

                connectionInfo.CopyTo(this);
            }
        }
    }

    private void ResetAuthenticationAndDatabases(object sender, EventArgs e)
    {
        if (!_IsUpdatingProperties)
        {
            /*
			AuthenticationOptions = _AuthenticationCollection;
			if (InfoConnection.ServerDefinition != ServerDefinition.Default)
			{
				InfoConnection.ServerDefinition = ServerDefinition.Default;
			}
			*/

            ResetDatabaseList();
        }
    }

    private void ResetDatabaseList(string defaultDbName = null)
    {
        if (!_IsUpdatingProperties)
        {
            SetDatabaseAreLoaded(loaded: false);
            SetDatabaseList(null, defaultDbName);
        }
    }

    public void OnAuthenticationTypeChanged(object sender, EventArgs e)
    {
        /*
		IsUserNameEnabled = !ConnectionInfoUtil.IsAnyIntegratedAuth(InfoConnection.AuthenticationType);
		IsPasswordEnabled = ConnectionInfoUtil.IsAnyPasswordAuth(InfoConnection.AuthenticationType);
		ResetDatabaseList();
		*/
    }

    private void SetDatabaseList(ObservableCollection<string> dataList = null, string defaultMoniker = null)
    {
        ObservableCollection<string> observableCollection = dataList ?? new ObservableCollection<string>();

        string ciMoniker = null;
        string thisMoniker = null;

        MonikerAgent moniker = new(true);

        if (InfoConnection != null && InfoConnection.IsComplete)
        {
            moniker.Parse(InfoConnection);
            ciMoniker = moniker.Moniker;

            if (defaultMoniker == null)
            {
                defaultMoniker = ciMoniker;
                ciMoniker = null;
            }
            else if (ciMoniker == defaultMoniker)
            {
                ciMoniker = null;
            }
        }

        if (IsComplete)
        {
            moniker.Parse(this);
            thisMoniker = moniker.Moniker;

            if (defaultMoniker == null)
            {
                defaultMoniker = thisMoniker;
                thisMoniker = null;
            }
            else if (thisMoniker == defaultMoniker
                || ciMoniker != null && thisMoniker == ciMoniker)
            {
                thisMoniker = null;
            }
        }

        int pos = 0;

        if (defaultMoniker != null && !observableCollection.Contains(defaultMoniker))
        {
            observableCollection.Insert(pos, defaultMoniker);
            pos++;
        }
        if (ciMoniker != null && !observableCollection.Contains(ciMoniker))
        {
            observableCollection.Insert(pos, ciMoniker);
            pos++;
        }
        if (thisMoniker != null && !observableCollection.Contains(thisMoniker))
        {
            observableCollection.Insert(pos, thisMoniker);
            pos++;
        }

        string str;
        DataTable table = XmlParser.Databases;

        foreach (DataRow row in table.Rows)
        {
            str = (string)row["InitialCatalog"];

            if (str == "")
                continue;

            moniker.User = (string)row["UserName"];
            moniker.Password = (string)row["Password"];
            moniker.Server = (string)row["DataSource"];
            moniker.Database = str;
            moniker.Port = (int)row["PortNumber"];
            moniker.Dataset = (string)row["Name"];

            str = moniker.Moniker;

            if (!observableCollection.Contains(str))
                observableCollection.Add(str);
        }

        _ThreadSafeCollection.SetDataList(observableCollection ?? new ObservableCollection<string>());
        Application.Current?.Dispatcher.Invoke(delegate
            {
                RaisePropertyChanged("DatabaseList");
                // InfoConnection.RaisePropertyChanged("Dataset");
            });
    }

    /*
	private bool SetSupportedAuthenticationTypes(IBServerDefinition serverDefinition = null)
	{
		if (serverDefinition != null)
		{
			IAuthenticationTypeProvider service = _authenticationTypeServiceManager.GetService(serverDefinition);
			if (service != null)
			{
				AuthenticationTypes supportedTypes = service.SupportedAuthentications;
				AuthenticationOptions = _AuthenticationCollection.Where((KeyValuePair<AuthenticationTypes, string> keyValue) => supportedTypes.HasFlag(keyValue.Key)).ToDictionary((keyValue) => keyValue.Key, (keyValue) => keyValue.Value);
				return true;
			}
		}

		return false;
	}
	*/
}
