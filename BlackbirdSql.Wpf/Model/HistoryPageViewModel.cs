#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Wpf.Model
{
	public class HistoryPageViewModel : ViewModelBase
    {
        public const string C_KeyHasRecentConnections = "HasRecentConnections";
        public const string C_KeyHasFavoriteConnections = "HasFavoriteConnections";
        public const string C_KeySelectedConnection = "SelectedConnection";
        public const string C_KeyIsConnectionExpanded = "IsConnectionExpanded";

        protected static new DescriberDictionary _Describers;


        private ObservableCollection<ConnectionInfo> _recentConnectionList;

        private ObservableCollection<ConnectionInfo> _favoriteConnectionList;

        // private readonly ConnectionInfo _selectedConnection;

        // private readonly bool _hasFavoriteConnections;

        // private readonly bool _hasRecentConnections;

        // private readonly bool _IsConnectionExpanded;

        private readonly ConnectionPropertySectionViewModel _connectionPropertyViewModel;

        private readonly IBConnectionMruManager _MruManager;

        public const int C_MaxFavoriteConnections = 10;



        public override DescriberDictionary Describers
        {
            get
            {
                if (_Describers == null)
                    CreateAndPopulatePropertySet(null);

                return _Describers;
            }
        }

        public ICommand PinUnpinCommand { get; private set; }

        public ICommand RemoveCommand { get; private set; }

        public Traceable Trace { get; set; }

        public bool IsConnectionListEmpty
        {
            get
            {
                if (FavoriteConnectionList.Count == 0 && RecentConnectionList.Count == 0)
                {
                    return true;
                }

                return false;
            }
        }


        public bool HasRecentConnections
        {
            get { return (bool)GetProperty(C_KeyHasRecentConnections); }
            set { SetProperty(C_KeyHasRecentConnections, value); }
        }


        public bool HasFavoriteConnections
        {
            get { return (bool)GetProperty(C_KeyHasFavoriteConnections); }
            set { SetProperty(C_KeyHasFavoriteConnections, value); }
        }


        public ObservableCollection<ConnectionInfo> RecentConnectionList
        {
            get
            {
                return _recentConnectionList;
            }
            set
            {
                _recentConnectionList = value;
            }
        }

        public ObservableCollection<ConnectionInfo> FavoriteConnectionList
        {
            get
            {
                return _favoriteConnectionList;
            }
            set
            {
                _favoriteConnectionList = value;
            }
        }

        public ConnectionInfo SelectedConnection
        {
            get { return (ConnectionInfo)GetProperty(C_KeySelectedConnection); }
            set { SetProperty(C_KeySelectedConnection, value); }
        }


        public bool IsConnectionExpanded
        {
            get
            {
                return (bool)GetProperty(C_KeyIsConnectionExpanded);
            }
            set
            {
                if (SetProperty(C_KeyIsConnectionExpanded, value))
                    ConnectionPropertyViewModel.IsConnectionExpanded = value;
            }
        }


        public ConnectionPropertySectionViewModel ConnectionPropertyViewModel => _connectionPropertyViewModel;

        public HistoryPageViewModel(IBDependencyManager dependencyManager, ConnectionPropertySectionViewModel connectionPropertyViewModel, IBEventsChannel channel, IBConnectionMruManager mruManager, bool isConnectionExpanded)
        {
            Cmd.CheckForNull(dependencyManager, "dependencyManager");
            Cmd.CheckForNull(connectionPropertyViewModel, "connectionPropertyViewModel");
            Cmd.CheckForNull(channel, "channel");
            Cmd.CheckForNull(mruManager, "mruManager");
            Trace = new Traceable(dependencyManager);
            if (connectionPropertyViewModel == null || channel == null)
            {
                return;
            }

            _connectionPropertyViewModel = connectionPropertyViewModel;
            _Channel = channel;
            PinUnpinCommand = new RelayCommand(delegate (object p)
            {
                ExecuteConnectionInfoCommand(p, PinUnpinConnection);
            });
            RemoveCommand = new RelayCommand(delegate (object p)
            {
                ExecuteConnectionInfoCommand(p, RemoveConnection);
            });
            _MruManager = mruManager;
            _recentConnectionList = new ObservableCollection<ConnectionInfo>();
            _favoriteConnectionList = new ObservableCollection<ConnectionInfo>();
            foreach (MruInfo mru in _MruManager.GetMruList())
            {
                if (mru.IsFavorite)
                {
                    _favoriteConnectionList.Add(ConnectionInfo.CreateFromMruInfo(mru));
                }
                else
                {
                    _recentConnectionList.Add(ConnectionInfo.CreateFromMruInfo(mru));
                }
            }

            IsConnectionExpanded = IsConnectionListEmpty || isConnectionExpanded;

            OnConnectionListsChanged();
        }

        public void OnMakeConnection()
        {
            _Channel.OnMakeConnection();
        }

        private void ExecuteConnectionInfoCommand(object p, Action<ConnectionInfo> executeCommand)
        {
            if (p != null)
            {
                ConnectionInfo connectionInfo = p as ConnectionInfo;
                Trace.AssertTraceEvent(connectionInfo != null, TraceEventType.Error, EnUiTraceId.HistoryPage, "connection is null");
                if (connectionInfo != null)
                {
                    executeCommand(connectionInfo);
                }
            }
        }

        private void PinUnpinConnection(ConnectionInfo connection)
        {
            if (connection.IsFavorite)
            {
                connection.IsFavorite = false;
                RecentConnectionList.Add(connection);
                FavoriteConnectionList.Remove(connection);
            }
            else
            {
                if (FavoriteConnectionList.Count == C_MaxFavoriteConnections)
                {
                    _Channel.OnShowMessage(SharedResx.Error_ReachMaximumNumberOfFavoriteConnection);
                    return;
                }

                connection.IsFavorite = true;
                RecentConnectionList.Remove(connection);
                FavoriteConnectionList.Add(connection);
            }

            MruInfo mruInfo = connection.ToMruInfo();
            _MruManager.UpdateFavorite(mruInfo.PropertyString, mruInfo.IsFavorite, mruInfo.ServerDefinition);
            OnConnectionListsChanged();
        }

        private void RemoveConnection(ConnectionInfo connection)
        {
            if (connection.IsFavorite ? FavoriteConnectionList.Remove(connection) : RecentConnectionList.Remove(connection))
            {
                MruInfo mruInfo = connection.ToMruInfo();
                _MruManager.RemoveConnection(mruInfo.PropertyString);
                OnConnectionListsChanged();
            }
        }

        public void OnConnectionListsChanged()
        {
            HasFavoriteConnections = FavoriteConnectionList.Count != 0;
            HasRecentConnections = RecentConnectionList.Count != 0;
        }

        public void UpdateSelectedConnection(ConnectionInfo connection)
        {
            ConnectionPropertyViewModel.UpdateConnectionProperty(connection);
        }
    }
}
