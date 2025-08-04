// Microsoft.Data.Tools.Components, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Components.Diagnostics.SqlTraceId

namespace BlackbirdSql.Sys.Enums;


internal enum EnEvsId : int
{
	Critical = 1,
	Error,
	Warning,
	Information,
	Trace,
	Debug,
	OpStart,
	OpStop,
	OpResume,
	OpSuspend
}
