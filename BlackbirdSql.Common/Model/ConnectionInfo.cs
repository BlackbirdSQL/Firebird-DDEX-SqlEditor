#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.IO;
using System.Text;

using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Properties;




namespace BlackbirdSql.Common.Model;


public class ConnectionInfo : AbstractDispatcherConnection
{
	protected const string C_KeyAuthenticationType = "AuthenticationType";
	protected const string C_KeyIsFavorite = "IsFavorite";
	protected const string C_KeyServerTypeTitle = "ServerTypeTitle";

	// private string _MruConnectionString;

	/*
	public AuthenticationTypes AuthenticationType
	{
		get
		{
			return (AuthenticationTypes)GetProperty(C_KeyAuthenticationType);
		}
		set
		{
			if (SetProperty(C_KeyAuthenticationType, value))
				OnAuthenticationChanged();
		}
	}
	*/

	public bool IsFavorite
	{
		get { return (bool)GetProperty(C_KeyIsFavorite); }
		set { SetProperty(C_KeyIsFavorite, value); }
	}

	public string ServerTypeTitle
	{
		get { return (string)GetProperty(C_KeyServerTypeTitle); }
		private set { SetProperty(C_KeyServerTypeTitle, value); }
	}


	public string ServerInfo
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(DataSource);
			stringBuilder.Append("[" + UserID + "]");
			return stringBuilder.ToString();
		}
	}

	public string ListItemAutomationName =>
		string.Format(SharedResx.ListViewItemAutomationProperty, Dataset, ServerInfo);



	public static string DomainUserName => Environment.UserDomainName + "\\" + Environment.UserName;



	public ConnectionInfo(IBEventsChannel channel = null, ConnectionInfo rhs = null, bool generateNewId = true)
		: base(channel, rhs, generateNewId)
	{
		Add(C_KeyAuthenticationType, typeof(int), false);
		Add(C_KeyIsFavorite, typeof(bool), false);
		Add(C_KeyServerTypeTitle, typeof(string), "");
	}

	public ConnectionInfo(ConnectionInfo rhs, bool generateNewId = true) : this(null, rhs, generateNewId)
	{
	}

	public static ConnectionInfo CreateFromMruInfo(MruInfo mruInfo, IBEventsChannel channel = null)
	{
		Cmd.CheckForNull(mruInfo, "mruInfo");
		ConnectionInfo connectionInfo = new(channel, mruInfo.ConnectionInfo as ConnectionInfo)
		{
			IsFavorite = mruInfo.IsFavorite,
			// _MruConnectionString = mruInfo.PropertyString
		};
		return connectionInfo;
	}

	public override IBPropertyAgent Copy()
	{
		return new ConnectionInfo(this, generateNewId: true);
	}

	public void SetConnectionInfo(ConnectionInfo connection)
	{
		if (connection != null)
		{
			UpdatePropertyInfo(connection.ToUiConnectionInfo());
			ServerTypeTitle = connection.ServerTypeTitle;
			IsFavorite = connection.IsFavorite;
		}
	}


	protected string GetEngineProduct(IBServerDefinition definition)
	{
		return definition != null ? definition.EngineProduct : "Firebird";
	}

	public static string GetEngineType(IBServerDefinition definition)
	{
		if (definition == null)
			return "Unknown";

		return definition.EngineType.ToString();

	}



	/*
	private void OnAuthenticationChanged()
	{
		if (ConnectionInfoUtil.IsAnyIntegratedAuth(AuthenticationType))
		{
			UserName = DomainUserName;
		}
		else
		{
			UserName = null;
		}

		UserID = null;


		if (!string.IsNullOrEmpty(Password))
		{
			Password = null;
		}

		PersistPassword = false;

		if (_Channel != null)
			_Channel.OnAuthenticationTypeChanged();
	}
	*/


	public MruInfo ToMruInfo()
	{
		return new MruInfo(this, IsFavorite, ServerDefinition);
	}



	public static bool IsDatabaseNameValid(string databaseName)
	{
		if (!string.IsNullOrEmpty(databaseName) && Path.GetFileNameWithoutExtension(databaseName) != "")
		{
			return !SharedResx.LoadingText.Equals(databaseName, StringComparison.OrdinalIgnoreCase);
		}

		return false;
	}

}
