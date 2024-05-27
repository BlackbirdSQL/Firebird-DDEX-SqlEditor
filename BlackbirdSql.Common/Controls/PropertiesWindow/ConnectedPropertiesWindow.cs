// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyWindow.SqlConnectionPropertiesDisplay

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl.ComponentModel;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Model;



namespace BlackbirdSql.Common.Controls.PropertiesWindow
{
	public class ConnectedPropertiesWindow(SqlConnectionStrategy connectionStrategy)
		: AbstractPropertiesWindow, IBPropertyWindowQueryManagerInitialize
	{
		private bool _Initialized;
		private Csb _Csa = null;
		private string _ConnectionInfo = null;
		private string _ServerVersion = null;
		private string _ClientVersion = null;





		[Browsable(false)]
		private SqlConnectionStrategy Strategy { get; set; } = connectionStrategy;

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

				return string.Empty;
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

				return string.Empty;
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

				if (_ServerVersion != null)
					return _ServerVersion;

				if (Connection != null && Connection.State == ConnectionState.Open)
					_ServerVersion = Connection.GetDataSourceVersion();
				else
					_ServerVersion = string.Empty;

				return _ServerVersion;
			}
		}

		[GlobalizedCategory("PropertyWindowConnectionDetails")]
		[GlobalizedDescription("PropertyWindowClientVersionDescription")]
		[GlobalizedDisplayName("PropertyWindowClientVersionDisplayName")]
		public string ClientVersion => _ClientVersion ??= DbNative.ClientVersion;


		[GlobalizedCategory("PropertyWindowConnectionDetails")]
		[GlobalizedDescription("PropertyWindowDatabaseDescription")]
		[GlobalizedDisplayName("PropertyWindowDatabaseDisplayName")]
		public string Database
		{
			get
			{
				if (Connection != null)
					return ((DbConnection)Connection).Database;

				return string.Empty;
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

				if (_Csa == null)
					return string.Empty;

				Csb csa = new(_Csa.ConnectionString);

				if (csa.ContainsKey(SysConstants.C_KeyPassword))
					csa.Password = "********";

				return csa.ConnectionString;
			}
		}


		[GlobalizedCategory("PropertyWindowQueryExecutionDetailsCategory")]
		[GlobalizedDescription("PropertyWindowStartTimeDescription")]
		[GlobalizedDisplayName("PropertyWindowStartTimeDisplayName")]
		public string StartTime
		{
			get
			{
				if (QryMgr == null || !QryMgr.QueryExecutionStartTime.HasValue)
				{
					return string.Empty;
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
					return string.Empty;
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
					return string.Empty;
				}

				return (QryMgr.QueryExecutionEndTime.Value - QryMgr.QueryExecutionStartTime.Value).FmtSqlStats();
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

				return string.Empty;
			}
		}

		[GlobalizedCategory("PropertyWindowConnectionDetails")]
		[GlobalizedDescription("PropertyWindowStateDescription")]
		[GlobalizedDisplayName("PropertyWindowStateDisplayName")]
		public string State
		{
			get
			{
				string result = string.Empty;
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

				return string.Empty;
			}

		}

		[GlobalizedCategory("PropertyWindowConnectionDetails")]
		[GlobalizedDescription("PropertyWindowSessionIDDescription")]
		[GlobalizedDisplayName("PropertyWindowSessionIDDisplayName")]
		public string SessionTracingID => Strategy.TracingID;

		[GlobalizedCategory("PropertyWindowConnection")]
		[GlobalizedDescription("PropertyWindowConnInfoDescription")]
		[GlobalizedDisplayName("PropertyWindowConnInfoDisplayName")]
		public string ConnectionInfo
		{
			get
			{
				ValidateStoredConnection();

				if (_ConnectionInfo != null)
					return _ConnectionInfo;

				if (_Csa != null)
				{
					_ConnectionInfo = string.Format(CultureInfo.CurrentCulture, ControlsResources.PropertyWindowConnectionInfo, _Csa.DatasetKey, _Csa.UserID);
				}
				else
				{
					_ConnectionInfo = string.Empty;
				}

				return _ConnectionInfo;
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
				return string.Empty;
			}

			return time.ToString(CultureInfo.CurrentCulture);
		}


		private string GetStateString(ConnectionState state)
		{
			return state switch
			{
				ConnectionState.Broken => ControlsResources.ConnectionStateBroken,
				ConnectionState.Closed => ControlsResources.ConnectionStateClosed,
				ConnectionState.Connecting => ControlsResources.ConnectionStateConnecting,
				ConnectionState.Executing => ControlsResources.ConnectionStateExecuting,
				ConnectionState.Fetching => ControlsResources.ConnectionStateFetching,
				_ => ControlsResources.ConnectionStateOpen,
			};
		}

		private void ValidateStoredConnection()
		{
			if (Connection != null)
			{
				if (_Csa == null || _Csa.Invalidated(Connection))
				{
					_Csa = RctManager.ShutdownState ? null : RctManager.CloneVolatile(Connection);

					_ConnectionInfo = null;
					_ServerVersion = null;
				}
				return;
			}

			if (_Csa != null)
			{
				_Csa = null;
				_ConnectionInfo = null;
				_ServerVersion = null;
			}
		}

	}
}
