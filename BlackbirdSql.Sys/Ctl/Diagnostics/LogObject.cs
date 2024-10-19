// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Diagnostics.LogObject

using System;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Sys.Ctl.Diagnostics;


/// <summary>
/// Deprecated.
/// </summary>
[Serializable]
public class LogObject
{

	public LogObject(string name, EnLogComplianceLevel complianceLevel)
	{
		Name = name;
		LogComplianceLevel = complianceLevel;
	}



	public string Name { get; private set; }

	public EnLogComplianceLevel LogComplianceLevel { get; private set; }



	public static LogObject CreateUntagged(string name)
	{
		return new LogObject(name, EnLogComplianceLevel.Untagged);
	}

	public static LogObject CreateSensitiveData(string name)
	{
		return new LogObject(name, EnLogComplianceLevel.SensitiveData);
	}

	public static LogObject CreateMasked(string name)
	{
		return new LogObject(name, EnLogComplianceLevel.Masked);
	}

	public bool ShouldMask()
	{
		if (LogComplianceLevel == EnLogComplianceLevel.SensitiveData)
		{
			return true;
		}

		return false;
	}

	public static EnLogComplianceLevel Max(EnLogComplianceLevel a, EnLogComplianceLevel b)
	{
		return (EnLogComplianceLevel)Math.Max((int)a, (int)b);
	}

	public override string ToString()
	{
		return Name;
	}
}
