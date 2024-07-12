// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Diagnostics.LogComplianceLevel
// namespace Microsoft.Data.Tools.Diagnostics

using System;


namespace BlackbirdSql.Sys.Enums;

[Serializable]
public enum EnLogComplianceLevel
{
	Untagged = 1000,
	Masked = 2000,
	SensitiveData = 3000
}
