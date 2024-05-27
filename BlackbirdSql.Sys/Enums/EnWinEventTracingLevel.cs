// Microsoft.Data.Tools.Components, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Components.Diagnostics.WindowsEventTracingLevel

namespace BlackbirdSql.Sys;

public enum EnWinEventTracingLevel : byte
{
	Always,
	Critical,
	Error,
	Warning,
	Informational,
	Verbose
}
