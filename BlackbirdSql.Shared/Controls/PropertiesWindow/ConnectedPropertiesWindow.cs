﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyWindow.SqlConnectionPropertiesDisplay

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.ComponentModel;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Controls.PropertiesWindow;


public class ConnectedPropertiesWindow : AbstractPropertiesWindow, IBsConnectedPropertiesWindow
{

	public ConnectedPropertiesWindow(ConnectionStrategy strategy) : base()
	{
		Strategy = strategy;
	}


	private bool _Initialized;
	private Csb _Csa = null;
	private string _ServerVersion = null;
	private string _ClientVersion = null;





	[Browsable(false)]
	private ConnectionStrategy Strategy { get; set; }

	[Browsable(false)]
	private QueryManager QryMgr { get; set; }

	[Browsable(false)]
	private IDbConnection Connection => Strategy.Connection;

	[GlobalizedCategory("PropertyWindowConnectionDetails")]
	[GlobalizedDescription("PropertyWindowNameDescription")]
	[GlobalizedDisplayName("PropertyWindowNameDisplayName")]
	public string Name
	{
		get
		{
			ValidateStoredConnection();
			if (_Csa != null)
				return _Csa.DatasetKey;

			return "";
		}
	}

	[GlobalizedCategory("PropertyWindowConnectionDetails")]
	[GlobalizedDescription("PropertyWindowServerNameDescription")]
	[GlobalizedDisplayName("PropertyWindowServerNameDisplayName")]
	public string DataSource
	{
		get
		{
			if (Connection != null)
			{
				return ((DbConnection)Connection).DataSource;
			}

			return "";
		}
	}

	[GlobalizedCategory("PropertyWindowConnectionDetails")]
	[GlobalizedDescription("PropertyWindowServerVersionDescription")]
	[GlobalizedDisplayName("PropertyWindowServerVersionDisplayName")]
	public string ServerVersion
	{
		get
		{
			ValidateStoredConnection();

			if (!string.IsNullOrEmpty(_ServerVersion))
				return _ServerVersion;


			if (Connection != null && Connection.State == ConnectionState.Open)
				_ServerVersion = Connection.GetDataSourceVersion();
			else
				_ServerVersion = "";

			return _ServerVersion;
		}
	}

	[GlobalizedCategory("PropertyWindowConnectionDetails")]
	[GlobalizedDescription("PropertyWindowClientVersionDescription")]
	[GlobalizedDisplayName("PropertyWindowClientVersionDisplayName")]
	public string ClientVersion => _ClientVersion ??= NativeDb.ClientVersion;


	[GlobalizedCategory("PropertyWindowConnectionDetails")]
	[GlobalizedDescription("PropertyWindowDatabaseDescription")]
	[GlobalizedDisplayName("PropertyWindowDatabaseDisplayName")]
	public string Database
	{
		get
		{
			if (Connection != null)
				return ((DbConnection)Connection).Database;

			return "";
		}
	}

	[GlobalizedCategory("PropertyWindowConnectionDetails")]
	[GlobalizedDescription("PropertyWindowPortDescription")]
	[GlobalizedDisplayName("PropertyWindowPortDisplayName")]
	public int Port
	{
		get
		{
			ValidateStoredConnection();
			if (_Csa != null)
				return _Csa.Port;

			return 0;
		}
	}

	[GlobalizedCategory("PropertyWindowConnectionDetails")]
	[GlobalizedDescription("PropertyWindowConnectionStringDescription")]
	[GlobalizedDisplayName("PropertyWindowConnectionStringDisplayName")]
	public string ConnectionString
	{
		get
		{
			ValidateStoredConnection();

			return _Csa?.ToDisplayString() ?? "";
		}
	}


	[GlobalizedCategory("PropertyWindowQueryExecutionDetailsCategory")]
	[GlobalizedDescription("PropertyWindowStartTimeDescription")]
	[GlobalizedDisplayName("PropertyWindowStartTimeDisplayName")]
	public string StartTime
	{
		get
		{
			if (!(QryMgr?.QueryExecutionStartTime.HasValue ?? false))
			{
				return "";
			}

			return FormatTime(QryMgr.QueryExecutionStartTime.Value);
		}
	}

	[GlobalizedCategory("PropertyWindowQueryExecutionDetailsCategory")]
	[GlobalizedDescription("PropertyWindowFinishTimeDescription")]
	[GlobalizedDisplayName("PropertyWindowFinishTimeDisplayName")]
	public string FinishTime
	{
		get
		{
			if (QryMgr == null || !QryMgr.QueryExecutionEndTime.HasValue)
			{
				return "";
			}

			return FormatTime(QryMgr.QueryExecutionEndTime.Value);
		}
	}

	[GlobalizedCategory("PropertyWindowQueryExecutionDetailsCategory")]
	[GlobalizedDescription("PropertyWindowElapsedTimeDescription")]
	[GlobalizedDisplayName("PropertyWindowElapsedTimeDisplayName")]
	public string ElapsedTime
	{
		get
		{
			if (QryMgr == null || !QryMgr.QueryExecutionEndTime.HasValue || !QryMgr.QueryExecutionStartTime.HasValue)
			{
				return "";
			}

			return (QryMgr.QueryExecutionEndTime.Value - QryMgr.QueryExecutionStartTime.Value).Fmt();
		}
	}

	[GlobalizedCategory("PropertyWindowQueryExecutionDetailsCategory")]
	[GlobalizedDescription("PropertyWindowTotalRowsReturnedDescription")]
	[GlobalizedDisplayName("PropertyWindowTotalRowsReturnedDisplayName")]
	public string TotalRowsReturned
	{
		get
		{
			if (QryMgr != null)
			{
				return QryMgr.RowsAffected.ToString(CultureInfo.CurrentCulture);
			}

			return "";
		}
	}

	[GlobalizedCategory("PropertyWindowConnectionDetails")]
	[GlobalizedDescription("PropertyWindowStateDescription")]
	[GlobalizedDisplayName("PropertyWindowStateDisplayName")]
	public string State
	{
		get
		{
			string result = "";
			if (Connection != null)
			{
				result = GetStateString(Connection.State);
			}

			return result;
		}
	}

	[GlobalizedCategory("PropertyWindowConnectionDetails")]
	[GlobalizedDescription("PropertyWindowLoginNameDescription")]
	[GlobalizedDisplayName("PropertyWindowLoginNameDisplayName")]
	public string LoginName
	{
		get
		{
			ValidateStoredConnection();
			if (_Csa != null)
				return _Csa.UserID;

			return "";
		}

	}

	public bool IsInitialized()
	{
		return _Initialized;
	}

	public void Initialize(QueryManager qryMgr)
	{
		QryMgr = qryMgr;
		_Initialized = true;
	}
	public override string GetClassName()
	{
		return AttributeResources.PropertyWindowCurrentConnectionParameters;
	}


	private string FormatTime(DateTime time)
	{
		if (!(time != DateTime.MinValue))
		{
			return "";
		}

		return time.ToString(CultureInfo.CurrentCulture);
	}


	private string GetStateString(ConnectionState state)
	{
		return state switch
		{
			ConnectionState.Broken => ControlsResources.PropertiesWindow_ConnectionStateBroken,
			ConnectionState.Closed => ControlsResources.PropertiesWindow_ConnectionStateClosed,
			ConnectionState.Connecting => ControlsResources.PropertiesWindow_ConnectionStateConnecting,
			ConnectionState.Executing => ControlsResources.ConnectionStateExecuting,
			ConnectionState.Fetching => ControlsResources.PropertiesWindow_ConnectionStateFetching,
			_ => ControlsResources.PropertiesWindow_ConnectionStateOpen,
		};
	}

	private void ValidateStoredConnection()
	{
		if (Connection != null)
		{
			if (_Csa == null || _Csa.IsInvalidated)
			{
				_Csa = RctManager.ShutdownState ? null : RctManager.CloneVolatile(Connection);
				_ServerVersion = null;
			}
			return;
		}

		if (_Csa != null)
		{
			_Csa = null;
			_ServerVersion = null;
		}
	}

}
