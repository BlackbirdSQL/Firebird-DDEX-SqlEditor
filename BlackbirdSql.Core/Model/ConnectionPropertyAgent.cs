// Microsoft.SqlServer.RegSvrEnum, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.Smo.RegSvrEnum.UIConnectionInfo

using System.Xml;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Sys;



namespace BlackbirdSql.Core.Model;

// Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0660
// Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning disable CS0661
public class ConnectionPropertyAgent(IBPropertyAgent lhs, bool generateNewId)
	: AbstractModelPropertyAgent(lhs, generateNewId)
{

	public ConnectionPropertyAgent() : this(null, true)
	{
	}

	public ConnectionPropertyAgent(IBPropertyAgent lhs) : this(lhs, true)
	{
	}




	// private const string XmlStart = "ConnectionInformation";

	// private const string XmlServerType = "ServerType";

	// private const string XmlServerName = "DataSource";

	// private const string XmlDisplayName = "DatasetId";

	// private const string XmlUserName = "UserID";

	// private const string XmlPassword = "Password";

	// private const string XmlAuthenticationType = "AuthenticationType";

	// private const string XmlAdvancedOptions = "AdvancedOptions";

	// private const string XmlItemTypeAttribute = "type";





	public static bool operator ==(ConnectionPropertyAgent infoA, ConnectionPropertyAgent infoB)
	{
		bool result = false;
		bool flag = infoA is null;
		bool flag2 = infoB is null;
		if (flag && flag2)
		{
			result = true;
		}
		else if (!flag && !flag2)
		{
			result = infoA.Equals(infoB);
		}

		return result;
	}

	public static bool operator ==(ConnectionPropertyAgent infoA, object infoB)
	{
		bool result = false;
		bool flag = infoA is null;
		bool flag2 = infoB == null;
		if (flag && flag2)
		{
			result = true;
		}
		else if (!flag && !flag2)
		{
			result = infoA.Equals(infoB);
		}

		return result;
	}

	public static bool operator ==(object infoA, ConnectionPropertyAgent infoB)
	{
		bool result = false;
		bool flag = infoA == null;
		bool flag2 = infoB is null;
		if (flag && flag2)
		{
			result = true;
		}
		else if (!flag && !flag2)
		{
			result = infoB.Equals(infoA);
		}

		return result;
	}

	public static bool operator !=(ConnectionPropertyAgent infoA, ConnectionPropertyAgent infoB)
	{
		return !(infoA == infoB);
	}

	public static bool operator !=(ConnectionPropertyAgent infoA, object infoB)
	{
		return !(infoA == infoB);
	}

	public static bool operator !=(object infoA, ConnectionPropertyAgent infoB)
	{
		return !(infoA == infoB);
	}



	public override IBPropertyAgent Copy()
	{
		return new ConnectionPropertyAgent(this, generateNewId: true);
	}



	protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
	{
		// Just pass any request down. This class uses it's parent's descriptor.
		AbstractModelPropertyAgent.CreateAndPopulatePropertySet(describers);
	}



	public static ConnectionPropertyAgent CreateFromStream(XmlReader reader)
	{
		ConnectionPropertyAgent connectionInfo = new ConnectionPropertyAgent();

		connectionInfo.LoadFromStream(reader);

		return connectionInfo;
	}



}
#pragma warning restore CS0661
#pragma warning restore CS0660
