// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.IServerDefinition

using System;
using BlackbirdSql.Core.Ctl.Enums;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBServerDefinition
{

	string EngineProduct { get; }

	EnEngineType EngineType { get; }

	string Key { get; }
}

public static class ServerDefinitionExtensionMembers
{

	internal static bool EqualsServerDefinition(this IBServerDefinition serverDefinition, IBServerDefinition otherServerDefinition)
	{
		if (serverDefinition == null && otherServerDefinition == null)
		{
			return true;
		}

		if (serverDefinition != null && otherServerDefinition != null)
		{
			if (string.IsNullOrEmpty(serverDefinition.EngineProduct) && string.IsNullOrEmpty(otherServerDefinition.EngineProduct) || serverDefinition.HasSameEngine(otherServerDefinition))
			{
				return serverDefinition.HasSameType(otherServerDefinition);
			}

			return false;
		}

		return false;
	}

	public static bool HasSameEngine(this IBServerDefinition serverDefinition, IBServerDefinition metadata)
	{
		if (serverDefinition != null && metadata != null)
		{
			return string.Equals(serverDefinition.EngineProduct, metadata.EngineProduct, StringComparison.OrdinalIgnoreCase);
		}

		return false;
	}

	public static bool HasSameType(this IBServerDefinition serverDefinition, IBServerDefinition metadata)
	{
		if (serverDefinition != null && metadata != null)
		{
			return serverDefinition.EngineType == metadata.EngineType;
		}

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
