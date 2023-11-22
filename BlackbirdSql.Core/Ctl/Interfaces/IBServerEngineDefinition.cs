// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.IServerDefinition

using BlackbirdSql.Core.Ctl.Enums;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBServerEngineDefinition
{
	EnEngineType EngineType { get; }

	string Key { get; }
}

public static class ServerDefinitionExtensionMembers
{

	internal static bool EqualsServerDefinition(this IBServerEngineDefinition serverDefinition, IBServerEngineDefinition otherServerDefinition)
	{
		if (serverDefinition == null && otherServerDefinition == null)
			return true;

		if (serverDefinition == null || otherServerDefinition == null)
			return false;

		return serverDefinition.HasSameEngineType(otherServerDefinition);
	}

	public static bool HasSameEngineType(this IBServerEngineDefinition serverDefinition, IBServerEngineDefinition metadata)
	{
		if (serverDefinition != null && metadata != null)
			return (int)serverDefinition.EngineType == (int)metadata.EngineType;

		return false;
	}




	public static string ToString(this EnEngineType value)
	{
		switch (value)
		{
			case EnEngineType.Unknown:
				return "Unknown";
			case EnEngineType.LocalClassicServer:
				return "Local Classic Server";
			case EnEngineType.LocalSuperServer:
				return "Local SuperServer";
			case EnEngineType.ClassicServer:
				return "Classic Server";
			case EnEngineType.SuperServer:
				return "SuperServer";
			case EnEngineType.EmbeddedDatabase:
				return "Embedded Database";
			default:
				return "Unknown";

		}
	}


}
