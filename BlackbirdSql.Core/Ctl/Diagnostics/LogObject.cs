// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Diagnostics.LogObject

using System;
using BlackbirdSql.Core.Ctl.Enums;

namespace BlackbirdSql.Core.Ctl.Diagnostics;

[Serializable]
internal class LogObject
{
	internal string Name { get; private set; }

	internal EnLogComplianceLevel LogComplianceLevel { get; private set; }

	internal LogObject(string name, EnLogComplianceLevel complianceLevel)
	{
		Name = name;
		LogComplianceLevel = complianceLevel;
	}

	internal static LogObject CreateUntagged(string name)
	{
		return new LogObject(name, EnLogComplianceLevel.Untagged);
	}

	internal static LogObject CreateSensitiveData(string name)
	{
		return new LogObject(name, EnLogComplianceLevel.SensitiveData);
	}

	internal static LogObject CreateMasked(string name)
	{
		return new LogObject(name, EnLogComplianceLevel.Masked);
	}

	internal bool ShouldMask()
	{
		if (LogComplianceLevel == EnLogComplianceLevel.SensitiveData)
		{
			return true;
		}

		return false;
	}

	internal static EnLogComplianceLevel Max(EnLogComplianceLevel a, EnLogComplianceLevel b)
	{
		return (EnLogComplianceLevel)Math.Max((int)a, (int)b);
	}

	public override string ToString()
	{
		return Name;
	}
}
