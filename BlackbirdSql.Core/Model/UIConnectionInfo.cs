// Microsoft.SqlServer.RegSvrEnum, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.Smo.RegSvrEnum.UIConnectionInfo

using System.Xml;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Interfaces;



namespace BlackbirdSql.Core.Model;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public class UIConnectionInfo : AbstractModelPropertyAgent
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
	// private const string XmlStart = "ConnectionInformation";

	// private const string XmlServerType = "ServerType";

	// private const string XmlServerName = "ServerName";

	// private const string XmlDisplayName = "DisplayName";

	// private const string XmlUserName = "UserName";

	// private const string XmlPassword = "Password";

	// private const string XmlAuthenticationType = "AuthenticationType";

	// private const string XmlAdvancedOptions = "AdvancedOptions";

	// private const string XmlItemTypeAttribute = "type";





	public static bool operator ==(UIConnectionInfo infoA, UIConnectionInfo infoB)
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

	public static bool operator ==(UIConnectionInfo infoA, object infoB)
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

	public static bool operator ==(object infoA, UIConnectionInfo infoB)
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

	public static bool operator !=(UIConnectionInfo infoA, UIConnectionInfo infoB)
	{
		return !(infoA == infoB);
	}

	public static bool operator !=(UIConnectionInfo infoA, object infoB)
	{
		return !(infoA == infoB);
	}

	public static bool operator !=(object infoA, UIConnectionInfo infoB)
	{
		return !(infoA == infoB);
	}




	public UIConnectionInfo() : this(null, true)
	{
	}

	public UIConnectionInfo(IBPropertyAgent lhs, bool generateNewId) : base(lhs, generateNewId)
	{
	}

	public UIConnectionInfo(IBPropertyAgent lhs) : this(lhs, true)
	{
	}



	public override IBPropertyAgent Copy()
	{
		return new UIConnectionInfo(this, generateNewId: true);
	}



	protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
	{
		// Just pass any request down. This class uses it's parent's descriptor.
		AbstractModelPropertyAgent.CreateAndPopulatePropertySet(describers);
	}



	public static UIConnectionInfo CreateFromStream(XmlReader reader)
	{
		UIConnectionInfo connectionInfo = new UIConnectionInfo();

		connectionInfo.LoadFromStream(reader);

		return connectionInfo;
	}



}
